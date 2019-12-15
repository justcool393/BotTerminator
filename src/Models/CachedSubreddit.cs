using BotTerminator.Configuration;
using BotTerminator.Configuration.Loader;
using BotTerminator.Exceptions;
using Newtonsoft.Json;
using RedditSharp;
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
		private const String pageName = "botConfig/botTerminator";
		public const Int32 minSupportedVersion = 1;
		public const Int32 maxSupportedVersion = 1;

		private static TimeSpan TimeToLive { get; set; } = new TimeSpan(0, 15, 0);

		public String Name => RedditSubreddit.DisplayName;
		public Subreddit RedditSubreddit { get; set; }
		public SubredditConfig SubredditConfig { get; set; }
		public DateTimeOffset LastRefreshedUtc { get; private set; }
		public TimeSpan TimeSinceRefresh => DateTimeOffset.UtcNow - LastRefreshedUtc;

		public SubredditOptionSet Options => SubredditConfig.Options;

		private IConfigurationLoader<String, SubredditConfig> ConfigurationLoader { get; set; }

		public CachedSubreddit(Subreddit subreddit, IConfigurationLoader<String, SubredditConfig> configurationLoader)
		{
			this.RedditSubreddit = subreddit;
			this.ConfigurationLoader = configurationLoader;
			this.SubredditConfig = new SubredditConfig();
		}

		private async Task<SubredditConfig> ReadConfigFromWikiAsync()
		{
			SubredditConfig config = await ConfigurationLoader.LoadConfigAsync((await RedditSubreddit.GetWiki.GetPageAsync(pageName.ToLowerInvariant())).MarkdownContent);
			config.ValidateSupportedVersion(minSupportedVersion, maxSupportedVersion);
			return config;
		}
		
		private async Task<bool> PageExistsAsync()
		{
			return (await RedditSubreddit.GetWiki.GetPageNamesAsync()).Contains(pageName.ToLowerInvariant());
		}

		public async Task ReloadOptionsAsync(BotTerminator bot)
		{
			// TODO: check permissions
			if (!bot.IsConfigurable(RedditSubreddit))
			{
				SubredditConfig = new SubredditConfig();
				return;
			}
			else if (TimeSinceRefresh < TimeToLive && SubredditConfig != null)
			{
				return;
			} 
			try
			{
				// catching the 404 here instead of an if check uses less requests and as such will be quicker
				SubredditConfig = await ReadConfigFromWikiAsync();
				LastRefreshedUtc = DateTimeOffset.UtcNow;
				return;
			}
			catch (RedditHttpException redditException)
			{
				//throw new ConfigurationException("Failed to read config due to Reddit error", redditException);
			}
			catch (JsonReaderException readerException)
			{
				//throw new ConfigurationException("Error reading the config", readerException);
			}
			SubredditConfig = new SubredditConfig();
		}

		public async Task<bool> SaveDefaultOptionSetAsync(AbstractSubredditOptionSet defaultSet, bool overrideIfExists = false)
		{
			SubredditConfig = new SubredditConfig()
			{
				Version = 1,
				Options = new SubredditOptionSet(),
			};
			// TODO: check permissions
			if (overrideIfExists || !await PageExistsAsync())
			{
				await RedditSubreddit.GetWiki.EditPageAsync(pageName, JsonConvert.SerializeObject(SubredditConfig, Formatting.Indented), null, "creating new BotTerminator config");
				await RedditSubreddit.GetWiki.SetPageSettingsAsync(pageName, true, WikiPageSettings.WikiPagePermissionLevel.Mods);
				return true;
			} 
			else
			{
				return false;
			}
		}
	}
}
