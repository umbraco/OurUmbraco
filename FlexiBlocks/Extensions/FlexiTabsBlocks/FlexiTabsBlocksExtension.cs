using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// A Markdig extension for <see cref="FlexiTabBlock"/>s.
    /// </summary>
    public class FlexiTabsBlocksExtension : BlockExtension<FlexiTabsBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiTabsBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiTabsBlockParser">The <see cref="BlockParser{T}"/> for parsing <see cref="FlexiTabsBlock"/>s from markdown.</param>
        /// <param name="flexiTabBlockParser">The <see cref="BlockParser{T}"/> for parsing <see cref="FlexiTabBlock"/>s from markdown.</param>
        /// <param name="flexiTabsBlockRenderer">The <see cref="BlockRenderer{T}"/> for rendering <see cref="FlexiTabsBlock"/>s as HTML.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiTabsBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiTabBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiTabsBlockRenderer"/> is <c>null</c>.</exception>
        public FlexiTabsBlocksExtension(ProxyBlockParser<FlexiTabsBlock, ProxyFlexiTabsBlock> flexiTabsBlockParser,
            BlockParser<FlexiTabBlock> flexiTabBlockParser,
            BlockRenderer<FlexiTabsBlock> flexiTabsBlockRenderer) :
            base(flexiTabsBlockRenderer, flexiTabsBlockParser, flexiTabBlockParser)
        {
            if (flexiTabsBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiTabsBlockParser));
            }

            if (flexiTabBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiTabBlockParser));
            }

            if (flexiTabsBlockRenderer == null)
            {
                throw new ArgumentNullException(nameof(flexiTabsBlockRenderer));
            }
        }
    }
}
