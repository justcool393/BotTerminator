using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator
{
	[JsonObject]
	public class BanList
	{
		[JsonProperty("configVersion")]
		public Int64 Version { get; set; }

		[JsonProperty("bannedUsers")]
		public List<String> Items { get; set; }
	}
}
