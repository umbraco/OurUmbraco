using Markdig.Parsers;
using Markdig.Syntax;
using System;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks
{
    /// <summary>
    /// The default implementation of <see cref="IFlexiAlertBlockFactory"/>.
    /// </summary>
    public class FlexiAlertBlockFactory : IFlexiAlertBlockFactory
    {
        private readonly IOptionsService<IFlexiAlertBlockOptions, IFlexiAlertBlocksExtensionOptions> _optionsService;

        /// <summary>
        /// Creates a <see cref="FlexiAlertBlockFactory"/>.
        /// </summary>
        /// <param name="optionsService">The service for creating <see cref="IFlexiAlertBlockOptions"/> and <see cref="IFlexiAlertBlocksExtensionOptions"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsService"/> is <c>null</c>.</exception>
        public FlexiAlertBlockFactory(IOptionsService<IFlexiAlertBlockOptions, IFlexiAlertBlocksExtensionOptions> optionsService)
        {
            _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
        }

        /// <inheritdoc />
        public FlexiAlertBlock Create(BlockProcessor blockProcessor, BlockParser blockParser)
        {
            (IFlexiAlertBlockOptions flexiAlertBlockOptions, IFlexiAlertBlocksExtensionOptions flexiAlertBlocksExtensionOptions) = _optionsService.CreateOptions(blockProcessor);

            // Block name
            string blockName = ResolveBlockName(flexiAlertBlockOptions.BlockName);

            // Type
            string type = ResolveType(flexiAlertBlockOptions.Type);

            // Icon
            string icon = ResolveIcon(flexiAlertBlockOptions.Icon, type, flexiAlertBlocksExtensionOptions.Icons);

            // Create block
            return new FlexiAlertBlock(blockName, type, icon, flexiAlertBlockOptions.Attributes, blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, blockProcessor.Line.End) // Might be the only line, parser will update end if there are more lines
                // Line is assigned by BlockProcessor
            };
        }

        internal virtual string ResolveBlockName(string blockName)
        {
            return string.IsNullOrWhiteSpace(blockName) ? "flexi-alert" : blockName;
        }

        internal virtual string ResolveType(string type)
        {
            return string.IsNullOrWhiteSpace(type) ? "info" : type;
        }

        internal virtual string ResolveIcon(string icon, string type, ReadOnlyDictionary<string, string> icons)
        {
            if (string.IsNullOrWhiteSpace(icon))
            {
                icons.TryGetValue(type, out icon);
            }

            return icon;
        }
    }
}
