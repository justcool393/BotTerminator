using BotTerminator.Exceptions;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public abstract class ListingBotModule<T> : StatusPagePushableBotModule where T : Thing
	{
		protected Listing<T> Listing { get; }
		protected sealed override String MetricId { get; }
		protected bool RequireUnique { get; set; }
		protected bool RequireInOrder { get; set; }

		protected ListingBotModule(BotTerminator bot, String metricId, Listing<T> listing) : base(bot)
		{
			MetricId = metricId;
			Listing = listing;
		}

		protected abstract bool PreRunItem(T thing);
		protected abstract Task RunItemAsync(T thing);

		protected abstract Task PostRunItemsAsync(ICollection<T> things);

		public override sealed async Task RunOnceAsync()
		{
			HashSet<String> fullnames = new HashSet<String>();
			ICollection<T> things = new List<T>();

			await Listing.ForEachAsync(thing =>
			{
				if (RequireUnique && fullnames.Contains(thing.FullName)) return;
				if (thing != null && PreRunItem(thing))
				{
					fullnames.Add(thing.FullName);
					things.Add(thing);
				}
			});
			if (RequireInOrder)
			{
				foreach (T thing in things)
				{
					await RunItemAsync(thing);
				}
			}
			else
			{
				await Task.WhenAll(things.Select(thing => RunItemAsync(thing)));
			}
			try
			{
				await PushMetricAsync(new Models.MetricData()
				{
					Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
					Value = things.Count,
				});
			}
			catch (StatusPagePushException ex)
			{
				if (!(ex.InnerException is ArgumentNullException))
				{
					Log.Warning("Failed to push to StatusPage: {0}", ex.Message);
				}
			}
			await PostRunItemsAsync(things);
		}
	}
}
