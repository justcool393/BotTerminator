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
		public static void Main(string[] args)
		{
			Console.WriteLine("starting");
			JToken config = LoadConfigDeprecated();
			BotWebAgent botWebAgent = new BotWebAgent(config["username"].Value<String>(), config["password"].Value<String>(), config["clientId"].Value<String>(), config["clientSecret"].Value<String>(), config["redirectUri"].Value<String>());
			Reddit r = new Reddit(botWebAgent, true);
			BotTerminator terminator = new BotTerminator(r, config["srName"].Value<String>());
			terminator.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		[Obsolete]
		private static JToken LoadConfigDeprecated(String configFileName = "config.json")
		{
			String fileData = File.ReadAllText(configFileName);
			return JsonConvert.DeserializeObject<JToken>(fileData);
		}
	}
}
