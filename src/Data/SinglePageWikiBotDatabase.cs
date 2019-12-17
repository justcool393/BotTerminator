using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BotTerminator.Configuration;
using Newtonsoft.Json;
using RedditSharp;

namespace BotTerminator.Data
{
	public class SinglePageWikiBotDatabase : WikiBotDatabase
	{
		public SinglePageWikiBotDatabase(Wiki wiki, String pageName) : base(wiki, pageName)
		{
		}

		public override async Task<BanListConfig> ReadConfigAsync()
		{
			String mdData = (await SubredditWiki.GetPageAsync(PageName)).MarkdownContent;
			BanListConfig config = JsonConvert.DeserializeObject<BanListConfig>(mdData);
			config.ValidateSupportedVersion(2, 2);
			return config;
		}

		public override async Task WriteConfigAsync(BanListConfig config, bool force)
		{
			await SubredditWiki.EditPageAsync(PageName, JsonConvert.SerializeObject(config, Formatting.Indented));
		}
	}
}
