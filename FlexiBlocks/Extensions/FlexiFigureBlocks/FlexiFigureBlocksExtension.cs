using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiFigureBlocks
{
    /// <summary>
    /// A Markdig extension for <see cref="FlexiFigureBlock"/>s.
    /// </summary>
    public class FlexiFigureBlocksExtension : BlockExtension<FlexiFigureBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiFigureBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiFigureBlockParser">The <see cref="BlockParser{T}"/> for parsing <see cref="FlexiFigureBlock"/>s from markdown.</param>
        /// <param name="flexiFigureBlockRenderer">The <see cref="BlockRenderer{T}"/> for rendering <see cref="FlexiFigureBlock"/>s as HTML.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiFigureBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiFigureBlockRenderer"/> is <c>null</c>.</exception>
        public FlexiFigureBlocksExtension(BlockParser<FlexiFigureBlock> flexiFigureBlockParser, BlockRenderer<FlexiFigureBlock> flexiFigureBlockRenderer) :
            base(flexiFigureBlockRenderer, flexiFigureBlockParser)
        {
            if (flexiFigureBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiFigureBlockParser));
            }

            if (flexiFigureBlockRenderer == null)
            {
                throw new ArgumentNullException(nameof(flexiFigureBlockRenderer));
            }
        }
    }
}
