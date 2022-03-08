using Markdig.Renderers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// A renderer that renders <see cref="FlexiTabsBlock"/>s as HTML.
    /// </summary>
    public class FlexiTabsBlockRenderer : BlockRenderer<FlexiTabsBlock>
    {
        /// <summary>
        /// Renders a <see cref="FlexiTabsBlock"/> as HTML.
        /// </summary>
        /// <param name="htmlRenderer">The renderer to write to.</param>
        /// <param name="block">The <see cref="FlexiTabsBlock"/> to render.</param>
        protected override void WriteBlock(HtmlRenderer htmlRenderer, FlexiTabsBlock block)
        {
            int numTabBlocks = block.Count;
            if (!htmlRenderer.EnableHtmlForBlock)
            {
                for (int i = 0; i < numTabBlocks; i++)
                {
                    var flexiTabBlock = block[i] as FlexiTabBlock;
                    htmlRenderer.
                        WriteLeafInline(flexiTabBlock[0] as LeafBlock).
                        EnsureLine().
                        WriteChildren(flexiTabBlock[1] as ContainerBlock, false).
                        EnsureLine();
                }

                return;
            }

            string blockName = block.BlockName;
            ReadOnlyDictionary<string, string> attributes = block.Attributes;

            // Root element
            htmlRenderer.
                Write("<div class=\"").
                Write(blockName).
                WriteAttributeValue(attributes, "class").
                Write('"').
                WriteAttributesExcept(attributes, "class").
                WriteLine(">");

            // Tabs
            htmlRenderer.
                WriteStartTagLineWithClasses("div", blockName, "scrollable-indicators", "scrollable-indicators scrollable-indicators_axis_horizontal").
                WriteStartTagLineWithClassesAndAttributes("div", blockName, "tab-list", "scrollable-indicators__scrollable", "role=\"tablist\"");
            for (int i = 0; i < numTabBlocks; i++)
            {
                WriteTab(htmlRenderer, block[i] as FlexiTabBlock, blockName, i);
            }
            htmlRenderer.
                WriteEndTagLine("div").
                WriteStartTagWithClasses("div", "scrollable-indicators", "indicator", "scrollable-indicators__indicator_start").
                WriteEndTagLine("div").
                WriteStartTagWithClasses("div", "scrollable-indicators", "indicator", "scrollable-indicators__indicator_end").
                WriteEndTagLine("div").
                WriteEndTagLine("div");

            // Panels
            for (int i = 0; i < numTabBlocks; i++)
            {
                WritePanel(htmlRenderer, block[i] as FlexiTabBlock, blockName, i);
            }

            htmlRenderer.
                WriteEndTagLine("div");
        }

        internal virtual void WriteTab(HtmlRenderer htmlRenderer, FlexiTabBlock tabBlock, string blockName, int index)
        {
            if (index == 0)
            {
                htmlRenderer.
                    WriteStartTagWithModifierClassAndAttributes("button",
                        blockName,
                        "tab",
                        "selected",
                        "role=\"tab\" aria-selected=\"true\""); // Buttons have tabindex 0 by default
            }
            else
            {
                htmlRenderer.
                    WriteStartTagWithAttributes("button",
                        blockName,
                        "tab",
                        "role=\"tab\" aria-selected=\"false\" tabindex=\"-1\"");
            }

            htmlRenderer.
                WriteLeafInline(tabBlock[0] as LeafBlock).
                WriteEndTagLine("button");
        }

        internal virtual void WritePanel(HtmlRenderer htmlRenderer, FlexiTabBlock tabBlock, string blockName, int index)
        {
            ReadOnlyDictionary<string, string> attributes = tabBlock.Attributes;

            htmlRenderer.
                Write("<div class=\"").
                WriteElementClass(blockName, "tab-panel").
                WriteElementBooleanModifierClass(index > 0, blockName, "tab-panel", "hidden").
                WriteAttributeValue(attributes, "class").
                Write('"').
                WriteAttributesExcept(attributes, "class").
                Write(" tabindex=\"0\" role=\"tabpanel\" aria-label=\"").
                WriteLeafInline(tabBlock[0] as LeafBlock, false). // Html disabled
                WriteLine("\">").
                WriteChildren(tabBlock[1] as ContainerBlock, false). // Same as default HTML for blockquotes
                WriteEndTagLine("div");
        }
    }
}
