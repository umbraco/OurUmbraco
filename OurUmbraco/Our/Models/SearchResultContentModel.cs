using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace OurUmbraco.Our.Models
{
    /// <summary>
    /// An IPublishedContent model used to display the search results on the SearchResults controller
    /// </summary>
    public class SearchResultContentModel : PublishedContentWrapped
    {
        public SearchResultModel Results { get; private set; }

        public SearchResultContentModel(IPublishedContent content, SearchResultModel results)
            : base(content)
        {
            Results = results;
        }
    }
}
