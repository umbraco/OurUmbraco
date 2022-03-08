using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiOptionsBlock"/>s from markdown.
    /// </summary>
    public class FlexiOptionsBlockParser : JsonBlockParser<FlexiOptionsBlock, ProxyJsonBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiOptionsBlockParser"/>.
        /// </summary>
        /// <param name="flexiOptionsBlockFactory">The factory for building <see cref="FlexiOptionsBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiOptionsBlockFactory"/> is <c>null</c>.</exception>
        public FlexiOptionsBlockParser(IJsonBlockFactory<FlexiOptionsBlock, ProxyJsonBlock> flexiOptionsBlockFactory) : base(flexiOptionsBlockFactory)
        {
            OpeningCharacters = new[] { 'o' };
        }
    }
}
