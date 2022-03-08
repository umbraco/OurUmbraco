using Markdig.Helpers;
using Markdig.Parsers;
using System;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiTableBlock"/>s from basic tables from markdown.
    /// </summary>
    public class BasicFlexiTableBlockParser : FlexiTableBlockParser
    {
        /// <summary>
        /// Creates a <see cref="BasicFlexiTableBlockParser"/>.
        /// </summary>
        /// <param name="flexiTableBlockFactory">The factory for building <see cref="FlexiTableBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiTableBlockFactory"/> is <c>null</c>.</exception>
        public BasicFlexiTableBlockParser(IFlexiTableBlockFactory flexiTableBlockFactory) : base(flexiTableBlockFactory)
        {
            OpeningCharacters = new char[] { '|' };
        }

        /// <summary>
        /// Opens a <see cref="ProxyTableBlock"/> if a line is a column definitions line or a row line.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> for the document that contains a line starting with '|'.</param>
        /// <returns>
        /// <see cref="BlockState.None"/> if the current line has code indent.
        /// <see cref="BlockState.None"/> if the current line is neither a column definitions line nor a row line.
        /// <see cref="BlockState.ContinueDiscard"/> if a <see cref="ProxyTableBlock"/> is opened.
        /// </returns>
        protected override BlockState TryOpenBlock(BlockProcessor blockProcessor)
        {
            // A grid table cannot start more than an indent
            if (blockProcessor.IsCodeIndent)
            {
                return BlockState.None;
            }

            // Parse line
            Row row = null;
            StringSlice line = blockProcessor.Line;
            List<ColumnDefinition> columnDefinitions;
            if ((columnDefinitions = TryParseColumnDefinitionsLine(line, '|', -1)) == null &&
                (row = TryParseRowLine(blockProcessor, 0, -1)) == null)
            {
                return BlockState.None;
            }

            // Create ProxyTableBlock
            ProxyTableBlock proxyTableBlock = _flexiTableBlockFactory.CreateProxy(blockProcessor, this);
            if (columnDefinitions != null)
            {
                proxyTableBlock.ColumnDefinitions = columnDefinitions;
                proxyTableBlock.NumColumns = columnDefinitions.Count;
            }
            else
            {
                proxyTableBlock.Rows.Add(row);
                proxyTableBlock.NumColumns = row.Count;
            }
            blockProcessor.NewBlocks.Push(proxyTableBlock);

            // Store current line in case the grid FlexiTableBlock is invalid
            proxyTableBlock.Lines.Add(line);

            return BlockState.ContinueDiscard;
        }

        /// <summary>
        /// Continues a <see cref="ProxyTableBlock"/> if the current line is a column definitions line or a row line.  
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> for the <see cref="ProxyTableBlock"/> to try continuing.</param>
        /// <param name="block">The <see cref="ProxyTableBlock"/> to try continuing.</param>
        /// <returns>
        /// <see cref="BlockState.Break"/> if the current line does not start with '|'. This closes the <see cref="ProxyTableBlock"/>.
        /// <see cref="BlockState.Break"/> if the current line starts with '|' but is not a column definitions line or row line. The <see cref="ProxyTableBlock"/> is replaced with a paragraph block.
        /// <see cref="BlockState.ContinueDiscard"/> if the <see cref="ProxyTableBlock"/> remains open.
        /// </returns>
        protected override BlockState TryContinueBlock(BlockProcessor blockProcessor, ProxyTableBlock block)
        {
            if (blockProcessor.CurrentChar != '|') // No longer in table
            {
                return BlockState.Break;
            }

            // Parse line
            StringSlice line = blockProcessor.Line;
            List<Row> rows = block.Rows;
            bool undo = true;
            int numColumns = block.NumColumns;
            List<ColumnDefinition> columnDefinitions;
            if (block.ColumnDefinitions == null &&
                (columnDefinitions = TryParseColumnDefinitionsLine(line, '|', numColumns)) != null)
            {
                undo = false;
                block.ColumnDefinitions = columnDefinitions;

                foreach (Row headerRow in rows)
                {
                    headerRow.IsHeaderRow = true;
                }
            }

            Row row;
            if (undo && (row = TryParseRowLine(blockProcessor, rows.Count, numColumns)) != null)
            {
                undo = false;
                rows.Add(row);
            }

            if (undo)
            {
                Undo(blockProcessor, block);

                return BlockState.Break;
            }

            // Update span end
            block.UpdateSpanEnd(line.End);

            // Store current line in case the grid FlexiTableBlock is invalid
            block.Lines.Add(line);

            return BlockState.ContinueDiscard;
        }

        // A row line is <cell> one or more times, followed by '|'.
        // <cell> is '|' followed by any number of characters.
        // The character '|' must be escaped like so: "\|", if it is part of a cell's content.
        //
        // Example row: | content |
        internal virtual Row TryParseRowLine(BlockProcessor blockProcessor, int rowIndex, int numCells)
        {
            StringSlice line = blockProcessor.Line;
            int lineIndex = blockProcessor.LineIndex;
            Row row = null;
            int lineStart = line.Start;
            int startOffset = 0;
            char previousChar, currentChar = '\0';
            int columnIndex = 0;
            int numCellsParsed = 0;
            while (line.NextChar() != '\0')
            {
                // Reset
                previousChar = currentChar;
                currentChar = line.CurrentChar;

                if (currentChar != '|' || previousChar == '\\')
                {
                    continue;
                }

                // Create cell
                int endOffset = line.Start - lineStart;
                var cell = new Cell(columnIndex,
                    columnIndex,
                    rowIndex,
                    rowIndex,
                    startOffset,
                    endOffset,
                    lineIndex);

                // Add cell
                row = row ?? new Row();
                row.Add(cell);
                numCellsParsed++;

                // Check if we are at the end
                line.NextChar();
                line.TrimStart();
                currentChar = line.CurrentChar;
                if (currentChar == '\0')
                {
                    // Wrong number of cells
                    if (numCells != -1 && numCellsParsed != numCells)
                    {
                        return null;
                    }

                    ExtractContent(blockProcessor.Line, row);

                    return row;
                }

                // Too many cells
                if (numCellsParsed == numCells)
                {
                    return null;
                }

                // Update
                startOffset = endOffset;
                columnIndex++;
            }

            // Missing ending '|' or no cells
            return null;
        }
    }
}
