using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Examine;
using Examine.LuceneEngine;
using Examine.SearchCriteria;
using Umbraco.Web.WebApi;
using uSearch.Library;

namespace uSearch.Api
{
    public class SearchController : UmbracoApiController
    {
        [HttpGet]
        public HttpResponseMessage FindSimiliarItems(string types, int maxItems, string query)
        {
            query = umbraco.library.StripHtml(query);
            var keywords = string.Join(" ", Xslt.GetKeywords(query)).Trim();
            var result = Xslt.LuceneInContentType(keywords, types, 0, 255, maxItems);

            return new HttpResponseMessage { Content = new StringContent(result.OuterXml, Encoding.UTF8, "application/xml") };
        }

        [HttpGet]
        public string FindProjects(string query, int parent, bool wildcard)
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
}
