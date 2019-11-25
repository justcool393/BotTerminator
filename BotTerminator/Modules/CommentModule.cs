using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Modules
{
	public class CommentModule : BotModule
	{
		public CommentModule(BotTerminator bot) : base(bot)
		{
		}

		public override async Task RunOnceAsync()
		{
			throw new NotImplementedException();
		}

		public override Task SetupAsync()
		{
			Console.WriteLine("Starting comment scanner module");
			return Task.CompletedTask;
		}

		public override Task TeardownAsync() => Task.CompletedTask;
	}
}
