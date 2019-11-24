using BotTerminator.Configuration;
using BotTerminator.Exceptions;
using JcBotAuth.Reddit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator
{
	class Program
	{
		private const int minSupportedAuthConfigVersion = 1;
		private const int maxSupportedAuthConfigVersion = 1;

		public static void Main(string[] args)
		{
			Console.WriteLine("starting");
			AuthenticationConfig config = LoadConfigFlatfile();
			config.ValidateSupportedVersion(minSupportedAuthConfigVersion, maxSupportedAuthConfigVersion);
			BotWebAgent botWebAgent = new BotWebAgent(config.Username, config.Password, config.ClientId, config.ClientSecret, config.RedirectUri)
			{
				//UserAgent = "BotTerminator v1.0.0.0 - /r/" + config.SrName,
			};
			Reddit r = new Reddit(botWebAgent, true);
			BotTerminator terminator = new BotTerminator(botWebAgent, r, config);
			terminator.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		private static AuthenticationConfig LoadConfigFlatfile(String configFileName = "config.json")
		{
			String fileData = File.ReadAllText(configFileName);
			return JsonConvert.DeserializeObject<AuthenticationConfig>(fileData);
		}
	}
}
