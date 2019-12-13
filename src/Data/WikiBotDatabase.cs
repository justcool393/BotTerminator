using BotTerminator.Configuration;
using Newtonsoft.Json;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Threading.Tasks;

namespace BotTerminator.Data
{
	public class WikiBotDatabase : IBotDatabase
	{
		private const String pageName = "botconfig/botterminator/banned";

		private Wiki SrWiki { get; set; }

		private BanListConfig Cache { get; set; } = new BanListConfig();

		private DateTimeOffset LastUpdatedAtUtc { get; set; } = DateTimeOffset.MinValue;

		private static readonly TimeSpan staleTimeSpan = new TimeSpan(0, 10, 0);
		public Boolean IsStale => Cache.Count == 0 || DateTimeOffset.UtcNow - LastUpdatedAtUtc > staleTimeSpan;

		public WikiBotDatabase(Subreddit sr)
		{
			this.SrWiki = sr.GetWiki;
		}

		public async Task<Boolean> CheckUserAsync(String name)
		{
			if (IsStale)
			{
				await GetUpdatedListFromWikiAsync();
				LastUpdatedAtUtc = DateTimeOffset.UtcNow;
			}
			throw new NotImplementedException();
			//return Cache.Items.Contains(name);
		}

		public async Task UpdateUserAsync(String name, Boolean value, Boolean force)
		{
			if (value)
			{
				throw new NotImplementedException();
				//Cache.Items.Add(name);
			}
			if (force || IsStale)
			{
				await SrWiki.EditPageAsync(pageName, JsonConvert.SerializeObject(Cache));
				LastUpdatedAtUtc = DateTimeOffset.UtcNow;
			}
		}

		private async Task GetUpdatedListFromWikiAsync()
		{
			String mdData = (await SrWiki.GetPageAsync(pageName)).MarkdownContent;
			Cache = JsonConvert.DeserializeObject<BanListConfig>(mdData);
			Cache.ValidateSupportedVersion(2, 2);
		}
	}
}
