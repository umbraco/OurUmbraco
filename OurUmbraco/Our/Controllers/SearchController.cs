using System.Collections.Generic;
using System.Web.Mvc;
using Examine.SearchCriteria;
using OurUmbraco.Our.Examine;
using OurUmbraco.Our.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class SearchController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render(string q, string cat = "", string order = "", int fid = 0, bool solved = false, bool replies = false)
        {
            var umbracoPage = UmbracoContext.PublishedContentRequest.PublishedContent;

            var nodeTypeAlias = cat;

            //TODO: If we are searching on projects, they need to be filtered to approved/live!

            var forumName = string.Empty;
            var filters = new List<SearchFilters>();

            if (nodeTypeAlias == "forum" && fid > 0)
            {
                var searchFilters = new SearchFilters(BooleanOperation.And);
                searchFilters.Filters.Add(new SearchFilter("parentId", fid));
                filters.Add(searchFilters);

                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                var forum = umbracoHelper.ContentQuery.TypedContent(fid);
                var parentForum = forum.Parent;
                forumName = forum.Name + " - " + parentForum.Name;
            }

            if (solved)
            {
                var searchFilters = new SearchFilters(BooleanOperation.Not);
                searchFilters.Filters.Add(new SearchFilter("solved", "0"));
                filters.Add(searchFilters);
            }

            if (replies)
            {
                var searchFilters = new SearchFilters(BooleanOperation.Not);
                searchFilters.Filters.Add(new SearchFilter("replies", "0"));
                filters.Add(searchFilters);
            }

            var ourSearcher = new OurSearcher(q,
                //TODO: Depending on what order by this is, we need to pass in a data
                // type here, for example, if its an INT or a Date!
                orderBy: order,
                maxResults: 100,
                nodeTypeAlias: nodeTypeAlias,
                filters: filters);

            var results = ourSearcher.Search();

            var model = new SearchResultContentModel(umbracoPage, results);
            if (string.IsNullOrWhiteSpace(forumName) == false)
                model.Results.Category = forumName;

            return PartialView("~/Views/Partials/SearchResults.cshtml", model);
        }
    }
}
