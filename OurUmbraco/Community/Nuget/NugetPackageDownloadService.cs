namespace OurUmbraco.Community.Nuget
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Hosting;
    using Examine;
    using Newtonsoft.Json;

    using RestSharp;

    using Umbraco.Core;
    using Umbraco.Core.Configuration;
    using Umbraco.Core.Models;
    using Umbraco.Web;
    using Umbraco.Web.Routing;
    using Umbraco.Web.Security;

    using File = System.IO.File;

    /// <summary>
    /// Represents nuget package download service.
    /// </summary>
    public class NugetPackageDownloadService
    {
        private  string _nugetServiceUrl = "https://api.nuget.org/v3/index.json";

        private  string _storageDirectory = HostingEnvironment.MapPath("~/App_Data/TEMP/NugetDownloads");

        private string _downloadsFile = "downloads.json";

        public void ImportNugetPackageDownloads()
        {
            // get all packages that have a nuget url specified
            var umbContxt = EnsureUmbracoContext();

            var projects = umbContxt.ContentCache.GetByXPath("//Community/Projects//Project [nuGetPackageUrl!='']").ToList();

            if (projects.Any())
            {
                var searchUrl = GetNugetService("SearchQueryService");
                if (!string.IsNullOrWhiteSpace(searchUrl))
                {
                    var nugetPackageDownloads = new List<NugetPackageInfo>();

                    // we will loop trough our projects in groups of 5 so we can query for multiple packages at ones
                    // the nuget api has a rate limit, so to avoid hitting that we query multiple packages at once

                    foreach (var projectGroup in projects.InGroupsOf(5))
                    {
                        var packageQuery = GetNugetPackageQuery(projectGroup);

                        if (!string.IsNullOrWhiteSpace(packageQuery))
                        {
                            var searchQuery = $"{searchUrl}?q={packageQuery.TrimEnd("+")}&prerelease=true";
                            var packageSearchResult = GetNugetSearchResults(searchQuery);

                            if (packageSearchResult != null)
                            {
                                foreach (var package in packageSearchResult.Results)
                                {
                                    var packageInfo = GetNugetPackageInfo(package);
                                    if (packageInfo != null)
                                    {
                                        nugetPackageDownloads.Add(packageInfo);
                                    }
                                }
                            }
                        }
                    }

                    // store downloads if any
                    if (nugetPackageDownloads.Any())
                    {
                        SavePackageInfo(nugetPackageDownloads);

                        
                    }

                }
            }
        }

        /// <summary>
        ///  calculate the nuget query for a group of packages (combines them +packageId:name1+packageId:name2}
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        private string GetNugetPackageQuery(IEnumerable<IPublishedContent> projects)
        {
            var packageIds = new List<string>();
            foreach (var project in projects)
            {
                var nuGetPackageCmd = GetNuGetPackageId(project);
                if (!string.IsNullOrWhiteSpace(nuGetPackageCmd))
                {
                    packageIds.Add($"packageid:{nuGetPackageCmd}");
                }
            }

            if (packageIds.Any())
            {
                return string.Join("+", packageIds);
            }

            return string.Empty;
        }

        /// <summary>
        ///  Get the Id for a service offered by the nuget.org api
        /// </summary>
        /// <param name="serviceName">Name of the service you want</param>
        /// <returns>Id value (url) of required service</returns>
        private string GetNugetService(string serviceName)
        {
            var response = GetNugetResponse<NugetServiceIndexResponse>(this._nugetServiceUrl);
            return response?.Resources?.FirstOrDefault(x => x.Type.Equals(serviceName))?.Id;
        }

        /// <summary>
        ///  Get the search result json from nuget based on the query url
        /// </summary>
        /// <param name="queryUrl">URL of search query (including searchService url)</param>
        /// <returns>NugetSearchResponse object with results</returns>
        private NugetSearchResponse GetNugetSearchResults(string queryUrl)
            => GetNugetResponse<NugetSearchResponse>(queryUrl);


        /// <summary>
        ///  Get Result of nuget query serialized into object based on type 
        /// </summary>
        /// <typeparam name="TResult">type of object you want results returned in</typeparam>
        /// <param name="url">url for the request</param>
        /// <returns>Results serialized from json into TResult object</returns>
        private TResult GetNugetResponse<TResult>(string url)
        {
            var client = new RestClient(url);
            var results = client.Execute(new RestRequest());
            return JsonConvert.DeserializeObject<TResult>(results.Content);

        }

        /// <summary>
        ///  calculate the information we need for a given nuget package
        /// </summary>
        /// <param name="nugetPackage">NugetSearchResult containing package info</param>
        /// <returns>The info we require for a package NugetPackageInfo</returns>
        private NugetPackageInfo GetNugetPackageInfo(NugetSearchResult nugetPackage)
        {
            var blankDate = new DateTime(1900, 1, 1);
            var oldestDate = DateTime.Now;

            var packageInfo = GetNugetResponse<NugetRegistrationResponse>(nugetPackage.PackageRegistrationUrl);
            if (packageInfo != null)
            {
                foreach (var item in packageInfo.Items)
                {

                    // workout the oldest date based on commit time stamp (when the package was uploaded not published)
                    var date = item.Items
                        .Select(x => new[] { 
                            x.CommitTimeStamp > blankDate ? x.CommitTimeStamp : DateTime.Now, 
                            x.CatalogEntry.PublishedDate > blankDate ? x.CatalogEntry.PublishedDate : DateTime.Now }.Min())
                        .OrderBy(x => x)
                        .FirstOrDefault();

                    if (date != null && date < oldestDate)
                    {
                        oldestDate = date;
                    }

                }
            }

            return new NugetPackageInfo
            {
                TotalDownLoads = nugetPackage.TotalDownloads,
                PackageId = nugetPackage.Id,
                AverageDownloadPerDay = GetAvarageDownloadsPerDay(nugetPackage.TotalDownloads, oldestDate),
                PackageDate = oldestDate
            };
        }

        /// <summary>
        ///  Calculate the avarageDownloadsPerDay for a given total and date.
        /// </summary>
        /// <param name="total">total number of downloads</param>
        /// <param name="published">date package was published</param>
        /// <returns>Int: Avarage downloads per day (rounded)</returns>
        private int GetAvarageDownloadsPerDay(int total, DateTime published)
        {
            var daysSincePublished = (DateTime.Now - published).TotalDays;
            if (daysSincePublished > 1)
            {
                return (int)Math.Ceiling(total / daysSincePublished);
            }
            else
            {
                return total;
            }
        }

        /// <summary>
        ///  save the calculated package info to disk, so we can run the index from there.
        /// </summary>
        private void SavePackageInfo(IEnumerable<NugetPackageInfo> packages)
        {
            if (packages.Any())
            {

                if (!Directory.Exists(this._storageDirectory))
                    Directory.CreateDirectory(this._storageDirectory);

                var file = Path.Combine(_storageDirectory.EnsureEndsWith(Path.DirectorySeparatorChar), _downloadsFile);

                var json = JsonConvert.SerializeObject(packages, Formatting.Indented);
                File.WriteAllText(file, json, Encoding.UTF8);
            }
        }

        /// <summary>
        ///  trigger the rebuild of the examine index for projects.
        /// </summary>
        private void RebuildProjectIndexer()
        {
            ExamineManager.Instance.IndexProviderCollection["projectIndexer"].RebuildIndex();
        }

        public List<NugetPackageInfo> GetNugetPackageDownloads()
        {
            var downloadsFile = $"{this._storageDirectory.EnsureEndsWith("/")}{this._downloadsFile}";

            return (List<NugetPackageInfo>)ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(
                "NugetDownloads",
                () =>
                    {
                        var downloads = new List<NugetPackageInfo>();

                        if (File.Exists(downloadsFile))
                        {
                            var rawJson = File.ReadAllText(downloadsFile, Encoding.UTF8);

                            if (!string.IsNullOrWhiteSpace(rawJson))
                            {
                                try
                                {
                                    downloads = JsonConvert.DeserializeObject<List<NugetPackageInfo>>(rawJson);
                                }
                                catch
                                {
                                    // should we log this
                                }
                            }
                        }

                        return downloads;
                    },
                TimeSpan.FromHours(1),
                false,
                CacheItemPriority.Normal,
                null,
                new[] { downloadsFile });

        }

        public string GetNuGetPackageId(IPublishedContent project)
        {
            var nuGetPackageUrl = project.GetPropertyValue<string>("nuGetPackageUrl");

            return GetNugetPackageIdFromUrl(nuGetPackageUrl);
        }

        public string GetNuGetPackageId(IContent project)
        {
            var nuGetPackageUrl = project.GetValue<string>("nuGetPackageUrl");

            return GetNugetPackageIdFromUrl(nuGetPackageUrl);
        }

        private string GetNugetPackageIdFromUrl(string nuGetPackageUrl)
        {
            string nuGetPackageCmd = string.Empty;
            if (!string.IsNullOrEmpty(nuGetPackageUrl))
            {
                Regex regex = new Regex(@"^(http|https)://", RegexOptions.IgnoreCase);

                if (!regex.Match(nuGetPackageUrl).Success)
                {
                    nuGetPackageUrl = "http://" + nuGetPackageUrl;
                }

                var nuGetPackageUri = new Uri(nuGetPackageUrl);

                if (nuGetPackageUri.Segments.Length >= 2)
                {
                    nuGetPackageCmd = nuGetPackageUri.Segments[2].Trim("/");
                }
            }

            return nuGetPackageCmd;
        }

        private static UmbracoContext EnsureUmbracoContext()
        {
            //TODO: To get at the IPublishedCaches it is only available on the UmbracoContext (which we need to fix)
            // but since this method operates async, there isn't one, so we need to make our own to get at the cache
            // object by creating a fake HttpContext. Not pretty but it works for now.
            if (UmbracoContext.Current != null)
                return UmbracoContext.Current;

            var dummyHttpContext = new HttpContextWrapper(new HttpContext(new SimpleWorkerRequest("blah.aspx", "", new StringWriter())));

            return UmbracoContext.EnsureContext(dummyHttpContext,
                ApplicationContext.Current,
                new WebSecurity(dummyHttpContext, ApplicationContext.Current),
                UmbracoConfig.For.UmbracoSettings(),
                UrlProviderResolver.Current.Providers,
                false);
        }
    }
}
