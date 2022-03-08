using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// The default implementation of <see cref="MultipartBlockFactory{T}"/> for creating <see cref="FlexiTabBlock"/>s.
    /// </summary>
    public class FlexiTabBlockFactory : MultipartBlockFactory<FlexiTabBlock>
    {
        private readonly IBlockOptionsFactory<IFlexiTabBlockOptions> _flexiTabBlockOptionsFactory;

        /// <summary>
        /// Creates a <see cref="FlexiTabBlockFactory"/>.
        /// </summary>
        /// <param name="flexiTabBlockOptionsFactory">The factory for creating <see cref="IFlexiTabBlockOptions"/>.</param>
        /// <param name="plainBlockParser">The <see cref="BlockParser"/> for parsing <see cref="FlexiTabBlock"/> parts.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiTabBlockOptionsFactory"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="plainBlockParser"/> is <c>null</c>.</exception>
        public FlexiTabBlockFactory(IBlockOptionsFactory<IFlexiTabBlockOptions> flexiTabBlockOptionsFactory, PlainBlockParser plainBlockParser) :
            base(plainBlockParser)
        {
            _flexiTabBlockOptionsFactory = flexiTabBlockOptionsFactory ?? throw new ArgumentNullException(nameof(flexiTabBlockOptionsFactory));
        }

        /// <summary>
        /// Creates a <see cref="FlexiTabBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiTabBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiTabBlock"/>.</param>
        /// <exception cref="BlockException">Thrown if <paramref name="blockProcessor"/>'s <see cref="BlockProcessor.CurrentContainer"/> is not 
        /// a <see cref="ProxyFlexiTabsBlock"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        public override FlexiTabBlock Create(BlockProcessor blockProcessor, BlockParser blockParser)
        {
            if (!(blockProcessor.CurrentContainer is ProxyFlexiTabsBlock proxyFlexiTabsBlock))
            {
                throw new BlockException(nameof(FlexiTabBlock),
                    blockProcessor.LineIndex,
                    blockProcessor.Column,
                    string.Format(Strings.BlockException_Shared_BlockCanOnlyExistWithinASpecificTypeOfBlock,
                        nameof(FlexiTabBlock),
                        nameof(FlexiTabsBlock)));
            }

            IFlexiTabBlockOptions flexiTabBlockOptions = _flexiTabBlockOptionsFactory.
                Create(proxyFlexiTabsBlock.FlexiTabsBlockOptions.DefaultTabOptions, blockProcessor);

            // Create block
            return new FlexiTabBlock(flexiTabBlockOptions.Attributes, blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, 0) // MultipartBlockParser will update end
                // Line is assigned by BlockProcessor
            };
        }
    }
}
