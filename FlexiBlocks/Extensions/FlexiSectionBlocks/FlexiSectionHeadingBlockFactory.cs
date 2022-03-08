using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Collections.Generic;
using System.IO;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// The default implementation of <see cref="IFlexiSectionHeadingBlockFactory"/>.
    /// </summary>
    public class FlexiSectionHeadingBlockFactory : IFlexiSectionHeadingBlockFactory
    {
        internal const string REFERENCE_LINKABLE_FLEXI_SECTION_HEADING_BLOCKS_KEY = "flexiSectionHeadingBlocks";
        internal const string GENERATED_IDS_KEY = "generatedIDs";

        /// <inheritdoc />
        public FlexiSectionHeadingBlock Create(BlockProcessor blockProcessor, IFlexiSectionBlockOptions flexiSectionBlockOptions, BlockParser blockParser)
        {
            // Create
            int lineIndex = blockProcessor.LineIndex;
            int column = blockProcessor.Column;
            ref StringSlice line = ref blockProcessor.Line;
            var flexiSectionHeadingBlock = new FlexiSectionHeadingBlock(blockParser)
            {
                Column = column,
                Span = new SourceSpan(line.Start, line.End),
                Line = lineIndex
            };

            // Add line to block
            flexiSectionHeadingBlock.AppendLine(ref line, column, lineIndex, blockProcessor.CurrentLineStartPosition);

            // ID generation and reference-linking
            SetupIDGenerationAndReferenceLinking(flexiSectionHeadingBlock, flexiSectionBlockOptions, blockProcessor);

            return flexiSectionHeadingBlock;
        }

        internal virtual void SetupIDGenerationAndReferenceLinking(FlexiSectionHeadingBlock flexiSectionHeadingBlock,
            IFlexiSectionBlockOptions flexiSectionBlockOptions,
            BlockProcessor blockProcessor)
        {
            if (flexiSectionBlockOptions.GenerateID && // Can only reference-link to FlexiSectionBlock GenerateID is true
                flexiSectionBlockOptions.ReferenceLinkable)
            {
                GetOrCreateReferenceLinkableFlexiSectionHeadingBlocks(blockProcessor.Document).Add(flexiSectionHeadingBlock);
            }
        }

        internal virtual List<FlexiSectionHeadingBlock> GetOrCreateReferenceLinkableFlexiSectionHeadingBlocks(MarkdownDocument markdownDocument)
        {
            if (!(markdownDocument.GetData(REFERENCE_LINKABLE_FLEXI_SECTION_HEADING_BLOCKS_KEY) is List<FlexiSectionHeadingBlock> referenceLinkableFlexiSectionHeadingBlocks))
            {
                referenceLinkableFlexiSectionHeadingBlocks = new List<FlexiSectionHeadingBlock>();
                markdownDocument.SetData(REFERENCE_LINKABLE_FLEXI_SECTION_HEADING_BLOCKS_KEY, referenceLinkableFlexiSectionHeadingBlocks);
                // If user messes with data, handler could be attached multiple times. To avoid issues, handler is written to avoid corrupting 
                // LinkReferenceDefinitions even if called more than once.
                markdownDocument.ProcessInlinesBegin += DocumentOnProcessInlinesBegin;
            }

            return referenceLinkableFlexiSectionHeadingBlocks;
        }

        /// <summary>
        /// Inserts <see cref="LinkReferenceDefinition"/>s into the <see cref="MarkdownDocument"/>'s <see cref="LinkReferenceDefinition"/>s,
        /// allowing for reference-linking to sections via heading content.
        /// </summary>
        internal virtual void DocumentOnProcessInlinesBegin(InlineProcessor inlineProcessor, Inline _)
        {
            Dictionary<string, int> generatedIDs = GetOrCreateGeneratedIDs(inlineProcessor.Document);

            foreach (FlexiSectionHeadingBlock referenceLinkableFlexiSectionHeadingBlock in (List<FlexiSectionHeadingBlock>)inlineProcessor.Document.GetData(REFERENCE_LINKABLE_FLEXI_SECTION_HEADING_BLOCKS_KEY)) // TODO may throw if user messes with data
            {
                var headingWriter = new StringWriter();
                var stripRenderer = new HtmlRenderer(headingWriter)
                {
                    EnableHtmlForInline = false,
                    EnableHtmlEscape = false
                };

                // Generate key
                inlineProcessor.ProcessInlineLeaf(referenceLinkableFlexiSectionHeadingBlock);
                referenceLinkableFlexiSectionHeadingBlock.ProcessInlines = false; // Don't process it again
                stripRenderer.WriteLeafInline(referenceLinkableFlexiSectionHeadingBlock);
                string label = headingWriter.ToString();

                // Set ID
                string id = (string.IsNullOrWhiteSpace(label) ? "section" : LinkHelper.UrilizeAsGfm(label));
                while (generatedIDs.TryGetValue(id, out int numDuplicates))
                {
                    generatedIDs[id] = ++numDuplicates;
                    id = $"{id}-{numDuplicates}";
                    label = $"{label} {numDuplicates}";
                }
                generatedIDs.Add(id, 0);
                referenceLinkableFlexiSectionHeadingBlock.GeneratedID = id;

                // Avoid overriding existing LinkReferenceDefinitions
                if (!inlineProcessor.Document.TryGetLinkReferenceDefinition(label, out LinkReferenceDefinition linkReferenceDefinition))
                {
                    // A markdown link that uses a link reference definition (https://spec.commonmark.org/0.28/#link-reference-definitions)
                    // has form "[<label>]". Markdig searches for a LinkReferenceDefinition with key <label> to handle such a link.
                    inlineProcessor.Document.SetLinkReferenceDefinition(label, new LinkReferenceDefinition(label, $"#{id}", null)); // No need title since anchor element has text content (label)
                }
            }
        }

        internal virtual Dictionary<string, int> GetOrCreateGeneratedIDs(MarkdownDocument markdownDocument)
        {
            if (!(markdownDocument.GetData(GENERATED_IDS_KEY) is Dictionary<string, int> generatedIDs))
            {
                generatedIDs = new Dictionary<string, int>();
                markdownDocument.SetData(GENERATED_IDS_KEY, generatedIDs);
            }

            return generatedIDs;
        }
    }
}
