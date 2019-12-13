using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotTerminator.Models;

namespace BotTerminator.Configuration
{
	public class ShadedOptionSet : AbstractSubredditOptionSet
	{
		private const String operationNotSupportedMessage = "Setting this value is not supported on a shaded option set";

		private readonly IReadOnlyCollection<AbstractSubredditOptionSet> optionSets;
		private readonly bool enumerablesAdditive;

		public ShadedOptionSet(IReadOnlyCollection<AbstractSubredditOptionSet> optionSets, bool enumerablesAdditive)
		{
			this.optionSets = optionSets.Where(optionSet => optionSet != null).ToList();
			this.enumerablesAdditive = enumerablesAdditive;
		}


		public override Boolean Enabled
		{
			get
			{
				return optionSets.First().Enabled;
			}
			set => throw new NotSupportedException(operationNotSupportedMessage);
		}

		public override Boolean ScanPosts
		{
			get
			{
				return optionSets.First().ScanPosts;
			}
			set => throw new NotSupportedException(operationNotSupportedMessage);
		}

		public override Boolean ScanComments
		{
			get
			{
				return optionSets.First().ScanComments;
			}
			set => throw new NotSupportedException(operationNotSupportedMessage);
		}

		public override String BanNote
		{
			get
			{
				return optionSets.First(optionSet => optionSet?.BanNote != null).BanNote;
			}
			set => throw new NotSupportedException(operationNotSupportedMessage);
		}

		public override String BanMessage
		{
			get
			{
				return optionSets.First(optionSet => optionSet?.BanMessage != null).BanMessage;
			}
			set => throw new NotSupportedException(operationNotSupportedMessage);
		}

		public override Int32 BanDuration
		{
			get
			{
				return optionSets.First().BanDuration;
			}
			set => throw new NotSupportedException(operationNotSupportedMessage);
		}

		public override IEnumerable<String> IgnoredUsers
		{
			get
			{
				if (enumerablesAdditive)
				{
					return optionSets.SelectMany(optionSet => optionSet.IgnoredUsers);
				}
				else
				{
					return optionSets.First(optionSet => optionSet.IgnoredUsers != null).IgnoredUsers;
				}
			}
			set => throw new NotSupportedException(operationNotSupportedMessage);
		}

		public override RemovalType RemovalType
		{
			get
			{
				return optionSets.First().RemovalType;
			}
			set => throw new NotSupportedException(operationNotSupportedMessage);
		}

		public override IEnumerable<String> ActionedUserTypes
		{
			get
			{
				return optionSets.First(optionSet => optionSet.ActionedUserTypes != null).ActionedUserTypes;
				// TODO: somehow respect enumerablesAdditive
			}
			set => throw new NotSupportedException(operationNotSupportedMessage);
		}
	}
}
