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
        public int MaxResults { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public OurSearcher(string term, string nodeTypeAlias = null, string orderBy = null, int maxResults = 20)
        {
            Term = term;
            NodeTypeAlias = nodeTypeAlias;
            OrderBy = orderBy;
         
            MaxResults = maxResults;
        }

        public SearchResultModel Search()
        {
            var multiIndexSearchProvider = (MultiIndexSearcher)ExamineManager.Instance.SearchProviderCollection["MultiIndexSearcher"];
            multiIndexSearchProvider.EnableLeadingWildcards = true;

            var criteria = multiIndexSearchProvider.CreateSearchCriteria();

            var sb = new StringBuilder();
            
            if (string.IsNullOrEmpty(NodeTypeAlias) == false)
            {
                //if node type alias is specified, make it a MUST
                sb.Append("+nodeTypeAlias:" + NodeTypeAlias);

                //if the term is also specified then  group the next queries as a sub MUST query
                if (!string.IsNullOrEmpty(Term))
                {
                    sb.Append(" +(");
                }
            }

            if (!string.IsNullOrEmpty(Term))
            {
                Term = Term.Replace(" OR ", " ").Replace(" or ", " ").Trim('*');
                // Replace double whitespaces with single space as they were giving errors
                Term = Regex.Replace(Term, @"\s{2,}", " ");
                sb.Append(string.Format("nodeName:*{0}* body:*{0}* ", Term));
            }

            //if the node type alias and term is specified we need to close the sub query
            if (!string.IsNullOrEmpty(NodeTypeAlias) && !string.IsNullOrEmpty(Term))
            {
                sb.Append(")");
            }

            criteria.RawQuery(sb.ToString());

            if (string.IsNullOrEmpty(OrderBy) == false)
            {
                criteria.OrderByDescending(OrderBy);
            }

            var watch = new Stopwatch();
            watch.Start();

            //TODO: The result.TotalSearchResults will yield a max of 100 which is incorrect, this  is an issue 
            // in Examine, it needs to limit the results to 100 but still tell you how many in total
            var result = multiIndexSearchProvider.Search(criteria, MaxResults);
            watch.Stop();

            
            return new SearchResultModel(result, watch.ElapsedMilliseconds, Term, OrderBy);
        }





    }
}
