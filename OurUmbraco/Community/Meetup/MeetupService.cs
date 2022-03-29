using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OurUmbraco.Community.Controllers;
using OurUmbraco.Community.Meetup.Models;
using Skybrud.Essentials.Http;
using Skybrud.Social.Meetup.Models.GraphQl;
using Umbraco.Core.Logging;

namespace OurUmbraco.Community.Meetup
{
    public class MeetupService
    {
        private readonly string _meetupEventsCachePath = HostingEnvironment.MapPath("~/App_Data/TEMP/UpcomingMeetups.json");

        public List<OurMeetupGroup> GetCachedUpcomingMeetups()
        {
            if (File.Exists(_meetupEventsCachePath) == false)
            {
                CacheUpcomingMeetups();
            }
        
            var meetupCache = File.ReadAllText(_meetupEventsCachePath);
            var deserializedCache = JsonConvert.DeserializeObject<List<OurMeetupGroup>>(meetupCache);
            return deserializedCache;
        }

        public void CacheUpcomingMeetups()
        {
            var meetupService = new MeetupService();
            var meetupGroups = meetupService.GetUpcomingMeetups();

            var meetupEvents = new List<OurMeetupGroup>();

            foreach (var groupResult in meetupGroups.Where(x => x.Data.GroupByUrlname != null))
            {
                var group = groupResult.Data.GroupByUrlname;
                
                var meetupGroup = new OurMeetupGroup
                {
                    Id = group.Id,
                    Name = group.Name,
                    LogoBaseUrl = group.Logo.BaseUrl + group.Logo.Id,
                    Events = new List<OurMeetupEvent>()
                };
                
                foreach (var eventsEdge in group.UpcomingEvents.Edges)
                {
                    var edgeNode = eventsEdge.Node;
                    meetupGroup.Events.Add(new OurMeetupEvent
                    {
                        Title = edgeNode.Title,
                        Description = edgeNode.Description,
                        Url = edgeNode.EventUrl,
                        Latitude = edgeNode.Venue.Latitude,
                        Longitude = edgeNode.Venue.Longitude,
                        VenueName = edgeNode.Venue.Name,
                        DateTime = edgeNode.DateTime
                    });
                }
                
                meetupEvents.Add(meetupGroup);
            }

            var rawJson = JsonConvert.SerializeObject(meetupEvents, Formatting.Indented);
            File.WriteAllText(_meetupEventsCachePath, rawJson, Encoding.UTF8);

            // WIP 
            //var cosmosService = new CosmosService();
            //var  result = await cosmosService.CreateMeetup(meetupEvents.First(x => x.Events.Count > 0).Events.First());
        }

        private List<MeetupGraphQlGroupResult> GetUpcomingMeetups()
        {
            var groups = new List<MeetupGraphQlGroupResult>();

            try
            {
                var configPath = HostingEnvironment.MapPath("~/config/MeetupUmbracoGroups.txt");
                if (File.Exists(configPath) == false)
                {
                    LogHelper.Debug<MeetupsController>("Config file was not found: " + configPath);
                    return null;
                }

                // Get the alias (urlname) of each group from the config file
                var aliases = File.ReadAllLines(configPath).Where(x => x.Trim() != "").Distinct().ToArray();

                var meetupGroups = new List<MeetupGraphQlGroupResult>();
                // Initialize a new service instance (we don't specify an API key since we're accessing public data) 
                var service = new Skybrud.Social.Meetup.MeetupService();

                foreach (var alias in aliases)
                {
                    try
                    {
                        var query = @"query($urlname: String!) {
  groupByUrlname(urlname: $urlname) {
    id
    name
    logo { id baseUrl preview }
    latitude
    longitude
    description
    urlname
    timezone
    city
    state
    country
    zip
    link
    joinMode
    welcomeBlurb
    upcomingEvents(input: { first: 3 }) {
      count
      pageInfo {
        endCursor
      }
      edges {
        cursor
        node {
          id
          title
          eventUrl
          description
          shortDescription
          howToFindUs
          venue { id name address city state postalCode crossStreet country neighborhood lat lng zoom radius }
          status
          dateTime
          duration
          timezone
          endTime
          createdAt
          eventType
          shortUrl
          isOnline
        }
      }
    }
  }
}";

                        var variables = new JObject
                        {
                            { "urlname", alias },
                        }.ToString();

                        var request = HttpRequest.Post("/gql", new JObject
                        {
                            { "query", query },
                            { "variables", variables }
                        });

                        var response = service.Client.GetResponse(request);

                        // Raw JSON string in response.Body
                        var jObject = JObject.Parse(response.Body);
                        var groupResult = MeetupGraphQlGroupResult.Parse(jObject);
                        if (groupResult != null)
                            meetupGroups.Add(groupResult);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<MeetupsController>(
                            "Could not get events from meetup.com for group with alias: " + alias, ex);
                    }
                }

                groups = meetupGroups;
            }
            catch (Exception ex)
            {
                LogHelper.Error<MeetupsController>("Could not get events from meetup.com", ex);
            }

            return groups;
        }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Logo
    {
        public string id { get; set; }
        public string baseUrl { get; set; }
    }

    public class PageInfo
    {
        public string endCursor { get; set; }
    }

    public class Venue
    {
        public string id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public object crossStreet { get; set; }
        public string country { get; set; }
        public object neighborhood { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public int zoom { get; set; }
        public int radius { get; set; }
    }

    public class Node
    {
        public string id { get; set; }
        public string title { get; set; }
        public string eventUrl { get; set; }
        public string description { get; set; }
        public string shortDescription { get; set; }
        public string howToFindUs { get; set; }
        public Venue venue { get; set; }
        public string status { get; set; }
        public string dateTime { get; set; }
        public string duration { get; set; }
        public string timezone { get; set; }
        public string endTime { get; set; }
        public object createdAt { get; set; }
        public string eventType { get; set; }
        public string shortUrl { get; set; }
        public bool isOnline { get; set; }
    }

    public class Edge
    {
        public string cursor { get; set; }
        public Node node { get; set; }
    }

    public class UpcomingEvents
    {
        public int count { get; set; }
        public PageInfo pageInfo { get; set; }
        public List<Edge> edges { get; set; }
    }

    public class GroupByUrlname
    {
        public string id { get; set; }
        public string name { get; set; }
        public Logo logo { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string description { get; set; }
        public string urlname { get; set; }
        public string timezone { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string zip { get; set; }
        public string link { get; set; }
        public string joinMode { get; set; }
        public object welcomeBlurb { get; set; }
        public UpcomingEvents upcomingEvents { get; set; }
    }

    public class Data
    {
        public GroupByUrlname groupByUrlname { get; set; }
    }

    public class Root
    {
        public Data data { get; set; }
    }
}