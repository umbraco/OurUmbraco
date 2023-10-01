using System;
using System.Collections.Generic;
using System.Linq;
using Skybrud.Social.Mastodon;
using Skybrud.Social.Mastodon.Models.Statuses;
using Skybrud.Social.Mastodon.Options.Timeline;
using Umbraco.Core.Logging;

namespace OurUmbraco.Community.Mastodon
{
    public class MastodonService
    {
        public List<MastodonStatus> GetStatuses(int limit, string maxId = null)
        {
            var cacheKey = "MastodonPosts" + limit;
            
            return (List<MastodonStatus>)Umbraco.Core.ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(
                cacheKey,
                () =>
                {
                    var mastodon = MastodonHttpService.CreateFromDomain("umbracocommunity.social");
                    var options = new MastodonGetHashtagTimelineOptions
                    {
                        Hashtag = "umbraco",
                        Limit = limit,
                        Local = false
                    };

                    try
                    {
                        var response = mastodon.Timelines.GetHashtagTimeline(options);
                        return response.Body.ToList();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<MastodonService>("Failed fetching statuses from the Mastodon API.", ex);
                    }

                    return new List<MastodonStatus>();
                }, TimeSpan.FromMinutes(10));
        }
    }
}