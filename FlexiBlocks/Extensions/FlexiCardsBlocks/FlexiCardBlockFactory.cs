using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// The default implementation of <see cref="MultipartBlockFactory{T}"/> for creating <see cref="FlexiCardBlock"/>s.
    /// </summary>
    public class FlexiCardBlockFactory : MultipartBlockFactory<FlexiCardBlock>
    {
        private readonly IBlockOptionsFactory<IFlexiCardBlockOptions> _flexiCardBlockOptionsFactory;

        /// <summary>
        /// Creates a <see cref="FlexiCardBlockFactory"/>.
        /// </summary>
        /// <param name="flexiCardBlockOptionsFactory">The factory for creating <see cref="IFlexiCardBlockOptions"/>.</param>
        /// <param name="plainBlockParser">The <see cref="BlockParser"/> for parsing <see cref="FlexiCardBlock"/> parts.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiCardBlockOptionsFactory"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="plainBlockParser"/> is <c>null</c>.</exception>
        public FlexiCardBlockFactory(IBlockOptionsFactory<IFlexiCardBlockOptions> flexiCardBlockOptionsFactory, PlainBlockParser plainBlockParser) :
            base(plainBlockParser)
        {
            _flexiCardBlockOptionsFactory = flexiCardBlockOptionsFactory ?? throw new ArgumentNullException(nameof(flexiCardBlockOptionsFactory));
        }

        /// <summary>
        /// Creates a <see cref="FlexiCardBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiCardBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiCardBlock"/>.</param>
        /// <exception cref="BlockException">Thrown if <paramref name="blockProcessor"/>'s <see cref="BlockProcessor.CurrentContainer"/> is not 
        /// a <see cref="ProxyFlexiCardsBlock"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        public override FlexiCardBlock Create(BlockProcessor blockProcessor, BlockParser blockParser)
        {
            if (!(blockProcessor.CurrentContainer is ProxyFlexiCardsBlock proxyFlexiCardsBlock))
            {
                throw new BlockException(nameof(FlexiCardBlock),
                    blockProcessor.LineIndex,
                    blockProcessor.Column,
                    string.Format(Strings.BlockException_Shared_BlockCanOnlyExistWithinASpecificTypeOfBlock,
                        nameof(FlexiCardBlock),
                        nameof(FlexiCardsBlock)));
            }

            IFlexiCardBlockOptions flexiCardBlockOptions = _flexiCardBlockOptionsFactory.
                Create(proxyFlexiCardsBlock.FlexiCardsBlockOptions.DefaultCardOptions, blockProcessor);

            // Create block
            return new FlexiCardBlock(flexiCardBlockOptions.Url,
                flexiCardBlockOptions.BackgroundIcon,
                flexiCardBlockOptions.Attributes,
                blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, 0) // MultipartBlockParser will update end
                // Line is assigned by BlockProcessor
            };
        }
    }
}
