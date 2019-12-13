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
			String subredditName = thing["subreddit"].Value<String>();
			if (!bot.SubredditLookup.ContainsKey(subredditName))
			{
				await bot.CacheSubredditAsync(subredditName);
			}
			CachedSubreddit subreddit = bot.SubredditLookup[subredditName];
			AbstractSubredditOptionSet options = new ShadedOptionSet(new[] { subreddit?.Options, GlobalConfig.GlobalOptions }, true);
			if (!options.Enabled) return;
			if (!options.ScanPosts && thing is Post) return;
			if (!options.ScanComments && thing is Comment) return;

			IReadOnlyCollection<Group> bannedGroups = await bot.GetBannedGroupsAsync(options);

			if (await bot.CheckShouldBanAsync(thing, bannedGroups.Select(group => group.Name)))
			{
				try
				{
					if (options.RemovalType == RemovalType.Spam)
					{
						await thing.RemoveSpamAsync();
					}
					else if (options.RemovalType == RemovalType.Remove)
					{
						await thing.RemoveAsync();
					}
				}
				catch (RedditHttpException ex)
				{
					Console.WriteLine("Could not remove thing {0} due to HTTP error from reddit: {1}", thing.FullName, ex.Message);
				}
				if (options.BanDuration > -1)
				{
					await subreddit.RedditSubreddit.BanUserAsync(thing.AuthorName, options.BanNote.Trim(), null, options.BanDuration, options.BanMessage.Trim());
				}
			}
		}
	}
}
