using Jering.Markdig.Extensions.FlexiBlocks.ContextObjects;
using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// The default implementation of <see cref="IExtensionOptionsFactory{TExtensionOptions, TBlockOptions}"/>.
    /// </summary>
    /// <typeparam name="TExtensionOptions">The extension options type.</typeparam>
    /// <typeparam name="TDefaultExtensionOptions">The default extension options concrete type.</typeparam>
    /// <typeparam name="TBlockOptions">The block options type.</typeparam>
    public class ExtensionOptionsFactory<TExtensionOptions, TDefaultExtensionOptions, TBlockOptions> : IExtensionOptionsFactory<TExtensionOptions, TBlockOptions>
        where TExtensionOptions : IExtensionOptions<TBlockOptions>
        where TDefaultExtensionOptions : class, TExtensionOptions, new()
        where TBlockOptions : IBlockOptions<TBlockOptions>
    {
        private readonly IContextObjectsService _contextObjectsService;
        private TDefaultExtensionOptions _defaultExtensionOptions;

        /// <summary>
        /// Creates an <see cref="ExtensionOptionsFactory{TExtensionOptions, TDefaultExtensionOptions, TBlockOptions}"/>.
        /// </summary>
        /// <param name="contextObjectService">The service used to try retrieve an <see cref="IExtensionOptions{T}"/> from a <see cref="BlockProcessor"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="contextObjectService"/> is <c>null</c>.</exception>
        public ExtensionOptionsFactory(IContextObjectsService contextObjectService)
        {
            _contextObjectsService = contextObjectService ?? throw new ArgumentNullException(nameof(contextObjectService));
        }

        /// <inheritdoc />
        public TExtensionOptions Create(BlockProcessor blockProcessor)
        {
            if (_contextObjectsService.TryGetContextObject(typeof(TExtensionOptions), blockProcessor, out object value) &&
                value is TExtensionOptions result)
            {
                return result;
            }

            return _defaultExtensionOptions ?? (_defaultExtensionOptions = new TDefaultExtensionOptions());
        }
    }
}
