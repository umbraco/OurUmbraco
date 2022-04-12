using Umbraco.Core.Logging;

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
                // get the services from nuget service index
                var restClient = new RestClient(this._nugetServiceUrl);

                var result = restClient.Execute(new RestRequest());

                var response = JsonConvert.DeserializeObject<NugetServiceIndexResponse>(result.Content);

                if (response != null)
                {
                    // get a url for the search service
                    var searchUrl = response.Resources.FirstOrDefault(x => x.Type == "SearchQueryService")?.Id;

                    if (!string.IsNullOrWhiteSpace(searchUrl))
                    {
                        var nugetPackageDownloads = new List<NugetPackageInfo>();

                        // we will loop trough our projects in groups of 5 so we can query for multiple packages at ones
                        // the nuget api has a rate limit, so to avoid hitting that we query multiple packages at once

                        foreach (var projectGroup in projects.InGroupsOf(5))
                        {
                            var packageQuery = string.Empty;

                            foreach (var project in projectGroup)
                            {
                                var nuGetPackageCmd = GetNuGetPackageId(project);

                                if (!string.IsNullOrWhiteSpace(nuGetPackageCmd))
                                {
                                    packageQuery += $"packageid:{nuGetPackageCmd}+";
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(packageQuery))
                            {
                                var searchQuery = $"{searchUrl}?q={packageQuery.TrimEnd("+")}&prerelease=true";

                                restClient = new RestClient(searchQuery);

                                var packageResponse = restClient.Execute(new RestRequest());

                                var packageSearchResult =
                                    JsonConvert.DeserializeObject<NugetSearchResponse>(packageResponse.Content);

                                if (packageSearchResult != null)
                                {
                                    foreach (var package in packageSearchResult.Results)
                                    {
                                        var packageInfo = new NugetPackageInfo
                                                              {
                                                                  PackageId = package.Id,
                                                                  TotalDownLoads = package.TotalDownloads
                                                              };

                                       

                                        // try get details about downloads over time
                                        // so we get the publish date of the package on nuget. And calculate the average downloads per day.
                                        // we can use this data for the popular package query on our
                                        // when a package has more than 128 release the response is paged,
                                        // the getNugetPackageEntries method manages this and returns a list of packageItems.

                                        var packageEntries = GetNugetPackageEntries(package.PackageRegistrationUrl);

                                        if (packageEntries.Any())
                                        {
                                            packageInfo.AverageDownloadPerDay = CalculateAverageDownloadsPerDay(packageEntries, package.TotalDownloads);
                                        }
                                        else
                                        {
                                            umbContxt.Application.ProfilingLogger.Logger.Warn(typeof(NugetPackageDownloadService), "Could not retrieve average downloads from nuget for package " + package.Id);
                                        }

                                        if (!nugetPackageDownloads.Any(x => x.PackageId == packageInfo.PackageId))
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
                            if (!Directory.Exists(this._storageDirectory))
                            {
                                Directory.CreateDirectory(this._storageDirectory);
                            }

                            var rawJson = JsonConvert.SerializeObject(nugetPackageDownloads, Formatting.Indented);
                            File.WriteAllText($"{this._storageDirectory.EnsureEndsWith("/")}{this._downloadsFile}", rawJson, Encoding.UTF8);

                            try
                            {
                                ExamineManager.Instance.IndexProviderCollection["projectIndexer"].RebuildIndex();
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error<NugetPackageDownloadService>("Rebuilding package index failed", ex);
                                throw;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Get all of the nuget package entries for a package 
        /// </summary>
        /// <remarks>
        ///  if there are more than 128 entries for a package, then nuget will return a list of paged urls
        ///  you can call to get all the package entries. 
        ///  
        ///  this method will then call the GetPagedNugetPackageEntries to get these pages and put them
        ///  into a single list for us to test. 
        /// </remarks>
        /// <param name="packageRegistrationUrl">the package Url</param>
        /// <returns>A list of package entries for the package</returns>
        private IEnumerable<NugetRegistrationItemEntry> GetNugetPackageEntries(string packageRegistrationUrl)
        {
            var registrationClient = new RestClient(packageRegistrationUrl);
            var registrationResponse = registrationClient.Execute(new RestRequest());
            if (!registrationResponse.IsSuccessful)
            {
                return Enumerable.Empty<NugetRegistrationItemEntry>();
            }

            var registrationResult = JsonConvert.DeserializeObject<NugetRegistrationResponse>(registrationResponse.Content);
            if (registrationResult == null || registrationResult.Items == null) 
            { 
                return Enumerable.Empty<NugetRegistrationItemEntry>();
            }

            List<NugetRegistrationItemEntry> registrationEntries = new List<NugetRegistrationItemEntry>();
            foreach (var item in registrationResult.Items)
            {
                if (item.Items == null && !string.IsNullOrWhiteSpace(item.Id))
                {
                    registrationEntries.AddRange(GetPagedNugetPackageEntries(item.Id));
                }
                else
                {
                    registrationEntries.AddRange(item.Items);
                }
            }
            return registrationEntries;
        }

        /// <summary>
        ///  get the package entries from a paged package url 
        /// </summary>
        /// <param name="packageRegistrationPagedUrl">The url to a page of package entries for a package</param>
        /// <returns></returns>
        private IEnumerable<NugetRegistrationItemEntry> GetPagedNugetPackageEntries(string packageRegistrationPagedUrl)
        {
            var registrationClient = new RestClient(packageRegistrationPagedUrl);
            var registrationResponse = registrationClient.Execute(new RestRequest());
            if (!registrationResponse.IsSuccessful)
            {
                return Enumerable.Empty<NugetRegistrationItemEntry>();
            }

            var registrationResult = JsonConvert.DeserializeObject<NugetRegistrationItem>(registrationResponse.Content);
            return registrationResult.Items ?? Enumerable.Empty<NugetRegistrationItemEntry>();
        }


        /// <summary>
        ///  calculate the avergate number of downloads per day for a nuget package.
        /// </summary>
        /// <remarks>
        ///  the average number of downloads is not something exposed by the api, 
        ///  and neigher is the package creation date, so we have to get the earliest package publication date
        ///  and then use that to get downloads per day. 
        ///  
        ///  this isn't perfect because if someone unlists a package, the publication date is wiped, so a newer
        ///  publication date will be used and the averages will be higher, but there doesn't appear to be another
        ///  trustable date we can get from the api. 
        /// </remarks>
        private int CalculateAverageDownloadsPerDay(IEnumerable<NugetRegistrationItemEntry> items, int totalDownloads)
        {
                var publishedDate = items.Select(x => x.CatalogEntry)
                    .OrderBy(x => x.PublishedDate).FirstOrDefault(x => x.PublishedDate.Year > 1900)?.PublishedDate;

                if (publishedDate.HasValue)
                {
                    var daysSincePublished = (DateTime.Now - publishedDate.Value).TotalDays;

                    return (int)Math.Ceiling(totalDownloads / daysSincePublished);
                }
            return 0;
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
