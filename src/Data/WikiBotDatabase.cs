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
	public abstract class WikiBotDatabase : IBotDatabase
	{
		/// <summary>
		/// Maximum wiki page size on Reddit. This information is source from
		/// <a href="https://github.com/reddit-archive/reddit/blob/master/r2/example.ini#L484">GitHub</a>.
		/// </summary>
		protected const Int32 WikiPageMaxSize = 256 * 1024;

		protected Wiki SubredditWiki { get; private set; }
		protected String PageName { get; private set; }

		protected WikiBotDatabase(Wiki wiki, String pageName)
		{
			this.SubredditWiki = wiki;
			this.PageName = pageName;
		}

		public abstract Task<BanListConfig> ReadConfigAsync();

		public abstract Task WriteConfigAsync(BanListConfig config, bool force);

		public async Task<IReadOnlyDictionary<String, Group>> GetAllGroupsAsync()
		{
			return (await ReadConfigAsync()).GroupLookup;
		}

		public async Task<IReadOnlyCollection<Group>> GetGroupsForUserAsync(String username)
		{
			return (await ReadConfigAsync()).GetGroupsByUser(username);
		}

		public async Task<Boolean> CheckUserAsync(String name, String group)
		{
			return (await ReadConfigAsync()).IsInGroup(group, name);
		}

		public async Task UpdateUserAsync(String name, String group, Boolean value, Boolean force)
		{
			BanListConfig config = await ReadConfigAsync();
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
			await WriteConfigAsync(config, force);
		}

		public async Task<IReadOnlyCollection<Group>> GetDefaultBannedGroupsAsync()
		{
			return (await ReadConfigAsync()).GetDefaultActionedOnGroups();
		}
	}
}
