using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
using RestSharp;

namespace OurUmbraco.Events.Meetup
{
    public class MeetupController : Controller
    {
        private static readonly string MeetupApiKey = ConfigurationManager.AppSettings["MeetupApiKey"];
        private const string MeetupJsonFile = "App_Data\\Meetup\\searchresults.json";

        public MeetupEventSearchResults GetAllFromFile()
        {
            if (System.IO.File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MeetupJsonFile)) == false)
                SaveAllToFile();

            var allText = System.IO.File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MeetupJsonFile));

            var meetupEventSearchResults = JsonConvert.DeserializeObject<MeetupEventSearchResults>(allText);
            return meetupEventSearchResults;
        }

        public string SaveAllToFile()
        {
            try
            {
                var client = new RestClient("https://api.meetup.com/2/open_events");
                var request = new RestRequest(string.Format("?&sign=true&key={0}&photo-host=public&status=upcoming&topic=umbraco&page=20", MeetupApiKey), Method.GET);
                var response = client.Execute(request);
                if (response != null)
                {
                    var meetupCacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MeetupJsonFile.Substring(0, MeetupJsonFile.LastIndexOf("\\", StringComparison.Ordinal)));
                    if (Directory.Exists(meetupCacheDirectory) == false)
                        Directory.CreateDirectory(meetupCacheDirectory);

                    using (var streamWriter = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MeetupJsonFile), false))
                    {
                        streamWriter.WriteLine(response.Content);
                    }
                }
                else
                {
                    return string.Format("{0} There was no data to serialize so a new cache file hasn't been created", DateTime.Now);
                }
            }
            catch (Exception exception)
            {
                return string.Format("{0} {1}", exception.Message, exception.StackTrace);
            }

            return string.Format("Results succesfully written to {0} at {1}", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MeetupJsonFile), DateTime.Now);
        }

        public class MeetupEventSearchResults
        {
            public IEnumerable<Result> Results { get; set; }
            public Meta Meta { get; set; }
        }

        public class Meta
        {
            public string Lon { get; set; }
            public int Count { get; set; }
            public string SignedUrl { get; set; }
            public string Link { get; set; }
            public string Next { get; set; }
            public int TotalCount { get; set; }
            public string Url { get; set; }
            public string Id { get; set; }
            public string Title { get; set; }
            [JsonConverter(typeof(UnixDateTimeConverter))]
            public DateTime Updated { get; set; }
            public string Description { get; set; }
            public string Method { get; set; }
            public string Lat { get; set; }
        }

        public class Result
        {
            public string Visibility { get; set; }
            public string Status { get; set; }
            public int Maybe_Rsvp_Count { get; set; }
            public int Utc_Offset { get; set; }
            public string Id { get; set; }
            [JsonConverter(typeof(UnixDateTimeConverter))]
            public DateTime Time { get; set; }
            public int Waitlist_Count { get; set; }
            [JsonConverter(typeof(UnixDateTimeConverter))]
            public DateTime Created { get; set; }
            public int Yes_Rsvp_Count { get; set; }
            [JsonConverter(typeof(UnixDateTimeConverter))]
            public DateTime Updated { get; set; }
            public string Event_Url { get; set; }
            public int Headcount { get; set; }
            public string Name { get; set; }
            public Group Group { get; set; }
            public int Rsvp_Limit { get; set; }
            public Venue Venue { get; set; }
            public string Description { get; set; }
            public string How_To_FindUs { get; set; }
        }

        public class Group
        {
            public int Id { get; set; }
            public float Group_Lat { get; set; }
            public string Name { get; set; }
            public float Group_Lon { get; set; }
            public string Join_Mode { get; set; }
            public string Urlname { get; set; }
            public string Who { get; set; }
        }

        public class Venue
        {
            public int Id { get; set; }
            public float Lon { get; set; }
            public bool Repinned { get; set; }
            public string Name { get; set; }
            public string Address_1 { get; set; }
            public float Lat { get; set; }
            public string Country { get; set; }
            public string City { get; set; }
            public string Zip { get; set; }
            public string State { get; set; }
        }

    }
}
