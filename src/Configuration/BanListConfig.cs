using BotTerminator.Models;
using Newtonsoft.Json;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BotTerminator.Configuration
{
	[JsonObject]
	public class BanListConfig : BotConfig
	{
		private const Int32 CurrentConfigVersion = 2;

		public BanListConfig(int version = CurrentConfigVersion)
		{
			this.Version = version;
		}

		[JsonProperty("nonGroupFlairCssClasses", DefaultValueHandling = DefaultValueHandling.Populate)]
		public IReadOnlyCollection<String> NonGroupFlairCssClasses { get; set; } = Array.Empty<String>();

		[JsonProperty("groups")]
		public Dictionary<String, Group> GroupLookup { get; set; } = new Dictionary<string, Group>();

		[JsonIgnore]
		public Int32 Count => GroupLookup.Values.Sum(group => group.Members.Count);

		public IEnumerable<String> GetAllNames()
		{
			return NonGroupFlairCssClasses.Concat(GroupLookup.Keys);
		}

		public IReadOnlyCollection<Group> GetDefaultActionedOnGroups()
		{
			return GroupLookup.Values.Where(group => group.ActionByDefault).ToArray();
		}

		public IReadOnlyCollection<Group> GetGroupsByUser(String username)
		{
			return GroupLookup.Values.Where(group => group.Members.Contains(username)).ToArray();
		}

		public bool IsInGroup(String groupCssClass, String username)
		{
			return GroupLookup.ContainsKey(groupCssClass) && GroupLookup[groupCssClass].Members.Contains(username);
		}

		public bool IsInAnyGroup(String username)
		{
			return GroupLookup.Values.Any(group => group.Members.Contains(username));
		}

		public bool ShouldHide(Post post)
		{
			String cssClass = post.LinkFlairCssClass;
			return IsInAnyGroup(cssClass) || NonGroupFlairCssClasses.Contains(cssClass);
		}
	}
}
