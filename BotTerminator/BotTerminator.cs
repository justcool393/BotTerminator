using BotTerminator.Configuration;
using BotTerminator.Modules;
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
		private static readonly Regex usernameRegex = new Regex(@"https?://(?:(?:www|old|new|[A-z]{2}|alpha|beta|ssl|pay)\.)?RedditInstance\.com//?u(?:ser)?/([\w_-]+)");
		private static readonly String[] ignoreAuthorCssClasses = new[]
		{
			"btproof", "botbustproof",
		};

		private readonly AuthenticationConfig authConfig;

		public const String CacheFreshenerUserName = "RedditInstance";
		private const String DeletedUserName = "[deleted]";
		public const String HideUrl = "/api/hide";
		public const String NewModCommentsUrl = "/r/mod/comments";
		public const Int32 PageLimit = 100;
		public const String QuarantineOptInUrl = "/api/quarantine_optin";

		internal GlobalConfig GlobalConfig { get; private set; }
		internal Reddit RedditInstance { get; private set; }
		private IReadOnlyCollection<BotModule> Modules { get; set; }
		internal Dictionary<String, Subreddit> SubredditLookup { get; private set; } = new Dictionary<String, Subreddit>();
		internal String SubredditName => authConfig.SubredditName;
		private IBotDatabase UserLookup { get; set; }
		internal IWebAgent WebAgent { get; private set; }

		public BotTerminator(IWebAgent webAgent, Reddit RedditInstance, AuthenticationConfig authConfig)
		{
			this.WebAgent = webAgent;
			this.RedditInstance = RedditInstance;
			this.authConfig = authConfig;
		}

		public async Task StartAsync()
		{
			Wiki subredditWiki = (await RedditInstance.GetSubredditAsync(SubredditName, false)).GetWiki;
			try
			{
				const String pageName = "botConfig/botTerminator";
				if (!(await subredditWiki.GetPageNamesAsync()).Contains(pageName.ToLower()))
				{
					GlobalConfig = new GlobalConfig();
					await subredditWiki.EditPageAsync(pageName, JsonConvert.SerializeObject(GlobalConfig), null, "create BotTerminator configuration");
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
			UserLookup = new WikiBotDatabase(await RedditInstance.GetSubredditAsync(SubredditName, false));
			await UserLookup.CheckUserAsync(CacheFreshenerUserName);
			await SrCacheUpdateAsync();

			Modules = new List<BotModule>()
			{
				new CommentScannerModule(this), new InviteAcceptorModule(this),
			};

			IEnumerable<Task> tasks = Modules.Select(s => s.RunForeverAsync()).Concat(new Task[]
			{
				StartNewBanUpdateLoopAsync(), StartSrCacheUpdateLoopAsync(), StartMakeSureCacheFreshLoopAsync()
			});
			await Task.WhenAll(tasks);
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

		public async Task SrCacheUpdateAsync()
		{
			await RedditInstance.User.GetModeratorSubreddits(-1).ForEachAsync(subreddit =>
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
					await RedditInstance.GetListing<Post>("/r/" + SubredditName + "/new", -1, PageLimit).ForEachAsync(post =>
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
							await WebAgent.ExecuteRequestAsync(() => WebAgent.CreateRequest(formattedUrl, requestVerb));
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
		public static Boolean IsUnbannable(Comment comment) => String.IsNullOrWhiteSpace(comment?.AuthorName) || comment?.AuthorName == DeletedUserName;

		internal async Task<Boolean> CheckShouldBanAsync(Comment comment)
		{
			if (IsUnbannable(comment)) return false;
			if (!String.IsNullOrWhiteSpace(comment.AuthorFlairCssClass) &&
				ignoreAuthorCssClasses.Any(cssClass => comment.AuthorFlairCssClass.Contains(cssClass)))
			{
				return false;
			}
			return await UserLookup.CheckUserAsync(comment.AuthorName);
		}

		internal async Task QuarantineOptInAsync(String subredditName)
		{
			const string requestVerb = "POST";
			await WebAgent.ExecuteRequestAsync(() => {
				HttpRequestMessage request = WebAgent.CreateRequest(QuarantineOptInUrl, requestVerb);
				request.Content = new StringContent("sr_name=" + subredditName, Encoding.UTF8);
				return request;
			});
		}
	}
}
