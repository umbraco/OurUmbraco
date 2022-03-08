using Markdig.Renderers;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks
{
    // TODO once https://github.com/WICG/intrinsicsize-attribute/issues/16 is supported, support it
    /// <summary>
    /// A renderer that renders <see cref="FlexiPictureBlock"/>s as HTML.
    /// </summary>
    public class FlexiPictureBlockRenderer : BlockRenderer<FlexiPictureBlock>
    {
        /// <summary>
        /// Renders a <see cref="FlexiPictureBlock"/> as HTML.
        /// </summary>
        /// <param name="htmlRenderer">The renderer to write to.</param>
        /// <param name="block">The <see cref="FlexiPictureBlock"/> to render.</param>
        protected override void WriteBlock(HtmlRenderer htmlRenderer, FlexiPictureBlock block)
        {
            string alt = block.Alt;
            bool hasAlt = !string.IsNullOrWhiteSpace(alt);

            if (!htmlRenderer.EnableHtmlForBlock)
            {
                htmlRenderer.
                    WriteLine(hasAlt, block.Alt);

                return;
            }

            string blockName = block.BlockName,
                exitFullscreenIcon = block.ExitFullscreenIcon,
                errorIcon = block.ErrorIcon,
                spinner = block.Spinner;
            double width = block.Width,
                //height = block.Height,
                aspectRatio = block.AspectRatio;
            bool isLazy = block.Lazy,
                hasWidth = width > 0,
                //hasHeight = height > 0,
                hasAspectRatio = aspectRatio > 0,
                hasExitFullscreenIcon = !string.IsNullOrWhiteSpace(exitFullscreenIcon),
                hasErrorIcon = !string.IsNullOrWhiteSpace(errorIcon),
                hasSpinner = !string.IsNullOrWhiteSpace(spinner);
            ReadOnlyDictionary<string, string> attributes = block.Attributes;

            // Root element
            htmlRenderer.
                Write("<div class=\"").
                Write(blockName).
                WriteHasFeatureClass(hasAlt, blockName, "alt").
                WriteIsTypeClass(isLazy, blockName, "lazy").
                WriteHasFeatureClass(hasWidth, blockName, "width").
                //WriteBlockHasFeatureClass(hasHeight, blockName, "height").
                WriteHasFeatureClass(hasAspectRatio, blockName, "aspect-ratio").
                WriteHasFeatureClass(hasExitFullscreenIcon, blockName, "exit-fullscreen-icon").
                WriteHasFeatureClass(hasErrorIcon, blockName, "error-icon").
                WriteHasFeatureClass(hasSpinner, blockName, "spinner").
                WriteAttributeValue(attributes, "class").
                Write('"').
                WriteAttributesExcept(attributes, "class").
                WriteLine(">");

            // Button
            htmlRenderer.
                WriteStartTagLineWithAttributes("button", blockName, "exit-fullscreen-button", "aria-label=\"Exit fullscreen\"").
                WriteHtmlFragmentLine(hasExitFullscreenIcon, exitFullscreenIcon, blockName, "exit-fullscreen-icon").
                WriteEndTagLine("button");

            // Container
            htmlRenderer.
                Write("<div class=\"").
                WriteElementClass(blockName, "container").
                Write('"').
                WriteStyleAttribute(hasWidth, "width", width, "px"). // So error notice is constrained width-wise
                WriteLine(">");

            // Error notice
            htmlRenderer.
                WriteStartTagLine("div", blockName, "error-notice").
                WriteHtmlFragmentLine(hasErrorIcon, errorIcon, blockName, "error-icon").
                WriteEndTagLine("div");

            // Spinner
            htmlRenderer.
                WriteHtmlFragmentLine(hasSpinner, spinner, blockName, "spinner");

            // Picture container
            htmlRenderer.
                Write("<div class=\"").
                WriteElementClass(blockName, "picture-container").
                Write('"').
                WriteStyleAttribute(hasWidth, "width", width, "px").
                WriteLine(">");

            // Picture
            htmlRenderer.
                Write("<picture class=\"").
                WriteElementClass(blockName, "picture").
                Write('"').
                WriteStyleAttribute(hasAspectRatio, "padding-bottom", aspectRatio, "%").
                WriteLine(">").
                Write("<img class=\"").
                WriteElementClass(blockName, "image").
                Write('"').
                WriteEscapedUrlAttribute(isLazy, "data-src", block.Src).
                WriteEscapedUrlAttribute(!isLazy, "src", block.Src).
                WriteAttribute(hasAlt, "alt", alt).
                // WriteAttribute(hasWidth, "width", width). // Needed for https://github.com/WICG/intrinsicsize-attribute/issues/16
                // WriteAttribute(hasHeight, "height", height). // Needed for https://github.com/WICG/intrinsicsize-attribute/issues/16
                WriteAttribute("tabindex", "-1"). // So when image is focused, esc keydown events fire
                WriteLine(">"). // img is a void element, doesn't need a forward slash
                WriteEndTagLine("picture").
                WriteEndTagLine("div").
                WriteEndTagLine("div").
                WriteEndTagLine("div");
        }
    }
}
