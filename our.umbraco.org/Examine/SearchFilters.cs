using System.Collections.Generic;
using System.Text;
using Examine.SearchCriteria;

namespace our.Examine
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

        public string GetLuceneFilter()
        {
            var sb = new StringBuilder();
            foreach (var filter in Filters)
            {
                switch (_booleanOperation)
                {
                    case BooleanOperation.And:
                        sb.Append("+");
                        break;                  
                    case BooleanOperation.Not:
                        sb.Append("-");
                        break;                    
                }
                sb.AppendFormat("{0}:{1} ", filter.FieldName, filter.Value);
            }
            return sb.ToString();
        }
    }
}