using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotTerminator.Configuration;
using BotTerminator.Models;

namespace BotTerminator.Data
{
	public class MemoryBotDatabase : IBotDatabase
	{
		private BanListConfig Users { get; set; } = new BanListConfig();

		public Task<BanListConfig> GetConfigAsync() => Task.FromResult(Users);

		public Task<IReadOnlyDictionary<String, Group>> GetAllGroupsAsync() => Task.FromResult<IReadOnlyDictionary<String, Group>>(Users.GroupLookup);

		public Task<Boolean> CheckUserAsync(String username, String groupName) => Task.FromResult(Users.IsInGroup(groupName, username));

		public Task<IReadOnlyCollection<Group>> GetDefaultBannedGroupsAsync() => Task.FromResult(Users.GroupLookup.Where(group => group.Value.ActionByDefault).ToList() as IReadOnlyCollection<Group>);

		public Task<IReadOnlyCollection<Group>> GetGroupsForUserAsync(String username) => Task.FromResult(Users.GetGroupsByUser(username));

		public Task UpdateUserAsync(String name, String groupName, Boolean value, Boolean force = false)
		{
			if (value)
			{
				Users.GroupLookup[groupName].Members.Add(name);
			}
			else
			{
				Users.GroupLookup[groupName].Members.Remove(name);
			}
			return Task.CompletedTask;
		}
	}
}
