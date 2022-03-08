using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks
{
    /// <summary>
    /// A markdig extension for <see cref="FlexiVideoBlock"/>s.
    /// </summary>
    public class FlexiVideoBlocksExtension : BlockExtension<FlexiVideoBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiVideoBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiVideoBlockParser">The <see cref="BlockParser{T}"/> for parsing <see cref="FlexiVideoBlock"/>s from markdown.</param>
        /// <param name="flexiVideoBlockRenderer">The <see cref="BlockRenderer{T}"/> for rendering <see cref="FlexiVideoBlock"/>s as HTML.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiVideoBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiVideoBlockRenderer"/> is <c>null</c>.</exception>
        public FlexiVideoBlocksExtension(ProxyBlockParser<FlexiVideoBlock, ProxyJsonBlock> flexiVideoBlockParser, BlockRenderer<FlexiVideoBlock> flexiVideoBlockRenderer) :
            base(flexiVideoBlockRenderer, flexiVideoBlockParser)
        {
            if (flexiVideoBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiVideoBlockParser));
            }

            if (flexiVideoBlockRenderer == null)
            {
                throw new ArgumentNullException(nameof(flexiVideoBlockRenderer));
            }
        }
    }
}
