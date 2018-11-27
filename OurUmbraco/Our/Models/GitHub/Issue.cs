using System;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Time;
using Skybrud.Social.GitHub.Models.Issues;
using Skybrud.Social.GitHub.Models.Labels;
using Skybrud.Social.GitHub.Models.Users;

namespace OurUmbraco.Our.Models.GitHub
{
    public class Issue
    {

        private readonly GitHubIssue _issue;
        private Comments[] _comments;
        private Event[] _events;

        public string Link => _issue.Urls.HtmlUrl;

        public int Number => _issue.Number;

        public string Title => _issue.Title;

        public GitHubUserItem User => _issue.User;

        public GitHubLabel[] Labels => _issue.Labels;

        public GitHubIssueState State => _issue.State;

        public GitHubUserItem[] Assignees => _issue.Assignees;

        public int CommentCount => _issue.Comments;

        public EssentialsDateTime CreateDateTime => _issue.CreatedAt;

        public EssentialsDateTime UpdateDateTime => _issue.UpdatedAt;

        public EssentialsDateTime ClosedDateTime => _issue.ClosedAt;

        public string Description => _issue.Body;

        public Comments[] Comments => _comments ?? (_comments = _issue.JObject.Value<Comments[]>("comments"));

        public Event[] Events => _events ?? (_events = _issue.JObject.Value<Event[]>("events"));

        // Custom properties

        public string RepositoryName => Link.Split('/')[4];

        public DateTime? FirstPrTeamOrHqComment { get; set; }

        public DateTime? InThisCategorySince { get; set; }

        public bool NeedsTeamUmbracoReply { get; set; }
        
        protected Issue(JObject obj) {
            _issue = GitHubIssue.Parse(obj);
        }

        public static Issue Parse(JObject obj) {
            return obj == null ? null : new Issue(obj);
        }

        // Note: leaving the other properties commented out in case we need them later

        //public string repository_url { get; set; }
        //public string labels_url { get; set; }
        //public string comments_url { get; set; }
        //public string events_url { get; set; }
        //public int id { get; set; }
        //public string node_id { get; set; }
        //public bool locked { get; set; }
        //public User assignee { get; set; }
        //public object milestone { get; set; }
        //public string author_association { get; set; }

    }

}