using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using OurUmbraco.Community.GitHub;
using OurUmbraco.Our.Extensions;
using OurUmbraco.Our.Models;
using OurUmbraco.Our.Models.GitHub;
using OurUmbraco.Our.Services;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class IssuesStatisticsController : UmbracoApiController
    {
        [MemberAuthorize(AllowGroup = "HQ,TeamUmbraco")]
        [HttpGet]
        public List<IssuesInPeriod> GetGroupedIssuesData(int fromDay, int fromMonth, int fromYear, int toDay,
            int toMonth, int toYear, string repository = "")
        {
            var gitHubService = new GitHubService();
            var teamMembers = new List<string>();
            foreach (var team in gitHubService.GetTeamMembers())
                teamMembers.AddRange(team.Members);

            if (fromDay == 0)
                fromDay = 1;
            if (toDay == 0)
                toDay = DateTime.DaysInMonth(toYear, toMonth);

            var fromDate = DateTime.Parse($"{fromYear}-{fromMonth}-{fromDay} 00:00:00");
            var toDate = DateTime.Parse($"{toYear}-{toMonth}-{toDay} 23:59:59");

            var repoService = new RepositoryManagementService();
            var allCommunityIssues = repoService.GetAllCommunityIssues(false)
                .Where(x => x.Labels.Any(l => l.Name == "status/idea") == false).ToList();

            if (string.IsNullOrWhiteSpace(repository) == false)
                allCommunityIssues = allCommunityIssues.Where(x => x.RepositoryName == repository).ToList();

            var issues = allCommunityIssues
                .Where(x => x.CreateDateTime >= fromDate && x.CreateDateTime <= toDate)
                .OrderBy(x => x.CreateDateTime)
                .GroupBy(x => new {x.CreateDateTime.Year, x.CreateDateTime.Month})
                .ToDictionary(x => x.Key, x => x.ToList());

            var groupedIssues = new List<IssuesInPeriod>();

            foreach (var issuesInPeriod in issues)
            {
                var period = $"{issuesInPeriod.Key.Year}{issuesInPeriod.Key.Month:00}";
                var groupName =
                    $"{DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(issuesInPeriod.Key.Month)} {issuesInPeriod.Key.Year}";
                var issuesList = new IssuesInPeriod
                {
                    MonthYear = period,
                    GroupName = groupName,

                    AllIssueClosingTimesInHours = string.Empty,
                    IssueAverageClosingTimeInHours = 0,
                    IssueMedianClosingTimeInHours = 0,
                    TargetClosingTimeInHours = 2880, // 120 business days

                    AllIssueFirstCommentTimesInHours = string.Empty,
                    IssueAverageFirstCommentTimesInHours = 0,
                    IssueMedianFirstCommentTimesInHours = 0,
                    TargetFirstCommentTimeInHours = 48, // 2 business days

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
                var allFirstCommentTimesInHours = new List<double>();
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
                    else
                    {
                        //var dateLastDayOfMonth = issue.CreateDateTime.GetDateLastDayOfMonth().AddDays(1).AddSeconds(-1);
                        var hoursOpen = issue.CreateDateTime.BusinessHoursUntil(DateTime.Now);
                        allClosingTimesInHours.Add(hoursOpen);
                    }

                    double hoursBeforeFirstReply = 0;
                    double hoursBeforeFirstLabel = 0;
                    foreach (var comment in issue.Comments.OrderBy(x => x.CreateDateTime))
                    {
                        if (teamMembers.InvariantContains(comment.User.Login))
                        {
                            hoursBeforeFirstReply = issue.CreateDateTime.BusinessHoursUntil(comment.CreateDateTime);
                        }
                        else
                        {
                            //var dateLastDayOfMonth = issue.CreateDateTime.GetDateLastDayOfMonth().AddDays(1).AddSeconds(-1);
                            hoursBeforeFirstReply = issue.CreateDateTime.BusinessHoursUntil(DateTime.Now);
                        }
                    }

                    if (issue.Events.Any())
                    {
                        var firstLabel = issue.Events.OrderBy(x => x.CreateDateTime)
                            .FirstOrDefault(x => x.Name == "labeled");
                        if (firstLabel != null)
                            hoursBeforeFirstLabel = issue.CreateDateTime.BusinessDaysUntil(firstLabel.CreateDateTime);
                    }

                    if (hoursBeforeFirstLabel != 0 && hoursBeforeFirstReply != 0)
                    {
                        double hours;
                        if (hoursBeforeFirstLabel < hoursBeforeFirstReply)
                            hours = hoursBeforeFirstLabel;
                        else
                            hours = hoursBeforeFirstReply;

                        allFirstCommentTimesInHours.Add(hours);
                    }
                }

                issuesList.AllIssueClosingTimesInHours = string.Join(",", allClosingTimesInHours);
                if (allClosingTimesInHours.Any())
                {
                    issuesList.IssueAverageClosingTimeInHours = allClosingTimesInHours.Average();
                    issuesList.IssueMedianClosingTimeInHours = allClosingTimesInHours.Median();
                }

                issuesList.AllIssueFirstCommentTimesInHours = string.Join(",", allFirstCommentTimesInHours);

                if (allFirstCommentTimesInHours.Any())
                {
                    issuesList.IssueAverageFirstCommentTimesInHours = allFirstCommentTimesInHours.Average();
                    issuesList.IssueMedianFirstCommentTimesInHours = allFirstCommentTimesInHours.Median();
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