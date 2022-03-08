using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiFigureBlocks
{
    /// <summary>
    /// Represents a figure.
    /// </summary>
    public class FlexiFigureBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiFigureBlock"/>.
        /// </summary>
        /// <param name="blockName">The <see cref="FlexiFigureBlock"/>'s BEM block name.</param>
        /// <param name="name">The <see cref="FlexiFigureBlock"/>'s name.</param>
        /// <param name="renderName">The value specifying whether to render the <see cref="FlexiFigureBlock"/>'s name.</param>
        /// <param name="linkLabelContent">The content of the link label for linking to the <see cref="FlexiFigureBlock"/>.</param>
        /// <param name="id">The <see cref="FlexiFigureBlock"/>'s ID.</param>
        /// <param name="attributes">HTML attributes for the <see cref="FlexiFigureBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiFigureBlock"/>.</param>
        public FlexiFigureBlock(string blockName,
            string name,
            bool renderName,
            string linkLabelContent,
            string id,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            BlockName = blockName;
            Name = name;
            RenderName = renderName;
            LinkLabelContent = linkLabelContent;
            ID = id;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the <see cref="FlexiFigureBlock"/>'s BEM block name.
        /// </summary>
        public virtual string BlockName { get; }

        /// <summary>
        /// Gets the <see cref="FlexiFigureBlock"/>'s name.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        /// Gets the value specifying whether to render the <see cref="FlexiFigureBlock"/>'s name.
        /// </summary>
        public virtual bool RenderName { get; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiFigureBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }

        /// <summary>
        /// Gets the content of the link label for linking to the <see cref="FlexiFigureBlock"/>.
        /// </summary>
        public virtual string LinkLabelContent { get; }

        /// <summary>
        /// Gets the <see cref="FlexiFigureBlock"/>'s ID.
        /// </summary>
        public virtual string ID { get; }
    }
}
