using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Community.Controllers
{
    public class TwitterSearchController : SurfaceController
    {
        public ActionResult TwitterSearchResult(int numberOfResults = 6)
        {
            if (numberOfResults > 30)
                numberOfResults = 6;
            
            ITweet[] filteredTweets = { };
            try
            {
                var tweets = ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<ITweet[]>("UmbracoSearchedTweets",
                    () =>
                    {
                        Auth.SetUserCredentials(ConfigurationManager.AppSettings["twitterConsumerKey"],
                            ConfigurationManager.AppSettings["twitterConsumerSecret"],
                            ConfigurationManager.AppSettings["twitterUserAccessToken"],
                            ConfigurationManager.AppSettings["twitterUserAccessSecret"]);
                        Tweetinvi.User.GetAuthenticatedUser();

                        var searchParameter = new SearchTweetsParameters("umbraco") { SearchType = SearchResultType.Recent };
                        return Search.SearchTweets(searchParameter).ToArray();

                    }, TimeSpan.FromMinutes(2));

                var settingsNode = Umbraco.TypedContentAtRoot().FirstOrDefault();
                if (settingsNode != null)
                {
                    var usernameFilter = settingsNode.GetPropertyValue<string>("twitterFilterAccounts")
                        .ToLowerInvariant().Split(',').Where(x => x != string.Empty);
                    var wordFilter = settingsNode.GetPropertyValue<string>("twitterFilterWords")
                        .ToLowerInvariant().Split(',').Where(x => x != string.Empty);

                    filteredTweets = tweets.Where(x => 
                            x.CreatedBy.UserIdentifier.ScreenName.ToLowerInvariant().ContainsAny(usernameFilter) == false
                            && x.Text.ToLowerInvariant().ContainsAny(wordFilter) == false)
                        .Take(numberOfResults)
                        .ToArray();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<ITweet>("Could not get tweets", ex);
            }

            return PartialView("~/Views/Partials/Home/TwitterSearchUmbraco.cshtml", filteredTweets);
        }
    }
}
