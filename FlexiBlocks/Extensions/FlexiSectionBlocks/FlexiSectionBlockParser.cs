using Markdig.Helpers;
using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// A parser that creates <see cref="FlexiSectionBlock"/>s from markdown.
    /// </summary>
    public class FlexiSectionBlockParser : BlockParser<FlexiSectionBlock>
    {
        private readonly IFlexiSectionBlockFactory _flexiSectionBlockFactory;

        /// <summary>
        /// Creates a <see cref="FlexiSectionBlockParser"/>.
        /// </summary>
        /// <param name="flexiSectionBlockFactory">The factory for building <see cref="FlexiSectionBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiSectionBlockFactory"/> is <c>null</c>.</exception>
        public FlexiSectionBlockParser(IFlexiSectionBlockFactory flexiSectionBlockFactory)
        {
            OpeningCharacters = new[] { '#' };
            _flexiSectionBlockFactory = flexiSectionBlockFactory ?? throw new ArgumentNullException(nameof(flexiSectionBlockFactory));
        }

        /// <summary>
        /// Opens a <see cref="FlexiSectionBlock"/> if a line begins with 0 to 3 spaces followed by 1-6 unescaped '#' characters followed by a space or the end of the line.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the document that contains a line with first non-white-space character '#'.</param>
        /// <returns>
        /// <see cref="BlockState.None"/> if the current line has code indent.
        /// <see cref="BlockState.None"/> if current line does not begin with the expected characters.
        /// <see cref="BlockState.ContinueDiscard"/> if a <see cref="FlexiSectionBlock"/> is opened.
        /// </returns>
        protected override BlockState TryOpenBlock(BlockProcessor blockProcessor)
        {
            if (blockProcessor.IsCodeIndent)
            {
                return BlockState.None;
            }

            // An ATX heading consists of a string of characters, parsed as inline content, 
            // after an opening sequence of 1–6 unescaped # characters
            char c;
            int level = 1;
            while ((c = blockProcessor.Line.PeekChar(level)) == '#')
            {
                if (++level > 6)
                {
                    return BlockState.None;
                }
            }

            // # characters must be followed by a space or the end of the line (a tab counts as spaces in this situation - https://spec.commonmark.org/0.28/#example-10)
            if (c != ' ' && c != '\t' && c != '\0')
            {
                return BlockState.None;
            }

            // At this point, we know we're in a FlexiSectionBlock, but we haven't added one to NewBlocks. BlockParser.TryOpen can't add block context 
            // to exceptions, so we must manually throw with block context.
            try
            {
                DiscardRedundantCharacters(level, blockProcessor);

                // Create FlexiSectionBlock
                blockProcessor.NewBlocks.Push(_flexiSectionBlockFactory.Create(level, blockProcessor, this));
            }
            catch (Exception exception)
            {
                throw new BlockException(nameof(FlexiSectionBlock),
                    blockProcessor.LineIndex + 1,
                    blockProcessor.Column,
                    Strings.BlockException_Shared_ExceptionOccurredWhileCreatingBlock,
                    exception);
            }

            // Keep open
            return BlockState.ContinueDiscard;
        }

        /// <summary>
        /// Always continues.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiSectionBlock"/> to continue.</param>
        /// <param name="block">The <see cref="FlexiSectionBlock"/> to continue.</param>
        /// <returns>
        /// <see cref="BlockState.Continue"/>.
        /// </returns>
        protected override BlockState TryContinueBlock(BlockProcessor blockProcessor, FlexiSectionBlock block)
        {
            // If BlockState.Skip is returned, this parser ignores the line, allowing other blocks to see if they can be continued. Note that returning BlockState.Skip 
            // will result in the block being closed by default, so we have to manually set IsOpen to true. 
            // 
            // It is important that BlockState.Continue isn't returned, otherwise, Markdig calls BlockProcessor.RestartIndext(), which consumes
            // the line's leading whitespace. This messes up blocks that require the leading whitespace, like code blocks.
            block.IsOpen = true;

            return BlockState.Skip;
        }

        internal virtual void DiscardRedundantCharacters(int level, BlockProcessor blockProcessor)
        {
            ref StringSlice line = ref blockProcessor.Line;
            line.Start += level; // Trim hashes at start
            line.TrimStart(out int numTrimmedFromStart); // Trim whitespaces before first non-space character

            // The optional closing sequence of #s must be preceded by a space and may be followed by spaces only.
            char c;
            int state = 0; // 0 == in trailing spaces, 1 == in closing #s
            for (int i = line.End; i >= line.Start - 1; i--)
            {
                c = line[i];

                if (state == 0)
                {
                    if (c == ' ')
                    {
                        continue;
                    }
                    if (c == '#')
                    {
                        state = 1;
                        continue;
                    }
                }
                if (state == 1)
                {
                    if (c == '#')
                    {
                        continue;
                    }
                    if (c == ' ')
                    {
                        line.End = i - 1;
                    }
                }

                break;
            }

            line.TrimEnd(); // Trim whitespaces after last non-space character
            blockProcessor.Column += level + numTrimmedFromStart;
        }
    }
}
