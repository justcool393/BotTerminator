using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Configuration
{
	public class SubredditOptions
	{
		[JsonProperty("banNote")]
		public String BanNoteRaw { get; set; }

		[JsonProperty("banMessage")]
		public String BanMessage { get; set; }

		/// <summary>
		/// The ban duration. Setting this to 0 means that the ban is permanent
		/// </summary>
		[JsonProperty("banDuration", Required = Required.DisallowNull)]
		public Int32 BanDuration { get; set; } = 0;

		public String[] IgnoredUsers { get; set; } = new[]
		{
			"AutoModerator", "reddit",
		};

		/// <summary>
		/// How an item that matches our rules is treated. By default, this
		/// is to remove as spam.
		/// </summary>
		/// <see cref="RemovalType"/>
		/// <see cref="RedditSharp.Things.ModeratableThing.RemoveAsync"/>
		/// <see cref="RedditSharp.Things.ModeratableThing.RemoveSpamAsync"/>
		[JsonProperty("removalType")]
		public RemovalType RemovalType { get; set; } = RemovalType.Spam;
	}
}
