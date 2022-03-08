using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiCodeBlock"/>s from indented code from markdown.
    /// </summary>
    public class IndentedFlexiCodeBlockParser : ProxyBlockParser<FlexiCodeBlock, ProxyLeafBlock>
    {
        private readonly IFlexiCodeBlockFactory _flexiCodeBlockFactory;

        /// <summary>
        /// Creates an <see cref="IndentedFlexiCodeBlockParser"/>.
        /// </summary>
        /// <param name="flexiCodeBlockFactory">The factory for building <see cref="FlexiCodeBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiCodeBlockFactory"/> is <c>null</c>.</exception>
        public IndentedFlexiCodeBlockParser(IFlexiCodeBlockFactory flexiCodeBlockFactory)
        {
            _flexiCodeBlockFactory = flexiCodeBlockFactory ?? throw new ArgumentNullException(nameof(flexiCodeBlockFactory));
        }

        /// <inheritdoc />
        public override bool CanInterrupt(BlockProcessor processor, Block block)
        {
            return !(block is ParagraphBlock); // Indented code blocks can't interrupt paragraph blocks
        }

        /// <summary>
        /// Opens a <see cref="ProxyLeafBlock"/> if a non-blank line begins with 4 or more spaces.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> for the document that contains a non-blank line that begins with 4 or more spaces.</param>
        /// <returns>
        /// <see cref="BlockState.None"/> if the current line is a blank line.
        /// <see cref="BlockState.None"/> if the current line does not have code indent.
        /// <see cref="BlockState.Continue"/> if a <see cref="ProxyLeafBlock"/> is opened.
        /// </returns>
        protected override BlockState TryOpenBlock(BlockProcessor blockProcessor)
        {
            return ParseLine(blockProcessor, null);
        }

        /// <summary>
        /// Continues a <see cref="ProxyLeafBlock"/> if the current line has code indent or is blank.  
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> for the <see cref="ProxyLeafBlock"/> to try continuing.</param>
        /// <param name="block">The <see cref="ProxyLeafBlock"/> to try continuing.</param>
        /// <returns>
        /// <see cref="BlockState.None"/> if the current line does not have code indent and is not blank. This closes the <see cref="ProxyLeafBlock"/>.
        /// <see cref="BlockState.Continue"/> if the <see cref="ProxyLeafBlock"/> remains open.
        /// </returns>
        protected override BlockState TryContinueBlock(BlockProcessor blockProcessor, ProxyLeafBlock block)
        {
            return ParseLine(blockProcessor, block);
        }

        /// <summary>
        /// Closes a <see cref="ProxyLeafBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="ProxyLeafBlock"/>.</param>
        /// <param name="proxyBlock">The <see cref="ProxyLeafBlock"/> to close. Never <c>null</c>.</param>
        /// <returns>
        /// Returns a <see cref="FlexiCodeBlock"/> to replace the <see cref="ProxyLeafBlock"/>.
        /// </returns>
        protected override FlexiCodeBlock CloseProxy(BlockProcessor blockProcessor, ProxyLeafBlock proxyBlock)
        {
            StringLine[] lines = proxyBlock.Lines.Lines;

            // Remove any trailing blankline
            for (int i = proxyBlock.Lines.Count - 1; i >= 0; i--)
            {
                if (lines[i].Slice.IsEmpty)
                {
                    proxyBlock.Lines.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }

            return _flexiCodeBlockFactory.Create(proxyBlock, blockProcessor);
        }

        /// <summary>
        /// Parses a line.
        /// </summary>
        internal virtual BlockState ParseLine(BlockProcessor blockProcessor, ProxyLeafBlock proxyLeafBlock)
        {
            bool isBlankLine = blockProcessor.IsBlankLine;
            bool blockExists = proxyLeafBlock != null;

            if (isBlankLine && !blockExists)
            {
                return BlockState.None;
            }

            if (!blockProcessor.IsCodeIndent && (!blockExists || !isBlankLine))
            {
                return BlockState.None;
            }

            // Reset indent so extra leading spaces are included in the line.
            // E.g before resetting, if the current line is "line" with indent 6, after resetting it will be "  line".
            if (blockProcessor.Indent > 4)
            {
                blockProcessor.GoToCodeIndent();
            }

            if (proxyLeafBlock == null)
            {
                proxyLeafBlock = _flexiCodeBlockFactory.CreateProxyLeafBlock(blockProcessor, this);
                blockProcessor.NewBlocks.Push(proxyLeafBlock);
            }
            else
            {
                proxyLeafBlock.UpdateSpanEnd(blockProcessor.Line.End);
            }

            return BlockState.Continue;
        }
    }
}
