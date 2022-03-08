using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks
{
    /// <summary>
    /// Represents a block containing options for another block.
    /// </summary>
    public class FlexiOptionsBlock : LeafBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiOptionsBlock"/>.
        /// </summary>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiOptionsBlock"/>.</param>
        public FlexiOptionsBlock(BlockParser blockParser) : base(blockParser)
        {
        }
    }
}
