using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for parsing fenced <see cref="Block"/>s.
    /// </summary>
    /// <typeparam name="TMain">The type of fenced <see cref="Block"/> this parser parsers.</typeparam>
    /// <typeparam name="TProxy">The type of <see cref="IProxyFencedBlock"/> to collect data for the fenced <see cref="Block"/>.</typeparam>
    public abstract class FencedBlockParser<TMain, TProxy> : ProxyBlockParser<TMain, TProxy>
        where TMain : Block
        where TProxy : Block, IProxyFencedBlock
    {
        private readonly char _fenceChar;
        private readonly IFencedBlockFactory<TMain, TProxy> _fencedBlockFactory;
        private readonly FenceTrailingCharacters _fenceTrailingCharacters;
        private readonly bool _matchingFencesRequired;

        /// <summary>
        /// Creates a <see cref="FencedBlockParser{TMain, TProxy}"/>.
        /// </summary>
        /// <param name="fencedBlockFactory">The factory for creating <typeparamref name="TMain"/>s and <typeparamref name="TProxy"/>s.</param>
        /// <param name="fenceChar">The character used in the <typeparamref name="TMain"/>'s fences.</param>
        /// <param name="matchingFencesRequired">The value specifying whether opening and closing fences must have the same number of characters.</param>
        /// <param name="fenceTrailingCharacters">The value specifying what trailing characters are allowed after the opening fence.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fencedBlockFactory"/> is <c>null</c>.</exception>
        protected FencedBlockParser(IFencedBlockFactory<TMain, TProxy> fencedBlockFactory,
            char fenceChar,
            bool matchingFencesRequired = false,
            FenceTrailingCharacters fenceTrailingCharacters = default)
        {
            _fencedBlockFactory = fencedBlockFactory ?? throw new ArgumentNullException(nameof(fencedBlockFactory));

            _matchingFencesRequired = matchingFencesRequired;
            _fenceTrailingCharacters = fenceTrailingCharacters;
            _fenceChar = fenceChar;
            OpeningCharacters = new char[] { fenceChar };
        }

        /// <summary>
        /// Opens a <typeparamref name="TProxy"/> if a line begins with at least 3 fence characters.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> for the document that contains a line with a fence character as its first character.</param>
        /// <returns>
        /// <see cref="BlockState.None"/> if the current line has code indent.
        /// <see cref="BlockState.None"/> if the current line does not contain an opening fence.
        /// <see cref="BlockState.ContinueDiscard"/> if the current line contains an opening fence and a <typeparamref name="TProxy"/> is opened.
        ///</returns>
        protected override BlockState TryOpenBlock(BlockProcessor blockProcessor)
        {
            if (blockProcessor.IsCodeIndent || !LineContainsOpeningFence(blockProcessor.Line, out int fenceCharCount))
            {
                return BlockState.None;
            }

            // At this point, we know we're in a TMain, but we haven't added a TProxy to NewBlocks. BlockParser.TryOpen can't add block context 
            // to exceptions, so we must manually throw with block context.
            try
            {
                TProxy proxyFencedBlock = _fencedBlockFactory.CreateProxyFencedBlock(blockProcessor.Indent, fenceCharCount, blockProcessor, this);
                blockProcessor.NewBlocks.Push(proxyFencedBlock);
            }
            catch (Exception exception)
            {
                throw new BlockException(typeof(TMain).Name,
                    blockProcessor.LineIndex + 1,
                    blockProcessor.Column,
                    Strings.BlockException_Shared_ExceptionOccurredWhileCreatingBlock,
                    exception);
            }

            return BlockState.ContinueDiscard;
        }

        /// <summary>
        /// Continues a <typeparamref name="TProxy"/> if the current line is not a closing fence.  
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> for the <typeparamref name="TProxy"/> to try continuing.</param>
        /// <param name="block">The <typeparamref name="TProxy"/> to try continuing.</param>
        /// <returns>
        /// <see cref="BlockState.Continue"/> if the current line has code indent.
        /// <see cref="BlockState.Continue"/> if the current line does not contain a closing fence.
        /// <see cref="BlockState.BreakDiscard"/> if the <typeparamref name="TProxy"/> is closed.
        /// </returns>
        protected override BlockState TryContinueBlock(BlockProcessor blockProcessor, TProxy block)
        {
            if (blockProcessor.IsCodeIndent || // Closing fence cannot be indented by more than 3 spaces
                !LineContainsClosingFence(blockProcessor.Line, block.OpeningFenceCharCount))
            {
               UpdateLineStart(blockProcessor, block);
                return BlockState.Continue;
            }

            block.UpdateSpanEnd(blockProcessor.Line.End);
            return BlockState.BreakDiscard;
        }

        /// <inheritdoc />
        protected override TMain CloseProxy(BlockProcessor blockProcessor, TProxy proxyBlock)
        {
            return _fencedBlockFactory.Create(proxyBlock, blockProcessor);
        }

        // Subtracts opening fence indent from current line indent
        internal virtual void UpdateLineStart(BlockProcessor blockProcessor, TProxy block)
        {
            blockProcessor.GoToColumn(blockProcessor.ColumnBeforeIndent + Math.Min(block.OpeningFenceIndent, blockProcessor.Indent));
        }

        internal virtual bool LineContainsOpeningFence(StringSlice line, out int numFenceChars)
        {
            numFenceChars = 0;
            char currentChar = line.CurrentChar;
            while (currentChar == _fenceChar)
            {
                numFenceChars++;
                currentChar = line.NextChar();
            }

            if (numFenceChars < 3)
            {
                return false; // Line must have at least 3 chars. Indices, so (end - start + 1) < 3
            }

            if (_fenceTrailingCharacters == FenceTrailingCharacters.All || currentChar == '\0')
            {
                return true;
            }

            if (_fenceTrailingCharacters == FenceTrailingCharacters.Whitespace)
            {
                line.TrimEnd();
                return line.CurrentChar == '\0';
            }

            if (_fenceTrailingCharacters == FenceTrailingCharacters.AllButFenceCharacter)
            {
                currentChar = line.NextChar();
                while (currentChar != '\0')
                {
                    if (currentChar == _fenceChar)
                    {
                        return false;
                    }
                    currentChar = line.NextChar();
                }
            }

            return true;
        }

        internal virtual bool LineContainsClosingFence(StringSlice line, int openingFenceCharCount)
        {
            int numFenceChars = 0;
            while (line.CurrentChar == _fenceChar)
            {
                numFenceChars++;
                line.NextChar();
            }
            line.TrimEnd();

            return (_matchingFencesRequired && numFenceChars == openingFenceCharCount ||
                !_matchingFencesRequired && numFenceChars >= openingFenceCharCount) && // By default (commonmark specs) closing fence must have as many or more fence chars
                line.CurrentChar == '\0'; // No trailing characters other than whitespace
        }
    }
}
