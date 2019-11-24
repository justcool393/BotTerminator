using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Exceptions
{
	public class BotTerminatorException : Exception
	{
		public BotTerminatorException()
		{
		}

		public BotTerminatorException(String message) : base(message)
		{
		}

		public BotTerminatorException(String message, Exception innerException) : base(message, innerException)
		{
		}

		protected BotTerminatorException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
