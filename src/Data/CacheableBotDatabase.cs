using BotTerminator.Configuration;
using BotTerminator.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Data
{
	public abstract class CacheableBotDatabase : IBotDatabase
	{
		protected BanListConfig Users { get; set; }

		protected DateTimeOffset LastUpdated { get; set; }

		protected TimeSpan Ttl { get; set; } = TimeSpan.MaxValue;
		public Boolean IsStale => DateTimeOffset.UtcNow - LastUpdated > Ttl;

		protected async Task UpdateIfStaleAsync()
		{
			if (IsStale)
			{
				await UpdateAsync();
				LastUpdated = DateTimeOffset.UtcNow;
			}
		}

		protected abstract Task UpdateAsync();

		protected abstract Task UpdateUserAsync(String username, String groupName, Boolean value);

		public async Task<BanListConfig> GetConfigAsync()
		{
			await UpdateIfStaleAsync();
			return Users;
		}

		public async Task<IReadOnlyDictionary<String, Group>> GetAllGroupsAsync()
		{
			await UpdateIfStaleAsync();
			return Users.GroupLookup;
		}

		public async Task<IReadOnlyCollection<Group>> GetGroupsForUserAsync(String name)
		{
			await UpdateIfStaleAsync();
			return Users.GetGroupsByUser(name);
		}

		public async Task<IReadOnlyCollection<Group>> GetDefaultBannedGroupsAsync()
		{
			await UpdateIfStaleAsync();
			return Users.GetDefaultActionedOnGroups();
		}

		public async Task<Boolean> CheckUserAsync(String username, String groupName)
		{
			await UpdateIfStaleAsync();
			return Users.IsInGroup(groupName, username);
		}

		public async Task UpdateUserAsync(String username, String groupName, Boolean value, Boolean force = false)
		{
			if (Users.GroupLookup.ContainsKey(groupName))
			{
				if (value)
				{
					Users.GroupLookup[groupName].Members.Add(username);
				}
				else
				{
					Users.GroupLookup[groupName].Members.Remove(username);
				}
			}
			if (force || IsStale)
			{
				await UpdateUserAsync(username, groupName, value);
				LastUpdated = DateTimeOffset.UtcNow;
			}
		}
	}
}
