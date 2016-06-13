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
            var packages = Service.GetPackages(category, query, order);

            //TODO: This will be interesting - not sure if we are using Examine for searching but if we are
            // and if the query is not empty, then we should order by score,
            // otherwise if there is no query we will order by the 'order' parameter
            Models.Package[] sorted;
            if (string.IsNullOrWhiteSpace(query))
            {
                //TODO: order by score if possible
                sorted = packages.OrderBy(x => x.Created).ToArray();
            }
            else if (order == PackageSortOrder.Latest)
            {
                sorted = packages.OrderBy(x => x.Created).ToArray();
            }
            else
            {
                //TODO: Also included downloads somehow?
                sorted = packages.OrderBy(x => x.Likes).ToArray();
            }

            return new PagedPackages
            {
                Packages = sorted.Skip(pageIndex * pageSize).Take(pageSize),
                Total = sorted.Length
            };
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