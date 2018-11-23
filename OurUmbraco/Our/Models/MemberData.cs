using System.Collections.Generic;
using System.Drawing;
using OurUmbraco.Forum.Models;
using Umbraco.Core.Models;

namespace OurUmbraco.Our.Models
{
    public class MemberData
    {
        public int Id => Member.Id;
        public IPublishedContent Member { get; set; }
        public string Email { get; set; }
        public string TwitterHandle { get; set; }
        public string GitHubUsername { get; set; }
        public bool HasGitHubUsername => !string.IsNullOrWhiteSpace(GitHubUsername);
        public int Karma { get; set; }
        public bool IsAdmin { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<ReadOnlyTopic> LatestTopics { get; set; }
        public int NumberOfForumPosts { get; set; }
        public string AvatarHtml { get; set; }
        public string AvatarPath { get; set; }
        public Image AvatarImage { get; set; }
        public bool AvatarImageTooSmall { get; set; }
        public bool IsBlocked { get; set; }
        public bool NewTosAccepted { get; set; }
    }
}
