using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
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
	FROM forumTopics
	WHERE created >= @startDate and created < @endDate
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
	FROM forumComments
	WHERE created >= @startDate and created < @endDate
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
    }

    public class MemberData
    {
        public int MemberId { get; set; }
        public int KarmaCurrent { get; set; }
    }
}
