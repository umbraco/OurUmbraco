using System.Collections.Generic;
using System.Text;
using Examine.SearchCriteria;

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

        public string GetLuceneAddFilters()
        {
            var sb = new StringBuilder();
            sb.Append("+(");

            foreach (var filter in Filters)
            {
                if (_booleanOperation != BooleanOperation.Not)
                {
                    sb.Append("+");
                    sb.AppendFormat("{0}:{1} ", filter.FieldName, filter.Value);                 
                }
            }

            sb.Append(")");

            return sb.ToString();
        }

        public string GetLuceneExcludeFilters()
        {
            var sb = new StringBuilder();
            foreach (var filter in Filters)
            {
                if (_booleanOperation == BooleanOperation.Not)
                {
                    sb.Append("-");
                    sb.AppendFormat("{0}:{1} ", filter.FieldName, filter.Value);
                }
            }
            return sb.ToString();
        }
    }
}