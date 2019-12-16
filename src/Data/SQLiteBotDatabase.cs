using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotTerminator.Configuration;
using BotTerminator.Models;

namespace BotTerminator.Data
{
	public class SQLiteBotDatabase : IBotDatabase
	{
		public Task<Boolean> CheckUserAsync(String name)
		{
			throw new NotImplementedException();
		}

		public Task<Boolean> CheckUserAsync(String username, String groupName)
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyDictionary<String, Group>> GetAllGroupsAsync()
		{
			throw new NotImplementedException();
		}

		public Task<BanListConfig> ReadConfigAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyCollection<Group>> GetDefaultBannedGroupsAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyCollection<Group>> GetGroupsForUserAsync(String name)
		{
			throw new NotImplementedException();
		}

		public Task UpdateUserAsync(String name, Boolean value, Boolean force = false)
		{
			throw new NotImplementedException();
		}

		public Task UpdateUserAsync(String username, String groupName, Boolean value, Boolean force = false)
		{
			throw new NotImplementedException();
		}

		public Task WriteConfigAsync(BanListConfig config, Boolean force = false)
		{
			throw new NotImplementedException();
		}
	}
}
