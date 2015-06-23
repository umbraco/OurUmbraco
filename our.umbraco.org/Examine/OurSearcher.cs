﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Examine;
using Examine.LuceneEngine.Providers;
using our.Models;

namespace our.Examine
{
    public class OurSearcher
    {

        public string Term { get; private set; }
        public string NodeTypeAlias { get; set; }
        public string OrderBy { get; set; }
        public int MaxResults { get; set; }
        public IEnumerable<SearchFilters> Filters { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public OurSearcher(string term, string nodeTypeAlias = null, string orderBy = null, int maxResults = 20, IEnumerable<SearchFilters> filters = null)
        {
            Term = term;
            NodeTypeAlias = nodeTypeAlias;
            OrderBy = orderBy;
         
            MaxResults = maxResults;
            Filters = filters ?? Enumerable.Empty<SearchFilters>();
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

                //if the term or filter is also specified then  group the next queries as a sub MUST query
                if (!string.IsNullOrEmpty(Term) || Filters.Any())
                {
                    sb.Append(" +(");
                }
            }

            if (Filters.Any())
            {
                //If there is a filter applied to the entire result then add it here, this is a MUST sub query
                sb.Append("+(");
                foreach (var filter in Filters)
                {
                    sb.Append(filter.GetLuceneFilter());
                }
                sb.Append(")");
            }

            if (!string.IsNullOrEmpty(Term))
            {
                Term = Term.Replace(" OR ", " ").Replace(" or ", " ").Trim('*').Trim('\'').Trim('"');
                // Replace double whitespaces with single space as they were giving errors
                Term = Regex.Replace(Term, @"\s{2,}", " ");

                //now we need to split the phrase into individual terms so the query parser can understand
                var split = Term.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length > 0)
                {
                    //do an exact phrase match with boost
                    sb.AppendFormat("(nodeName:\"{0}\" body:\"{0}\")^100 ", Term);
                    
                    //do standard match with boost on each term
                    sb.Append("(");
                    foreach (var s in split)
                    {
                        sb.AppendFormat("nodeName:{0} body:{0} ", s);    
                    }
                    sb.Append(")^50 ");

                    //do prefix/suffix phrase with wildcards
                    sb.Append("(");
                    foreach (var s in split)
                    {
                        sb.AppendFormat("nodeName:*{0}* body:*{0}* ", s);
                    }
                    sb.Append(") ");

                }

            }

            //if the node type alias and (term or filter) is specified we need to close the sub query
            if (!string.IsNullOrEmpty(NodeTypeAlias) && (!string.IsNullOrEmpty(Term) || Filters.Any()))
            {
                sb.Append(")");
            }

            if (sb.Length == 0)
            {
                return new SearchResultModel(new EmptySearchResults(), 0, "", "");
            }

            criteria.RawQuery(sb.ToString());

            if (string.IsNullOrEmpty(OrderBy) == false)
            {
                criteria.OrderByDescending(OrderBy);
            }

            var watch = new Stopwatch();
            watch.Start();

            var result = multiIndexSearchProvider.Search(criteria, MaxResults);
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
