using Newtonsoft.Json;
using System.ComponentModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// Represents a range of lines with an associated sequence of numbers.
    /// </summary>
    public class NumberedLineRange : LineRange
    {
        private const int _defaultStartNumber = 1;

        /// <summary>
        /// Creates a <see cref="NumberedLineRange"/>.
        /// </summary>
        /// <param name="startLine">
        /// <para>The line number of the <see cref="NumberedLineRange"/>'s start line.</para>
        /// <para>If this value is <c>-n</c>, the start line is the nth last line. For example, if this value is <c>-2</c>, the start line is the 2nd last line.</para>
        /// <para>This value must not be <c>0</c>.</para>
        /// <para>Defaults to <c>1</c>.</para>
        /// </param>
        /// <param name="endLine">
        /// <para>The line number of the <see cref="NumberedLineRange"/>'s end line.</para>
        /// <para>If this value is <c>-n</c>, the end line is the nth last line. For example, if this value is <c>-2</c>, the end line is the 2nd last line.</para>
        /// <para>This value must not be <c>0</c> or an integer representing a line before the start line.</para>
        /// <para>Defaults to <c>-1</c>.</para>
        /// </param>
        /// <param name="startNumber">
        /// <para>The number associated with this <see cref="NumberedLineRange"/>'s start line.</para>
        /// <para>The number associated with each subsequent line is incremented by 1.</para>
        /// <para>This value must be greater than 0.</para>
        /// <para>Defaults to <c>1</c>.</para>
        /// </param>
        /// <exception cref="OptionsException">Thrown if <paramref name="startLine"/> is 0.</exception>
        /// <exception cref="OptionsException">Thrown if <paramref name="endLine"/> is 0.</exception>
        /// <exception cref="OptionsException">Thrown if the end line is a line before the start line.</exception>
        /// <exception cref="OptionsException">Thrown if <paramref name="startNumber"/> is less than 1.</exception>
        public NumberedLineRange(
            int startLine = _defaultStartLine,
            int endLine = _defaultEndLine,
            int startNumber = _defaultStartNumber) : base(startLine, endLine)
        {
            if (startNumber < 1)
            {
                throw new OptionsException(nameof(StartNumber), string.Format(Strings.OptionsException_Shared_ValueMustBeIntegerGreaterThan0, startNumber));
            }

            StartNumber = startNumber;
        }

        /// <summary>
        /// Gets the number associated with the <see cref="NumberedLineRange"/>'s start line.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(_defaultStartNumber)]
        public int StartNumber { get; private set; }

        /// <summary>
        /// Returns the string representation of the instance.
        /// </summary>
        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(StartNumber)}: {StartNumber}";
        }

        /// <summary>
        /// Checks for value equality between the <see cref="NumberedLineRange"/> and an object.
        /// </summary>
        /// <param name="obj">The object to check for value equality.</param>
        /// <returns>True if the <see cref="NumberedLineRange"/>'s value is equal to the object's value, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is NumberedLineRange range &&
                   base.Equals(obj) &&
                   StartNumber == range.StartNumber;
        }

        /// <summary>
        /// Returns the hash code for the object.
        /// </summary>
        /// <returns>The hash code for the object.</returns>
        public override int GetHashCode()
        {
            int hashCode = 1733802996;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            return hashCode * -1521134295 + StartNumber.GetHashCode();
        }
    }
}
