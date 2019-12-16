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

		private HttpClient statusPageClient;


		public StatusPageStatusPusherModule(BotTerminator bot) : base(bot)
		{
			this.statusPageClient = new HttpClient();
			this.statusPageClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", bot.AuthenticationConfig.StatusPageApiKey);
		}

		public override async Task RunOnceAsync()
		{
			while (bot.StatusPageQueue.TryDequeue(out Func<HttpRequestMessage> requestFunction))
			{
				bool success = false;
				int retry = 1;
				while (!success && retry <= MaxRetryValue)
				{
					try
					{
						HttpResponseMessage response = await statusPageClient.SendAsync(requestFunction());
						response.EnsureSuccessStatusCode();
						success = true;
					}
					catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException)
					{
						Log.Error(ex, "Failed to push to status page (try {Retry}/{MaxRetryValue}: {ExceptionMessage}", retry, MaxRetryValue, ex.Message);
					}
					finally
					{
						await Task.Delay(StatusPagePushWait);
					}
					retry++;
				}
			}
		}

		public override Task SetupAsync()
		{
			Log.Information("Starting StatusPage pusher");
			return Task.CompletedTask;
		}

		public override Task TeardownAsync() => Task.CompletedTask;
	}
}
