using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Hangfire.Console;
using Hangfire.Server;
using Newtonsoft.Json;
using OurUmbraco.Our.Extensions;
using OurUmbraco.Our.Models;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace OurUmbraco.Our.Services
{
    public class ReleasesService
    {
        private static readonly string CacheFile = HostingEnvironment.MapPath("~/App_Data/TEMP/Releases.json");
        private const string ReleasesCacheCacheKey = "ReleasesCache";

        public List<Release> GetReleasesCache()
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache
                .GetCacheItem(ReleasesCacheCacheKey, () => ReleasesCache(), TimeSpan.FromHours(2));
            return (List<Release>)ApplicationContext.Current.ApplicationCache.RuntimeCache
                .GetCacheItem(ReleasesCacheCacheKey);
        }

        public Release GetLatestRelease()
        {
            var cache = GetReleasesCache();
            return cache.FirstOrDefault(x => x.LatestRelease);
        }

        internal List<Release> ReleasesCache()
        {
            var fileContent = File.ReadAllText(CacheFile);
            var releases = JsonConvert.DeserializeObject<List<Release>>(fileContent);
            foreach (var release in releases)
            {
                release.FullVersion = release.Version.AsFullVersion();
                release.TotalIssueCount = release.Issues.Count;
                release.Bugs = release.Issues.Where(x => x.Type == "type/bug" || x.Type.InvariantContains("Feature") == false);
                release.Features = release.Issues.Except(release.Bugs);

                release.IssuesCompleted = release.Issues.Where(x => 
                    x.State.ToLowerInvariant() == "duplicate"
                    || x.State.ToLowerInvariant() == "fixed"
                    || x.State.ToLowerInvariant() == "closed"
                    || x.State.ToLowerInvariant() == "can't reproduce"
                    || x.State.ToLowerInvariant() == "obsolete"
                    || x.State.ToLowerInvariant() == "workaround posted");

                release.PercentComplete = ReleasePercentComplete(release);
            }

            return releases;
        }

        private static double ReleasePercentComplete(Release release)
        {
            if (release.TotalIssueCount == 0 || release.IssuesCompleted.Any() == false)
                return 0;

            return Math.Round(((double) 100 / release.TotalIssueCount) * release.IssuesCompleted.Count());
        }

        public void GenerateReleasesCache(PerformContext context)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(ReleasesCacheCacheKey);

            var releasesPageNodeId = int.Parse(ConfigurationManager.AppSettings["uReleaseParentNodeId"]);
            var umbracoHelper = new UmbracoHelper(GetUmbracoContext());
            var releasesNode = umbracoHelper.TypedContent(releasesPageNodeId);

            var releaseNodes = releasesNode.Children.ToArray();
            context.WriteLine($"Found {releaseNodes.Length} releases");

            var releases = new List<Release>();
            foreach (var node in releaseNodes)
            {
                if (System.Version.TryParse(node.Name, out var version) == false)
                    continue;

                var status = node.GetPropertyValue<string>("releaseStatus");
                var recommendedRelease = node.GetPropertyValue<bool>("recommendedRelease");
                var releaseDescription = node.GetPropertyValue<string>("bodyText");

                var release = new Release
                {
                    Version = version.ToString(),
                    IsPatch = version.Minor != 0,
                    ReleaseDescription = releaseDescription,
                    CurrentRelease = recommendedRelease,
                    ReleaseStatus = status,
                    InProgressRelease = status != "Released",
                    PlannedRelease = status != "Released",
                    Released = status == "Released",
                    Issues = new List<Release.Issue>()
                };
                releases.Add(release);
            }

            var latestRelease = releases.OrderBy(x => x.Version).Last(x => x.CurrentRelease && x.Released);
            latestRelease.LatestRelease = true;
            context.WriteLine($"Newest downloadable release is {latestRelease.Version}");

            PopulateGitHubIssues(context, releases);
            PopulateYouTrackIssues(context, releases);

            var releasesCache = JsonConvert.SerializeObject(releases, Formatting.Indented);
            File.WriteAllText(CacheFile, releasesCache, Encoding.UTF8);
        }

        private static void PopulateYouTrackIssues(PerformContext context, List<Release> releases)
        {
            var httpClient = new HttpClient();
            const string issuesUrlPrefix = "https://issues.s1.umbraco.io/Umbraco/Api/Releases";

            foreach (var release in releases)
            {
                var url = $"{issuesUrlPrefix}/GetIssuesForRelease?release={release.Version}";
                var response = httpClient.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var releaseIssues = response.Content.ReadAsAsync<ReleaseIssues>().Result;
                    context.WriteLine($"Found {releaseIssues.Issues.Count} issues for release {release.Version}");
                    release.ReleaseDate = releaseIssues.ReleaseDate;
                    foreach (var issue in releaseIssues.Issues)
                    {
                        release.Issues.Add(new Release.Issue
                        {
                            Id = issue.IssueNumber,
                            Breaking = issue.Breaking,
                            State = issue.State,
                            Title = issue.IssueTitle,
                            Type = issue.Type,
                            Source = "YouTrack"
                        });
                    }
                }
                else
                {
                    context.WriteLine($"Release {release.Version} could not be found on YouTrack");
                }
            }

        }

        private static void PopulateGitHubIssues(PerformContext context, List<Release> releases)
        {
            var repository = new OurUmbraco.Community.Models.Repository("umbraco-cms", "umbraco", "Umbraco-CMS", "Umbraco CMS");
            var issuesDirectory = HostingEnvironment.MapPath($"{repository.IssuesStorageDirectory()}/");
            var files = Directory.EnumerateFiles(issuesDirectory, "*.issue.combined.json").ToArray();
            context.WriteLine($"Found {files.Length} issues");

            foreach (var file in files)
            {
                var fileContent = File.ReadAllText(file);
                var issue = JsonConvert.DeserializeObject<GitHubIssueModel>(fileContent);

                foreach (var label in issue.labels)
                {
                    if (label.name.StartsWith("release/") == false)
                        continue;

                    var version = label.name.Replace("release/", string.Empty);
                    var release = releases.FirstOrDefault(x => x.Version == version);
                    if (release == null)
                    {
                        context.WriteLine(
                            $"Issue {issue.number} is tagged with release version {version} but this release has no corresponding node in Our");
                        continue;
                    }

                    var breaking = issue.labels.Any(x => x.name == "compatibility/breaking");

                    var stateLabel = issue.labels.FirstOrDefault(x => x.name.StartsWith("state/"));
                    
                    // default state
                    var state = "new";
                    
                    if (stateLabel != null)
                        // if there's a label with a state then use that as the state
                        state = stateLabel.name.Replace("state/", string.Empty);
                    else if (issue.state == "closed" && issue.labels.Any(x => x.name.StartsWith("release")))
                        // there is no state label applied
                        // if the issue is closed and has a release label on it then we set it to fixed
                        state = "fixed";

                    var typeLabel = issue.labels.FirstOrDefault(x => x.name.StartsWith("type/"));
                    var type = string.Empty;
                    if (typeLabel != null)
                        type = typeLabel.name.Replace("type/", string.Empty);

                    release.Issues.Add(new Release.Issue
                    {
                        Id = issue.number.ToString(),
                        Breaking = breaking,
                        State = state,
                        Title = issue.title,
                        Type = type,
                        Source = "GitHub"
                    });
                }
            }
        }

        internal UmbracoContext GetUmbracoContext()
        {
            var httpContext = new HttpContextWrapper(HttpContext.Current ?? new HttpContext(new SimpleWorkerRequest("temp.aspx", "", new StringWriter())));

            return UmbracoContext.EnsureContext(
                httpContext,
                ApplicationContext.Current,
                new WebSecurity(httpContext, ApplicationContext.Current),
                UmbracoConfig.For.UmbracoSettings(),
                UrlProviderResolver.Current.Providers,
                false);
        }
    }

    public class ReleaseIssues
    {
        public List<ReleaseIssue> Issues { get; set; }
        public DateTime ReleaseDate { get; set; }
    }

    public class ReleaseIssue
    {
        public string IssueNumber { get; set; }
        public string IssueTitle { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public bool Resolved { get; set; }
        public bool Breaking { get; set; }
    }

    internal class GitHubIssueModel
    {
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
        public object assignee { get; set; }
        public object[] assignees { get; set; }
        public object milestone { get; set; }
        public int comments { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object closed_at { get; set; }
        public string author_association { get; set; }
        public string body { get; set; }
        public _Comments[] _comments { get; set; }
    }

    internal class User
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

    internal class Label
    {
        public int id { get; set; }
        public string node_id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string color { get; set; }
        public bool _default { get; set; }
    }

    internal class _Comments
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

    internal class User1
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
