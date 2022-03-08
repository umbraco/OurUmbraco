using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// A markdig extension for <see cref="FlexiCodeBlock"/>s.
    /// </summary>
    public class FlexiCodeBlocksExtension : BlockExtension<FlexiCodeBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiCodeBlocksExtension"/>.
        /// </summary>
        /// <param name="indentedFlexiCodeBlockParser">The <see cref="ProxyBlockParser{TMain, TProxy}"/> for creating <see cref="FlexiCodeBlock"/>s from indented code in markdown.</param>
        /// <param name="fencedFlexiCodeBlockParsers">The <see cref="ProxyBlockParser{TMain, TProxy}"/>s for creating <see cref="FlexiCodeBlock"/>s from fenced code in markdown.</param>
        /// <param name="flexiCodeBlockRenderer">The <see cref="BlockRenderer{T}"/> for rendering <see cref="FlexiCodeBlock"/>s as HTML.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="indentedFlexiCodeBlockParser"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fencedFlexiCodeBlockParsers"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="fencedFlexiCodeBlockParsers"/> does not contain at least two elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiCodeBlockRenderer"/> is <c>null</c>.</exception>
        public FlexiCodeBlocksExtension(ProxyBlockParser<FlexiCodeBlock, ProxyLeafBlock> indentedFlexiCodeBlockParser,
            IEnumerable<ProxyBlockParser<FlexiCodeBlock, ProxyFencedLeafBlock>> fencedFlexiCodeBlockParsers,
            BlockRenderer<FlexiCodeBlock> flexiCodeBlockRenderer) :
            base(flexiCodeBlockRenderer,
                new BlockParser[] { indentedFlexiCodeBlockParser, fencedFlexiCodeBlockParsers.ElementAt(0), fencedFlexiCodeBlockParsers.ElementAt(1) }) // Throws if fencedFlexiCodeBlockParsers is null or has < 2 elements
        {
            if (indentedFlexiCodeBlockParser == null)
            {
                throw new ArgumentNullException(nameof(indentedFlexiCodeBlockParser));
            }

            if (flexiCodeBlockRenderer == null)
            {
                throw new ArgumentNullException(nameof(flexiCodeBlockRenderer));
            }
        }

        /// <summary>
        /// Removes <see cref="CodeBlock"/> <see cref="BlockParser"/>s.
        /// </summary>
        /// <param name="markdownPipelineBuilder">The <see cref="MarkdownPipelineBuilder"/> to remove <see cref="BlockParser"/>s from.</param>
        protected override void SetupParsers(MarkdownPipelineBuilder markdownPipelineBuilder)
        {
            // FencedCodeBlockParser and IndentedCodeBlockParser are default parsers registered in MarkdownPipelineBuilder's constructor.
            // We need to remove them so FlexiCodeBlock parsers parse code blocks.
            markdownPipelineBuilder.BlockParsers.RemoveAll(blockParser => blockParser is FencedCodeBlockParser || blockParser is IndentedCodeBlockParser);
        }

        /// <summary>
        /// Removes <see cref="CodeBlockRenderer"/>.
        /// </summary>
        /// <param name="markdownPipeline">Unused.</param>
        /// <param name="markdownRenderer">The <see cref="IMarkdownRenderer"/> to remove the <see cref="CodeBlockRenderer"/> from.</param>
        protected override void SetupRenderers(MarkdownPipeline markdownPipeline, IMarkdownRenderer markdownRenderer)
        {
            if (markdownRenderer is HtmlRenderer htmlRenderer)
            {
                // FlexiCodeBlockRenderer renders all code blocks, CodeBlockRenderer isn't needed
                htmlRenderer.ObjectRenderers.RemoveAll(blockRenderer => blockRenderer is CodeBlockRenderer);
            }
        }
    }
}
