using Markdig.Renderers;
using Markdig.Syntax;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// A renderer that renders <see cref="FlexiSectionBlock"/>s as HTML.
    /// </summary>
    public class FlexiSectionBlockRenderer : BlockRenderer<FlexiSectionBlock>
    {
        private readonly ReadOnlyDictionary<SectioningContentElement, string> _elementNames = new ReadOnlyDictionary<SectioningContentElement, string>(new Dictionary<SectioningContentElement, string> {
            { SectioningContentElement.Section, nameof(SectioningContentElement.Section).ToLower() },
            { SectioningContentElement.Article, nameof(SectioningContentElement.Article).ToLower() },
            { SectioningContentElement.Aside, nameof(SectioningContentElement.Aside).ToLower() },
            { SectioningContentElement.Nav, nameof(SectioningContentElement.Nav).ToLower() }
        });

        private readonly string[] _headingTags = new string[] { "h1", "h2", "h3", "h4", "h5", "h6" };
        private readonly char[] _levels = new char[] { '1', '2', '3', '4', '5', '6' };

        /// <summary>
        /// Renders a <see cref="FlexiSectionBlock"/> as HTML.
        /// </summary>
        /// <param name="htmlRenderer">The renderer to write to.</param>
        /// <param name="block">The <see cref="FlexiSectionBlock"/> to render.</param>
        protected override void WriteBlock(HtmlRenderer htmlRenderer, FlexiSectionBlock block)
        {
            if (!htmlRenderer.EnableHtmlForBlock)
            {
                htmlRenderer.WriteChildren(block, false);
                return;
            }

            if (block.RenderingMode == FlexiSectionBlockRenderingMode.Classic)
            {
                WriteClassic(htmlRenderer, block);
                return;
            }

            WriteStandard(htmlRenderer, block);
        }

        internal virtual void WriteClassic(HtmlRenderer htmlRenderer, FlexiSectionBlock flexiSectionBlock)
        {
            string headingTag = _headingTags[flexiSectionBlock.Level - 1];
            htmlRenderer.
                Write('<').
                Write(headingTag).
                Write('>').
                WriteLeafInline(flexiSectionBlock[0] as LeafBlock).
                Write("</").
                Write(headingTag).
                WriteLine(">").
                WriteChildren(flexiSectionBlock);
        }

        internal virtual void WriteStandard(HtmlRenderer htmlRenderer, FlexiSectionBlock flexiSectionBlock)
        {
            var flexiSectionHeadingBlock = flexiSectionBlock[0] as FlexiSectionHeadingBlock;
            string blockName = flexiSectionBlock.BlockName,
                   linkIcon = flexiSectionBlock.LinkIcon,
                   generatedID = flexiSectionHeadingBlock.GeneratedID;
            bool hasLinkIcon = !string.IsNullOrWhiteSpace(linkIcon),
                 hasGeneratedID = !string.IsNullOrWhiteSpace(generatedID);
            ReadOnlyDictionary<string, string> attributes = flexiSectionBlock.Attributes;

            // Root element
            string elementName = _elementNames[flexiSectionBlock.Element];
            int levelIndex = flexiSectionBlock.Level - 1;
            htmlRenderer.
                Write("<").
                Write(elementName).
                Write(" class=\"").
                Write(blockName).
                WriteBlockKeyValueModifierClass(blockName, "level", _levels[levelIndex]).
                WriteHasFeatureClass(hasLinkIcon, blockName, "link-icon").
                WriteAttributeValue(attributes, "class").
                Write('"').
                Write(hasGeneratedID, ' ', "id=\"", generatedID, "\"");
            if (hasGeneratedID)
            {
                htmlRenderer.WriteAttributesExcept(attributes, "class", "id"); // Generated ID takes precedence
            }
            else
            {
                htmlRenderer.WriteAttributesExcept(attributes, "class");
            }
            htmlRenderer.WriteLine(">");

            // Header
            string headingTag = _headingTags[levelIndex];
            htmlRenderer.
                WriteStartTagLine("header", blockName, "header").
                WriteElementLine(headingTag, blockName, "heading", flexiSectionHeadingBlock).
                WriteStartTagLineWithAttributes("button", blockName, "link-button", "aria-label=\"Copy link\"").
                WriteHtmlFragmentLine(hasLinkIcon, linkIcon, blockName, "link-icon").
                WriteEndTagLine("button").
                WriteEndTag("header");

            // Children
            htmlRenderer.
                WriteChildren(flexiSectionBlock, false). // Doesn't rewrite FlexiSectionHeadingBlock since there is no renderer registered for that block type
                EnsureLine().
                WriteEndTagLine(elementName);
        }
    }
}
