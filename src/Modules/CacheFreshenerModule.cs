using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class CacheFreshenerModule : ListingBotModule<ModAction>
	{
		private readonly ISet<String> processedModActionNames = new HashSet<String>();

		public CacheFreshenerModule(BotTerminator bot) : base(bot, null, bot.RedditInstance.GetListing<ModAction>($"/r/mod-{bot.SubredditName}/about/log?type=wikirevise", 100, 100))
		{
			RunForeverCooldown = new TimeSpan(0, 3, 0);
		}

		public override Task SetupAsync()
		{
			Log.Information("Starting cache freshener module");
			return Task.CompletedTask;
		}

		public override async Task TeardownAsync()
		{
			await bot.UserLookup.UpdateUserAsync(BotTerminator.CacheFreshenerUserName, String.Empty, false);
		}

		protected override async Task PostRunItemsAsync(ICollection<ModAction> actions)
		{
			foreach (ModAction action in actions)
			{
				processedModActionNames.Add(action.Id);
			}
			await bot.UserLookup.UpdateUserAsync(BotTerminator.CacheFreshenerUserName, String.Empty, false);
		}

		protected override Boolean PreRunItem(ModAction action)
		{
			return !processedModActionNames.Contains(action.Id) && action.Details == "Page botconfig/botterminator edited";
		}

		protected override async Task RunItemAsync(ModAction action)
		{
			await bot.CacheSubredditAsync(action.SubredditName);
		}
	}
}
