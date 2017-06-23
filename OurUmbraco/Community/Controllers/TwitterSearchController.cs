using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using OurUmbraco.Community.Models;
using OurUmbraco.Forum.Extensions;
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
        public ActionResult TwitterSearchResult(int numberOfResults = 6, bool adminOverview = false)
        {
            var model = new TweetsModel { ShowAdminOverView = adminOverview };
            var member = Members.GetCurrentMember();
            if (member == null || member.IsHq() == false)
                model.ShowAdminOverView = false;

            if (member.IsHq() == false && numberOfResults > 30)
                numberOfResults = 6;

            ITweet[] filteredTweets = {};
            try
            {
                var tweets =
                    ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<ITweet[]>("UmbracoSearchedTweets",
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

                var settingsNode = Umbraco.TypedContentAtRoot().FirstOrDefault();
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

                model.Tweets = filteredTweets;
            }
            catch (Exception ex)
            {
                LogHelper.Error<ITweet>("Could not get tweets", ex);
            }
            
            return PartialView("~/Views/Partials/Home/TwitterSearchUmbraco.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsSpam(string twitterHandle)
        {
            var redirectUrl = HttpContext.Request.UrlReferrer.AbsoluteUri;

            var member = Members.GetCurrentMember();
            var settingsNode = Umbraco.TypedContentAtRoot().FirstOrDefault();
            if (string.IsNullOrEmpty(twitterHandle) 
                || member == null
                || member.IsHq() == false 
                || ModelState.IsValid == false
                || settingsNode == null)
                return Redirect(redirectUrl);
            
            var contentService = ApplicationContext.Services.ContentService;
            var settingsContent = contentService.GetById(settingsNode.Id);
            var twitterFilterAccountsValue = settingsContent.GetValue<string>("twitterFilterAccounts");
            settingsContent.SetValue("twitterFilterAccounts", string.Format("{0},{1}", twitterFilterAccountsValue, twitterHandle));
            var publishStatus = contentService.PublishWithStatus(settingsContent);
            if (publishStatus.Exception == null)
                LogHelper.Info<TwitterSearchController>(string.Format("Twitter handle {0} marked as spam by {1} (id: {2})", twitterHandle, member.Name, member.Id));
            
            return Redirect(redirectUrl);
        }
    }
}
