using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using OurUmbraco.Repository.Models;
using OurUmbraco.Repository.Services;
using Umbraco.Core;
using Umbraco.Web.WebApi;
using System.Net.Http;
using System.Net;

namespace OurUmbraco.Repository.Controllers
{
    /// <summary>
    /// The package repository controller for querying packages
    /// </summary>
    /// <remarks>
    /// This is NOT auto-routed
    /// </remarks>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [JsonCamelCaseFormatter]
    public class PackageRepositoryController : UmbracoApiControllerBase
    {
        private readonly PackageRepositoryService Service;

        public PackageRepositoryController()
        {
            Service = new PackageRepositoryService(Umbraco, Members, DatabaseContext);
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
            PackageSortOrder order = PackageSortOrder.Latest)
        {
            return Service.GetPackages(pageIndex, pageSize, category, query, order);
        }

        public Models.PackageDetails GetDetails(Guid id)
        {
            var package = Service.GetDetails(id);

            if (package == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return package;
        }
    }
}