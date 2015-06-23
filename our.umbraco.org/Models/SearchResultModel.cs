using System.Runtime.Serialization;
using Examine;

namespace our.Models
{
    /// <summary>
    /// A model to display search results
    /// </summary>
    [DataContract]
    public class SearchResultModel
    {
        public SearchResultModel(ISearchResults searchResults, double totalmilliseconds, string searchTerm, string orderBy)
        {
            SearchResults = searchResults;
            Totalmilliseconds = totalmilliseconds;
            SearchTerm = searchTerm;
            OrderBy = orderBy;
        }

        [DataMember(Name = "orderBy")]
        public string OrderBy { get; set; }

        [DataMember(Name = "items")]
        public ISearchResults SearchResults { get; private set; }

        [DataMember(Name = "milliseconds")]
        public double Totalmilliseconds { get; private set; }

        [DataMember(Name = "term")]
        public string SearchTerm { get; private set; }

        //NOTE: Used for debugging
        [DataMember(Name = "luceneQuery")]
        public string LuceneQuery { get; set; }
    }
}