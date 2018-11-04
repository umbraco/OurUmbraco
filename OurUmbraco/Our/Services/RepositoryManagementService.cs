using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;
using OurUmbraco.Our.Models.GitHub;

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
                    SortOrder = 1, CategoryDescription = "No reply", CategoryKey = CategoryKey.NoReply, Issues = new List<Issue>()
                },
                new GitHubCategorizedIssues
                {
                    SortOrder = 10, CategoryDescription = "Up For Grabs", CategoryKey = CategoryKey.UpForGrabs, Issues = new List<Issue>()
                },
                new GitHubCategorizedIssues
                {
                    SortOrder = 20, CategoryDescription = "Pull Request Pending", CategoryKey = CategoryKey.PullRequestPending, Issues = new List<Issue>()
                },
                new GitHubCategorizedIssues
                {
                    SortOrder = 10000, CategoryDescription = "Other", CategoryKey = CategoryKey.Other, Issues = new List<Issue>()
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

                var reviewers = hqMembers;
                var team = teamMembers.FirstOrDefault(x => x.TeamName == repositoryName);
                if (team != null)
                    reviewers.AddRange(team.Members);

                foreach (var file in issueFiles)
                {
                    var fileContent = File.ReadAllText(file);
                    var item = JsonConvert.DeserializeObject<Issue>(fileContent);
                    if (item.State == "closed")
                        continue;
                    if (hqMembers.Contains(item.User.Login.ToLowerInvariant()))
                        continue;

                    foreach (var comment in item.Comments)
                    {
                        var commenter = comment.User.Login.ToLowerInvariant();
                        if (hqMembers.Contains(commenter))
                            item.HasPrTeamComment = true;
                    }

                    item.RepositoryName = repositoryName;

                    if (item.Labels.Length == 0 && item.CommentCount == 0)
                    {
                        var noFirstReplyCategory = openIssues.First(x => x.CategoryKey == CategoryKey.NoReply);
                        noFirstReplyCategory.Issues.Add(item);
                        continue;
                    }

                    if (item.Labels.Length != 0)
                    {
                        var matchedLabel = false;
                        foreach (var label in item.Labels)
                        {
                            if (label.Name != "community/pr")
                                continue;

                            matchedLabel = true;
                            var prPendingCategory = openIssues.First(x => x.CategoryKey == CategoryKey.PullRequestPending);
                            prPendingCategory.Issues.Add(item);
                        }

                        // only go to the next item in the
                        // foreach if we found a match here
                        if (matchedLabel)
                            continue;

                        foreach (var label in item.Labels)
                        {
                            if (label.Name != "community/up-for-grabs" && label.Name != "up-for-grabs" && label.Name != "help wanted")
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
            public List<Issue> Issues { get; set; }
        }
    }
}