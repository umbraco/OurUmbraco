using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using OurUmbraco.Community.Models;
using OurUmbraco.Forum.Extensions;
using TweetSharp;
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

            List<TwitterStatus> filteredTweets = new List<TwitterStatus>();
            try
            {
                var tweets =
                    UmbracoContext.Current.Application.ApplicationCache.RuntimeCache.GetCacheItem<IEnumerable<TwitterStatus>>("UmbracoSearchedTweets",
                        () =>
                        {
                            var service = new TweetSharp.TwitterService(
                    ConfigurationManager.AppSettings["twitterConsumerKey"],
                    ConfigurationManager.AppSettings["twitterConsumerSecret"]);
                            service.AuthenticateWith(
                                ConfigurationManager.AppSettings["twitterUserAccessToken"],
                                ConfigurationManager.AppSettings["twitterUserAccessSecret"]);

                            var options = new SearchOptions
                            {
                                Count = 100,
                                Resulttype = TwitterSearchResultType.Recent,
                                Q = "umbraco"
                            };

                            var results = service.Search(options);
                            return results.Statuses;

                        }, TimeSpan.FromMinutes(2));

                var settingsNode = umbracoHelper.TypedContentAtRoot().FirstOrDefault();
                if (settingsNode != null)
                {
                    var usernameFilter = settingsNode.GetPropertyValue<string>("twitterFilterAccounts")
                        .ToLowerInvariant().Split(',').Where(x => x != string.Empty).ToArray();
                    var wordFilter = settingsNode.GetPropertyValue<string>("twitterFilterWords")
                        .ToLowerInvariant().Split(',').Where(x => x != string.Empty);

                    filteredTweets = tweets.Where(x =>
                            x.Author.ScreenName.ToLowerInvariant().ContainsAny(usernameFilter) == false
                            && x.Text.ToLowerInvariant().ContainsAny(wordFilter) == false
                            && x.Text.StartsWith("RT ") == false)
                        .Take(numberOfResults)
                        .ToList();
                }

                tweetsModel.Tweets = filteredTweets;
            }
            catch (Exception ex)
            {
                LogHelper.Error<TwitterService>("Could not get tweets", ex);
            }

            return tweetsModel;
        }
    }
}
