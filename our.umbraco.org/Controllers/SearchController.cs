using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Examine;
using Examine.LuceneEngine.Providers;
using our.Examine;
using our.Models;
using Umbraco.Web.Models;

namespace our.Controllers
{
    public class SearchController : Controller
    {

        public ActionResult Search(RenderModel model, string term)
        {
            var ourSearcher = new OurSearcher(term, maxResults: 100);

            var results = ourSearcher.Search();

            return View(new RenderModel<SearchResultContentModel>(
                new SearchResultContentModel(model.Content, results)));
        }

    }

}
