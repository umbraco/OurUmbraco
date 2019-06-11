using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using OurUmbraco.Community.GitHub;
using OurUmbraco.Our.Extensions;
using OurUmbraco.Our.Models.GitHub;
using OurUmbraco.Our.Services;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class DashboardStatisticsController : UmbracoApiController
    {
        [MemberAuthorize(AllowGroup = "HQ")]
        [HttpGet]
        public List<Contributions> GetContributorStatistics(int startMonth = 6, int startYear = 2010, string repository = "", bool monthly = true)
        {
            var totalContributors = new List<Contributions>();

            var firstPrs = new List<string>();

            var repoService = new RepositoryManagementService();

            var pullsNonHq = string.IsNullOrWhiteSpace(repository)
                ? repoService.GetAllCommunityIssues(true).ToList()
                : repoService.GetAllCommunityIssues(true).Where(x => x.RepositoryName == repository).ToList();

            var date = new DateTime(startYear, startMonth, 1);
            while (date < DateTime.Now)
            {
                var year = date.Year;

                var endMonth = monthly ? date.AddMonths(1).Month : date.AddYears(1).Month;
                var endYear = monthly ? date.AddMonths(1).Year : date.AddYears(1).Year;

                var startDate = new DateTime(year, date.Month, 1);
                var endDate = new DateTime(endYear, endMonth, 1);

                var prsCreated = pullsNonHq.Where(x => x.CreateDateTime >= startDate && x.CreateDateTime < endDate).ToList();

                var repoStatistics = new Contributions
                {
                    CodegardenYear = year,
                    Title = date.ToString(monthly ? "yyyyMM" : "yyyy"),
                    FirstCommentStatistics = new FirstCommentStatistics { AllFirstEventTimesInHours = new List<double>() },
                    AllPulls = prsCreated
                };

                var allFirstCommentTimesInHours = new List<double>();
                var allClosingTimesInHours = new List<double>();

                foreach (var pr in prsCreated)
                {
                    var firstCommentStatistics = GetFirstEventStatistics(pr);
                    repoStatistics.FirstCommentStatistics.FirstEventOnTime += firstCommentStatistics.FirstEventOnTime;
                    repoStatistics.FirstCommentStatistics.FirstEventLate += firstCommentStatistics.FirstEventLate;
                    repoStatistics.FirstCommentStatistics.TeamEventMissing += firstCommentStatistics.TeamEventMissing;
                    allFirstCommentTimesInHours.AddRange(firstCommentStatistics.AllFirstEventTimesInHours);

                    if (pr.ClosedDateTime != null)
                    {
                        var timeSpan = Convert.ToInt32(pr.CreateDateTime.BusinessHoursUntil(pr.ClosedDateTime.Value));
                        allClosingTimesInHours.Add(timeSpan);

                        if (pr.Labels.Any(x => x.Name.StartsWith("release/")))
                            repoStatistics.ReleasePullRequests += 1;
                    }
                }

                if (allFirstCommentTimesInHours.Any())
                {
                    repoStatistics.AverageHoursToFirstComment = (int)Math.Round(allFirstCommentTimesInHours.Average());
                    repoStatistics.MedianHoursToFirstComment = (int)Math.Round(allFirstCommentTimesInHours.Median());
                }

                if (allClosingTimesInHours.Any())
                {
                    repoStatistics.AverageHoursToClose = (int)Math.Round(allClosingTimesInHours.Average());
                    repoStatistics.MedianHoursToClose = (int)Math.Round(allClosingTimesInHours.Median());
                }

                repoStatistics.PullRequestsCreated = prsCreated.Count;

                // GitHub marks all merged PRs as "closed", so we want to differentiate: if there's no "merged" event then it was closed without merging
                var prsClosed = pullsNonHq.Where(x => x.State == "closed"
                                                      && x.Events.Any(y => y.Name == "merged") == false
                                                      && x.ClosedDateTime >= startDate
                                                      && x.ClosedDateTime < endDate).ToList();

                // There was a "merged" event and it was in the current period
                var prsMerged = pullsNonHq.Where(x => x.State == "closed"
                                                      && x.Events.Any(y => y.Name == "merged") == true
                                                      && x.Events.First(y => y.Name == "merged").CreateDateTime >= startDate
                                                      && x.Events.First(y => y.Name == "merged").CreateDateTime < endDate).ToList();

                // Created in this period and still open now or merged/closed after the last date in this period
                var prsStillOpen = pullsNonHq.Where(x =>
                    x.CreateDateTime >= startDate
                    && x.CreateDateTime < endDate
                    && (x.State == "open" || x.ClosedDateTime >= endDate || x.ClosedDateTime > endDate)).ToList();

                repoStatistics.PullRequestsClosed = prsClosed.Count;
                repoStatistics.PullRequestsMerged = prsMerged.Count;
                repoStatistics.PullRequestsProcessed = prsClosed.Count + prsMerged.Count;
                repoStatistics.PullRequestsStillOpenInPeriod = prsStillOpen.Count;

                foreach (var pr in prsMerged)
                {
                    if (firstPrs.Any(x => x == pr.User.Login))
                        continue;

                    firstPrs.Add(pr.User.Login);
                    repoStatistics.FirstAcceptedPullRequests += 1;
                }

                var contributors = prsMerged.Select(x => x.User.Login).Distinct().ToList();
                repoStatistics.UniqueContributorCount = contributors.Count;
                totalContributors.Add(repoStatistics);

                date = monthly ? date.AddMonths(1) : date.AddYears(1);
            }

            return new List<Contributions>(totalContributors.Where(x => x.UniqueContributorCount != 0).OrderBy(x => x.CodegardenYear));
        }

        [MemberAuthorize(AllowGroup = "HQ")]
        [HttpGet]
        public List<IssueStatistics> GetIssueStatistics(int startMonth = 6, int startYear = 2010, string repository = "", bool monthly = true)
        {
            var repoService = new RepositoryManagementService();

            var allCommunityIssues = string.IsNullOrWhiteSpace(repository)
                ? repoService.GetAllCommunityIssues(false).ToList()
                : repoService.GetAllCommunityIssues(false).Where(x => x.RepositoryName == repository).ToList();

            var issueStatistics = new List<IssueStatistics>();
            var date = new DateTime(startYear, startMonth, 1);
            while (date < DateTime.Now)
            {
                var year = date.Year;

                var endMonth = monthly ? date.AddMonths(1).Month : date.AddYears(1).Month;
                var endYear = monthly ? date.AddMonths(1).Year : date.AddYears(1).Year;

                var startDate = new DateTime(year, date.Month, 1);
                var endDate = new DateTime(endYear, endMonth, 1);

                var issuesInPeriod = allCommunityIssues.Where(x => x.CreateDateTime >= startDate && x.CreateDateTime < endDate).ToList();
                var issuesClosedInPeriod = issuesInPeriod.Where(x => x.State == "closed" && x.ClosedDateTime >= startDate && x.ClosedDateTime < endDate).ToList();

                var yearStatistics = new IssueStatistics
                {
                    CodegardenYear = year,
                    Title = date.ToString(monthly ? "yyyyMM" : "yyyy"),
                    CreatedIssues = issuesInPeriod.Count,
                    ClosedIssues = issuesClosedInPeriod.Count,
                    FirstCommentStatistics = new FirstCommentStatistics { AllFirstEventTimesInHours = new List<double>() },
                    AllIssues = issuesInPeriod
                };

                var allFirstCommentTimesInHours = new List<double>();
                var allClosingTimesInHours = new List<double>();

                foreach (var issue in issuesInPeriod)
                {
                    var firstCommentStatistics = GetFirstEventStatistics(issue);
                    yearStatistics.FirstCommentStatistics.FirstEventOnTime += firstCommentStatistics.FirstEventOnTime;
                    yearStatistics.FirstCommentStatistics.FirstEventLate += firstCommentStatistics.FirstEventLate;
                    yearStatistics.FirstCommentStatistics.TeamEventMissing += firstCommentStatistics.TeamEventMissing;
                    allFirstCommentTimesInHours.AddRange(firstCommentStatistics.AllFirstEventTimesInHours);

                    if (issue.ClosedDateTime != null)
                    {
                        var timeSpan = Convert.ToInt32(issue.CreateDateTime.BusinessHoursUntil(issue.ClosedDateTime.Value));
                        allClosingTimesInHours.Add(timeSpan);

                        if (issue.Labels.Any(x => x.Name.StartsWith("release/")))
                            yearStatistics.ReleaseIssues += 1;
                    }
                }

                if (allFirstCommentTimesInHours.Any())
                {
                    yearStatistics.AverageHoursToFirstComment = (int)Math.Round(allFirstCommentTimesInHours.Average());
                    yearStatistics.MedianHoursToFirstComment = (int)Math.Round(allFirstCommentTimesInHours.Median());
                }

                if (allClosingTimesInHours.Any())
                {
                    yearStatistics.AverageHoursToClose = (int)Math.Round(allClosingTimesInHours.Average());
                    yearStatistics.MedianHoursToClose = (int)Math.Round(allClosingTimesInHours.Median());
                }

                issueStatistics.Add(yearStatistics);

                date = monthly ? date.AddMonths(1) : date.AddYears(1);
            }

            return issueStatistics;
        }

        private FirstCommentStatistics GetFirstEventStatistics(Issue issue)
        {
            var firstCommentStatistics = new FirstCommentStatistics
            {
                AllFirstEventTimesInHours = new List<double>(),
                IssuesNoComments = new List<Issue>()
            };

            var firstTeamEventDateTime = GetFirstTeamEventDateTime(issue);

            if (firstTeamEventDateTime == default(DateTime))
            {
                firstCommentStatistics.TeamEventMissing += 1;
                firstCommentStatistics.IssuesNoComments.Add(issue);
            }
            else
            {
                var timeSpan = Convert.ToInt32(issue.CreateDateTime.BusinessHoursUntil(firstTeamEventDateTime.Value));
                firstCommentStatistics.AllFirstEventTimesInHours.Add(timeSpan);
                if (timeSpan <= 48)
                {
                    firstCommentStatistics.FirstEventOnTime += 1;
                }
                else
                {
                    firstCommentStatistics.FirstEventLate += 1;
                }
            }

            return firstCommentStatistics;
        }

        public DateTime? GetFirstTeamEventDateTime(Issue issue)
        {
            var gitHubService = new GitHubService();
            var users = gitHubService.GetTeam(issue.RepositoryName).Members.Select(x => x.ToLower());

            var eventDateTimes = new List<DateTime>();
            var comments = issue.Comments.Where(x => x.User != null && users.Contains(x.User.Login.ToLowerInvariant()));

            // When someone mentions a team member the "actor" is set to that team member, where it should be the user (community member) that is the actor
            // When a team member subscribes to a PR / issue, this does not send any signals, nobody can see it, so it should not count as an event
            // If wafflebot assigns something, ignore that as well, it had weird rules so just don't trust it
            var events = issue.Events.Where(x => x.Actor != null && users.Contains(x.Actor.Login.ToLowerInvariant()) && x.Name != "mentioned" && x.Name != "subscribed" && (x.Assigner != null && x.Assigner.Login != "wafflebot[bot]"));
            eventDateTimes.AddRange(comments.Select(x => x.CreateDateTime));
            eventDateTimes.AddRange(events.Select(x => x.CreateDateTime));

            if (issue.Reviews != null)
            {
                var reviews = issue.Reviews.Where(x => x.Actor != null && users.Contains(x.Actor.Login.ToLowerInvariant()));
                eventDateTimes.AddRange(reviews.Select(x => x.CreateDateTime));
            }

            // Special case: if a user closes their own issue we can count that as a team event
            if (issue.State == "closed")
            {
                var closedEvent = issue.Events.FirstOrDefault(x => x.Name == "closed");
                if (closedEvent != null)
                {
                    // Nice one: if a user deletes themselves from GitHub, the Actor becomes.. null :/
                    if (closedEvent.Actor == null || issue.User.Login == closedEvent.Actor.Login)
                        eventDateTimes.Add(closedEvent.CreateDateTime);
                }
            }

            return eventDateTimes.OrderBy(x => x).FirstOrDefault();
        }
    }

    public class Contributions
    {
        public string Title { get; set; }
        public int CodegardenYear { get; set; }
        public int UniqueContributorCount { get; set; }
        public int PullRequestsCreated { get; set; }
        public int PullRequestsMerged { get; set; }
        public int PullRequestsClosed { get; set; }
        public int PullRequestsProcessed { get; set; }
        public int PullRequestsStillOpenInPeriod { get; set; }
        public int FirstAcceptedPullRequests { get; set; }
        public int ReleasePullRequests { get; set; }
        public int AverageHoursToFirstComment { get; set; }
        public int MedianHoursToFirstComment { get; set; }
        public int AverageHoursToClose { get; set; }
        public int MedianHoursToClose { get; set; }
        public FirstCommentStatistics FirstCommentStatistics { get; set; }
        public List<Issue> AllPulls { get; set; }
    }

    public class IssueStatistics
    {
        public string Title { get; set; }
        public int CodegardenYear { get; set; }
        public int CreatedIssues { get; set; }
        public int ClosedIssues { get; set; }
        public int ReleaseIssues { get; set; }
        public int AverageHoursToFirstComment { get; set; }
        public int MedianHoursToFirstComment { get; set; }
        public int AverageHoursToClose { get; set; }
        public int MedianHoursToClose { get; set; }
        public FirstCommentStatistics FirstCommentStatistics { get; set; }
        public List<Issue> AllIssues { get; set; }
    }

    public class FirstCommentStatistics
    {
        public int FirstEventOnTime { get; set; }
        public int FirstEventLate { get; set; }
        public int TeamEventMissing { get; set; }
        public List<double> AllFirstEventTimesInHours { get; set; }
        public List<Issue> IssuesNoComments { get; set; }
    }
}
