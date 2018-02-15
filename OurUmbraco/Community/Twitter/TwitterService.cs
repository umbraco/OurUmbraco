using System;
using System.Configuration;
using System.Linq;
using OurUmbraco.Community.Models;
using OurUmbraco.Forum.Extensions;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using umbraco.MacroEngines;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Web;

namespace OurUmbraco.Community.Twitter
{
    public class TwitterService
    {
        public TweetsModel GetTweets(int numberOfResults, bool adminOverview)
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var tweetsModel = new TweetsModel { ShowAdminOverView = adminOverview };
            var member = umbracoHelper.MembershipHelper.GetCurrentMember();
            if (member == null || member.IsHq() == false)
                tweetsModel.ShowAdminOverView = false;

            ITweet[] filteredTweets = { };
            try
            {
                var tweets =
                    UmbracoContext.Current.Application.ApplicationCache.RuntimeCache.GetCacheItem<ITweet[]>("UmbracoSearchedTweets",
                        () =>
                        {
                            Auth.SetUserCredentials(ConfigurationManager.AppSettings["twitterConsumerKey"],
                                ConfigurationManager.AppSettings["twitterConsumerSecret"],
                                ConfigurationManager.AppSettings["twitterUserAccessToken"],
                                ConfigurationManager.AppSettings["twitterUserAccessSecret"]);
                            Tweetinvi.User.GetAuthenticatedUser();

                            var searchParameter = new SearchTweetsParameters("umbraco")
                            {
                                SearchType = SearchResultType.Recent
                            };
                            return Search.SearchTweets(searchParameter).ToArray();

                        }, TimeSpan.FromMinutes(2));
                
                var settingsNode = umbracoHelper.TypedContentAtRoot().FirstOrDefault();
                if (settingsNode != null)
                {
                    var usernameFilter = settingsNode.GetPropertyValue<string>("twitterFilterAccounts")
                        .ToLowerInvariant().Split(',').Where(x => x != string.Empty).ToArray();
                    var wordFilter = settingsNode.GetPropertyValue<string>("twitterFilterWords")
                        .ToLowerInvariant().Split(',').Where(x => x != string.Empty);

                    filteredTweets = tweets.Where(x =>
                            x.CreatedBy.UserIdentifier.ScreenName.ToLowerInvariant().ContainsAny(usernameFilter) ==
                            false
                            && x.UserMentions.Any(m => m.ScreenName.ContainsAny(usernameFilter)) == false
                            && x.Text.ToLowerInvariant().ContainsAny(wordFilter) == false
                            && x.Text.StartsWith("RT ") == false)
                        .Take(numberOfResults)
                        .ToArray();
                }

                tweetsModel.Tweets = filteredTweets;
            }
            catch (Exception ex)
            {
                LogHelper.Error<ITweet>("Could not get tweets", ex);
            }

            return tweetsModel;
        }
    }
}
