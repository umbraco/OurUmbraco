using Examine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Examine.SearchCriteria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace our.Examine
{
    public class OurSearcher
    {
        static readonly Dictionary<Tuple<bool, bool, bool, bool>, Func<IEnumerable<SearchResult>, IEnumerable<SearchResult>>> lookup = new Dictionary<Tuple<bool, bool, bool, bool>, Func<IEnumerable<SearchResult>, IEnumerable<SearchResult>>>();
        protected BaseSearchProvider Searcher;

        public string Term { get; set; }
        public string NodeTypeAlias { get; set; }
        public string OrderBy { get; set; }


        public OurSearcher()
        {
            
        }

        public ISearchResults Search()
        {
            var multiIndexSearchProvider = (MultiIndexSearcher)ExamineManager.Instance.SearchProviderCollection["MultiIndexSearcher"];

            var criteria = multiIndexSearchProvider.CreateSearchCriteria();
            if (string.IsNullOrEmpty(NodeTypeAlias) == false)
            {
                criteria.NodeTypeAlias(NodeTypeAlias);
            }

            if (string.IsNullOrEmpty(OrderBy) == false)
            {
                criteria.OrderByDescending(OrderBy);    
            }


            ISearchCriteria compiled;

            if (string.IsNullOrEmpty(Term))
                compiled = criteria.NodeTypeAlias(NodeTypeAlias).Compile();
            else
            {
                Term = Term.Replace(" OR ", " ").Replace(" or ", " ");
                // Replace double whitespaces with single space as they were giving errors
                Term = Regex.Replace(Term, @"\s{2,}", " ");
                compiled = criteria
                            .GroupedOr(new[] { "nodeName", "body", "description" }, Term)
                            .Compile();
            }

            //TODO: The result.TotalSearchResults will yield a max of 100 which is incorrect, this  is an issue 
            // in Examine, it needs to limit the results to 100 but still tell you how many in total
           var result = multiIndexSearchProvider.Search(compiled);
           return result;
        }



        

    }
}
