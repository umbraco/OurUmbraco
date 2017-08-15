using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OurUmbraco.Announcements.Models;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Cache;
using Umbraco.Web;

namespace OurUmbraco.Announcements.Services
{
    public class AnnouncementsService
    {
        private const string CacheKey = "OurAnnouncements";

        public static AnnouncementDisplayModel GetAnnouncement(string area, string country, string readAnnouncements)
        {
            // the home node is called community, but referred to in announcements as home
            if (area.Equals("community", StringComparison.OrdinalIgnoreCase))
            {
                area = "home";
            }

            var publishedAnnouncements = GetPublishedAnnouncements();
            var filteredAnnouncements = publishedAnnouncements
                .Where(x => readAnnouncements.Contains(x.Id.ToString()) == false && (x.Area.Equals(area, StringComparison.OrdinalIgnoreCase) || x.Area == "global") &&
                            (string.IsNullOrWhiteSpace(x.GeoLocation) == true ||
                             x.GeoLocation.ToUpper().Split(',').Contains(country))).OrderByDescending(x => x.GeoLocation);

            if (filteredAnnouncements.Any())
            {
                // if there's multiple announcements, we'll remove those that are global (less important)
                if (filteredAnnouncements.Count(x => x.Area == "global") < filteredAnnouncements.Count())
                {
                    filteredAnnouncements = filteredAnnouncements.Where(x => x.Area != "global").OrderByDescending(x => x.GeoLocation);
                }
                return filteredAnnouncements.FirstOrDefault();
            }

            return null;
        }

        public static IEnumerable<AnnouncementDisplayModel> GetPublishedAnnouncements()
        {
            var publishedAnnouncements =
                ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem<IEnumerable<AnnouncementDisplayModel>>(
                    CacheKey,
                    () =>
                    {
                        var announcements = new List<AnnouncementDisplayModel>();
                        var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                        IEnumerable<IPublishedContent> announcementNodes = umbracoHelper.TypedContentAtXPath("/root/announcements/*");
                        foreach (var notification in announcementNodes)
                        {
                            if (notification.DocumentTypeAlias.Equals("announcement", StringComparison.OrdinalIgnoreCase))
                            {
                                var announcement = new AnnouncementDisplayModel()
                                {
                                    Id = notification.GetKey(),
                                    AnnouncementType = notification.GetPropertyValue<string>("type"),
                                    Area = notification.GetPropertyValue<string>("area"),
                                    Content =  notification.GetPropertyValue<string>("content"),
                                    CssClass = notification.GetPropertyValue<string>("design"),
                                    Header = notification.Name,
                                    Permanent = notification.GetPropertyValue<bool>("permanent"),
                                    GeoLocation = notification.GetPropertyValue<string>("geoLocation")
                                };

                                // update links with utm
                                announcement.Content = addGoogleAnalyticsTrackingToLink(announcement.Content,
                                    announcement.AnnouncementType, announcement.Area);
                                announcement.Header = addGoogleAnalyticsTrackingToLink(announcement.Header,
                                    announcement.AnnouncementType, announcement.Area);
                                announcements.Add(announcement);
                            }
                        }
                        return announcements;
                    }, TimeSpan.FromMinutes(5));

            return publishedAnnouncements;

        }

        private static string addGoogleAnalyticsTrackingToLink(string content, string medium, string campaign)
        {
            if (string.IsNullOrWhiteSpace(content) == false)
            {
                content = Regex.Replace(content,
                    @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
                    $"$1?utm_source=our&utm_medium={medium}&utm_content=link&utm_campaign={campaign}");
            }
            return content;
        }
    }
}
