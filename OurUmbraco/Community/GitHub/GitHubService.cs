using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OurUmbraco.Community.GitHub.Models;
using OurUmbraco.Community.GitHub.Models.Cached;
using RestSharp;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Time;
using Umbraco.Core.Logging;

namespace OurUmbraco.Community.GitHub
{
    public class GitHubService
    {
        
        private const string RepositoryOwner = "Umbraco";
        private const string GitHubApiClient = "https://api.github.com";
        private const string UserAgent = "OurUmbraco";

        public readonly string JsonPath =  HostingEnvironment.MapPath("~/App_Data/TEMP/GithubContributors.json");

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

        /// <summary>
        /// Gets a list of contributors (<see cref="GitHubContributorModel"/>) for a single GitHub repository.
        /// </summary>
        /// <param name="repo">The alias (slug) of the repository.</param>
        /// <returns>A list of <see cref="GitHubContributorModel"/>.</returns>
        public IRestResponse<List<GitHubContributorModel>> GetRepositoryContributors(string repo)
        {

            // Initialize the request
            RestClient client = new RestClient(GitHubApiClient);
            RestRequest request = new RestRequest(string.Format("/repos/{0}/{1}/stats/contributors", RepositoryOwner, repo), Method.GET);
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
            var client = new RestClient(GitHubApiClient);
            var request = new RestRequest(string.Format("/repos/{0}/{1}/stats/contributors", RepositoryOwner, repo), Method.GET);
            client.UserAgent = UserAgent;
            var response = client.Execute<List<GitHubContributorModel>>(request);
            return response;
        }

        public GitHubContributorsResult GetOverallContributors(int maxAttempts = 3)
        {

            // Initialize a new StringBuilder for logging/testing purposes
            StringBuilder sb = new StringBuilder();

            // Map the path to the file containg HQ members that should be excluded in the list
            string configPath = HostingEnvironment.MapPath("~/config/githubhq.txt");
            if (!System.IO.File.Exists(configPath))
            {
                LogHelper.Debug<GitHubService>("Config file was not found: " + configPath);
                throw new Exception("Config file was not found: " + configPath);
            }

            // Parse the logins (usernames)
            string[] login = System.IO.File.ReadAllLines(configPath).Where(x => x.Trim() != "").Distinct().ToArray();

            // A dictionary for the response of each repository
            Dictionary<string, IRestResponse<List<GitHubContributorModel>>> responses = new Dictionary<string, IRestResponse<List<GitHubContributorModel>>>();

            // Hashset for keeping track of missing responses
            HashSet<string> missing = new HashSet<string>();

            Log(sb, "Attempt 1");

            // Iterate over the repositories
            foreach (string repo in GetRepositories())
            {

                Log(sb, "-> Making request to " + GitHubApiClient + string.Format("/repos/{0}/{1}/stats/contributors", RepositoryOwner, repo));

                // Make the request to the GitHub API
                IRestResponse<List<GitHubContributorModel>> response = GetRepositoryContributors(repo);

                Log(sb, "  -> " + ((int) response.StatusCode) + " -> " + response.StatusCode);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        responses[repo] = response;
                        break;
                    case HttpStatusCode.Accepted:
                        missing.Add(repo);
                        break;
                    default:
                        Log(sb, "Failed getting contributors for repository " + repo + ": " + response.StatusCode + "\r\n\r\n" + response.Content);
                        throw new Exception("Failed getting contributors for repository " + repo + ": " + response.StatusCode);
                }

            }


            for (int i = 2; i <= maxAttempts; i++)
            {

                // Break the loop if there are no missing repositories
                if (missing.Count == 0)
                {
                    break;
                }

                // Wait for a few seconds so the GitHub cache hopefully has been populated
                Thread.Sleep(5000);

                Log(sb, "Attempt " + i);

                foreach (string repo in GetRepositories())
                {

                    // Make the request to the GitHub API
                    IRestResponse<List<GitHubContributorModel>> response = GetRepositoryContributors(repo);

                    // Error checking
                    switch (response.StatusCode) {

                        case HttpStatusCode.OK:
                            
                            // Set the response in the dictionary
                            responses[repo] = response;

                            // Remove the repository from queue
                            missing.Remove(repo);

                            break;

                        case HttpStatusCode.Accepted:
                            // zZzZz
                            break;

                        default:
                            Log(sb, "Failed getting contributors for repository " + repo + ": " + response.StatusCode + "\r\n\r\n" + response.Content);
                            throw new Exception("Failed getting contributors for repository " + repo + ": " + response.StatusCode + "\r\n\r\n" + response.Content);
                    
                    }

                }

            }

            // (╯°□°)╯︵ ┻━┻
            if (missing.Count > 0)
            {
                Log(sb, "Unable to get contributors for one or more repositories:\r\n" + String.Join("\r\n", missing));
                throw new Exception("Unable to get contributors for one or more repositories");
            }

            // filter to only include items from the last year (if we ran 4.6.1 we could have used ToUnixTimeSeconds())
            int filteredRange = TimeUtils.GetUnixTimeFromDateTime(DateTime.UtcNow.AddYears(-1));

            List<GitHubContributorModel> contributors = new List<GitHubContributorModel>();

            // Iterate over the responses (we don't care about the keys at this point)
            foreach (IRestResponse<List<GitHubContributorModel>> response in responses.Values)
            {

                // Iterate over the contributors of the individual response
                foreach (GitHubContributorModel contrib in response.Data)
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
            List<GitHubGlobalContributorModel> globalContributors = contributors
                .Where(g => g.Total > 0 && !login.Contains(g.Author.Login))
                .GroupBy(g => g.Author.Id)
                .Select(g => new GitHubGlobalContributorModel(g))
                .OrderByDescending(c => c.TotalCommits)
                .ThenByDescending(c => c.TotalAdditions)
                .ThenByDescending(c => c.TotalDeletions)
                .ToList();

            return new GitHubContributorsResult(globalContributors, sb + "");

        }

        public GitHubContributorsResult UpdateOverallContributors(int maxAttempts = 3)
        {

            // Load the contributors via the GitHub API
            GitHubContributorsResult result = GetOverallContributors(maxAttempts);

            // Map the contributors to the cached model
            IEnumerable<GitHubCachedGlobalContributorModel> contributors = result.Contributors.Select(x => new GitHubCachedGlobalContributorModel(x));

            // Serialize the contributors to raw JSON
            string rawJson = JsonConvert.SerializeObject(contributors, Formatting.Indented);

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
            return (
                from JObject item in JsonUtils.LoadJsonArray(JsonPath)
                select item.ToObject<GitHubCachedGlobalContributorModel>()
            ).ToList();
        }

        /// <summary>
        /// Appends the specified <paramref name="str"/> to <paramref name="sb"/>. Mostly used for debugging purposes.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> representing the log.</param>
        /// <param name="str">The string to be added to <paramref name="sb"/>.</param>
        private void Log(StringBuilder sb, string str)
        {
            sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + str);
        }

    }

}