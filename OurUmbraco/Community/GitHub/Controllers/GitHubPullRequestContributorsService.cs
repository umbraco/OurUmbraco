using System;
using System.Linq;
using System.Collections.Generic;
using OurUmbraco.Community.GitHub.Models;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace OurUmbraco.Community.GitHub.Controllers
{
    public class GitHubPullRequestContributorsService
    {
        public IEnumerable<AuthorPrs> Contributors()
        {
            var database = ApplicationContext.Current.DatabaseContext.Database;
            var service = new GitHubService();
            var hqMembers = new HashSet<string>(service.GetHqMembers());

            var query = new Sql()
                .Select("authorlogin as username, min(authorurl) as url, min(authoravatarurl) as avatarurl, count(*) as pullrequestcount, sum(additions) as additions, sum(deletions) as deletions")
                .From<GitHubPullRequestDataModel>(new SqlServerSyntaxProvider())
                .Append("where lastModified > @0", DateTime.Now.AddYears(-1))
                .GroupBy("authorlogin");

            var allPrs = database.Fetch<AuthorPrs>(query);

            return allPrs
                .Where(p => !hqMembers.Contains(p.Username.ToLowerInvariant()));
        }

    }

    public class AuthorPrs
    {
        public int PullRequestCount { get; set; }
        public string Username { get; set; }
        public string Url {get;set;}
        public string AvatarUrl {get;set;}
        public int Additions { get; set; }
        public int Deletions { get; set; }
    }
}
