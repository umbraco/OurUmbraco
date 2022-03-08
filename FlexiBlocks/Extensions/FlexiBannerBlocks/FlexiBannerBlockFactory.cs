using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiBannerBlocks
{
    /// <summary>
    /// The default implementation of <see cref="MultipartBlockFactory{T}"/> for creating <see cref="FlexiBannerBlock"/>s.
    /// </summary>
    public class FlexiBannerBlockFactory : MultipartBlockFactory<FlexiBannerBlock>
    {
        private readonly IOptionsService<IFlexiBannerBlockOptions, IFlexiBannerBlocksExtensionOptions> _optionsService;

        /// <summary>
        /// Creates a <see cref="FlexiBannerBlockFactory"/>.
        /// </summary>
        /// <param name="optionsService">The service for creating <see cref="IFlexiBannerBlockOptions"/> and <see cref="IFlexiBannerBlocksExtensionOptions"/>.</param>
        /// <param name="plainBlockParser">The <see cref="BlockParser"/> for parsing <see cref="FlexiBannerBlock"/> parts.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsService"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="plainBlockParser"/> is <c>null</c>.</exception>
        public FlexiBannerBlockFactory(IOptionsService<IFlexiBannerBlockOptions, IFlexiBannerBlocksExtensionOptions> optionsService, PlainBlockParser plainBlockParser) :
            base(plainBlockParser)
        {
            _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
        }

        /// <summary>
        /// Creates a <see cref="FlexiBannerBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiBannerBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiBannerBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        public override FlexiBannerBlock Create(BlockProcessor blockProcessor, BlockParser blockParser)
        {
            (IFlexiBannerBlockOptions flexiBannerBlockOptions, IFlexiBannerBlocksExtensionOptions _) = _optionsService.CreateOptions(blockProcessor);

            // Block name
            string blockName = ResolveBlockName(flexiBannerBlockOptions.BlockName);

            // Create block
            return new FlexiBannerBlock(blockName,
                flexiBannerBlockOptions.LogoIcon,
                flexiBannerBlockOptions.BackgroundIcon,
                flexiBannerBlockOptions.Attributes,
                blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, 0) // MultipartBlockParser will update end
                // Line is assigned by BlockProcessor
            };
        }

        internal virtual string ResolveBlockName(string blockName)
        {
            return string.IsNullOrWhiteSpace(blockName) ? "flexi-banner" : blockName;
        }
    }
}
