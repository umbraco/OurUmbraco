using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// An abstraction for retrieving content from sources.
    /// </summary>
    public interface IContentRetrieverService
    {
        /// <summary>
        /// Retrieves content from a source.
        /// </summary>
        /// <param name="source">The <see cref="Uri"/> of the source to retrieve from.</param>
        /// <param name="cacheDirectory">
        /// <para>The directory to cache content from remote sources in.</para>
        /// <para>If <c>null</c>, whitespace or an empty string, content from remote sources will not be cached.</para>
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation.</param>
        /// <returns>The retrieved content as a read-only collection of the lines.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> has an unsupported scheme.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> is an invalid local URI.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> is remote and does not exist.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> is remote and access to it is forbidden.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> is remote and cannot be retrieved from after multiple attempts.</exception>
        ReadOnlyCollection<string> GetContent(Uri source, string cacheDirectory = null, CancellationToken cancellationToken = default);
    }
}
