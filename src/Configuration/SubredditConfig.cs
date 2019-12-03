using BotTerminator.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotTerminator.Configuration
{
	public class SubredditConfig : BotConfig
	{
		[JsonProperty("options", Required = Required.DisallowNull)]
		public SubredditOptionSet Options { get; set; }
	}
}
