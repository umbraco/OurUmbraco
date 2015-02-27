using System.IO;
using Lucene.Net.Analysis;

namespace our.Examine
{
    /// <summary>
    /// Custom version field analyzer which uses the Version Tokenizer
    /// </summary>
    public class VersionAnalyzer : Analyzer
    {
        /// <summary>
        /// Returns a VersionTokenizer
        /// </summary>
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var tokenizer = new VersionTokenizer(reader);
            return tokenizer;
        }
    }
}