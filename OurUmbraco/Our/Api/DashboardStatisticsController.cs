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
        public List<Contributions> GetContributorStatistics(int startMonth = 6, string repository = "", bool monthly = true)
        {
            var totalContributors = new List<Contributions>();

            var firstPrs = new List<string>();

            var repoService = new RepositoryManagementService();
            
            var pullsNonHq = string.IsNullOrWhiteSpace(repository)
                ? repoService.GetAllCommunityIssues(true).ToList()
                : repoService.GetAllCommunityIssues(true).Where(x => x.RepositoryName == repository).ToList();
            
            var date = new DateTime(2010, startMonth, 1);
            while (date < DateTime.Now)
            {
                var year = date.Year;

                var endMonth = monthly ? date.AddMonths(1).Month : date.AddYears(1).Month;
                var endYear = monthly ? date.AddMonths(1).Year : date.AddYears(1).Year;
                
                var startDate = new DateTime(year, date.Month, 1);
                var endDate = new DateTime(endYear, endMonth, 1);

                var repoStatistics = new Contributions
                {
                    CodegardenYear = year,
                    Title = date.ToString(monthly ? "yyyyMM" : "yyyy")
                };

                var prsCreated = pullsNonHq.Where(x => x.CreateDateTime >= startDate && x.CreateDateTime < endDate).ToList();

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

                foreach (var pr in prsMerged.Where(x => x.Labels != null && x.Labels.Any()))
                {
                    if (pr.Labels.Any(x => x.Name.StartsWith("release/")))
                        repoStatistics.ReleasePullRequests = repoStatistics.ReleasePullRequests + 1;
                }

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
        public List<IssueStatistics> GetIssueStatistics(int startMonth = 6, string repository = "", bool monthly = true)
        {
            var repoService = new RepositoryManagementService();

            var allCommunityIssues = string.IsNullOrWhiteSpace(repository)
                ? repoService.GetAllCommunityIssues(false).ToList()
                : repoService.GetAllCommunityIssues(false).Where(x => x.RepositoryName == repository).ToList();

            var issueStatistics = new List<IssueStatistics>();
            var date = new DateTime(2010, startMonth, 1);
            while (date < DateTime.Now)
            {
                var year = date.Year;

                var endMonth = monthly ? date.AddMonths(1).Month : date.AddYears(1).Month;
                var endYear = monthly ? date.AddMonths(1).Year : date.AddYears(1).Year;

                var startDate = new DateTime(year, date.Month, 1);
                var endDate = new DateTime(endYear, endMonth, 1);

                var yearIssues = allCommunityIssues.Where(x => x.CreateDateTime >= startDate && x.CreateDateTime < endDate).ToList();
                var closedYearIssues = yearIssues.Where(x => x.State == "closed" && x.ClosedDateTime >= startDate && x.ClosedDateTime < endDate).ToList();

                var yearStatistics = new IssueStatistics
                {
                    CodegardenYear = year,
                    Title = date.ToString(monthly ? "yyyyMM" : "yyyy"),
                    CreatedIssues = yearIssues.Count,
                    ClosedIssues = closedYearIssues.Count
                };

                var allFirstCommentTimesInHours = new List<double>();
                var allClosingTimesInHours = new List<double>();

                foreach (var issue in yearIssues)
                {
                    var firstTeamComment = GetFirstTeamComment(issue);

                    DateTime labeledAt = default;
                    var labeledEvent = issue.Events.FirstOrDefault(x => x.Name == "labeled");
                    if (labeledEvent != null)
                        labeledAt = labeledEvent.CreateDateTime;

                    if (firstTeamComment == null)
                    {
                        // For now we'll treat a label as a "comment"
                        // Only HQ can add labels
                        if (labeledAt != default)
                        {
                            var timeSpan = Convert.ToInt32(issue.CreateDateTime.BusinessHoursUntil(labeledAt));
                            allFirstCommentTimesInHours.Add(timeSpan);
                            yearStatistics.TeamCommentMissing = yearStatistics.TeamCommentMissing + 1;
                        }
                    }
                    else
                    {
                        var timeSpan = Convert.ToInt32(issue.CreateDateTime.BusinessHoursUntil(firstTeamComment.CreateDateTime));
                        allFirstCommentTimesInHours.Add(timeSpan);
                        if (timeSpan <= 48)
                        {
                            yearStatistics.FirstTeamCommentOnTime = yearStatistics.FirstTeamCommentOnTime + 1;
                        }
                        else
                        {
                            yearStatistics.FirstTeamCommentLate = yearStatistics.FirstTeamCommentLate + 1;
                        }
                    }

                    if (issue.ClosedDateTime != null)
                    {
                        var timeSpan = Convert.ToInt32(issue.CreateDateTime.BusinessHoursUntil(issue.ClosedDateTime.Value));
                        allClosingTimesInHours.Add(timeSpan);

                        if (issue.Labels.Any(x => x.Name.StartsWith("release/")))
                            yearStatistics.ReleaseIssues = yearStatistics.ReleaseIssues + 1;
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

        public Comments GetFirstTeamComment(Issue issue)
        {
            Comments firstTeamCommment = null;
            var gitHubService = new GitHubService();
            var users = gitHubService.GetTeam(issue.RepositoryName).Members.Select(x => x.ToLower());
            var foundComment = issue.Comments.OrderBy(x => x.CreateDateTime).FirstOrDefault(x => users.Contains(x.User.Login.ToLowerInvariant()));
            if (foundComment != null)
                firstTeamCommment = foundComment;

            return firstTeamCommment;
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
    }

    public class IssueStatistics
    {
        public string Title { get; set; }
        public int CodegardenYear { get; set; }
        public int CreatedIssues { get; set; }
        public int ClosedIssues { get; set; }
        public int ReleaseIssues { get; set; }
        public int FirstTeamCommentOnTime { get; set; }
        public int FirstTeamCommentLate { get; set; }
        public int TeamCommentMissing { get; set; }
        public int AverageHoursToFirstComment { get; set; }
        public int MedianHoursToFirstComment { get; set; }
        public int AverageHoursToClose { get; set; }
        public int MedianHoursToClose { get; set; }
    }
}
