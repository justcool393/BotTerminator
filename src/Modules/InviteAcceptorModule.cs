using BotTerminator.Exceptions;
using BotTerminator.Models;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class InviteAcceptorModule : ListingBotModule<Thing>
	{
		private const String moderatorAddSubject = "you are a moderator";
		/// <summary>
		/// A regex for the text of a moderator addition.
		/// </summary>
		/// <remarks>
		/// This regex includes support for the time subreddits (e.g. /r/t:1337)
		/// and subreddits that have periods in their names (e.g. /r/reddit.com).
		/// </remarks>
		private static readonly Regex moderatorAddRegex = new Regex(@"^you have been added as a moderator to \[/r/((?:t:)?\w_-\.)+:.*\]\(/r/.+\)\.$", RegexOptions.Compiled);
		private const String moderatorRemoval = " has been removed as a moderator from /r/";

		public InviteAcceptorModule(BotTerminator bot) : base(bot, bot.GlobalConfig.InviteAcceptorMetricId, bot.RedditInstance.User.GetUnreadMessages(-1))
		{
			
		}

		public override Task SetupAsync()
		{
			Log.Information("Starting invite acceptor module");
			return Task.CompletedTask;
		}

		public override Task TeardownAsync() => Task.CompletedTask;

		protected override async Task PostRunItemsAsync(ICollection<Thing> things)
		{
			if (things.Count > 0)
			{
				await bot.UpdateSubredditCacheAsync();
			}
		}

		protected override Boolean PreRunItem(Thing message)
		{
			return message is PrivateMessage privateMessage && privateMessage.FirstMessageName == null &&
			       (privateMessage.Subreddit != null || privateMessage.Subject == moderatorAddSubject);
		}

		protected override async Task RunItemAsync(Thing messageAsThing)
		{
			const String modInviteMsg = "invitation to moderate /r/";
			PrivateMessage privateMessage = (PrivateMessage)messageAsThing;
			bool shouldMarkRead = true;
			Log.Verbose("Found message {MessageFullname} to process", privateMessage.FullName);
			if (privateMessage.Subreddit != null && privateMessage.Subject.StartsWith(modInviteMsg) && 
				privateMessage.Subject.Length > modInviteMsg.Length)
			{
				String subredditName = privateMessage.Subreddit;
				try
				{
					await (await RedditInstance.GetSubredditAsync(subredditName, false)).AcceptModeratorInviteAsync();
					bot.IncrementStatisticIfExists("requestRate");
					bot.IncrementStatisticIfExists("requestRate");
					Log.Information("Accepted moderator invite to {Subreddit}", "/r/" + subredditName);
				}
				catch (RedditHttpException ex)
				{
					shouldMarkRead = !await AcceptQuarantineIfValidException(subredditName, ex);
				}
				catch (Exception ex)
				{
					Log.Error("Failed to accept moderator invite for subreddit {Subreddit}: {ExceptionMessage}", "/r/" + subredditName, ex.Message);
				}
			}
			else if (privateMessage.Subject == moderatorAddSubject) // these messages aren't sent as a subreddit
			{
				Match moderatorAddMatch = moderatorAddRegex.Match(privateMessage.Body);
				if (moderatorAddMatch != null && moderatorAddMatch.Groups.Count >= 2)
				{
					String subredditName = moderatorAddMatch.Groups[1].Value;
					try
					{
						Subreddit subreddit = await RedditInstance.GetSubredditAsync(subredditName, false);
						bot.IncrementStatisticIfExists("requestRate");
					}
					catch (RedditHttpException ex)
					{
						// it is possible although rare that we could be added as a moderator to a subreddit that's quarantined
						await AcceptQuarantineIfValidException(subredditName, ex);
					}
				}

			}
			if (shouldMarkRead)
			{
				Log.Verbose("Marking message {MessageFullName} as read", privateMessage.FullName);
				try
				{
					await privateMessage.SetAsReadAsync();
					bot.IncrementStatisticIfExists("requestRate");
				}
				catch (Exception ex)
				{
					Log.Warning("Failed to mark message {MessageFullName} as read: {ExceptionMessage}", privateMessage.FullName, ex.Message);
				}
			}
		}

		private async Task<bool> AcceptQuarantineIfValidException(String subredditName, RedditHttpException exception)
		{
			// This is the best we can do without an update to the library
			if (exception.StatusCode == System.Net.HttpStatusCode.Forbidden)
			{
				try
				{
					await bot.QuarantineOptInAsync(subredditName);
					bot.IncrementStatisticIfExists("requestRate");
					Log.Information("Opted in to the quarantine for subreddit {Subreddit}", "/r/" + subredditName);
					return true;
				}
				catch (Exception subException)
				{
					Log.Error(subException, "Failed to opt in to the quarantine for subreddit {Subreddit} (if it exists): {ExceptionMessage}", subredditName, subException.Message);
				}
			}
			return false;
		}
	}
}
