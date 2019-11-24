using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator
{
	public class WikiBotDatabase : IBotDatabase
	{
		private Wiki SrWiki { get; set; }

		public WikiBotDatabase(Subreddit sr)
		{
			this.SrWiki = sr.GetWiki;
		}

		public async Task<Boolean> CheckUserAsync(String name)
		{
			String mdData = (await SrWiki.GetPageAsync("botconfig/botterminator/banned")).MarkdownContent;
			BanList b = JsonConvert.DeserializeObject<BanList>(mdData);
			return b.Items.Contains(name);
		}

		public async Task UpdateUserAsync(String name, Boolean value)
		{
			String mdData = (await SrWiki.GetPageAsync("botconfig/botterminator/banned")).MarkdownContent;
			BanList b = JsonConvert.DeserializeObject<BanList>(mdData);
			b.Items.Add(name);
			await SrWiki.EditPageAsync("botconfig/botterminator/banned", JsonConvert.SerializeObject(b));
		}
	}
}
