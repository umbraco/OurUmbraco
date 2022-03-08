using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiQuoteBlocks
{
    /// <summary>
    /// Represents a quote with a citation.
    /// </summary>
    public class FlexiQuoteBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiQuoteBlock"/>.
        /// </summary>
        /// <param name="blockName">The <see cref="FlexiQuoteBlock"/>'s BEM block name.</param>
        /// <param name="icon">The <see cref="FlexiQuoteBlock"/>'s icon as an HTML fragment.</param>
        /// <param name="citeLink">The index of the link in the <see cref="FlexiQuoteBlock"/>'s citation that points to the work where its quote comes from.</param>
        /// <param name="attributes">HTML attributes for the <see cref="FlexiQuoteBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiQuoteBlock"/>.</param>
        public FlexiQuoteBlock(string blockName,
            string icon,
            int citeLink,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            BlockName = blockName;
            Icon = icon;
            CiteLink = citeLink;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the <see cref="FlexiQuoteBlock"/>'s BEM block name.
        /// </summary>
        public virtual string BlockName { get; }

        /// <summary>
        /// Gets the <see cref="FlexiQuoteBlock"/>'s icon as an HTML fragment.
        /// </summary>
        public virtual string Icon { get; }

        /// <summary>
        /// Gets or sets the URL of the work where the <see cref="FlexiQuoteBlock"/>'s quote comes from.
        /// </summary>
        public virtual string CiteUrl { get; set; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiQuoteBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }

        /// <summary>
        /// Gets the index of the link in the <see cref="FlexiQuoteBlock"/>'s citation that points to the work where its quote comes from.
        /// </summary>
        public virtual int CiteLink { get; }
    }
}
