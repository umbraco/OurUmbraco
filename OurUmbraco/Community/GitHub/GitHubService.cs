using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Configuration;
using System.Web.Hosting;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using Hangfire.Console;
using Hangfire.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OurUmbraco.Community.GitHub.Models;
using OurUmbraco.Community.GitHub.Models.Cached;
using OurUmbraco.Community.Models;
using OurUmbraco.Our.Models.GitHub;
using RestSharp;
using Skybrud.Essentials.Json;
using Skybrud.Social.GitHub.Exceptions;
using Skybrud.Social.GitHub.Options;
using Skybrud.Social.GitHub.Options.Issues;
using Skybrud.Social.GitHub.Responses.Issues;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using File = System.IO.File;
using GitHubIssueState = Skybrud.Social.GitHub.Options.Issues.GitHubIssueState;

namespace OurUmbraco.Community.GitHub
{
    public class GitHubService
    {
        private readonly Random random = new Random();

        private const string RepositoryOwner = "Umbraco";
        private const string GitHubApiClient = "https://api.github.com";
        private const string UserAgent = "OurUmbraco";

        public readonly string JsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/GithubContributors.json");
        public readonly string PullRequestsJsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/GithubPullRequests.json");

        private static readonly object Lock = new object();
        public static bool IsLocked { get; set; }

        private readonly string _teamUmbracoUsersFile = HostingEnvironment.MapPath("~/Config/TeamUmbraco.json");

        private static int _gitHubUserIdPropertyTypeId;

        /// <summary>
        /// Gets the numeric IDs of all members with the specified <paramref name="githubId"/>.
        ///
        /// Ideally there should only ever be zero or one matches members.
        ///
        /// No matches indicate that the GitHub user is not linked to any Our members. More than one match indicates an
        /// error, as there should not be more than one Our member linked to the same GitHub user.
        /// </summary>
        /// <param name="githubId">The ID of a GitHub user.</param>
        /// <returns>Array if member IDs.</returns>
        public int[] GetMemberIdsFromGitHubUserId(int githubId)
        {

            var db = ApplicationContext.Current.DatabaseContext.Database;

            const string propertyTypeAlias = "githubId";

            // In order to lookup the GitHub user ID in the database, we first need the ID of the
            // property type holding the value. To minimize calls to the database, the ID is stored
            // in a static field once we have found it, ensuring we only have to look it up once
            // during the application lifetime
            if (_gitHubUserIdPropertyTypeId == 0)
            {

                // Declare a nice and raw SQL query
                Sql sql1 = new Sql("SELECT [id] FROM [dbo].[cmsPropertyType] WHERE [Alias] = @0;", propertyTypeAlias);

                // Fire it up in the database
                _gitHubUserIdPropertyTypeId = db.FirstOrDefault<int>(sql1);

                // The result will be "0" if a matching row isn't found (which should then trigger an exception)
                if (_gitHubUserIdPropertyTypeId == 0) throw new Exception("Failed retrieving ID of property type with alias " + propertyTypeAlias);

            }

            // Declare another nice and raw SQL query
            Sql sql2 = new Sql("SELECT [contentNodeId] FROM [dbo].[cmsPropertyData] WHERE [propertytypeid] = @0 AND [dataNvarchar] = @1", _gitHubUserIdPropertyTypeId, githubId);

            // Get the IDs of matching members
            return db.Fetch<int>(sql2).ToArray();

        }

        public TeamUmbraco GetTeam(string repository)
        {
            var teamUmbraco = new TeamUmbraco();
            
            var usersService = new UsersService();
            var usernames = usersService.GetIgnoredGitHubUsers().Result.ToArray();
            
            var content = File.ReadAllText(_teamUmbracoUsersFile);
            var teamUmbracoUsers = JsonConvert.DeserializeObject<List<TeamUmbraco>>(content);
            var team = teamUmbracoUsers.FirstOrDefault(x => x.TeamName == repository);
            if (team != null)
            {
                team.Members.AddRange(usernames);
                teamUmbraco = team;
            }
            else
            {
                teamUmbraco = new TeamUmbraco
                {
                    TeamName = repository,
                    Members = usernames.ToList()
                };
            }

            return teamUmbraco;
        }

        public List<TeamUmbraco> GetTeamMembers()
        {
            var content = File.ReadAllText(_teamUmbracoUsersFile);
            var teamUmbracoUsers = JsonConvert.DeserializeObject<List<TeamUmbraco>>(content);
            return teamUmbracoUsers;
        }

        /// <summary>
        /// Gets a list of the repositories that should be included in the list of contributors.
        /// </summary>
        /// <returns></returns>
        public string[] GetRepositories()
        {
            return new[] {
                "Umbraco-CMS",
                "UmbracoDocs",
                "OurUmbraco",
                "Umbraco.Courier.Contrib",
                "Umbraco.Deploy.Contrib",
                "Umbraco.Deploy.ValueConnectors",
                "UmbPack",
                "Package.Templates",
                "rfcs",	
                "organizer-guide",
                "The-Starter-Kit",
                "Umbraco.CMS.Backoffice",
                "Umbraco.UI"
            };
        }

        public void UpdateAllPullRequestsForRepository()
        {
            var enabledSetting = ConfigurationManager.AppSettings["GithubPullsImportEnabled"];
            var enabled = string.Equals(enabledSetting, "true", StringComparison.InvariantCultureIgnoreCase);
            if (enabled == false)
            {
                LogHelper.Info<GitHubService>("GithubPullsImportEnabled is disabled in web.config, returning without attempting imports");
                IsLocked = false;
                return;
            }

            if (IsLocked)
                return;

            lock (Lock)
            {
                IsLocked = true;

                var pulls = new List<GithubPullRequestModel>();

                foreach (var repository in GetRepositories())
                {
                    var existingPulls = GetExistingPullsFromDisk(repository);
                    foreach (var pull in existingPulls)
                    {
                        DateTime? mergeDateTime = null;
                        var mergedEvent = pull.Events.FirstOrDefault(x => x.Name == "merged");
                        if (mergedEvent != null)
                        {
                            mergeDateTime = mergedEvent.CreateDateTime;
                        }

                        var pullRequest = new GithubPullRequestModel
                        {
                            Id = pull.Id,
                            ClosedAt = pull.ClosedDateTime,
                            CreatedAt = pull.CreateDateTime,
                            UpdatedAt = pull.UpdateDateTime,
                            MergedAt = mergeDateTime,
                            Number = pull.Number,
                            Repository = pull.RepoSlug,
                            State = pull.State,
                            User = new GithubPullRequestUser
                            {
                                Id = pull.User.Id,
                                Login = pull.User.Login
                            },
                            Title = pull.Title
                        };
                        pulls.Add(pullRequest);
                    }
                }

                // Save the JSON to disk
                var rawJson = JsonConvert.SerializeObject(pulls, Formatting.Indented);
                File.WriteAllText(PullRequestsJsonPath, rawJson, Encoding.UTF8);


                IsLocked = false;
            }
            
            ExamineManager.Instance.IndexProviderCollection["PullRequestIndexer"].RebuildIndex();
        }

        internal List<Issue> GetExistingPullsFromDisk(string alias)
        {
            var repository = new Community.Models.Repository(alias, "umbraco", alias, alias);
            var dir = HostingEnvironment.MapPath(repository.PullsStorageDirectory());
            var pulls = new List<Issue>();
            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir, "*.pull.combined.json");

                foreach (var file in files)
                {
                    try
                    {
                        var content = File.ReadAllText(file);
                        var pull = JsonConvert.DeserializeObject<Issue>(content);
                        pulls.Add(pull);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);

                    }
                }
            }
            return pulls;
        }

        /// <summary>
        /// Gets a list of contributors (<see cref="GitHubContributorModel"/>) for a single GitHub repository.
        /// </summary>
        /// <param name="repo">The alias (slug) of the repository.</param>
        /// <returns>A list of <see cref="GitHubContributorModel"/>.</returns>
        public IRestResponse<List<GitHubContributorModel>> GetRepositoryContributors(string repo)
        {
            // Initialize the request
            var client = new RestClient(GitHubApiClient);
            var request = new RestRequest($"/repos/{RepositoryOwner}/{repo}/stats/contributors", Method.GET);
            client.UserAgent = UserAgent;

            // Make the request to the GitHub API
            return client.Execute<List<GitHubContributorModel>>(request);
        }

        /// <summary>
        /// Attempts to load the JSON file at <see cref="JsonPath"/>. If successful, the global list of contributors
        /// will be returned.
        /// </summary>
        /// <returns>A list of <see cref="GitHubCachedGlobalContributorModel"/>.</returns>
        public List<GitHubCachedGlobalContributorModel> GetOverallContributorsFromDisk()
        {
            return JsonUtils.LoadJsonArray(JsonPath).Select(item => item.ToObject<GitHubCachedGlobalContributorModel>()).ToList();
        }

        /// <summary>
        /// Appends the specified <paramref name="str"/> to <paramref name="sb"/>. Mostly used for debugging purposes.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> representing the log.</param>
        /// <param name="str">The string to be added to <paramref name="sb"/>.</param>
        private void Log(StringBuilder sb, string str)
        {
            sb.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {str}");
        }

        private Skybrud.Social.GitHub.GitHubService _github;
        private Skybrud.Social.GitHub.GitHubService _githubBot;

        public Skybrud.Social.GitHub.GitHubService GitHubApi => _github
                ?? (_github = Skybrud.Social.GitHub.GitHubService.CreateFromAccessToken(WebConfigurationManager.AppSettings["GitHubAccessToken"]));

        public void UpdateIssues(PerformContext context, Community.Models.Repository repository)
        {
            // Accept newer versions of the TLS protocol
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                EnsureCacheDirectories(repository);

                var options = new GitHubGetRepositoryIssuesOptions
                {
                    Owner = repository.Owner,
                    Repository = repository.Alias,
                    Sort = GitHubIssueSortField.Updated,
                    Direction = GitHubSortDirection.Descending,
                    State = GitHubIssueState.All,
                    PerPage = 100,
                    Page = 1
                };

                while (true)
                {
                    var localCount = 0;

                    // Make the initial request to the API
                    context.WriteLine($"Fetching issues for repo {repository.Alias} (page {options.Page})");
                    GitHubGetIssuesResponse issuesResponse;
                    try
                    {
                        issuesResponse = GitHubApi.Issues.GetIssues(options);
                        context.WriteLine($"GitHub says: {issuesResponse.RateLimiting.Remaining} requests remaining - will reset at {issuesResponse.RateLimiting.Reset.ToString("yyyy-MM-dd HH:mm")} UTC");
                    }
                    catch (GitHubHttpException ex)
                    {
                        throw new Exception(
                            $"Failed fetching page {options.Page}\r\n\r\n{ex.Response.ResponseUri}", ex);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed fetching page {options.Page}", ex);
                    }


                    foreach (var response in issuesResponse.Body)
                    {
                        // Local flag used later to determine whether new data was fetched for this issue
                        var updated = false;

                        var gitHubResponse = JsonConvert.DeserializeObject<GitHubResponse>(response.JObject.ToString());
                        var responseIsIssue = gitHubResponse.pull_request == null;

                        var directory = responseIsIssue ? repository.IssuesStorageDirectory() : repository.PullsStorageDirectory();
                        var issueTypename = responseIsIssue ? "issue" : "pull";
                        var filePrefix = $"{directory}\\{response.Number}.{issueTypename}";

                        var issuesFile = HostingEnvironment.MapPath($"{filePrefix}.json");
                        var issuesCommentFile = HostingEnvironment.MapPath($"{filePrefix}.comments.json");
                        var issuesEventsFile = HostingEnvironment.MapPath($"{filePrefix}.events.json");
                        var issuesReviewsFile = HostingEnvironment.MapPath($"{filePrefix}.reviews.json");
                        var issuesCombinedFile = HostingEnvironment.MapPath($"{filePrefix}.combined.json");

                        JsonUtils.SaveJsonObject(issuesFile, response);

                        // Fetch comments and events if the local JSON file is older than the last update time of the issue
                        JArray comments;
                        JArray events;
                        JArray reviews = null;

                        if (File.Exists(issuesCommentFile) && File.GetLastWriteTimeUtc(issuesCommentFile) > response.UpdatedAt.ToUniversalTime())
                        {
                            comments = JsonUtils.LoadJsonArray(issuesCommentFile);
                        }
                        else
                        {
                            context.WriteLine($"Fetching comments for issue {response.Number} {response.Title}");
                            var issueCommentsResponse = GitHubApi.Client.DoHttpGetRequest($"/repos/{repository.Owner}/{repository.Alias}/issues/{response.Number}/comments");
                            if (issueCommentsResponse.StatusCode != HttpStatusCode.OK)
                                throw new Exception($"Failed fetching comments for issue #{response.Number} ({issueCommentsResponse.StatusCode})");

                            comments = JsonUtils.ParseJsonArray(issueCommentsResponse.Body);
                            JsonUtils.SaveJsonArray(issuesCommentFile, comments);

                            updated = true;
                        }

                        if (File.Exists(issuesEventsFile) && File.GetLastWriteTimeUtc(issuesEventsFile) > response.UpdatedAt.ToUniversalTime())
                        {
                            events = JsonUtils.LoadJsonArray(issuesEventsFile);
                        }
                        else
                        {
                            context.WriteLine($"Fetching events for issue {response.Number} {response.Title}");
                            var issueEventsResponse = GitHubApi.Client.DoHttpGetRequest($"/repos/{repository.Owner}/{repository.Alias}/issues/{response.Number}/events");
                            if (issueEventsResponse.StatusCode != HttpStatusCode.OK)
                                throw new Exception(
                                    $"Failed fetching events for issue #{response.Number} ({issueEventsResponse.StatusCode})");

                            events = JsonUtils.ParseJsonArray(issueEventsResponse.Body);
                            JsonUtils.SaveJsonArray(issuesEventsFile, events);
                        }

                        // Only PRs have reviews, no need to check for them on issues
                        if (responseIsIssue == false)
                        {
                            if (File.Exists(issuesReviewsFile) && File.GetLastWriteTimeUtc(issuesReviewsFile) > response.UpdatedAt.ToUniversalTime())
                            {
                                reviews = JsonUtils.LoadJsonArray(issuesReviewsFile);
                            }
                            else
                            {
                                context.WriteLine($"Fetching reviews for PR {response.Number} {response.Title}");
                                var issueReviewsResponse = GitHubApi.Client.DoHttpGetRequest($"/repos/{repository.Owner}/{repository.Alias}/pulls/{response.Number}/reviews");
                                if (issueReviewsResponse.StatusCode == HttpStatusCode.NotFound)
                                {
                                    // make sure to save an empty array for all of them so we don't keep revisiting this unless there's been a PR update
                                    reviews = JsonUtils.ParseJsonArray("[]");
                                    JsonUtils.SaveJsonArray(issuesReviewsFile, reviews);
                                    continue;
                                }

                                if (issueReviewsResponse.StatusCode != HttpStatusCode.OK)
                                    throw new Exception($"Failed fetching reviews for PR #{response.Number} ({issueReviewsResponse.StatusCode})");

                                reviews = JsonUtils.ParseJsonArray(issueReviewsResponse.Body);
                                JsonUtils.SaveJsonArray(issuesReviewsFile, reviews);
                            }
                        }

                        // Save a JSON file with all the combined data we have for the issue / PR
                        response.JObject.Add("_comments", comments);
                        response.JObject.Add("events", events);
                        response.JObject.Add("reviews", reviews);
                        JsonUtils.SaveJsonObject(issuesCombinedFile, response);

                        if (updated) localCount++;
                    }

                    context.WriteLine($"Updated {localCount} issues on page {options.Page}");

                    // Break the loop if not all issues on the page were updated
                    if (localCount < options.PerPage) break;

                    // Increment the page count
                    options.Page++;
                }
            }
            catch (Exception ex)
            {
                context.WriteLine("Error while fetching issues", ex);
                context.WriteLine("Error:" + ex.Message + " - Stack trace: " + ex.StackTrace);
            }
        }

        private static void EnsureCacheDirectories(Community.Models.Repository repository)
        {
            var issuesDirectory = HostingEnvironment.MapPath(repository.IssuesStorageDirectory());
            if (Directory.Exists(issuesDirectory) == false)
                Directory.CreateDirectory(issuesDirectory);
            var pullsDirectory = HostingEnvironment.MapPath(repository.PullsStorageDirectory());
            if (Directory.Exists(pullsDirectory) == false)
                Directory.CreateDirectory(pullsDirectory);
        }
    }

    public class GitHubResponse
    {
        public Pull_Request pull_request { get; set; }
    }

    public class Pull_Request
    {
        public string url { get; set; }
    }

    public class Label
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("node_id")]
        public string NodeId { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("color")]
        public string Color { get; set; }
        [JsonProperty("_default")]
        public bool Default { get; set; }
    }

    // From: https://stackoverflow.com/a/11473510/5018
    public static class StaticRandom
    {
        private static int seed;

        private static ThreadLocal<Random> threadLocal = new ThreadLocal<Random>
            (() => new Random(Interlocked.Increment(ref seed)));

        static StaticRandom()
        {
            seed = Environment.TickCount;
        }

        public static Random Instance { get { return threadLocal.Value; } }
    }
}