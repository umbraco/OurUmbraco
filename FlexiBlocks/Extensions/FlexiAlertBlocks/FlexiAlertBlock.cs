using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks
{
    /// <summary>
    /// Represents an alert containing content you'd like to draw readers attention to, such as warnings and important information.
    /// </summary>
    public class FlexiAlertBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiAlertBlock"/>.
        /// </summary>
        /// <param name="blockName">The <see cref="FlexiAlertBlock"/>'s BEM block name.</param>
        /// <param name="type">The <see cref="FlexiAlertBlock"/>'s type.</param>
        /// <param name="icon">The <see cref="FlexiAlertBlock"/>'s icon as an HTML fragment.</param>
        /// <param name="attributes">HTML attributes for the <see cref="FlexiAlertBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiAlertBlock"/>.</param>
        public FlexiAlertBlock(string blockName,
            string type,
            string icon,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            BlockName = blockName;
            Type = type;
            Icon = icon;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the <see cref="FlexiAlertBlock"/>'s BEM block name.
        /// </summary>
        public virtual string BlockName { get; }

        /// <summary>
        /// Gets the <see cref="FlexiAlertBlock"/>'s type.
        /// </summary>
        public virtual string Type { get; }

        /// <summary>
        /// Gets the <see cref="FlexiAlertBlock"/>'s icon as an HTML fragment.
        /// </summary>
        public virtual string Icon { get; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiAlertBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }
    }
}
