using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public abstract class ListingBotModule<T> : BotModule where T : Thing
	{
		protected Listing<T> Listing { get; }
		protected bool RequireUnique { get; }
		protected bool RequireInOrder { get; }

		public ListingBotModule(BotTerminator bot, Listing<T> listing) : base(bot)
		{
			Listing = listing;
		}

		protected abstract bool PreRunItem(T thing);
		protected abstract Task RunItemAsync(T thing);

		public override async Task RunOnceAsync()
		{
			HashSet<String> fullnames = new HashSet<String>();
			List<T> things = new List<T>();

			await Listing.ForEachAsync(thing =>
			{
				if (RequireUnique && fullnames.Contains(thing.FullName)) return;
				if (PreRunItem(thing))
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
		}
	}
}
