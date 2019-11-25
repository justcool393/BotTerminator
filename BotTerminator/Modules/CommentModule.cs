using BotTerminator.Configuration;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class CommentModule : BotModule
	{
		public CommentModule(BotTerminator bot) : base(bot)
		{
		}

		public override async Task RunOnceAsync()
		{
			List<Comment> comments = new List<Comment>();
			await RedditInstance.GetListing<Comment>(BotTerminator.NewModCommentsUrl, 250, BotTerminator.PageLimit).ForEachAsync(comment =>
			{
				if (BotTerminator.IsUnbannable(comment) || (comment.BannedBy != null || comment.BannedBy == RedditInstance.User.Name)) return;
				// all distinguishes are given to moderators (who can't be banned) or known humans
				if (comment.Distinguished != ModeratableThing.DistinguishType.None) return;
				comments.Add(comment);
			});

			foreach (Comment comment in comments)
			{
				if (await bot.CheckShouldBanAsync(comment))
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
						await bot.SubredditLookup[comment.Subreddit].BanUserAsync(comment.AuthorName, options.BanNote, null, options.BanDuration, options.BanMessage);
					}
				}
			}
		}

		public override Task SetupAsync()
		{
			Console.WriteLine("Starting comment scanner module");
			return Task.CompletedTask;
		}

		public override Task TeardownAsync() => Task.CompletedTask;
	}
}
