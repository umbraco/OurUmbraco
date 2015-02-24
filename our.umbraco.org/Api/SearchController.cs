using Examine;
using our.Examine;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.WebApi;

namespace our.Api
{
    public class OurSearchController : UmbracoApiController
    {
        public dynamic GetGlobalSearchResults(string term)
        {
            var searcher = new OurSearcher();
            searcher.Term = term;
            var searchResult = searcher.Search();

            dynamic result = new ExpandoObject();
            result.total = searchResult.TotalItemCount;
            result.items = searchResult.Take(5);
            result.term = term;

            return result;
        }

    }
}
