using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Web.Http;
using Newtonsoft.Json;
using OurUmbraco.Community.GitHub.Models;
using OurUmbraco.Our.Models;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class PullRequestStatisticsController : UmbracoAuthorizedApiController
    {
        public readonly string JsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/GithubPullRequests.json");
        public readonly string HqMembers = HostingEnvironment.MapPath("~/config/githubhq.txt");

        [HttpGet]
        public List<PullRequestsInPeriod> GetGroupedPullRequestData(DateTime fromDate, DateTime toDate, string repository = "Umbraco-CMS")
        {
            var pullsNonHq = GetPullsNonHq(repository);

            var mergedPullsInPeriod = pullsNonHq
                .Where(x => x.MergedAt != null
                            && x.MergedAt > fromDate &&
                            x.MergedAt <= toDate)
                .OrderBy(x => x.MergedAt)
                .GroupBy(x => new { x.MergedAt.Value.Year, x.MergedAt.Value.Month })
                .ToDictionary(x => x.Key, x => x.ToList());

            var closedPullsInPeriod = pullsNonHq
                .Where(x => x.ClosedAt != null
                            && x.ClosedAt > fromDate &&
                            x.ClosedAt <= toDate)
                .OrderBy(x => x.ClosedAt)
                .GroupBy(x => new { x.ClosedAt.Value.Year, x.ClosedAt.Value.Month })
                .ToDictionary(x => x.Key, x => x.ToList());

            var createdPullsInPeriod = pullsNonHq
                .Where(x => x.CreatedAt != null
                            && x.CreatedAt > fromDate &&
                            x.CreatedAt <= toDate)
                .OrderBy(x => x.CreatedAt)
                .GroupBy(x => new { x.CreatedAt.Value.Year, x.CreatedAt.Value.Month })
                .ToDictionary(x => x.Key, x => x.ToList());

            var firstPrs = new List<FirstPullRequest>();
            foreach (var pr in pullsNonHq)
                if (pr.MergedAt != null && firstPrs.Any(x => x.Username == pr.User.Login) == false)
                    firstPrs.Add(new FirstPullRequest { Username = pr.User.Login, Year = pr.MergedAt.Value.Year, Month = pr.MergedAt.Value.Month });

            var groupedPrs = new List<PullRequestsInPeriod>();

            foreach (var prInPeriod in mergedPullsInPeriod)
            {
                var recentPrsMerged = 0;
                var totalMergeTimeInHours = 0;
                var totalMergedOnTime = 0;
                var totalMergedNotOnTime = 0;

                foreach (var pr in prInPeriod.Value)
                {
                    var mergeTimeInHours = Convert.ToInt32(pr.MergedAt.Value.Subtract(pr.CreatedAt.Value).TotalHours);
                    var mergeTimeInDays = Convert.ToInt32(pr.MergedAt.Value.Subtract(pr.CreatedAt.Value).TotalDays);

                    if (pr.CreatedAt >= new DateTime(2018, 01, 01))
                    {
                        recentPrsMerged = recentPrsMerged + 1;
                        totalMergeTimeInHours = totalMergeTimeInHours + mergeTimeInHours;
                    }

                    if (mergeTimeInDays <= 30)
                        totalMergedOnTime = totalMergedOnTime + 1;
                    else
                        totalMergedNotOnTime = totalMergedNotOnTime + 1;
                }

                var period = $"{prInPeriod.Key.Year}{prInPeriod.Key.Month:00}";
                var mergedPrs = new PullRequestsInPeriod
                {
                    MonthYear = period,
                    NumberMerged = prInPeriod.Value.Count,
                };

                var firstPrsUsernames = new List<string>();
                foreach (var firstPr in firstPrs)
                {
                    if (firstPr.Year == prInPeriod.Key.Year && firstPr.Month == prInPeriod.Key.Month)
                        firstPrsUsernames.Add(firstPr.Username);
                }

                mergedPrs.NewContributors = firstPrsUsernames;
                mergedPrs.NumberOfNewContributors = firstPrsUsernames.Count;
                mergedPrs.NumberMergedInThirtyDays = totalMergedOnTime;
                mergedPrs.NumberNotMergedInThirtyDays = totalMergedNotOnTime;
                mergedPrs.NumberMergedRecent = recentPrsMerged;

                mergedPrs.TotalMergeTimeInHours = totalMergeTimeInHours;
                var averageMergeTime = 0;
                if (recentPrsMerged > 0)
                    averageMergeTime = Convert.ToInt32(mergedPrs.TotalMergeTimeInHours / recentPrsMerged);
                mergedPrs.AveragePullRequestClosingTimeInHours = averageMergeTime;

                groupedPrs.Add(mergedPrs);
            }

            foreach (var prInPeriod in closedPullsInPeriod)
            {
                var period = $"{prInPeriod.Key.Year}{prInPeriod.Key.Month:00}";
                var prs = new PullRequestsInPeriod();
                if (groupedPrs.Any(x => x.MonthYear == period))
                    prs = groupedPrs.First(x => x.MonthYear == period);
                else
                    prs.MonthYear = period;

                prs.NumberClosed = prInPeriod.Value.Count - prs.NumberMerged;
            }

            foreach (var prInPeriod in createdPullsInPeriod)
            {
                var period = $"{prInPeriod.Key.Year}{prInPeriod.Key.Month:00}";
                var prs = new PullRequestsInPeriod();
                if (groupedPrs.Any(x => x.MonthYear == period))
                    prs = groupedPrs.First(x => x.MonthYear == period);
                else
                    prs.MonthYear = period;

                prs.NumberCreated = prInPeriod.Value.Count;
            }

            foreach (var pullRequestsInPeriod in groupedPrs)
            {
                var currentYear = int.Parse(pullRequestsInPeriod.MonthYear.Substring(0, 4));
                var currentMonth = int.Parse(pullRequestsInPeriod.MonthYear.Substring(4));
                var lastDayInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
                var maxDateInPeriod = new DateTime(currentYear, currentMonth, lastDayInMonth, 23, 59, 59);

                var openPrsForPeriod = new List<GithubPullRequestModel>();

                // if the PR was created in or before this period and is not closed or merged IN this period then it counts as open for this period
                var pullsCreatedBeforePeriod = pullsNonHq.Where(x => x.CreatedAt.Value <= maxDateInPeriod).OrderBy(x => x.CreatedAt);
                foreach (var pr in pullsCreatedBeforePeriod)
                {
                    if (pr.ClosedAt == null)
                        openPrsForPeriod.Add(pr);
                    else
                        // Was it closed (merged items also get set as closed) after the current period? Then it's stil open for this period.
                    if (pr.ClosedAt != null && pr.ClosedAt > maxDateInPeriod)
                        openPrsForPeriod.Add(pr);
                }
                pullRequestsInPeriod.TotalNumberOpen = openPrsForPeriod.Count;
                foreach (var githubPullRequestModel in openPrsForPeriod)
                {
                    if (githubPullRequestModel.CreatedAt > DateTime.Parse("2018-05-25") && githubPullRequestModel.MergedAt == null || githubPullRequestModel.MergedAt == DateTime.MinValue)
                    {
                        pullRequestsInPeriod.TotalNumberOpenAfterCodeGarden18 = pullRequestsInPeriod.TotalNumberOpenAfterCodeGarden18 + 1;
                    }
                }

                var activeContributors = new List<string>();
                foreach (var pr in pullsNonHq)
                {
                    var startDate = maxDateInPeriod.AddYears(-1).AddMonths(-1).AddDays(1);
                    if (pr.CreatedAt <= maxDateInPeriod && pr.CreatedAt >= startDate)
                        if (activeContributors.Any(x => x == pr.User.Login) == false)
                            activeContributors.Add(pr.User.Login);
                }

                pullRequestsInPeriod.NumberOfActiveContributorsInPastYear = activeContributors.Count;

                pullRequestsInPeriod.TotalNumberOfContributors = firstPrs.Count;
            }
            
            return groupedPrs;
        }

        private List<GithubPullRequestModel> GetPullsNonHq(string repository)
        {
            var content = System.IO.File.ReadAllText(JsonPath);
            var pulls = JsonConvert.DeserializeObject<List<GithubPullRequestModel>>(content).Where(x => x.Repository == repository);
            var hqList = System.IO.File.ReadAllText(HqMembers).Split('\n');
            var pullsNonHq = new List<GithubPullRequestModel>();
            foreach (var pull in pulls)
            {
                if (pull?.User?.Login == null)
                    continue;

                var isHq = hqList.Any(
                    y => string.Equals(y.Trim(), pull.User.Login, StringComparison.InvariantCultureIgnoreCase));
                if (isHq == false)
                    pullsNonHq.Add(pull);
            }

            pullsNonHq = pullsNonHq.ToList();
            return pullsNonHq;
        }
    }
}
