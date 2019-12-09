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

		private HttpClient client { get; set; }

		private String PageId { get; set; }

		protected abstract String MetricId { get; }
		
		protected StatusPagePushableBotModule(BotTerminator bot) : base(bot)
		{
			client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", bot.AuthenticationConfig.StatusPageApiKey);
			PageId = bot.GlobalConfig.StatusPagePageId;
		}

		protected async Task PushMetricAsync(MetricData metric)
		{
			if (PageId == null)
			{
				throw new StatusPagePushException("Cannot push to status page when page id is null", new ArgumentNullException(nameof(PageId)));
			}
			else if (MetricId == null)
			{
				throw new StatusPagePushException("Cannot push to status page when metric id is null", new ArgumentNullException(nameof(MetricId)));
			}
			JObject data = new JObject(new JProperty("data", JObject.FromObject(metric)));
			try
			{
				StringContent content = new StringContent(data.ToString(Newtonsoft.Json.Formatting.None), Encoding.UTF8, "application/json");
				await client.PostAsync(String.Format(ApiBaseUrl, PageId, MetricId), content);
			}
			catch (HttpRequestException ex)
			{
				throw new StatusPagePushException(ex.Message, ex);
			}
			catch (OperationCanceledException ex)
			{
				throw new StatusPagePushException(ex.Message, ex);
			}
		}
	}
}
