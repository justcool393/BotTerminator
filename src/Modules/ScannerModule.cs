using BotTerminator.Configuration;
using BotTerminator.Models;
using Newtonsoft.Json.Linq;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class ScannerModule<T> : ListingBotModule<T> where T : ModeratableThing
	{
		public ScannerModule(BotTerminator bot, String metricId, Listing<T> listing) : base(bot, metricId, listing)
		{
		}

		public override Task SetupAsync() => Task.CompletedTask;

		public override Task TeardownAsync() => Task.CompletedTask;

		protected override Task PostRunItemsAsync(ICollection<T> things) => Task.CompletedTask;

		protected override sealed Boolean PreRunItem(T thing)
		{
			if (BotTerminator.IsUnbannable(thing) ||
				   (thing.BannedBy != null && thing.BannedBy == RedditInstance.User.Name) ||
				   (!GlobalConfig.AllowNsfw && thing["over_18"].Value<bool?>().GetValueOrDefault(false)) ||
				   (!GlobalConfig.AllowQuarantined && thing["quarantine"].Value<bool?>().GetValueOrDefault(false))) return false;
			// all distinguishes are given to moderators (who can't be banned) or known humans
			return thing.Distinguished == ModeratableThing.DistinguishType.None;
		}

		protected override sealed async Task RunItemAsync(T thing)
		{
			Log.Verbose("Scanning for banned users on thing {ThingFullname}", thing.FullName);
			String subredditName = thing["subreddit"].Value<String>();
			if (!bot.SubredditLookup.ContainsKey(subredditName))
			{
				Log.Verbose("Subreddit {SubredditName} not in cache. Adding to cache now.", subredditName);
				await bot.CacheSubredditAsync(subredditName);
			}
			CachedSubreddit subreddit = bot.SubredditLookup[subredditName];
			AbstractSubredditOptionSet options = new ShadedOptionSet(new[] { subreddit?.Options, GlobalConfig.GlobalOptions }, true);
			if (!options.Enabled) return;
			if (!options.ScanPosts && thing is Post) return;
			if (!options.ScanComments && thing is Comment) return;

			bot.IncrementStatisticIfExists("postCommentCount");

			IReadOnlyCollection<Group> bannedGroups = await bot.GetBannedGroupsAsync(options);

			if (await bot.CheckShouldBanAsync(thing, bannedGroups.Select(group => group.Name)))
			{
				Log.Debug("Found user {User}. Taking action {Action} based on subreddit setting for {Subreddit}", thing.AuthorName, options.RemovalType.ToString(), subreddit.RedditSubreddit.DisplayName);
				try
				{
					if (options.RemovalType == RemovalType.Spam)
					{
						await thing.RemoveSpamAsync();
						bot.IncrementStatisticIfExists("requestRate");
					}
					else if (options.RemovalType == RemovalType.Remove)
					{
						await thing.RemoveAsync();
						bot.IncrementStatisticIfExists("requestRate");
					}
				}
				catch (RedditHttpException ex)
				{
					Log.Error("Could not remove thing {ThingFullname} due to HTTP error from reddit: {ExceptionMessage}", thing.FullName, ex.Message);
				}
				if (options.BanDuration > -1)
				{
					Log.Verbose("Banning user {User} now.", thing.AuthorName);
					await subreddit.RedditSubreddit.BanUserAsync(thing.AuthorName, options.BanNote.Trim(), null, options.BanDuration, options.BanMessage.Trim());
					bot.IncrementStatisticIfExists("requestRate");
				}
			}
		}
	}
}
