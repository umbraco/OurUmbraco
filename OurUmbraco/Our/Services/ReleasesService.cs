using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Helpers;
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
        
        public Release GetLatestVersion8Release()
        {
            var cache = GetReleasesCache();
            return cache.OrderBy(x => x.FullVersion).LastOrDefault(x => x.Released && x.FullVersion.Major == 8);
        }

        internal List<Release> ReleasesCache()
        {
            var fileContent = File.ReadAllText(CacheFile);
            var releases = JsonConvert.DeserializeObject<List<Release>>(fileContent);
            
            foreach (var release in releases)
            {
                release.FullVersion = release.Version.AsFullVersion();
                release.TotalIssueCount = release.Issues.Count;
                
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
            
                release.Bugs = release.Issues.Where(x => x.Type == "type/bug" || x.Type.InvariantContains("Feature") == false);
                release.Features = release.Issues.Except(release.Bugs);
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

            var releasesCache = JsonConvert.SerializeObject(releases, Formatting.None);
            File.WriteAllText(CacheFile, releasesCache, Encoding.UTF8);

            // Clear cache so it will be fetched again the next time someone asks for it
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(ReleasesCacheCacheKey);
        }

        private static void PopulateYouTrackIssues(PerformContext context, List<Release> releases)
        {
            var youTrackArchiveFile = HostingEnvironment.MapPath("~/config/YouTrackArchive.json");
            var text = File.ReadAllText(youTrackArchiveFile);
            var youTrackArchive = JsonConvert.DeserializeObject<List<Release.Issue>>(text);

            foreach (var release in releases)
            {
                if(release.Version == "8.0.0")
                    continue;

                var issues = youTrackArchive.Where(x => x.Version == release.Version);
                release.Issues.AddRange(issues);
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

                    // this label makes sure the item doesn't show up in the release notes, it adds no value
                    if(item.Labels.Any(x => x.Name == "release/no-notes"))
                        continue;

                    var version = label.Name.Replace("release/", string.Empty);
                    var release = releases.FirstOrDefault(x => x.Version == version);
                    if (release == null)
                    {
                        context.WriteLine(
                            $"Item {item.Number} is tagged with release version {version} but this release has no corresponding node in Our");
                        continue;
                    }

                    var fullVersion = new System.Version();
                    System.Version.TryParse(release.Version, out fullVersion); 
                    release.FullVersion = fullVersion;
                    if (release.CategorizedIssues == null)
                        release.CategorizedIssues = GetDefaultCategories();
                    
                    var breaking = item.Labels.Any(x => x.Name == "category/breaking");
                    var communityContrib = item.Labels.Any(x => x.Name == "community/pr");
                    
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

                    var issue = new Release.Issue
                    {
                        Id = item.Number.ToString(),
                        Breaking = breaking,
                        State = state,
                        Title = item.Title.Replace("v8: ", "").Replace("V8: ", "").Replace("v8:", "").Replace("v8:", ""),
                        Type = type,
                        Source = ReleaseSource.GitHub,
                        CommunityContribution = communityContrib,
                        ContributorAvatar = item.IsPr ? item.User.AvatarUrl : ""
                    };

                    if (release.FullVersion >= new System.Version(8, 7, 0))
                        AddCategoriesToItems(release, item, issue);

                    release.Issues.Add(issue);
                }
            }

            foreach (var release in releases.Where(x => x.FullVersion >= new System.Version(8,7,0)))
            {
                // work on a local copy not the global/shared one
                var releaseIssues = release.Issues; 
                
                var categorizedIssueIds = new HashSet<string>();
                foreach (var issue in release.CategorizedIssues.SelectMany(category => category.Issues))
                {
                    categorizedIssueIds.Add(issue.Id);
                    var removeItem = releaseIssues.FirstOrDefault(x => x.Id == issue.Id);
                    if (removeItem != null)
                        releaseIssues.Remove(removeItem);
                }
                
                var bugs = releaseIssues.Where(x => x.Type == "type/bug" || x.Type.InvariantContains("Feature") == false);
                if (bugs != null)
                {
                    var categoryBugFix = release.CategorizedIssues.FirstOrDefault(x => x.Name == "Bugfixes");
                    if (categoryBugFix != null)
                        categoryBugFix.Issues.AddRange(bugs);
                }

                var features = bugs != null ? releaseIssues.Except(bugs) : releaseIssues;
                if (features != null)
                {
                    var categoryOtherFeatures = release.CategorizedIssues.FirstOrDefault(x => x.Name == "Other features");
                    if (categoryOtherFeatures != null)
                        categoryOtherFeatures.Issues.AddRange(features);
                }
            }
        }

        private static List<Release.Category> GetDefaultCategories()
        {
            var categories = new List<Release.Category>
            {
                new Release.Category
                {
                    Name = "Notable features",
                    MatchingLabels = new List<string> {"category/notable"},
                    Issues = new List<Release.Issue>(),
                    Priority = -40
                },
                new Release.Category
                {
                    Name = "Other features",
                    MatchingLabels = new List<string>(),
                    Issues = new List<Release.Issue>(),
                    Priority = 100
                },
                new Release.Category
                {
                    Name = "Bugfixes",
                    MatchingLabels = new List<string>(),
                    Issues = new List<Release.Issue>(),
                    Priority = 110
                },
                new Release.Category
                {
                    Name = "Breaking changes",
                    MatchingLabels = new List<string> {"category/breaking"},
                    Issues = new List<Release.Issue>(),
                    Priority = -10
                },
                new Release.Category
                {
                    Name = "UI and UX updates",
                    MatchingLabels = new List<string> {"category/ui", "category/ux"},
                    Issues = new List<Release.Issue>()
                },
                new Release.Category
                {
                    Name = "API and API documentation updates",
                    MatchingLabels = new List<string> {"category/api", "category/api-documentation"},
                    Issues = new List<Release.Issue>()
                },
                new Release.Category
                {
                    Name = "Developer experience",
                    MatchingLabels = new List<string> {"category/dx"},
                    Issues = new List<Release.Issue>()
                }
            };
            return categories;
        }

        private static void AddCategoriesToItems(Release release, Issue item, Release.Issue issue)
        {
            var specialCategoryLabels = new List<string>();
            foreach (var issueCategory in release.CategorizedIssues)
            {
                specialCategoryLabels.AddRange(issueCategory.MatchingLabels);
                foreach (var itemLabel in item.Labels)
                {
                    if (issueCategory.MatchingLabels.Contains(itemLabel.Name) == false)
                        continue;

                    if (issueCategory.Issues.Contains(issue) == false)
                        issueCategory.Issues.Add(issue);
                }
            }

            foreach (var itemLabel in item.Labels)
            {
                var existingCategory = release.CategorizedIssues
                    .FirstOrDefault(x => x.MatchingLabels.Any(y => y == itemLabel.Name));
                if (existingCategory != null)
                {
                    // add to existing category
                    if (existingCategory.Issues.Contains(issue) == false)
                        existingCategory.Issues.Add(issue);
                }
                else
                {
                    // create new category and add the item
                    var categoryName = string.Empty;
                    var splitLabel = itemLabel.Name.Split('/');
                    // skip if it's not one of the categories, projects or a dependency
                    if (splitLabel.First() != "category" && splitLabel.First() != "project" &&
                        splitLabel.First() != "dependencies")
                        continue;

                    categoryName = splitLabel.Length > 1 ? splitLabel[1] : splitLabel[0];
                    categoryName = categoryName.Replace("angularjs", "AngularJS");
                    categoryName = categoryName.Replace("net-core", ".NET Core");
                    categoryName = categoryName.Replace("-", " ");
                    categoryName = categoryName.ToFirstUpper();
                    release.CategorizedIssues.Add(new Release.Category
                    {
                        Name = categoryName,
                        MatchingLabels = new List<string> {itemLabel.Name},
                        Issues = new List<Release.Issue> {issue}
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
