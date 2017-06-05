using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OurUmbraco.Community.Models
{
    [DataContract]
    public class GitHubContributorModel : IGitHubContributorModel
    {

        private List<Week> _weeks;

        public int Total { get; set; }
        public int TotalAdditions { get; set; }
        public int TotalDeletions { get; set; }

        [DataMember(Name = "weeks")]
        public List<Week> Weeks
        {
            get { return _weeks; }

            set
            {
                _weeks = value;


                int totalAdditions = 0;
                int totalDeletions = 0;
                foreach (var week in _weeks)
                {
                    totalDeletions += week.D;
                    totalAdditions += week.A;
                }

                TotalAdditions = totalAdditions;
                TotalDeletions = totalDeletions;

            }

        }
        public Author Author { get; set; }
    }

    [DataContract]
    public class Week
    {
        /// <summary>
        /// Timestamp
        /// </summary>
        public int W { get; set; }

        /// <summary>
        /// Additions
        /// </summary>
        public int A { get; set; }

        /// <summary>
        /// Deletions
        /// </summary>
        public int D { get; set; }

        /// <summary>
        /// Commits
        /// </summary>
        public int C { get; set; }
    }

    [DataContract]
    public class Author
    {
        public string Login { get; set; }

        public int Id { get; set; }

        [DataMember(Name = "avatar_url")]
        public string AvatarUrl { get; set; }

        [DataMember(Name = "gravatar_id")]
        public string GravatarId { get; set; }

        public string Url { get; set; }

        [DataMember(Name = "html_url")]
        public string HtmlUrl { get; set; }

        [DataMember(Name = "followers_url")]
        public string FollowersUrl { get; set; }

        [DataMember(Name = "following_url")]
        public string FollowingsUrl { get; set; }

        [DataMember(Name = "gists_url")]
        public string GistsUrl { get; set; }

        [DataMember(Name = "starred_url")]
        public string StarredUrl { get; set; }

        [DataMember(Name = "subscriptions_url")]
        public string SubscriptionsUrl { get; set; }

        [DataMember(Name = "organizations_url")]
        public string OrganizationsUrl { get; set; }

        [DataMember(Name = "repos_url")]
        public string ReposUrl { get; set; }

        [DataMember(Name = "events_url")]
        public string EventsUrl { get; set; }

        [DataMember(Name = "received_events_url")]
        public string ReceivedEventsUrl { get; set; }

        public string Type { get; set; }

        [DataMember(Name = "site_admin")]
        public bool SiteAdmin { get; set; }
    }
}