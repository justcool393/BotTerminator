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
		private static Regex re = new Regex(@"https://(?:www|old|new|np|beta)\.reddit\.com/user/([A-z_-]+)");

		private readonly Reddit r;
		private readonly String srName;

		public BotTerminator(Reddit r, String srName)
		{
			this.r = r;
			this.srName = srName;
		}

		private Dictionary<String, Subreddit> SrLookup { get; set; } = new Dictionary<String, Subreddit>();
		private IBotDatabase UserLookup { get; set; }

		public async Task StartAsync()
		{
			UserLookup = new WikiBotDatabase(await r.GetSubredditAsync(srName, false));
			await SrCacheUpdateAsync();
			await Task.WhenAll(StartCommentLoopAsync(), StartNewBanUpdateLoopAsync(), StartSrCacheUpdateLoopAsync(), StartSrCacheUpdateLoopAsync(), StartInviteAcceptorLoopAsync());
		}

		private async Task StartInviteAcceptorLoopAsync()
		{
			Console.WriteLine("Starting invite acceptor loop...");
			while (true)
			{
				try
				{
					List<PrivateMessage> pms = new List<PrivateMessage>();
					await r.User.GetUnreadMessages(-1).ForEachAsync(m =>
					{
						if (m is PrivateMessage pm)
						{
							pms.Add(pm);
						}
					});
					foreach (PrivateMessage m in pms)
					{
						await m.SetAsReadAsync();
						if (m.Subreddit != null && m.FirstMessageName == null && m.Subject.StartsWith("invitation to moderate /r/"))
						{
							String srName = m.Subject.Substring(m.Subject.IndexOf("invitation to moderate /r/"));
							if (String.IsNullOrWhiteSpace(srName))
							{
								continue; // handle weird edge case where the subject is literally just "invitation to moderate /r/"
							}
							try
							{
								await (await r.GetSubredditAsync(srName, false)).AcceptModeratorInviteAsync();
								await SrCacheUpdateAsync();
								Console.WriteLine("Accepted moderator invite to /r/{0}", srName);
							}
							catch (Exception ex)
							{
								Console.WriteLine("Failed to accept moderator invite for subreddit /r/{0}: {1}", srName, ex.Message);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Failed to run accept invite loop: {0}", ex.Message);
				}
			}
		}

		private async Task StartSrCacheUpdateLoopAsync()
		{
			Console.WriteLine("Starting subreddit cache update loop...");
			while (true)
			{
				try
				{
					await Task.Delay(60000);
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
			await r.User.GetModeratorSubreddits(-1).ForEachAsync(sr =>
			{
				if (!SrLookup.ContainsKey(sr.DisplayName))
				{
					SrLookup.Add(sr.DisplayName, sr);
				}
				else
				{
					SrLookup[sr.DisplayName] = sr;
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
					await r.GetListing<Comment>("/r/mod/comments", -1, 100).ForEachAsync(c =>
					{
						if (IsUnbannable(c) || (c.BannedBy != null || c.BannedBy == r.User.Name)) return;
						comments.Add(c);
					});

					foreach (Comment c in comments)
					{
						if (await CheckShouldBanAsync(c))
						{
							await Task.WhenAll(c.RemoveSpamAsync(), SrLookup[c.Subreddit].BanUserAsync(c.AuthorName, "spam", "botterminator banned", 0, String.Empty));
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
			while (true)
			{
				Console.WriteLine("Starting config updater loop...");
				try
				{
					List<Post> posts = new List<Post>();
					await r.GetListing<Post>("/r/" + srName + "/new", -1, 100).ForEachAsync(p =>
					{
						if (p.LinkFlairText != "Banned") return;
						if (p.IsHidden) return;
						posts.Add(p);
					});
					foreach (Post p in posts)
					{

						Match m = re.Match(p["url"].Value<String>().Trim());
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
						await UserLookup.UpdateUserAsync(target, true);
						await p.HideAsync();
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
			return await UserLookup.CheckUserAsync(c.AuthorName);
		}
	}
}
