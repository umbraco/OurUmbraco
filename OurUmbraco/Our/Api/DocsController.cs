using System.Collections.Generic;
using System.Web.Http;
using Examine;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class DocsController : UmbracoAuthorizedApiController
    {
        [HttpGet]
        public List<string> GetInvalidDocsArticles()
        {
            var searcher = ExamineManager.Instance.SearchProviderCollection["documentationSearcher"];
            var criteria = searcher.CreateSearchCriteria();
            var rawQuery = $"+tags: yamlissue";
            var query = criteria.RawQuery(rawQuery);
            var results = searcher.Search(query);

            var articleList = new List<string>();

            foreach (var searchResult in results)
            {
                articleList.Add(searchResult["url"]);
            }

            return articleList;
        }
    }
}
