using Markdig.Syntax;
using System;
using System.Runtime.Serialization;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Represents an unrecoverable situation encountered while parsing an <see cref="IBlock"/>.
    /// </summary>
    [Serializable]
    public class BlockException : Exception
    {
        /// <summary>
        /// Gets the context of the unrecoverable situation.
        /// </summary>
        public BlockExceptionContext Context { get; }

        /// <summary>
        /// Gets the unrecoverable situation's description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the offending markdown's line number.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the column the offending markdown starts at.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the offending <see cref="IBlock"/>'s type name.
        /// </summary>
        public string BlockTypeName { get; }

        /// <summary>
        /// Creates a <see cref="BlockException"/>.
        /// </summary>
        public BlockException()
        {
        }

        /// <summary>
        /// Creates a <see cref="BlockException"/>.
        /// </summary>
        /// <param name="message">This exception's message.</param>
        public BlockException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a <see cref="BlockException"/>.
        /// </summary>
        /// <param name="message">This exception's message.</param>
        /// <param name="innerException">This exception's inner exception.</param>
        public BlockException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a <see cref="BlockException"/>.
        /// </summary>
        /// <param name="lineNumber">The offending markdown's line number.</param>
        /// <param name="column">The column the offending markdown starts at.</param>
        /// <param name="description">
        /// <para>The unrecoverable situation's description.</para>
        /// <para>If this value is <c>null</c>, this exception's description is <see cref="Strings.BlockException_BlockException_ExceptionOccurredWhileProcessingBlock"/>.</para>
        /// </param>
        /// <param name="innerException">This exception's inner exception.</param>
        public BlockException(int lineNumber, int column, string description = null, Exception innerException = null) : base(null, innerException)
        {
            LineNumber = lineNumber;
            Column = column;
            Description = description;

            Context = BlockExceptionContext.Line;
        }

        /// <summary>
        /// Creates a <see cref="BlockException"/>.
        /// </summary>
        /// <param name="offendingBlock">The offending <see cref="IBlock"/>.</param>
        /// <param name="description">
        /// <para>The unrecoverable situation's description.</para>
        /// <para>If this value is <c>null</c>, this exception's description is <see cref="Strings.BlockException_BlockException_ExceptionOccurredWhileProcessingBlock"/>.</para>
        /// </param>
        /// <param name="innerException">This exception's inner exception.</param>
        /// <param name="lineNumber">
        /// <para>The offending markdown's line number.</para>
        /// <para>If this value is <c>null</c>, the offending markdown's line number is inferred from <paramref name="offendingBlock"/>.</para>
        /// </param>
        /// <param name="column">
        /// <para>The column the offending markdown starts at.</para>
        /// <para>If this value is <c>null</c>, the offending markdown's column is inferred from <paramref name="offendingBlock"/>.</para>
        /// </param>
        public BlockException(IBlock offendingBlock, string description = null, Exception innerException = null, int? lineNumber = null, int? column = null) : base(description, innerException)
        {
            if (offendingBlock != null)
            {
                Description = description;
                LineNumber = lineNumber ?? offendingBlock.Line + 1;
                Column = column ?? offendingBlock.Column;
                BlockTypeName = offendingBlock is IProxyBlock proxyBlock ? proxyBlock.MainTypeName : offendingBlock.GetType().Name;

                Context = BlockExceptionContext.Block;
            }
        }

        /// <summary>
        /// Creates a <see cref="BlockException"/>.
        /// </summary>
        /// <param name="offendingBlockTypeName">The name of the type of the offending <see cref="IBlock"/>.</param>
        /// <param name="lineNumber">The offending markdown's line number.</param>
        /// <param name="column">The column the offending markdown starts at.</param>
        /// <param name="description">
        /// <para>The unrecoverable situation's description.</para>
        /// <para>If this value is <c>null</c>, this exception's description is <see cref="Strings.BlockException_BlockException_ExceptionOccurredWhileProcessingBlock"/>.</para>
        /// </param>
        /// <param name="innerException">This exception's inner exception.</param>
        public BlockException(string offendingBlockTypeName, int lineNumber, int column, string description = null, Exception innerException = null) : base(description, innerException)
        {
            Description = description;
            LineNumber = lineNumber;
            Column = column;
            BlockTypeName = offendingBlockTypeName;

            Context = BlockExceptionContext.Block;
        }

        /// <summary>
        /// Creates a <see cref="BlockException"/>.
        /// </summary>
        /// <param name="info">The data store for serialization/deserialization.</param>
        /// <param name="context">The struct representing the source and destination of a serialized stream.</param>
        protected BlockException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Context = (BlockExceptionContext)info.GetInt32(nameof(Context));
            Description = info.GetString(nameof(Description));
            LineNumber = info.GetInt32(nameof(LineNumber));
            Column = info.GetInt32(nameof(LineNumber));
            BlockTypeName = info.GetString(nameof(BlockTypeName));
        }

        /// <summary>
        /// Gets object data for binary serialization.
        /// </summary>
        /// <param name="info">The data store for serialization/deserialization.</param>
        /// <param name="context">The struct representing the source and destination of a serialized stream.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Context), Context, typeof(int));
            info.AddValue(nameof(Description), Description, typeof(string));
            info.AddValue(nameof(LineNumber), LineNumber, typeof(int));
            info.AddValue(nameof(Column), Column, typeof(int));
            info.AddValue(nameof(BlockTypeName), BlockTypeName, typeof(string));
        }

        /// <summary>
        /// Gets a message containing everything known about the unrecoverable situation.
        /// </summary>
        public override string Message
        {
            get
            {
                if (Context == BlockExceptionContext.None)
                {
                    return base.Message;
                }

                return string.Format(Strings.BlockException_BlockException_InvalidBlock,
                    Context == BlockExceptionContext.Block && !string.IsNullOrWhiteSpace(BlockTypeName) ? BlockTypeName : "block of unknown type",
                    LineNumber,
                    Column,
                    Description ?? Strings.BlockException_BlockException_ExceptionOccurredWhileProcessingBlock);
            }
        }
    }
}
