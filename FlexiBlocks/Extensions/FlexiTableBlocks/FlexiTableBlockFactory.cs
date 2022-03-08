using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// The default implementation of <see cref="IFlexiTableBlockFactory"/>.
    /// </summary>
    public class FlexiTableBlockFactory : IFlexiTableBlockFactory
    {
        private readonly IOptionsService<IFlexiTableBlockOptions, IFlexiTableBlocksExtensionOptions> _optionsService;

        /// <summary>
        /// Creates a <see cref="FlexiTableBlockFactory"/>.
        /// </summary>
        /// <param name="optionsService">The service for creating <see cref="IFlexiTableBlockOptions"/> and <see cref="IFlexiTableBlocksExtensionOptions"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsService"/> is <c>null</c>.</exception>
        public FlexiTableBlockFactory(IOptionsService<IFlexiTableBlockOptions, IFlexiTableBlocksExtensionOptions> optionsService)
        {
            _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
        }

        /// <inheritdoc />
        public FlexiTableBlock Create(ProxyTableBlock proxyTableBlock, BlockProcessor blockProcessor)
        {
            (IFlexiTableBlockOptions flexiTableBlockOptions, IFlexiTableBlocksExtensionOptions _) = _optionsService.
                CreateOptions(blockProcessor);

            // Block name
            string blockName = ResolveBlockName(flexiTableBlockOptions.BlockName);

            // Type
            FlexiTableType type = flexiTableBlockOptions.Type;
            ValidateType(type);

            // Create block
            return CreateFlexiTableBlock(blockName, type, flexiTableBlockOptions.Attributes, proxyTableBlock, blockProcessor);
        }

        /// <inheritdoc />
        public ProxyTableBlock CreateProxy(BlockProcessor blockProcessor, BlockParser blockParser)
        {
            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            return new ProxyTableBlock(blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, blockProcessor.Line.End)
                // Line is assigned by BlockProcessor
            };
        }

        internal virtual string ResolveBlockName(string blockName)
        {
            return string.IsNullOrWhiteSpace(blockName) ? "flexi-table" : blockName;
        }

        internal virtual void ValidateType(FlexiTableType type)
        {
            if (!Enum.IsDefined(typeof(FlexiTableType), type))
            {
                throw new OptionsException(nameof(IFlexiTableBlockOptions.Type),
                        string.Format(Strings.OptionsException_Shared_ValueMustBeAValidEnumValue,
                            type,
                            nameof(FlexiTableType)));
            }
        }

        internal virtual FlexiTableBlock CreateFlexiTableBlock(string blockName,
            FlexiTableType type,
            ReadOnlyDictionary<string, string> attributes,
            ProxyTableBlock proxyTableBlock,
            BlockProcessor blockProcessor)
        {
            // Create table block
            var flexiTableBlock = new FlexiTableBlock(blockName, type, attributes, proxyTableBlock.Parser)
            {
                Column = proxyTableBlock.Column,
                Line = proxyTableBlock.Line,
                Span = proxyTableBlock.Span
            };

            // Create row blocks
            bool headerRowFound = false;
            BlockProcessor childBlockProcessor = blockProcessor.CreateChild();
            List<Row> rows = proxyTableBlock.Rows;
            List<ColumnDefinition> columnDefinitions = proxyTableBlock.ColumnDefinitions;
            int numColumns = proxyTableBlock.NumColumns;
            int numRows = rows.Count;
            bool typeIsUnresponsive = type == FlexiTableType.Unresponsive;
            for (int rowIndex = 0; rowIndex < numRows; rowIndex++)
            {
                Row row = rows[rowIndex];
                bool isHeaderRow = row.IsHeaderRow;
                if (!typeIsUnresponsive && isHeaderRow)
                {
                    if (headerRowFound)
                    {
                        throw new OptionsException(nameof(IFlexiTableBlockOptions.Type), Strings.OptionsException_FlexiTableBlockFactory_TypeInvalidForTablesWithMultipleHeaderRows);
                    }
                    headerRowFound = true;
                }

                // Create and add row
                flexiTableBlock.Add(CreateFlexiTableRowBlock(type,
                    childBlockProcessor,
                    columnDefinitions,
                    numColumns,
                    rowIndex,
                    row,
                    isHeaderRow));
            }

            // Release for reuse
            childBlockProcessor.ReleaseChild();

            return flexiTableBlock;
        }

        internal virtual FlexiTableRowBlock CreateFlexiTableRowBlock(FlexiTableType type,
            BlockProcessor childBlockProcessor,
            List<ColumnDefinition> columnDefinitions,
            int numColumns,
            int rowIndex,
            Row row,
            bool isHeaderRow)
        {
            var flexiTableRowBlock = new FlexiTableRowBlock(isHeaderRow);
            for (int columnIndex = 0; columnIndex < numColumns;)
            {
                Cell cell = row[columnIndex];
                columnIndex = cell.EndColumnIndex + 1;

                if (cell.StartRowIndex < rowIndex) // Cell with rowspan that's already been created
                {
                    continue;
                }

                // Create cell block
                FlexiTableCellBlock flexiTableCellBlock = CreateFlexiTableCellBlock(type, childBlockProcessor, columnDefinitions, cell);

                // Add cell block to row block
                flexiTableRowBlock.Add(flexiTableCellBlock);
            }

            // Could be empty if all cells have rowspan and have already been created. We return it anyway since it could be relevant.
            // For example, if we remove this row and some cells in the row have rowspan 3 while others only have rowspan 2, the cells 
            // with rowspan 3 will erroneously span into an extra row.
            return flexiTableRowBlock;
        }

        internal virtual FlexiTableCellBlock CreateFlexiTableCellBlock(FlexiTableType type,
            BlockProcessor childBlockProcessor,
            List<ColumnDefinition> columnDefinitions,
            Cell cell)
        {
            // Colspan and rowspan
            int startColumnIndex = cell.StartColumnIndex;
            int colspan = cell.EndColumnIndex - startColumnIndex + 1;
            int rowspan = cell.EndRowIndex - cell.StartRowIndex + 1;

            if (type != FlexiTableType.Unresponsive && (rowspan > 1 || colspan > 1))
            {
                throw new OptionsException(nameof(IFlexiTableBlockOptions.Type), Strings.OptionsException_FlexiTableBlockFactory_TypeInvalidForTablesWithCellsThatHaveRowspanOrColspan);
            }

            // Create
            var flexiTableCellBlock = new FlexiTableCellBlock(colspan,
                rowspan,
                columnDefinitions?[startColumnIndex].ContentAlignment ?? ContentAlignment.None);

            // Process cell block contents
            childBlockProcessor.LineIndex = cell.LineIndex;
            childBlockProcessor.Open(flexiTableCellBlock);
            ref StringLineGroup stringLineGroup = ref cell.Lines;
            StringLine[] lines = stringLineGroup.Lines;
            int numLines = stringLineGroup.Count;
            for (int i = 0; i < numLines; i++)
            {
                childBlockProcessor.ProcessLine(lines[i].Slice); // TODO some way to indicate column. We can set childBlockProcessor.Column, but ProcessLine resets it to 0.
            }
            childBlockProcessor.Close(flexiTableCellBlock);

            return flexiTableCellBlock;
        }
    }
}
