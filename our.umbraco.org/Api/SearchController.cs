using Examine;
using our.Examine;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Documents;
using our.Models;
using uForum;
using uForum.Extensions;
using Umbraco.Web.WebApi;

namespace our.Api
{
    public class OurSearchController : UmbracoApiController
    {
        public SearchResultModel GetGlobalSearchResults(string term)
        {
            var searcher = new OurSearcher(term, maxResults: 5);
            var searchResult = searcher.Search();
            return searchResult;
        }


        public SearchResultModel GetProjectSearchResults(string term)
        {
            var searcher = new OurSearcher(term, nodeTypeAlias:"project");
            var searchResult = searcher.Search();
            return searchResult;
        }

        public SearchResultModel GetDocsSearchResults(string term)
        {
            var searcher = new OurSearcher(term, nodeTypeAlias: "documentation");
            var searchResult = searcher.Search();
            return searchResult;
        }

        public SearchResultModel GetForumSearchResults(string term)
        {
            var searcher = new OurSearcher(term, nodeTypeAlias: "forum");
            var searchResult = searcher.Search();

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
