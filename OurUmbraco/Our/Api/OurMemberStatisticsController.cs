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

            var sql = @"SELECT DISTINCT(memberId) FROM";
            var commentMemberIds = ApplicationContext.DatabaseContext.Database.Fetch<int>($"{sql} powersComment");
            var projectMemberIds = ApplicationContext.DatabaseContext.Database.Fetch<int>($"{sql} powersProject");
            var projectVersionMemberIds = ApplicationContext.DatabaseContext.Database.Fetch<int>($"{sql} powersProjectVersion");
            var topicMemberIds = ApplicationContext.DatabaseContext.Database.Fetch<int>($"{sql} powersTopic");
            var wikiMemberIds = ApplicationContext.DatabaseContext.Database.Fetch<int>($"{sql} powersWiki");
            var contributorMemberIds = ApplicationContext.DatabaseContext.Database.Fetch<int>($"{sql} projectContributors");
            var downloadMemberIds = ApplicationContext.DatabaseContext.Database.Fetch<int>($"{sql} projectDownload");

            var uniqueMemberIds = commentMemberIds
                .Union(projectMemberIds)
                .Union(projectVersionMemberIds)
                .Union(topicMemberIds)
                .Union(wikiMemberIds)
                .Union(contributorMemberIds)
                .Union(downloadMemberIds).Count();

            // get create date for members: SELECT createDate FROM umbracoNode WHERE nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560'

            var statistics = new OurMemberStatistics
            {
                TotalMembers = totalMembers,
                TotalNonSpamMembers = totalNonSpamMembers,
                TotalNonSpamMembersWithLogin = totalNonSpamMembersWithLogin,
                TotalBlockedMembers = numberBlocked,
                TotalMembersEarningKarma = uniqueMemberIds
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
        public int TotalMembersEarningKarma { get; set; }
    }
}
