using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// <para>An abstraction for creating <see cref="IBlockOptions{T}"/>s and <see cref="IExtensionOptions{T}"/>.</para>
    /// <para>This abstraction exists to keep <see cref="IBlock"/> factories dry.</para>
    /// </summary>
    /// <typeparam name="TBlockOptions">The type of <see cref="IBlockOptions{T}"/>s this service creates.</typeparam>
    /// <typeparam name="TExtensionOptions">The type of <see cref="IExtensionOptions{T}"/> this service creates.</typeparam>
    public interface IOptionsService<TBlockOptions, TExtensionOptions>
        where TBlockOptions : IBlockOptions<TBlockOptions>
        where TExtensionOptions : IExtensionOptions<TBlockOptions>
    {
        /// <summary>
        /// <para>Creates a (<typeparamref name="TBlockOptions"/>, <typeparamref name="TExtensionOptions"/>) tuple.</para>
        /// <para>Populates the <typeparamref name="TBlockOptions"/> with JSON from a <see cref="LeafBlock"/>.</para>
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="IBlock"/> this method is creating options for.</param>
        /// <param name="leafBlock">The <see cref="LeafBlock"/> containing JSON.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        /// <exception cref="BlockException">Thrown if the <see cref="LeafBlock"/>'s JSON cannot be deserialized.</exception>
        (TBlockOptions, TExtensionOptions) CreateOptions(BlockProcessor blockProcessor, LeafBlock leafBlock);

        /// <summary>
        /// <para>Creates a (<typeparamref name="TBlockOptions"/>, <typeparamref name="TExtensionOptions"/>) tuple.</para>
        /// <para>Attempts to populate the <typeparamref name="TBlockOptions"/> with options from a <see cref="FlexiOptionsBlock"/>.</para>
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="IBlock"/> this method is creating options for.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        /// <exception cref="BlockException">Thrown if a <see cref="FlexiOptionsBlock"/> is found and its JSON cannot be deserialized.</exception>
        (TBlockOptions, TExtensionOptions) CreateOptions(BlockProcessor blockProcessor);
    }
}
