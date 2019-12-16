using BotTerminator.Configuration;
using BotTerminator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Data
{
	public interface IBotDatabase
	{
		Task<BanListConfig> ReadConfigAsync();

		Task<IReadOnlyDictionary<String, Group>> GetAllGroupsAsync();

		Task<IReadOnlyCollection<Group>> GetGroupsForUserAsync(String name);

		Task<IReadOnlyCollection<Group>> GetDefaultBannedGroupsAsync();

		Task<bool> CheckUserAsync(String username, String groupName);

		Task UpdateUserAsync(String username, String groupName, Boolean value, Boolean force = false);

		Task WriteConfigAsync(BanListConfig config, Boolean force = false);
	}
}
