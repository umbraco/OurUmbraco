using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for parsing multipart <see cref="Block"/>s.
    /// </summary>
    /// <typeparam name="T">The type of multipart <see cref="Block"/> this parser parsers.</typeparam>
    public abstract class MultipartBlockParser<T> : BlockParser<T> where T : ContainerBlock
    {
        internal string OPEN_MULTIPART_BLOCKS_KEY = "openMultipartBlocksKey";

        private readonly IMultipartBlockFactory<T> _multipartBlockFactory;
        private readonly PartType[] _partTypes;
        private readonly int _nameLength;
        private readonly int _numParts;
        private readonly string _name;

        /// <summary>
        /// Creates a <see cref="MultipartBlockParser{T}"/>.
        /// </summary>
        /// <param name="multipartBlockFactory">The factory for creating <typeparamref name="T"/>s.</param>
        /// <param name="name"><typeparamref name="T"/>'s name. Expected in its opening line.</param>
        /// <param name="partTypes">The <see cref="PartType"/> of each part of a <typeparamref name="T"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="multipartBlockFactory"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is <c>null</c>, whitespace or an empty string.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="partTypes"/> is <c>null</c> or empty.</exception>
        protected MultipartBlockParser(IMultipartBlockFactory<T> multipartBlockFactory,
            string name,
            PartType[] partTypes)
        {
            _multipartBlockFactory = multipartBlockFactory ?? throw new ArgumentNullException(nameof(multipartBlockFactory));

            if (!(partTypes?.Length > 0))
            {
                throw new ArgumentException(nameof(partTypes));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            OpeningCharacters = new char[] { '+' };
            _name = name;
            _nameLength = _name.Length;
            _partTypes = partTypes;
            _numParts = _partTypes.Length;
        }

        /// <summary>
        /// Opens a <typeparamref name="T"/> if a line is "+++ &lt;name&gt;". Trailing whitespace is ignored.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> for the document that contains a line with first non-white-space character '+'.</param>
        /// <returns>
        /// <see cref="BlockState.None"/> if the current line has code indent.
        /// <see cref="BlockState.None"/> if the current line is not an opening line.
        /// <see cref="BlockState.ContinueDiscard"/> if a <typeparamref name="T"/> is opened.
        ///</returns>
        protected override BlockState TryOpenBlock(BlockProcessor blockProcessor)
        {
            if (blockProcessor.IsCodeIndent ||
                !IsOpeningLine(blockProcessor)) // Line must be "+++ <name>" (trailing whitespace ignored)
            {
                return BlockState.None;
            }

            // At this point, we know we're in a T, but we haven't added a T to NewBlocks. BlockParser.TryOpen can't add block context 
            // to exceptions, so we must manually throw with block context.
            try
            {
                // Create first part
                blockProcessor.NewBlocks.Push(_multipartBlockFactory.CreatePart(_partTypes[0], blockProcessor));

                // Create multipart block
                T multipartBlock = _multipartBlockFactory.Create(blockProcessor, this);
                blockProcessor.NewBlocks.Push(multipartBlock);
                GetOrCreateOpenMultipartBlocks(blockProcessor).Push(multipartBlock); // Keep track so we can ignore all but the most recently opened block in TryContinueBlock
            }
            catch (Exception exception)
            {
                throw new BlockException(typeof(T).Name,
                    blockProcessor.LineIndex + 1,
                    blockProcessor.Column,
                    Strings.BlockException_Shared_ExceptionOccurredWhileCreatingBlock,
                    exception);
            }

            return BlockState.ContinueDiscard;
        }

        /// <summary>
        /// Continues a <typeparamref name="T"/> if not all its parts have been parsed.  
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> for the <typeparamref name="T"/> to try continuing.</param>
        /// <param name="block">The <typeparamref name="T"/> to try continuing.</param>
        /// <returns>
        /// <see cref="BlockState.Continue"/> if the current line has code indent.
        /// <see cref="BlockState.Continue"/> if the <paramref name="block"/> is not the most recently opened multipart <see cref="Block"/>.
        /// <see cref="BlockState.Continue"/> if the current line is not a part divider line.
        /// <see cref="BlockState.BreakDiscard"/> if the <typeparamref name="T"/> is closed.
        /// <see cref="BlockState.ContinueDiscard"/> if a new part is opened.
        /// </returns>
        protected override BlockState TryContinueBlock(BlockProcessor blockProcessor, T block)
        {
            Stack<ContainerBlock> openMultipartBlocks = GetOrCreateOpenMultipartBlocks(blockProcessor);
            if (blockProcessor.IsCodeIndent ||
                openMultipartBlocks.Peek() != block || // Should never be empty, at minimum, the block we're trying to continue should be in stack
                !IsPartDividerLine(blockProcessor))
            {
                return BlockState.Continue;
            }

            // If all parts have been parsed, close block
            int count = block.Count;
            if (count == _numParts)
            {
                // Remove from stack
                openMultipartBlocks.Pop();

                // Update span end
                block.UpdateSpanEnd(blockProcessor.Line.End);

                return BlockState.BreakDiscard;
            }

            // Close previous part so we can open next part as child of multipart block
            Block lastChild = block.LastChild;
            lastChild.UpdateSpanEnd(blockProcessor.Line.End); // Parts overlap, should not be a problem though. Important thing is that the spans allows us to locate parts.
            blockProcessor.Close(lastChild);

            // Create next part
            blockProcessor.NewBlocks.Push(_multipartBlockFactory.CreatePart(_partTypes[count], blockProcessor));

            return BlockState.ContinueDiscard;
        }

        /// <summary>
        /// Closes a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="blockProcessor">
        /// <para>The <see cref="BlockProcessor"/> processing the <typeparamref name="T"/> to close. Never <c>null</c>.</para>
        /// </param>
        /// <param name="block">
        /// <para>The <typeparamref name="T"/> to close. Never <c>null</c>.</para>
        /// </param>
        /// <exception cref="BlockException">Thrown if the <typeparamref name="T"/> has the wrong number of parts.</exception>
        protected override bool CloseBlock(BlockProcessor blockProcessor, T block)
        {
            int count = block.Count;
            if (count != _numParts)
            {
                // Parent got closed
                throw new BlockException(block, string.Format(Strings.BlockException_MultipartBlockParser_IncorrectNumberOfParts, typeof(T).Name, _numParts, count));
            }

            // Keep block
            return true;
        }

        // "+++" with optional trailing whitespace
        internal virtual bool IsPartDividerLine(BlockProcessor blockProcessor)
        {
            StringSlice line = blockProcessor.Line;

            if (line.CurrentChar != '+' ||
               line.NextChar() != '+' ||
               line.NextChar() != '+')
            {
                return false;
            }

            // Nothing other than whitespace after "+++"
            line.NextChar();
            line.TrimEnd();

            return line.CurrentChar == '\0';
        }

        // "+++ <name>" with optional trailing whitespace
        internal virtual bool IsOpeningLine(BlockProcessor blockProcessor)
        {
            StringSlice line = blockProcessor.Line;

            if (line.NextChar() != '+' ||
               line.NextChar() != '+' ||
               line.NextChar() != ' ')
            {
                return false;
            }

            for (int i = 0; i < _nameLength; i++)
            {
                if (line.NextChar() != _name[i])
                {
                    return false;
                }
            }

            // Nothing other than whitespace after the name. A multipart block's name may begin with the name of another 
            // multipart block, this ensures we don't confuse such blocks.
            line.NextChar();
            line.TrimEnd();

            return line.CurrentChar == '\0';
        }

        // Nested multipart blocks have the same divider lines ("+++"). We must keep track of the order multipart blocks are opened in
        // to know which block a divider line belongs to.
        internal virtual Stack<ContainerBlock> GetOrCreateOpenMultipartBlocks(BlockProcessor blockProcessor)
        {
            MarkdownDocument markdownDocument = blockProcessor.Document;
            if (!(markdownDocument.GetData(OPEN_MULTIPART_BLOCKS_KEY) is Stack<ContainerBlock> openMulipartBlocks))
            {
                openMulipartBlocks = new Stack<ContainerBlock>();
                markdownDocument.SetData(OPEN_MULTIPART_BLOCKS_KEY, openMulipartBlocks);
            }

            return openMulipartBlocks;
        }
    }
}
