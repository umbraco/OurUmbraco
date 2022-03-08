using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiQuoteBlocks
{
    /// <summary>
    /// The default implementation of <see cref="MultipartBlockFactory{T}"/> for creating <see cref="FlexiQuoteBlock"/>s.
    /// </summary>
    public class FlexiQuoteBlockFactory : MultipartBlockFactory<FlexiQuoteBlock>
    {
        private readonly IOptionsService<IFlexiQuoteBlockOptions, IFlexiQuoteBlocksExtensionOptions> _optionsService;

        /// <summary>
        /// Creates a <see cref="FlexiQuoteBlockFactory"/>.
        /// </summary>
        /// <param name="optionsService">The service for creating <see cref="IFlexiQuoteBlockOptions"/> and <see cref="IFlexiQuoteBlocksExtensionOptions"/>.</param>
        /// <param name="plainBlockParser">The <see cref="BlockParser"/> for parsing <see cref="FlexiQuoteBlock"/> parts.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsService"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="plainBlockParser"/> is <c>null</c>.</exception>
        public FlexiQuoteBlockFactory(IOptionsService<IFlexiQuoteBlockOptions, IFlexiQuoteBlocksExtensionOptions> optionsService, PlainBlockParser plainBlockParser) :
            base(plainBlockParser)
        {
            _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
        }

        /// <summary>
        /// Creates a <see cref="FlexiQuoteBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiQuoteBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiQuoteBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        public override FlexiQuoteBlock Create(BlockProcessor blockProcessor, BlockParser blockParser)
        {
            (IFlexiQuoteBlockOptions flexiQuoteBlockOptions, IFlexiQuoteBlocksExtensionOptions _) = _optionsService.
                CreateOptions(blockProcessor);

            // Block name
            string blockName = ResolveBlockName(flexiQuoteBlockOptions.BlockName);

            // Create block
            var flexiQuoteBlock = new FlexiQuoteBlock(blockName,
                flexiQuoteBlockOptions.Icon,
                flexiQuoteBlockOptions.CiteLink,
                flexiQuoteBlockOptions.Attributes,
                blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, 0) // MultipartBlockParser will update end
                // Line is assigned by BlockProcessor
            };

            // Subscribe to ProcessInlinesEnd event - LinkInlines only exist after inline processing is done
            flexiQuoteBlock.ProcessInlinesEnd += ExtractCiteUrl;

            return flexiQuoteBlock;
        }

        internal virtual string ResolveBlockName(string blockName)
        {
            return string.IsNullOrWhiteSpace(blockName) ? "flexi-quote" : blockName;
        }

        internal void ExtractCiteUrl(InlineProcessor inlineProcessor, Inline _)
        {
            LeafBlock citationBlock = inlineProcessor.Block;
            IEnumerable<LinkInline> linkInlines = citationBlock.Inline.FindDescendants<LinkInline>();
            int numLinks = linkInlines.Count();

            // No links, regardless of what sourceIndex is, return
            if (numLinks == 0)
            {
                return;
            }

            var flexiQuoteBlock = (FlexiQuoteBlock)citationBlock.Parent;
            int citeLinkIndex = NormalizeCiteLinkIndex(numLinks, flexiQuoteBlock);
            LinkInline linkInline = linkInlines.ElementAt(citeLinkIndex);

            flexiQuoteBlock.CiteUrl = linkInline.Url;
        }

        internal virtual int NormalizeCiteLinkIndex(int numLinks, FlexiQuoteBlock flexiQuoteBlock)
        {
            int citeLinkIndex = flexiQuoteBlock.CiteLink;
            int normalizedCiteLinkIndex = citeLinkIndex < 0 ? numLinks + citeLinkIndex : citeLinkIndex;
            if (normalizedCiteLinkIndex < 0 || normalizedCiteLinkIndex > numLinks - 1)
            {
                var optionsException = new OptionsException(nameof(IFlexiQuoteBlockOptions.CiteLink),
                    string.Format(Strings.OptionsException_FlexiQuoteBlockFactory_UnableToNormalize, citeLinkIndex, numLinks));

                throw new BlockException(flexiQuoteBlock, innerException: optionsException);
            }

            return normalizedCiteLinkIndex;
        }
    }
}
