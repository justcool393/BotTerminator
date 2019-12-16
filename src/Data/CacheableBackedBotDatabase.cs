using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BotTerminator.Configuration;

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

		protected override async Task ReadNoncachedAsync()
		{
			Users = await Database.ReadConfigAsync();
		}

		protected override async Task FlushAsync()
		{
			await Database.WriteConfigAsync(Users, true);
		}
	}
}
