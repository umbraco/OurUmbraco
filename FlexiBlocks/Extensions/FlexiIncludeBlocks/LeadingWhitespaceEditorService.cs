using Markdig.Helpers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// The default implementation of <see cref="ILeadingWhitespaceEditorService"/>.
    /// </summary>
    public class LeadingWhitespaceEditorService : ILeadingWhitespaceEditorService
    {
        /// <inheritdoc />
        public virtual StringSlice Indent(StringSlice line, int indentLength)
        {
            if (indentLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indentLength), string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueCannotBeNegative, indentLength));
            }

            if (indentLength == 0)
            {
                return line;
            }

            // TODO difficult to avoid these allocations without changes to Markdig. While this implementation isn't efficient, it is very useful.
            return new StringSlice(new string(' ', indentLength) + line.ToString());
        }

        /// <inheritdoc />
        public virtual void Dedent(ref StringSlice line, int dedentLength)
        {
            if (dedentLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dedentLength), string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueCannotBeNegative, dedentLength));
            }

            if (line.IsEmpty || dedentLength == 0)
            {
                return;
            }

            for (int offset = 0; offset < dedentLength; offset++)
            {
                if (!line.PeekChar(offset).IsWhitespace())
                {
                    line.Start += offset;
                    return; // No more whitespace to dedent or collapse
                }
            }

            line.Start += dedentLength;
        }

        /// <inheritdoc />
        public virtual void Collapse(ref StringSlice line, float collapseRatio)
        {
            if (collapseRatio < 0 || collapseRatio > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(collapseRatio),
                    string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueMustBeWithinRange, "[0, 1]", collapseRatio));
            }

            if (line.IsEmpty || collapseRatio == 1)
            {
                return;
            }

            if (collapseRatio == 0)
            {
                line.TrimStart(); // Remove all leading whitespace
            }
            else
            {
                int leadingWhitespaceCount = 0;
                while (line.PeekChar(leadingWhitespaceCount).IsWhitespace())
                {
                    leadingWhitespaceCount++;
                }

                if (leadingWhitespaceCount == 0)
                {
                    return;
                }

                // collapseRatio is defined as finalLeadingWhitespaceCount/initialLeadingWhitespaceCount,
                // so collapseLength = initialLeadingWhitespaceCount - finalLeadingWhitespaceCount = initialLeadingWhitespaceCount - initialLeadingWhitespaceCount*collapseRatio
                int collapseLength = leadingWhitespaceCount - (int)Math.Round(leadingWhitespaceCount * collapseRatio);

                for (int start = 0; start < collapseLength; start++)
                {
                    line.NextChar();
                }
            }
        }
    }
}
