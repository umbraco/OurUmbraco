using System.Collections.Generic;
using Examine.SearchCriteria;
using Lucene.Net.Documents;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.Our.Examine;
using OurUmbraco.Our.Models;
using OurUmbraco.Project;
using Umbraco.Web.WebApi;
using Umbraco.Core;

namespace OurUmbraco.Our.Api
{
    public class OurSearchController : UmbracoApiController
    {
        public SearchResultModel GetGlobalSearchResults(string term)
        {
            var searcher = new OurSearcher(term, maxResults: 5);
            var searchResult = searcher.Search();
            return searchResult;
        }


        public SearchResultModel GetProjectSearchResults(string term, string version = null)
        {
            var filters = new List<SearchFilters>();
            var searchFilters = new SearchFilters(BooleanOperation.And);
            //MUST be approved and live
            searchFilters.Filters.Add(new SearchFilter("approved", "1"));
            searchFilters.Filters.Add(new SearchFilter("projectLive", "1"));
            filters.Add(searchFilters);
            //need to clean up this string, it could be all sorts of things
            var parsedVersion = version.GetFromUmbracoString();
            if (parsedVersion != null)
            {
                var numericalVersion = parsedVersion.GetNumericalValue();
                var versionFilters = new SearchFilters(BooleanOperation.Or);
                versionFilters.Filters.Add(new RangeSearchFilter("num_version", numericalVersion, long.MaxValue));
                versionFilters.Filters.Add(new RangeSearchFilter("num_compatVersions", numericalVersion, long.MaxValue));
                filters.Add(versionFilters);
            }

            var searcher = new OurSearcher(term, nodeTypeAlias:"project", filters: filters);
            var searchResult = searcher.Search("projectSearcher");
            return searchResult;
        }

        public SearchResultModel GetDocsSearchResults(string term)
        {
            var searcher = new OurSearcher(term, nodeTypeAlias: "documentation");
            var searchResult = searcher.Search("documentationSearcher");
            return searchResult;
        }

        public SearchResultModel GetForumSearchResults(string term, string forum = "")
        {
            int forumId;
            var filters = new List<SearchFilters>();
            if (int.TryParse(forum, out forumId))
            {
                var searchFilters = new SearchFilters(BooleanOperation.And);
                searchFilters.Filters.Add(new SearchFilter("parentId", forumId.ToString()));
                filters.Add(searchFilters);
            }

            var searcher = new OurSearcher(term, nodeTypeAlias: "forum", filters: filters);
            var searchResult = searcher.Search("ForumSearcher");

            foreach (var result in searchResult.SearchResults)
            {
                result.Fields["url"] = result.FullUrl();

                //Since these results are going to be displayed in the forum, we need to convert the date field to 
                // the 'relative' value that is normally shown for the forum date
                var updateDate = result.Fields["updateDate"] = DateTools.StringToDate(result.Fields["updateDate"]).ConvertToRelativeTime();
            }

            

            return searchResult;
        }
    }
}
