using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	class StatisticsPusherModule : BotModule
	{
		public StatisticsPusherModule(BotTerminator bot) : base(bot)
		{
			this.RunForeverCooldown = new TimeSpan(0, 1, 0);
		}

		public override Task RunOnceAsync()
		{
			foreach (String statKey in bot.Statistics.Keys)
			{
				bot.Statistics[statKey].PushMetric(bot);
				bot.Statistics[statKey].Reset();
			}
			return Task.CompletedTask;
		}

		public override Task SetupAsync()
		{
			Log.Information("Starting statistics pusher module");
			return Task.CompletedTask;
		}

		public override Task TeardownAsync() => Task.CompletedTask;
	}
}
