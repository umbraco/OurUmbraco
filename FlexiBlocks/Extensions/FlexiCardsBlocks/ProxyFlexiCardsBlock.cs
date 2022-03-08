using Markdig.Parsers;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// Represents a proxy <see cref="FlexiCardsBlock"/>.
    /// </summary>
    public class ProxyFlexiCardsBlock : ProxyFencedContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="ProxyFlexiCardsBlock"/>.
        /// </summary>
        /// <param name="flexiCardsBlockOptions">The <see cref="IFlexiCardsBlockOptions"/> for the <see cref="FlexiCardsBlock"/>.</param>
        /// <param name="openingFenceIndent">The indent of the opening fence.</param>
        /// <param name="openingFenceCharCount">The number of characters in opening fence.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the fenced <see cref="ProxyFlexiCardsBlock"/>.</param>
        public ProxyFlexiCardsBlock(IFlexiCardsBlockOptions flexiCardsBlockOptions,
            int openingFenceIndent,
            int openingFenceCharCount,
            BlockParser blockParser) :
            base(openingFenceIndent, openingFenceCharCount, nameof(FlexiCardsBlock), blockParser)
        {
            FlexiCardsBlockOptions = flexiCardsBlockOptions;
        }

        /// <summary>
        /// Gets the <see cref="IFlexiCardsBlockOptions"/> for the <see cref="FlexiCardsBlock"/>.
        /// </summary>
        public IFlexiCardsBlockOptions FlexiCardsBlockOptions { get; }
    }
}
