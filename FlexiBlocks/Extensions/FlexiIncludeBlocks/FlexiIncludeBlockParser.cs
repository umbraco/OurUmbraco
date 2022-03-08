using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiIncludeBlock"/>s from markdown.
    /// </summary>
    public class FlexiIncludeBlockParser : JsonBlockParser<FlexiIncludeBlock, ProxyJsonBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiIncludeBlockParser"/>.
        /// </summary>
        /// <param name="flexiIncludeBlockFactory">The factory for building <see cref="FlexiIncludeBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiIncludeBlockFactory"/> is <c>null</c>.</exception>
        public FlexiIncludeBlockParser(IJsonBlockFactory<FlexiIncludeBlock, ProxyJsonBlock> flexiIncludeBlockFactory) : base(flexiIncludeBlockFactory)
        {
            OpeningCharacters = new[] { 'i' };
        }
    }
}
