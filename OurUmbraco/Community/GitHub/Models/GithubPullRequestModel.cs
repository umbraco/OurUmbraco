using System;

namespace OurUmbraco.Community.GitHub.Models
{
    public class GithubPullRequestModel
    {
        public int Id { get; set; }
        public string Repository { get; set; }
        public string State { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? MergedAt { get; set; }
        public User User { get; set; }
    }
    
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
    }
}
