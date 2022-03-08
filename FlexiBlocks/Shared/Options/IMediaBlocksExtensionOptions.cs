using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction representing a media <see cref="IBlock"/> extension's options.
    /// </summary>
    public interface IMediaBlocksExtensionOptions<T> : IExtensionOptions<T> where T : IBlockOptions<T>
    {
        /// <summary>
        /// Gets the local directory to search for media files in.
        /// </summary>
        string LocalMediaDirectory { get; }
    }
}
