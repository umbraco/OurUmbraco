using Markdig;
using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// A markdig extension for <see cref="FlexiSectionBlock"/>s.
    /// </summary>
    public class FlexiSectionBlocksExtension : BlockExtension<FlexiSectionBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiSectionBlocksExtension"/>.
        /// </summary>
        /// <param name="flexiSectionBlockParser">The <see cref="BlockParser{T}"/> for parsing <see cref="FlexiSectionBlock"/>s from markdown.</param>
        /// <param name="flexiSectionBlockRenderer">The <see cref="BlockRenderer{T}"/> for rendering <see cref="FlexiSectionBlock"/>s as HTML.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiSectionBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiSectionBlockRenderer"/> is <c>null</c>.</exception>
        public FlexiSectionBlocksExtension(BlockParser<FlexiSectionBlock> flexiSectionBlockParser,
            BlockRenderer<FlexiSectionBlock> flexiSectionBlockRenderer) :
            base(flexiSectionBlockRenderer, flexiSectionBlockParser)
        {
            if (flexiSectionBlockParser == null)
            {
                throw new ArgumentNullException(nameof(flexiSectionBlockParser));
            }

            if (flexiSectionBlockRenderer == null)
            {
                throw new ArgumentNullException(nameof(flexiSectionBlockRenderer));
            }
        }

        /// <summary>
        /// Removes <see cref="HeadingBlockParser"/>.
        /// </summary>
        /// <param name="markdownPipelineBuilder">The <see cref="MarkdownPipelineBuilder"/> to remove the <see cref="HeadingBlockParser"/> from.</param>
        protected override void SetupParsers(MarkdownPipelineBuilder markdownPipelineBuilder)
        {
            // HeadingBlockParser is a default parser registered in MarkdownPipelineBuilder's constructor.
            // FlexiSectionBlockParser makes it redundant.
            HeadingBlockParser headingBlockParser = markdownPipelineBuilder.BlockParsers.Find<HeadingBlockParser>();
            if (headingBlockParser != null)
            {
                markdownPipelineBuilder.BlockParsers.Remove(headingBlockParser);
            }
        }

        // Note - We still need HeadingRenderer for setext headers
    }
}
