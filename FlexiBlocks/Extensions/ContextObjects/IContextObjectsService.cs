using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.ContextObjects
{
    /// <summary>
    /// An abstraction for retrieving and adding context objects.
    /// </summary>
    public interface IContextObjectsService
    {
        /// <summary>
        /// Attempts to retrieve a context object from a <see cref="BlockProcessor"/>.
        /// </summary>
        /// <param name="key">The context object's key.</param>
        /// <param name="blockProcessor">The processor to retrieve the context object from.</param>
        /// <param name="value">The context object.</param>
        /// <returns>True if a context object with the specified key exists. False otherwise. </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        bool TryGetContextObject(object key, BlockProcessor blockProcessor, out object value);

        /// <summary>
        /// <para>Attempts to add a context object to a <see cref="BlockProcessor"/>.</para>
        /// <para>This operation fails if <see cref="BlockProcessor.Context"/> is <c>null</c> and <see cref="BlockProcessor.Parsers"/> does not contain a 
        /// <see cref="ContextObjectsStore"/>. Either of those must be present to store context objects.</para>
        /// </summary>
        /// <param name="key">The context object's key.</param>
        /// <param name="value">The context object.</param>
        /// <param name="blockProcessor">The processor to add the context object to.</param>
        /// <returns>True if the context object is added. False otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        bool TryAddContextObject(object key, object value, BlockProcessor blockProcessor);
    }
}
