using BotTerminator.Configuration;
using BotTerminator.Configuration.Loader;
using Newtonsoft.Json;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Models
{
	public class CachedSubreddit
	{
		private const String pageName = "botconfig/botterminator";
		public const Int32 minSupportedVersion = 1;
		public const Int32 maxSupportedVersion = 1;

		public String Name => RedditSubreddit.DisplayName;
		public Subreddit RedditSubreddit { get; set; }
		public SubredditOptionSet Options { get; set; }

		private IConfigurationLoader<String, SubredditConfig> ConfigurationLoader { get; set; }

		public CachedSubreddit(Subreddit subreddit, IConfigurationLoader<String, SubredditConfig> configurationLoader)
		{
			this.RedditSubreddit = subreddit;
			this.ConfigurationLoader = configurationLoader;
		}

		private async Task<SubredditOptionSet> ReadConfigFromWikiAsync()
		{
			SubredditConfig config = await ConfigurationLoader.LoadConfigAsync((await RedditSubreddit.GetWiki.GetPageAsync(pageName)).MarkdownContent);
			config.ValidateSupportedVersion(minSupportedVersion, maxSupportedVersion);
			return config.Options;
		}
		
		private async Task<bool> PageExistsAsync()
		{
			return (await RedditSubreddit.GetWiki.GetPageNamesAsync()).Contains(pageName);
		}

		public async Task ReloadOptionsAsync()
		{
			// TODO: check permissions
			if (await PageExistsAsync())
			{
				Options = await ReadConfigFromWikiAsync();
			}
			else
			{
				Options = null;
			}
		}

		public async Task<bool> SaveDefaultOptionSetAsync(AbstractSubredditOptionSet defaultSet, bool overrideIfExists = false)
		{
			SubredditOptionSet options = new SubredditOptionSet(defaultSet);
			// TODO: check permissions
			if (!await PageExistsAsync() || overrideIfExists)
			{
				Options = options;
				await RedditSubreddit.GetWiki.EditPageAsync(pageName, JsonConvert.SerializeObject(options), null, "creating new BotTerminator config");
				return true;
			} 
			else
			{
				return false;
			}
		}
	}
}
