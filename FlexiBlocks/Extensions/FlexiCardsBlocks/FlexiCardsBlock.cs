using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// Represents a collection of cards.
    /// </summary>
    public class FlexiCardsBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiCardsBlock"/>.
        /// </summary>
        /// <param name="blockName">The <see cref="FlexiCardsBlock"/>'s BEM block name.</param>
        /// <param name="cardSize">The display size of contained <see cref="FlexiCardBlock"/>s.</param>
        /// <param name="attributes">HTML attributes for the <see cref="FlexiCardsBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiCardsBlock"/>.</param>
        public FlexiCardsBlock(string blockName,
            FlexiCardBlockSize cardSize,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            BlockName = blockName;
            CardSize = cardSize;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the <see cref="FlexiCardsBlock"/>'s BEM block name.
        /// </summary>
        public virtual string BlockName { get; }

        /// <summary>
        /// Gets the display size of contained <see cref="FlexiCardBlock"/>s.
        /// </summary>
        public virtual FlexiCardBlockSize CardSize { get; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiCardsBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }
    }
}
