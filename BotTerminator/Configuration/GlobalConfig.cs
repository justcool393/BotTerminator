using BotTerminator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Configuration
{
	public class GlobalConfig : BotConfig
	{
		[JsonProperty("allowOver18")]
		public bool AllowNsfw { get; set; } = true;

		[JsonProperty("allowQuarantined")]
		public bool AllowQuarantined { get; set; } = true;

		[JsonProperty("blockList")]
		public List<GlobalBan> GlobalBanList { get; set; } = new List<GlobalBan>();

		[JsonProperty("defaultSubredditOptions", Required = Required.Always)]
		public SubredditOptions DefaultSubredditOptions { get; set; }

		[JsonProperty("globalIgnoreList")]
		public String[] GlobalIgnoreList { get; set; } = new[]
		{
			"AutoModerator", "reddit",
		};
	}
}
