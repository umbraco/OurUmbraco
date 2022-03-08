namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiIncludeBlocksExtension"/> options.
    /// </summary>
    public interface IFlexiIncludeBlocksExtensionOptions : IExtensionOptions<IFlexiIncludeBlockOptions>
    {
        /// <summary>
        /// Gets the base URI for <see cref="FlexiIncludeBlock"/>s in root content.
        /// </summary>
        string BaseUri { get; }
    }
}
