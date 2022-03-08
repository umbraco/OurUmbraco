using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Represents a plain container <see cref="IBlock"/>.
    /// </summary>
    public class PlainContainerBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="PlainContainerBlock"/>.
        /// </summary>
        /// <param name="parser">The <see cref="BlockParser"/> parsing the <see cref="PlainContainerBlock"/>.</param>
        public PlainContainerBlock(BlockParser parser) : base(parser)
        {
        }
    }
}
