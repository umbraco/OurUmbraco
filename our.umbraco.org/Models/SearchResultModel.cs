using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace our.Models
{
    public class SearchResultModel : PublishedContentWrapped
    {
        public ISearchResults SearchResults { get; private set; }
        public double Totalmilliseconds { get; private set; }
        public string SearchTerm { get; private set; }

        public SearchResultModel(IPublishedContent content, ISearchResults searchResults, double totalmilliseconds, string searchTerm)
            : base(content)
        {
            SearchResults = searchResults;
            Totalmilliseconds = totalmilliseconds;
            SearchTerm = searchTerm;
        }
    }
}
