using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web.Mvc;
using Umbraco.Core.Cache;
using Umbraco.Web;
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

        public static List<EventFeedModel> GetMeetups()
        {
            return UmbracoContext.Current.Application.ApplicationCache.RuntimeCache.GetCacheItem<List<EventFeedModel>>("EventFeed",
                () =>
                {
                    var eventFeed = new List<EventFeedModel>();
                    using (var client = new HttpClient())
                    {
                        using (var result = client.GetAsync("https://umbracalendar.com/api/EventFeed/").Result)
                        {
                            if (result.IsSuccessStatusCode == false)
                            {
                                // log?
                            }

                            var json = result.Content.ReadAsStringAsync().Result;
                            eventFeed = JsonSerializer.Deserialize<List<EventFeedModel>>(json);
                        }
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