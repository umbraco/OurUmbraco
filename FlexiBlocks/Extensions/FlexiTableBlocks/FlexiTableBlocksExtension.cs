using System;
using System.Collections.Generic;
using System.Linq;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    // Consider allowing card type tables to have col/rowspan. colspan will become rowspan in card, rowspan should not change anything (entire content of cell used as label).
    /// <summary>
    /// A markdig extension for <see cref="FlexiTableBlock"/>s.
    /// </summary>
    public class FlexiTableBlocksExtension : BlockExtension<FlexiTableBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiTableBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiTableBlockParsers">The <see cref="ProxyBlockParser{TMain, TProxy}"/>s for creating <see cref="FlexiTableBlock"/>s from tables in markdown.</param>
        /// <param name="flexiTableBlockRenderer">The <see cref="BlockRenderer{T}"/> for rendering <see cref="FlexiTableBlock"/>s as HTML.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiTableBlockParsers"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiTableBlockRenderer"/> is <c>null</c>.</exception>
        public FlexiTableBlocksExtension(IEnumerable<ProxyBlockParser<FlexiTableBlock, ProxyTableBlock>> flexiTableBlockParsers,
            BlockRenderer<FlexiTableBlock> flexiTableBlockRenderer) :
            base(flexiTableBlockRenderer, flexiTableBlockParsers)
        {
            if (flexiTableBlockParsers == null)
            {
                throw new ArgumentNullException(nameof(flexiTableBlockParsers));
            }

            if (!flexiTableBlockParsers.Any()) // A bit of flexiblity here, user could add/remove parser services. This extension would not make sense with no parsers though.
            {
                throw new ArgumentException(nameof(flexiTableBlockParsers));
            }

            if (flexiTableBlockRenderer == null)
            {
                throw new ArgumentNullException(nameof(flexiTableBlockRenderer));
            }
        }
    }
}
