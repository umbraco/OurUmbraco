using System.Collections.Generic;
using System.Text;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace OurUmbraco.Our.Examine
{
    public class SearchFilters
    {
        private readonly BooleanOperation _booleanOperation;

        public SearchFilters(BooleanOperation booleanOperation)
        {
            _booleanOperation = booleanOperation;
            Filters = new List<SearchFilter>();
        }

        public List<SearchFilter> Filters { get; private set; }

        public void ProcessLuceneAddFilters(BaseLuceneSearcher searcher, LuceneSearchCriteria luceneSearchCriteria)
        {
            var queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "", searcher.IndexingAnalyzer);
            var bQuery = new BooleanQuery();
            var occur = _booleanOperation == BooleanOperation.And ? BooleanClause.Occur.MUST : BooleanClause.Occur.SHOULD;
            
            foreach (var filter in Filters)
            {
                if (_booleanOperation == BooleanOperation.Not) continue;

                //a filter can return a true lucene query, if there is one use it, otherwise parse it's string format
                var luceneQueryObj = filter.GetLuceneQuery();
                bQuery.Add(luceneQueryObj ?? queryParser.Parse(filter.ToString()), occur);
            }

            luceneSearchCriteria.LuceneQuery(bQuery);
        }

        public void ProcessLuceneExcludeFilters(BaseLuceneSearcher searcher, LuceneSearchCriteria luceneSearchCriteria)
        {
            var queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "", searcher.IndexingAnalyzer);
            var bQuery = new BooleanQuery();
            
            //var sb = new StringBuilder();
            foreach (var filter in Filters)
            {
                if (_booleanOperation != BooleanOperation.Not) continue;

                //a filter can return a true lucene query, if there is one use it, otherwise parse it's string format
                var luceneQueryObj = filter.GetLuceneQuery();
                bQuery.Add(luceneQueryObj ?? queryParser.Parse(filter.ToString()), BooleanClause.Occur.MUST_NOT);                
            }            
        }
    }
}