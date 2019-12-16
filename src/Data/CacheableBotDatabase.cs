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

		protected async Task UpdateIfStaleAsync(bool read = true, bool force = false)
		{
			if (force || IsStale)
			{
				if (read)
				{
					await ReadNoncachedAsync();
				}
				else
				{
					await FlushAsync();
				}
				LastUpdated = DateTimeOffset.UtcNow;
			}
		}

		public async Task<BanListConfig> ReadConfigAsync()
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
			await UpdateIfStaleAsync(false, force);
		}

		public async Task WriteConfigAsync(BanListConfig config, Boolean force)
		{
			await UpdateIfStaleAsync(false, force);
		}

		protected abstract Task ReadNoncachedAsync();

		protected abstract Task FlushAsync();
	}
}
