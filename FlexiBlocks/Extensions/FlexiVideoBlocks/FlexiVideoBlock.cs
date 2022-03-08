using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks
{
    /// <summary>
    /// Represents a video block.
    /// </summary>
    public class FlexiVideoBlock : Block
    {
        /// <summary>
        /// Creates a <see cref="FlexiVideoBlock"/>.
        /// </summary>
        /// <param name="blockName">The <see cref="FlexiVideoBlock"/>'s BEM block name.</param>
        /// <param name="src">The <see cref="FlexiVideoBlock"/>'s source URI.</param>
        /// <param name="type">The <see cref="FlexiVideoBlock"/>'s MIME type.</param>
        /// <param name="poster">The <see cref="FlexiVideoBlock"/>'s poster URI.</param>
        /// <param name="width">The <see cref="FlexiVideoBlock"/>'s width.</param>
        /// <param name="height">The <see cref="FlexiVideoBlock"/>'s height.</param>
        /// <param name="aspectRatio">The <see cref="FlexiVideoBlock"/>'s aspect ratio.</param>
        /// <param name="duration">The <see cref="FlexiVideoBlock"/>'s duration.</param>
        /// <param name="spinner">The <see cref="FlexiVideoBlock"/>'s spinner as an HTML fragment.</param>
        /// <param name="playIcon">The <see cref="FlexiVideoBlock"/>'s play icon as an HTML fragment.</param>
        /// <param name="pauseIcon">The <see cref="FlexiVideoBlock"/>'s pause icon as an HTML fragment.</param>
        /// <param name="fullscreenIcon">The <see cref="FlexiVideoBlock"/>'s fullscreen icon as an HTML fragment.</param>
        /// <param name="exitFullscreenIcon">The <see cref="FlexiVideoBlock"/>'s exit fullscreen icon as an HTML fragment.</param>
        /// <param name="errorIcon">The <see cref="FlexiVideoBlock"/>'s error icon as an HTML fragment.</param>
        /// <param name="attributes">The HTML attributes for the <see cref="FlexiVideoBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiVideoBlock"/>.</param>
        public FlexiVideoBlock(string blockName,
            string src,
            string type,
            string poster,
            double width,
            double height,
            double aspectRatio,
            double duration,
            string spinner,
            string playIcon,
            string pauseIcon,
            string fullscreenIcon,
            string exitFullscreenIcon,
            string errorIcon,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            BlockName = blockName;
            Src = src;
            Type = type;
            Poster = poster;
            Width = width;
            Height = height;
            AspectRatio = aspectRatio;
            Duration = duration;
            Spinner = spinner;
            PlayIcon = playIcon;
            PauseIcon = pauseIcon;
            FullscreenIcon = fullscreenIcon;
            ExitFullscreenIcon = exitFullscreenIcon;
            ErrorIcon = errorIcon;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s BEM block name.
        /// </summary>
        public string BlockName { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s source URI.
        /// </summary>
        public string Src { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s MIME type.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s poster URI.
        /// </summary>
        public string Poster { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s width.
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s height.
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s aspect ratio.
        /// </summary>
        public double AspectRatio { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s duration.
        /// </summary>
        public double Duration { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s spinner as an HTML fragment.
        /// </summary>
        public string Spinner { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s play icon as an HTML fragment.
        /// </summary>
        public string PlayIcon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s pause icon as an HTML fragment.
        /// </summary>
        public string PauseIcon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s fullscreen icon as an HTML fragment.
        /// </summary>
        public string FullscreenIcon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s exit fullscreen icon as an HTML fragment.
        /// </summary>
        public string ExitFullscreenIcon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiVideoBlock"/>'s error icon as an HTML fragment.
        /// </summary>
        public string ErrorIcon { get; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiVideoBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }
    }
}
