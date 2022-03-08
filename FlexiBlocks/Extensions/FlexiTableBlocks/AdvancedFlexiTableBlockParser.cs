using Markdig.Helpers;
using Markdig.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiTableBlock"/>s from advanced tables from markdown.
    /// </summary>
    public class AdvancedFlexiTableBlockParser : FlexiTableBlockParser
    {
        /// <summary>
        /// Creates an <see cref="AdvancedFlexiTableBlockParser"/>.
        /// </summary>
        /// <param name="flexiTableBlockFactory">The factory for building <see cref="FlexiTableBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiTableBlockFactory"/> is <c>null</c>.</exception>
        public AdvancedFlexiTableBlockParser(IFlexiTableBlockFactory flexiTableBlockFactory) : base(flexiTableBlockFactory)
        {
            OpeningCharacters = new char[] { '+' };
        }

        /// <summary>
        /// Opens a <see cref="ProxyTableBlock"/> if a line is a column definitions line.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> for the document that contains a line starting with '+'.</param>
        /// <returns>
        /// <see cref="BlockState.None"/> if the current line has code indent.
        /// <see cref="BlockState.None"/> if the current line is not a column definitions line.
        /// <see cref="BlockState.ContinueDiscard"/> if a <see cref="ProxyTableBlock"/> is opened.
        /// </returns>
        protected override BlockState TryOpenBlock(BlockProcessor blockProcessor)
        {
            // A grid table cannot start more than an indent
            if (blockProcessor.IsCodeIndent)
            {
                return BlockState.None;
            }

            // Check if line is a column definitions line, parses column definitions if it is
            List<ColumnDefinition> columnDefinitions;
            if ((columnDefinitions = TryParseColumnDefinitionsLine(blockProcessor.Line, '+', -1)) == null)
            {
                return BlockState.None;
            }

            // Create ProxyTableBlock
            ProxyTableBlock proxyTableBlock = _flexiTableBlockFactory.CreateProxy(blockProcessor, this);
            proxyTableBlock.ColumnDefinitions = columnDefinitions;
            proxyTableBlock.NumColumns = columnDefinitions.Count;
            blockProcessor.NewBlocks.Push(proxyTableBlock);

            // Store current line in case the grid FlexiTableBlock is invalid
            proxyTableBlock.Lines.Add(blockProcessor.Line);

            return BlockState.ContinueDiscard;
        }

        /// <summary>
        /// Continues a <see cref="ProxyTableBlock"/> if the current line is a content line or a separator line.  
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="ProxyTableBlock"/> to try continuing.</param>
        /// <param name="block">The <see cref="ProxyTableBlock"/> to try continuing.</param>
        /// <returns>
        /// <see cref="BlockState.Break"/> if the current line does not start with '|' or '+'. This closes the <see cref="ProxyTableBlock"/>.
        /// <see cref="BlockState.Break"/> if the current line starts with '|' or '+' but is not a content line or separator line. The <see cref="ProxyTableBlock"/> is replaced with a paragraph block.
        /// <see cref="BlockState.ContinueDiscard"/> if the <see cref="ProxyTableBlock"/> remains open.
        /// </returns>
        protected override BlockState TryContinueBlock(BlockProcessor blockProcessor, ProxyTableBlock block)
        {
            // Parse line
            bool undo;
            if (blockProcessor.CurrentChar == '|') // Content line
            {
                undo = !TryParseContentLine(blockProcessor, block);
            }
            else if (blockProcessor.CurrentChar == '+') // Row separator line
            {
                undo = !TryParseSeparatorLine(blockProcessor.Line, block);
            }
            else // No longer in table
            {
                return BlockState.Break;
            }

            if (undo)
            {
                Undo(blockProcessor, block);
                return BlockState.Break;
            }

            // Update span end
            block.UpdateSpanEnd(blockProcessor.Line.End);

            // Store current line in case the grid FlexiTableBlock is invalid
            block.Lines.Add(blockProcessor.Line);

            return BlockState.ContinueDiscard;
        }

        // A content line is <content> one or more times followed by '|'.
        // <content> is '|' followed by a series of characters, where '|' is aligned with a '+' in the column definitions line.
        //
        // Example content lines: | content |
        internal virtual bool TryParseContentLine(BlockProcessor blockProcessor, ProxyTableBlock proxyTableBlock)
        {
            ref StringSlice line = ref blockProcessor.Line;
            List<ColumnDefinition> columnDefinitions = proxyTableBlock.ColumnDefinitions;
            List<Row> rows = proxyTableBlock.Rows;
            int numRows = rows.Count;
            Row currentRow = numRows > 0 ? rows.Last() : null;

            if (currentRow?.IsOpen == true)
            {
                if (!ValidateCellAlignment(line, columnDefinitions, currentRow)) // Ensure cells are aligned if current row is open)
                {
                    return false;
                }
            }
            else if ((currentRow = TryCreateRow(line, columnDefinitions, currentRow, numRows, blockProcessor)) != null)
            {
                rows.Add(currentRow);
            }
            else
            {
                return false;
            }

            // Divide content amongst cells
            ExtractContent(line, currentRow);

            return true;
        }

        // TODO should return false if there are non whitespace characters after last cell
        // Checks whether cells are aligned
        internal virtual bool ValidateCellAlignment(StringSlice line, List<ColumnDefinition> columnDefinitions, Row currentRow)
        {
            int numColumns = columnDefinitions.Count;
            int lastColumnIndex = numColumns - 1;
            int currentStartColumnIndex = 0;
            for (int columnIndex = 0; columnIndex < numColumns; columnIndex++)
            {
                char columnSeparator = line.PeekChar(columnDefinitions[columnIndex].EndOffset);

                if (columnSeparator == '|')
                {
                    Cell cell = currentRow[currentStartColumnIndex];

                    // Cells must be aligned
                    if (cell.StartColumnIndex != currentStartColumnIndex || cell.EndColumnIndex != columnIndex)
                    {
                        return false;
                    }

                    currentStartColumnIndex = columnIndex + 1;
                }
                else if (columnIndex == lastColumnIndex) // Missing ending '|'
                {
                    return false;
                }
            }

            return true;
        }

        internal virtual Row TryCreateRow(StringSlice line,
            List<ColumnDefinition> columnDefinitions,
            Row lastRow,
            int rowIndex,
            BlockProcessor blockProcessor)
        {
            int lineIndex = blockProcessor.LineIndex;
            int numColumns = columnDefinitions.Count;
            int lastColumnIndex = numColumns - 1;
            int startColumnIndex = 0;
            int startOffset = -1;
            Row result = null;
            for (int columnIndex = 0; columnIndex < numColumns; columnIndex++)
            {
                ColumnDefinition columnDefinition = columnDefinitions[columnIndex];

                if (startOffset == -1)
                {
                    startOffset = columnDefinition.StartOffset;
                }

                char columnSeparator = line.PeekChar(columnDefinition.EndOffset);

                if (columnSeparator == '|')
                {
                    // Check whether last row has an open cell at the current column
                    Cell lastRowCell = lastRow?[startColumnIndex];
                    Cell newRowCell;
                    if (lastRowCell?.IsOpen == true)
                    {
                        // If cells are not aligned, return false
                        if (lastRowCell.StartColumnIndex != startColumnIndex || lastRowCell.EndColumnIndex != columnIndex)
                        {
                            return null;
                        }

                        newRowCell = lastRowCell;
                        newRowCell.EndRowIndex++;
                    }
                    else
                    {
                        if (lastRow != null)
                        {
                            // If any cell within the start and end columns in the previous row is open but unaligned, return false
                            for (int unalignedColumnIndex = lastRowCell.EndColumnIndex + 1; unalignedColumnIndex <= columnIndex;)
                            {
                                Cell unalignedCell = lastRow[unalignedColumnIndex];
                                if (unalignedCell.IsOpen)
                                {
                                    return null;
                                }
                                unalignedColumnIndex = unalignedCell.EndColumnIndex + 1;
                            }
                        }

                        newRowCell = new Cell(startColumnIndex,
                            columnIndex,
                            rowIndex,
                            rowIndex,
                            startOffset,
                            columnDefinition.EndOffset,
                            lineIndex);
                    }

                    // Add cell - note that cells can span multiple columns
                    result = result ?? new Row();
                    for (int i = startColumnIndex; i <= columnIndex; i++)
                    {
                        result.Add(newRowCell);
                    }

                    // Reset
                    startColumnIndex = columnIndex + 1;
                    startOffset = -1;
                }
                else if (columnIndex == lastColumnIndex) // Missing ending '|'
                {
                    return null;
                }
            }

            return result;
        }

        internal virtual bool TryParseSeparatorLine(StringSlice line, ProxyTableBlock proxyTableBlock)
        {
            // There must be an open row before a row separator line
            List<Row> rows = proxyTableBlock.Rows;
            Row lastRow;
            if (rows.Count == 0 || !(lastRow = rows.Last()).IsOpen)
            {
                return false;
            }

            // Parse line
            List<ColumnDefinition> columnDefinitions = proxyTableBlock.ColumnDefinitions;
            if (!proxyTableBlock.HasHeaderRows && line.PeekChar() == '=')  // If a head separator has already been parsed, row separator lines can contain '=' in their content
            {
                if (!TryParseHeadSeparatorLine(line, columnDefinitions, rows))
                {
                    return false;
                }

                // Don't test for header rows anymore
                proxyTableBlock.HasHeaderRows = true;
            }
            else if (!TryParseRowSeparatorLine(line, columnDefinitions, lastRow))
            {
                return false;
            }

            ExtractContent(line, lastRow);

            // Close last row
            lastRow.IsOpen = false;

            return true;
        }

        // A head separator line is <head separator> one or more times followed by '+'.
        // <head separator> is '+' followed by one or more '=', where each '+' is aligned with a '+' in the column definitions line.
        //
        // Example head separator line: +===+===+
        internal virtual bool TryParseHeadSeparatorLine(StringSlice line,
            List<ColumnDefinition> columnDefinitions,
            List<Row> rows)
        {
            // Validate head separator line
            int numColumns = columnDefinitions.Count;
            for (int columnIndex = 0; columnIndex < numColumns; columnIndex++)
            {
                ColumnDefinition columnDefinition = columnDefinitions[columnIndex];

                // Ending '+' must be aligned
                int endOffset = columnDefinition.EndOffset;
                if (line.PeekChar(endOffset) != '+')
                {
                    return false;
                }

                // '='s between '+'s
                for (int i = columnDefinition.StartOffset + 1; i < endOffset; i++)
                {
                    if (line.PeekChar(i) != '=')
                    {
                        return false;
                    }
                }
            }

            // All rows before head row separator line are header rows
            foreach (Row row in rows)
            {
                row.IsHeaderRow = true;
            }

            // Close all cells
            foreach (Cell cell in rows.Last())
            {
                cell.IsOpen = false;
            }

            return true;
        }

        // A row separator line is <row separator> one or more times followed by '+'.
        // <row separator> is '+' followed by one or more '-' or a series of characters, where each '+' is aligned with a '+' in the column definitions line.
        //
        // Example row separator lines:
        //   +---+----+
        //   + multi-row cell +---+
        internal virtual bool TryParseRowSeparatorLine(StringSlice line,
            List<ColumnDefinition> columnDefinitions,
            Row lastRow)
        {
            // General row separators for each cell must either all be open or close for each cell, non-rectangular cells aren't allowed
            int count = lastRow.Count;
            for (int columnIndex = 0; columnIndex < count;)
            {
                Cell cell = lastRow[columnIndex];
                int startCellColumnIndex = cell.StartColumnIndex;
                int endCellColumnIndex = cell.EndColumnIndex;
                bool firstColumn = true;
                for (int cellColumnIndex = startCellColumnIndex; cellColumnIndex <= endCellColumnIndex; cellColumnIndex++)
                {
                    ColumnDefinition columnDefinition = columnDefinitions[cellColumnIndex];

                    // Check whether all content characters are '-'s
                    bool hasNonDashContent = false;
                    int startOffset = columnDefinition.StartOffset;
                    int endOffset = columnDefinition.EndOffset;
                    for (int i = startOffset + 1; i < endOffset; i++)
                    {
                        if (line.PeekChar(i) != '-')
                        {
                            hasNonDashContent = true;
                            break;
                        }
                    }

                    if (!hasNonDashContent && line.PeekChar(startOffset) == '+' && line.PeekChar(endOffset) == '+') // If content is a series of '-', row separator is closed
                    {
                        if (firstColumn)
                        {
                            cell.IsOpen = false;
                        }
                        else if (cell.IsOpen)
                        {
                            return false;
                        }
                    }
                    // Row separator is open
                    else if (firstColumn)
                    {
                        cell.IsOpen = true;
                    }
                    else if (!cell.IsOpen)
                    {
                        return false;
                    }

                    // Ensure that column span isn't greater than existing cell's column span
                    if (cell.IsOpen && cellColumnIndex == endCellColumnIndex && line.PeekChar(endOffset) != '+')
                    {
                        return false;
                    }

                    // Reset
                    firstColumn = false;
                }

                columnIndex = endCellColumnIndex + 1;
            }

            return true;
        }
    }
}
