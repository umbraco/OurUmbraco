using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for creating <see cref="IBlockOptions{T}"/>s.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IBlockOptions{T}"/> this factory creates.</typeparam>
    public interface IBlockOptionsFactory<T> where T : IBlockOptions<T>
    {
        /// <summary>
        /// <para>Creates an <see cref="IBlockOptions{T}"/> for an <see cref="IBlock"/>.</para>
        /// <para>Attempts to populate the <see cref="IBlockOptions{T}"/> with options from a <see cref="FlexiOptionsBlock"/>.</para>
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>The default <see cref="IBlockOptions{T}"/>.</para>
        /// <para>If a <see cref="FlexiOptionsBlock"/> is found, this value is cloned and values in the clone with alternatives in the <see cref="FlexiOptionsBlock"/> are overriden.</para>
        /// </param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="IBlock"/> this method is creating options for.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="defaultBlockOptions"/> is <c>null</c>.</exception>
        /// <exception cref="BlockException">Thrown if the <see cref="FlexiOptionsBlock"/>'s JSON cannot be deserialized.</exception>
        T Create(T defaultBlockOptions, BlockProcessor blockProcessor);

        /// <summary>
        /// <para>Creates an <see cref="IBlockOptions{T}"/> for an <see cref="IBlock"/>.</para>
        /// <para>Populates the <see cref="IBlockOptions{T}"/> with JSON from a <see cref="LeafBlock"/>.</para>
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>The default <see cref="IBlockOptions{T}"/>.</para>
        /// <para>This value is cloned and values in the clone with alternatives in the <see cref="LeafBlock"/>'s JSON are overriden.</para>
        /// </param>
        /// <param name="leafBlock">The <see cref="LeafBlock"/> containing JSON.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="defaultBlockOptions"/> is <c>null</c>.</exception>
        /// <exception cref="BlockException">Thrown if the <see cref="LeafBlock"/>'s JSON cannot be deserialized.</exception>
        T Create(T defaultBlockOptions, LeafBlock leafBlock);
    }
}
