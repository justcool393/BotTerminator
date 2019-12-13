using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotTerminator.Models
{
	[JsonObject]
	public class Group
	{
		[JsonProperty("actionByDefault")]
		public bool ActionByDefault { get; private set; }

		[JsonProperty("id", Required = Required.Always)]
		public Guid Id { get; private set; }

		[JsonProperty("name", Required = Required.Always)]
		public String Name { get; private set; }
		
		[JsonProperty("postFlairCssClass", Required = Required.Always)]
		public String PostFlairCssClass { get; private set; }

		[JsonProperty("members")]
		public ISet<String> Members { get; private set; } = new HashSet<String>();
	}
}
