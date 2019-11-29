using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Data
{
	public class SQLiteBotDatabase : IBotDatabase
	{
		public Task<Boolean> CheckUserAsync(String name)
		{
			throw new NotImplementedException();
		}

		public Task UpdateUserAsync(String name, Boolean value, Boolean force = false)
		{
			throw new NotImplementedException();
		}
	}
}
