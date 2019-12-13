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
	public class UpdateBanListModule : ListingBotModule<Post>
	{
		private static readonly Regex usernameRegex = new Regex(@"https?://(?:(?:www|old|new|[A-z]{2}|alpha|beta|ssl|pay)\.)?(?:c|r(?:emoveddit)?)eddit\.com//?u(?:ser)?/([\w_-]+)");

		private static bool first = true;

		public UpdateBanListModule(BotTerminator bot) : base(bot, bot.GlobalConfig.BanListMetricId, bot.RedditInstance.GetListing<Post>("/r/" + bot.SubredditName + "/new", -1, BotTerminator.PageLimit))
		{
			RequireUnique = true;
		}

		public override Task SetupAsync()
		{
			Console.WriteLine("Starting update ban list module");
			return Task.CompletedTask;
		}

		public override async Task TeardownAsync()
		{
			await bot.UserLookup.UpdateUserAsync(BotTerminator.CacheFreshenerUserName, String.Empty, false, true);
		}

		protected override async Task PostRunItemsAsync(ICollection<Post> subredditPosts)
		{
			// hide all of them at once
			if (subredditPosts.Count > 0)
			{
				const String requestVerb = "POST";
				for (int i = 0; i < subredditPosts.Count; i += 25)
				{
					String formattedUrl = String.Format("{0}?id={1}", BotTerminator.HideUrl, String.Join(",", subredditPosts.Select(s => s.FullName).Skip(i).Take(25)));
					await bot.WebAgent.ExecuteRequestAsync(() => bot.WebAgent.CreateRequest(formattedUrl, requestVerb));
				}
			}
		}

		protected override Boolean PreRunItem(Post subredditPost)
		{
			return !subredditPost.IsHidden && subredditPost.LinkFlairText != null && (subredditPost.LinkFlairText == "Banned" || subredditPost.LinkFlairText == "Meta");
		}

		protected override async Task RunItemAsync(Post subredditPost)
		{
			// We don't need to even look at meta posts
			if (subredditPost.LinkFlairText == "Meta") return;
			String[] groups = subredditPost.LinkFlairCssClass.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			/* 
			 * We don't use the post.Url property here because if the Url is not a
			 * well formed URI, RedditSharp throws an UriFormatException. The cases
			 * where this is a problem is exceedingly rare, but it is possible.
			 */
			Match match = usernameRegex.Match(subredditPost["url"].Value<String>().Trim());
			if (match == null || match.Groups.Count != 2) return;
			Console.WriteLine("Found new bot to ban " + match.Groups[1].Value);
			String targetUserName = match.Groups[1].Value;
			//await bot.UserLookup.UpdateUserAsync(targetUserName, true, first);
			first = false;
		}
	}
}
