using BotTerminator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Configuration
{
	public class SubredditOptionSet : AbstractSubredditOptionSet
	{
		public SubredditOptionSet()
		{

		}

		public SubredditOptionSet(AbstractSubredditOptionSet toCopyFrom)
		{
			// TODO: is there a better way to do this?
			this.Enabled = toCopyFrom.Enabled;
			this.ScanPosts = toCopyFrom.ScanPosts;
			this.ScanComments = toCopyFrom.ScanComments;
			this.BanNote = toCopyFrom.BanNote;
			this.BanMessage = toCopyFrom.BanMessage;
			this.BanDuration = toCopyFrom.BanDuration;
			this.IgnoredUsers = toCopyFrom.IgnoredUsers;
			this.RemovalType = toCopyFrom.RemovalType;
		}

		public override Boolean Enabled { get; set; } = true;

		public override Boolean ScanPosts { get; set; } = true;

		public override Boolean ScanComments { get; set; } = true;

		public override String BanNote { get; set; }

		public override String BanMessage { get; set; }

		public override Int32 BanDuration { get; set; } = 0;

		// TODO: can we use a set here?

		public override IEnumerable<String> IgnoredUsers { get; set; } = new List<String>(0);

		public override IEnumerable<String> ActionedUserTypes { get; set; }

		public override RemovalType RemovalType { get; set; } = RemovalType.Spam;
	}
}
