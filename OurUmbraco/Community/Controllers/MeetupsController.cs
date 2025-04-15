using OurUmbraco.Forum.Controllers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web.Mvc;
using Umbraco.Core.Cache;
using Umbraco.Web;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Community.Controllers
{

    public class MeetupsController : SurfaceController
    {
        public ActionResult GetEvents()
        {
            var events = GetMeetups();
            return PartialView("~/Views/Partials/Home/Meetups.cshtml", events);
        }

        public List<EventFeedModel> GetMeetups()
        {
            return UmbracoContext.Current.Application.ApplicationCache.RuntimeCache.GetCacheItem<List<EventFeedModel>>("EventFeed",
                () =>
                {
                    var eventFeed = new List<EventFeedModel>();
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var feedUrl = "https://umbracalendar.com/api/EventFeed/";
                            using (var result = client.GetAsync(feedUrl).Result)
                            {
                                if (result.IsSuccessStatusCode == false)
                                {
                                    Logger.Warn(typeof(MeetupsController), $"Failed to get {feedUrl}, result: {result.StatusCode} || {result.ReasonPhrase} ");

                                }

                                var json = result.Content.ReadAsStringAsync().Result;
                                eventFeed = JsonSerializer.Deserialize<List<EventFeedModel>>(json);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(typeof(MeetupsController), "Fetching event feed not successful" + ex.Message, ex);
                        Logger.Error(typeof(MeetupsController), "InnerException" + ex.InnerException.Message, ex.InnerException);
                    }

                    return eventFeed;
                }, TimeSpan.FromHours(1));
        }
    }

    public class EventFeedModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("organizer")]
        public string Organizer { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }
    }
}