using System.Collections.Generic;

namespace OurUmbraco.Our.Models
{

    public class PullRequestsInPeriod
    {
        /// <summary>
        /// The year and month that this period covers
        /// </summary>
        public string MonthYear { get; set; }

        /// <summary>
        /// The year and month that this period covers, formatted nicely as abbreviated month  + year
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Total number of PRs received in this period
        /// </summary>
        public int NumberCreated { get; set; }

        /// <summary>
        /// PR closed without merging in this period
        /// </summary>
        public int NumberClosed { get; set; }

        /// <summary>
        /// PR Merged in this period
        /// </summary>
        public int NumberMerged { get; set; }

        /// <summary>
        /// PR Merged in this period that were created since January 2018
        /// We stop in January since we weren't very good at maintaining this before 
        /// </summary>
        public int NumberMergedRecent { get; set; }

        /// <summary>
        /// Number of PRs that were created in this period and merged within 30 days after creating the PR
        /// </summary>
        public int NumberMergedInThirtyDays { get; set; }

        /// <summary>
        /// Number of PRs that were created in this period and merged, but not merged within 30 days after creating the PR
        /// </summary>
        public int NumberNotMergedInThirtyDays { get; set; }

        /// <summary>
        ///  Total number of pull requests still open per month 
        /// </summary>
        public int TotalNumberOpen { get; set; }

        /// <summary>
        ///  Total number of pull requests still open per month which were created after CodeGarden 2018
        /// </summary>
        public int TotalNumberOpenAfterCodeGarden18 { get; set; }

        /// <summary>
        /// First PR submitted in this period
        /// </summary>
        public int NumberOfNewContributors { get; set; }

        /// <summary>
        /// Usernames of the first time contributors
        /// </summary>
        public List<string> NewContributors { get; set; }

        /// <summary>
        /// All contributors that submitted a pull-request within the past year 
        /// </summary>
        public int NumberOfActiveContributorsInPastYear { get; set; }

        /// <summary>
        /// The total number of hours it took to merge all PRs in this period
        /// </summary>
        public int TotalMergeTimeInHours { get; set; }

        /// <summary>
        /// Average time in hours between create and merge date
        /// </summary>
        public int AveragePullRequestClosingTimeInHours { get; set; }
       
        /// <summary>
        /// Median time in hours between create and merge date
        /// </summary>
        public int MedianPullRequestClosingTimeInHours { get; set; }


        public string AllPullRequestClosingTimesInHours { get; set; }

        public string AllFirstTeamCommentTimesInHours { get; set; }

        public int TargetPullRequestClosingTimeInHours { get; set; } = 720;

        /// <summary>
        /// Average time in hours between create and first comment from PR team
        /// </summary>
        public int AverageFirstTeamCommentTimeInHours { get; set; }

        public int TargetFirstTeamCommentTimeInHours { get; set; } = 48;

        /// <summary>
        /// Median time in hours between create and first comment from PR team
        /// </summary>
        public int MedianFirstTeamCommentTimeInHours { get; set; }

        /// <summary>
        /// All the contributors over all time who are not part of HQ
        /// </summary>
        public int TotalNumberOfContributors { get; set; }

        public int NumberOfActiveContributorsAfterCg18 { get; set; }
        public int NumberNewOfActiveContributorsAfterCg18 { get; set; }
    }
}
