using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BotTerminator.Configuration
{
	[JsonObject]
	public class BanListConfig : BotConfig
	{
		[JsonProperty("bannedUsers")]
		public ISet<String> Items { get; set; } = new HashSet<String>();
	}
}
