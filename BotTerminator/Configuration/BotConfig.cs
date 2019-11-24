using BotTerminator.Exceptions;
using Newtonsoft.Json;
using System;

namespace BotTerminator.Configuration
{
	public class BotConfig
	{
		[JsonProperty("configVersion", Required = Required.Always)]
		public Int32 Version { get; set; }

		public void ValidateSupportedVersion(int minVersion, int maxVersion)
		{
			if (Version < minVersion || Version > maxVersion)
			{
				throw new UnsupportedConfigVersionException(minVersion, maxVersion, Version);
			}
		}
	}
}
