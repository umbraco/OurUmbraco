using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Examine;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class OurMemberStatisticsController : UmbracoAuthorizedApiController
    {
        [HttpGet]
        public OurMemberStatistics GetOurMemberStatistics()
        {

            var searcher = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];
            var criteria = searcher.CreateSearchCriteria();

            // Empty query to get all of the members
            var query = criteria.RawQuery("nodeTypeAlias: member");
            var results = searcher.Search(query).ToList();


            var neverLoggedIn = results.Count(x => string.IsNullOrEmpty(x.Fields["umbracoMemberLastLogin"]));
            var numberBlocked = results.Count(x => x.Fields["blocked"] != null && x.Fields["blocked"] == "1");
            var totalMembers = results.Count;
            var totalNonSpamMembers = totalMembers - numberBlocked;
            var totalNonSpamMembersWithLogin = totalMembers - numberBlocked - neverLoggedIn;

            var statistics = new OurMemberStatistics
            {
                TotalMembers = totalMembers,
                TotalNonSpamMembers = totalNonSpamMembers,
                TotalNonSpamMembersWithLogin = totalNonSpamMembersWithLogin,
                TotalBlockedMembers = numberBlocked
            };
            
            return statistics;
        }
    }

    public class OurMemberStatistics
    {
        public int TotalMembers { get; set; }
        public int TotalNonSpamMembers { get; set; }
        public int TotalNonSpamMembersWithLogin { get; set; }
        public int TotalBlockedMembers { get; set; }
    }
}
