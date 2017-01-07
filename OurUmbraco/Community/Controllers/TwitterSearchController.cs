using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Community.Controllers
{
    public class TwitterSearchController : SurfaceController
    {
        public ActionResult TwitterSearchResult(string searchWord = "umbraco")
        {
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

                        var searchParameter = new SearchTweetsParameters(searchWord) { SearchType = SearchResultType.Recent };
                        return Search.SearchTweets(searchParameter).ToArray();

                    }, TimeSpan.FromMinutes(2));

                var usernameFilter = "Technologx,Technologx4Real,SOAzure,AdamSmith1,AdamSmitht1,UriSamuels,coding_jobfeeds,TechSparkUk,ItProjectBoard,liveedutv,AdeboyejoAde,VivacitySocial,CHHCInc,DevOpsBlogs,ItCrowdSource,EquiKey".ToLowerInvariant().Split(',');
                var wordFilter = "exegesis,#Job Alert:,#pigefodboldbsf".ToLowerInvariant().Split(',');

                filteredTweets = tweets.Where(
                    x => x.Urls.Any(u => u.ExpandedURL.Contains("umbraco-proxy.com")) == false
                    && x.CreatedBy.UserIdentifier.ScreenName.ToLowerInvariant().ContainsAny(usernameFilter) == false
                    && x.Text.ToLowerInvariant().ContainsAny(wordFilter) == false).ToArray();
            }
            catch (Exception ex)
            {
                LogHelper.Error<ITweet>("Could not get tweets", ex);
            }

            return PartialView("~/Views/Partials/Home/TwitterSearchUmbraco.cshtml", filteredTweets);
        }
    }
}
