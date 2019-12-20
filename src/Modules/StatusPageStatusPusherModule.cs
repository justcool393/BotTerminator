using BotTerminator.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class StatusPageStatusPusherModule : BotModule
	{
		private const Int32 MaxRetryValue = 10;
		
		private static readonly TimeSpan StatusPagePushWait = new TimeSpan(0, 0, 10);

		private const String ApiBaseUrl = "https://api.statuspage.io/v1/";

		private const String ApiMetricPushUrl = ApiBaseUrl + "pages/{0}/metrics/data.json";

		private HttpClient statusPageClient;


		public StatusPageStatusPusherModule(BotTerminator bot) : base(bot)
		{
			this.statusPageClient = new HttpClient();
			this.statusPageClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", bot.AuthenticationConfig.StatusPageApiKey);
		}

		public override async Task RunOnceAsync()
		{
			IList<MetricData> metrics = new List<MetricData>();
			while (bot.StatusPageQueueData.TryDequeue(out MetricData metric))
			{
				metrics.Add(metric);
			}

			Func<HttpRequestMessage> request = MakeRequest(metrics);
			while (bot.StatusPageQueue.TryDequeue(out Func<HttpRequestMessage> requestFunction))
			{
				bool success = false;
				int retry = 1;
				while (!success && retry <= MaxRetryValue)
				{
					try
					{
						HttpResponseMessage response = await statusPageClient.SendAsync(request());
						response.EnsureSuccessStatusCode();
						success = true;
					}
					catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException)
					{
						Log.Error(ex, "Failed to push to status page (try {Retry}/{MaxRetryValue}): {ExceptionMessage}", retry, MaxRetryValue, ex.Message);
					}
					finally
					{
						await Task.Delay(StatusPagePushWait);
					}
					retry++;
				}
			}
		}

		private Func<HttpRequestMessage> MakeRequest(IEnumerable<MetricData> metrics)
		{
#warning Memory leak!
			StringContent content = MakeContent(metrics);
			return () => new HttpRequestMessage(HttpMethod.Post, String.Format(ApiMetricPushUrl, bot.GlobalConfig.StatusPagePageId))
			{
				Content = content,
			};
		}

		private StringContent MakeContent(IEnumerable<MetricData> metrics)
		{
			JObject data = new JObject();
			foreach (MetricData metric in metrics)
			{
				data.Add(metric.MetricId, JObject.FromObject(metric));
			}
			data = new JObject(new JProperty("data", data));
			return new StringContent(data.ToString(Newtonsoft.Json.Formatting.None), Encoding.UTF8, "application/json");
		}

		public override Task SetupAsync()
		{
			Log.Information("Starting StatusPage pusher");
			return Task.CompletedTask;
		}

		public override Task TeardownAsync() => Task.CompletedTask;
	}
}
