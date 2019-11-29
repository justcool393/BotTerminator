using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Data
{
	public class NullBotDatabase : IBotDatabase
	{
		public Task<Boolean> CheckUserAsync(String name) => Task.FromResult(false);

		public Task UpdateUserAsync(String name, Boolean value, Boolean force) => Task.CompletedTask;
	}
}
