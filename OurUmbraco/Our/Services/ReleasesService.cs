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
using OurUmbraco.Our.Models.GitHub;
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

                // if a manual release percentage isn't set, we'll try to calculate based on issue completion
                if (release.PercentComplete == 0) { 
                    release.PercentComplete = ReleasePercentComplete(release);
                }
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
                var manualProgressPercent = node.GetPropertyValue<int>("overrideYouTrackReleaseProgress");

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
                    Issues = new List<Release.Issue>(),
                    PercentComplete = manualProgressPercent
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

            // Clear cache so it will be fetched again the next time someone asks for it
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(ReleasesCacheCacheKey);
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
                        // Anything from the old issue tracker that's not set to
                        // fixed should not appear in the release notes of v8
                        if(release.Version == "8.0.0" && issue.State != "Fixed")
                            continue;

                        release.Issues.Add(new Release.Issue
                        {
                            Id = issue.IssueNumber,
                            Breaking = issue.Breaking,
                            State = issue.State,
                            Title = issue.IssueTitle,
                            Type = issue.Type,
                            Source = ReleaseSource.YouTrack
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
            var pullsDirectory = HostingEnvironment.MapPath($"{repository.IssuesStorageDirectory()}/pulls/");
            AddItemsToReleases(context, releases, issuesDirectory, "issue");
            AddItemsToReleases(context, releases, pullsDirectory, "pull");
        }

        private static void AddItemsToReleases(PerformContext context, List<Release> releases, string directory, string fileTypeName) 
        {
            context.WriteLine($"Processing {fileTypeName}s");
            var files = Directory.EnumerateFiles(directory, $"*.{fileTypeName}.combined.json").ToArray();
            context.WriteLine($"Found {files.Length} items");

            foreach (var file in files)
            {
                var fileContent = File.ReadAllText(file);
                var item = JsonConvert.DeserializeObject<Issue>(fileContent);

                foreach (var label in item.Labels)
                {
                    if (label.Name.StartsWith("release/") == false)
                        continue;

                    var version = label.Name.Replace("release/", string.Empty);
                    var release = releases.FirstOrDefault(x => x.Version == version);
                    if (release == null)
                    {
                        context.WriteLine(
                            $"Item {item.Number} is tagged with release version {version} but this release has no corresponding node in Our");
                        continue;
                    }

                    var breaking = item.Labels.Any(x => x.Name == "category/breaking");

                    var stateLabel = item.Labels.FirstOrDefault(x => x.Name.StartsWith("state/"));

                    // default state
                    var state = "new";

                    if (stateLabel != null)
                        // if there's a label with a state then use that as the state
                        state = stateLabel.Name.Replace("state/", string.Empty);
                    else if (item.State == "closed" && item.Labels.Any(x => x.Name.StartsWith("release")))
                        // there is no state label applied
                        // if the item is closed and has a release label on it then we set it to fixed
                        state = "fixed";

                    var typeLabel = item.Labels.FirstOrDefault(x => x.Name.StartsWith("type/"));
                    var type = string.Empty;
                    if (typeLabel != null)
                        type = typeLabel.Name.Replace("type/", string.Empty);

                    context.WriteLine($"Adding {fileTypeName} {item.Number} to release {release.Version}");

                    release.Issues.Add(new Release.Issue
                    {
                        Id = item.Number.ToString(),
                        Breaking = breaking,
                        State = state,
                        Title = item.Title,
                        Type = type,
                        Source = ReleaseSource.GitHub
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
}
