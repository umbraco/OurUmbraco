using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// Represents a table cell.
    /// </summary>
    public class FlexiTableCellBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiTableCellBlock"/>.
        /// </summary>
        /// <param name="colspan">The <see cref="FlexiTableCellBlock"/>'s column span.</param>
        /// <param name="rowspan">The <see cref="FlexiTableCellBlock"/>'s row span.</param>
        /// <param name="contentAlignment">The <see cref="FlexiTableCellBlock"/>'s content-alignment.</param>
        public FlexiTableCellBlock(int colspan, int rowspan, ContentAlignment contentAlignment) : base(null)
        {
            Colspan = colspan;
            Rowspan = rowspan;
            ContentAlignment = contentAlignment;
        }

        /// <summary>
        /// Gets the <see cref="FlexiTableCellBlock"/>'s column span.
        /// </summary>
        public int Colspan { get; }

        /// <summary>
        /// Gets the <see cref="FlexiTableCellBlock"/>'s row span.
        /// </summary>
        public int Rowspan { get; }

        /// <summary>
        /// Gets the <see cref="FlexiTableCellBlock"/>'s content-alignment.
        /// </summary>
        public ContentAlignment ContentAlignment { get; }
    }
}
