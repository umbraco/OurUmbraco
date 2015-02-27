using System.IO;
using Lucene.Net.Analysis;

namespace our.Examine
{
    /// <summary>
    /// Custom tokenizer to ensure that only version information is indexed
    /// </summary>
    public class VersionTokenizer : CharTokenizer
    {
        public VersionTokenizer(TextReader input) : base(input)
        {
        }

        /// <summary>
        /// Returns true if a character should be included in a token.
        /// </summary>
        /// <remarks>
        /// Only accept numbers and decimal since this is what makes up a version
        /// </remarks>
        protected override bool IsTokenChar(char c)
        {
            if (char.IsNumber(c)) return true;
            if (c == '.') return true;
            return false;
        }
    }
}