﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public abstract class BotModule
	{
		protected BotTerminator bot;

		public BotModule(BotTerminator bot)
		{
			this.bot = bot;
		}

		public async Task RunForeverAsync()
		{
			await SetupAsync();
			while (bot.GlobalConfig.GlobalOptions.Enabled)
			{
				try
				{
					await RunOnceAsync();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Module {0} failed to run due to {1}: {2}", GetType().Name, ex.GetType().Name, ex.Message);
					Console.WriteLine(ex.ToString());
				}
			}
			await TeardownAsync();
		}

		public abstract Task SetupAsync();

		public abstract Task RunOnceAsync();

		public abstract Task TeardownAsync();
	}
}
