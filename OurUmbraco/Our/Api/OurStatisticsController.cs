using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Api
{
    public class OurStatisticsController : UmbracoAuthorizedController
    {
        [System.Web.Mvc.HttpGetAttribute]
        public ActionResult GetForumTopicStatistics(string startDate, string endDate)
        {
            const string topicDataSql = 
                @"SELECT contentNodeId as memberId, dataInt AS karmaCurrent
FROM cmsPropertyData
WHERE propertytypeid = 32 AND contentNodeId IN (
    SELECT DISTINCT memberId
    FROM forumComments
    WHERE isSpam = 0 AND
    created >=  @startDate and created <= @endDate
) ORDER BY memberId";
            var typeName = "topics";
            
            var file = ForumTopicStatistics(startDate, endDate, topicDataSql, typeName);
            return file;
        }

        [System.Web.Mvc.HttpGetAttribute]
        public ActionResult GetForumCommentStatistics(string startDate, string endDate)
        {
            const string commentDataSql = 
@"SELECT contentNodeId as memberId, dataInt AS karmaCurrent
FROM cmsPropertyData
WHERE propertytypeid = 32 AND contentNodeId IN (
    SELECT DISTINCT memberId
    FROM forumTopics
    WHERE isSpam = 0 AND
    created >=  @startDate and created <= @endDate
) ORDER BY memberId";
          
            var file = ForumTopicStatistics(startDate, endDate, commentDataSql, "comments");
            return file;
        }
        
        private FileContentResult ForumTopicStatistics(string startDate, string endDate, string topicDataSql, string typeName)
        {
            if (DateTime.TryParse(startDate, out var start) == false)
                return null;

            if (DateTime.TryParse(endDate, out var end) == false)
                return null;

            var queryResults = ApplicationContext.DatabaseContext.Database.Fetch<MemberData>(topicDataSql,
                new
                {
                    startDate = start.ToString("yyyy-MM-dd"),
                    endDate = end.ToString("yyyy-MM-dd")
                });

            var resultsCsv = new List<string> { "MemberId,CurrentKarma" };
            resultsCsv.AddRange(queryResults.Select(resultsTopic => $"{resultsTopic.MemberId},{resultsTopic.KarmaCurrent}"));
            var csvResult = string.Join(Environment.NewLine, resultsCsv);

            var fileName = $"{typeName}_{startDate}_{endDate}.csv";
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvResult);
            var file = File(fileBytes, "text/csv", fileName);
            return file;
        }
        
        
        [System.Web.Mvc.HttpGetAttribute]
        public ActionResult GetActiveMemberSignupStatistics()
        {
            const string sql = @"SELECT id FROM umbracoNode WHERE CONVERT(VARCHAR(4), createDate, 112) != 2016 AND id IN (SELECT cmsPropertyData.contentNodeId FROM cmsPropertyData WHERE (propertyTypeId = 211  AND dataDate IS NOT NULL) OR (propertyTypeId = 69 AND dataNvarchar IS NOT NULL) AND contentNodeId IN (SELECT nodeId FROM cmsMember))";
            var membersLoggedIn = ApplicationContext.DatabaseContext.Database.Fetch<int>(sql);

            var forumSubscribers = ApplicationContext.DatabaseContext.Database.Fetch<int>(@"SELECT DISTINCT(memberId) FROM forumSubscribers");
            var forumTopicSubscribers = ApplicationContext.DatabaseContext.Database.Fetch<int>(@"SELECT DISTINCT(memberId) FROM forumTopicSubscribers");
            var powersComment = ApplicationContext.DatabaseContext.Database.Fetch<int>(@"SELECT DISTINCT(memberId) FROM powersComment");
            var powersTopic = ApplicationContext.DatabaseContext.Database.Fetch<int>(@"SELECT DISTINCT(memberId) FROM powersTopic");
            var powersProject = ApplicationContext.DatabaseContext.Database.Fetch<int>(@"SELECT DISTINCT(memberId) FROM powersProject");
            var powersProjectVersion = ApplicationContext.DatabaseContext.Database.Fetch<int>(@"SELECT DISTINCT(memberId) FROM powersProjectVersion");
            var powersWiki = ApplicationContext.DatabaseContext.Database.Fetch<int>(@"SELECT DISTINCT(memberId) FROM powersWiki");
            var projectContributors = ApplicationContext.DatabaseContext.Database.Fetch<int>(@"SELECT DISTINCT(memberId) FROM projectContributors");
            var projectDownload = ApplicationContext.DatabaseContext.Database.Fetch<int>(@"SELECT DISTINCT(memberId) FROM projectDownload");
            var wikiFilesCreated = ApplicationContext.DatabaseContext.Database.Fetch<int>(@"SELECT DISTINCT(createdBy) FROM wikiFiles");

            var uniqueActiveMembers = new HashSet<int>();
            foreach (var id in forumSubscribers)
                uniqueActiveMembers.Add(id);
            foreach (var id in forumTopicSubscribers)
                uniqueActiveMembers.Add(id);
            foreach (var id in powersComment)
                uniqueActiveMembers.Add(id);
            foreach (var id in powersTopic)
                uniqueActiveMembers.Add(id);
            foreach (var id in powersProject)
                uniqueActiveMembers.Add(id);
            foreach (var id in powersProjectVersion)
                uniqueActiveMembers.Add(id);
            foreach (var id in powersWiki)
                uniqueActiveMembers.Add(id);
            foreach (var id in projectContributors)
                uniqueActiveMembers.Add(id);
            foreach (var id in projectDownload)
                uniqueActiveMembers.Add(id);
            foreach (var id in membersLoggedIn)
                 uniqueActiveMembers.Add(id);

            var createDates2 = new List<DateTime>();
            foreach (var memberGroup in uniqueActiveMembers.InGroupsOf(20000))
            {
                var sql2 = $"SELECT createDate FROM umbracoNode WHERE id IN ({string.Join(",", memberGroup.ToList())})";
                createDates2.AddRange(ApplicationContext.DatabaseContext.Database.Fetch<DateTime>(sql2));
            }

            var groupedYears = createDates2.GroupBy(p => new {year = p.Year})
                .Select(d => new YearStat() { Year = d.Key.year, Count = d.Count()}).ToList();

            var resultsCsv = new List<string> { "Year,Signups" };
            resultsCsv.AddRange(groupedYears.Select(x => $"{x.Year},{x.Count}"));
            var csvResult = string.Join(Environment.NewLine, resultsCsv);
            
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvResult);
            var file = File(fileBytes, "text/csv", "membersignups.csv");
            return file;
        }
                
        public class YearStat
        {
            public int Year { get; set; }
            public int Count { get; set; }
        }
    }

    public class MemberData
    {
        public int MemberId { get; set; }
        public int KarmaCurrent { get; set; }
    }
}
