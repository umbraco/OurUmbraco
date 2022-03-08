using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiTabsBlock"/>s from markdown.
    /// </summary>
    public class FlexiTabsBlockParser : FencedBlockParser<FlexiTabsBlock, ProxyFlexiTabsBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiTabsBlockParser"/>.
        /// </summary>
        /// <param name="flexiTabsBlockFactory">The factory for creating <see cref="FlexiTabsBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiTabsBlockFactory"/> is <c>null</c>.</exception>
        public FlexiTabsBlockParser(IFencedBlockFactory<FlexiTabsBlock, ProxyFlexiTabsBlock> flexiTabsBlockFactory) :
            base(flexiTabsBlockFactory, '/', true, FenceTrailingCharacters.Whitespace)
        {
        }
    }
}
