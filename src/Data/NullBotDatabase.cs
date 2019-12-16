using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotTerminator.Configuration;
using BotTerminator.Models;

namespace BotTerminator.Data
{
	public class NullBotDatabase : IBotDatabase
	{
		public Task<Boolean> CheckUserAsync(String username, String groupName) => Task.FromResult(false);

		public Task<BanListConfig> ReadConfigAsync() => Task.FromResult(new BanListConfig());

		public Task WriteConfigAsync(BanListConfig config, bool force) => Task.CompletedTask;

		public Task<IReadOnlyDictionary<String, Group>> GetAllGroupsAsync() => Task.FromResult<IReadOnlyDictionary<String, Group>>(new Dictionary<String, Group>());

		public Task<IReadOnlyCollection<Group>> GetDefaultBannedGroupsAsync() => Task.FromResult(Array.Empty<Group>() as IReadOnlyCollection<Group>);

		public Task<IReadOnlyCollection<Group>> GetGroupsForUserAsync(String name) => Task.FromResult(Array.Empty<Group>() as IReadOnlyCollection<Group>);

		public Task UpdateUserAsync(String username, String groupName, Boolean value, Boolean force = false) => Task.CompletedTask;
	}
}
