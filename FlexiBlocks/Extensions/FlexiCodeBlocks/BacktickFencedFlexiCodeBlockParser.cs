using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// A parser that parses fenced <see cref="FlexiCodeBlock"/>s from markdown.
    /// </summary>
    public class BacktickFencedFlexiCodeBlockParser : FencedBlockParser<FlexiCodeBlock, ProxyFencedLeafBlock>
    {
        /// <summary>
        /// Creates a <see cref="BacktickFencedFlexiCodeBlockParser"/>.
        /// </summary>
        /// <param name="flexiCodeBlockFactory">The factory for creating <see cref="FlexiCodeBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiCodeBlockFactory"/> is <c>null</c>.</exception>
        public BacktickFencedFlexiCodeBlockParser(IFlexiCodeBlockFactory flexiCodeBlockFactory) :
            base(flexiCodeBlockFactory, '`', fenceTrailingCharacters: FenceTrailingCharacters.AllButFenceCharacter)
        {
        }
    }
}
