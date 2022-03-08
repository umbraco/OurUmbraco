using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// The default implementation of <see cref="CollectionBlockFactory{TMain, TProxy, TChild}"/> for creating <see cref="FlexiCardsBlock"/>s.
    /// </summary>
    public class FlexiCardsBlockFactory : CollectionBlockFactory<FlexiCardsBlock, ProxyFlexiCardsBlock, FlexiCardBlock>
    {
        private readonly IOptionsService<IFlexiCardsBlockOptions, IFlexiCardsBlocksExtensionOptions> _optionsService;

        /// <summary>
        /// Creates a <see cref="FlexiCardsBlockFactory"/>.
        /// </summary>
        /// <param name="optionsService">The service for creating <see cref="IFlexiCardsBlockOptions"/> and <see cref="IFlexiCardsBlocksExtensionOptions"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsService"/> is <c>null</c>.</exception>
        public FlexiCardsBlockFactory(IOptionsService<IFlexiCardsBlockOptions, IFlexiCardsBlocksExtensionOptions> optionsService)
        {
            _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
        }

        /// <summary>
        /// Creates a <see cref="ProxyFlexiCardsBlock"/>.
        /// </summary>
        /// <param name="openingFenceIndent">The indent of the opening fence.</param>
        /// <param name="openingFenceCharCount">The number of characters in the opening fence.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="ProxyFlexiCardsBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="ProxyFlexiCardsBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        public override ProxyFlexiCardsBlock CreateProxyFencedBlock(int openingFenceIndent, int openingFenceCharCount, BlockProcessor blockProcessor, BlockParser blockParser)
        {
            (IFlexiCardsBlockOptions flexiCardsBlockOptions, IFlexiCardsBlocksExtensionOptions _) = _optionsService.CreateOptions(blockProcessor);

            // Default card options
            ValidateDefaultCardOptions(flexiCardsBlockOptions);

            return new ProxyFlexiCardsBlock(flexiCardsBlockOptions, openingFenceIndent, openingFenceCharCount, blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, 0) // FencedBlockParser will update end
                // Line is assigned by BlockProcessor
            };
        }

        /// <summary>
        /// Creates a <see cref="FlexiCardsBlock"/> from a <see cref="ProxyFlexiCardsBlock"/>.
        /// </summary>
        /// <param name="proxyFencedBlock">The <see cref="ProxyFlexiCardsBlock"/> containing data for the <see cref="FlexiCardsBlock"/>.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiCardsBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="proxyFencedBlock"/> is <c>null</c>.</exception>
        /// <exception cref="OptionsException">Thrown if an option is invalid.</exception>
        public override FlexiCardsBlock Create(ProxyFlexiCardsBlock proxyFencedBlock, BlockProcessor blockProcessor)
        {
            if (proxyFencedBlock == null)
            {
                throw new ArgumentNullException(nameof(proxyFencedBlock));
            }

            IFlexiCardsBlockOptions flexiCardsBlockOptions = proxyFencedBlock.FlexiCardsBlockOptions;

            // Block name
            string blockName = ResolveBlockName(flexiCardsBlockOptions.BlockName);

            // Card size
            FlexiCardBlockSize cardSize = flexiCardsBlockOptions.CardSize;
            ValidateCardSize(cardSize);

            // Create block
            var flexiCardsBlock = new FlexiCardsBlock(blockName, cardSize, flexiCardsBlockOptions.Attributes, proxyFencedBlock.Parser)
            {
                Line = proxyFencedBlock.Line,
                Column = proxyFencedBlock.Column,
                Span = proxyFencedBlock.Span
            };

            MoveChildren(proxyFencedBlock, flexiCardsBlock);

            return flexiCardsBlock;
        }

        internal virtual string ResolveBlockName(string blockName)
        {
            return string.IsNullOrWhiteSpace(blockName) ? "flexi-cards" : blockName;
        }

        internal virtual void ValidateDefaultCardOptions(IFlexiCardsBlockOptions flexiCardsBlockOptions)
        {
            if (flexiCardsBlockOptions.DefaultCardOptions == null)
            {
                throw new OptionsException(nameof(IFlexiCardsBlockOptions.DefaultCardOptions), Strings.OptionsException_Shared_ValueMustNotBeNull);
            }
        }

        internal virtual void ValidateCardSize(FlexiCardBlockSize cardSize)
        {
            if (!Enum.IsDefined(typeof(FlexiCardBlockSize), cardSize))
            {
                throw new OptionsException(nameof(IFlexiCardsBlockOptions.CardSize),
                        string.Format(Strings.OptionsException_Shared_ValueMustBeAValidEnumValue,
                            cardSize,
                            nameof(FlexiCardBlockSize)));
            }
        }
    }
}
