using BotTerminator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Configuration
{
	public abstract class AbstractSubredditOptionSet
	{
		[JsonProperty("enabled")]
		public abstract bool Enabled { get; set; }

		[JsonProperty("scanPosts")]
		public abstract bool ScanPosts { get; set; }

		[JsonProperty("scanComments")]
		public abstract bool ScanComments { get; set; }

		[JsonProperty("banNote")]
		public abstract String BanNote { get; set; }

		[JsonProperty("banMessage")]
		public abstract String BanMessage { get; set; }

		/// <summary>
		/// The ban duration. Setting this to 0 means that the ban is permanent
		/// </summary>
		[JsonProperty("banDuration", Required = Required.DisallowNull)]
		public abstract Int32 BanDuration { get; set; }

		[JsonProperty("actionedUserTypes")]
		public abstract IEnumerable<String> ActionedUserTypes { get; set; }

		[JsonProperty("ignoredUsers")]
		public abstract IEnumerable<String> IgnoredUsers { get; set; }

		/// <summary>
		/// How an item that matches our rules is treated.
		/// </summary>
		/// <see cref="RemovalType"/>
		/// <see cref="RedditSharp.Things.ModeratableThing.RemoveAsync"/>
		/// <see cref="RedditSharp.Things.ModeratableThing.RemoveSpamAsync"/>
		[JsonProperty("removalType")]
		[JsonConverter(typeof(StringEnumConverter))]
		public abstract RemovalType RemovalType { get; set; }
	}
}
