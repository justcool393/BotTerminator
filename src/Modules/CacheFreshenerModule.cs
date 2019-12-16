﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class CacheFreshenerModule : BotModule
	{
		public CacheFreshenerModule(BotTerminator bot) : base(bot)
		{
			RunForeverCooldown = new TimeSpan(0, 10, 0);
		}

		public override async Task RunOnceAsync()
		{
			await bot.UserLookup.UpdateUserAsync(BotTerminator.CacheFreshenerUserName, String.Empty, false);
			await bot.UpdateSubredditCacheAsync();
		}

		public override Task SetupAsync()
		{
			Log.Information("Starting cache freshener module");
			return Task.CompletedTask;
		}

		public override Task TeardownAsync() => Task.CompletedTask;
	}
}
