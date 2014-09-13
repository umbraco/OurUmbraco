using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Xml.XPath;
using Examine;
using Examine.SearchCriteria;
using Umbraco.Web.WebApi;
using uSearch.Library;

namespace uSearch.Api
{
    public class SearchController : UmbracoApiController
    {
        [HttpGet]
        public static XPathNodeIterator FindSimiliarItems(string types, int maxItems)
        {
            var query = umbraco.library.StripHtml(HttpContext.Current.Request["q"]);
            var keywords = string.Join(" ", Xslt.GetKeywords(query)).Trim();
            return Xslt.LuceneInContentType(keywords, types, 0, 255, maxItems);
        }

        [HttpGet]
        public static string FindProjects(string query, int parent, bool wildcard)
        {
            if (query.ToLower() == "useqsstring") 
                query = umbraco.presentation.UmbracoContext.Current.Request.QueryString["term"];

            if (wildcard && query.EndsWith("*") == false) 
                query += "*";

            var searchTerm = query;
            var searcher = ExamineManager.Instance.SearchProviderCollection["MultiIndexSearcher"];
            
            //Search Criteria for WIKI & Projects
            var searchCriteria = searcher.CreateSearchCriteria(BooleanOperation.Or);
            var searchQuery = searchTerm.BuildExamineString(99, "nodeName");
            searchQuery += searchTerm.BuildExamineString(10, "description");
            searchQuery = "(" + searchQuery + ") AND +approved:1";
            var searchFilter = searchCriteria.RawQuery(searchQuery);
            IEnumerable<SearchResult> searchResults = searcher.Search(searchFilter).OrderByDescending(x => x.Score);
            searchResults = from r in searchResults
                            where r["__IndexType"] == "content" && r["nodeTypeAlias"] == "Project"
                            select r;            

            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(searchResults);
        }
    }

    public static class ExamineHelpers
    {
        public static string BuildExamineString(this string term, int boost, string field)
        {
            var qs = field + ":";
            qs += "\"" + term + "\"^" + (boost + 30000) + " ";
            qs += field + ":(+" + term.Replace(" ", " +") + ")^" + (boost + 5) + " ";
            qs += field + ":(" + term + ")^" + boost + " ";
            return qs;
        }
    }
}
