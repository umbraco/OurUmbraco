using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using GitterSharp.Model;
using OurUmbraco.Our.Extensions;
using OurUmbraco.Our.Models;
using OurUmbraco.Our.Services;
using Umbraco.Web.WebApi;
using String = Umbraco.Web.Media.EmbedProviders.Settings.String;

namespace OurUmbraco.Our.Api
{
    public class IssuesStatisticsController : UmbracoApiController
    {
        [MemberAuthorize(AllowGroup = "HQ,TeamUmbraco")]
        [HttpGet]
        public List<IssuesInPeriod> GetGroupedIssuesData(int fromDay, int fromMonth, int fromYear, int toDay, int toMonth, int toYear)
        {
            if (fromDay == 0)
                fromDay = 1;
            if (toDay == 0)
                toDay = DateTime.DaysInMonth(toYear, toMonth);

            var fromDate = DateTime.Parse($"{fromYear}-{fromMonth}-{fromDay} 00:00:00");
            var toDate = DateTime.Parse($"{toYear}-{toMonth}-{toDay} 23:59:59");

            var repoService = new RepositoryManagementService();
            var allCommunityIssues = repoService.GetAllCommunityIssues().ToList();
            var issues = allCommunityIssues
                .Where(x => x.CreateDateTime >= fromDate && x.CreateDateTime <= toDate)
                .OrderBy(x => x.CreateDateTime)
                .GroupBy(x => new { x.CreateDateTime.Year, x.CreateDateTime.Month })
                .ToDictionary(x => x.Key, x => x.ToList());

            var groupedIssues = new List<IssuesInPeriod>();

            foreach (var issuesInPeriod in issues)
            {
                var period = $"{issuesInPeriod.Key.Year}{issuesInPeriod.Key.Month:00}";
                var groupName = $"{DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(issuesInPeriod.Key.Month)} {issuesInPeriod.Key.Year}";
                var issuesList = new IssuesInPeriod
                {
                    MonthYear = period,
                    GroupName = groupName,
                    AllIssueClosingTimesInHours = string.Empty,
                    IssueAverageClosingTimeInHours = 0,
                    IssueMedianClosingTimeInHours = 0,
                    //AllIssueFirstCommentTimesInHours = "",
                    NumberClosed = 0,
                    NumberOpen = 0,
                    NumberCreated = issuesInPeriod.Value.Count
                };

                var year = issuesInPeriod.Key.Year;
                var month = issuesInPeriod.Key.Month;
                var periodFirstDay = new DateTime(year, month, 1);
                var periodLastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                foreach (var issue in allCommunityIssues)
                {
                    if (issue.CreateDateTime <= periodLastDay && issue.State != "closed")
                        issuesList.NumberOpen = issuesList.NumberOpen + 1;
                }

                var allClosingTimesInHours = new List<double>();
                foreach (var issue in issuesInPeriod.Value)
                {
                    if (issue.State == "closed" && issue.ClosedDateTime.HasValue)
                    {
                        var createDateTime = issue.CreateDateTime;
                        var closedDateTime = issue.ClosedDateTime.Value;

                        if (closedDateTime < periodFirstDay || closedDateTime > periodLastDay)
                            continue;

                        issuesList.NumberClosed = issuesList.NumberClosed + 1;

                        var hoursOpen = createDateTime.BusinessHoursUntil(closedDateTime);
                        allClosingTimesInHours.Add(hoursOpen);
                    }
                }

                issuesList.AllIssueClosingTimesInHours = string.Join(",", allClosingTimesInHours);
                if (allClosingTimesInHours.Any())
                {
                    issuesList.IssueAverageClosingTimeInHours = allClosingTimesInHours.Average();
                    issuesList.IssueMedianClosingTimeInHours = allClosingTimesInHours.Median();
                }

                groupedIssues.Add(issuesList);
            }

            //foreach (var issuesInPeriod in issues)
            //{
            //    foreach (var issue in issuesInPeriod.Value)
            //    {
            //        if(issue.CreateDateTime >= fromDate && issue.CreateDateTime <= toDate)
            //        {
            //            var year = issuesInPeriod.Key.Year;
            //            var month = issuesInPeriod.Key.Month;
            //            if(issue.ClosedDateTime != null && issue.ClosedDateTime > new DateTime(year, month, DateTime.DaysInMonth(year, month))
            //               groupedIssues.Where(x => x.MonthYear == )
            //        }
            //    }
            //}

            return groupedIssues.OrderBy(x => x.MonthYear).ToList();
        }
    }
}
