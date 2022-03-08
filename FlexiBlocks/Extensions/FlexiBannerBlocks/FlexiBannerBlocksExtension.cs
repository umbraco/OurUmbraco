using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiBannerBlocks
{
    /// <summary>
    /// A Markdig extension for <see cref="FlexiBannerBlock"/>s.
    /// </summary>
    public class FlexiBannerBlocksExtension : BlockExtension<FlexiBannerBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiBannerBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiBannerBlockParser">The <see cref="BlockParser{T}"/> for parsing <see cref="FlexiBannerBlock"/>s from markdown.</param>
        /// <param name="flexiBannerBlockRenderer">The <see cref="BlockRenderer{T}"/> for rendering <see cref="FlexiBannerBlock"/>s as HTML.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiBannerBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiBannerBlockRenderer"/> is <c>null</c>.</exception>
        public FlexiBannerBlocksExtension(BlockParser<FlexiBannerBlock> flexiBannerBlockParser, BlockRenderer<FlexiBannerBlock> flexiBannerBlockRenderer) :
            base(flexiBannerBlockRenderer, flexiBannerBlockParser)
        {
            if (flexiBannerBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiBannerBlockParser));
            }

            if (flexiBannerBlockRenderer == null)
            {
                throw new ArgumentNullException(nameof(flexiBannerBlockRenderer));
            }
        }
    }
}
