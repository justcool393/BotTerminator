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

		[JsonProperty("globalOptions", Required = Required.Always)]
		public SubredditOptionSet GlobalOptions { get; set; }

		[JsonProperty("banListMetricId")]
		public String BanListMetricId { get; set; }

		[JsonProperty("inviteAcceptorMetricId")]
		public String InviteAcceptorMetricId { get; set; }

		[JsonProperty("metricIds")]
		public IReadOnlyDictionary<String, String> MetricIds { get; set; }

		[JsonProperty("statusPagePageId")]
		public String StatusPagePageId { get; set; }

		[JsonProperty("globalIgnoreList")]
		public IReadOnlyList<String> GlobalIgnoreList { get; set; } = new List<String>(2)
		{
			"AutoModerator", "reddit",
		};
	}
}
