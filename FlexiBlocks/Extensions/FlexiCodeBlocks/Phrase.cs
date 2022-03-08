using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// Represents a sequence of characters.
    /// </summary>
    public class Phrase : IComparable<Phrase>
    {
        /// <summary>
        /// Creates a <see cref="Phrase"/>.
        /// </summary>
        /// <param name="start">The index of the <see cref="Phrase"/>'s first character.</param>
        /// <param name="end">The index of the <see cref="Phrase"/>'s last character.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="start"/> is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="end"/> is less than <paramref name="start"/>.</exception>
        public Phrase(int start, int end)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueCannotBeNegative, start), nameof(start));
            }

            if (end < start)
            {
                throw new ArgumentOutOfRangeException(string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueMustNotBeLessThanOtherValue,
                        end,
                        nameof(start),
                        start),
                    nameof(end));
            }

            Start = start;
            End = end;
        }

        /// <summary>
        /// Gets the index of the <see cref="Phrase"/>'s first character.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the index of the <see cref="Phrase"/>'s last character.
        /// </summary>
        public int End { get; }

        /// <summary>
        /// Compares the <see cref="Phrase"/> and another <see cref="Phrase"/>.
        /// </summary>
        /// <param name="other">The <see cref="Phrase"/> to compare with.</param>
        /// <returns>
        /// -1 if the <see cref="Phrase"/> occurs before the other <see cref="Phrase"/>.
        /// 0 if the <see cref="Phrase"/>s represent the same sequence.
        /// 1 if the <see cref="Phrase"/> occurs after the other <see cref="Phrase"/>.
        /// </returns>
        public int CompareTo(Phrase other)
        {
            if (Start < other.Start)
            {
                return -1;
            }

            if (Start > other.Start)
            {
                return 1;
            }

            // For phrases that start at the same point, we want longer phrases before
            // shorter ones for easier highlighting
            if (End < other.End)
            {
                return 1;
            }

            if (End > other.End)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Checks for value equality between the <see cref="Phrase"/> and an object.
        /// </summary>
        /// <param name="obj">The object to check for value equality.</param>
        /// <returns>True if the <see cref="Phrase"/>'s value is equal to the object's value, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is Phrase phrase &&
                   Start == phrase.Start &&
                   End == phrase.End;
        }

        /// <summary>
        /// Returns the hash code for the object.
        /// </summary>
        /// <returns>The hash code for the object.</returns>
        public override int GetHashCode()
        {
            int hashCode = 866674315;
            hashCode = hashCode * -1521134295 + Start.GetHashCode();
            return hashCode * -1521134295 + End.GetHashCode();
        }
    }
}
