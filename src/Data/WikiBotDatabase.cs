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

		public WikiBotDatabase(Subreddit sr)
		{
			this.SrWiki = sr.GetWiki;
		}

		public async Task<BanListConfig> GetConfigAsync()
		{
			String mdData = (await SrWiki.GetPageAsync(pageName)).MarkdownContent;
			BanListConfig config = JsonConvert.DeserializeObject<BanListConfig>(mdData);
			config.ValidateSupportedVersion(2, 2);
			return config;
		}

		public async Task<IReadOnlyDictionary<String, Group>> GetAllGroupsAsync()
		{
			return (await GetConfigAsync()).GroupLookup;
		}

		public async Task<IReadOnlyCollection<Group>> GetGroupsForUserAsync(String username)
		{
			return (await GetConfigAsync()).GetGroupsByUser(username);
		}

		public async Task<Boolean> CheckUserAsync(String name, String group)
		{
			return (await GetConfigAsync()).IsInGroup(group, name);
		}

		public async Task UpdateUserAsync(String name, String group, Boolean value, Boolean force)
		{
			BanListConfig config = await GetConfigAsync();
			if (config.GroupLookup.ContainsKey(group))
			{
				if (value)
				{
					config.GroupLookup[group].Members.Add(name);
				}
				else
				{
					config.GroupLookup[group].Members.Remove(name);
				}
			}
			if (force)
			{
				await SrWiki.EditPageAsync(pageName, JsonConvert.SerializeObject(config, Formatting.Indented));
			}
		}

		public async Task<IReadOnlyCollection<Group>> GetDefaultBannedGroupsAsync()
		{
			return (await GetConfigAsync()).GetDefaultActionedOnGroups();
		}
	}
}
