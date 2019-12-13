using BotTerminator.Configuration;
using BotTerminator.Configuration.Loader;
using BotTerminator.Data;
using BotTerminator.Models;
using BotTerminator.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Concurrent;
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
		private static readonly IConfigurationLoader<String, SubredditConfig> configurationLoader = new JsonConfigurationLoader<SubredditConfig>();

		public const String CacheFreshenerUserName = "reddit";
		private const String DeletedUserName = "[deleted]";
		public const String HideUrl = "/api/hide";
		public const String NewModCommentsUrl = "/r/mod/comments";
		public const String NewModUrl = "/r/mod/new";
		public const String ModRemovedUrl = "/r/mod/about/spam?only=links";
		public const Int32 PageLimit = 100;
		public const String QuarantineOptInUrl = "/api/quarantine";

		internal ConcurrentQueue<Func<HttpRequestMessage>> StatusPageQueue { get; private set; }
		internal AuthenticationConfig AuthenticationConfig { get; private set; }
		internal GlobalConfig GlobalConfig { get; private set; }
		internal Reddit RedditInstance { get; private set; }
		private IReadOnlyCollection<BotModule> Modules { get; set; }
		internal Dictionary<String, CachedSubreddit> SubredditLookup { get; private set; } = new Dictionary<String, CachedSubreddit>();
		public String SubredditName => AuthenticationConfig.SubredditName;
		public IBotDatabase UserLookup { get; private set; }
		internal IWebAgent WebAgent { get; private set; }

		public BotTerminator(IWebAgent webAgent, Reddit redditInstance, AuthenticationConfig authenticationConfig)
		{
			this.WebAgent = webAgent;
			this.RedditInstance = redditInstance;
			this.AuthenticationConfig = authenticationConfig;
			this.StatusPageQueue = new ConcurrentQueue<Func<HttpRequestMessage>>();
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
			await UserLookup.CheckUserAsync(CacheFreshenerUserName, String.Empty);
			await UpdateSubredditCacheAsync();

			this.Modules = new List<BotModule>()
			{
				new CommentScannerModule(this), new PostScannerModule(this),
				new RemovedPostScannerModule(this), new InviteAcceptorModule(this),
				new CacheFreshenerModule(this), new UpdateBanListModule(this),
				new StatusPageStatusPusherModule(this),
			};

			await Task.WhenAll(Modules.Select(s => s.RunForeverAsync()));
		}

		public async Task CacheSubredditAsync(String subredditName)
		{
			SubredditLookup[subredditName] = new CachedSubreddit(await RedditInstance.GetSubredditAsync(subredditName, false), configurationLoader);
			await SubredditLookup[subredditName].ReloadOptionsAsync(this);
		}

		public async Task UpdateSubredditCacheAsync()
		{
			ICollection<Subreddit> moderatedSubreddits = new List<Subreddit>();
			await RedditInstance.User.GetModeratorSubreddits(-1).ForEachAsync(subreddit =>
			{
				moderatedSubreddits.Add(subreddit);
			});
			foreach (Subreddit subreddit in moderatedSubreddits)
			{
				if (!SubredditLookup.ContainsKey(subreddit.DisplayName))
				{
					SubredditLookup.Add(subreddit.DisplayName, new CachedSubreddit(subreddit, configurationLoader));
					if (IsConfigurable(subreddit))
					{
						try
						{
							await SubredditLookup[subreddit.DisplayName].SaveDefaultOptionSetAsync(GlobalConfig.GlobalOptions, false);
						}
						catch (RedditHttpException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
						{
							Console.WriteLine("Failed to create configuration for subreddit /r/{0}: forbidden", subreddit.DisplayName);
						}
					}
					
				}
				else
				{
					SubredditLookup[subreddit.DisplayName] = new CachedSubreddit(subreddit, configurationLoader);
				}
			}
			await Task.WhenAll(moderatedSubreddits.Select(subreddit => SubredditLookup[subreddit.DisplayName].ReloadOptionsAsync(this)));
		}

		public bool IsConfigurable(Subreddit subreddit)
		{
			return subreddit != null &&
			       !subreddit.DisplayName.Equals(AuthenticationConfig.SubredditName, StringComparison.InvariantCultureIgnoreCase) &&
			       subreddit.ModPermissions.HasFlag(ModeratorPermission.Wiki) &&
				   subreddit["subreddit_type"].Value<String>() != "user";
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

		internal async Task<Boolean> CheckShouldBanAsync(ModeratableThing thing, IEnumerable<String> bannedGroups)
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
			IEnumerable<String> groupNames = (await UserLookup.GetGroupsForUserAsync(thing.AuthorName)).Select(group => group.Name);
			return groupNames.Any(groupName => bannedGroups.Contains(groupName));
		}

		internal async Task<IReadOnlyCollection<Models.Group>> GetBannedGroupsAsync(AbstractSubredditOptionSet options)
		{
			IReadOnlyCollection<String> actioned = options.ActionedUserTypes.ToHashSet();
			return actioned.Count == 0 ? await UserLookup.GetDefaultBannedGroupsAsync() : (await UserLookup.GetAllGroupsAsync()).Values.Where(group => actioned.Contains(group.Name)).ToArray();
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
