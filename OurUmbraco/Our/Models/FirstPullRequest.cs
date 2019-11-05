using System;

namespace OurUmbraco.Our.Models
{
    public class FirstPullRequest
    {
        public string Username { get; set; }
        
        public int IssueNumber { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        
        public DateTime CreateDateTime { get; set; }
        
        public string RepositoryName { get; set; }
    }
}
