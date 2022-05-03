using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using Newtonsoft.Json;
using OurUmbraco.Community.GitHub;
using OurUmbraco.Community.Nuget;
using OurUmbraco.Our.Services;
using OurUmbraco.Wiki.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class OurCommunityStatisticsController : UmbracoAuthorizedApiController
    {
        [System.Web.Http.HttpGet]
        public List<string> GetPullRequestStats(string startDate, string endDate)
        {
            if (DateTime.TryParse(startDate, out var start) == false)
                return null;

            if (DateTime.TryParse(endDate, out var end) == false)
                return null;

            var repoManagementService = new RepositoryManagementService();
            var repositories = repoManagementService.GetAllPublicRepositories().Where(x => x.InDashboard);

            var githubService = new GitHubService();
            var contributors = new HashSet<string>();
            foreach (var repository in repositories)
            {
                var allPulls = githubService
                    .GetExistingPullsFromDisk(repository.Alias)
                    .Where(x => x != null && x.IsPr && x.CreateDateTime >= start && x.CreateDateTime < end);

                foreach (var pull in allPulls)
                {
                    contributors.Add(pull.User.Login);
                }
            }
            
            var usersService = new UsersService();
            var hqList = usersService.GetIgnoredGitHubUsers().Result.ToArray();

            var contribFiltered = new List<string>();
            foreach (var contributor in contributors)
            {
                if (hqList.Any(x => 
                        string.Equals(x, contributor, StringComparison.InvariantCultureIgnoreCase)) == false)
                    contribFiltered.Add(contributor);
            }

            return contribFiltered;
        }
        
        [System.Web.Http.HttpGet]
        public PackageStats GetPackageStats(string startDate, string endDate)
        {  
            if (DateTime.TryParse(startDate, out var start) == false)
                return null;

            if (DateTime.TryParse(endDate, out var end) == false)
                return null;
            
            var packageStats = new PackageStats
            {
                Created = new HashSet<string>(), 
                Updated = new HashSet<string>(),
                All = new HashSet<string>(),
                CreatorNames = new HashSet<string>(),
                ContributorNames = new HashSet<string>()
            };

            var contributorNames = new HashSet<string>();
            
            var allPackages = GetAllPackages();

            var createdPackages = allPackages
                .Where(x => x.CreateDate >= start && x.CreateDate <= end)
                .ToList();

            foreach (var package in createdPackages)
            {
                packageStats.CreatorNames.Add(GetOwnerName(package));

                foreach (var contributorName in GetContributorNames(package))
                    contributorNames.Add(contributorName);
            }

            var updatedPackages = new List<IPublishedContent>();
            foreach (var package in allPackages)
            {
                var wikiFiles = WikiFile.CurrentFiles(package.Id);
                
                if (wikiFiles.Any(x => x.CreateDate >= start && x.CreateDate < end) == false) continue;
                if (createdPackages.Any(x => x.Id == package.Id)) continue;
                
                updatedPackages.Add(package);
                
                packageStats.CreatorNames.Add(GetOwnerName(package));

                foreach (var contributorName in GetContributorNames(package))
                    contributorNames.Add(contributorName);
            }

            foreach (var package in createdPackages)
            {
                packageStats.Created.Add(package.Name);
            }
            
            foreach (var package in updatedPackages)
            {
                packageStats.Updated.Add(package.Name);
            }

            foreach (var package in allPackages)
            {
                packageStats.All.Add(package.Name);
            }
        

            foreach (var name in contributorNames)
            {
                if (packageStats.CreatorNames.Contains(name) == false)
                    packageStats.ContributorNames.Add(name);
            }
            
            return packageStats;
        }

        private List<IPublishedContent> GetAllPackages()
        {
            var packagesRoot = Umbraco.TypedContentAtRoot()
                .First()
                .Children.FirstOrDefault(x =>
                    string.Equals(x.DocumentTypeAlias, "Projects", StringComparison.InvariantCultureIgnoreCase));

            return packagesRoot?.Descendants()
                .Where(x => string.Equals(x.DocumentTypeAlias, "Project", StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }

        [System.Web.Http.HttpGet]
        public List<string> GetPackagesByUser(int ownerId)
        {
            var queryResults = GetPackageIdsByOwnerId(ownerId);

            var results = new HashSet<string>();
            foreach (var id in queryResults)
            {
                var node = Umbraco.TypedContent(id);
                if (node != null)
                {
                    var name = node.Name;
                    var url = string.Empty;
                    if (node.Url != null)
                        url = node.Url;
                    results.Add($"{name} - {url}");
                }
            }
            
            return results.ToList();
        }
        
        [System.Web.Http.HttpGet]
        public string UnpublishPackagesByUser(int ownerId)
        {
            var queryResults = GetPackageIdsByOwnerId(ownerId);

            var contentService = ApplicationContext.Services.ContentService;

            foreach (var id in queryResults)
            {
                contentService.UnPublish(contentService.GetById(id));
            }
            
            return "OK";
        }

        private List<int> GetPackageIdsByOwnerId(int ownerId)
        {
            const string packagesSql =
                @"SELECT id FROM umbracoNode WHERE id IN (
	SELECT DISTINCT contentNodeId
	FROM cmsPropertyData
	WHERE dataInt = @ownerId AND propertyTypeId IN (
	  SELECT id FROM cmsPropertyType
	  WHERE alias = 'owner' AND contentTypeId IN (
		  SELECT nodeId
		  FROM cmsContentType
		  WHERE alias = 'project'
	  )
	)
)";
            var queryResults = ApplicationContext.DatabaseContext.Database.Fetch<int>(packagesSql,
                new
                {
                    ownerId = ownerId,
                });
            return queryResults;
        }

        private IEnumerable<string> GetContributorNames(IPublishedContent package)
        {
            var contributors = Utils.GetProjectContributors(package.Id);
            var names = new HashSet<string>();
            foreach (var contributor in contributors)
            {
                var contributorName = Members.GetById(contributor).Name;
                names.Add(contributorName);
            }

            return names;
        }

        private string GetOwnerName(IPublishedContent package)
        {
            var owner = package.GetPropertyValue<int>("owner");
            var ownerName = Members.GetById(owner).Name;
            return ownerName;
        }
    }

    public class PackageStats
    {
        public HashSet<string> Created { get; set; }
        public HashSet<string> Updated { get; set; }
        public HashSet<string> All { get; set; }
        public HashSet<string> CreatorNames { get; set; }
        public HashSet<string> ContributorNames { get; set; }
    }
}