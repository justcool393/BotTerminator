using BotTerminator.Configuration;
using BotTerminator.Models;
using Newtonsoft.Json;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
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

		public async Task<BanListConfig> GetConfigAsync()
		{
			await UpdateIfStaleAsync();
			return Cache;
		}

		public async Task<IReadOnlyDictionary<String, Group>> GetAllGroupsAsync()
		{
			await UpdateIfStaleAsync();
			return Cache.GroupLookup;
		}

		public async Task<IReadOnlyCollection<Group>> GetGroupsForUserAsync(String username)
		{
			await UpdateIfStaleAsync();
			return Cache.GetGroupsByUser(username);
		}

		public async Task<Boolean> CheckUserAsync(String name, String group)
		{
			await UpdateIfStaleAsync();
			return Cache.IsInGroup(group, name);
		}

		public async Task UpdateUserAsync(String name, String group, Boolean value, Boolean force)
		{
			if (Cache.GroupLookup.ContainsKey(group))
			{
				if (value)
				{
					Cache.GroupLookup[group].Members.Add(name);
				}
				else
				{
					Cache.GroupLookup[group].Members.Remove(name);
				}
			}
			if (force || IsStale)
			{
				await SrWiki.EditPageAsync(pageName, JsonConvert.SerializeObject(Cache, Formatting.Indented));
				LastUpdatedAtUtc = DateTimeOffset.UtcNow;
			}
		}

		private async Task UpdateIfStaleAsync()
		{
			if (IsStale)
			{
				try
				{
					await GetUpdatedListFromWikiAsync();
					LastUpdatedAtUtc = DateTimeOffset.UtcNow;
				}
				catch (RedditHttpException ex)
				{
					Console.WriteLine("Failed to update cache: {0}", ex.Message);
				}
				catch (OperationCanceledException)
				{
					Console.WriteLine("Failed to update cache: timed out");
				}
			}
		}

		private async Task GetUpdatedListFromWikiAsync()
		{
			String mdData = (await SrWiki.GetPageAsync(pageName)).MarkdownContent;
			Cache = JsonConvert.DeserializeObject<BanListConfig>(mdData);
			Cache.ValidateSupportedVersion(2, 2);
		}

		public async Task<IReadOnlyCollection<Group>> GetDefaultBannedGroupsAsync()
		{
			await UpdateIfStaleAsync();
			return Cache.GetDefaultActionedOnGroups();
		}
	}
}
