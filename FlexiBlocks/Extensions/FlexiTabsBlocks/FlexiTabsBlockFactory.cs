using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// The default implementation of <see cref="CollectionBlockFactory{TMain, TProxy, TChild}"/> for creating <see cref="FlexiTabsBlock"/>s.
    /// </summary>
    public class FlexiTabsBlockFactory : CollectionBlockFactory<FlexiTabsBlock, ProxyFlexiTabsBlock, FlexiTabBlock>
    {
        private readonly IOptionsService<IFlexiTabsBlockOptions, IFlexiTabsBlocksExtensionOptions> _optionsService;

        /// <summary>
        /// Creates a <see cref="FlexiTabsBlockFactory"/>.
        /// </summary>
        /// <param name="optionsService">The service for creating <see cref="IFlexiTabsBlockOptions"/> and <see cref="IFlexiTabsBlocksExtensionOptions"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsService"/> is <c>null</c>.</exception>
        public FlexiTabsBlockFactory(IOptionsService<IFlexiTabsBlockOptions, IFlexiTabsBlocksExtensionOptions> optionsService)
        {
            _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
        }

        /// <summary>
        /// Creates a <see cref="ProxyFlexiTabsBlock"/>.
        /// </summary>
        /// <param name="openingFenceIndent">The indent of the opening fence.</param>
        /// <param name="openingFenceCharCount">The number of characters in the opening fence.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="ProxyFlexiTabsBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="ProxyFlexiTabsBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        public override ProxyFlexiTabsBlock CreateProxyFencedBlock(int openingFenceIndent, int openingFenceCharCount, BlockProcessor blockProcessor, BlockParser blockParser)
        {
            (IFlexiTabsBlockOptions flexiTabsBlockOptions, IFlexiTabsBlocksExtensionOptions _) = _optionsService.CreateOptions(blockProcessor);

            // Default tab options
            ValidateDefaultTabOptions(flexiTabsBlockOptions);

            return new ProxyFlexiTabsBlock(flexiTabsBlockOptions, openingFenceIndent, openingFenceCharCount, blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, 0) // FencedBlockParser will update end
                // Line is assigned by BlockProcessor
            };
        }

        /// <summary>
        /// Creates a <see cref="FlexiTabsBlock"/> from a <see cref="ProxyFlexiTabsBlock"/>.
        /// </summary>
        /// <param name="proxyFencedBlock">The <see cref="ProxyFlexiTabsBlock"/> containing data for the <see cref="FlexiTabsBlock"/>.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiTabsBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="proxyFencedBlock"/> is <c>null</c>.</exception>
        /// <exception cref="OptionsException">Thrown if an option is invalid.</exception>
        public override FlexiTabsBlock Create(ProxyFlexiTabsBlock proxyFencedBlock, BlockProcessor blockProcessor)
        {
            if (proxyFencedBlock == null)
            {
                throw new ArgumentNullException(nameof(proxyFencedBlock));
            }

            IFlexiTabsBlockOptions flexiTabsBlockOptions = proxyFencedBlock.FlexiTabsBlockOptions;

            // Block name
            string blockName = ResolveBlockName(flexiTabsBlockOptions.BlockName);

            // Create block
            var flexiTabsBlock = new FlexiTabsBlock(blockName, flexiTabsBlockOptions.Attributes, proxyFencedBlock.Parser)
            {
                Line = proxyFencedBlock.Line,
                Column = proxyFencedBlock.Column,
                Span = proxyFencedBlock.Span
            };

            MoveChildren(proxyFencedBlock, flexiTabsBlock);

            return flexiTabsBlock;
        }

        internal virtual string ResolveBlockName(string blockName)
        {
            return string.IsNullOrWhiteSpace(blockName) ? "flexi-tabs" : blockName;
        }

        internal virtual void ValidateDefaultTabOptions(IFlexiTabsBlockOptions flexiTabsBlockOptions)
        {
            if (flexiTabsBlockOptions.DefaultTabOptions == null)
            {
                throw new OptionsException(nameof(IFlexiTabsBlockOptions.DefaultTabOptions), Strings.OptionsException_Shared_ValueMustNotBeNull);
            }
        }
    }
}
