using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// A markdig extension for <see cref="FlexiIncludeBlock"/>s.
    /// </summary>
    public class FlexiIncludeBlocksExtension : BlockExtension<FlexiIncludeBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiIncludeBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiIncludeBlockParser">The <see cref="ProxyBlockParser{TMain, TProxy}"/> for creating <see cref="FlexiIncludeBlock"/>s from markdown.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiIncludeBlockParser"/> is <c>null</c>.</exception>
        public FlexiIncludeBlocksExtension(ProxyBlockParser<FlexiIncludeBlock, ProxyJsonBlock> flexiIncludeBlockParser) : base(null, flexiIncludeBlockParser)
        {
            if (flexiIncludeBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiIncludeBlockParser));
            }
        }
    }
}
