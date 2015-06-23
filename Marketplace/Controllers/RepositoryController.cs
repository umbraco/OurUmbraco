using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Examine;
using Examine.SearchCriteria;
using Umbraco.Web.WebApi;

namespace uProject.Controllers
{
    public class RepositoryController : UmbracoApiController
    {
        [HttpGet]
        public HttpResponseMessage Search(string query, int parent, bool wildcard)
        {
            if (query.ToLower() == "useqsstring") query = HttpContext.Current.Request.QueryString["term"];
            if (wildcard && !query.EndsWith("*")) query += "*";
            var searchTerm = query;
            var searcher = ExamineManager.Instance.SearchProviderCollection["projectSearcher"];
            
            ////Search Criteria for WIKI & Projects
            var searchCriteria = searcher.CreateSearchCriteria(BooleanOperation.Or);
            var searchQuery = searchTerm.BuildExamineString(99, "nodeName", false);
            searchQuery += searchTerm.BuildExamineString(10, "description", false);
            var searchFilter = searchCriteria.RawQuery(searchQuery);
            IEnumerable<SearchResult> searchResults = searcher.Search(searchFilter).OrderByDescending(x => x.Score);
            searchResults = from r in searchResults
                            where r["nodeTypeAlias"] == "project"
                            select r;

            var resp = Request.CreateResponse(HttpStatusCode.OK, searchResults, Configuration.Formatters.JsonFormatter);
            return resp;
        }
    }
}