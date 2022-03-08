using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiVideoBlocksExtension"/> options.
    /// </summary>
    public interface IFlexiVideoBlocksExtensionOptions : IMediaBlocksExtensionOptions<IFlexiVideoBlockOptions>
    {
        /// <summary>
        /// Gets the local directory to search for video files in.
        /// </summary>
        new string LocalMediaDirectory { get; }

        /// <summary>
        /// Gets a map of MIME types to file extensions.
        /// </summary>
        ReadOnlyDictionary<string, string> MimeTypes { get; }
    }
}
