using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// Represents a section.
    /// </summary>
    public class FlexiSectionBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiSectionBlock"/>.
        /// </summary>
        /// <param name="blockName">The <see cref="FlexiSectionBlock"/>'s BEM block name.</param>
        /// <param name="element">The <see cref="FlexiSectionBlock"/> root element type.</param>
        /// <param name="linkIcon">The <see cref="FlexiSectionBlock"/>'s link icon as an HTML fragment.</param>
        /// <param name="renderingMode">The <see cref="FlexiSectionBlock"/>'s rendering mode.</param>
        /// <param name="level">The <see cref="FlexiSectionBlock"/>'s level.</param>
        /// <param name="attributes">The HTML attributes for the <see cref="FlexiSectionBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiSectionBlock"/>.</param>
        public FlexiSectionBlock(string blockName,
            SectioningContentElement element,
            string linkIcon,
            FlexiSectionBlockRenderingMode renderingMode,
            int level,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            BlockName = blockName;
            Element = element;
            LinkIcon = linkIcon;
            RenderingMode = renderingMode;
            Level = level;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the <see cref="FlexiSectionBlock"/>'s BEM block name.
        /// </summary>
        public virtual string BlockName { get; }

        /// <summary>
        /// Gets the <see cref="FlexiSectionBlock"/>'s link icon as an HTML fragment.
        /// </summary>
        public virtual string LinkIcon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiSectionBlock"/>'s level.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiSectionBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }

        /// <summary>
        /// Gets the <see cref="FlexiSectionBlock"/> root element type.
        /// </summary>
        public SectioningContentElement Element { get; }

        /// <summary>
        /// Gets the <see cref="FlexiSectionBlock"/>'s rendering mode.
        /// </summary>
        public FlexiSectionBlockRenderingMode RenderingMode { get; }
    }
}
