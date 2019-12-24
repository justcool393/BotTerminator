using BotTerminator.Configuration;
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
			Log.Information("Starting update ban list module");
			return Task.CompletedTask;
		}

		public override async Task TeardownAsync()
		{
			await bot.UserLookup.UpdateUserAsync(BotTerminator.CacheFreshenerUserName, String.Empty, false, true);
		}

		protected override async Task PostRunItemsAsync(ICollection<Post> subredditPosts)
		{
			// hide all of them at once
			BanListConfig config = await bot.UserLookup.ReadConfigAsync();
			ICollection<Post> hideable = subredditPosts.Where(post => config.ShouldHide(post)).ToList();
			Log.Debug("Finished scanning for new users to process (found {PostCount}, hideable {HideableCount}). Hiding posts now.", subredditPosts.Count, hideable.Count);
			if (subredditPosts.Count > 0)
			{
				const String requestVerb = "POST";
				IList<Task> tasks = new List<Task>();
				for (int i = 0; i < hideable.Count; i += 25)
				{
					String formattedUrl = String.Format("{0}?id={1}", BotTerminator.HideUrl, String.Join(",", subredditPosts.Select(post => post.FullName).Skip(i).Take(25)));
					tasks.Add(bot.WebAgent.ExecuteRequestAsync(() => bot.WebAgent.CreateRequest(formattedUrl, requestVerb)));
					bot.IncrementStatisticIfExists("requestRate");
				}
				await Task.WhenAll(tasks);
			}
		}

		protected override Boolean PreRunItem(Post subredditPost)
		{
			String[] groups = GetGroupNames(subredditPost.LinkFlairCssClass);
			// TODO: maybe add async override or something?
			if (groups.Length == 0 || groups.Contains("await")) return false;
			return !subredditPost.IsHidden && subredditPost.LinkFlairCssClass != null;
		}

		private String[] GetGroupNames(String linkFlairCssClass)
		{
			return linkFlairCssClass.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}

		protected override async Task RunItemAsync(Post subredditPost)
		{
			// We don't need to even look at meta posts
			BanListConfig config = await bot.UserLookup.ReadConfigAsync();
			IEnumerable<String> groups = GetGroupNames(subredditPost.LinkFlairCssClass).Where(group => !config.NonGroupFlairCssClasses.Contains(group));
			/* 
			 * We don't use the post.Url property here because if the Url is not a
			 * well formed URI, RedditSharp throws an UriFormatException. The cases
			 * where this is a problem is exceedingly rare, but it is possible.
			 */
			Match match = usernameRegex.Match(subredditPost["url"].Value<String>().Trim());
			if (match == null || match.Groups.Count != 2) return;
			Log.Information("Processing user {BotUsername}", match.Groups[1].Value);
			String targetUserName = match.Groups[1].Value;
			foreach (String group in groups)
			{
				await bot.UserLookup.UpdateUserAsync(targetUserName, group, true, first);
			}
			first = false;
		}
	}
}
