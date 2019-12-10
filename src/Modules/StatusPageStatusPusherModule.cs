using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class StatusPageStatusPusherModule : BotModule
	{
		private HttpClient statusPageClient;
		private const Int32 MaxRetryValue = 10;
		private static readonly TimeSpan StatusPagePushWait = new TimeSpan(0, 0, 10);

		public StatusPageStatusPusherModule(BotTerminator bot) : base(bot)
		{
			this.statusPageClient = new HttpClient();
		}

		public override async Task RunOnceAsync()
		{
			while (bot.StatusPageQueue.TryDequeue(out Func<HttpRequestMessage> requestFunction))
			{
				bool success = false;
				int retry = 1;
				while (!success || retry <= MaxRetryValue)
				{
					try
					{
						HttpResponseMessage response = await statusPageClient.SendAsync(requestFunction());
						response.EnsureSuccessStatusCode();
						success = true;
					}
					catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException)
					{
						Console.WriteLine("Failed to push to statuspage (try {0}/{1}): {2}", retry, MaxRetryValue, ex.Message);
					}
					retry++;
					await Task.Delay(StatusPagePushWait);
				}
			}
		}

		public override Task SetupAsync()
		{
			Console.WriteLine("Starting StatusPage pusher");
			return Task.CompletedTask;
		}

		public override Task TeardownAsync()
		{
			throw new NotImplementedException();
		}
	}
}
