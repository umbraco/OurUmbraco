using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Lucene.Net.Analysis.Standard;
using OurUmbraco.Our.Extensions;
using OurUmbraco.Our.Models;
using Umbraco.Core;
using System.Configuration;

namespace OurUmbraco.Our.Examine
{
    public class OurSearcher
    {
        public string Term { get; private set; }
        public int? MajorDocsVersion { get; set; }
        public string NodeTypeAlias { get; set; }
        public string OrderBy { get; set; }
        public int MaxResults { get; set; }
        public IEnumerable<SearchFilters> Filters { get; set; }

        public OurSearcher(string term, string nodeTypeAlias = null,
            int? majorDocsVersion = null,
            string orderBy = null,
            int maxResults = 20,
            IEnumerable<SearchFilters> filters = null)
        {
            Term = term.MakeSearchQuerySafe();
            MajorDocsVersion = majorDocsVersion;
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
        private ISearchCriteria GetSearchCriteria(BaseLuceneSearcher searcher)
        {
            var criteria = (LuceneSearchCriteria)searcher.CreateSearchCriteria();

            //check if there's anything to process
            if (NodeTypeAlias.IsNullOrWhiteSpace() && Term.IsNullOrWhiteSpace() && !Filters.Any())
                return null;


            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(NodeTypeAlias) == false)
            {
                sb.AppendFormat("+nodeTypeAlias: {0} ", NodeTypeAlias);
            }

            // Three possiblities:
            // * docs version (MajorDocsVersion) supplied, give current version and NEGATE OTHERS
            // * no docs version (MajorDocsVersion) is not suplied, use it and NEGATE others
            // * all versions are requests, this is currently not implemented
            var currentMajorVersions = new string[] { "6", "7", "8" };

            // add mandatory majorVersion is parameter is supplied
            string versionToFilterBy = MajorDocsVersion == null
                ? ConfigurationManager.AppSettings[Constants.AppSettings.DocumentationCurrentMajorVersion]
                : MajorDocsVersion.ToString();

            //we filter by this version by excluding the other major versions in lucene so
            if (NodeTypeAlias.InvariantEquals("documentation") == false)
            {
                var versionsToNegate = currentMajorVersions.Where(f => f != versionToFilterBy).ToArray<string>();
                foreach (var versionToNegate in versionsToNegate)
                {
                    sb.AppendFormat("-majorVersion:{0} ", versionToNegate);
                }
            }

            // do it the other way around for documentation
            if (NodeTypeAlias.InvariantEquals("documentation"))
            {
                //we filter by this version by using the major versions
                var versionToFind = currentMajorVersions.Where(f => f == versionToFilterBy).ToArray<string>();
                foreach (var versionToNegate in versionToFind)
                {
                    sb.AppendFormat("+majorVersion:{0} ", versionToNegate);
                }
            }

            if (!string.IsNullOrEmpty(Term))
            {
                sb.Append("+(");
                // Cleanup the term so there are no errors
                Term = Term
                    .Replace("\"", string.Empty)
                    .Replace(":", string.Empty)
                    .Replace("\\", string.Empty)
                    .Trim('*');
                // Replace OR's with case insensitive matching
                Term = Regex.Replace(Term, @" OR ", " ", RegexOptions.IgnoreCase);
                // Replace double whitespaces with single space as they were giving errors
                Term = Regex.Replace(Term, @"\s{2,}", " ");

                // Do an exact phrase match with boost
                sb.AppendFormat("nodeName:\"{0}\"^20000 body:\"{0}\"^5000 ", Term);

                // SEARCH YAML stuff
                sb.AppendFormat("tags:\"{0}\"^20000 ", Term);
                sb.AppendFormat("keywords:\"{0}\"^20000 ", Term);

                // Now we need to split the phrase into individual terms so the query parser can understand
                var split = Term.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                //now de-duplicate the terms and remove the stop words and don't include single chars
                var deduped = new List<string>();
                foreach (var s in split)
                {
                    if (s.Length > 1
                        && !deduped.Contains(s, StringComparer.InvariantCultureIgnoreCase)
                        && !StandardAnalyzer.STOP_WORDS_SET.Contains(s))
                    {
                        deduped.Add(s);
                    }
                }

                if (deduped.Count > 20)
                {
                    //truncate, we don't want to search on all of these individually
                    deduped = deduped.Take(20).ToList();
                }

                if (deduped.Count > 0)
                {
                    //do standard match with boost on each term
                    foreach (var s in deduped)
                    {
                        sb.AppendFormat("nodeName:{0}^10000 body:{0}^50 ", s);
                    }

                    //do suffix with wildcards
                    foreach (var s in deduped)
                    {
                        sb.AppendFormat("nodeName:{0}*^1000 body:{0}* ", s);
                    }

                    //do fuzzy (close match 0.9)
                    foreach (var s in deduped)
                    {
                        sb.AppendFormat("nodeName:{0}~0.9^0.1 body:{0}~0.9^0.1 ", s);
                    }
                }
                sb.Append(")");

            }

            //nothing to process, return
            if (sb.Length > 0)
            {
                //render out the raw query that was constructed above
                criteria = (LuceneSearchCriteria)criteria.RawQuery(sb.ToString());
            }

            //Now we can apply any filters, this is done by using native Lucene query objects
            if (Filters.Any())
            {
                //If there is a filter applied to the entire result then add it here, this is a MUST sub query
                foreach (var filter in Filters)
                {
                    filter.ProcessLuceneAddFilters(searcher, criteria);
                }

                //need to process the excludes after - since that is how lucene works, you can only exclude after you've included
                foreach (var filter in Filters)
                {
                    filter.ProcessLuceneExcludeFilters(searcher, criteria);
                }
            }

            if (string.IsNullOrEmpty(OrderBy) == false)
            {
                criteria.OrderByDescending(OrderBy);
            }

            return criteria;
        }

        public SearchResultModel Search(string searcherName = null, int skip = 0, bool populateUrls = true)
        {
            var multiIndexSearchProvider = (BaseLuceneSearcher)ExamineManager.Instance.SearchProviderCollection[
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
