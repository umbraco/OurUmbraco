using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks
{
    /// <summary>
    /// A markdig extension for <see cref="FlexiPictureBlock"/>s.
    /// </summary>
    public class FlexiPictureBlocksExtension : BlockExtension<FlexiPictureBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiPictureBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiPictureBlockParser">The <see cref="BlockParser{T}"/> for parsing <see cref="FlexiPictureBlock"/>s from markdown.</param>
        /// <param name="flexiPictureBlockRenderer">The <see cref="BlockRenderer{T}"/> for rendering <see cref="FlexiPictureBlock"/>s as HTML.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiPictureBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiPictureBlockRenderer"/> is <c>null</c>.</exception>
        public FlexiPictureBlocksExtension(ProxyBlockParser<FlexiPictureBlock, ProxyJsonBlock> flexiPictureBlockParser, BlockRenderer<FlexiPictureBlock> flexiPictureBlockRenderer) :
            base(flexiPictureBlockRenderer, flexiPictureBlockParser)
        {
            if (flexiPictureBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiPictureBlockParser));
            }

            if (flexiPictureBlockRenderer == null)
            {
                throw new ArgumentNullException(nameof(flexiPictureBlockRenderer));
            }
        }
    }
}
