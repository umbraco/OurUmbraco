namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiVideoBlock"/> options.
    /// </summary>
    public interface IFlexiVideoBlockOptions : IMediaBlockOptions<IFlexiVideoBlockOptions>
    {
        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s MIME type.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s poster URI.
        /// </summary>
        string Poster { get; }

        /// <summary>
        /// Gets the value specifying whether to generate a poster for the <see cref="FlexiVideoBlock"/>.
        /// </summary>
        bool GeneratePoster { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s duration.
        /// </summary>
        double Duration { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s play icon as an HTML fragment.
        /// </summary>
        string PlayIcon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s pause icon as an HTML fragment.
        /// </summary>
        string PauseIcon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s fullscreen icon as an HTML fragment.
        /// </summary>
        string FullscreenIcon { get; }
    }
}
