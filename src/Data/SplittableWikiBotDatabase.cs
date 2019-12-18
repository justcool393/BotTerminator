using BotTerminator.Configuration;
using BotTerminator.Exceptions;
using Newtonsoft.Json;
using RedditSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminator.Data
{
	public class SplittableWikiBotDatabase : WikiBotDatabase
	{
		public SplittableWikiBotDatabase(Wiki wiki, String pageName) : base(wiki, pageName)
		{
			
		}

		public override async Task<BanListConfig> ReadConfigAsync()
		{
			WikiSplitConfig meta = JsonConvert.DeserializeObject<WikiSplitConfig>((await SubredditWiki.GetPageAsync(PageName)).MarkdownContent);
			meta.ValidateSupportedVersion(1, 1);
			IList<Task<String>> tasks = new List<Task<String>>();
			for (int i = 1; i <= meta.Pages; i++)
			{
				tasks.Add(ReadSplit(i));
			}
			await Task.WhenAll(tasks);
			StringBuilder sb = new StringBuilder();
			foreach (Task<String> configPageTask in tasks)
			{
				if (configPageTask.IsFaulted)
				{
					throw new ConfigurationException("Failed to load subpage from wiki", configPageTask.Exception);
				}
				sb.Append(configPageTask.Result);
			}
			BanListConfig config = JsonConvert.DeserializeObject<BanListConfig>(sb.ToString());
			config.ValidateSupportedVersion(2, 2);
			return config;
		}

		public override async Task WriteConfigAsync(BanListConfig config, Boolean force)
		{
			String unsplit = JsonConvert.SerializeObject(config, Formatting.Indented);
			IReadOnlyList<String> split = SplitString(unsplit, WikiPageMaxSize - 1).ToArray();
			IList<Task> tasks = new List<Task>();
			for (int i = 0; i < split.Count; i++)
			{
				tasks.Add(WriteSplit(split[i], i));
			}
			await Task.WhenAll(tasks);
		}

		private async Task<String> ReadSplit(Int32 subpage)
		{
			return (await SubredditWiki.GetPageAsync(PageName + "/" + subpage.ToString())).MarkdownContent;
		}

		private async Task WriteSplit(String portion, Int32 subpage)
		{
			await SubredditWiki.EditPageAsync(PageName + "/" + subpage.ToString(), portion);
		}

		private static IEnumerable<String> SplitString(String toSplit, int splitSize)
		{
			for (int i = 0; i < toSplit.Length; i += splitSize)
			{
				yield return toSplit.Substring(i, Math.Min(splitSize, toSplit.Length - i));
			}
		}
	}
}
