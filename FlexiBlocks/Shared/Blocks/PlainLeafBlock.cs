using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Represents a plain leaf <see cref="IBlock"/>.
    /// </summary>
    public class PlainLeafBlock : LeafBlock
    {
        /// <summary>
        /// Creates a <see cref="PlainLeafBlock"/>.
        /// </summary>
        /// <param name="parser">The <see cref="BlockParser"/> parsing the <see cref="PlainLeafBlock"/>.</param>
        public PlainLeafBlock(BlockParser parser) : base(parser)
        {
            ProcessInlines = true;
        }
    }
}
