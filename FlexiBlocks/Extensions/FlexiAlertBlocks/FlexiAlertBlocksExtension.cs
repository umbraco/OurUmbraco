using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks
{
    /// <summary>
    /// A Markdig extension for <see cref="FlexiAlertBlock"/>s.
    /// </summary>
    public class FlexiAlertBlocksExtension : BlockExtension<FlexiAlertBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiAlertBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiAlertBlockParser">The <see cref="BlockParser{T}"/> for parsing <see cref="FlexiAlertBlock"/>s from markdown.</param>
        /// <param name="flexiAlertBlockRenderer">The <see cref="BlockRenderer{T}"/> for rendering <see cref="FlexiAlertBlock"/>s as HTML.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiAlertBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiAlertBlockRenderer"/> is <c>null</c>.</exception>
        public FlexiAlertBlocksExtension(BlockParser<FlexiAlertBlock> flexiAlertBlockParser, BlockRenderer<FlexiAlertBlock> flexiAlertBlockRenderer) :
            base(flexiAlertBlockRenderer, flexiAlertBlockParser)
        {
            if (flexiAlertBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiAlertBlockParser));
            }

            if (flexiAlertBlockRenderer == null)
            {
                throw new ArgumentNullException(nameof(flexiAlertBlockRenderer));
            }
        }
    }
}
