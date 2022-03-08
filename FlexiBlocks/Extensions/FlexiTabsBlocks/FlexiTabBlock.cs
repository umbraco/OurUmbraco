using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// Represents a tab and the panel it points to.
    /// </summary>
    public class FlexiTabBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiTabBlock"/>.
        /// </summary>
        /// <param name="attributes">HTML attributes for the <see cref="FlexiTabBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiTabBlock"/>.</param>
        public FlexiTabBlock(ReadOnlyDictionary<string, string> attributes, BlockParser blockParser) : base(blockParser)
        {
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiTabBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }
    }
}
