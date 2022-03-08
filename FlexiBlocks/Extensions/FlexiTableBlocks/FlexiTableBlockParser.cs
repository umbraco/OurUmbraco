using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// An abstraction for parsing <see cref="FlexiTableBlock"/>s.
    /// </summary>
    public abstract class FlexiTableBlockParser : ProxyBlockParser<FlexiTableBlock, ProxyTableBlock>
    {
        /// <summary>
        /// The factory for creating <see cref="FlexiTableBlock"/>s.
        /// </summary>
        protected readonly IFlexiTableBlockFactory _flexiTableBlockFactory;

        /// <summary>
        /// Creates a <see cref="FlexiTableBlockParser"/>.
        /// </summary>
        /// <param name="flexiTableBlockFactory">The factory for creating <see cref="FlexiTableBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiTableBlockFactory"/> is <c>null</c>.</exception>
        protected FlexiTableBlockParser(IFlexiTableBlockFactory flexiTableBlockFactory)
        {
            _flexiTableBlockFactory = flexiTableBlockFactory ?? throw new ArgumentNullException(nameof(flexiTableBlockFactory));
        }

        /// <inheritdoc />
        public override bool CanInterrupt(BlockProcessor processor, Block block)
        {
            return !(block is ParagraphBlock); // Table cannot interrupt a paragraph
        }

        /// <summary>
        /// <para>Attempts to parse a line as a column definitions line.</para>
        /// <para>The column definitions line is &lt;column definition&gt; one or more times followed by '+' or '|'.</para>
        /// <para>&lt;column definition&gt; is '+' or '|' followed optionally by ':', followed by one or more '-', followed optionally by ':'.</para>
        /// <para>Example column definitions lines:</para>
        /// <para>+---+---+</para>
        /// <para>|---|---|</para>
        /// <para>+:--+:--:+--:+</para>
        /// <para>|:--|:--:|--:|</para>
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <param name="columnDefinitionStartChar">The start character of each column definition.</param>
        /// <param name="numColumnDefinitions">
        /// <para>The expected number of column definitions.</para> 
        /// <para>If this value is <c>-1</c>, any number of column definitions is allowed.</para>
        /// <para><c>null</c> is returned if the wrong number of column definitions are found.</para>
        /// </param>
        protected virtual List<ColumnDefinition> TryParseColumnDefinitionsLine(StringSlice line, char columnDefinitionStartChar, int numColumnDefinitions)
        {
            List<ColumnDefinition> columnDefinitions = null;
            int lineStart = line.Start;
            int numColumnDefinitionsParsed = 0;
            char currentChar = line.CurrentChar;
            while (currentChar == columnDefinitionStartChar)
            {
                int columnStart = line.Start;

                // Skip startChar
                currentChar = line.NextChar();

                // Eol
                if (currentChar == '\0')
                {
                    return numColumnDefinitions == -1 || numColumnDefinitionsParsed == numColumnDefinitions ? columnDefinitions : null; // null if no column definitions found or wrong number of columns
                }

                // Too many columns
                if (numColumnDefinitionsParsed == numColumnDefinitions)
                {
                    return null;
                }

                // Parse column definition
                bool hasStart = false;
                bool hasEnd = false;

                // Alignment - start
                if (currentChar == ':')
                {
                    hasStart = true;
                    currentChar = line.NextChar();
                }

                // Column definition must have at least one '-'
                if (currentChar != '-')
                {
                    return null;
                }

                // Skip dashes
                while (currentChar == '-')
                {
                    currentChar = line.NextChar();
                }

                // Alignment - end
                if (currentChar == ':')
                {
                    hasEnd = true;
                    currentChar = line.NextChar();
                }

                ContentAlignment contentAlignment = hasStart && hasEnd ? ContentAlignment.Center :
                    hasStart ? ContentAlignment.Start :
                    hasEnd ? ContentAlignment.End :
                    ContentAlignment.None;

                columnDefinitions = columnDefinitions ?? new List<ColumnDefinition>();
                columnDefinitions.Add(new ColumnDefinition(contentAlignment, columnStart - lineStart, line.Start - lineStart));
                numColumnDefinitionsParsed++;
            }

            // No columns definitions or line does not end with columnDefinitionStartChar
            return null;
        }

        /// <summary>
        /// Extracts content for a <see cref="Row"/>.
        /// </summary>
        /// <param name="line">The line to extract content from.</param>
        /// <param name="targetRow">The <see cref="Row"/> to extract content for.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="targetRow"/> is <c>null</c>.</exception>
        protected virtual void ExtractContent(StringSlice line, Row targetRow)
        {
            if (targetRow == null)
            {
                throw new ArgumentNullException(nameof(targetRow));
            }

            int numColumns = targetRow.Count;
            int lineStart = line.Start;
            for (int columnIndex = 0; columnIndex < numColumns;)
            {
                Cell targetCell = targetRow[columnIndex];

                if (targetCell.IsOpen)
                {
                    // Generate slice for cell
                    StringSlice cellLine = line;
                    cellLine.Start = lineStart + targetCell.StartOffset + 1; // Skip '|' or '+', don't trim leading spaces in case we have a block that depends on them
                    cellLine.End = lineStart + targetCell.EndOffset - 1; // Don't include ending '|' or '+'
                    cellLine.TrimEnd();

                    // Add slice to cell
                    targetCell.Lines.Add(cellLine);
                }

                columnIndex = targetCell.EndColumnIndex + 1;
            }
        }

        /// <summary>
        /// Replaces a <see cref="ProxyTableBlock"/> with a <see cref="ParagraphBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="ProxyTableBlock"/> to undo.</param>
        /// <param name="proxyTableBlock">The <see cref="ProxyTableBlock"/> to undo.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="proxyTableBlock"/> is <c>null</c>.</exception>
        protected virtual void Undo(BlockProcessor blockProcessor, ProxyTableBlock proxyTableBlock)
        {
            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            if (proxyTableBlock == null)
            {
                throw new ArgumentNullException(nameof(proxyTableBlock));
            }

            // Discard proxyTableBlock
            ContainerBlock parent = proxyTableBlock.Parent;
            blockProcessor.Discard(proxyTableBlock);

            // Replace with paragraph block
            ParagraphBlockParser parser = blockProcessor.Parsers.FindExact<ParagraphBlockParser>();
            var paragraphBlock = new ParagraphBlock(parser)
            {
                Lines = proxyTableBlock.Lines,
            };
            parent.Add(paragraphBlock);
            blockProcessor.Open(paragraphBlock);
        }

        /// <summary>
        /// Closes a <see cref="ProxyTableBlock"/>, returning a <see cref="FlexiTableBlock"/> to replace it with.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="ProxyTableBlock"/> to close. Never <c>null</c>.</param>
        /// <param name="proxyBlock">The <see cref="ProxyTableBlock"/> to close. Never <c>null</c>.</param>
        protected override FlexiTableBlock CloseProxy(BlockProcessor blockProcessor, ProxyTableBlock proxyBlock)
        {
            return _flexiTableBlockFactory.Create(proxyBlock, blockProcessor);
        }
    }
}
