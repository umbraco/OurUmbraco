using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// A Markdig extension for <see cref="FlexiCardBlock"/>s.
    /// </summary>
    public class FlexiCardsBlocksExtension : BlockExtension<FlexiCardsBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiCardsBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiCardsBlockParser">The <see cref="BlockParser{T}"/> for parsing <see cref="FlexiCardsBlock"/>s from markdown.</param>
        /// <param name="flexiCardBlockParser">The <see cref="BlockParser{T}"/> for parsing <see cref="FlexiCardBlock"/>s from markdown.</param>
        /// <param name="flexiCardsBlockRenderer">The <see cref="BlockRenderer{T}"/> for rendering <see cref="FlexiCardsBlock"/>s as HTML.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiCardsBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiCardBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiCardsBlockRenderer"/> is <c>null</c>.</exception>
        public FlexiCardsBlocksExtension(ProxyBlockParser<FlexiCardsBlock, ProxyFlexiCardsBlock> flexiCardsBlockParser,
            BlockParser<FlexiCardBlock> flexiCardBlockParser,
            BlockRenderer<FlexiCardsBlock> flexiCardsBlockRenderer) :
            base(flexiCardsBlockRenderer, flexiCardsBlockParser, flexiCardBlockParser)
        {
            if (flexiCardsBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiCardsBlockParser));
            }

            if (flexiCardBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiCardBlockParser));
            }

            if (flexiCardsBlockRenderer == null)
            {
                throw new ArgumentNullException(nameof(flexiCardsBlockRenderer));
            }
        }
    }
}
