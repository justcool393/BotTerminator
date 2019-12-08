using BotTerminator.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotTerminator.Exceptions
{
	public class StatusPagePushException : BotTerminatorException
	{
		public StatusPagePushException(String message) : base(message)
		{
		}

		public StatusPagePushException(String message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
