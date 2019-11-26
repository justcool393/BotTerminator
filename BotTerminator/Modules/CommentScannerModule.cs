using BotTerminator.Configuration;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class CommentScannerModule : ListingBotModule<Comment>
	{
		public CommentScannerModule(BotTerminator bot) : base(bot, bot.RedditInstance.GetListing<Comment>(BotTerminator.NewModCommentsUrl, 250, BotTerminator.PageLimit))
		{
		}

		public override Task SetupAsync()
		{
			Console.WriteLine("Starting comment scanner module");
			return Task.CompletedTask;
		}

		public override Task TeardownAsync() => Task.CompletedTask;

		protected override Task PostRunItemsAsync(ICollection<Comment> things) => Task.CompletedTask;

		protected override Boolean PreRunItem(Comment comment)
		{
			if (BotTerminator.IsUnbannable(comment) || (comment.BannedBy != null || comment.BannedBy == RedditInstance.User.Name)) return false;
			// all distinguishes are given to moderators (who can't be banned) or known humans
			return comment.Distinguished == ModeratableThing.DistinguishType.None;
		}

		protected override async Task RunItemAsync(Comment comment)
		{
			if (await bot.CheckShouldBanAsync(comment))
			{
				AbstractSubredditOptionSet options = GlobalConfig.GlobalOptions;
				if (!options.Enabled) return;
				if (options.RemovalType == Models.RemovalType.Spam)
				{
					try
					{
						await comment.RemoveSpamAsync();
					}
					catch (RedditHttpException ex)
					{
						Console.WriteLine("Could not remove comment {0} due to HTTP error from reddit: {1}", comment.FullName, ex.Message);
					}
				}
				else if (options.RemovalType == Models.RemovalType.Remove)
				{
					await comment.RemoveAsync();
				}
				if (options.BanDuration > -1)
				{
					await bot.SubredditLookup[comment.Subreddit].BanUserAsync(comment.AuthorName, options.BanNote, null, options.BanDuration, options.BanMessage);
				}
			}
		}
	}
}
