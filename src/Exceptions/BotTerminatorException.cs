using System;
using System.Runtime.Serialization;

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
