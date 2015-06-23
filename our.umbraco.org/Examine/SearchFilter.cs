namespace our.Examine
{
    public class SearchFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SearchFilter(string fieldName, string value)
        {
            FieldName = fieldName;
            Value = value;
        }

        public string FieldName { get; private set; }
        public string Value { get; private set; }

        public string GetLuceneFilter()
        {
            return string.Format("{0}:{1}", FieldName, Value);
        }
    }
}