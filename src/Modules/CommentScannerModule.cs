using RedditSharp.Things;
using System;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class CommentScannerModule : ScannerModule<Comment>
	{
		public CommentScannerModule(BotTerminator bot) : base(bot, null, bot.RedditInstance.GetListing<Comment>(BotTerminator.NewModCommentsUrl, 250, BotTerminator.PageLimit))
		{
			RunForeverCooldown = new TimeSpan(0, 0, 0);
		}

		public override Task SetupAsync()
		{
			Log.Information("Starting comment scanner module");
			return Task.CompletedTask;
		}
	}
}
