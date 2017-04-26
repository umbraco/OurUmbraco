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
using OurUmbraco.Project;
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
    public class PackageRepositoryService
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
        /// <param name="version"></param>
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
            string version = null,
            PackageSortOrder order = PackageSortOrder.Default)
        {
            var filters = new List<SearchFilters>();
            var searchFilters = new SearchFilters(BooleanOperation.And);
            //MUST be live
            searchFilters.Filters.Add(new SearchFilter("projectLive", "1"));
            filters.Add(searchFilters);
            if (version.IsNullOrWhiteSpace() == false)
            {
                //need to clean up this string, it could be all sorts of things
                var parsedVersion = version.GetFromUmbracoString();
                if (parsedVersion != null)
                {
                    var numericalVersion = parsedVersion.GetNumericalValue();
                    var versionFilters = new SearchFilters(BooleanOperation.Or);
                    versionFilters.Filters.Add(new RangeSearchFilter("num_version", 0, numericalVersion));
                    versionFilters.Filters.Add(new RangeSearchFilter("num_compatVersions", 0, numericalVersion));
                    filters.Add(versionFilters);
                }
            }

            query = string.IsNullOrWhiteSpace(query) ? string.Empty : query;

            var orderBy = string.Empty;
            switch (order)
            {
                case PackageSortOrder.Latest:
                    orderBy = "createDate[Type=LONG]";
                    break;
                case PackageSortOrder.Popular:
                    orderBy = "popularity[Type=INT]";
                    break;
            }
            
            //Return based on a query
            if (!string.IsNullOrWhiteSpace(category))
            {
                var catFilters = new SearchFilters(BooleanOperation.And);
                catFilters.Filters.Add(new SearchFilter("categoryFolder", string.Format("\"{0}\"", category)));
                filters.Add(catFilters);
            }

            var ourSearcher = new OurSearcher(query, nodeTypeAlias: "project", maxResults: pageSize * (pageIndex + 1), orderBy: orderBy, filters: filters);
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

        /// <summary>
        /// Returns the package details for the package Id passed in and ensures that 
        /// the resulting ZipUrl is the compatible package for the version passed in.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version">The umbraco version requesting the details, if null than the ZipUrl will be the latest package zip</param>
        /// <returns></returns>
        public PackageDetails GetDetails(Guid id, System.Version version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));

            // [LK:2016-06-13@CGRT16] We're using XPath as we experienced issues with query Examine for GUIDs,
            // (it might worth but we were up against the clock).
            // The XPath 'translate' is being used to force the 'packageGuid' to be lowercase for comparison.
            var xpath = string.Format("//Project[@isDoc and projectLive = 1 and translate(packageGuid,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') = '{0}']", id.ToString("D").ToLowerInvariant());
            var item = UmbracoHelper.TypedContentSingleAtXPath(xpath);
            
            if (item == null)
                return null;

            return MapContentToPackageDetails(item, version);
        }

        private Models.Package MapContentToPackage(IPublishedContent content)
        {
            if (content == null)
                return null;
            
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
                OwnerInfo = GetPackageOwnerInfo(content.GetPropertyValue<int>("owner"), content.GetPropertyValue<bool>("openForCollab", false), content.Id),
                Url = string.Concat(BASE_URL, content.Url)
            };
        }
        
        private PackageDetails MapContentToPackageDetails(IPublishedContent content, System.Version currentUmbracoVersion)
        {            
            if (currentUmbracoVersion == null) throw new ArgumentNullException(nameof(currentUmbracoVersion));
            if (content == null) return null;

            var package = MapContentToPackage(content);

            var wikiFiles = WikiFile.CurrentFiles(content.Id);

            //TODO: SD: I dunno where these come from or if we care about it?
            //var deliCompatVersions = Utils.GetProjectCompatibleVersions(content.Id) ?? new List<string>();

            var allPackageFiles = wikiFiles.Where(x => x.FileType.InvariantEquals("package")).ToArray();

            //get the strict packages in the correct desc order
            var strictPackageFileVersions = GetAllStrictSupportedPackageVersions(allPackageFiles).ToArray();

            var packageDetails = new PackageDetails(package)
            {
                TargetedUmbracoVersions = GetAllFilePackageVersions(allPackageFiles).Select(x => x.ToString(3)).ToArray(),
                Compatibility = GetPackageCompatibility(content),
                NetVersion = content.GetPropertyValue<string>("dotNetVersion"),
                LicenseName = content.GetPropertyValue<string>("licenseName"),
                LicenseUrl = content.GetPropertyValue<string>("licenseUrl"),
                Description = content.GetPropertyValue<string>("description").CleanHtmlAttributes(),
                Images = GetPackageImages(wikiFiles.Where(x => x.FileType.InvariantEquals("screenshot")), 154, 281),
                ExternalSources = GetExternalSources(content)
            };   

            var version75 = new System.Version(7, 5, 0);

            //this is the file marked as the current/latest release
            var currentReleaseFile = content.GetPropertyValue<int>("file");

            if (strictPackageFileVersions.Length == 0)
            {
                //if there are no strict package files then return the latest package file
                
                packageDetails.ZipUrl = string.Concat(BASE_URL, "/FileDownload?id=", currentReleaseFile);
                packageDetails.ZipFileId = currentReleaseFile;
            }
            else if (currentUmbracoVersion < version75)
            {
                //if the umbraco version is < 7.5 it means that strict package formats are not supported

                //TODO: Now we have to do the opposite of below and filter out any package file versions that have strict
                // umbraco dependencies applied. Anything that has 7.5 (which would be the very minimum strict dependency) we can check for

                //these are ordered by package version desc
                var nonStrictPackageFiles = GetNonStrictSupportedPackageVersions(wikiFiles).ToArray();

                if (nonStrictPackageFiles.Length == 0)
                {
                    //Looks like there's nothing compatible 
                    return null;
                }

                //there might be a case where the 'current release file' is not the latest version found, so let's check if
                //the latest release file is included in the non-strict packages and if so we'll use that, otherwise we'll use the latest
                var found = nonStrictPackageFiles.FirstOrDefault(x => x.PackageId == currentReleaseFile);
                if (found != null)
                {
                    //it's included in the non strict packages so use it
                    packageDetails.ZipUrl = string.Concat(BASE_URL, "/FileDownload?id=", currentReleaseFile);
                    packageDetails.ZipFileId = currentReleaseFile;
                }
                else
                {
                    //use the latest available package version
                    packageDetails.ZipUrl = string.Concat(BASE_URL, "/FileDownload?id=", nonStrictPackageFiles[0].PackageId);
                    packageDetails.ZipFileId = nonStrictPackageFiles[0].PackageId;
                }
            }
            else
            {
                //this package has some strict version dependency files, so we need to figure out which one is 
                // compatible with the current umbraco version passed in and also use the latest available 
                // package version that is compatible.

                int found = -1;
                foreach (var pckVersion in strictPackageFileVersions)
                {
                    //if this version will work with the umbraco version, then use it
                    if (currentUmbracoVersion >= pckVersion.MinUmbracoVersion)
                    {
                        found = pckVersion.PackageId;
                        break;
                    }
                }

                if (found != -1)
                {
                    //got one! so use it's id for the file download
                    packageDetails.ZipUrl = string.Concat(BASE_URL, "/FileDownload?id=", found);
                    packageDetails.ZipFileId = found;
                }
                else
                {
                    //not compatible!
                    return null;
                }
            }
            
            packageDetails.Created = content.CreateDate;

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

        /// <summary>
        /// Based on all of the files that this package go retrieve all targeted Umbraco versions
        /// </summary>
        /// <param name="packages"></param>
        /// <returns></returns>
        private IEnumerable<System.Version> GetAllFilePackageVersions(IEnumerable<WikiFile> packages)
        {
            var allVersions = packages
                .Select(x => ConvertToVersion(x.Version.Version))
                .WhereNotNull()
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            return allVersions;
        }

        /// <summary>
        /// Based on all of the files that this package go retrieve all targeted Umbraco versions
        /// </summary>
        /// <param name="packages"></param>
        /// <returns>
        /// Order from latest package version and lastest min umb version and start from the top, we want to return the most recent 
        /// available package version for the current Umbraco version
        /// </returns>
        private IEnumerable<PackageVersionSupport> GetAllStrictSupportedPackageVersions(IEnumerable<WikiFile> packages)
        {
            var allVersions = packages
                .Where(x => x.MinimumVersionStrict.IsNullOrWhiteSpace() == false)
                .Select(x => new PackageVersionSupport(x.Id, ConvertToVersion(x.Version.Version), ConvertToVersion(x.MinimumVersionStrict)))
                .Where(x => x.PackageVersion != null && x.MinUmbracoVersion != null)
                .OrderByDescending(x => x.PackageVersion)
                .ThenByDescending(x => x.MinUmbracoVersion)
                .ToArray();
            return allVersions;
        }

        /// <summary>
        /// Based on all of the files that this package go retrieve all non targeted Umbraco versions
        /// </summary>
        /// <param name="packages"></param>
        /// <returns>
        /// Order from latest package version and start from the top, we want to return the most recent 
        /// available package version for the current Umbraco version
        /// </returns>
        private IEnumerable<PackageVersionSupport> GetNonStrictSupportedPackageVersions(IEnumerable<WikiFile> packages)
        {
            var allVersions = packages
                .Where(x => x.MinimumVersionStrict.IsNullOrWhiteSpace())
                .Select(x => new PackageVersionSupport(x.Id, ConvertToVersion(x.Version.Version), null))
                .Where(x => x.PackageVersion != null)
                .OrderByDescending(x => x.PackageVersion)
                .ToArray();
            return allVersions;
        }

        private class PackageVersionSupport : IEquatable<PackageVersionSupport>
        {
            private readonly int _packageId;

            public PackageVersionSupport(int packageId, System.Version packageVersion, System.Version minUmbracoVersion)
            {
                _packageId = packageId;
                MinUmbracoVersion = minUmbracoVersion;
                PackageVersion = packageVersion;
            }

            public int PackageId
            {
                get { return _packageId; }
            }

            public System.Version MinUmbracoVersion { get; private set; }
            public System.Version PackageVersion { get; private set; }

            public bool Equals(PackageVersionSupport other)
            {
                return _packageId == other._packageId;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is PackageVersionSupport && Equals((PackageVersionSupport) obj);
            }

            public override int GetHashCode()
            {
                return _packageId;
            }

            public static bool operator ==(PackageVersionSupport left, PackageVersionSupport right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(PackageVersionSupport left, PackageVersionSupport right)
            {
                return !left.Equals(right);
            }
        }

        /// <summary>
        /// Try to convert the string to a real version based on legacy version formats
        /// </summary>
        /// <param name="ver"></param>
        /// <returns></returns>
        private System.Version ConvertToVersion(string ver)
        {
            string normalized = ver;
            if (ver.InvariantStartsWith("v"))
            {
                if (ver.InvariantStartsWith("v4"))
                {
                    normalized = string.Concat(ver.Replace("v4", "4."), ".0");
                }
                else
                {
                    normalized = string.Join(".", ver.ToCharArray().Skip(1));
                }
            }

            if (normalized.EndsWith(".x"))
            {
                normalized = normalized.TrimEnd(".x").EnsureEndsWith(".0");
            }

            System.Version result;
            if (System.Version.TryParse(normalized, out result))
            {
                return result;
            }
            return null;
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