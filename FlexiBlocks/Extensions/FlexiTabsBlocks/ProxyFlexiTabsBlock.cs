using Markdig.Parsers;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// Represents a proxy <see cref="FlexiTabsBlock"/>.
    /// </summary>
    public class ProxyFlexiTabsBlock : ProxyFencedContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="ProxyFlexiTabsBlock"/>.
        /// </summary>
        /// <param name="flexiTabsBlockOptions">The <see cref="IFlexiTabsBlockOptions"/> for the <see cref="FlexiTabsBlock"/>.</param>
        /// <param name="openingFenceIndent">The indent of the opening fence.</param>
        /// <param name="openingFenceCharCount">The number of characters in opening fence.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the fenced <see cref="ProxyFlexiTabsBlock"/>.</param>
        public ProxyFlexiTabsBlock(IFlexiTabsBlockOptions flexiTabsBlockOptions,
            int openingFenceIndent,
            int openingFenceCharCount,
            BlockParser blockParser) :
            base(openingFenceIndent, openingFenceCharCount, nameof(FlexiTabsBlock), blockParser)
        {
            FlexiTabsBlockOptions = flexiTabsBlockOptions;
        }

        /// <summary>
        /// Gets the <see cref="IFlexiTabsBlockOptions"/> for the <see cref="FlexiTabsBlock"/>.
        /// </summary>
        public IFlexiTabsBlockOptions FlexiTabsBlockOptions { get; }
    }
}
