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
            //the compat versions include an 'x' so we'll allow it
            if (c == 'x') return true;
            //TODO: remove this when we start indexing versions with multiple values in the same field, see TODO in ProjectIndexer_DocumentWriting
            // until then we'll also accept commas since that is what it is delimited by.
            if (c == ',') return true;
            return false;
        }
    }
}