using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Examine;
using Examine.LuceneEngine.Providers;
using our.Models;
using Umbraco.Web.Models;

namespace our.Controllers
{
    public class SearchController : Controller
    {

        public ActionResult Search(RenderModel model, string term)
        {
            var multiIndexSearchProvider = (MultiIndexSearcher)ExamineManager.Instance.SearchProviderCollection["MultiIndexSearcher"];

            var criteria = multiIndexSearchProvider.CreateSearchCriteria();
            var compiled = criteria
                .GroupedOr(new[] { "Body", "bodyText", "description", "nodeName" }, term)
                .Compile();

            var watch = new Stopwatch();
            watch.Start();
            //TODO: The result.TotalSearchResults will yield a max of 100 which is incorrect, this  is an issue 
            // in Examine, it needs to limit the results to 100 but still tell you how many in total
            var result = multiIndexSearchProvider.Search(compiled, 100);
            watch.Stop();

            return View(new RenderModel<SearchResultContentModel>(
                new SearchResultContentModel(model.Content, new SearchResultModel(result, watch.ElapsedMilliseconds, term, ""))));
        }

    }

}
