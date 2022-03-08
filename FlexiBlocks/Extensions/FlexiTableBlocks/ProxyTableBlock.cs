using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// Represents a proxy for parsing <see cref="FlexiTableBlock"/>s.
    /// </summary>
    public class ProxyTableBlock : ContainerBlock, IProxyBlock
    {
        /// <summary>
        /// The lines that constitute the <see cref="FlexiTableBlock"/>. Stored in case the <see cref="FlexiTableBlock"/> needs to be undone.
        /// </summary>
        public StringLineGroup Lines;

        /// <summary>
        /// Creates a <see cref="ProxyTableBlock"/>.
        /// </summary>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiTableBlock"/>.</param>
        public ProxyTableBlock(BlockParser blockParser) : base(blockParser)
        {
            MainTypeName = nameof(FlexiTableBlock);
            Lines = new StringLineGroup(4);
            Rows = new List<Row>();
        }

        /// <inheritdoc />
        public string MainTypeName { get; }

        /// <summary>
        /// Gets the <see cref="FlexiTableBlock"/>'s rows.
        /// </summary>
        public List<Row> Rows { get; }

        /// <summary>
        /// Gets or sets the value indicating whether the <see cref="FlexiTableBlock"/> has header rows.
        /// </summary>
        public bool HasHeaderRows { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FlexiTableBlock"/>'s column definitions.
        /// </summary>
        public List<ColumnDefinition> ColumnDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the number of columns in the <see cref="FlexiTableBlock"/>.
        /// </summary>
        public int NumColumns { get; set; }
    }
}
