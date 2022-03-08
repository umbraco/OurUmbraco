using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// Represents a collection of tabs and the panels they point to.
    /// </summary>
    public class FlexiTabsBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiTabsBlock"/>.
        /// </summary>
        /// <param name="blockName">The <see cref="FlexiTabsBlock"/>'s BEM block name.</param>
        /// <param name="attributes">HTML attributes for the <see cref="FlexiTabsBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiTabsBlock"/>.</param>
        public FlexiTabsBlock(string blockName,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            BlockName = blockName;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the <see cref="FlexiTabsBlock"/>'s BEM block name.
        /// </summary>
        public virtual string BlockName { get; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiTabsBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }
    }
}
