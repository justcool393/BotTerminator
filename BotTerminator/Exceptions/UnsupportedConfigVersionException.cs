using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Exceptions
{
	public class UnsupportedConfigVersionException : BotTerminatorException
	{
		private const String baseExceptionMessage = "The configuration version that was loaded is not supported";

		public Int32 MinSupported { get; private set; }
		public Int32 MaxSupported { get; private set; }
		public Int32 Requested { get; private set; }


		public UnsupportedConfigVersionException(Int32 minSupported, Int32 maxSupported, Int32 requested) : this(baseExceptionMessage, minSupported, maxSupported, requested)
		{
		}

		public UnsupportedConfigVersionException(String message, Int32 minSupported, Int32 maxSupported, Int32 requested) : base(message)
		{
			MinSupported = minSupported;
			MaxSupported = maxSupported;
			Requested = requested;
		}

		public UnsupportedConfigVersionException(String message, Int32 minSupported, Int32 maxSupported, Int32 requested, Exception innerException) : base(message, innerException)
		{
			MinSupported = minSupported;
			MaxSupported = maxSupported;
			Requested = requested;
		}

		protected UnsupportedConfigVersionException(Int32 minSupported, Int32 maxSupported, Int32 requested, SerializationInfo info, StreamingContext context) : base(info, context)
		{
			MinSupported = minSupported;
			MaxSupported = maxSupported;
			Requested = requested;
		}
	}
}
