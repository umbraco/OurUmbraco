using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// Represents a column definition.
    /// </summary>
    public class ColumnDefinition
    {
        /// <summary>
        /// Creates a <see cref="ColumnDefinition"/>.
        /// </summary>
        /// <param name="contentAlignment">The content-alignment of cells in the column.</param>
        /// <param name="startOffset">The offset of the start of cells in the column from the 
        /// leftmost character of the containing table.</param>
        /// <param name="endOffset">The offset of the end of cells in the column from the 
        /// leftmost character of the containing table.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="startOffset"/> is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="endOffset"/> is not greater than <paramref name="startOffset"/>.</exception>
        public ColumnDefinition(ContentAlignment contentAlignment, int startOffset, int endOffset)
        {
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

            ContentAlignment = contentAlignment;
            StartOffset = startOffset;
            EndOffset = endOffset;
        }

        /// <summary>
        /// Gets the content-alignment of cells in the column.
        /// </summary>
        public ContentAlignment ContentAlignment { get; }

        /// <summary>
        /// Gets the offset of the start of cells in the column from the 
        /// leftmost character of the containing table.
        /// </summary>
        public int StartOffset { get; }

        /// <summary>
        /// Gets the offset of the end of cells in the column from the 
        /// leftmost character of the containing table.
        /// </summary>
        public int EndOffset { get; }

        /// <summary>
        /// Checks for value equality between the <see cref="ColumnDefinition"/> and an object.
        /// </summary>
        /// <param name="obj">The object to check for value equality.</param>
        /// <returns>True if the <see cref="ColumnDefinition"/>'s value is equal to the object's value, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is ColumnDefinition definition &&
                   ContentAlignment == definition.ContentAlignment &&
                   StartOffset == definition.StartOffset &&
                   EndOffset == definition.EndOffset;
        }

        /// <summary>
        /// Returns the hash code for the object.
        /// </summary>
        /// <returns>The hash code for the object.</returns>
        public override int GetHashCode()
        {
            int hashCode = -628491361;
            hashCode = hashCode * -1521134295 + ContentAlignment.GetHashCode();
            hashCode = hashCode * -1521134295 + StartOffset.GetHashCode();
            return hashCode * -1521134295 + EndOffset.GetHashCode();
        }
    }
}
