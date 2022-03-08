using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiAlertBlocksExtension"/> options.
    /// </summary>
    public interface IFlexiAlertBlocksExtensionOptions : IExtensionOptions<IFlexiAlertBlockOptions>
    {
        /// <summary>
        /// Gets a map of <see cref="FlexiAlertBlock"/> types to icon HTML fragments.
        /// </summary>
        ReadOnlyDictionary<string, string> Icons { get; }
    }
}
