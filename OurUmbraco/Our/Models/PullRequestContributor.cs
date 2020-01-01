using System;

namespace OurUmbraco.Our.Models
{
    public class PullRequestContributor
    {
        public string Username { get; set; }
        public int Contributions { get; set; }
        public int OpenContributions { get; set; }
        public int ClosedContributions { get; set; }
        public int MergedContributions { get; set; }
        public DateTime? FirstContribution { get; set; }
    }
}
