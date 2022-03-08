using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks
{
    /// <summary>
    /// A Markdig extension for <see cref="FlexiOptionsBlock"/>s.
    /// </summary>
    public class FlexiOptionsBlocksExtension : BlockExtension<FlexiOptionsBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiOptionsBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiOptionsBlockParser">The <see cref="ProxyBlockParser{TMain, TProxy}"/> for creating <see cref="FlexiOptionsBlock"/>s from markdown.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiOptionsBlockParser"/> is <c>null</c>.</exception>
        public FlexiOptionsBlocksExtension(ProxyBlockParser<FlexiOptionsBlock, ProxyJsonBlock> flexiOptionsBlockParser) : base(null, flexiOptionsBlockParser)
        {
            if (flexiOptionsBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiOptionsBlockParser));
            }
        }
    }
}
