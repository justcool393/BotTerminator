using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class InviteAcceptorModule : ListingBotModule<Thing>
	{
		public InviteAcceptorModule(BotTerminator bot) : base(bot, bot.RedditInstance.User.GetUnreadMessages(-1))
		{
		}

		public override Task SetupAsync()
		{
			Console.WriteLine("Starting invite acceptor module");
			return Task.CompletedTask;
		}

		public override Task TeardownAsync() => Task.CompletedTask;

		protected override async Task PostRunItemsAsync(ICollection<Thing> things)
		{
			if (things.Count > 0)
			{
				await bot.UpdateSubredditCacheAsync();
			}
			await Task.Delay(new TimeSpan(0, 0, 30));
		}

		protected override Boolean PreRunItem(Thing message)
		{
			return message is PrivateMessage privateMessage && privateMessage.Subreddit != null && privateMessage.FirstMessageName == null;
		}

		protected override async Task RunItemAsync(Thing messageAsThing)
		{
			const String modInviteMsg = "invitation to moderate /r/";
			PrivateMessage privateMessage = (PrivateMessage)messageAsThing;
			bool shouldMarkRead = true;

			if (privateMessage.Subject.StartsWith(modInviteMsg) && privateMessage.Subject.Length > modInviteMsg.Length)
			{
				String srName = privateMessage.Subject.Substring(privateMessage.Subject.IndexOf(modInviteMsg) + modInviteMsg.Length);
				if (String.IsNullOrWhiteSpace(srName))
				{
					await privateMessage.SetAsReadAsync();
					return; // handle weird edge case where the subject is literally just "invitation to moderate /r/"
				}
				try
				{
					await (await RedditInstance.GetSubredditAsync(srName, false)).AcceptModeratorInviteAsync();
					Console.WriteLine("Accepted moderator invite to /r/{0}", srName);
				}
				catch (RedditHttpException ex)
				{
					// This is the best we can do without an update to the library
					if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
					{
						try
						{
							await bot.QuarantineOptInAsync(srName);
							Console.WriteLine("Opted in to the quarantine for subreddit /r/{0}", srName);
							shouldMarkRead = false;
						}
						catch (Exception subException)
						{
							Console.WriteLine("Failed to opt in to the quarantine (if it exists): {0}", subException.Message);
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Failed to accept moderator invite for subreddit /r/{0}: {1}", srName, ex.Message);
				}
			}
			if (shouldMarkRead)
			{
				try
				{
					await privateMessage.SetAsReadAsync();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Failed to mark message {0} as read: {1}", privateMessage.FullName, ex.Message);
				}
			}
		}
	}
}
