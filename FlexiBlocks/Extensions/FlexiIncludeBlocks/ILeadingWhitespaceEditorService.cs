using Markdig.Helpers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// An abstraction for editing leading whitespace of <see cref="StringSlice"/>s.
    /// </summary>
    public interface ILeadingWhitespaceEditorService
    {
        /// <summary>
        /// Creates a new <see cref="StringSlice"/> with added leading spaces.
        /// </summary>
        /// <param name="line">The <see cref="StringSlice"/> to add leading spaces to.</param>
        /// <param name="indentLength">
        /// <para>The number of leading spaces to add.</para>
        /// <para>This value cannot be negative.</para>
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="indentLength"/> is negative.</exception>
        StringSlice Indent(StringSlice line, int indentLength);

        /// <summary>
        /// Removes whitespace characters from the beginning of a <see cref="StringSlice"/>.
        /// </summary>
        /// <param name="line">The <see cref="StringSlice"/> to dedent.</param>
        /// <param name="dedentLength">
        /// <para>The number of whitespace characters to remove.</para>
        /// <para>This value cannot be negative.</para>
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="dedentLength"/> is negative.</exception>
        void Dedent(ref StringSlice line, int dedentLength);

        /// <summary>
        /// Collapses whitespace at the beginning of a <see cref="StringSlice"/>.
        /// </summary>
        /// <param name="line">The <see cref="StringSlice"/> whose whitespace will be collapsed.</param>
        /// <param name="collapseRatio">
        /// <para>The ratio of the final leading whitespace character count to the initial leading whitespace character count.</para>
        /// <para>This value must be within the range [0, 1].</para>
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="collapseRatio"/> is not within the range [0, 1].</exception>
        void Collapse(ref StringSlice line, float collapseRatio);
    }
}
