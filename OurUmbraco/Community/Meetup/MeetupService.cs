using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Newtonsoft.Json;
using OurUmbraco.Community.Controllers;
using OurUmbraco.Community.Meetup.Models;
using OurUmbraco.Community.Models;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Social.Meetup;
using Skybrud.Social.Meetup.Models.Events;
using Skybrud.Social.Meetup.Models.Groups;
using Skybrud.Social.Meetup.OAuth;
using Skybrud.Social.Meetup.Responses.Events;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Web;

namespace OurUmbraco.Community.Meetup
{
    public class MeetupService
    {
        public void UpdateMeetupStats()
        {
            var configPath = HostingEnvironment.MapPath("~/config/MeetupUmbracoGroups.txt");
            // Get the alias (urlname) of each group from the config file
            var aliases = File.ReadAllLines(configPath).Where(x => x.Trim() != "").Distinct().ToArray();

            var counterPath = HostingEnvironment.MapPath("~/App_Data/TEMP/MeetupStatisticsCounter.txt");
            var counter = 0;
            if (File.Exists(counterPath))
            {
                var savedCounter = File.ReadAllLines(counterPath).First();
                int.TryParse(savedCounter, out counter);
            }

            var newCounter = aliases.Length <= counter ? 0 : counter + 1;
            File.WriteAllText(counterPath, newCounter.ToString(), Encoding.UTF8);

            var client = new MeetupOAuth2Client();
            var response = client.DoHttpGetRequest(string.Format("https://api.meetup.com/{0}/events?page=1000&status=past", aliases[counter]));
            var events = MeetupGetEventsResponse.ParseResponse(response).Body;

            var meetupCache = new List<MeetupCacheItem>();
            var meetupCacheFile = HostingEnvironment.MapPath("~/App_Data/TEMP/MeetupStatisticsCache.json");
            if (File.Exists(meetupCacheFile))
            {
                var json = File.ReadAllText(meetupCacheFile);
                using (var stringReader = new StringReader(json))
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    var jsonSerializer = new JsonSerializer();
                    meetupCache = jsonSerializer.Deserialize<List<MeetupCacheItem>>(jsonTextReader);
                }
            }

            foreach (var meetupEvent in events)
            {
                if (meetupCache.Any(x => x.Id == meetupEvent.Id))
                    continue;

                var meetupCacheItem = new MeetupCacheItem
                {
                    Time = meetupEvent.Time,
                    Created = meetupEvent.Created,
                    Description = meetupEvent.Description,
                    HasVenue = meetupEvent.HasVenue,
                    Id = meetupEvent.Id,
                    Link = meetupEvent.Link,
                    Name = meetupEvent.Name,
                    Updated = meetupEvent.Updated,
                    Visibility = meetupEvent.Visibility
                };
                meetupCache.Add(meetupCacheItem);
            }

            var rawJson = JsonConvert.SerializeObject(meetupCache, Formatting.Indented);
            File.WriteAllText(meetupCacheFile, rawJson, Encoding.UTF8);
        }


        public MeetupEventsModel GetUpcomingMeetups()
        {
            var meetups = new MeetupEventsModel
            {
                Items = new MeetupItem[0]
            };

            try
            {
                var configPath = HostingEnvironment.MapPath("~/config/MeetupUmbracoGroups.txt");
                if (File.Exists(configPath) == false)
                {
                    LogHelper.Debug<MeetupsController>("Config file was not found: " + configPath);
                    return meetups;
                }

                // Get the alias (urlname) of each group from the config file
                var aliases = File.ReadAllLines(configPath).Where(x => x.Trim() != "").Distinct().ToArray();

                meetups.Items = UmbracoContext.Current.Application.ApplicationCache.RuntimeCache.GetCacheItem<MeetupItem[]>("UmbracoSearchedMeetups",
                    () =>
                    {
                        // Initialize a new service instance (we don't specify an API key since we're accessing public data) 
                        var service = new Skybrud.Social.Meetup.MeetupService();

                        var items = new List<MeetupItem>();

                        foreach (var alias in aliases)
                        {
                            try
                            {
                                // Get information about the group
                                var meetupGroup = service.Groups.GetGroup(alias).Body;

                                if (meetupGroup.JObject.HasValue("next_event") == false)
                                    continue;

                                var nextEventId = meetupGroup.JObject.GetString("next_event.id");

                                // Make the call to the Meetup.com API to get upcoming events
                                var events = service.Events.GetEvents(alias);

                                // Get the next event(s)
                                var nextEvent = events.Body.FirstOrDefault(x => x.Id == nextEventId);

                                // Append the first event of the group
                                if (nextEvent != null)
                                    items.Add(new MeetupItem(meetupGroup, nextEvent));
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error<MeetupsController>("Could not get events from meetup.com for group with alias: " + alias, ex);
                            }
                        }

                        return items.OrderBy(x => x.Event.Time).ToArray();

                    }, TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                LogHelper.Error<MeetupsController>("Could not get events from meetup.com", ex);
            }

            return meetups;
        }
    }
}