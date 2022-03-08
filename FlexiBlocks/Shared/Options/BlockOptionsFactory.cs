using Jering.IocServices.Newtonsoft.Json;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Markdig.Parsers;
using Markdig.Syntax;
using Newtonsoft.Json;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// The default implementation of <see cref="IBlockOptionsFactory{T}"/>.
    /// </summary>
    public class BlockOptionsFactory<T> : IBlockOptionsFactory<T> where T : IBlockOptions<T>
    {
        private readonly IJsonSerializerService _jsonSerializerService;

        /// <summary>
        /// Creates a <see cref="BlockOptionsFactory{T}"/>.
        /// </summary>
        /// <param name="jsonSerializerService">The service that handles JSON deserialization.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="jsonSerializerService"/> is <c>null</c>.</exception>
        public BlockOptionsFactory(IJsonSerializerService jsonSerializerService)
        {
            _jsonSerializerService = jsonSerializerService ?? throw new ArgumentNullException(nameof(jsonSerializerService));
        }

        /// <inheritdoc />
        public virtual T Create(T defaultBlockOptions, BlockProcessor blockProcessor)
        {
            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            // Get FlexiOptionsBlock
            FlexiOptionsBlock flexiOptionsBlock = TryGetFlexiOptionsBlock(blockProcessor);
            if (flexiOptionsBlock == null)
            {
                return defaultBlockOptions; // Returns null if defaultBlockOptions is null
            }

            return Create(defaultBlockOptions, flexiOptionsBlock);
        }

        /// <inheritdoc />
        public virtual T Create(T defaultBlockOptions, LeafBlock leafBlock)
        {
            if (defaultBlockOptions == null)
            {
                throw new ArgumentNullException(nameof(defaultBlockOptions));
            }

            if (leafBlock == null)
            {
                throw new ArgumentNullException(nameof(leafBlock));
            }

            T result = defaultBlockOptions.Clone();

            try
            {
                using (var jsonTextReader = new JsonTextReader(new LeafBlockReader(leafBlock)))
                {
                    _jsonSerializerService.Populate(jsonTextReader, result);
                }
            }
            catch (Exception exception)
            {
                throw new BlockException(leafBlock, string.Format(Strings.OptionsException_BlockOptionsFactory_InvalidJson, leafBlock.Lines.ToString()), exception);
            }

            return result;
        }

        /// <summary>
        /// Returns <c>null</c> if unable to retrieve a <see cref="FlexiOptionsBlock"/>.
        /// </summary>
        internal virtual FlexiOptionsBlock TryGetFlexiOptionsBlock(BlockProcessor processor)
        {
            MarkdownDocument markdownDocument = processor.Document;
            if (markdownDocument.GetData(FlexiOptionsBlockFactory.PENDING_FLEXI_OPTIONS_BLOCK) is FlexiOptionsBlock FlexiOptionsBlock)
            {
                markdownDocument.RemoveData(FlexiOptionsBlockFactory.PENDING_FLEXI_OPTIONS_BLOCK);

                return FlexiOptionsBlock;
            }

            return null;
        }
    }
}
