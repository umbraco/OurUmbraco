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
                                        // we can run into issues when a package has more than 128 versions. This call will return a different response then
                                        // see https://docs.microsoft.com/en-us/nuget/api/registration-base-url-resource

                                        var registrationClient = new RestClient(package.PackageRegistrationUrl);

                                        var registrationResponse = registrationClient.Execute(new RestRequest());

                                        var registrationResult =
                                            JsonConvert.DeserializeObject<NugetRegistrationResponse>(
                                                registrationResponse.Content);

                                        if (registrationResult != null)
                                        {
                                            // get the lowest publish date
                                            var registrationItem = registrationResult.Items.FirstOrDefault();

                                            if (registrationItem != null)
                                            {
                                                var publishedDate = registrationItem.Items.Select(x => x.CatalogEntry)
                                                    .OrderBy(x => x.PublishedDate).FirstOrDefault(x => x.PublishedDate.Year > 1900)?.PublishedDate;

                                                if (publishedDate.HasValue)
                                                {
                                                    var daysSincePublished =
                                                        (DateTime.Now - publishedDate.Value).TotalDays;

                                                    packageInfo.AverageDownloadPerDay = (int)Math.Ceiling(package.TotalDownloads / daysSincePublished);
                                                }
                                            }
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

                            ExamineManager.Instance.IndexProviderCollection["projectIndexer"].RebuildIndex();
                        }
                    }
                }
            }
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
