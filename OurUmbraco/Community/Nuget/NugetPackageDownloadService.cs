using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using Examine;
using Hangfire.Console;
using Hangfire.Server;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using File = System.IO.File;
using Task = System.Threading.Tasks.Task;

namespace OurUmbraco.Community.Nuget
{
    /// <summary>
    /// Represents nuget package download service.
    /// </summary>
    public class NugetPackageDownloadService
    {
        private readonly string _storageDirectory = HostingEnvironment.MapPath("~/App_Data/TEMP/NugetDownloads");

        private readonly string _downloadsFile = "downloads.json";

        public async Task ImportNugetPackageDownloads(PerformContext context)
        {
            // get all packages that have a nuget url specified
            var umbContxt = EnsureUmbracoContext();

            var projects = umbContxt.ContentCache.GetByXPath("//Community/Projects//Project [nuGetPackageUrl!='']")
                .ToList();

            if (projects.Any() == false)
            {
                context.WriteLine("Found no packages with a `nuGetPackageUrl`");
                return;
            }

            var packageInfos = new List<NugetPackageInfo>();
            foreach (var project in projects)
            {
                var nuGetPackageCmd = GetNuGetPackageId(project);
                    
                if (string.IsNullOrWhiteSpace(nuGetPackageCmd)) 
                    continue;

                packageInfos.Add(new NugetPackageInfo { Name = project.Name, PackageId = nuGetPackageCmd });
            }
            
            var bearerToken = ConfigurationManager.AppSettings["CollabBearerToken"];
            const string url = "https://collaboratorsv2.euwest01.umbraco.io/umbraco/api/NuGet/PackageStatistics";
            
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
                var json = JsonConvert.SerializeObject(packageInfos); 
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, httpContent);
                if (response.IsSuccessStatusCode == false)
                {
                    context.WriteLine($"Response from {url} was {response.StatusCode} - {response.ReasonPhrase}");
                    return;
                }

                var nugetPackageDownloads = await response.Content.ReadAsStringAsync();
                
                // store downloads if any
                if (string.IsNullOrWhiteSpace(nugetPackageDownloads))
                {
                    context.WriteLine($"Response data from {url} was an empty string, exiting");
                    return;
                }

                if (!Directory.Exists(_storageDirectory))
                {
                    Directory.CreateDirectory(_storageDirectory);
                }

                File.WriteAllText($"{_storageDirectory.EnsureEndsWith("/")}{_downloadsFile}", nugetPackageDownloads, Encoding.UTF8);

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

        public List<NugetPackageInfo> GetNugetPackageDownloads()
        {
            var downloadsFile = $"{_storageDirectory.EnsureEndsWith("/")}{_downloadsFile}";

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
            var nuGetPackageUrl = project.GetProperty("nuGetPackageUrl").DataValue.ToString();

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

                if (nuGetPackageUri.Segments.Length >= 3)
                {
                    nuGetPackageCmd = nuGetPackageUri.Segments[2].Trim('/');
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

            var dummyHttpContext =
                new HttpContextWrapper(new HttpContext(new SimpleWorkerRequest("blah.aspx", "", new StringWriter())));

            return UmbracoContext.EnsureContext(dummyHttpContext,
                ApplicationContext.Current,
                new WebSecurity(dummyHttpContext, ApplicationContext.Current),
                UmbracoConfig.For.UmbracoSettings(),
                UrlProviderResolver.Current.Providers,
                false);
        }
    }
}