using System.IO;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// An abstraction for caching data on disk. 
    /// </summary>
    public interface IDiskCacheService
    {
        /// <summary>
        /// <para>Attempts to get a read-only <see cref="FileStream"/> for the cache file for the specified source.</para>
        /// <para>This method serves as a way to check if a cache file exists and to retrieve it if it does, in one operation. This method returns a read-only 
        /// <see cref="FileStream"/> because cache files aren't meant to be edited once they've been created.</para>
        /// </summary>
        /// <param name="source">The URI of the source whose cache file will be retrieved.</param>
        /// <param name="cacheDirectory">The directory to look for the cache file.</param>
        /// <returns>
        /// The <see cref="FileStream"/> for the cache file if it exists, <c>null</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> is <c>null</c>, whitespace, or an empty string.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="cacheDirectory"/> is <c>null</c>, whitespace, or an empty string.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="cacheDirectory"/> contains invalid path characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an unexpected exception occurs when attempting to retrieve the cache file.</exception>
        FileStream TryGetCacheFile(string source, string cacheDirectory);

        /// <summary>
        /// <para>Gets a read-write <see cref="FileStream"/> for the cache file for the specified source.</para> 
        /// <para>Creates the cache file if it doesn't already exist.</para>
        /// </summary>
        /// <param name="source">The URI of the source to create or get a cache file for.</param>
        /// <param name="cacheDirectory">The directory to look for the cache file or create the cache file in.</param>
        /// <returns>A write only <see cref="FileStream"/> for the cache file with the specified identifier.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> is <c>null</c>, whitespace, or an empty string.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="cacheDirectory"/> is <c>null</c>, whitespace, or an empty string.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="cacheDirectory"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an unexpected exception occurs when creating or opening the cache file.</exception>
        FileStream CreateOrGetCacheFile(string source, string cacheDirectory);
    }
}
