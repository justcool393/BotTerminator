using Newtonsoft.Json.Linq;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotTerminator
{
	public class BotTerminator
	{
		private static Regex UserUrlRegex = new Regex(@"https://(?:www|old|new|np|beta)\.reddit\.com/user/[\w-]+/?");

		private readonly Reddit RedditClient;
		private readonly String DataSubredditName;

		public BotTerminator(Reddit redditClient, String dataSubredditName)
		{
			this.RedditClient = redditClient;
			this.DataSubredditName = dataSubredditName;
		}

		private Dictionary<String, Subreddit> Subreddits { get; set; } = new Dictionary<String, Subreddit>();
		private IBotDatabase UserDatabase { get; set; }

		public async Task StartAsync()
		{
			this.UserDatabase = new WikiBotDatabase(await this.RedditClient.GetSubredditAsync(this.DataSubredditName, false));
			await SubredditCacheUpdateAsync();
			await Task.WhenAll(StartCommentLoopAsync(), StartNewBanUpdateLoopAsync(), StartSubredditCacheUpdateLoopAsync(), StartSubredditCacheUpdateLoopAsync(), StartInviteAcceptorLoopAsync());
		}

		private async Task StartInviteAcceptorLoopAsync()
		{
			Console.WriteLine("Starting invite acceptor loop...");
			while (true)
			{
				try
				{
					List<PrivateMessage> pms = new List<PrivateMessage>();
					await this.RedditClient.User.GetUnreadMessages(-1).ForEachAsync(m =>
					{
						if (m is PrivateMessage pm)
						{
							pms.Add(pm);
						}
					});
					const String modInviteMsg = "invitation to moderate /r/";
					foreach (PrivateMessage m in pms)
					{
						await m.SetAsReadAsync();
						if (m.Subreddit != null && m.FirstMessageName == null && m.Subject.StartsWith(modInviteMsg))
						{
							String srName = m.Subject.Substring(m.Subject.IndexOf(modInviteMsg) + modInviteMsg.Length);
							if (String.IsNullOrWhiteSpace(srName))
							{
								continue; // handle weird edge case where the subject is literally just "invitation to moderate /r/"
							}
							try
							{
								await (await this.RedditClient.GetSubredditAsync(srName, false)).AcceptModeratorInviteAsync();
								Console.WriteLine("Accepted moderator invite to /r/{0}", srName);
							}
							catch (Exception ex)
							{
								Console.WriteLine("Failed to accept moderator invite for subreddit /r/{0}: {1}", srName, ex.Message);
							}
							await SubredditCacheUpdateAsync();
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Failed to run accept invite loop: {0}", ex.Message);
				}
			}
		}

		private async Task StartSubredditCacheUpdateLoopAsync()
		{
			Console.WriteLine("Starting subreddit cache update loop...");
			while (true)
			{
				try
				{
					await Task.Delay(60000);
					await SubredditCacheUpdateAsync();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}

		private async Task SubredditCacheUpdateAsync()
		{
			await this.RedditClient.User.GetModeratorSubreddits(-1).ForEachAsync(sr =>
			{
				if (!this.Subreddits.ContainsKey(sr.DisplayName))
				{
					this.Subreddits.Add(sr.DisplayName, sr);
				}
				else
				{
					this.Subreddits[sr.DisplayName] = sr;
				}
			});
		}

		public async Task StartCommentLoopAsync()
		{
			Console.WriteLine("Starting comment loop...");
			while (true)
			{
				try
				{
					List<Comment> comments = new List<Comment>();
					await this.RedditClient.GetListing<Comment>("/r/mod/comments", -1, 100).ForEachAsync(c =>
					{
						if (IsUnbannable(c) || (c.BannedBy != null || c.BannedBy == this.RedditClient.User.Name)) return;
						comments.Add(c);
					});

					foreach (Comment c in comments)
					{
						if (await CheckShouldBanAsync(c))
						{
							await Task.WhenAll(c.RemoveSpamAsync(), this.Subreddits[c.Subreddit].BanUserAsync(c.AuthorName, "spam", "botterminator banned", 0, String.Empty));
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
			Console.WriteLine("Starting config updater loop...");
			while (true)
			{
				try
				{
					List<Post> posts = new List<Post>();
					await this.RedditClient.GetListing<Post>("/r/" + this.DataSubredditName + "/new", -1, 100).ForEachAsync(p =>
					{
						if (p.LinkFlairText != "Banned") return;
						if (p.IsHidden) return;
						posts.Add(p);
					});
					foreach (Post post in posts)
					{

						Match m = UserUrlRegex.Match(post["url"].Value<String>().Trim());
						if (m == null)
						{
							continue;
						}
						else if (m.Groups.Count != 2)
						{
							continue;
						}
						Console.WriteLine("found target " + m.Groups[1].Value);
						String target = m.Groups[1].Value;
						await this.UserDatabase.UpdateUserAsync(target, true);
						await post.HideAsync();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}

				await Task.Delay(5000);
			}
		}

		private Boolean IsUnbannable(Comment c) => c.AuthorName == "[deleted]";

		private async Task<Boolean> CheckShouldBanAsync(Comment c)
		{
			if (IsUnbannable(c)) return false;
			if (!String.IsNullOrWhiteSpace(c.AuthorFlairCssClass) &&
				(c.AuthorFlairCssClass.Contains("botbustproof") || c.AuthorFlairCssClass.Contains("btproof")))
			{
				return false;
			}
			return await this.UserDatabase.CheckUserAsync(c.AuthorName);
		}
	}
}
