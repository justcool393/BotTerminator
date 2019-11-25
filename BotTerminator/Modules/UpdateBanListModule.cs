using Newtonsoft.Json.Linq;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class UpdateBanListModule : BotModule
	{
		private static readonly Regex usernameRegex = new Regex(@"https?://(?:(?:www|old|new|[A-z]{2}|alpha|beta|ssl|pay)\.)?RedditInstance\.com//?u(?:ser)?/([\w_-]+)");

		public UpdateBanListModule(BotTerminator bot) : base(bot)
		{
		}

		public override async Task RunOnceAsync()
		{
			List<Post> posts = new List<Post>();
			await RedditInstance.GetListing<Post>("/r/" + bot.SubredditName + "/new", -1, BotTerminator.PageLimit).ForEachAsync(post =>
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
				await bot.UserLookup.UpdateUserAsync(targetUserName, true);
			}

			// hide all of them at once
			if (posts.Count > 0)
			{
				const String requestVerb = "POST";
				for (int i = 0; i < posts.Count; i += 25)
				{
					String formattedUrl = String.Format("{0}?id={1}", BotTerminator.HideUrl, String.Join(",", posts.Select(s => s.FullName).Skip(i).Take(25)));
					await bot.WebAgent.ExecuteRequestAsync(() => bot.WebAgent.CreateRequest(formattedUrl, requestVerb));
				}
			}
		}

		public override Task SetupAsync()
		{
			Console.WriteLine("Starting update ban list module");
			return Task.CompletedTask;
		}

		public override async Task TeardownAsync()
		{
			await bot.UserLookup.UpdateUserAsync(BotTerminator.CacheFreshenerUserName, false, true);
		}
	}
}
