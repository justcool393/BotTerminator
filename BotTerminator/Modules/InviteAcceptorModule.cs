using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class InviteAcceptorModule : BotModule
	{
		public InviteAcceptorModule(BotTerminator bot) : base(bot)
		{
		}

		public override async Task RunOnceAsync()
		{
			List<PrivateMessage> privateMessages = new List<PrivateMessage>();
			await RedditInstance.User.GetUnreadMessages(-1).ForEachAsync(unreadMessage =>
			{
				if (unreadMessage is PrivateMessage message)
				{
					privateMessages.Add(message);
				}
			});
			const String modInviteMsg = "invitation to moderate /r/";
			foreach (PrivateMessage privateMessage in privateMessages)
			{
				bool shouldMarkRead = true;

				if (privateMessage.Subreddit != null && privateMessage.FirstMessageName == null && privateMessage.Subject.StartsWith(modInviteMsg))
				{
					String srName = privateMessage.Subject.Substring(privateMessage.Subject.IndexOf(modInviteMsg) + modInviteMsg.Length);
					if (String.IsNullOrWhiteSpace(srName))
					{
						continue; // handle weird edge case where the subject is literally just "invitation to moderate /r/"
					}
					try
					{
						await(await RedditInstance.GetSubredditAsync(srName, false)).AcceptModeratorInviteAsync();
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
			await bot.UpdateSubredditCacheAsync();
		}

		public override Task SetupAsync()
		{
			Console.WriteLine("Starting invite acceptor module");
			return Task.CompletedTask;
		}

		public override Task TeardownAsync() => Task.CompletedTask;
	}
}
