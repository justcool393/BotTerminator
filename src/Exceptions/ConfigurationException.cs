using BotTerminator.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BotTerminator.Exceptions
{
	public class ConfigurationException : BotTerminatorException
	{
		private const String baseExceptionMessage = "The configuration that was loaded is invalid";

		public ConfigurationException() : this(baseExceptionMessage)
		{
		}

		public ConfigurationException(String message) : base(message)
		{
		}

		public ConfigurationException(String message, Exception innerException) : base(message, innerException)
		{
		}

		protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
