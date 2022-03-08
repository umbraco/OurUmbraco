using Markdig.Parsers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// <para>An abstraction for parsing <see cref="Block"/>s.</para> 
    /// 
    /// <para>This class's primary functionality is consistent exception handling.</para>
    /// 
    /// <para>Exceptions thrown by implementers are wrapped in <see cref="BlockException"/>s with the locations of offending markdown.</para>
    /// 
    /// <para>Without this class's exception handling, exceptions thrown by implementers bubble up through Markdig and
    /// user facing applications with no information on locations of offending markdown.</para>
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Block"/> this parser parsers.</typeparam>
    public abstract class BlockParser<T> : BlockParser where T : Block
    {
        /// <summary>
        /// Opens a <typeparamref name="T"/> if a line contains the expected content.
        /// </summary>
        /// <param name="blockProcessor">
        /// <para>The <see cref="BlockProcessor"/> processing the document that contains the line.</para>
        /// <para>Never <c>null</c>.</para>
        /// </param>
        protected abstract BlockState TryOpenBlock(BlockProcessor blockProcessor);

        /// <summary>
        /// Opens a <typeparamref name="T"/> if a line contains the expected content.
        /// </summary>
        /// <param name="processor">The <see cref="BlockProcessor"/> processing the document that contains the line.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="processor"/> is <c>null</c>.</exception>
        /// <exception cref="BlockException">Thrown if an exception is thrown while opening the <see cref="Block"/>.</exception>
        public sealed override BlockState TryOpen(BlockProcessor processor)
        {
            if (processor == null)
            {
                throw new ArgumentNullException(nameof(processor));
            }

            try
            {
                return TryOpenBlock(processor);
            }
            catch (Exception exception) when ((exception as BlockException)?.Context != BlockExceptionContext.Block)
            {
                Stack<Block> newBlocks = processor.NewBlocks;
                // If TryOpenBlock added multiple blocks, the block on top is likely to be the parent of the rest, 
                // so we use its location as the location of the offending markdown.
                Block newBlock = newBlocks.Count == 0 ? null : newBlocks.Peek();

                if (newBlock == null) // Didn't get far enough to know what kind of block we're parsing
                {
                    // Can't add a more specific context
                    if ((exception as BlockException)?.Context == BlockExceptionContext.Line)
                    {
                        throw;
                    }

                    throw new BlockException(processor.LineIndex + 1,
                        processor.Column,
                        string.Format(Strings.BlockException_BlockParser_ExceptionOccurredWhileAttemptingToOpenBlock, GetType().Name),
                        exception);
                }
                else
                {
                    throw new BlockException(newBlock, innerException: exception);
                }
            }
        }

        /// <summary>
        /// Continues a <typeparamref name="T"/> if requirements are met.
        /// </summary>
        /// <param name="blockProcessor">
        /// <para>The <see cref="BlockProcessor"/> processing the <typeparamref name="T"/> to try and continue.</para>
        /// <para>Never <c>null</c>.</para>
        /// </param>
        /// <param name="block">
        /// <para>The <typeparamref name="T"/> to try and continue.</para>
        /// <para>Never <c>null</c>.</para>
        /// </param>
        protected virtual BlockState TryContinueBlock(BlockProcessor blockProcessor, T block)
        {
            // Do nothing by default
            return BlockState.None;
        }

        /// <summary>
        /// Continues a <typeparamref name="T"/> if requirements are met.
        /// </summary>
        /// <param name="processor">The <see cref="BlockProcessor"/> processing the <typeparamref name="T"/> to try and continue.</param>
        /// <param name="block">The <typeparamref name="T"/> to try and continue.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="processor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="block"/> is not of type <typeparamref name="T"/>.</exception>
        /// <exception cref="BlockException">Thrown if an exception is thrown while continuing the <typeparamref name="T"/>.</exception>
        public sealed override BlockState TryContinue(BlockProcessor processor, Block block)
        {
            if (processor == null)
            {
                throw new ArgumentNullException(nameof(processor));
            }

            if (!(block is T blockAsT))
            {
                throw new ArgumentException(string.Format(Strings.ArgumentException_Shared_ValueMustBeOfExpectedType,
                    nameof(block),
                    typeof(T).Name,
                    block.GetType().Name));
            }

            try
            {
                return TryContinueBlock(processor, blockAsT);
            }
            catch (Exception exception) when ((exception as BlockException)?.Context != BlockExceptionContext.Block)
            {
                throw new BlockException(block, innerException: exception);
            }
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
        /// <returns>False if the block should be discarded, true otherwise.</returns>
        protected virtual bool CloseBlock(BlockProcessor blockProcessor, T block)
        {
            // Keep the block by default
            return true;
        }

        /// <summary>
        /// Closes a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="processor">The <see cref="BlockProcessor"/> processing the <typeparamref name="T"/> to close.</param>
        /// <param name="block">The <typeparamref name="T"/> to close.</param>
        /// <returns>False if the block should be discarded, true otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="processor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="block"/> is not of type <typeparamref name="T"/>.</exception>
        /// <exception cref="BlockException">Thrown if an exception is thrown while attempting to close the <typeparamref name="T"/>.</exception>
        public sealed override bool Close(BlockProcessor processor, Block block)
        {
            if (processor == null)
            {
                throw new ArgumentNullException(nameof(processor));
            }

            if (!(block is T blockAsT))
            {
                throw new ArgumentException(string.Format(Strings.ArgumentException_Shared_ValueMustBeOfExpectedType,
                    nameof(block),
                    typeof(T).Name,
                    block.GetType().Name));
            }

            try
            {
                return CloseBlock(processor, blockAsT);
            }
            catch (Exception exception) when ((exception as BlockException)?.Context != BlockExceptionContext.Block)
            {
                throw new BlockException(block, innerException: exception);
            }
        }
    }
}
