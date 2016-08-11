using Lucene.Net.Search;

namespace OurUmbraco.Our.Examine
{
    public class RangeSearchFilter : SearchFilter
    {
        public long To { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RangeSearchFilter(string fieldName, long from, long to) : base(fieldName, from)
        {
            To = to;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            //see https://lucene.apache.org/core/2_9_4/queryparsersyntax.html#Range Searches
            return string.Format("{0}:[{1} TO {2}]", FieldName, Value, To);
        }

        /// <summary>
        /// Can be used to return a true LUcene query object if the string format is not good enough
        /// </summary>
        /// <returns></returns>
        public override Query GetLuceneQuery()
        {
            return NumericRangeQuery.NewLongRange(FieldName, (long)Value, To, true, true);
        }
    }
}