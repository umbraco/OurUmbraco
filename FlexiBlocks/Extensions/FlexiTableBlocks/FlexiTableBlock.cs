using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// Represents a table.
    /// </summary>
    public class FlexiTableBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiTableBlock"/>.
        /// </summary>
        /// <param name="blockName">The <see cref="FlexiTableBlock"/>'s BEM block name.</param>
        /// <param name="type">The <see cref="FlexiTableBlock"/>'s type.</param>
        /// <param name="attributes">HTML attributes for the <see cref="FlexiTableBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiTableBlock"/>.</param>
        public FlexiTableBlock(string blockName,
            FlexiTableType type,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            BlockName = blockName;
            Type = type;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the <see cref="FlexiTableBlock"/>'s BEM block name.
        /// </summary>
        public virtual string BlockName { get; }

        /// <summary>
        /// Gets the <see cref="FlexiTableBlock"/>'s type.
        /// </summary>
        public virtual FlexiTableType Type { get; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiTableBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }
    }
}
