using BotTerminator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Configuration
{
	public class SubredditOptions : AbstractSubredditOptionSet
	{
		public override String BanNote { get; set; }

		public override String BanMessage { get; set; }

		public override Int32 BanDuration { get; set; } = 0;

		public override IEnumerable<String> IgnoredUsers { get; set; } = new List<String>(2)
		{
			"AutoModerator", "reddit",
		};

		public override RemovalType RemovalType { get; set; } = RemovalType.Spam;
	}
}
