using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Newtonsoft.Json;
using OurUmbraco.Community.Meetup.Models;
using Skybrud.Social.Meetup.OAuth;
using Skybrud.Social.Meetup.Responses.Events;

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
    }
}