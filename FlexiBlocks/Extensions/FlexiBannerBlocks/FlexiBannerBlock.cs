using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiBannerBlocks
{
    /// <summary>
    /// Represents a banner.
    /// </summary>
    public class FlexiBannerBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiBannerBlock"/>.
        /// </summary>
        /// <param name="blockName">The <see cref="FlexiBannerBlock"/>'s BEM block name.</param>
        /// <param name="logoIcon">The <see cref="FlexiBannerBlock"/>'s logo icon as an HTML fragment.</param>
        /// <param name="backgroundIcon">The <see cref="FlexiBannerBlock"/>'s background icon as an HTML fragment.</param>
        /// <param name="attributes">HTML attributes for the <see cref="FlexiBannerBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiBannerBlock"/>.</param>
        public FlexiBannerBlock(string blockName,
            string logoIcon,
            string backgroundIcon,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            BlockName = blockName;
            LogoIcon = logoIcon;
            BackgroundIcon = backgroundIcon;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the <see cref="FlexiBannerBlock"/>'s BEM block name.
        /// </summary>
        public virtual string BlockName { get; }

        /// <summary>
        /// Gets the <see cref="FlexiBannerBlock"/>'s logo icon as an HTML fragment.
        /// </summary>
        public virtual string LogoIcon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiBannerBlock"/>'s background icon as an HTML fragment.
        /// </summary>
        public virtual string BackgroundIcon { get; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiBannerBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }
    }
}
