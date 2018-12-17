namespace OurUmbraco.Our.Models
{

    public class IssuesInPeriod
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
        /// Total number of issues received in this period
        /// </summary>
        public int NumberCreated { get; set; }

        /// <summary>
        /// Issues closed in this period
        /// </summary>
        public int NumberClosed { get; set; }

        /// <summary>
        /// Issues still open in this period
        /// </summary>
        public int NumberOpen { get; set; }

        /// <summary>
        /// For each closed issues in this period how long it took to close the issue
        /// For example: "15,653" means that there were two issues closed in this period and they were open for 15 and 653 hours
        /// </summary>
        public string AllIssueClosingTimesInHours { get; set; }
        public double IssueAverageClosingTimeInHours { get; set; }
        public double IssueMedianClosingTimeInHours { get; set; }
        public double TargetClosingTimeInHours { get; set; }

        /// <summary>
        /// For each issue with a first HQ/PR Team comment, how long it took for the first comment to to be added to the issue
        /// For example: "12,293" means that there were two issues where the first comment came in after for 12 and 293 hours
        /// </summary>
        public string AllIssueFirstCommentTimesInHours { get; set; }
        public double IssueAverageFirstCommentTimesInHours { get; set; }
        public double IssueMedianFirstCommentTimesInHours { get; set; }
        public double TargetFirstCommentTimeInHours { get; set; }

    }
}
