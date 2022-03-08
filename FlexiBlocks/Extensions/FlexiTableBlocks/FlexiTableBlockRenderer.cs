using Markdig.Renderers;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// A renderer that renders <see cref="FlexiTableBlock"/>s as HTML.
    /// </summary>
    public class FlexiTableBlockRenderer : BlockRenderer<FlexiTableBlock>
    {
        private readonly string[] _types = new string[] { "cards", "fixed-titles", "unresponsive" };
        private readonly string[] _alignments = new string[] { "none", "start", "center", "end" };

        /// <summary>
        /// Renders a <see cref="FlexiTableBlock"/> as HTML.
        /// </summary>
        /// <param name="htmlRenderer">The renderer to write to.</param>
        /// <param name="block">The <see cref="FlexiTableBlock"/> to render.</param>
        protected override void WriteBlock(HtmlRenderer htmlRenderer, FlexiTableBlock block)
        {
            if (!htmlRenderer.EnableHtmlForBlock)
            {
                htmlRenderer.WriteChildren(block, false);
                return;
            }

            ReadOnlyDictionary<string, string> attributes = block.Attributes;
            string blockName = block.BlockName;
            FlexiTableType type = block.Type;

            // Root element
            // Wrap table in a div. Why?
            // - The "auto" algorithm for determining a table's width basically adds up the minimum content widths (MCW) of each column > https://www.w3.org/TR/CSS2/tables.html#auto-table-layout.
            // - When using "overflow-wrap: break-word" MCW does not take soft wrap oppurtunities into account > https://www.w3.org/TR/css-text-3/#valdef-overflow-wrap-break-word. 
            // - The above two points result in long words not wrapping in table cells. Instead, long words cause long cells, in turn causing tables to overflow their parents.
            // - This will no longer be an issue when "overflow-wrap: anywhere" works.
            // - For now, <table> elements must be wrapped in <div>s with "overflow: auto". It is possible to set "overflow: auto" on tables themselves but this will not always work because table widths
            //   are overriden by sum of MCWs of its columns (i.e even if you set a fixed width for a table, it gets overriden in most cases). It is possible to make "overflow: auto" on tables work by 
            //   setting the table's display to block (Github does this), but this is a hack that just happens to work (that "display: block" doesn't affect rendering of the table, which should have
            //   "display: table", is a coincidence).
            htmlRenderer.
                Write("<div class=\"").
                Write(blockName).
                WriteBlockKeyValueModifierClass(blockName, "type", _types[(int)type]).
                WriteAttributeValue(attributes, "class").
                Write('"').
                WriteAttributesExcept(attributes, "class").
                WriteLine(">");

            // Table
            htmlRenderer.WriteStartTagLine("table", blockName, "table");

            // Table - Rows
            FlexiTableRowBlock labelsFlexiTableRowBlock = null;
            int numRows = block.Count;
            bool headStartRendered = false, headEndRendered = false, bodyStartRendered = false;
            for (int rowIndex = 0; rowIndex < numRows; rowIndex++)
            {
                var flexiTableRowBlock = block[rowIndex] as FlexiTableRowBlock;

                if (flexiTableRowBlock.IsHeaderRow)
                {
                    if (!headStartRendered)
                    {
                        htmlRenderer.WriteStartTagLine("thead", blockName, "head");
                        headStartRendered = true;

                        if (type == FlexiTableType.Cards)
                        {
                            labelsFlexiTableRowBlock = flexiTableRowBlock; // TODO we're only using content from first header row in labels, should concatenate content from all header rows
                        }
                    }
                }
                else
                {
                    if (headStartRendered && !headEndRendered) // Table may not have header rows
                    {
                        htmlRenderer.WriteEndTagLine("thead");
                        headEndRendered = true;
                    }

                    if (!bodyStartRendered)
                    {
                        htmlRenderer.WriteStartTagLine("tbody", blockName, "body");
                        bodyStartRendered = true;
                    }
                }

                WriteRowBlock(htmlRenderer, blockName, labelsFlexiTableRowBlock, flexiTableRowBlock);
            }
            htmlRenderer.
                WriteEndTagLine(headStartRendered && !headEndRendered, "thead").
                WriteEndTagLine(bodyStartRendered, "tbody").
                WriteEndTagLine("table").
                WriteEndTagLine("div");
        }

        private void WriteRowBlock(HtmlRenderer htmlRenderer, string blockName, FlexiTableRowBlock labelsFlexiTableRowBlock, FlexiTableRowBlock flexiTableRowBlock)
        {
            bool isHeaderRow = flexiTableRowBlock.IsHeaderRow;
            string cellElementName = isHeaderRow ? "th" : "td";
            string cellClassName = isHeaderRow ? "header" : "data";
            int numCells = flexiTableRowBlock.Count;
            bool renderLabels = !isHeaderRow && labelsFlexiTableRowBlock != null;

            htmlRenderer.WriteStartTagLine("tr", blockName, "row");
            for (int cellIndex = 0; cellIndex < numCells; cellIndex++)
            {
                WriteCellBlock(htmlRenderer,
                    blockName,
                    cellElementName,
                    cellClassName,
                    renderLabels ? labelsFlexiTableRowBlock[cellIndex] as FlexiTableCellBlock : null, // Card type table may not have a head row. Also note, card type table cells can't have colspan.
                    flexiTableRowBlock[cellIndex] as FlexiTableCellBlock);
            }
            htmlRenderer.WriteEndTagLine("tr");
        }

        private void WriteCellBlock(HtmlRenderer htmlRenderer,
            string blockName,
            string elementName,
            string className,
            FlexiTableCellBlock labelFlexiTableCellBlock,
            FlexiTableCellBlock flexiTableCellBlock)
        {
            // Colspan, rowspan and alignment
            htmlRenderer.
                Write('<').
                Write(elementName).
                Write(" class=\"").
                WriteElementClass(blockName, className);
            if (flexiTableCellBlock.ContentAlignment != ContentAlignment.None) // Avoid array access if unecessary
            {
                htmlRenderer.WriteElementKeyValueModifierClass(blockName, className, "align", _alignments[(int)flexiTableCellBlock.ContentAlignment]);
            }
            htmlRenderer.
                Write('"').
                Write(flexiTableCellBlock.Colspan > 1, " colspan=\"", flexiTableCellBlock.Colspan.ToString(), "\""). // TODO avoid allocation. Use a map or String.Create
                Write(flexiTableCellBlock.Rowspan > 1, " rowspan=\"", flexiTableCellBlock.Rowspan.ToString(), "\""). // TODO avoid allocation. Use a map or String.Create
                WriteLine(">");

            // Label
            bool renderLabel = labelFlexiTableCellBlock != null;
            if (renderLabel)
            {
                htmlRenderer.
                    WriteElementLine("div", blockName, "label", labelFlexiTableCellBlock, labelFlexiTableCellBlock.Count == 1). // Don't need a <p> wrapper if there is only 1 leaf block
                    WriteStartTagLine("div", blockName, "content");
            }

            // Content
            htmlRenderer.
                WriteChildren(flexiTableCellBlock, flexiTableCellBlock.Count == 1). // Don't need a <p> wrapper if there is only 1 leaf block
                EnsureLine().
                WriteEndTagLine(renderLabel, "div").
                WriteEndTagLine(elementName);
        }
    }
}
