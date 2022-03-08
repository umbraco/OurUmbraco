using Markdig.Renderers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiFigureBlocks
{
    /// <summary>
    /// A renderer that renders <see cref="FlexiFigureBlock"/>s as HTML.
    /// </summary>
    public class FlexiFigureBlockRenderer : BlockRenderer<FlexiFigureBlock>
    {
        /// <summary>
        /// Renders a <see cref="FlexiFigureBlock"/> as HTML.
        /// </summary>
        /// <param name="htmlRenderer">The renderer to write to.</param>
        /// <param name="block">The <see cref="FlexiFigureBlock"/> to render.</param>
        protected override void WriteBlock(HtmlRenderer htmlRenderer, FlexiFigureBlock block)
        {
            if (!htmlRenderer.EnableHtmlForBlock)
            {
                htmlRenderer.
                    WriteChildren(block[0] as ContainerBlock, false).
                    EnsureLine().
                    WriteLeafInline(block[1] as LeafBlock).
                    EnsureLine();

                return;
            }

            ReadOnlyDictionary<string, string> attributes = block.Attributes;
            string blockName = block.BlockName,
                   name = block.Name,
                   id = block.ID;
            bool hasName = block.RenderName && !string.IsNullOrWhiteSpace(name);

            // Root element
            htmlRenderer.
                Write("<figure").
                Write(" class=\"").
                Write(blockName).
                WriteHasFeatureClass(hasName, blockName, "name").
                WriteAttributeValue(attributes, "class").
                Write("\"").
                WriteAttribute(!string.IsNullOrWhiteSpace(id), "id", id).
                WriteAttributesExcept(attributes, "class", "id").
                WriteLine(">");

            // Content
            htmlRenderer.
                WriteElementLine("div", blockName, "content", block[0] as ContainerBlock, false);

            // Caption
            htmlRenderer.
                WriteStartTag("figcaption", blockName, "caption").
                WriteElement(hasName, "span", blockName, "name", name, ". ").
                WriteLeafInline(block[1] as LeafBlock).
                WriteEndTagLine("figcaption").
                WriteEndTagLine("figure");
        }
    }
}
