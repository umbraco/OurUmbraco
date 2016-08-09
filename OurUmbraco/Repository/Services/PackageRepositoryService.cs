using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Examine;
using Examine.LuceneEngine;
using Examine.SearchCriteria;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.MarketPlace.Providers;
using OurUmbraco.Our;
using OurUmbraco.Our.Examine;
using OurUmbraco.Our.Models;
using OurUmbraco.Project.Services;
using OurUmbraco.Repository.Controllers;
using OurUmbraco.Repository.Models;
using OurUmbraco.Wiki.BusinessLogic;
using umbraco;
using umbraco.MacroEngines;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Cache;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Security;

namespace OurUmbraco.Repository.Services
{
    internal class PackageRepositoryService
    {
        private readonly DatabaseContext DatabaseContext;
        private readonly MembershipHelper MembershipHelper;
        private readonly UmbracoHelper UmbracoHelper;

        const string BASE_URL = "https://our.umbraco.org";

        public PackageRepositoryService(UmbracoHelper umbracoHelper, MembershipHelper membershipHelper, DatabaseContext databaseContext)
        {
            UmbracoHelper = umbracoHelper;
            MembershipHelper = membershipHelper;
            DatabaseContext = databaseContext;
        }

        public IEnumerable<Models.Category> GetCategories()
        {
            var xpath = "//ProjectGroup[@isDoc]";
            var helper = new UmbracoHelper(UmbracoContext.Current);

            var items = helper.TypedContentAtXPath(xpath);

            return items
                .OrderBy(x => x.SortOrder)
                .Select(x => new Models.Category
                {
                    Icon = "",
                    Name = x.Name
                });
        }

        /// <summary>
        /// Returns a list of packages based on a search
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="category"></param>
        /// <param name="query"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        /// <remarks>
        /// This caches each query for 2 minutes (non-sliding)
        /// </remarks>
        public PagedPackages GetPackages(
            int pageIndex,
            int pageSize,
            string category = null,
            string query = null,
            PackageSortOrder order = PackageSortOrder.Default)
        {
            var filters = new List<SearchFilters>();
            var searchFilters = new SearchFilters(BooleanOperation.And);
            //MUST be approved and live
            searchFilters.Filters.Add(new SearchFilter("approved", "1"));
            searchFilters.Filters.Add(new SearchFilter("projectLive", "1"));

            query = string.IsNullOrWhiteSpace(query) ? string.Empty : query;

            var orderBy = string.Empty;
            switch (order)
            {
                case PackageSortOrder.Latest:
                    orderBy = "updateDate";
                    break;
                case PackageSortOrder.Popular:
                    orderBy = "popularity";
                    break;
            }

            var ourSearcher = new OurSearcher(query, nodeTypeAlias: "project", maxResults: pageSize * (pageIndex + 1), orderBy: orderBy, filters:filters);

            if (!string.IsNullOrWhiteSpace(category) || !string.IsNullOrWhiteSpace(query))
            {
                //Return based on a query
                if (!string.IsNullOrWhiteSpace(category))
                {
                    searchFilters.Filters.Add(new SearchFilter("categoryFolder", string.Format("\"{0}\"", category)));
                }
                filters.Add(searchFilters);

                ourSearcher.Filters = filters;
            }
            
            var searchResult = ourSearcher.Search("projectSearcher", skip: pageIndex * pageSize);
            return FromSearchResults(searchResult, pageIndex, pageSize);
        }

        private PagedPackages FromSearchResults(SearchResultModel searchResult, int pageIndex, int pageSize)
        {
            if (searchResult == null)
            {
                return null;
            }

            return new PagedPackages
            {
                Packages = searchResult.SearchResults
                    .Skip(pageIndex * pageSize)
                    .Select(x => UmbracoHelper.TypedContent(x.Id))
                    //TODO: This will cause strangeness with paging if someething is actually null
                    .WhereNotNull()
                    .Select(MapContentToPackage).ToArray(),
                Total = searchResult.SearchResults.TotalItemCount
            };
        }

        public PackageDetails GetDetails(Guid id)
        {
            // [LK:2016-06-13@CGRT16] We're using XPath as we experienced issues with query Examine for GUIDs,
            // (it might worth but we were up against the clock).
            // The XPath 'translate' is being used to force the 'packageGuid' to be lowercase for comparison.
            var xpath = string.Format("//Project[@isDoc and projectLive = 1 and approved = 1 and translate(packageGuid,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') = '{0}']", id.ToString("D").ToLowerInvariant());
            var item = UmbracoHelper.TypedContentSingleAtXPath(xpath);

            if (item == null)
                return null;

            return MapContentToPackageDetails(item);
        }

        private Models.Package MapContentToPackage(IPublishedContent content)
        {
            if (content == null)
                return null;

            var wikiFiles = WikiFile.CurrentFiles(content.Id);

            return new Models.Package
            {
                Category = content.Parent.Name,
                Created = content.CreateDate,
                Excerpt = GetPackageExcerpt(content, 12),
                Downloads = Utils.GetProjectTotalDownloadCount(content.Id),
                Id = content.GetPropertyValue<Guid>("packageGuid"),
                Likes = Utils.GetProjectTotalVotes(content.Id),
                Name = content.Name,
                Icon = GetThumbnailUrl(BASE_URL + content.GetPropertyValue<string>("defaultScreenshotPath", "/css/img/package2.png"), 154, 281),
                LatestVersion = content.GetPropertyValue<string>("version"),
                MinimumVersion = GetMinimumVersion(content.GetPropertyValue<int>("file"), wikiFiles.Where(x => x.FileType.InvariantEquals("package"))),
                OwnerInfo = GetPackageOwnerInfo(content.GetPropertyValue<int>("owner"), content.GetPropertyValue<bool>("openForCollab", false), content.Id),
                Url = string.Concat(BASE_URL, content.Url)
            };
        }
        
        private PackageDetails MapContentToPackageDetails(IPublishedContent content)
        {
            if (content == null)
                return null;

            var package = MapContentToPackage(content);
            var packageDetails = new PackageDetails(package);
            var wikiFiles = WikiFile.CurrentFiles(content.Id);

            packageDetails.Compatibility = GetPackageCompatibility(content);
            packageDetails.NetVersion = content.GetPropertyValue<string>("dotNetVersion");
            packageDetails.LicenseName = content.GetPropertyValue<string>("licenseName");
            packageDetails.LicenseUrl = content.GetPropertyValue<string>("licenseUrl");
            packageDetails.Description = content.GetPropertyValue<string>("description");
            packageDetails.Images = GetPackageImages(wikiFiles.Where(x => x.FileType.InvariantEquals("screenshot")), 154, 281);
            packageDetails.ExternalSources = GetExternalSources(content);
            packageDetails.ZipUrl = string.Concat(BASE_URL, "/FileDownload?id=", content.GetPropertyValue<string>("file"));

            return packageDetails;
        }

        private List<PackageCompatibility> GetPackageCompatibility(IPublishedContent content)
        {
            var service = new VersionCompatibilityService(DatabaseContext);
            var report = service.GetCompatibilityReport(content.Id);

            if (report == null || !report.Any())
                return null;

            return report
                .Select(x => new PackageCompatibility
                {
                    Percentage = x.Percentage,
                    Version = x.Version
                })
                .ToList();
        }

        private string GetMinimumVersion(int filePropertyValue, IEnumerable<WikiFile> packages)
        {
            var currentVersion = filePropertyValue;
            var latest = packages.FirstOrDefault(x => x.Id == currentVersion);

            if (latest == null || string.IsNullOrWhiteSpace(latest.Version.Version) || latest.Version.Version.InvariantEquals("nan"))
                return null;

            if (latest.Version.Version.InvariantStartsWith("v"))
            {
                var legacyFormat = latest.Version.Version;

                if (legacyFormat.InvariantStartsWith("v4"))
                {
                    return string.Concat(legacyFormat.Replace("v4", "4."), ".0");
                }
                else
                {
                    return string.Join(".", legacyFormat.ToCharArray().Skip(1));
                }
            }

            return latest.Version.Version;
        }

        private PackageOwnerInfo GetPackageOwnerInfo(int ownerId, bool openForCollab, int contentId)
        {
            var owner = MembershipHelper.GetById(ownerId);

            var ownerInfo = new PackageOwnerInfo
            {
                Karma = owner.Karma(),
                Owner = owner.Name,
                OwnerAvatar = Utils.GetMemberAvatar(owner, 200, true)
            };

            if (openForCollab)
            {
                var service = new ContributionService(DatabaseContext);
                var contributors = service.GetContributors(contentId).ToList();

                if (contributors != null && contributors.Any())
                {
                    var names = new List<string>();

                    foreach (var contributor in contributors)
                    {
                        var member = MembershipHelper.GetById(contributor.MemberId);
                        if (member != null)
                        {
                            names.Add(member.Name);
                        }
                    }

                    ownerInfo.Contributors = names.ToArray();
                }
            }

            return ownerInfo;
        }

        private List<PackageImage> GetPackageImages(IEnumerable<WikiFile> images, int thumbnailHeight, int thumbnailWidth)
        {
            var items = new List<PackageImage>();

            foreach (var image in images)
            {
                var url = string.Concat(BASE_URL, image.Path);

                items.Add(new PackageImage
                {
                    Source = url,
                    Thumbnail = GetThumbnailUrl(url, thumbnailHeight, thumbnailWidth)
                });
            }

            return items;
        }

        private string GetThumbnailUrl(string url, int height, int width)
        {
            return string.Format("{0}?bgcolor=fff&height={1}&width={2}&format=png", url, height, width);
        }

        private List<ExternalSource> GetExternalSources(IPublishedContent content)
        {
            var items = new List<ExternalSource>();

            if (content.HasValue("websiteUrl"))
            {
                items.Add(new ExternalSource
                {
                    Name = "Project website",
                    Url = content.GetPropertyValue<string>("websiteUrl")
                });
            }

            if (content.HasValue("sourceUrl"))
            {
                items.Add(new ExternalSource
                {
                    Name = "Source code",
                    Url = content.GetPropertyValue<string>("sourceUrl")
                });
            }

            if (content.HasValue("demoUrl"))
            {
                items.Add(new ExternalSource
                {
                    Name = "Demonstration",
                    Url = content.GetPropertyValue<string>("demoUrl")
                });
            }

            if (content.HasValue("supportUrl"))
            {
                items.Add(new ExternalSource
                {
                    Name = "Issue tracker",
                    Url = content.GetPropertyValue<string>("supportUrl")
                });
            }

            return items;
        }

        private string GetPackageExcerpt(IPublishedContent content, int count = 35)
        {
            var input = content.GetPropertyValue<string>("description");

            if (string.IsNullOrWhiteSpace(input))
                return null;

            var words = input
                .StripHtml()
                .StripNewLines()
                .Replace("&nbsp;", " ")
                .Split(new[] { ' ' })
                .Take(count)
                .ToList();

            return string.Concat(string.Join(" ", words), "...");
        }


    }
}