using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// Represents a table row.
    /// </summary>
    public class FlexiTableRowBlock : ContainerBlock
    {
        /// <summary>
        /// Creates a <see cref="FlexiTableRowBlock"/>.
        /// </summary>
        /// <param name="isHeaderRow">The value indicating whether the <see cref="FlexiTableRowBlock"/> is a header row.</param>
        public FlexiTableRowBlock(bool isHeaderRow) : base(null)
        {
            IsHeaderRow = isHeaderRow;
        }

        /// <summary>
        /// Gets the value indicating whether the <see cref="FlexiTableRowBlock"/> is a header row.
        /// </summary>
        public bool IsHeaderRow { get; }
    }
}
