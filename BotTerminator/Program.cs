using BotTerminator.Configuration;
using Newtonsoft.Json;
using RedditSharp;
using System;
using System.IO;

namespace BotTerminator
{
	class Program
	{
		private const int minSupportedAuthConfigVersion = 1;
		private const int maxSupportedAuthConfigVersion = 1;
		private const String defaultFileName = "config.json";

		public static void Main(string[] args)
		{
			Console.WriteLine("Loading configuration file...");
			String fileName = args.Length != 0 ? args[0] : defaultFileName;
			AuthenticationConfig config = LoadConfigJsonFlatfile(fileName);
			config.ValidateSupportedVersion(minSupportedAuthConfigVersion, maxSupportedAuthConfigVersion);
			BotWebAgent botWebAgent = new BotWebAgent(config.Username, config.Password, config.ClientId, config.ClientSecret, config.RedirectUri)
			{
				//UserAgent = "BotTerminator v1.0.0.0 - /r/" + config.SrName,
			};
			Reddit r = new Reddit(botWebAgent, true);
			Console.WriteLine("Starting BotTerminator...");
			BotTerminator terminator = new BotTerminator(botWebAgent, r, config);
			terminator.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		private static AuthenticationConfig LoadConfigJsonFlatfile(String configFileName = defaultFileName)
		{
			String fileData = File.ReadAllText(configFileName);
			return JsonConvert.DeserializeObject<AuthenticationConfig>(fileData);
		}
	}
}
