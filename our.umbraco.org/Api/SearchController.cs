using Examine;
using our.Examine;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using our.Models;
using Umbraco.Web.WebApi;

namespace our.Api
{
    public class OurSearchController : UmbracoApiController
    {
        public SearchResultModel GetGlobalSearchResults(string term)
        {
            var searcher = new OurSearcher(term);
            var searchResult = searcher.Search();
            return searchResult;
        }


        public SearchResultModel GetProjectSearchResults(string term)
        {
            var searcher = new OurSearcher(term, nodeTypeAlias:"project");
            var searchResult = searcher.Search();
            return searchResult;
        }
    }
}
