namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiPictureBlocksExtension"/> options.
    /// </summary>
    public interface IFlexiPictureBlocksExtensionOptions : IMediaBlocksExtensionOptions<IFlexiPictureBlockOptions>
    {
        /// <summary>
        /// Gets the local directory to search for picture files in.
        /// </summary>
        new string LocalMediaDirectory { get; }
    }
}
