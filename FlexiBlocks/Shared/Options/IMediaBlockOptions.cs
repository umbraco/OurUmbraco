using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction representing options for a media <see cref="IBlock"/>.
    /// </summary>
    public interface IMediaBlockOptions<T> : IRenderedRootBlockOptions<T> where T : IMediaBlockOptions<T>
    {
        /// <summary>
        /// Gets the media <see cref="IBlock"/>'s exit fullscreen icon as an HTML fragment.
        /// </summary>
        string ExitFullscreenIcon { get; }

        /// <summary>
        /// Gets the media <see cref="IBlock"/>'s width.
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Gets the media <see cref="IBlock"/>'s height.
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Gets the media <see cref="IBlock"/>'s error icon as an HTML fragment.
        /// </summary>
        string ErrorIcon { get; }

        /// <summary>
        /// Gets the media <see cref="IBlock"/>'s spinner as an HTML fragment.
        /// </summary>
        string Spinner { get; }

        /// <summary>
        /// Gets the value specifying whether file operations are enabled for the media <see cref="IBlock"/>.
        /// </summary>
        bool EnableFileOperations { get; }

        /// <summary>
        /// Gets the media <see cref="IBlock"/>'s source URI.
        /// </summary>
        string Src { get; }
    }
}
