using Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks;
using Markdig;
using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// <para>An abstraction for creating <see cref="IExtensionOptions{T}"/>.</para>
    /// <para>This factory facilitates specifying of <see cref="IExtensionOptions{T}"/> on a per-markdown-to-html-run basis.</para>
    /// </summary>
    /// <typeparam name="TExtensionOptions">The extension options type to create.</typeparam>
    /// <typeparam name="TBlockOptions">The block options type.</typeparam>
    public interface IExtensionOptionsFactory<TExtensionOptions, TBlockOptions>
        where TExtensionOptions : IExtensionOptions<TBlockOptions>
        where TBlockOptions : IBlockOptions<TBlockOptions>
    {
        /// <summary>
        /// <para>Attempts to retrieve a <typeparamref name="TExtensionOptions"/>.</para>
        /// <para>First, this method looks for a <typeparamref name="TExtensionOptions"/> in <see cref="MarkdownParserContext.Properties"/>.</para>
        /// <para>If it is unable to find one, this method looks for a <typeparamref name="TExtensionOptions"/> passed when building the <see cref="MarkdownPipeline"/> using methods
        /// like <see cref="MarkdownPipelineBuilderExtensions.UseFlexiAlertBlocks(MarkdownPipelineBuilder, IFlexiAlertBlocksExtensionOptions)"/>.
        /// Note, <see cref="IFlexiAlertBlocksExtensionOptions"/> implements <see cref="IExtensionOptions{T}"/>.</para>
        /// <para>If both retrieval attempts fail, returns a default <typeparamref name="TExtensionOptions"/>.</para>
        /// </summary>
        /// <param name="blockProcessor">A <see cref="BlockProcessor"/> that might hold a reference to an <see cref="IExtensionOptions{T}"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        TExtensionOptions Create(BlockProcessor blockProcessor);
    }
}
