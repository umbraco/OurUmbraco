using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using OurUmbraco.Repository.Models;
using OurUmbraco.Repository.Services;
using Umbraco.Core.Cache;
using Umbraco.Web.WebApi;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using OurUmbraco.Wiki.BusinessLogic;
using Semver;
using Umbraco.Core;
using Umbraco.Web;

namespace OurUmbraco.Repository.Controllers
{
    /// <summary>
    /// The package repository controller for querying packages
    /// </summary>
    /// <remarks>
    /// This is NOT auto-routed
    /// </remarks>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [CamelCaseFormatter]
    [OutgoingDateTimeFormat]
    public class PackageRepositoryController : UmbracoApiControllerBase
    {
        private PackageRepositoryService _service;

        internal PackageRepositoryService Service
        {
            get { return _service ?? (_service = new PackageRepositoryService(Umbraco, Members, DatabaseContext)); }
        }

        public IEnumerable<Models.Category> GetCategories()
        {
            // [LK:2016-06-13@CGRT16] We're hardcoding the categories as the 'icon' isn't
            // content-manageable (yet). There is a media-picker icon, but that's for a different use.
            // When the time comes, we can switch to query for the Category nodes directly.
            return new[]
            {
                new Models.Category
                {
                    Icon = "icon-male-and-female",
                    Name = "Collaboration"
                },
                new Models.Category
                {
                    Icon = "icon-molecular-network",
                    Name = "Backoffice extensions"
                },
                new Models.Category
                {
                    Icon = "icon-brackets",
                    Name = "Developer tools"
                },
                new Models.Category
                {
                    Icon = "icon-wand",
                    Name = "Starter kits"
                },
                new Models.Category
                {
                    Icon = "icon-medal",
                    Name = "Umbraco Pro"
                },
                new Models.Category
                {
                    Icon = "icon-wrench",
                    Name = "Website utilities"
                }
            };
        }

        [HttpGet]
        public PagedPackages Search(
            int pageIndex,
            int pageSize,
            string category = null,
            string query = null,
            string version = null,
            PackageSortOrder order = PackageSortOrder.Latest)
        {
            //return the results, but cache for 1 minute
            var key = string.Format("PackageRepositoryController.GetCategories.{0}.{1}.{2}.{3}.{4}", pageIndex, pageSize, category ?? string.Empty, query ?? string.Empty, order);
            return ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<PagedPackages>
                (key,
                    () => Service.GetPackages(pageIndex, pageSize, category, query, version, order),
                    TimeSpan.FromMinutes(1)); //cache for 1 min    
        }

        /// <summary>
        /// Returns the package details for the package Id passed in and ensures that 
        /// the resulting ZipUrl is the compatible package for the version passed in.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version">The umbraco version requesting the details, if null than the ZipUrl will be the latest package zip</param>
        /// <param name="asFile">pass in true to get the package file otherwise leave blank or set to false to retreive the package details</param>
        /// <returns></returns>
        public HttpResponseMessage Get(Guid id, string version = null, bool? asFile = false)
        {
            SemVersion parsed = null;
            if (version.IsNullOrWhiteSpace() == false)
            {
                SemVersion.TryParse(version, out parsed);
            }
            else
            {
                //if the version is null then the current umbraco version must be 7.5.x because this endpoint was only ever used by 7.5.x and 7.6.x and above will always 
                // suppy the version, therefore we can assume that this is 7.5.x, let's make it 7.5.13 which is the latest 7.5 we have right now
                parsed = new SemVersion(7, 5, 13);
            }

            //this should never be null, if it is return not found
            if (parsed == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            var v = new System.Version(parsed.Major, parsed.Minor, parsed.Patch);

            return asFile.HasValue && asFile.Value
                ? GetPackageFile(id, v)
                : GetDetails(id, v);
        }

        private HttpResponseMessage GetDetails(Guid id, System.Version currUmbracoVersion)
        {
            //return the results, but cache for 1 minute
            var key = string.Format("PackageRepositoryController.GetDetails.{0}.{1}", id, currUmbracoVersion.ToString(3));
            try
            {
                var package = ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<PackageDetails>
                    (key,
                        () =>
                        {
                            var details = Service.GetDetails(id, currUmbracoVersion);

                            if (details == null)
                                throw new InvalidOperationException("No package found with id " + id);

                            if (details.ZipUrl.IsNullOrWhiteSpace())
                                throw new InvalidOperationException("This package is not compatible with the Umbraco version " + currUmbracoVersion);

                            return details;
                        },
                        TimeSpan.FromMinutes(1)); //cache for 1 min    

                if (package == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                return Request.CreateResponse(HttpStatusCode.OK, package);
            }
            catch (InvalidOperationException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            
        }

        private HttpResponseMessage GetPackageFile(Guid packageId, System.Version currUmbracoVersion)
        {
            var pckRepoService = new PackageRepositoryService(Umbraco, Members, DatabaseContext);            

            var details = pckRepoService.GetDetails(packageId, currUmbracoVersion);
            if (details == null)
                throw new InvalidOperationException("No package found with id " + packageId);

            if (details.ZipUrl.IsNullOrWhiteSpace())
                throw new InvalidOperationException("This package is not compatible with the Umbraco version " + currUmbracoVersion);

            var wf = new WikiFile(details.ZipFileId);
            if (wf == null)
                throw new InvalidOperationException("Could not find wiki file by id " + details.ZipFileId);
            wf.UpdateDownloadCounter(true, true);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(wf.ToByteArray())
            };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }
    }
}