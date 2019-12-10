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
		private const String ApiBaseUrl = "https://api.statuspage.io/v1/pages/{0}/metrics/{1}/data.json";

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
			JObject data = new JObject(new JProperty("data", JObject.FromObject(metric)));
			bot.StatusPageQueue.Enqueue(() =>
			{
				StringContent content = new StringContent(data.ToString(Newtonsoft.Json.Formatting.None), Encoding.UTF8, "application/json");
				return new HttpRequestMessage(HttpMethod.Post, String.Format(ApiBaseUrl, bot.GlobalConfig.StatusPagePageId, MetricId))
				{
					Content = content,
				};
			});
			return Task.CompletedTask;
		}
	}
}
