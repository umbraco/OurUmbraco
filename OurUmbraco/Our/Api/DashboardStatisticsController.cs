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
        public List<ProcessedPullRequest> GetPullRequestCloseData(int startMonth = 6, int startYear = 2010, string repository = "")
        {
            var repoService = new RepositoryManagementService();

            var pulls = string.IsNullOrWhiteSpace(repository)
                ? repoService.GetAllIssues(true).ToList()
                : repoService.GetAllIssues(true).Where(x => x.RepositoryName == repository).ToList();
            
            var date = new DateTime(startYear, startMonth, 1);
            var prsCreated = pulls.Where(x => x.CreateDateTime >= date).ToList();

            var processedPullRequests = new List<ProcessedPullRequest>();
            foreach (var pr in prsCreated)
            {
                if(pr.State != "closed")
                    // still open, continue
                    continue;
                
                var processedPr = new ProcessedPullRequest();
                processedPr.Number = pr.Number;
                processedPr.Repository = pr.RepositoryName;
                processedPr.Title = pr.Title;
                if(pr.ClosedDateTime.HasValue)
                    processedPr.CloseDateTime = pr.ClosedDateTime.Value;
                processedPr.ClosedByUser = pr.CloseUser?.Login;
                
                // GitHub marks all merged PRs as "closed", so we want to differentiate: if there's no "merged" event then it was closed without merging
                if (pr.Events.Any(y => y.Name == "merged") == false)
                {
                    var closeEvent = pr.Events.LastOrDefault(y => y.Name == "closed");
                    processedPr.ClosedByUser = closeEvent?.Actor?.Login;
                    processedPr.CloseType = "closed";
                }

                // There was a "merged" event
                if (pr.Events.Any(y => y.Name == "merged"))
                {
                    var mergeEvent = pr.Events.LastOrDefault(y => y.Name == "merged");
                    processedPr.ClosedByUser = mergeEvent?.Actor?.Login;
                    processedPr.CloseType = "merged";
                }

                processedPullRequests.Add(processedPr);
            }

            return processedPullRequests;
        }
        
        [MemberAuthorize(AllowGroup = "HQ")]
        [HttpGet]
        public List<ApproveddPullRequest> GetApprovedRequestCloseData(int startMonth = 6, int startYear = 2010, string repository = "")
        {
            var repoService = new RepositoryManagementService();
            var date = new DateTime(startYear, startMonth, 1);
            
            var approvedPulls = string.IsNullOrWhiteSpace(repository)
                ? repoService.GetAllIssues(true).Where(x => x.CreateDateTime >= date && x.State != "closed" && x.Reviews.Any(y => y.State == "APPROVED")).ToList()
                : repoService.GetAllIssues(true).Where(x => x.CreateDateTime >= date && x.State != "closed" && x.Reviews.Any(y => y.State == "APPROVED") && x.RepositoryName == repository).ToList();

            var approvedPullRequestData = new List<ApproveddPullRequest>();
            foreach (var pull in approvedPulls)
            {
                var approvals = pull.Reviews.Where(x => x.State == "APPROVED").ToList();
                var approvalNames = new List<string>();
                foreach (var approval in approvals)
                {
                    if(approvalNames.Contains(approval.Actor.Login) == false)
                        approvalNames.Add(approval.Actor.Login);
                }
                var firstApproval = approvals.OrderBy(x => x.CreateDateTime).First();
                var approvers = string.Join(",", approvalNames);
                approvedPullRequestData.Add(new ApproveddPullRequest
                {
                    Number = pull.Number,
                    Repository = pull.RepositoryName,
                    Title = pull.Title,
                    ApprovedDateTime = firstApproval.CreateDateTime,
                    ApprovedByUser = approvers
                });
            }
            
            return approvedPullRequestData;
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

            if (firstTeamEventDateTime == default(DateTime) && issue.ClosedDateTime == null)
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
            var events = issue.Events.Where(x => x.Actor != null && users.Contains(x.Actor.Login.ToLowerInvariant()) && x.Name != "mentioned" && x.Name != "subscribed" && (x.Assigner == null || x.Assigner.Login != "wafflebot[bot]"));
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

        [MemberAuthorize(AllowGroup = "HQ")]
        [HttpGet]
        public List<Issue> GetOldUpForGrabsIssues()
        {
            var repoService = new RepositoryManagementService();
            var allCommunityIssues = repoService.GetAllCommunityIssues(false)
                .Where(x => x.ClosedDateTime == null && x.Labels.Any(l => l.Name == "status/idea") == false).ToList();

            var oldDate = DateTime.Now.AddDays(-120);

            var upForGrabsIssues = allCommunityIssues
                .Where(x => x.Labels.Any(l => l.Name == "community/up-for-grabs" || l.Name == "help wanted"))
                .OrderBy(x => x.CreateDateTime)
                .ToList();

            var oldUpForGrabsIssues = new List<Issue>();

            foreach (var issue in upForGrabsIssues)
            {
                foreach (var issueEvent in issue.Events)
                {
                    if (issueEvent.Name == "labeled" &&
                        (issueEvent.Label.Name == "community/up-for-grabs" || issueEvent.Label.Name == "help wanted"))
                    {
                        if (issueEvent.CreateDateTime <= oldDate)
                        {
                            issue.SetToUpForGrabs = issueEvent.CreateDateTime;
                            if (oldUpForGrabsIssues.Any(x => x.Number == issue.Number) == false)
                                oldUpForGrabsIssues.Add(issue);
                        }
                    }
                }
            }

            return oldUpForGrabsIssues;
        }

        [MemberAuthorize(AllowGroup = "HQ")]
        [HttpGet]
        public List<Issue> GetIssuesWithLabel(string label)
        {
            var repoService = new RepositoryManagementService();
            var labelIssues = repoService.GetAllCommunityIssues(false)
                .Where(x => x.ClosedDateTime == null 
                            && x.Labels.Any(l => l.Name == "status/idea") == false 
                            && x.Labels.Any(l => string.Equals(l.Name, label, StringComparison.InvariantCultureIgnoreCase))).ToList();
            
            foreach (var issue in labelIssues)
            {
                foreach (var issueEvent in issue.Events)
                {
                    if (issueEvent.Name == "labeled" && string.Equals(issueEvent.Label.Name, label, StringComparison.InvariantCultureIgnoreCase))
                    {
                        issue.LabelAdded = issueEvent.CreateDateTime;
                    }
                }
            }

            return labelIssues.OrderByDescending(x => x.LabelAdded).ToList();
        }

        [MemberAuthorize(AllowGroup = "HQ")]
        [HttpGet]
        public List<Issue> GetAllOpenIssues(string label)
        {
            var repoService = new RepositoryManagementService();
            var allOpenIssues = repoService.GetAllCommunityIssues(false).Where(x => x.ClosedDateTime == null).ToList();

            foreach (var issue in allOpenIssues)
            {
                var firstLabel = issue.Events.OrderBy(x => x.CreateDateTime).FirstOrDefault(x => x.Name == "labeled");
            }
            

            foreach (var issue in allOpenIssues)
            {
                var firstComment = issue.Comments.OrderBy(x => x.CreateDateTime).FirstOrDefault();

            }

            // Added label by HQ
            // -- Discussion / HQ Reply
            // -- Estimation
            // 

            //foreach (var issue in labelIssues)
            //{
            //    foreach (var issueEvent in issue.Events)
            //    {
            //        if (issueEvent.Name == "labeled" && string.Equals(issueEvent.Label.Name, label, StringComparison.InvariantCultureIgnoreCase))
            //        {
            //            issue.LabelAdded = issueEvent.CreateDateTime;
            //        }
            //    }
            //}

            return allOpenIssues.ToList();
        }

        [MemberAuthorize(AllowGroup = "HQ")]
        [HttpGet]
        public List<Issue> GetOpenPulls()
        {
            var repoService = new RepositoryManagementService();
            var openPrs = repoService.GetAllCommunityIssues(true)
                .Where(x => x.ClosedDateTime == null).ToList();

            foreach (var pr in openPrs)
            {
                var lastReview = pr.Reviews.LastOrDefault();
                if (lastReview != null)
                {
                    pr.ReviewState = lastReview.State;
                    pr.LastReviewDate = lastReview.CreateDateTime; 
                }

            }

            return openPrs.OrderByDescending(x => x.LastReviewDate).ToList();
        }

        [MemberAuthorize(AllowGroup = "HQ")]
        [HttpGet]
        public List<Issue> GetIdeasIssues()
        {
            var repoService = new RepositoryManagementService();
            var ideasIssues = repoService.GetAllCommunityIssues(false)
                .Where(x => x.ClosedDateTime == null && x.Labels.Any(l => l.Name == "status/idea")).ToList();

            foreach (var issue in ideasIssues)
            {
                var ideaLabelEvent = issue.Events.FirstOrDefault(x => x.Name == "labeled" && x.Label.Name == "status/idea");
                if (ideaLabelEvent != null)
                    issue.SetToIdea = ideaLabelEvent.CreateDateTime;
            }

            return ideasIssues;
        }

        [MemberAuthorize(AllowGroup = "HQ")]
        [HttpGet]
        public List<Issue> GetNoCommentIssues()
        {
            var repoService = new RepositoryManagementService();
            var communityIssues = repoService.GetAllCommunityIssues(false)
                .Where(x => x.ClosedDateTime == null && x.Labels.Any(l => l.Name == "status/idea") == false).ToList();

            var noCommentIssues = new List<Issue>();

            foreach (var issue in communityIssues)
            {
                var firstTeamEvent = GetFirstTeamEventDateTime(issue);
                if (firstTeamEvent == null || firstTeamEvent == default(DateTime))
                    noCommentIssues.Add(issue);
            }

            return noCommentIssues;
        }

        [MemberAuthorize(AllowGroup = "HQ")]
        [HttpGet]
        public List<Issue> GetAssignedIssues()
        {
            var repoService = new RepositoryManagementService();
            var assignedIssues = repoService.GetAllCommunityIssues(false)
                .Where(x => x.ClosedDateTime == null && x.Labels.Any(l => l.Name == "status/idea") == false && x.Assignees.Any()).ToList();

            return assignedIssues;
        }
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

public class ProcessedPullRequest
{
    public int Number { get; set; }
    
    public string Repository { get; set; }
    public string Title { get; set; }
    public DateTime CloseDateTime { get; set; }
    public string CloseType { get; set; }
    public string ClosedByUser { get; set; }
}

public class ApproveddPullRequest
{
    public int Number { get; set; }
    public string Repository { get; set; }
    public string Title { get; set; }
    public DateTime ApprovedDateTime { get; set; }
    public string ApprovedByUser { get; set; }
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