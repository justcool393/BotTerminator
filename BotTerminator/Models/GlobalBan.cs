using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Models
{
	/// <summary>
	/// Represents a globally applied ban to usage of the bot
	/// </summary>
	public class GlobalBan
	{
		[JsonProperty("fullname", Required = Required.Always)]
		public String Fullname { get; set; }

		[JsonIgnore]
		private bool IsSubreddit => Fullname.StartsWith("t5_");

		[JsonProperty("reason")]
		public String Reason { get; set; }

		[JsonProperty("shouldBlockThroughApi")]
		public bool ShouldBlockThroughApi { get; set; }
	}
}
