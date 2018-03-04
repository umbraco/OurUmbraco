using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Examine;
using Umbraco.Web.WebApi;
using UmbracoExamine;

namespace OurUmbraco.Community.Map
{
    public class MapApiController : UmbracoApiController
    {
        [HttpGet]
        public List<MemberLocation> GetAllMemberLocations()
        { 
            var memberSearcher = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];
            var query = memberSearcher.CreateSearchCriteria(IndexTypes.Member);

            //Find all active members - where a lat & lon is set (with new NodeGathering field to index)
            query.Field("hasLocationSet", "1");

                /*
                .And().Field("blocked", "0")
                .And().Field("umbracoMemberApproved ", "1").Compile();
                */

            //Query
            var results = memberSearcher.Search(query);
            var rawQuery = query.ToString();

            //Pluck the fields we need from the Examine index fields
            //For our much simpler model to send back as the JSON response
            var members = results.Select(result => new MemberLocation
            {
                Name = result.Fields["nodeName"],
                Lat = result.Fields["latitude"],
                Lon = result.Fields["longitude"]
            })
            .ToList();
            
            return members;
        }

    }
}