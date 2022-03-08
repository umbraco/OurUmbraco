using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiCardsBlock"/>s from markdown.
    /// </summary>
    public class FlexiCardsBlockParser : FencedBlockParser<FlexiCardsBlock, ProxyFlexiCardsBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiCardsBlockParser"/>.
        /// </summary>
        /// <param name="flexiCardsBlockFactory">The factory for creating <see cref="FlexiCardsBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiCardsBlockFactory"/> is <c>null</c>.</exception>
        public FlexiCardsBlockParser(IFencedBlockFactory<FlexiCardsBlock, ProxyFlexiCardsBlock> flexiCardsBlockFactory) :
            base(flexiCardsBlockFactory, '[', true, FenceTrailingCharacters.Whitespace)
        {
        }
    }
}
