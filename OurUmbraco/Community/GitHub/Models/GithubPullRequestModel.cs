using System;
using Newtonsoft.Json;

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
        public GithubPullRequestUser User { get; set; }
        [JsonProperty("_comments", NullValueHandling = NullValueHandling.Ignore)]
        public GithubPullRequestComment[] Comments { get; set; }
        public int FirstTeamCommentTimeInHours { get; set; }
        public Label[] Labels { get; set; }
    }

    public class GithubPullRequestUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
    }

    public class GithubPullRequestComment
    {
        public int id { get; set; }
        public GithubPullRequestUser user { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
