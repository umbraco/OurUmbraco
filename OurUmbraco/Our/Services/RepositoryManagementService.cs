using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OurUmbraco.Community.GitHub;
using OurUmbraco.Our.Models.GitHub;
using OurUmbraco.Our.Models.GitHub.AutoReplies;
using Skybrud.Essentials.Json.Extensions;
using Umbraco.Core;
using Umbraco.Core.Logging;
using File = System.IO.File;

namespace OurUmbraco.Our.Services
{

    public class RepositoryManagementService
    {
        private static readonly string IssuesBaseDirectory = HostingEnvironment.MapPath("~/App_Data/TEMP/GitHub/");

        public List<Issue> GetAllCommunityIssues(bool pulls, DateTime? since = null)
        {
            var issues = new List<Issue>();

            var pullRequestService = new GitHubService();
            var hqMembers = pullRequestService.GetHqMembers();
            var teamMembers = pullRequestService.GetTeamMembers();

            foreach (var directory in Directory.EnumerateDirectories(IssuesBaseDirectory))
            {
                var directoryName = directory.Split('\\').Last();
                var repositoryName = directoryName.Substring(directoryName.LastIndexOf("__", StringComparison.Ordinal) + 2);
                var issuesDirectory = directory + "\\issues\\";

                if (pulls)
                    issuesDirectory = issuesDirectory + "\\pulls\\";

                if (Directory.Exists(issuesDirectory) == false)
                    continue;

                var issueFiles = Directory.EnumerateFiles(issuesDirectory, "*.combined.json");

                var reviewers = new List<string>();
                reviewers.AddRange(hqMembers);
                var team = teamMembers.FirstOrDefault(x => x.TeamName == repositoryName);
                if (team != null)
                    reviewers.AddRange(team.Members);

                foreach (var file in issueFiles)
                {

                    // Skip the file if older than the specified timestamp
                    if (since != null && File.GetLastWriteTimeUtc(file) < since.Value) continue;

                    var fileContent = File.ReadAllText(file);
                    var item = JsonConvert.DeserializeObject<Issue>(fileContent);

                    // Exclude issues created by HQ
                    if (hqMembers.Contains(item.User.Login.ToLowerInvariant()))
                        continue;

                    item.RepositoryName = repositoryName;

                    foreach (var comment in item.Comments)
                    {
                        var commenter = comment.User.Login.ToLowerInvariant();
                        if (item.FirstPrTeamOrHqComment == null && reviewers.Contains(commenter))
                            item.FirstPrTeamOrHqComment = comment.CreateDateTime.ToLocalTime();
                    }

                    issues.Add(item);
                }
            }

            return issues;
        }
        
        public List<Issue> GetAllIssues(bool pulls, DateTime? since = null)
        {
            var issues = new List<Issue>();

            foreach (var directory in Directory.EnumerateDirectories(IssuesBaseDirectory))
            {
                var directoryName = directory.Split('\\').Last();
                var repositoryName = directoryName.Substring(directoryName.LastIndexOf("__", StringComparison.Ordinal) + 2);
                var issuesDirectory = directory + "\\issues\\";

                if (pulls)
                    issuesDirectory = issuesDirectory + "\\pulls\\";

                if (Directory.Exists(issuesDirectory) == false)
                    continue;

                var issueFiles = Directory.EnumerateFiles(issuesDirectory, "*.combined.json");

                foreach (var file in issueFiles)
                {

                    // Skip the file if older than the specified timestamp
                    if (since != null && File.GetLastWriteTimeUtc(file) < since.Value) continue;

                    var fileContent = File.ReadAllText(file);
                    var item = JsonConvert.DeserializeObject<Issue>(fileContent);
                    item.RepositoryName = repositoryName;

                    issues.Add(item);
                }
            }

            return issues;
        }

        public List<GitHubCategorizedIssues> GetAllOpenIssues(bool pulls)
        {
            var allIssues = GetAllCommunityIssues(pulls);

            var openIssues = new List<GitHubCategorizedIssues>
            {
                new GitHubCategorizedIssues
                {
                    SortOrder = 1, CategoryDescription = "No reply", CategoryKey = CategoryKey.NoReply, Issues = new List<Issue>()
                },
                new GitHubCategorizedIssues
                {
                    SortOrder = 10, CategoryDescription = "HQ discussion", CategoryKey = CategoryKey.HqDiscussion, Issues = new List<Issue>()
                },
                new GitHubCategorizedIssues
                {
                    SortOrder = 20, CategoryDescription = "HQ reply", CategoryKey = CategoryKey.HqReply, Issues = new List<Issue>()
                },
                new GitHubCategorizedIssues
                {
                    SortOrder = 30, CategoryDescription = "Up For Grabs", CategoryKey = CategoryKey.UpForGrabs, Issues = new List<Issue>()
                },
                new GitHubCategorizedIssues
                {
                    SortOrder = 40, CategoryDescription = "Pull Request Pending", CategoryKey = CategoryKey.PullRequestPending, Issues = new List<Issue>()
                },
                 new GitHubCategorizedIssues
                {
                    SortOrder = 50, CategoryDescription = "Awaiting Feedback", CategoryKey = CategoryKey.Other, Issues = new List<Issue>()
                },
                new GitHubCategorizedIssues
                {
                    SortOrder = 10000, CategoryDescription = "Other", CategoryKey = CategoryKey.Other, Issues = new List<Issue>()
                }
               
            };


            var pullRequestService = new GitHubService();
            var hqMembers = pullRequestService.GetHqMembers();
            var teamMembers = pullRequestService.GetTeamMembers();
            var teamUmbraco = hqMembers;

            if (teamMembers != null)
            {
                foreach (var teamMember in teamMembers)
                    foreach (var member in teamMember.Members)
                        teamUmbraco.Add(member);
            }

            foreach (var item in allIssues.Where(x => x.Labels.Any(l => l.Name == "status/idea") == false))
            {
                if (item.State == "closed")
                    continue;

                if (item.Comments.Any())
                {
                    // Only Team Umbraco members can add labels, it's not exactly what we want to do but
                    // labeling counts as a "reply" even if there's no comments by Team Umbraco on the issue
                    if (item.Labels.Length == 0)
                    {
                        // If nobody on Team Umbraco labeled the issue NOR left a comment, it needs a reply
                        var teamUmbracoHasReplied = item.Comments.Any(c => teamUmbraco.InvariantContains(c.User.Login));
                        if (teamUmbracoHasReplied == false)
                        {
                            var noFirstReplyCategory = openIssues.First(x => x.CategoryKey == CategoryKey.NoReply);
                            noFirstReplyCategory.Issues.Add(item);
                            continue;
                        }
                    }

                    // The last comment was not from Team Umbraco, we might need to help move this issue along
                    var latestComment = item.Comments.OrderByDescending(x => x.CreateDateTime).FirstOrDefault();
                    if (latestComment != null && teamUmbraco.InvariantContains(latestComment.User.Login) == false)
                        item.NeedsTeamUmbracoReply = true;
                }
                else
                {
                    item.NeedsTeamUmbracoReply = true;
                }

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
                        var labels = new[] { "state/hq-discussion-ux", "state/hq-discussion-cms", "state/hq-discussion-cloud" };
                        if (labels.Contains(label.Name) == false)
                            continue;

                        matchedLabel = true;

                        AddCategoryCreatedDate(item, label, labels);
                        var hqDiscussionCategory = openIssues.First(x => x.CategoryKey == CategoryKey.HqDiscussion);
                        hqDiscussionCategory.Issues.Add(item);
                    }

                    // only go to the next item in the
                    // foreach if we found a match here
                    if (matchedLabel)
                        continue;

                    foreach (var label in item.Labels)
                    {
                        var labels = new[] { "state/hq-reply" };
                        if (labels.Contains(label.Name) == false)
                            continue;

                        matchedLabel = true;

                        AddCategoryCreatedDate(item, label, labels);
                        var hqReplyCategory = openIssues.First(x => x.CategoryKey == CategoryKey.HqReply);
                        hqReplyCategory.Issues.Add(item);
                    }

                    // only go to the next item in the
                    // foreach if we found a match here
                    if (matchedLabel)
                        continue;

                    foreach (var label in item.Labels)
                    {
                        var labels = new[] { "community/pr" };
                        if (labels.Contains(label.Name) == false)
                            continue;

                        matchedLabel = true;

                        AddCategoryCreatedDate(item, label, labels);
                        var prPendingCategory = openIssues.First(x => x.CategoryKey == CategoryKey.PullRequestPending);
                        prPendingCategory.Issues.Add(item);
                    }

                    // only go to the next item in the
                    // foreach if we found a match here
                    if (matchedLabel)
                        continue;

                    foreach (var label in item.Labels)
                    {
                        var labels = new[] { "up-for-grabs", "community/up-for-grabs", "help wanted" };
                        if (labels.Contains(label.Name) == false)
                            continue;

                        matchedLabel = true;
                        var upForGrabsCategory = openIssues.First(x => x.CategoryKey == CategoryKey.UpForGrabs);
                        // We're matching on several labels, so only add the item once in case there's multiple matches
                        if (upForGrabsCategory.Issues.Contains(item) == false)
                        {
                            AddCategoryCreatedDate(item, label, labels);

                            upForGrabsCategory.Issues.Add(item);
                        }
                    }

                    // only go to the next item in the
                    // foreach if we found a match here
                    if (matchedLabel)
                        continue;
                }


                var otherCategory = openIssues.First(x => x.CategoryKey == CategoryKey.Other);
                otherCategory.Issues.Add(item);
            }

            return openIssues.OrderBy(x => x.SortOrder).ToList();
        }

        public List<GitHubCategorizedIssues> GetPendingItemsForUser(string githubUsername)
        {
            var myIssues = new List<GitHubCategorizedIssues>
            {
                new GitHubCategorizedIssues
                {
                    SortOrder = 0,
                    CategoryDescription = "Reply pending",
                    CategoryKey = CategoryKey.ReplyPending,
                    Issues = new List<Issue>()
                }
            };

            var allIssues = GetAllCommunityIssues(pulls: false);
            foreach (var issue in allIssues.Where(x => x.State != "closed"))
            {
                // only issues the requested user has replied to
                if (issue.Comments.Any(x => string.Equals(x.User.Login, githubUsername, StringComparison.InvariantCultureIgnoreCase)) == false)
                    continue;


                if (string.Equals(issue.Comments.Last().User.Login, githubUsername, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                // if the requested user doesn't have the last reply, add it to the list
                myIssues.First().Issues.Add(issue);
            }

            var allPrs = GetAllCommunityIssues(pulls: true);
            foreach (var issue in allPrs.Where(x => x.State != "closed"))
            {
                // only issues the requested user has replied to
                if (issue.Comments.Any(x => string.Equals(x.User.Login, githubUsername, StringComparison.InvariantCultureIgnoreCase)) == false)
                    continue;


                if (string.Equals(issue.Comments.Last().User.Login, githubUsername, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                // if the requested user doesn't have the last reply, add it to the list
                myIssues.First().Issues.Add(issue);
            }

            return myIssues;
        }

        public List<OurUmbraco.Community.Models.Repository> GetAllPublicRepositories()
        {
            var configFile = HostingEnvironment.MapPath("~/Config/GitHubPublicRepositories.json");
            var fileContent = File.ReadAllText(configFile);
            var repositories = JsonConvert.DeserializeObject<List<OurUmbraco.Community.Models.Repository>>(fileContent);
            return repositories;
        }

        private static void AddCategoryCreatedDate(Issue item, Models.GitHub.Label label, string[] labelNames)
        {
            if (item.Events != null)
            {
                // Get the newest event where an "up for grabs" (etc) label was added
                var labeledUpForGrabs = item.Events.OrderByDescending(x => x.CreateDateTime).ToList()
                    .FirstOrDefault(x => x.Name == "labeled" && labelNames.Contains(x.Label.Name));

                if (labeledUpForGrabs != null)
                    item.InThisCategorySince = labeledUpForGrabs.CreateDateTime;
            }
        }
        
        public enum CategoryKey
        {
            NoReply,
            UpForGrabs,
            HqDiscussion,
            HqReply,
            PullRequestPending,
            ReplyPending,
            AwaitingFeedback,
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