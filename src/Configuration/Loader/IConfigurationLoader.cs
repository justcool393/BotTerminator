using BotTerminator.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Configuration.Loader
{
	public interface IConfigurationLoader<TSource, TConfig> where TConfig : BotConfig
	{
		Task<TConfig> LoadConfigAsync(TSource sourceData);
	}
}
