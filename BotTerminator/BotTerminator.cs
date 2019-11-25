using BotTerminator.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotTerminator
{
	public class BotTerminator
	{
		private static readonly Regex usernameRegex = new Regex(@"https?://(?:(?:www|old|new|[A-z]{2}|alpha|beta|ssl|pay)\.)?reddit\.com//?u(?:ser)?/([\w_-]+)");
		private static readonly String[] ignoreAuthorCssClasses = new[]
		{
			"btproof", "botbustproof",
		};

		private readonly IWebAgent webAgent;
		private readonly Reddit reddit;
		private readonly AuthenticationConfig authConfig;

		internal GlobalConfig GlobalConfig { get; private set; }

		private const String HideUrl = "/api/hide";
		private const String NewModCommentsUrl = "/r/mod/comments";
		private const String QuarantineOptInUrl = "/api/quarantine_optin";

		private const Int32 PageLimit = 100;

		private const String CacheFreshenerUserName = "reddit";
		private const String DeletedUserName = "[deleted]";

		private String SubredditName => authConfig.SubredditName;

		public BotTerminator(IWebAgent webAgent, Reddit reddit, AuthenticationConfig authConfig)
		{
			this.webAgent = webAgent;
			this.reddit = reddit;
			this.authConfig = authConfig;
		}

		private Dictionary<String, Subreddit> SubredditLookup { get; set; } = new Dictionary<String, Subreddit>();
		private IBotDatabase UserLookup { get; set; }

		public async Task StartAsync()
		{
			Wiki subredditWiki = (await reddit.GetSubredditAsync(SubredditName, false)).GetWiki;
			try
			{
				const String pageName = "botConfig/botTerminator";
				if (!(await subredditWiki.GetPageNamesAsync()).Contains(pageName.ToLower()))
				{
					GlobalConfig = new GlobalConfig();
					await subredditWiki.EditPageAsync(pageName, JsonConvert.SerializeObject(globalConfig), null, "create BotTerminator configuration");
					await subredditWiki.SetPageSettingsAsync(pageName, true, WikiPageSettings.WikiPagePermissionLevel.Mods);
				}
				else
				{
					GlobalConfig = JsonConvert.DeserializeObject<GlobalConfig>((await subredditWiki.GetPageAsync(pageName.ToLower())).MarkdownContent);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to load or create subreddit config: " + ex.Message);
				return;
			}
			UserLookup = new WikiBotDatabase(await reddit.GetSubredditAsync(SubredditName, false));
			await UserLookup.CheckUserAsync(CacheFreshenerUserName);
			await SrCacheUpdateAsync();
			await Task.WhenAll(StartCommentLoopAsync(), StartNewBanUpdateLoopAsync(), StartSrCacheUpdateLoopAsync(), StartInviteAcceptorLoopAsync(), StartMakeSureCacheFreshLoopAsync());
		}

		private async Task StartMakeSureCacheFreshLoopAsync()
		{
			Console.WriteLine("Starting cache freshener loop");
			while (true)
			{
				try
				{
					await UserLookup.UpdateUserAsync(CacheFreshenerUserName, false);
					await Task.Delay(new TimeSpan(0, 10, 0));
				}
				catch { } // we don't really care
			}
		}

		private async Task StartInviteAcceptorLoopAsync()
		{
			Console.WriteLine("Starting invite acceptor loop");
			while (true)
			{
				try
				{
					List<PrivateMessage> privateMessages = new List<PrivateMessage>();
					await reddit.User.GetUnreadMessages(-1).ForEachAsync(unreadMessage =>
					{
						if (unreadMessage is PrivateMessage message)
						{
							privateMessages.Add(message);
						}
					});
					const String modInviteMsg = "invitation to moderate /r/";
					foreach (PrivateMessage privateMessage in privateMessages)
					{
						bool shouldMarkRead = true;

						if (privateMessage.Subreddit != null && privateMessage.FirstMessageName == null && privateMessage.Subject.StartsWith(modInviteMsg))
						{
							String srName = privateMessage.Subject.Substring(privateMessage.Subject.IndexOf(modInviteMsg) + modInviteMsg.Length);
							if (String.IsNullOrWhiteSpace(srName))
							{
								continue; // handle weird edge case where the subject is literally just "invitation to moderate /r/"
							}
							try
							{
								await (await reddit.GetSubredditAsync(srName, false)).AcceptModeratorInviteAsync();
								Console.WriteLine("Accepted moderator invite to /r/{0}", srName);
							}
							catch (RedditHttpException ex)
							{
								// This is the best we can do without an update to the library
								if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
								{
									try
									{
										await QuarantineOptInAsync(srName);
										shouldMarkRead = false;
									}
									catch (Exception subException)
									{
										Console.WriteLine("Failed to opt in to the quarantine (if it exists): {0}", subException.Message);
									}
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine("Failed to accept moderator invite for subreddit /r/{0}: {1}", srName, ex.Message);
							}
						}
						if (shouldMarkRead)
						{
							try
							{
								await privateMessage.SetAsReadAsync();
							}
							catch (Exception ex)
							{
								Console.WriteLine("Failed to mark message {0} as read: {1}", privateMessage.FullName, ex.Message);
							}
						}
					}
					await SrCacheUpdateAsync();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Failed to run accept invite loop: {0}", ex.Message);
				}
			}
		}

		private async Task StartSrCacheUpdateLoopAsync()
		{
			Console.WriteLine("Starting subreddit cache update loop");
			while (true)
			{
				try
				{
					await Task.Delay(new TimeSpan(0, 10, 0));
					await SrCacheUpdateAsync();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}

		private async Task SrCacheUpdateAsync()
		{
			await reddit.User.GetModeratorSubreddits(-1).ForEachAsync(subreddit =>
			{
				if (!SubredditLookup.ContainsKey(subreddit.DisplayName))
				{
					SubredditLookup.Add(subreddit.DisplayName, subreddit);
				}
				else
				{
					SubredditLookup[subreddit.DisplayName] = subreddit;
				}
			});
		}

		public async Task StartCommentLoopAsync()
		{
			Console.WriteLine("Starting comment loop");
			while (true)
			{
				try
				{
					List<Comment> comments = new List<Comment>();
					await reddit.GetListing<Comment>(NewModCommentsUrl, 250, PageLimit).ForEachAsync(comment =>
					{
						if (IsUnbannable(comment) || (comment.BannedBy != null || comment.BannedBy == reddit.User.Name)) return;
						// all distinguishes are given to moderators (who can't be banned) or known humans
						if (comment.Distinguished != ModeratableThing.DistinguishType.None) return;
						comments.Add(comment);
					});

					foreach (Comment comment in comments)
					{
						if (await CheckShouldBanAsync(comment))
						{
							AbstractSubredditOptionSet options = GlobalConfig.GlobalOptions;
							if (!options.Enabled)
							{
								continue;
							}
							// TODO: Magic string
							if (options.RemovalType == Models.RemovalType.Spam)
							{
								await comment.RemoveSpamAsync();
							}
							else if (options.RemovalType == Models.RemovalType.Remove)
							{
								await comment.RemoveAsync();
							}
							if (options.BanDuration > -1)
							{
								await SubredditLookup[comment.Subreddit].BanUserAsync(comment.AuthorName, options.BanNote, null, options.BanDuration, options.BanMessage);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
				await Task.Delay(5000);
			}
		}

		public async Task StartNewBanUpdateLoopAsync()
		{
			Console.WriteLine("Starting config updater loop");
			while (true)
			{
				try
				{
					List<Post> posts = new List<Post>();
					await reddit.GetListing<Post>("/r/" + SubredditName + "/new", -1, PageLimit).ForEachAsync(post =>
					{
						if (post?.LinkFlairText == null || (post.LinkFlairText != "Banned" && post.LinkFlairText != "Meta")) return;
						if (post.IsHidden) return;
						posts.Add(post);
					});
					foreach (Post post in posts)
					{
						// We don't need to even look at meta posts
						if (post.LinkFlairText == "Meta") continue;

						/* 
						 * We don't use the post.Url property here because if the Url is not a
						 * well formed URI, RedditSharp throws an UriFormatException. The cases
						 * where this is a problem is exceedingly rare, but it is possible.
						 */
						Match match = usernameRegex.Match(post["url"].Value<String>().Trim());
						if (match == null || match.Groups.Count != 2)
						{
							continue;
						}
						Console.WriteLine("Found new bot to ban " + match.Groups[1].Value);
						String targetUserName = match.Groups[1].Value;
						await UserLookup.UpdateUserAsync(targetUserName, true);
					}

					// hide all of them at once
					if (posts.Count > 0)
					{
						const String requestVerb = "POST";
						for (int i = 0; i < posts.Count; i+=25)
						{
							String formattedUrl = String.Format("{0}?id={1}", HideUrl, String.Join(",", posts.Select(s => s.FullName).Skip(i).Take(25)));
							await webAgent.ExecuteRequestAsync(() => webAgent.CreateRequest(formattedUrl, requestVerb));
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}

				await Task.Delay(5000);
			}
		}

		/// <summary>
		/// Checks whether this user is ineligible to be banned. The only case where
		/// this currently returns <see langword="true" /> is when the <paramref name="comment"/>
		/// or its <see cref="ModeratableThing.AuthorName"/> is <see langword="null"/>, whitespace,
		/// or equal to <see cref="DeletedUserName"/>.
		/// </summary>
		/// <param name="comment">The comment to test for the ability to ban</param>
		/// <returns>A value determining whether the user is not bannable on Reddit.</returns>
		private Boolean IsUnbannable(Comment comment) => String.IsNullOrWhiteSpace(comment?.AuthorName) || comment?.AuthorName == DeletedUserName;

		private async Task<Boolean> CheckShouldBanAsync(Comment comment)
		{
			if (IsUnbannable(comment)) return false;
			if (!String.IsNullOrWhiteSpace(comment.AuthorFlairCssClass) &&
				ignoreAuthorCssClasses.Any(cssClass => comment.AuthorFlairCssClass.Contains(cssClass)))
			{
				return false;
			}
			return await UserLookup.CheckUserAsync(comment.AuthorName);
		}

		private async Task QuarantineOptInAsync(String subredditName)
		{
			const string requestVerb = "POST";
			await webAgent.ExecuteRequestAsync(() => {
				HttpRequestMessage request = webAgent.CreateRequest(QuarantineOptInUrl, requestVerb);
				request.Content = new StringContent("sr_name=" + subredditName, Encoding.UTF8);
				return request;
			});
		}
	}
}
