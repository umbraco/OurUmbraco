using Markdig.Renderers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiBannerBlocks
{
    /// <summary>
    /// A renderer that renders <see cref="FlexiBannerBlock"/>s as HTML.
    /// </summary>
    public class FlexiBannerBlockRenderer : BlockRenderer<FlexiBannerBlock>
    {
        /// <summary>
        /// Renders a <see cref="FlexiBannerBlock"/> as HTML.
        /// </summary>
        /// <param name="htmlRenderer">The renderer to write to.</param>
        /// <param name="block">The <see cref="FlexiBannerBlock"/> to render.</param>
        protected override void WriteBlock(HtmlRenderer htmlRenderer, FlexiBannerBlock block)
        {
            if (!htmlRenderer.EnableHtmlForBlock)
            {
                htmlRenderer.
                    WriteLeafInline(block[0] as LeafBlock).
                    EnsureLine().
                    WriteLeafInline(block[1] as LeafBlock).
                    EnsureLine();
                return;
            }

            string blockName = block.BlockName;
            string logoIcon = block.LogoIcon;
            string backgroundIcon = block.BackgroundIcon;
            bool hasLogoIcon = !string.IsNullOrWhiteSpace(logoIcon),
                 hasBackgroundIcon = !string.IsNullOrWhiteSpace(backgroundIcon);
            ReadOnlyDictionary<string, string> attributes = block.Attributes;

            // Root element
            htmlRenderer.
                Write("<div class=\"").
                Write(blockName).
                WriteHasFeatureClass(hasLogoIcon, blockName, "logo-icon").
                WriteHasFeatureClass(hasBackgroundIcon, blockName, "background-icon").
                WriteAttributeValue(attributes, "class").
                Write('"').
                WriteAttributesExcept(attributes, "class").
                WriteLine(">");

            // Background
            htmlRenderer.
                WriteHtmlFragmentLine(hasBackgroundIcon, backgroundIcon, blockName, "background-icon");

            // Logo
            htmlRenderer.
                WriteHtmlFragmentLine(hasLogoIcon, logoIcon, blockName, "logo-icon");

            // Title
            htmlRenderer.
                WriteElementLine("h1", blockName, "title", block[0] as LeafBlock);

            // Blurb
            htmlRenderer.
                WriteElementLine("p", blockName, "blurb", block[1] as LeafBlock).
                WriteEndTagLine("div");
        }
    }
}
