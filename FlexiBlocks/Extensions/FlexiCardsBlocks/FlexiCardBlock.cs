using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// Represents a card containing content you'd like to draw readers attention to, such as warnings and important information.
    /// </summary>
    public class FlexiCardBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiCardBlock"/>.
        /// </summary>
        /// <param name="url">The URL the <see cref="FlexiCardBlock"/> points to.</param>
        /// <param name="backgroundIcon">The <see cref="FlexiCardBlock"/>'s background icon as an HTML fragment.</param>
        /// <param name="attributes">HTML attributes for the <see cref="FlexiCardBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiCardBlock"/>.</param>
        public FlexiCardBlock(string url,
            string backgroundIcon,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            Url = url;
            BackgroundIcon = backgroundIcon;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the URL the <see cref="FlexiCardBlock"/> points to.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets the <see cref="FlexiCardBlock"/>'s background icon as an HTML fragment.
        /// </summary>
        public string BackgroundIcon { get; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiCardBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }
    }
}
