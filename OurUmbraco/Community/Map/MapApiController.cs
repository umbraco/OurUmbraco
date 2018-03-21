using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Umbraco.Web.WebApi;
using UmbracoExamine;

namespace OurUmbraco.Community.Map
{
    public class MapApiController : UmbracoApiController
    {
        [HttpGet]
        public List<MemberLocation> GetAllMemberLocations(string swLat, string swLon, string neLat, string neLon)
        {           
            var memberSearcher = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];
            var searchCritera = memberSearcher.CreateSearchCriteria(IndexTypes.Member);

            var longSwLat = Convert.ToDouble(swLat);
            var longSwLon = Convert.ToDouble(swLon);
            var longNeLat = Convert.ToDouble(neLat);
            var longNeLon = Convert.ToDouble(neLon);
            
            //Find all active members - where a lat & lon is set (with new NodeGathering field to index)
            var query = searchCritera.Field("hasLocationSet", "1")
                .And().Field("blocked", "0")
                .And().Field("umbracoMemberApproved", "1")
                .And().Range("latitudeNumber", longSwLat, longNeLat, true, true)
                .And().Range("longitudeNumber", longSwLon, longNeLon, true, true)
                .Not().Range(LuceneIndexer.SortedFieldNamePrefix + "karma", 0, 70, true, true) //The raw field  in the index has the magic prefix
                .And().OrderByDescending(new SortableField("karma", SortType.Int)) //When you use a sortable field - you need to not include the magic string prefix - as Examine adds it in
                .Compile();
            
            var results = memberSearcher.Search(query);

            //Pluck the fields we need from the Examine index fields
            //For our much simpler model to send back as the JSON response
            var members = results.Select(result => new MemberLocation
            {
                Id = result.Id,
                Name = result.Fields["nodeName"],
                Avatar = GetAvatar(result),
                Lat = GetLatitude(result),
                Lon = GetLongitude(result)
            })
            .ToList();

            //Loop over list and try to find members that have the exact same lat & lon
            //Mark them as a 'someoneElseIsHere' to true
            foreach (var member in members)
            {
                //Check if a member exist with same lat & lon & exclude itself from the lookup
                member.SomeoneElseIsHere = members.Exists(x => x.Lat == member.Lat && x.Lon == member.Lon && x.Id != member.Id);
            }
            
            return members;
        }

        /// <summary>
        /// https://gizmodo.com/how-precise-is-one-degree-of-longitude-or-latitude-1631241162
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private string GetLatitude(SearchResult result)
        {
            var lat = result.Fields["latitude"];
            var latAsDouble = Double.Parse(lat);
            var latRounded = Math.Round(latAsDouble, 3);
            return latRounded.ToString();
        }

        /// <summary>
        /// https://gizmodo.com/how-precise-is-one-degree-of-longitude-or-latitude-1631241162
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private string GetLongitude(SearchResult result)
        {
            var lon = result.Fields["longitude"];
            var lonAsDouble = Double.Parse(lon);
            var lonRounded = Math.Round(lonAsDouble, 3);
            return lonRounded.ToString();
        }

        private string GetAvatar(SearchResult result)
        {
            var email = result.Fields["email"];
            var name = result.Fields["nodeName"];
            return Our.Utils.GetGravatar(email, 50, name, true);
        }
    }
}