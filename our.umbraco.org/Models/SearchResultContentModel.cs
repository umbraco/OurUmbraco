using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace our.Models
{
    /// <summary>
    /// An IPublishedContent model used to display the search results on the SearchResults controller
    /// </summary>
    public class SearchResultContentModel : PublishedContentWrapped
    {
        public SearchResultModel SearchResultModel { get; private set; }

        public SearchResultContentModel(IPublishedContent content, SearchResultModel searchResultModel)
            : base(content)
        {
            SearchResultModel = searchResultModel;
        }
    }
}
