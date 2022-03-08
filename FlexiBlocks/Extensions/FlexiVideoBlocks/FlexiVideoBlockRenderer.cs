using Markdig.Renderers;
using System;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks
{
    // TODO once https://github.com/WICG/intrinsicsize-attribute/issues/16 is supported, support it
    /// <summary>
    /// A renderer that renders <see cref="FlexiVideoBlock"/>s as HTML.
    /// </summary>
    public class FlexiVideoBlockRenderer : BlockRenderer<FlexiVideoBlock>
    {
        /// <summary>
        /// Renders a <see cref="FlexiVideoBlock"/> as HTML.
        /// </summary>
        /// <param name="htmlRenderer">The renderer to write to.</param>
        /// <param name="block">The <see cref="FlexiVideoBlock"/> to render.</param>
        protected override void WriteBlock(HtmlRenderer htmlRenderer, FlexiVideoBlock block)
        {
            if (!htmlRenderer.EnableHtmlForBlock)
            {
                return;
            }

            string blockName = block.BlockName,
                poster = block.Poster,
                type = block.Type,
                spinner = block.Spinner,
                pauseIcon = block.PauseIcon,
                playIcon = block.PlayIcon,
                fullscreenIcon = block.FullscreenIcon,
                exitFullscreenIcon = block.ExitFullscreenIcon,
                errorIcon = block.ErrorIcon;
            double width = block.Width,
                //height = block.Height,
                aspectRatio = block.AspectRatio,
                duration = block.Duration;
            bool hasPoster = !string.IsNullOrWhiteSpace(poster),
                hasType = !string.IsNullOrWhiteSpace(type),
                hasWidth = width > 0,
                //hasHeight = height > 0,
                hasAspectRatio = aspectRatio > 0,
                hasDuration = duration > 0,
                hasSpinner = !string.IsNullOrWhiteSpace(spinner),
                hasPauseIcon = !string.IsNullOrWhiteSpace(pauseIcon),
                hasPlayIcon = !string.IsNullOrWhiteSpace(playIcon),
                hasFullscreenIcon = !string.IsNullOrWhiteSpace(fullscreenIcon),
                hasExitFullscreenIcon = !string.IsNullOrWhiteSpace(exitFullscreenIcon),
                hasErrorIcon = !string.IsNullOrWhiteSpace(errorIcon);
            ReadOnlyDictionary<string, string> attributes = block.Attributes;

            // Root element
            htmlRenderer.
                Write("<div class=\"").
                Write(blockName).
                WriteHasFeatureClass(hasPoster, blockName, "poster").
                WriteHasFeatureClass(hasWidth, blockName, "width").
                //WriteBlockHasFeatureClass(hasHeight, blockName, "height").
                WriteHasFeatureClass(hasAspectRatio, blockName, "aspect-ratio").
                WriteHasFeatureClass(hasDuration, blockName, "duration").
                WriteHasFeatureClass(hasType, blockName, "type").
                WriteHasFeatureClass(hasSpinner, blockName, "spinner").
                WriteHasFeatureClass(hasPlayIcon, blockName, "play-icon").
                WriteHasFeatureClass(hasPauseIcon, blockName, "pause-icon").
                WriteHasFeatureClass(hasFullscreenIcon, blockName, "fullscreen-icon").
                WriteHasFeatureClass(hasExitFullscreenIcon, blockName, "exit-fullscreen-icon").
                WriteHasFeatureClass(hasErrorIcon, blockName, "error-icon").
                WriteAttributeValue(attributes, "class").
                Write('"').
                WriteAttributesExcept(attributes, "class").
                WriteLine(">");

            // Container
            htmlRenderer.
                Write("<div class=\"").
                WriteElementClass(blockName, "container").
                Write('"').
                WriteAttribute("tabindex", "-1").
                WriteStyleAttribute(hasWidth, "width", width, "px"). // So error notice is constrained width-wise
                WriteLine(">");

            // Video outer container
            htmlRenderer.
                Write("<div class=\"").
                WriteElementClass(blockName, "video-outer-container").
                Write('"').
                WriteStyleAttribute(hasWidth, "width", width, "px").
                WriteLine(">");

            // Video inner container
            htmlRenderer.
                Write("<div class=\"").
                WriteElementClass(blockName, "video-inner-container").
                Write('"').
                WriteStyleAttribute(hasAspectRatio, "padding-bottom", aspectRatio, "%").
                WriteLine(">");

            // Video
            htmlRenderer.
                Write("<video class=\"").
                WriteElementClass(blockName, "video").
                Write('"').
                WriteAttribute("preload", "auto"). // If this is none, video never loads in edge, even if we set preload to auto before calling VideoElement.load()
                WriteAttribute(hasPoster, "poster", poster). // Above the fold video blocks should have posters
                Write(" muted playsInline disablePictureInPicture loop").
                // WriteAttribute(hasWidth, "width", width). // Needed for https://github.com/WICG/intrinsicsize-attribute/issues/16
                // WriteAttribute(hasHeight, "height", height). // Needed for https://github.com/WICG/intrinsicsize-attribute/issues/16
                WriteLine(">").
                Write("<source class=\"").
                WriteElementClass(blockName, "source").
                Write('"').
                WriteAttribute("data-src", block.Src).
                WriteAttribute(hasType, "type", type).
                WriteLine(">").
                WriteEndTagLine("video").
                WriteEndTagLine("div").
                WriteEndTagLine("div");

            // Controls
            htmlRenderer.
                WriteStartTagLine("div", blockName, "controls").
                WriteStartTagLineWithAttributes("button", blockName, "play-pause-button", "aria-label=\"Pause/play\"").
                WriteHtmlFragmentLine(hasPlayIcon, playIcon, blockName, "play-icon").
                WriteHtmlFragmentLine(hasPauseIcon, pauseIcon, blockName, "pause-icon").
                WriteEndTagLine("button").
                WriteStartTagLine("div", blockName, "elapsed-time").
                WriteElementLine("span", blockName, "current-time", "0:00").
                Write("/").
                WriteElementLine(!hasDuration, "span", blockName, "duration", "0:00").
                WriteElementLine(hasDuration, "span", blockName, "duration", TimeSpan.FromSeconds(Math.Round(duration)).ToString("m\\:ss")). // TODO avoidable allocation
                WriteEndTagLine("div").
                WriteStartTagLine("div", blockName, "progress").
                WriteStartTagLine("div", blockName, "progress-track").
                WriteStartTag("div", blockName, "progress-played").
                WriteEndTagLine("div").
                WriteStartTag("div", blockName, "progress-buffered").
                WriteEndTagLine("div").
                WriteEndTagLine("div").
                WriteEndTagLine("div").
                WriteStartTagLineWithAttributes("button", blockName, "fullscreen-button", "aria-label=\"Toggle fullscreen\"").
                WriteHtmlFragmentLine(hasFullscreenIcon, fullscreenIcon, blockName, "fullscreen-icon").
                WriteHtmlFragmentLine(hasExitFullscreenIcon, exitFullscreenIcon, blockName, "exit-fullscreen-icon").
                WriteEndTagLine("button").
                WriteEndTagLine("div");

            // Error notice
            htmlRenderer.
                WriteStartTagLine("div", blockName, "error-notice").
                WriteHtmlFragmentLine(hasErrorIcon, errorIcon, blockName, "error-icon").
                WriteEndTagLine("div");

            // Spinner
            htmlRenderer.
                WriteHtmlFragmentLine(hasSpinner, spinner, blockName, "spinner").
                WriteEndTagLine("div").
                WriteEndTagLine("div");
        }
    }
}
