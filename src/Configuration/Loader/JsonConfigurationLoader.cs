using BotTerminator.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Configuration.Loader
{
	class JsonConfigurationLoader<T> : IConfigurationLoader<String, T> where T : BotConfig
	{
		public Task<T> LoadConfigAsync(String sourceData)
		{
			return Task.FromResult(JsonConvert.DeserializeObject<T>(sourceData));
		}
	}
}
