using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotTerminator.Configuration
{
	public class WikiSplitConfig : BotConfig
	{
		[JsonProperty("subpageCount")]
		public Int32 Pages { get; set; }
	}
}
