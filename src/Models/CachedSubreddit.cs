using BotTerminator.Configuration;
using BotTerminator.Configuration.Loader;
using Newtonsoft.Json;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Models
{
	public class CachedSubreddit
	{
		const String pageName = "botconfig/botterminator";

		public String Name { get; set; }
		public Subreddit RedditSubreddit { get; set; }
		public SubredditOptionSet Options { get; set; }

		private IConfigurationLoader<String, SubredditConfig> Loader { get; set; }

		private async Task<SubredditOptionSet> ReadConfigFromWikiAsync()
		{
			return (await Loader.LoadConfigAsync((await RedditSubreddit.GetWiki.GetPageAsync(pageName)).MarkdownContent)).Options;
		}

		public async Task ReloadOptionsAsync()
		{

		}

		public async Task SaveDefaultOptionSetAsync(AbstractSubredditOptionSet defaultSet, bool overrideIfExists = false)
		{
			SubredditOptionSet options = new SubredditOptionSet(defaultSet);
		}
	}
}
