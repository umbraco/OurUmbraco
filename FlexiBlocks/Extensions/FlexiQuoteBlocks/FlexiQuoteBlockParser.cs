using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiQuoteBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiQuoteBlock"/>s from markdown.
    /// </summary>
    public class FlexiQuoteBlockParser : MultipartBlockParser<FlexiQuoteBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiQuoteBlockParser"/>.
        /// </summary>
        /// <param name="flexiQuoteBlockFactory">The factory for building <see cref="FlexiQuoteBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiQuoteBlockFactory"/> is <c>null</c>.</exception>
        public FlexiQuoteBlockParser(IMultipartBlockFactory<FlexiQuoteBlock> flexiQuoteBlockFactory) :
            base(flexiQuoteBlockFactory, "quote", new PartType[] { PartType.Container, PartType.Leaf })
        {
        }
    }
}
