using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Examine;
using Examine.SearchCriteria;
using GraphQL;
using OurUmbraco.Community.Nuget;
using OurUmbraco.Community.People;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.Our;
using OurUmbraco.Our.Examine;
using OurUmbraco.Our.Models;
using OurUmbraco.Project;
using OurUmbraco.Project.Services;
using OurUmbraco.Repository.Controllers;
using OurUmbraco.Repository.Models;
using OurUmbraco.Wiki.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace OurUmbraco.Repository.Services
{
    public class PackageRepositoryService
    {
        private readonly DatabaseContext DatabaseContext;
        private readonly MembershipHelper MembershipHelper;
        private readonly UmbracoHelper UmbracoHelper;

        string _baseUrl = ConfigurationManager.AppSettings["PackagesBaseUrl"] ?? "https://our.umbraco.com";
        

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
        /// <param name="includeHidden">Some packages are hidden (i.e. projectLive), set to true to ignore this switch (i.e. for starter kits)</param>
        /// <param name="onlyPromoted">When flag set only promoted packages are returned.</param>
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
            PackageSortOrder order = PackageSortOrder.Default,
            bool? includeHidden = false,
            bool? onlyPromoted = false)
        {
            var filters = new List<SearchFilters>();
            var searchFilters = new SearchFilters(BooleanOperation.And);

            if (includeHidden == false)
            {
                //MUST be live
                searchFilters.Filters.Add(new SearchFilter("projectLive", "1"));
                searchFilters.Filters.Add(new SearchFilter("isRetired", "0"));
            }

            if (onlyPromoted.HasValue && onlyPromoted.Value)
            {
                searchFilters.Filters.Add(new SearchFilter("isPromoted", "1"));
            }

            if (version.IsNullOrWhiteSpace() == false)
            {
                //need to clean up this string, it could be all sorts of things
                var parsedVersion = version.GetFromUmbracoString(reduceToConfigured: false);
                if (parsedVersion != null)
                {
                    // As of version 9, there will no longer be package files - those are on NuGet.org only so don't check file compatibility
                    if (parsedVersion.Major >= 9)
                    {
                        searchFilters.Filters.Add(new SearchFilter("isNuGetFormat", "1"));
                    }
                    else
                    {
                        var numericalVersion = parsedVersion.GetNumericalValue();
                        var versionFilters = new SearchFilters(BooleanOperation.Or);

                        //search for all versions from the current major to the version passed in
                        var currMajor = new System.Version(parsedVersion.Major, 0, 0).GetNumericalValue();

                        versionFilters.Filters.Add(
                            new RangeSearchFilter("num_version", currMajor, numericalVersion));
                        filters.Add(versionFilters);
                    }
                }
            }
            
            filters.Add(searchFilters);
            
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
                case PackageSortOrder.Default:
                    orderBy = "updateDate[Type=LONG]";
                    break;
                case PackageSortOrder.Downloads:
                    orderBy = "downloads[Type=INT]";
                    break;
            }
            
            //Return based on a query
            if (!string.IsNullOrWhiteSpace(category))
            {
                var catFilters = new SearchFilters(BooleanOperation.And);
                if (category.InvariantEquals("uaas"))
                {
                    catFilters.Filters.Add(new SearchFilter("worksOnUaaS", string.Format("\"{0}\"", "True")));
                }
                else
                {
                    catFilters.Filters.Add(new SearchFilter("categoryFolder", string.Format("\"{0}\"", category)));
                }
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
                    .Select(MapContentToPackage)
                    //TODO: This will cause strangeness with paging if someething is actually null
                    .WhereNotNull(),
                
                Pages = (searchResult.SearchResults.TotalItemCount / pageSize) + 1,
                Total = searchResult.SearchResults.TotalItemCount
            };
        }

        /// <summary>
        /// Returns the package details for the package Id passed in and ensures that 
        /// the resulting ZipUrl is the compatible package for the version passed in.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version">The umbraco version requesting the details, if null than the ZipUrl will be the latest package zip</param>
        /// <param name="includeHidden">Some packages are hidden (i.e. projectLive), set to true to ignore this switch (i.e. for starter kits)</param>
        /// <returns>
        /// If the current umbraco version is not compatible with any package files, the ZipUrl and ZipFileId will be empty
        /// </returns>
        public PackageDetails GetDetails(Guid id, System.Version version, bool includeHidden)
        {
            if (version == null) throw new ArgumentNullException("version");

            // [LK:2016-06-13@CGRT16] We're using XPath as we experienced issues with query Examine for GUIDs,
            // (it might worth but we were up against the clock).
            // The XPath 'translate' is being used to force the 'packageGuid' to be lowercase for comparison.
            var xpath = includeHidden
                ? string.Format("//Project[@isDoc and translate(packageGuid,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') = '{0}']", id.ToString("D").ToLowerInvariant())
                : string.Format("//Project[@isDoc and projectLive = 1 and translate(packageGuid,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') = '{0}']", id.ToString("D").ToLowerInvariant());
            var item = UmbracoHelper.TypedContentSingleAtXPath(xpath);
            
            if (item == null)
                return null;

            return MapContentToPackageDetails(item, version);
        }

        private Models.Package MapContentToPackage(SearchResult result)
        {
            if (result == null) return null;

            var content = UmbracoHelper.TypedContent(result.Id);
            if (content == null) return null;

            return MapContentToPackage(content, result);
        }

        private Models.Package MapContentToPackage(IPublishedContent content, SearchResult result)
        {
            if (content == null)
                return null;

            var ownerId = content != null & content.HasProperty("owner") ? content.GetPropertyValue<int>("owner") : 0;
            var openForCollab = content != null && content.HasProperty("openForCollab") ? content.GetPropertyValue<bool>("openForCollab", false) : false;

            var score = result != null ? GetScore(result.Fields) : 0;
            var version = result != null ? ParseVersion(result) : "";
            var downloads = result != null ? GetCombinedDownloads(result.Fields, Utils.GetProjectTotalDownloadCount(content.Id)) : Utils.GetProjectTotalDownloadCount(content.Id);
            var nugetService = new NugetPackageDownloadService();
            var nuGetPackageId = nugetService.GetNuGetPackageId(content);
            
            return new Models.Package
            {
                Category = content.Parent.Name,
                Created = content.CreateDate,
                Excerpt = GetPackageExcerpt(content, 12),
                Downloads = downloads,
                Id = content.GetPropertyValue<Guid>("packageGuid"),
                Likes = Utils.GetProjectTotalVotes(content.Id),
                Name = content.Name,
                Icon = GetThumbnailUrl(_baseUrl + content.GetPropertyValue<string>("defaultScreenshotPath", "/css/img/package2.png"), 154, 281),
                LatestVersion = content.GetPropertyValue<string>("version"),
                OwnerInfo = ownerId != 0 ? GetPackageOwnerInfo(ownerId, openForCollab, content.Id) : new PackageOwnerInfo(),
                Url = string.Concat(_baseUrl, content.Url),
                // stuff added to combine the search between our.umbraco.com and the backoffice
                Score = score,
                VersionRange = version,
                Image = _baseUrl + content.GetPropertyValue<string>("defaultScreenshotPath", "/css/img/package2.png"),
                Summary = GetPackageSummary(content, 50),
                CertifiedToWorkOnUmbracoCloud = content.GetPropertyValue<bool>("worksOnUaaS"),
                NuGetPackageId = nuGetPackageId,
                IsNuGetFormat =  content.GetPropertyValue<bool>("isNuGetFormat"),
                IsPromoted = content.GetPropertyValue<bool>("isPromoted")
            };
        }


        /// <summary>
        ///  returns the popularity score for a project
        /// </summary>
        private long GetScore(IDictionary<string,string> fields)
            => GetFieldValue(fields, "popularity", 0);

        private int GetCombinedDownloads(IDictionary<string, string> fields, int defaultValue)
            => GetFieldValue(fields, "downloads", defaultValue);

        /// <summary>
        ///  get a value from the search fields, and convert it to the required type
        /// </summary>
        /// <typeparam name="TObject">Type to convert to</typeparam>
        /// <param name="fields">Collection of fields</param>
        /// <param name="key">Key in collection</param>
        /// <param name="defaultValue">Default value to return if item is missing or not a valid value</param>
        /// <returns>the value converted to TObject or the defaultValue</returns>
        public TObject GetFieldValue<TObject>(IDictionary<string, string> fields, string key, TObject defaultValue)
        {
            if (fields != null && fields.ContainsKey(key))
            {
                var value = fields[key];

                var attempt = value.TryConvertTo<TObject>();
                if (attempt.Success)
                    return attempt.Result;
            }

            return defaultValue;
        }


        /// <summary>
        ///  calculates the version range a package will work on
        /// </summary>
        /// <remarks>
        ///  Moved from the partial view (listprojects) - so we can use one search
        /// </remarks>
        public string ParseVersion(SearchResult result)
        {
            if (result == null) return string.Empty;

            var versions = result.GetValues("versions").ToList();
            if (result.Fields.Keys.Contains("versions"))
            {
                versions.Add(result["versions"]);
            }

            var orderedVersions = versions
                .Select(x =>
                {
                    System.Version v;
                    return System.Version.TryParse(x, out v) ? v : null;
                }).WhereNotNull()
                .OrderByDescending(x => x)
                .ToArray();
            
            var umbHelper = new UmbracoHelper(UmbracoContext.Current);
            var node = umbHelper.TypedContent(result.Id);
            var isVersion9 = node.GetPropertyValue<bool>("isNuGetFormat");
            
            if (orderedVersions.Any() == false)
            {
                return isVersion9 ? "9+" : "n/a";
            }

            if (orderedVersions.Length == 1)
                return orderedVersions.First().ToString();

            if (orderedVersions.Min() == orderedVersions.Max())
                return orderedVersions.Min().ToString();

            if (isVersion9)
            {
                return orderedVersions.Min() + " - 9+";
            }

            return orderedVersions.Min() + " - " + orderedVersions.Max();
        }

        /// <summary>
        /// Returns a PackageDetails instance
        /// </summary>
        /// <param name="content"></param>
        /// <param name="currentUmbracoVersion"></param>
        /// <returns>
        /// If the current umbraco version is not compatible with any package files, the ZipUrl and ZipFileId will be empty
        /// </returns>
        private PackageDetails MapContentToPackageDetails(IPublishedContent content, System.Version currentUmbracoVersion)
        {            
            if (currentUmbracoVersion == null) throw new ArgumentNullException("currentUmbracoVersion");
            if (content == null) return null;

            var package = MapContentToPackage(content, null);
            if (package == null)
                return null;

            var wikiFiles = WikiFile.CurrentFiles(content.Id);

            //TODO: SD: I dunno where these come from or if we care about it?
            //var deliCompatVersions = Utils.GetProjectCompatibleVersions(content.Id) ?? new List<string>();

            var allPackageFiles = wikiFiles.Where(x => x.FileType.InvariantEquals("package") && x.Archived == false).ToArray();

            //get the strict packages in the correct desc order
            var strictPackageFileVersions = GetAllStrictSupportedPackageVersions(allPackageFiles).ToArray();
            //these are ordered by package version desc
            var nonStrictPackageFiles = GetNonStrictSupportedPackageVersions(allPackageFiles).ToArray();
            var nugetService = new NugetPackageDownloadService();
            var nuGetPackageId = nugetService.GetNuGetPackageId(content);
            
            var packageDetails = new PackageDetails(package)
            {
                TargetedUmbracoVersions = GetAllFilePackageVersions(allPackageFiles).Select(x => {
                    //ensure the version has consistent parts (major.minor.build)
                    var version = x.Build >= 0 ? x : new System.Version(x.Major, x.Minor, 0);
                    return version.ToString(3);
                }).ToArray(),
                Compatibility = GetPackageCompatibility(content),
                StrictFileVersions = strictPackageFileVersions.Select(x => new PackageFileVersion
                {
                    PackageVersion = x.PackageVersion.Build >= 0 
                        ? x.PackageVersion.ToString(3) 
                        : new System.Version(x.PackageVersion.Major, x.PackageVersion.Minor, 0).ToString(3),
                    MinUmbracoVersion = x.MinUmbracoVersion.Build >= 0 
                        ? x.MinUmbracoVersion.ToString(3) 
                        : new System.Version(x.MinUmbracoVersion.Major, x.MinUmbracoVersion.Minor, 0).ToString(3),
                    FileId = x.FileId
                }).ToList(),
                NetVersion = content.GetPropertyValue<string>("dotNetVersion"),
                LicenseName = content.GetPropertyValue<string>("licenseName"),
                LicenseUrl = content.GetPropertyValue<string>("licenseUrl"),
                Description = content.GetPropertyValue<string>("description").CleanHtmlAttributes(),
                Images = GetPackageImages(wikiFiles.Where(x => x.FileType.InvariantEquals("screenshot")), 154, 281),
                ExternalSources = GetExternalSources(content),
                NuGetPackageId = nuGetPackageId,
                IsNuGetFormat =  content.GetPropertyValue<bool>("isNuGetFormat")
            };   

            var version75 = new System.Version(7, 5, 0);

            //this is the file marked as the current/latest release
            var currentReleaseFile = content.GetPropertyValue<int>("file");

            if (strictPackageFileVersions.Length == 0)
            {
                //if there are no strict package files then return the latest package file
                
                packageDetails.ZipUrl = string.Concat(_baseUrl, "/FileDownload?id=", currentReleaseFile);
                packageDetails.ZipFileId = currentReleaseFile;
            }
            else if (currentUmbracoVersion < version75)
            {
                //if the umbraco version is < 7.5 it means that strict package formats are not supported
                AssignLatestNonStrictPackageFile(nonStrictPackageFiles, currentReleaseFile, packageDetails);                
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
                        found = pckVersion.FileId;
                        break;
                    }
                }

                if (found != -1)
                {
                    //got one! so use it's id for the file download
                    packageDetails.ZipUrl = string.Concat(_baseUrl, "/FileDownload?id=", found);
                    packageDetails.ZipFileId = found;
                }
                else if (nonStrictPackageFiles.Length > 0)
                {
                    //Here's the other case, if this package has both strict and non-strict package file versions and we didn't find one above, 
                    //than we need to determine if the latest non-strict package file format should be used for the current version being passed in

                    AssignLatestNonStrictPackageFile(nonStrictPackageFiles, currentReleaseFile, packageDetails);
                }
                
            }
            
            packageDetails.Created = content.CreateDate;

            return packageDetails;
        }

        private void AssignLatestNonStrictPackageFile(PackageVersionSupport[] nonStrictPackageFiles, int currentReleaseFile, PackageDetails packageDetails)
        {
            if (nonStrictPackageFiles.Length > 0)
            {
                //there might be a case where the 'current release file' is not the latest version found, so let's check if
                //the latest release file is included in the non-strict packages and if so we'll use that, otherwise we'll use the latest
                var found = nonStrictPackageFiles.FirstOrDefault(x => x.FileId == currentReleaseFile);
                if (found != null)
                {
                    //it's included in the non strict packages so use it
                    packageDetails.ZipUrl = string.Concat(_baseUrl, "/FileDownload?id=", currentReleaseFile);
                    packageDetails.ZipFileId = currentReleaseFile;
                }
                else
                {
                    //use the latest available package version
                    packageDetails.ZipUrl = string.Concat(_baseUrl, "/FileDownload?id=", nonStrictPackageFiles[0].FileId);
                    packageDetails.ZipFileId = nonStrictPackageFiles[0].FileId;
                }
            }
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
                .Select(x => x.Version.Version.GetFromUmbracoString(reduceToConfigured: false))
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
        internal static IEnumerable<PackageVersionSupport> GetAllStrictSupportedPackageVersions(IEnumerable<WikiFile> packages)
        {
            var allVersions = packages
                .Where(x => x.MinimumVersionStrict.IsNullOrWhiteSpace() == false)
                .Select(x => new PackageVersionSupport(x.Id, x.Version.Version.GetFromUmbracoString(reduceToConfigured: false), x.MinimumVersionStrict.GetFromUmbracoString(reduceToConfigured: false)))
                .Where(x => x.PackageVersion != null && x.MinUmbracoVersion != null)
                .OrderByDescending(x => x.PackageVersion)
                .ThenByDescending(x => x.MinUmbracoVersion)
                //need to sort by latest file Id too since multiple files can target the same umbraco versions but we want to take the latest uploaded file
                .ThenByDescending(x => x.FileId)
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
                .Select(x => new PackageVersionSupport(x.Id, x.Version.Version.GetFromUmbracoString(reduceToConfigured:false), null))
                .Where(x => x.PackageVersion != null)
                .OrderByDescending(x => x.PackageVersion)
                //need to sort by latest file Id too since multiple files can target the same umbraco versions but we want to take the latest uploaded file
                .ThenByDescending(x => x.FileId)
                .ToArray();
            return allVersions;
        }     

        private PackageOwnerInfo GetPackageOwnerInfo(int ownerId, bool openForCollab, int contentId)
        {
            var owner = MembershipHelper.GetById(ownerId);
            var avatarService = new AvatarService();
            var avatarPath = avatarService.GetMemberAvatar(owner);
            var avatar = $"{avatarPath}?width=200&height=200&mode=crop&upscale=true";
            
            var ownerInfo = new PackageOwnerInfo
            {
                Karma = owner?.Karma() ?? 0,
                Owner = owner?.Name ?? "",
                OwnerAvatar = avatar
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
                var url = string.Concat(_baseUrl, image.Path);

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

        /// <summary>
        ///  returns the short summary line that is shown on our.umbraco.com
        /// </summary>
        private string GetPackageSummary(IPublishedContent content, int count = 50)
        {
            var input = content.GetPropertyValue<string>("description");
            return input.StripHtml().Truncate(count);
        }

    }
}