using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace OurUmbraco.Our.Services
{
    public class RepositoryManagementService
    {
        private static readonly string IssuesBaseDirectory = HostingEnvironment.MapPath("~/App_Data/TEMP/GitHub/");

        public List<GitHubCategorizedIssues> GetAllOpenIssues()
        {
            var openIssues = new List<GitHubCategorizedIssues>
            {
                new GitHubCategorizedIssues
                {
                    SortOrder = 1, CategoryDescription = "No reply", CategoryKey = CategoryKey.NoReply, Issues = new List<GitHubIssueModel>()
                },
                new GitHubCategorizedIssues
                {
                    SortOrder = 10, CategoryDescription = "Up For Grabs", CategoryKey = CategoryKey.UpForGrabs, Issues = new List<GitHubIssueModel>()
                },
                new GitHubCategorizedIssues
                {
                    SortOrder = 20, CategoryDescription = "Pull Request Pending", CategoryKey = CategoryKey.PullRequestPending, Issues = new List<GitHubIssueModel>()
                },
                new GitHubCategorizedIssues
                {
                    SortOrder = 10000, CategoryDescription = "Other", CategoryKey = CategoryKey.Other, Issues = new List<GitHubIssueModel>()
                }
            };

            var pullRequestService = new PullRequestService();
            var hqMembers = pullRequestService.GetHqMembers();
            var teamMembers = pullRequestService.GetTeamMembers();

            foreach (var directory in Directory.EnumerateDirectories(IssuesBaseDirectory))
            {
                var directoryName = directory.Split('\\').Last();
                var repositoryName = directoryName.Substring(directoryName.LastIndexOf("__", StringComparison.Ordinal) + 2);
                var issuesDirectory = directory + "\\issues\\";
                var issueFiles = Directory.EnumerateFiles(issuesDirectory, "*.combined.json");

                foreach (var file in issueFiles)
                {
                    var fileContent = File.ReadAllText(file);
                    var item = JsonConvert.DeserializeObject<GitHubIssueModel>(fileContent);
                    if (item.state == "closed")
                        continue;
                    if (hqMembers.Contains(item.user.login.ToLowerInvariant()))
                        continue;

                    foreach (var comment in item._comments)
                    {
                        var commenter = comment.user.login.ToLowerInvariant();
                        if (hqMembers.Contains(commenter))
                            item.hasPrTeamComment = true;
                    }

                    item.repositoryName = repositoryName;

                    if (item.labels.Length == 0 && item.comments == 0)
                    {
                        var noFirstReplyCategory = openIssues.First(x => x.CategoryKey == CategoryKey.NoReply);
                        noFirstReplyCategory.Issues.Add(item);
                        continue;
                    }

                    if (item.labels.Length != 0)
                    {
                        var matchedLabel = false;
                        foreach (var label in item.labels)
                        {
                            if (label.name != "community/pr")
                                continue;

                            matchedLabel = true;
                            var prPendingCategory = openIssues.First(x => x.CategoryKey == CategoryKey.PullRequestPending);
                            prPendingCategory.Issues.Add(item);
                        }

                        // only go to the next item in the
                        // foreach if we found a match here
                        if (matchedLabel)
                            continue;

                        foreach (var label in item.labels)
                        {
                            if (label.name != "community/up-for-grabs" && label.name != "up-for-grabs" && label.name != "help wanted")
                                continue;

                            matchedLabel = true;
                            var upForGrabsCategory = openIssues.First(x => x.CategoryKey == CategoryKey.UpForGrabs);
                            // We're matching on several labels, so only add the item once in case there's multiple matches
                            if(upForGrabsCategory.Issues.Contains(item) == false)
                                upForGrabsCategory.Issues.Add(item);
                        }

                        // only go to the next item in the
                        // foreach if we found a match here
                        if (matchedLabel)
                            continue;
                    }

                    var otherCategory = openIssues.First(x => x.CategoryKey == CategoryKey.Other);
                    otherCategory.Issues.Add(item);
                }
            }

            return openIssues;
        }

        public enum CategoryKey
        {
            NoReply,
            UpForGrabs,
            PullRequestPending,
            Other
        }

        public class GitHubCategorizedIssues
        {
            public int SortOrder { get; set; }
            public CategoryKey CategoryKey { get; set; }
            public string CategoryDescription { get; set; }
            public List<GitHubIssueModel> Issues { get; set; }
        }

        public class GitHubIssueModel
        {
            public string repositoryName { get; set; }
            public string url { get; set; }
            public string repository_url { get; set; }
            public string labels_url { get; set; }
            public string comments_url { get; set; }
            public string events_url { get; set; }
            public string html_url { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public int number { get; set; }
            public string title { get; set; }
            public User user { get; set; }
            public Label[] labels { get; set; }
            public string state { get; set; }
            public bool locked { get; set; }
            public Assignee assignee { get; set; }
            public Assignee1[] assignees { get; set; }
            public object milestone { get; set; }
            public int comments { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object closed_at { get; set; }
            public string author_association { get; set; }
            public string body { get; set; }
            public _Comments[] _comments { get; set; }
            public bool hasPrTeamComment { get; set; }
        }

        public class User
        {
            public string login { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public string avatar_url { get; set; }
            public string gravatar_id { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string followers_url { get; set; }
            public string following_url { get; set; }
            public string gists_url { get; set; }
            public string starred_url { get; set; }
            public string subscriptions_url { get; set; }
            public string organizations_url { get; set; }
            public string repos_url { get; set; }
            public string events_url { get; set; }
            public string received_events_url { get; set; }
            public string type { get; set; }
            public bool site_admin { get; set; }
        }

        public class Label
        {
            public int id { get; set; }
            public string node_id { get; set; }
            public string url { get; set; }
            public string name { get; set; }
            public string color { get; set; }
            public bool _default { get; set; }
        }

        public class _Comments
        {
            public string url { get; set; }
            public string html_url { get; set; }
            public string issue_url { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public User1 user { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string author_association { get; set; }
            public string body { get; set; }
        }

        public class User1
        {
            public string login { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public string avatar_url { get; set; }
            public string gravatar_id { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string followers_url { get; set; }
            public string following_url { get; set; }
            public string gists_url { get; set; }
            public string starred_url { get; set; }
            public string subscriptions_url { get; set; }
            public string organizations_url { get; set; }
            public string repos_url { get; set; }
            public string events_url { get; set; }
            public string received_events_url { get; set; }
            public string type { get; set; }
            public bool site_admin { get; set; }
        }

        public class Assignee
        {
            public string login { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public string avatar_url { get; set; }
            public string gravatar_id { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string followers_url { get; set; }
            public string following_url { get; set; }
            public string gists_url { get; set; }
            public string starred_url { get; set; }
            public string subscriptions_url { get; set; }
            public string organizations_url { get; set; }
            public string repos_url { get; set; }
            public string events_url { get; set; }
            public string received_events_url { get; set; }
            public string type { get; set; }
            public bool site_admin { get; set; }
        }

        public class Assignee1
        {
            public string login { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public string avatar_url { get; set; }
            public string gravatar_id { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string followers_url { get; set; }
            public string following_url { get; set; }
            public string gists_url { get; set; }
            public string starred_url { get; set; }
            public string subscriptions_url { get; set; }
            public string organizations_url { get; set; }
            public string repos_url { get; set; }
            public string events_url { get; set; }
            public string received_events_url { get; set; }
            public string type { get; set; }
            public bool site_admin { get; set; }
        }
    }
}