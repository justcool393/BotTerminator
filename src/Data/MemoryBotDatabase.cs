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
		protected override Task FlushAsync() => Task.CompletedTask;

		protected override Task ReadNoncachedAsync() => Task.CompletedTask;
	}
}
