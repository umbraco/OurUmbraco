using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// <para>An abstraction for <see cref="IBlock"/> extensions.</para>
    /// <para>Implements <see cref="IBlockExtension{T}"/>.</para>
    /// </summary>
    public abstract class BlockExtension<T> : IBlockExtension<T> where T : MarkdownObject, IBlock
    {
        // This can't be BlockParser<T>[]. Jering.Markdig.Extensions.FlexiBlocks.BlockParser<T> can't be defined with a covariant generic argument, i.e BlockParser<out T>,
        // because it is not an interface. It can't be an interface because it must implement Markdig.Parsers.BlockParser so it can be added to MarkdownPipelineBuilder.BlockParsers.
        private readonly IEnumerable<BlockParser> _blockParsers;
        private readonly BlockRenderer<T> _blockRenderer;

        /// <summary>
        /// Creates a <see cref="BlockExtension{T}"/>.
        /// </summary>
        /// <param name="blockRenderer">
        /// <para>The renderer for rendering <see cref="IBlock"/>s as HTML.</para>
        /// <para>This value can be <c>null</c>. An extension can parse <see cref="IBlock"/>s that aren't rendered.</para>
        /// <para>If not <c>null</c>, this value is added to the <see cref="MarkdownPipeline"/>'s renderers.</para>
        /// </param>
        /// <param name="blockParsers">
        /// <para>The <see cref="BlockParser"/>s for creating <see cref="IBlock"/>s from markdown.</para>
        /// <para>This value can be empty. An extension can render <see cref="IBlock"/>s parsed by other extensions.</para>
        /// <para>This value's elements are added to the <see cref="MarkdownPipeline"/>'s <see cref="BlockParser"/>s.</para>
        /// </param>
        protected BlockExtension(BlockRenderer<T> blockRenderer, params BlockParser[] blockParsers)
        {
            _blockRenderer = blockRenderer;
            _blockParsers = blockParsers;
        }

        /// <summary>
        /// Creates a <see cref="BlockExtension{T}"/>.
        /// </summary>
        /// <param name="blockRenderer">
        /// <para>The renderer for rendering <see cref="IBlock"/>s as HTML.</para>
        /// <para>This value can be <c>null</c>. An extension can parse <see cref="IBlock"/>s that aren't rendered.</para>
        /// <para>If not <c>null</c>, this value is added to the <see cref="MarkdownPipeline"/>'s renderers.</para>
        /// </param>
        /// <param name="blockParsers">
        /// <para>The <see cref="BlockParser"/>s for creating <see cref="IBlock"/>s from markdown.</para>
        /// <para>This value can be empty. An extension can render <see cref="IBlock"/>s parsed by other extensions.</para>
        /// <para>This value's elements are added to the <see cref="MarkdownPipeline"/>'s <see cref="BlockParser"/>s.</para>
        /// </param>
        protected BlockExtension(BlockRenderer<T> blockRenderer, IEnumerable<BlockParser> blockParsers)
        {
            _blockRenderer = blockRenderer;
            _blockParsers = blockParsers;
        }

        /// <summary>
        /// Set up <see cref="BlockParser"/>s.
        /// </summary>
        /// <param name="markdownPipelineBuilder">
        /// <para>The <see cref="MarkdownPipelineBuilder"/> to register the <see cref="BlockParser"/> to.</para>
        /// <para>Never <c>null</c>.</para>
        /// </param>
        protected virtual void SetupParsers(MarkdownPipelineBuilder markdownPipelineBuilder)
        {
            // Do nothing by default
        }

        /// <summary>
        /// Registers <see cref="BlockParser"/>s.
        /// </summary>
        /// <para>The <see cref="MarkdownPipelineBuilder"/> to register the <see cref="BlockParser"/> to.</para>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pipeline"/> is <c>null</c>.</exception>
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (pipeline == null)
            {
                throw new ArgumentNullException(nameof(pipeline));
            }

            if (_blockParsers != null)
            {
                OrderedList<BlockParser> pipelineBlockParsers = pipeline.BlockParsers;
                foreach (BlockParser blockParser in _blockParsers)
                {
                    if (blockParser == null)
                    {
                        continue;
                    }

                    char[] openingCharacters = blockParser.OpeningCharacters;
                    if (openingCharacters?.Length >= 0)
                    {
                        pipelineBlockParsers.Add(blockParser);
                    }
                    else
                    {
                        pipelineBlockParsers.InsertBefore<ParagraphBlockParser>(blockParser); // Global parsers must be inserted before catch all ParagraphBlockParser
                    }
                }
            }

            SetupParsers(pipeline);
        }

        /// <summary>
        /// Set up <see cref="BlockRenderer{T}"/>s.
        /// </summary>
        /// <param name="markdownPipeline">
        /// <para>The <see cref="MarkdownPipeline"/> to register <see cref="BlockRenderer{T}"/>s to.</para>
        /// <para>Never <c>null</c>.</para>
        /// </param>
        /// <param name="markdownRenderer">
        /// <para>The <see cref="IMarkdownRenderer"/> to register <see cref="BlockRenderer{T}"/>s to.</para>
        /// <para>Never <c>null</c>.</para>
        /// </param>
        protected virtual void SetupRenderers(MarkdownPipeline markdownPipeline, IMarkdownRenderer markdownRenderer)
        {
            // Do nothing by default
        }

        /// <summary>
        /// Registers <see cref="BlockRenderer{T}"/>s.
        /// </summary>
        /// <param name="pipeline">The <see cref="MarkdownPipeline"/> to register <see cref="BlockRenderer{T}"/>s to.</param>
        /// <param name="renderer">The <see cref="IMarkdownRenderer"/> to register <see cref="BlockRenderer{T}"/>s to.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pipeline"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="renderer"/> is <c>null</c>.</exception>
        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (pipeline == null)
            {
                throw new ArgumentNullException(nameof(pipeline));
            }

            if (_blockRenderer != null && renderer is HtmlRenderer)
            {
                renderer.ObjectRenderers.AddIfNotAlready(_blockRenderer);
            }

            SetupRenderers(pipeline, renderer);
        }

        /// <summary>
        /// Callback called when an <see cref="IBlock"/> is closed.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the closed <see cref="IBlock"/>.</param>
        /// <param name="block">The <see cref="IBlock"/> that has just been closed.</param>
        protected virtual void OnBlockClosed(BlockProcessor blockProcessor, T block)
        {
            // Do nothing by default
        }

        /// <summary>
        /// Callback called when an <see cref="IBlock"/> is closed.
        /// </summary>
        /// <param name="blockProcessor">
        /// <para>The <see cref="BlockProcessor"/> processing the closed <see cref="IBlock"/>.</para>
        /// <para>Never <c>null</c>.</para>
        /// </param>
        /// <param name="block">
        /// <para>The <see cref="IBlock"/> that has just been closed.</para>
        /// <para>Never <c>null</c>.</para>
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="block"/> is <c>null</c>.</exception>
        /// <exception cref="BlockException">Thrown if an exception is thrown in callback for a closed <see cref="IBlock"/>.</exception>
        protected void OnClosed(BlockProcessor blockProcessor, T block)
        {
            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            try
            {
                OnBlockClosed(blockProcessor, block);
            }
            catch (Exception exception) when ((exception as BlockException)?.Context != BlockExceptionContext.Block)
            {
                throw new BlockException(block, innerException: exception);
            }
        }
    }
}
