using RedditSharp.Things;
using System;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class PostScannerModule : ScannerModule<Post>
	{
		public PostScannerModule(BotTerminator bot) : base(bot, bot.RedditInstance.GetListing<Post>(BotTerminator.NewModUrl, 250, BotTerminator.PageLimit))
		{
			RunForeverCooldown = new TimeSpan(0, 0, 10);
		}

		public override Task SetupAsync()
		{
			Console.WriteLine("Starting post scanner module");
			return Task.CompletedTask;
		}
	}
}
