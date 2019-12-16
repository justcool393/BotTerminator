using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Data
{
	public class CacheableBackedBotDatabase : CacheableBotDatabase
	{
		private IBotDatabase Database { get; set; }

		public CacheableBackedBotDatabase(IBotDatabase database, TimeSpan ttl)
		{
			this.Database = database;
			this.Ttl = ttl;
		}

		protected override async Task UpdateAsync()
		{
			Users = await Database.GetConfigAsync();
		}

		protected override async Task UpdateUserAsync(String username, String groupName, Boolean value)
		{
			await Database.UpdateUserAsync(username, groupName, value, true);
		}
	}
}
