using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Examine;
using Examine.SearchCriteria;
using OurUmbraco.Our.Examine;
using Umbraco.Core;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Project.Controllers
{
    public class RepositoryController : UmbracoApiController
    {
        [HttpGet]
        public HttpResponseMessage Search(string query, string version = null)
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
                    filters.Add(versionFilters);
                }
            }

            var ourSearcher = new OurSearcher(query, "project", maxResults:100, filters:filters);
            var result = ourSearcher.Search("projectSearcher");         

            var resp = Request.CreateResponse(
                HttpStatusCode.OK, 
                (IEnumerable<SearchResult>)result.SearchResults, 
                Configuration.Formatters.JsonFormatter);

            return resp;
        }
    }
}