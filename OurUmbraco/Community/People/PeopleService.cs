using System;
using System.Collections.Generic;
using OurUmbraco.Community.People.Models;
using Umbraco.Core;

namespace OurUmbraco.Community.People
{
    public class PeopleService
    {
        public List<PeopleKarmaResult> GetMostActiveDateRange(DateTime fromDate, DateTime toDate, int numberOfResults = 25)
        {
            var sqlQuery = GetMostActiveDateRangeQuery(fromDate, toDate, numberOfResults);
            var results = ApplicationContext.Current.DatabaseContext.Database.Fetch<PeopleKarmaResult>(sqlQuery);
            return results;
        }

        private static string GetMostActiveDateRangeQuery(DateTime fromDate, DateTime toDate, int numberOfResults)
        {
            var where = string.Format("where (date BETWEEN '{0}' AND '{1}')", fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"));
            var top = string.Format("TOP {0}", numberOfResults);

            var query = string.Format(@";with score as(
                          SELECT receiverId AS memberId, 0 as performed, SUM(receiverPoints) as received
                          FROM [powersTopic]
                          {0}
                          group by receiverId
                          
                        UNION ALL
                        
                        SELECT receiverId AS memberId, 0 as performed, SUM(receiverPoints)as received
                        FROM [powersProject]
                        {0}
                        GROUP BY receiverId  
                        
                        UNION ALL
                        SELECT receiverId AS memberId, 0 as performed, SUM(receiverPoints)as received
                          FROM [powersComment]
                          {0}
                          GROUP BY receiverId
                       
                        UNION ALL
                         SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM [powersComment]
                          {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersProject
                          {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersTopic
                          {0}
                          GROUP BY memberId
                          
                        UNION ALL
                        SELECT memberId, SUM(performerPoints)as performed, 0 as received
                          FROM powersWiki
                          {0}
                          GROUP BY memberId
                         )
                         
                         select {1} text as memberName, memberId, sum(performed) as performed, SUM(received) as received, (sum(received) + sum(performed)) as totalPointsInPeriod from score
                           inner join umbracoNode ON memberId = id  
                        where memberId IS NOT NULL and memberId > 0
                         group by text, memberId order by totalPointsInPeriod DESC", @where, top);

            return query;
        }
    }
}
