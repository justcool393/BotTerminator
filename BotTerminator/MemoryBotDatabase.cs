using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator
{
	public class MemoryBotDatabase : IBotDatabase
	{
		private HashSet<String> Users { get; set; } = new HashSet<string>();

		public Task<Boolean> CheckUserAsync(String name) => Task.FromResult(Users.Contains(name));

		public Task UpdateUserAsync(String name, Boolean value, Boolean force)
		{
			if (value)
			{
				Users.Add(name);
			}
			else
			{
				Users.Remove(name);
			}
			return Task.CompletedTask;
		}
	}
}
