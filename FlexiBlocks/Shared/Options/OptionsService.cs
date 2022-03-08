using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// The default implementation of <see cref="IOptionsService{TBlockOptions, TExtensionOptions}"/>.
    /// </summary>
    public class OptionsService<TBlockOptions, TExtensionOptions> : IOptionsService<TBlockOptions, TExtensionOptions>
        where TBlockOptions : IBlockOptions<TBlockOptions>
        where TExtensionOptions : IExtensionOptions<TBlockOptions>
    {
        private readonly IBlockOptionsFactory<TBlockOptions> _blockOptionsFactory;
        private readonly IExtensionOptionsFactory<TExtensionOptions, TBlockOptions> _extensionOptionsFactory;

        /// <summary>
        /// Creates an <see cref="OptionsService{TBlockOptions, TExtensionOptions}"/>.
        /// </summary>
        /// <param name="blockOptionsFactory">The factory for building <see cref="IBlockOptions{T}"/>.</param>
        /// <param name="extensionOptionsFactory">The factory for building <see cref="IExtensionOptions{T}"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockOptionsFactory"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="extensionOptionsFactory"/> is <c>null</c>.</exception>
        public OptionsService(IBlockOptionsFactory<TBlockOptions> blockOptionsFactory,
            IExtensionOptionsFactory<TExtensionOptions, TBlockOptions> extensionOptionsFactory)
        {
            _blockOptionsFactory = blockOptionsFactory ?? throw new ArgumentNullException(nameof(blockOptionsFactory));
            _extensionOptionsFactory = extensionOptionsFactory ?? throw new ArgumentNullException(nameof(extensionOptionsFactory));
        }

        /// <inheritdoc />
        public (TBlockOptions, TExtensionOptions) CreateOptions(BlockProcessor blockProcessor, LeafBlock leafBlock)
        {
            TExtensionOptions extensionOptions = _extensionOptionsFactory.Create(blockProcessor);
            TBlockOptions blockOptions = _blockOptionsFactory.Create(extensionOptions.DefaultBlockOptions, leafBlock);

            return (blockOptions, extensionOptions);
        }

        /// <inheritdoc />
        public (TBlockOptions, TExtensionOptions) CreateOptions(BlockProcessor blockProcessor)
        {
            TExtensionOptions extensionOptions = _extensionOptionsFactory.Create(blockProcessor);
            TBlockOptions blockOptions = _blockOptionsFactory.Create(extensionOptions.DefaultBlockOptions, blockProcessor);

            return (blockOptions, extensionOptions);
        }
    }
}
