using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BotTerminator.Configuration
{
	[JsonObject]
	public class BanListConfig
	{
		[JsonProperty("configVersion")]
		public Int64 Version { get; set; } = 1;

		[JsonProperty("bannedUsers")]
		public HashSet<String> Items { get; set; } = new HashSet<String>();
	}
}
