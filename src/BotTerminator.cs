using BotTerminator.Configuration;
using BotTerminator.Data;
using BotTerminator.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotTerminator
{
	public class BotTerminator
	{
		private static readonly String[] ignoreAuthorCssClasses = new[]
		{
			"btproof", "botbustproof",
		};

		private readonly AuthenticationConfig authConfig;

		public const String CacheFreshenerUserName = "reddit";
		private const String DeletedUserName = "[deleted]";
		public const String HideUrl = "/api/hide";
		public const String NewModCommentsUrl = "/r/mod/comments";
		public const String NewModUrl = "/r/mod/new";
		public const Int32 PageLimit = 100;
		public const String QuarantineOptInUrl = "/api/quarantine";

		internal GlobalConfig GlobalConfig { get; private set; }
		internal Reddit RedditInstance { get; private set; }
		private IReadOnlyCollection<BotModule> Modules { get; set; }
		internal Dictionary<String, Subreddit> SubredditLookup { get; private set; } = new Dictionary<String, Subreddit>();
		internal String SubredditName => authConfig.SubredditName;
		public IBotDatabase UserLookup { get; private set; }
		internal IWebAgent WebAgent { get; private set; }

		public BotTerminator(IWebAgent webAgent, Reddit RedditInstance, AuthenticationConfig authConfig)
		{
			this.WebAgent = webAgent;
			this.RedditInstance = RedditInstance;
			this.authConfig = authConfig; 
			this.Modules = new List<BotModule>()
			{
				new CommentScannerModule(this), new PostScannerModule(this),
				new InviteAcceptorModule(this), new CacheFreshenerModule(this),
				new UpdateBanListModule(this),
			};
		}

		public async Task StartAsync()
		{
			Wiki subredditWiki = (await RedditInstance.GetSubredditAsync(SubredditName, false)).GetWiki;
			try
			{
				const String pageName = "botConfig/botTerminator";
				if (!(await subredditWiki.GetPageNamesAsync()).Contains(pageName.ToLower()))
				{
					GlobalConfig = new GlobalConfig();
					await subredditWiki.EditPageAsync(pageName, JsonConvert.SerializeObject(GlobalConfig), null, "create BotTerminator configuration");
					await subredditWiki.SetPageSettingsAsync(pageName, true, WikiPageSettings.WikiPagePermissionLevel.Mods);
				}
				else
				{
					GlobalConfig = JsonConvert.DeserializeObject<GlobalConfig>((await subredditWiki.GetPageAsync(pageName.ToLower())).MarkdownContent);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to load or create subreddit config: " + ex.Message);
				return;
			}
			UserLookup = new WikiBotDatabase(await RedditInstance.GetSubredditAsync(SubredditName, false));
			await UserLookup.CheckUserAsync(CacheFreshenerUserName);
			await UpdateSubredditCacheAsync();

			await Task.WhenAll(Modules.Select(s => s.RunForeverAsync()));
		}

		public async Task UpdateSubredditCacheAsync()
		{
			await RedditInstance.User.GetModeratorSubreddits(-1).ForEachAsync(subreddit =>
			{
				if (!SubredditLookup.ContainsKey(subreddit.DisplayName))
				{
					SubredditLookup.Add(subreddit.DisplayName, subreddit);
				}
				else
				{
					SubredditLookup[subreddit.DisplayName] = subreddit;
				}
			});
		}

		/// <summary>
		/// Checks whether this user is ineligible to be banned. The only case where
		/// this currently returns <see langword="true" /> is when the <paramref name="thing"/>
		/// or its <see cref="ModeratableThing.AuthorName"/> is <see langword="null"/>, whitespace,
		/// or equal to <see cref="DeletedUserName"/>.
		/// </summary>
		/// <param name="thing">The comment to test for the ability to ban</param>
		/// <returns>A value determining whether the user is not bannable on Reddit.</returns>
		public static Boolean IsUnbannable(ModeratableThing thing) => String.IsNullOrWhiteSpace(thing?.AuthorName) || thing?.AuthorName == DeletedUserName;

		internal async Task<Boolean> CheckShouldBanAsync(ModeratableThing thing)
		{
			if (IsUnbannable(thing)) return false;

			// flair bypass only applies to authors that can have flair
			if (thing is VotableThing votableThing)
			{
				if (!String.IsNullOrWhiteSpace(votableThing.AuthorFlairCssClass) &&
					ignoreAuthorCssClasses.Any(cssClass => votableThing.AuthorFlairCssClass.Contains(cssClass)))
				{
					return false;
				}
			}
			return await UserLookup.CheckUserAsync(thing.AuthorName);
		}

		internal async Task QuarantineOptInAsync(String subredditName)
		{
			const string requestVerb = "POST";
			await WebAgent.ExecuteRequestAsync(() => {
				HttpRequestMessage request = WebAgent.CreateRequest(QuarantineOptInUrl, requestVerb);
				request.Content = new StringContent("sr_name=" + subredditName, Encoding.UTF8);
				return request;
			});
		}
	}
}
