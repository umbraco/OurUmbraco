using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks
{
    /// <summary>
    /// Represents a picture block.
    /// </summary>
    public class FlexiPictureBlock : Block
    {
        /// <summary>
        /// Creates a <see cref="FlexiPictureBlock"/>.
        /// </summary>
        /// <param name="blockName">The <see cref="FlexiPictureBlock"/>'s BEM block name.</param>
        /// <param name="src">The <see cref="FlexiPictureBlock"/>'s source URI.</param>
        /// <param name="alt">The <see cref="FlexiPictureBlock"/>'s alt text.</param>
        /// <param name="lazy">The value specifying whether the <see cref="FlexiPictureBlock"/> loads lazily.</param>
        /// <param name="width">The <see cref="FlexiPictureBlock"/>'s width.</param>
        /// <param name="height">The <see cref="FlexiPictureBlock"/>'s height.</param>
        /// <param name="aspectRatio">The <see cref="FlexiPictureBlock"/>'s aspect ratio.</param>
        /// <param name="exitFullscreenIcon">The <see cref="FlexiPictureBlock"/>'s exit fullscreen icon as an HTML fragment.</param>
        /// <param name="errorIcon">The <see cref="FlexiPictureBlock"/>'s error icon as an HTML fragment.</param>
        /// <param name="spinner">The <see cref="FlexiPictureBlock"/>'s spinner as an HTML fragment.</param>
        /// <param name="attributes">The HTML attributes for the <see cref="FlexiPictureBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiPictureBlock"/>.</param>
        public FlexiPictureBlock(string blockName,
            string src,
            string alt,
            bool lazy,
            double width,
            double height,
            double aspectRatio,
            string exitFullscreenIcon,
            string errorIcon,
            string spinner,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            BlockName = blockName;
            Src = src;
            Alt = alt;
            Lazy = lazy;
            Width = width;
            Height = height;
            AspectRatio = aspectRatio;
            ExitFullscreenIcon = exitFullscreenIcon;
            ErrorIcon = errorIcon;
            Spinner = spinner;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the <see cref="FlexiPictureBlock"/>'s BEM block name.
        /// </summary>
        public string BlockName { get; }

        /// <summary>
        /// Gets the <see cref="FlexiPictureBlock"/>'s source URI.
        /// </summary>
        public string Src { get; }

        /// <summary>
        /// Gets the <see cref="FlexiPictureBlock"/>'s alt text.
        /// </summary>
        public string Alt { get; }

        /// <summary>
        /// Gets the value specifying whether the<see cref="FlexiPictureBlock"/> loads lazily.
        /// </summary>
        public bool Lazy { get; }

        /// <summary>
        /// Gets the <see cref="FlexiPictureBlock"/>'s width.
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Gets the <see cref="FlexiPictureBlock"/>'s height.
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Gets the <see cref="FlexiPictureBlock"/>'s aspect ratio.
        /// </summary>
        public double AspectRatio { get; }

        /// <summary>
        /// Gets the <see cref="FlexiPictureBlock"/>'s exit fullscreen icon as an HTML fragment.
        /// </summary>
        public string ExitFullscreenIcon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiPictureBlock"/>'s error icon as an HTML fragment.
        /// </summary>
        public string ErrorIcon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiPictureBlock"/>'s spinner as an HTML fragment.
        /// </summary>
        public string Spinner { get; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiPictureBlock"/>'s root element.
        /// </summary>
        public virtual ReadOnlyDictionary<string, string> Attributes { get; }
    }
}
