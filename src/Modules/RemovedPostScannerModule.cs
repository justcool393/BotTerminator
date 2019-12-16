using RedditSharp;
using RedditSharp.Things;
using System;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class RemovedPostScannerModule : ScannerModule<Post>
	{
		public RemovedPostScannerModule(BotTerminator bot) : base(bot, null, bot.RedditInstance.GetListing<Post>(BotTerminator.ModRemovedUrl, 100, 100))
		{
			RunForeverCooldown = new TimeSpan(0, 0, 10);
		}

		public override Task SetupAsync()
		{
			Log.Information("Starting removed post scanner module");
			return Task.CompletedTask;
		}
	}
}
