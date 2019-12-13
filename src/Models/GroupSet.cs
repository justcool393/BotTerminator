using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotTerminator.Models
{
	public class GroupSet
	{
		[JsonProperty("groups")]
		public Dictionary<String, Group> GroupLookup { get; set; }

		public bool IsInGroup(String groupCssClass, String username)
		{
			return GroupLookup[groupCssClass].Members.Contains(username);
		}

		public bool IsInAnyGroup(String username)
		{
			return GroupLookup.Values.Any(group => group.Members.Contains(username));
		}
	}
}
