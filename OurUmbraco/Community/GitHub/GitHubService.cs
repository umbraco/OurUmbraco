using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using GraphQL.Client;
using GraphQL.Common.Request;
using Hangfire.Console;
using Hangfire.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OurUmbraco.Community.GitHub.Models;
using OurUmbraco.Community.GitHub.Models.Cached;
using OurUmbraco.Community.Models;
using OurUmbraco.Our.Models.GitHub;
using OurUmbraco.Our.Services;
using RestSharp;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Time;
using Skybrud.Social.GitHub.Exceptions;
using Skybrud.Social.GitHub.Options;
using Skybrud.Social.GitHub.Options.Issues;
using Skybrud.Social.GitHub.Responses.Issues;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Security;
using File = System.IO.File;
using GitHubIssueState = Skybrud.Social.GitHub.Options.Issues.GitHubIssueState;
using Task = System.Threading.Tasks.Task;

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
        public readonly string LabelsJsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/GitHubLabels/");

        private static readonly object Lock = new object();
        public static bool IsLocked { get; set; }

        private readonly string _hqUsersFile = HostingEnvironment.MapPath("~/Config/githubhq.txt");
        private readonly string _teamUmbracoUsersFile = HostingEnvironment.MapPath("~/Config/TeamUmbraco.json");

        public TeamUmbraco GetTeam(string repository)
        {
            var teamUmbraco = new TeamUmbraco();
            var usernames = File.ReadAllLines(_hqUsersFile).Where(x => x.Trim() != "").Distinct().ToArray();

            var content = File.ReadAllText(_teamUmbracoUsersFile);
            var teamUmbracoUsers = JsonConvert.DeserializeObject<List<TeamUmbraco>>(content);
            var team = teamUmbracoUsers.FirstOrDefault(x => x.TeamName == repository);
            if (team != null)
            {
                team.Members.AddRange(usernames);
                teamUmbraco = team;
            }

            return teamUmbraco;
        }

        public List<string> GetHqMembers()
        {
            if (!File.Exists(_hqUsersFile))
            {
                var message = $"Config file was not found: {_hqUsersFile}";
                LogHelper.Debug<GitHubService>(message);
                throw new Exception(message);
            }

            var hqUsernames = File.ReadAllLines(_hqUsersFile).Where(x => x.Trim() != "").Distinct().ToArray();
            var hqMembers = hqUsernames.Select(hqUsername => hqUsername.ToLowerInvariant()).ToList();
            return hqMembers;
        }

        public List<TeamUmbraco> GetTeamMembers()
        {
            var content = File.ReadAllText(_teamUmbracoUsersFile);
            var teamUmbracoUsers = JsonConvert.DeserializeObject<List<TeamUmbraco>>(content);
            return teamUmbracoUsers;
        }

        public List<Label> RequiredLabels()
        {
            var labels = new List<Label>();
            labels.AddRange(TypeLabels());
            labels.AddRange(StatusLabels());
            labels.AddRange(StateLabels());

            return labels;
        }

        public List<Label> TypeLabels()
        {
            var labels = new List<Label>
            {
                new Label { Name = "type/bug" },
                new Label { Name = "type/feature" }
            };

            return labels;
        }

        public List<Label> StatusLabels()
        {
            var labels = new List<Label>
            {
                new Label { Name = "status/awaiting-feedback" },
                new Label { Name = "status/blocked" },
                new Label { Name = "status/idea" }
            };

            return labels;
        }

        public List<Label> StateLabels()
        {
            var labels = new List<Label>
            {
                new Label { Name = "state/backlog" },
                new Label { Name = "state/estimation" },
                new Label { Name = "state/in-progress" },
                new Label { Name = "state/maturing" },
                new Label { Name = "state/reopened" },
                new Label { Name = "state/review" },
                new Label { Name = "state/sprint-backlog" }
            };

            return labels;
        }

        public string RequiredLabelColor(string labelName, string color)
        {
            var labelPrefix = labelName.Split('/').FirstOrDefault();
            if (labelPrefix == null)
                return string.Empty;

            switch (labelPrefix)
            {
                case "category":
                    return "ffccf8";
                case "community":
                    return "b8d9ff";
                case "partner":
                    return "fdffe8";
                case "release":
                    return "daf0c9";
                case "state":
                    return "eaf0f7";
                case "status":
                    return "f9c5a4";
                case "type":
                    return "9644bf";
                case "project":
                    return "f9f9ca";
                default:
                    return string.Empty;
            }
        }

        public string[] GetLabelRepositories()
        {
            return new[] {
                // Community
                "Issues.Community",
                "OurUmbraco",
                "The-Starter-Kit",
                "UmbracoExamine.PDF",
                "UmbracoIdentityExtensions",

                // Core
                "Umbraco.Private",
                "Umbraco-Courier",
                "Umbraco-Deploy",
                "Forms",
                "Legacy.Headless.Client.Net",
                "Umbraco.Headless.Client.NodeJs",
                "Umbraco.Headless",
                "Umbraco.Headless.RestApi",
                "Umbraco.Headless.Client.Net",
                "Umbraco-Ecom",
                "Umbraco-CMS",
                "Umbraco.Forms.Issues",

                // Cloud.Infrastructure
                "Cloud.Infrastructure",
                "Concorde.DevOps",
                "InternalCloudDocumentation",

                // Concorde
                "Concorde",
                "Umbraco.Courier.Contrib",
                "Umbraco.Deploy.Contrib",
                "Concorde.AutoUpgrader",
                "Concorde.AzureAutomation",
                "Concorde.BaselineChild.Service",
                "Concorde.CleanupService",
                "Concorde.CreatePreallocationBaseline",
                "Concorde.Functions",
                "Concorde.George",
                "Concorde.KuduCourierSync",
                "Concorde.Latch",
                "Concorde.LiveEdit.Client",
                "Concorde.Messaging",
                "Concorde.Pack",
                "Concorde.Preallocation.Service",
                "Concorde.VisualStudio.Generator.Waasp",
                "Concorde.Websites",
                "ConcordePlatform",
                "Umbraco-Cloud-",
                "Umbraco.Cloud.Issues",
                "Umbraco.Courier.Issues",
                "Umbraco.Deploy.ValueConnectors"
            };
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
                "Umbraco.Deploy.ValueConnectors"
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

                foreach (var repository in GetRepositories())
                {
                    var pulls = GetExistingPullsFromDisk();

                    var stopImport = false;
                    // GitHub paging starts at 1, not 0, so we'll start couting at 1 as well
                    for (var i = 1; i < int.MaxValue; i++)
                    {
                        if (stopImport)
                            break;

                        pulls = GetPulls(repository: repository, page: i, pulls: pulls, stopImport: out stopImport);
                        Thread.Sleep(1000);
                    }

                    // Save the JSON to disk
                    var rawJson = JsonConvert.SerializeObject(pulls, Formatting.Indented);
                    File.WriteAllText(PullRequestsJsonPath, rawJson, Encoding.UTF8);
                }

                IsLocked = false;
            }
        }

        private List<GithubPullRequestModel> GetExistingPullsFromDisk()
        {
            var pulls = new List<GithubPullRequestModel>();
            if (File.Exists(PullRequestsJsonPath) == false)
                return pulls;

            var content = File.ReadAllText(PullRequestsJsonPath);
            pulls = JsonConvert.DeserializeObject<List<GithubPullRequestModel>>(content);

            return pulls;
        }

        private List<GithubPullRequestModel> GetPulls(string repository, int page, List<GithubPullRequestModel> pulls, out bool stopImport)
        {
            // Initialize the request
            var username = ConfigurationManager.AppSettings["GitHubUsername"];
            var password = ConfigurationManager.AppSettings["GitHubPassword"];
            var client = new RestClient(GitHubApiClient) { Authenticator = new HttpBasicAuthenticator(username, password) };

            var resource = $"/repos/{RepositoryOwner}/{repository}/pulls?state=all&page={page}&sort=updated&direction=desc";
            LogHelper.Info<GitHubService>($"Getting PR data from {resource}");

            var request = new RestRequest(resource, Method.GET);
            client.UserAgent = UserAgent;

            // Make the request to the GitHub API
            var result = client.Execute<List<GithubPullRequestModel>>(request);

            stopImport = false;

            if (result.Data.Count == 0)
            {
                LogHelper.Info<GitHubService>("No records returned from the API, setting stopImport to true");
                stopImport = true;
                return pulls;
            }

            foreach (var pull in result.Data)
            {
                var existing = pulls.FirstOrDefault(x => x.Id == pull.Id);

                var isFound = existing != null;
                var isUpdated = isFound && existing.UpdatedAt != pull.UpdatedAt;

                if (isFound)
                {
                    if (isUpdated)
                    {
                        // It's been updated, remove it, so we can replace with the latest version
                        pulls.Remove(existing);
                    }
                    else
                    {
                        // We've already imported this one, stop going down the list
                        LogHelper.Info<GitHubService>("We've already imported this PR, so we'll stop importing here");
                        stopImport = true;
                        break;
                    }
                }

                pull.Repository = repository;
                pulls.Add(pull);
            }

            return pulls;
        }

        public List<PullRequestMember> MatchPullsToMembers()
        {
            var searcher = ExamineManager.Instance.SearchProviderCollection["PullRequestSearcher"];
            var criteria = (LuceneSearchCriteria)searcher.CreateSearchCriteria();

            criteria = (LuceneSearchCriteria)criteria.RawQuery("*:*");
            var searchResults = searcher.Search(criteria);
            var contributors = searchResults
                .Where(x => x.Fields["memberId"] != null && string.IsNullOrWhiteSpace(x.Fields["memberId"]) == false).ToList();

            var members = new List<int>();
            foreach (var match in contributors)
            {
                int.TryParse(match.Fields["memberId"], out var memberId);
                if (members.Contains(memberId) == false)
                    members.Add(memberId);
            }

            var pullRequestMembers = new List<PullRequestMember>();

            foreach (var memberId in members)
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                var member = umbracoHelper.TypedMember(memberId);

                var pullRequestMember = new PullRequestMember
                {
                    MemberId = member.Id,
                    Name = member.Name,
                    GitHubUsername = member.GetPropertyValue<string>("github"),
                    Repositories = new List<string>()
                };

                criteria = (LuceneSearchCriteria)searcher.CreateSearchCriteria();
                criteria = (LuceneSearchCriteria)criteria.RawQuery($"memberId:{memberId}");
                searchResults = searcher.Search(criteria);

                if (searchResults.Any())
                {
                    var totalPulls = searchResults.Count();
                    var openPulls = searchResults.Count(x => x.Fields["state"] != null && x.Fields["state"] == "open");
                    var acceptedPulls = searchResults.Count(x => x.Fields["mergedAt"] != null && string.IsNullOrWhiteSpace(x.Fields["mergedAt"]) == false);
                    var closedPulls = totalPulls - openPulls - acceptedPulls;

                    var repositories = new List<string>();
                    foreach (var searchResult in searchResults)
                    {
                        var repository = searchResult.Fields["repository"];
                        if (repositories.Contains(repository) == false)
                            repositories.Add(repository);
                    }

                    pullRequestMember.TotalPulls = totalPulls;
                    pullRequestMember.OpenPulls = openPulls;
                    pullRequestMember.AcceptedPulls = acceptedPulls;
                    pullRequestMember.ClosedPulls = closedPulls;
                    pullRequestMember.Repositories = repositories;
                }

                pullRequestMembers.Add(pullRequestMember);
            }

            return pullRequestMembers;
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
        /// Gets a list of contributors (<see cref="GitHubContributorModel"/>) for a single GitHub repository.
        /// </summary>
        /// <param name="repo">The alias (slug) of the repository.</param>
        /// <returns>A list of <see cref="GitHubContributorModel"/>.</returns>
        public List<Label> GetLabels(PerformContext context, string repo)
        {
            var client = GitHubApi.Client;
            client.AccessToken = WebConfigurationManager.AppSettings["GitHubAccessToken"];
            var labelsResponse = client.DoHttpGetRequest($"/repos/{RepositoryOwner.ToLowerInvariant()}/{repo}/labels");

            if (labelsResponse.StatusCode != HttpStatusCode.OK)
            {
                context.WriteLine($"Failed getting labels for repository: {repo}", labelsResponse.Body);
                return new List<Label>();
            }

            var labels = JsonConvert.DeserializeObject<List<Label>>(labelsResponse.Body);
            return labels;
        }

        public List<LabelReport> GetLabelReport()
        {
            var fileContents = File.ReadAllText($"{LabelsJsonPath}AllLabels.json");
            var allLabels = JsonConvert.DeserializeObject<List<LabelReport>>(fileContents);
            return allLabels;
        }

        public void DownloadAllLabels(PerformContext context)
        {
            var repos = GetLabelRepositories();
            var progressBar = context.WriteProgressBar();

            Directory.CreateDirectory(Path.GetDirectoryName(LabelsJsonPath));
            context.WriteLine($"Found {repos.Count()} repositories");

            var allLabels = new List<LabelReport>();
            foreach (var repository in repos.WithProgress(progressBar))
            {
                var labels = GetLabels(context, repository);
                var repositoryLabels = new LabelReport
                {
                    Repository = repository,
                    NonCompliantLabels = new List<NonCompliantLabel>(),
                    Categories = new List<Label>(),
                    Projects = new List<Label>(),
                    RequiredLabels = new List<Label>(),
                    RogueLabels = new List<Label>()
                };

                var requiredLabels = RequiredLabels();
                var requiredLabelsCount = 0;
                foreach (var label in labels)
                {
                    if (label.Name.Contains("/") == false || label.Name.Contains(" "))
                    {
                        var nonCompliantLabel = new NonCompliantLabel
                        {
                            Label = label,
                            LabelProblem = "Contains space or missing a slash"
                        };
                        repositoryLabels.NonCompliantLabels.Add(nonCompliantLabel);
                    }
                    else
                    {
                        var foundRequiredLabel = requiredLabels.FirstOrDefault(x => x.Name == label.Name);
                        if (foundRequiredLabel != null)
                        {
                            var labelShouldHaveColor = RequiredLabelColor(label.Name, label.Color);
                            if (labelShouldHaveColor != string.Empty && labelShouldHaveColor != label.Color)
                            {
                                var nonCompliantLabel = new NonCompliantLabel
                                {
                                    Label = label,
                                    LabelProblem = $"Wrong color '{label.Color}', should be '{labelShouldHaveColor}'"
                                };
                                repositoryLabels.NonCompliantLabels.Add(nonCompliantLabel);
                            }
                            else
                            {
                                requiredLabelsCount = requiredLabelsCount + 1;
                                repositoryLabels.RequiredLabels.Add(label);
                            }
                        }
                        else
                        {
                            foundRequiredLabel = requiredLabels.FirstOrDefault(x => string.Equals(x.Name, label.Name, StringComparison.InvariantCultureIgnoreCase));
                            if (foundRequiredLabel == null)
                                continue;

                            var nonCompliantLabel = new NonCompliantLabel
                            {
                                Label = label,
                                LabelProblem = $"Wrong casing '{label.Name}', should be '{foundRequiredLabel.Name}'"
                            };
                            repositoryLabels.NonCompliantLabels.Add(nonCompliantLabel);
                        }
                    }
                }

                foreach (var label in labels)
                {
                    var labelPrefix = label.Name.Split('/').FirstOrDefault();

                    var rogueLabels = new List<NonCompliantLabel>();
                    if (label.Name.Contains("/") && label.Name.Contains(" ") == false &&
                        labelPrefix == "state" && StateLabels().Any(x => x.Name == label.Name) == false)
                    {
                        var rogueLabel = new NonCompliantLabel
                        {
                            Label = label,
                            LabelProblem = $"Rogue state label {label.Name} is not a known state"
                        };
                        rogueLabels.Add(rogueLabel);
                        repositoryLabels.RogueLabels.Add(label);
                    }

                    if (label.Name.Contains("/") && label.Name.Contains(" ") == false &&
                        labelPrefix == "status" && StatusLabels().Any(x => x.Name == label.Name) == false)
                    {
                        var rogueLabel = new NonCompliantLabel
                        {
                            Label = label,
                            LabelProblem = $"Rogue status label {label.Name} is not a known status"
                        };
                        rogueLabels.Add(rogueLabel);
                        repositoryLabels.RogueLabels.Add(label);
                    }

                    if (label.Name.Contains("/") && label.Name.Contains(" ") == false &&
                        labelPrefix == "type" && TypeLabels().Any(x => x.Name == label.Name) == false)
                    {
                        // Spike is not a required type, but also not a rogue one
                        if (label.Name.EndsWith("/spike") == false)
                        {
                            var rogueLabel = new NonCompliantLabel
                            {
                                Label = label,
                                LabelProblem = $"Rogue type label {label.Name} is not a known type"
                            };
                            rogueLabels.Add(rogueLabel);
                            repositoryLabels.RogueLabels.Add(label);
                        }
                    }

                    if (rogueLabels.Any())
                    {
                        repositoryLabels.NonCompliantLabels.AddRange(rogueLabels);
                        // No need to check for color compliance now, the label is not supposed to be there
                        continue;
                    }

                    // Required labels have already been checked
                    if (requiredLabels.Any(x => x.Name == label.Name))
                        continue;

                    // non-required label, check for color compliance
                    var labelShouldHaveColor = RequiredLabelColor(label.Name, label.Color);
                    if (labelShouldHaveColor == string.Empty || labelShouldHaveColor == label.Color)
                        continue;

                    var nonCompliantLabel = new NonCompliantLabel
                    {
                        Label = label,
                        LabelProblem = $"Wrong color '{label.Color}', should be '{labelShouldHaveColor}'"
                    };
                    repositoryLabels.NonCompliantLabels.Add(nonCompliantLabel);
                }

                if (requiredLabels.Count == requiredLabelsCount)
                    repositoryLabels.HasRequiredLabels = true;

                repositoryLabels.Categories = labels.Where(x => x.Name.StartsWith("category") && x.Name.EndsWith("/breaking") == false).ToList();
                repositoryLabels.Projects = labels.Where(x => x.Name.StartsWith("project")).ToList();

                allLabels.Add(repositoryLabels);

                var rawJson = JsonConvert.SerializeObject(labels, Formatting.Indented);

                // Save the JSON to disk
                context.WriteLine($"Writing file {repository}.json");
                File.WriteAllText($"{LabelsJsonPath}{repository}.json", rawJson, Encoding.UTF8);
            }

            var allLabelsJson = JsonConvert.SerializeObject(allLabels);
            File.WriteAllText($"{LabelsJsonPath}AllLabels.json", allLabelsJson, Encoding.UTF8);
        }

        public async Task NotifyUnmergeablePullRequests(PerformContext context)
        {
            var queryFileLocation = "~/Config/Queries/GetUnmergeablePullRequests.json";
            var query = GetQueryFromFile(queryFileLocation);
            if (query == null)
                context.WriteLine($"Query file not found {queryFileLocation}");

            var cursor = string.Empty;

            var repoManagementService = new RepositoryManagementService();
            var repositories = repoManagementService.GetAllPublicRepositories();

            foreach (var repo in repositories)
            {
                var repositoryName = repo.Alias;

                while (true)
                {
                    var graphClient = GetGraphQlClient();
                    var request = string.IsNullOrEmpty(cursor)
                        ? new GraphQLRequest { Query = query, Variables = $"{{ \"repository\": \"{repositoryName}\" }}" }
                        : new GraphQLRequest { Query = query, Variables = $"{{ \"cursor\": \"{cursor}\", \"repository\": \"{repositoryName}\" }}" };

                    try
                    {
                        var response = await graphClient.PostAsync(request).ConfigureAwait(false);

                        if (response.Errors != null)
                            foreach (var error in response.Errors)
                            {
                                context.SetTextColor(ConsoleTextColor.Red);
                                context.WriteLine($"Error executing GraphQL query: {error.Message}");
                                context.ResetTextColor();
                            }

                        var repository = response.GetDataFieldAs<GraphQLModel.Repository>("repository");
                        if (repository.PullRequests.Edges.Any() == false)
                            break;

                        var unmergeable = repository.PullRequests.Edges.Where(x => x.Node.Mergeable == "CONFLICTING").ToList();

                        cursor = repository.PullRequests.PageInfo.EndCursor;

                        foreach (var pr in unmergeable)
                            context.WriteLine($"Pull number {pr.Node.Number} for repository {repo.Alias} is not mergeable due to a conflict");
                    }
                    catch (Exception ex)
                    {
                        context.WriteLine($"Ran into a bit of trouble - {ex.Message}");
                    }
                }
            }
        }

        private static string GetQueryFromFile(string queryFileLocation)
        {
            var queryFile = HostingEnvironment.MapPath(queryFileLocation);
            var query = File.ReadAllText(queryFile);
            return query;
        }

        private static GraphQLClient GetGraphQlClient()
        {
            var gitHubAccessToken = ConfigurationManager.AppSettings["GitHubAccessToken"];
            var graphClient = new GraphQLClient("https://api.github.com/graphql");
            graphClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {gitHubAccessToken}");
            graphClient.DefaultRequestHeaders.Add("User-Agent", "OurUmbraco");
            return graphClient;
        }


        /// <summary>
        /// Get all contributors from GitHub Umbraco repositories
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        public IRestResponse<List<GitHubContributorModel>> GetAllRepoContributors(string repo)
        {
            var client = new RestClient(GitHubApiClient);
            var request = new RestRequest($"/repos/{RepositoryOwner}/{repo}/stats/contributors", Method.GET);
            client.UserAgent = UserAgent;
            var response = client.Execute<List<GitHubContributorModel>>(request);
            return response;
        }

        public GitHubContributorsResult GetOverallContributors(int maxAttempts = 3)
        {
            // Initialize a new StringBuilder for logging/testing purposes
            var stringBuilder = new StringBuilder();

            var logins = GetHqMembers();

            // A dictionary for the response of each repository
            var responses = new Dictionary<string, IRestResponse<List<GitHubContributorModel>>>();

            // Hashset for keeping track of missing responses
            var missing = new HashSet<string>();

            Log(stringBuilder, "Attempt 1");

            // Iterate over the repositories
            foreach (var repo in GetRepositories())
            {
                Log(stringBuilder, $"-> Making request to {GitHubApiClient}/repos/{RepositoryOwner}/{repo}/stats/contributors");

                // Make the request to the GitHub API
                var response = GetRepositoryContributors(repo);

                Log(stringBuilder, $"  -> {(int)response.StatusCode} -> {response.StatusCode}");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        responses[repo] = response;
                        break;
                    case HttpStatusCode.Accepted:
                        missing.Add(repo);
                        break;
                    default:
                        var message = $"Failed getting contributors for repository {repo}: {response.StatusCode}\r\n\r\n{response.Content}";
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

                Log(stringBuilder, $"Attempt {i}");

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
                            var message = $"Failed getting contributors for repository {repo}: {response.StatusCode}\r\n\r\n{response.Content}";
                            Log(stringBuilder, message);
                            throw new Exception(message);
                    }

                }
            }

            if (missing.Count > 0)
            {
                var message = $"Unable to get contributors for one or more repositories:\r\n{string.Join("\r\n", missing)}";
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
                .Where(g => g.Total > 0 && logins.Contains(g.Author.Login.ToLowerInvariant()) == false)
                .GroupBy(g => g.Author.Id)
                .Select(g => new GitHubGlobalContributorModel(g))
                .OrderByDescending(c => c.TotalCommits)
                .ThenByDescending(c => c.TotalAdditions)
                .ThenByDescending(c => c.TotalDeletions)
                .ToList();

            return new GitHubContributorsResult(globalContributors, $"{stringBuilder}{string.Empty}");
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
            File.WriteAllText(JsonPath, rawJson, Encoding.UTF8);

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
            sb.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {str}");
        }

        private Skybrud.Social.GitHub.GitHubService _github;

        public Skybrud.Social.GitHub.GitHubService GitHubApi => _github
                ?? (_github = Skybrud.Social.GitHub.GitHubService.CreateFromAccessToken(WebConfigurationManager.AppSettings["GitHubAccessToken"]));

        public void UpdateIssues(PerformContext context, Community.Models.Repository repository)
        {
            // Accept newer versions of the TLS protocol
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
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
                    context.WriteLine($"Fetching page issues for repo {repository.Alias} (page {options.Page})");
                    GitHubGetIssuesResponse issuesResponse;
                    try
                    {
                        issuesResponse = GitHubApi.Issues.GetIssues(options);
                    }
                    catch (GitHubHttpException ex)
                    {
                        throw new Exception($"Failed fetching page {options.Page}\r\n\r\n{ex.Response.Response.ResponseUri}", ex);
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

                        string issuesFile;
                        string issuesCommentFile;
                        string issuesEventsFile;
                        string issuesCombinedFile;
                        if (responseIsIssue)
                        {
                            issuesFile = HostingEnvironment.MapPath($"{repository.IssuesStorageDirectory()}/{response.Number}.issue.json");
                            issuesCommentFile = HostingEnvironment.MapPath($"{repository.IssuesStorageDirectory()}/{response.Number}.issue.comments.json");
                            issuesEventsFile = HostingEnvironment.MapPath($"{repository.IssuesStorageDirectory()}/{response.Number}.issue.events.json");
                            issuesCombinedFile = HostingEnvironment.MapPath($"{repository.IssuesStorageDirectory()}/{response.Number}.issue.combined.json");
                        }
                        else
                        {
                            issuesFile = HostingEnvironment.MapPath($"{repository.IssuesStorageDirectory()}/pulls/{response.Number}.pull.json");
                            issuesCommentFile = HostingEnvironment.MapPath($"{repository.IssuesStorageDirectory()}/pulls/{response.Number}.pull.comments.json");
                            issuesEventsFile = HostingEnvironment.MapPath($"{repository.IssuesStorageDirectory()}/pulls/{response.Number}.pull.events.json");
                            issuesCombinedFile = HostingEnvironment.MapPath($"{repository.IssuesStorageDirectory()}/pulls/{response.Number}.pull.combined.json");
                        }

                        // Make sure we have a directory
                        Directory.CreateDirectory(Path.GetDirectoryName(issuesFile));

                        JsonUtils.SaveJsonObject(issuesFile, response);

                        // Fetch comments and events if the local JSON file is older than the last update time of the issue
                        JArray comments;
                        JArray events;
                        if (File.Exists(issuesCommentFile) && File.GetLastWriteTimeUtc(issuesCommentFile) > response.UpdatedAt.DateTime.ToUniversalTime())
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

                        if (File.Exists(issuesEventsFile) && File.GetLastWriteTimeUtc(issuesEventsFile) > response.UpdatedAt.DateTime.ToUniversalTime())
                        {
                            events = JsonUtils.LoadJsonArray(issuesEventsFile);
                        }
                        else
                        {
                            context.WriteLine($"Fetching events for issue {response.Number} {response.Title}");
                            var issueEventssResponse = GitHubApi.Client.DoHttpGetRequest($"/repos/{repository.Owner}/{repository.Alias}/issues/{response.Number}/events");
                            if (issueEventssResponse.StatusCode != HttpStatusCode.OK)
                                throw new Exception($"Failed fetching events for issue #{response.Number} ({issueEventssResponse.StatusCode})");

                            events = JsonUtils.ParseJsonArray(issueEventssResponse.Body);
                            JsonUtils.SaveJsonArray(issuesEventsFile, events);
                        }

                        // Save a JSON file with all the combined data we have for the issue
                        response.JObject.Add("_comments", comments);
                        response.JObject.Add("events", events);
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

        public void AddCommentToUpForGrabsIssues(PerformContext context)
        {
            var repositoryService = new RepositoryManagementService();
            var issues = repositoryService.GetAllOpenIssues(false);
            var upForGrabsIssues = issues.Find(i => i.CategoryKey == RepositoryManagementService.CategoryKey.UpForGrabs);
            AddLabelDescriptionAsComment(context, upForGrabsIssues, "up for grabs".ToLowerInvariant());
        }

        public void AddCommentToAwaitingFeedbackIssues(PerformContext context)
        {
            var repositoryService = new RepositoryManagementService();
            var issues = repositoryService.GetAllOpenIssues(false);
            var awaitingFeedback = issues.Find(i => i.CategoryKey == RepositoryManagementService.CategoryKey.AwaitingFeedback);
            AddLabelDescriptionAsComment(context, awaitingFeedback, "awaiting feedback".ToLowerInvariant());
        }

        private void AddLabelDescriptionAsComment(PerformContext context, RepositoryManagementService.GitHubCategorizedIssues categorizedIssues, string taskAlias)
        {
            if (categorizedIssues == null || categorizedIssues.Issues.Any() == false)
                return;

            var umbracoHelper = new UmbracoHelper(EnsureUmbracoContext());

            var labelType = umbracoHelper
                .TypedContentSingleAtXPath("//gitHubLabelCommentRepository")
                .Children(c => c.GetPropertyValue<string>("taskAlias").ToLowerInvariant() == taskAlias);

            if (labelType == null)
                return;

            var actionNode = labelType.FirstOrDefault();
            var friendlyComments = actionNode.GetPropertyValue<IEnumerable<IPublishedContent>>("gitHubLabelComments").ToList();
            

            foreach (var issue in categorizedIssues.Issues.Where(x => x.Number == 5184 && x.RepositoryName == "Umbraco-CMS"))
            {
                var randomCommentIndex = StaticRandom.Instance.Next(0, friendlyComments.Count - 1);
                var selectedComment = friendlyComments[randomCommentIndex];

                if (selectedComment == null)
                    return;

                var addComment = new AddComment { CommentBody = selectedComment.GetProperty("comment").DataValue.ToString() };

                var gitHubAccessToken = ConfigurationManager.AppSettings["GitHubUmbraBotAccessToken"];

                // Initialize the request
                var username = ConfigurationManager.AppSettings["GitHubUsername"];
                var password = ConfigurationManager.AppSettings["GitHubPassword"];
                var client = new RestClient(GitHubApiClient) { Authenticator = new HttpBasicAuthenticator(username, password) };
                var resource = $"/repos/{RepositoryOwner}/{issue.RepositoryName}/issues/";

                addComment.CommentBody = addComment.CommentBody.Replace("{{issueowner}}", "@" + issue.User.Login);
                var commentsEndPoint = string.Concat(resource, issue.Number, "/comments");
                var request = new RestRequest(commentsEndPoint, Method.POST);
                request.AddHeader("Authorization", $"Bearer {gitHubAccessToken}");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("undefined", JsonConvert.SerializeObject(addComment), ParameterType.RequestBody);
                client.UserAgent = UserAgent;
                var result = client.Execute<List<GithubPullRequestModel>>(request);

                context.WriteLine($"Added comment to issue {issue.Number}");
            }

        }

        private UmbracoContext EnsureUmbracoContext()
        {
            if (UmbracoContext.Current != null) return UmbracoContext.Current;
            var dummyContext = new HttpContextWrapper(new System.Web.HttpContext(new SimpleWorkerRequest("/", string.Empty, new StringWriter())));
            return UmbracoContext.EnsureContext(dummyContext, ApplicationContext.Current, new WebSecurity(dummyContext, ApplicationContext.Current), false);
        }
    }

    public class PullRequestMember
    {
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string GitHubUsername { get; set; }
        public int TotalPulls { get; set; }
        public int OpenPulls { get; set; }
        public int AcceptedPulls { get; set; }
        public int ClosedPulls { get; set; }
        public List<string> Repositories { get; set; }
    }


    public class GitHubResponse
    {
        public Pull_Request pull_request { get; set; }
    }

    public class Pull_Request
    {
        public string url { get; set; }
    }

    public class RepositoryLabels
    {
        public string Repository { get; set; }
        public List<Label> Labels { get; set; }
        public bool HasRequiredLabels { get; set; }
        public List<NonCompliantLabel> NonCompliantLabels { get; set; }
    }

    public class NonCompliantLabel
    {
        public Label Label { get; set; }
        public string LabelProblem { get; set; }
    }

    public class Label
    {
        [JsonProperty("id")]
        public int Id { get; set; }
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

    public class LabelReport
    {
        public string Repository { get; set; }
        public bool HasRequiredLabels { get; set; }
        public List<NonCompliantLabel> NonCompliantLabels { get; set; }
        public List<Label> Projects { get; set; }
        public List<Label> Categories { get; set; }
        public List<Label> RequiredLabels { get; set; }
        public List<Label> RogueLabels { get; set; }
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