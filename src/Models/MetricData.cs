using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotTerminator.Models
{
	[JsonObject]
	public class MetricData
	{
		[JsonProperty("timestamp")]
		public Int64 Timestamp { get; set; }

		[JsonProperty("value")]
		public Single Value { get; set; }
	}
}
