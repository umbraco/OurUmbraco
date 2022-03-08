using Markdig.Helpers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// Represents a table cell.
    /// </summary>
    public class Cell
    {
        // Field since it's a struct. If it's a property, the getter will return a clone and changes to the clone will not be reflected in the original (pass by value for value types).
        /// <summary>
        /// The <see cref="Cell"/>'s content.
        /// </summary>
        public StringLineGroup Lines;

        /// <summary>
        /// Creates a <see cref="Cell"/>.
        /// </summary>
        /// <param name="startColumnIndex">The index of the <see cref="Cell"/>'s start column.</param>
        /// <param name="endColumnIndex">The index of the <see cref="Cell"/>'s end column.</param>
        /// <param name="startRowIndex">The index of the <see cref="Cell"/>'s start row.</param>
        /// <param name="endRowIndex">The index of the <see cref="Cell"/>'s end row.</param>
        /// <param name="startOffset">The offset of the start of the <see cref="Cell"/> from the 
        /// leftmost character of its containing table.</param>
        /// <param name="endOffset">The offset of the end of the <see cref="Cell"/> from the 
        /// leftmost character of its containing table.</param>
        /// <param name="lineIndex">The index of the line within the <see cref="Cell"/>'s containing markdown document
        /// that the <see cref="Cell"/> begins at.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="startColumnIndex"/> is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="endColumnIndex"/> is less than <paramref name="startColumnIndex"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="startRowIndex"/> is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="endRowIndex"/> is less than <paramref name="startRowIndex"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="startOffset"/> is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="endOffset"/> is not greater than <paramref name="startOffset"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="lineIndex"/> is negative.</exception>
        public Cell(int startColumnIndex,
            int endColumnIndex,
            int startRowIndex,
            int endRowIndex,
            int startOffset,
            int endOffset,
            int lineIndex)
        {
            if (startColumnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueCannotBeNegative, startColumnIndex),
                        nameof(startColumnIndex));
            }

            if (endColumnIndex < startColumnIndex)
            {
                throw new ArgumentOutOfRangeException(string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueMustNotBeLessThanOtherValue,
                        endColumnIndex,
                        nameof(startColumnIndex),
                        startColumnIndex),
                    nameof(endColumnIndex));
            }

            if (startRowIndex < 0)
            {
                throw new ArgumentOutOfRangeException(string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueCannotBeNegative, startRowIndex),
                        nameof(startRowIndex));
            }

            if (endRowIndex < startRowIndex)
            {
                throw new ArgumentOutOfRangeException(string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueMustNotBeLessThanOtherValue,
                        endRowIndex,
                        nameof(startRowIndex),
                        startRowIndex),
                    nameof(endRowIndex));
            }

            if (lineIndex < 0)
            {
                throw new ArgumentOutOfRangeException(string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueCannotBeNegative, lineIndex),
                    nameof(lineIndex));
            }

            if (startOffset < 0)
            {
                throw new ArgumentOutOfRangeException(string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueCannotBeNegative, startOffset),
                    nameof(startOffset));
            }

            if (endOffset <= startOffset)
            {
                throw new ArgumentOutOfRangeException(string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueMustBeGreaterThanOtherValue,
                    endOffset,
                    nameof(startOffset),
                    startOffset),
                nameof(endOffset));
            }

            StartColumnIndex = startColumnIndex;
            EndColumnIndex = endColumnIndex;
            StartRowIndex = startRowIndex;
            EndRowIndex = endRowIndex;
            StartOffset = startOffset;
            EndOffset = endOffset;
            LineIndex = lineIndex;
            Lines = new StringLineGroup(4);
            IsOpen = true;
        }

        /// <summary>
        /// Gets the index of the <see cref="Cell"/>'s start column.
        /// </summary>
        public int StartColumnIndex { get; }

        /// <summary>
        /// Gets the index of the <see cref="Cell"/>'s end column.
        /// </summary>
        public int EndColumnIndex { get; }

        /// <summary>
        /// Gets the index of the <see cref="Cell"/>'s start row.
        /// </summary>
        public int StartRowIndex { get; }

        /// <summary>
        /// Gets or sets the index of the <see cref="Cell"/>'s end row.
        /// </summary>
        public int EndRowIndex { get; set; }

        /// <summary>
        /// Gets the offset of start of the <see cref="Cell"/>'s content from the 
        /// leftmost character of its containing table.
        /// </summary>
        public int StartOffset { get; }

        /// <summary>
        /// Gets the offset of end of the <see cref="Cell"/>'s content from the 
        /// leftmost character of its containing table.
        /// </summary>
        public int EndOffset { get; }

        /// <summary>
        /// Gets the index of the line within the <see cref="Cell"/>'s containing markdown document
        /// that the <see cref="Cell"/> begins at.
        /// </summary>
        public int LineIndex { get; }

        /// <summary>
        /// Gets or sets the value indicating whether the <see cref="Cell"/> is open.
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Checks for value equality between the <see cref="Cell"/> and an object.
        /// </summary>
        /// <param name="obj">The object to check for value equality.</param>
        /// <returns>True if the <see cref="Cell"/>'s value is equal to the object's value, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is Cell cell &&
                   Lines.ToString() == cell.Lines.ToString() && // Not efficient, but StringLineGroup does not override Object.Equals
                   StartColumnIndex == cell.StartColumnIndex &&
                   EndColumnIndex == cell.EndColumnIndex &&
                   StartRowIndex == cell.StartRowIndex &&
                   EndRowIndex == cell.EndRowIndex &&
                   StartOffset == cell.StartOffset &&
                   EndOffset == cell.EndOffset &&
                   LineIndex == cell.LineIndex &&
                   IsOpen == cell.IsOpen;
        }

        /// <summary>
        /// WARNING: always returns 0. This type is mutable - it is not suitable for use as the key type in a dictionary and
        /// instances of this type should not be stored in a hashset.
        /// </summary>
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
