using BotTerminator.Configuration;
using RedditSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public abstract class BotModule
	{
		protected BotTerminator bot;

		protected GlobalConfig GlobalConfig => bot.GlobalConfig;
		protected Reddit RedditInstance => bot.RedditInstance;
		protected ILogger Log => bot.Log;
		protected TimeSpan RunForeverCooldown { get; set; } = new TimeSpan(0, 0, 30);

		protected BotModule(BotTerminator bot)
		{
			this.bot = bot;
		}

		public async Task RunForeverAsync()
		{
			await SetupAsync();
			while (bot.GlobalConfig.GlobalOptions.Enabled)
			{
				try
				{
					await RunOnceAsync();
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Module {ModuleTypeName} failed to run due to {ExceptionType}: {ExceptionMessage}", GetType().Name, ex.GetType().Name, ex.Message);
					bot.IncrementStatisticIfExists("errorRate");
				}
				if (RunForeverCooldown.Ticks > 0)
				{
					await Task.Delay(RunForeverCooldown);
				}
			}
			await TeardownAsync();
		}

		public abstract Task SetupAsync();

		public abstract Task RunOnceAsync();

		public abstract Task TeardownAsync();
	}
}
