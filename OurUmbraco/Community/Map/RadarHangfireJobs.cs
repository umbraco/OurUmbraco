using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Hangfire;
using Hangfire.Server;
using Hangfire.Console;
using RestSharp;
using Umbraco.Core;
using UmbracoExamine;

namespace OurUmbraco.Community.Map
{
    public class RadarHangfireJobs
    {
        /// <summary>
        /// Entry point to trigger the HangFire Job
        /// Which is used to look up members who do not have lat/lon set on their profile
        /// But have the legacy free textfield for location set - so we can attempt a Google Map GeoLookup
        /// </summary>
        public void FindSignals()
        {
            //Run daily at midnight UTC - As Google's 2500 API limit resets daily
            RecurringJob.AddOrUpdate(() => AmplifySignal(null), Cron.Daily);
        }

        [AutomaticRetry(Attempts = 0)]
        public void AmplifySignal(PerformContext context)
        {
            //Use Examine to query members with more than 70 karma points
            //Who does not have lat & lon set
            //But has a location field

            var memberSearcher = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];
            var searchCritera = memberSearcher.CreateSearchCriteria(IndexTypes.Member);

            var query = searchCritera.Field("hasLocationSet", "0")
                .And().Field("hasLocationTextFieldSet", "1")
                .And().Field("blocked", "0")
                .And().Field("umbracoMemberApproved", "1")
                .Not().Range(LuceneIndexer.SortedFieldNamePrefix + "karma", 0, 70, true, true) //The raw field  in the index has the magic prefix
                .And().OrderByDescending(new SortableField("karma", SortType.Int)) //When you use a sortable field - you need to not include the magic string prefix - as Examine adds it in
                .Compile();

            var results = memberSearcher.Search(query);

            //We should see this number go down to 0 eventually
            context.WriteLine($"There are {results.TotalItemCount} members with more than 70 karma points who have a location set but no lat/lon set on their profile");

            //Eventually we would have processed all members that we can (once we run through this process several times)
            if (results.Any() == false)
                return;

            var apiKey = WebConfigurationManager.AppSettings["GoogleServerKey"];
            var client = new RestClient("https://maps.googleapis.com");
            
            var progressBar = context.WriteProgressBar();

            foreach (var result in results.WithProgress(progressBar, results.TotalItemCount))
            {
                //Get the 'location' field
                var memberFreeTextLocation = result.Fields.ContainsKey("location") ? result.Fields["location"] : null;

                if (string.IsNullOrWhiteSpace(memberFreeTextLocation))
                    continue;

                var newLat = string.Empty;
                var newLon = string.Empty;

                //Adds querystring param to the HTTP GET request to Google API
                var request = new RestRequest("maps/api/geocode/json", Method.GET);
                request.AddParameter("key", apiKey);
                request.AddParameter("address", memberFreeTextLocation);

                var response = client.Execute<GoogleMapResult>(request);


                //REQUEST_DENIED
                //OVER_QUERY_LIMIT
                //OK
                //ZERO_RESULTS
                if (response.Data.status == "OVER_QUERY_LIMIT" || response.Data.status == "REQUEST_DENIED")
                {
                    //TODO: Return a better excpetion type
                    var message = "We have hit the Google Rate API Limit or have been denied API access - we will need to be re-run";

                    context.SetTextColor(ConsoleTextColor.Red);
                    context.WriteLine(message);
                    context.ResetTextColor();

                    throw new Exception(message);
                }

                //If we get more than one result back from Google - the first result is the best guess of the location (so use that)
                var googleLocation = response.Data.results.FirstOrDefault();
                if (googleLocation != null)
                {
                    newLat = googleLocation.geometry.location.lat.ToString();
                    newLon = googleLocation.geometry.location.lng.ToString();

                    context.SetTextColor(ConsoleTextColor.Green);
                    context.WriteLine($"Found {memberFreeTextLocation} as Latitude '{newLat}' Longtitude '{newLon}'");
                    context.ResetTextColor();
                }
                else
                {
                    context.SetTextColor(ConsoleTextColor.Yellow);
                    context.WriteLine($"Google can not find a location for Member ID '{result.Id}' with location '{memberFreeTextLocation}'");
                    context.ResetTextColor();
                }

                //Get the member from it's ID
                var memberId = result.Id;
                var memberService = ApplicationContext.Current.Services.MemberService;
                var member = memberService.GetById(memberId);

                if (member == null)
                    continue;
                
                //Super paranoid checks to ensure the member doesn't have values already set for lat/lon
                var currentLat = member.Properties["latitude"].Value?.ToString();
                var currentLon = member.Properties["longitude"].Value?.ToString();
                
                if (string.IsNullOrEmpty(currentLat)|| string.IsNullOrEmpty(currentLon))
                {
                    //Remove the old legacy value & use lat/lon
                    member.Properties["location"].Value = string.Empty;

                    member.Properties["latitude"].Value = newLat;
                    member.Properties["longitude"].Value = newLon;

                    memberService.Save(member);
                }

                //Wait a bit of time (so we don't hit the rate limit) ?
                Thread.Sleep(1500);
            }
        }
    }

    //Partial C# classes from the JSON to map to
    public class GoogleMapResult
    {
        public List<Result> results { get; set; }
        public string status { get; set; }
    }

    public class Result
    {
        public Geometry geometry { get; set; }
    }

    public class Geometry
    {
        public Location location { get; set; }
    }

    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
}
