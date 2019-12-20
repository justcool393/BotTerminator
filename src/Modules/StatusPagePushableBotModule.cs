using BotTerminator.Configuration;
using BotTerminator.Exceptions;
using BotTerminator.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public abstract class StatusPagePushableBotModule : BotModule
	{
		protected abstract String MetricId { get; }

		protected StatusPagePushableBotModule(BotTerminator bot) : base(bot) 
		{ 
  
		}

		protected Task PushMetricAsync(MetricData metric)
		{
			if (bot.GlobalConfig.StatusPagePageId == null)
			{
				throw new StatusPagePushException("Cannot push to status page when page id is null", new ArgumentNullException(nameof(bot.GlobalConfig.StatusPagePageId)));
			}
			else if (MetricId == null)
			{
				throw new StatusPagePushException("Cannot push to status page when metric id is null", new ArgumentNullException(nameof(MetricId)));
			}
			metric.MetricId = metric.MetricId ?? MetricId;
			bot.StatusPageQueueData.Enqueue(metric);
			return Task.CompletedTask;
		}
	}
}
