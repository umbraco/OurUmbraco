using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

        public List<EventFeedModel> GetMeetups()
        {
            try
            {
                return UmbracoContext.Current.Application.ApplicationCache.RuntimeCache.GetCacheItem<List<EventFeedModel>>("EventFeed",
                () =>
                {
                    var eventFeed = new List<EventFeedModel>();

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
                            eventFeed = JsonConvert.DeserializeObject<List<EventFeedModel>>(json);
                        }
                    }

                    return eventFeed;
                }, TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(MeetupsController), "Fetching event feed not successful" + ex.Message, ex);
                Logger.Error(typeof(MeetupsController), "InnerException" + ex.InnerException.Message, ex.InnerException);
                return new List<EventFeedModel>();
            } 
        }
    }

    public class EventFeedModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("organizer")]
        public string Organizer { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }
}