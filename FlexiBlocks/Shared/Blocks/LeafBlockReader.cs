// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using Markdig.Helpers;
using Markdig.Syntax;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// A <see cref="TextReader"/> that reads from a <see cref="LeafBlock"/>.
    /// </summary>
    public class LeafBlockReader : TextReader
    {
        private bool _disposed;
        private StringLineGroup.Iterator _iterator;

        /// <summary>
        /// Creates a <see cref="LeafBlockReader"/>.
        /// </summary>
        /// <param name="leafBlock"></param>
        public LeafBlockReader(LeafBlock leafBlock)
        {
            _iterator = leafBlock.Lines.ToCharIterator();
        }

        /// <summary>
        /// Closes the <see cref="LeafBlockReader"/>.
        /// </summary>
        public override void Close()
        {
            _disposed = true;
            Dispose(true);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="LeafBlockReader"/> and optionally releases the managed resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Returns the next available character but does not consume it.
        /// </summary>
        public override int Peek()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null, Strings.ObjectDisposedException_Shared_ReaderClosed);
            }

            char c = _iterator.CurrentChar;

            return c == '\0' ? -1 : c;
        }

        /// <summary>
        /// Reads the next character from the input string.
        /// </summary>
        public override int Read()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null, Strings.ObjectDisposedException_Shared_ReaderClosed);
            }

            char c = _iterator.CurrentChar;
            _iterator.NextChar();

            return c == '\0' ? -1 : c;
        }

        /// <summary>
        /// Reads the next set of characters from the input string.
        /// </summary>
        public override int Read(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer), Strings.ArgumentNullException_Shared_Buffer);
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueCannotBeNegative, index));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueCannotBeNegative, index));
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException(Strings.ArgumentException_Shared_InvalidOffLen);
            }
            if (_disposed)
            {
                throw new ObjectDisposedException(null, Strings.ObjectDisposedException_Shared_ReaderClosed);
            }

            // TODO could be much faster without allocating - iterate over slices and call copyTo for each one
            // TODO does all this extra logic justify avoiding the single string allocation for a standard StringReader? Note that 
            // the single string allocation will require similar logic to construct the string from a StringLineGroup - profile
            count = Math.Min(count, _iterator.End - _iterator.Start + 1);
            buffer[index] = _iterator.CurrentChar;
            int end = index + count;
            for (int i = index + 1; i < end; i++)
            {
                buffer[i] = _iterator.NextChar();
            }
            _iterator.NextChar();

            return count;
        }

        /// <summary>
        /// Reads all characters from the current position to the end of the string and returns them as a single string.
        /// </summary>
        public override string ReadToEnd()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null, Strings.ObjectDisposedException_Shared_ReaderClosed);
            }

            // TODO does not increment _iterator.Start
            return _iterator.CurrentChar == '\0' ? null : _iterator.Remaining().ToString();
        }

        /// <summary>
        /// Reads a line of characters from the current string and returns the data as a string.
        /// </summary>
        public override string ReadLine()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null, Strings.ObjectDisposedException_Shared_ReaderClosed);
            }

            // TODO does not increment _iterator.Start
            return _iterator.CurrentChar == '\0' ? null : _iterator.Remaining().Lines[0].ToString();
        }

        #region Task based Async APIs
        /// <summary>
        /// Reads a line of characters asynchronously from the current string and returns the data as a string.
        /// </summary>
        public override Task<string> ReadLineAsync()
        {
            return Task.FromResult(ReadLine());
        }

        /// <summary>
        /// Reads all characters from the current position to the end of the string asynchronously and returns them as a single string.
        /// </summary>
        public override Task<string> ReadToEndAsync()
        {
            return Task.FromResult(ReadToEnd());
        }

        /// <summary>
        /// Reads a specified maximum number of characters from the current string asynchronously and writes the data to a buffer, beginning at the specified index.
        /// </summary>
        public override Task<int> ReadBlockAsync(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer), Strings.ArgumentNullException_Shared_Buffer);
            }
            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException(index < 0 ? nameof(index) : nameof(count), string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueCannotBeNegative, index));
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException(Strings.ArgumentException_Shared_InvalidOffLen);
            }

            return Task.FromResult(Read(buffer, index, count));
        }

        /// <summary>
        /// Reads a specified maximum number of characters from the current string asynchronously and writes the data to a buffer, beginning at the specified index.
        /// </summary>
        public override Task<int> ReadAsync(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer), Strings.ArgumentNullException_Shared_Buffer);
            }
            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException(index < 0 ? nameof(index) : nameof(count), string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueCannotBeNegative, index));
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException(Strings.ArgumentException_Shared_InvalidOffLen);
            }

            return Task.FromResult(Read(buffer, index, count));
        }
        #endregion
    }
}
