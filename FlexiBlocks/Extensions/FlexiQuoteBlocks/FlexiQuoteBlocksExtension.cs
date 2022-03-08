using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiQuoteBlocks
{
    /// <summary>
    /// A Markdig extension for <see cref="FlexiQuoteBlock"/>s.
    /// </summary>
    public class FlexiQuoteBlocksExtension : BlockExtension<FlexiQuoteBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiQuoteBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiQuoteBlockParser">The <see cref="BlockParser{T}"/> for parsing <see cref="FlexiQuoteBlock"/>s from markdown.</param>
        /// <param name="flexiQuoteBlockRenderer">The <see cref="BlockRenderer{T}"/> for rendering <see cref="FlexiQuoteBlock"/>s as HTML.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiQuoteBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiQuoteBlockRenderer"/> is <c>null</c>.</exception>
        public FlexiQuoteBlocksExtension(BlockParser<FlexiQuoteBlock> flexiQuoteBlockParser, BlockRenderer<FlexiQuoteBlock> flexiQuoteBlockRenderer) :
            base(flexiQuoteBlockRenderer, flexiQuoteBlockParser)
        {
            if (flexiQuoteBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiQuoteBlockParser));
            }

            if (flexiQuoteBlockRenderer == null)
            {
                throw new ArgumentNullException(nameof(flexiQuoteBlockRenderer));
            }
        }
    }
}
