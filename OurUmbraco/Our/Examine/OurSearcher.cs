using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.SearchCriteria;
using Lucene.Net.QueryParsers;
using OurUmbraco.Our.Models;

namespace OurUmbraco.Our.Examine
{
    public class OurSearcher
    {

        public string Term { get; private set; }
        public string NodeTypeAlias { get; set; }
        public string OrderBy { get; set; }
        public int MaxResults { get; set; }
        public IEnumerable<SearchFilters> Filters { get; set; }
        
        public OurSearcher(string term, string nodeTypeAlias = null, string orderBy = null, int maxResults = 20, IEnumerable<SearchFilters> filters = null)
        {
            Term = string.IsNullOrWhiteSpace(term) ? term : QueryParser.Escape(term);
            NodeTypeAlias = nodeTypeAlias;
            OrderBy = orderBy;
         
            MaxResults = maxResults;
            Filters = filters ?? Enumerable.Empty<SearchFilters>();
        }

        /// <summary>
        /// Generates the search criteria that we want to search on
        /// </summary>
        /// <param name="searcher"></param>
        /// <returns></returns>
        public ISearchCriteria GetSearchCriteria(ISearcher searcher)
        {            
            var criteria = searcher.CreateSearchCriteria();

            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(NodeTypeAlias) == false)
            {
                //if node type alias is specified, make it a MUST
                sb.Append("+nodeTypeAlias:" + NodeTypeAlias);

                //if the term or filter is also specified then  group the next queries as a sub MUST query
                if (!string.IsNullOrEmpty(Term) || Filters.Any())
                {
                    sb.Append(" +(");
                }
            }

            if (!string.IsNullOrEmpty(Term))
            {
                //Cleanup the term so there are no errors
                Term = Term.Replace("\"", string.Empty)
                    .Replace(":", string.Empty)
                    .Replace("\\", string.Empty).Trim('*');
                //replace OR's with case insensitive matching
                Term = Regex.Replace(Term, @" OR ", " ", RegexOptions.IgnoreCase);
                // Replace double whitespaces with single space as they were giving errors
                Term = Regex.Replace(Term, @"\s{2,}", " ");

                //now we need to split the phrase into individual terms so the query parser can understand
                var split = Term.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length > 1)
                {
                    //do an exact phrase match with boost
                    sb.AppendFormat("nodeName:\"{0}\"^200 body:\"{0}\"^50 ", Term);
                }

                if (split.Length > 0)
                {
                    //do standard match with boost on each term
                    foreach (var s in split)
                    {
                        sb.AppendFormat("nodeName:{0}^100 body:{0}^50 ", s);
                    }

                    //do suffix with wildcards
                    foreach (var s in split)
                    {
                        sb.AppendFormat("nodeName:{0}*^0.9 body:{0}*^0.5 ", s);
                    }
                }
            }

            //if the node type alias and (term or filter) is specified we need to close the sub query
            if (!string.IsNullOrEmpty(NodeTypeAlias) && (!string.IsNullOrEmpty(Term) || Filters.Any()))
            {
                sb.Append(")");
            }

            if (sb.Length == 0)
            {
                return null;
            }

            if (Filters.Any())
            {
                //If there is a filter applied to the entire result then add it here, this is a MUST sub query

                foreach (var filter in Filters)
                {
                    sb.Append(filter.GetLuceneAddFilters());
                }


                foreach (var filter in Filters)
                {
                    sb.Append(filter.GetLuceneExcludeFilters());
                }
            }


            var query = sb.ToString().Replace("+()", string.Empty);
            criteria.RawQuery(query);

            if (string.IsNullOrEmpty(OrderBy) == false)
            {
                criteria.OrderByDescending(OrderBy);
            }

            return criteria;
        }

        public SearchResultModel Search(string searcherName = null, int skip = 0, bool populateUrls = true)
        {
            var multiIndexSearchProvider = ExamineManager.Instance.SearchProviderCollection[
                string.IsNullOrWhiteSpace(searcherName) ? "MultiIndexSearcher" : searcherName];

            var criteria = GetSearchCriteria(multiIndexSearchProvider);

            if (criteria == null)
            {
                return new SearchResultModel(new EmptySearchResults(), 0, "", "");
            }

            var watch = new Stopwatch();
            watch.Start();

            var result = multiIndexSearchProvider.Search(criteria, MaxResults);

            if (populateUrls)
            {
                foreach (var searchResult in result.Skip(skip))
                {
                    if (searchResult.Fields.ContainsKey("url") == false)
                        searchResult.Fields["url"] = searchResult.FullUrl();
                }
            }
            
            watch.Stop();

            
            return new SearchResultModel(result, watch.ElapsedMilliseconds, Term, string.IsNullOrEmpty(OrderBy) ? "score" : OrderBy)
            {
                //NOTE: used for debugging
                LuceneQuery = criteria.ToString()
            };
        }


        private class EmptySearchResults : ISearchResults
        {

            private readonly List<SearchResult> _results = new List<SearchResult>(); 

            public IEnumerator<SearchResult> GetEnumerator()
            {
                return _results.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerable<SearchResult> Skip(int skip)
            {
                return _results.Skip(skip);
            }

            public int TotalItemCount
            {
                get { return 0; }
            }
        }


    }
}
