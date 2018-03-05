using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Examine;
using Examine.SearchCriteria;
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
            var searchCritera = memberSearcher.CreateSearchCriteria(IndexTypes.Member);
            
            //Find all active members - where a lat & lon is set (with new NodeGathering field to index)
            var query = searchCritera.Field("hasLocationSet", "1");

            //TODO: Check for members where blocked is false (0)
            //And umbracoMemberApproved is true (1)
            //I can't get to get it to work with the results I would expect currently

            var results = memberSearcher.Search(query.Compile());

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