using Newtonsoft.Json;
using System;

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

		[JsonProperty("srName", Required = Required.Always)]
		public String SubredditName { get; set; } = "BotTerminator";

		[JsonProperty("statusPageApiKey")]
		public String StatusPageApiKey { get; set; }
	}
}
