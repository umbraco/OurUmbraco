using Markdig.Syntax;
using Newtonsoft.Json;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// <para>An abstraction representing an extension's options.</para>
    /// <para>Implements <see cref="IExtensionOptions{T}"/>.</para>
    /// </summary>
    public abstract class ExtensionOptions<T> : IExtensionOptions<T> where T : class, IBlockOptions<T>
    {
        /// <summary>
        /// Creates an <see cref="ExtensionOptions{T}"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">The default <see cref="IBlockOptions{T}"/> for <see cref="IBlock"/>s created by the extension.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="defaultBlockOptions"/> is <c>null</c>.</exception>
        protected ExtensionOptions(T defaultBlockOptions)
        {
            DefaultBlockOptions = defaultBlockOptions ?? throw new ArgumentNullException(nameof(defaultBlockOptions));
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public T DefaultBlockOptions { get; private set; }
    }
}
