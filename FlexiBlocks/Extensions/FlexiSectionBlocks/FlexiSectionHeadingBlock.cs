using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// <para>Represents a <see cref="FlexiSectionBlock"/>'s heading.</para>
    /// <para>A <see cref="FlexiSectionBlock"/> is demarcated by an ATX heading. The inline content of the ATX heading must be processed as per normal. 
    /// For this to happen, the contents of the ATX heading must be assigned to a <see cref="LeafBlock"/>.</para>
    /// <para><see cref="FlexiSectionHeadingBlock"/> serves as the leaf block for containing the ATX heading's inline content.</para>
    /// </summary>
    public class FlexiSectionHeadingBlock : LeafBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiSectionHeadingBlock"/>.
        /// </summary>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiSectionHeadingBlock"/>.</param>
        public FlexiSectionHeadingBlock(BlockParser blockParser) : base(blockParser)
        {
            ProcessInlines = true;
        }

        /// <summary>
        /// Gets or sets the ID generated from the <see cref="FlexiSectionHeadingBlock"/>'s content.
        /// </summary>
        public string GeneratedID { get; set; }
    }
}
