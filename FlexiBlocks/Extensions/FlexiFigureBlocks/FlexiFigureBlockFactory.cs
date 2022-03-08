using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiFigureBlocks
{
    /// <summary>
    /// The default implementation of <see cref="MultipartBlockFactory{T}"/> for creating <see cref="FlexiFigureBlock"/>s.
    /// </summary>
    public class FlexiFigureBlockFactory : MultipartBlockFactory<FlexiFigureBlock>
    {
        private readonly IOptionsService<IFlexiFigureBlockOptions, IFlexiFigureBlocksExtensionOptions> _optionsService;
        internal const string REFERENCE_LINKABLE_FLEXI_FIGURE_BLOCKS_KEY = "referenceLinkableflexiFigureBlocks";
        internal const string NEXT_FLEXI_FIGURE_BLOCK_NUMBER_KEY = "nextFlexiFigureBlockNumber";

        /// <summary>
        /// Creates a <see cref="FlexiFigureBlockFactory"/>.
        /// </summary>
        /// <param name="optionsService">The service for creating <see cref="IFlexiFigureBlockOptions"/> and <see cref="IFlexiFigureBlocksExtensionOptions"/>.</param>
        /// <param name="partBlockParser">The <see cref="BlockParser"/> for parsing <see cref="FlexiFigureBlock"/> parts.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsService"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="partBlockParser"/> is <c>null</c>.</exception>
        public FlexiFigureBlockFactory(IOptionsService<IFlexiFigureBlockOptions, IFlexiFigureBlocksExtensionOptions> optionsService, PlainBlockParser partBlockParser) :
            base(partBlockParser)
        {
            _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
        }

        /// <summary>
        /// Creates a <see cref="FlexiFigureBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiFigureBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiFigureBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        public override FlexiFigureBlock Create(BlockProcessor blockProcessor, BlockParser blockParser)
        {
            (IFlexiFigureBlockOptions flexiFigureBlockOptions, IFlexiFigureBlocksExtensionOptions _) = _optionsService.CreateOptions(blockProcessor);

            // Block name
            string blockName = ResolveBlockName(flexiFigureBlockOptions.BlockName);

            // Figure number
            int figureNumber = GetFlexiFigureBlockNumber(blockProcessor);

            // ID
            ReadOnlyDictionary<string, string> attributes = flexiFigureBlockOptions.Attributes;
            string id = ResolveID(flexiFigureBlockOptions.GenerateID, figureNumber, attributes);

            // ReferenceLinkable
            bool referenceLinkable = ResolveReferenceLinkable(flexiFigureBlockOptions.ReferenceLinkable, id);

            // LinkLabelContent specified
            string linkLabelContent = flexiFigureBlockOptions.LinkLabelContent;
            bool linkLabelContentSpecified = IsLinkLabelContentSpecified(linkLabelContent);

            // Name
            bool renderName = flexiFigureBlockOptions.RenderName;
            string name = ResolveName(referenceLinkable, linkLabelContentSpecified, renderName, figureNumber);

            // LinkLabelContent
            string resolvedLinkLabelContent = ResolveLinkLabelContent(referenceLinkable, linkLabelContentSpecified, name, linkLabelContent);

            // Create block
            var result = new FlexiFigureBlock(blockName,
                name,
                renderName,
                resolvedLinkLabelContent,
                id,
                attributes,
                blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, 0) // MultipartBlockParser will update end
                // Line is assigned by BlockProcessor
            };

            // Reference-linking
            SetupReferenceLinking(result, referenceLinkable, blockProcessor);

            return result;
        }

        internal virtual string ResolveBlockName(string blockName)
        {
            return string.IsNullOrWhiteSpace(blockName) ? "flexi-figure" : blockName;
        }

        // TODO increments even if we don't render figure number, also, always starts at 1 for each document, what if we want figure numbers across multiple
        // documents to be a contiguous integer sequence?
        internal virtual int GetFlexiFigureBlockNumber(BlockProcessor blockProcessor)
        {
            MarkdownDocument markdownDocument = blockProcessor.Document;
            if (!(markdownDocument.GetData(NEXT_FLEXI_FIGURE_BLOCK_NUMBER_KEY) is int nextFlexiFigureBlockNumber))
            {
                nextFlexiFigureBlockNumber = 1;
            }

            markdownDocument.SetData(NEXT_FLEXI_FIGURE_BLOCK_NUMBER_KEY, nextFlexiFigureBlockNumber + 1);

            return nextFlexiFigureBlockNumber;
        }

        internal virtual string ResolveID(bool generateID, int figureNumber, ReadOnlyDictionary<string, string> attributes)
        {
            string customID = null;
            if (attributes?.TryGetValue("id", out customID) == true && !string.IsNullOrWhiteSpace(customID)) // Check whether a custom id has been specified
            {
                return customID;
            }

            return generateID ? $"figure-{figureNumber}" : null; // TODO we could avoid some allocations by caching
        }

        internal virtual bool ResolveReferenceLinkable(bool referenceLinkable, string id)
        {
            return referenceLinkable && id != null;
        }

        internal virtual bool IsLinkLabelContentSpecified(string linkLabelContent)
        {
            return !string.IsNullOrWhiteSpace(linkLabelContent);
        }

        internal virtual string ResolveName(bool referenceLinkable, bool linkLabelContentSpecified, bool renderName, int figureNumber)
        {
            return (referenceLinkable && !linkLabelContentSpecified) || renderName ? $"Figure {figureNumber}" : null; // TODO we could avoid some allocations by caching
        }

        internal virtual string ResolveLinkLabelContent(bool referenceLinkable, bool linkLabelContentSpecified, string name, string linkLabelContent)
        {
            if (!referenceLinkable)
            {
                return null;
            }

            return linkLabelContentSpecified ? linkLabelContent : name;
        }

        internal virtual void SetupReferenceLinking(FlexiFigureBlock flexiFigureBlock,
            bool referenceLinkable,
            BlockProcessor blockProcessor)
        {
            if (!referenceLinkable)
            {
                return;
            }

            GetOrCreateReferenceLinkableFlexiFigureBlocks(blockProcessor.Document).Add(flexiFigureBlock);
        }

        internal virtual List<FlexiFigureBlock> GetOrCreateReferenceLinkableFlexiFigureBlocks(MarkdownDocument markdownDocument)
        {
            if (!(markdownDocument.GetData(REFERENCE_LINKABLE_FLEXI_FIGURE_BLOCKS_KEY) is List<FlexiFigureBlock> referenceLinkableFlexiFigureBlocks))
            {
                referenceLinkableFlexiFigureBlocks = new List<FlexiFigureBlock>();
                markdownDocument.SetData(REFERENCE_LINKABLE_FLEXI_FIGURE_BLOCKS_KEY, referenceLinkableFlexiFigureBlocks);
                // If user messes with data, handler could be attached multiple times. To avoid issues, handler is written to avoid corrupting 
                // LinkReferenceDefinitions even if called more than once.
                markdownDocument.ProcessInlinesBegin += DocumentOnProcessInlinesBegin;
            }

            return referenceLinkableFlexiFigureBlocks;
        }

        internal virtual void DocumentOnProcessInlinesBegin(InlineProcessor inlineProcessor, Inline _)
        {
            foreach (FlexiFigureBlock referenceLinkableFlexiFigureBlock in (List<FlexiFigureBlock>)inlineProcessor.Document.GetData(REFERENCE_LINKABLE_FLEXI_FIGURE_BLOCKS_KEY)) // TODO may throw if user messes with data
            {
                string linkLabelContent = referenceLinkableFlexiFigureBlock.LinkLabelContent;

                // Avoid overriding existing LinkReferenceDefinitions
                if (!inlineProcessor.Document.TryGetLinkReferenceDefinition(linkLabelContent, out LinkReferenceDefinition _))
                {
                    var linkReferenceDefinition = new LinkReferenceDefinition(linkLabelContent, $"#{referenceLinkableFlexiFigureBlock.ID}", referenceLinkableFlexiFigureBlock.Name); // Use the Title property to hold the block's name
                    linkReferenceDefinition.CreateLinkInline = CreateLinkInline;

                    // A markdown link that uses a link reference definition (https://spec.commonmark.org/0.28/#link-reference-definitions)
                    // has form "[<label>]". Markdig searches for a LinkReferenceDefinition with key <label> to handle such a link.
                    inlineProcessor.Document.SetLinkReferenceDefinition(linkLabelContent, linkReferenceDefinition);
                }
            }
        }

        internal virtual Inline CreateLinkInline(InlineProcessor _, LinkReferenceDefinition linkRef, Inline child = null)
        {
            // TODO LinkReferenceDefinition dictionary lookups aren't case sensitive, so LiteralInline content may have different case from Label/Title, 
            // shouldn't be an issue but should document.
            //
            // TODO If label contains inline markdown, child might not be a literalInline. e.g if link is [*text*], i.e label is "*text*", child will be an EmphasisDelimiterInline.
            // We just ignore such cases for now, extremely unlikely since user will have to linkLabelContent to a string with inlines like "*text*".
            // 
            // Replace reference-link linkLabelContent with figure number string if they defer. 
            if (linkRef.Label != linkRef.Title && child is LiteralInline childLiteralInline)
            {
                childLiteralInline.Content = new StringSlice(linkRef.Title);
            }

            return new LinkInline(linkRef.Url, null);
        }
    }
}
