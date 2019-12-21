using BotTerminator.Configuration;
using Newtonsoft.Json;
using RedditSharp;
using Serilog;
using System;
using System.IO;

namespace BotTerminator
{
	class Program
	{
		private const int minSupportedAuthConfigVersion = 1;
		private const int maxSupportedAuthConfigVersion = 1;
		private const String defaultFileName = "config.json";
		private const String defaultLogFileName = "BotTerminator_{Date}.log";

		public static void Main(string[] args)
		{
			String logFileName = args.Length > 1 ? args[1] : defaultLogFileName;
			Console.WriteLine("Loading logging...");
			ILogger log;
			try
			{
				log = LoadLogConfig(logFileName);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to load log. Cannot continue.");
				Console.WriteLine(ex);
				return;
			}
			log.Information("Loading configuration file...");
			String fileName = args.Length != 0 ? args[0] : defaultFileName;
			AuthenticationConfig config = LoadConfigJsonFlatfile(fileName);
			config.ValidateSupportedVersion(minSupportedAuthConfigVersion, maxSupportedAuthConfigVersion);
			BotWebAgent botWebAgent = new BotWebAgent(config.Username, config.Password, config.ClientId, config.ClientSecret, config.RedirectUri)
			{
				//UserAgent = "BotTerminator v1.0.0.0 - /r/" + config.SrName,
			};
			Reddit r = new Reddit(botWebAgent, true);
			BotTerminator terminator = new BotTerminator(botWebAgent, r, config, log);
			terminator.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		private static ILogger LoadLogConfig(String logFileName = defaultLogFileName)
		{
			return new LoggerConfiguration()
				   .MinimumLevel.Verbose()
				   .WriteTo.Console()
				   .WriteTo.RollingFile(logFileName, retainedFileCountLimit: null)
				   .CreateLogger();
		}

		private static AuthenticationConfig LoadConfigJsonFlatfile(String configFileName = defaultFileName)
		{
			String fileData = File.ReadAllText(configFileName);
			return JsonConvert.DeserializeObject<AuthenticationConfig>(fileData);
		}
	}
}
