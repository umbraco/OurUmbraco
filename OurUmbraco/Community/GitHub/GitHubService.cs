using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Hosting;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using Newtonsoft.Json;
using OurUmbraco.Community.GitHub.Models;
using OurUmbraco.Community.GitHub.Models.Cached;
using OurUmbraco.Our.Api;
using RestSharp;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Time;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Web;

namespace OurUmbraco.Community.GitHub
{
    public class GitHubService
    {
        private const string RepositoryOwner = "Umbraco";
        private const string GitHubApiClient = "https://api.github.com";
        private const string UserAgent = "OurUmbraco";

        public readonly string JsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/GithubContributors.json");
        public readonly string PullRequestsJsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/GithubPullRequests.json");

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
                "Umbraco.Deploy.Contrib",
                "Umbraco.Courier.Contrib",
                "Umbraco.Deploy.ValueConnectors"
            };
        }

        public List<GithubPullRequestModel> UpdateAllPullRequestsForRepository(string repository, bool sortByUpdated)
        {
            var pulls = new List<GithubPullRequestModel>();
            if (File.Exists(PullRequestsJsonPath))
            {
                var content = File.ReadAllText(PullRequestsJsonPath);
                pulls = JsonConvert.DeserializeObject<List<GithubPullRequestModel>>(content);
            }

            var stopImport = false;
            for (var i = 0; i < int.MaxValue; i++)
            {
                if (stopImport)
                    break;

                pulls = GetPulls(repository: repository, page: i, pulls: pulls, sortByUpdated: sortByUpdated, stopImport: out stopImport);
                Thread.Sleep(2000);
            }

            // Save the JSON to disk
            var rawJson = JsonConvert.SerializeObject(pulls, Formatting.Indented);
            File.WriteAllText(PullRequestsJsonPath, rawJson, Encoding.UTF8);

            return pulls;
        }

        private List<GithubPullRequestModel> GetPulls(string repository, int page, List<GithubPullRequestModel> pulls, bool sortByUpdated, out bool stopImport)
        {
            var enabledSetting = ConfigurationManager.AppSettings["GithubPullsImportEnabled"];
            var enabled = string.Equals(enabledSetting, "true", StringComparison.InvariantCultureIgnoreCase);
            if (enabled == false)
            {
                stopImport = true;
                return pulls;
            }

            // Initialize the request
            var client = new RestClient(GitHubApiClient);

            var resource = string.Format("/repos/{0}/{1}/pulls?state=all&page={2}", RepositoryOwner, repository, page);
            if (sortByUpdated)
                resource = string.Format("{0}&sort=updated&direction=desc", resource);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request = new RestRequest(resource, Method.GET);
            client.UserAgent = UserAgent;

            // Make the request to the GitHub API
            var result = client.Execute<List<GithubPullRequestModel>>(request);

            stopImport = false;

            if (result.Data == null)
            {
                stopImport = true;
                return null;
            }

            foreach (var pull in result.Data)
            {
                var existing = pulls.FirstOrDefault(x => x.Id == pull.Id);
                if (existing != null && existing.UpdatedAt != pull.UpdatedAt)
                {
                    existing.ClosedAt = pull.ClosedAt;
                    existing.MergedAt = pull.MergedAt;
                    existing.State = pull.State;
                    existing.Title = pull.Title;
                    existing.UpdatedAt = pull.UpdatedAt;
                }
                else if (existing != null && existing.UpdatedAt == pull.UpdatedAt)
                {
                    // we've already imported these, stop going down the list
                    stopImport = true;
                    break;
                }
                else
                {
                    pull.Repository = repository;
                    pulls.Add(pull);
                }
            }

            return pulls;
        }

        public List<string> MatchPullsToMembers()
        {
            var matches = UmbracoContext.Current.Application.ApplicationCache.RuntimeCache.GetCacheItem<List<string>>("OurPullsMatchToMembers",
                () =>
                {
                    var pulls = new List<GithubPullRequestModel>();

                    if (File.Exists(PullRequestsJsonPath))
                    {
                        var content = File.ReadAllText(PullRequestsJsonPath);
                        pulls = JsonConvert.DeserializeObject<List<GithubPullRequestModel>>(content);
                    }

                    var results = new List<string>();

                    var searcher = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];
                    var criteria = (LuceneSearchCriteria)searcher.CreateSearchCriteria();

                    // TODO: Linq is inefficient. Find Lucene query to give all results which are not empty for the `github` field
                    criteria = (LuceneSearchCriteria)criteria.RawQuery("*:*");
                    var searchResults = searcher.Search(criteria);
                    var contributors = searchResults.Where(x => x.Fields["github"] != null && string.IsNullOrWhiteSpace(x.Fields["github"]) == false);

                    foreach (var contributor in contributors)
                    {
                        var ourName = contributor.Fields["nodeName"];
                        var ourId = contributor.Fields["id"];
                        var githubUsername = contributor.Fields["github"];
                        var totalPulls = pulls.Where(x => x.User.Login == githubUsername).ToList();
                        var acceptedPulls = totalPulls.Where(x => x.MergedAt != null).ToList();
                        var closedPulls = totalPulls.Where(x => x.MergedAt == null && x.ClosedAt != null).ToList();
                        results.Add(string.Format("<a href=\"/member/{0}\">{1}</a> (<a href=\"https://github.com/{2}\">{2}</a> on GitHub) has sent {3} PRs of which {4} have been accepted and {5} have been closed without merging.",
                            ourId, ourName, githubUsername, totalPulls.Count, acceptedPulls.Count, closedPulls.Count));
                    }

                    return results;

                }, TimeSpan.FromMinutes(15));

            return matches;
        }

        /// <summary>
        /// Gets a list of contributors (<see cref="GitHubContributorModel"/>) for a single GitHub repository.
        /// </summary>
        /// <param name="repo">The alias (slug) of the repository.</param>
        /// <returns>A list of <see cref="GitHubContributorModel"/>.</returns>
        public IRestResponse<List<GitHubContributorModel>> GetRepositoryContributors(string repo)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Initialize the request
            var client = new RestClient(GitHubApiClient);
            var request = new RestRequest(string.Format("/repos/{0}/{1}/stats/contributors", RepositoryOwner, repo), Method.GET);
            client.UserAgent = UserAgent;

            // Make the request to the GitHub API
            return client.Execute<List<GitHubContributorModel>>(request);
        }

        /// <summary>
        /// Get all contributors from GitHub Umbraco repositories
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        public IRestResponse<List<GitHubContributorModel>> GetAllRepoContributors(string repo)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var client = new RestClient(GitHubApiClient);
            var request = new RestRequest(string.Format("/repos/{0}/{1}/stats/contributors", RepositoryOwner, repo), Method.GET);
            client.UserAgent = UserAgent;
            var response = client.Execute<List<GitHubContributorModel>>(request);
            return response;
        }

        public GitHubContributorsResult GetOverallContributors(int maxAttempts = 3)
        {
            // Initialize a new StringBuilder for logging/testing purposes
            var stringBuilder = new StringBuilder();

            // Map the path to the file containg HQ members that should be excluded in the list
            var configPath = HostingEnvironment.MapPath("~/config/githubhq.txt");
            if (!File.Exists(configPath))
            {
                var message = string.Format("Config file was not found: {0}", configPath);
                LogHelper.Debug<GitHubService>(message);
                throw new Exception(message);
            }

            // Parse the logins (usernames)
            var login = System.IO.File.ReadAllLines(configPath).Where(x => x.Trim() != "").Distinct().ToArray();

            // A dictionary for the response of each repository
            var responses = new Dictionary<string, IRestResponse<List<GitHubContributorModel>>>();

            // Hashset for keeping track of missing responses
            var missing = new HashSet<string>();

            Log(stringBuilder, "Attempt 1");

            // Iterate over the repositories
            foreach (var repo in GetRepositories())
            {
                Log(stringBuilder, string.Format("-> Making request to {0}{1}", GitHubApiClient, string.Format("/repos/{0}/{1}/stats/contributors", RepositoryOwner, repo)));

                // Make the request to the GitHub API
                var response = GetRepositoryContributors(repo);

                Log(stringBuilder, string.Format("  -> {0} -> {1}", (int)response.StatusCode, response.StatusCode));

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        responses[repo] = response;
                        break;
                    case HttpStatusCode.Accepted:
                        missing.Add(repo);
                        break;
                    default:
                        var message = string.Format("Failed getting contributors for repository {0}: {1}\r\n\r\n{2}", repo, response.StatusCode, response.Content);
                        Log(stringBuilder, message);
                        throw new Exception(message);
                }
            }

            for (var i = 2; i <= maxAttempts; i++)
            {
                // Break the loop if there are no missing repositories
                if (missing.Count == 0)
                    break;

                // Wait for a few seconds so the GitHub cache hopefully has been populated
                Thread.Sleep(5000);

                Log(stringBuilder, string.Format("Attempt {0}", i));

                foreach (var repo in GetRepositories())
                {
                    // Make the request to the GitHub API
                    var response = GetRepositoryContributors(repo);

                    // Error checking
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            // Set the response in the dictionary
                            responses[repo] = response;
                            // Remove the repository from queue
                            missing.Remove(repo);
                            break;
                        case HttpStatusCode.Accepted:
                            break;
                        default:
                            var message = string.Format("Failed getting contributors for repository {0}: {1}\r\n\r\n{2}", repo, response.StatusCode, response.Content);
                            Log(stringBuilder, message);
                            throw new Exception(message);
                    }

                }
            }

            if (missing.Count > 0)
            {
                var message = string.Format("Unable to get contributors for one or more repositories:\r\n{0}", string.Join("\r\n", missing));
                Log(stringBuilder, message);
                throw new Exception(message);
            }

            // filter to only include items from the last year (if we ran 4.6.1 we could have used ToUnixTimeSeconds())
            var filteredRange = TimeUtils.GetUnixTimeFromDateTime(DateTime.UtcNow.AddYears(-1));
            var contributors = new List<GitHubContributorModel>();

            // Iterate over the responses (we don't care about the keys at this point)
            foreach (var response in responses.Values)
            {
                // Iterate over the contributors of the individual response
                foreach (var contrib in response.Data)
                {
                    // Make sure we only get weeks from the past year
                    var contribWeeks = contrib.Weeks.Where(x => x.W >= filteredRange).ToList();

                    // Populate the properties
                    contrib.TotalAdditions = contribWeeks.Sum(x => x.A);
                    contrib.TotalDeletions = contribWeeks.Sum(x => x.D);
                    contrib.Total = contribWeeks.Sum(x => x.C);

                    // Append the contributor to the aggregated list
                    contributors.Add(contrib);
                }
            }

            // Group and sort the contributors from each repository
            var globalContributors = contributors
                .Where(g => g.Total > 0 && login.Contains(g.Author.Login) == false)
                .GroupBy(g => g.Author.Id)
                .Select(g => new GitHubGlobalContributorModel(g))
                .OrderByDescending(c => c.TotalCommits)
                .ThenByDescending(c => c.TotalAdditions)
                .ThenByDescending(c => c.TotalDeletions)
                .ToList();

            return new GitHubContributorsResult(globalContributors, string.Format("{0}{1}", stringBuilder, string.Empty));
        }

        public GitHubContributorsResult UpdateOverallContributors(int maxAttempts = 3)
        {
            // Load the contributors via the GitHub API
            var result = GetOverallContributors(maxAttempts);

            // Map the contributors to the cached model
            var contributors = result.Contributors.Select(x => new GitHubCachedGlobalContributorModel(x));

            // Serialize the contributors to raw JSON
            var rawJson = JsonConvert.SerializeObject(contributors, Formatting.Indented);

            // Save the JSON to disk
            System.IO.File.WriteAllText(JsonPath, rawJson, Encoding.UTF8);

            return result;
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
            sb.AppendLine(string.Format("{0:yyyy-MM-dd HH:mm:ss} {1}", DateTime.Now, str));
        }
    }
}