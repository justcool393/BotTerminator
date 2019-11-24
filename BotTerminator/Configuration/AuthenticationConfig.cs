using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Configuration
{
	public class AuthenticationConfig : BotConfig
	{
		[JsonProperty("username", Required = Required.Always)]
		public String Username { get; set; }

		[JsonProperty("password", Required = Required.Always)]
		public String Password { get; set; }

		[JsonProperty("clientID", Required = Required.Always)]
		public String ClientId { get; set; }

		[JsonProperty("clientSecret", Required = Required.Always)]
		public String ClientSecret { get; set; }

		[JsonProperty("redirectUri", Required = Required.Always)]
		public String RedirectUri { get; set; }

		[JsonProperty("srName")]
		public String SrName { get; set; } = "BotTerminator";
	}
}
