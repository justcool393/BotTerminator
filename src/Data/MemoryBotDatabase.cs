using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotTerminator.Configuration;
using BotTerminator.Models;

namespace BotTerminator.Data
{
	public class MemoryBotDatabase : CacheableBotDatabase
	{
		protected override Task UpdateAsync() => Task.CompletedTask;

		protected override Task UpdateUserAsync(String username, String groupName, Boolean value) => Task.CompletedTask;
	}
}
