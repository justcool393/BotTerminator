using BotTerminator.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Configuration
{
	public class BotConfig
	{
		[JsonProperty("configVersion")]
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
