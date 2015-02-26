using Examine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Examine.SearchCriteria;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using our.Models;

namespace our.Examine
{
    public class OurSearcher
    {

        public string Term { get; private set; }
        public string NodeTypeAlias { get; set; }
        public string OrderBy { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public OurSearcher(string term, string nodeTypeAlias = null, string orderBy = null)
        {
            if (string.IsNullOrWhiteSpace(term)) throw new ArgumentNullException("term", "term cannot be empty");

            Term = term;
            NodeTypeAlias = nodeTypeAlias;
            OrderBy = orderBy;
        }

        public SearchResultModel Search()
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

            var watch = new Stopwatch();
            watch.Start();

            //TODO: The result.TotalSearchResults will yield a max of 100 which is incorrect, this  is an issue 
            // in Examine, it needs to limit the results to 100 but still tell you how many in total
            var result = multiIndexSearchProvider.Search(compiled, 100);

            watch.Stop();

            return new SearchResultModel(result, watch.ElapsedMilliseconds, Term, OrderBy);
        }





    }
}
