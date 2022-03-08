using Markdig.Helpers;
using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiAlertBlock"/>s from markdown.
    /// </summary>
    public class FlexiAlertBlockParser : BlockParser<FlexiAlertBlock>
    {
        private const char _openingChar = '!';
        private readonly IFlexiAlertBlockFactory _flexiAlertBlockFactory;

        /// <summary>
        /// Creates a <see cref="FlexiAlertBlockParser"/>.
        /// </summary>
        /// <param name="flexiAlertBlockFactory">The factory for building <see cref="FlexiAlertBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiAlertBlockFactory"/> is <c>null</c>.</exception>
        public FlexiAlertBlockParser(IFlexiAlertBlockFactory flexiAlertBlockFactory)
        {
            OpeningCharacters = new[] { _openingChar };
            _flexiAlertBlockFactory = flexiAlertBlockFactory ?? throw new ArgumentNullException(nameof(flexiAlertBlockFactory));
        }

        /// <summary>
        /// Opens a <see cref="FlexiAlertBlock"/> if a line begins with 0 to 3 spaces followed by '!' and any character other than '['.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the document that contains a line with first non-white-space character '!'.</param>
        /// <returns>
        /// <see cref="BlockState.None"/> if the current line has code indent.
        /// <see cref="BlockState.None"/> if the current line does not start with the expected characters. 
        /// <see cref="BlockState.Continue"/> if a <see cref="FlexiAlertBlock"/> is opened.
        /// </returns>
        protected override BlockState TryOpenBlock(BlockProcessor blockProcessor)
        {
            return ParseLine(blockProcessor, null);
        }

        /// <summary>
        /// Continues a <see cref="FlexiAlertBlock"/> if the current line begins with 0 to 3 spaces followed by '!'.  
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiAlertBlock"/> to try continuing.</param>
        /// <param name="block">The <see cref="FlexiAlertBlock"/> to try continuing.</param>
        /// <returns>
        /// <see cref="BlockState.None"/> if the current line has code indent.
        /// <see cref="BlockState.None"/> if the current line does not begin with the expected characters.
        /// <see cref="BlockState.BreakDiscard"/> if the current line is blank, indicating that the <see cref="FlexiAlertBlock"/> has ended and should be closed.
        /// <see cref="BlockState.Continue"/> if the <see cref="FlexiAlertBlock"/> remains open.
        /// </returns>
        protected override BlockState TryContinueBlock(BlockProcessor blockProcessor, FlexiAlertBlock block)
        {
            return ParseLine(blockProcessor, block);
        }

        /// <summary>
        /// Parses a line.
        /// </summary>
        internal virtual BlockState ParseLine(BlockProcessor blockProcessor, FlexiAlertBlock flexiAlertBlock)
        {
            if (blockProcessor.IsCodeIndent)
            {
                return BlockState.None;
            }

            if (blockProcessor.CurrentChar != _openingChar)
            {
                return blockProcessor.IsBlankLine ? BlockState.BreakDiscard : BlockState.None;
            }

            if (blockProcessor.PeekChar(1) == '[') // Avoid conflicting with images
            {
                return BlockState.None;
            }

            if (flexiAlertBlock == null)
            {
                blockProcessor.NewBlocks.Push(_flexiAlertBlockFactory.Create(blockProcessor, this));
            }
            else
            {
                flexiAlertBlock.UpdateSpanEnd(blockProcessor.Line.End);
            }

            // Skip opening char and first whitespace char following it
            if (blockProcessor.NextChar().IsSpaceOrTab())
            {
                blockProcessor.NextChar();
            }

            return BlockState.Continue;
        }
    }
}
