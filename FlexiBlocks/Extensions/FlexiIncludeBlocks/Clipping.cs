using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// Represents a clipping from a sequence of lines.
    /// </summary>
    public class Clipping : LineRange
    {
        private const int _defaultDedent = 0;
        private const int _defaultIndent = 0;
        private const float _defaultCollapse = 1;
        private bool _startAndEndStringsNormalized;
        private string _normalizedStartString;
        private string _normalizedEndString;

        /// <summary>
        /// Creates a <see cref="Clipping"/>.
        /// </summary>
        /// <param name="startLine">
        /// <para>The line number of the <see cref="Clipping"/>'s start line.</para>
        /// <para>If this value is <c>-n</c>, the start line is the nth last line. For example, if this value is <c>-2</c>, the start line is the 2nd last line.</para>
        /// <para>This value must not be <c>0</c>.</para>
        /// <para>Defaults to <c>1</c>.</para>
        /// </param>
        /// <param name="endLine">
        /// <para>The line number of the <see cref="Clipping"/>'s end line.</para>
        /// <para>If this value is <c>-n</c>, the end line is the nth last line. For example, if this value is <c>-2</c>, the end line is the 2nd last line.</para>
        /// <para>This value must not be <c>0</c> or an integer representing a line before the start line.</para>
        /// <para>Defaults to <c>-1</c>.</para>
        /// </param>
        /// <param name="region">
        /// <para>The name of the region that the <see cref="Clipping"/> contains.</para>
        /// <para>This value is an alternative to <paramref name="startLine"/> and <paramref name="endLine"/> and takes precedence over them.</para>
        /// <para>It is shorthand for specifying <paramref name="startString"/> and <paramref name="endString"/> - 
        /// if this value is not <c>null</c>, whitespace or an empty string, <paramref name="startString"/> is set to "#region &lt;<paramref name="region"/>&gt;" and <paramref name="endString"/> is
        /// set to "#endregion".</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="startString">
        /// <para>The substring that the line immediately preceding the <see cref="Clipping"/> contains.</para>
        /// <para>This value is an alternative to <paramref name="startLine"/>.</para>
        /// <para>If this value is not <c>null</c>, whitespace or an empty string, it takes precedence over <paramref name="startLine"/> and <paramref name="region"/>.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="endString">
        /// <para>The substring that the line immediately after the <see cref="Clipping"/> contains.</para>
        /// <para>This value is an alternative to <paramref name="endLine"/>.</para>
        /// <para>If this value is not <c>null</c>, whitespace or an empty string, it takes precedence over <paramref name="endLine"/> and <paramref name="region"/>.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="dedent">
        /// <para>The number of leading whitespace characters to remove from each line in the <see cref="Clipping"/>.</para>
        /// <para>This value must not be negative.</para>
        /// <para>Defaults to 0.</para>
        /// </param>
        /// <param name="indent">
        /// <para>The number of leading whitespace characters to add to each line in the <see cref="Clipping"/>.</para>
        /// <para>Due to Markdig limitations, this value may not work properly if the <see cref="FlexiIncludeBlock"/> this <see cref="Clipping"/> belongs to has <see cref="IFlexiIncludeBlockOptions.Type"/>
        /// <see cref="FlexiIncludeType.Markdown"/>.</para>
        /// <para>This value must not be negative.</para>
        /// <para>Defaults to 0.</para>
        /// </param>
        /// <param name="collapse">
        /// <para>The proportion of leading whitespace characters (after dedenting and indenting) to keep.</para>
        /// <para>For example, if there are 9 leading whitespace characters after dedenting and this value is 0.33, the final number of leading whitespace characters will be 3.</para> 
        /// <para>This value must be in the range [0, 1].</para>
        /// <para>Defaults to 1.</para>
        /// </param>
        /// <param name="before">
        /// <para>The content to be prepended to the <see cref="Clipping"/>.</para>
        /// <para>This value is ignored if the <see cref="FlexiIncludeBlock"/> this <see cref="Clipping"/> belongs to has <see cref="IFlexiIncludeBlockOptions.Type"/> <see cref="FlexiIncludeType.Markdown"/>.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="after">
        /// <para>The content to be appended to the <see cref="Clipping"/>.</para>
        /// <para>This value is ignored if the <see cref="FlexiIncludeBlock"/> this <see cref="Clipping"/> belongs to has <see cref="IFlexiIncludeBlockOptions.Type"/> <see cref="FlexiIncludeType.Markdown"/> .</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <exception cref="OptionsException">Thrown if <paramref name="startLine"/> is 0.</exception>
        /// <exception cref="OptionsException">Thrown if <paramref name="endLine"/> is 0.</exception>
        /// <exception cref="OptionsException">Thrown if the end line is a line before the start line.</exception>
        /// <exception cref="OptionsException">Thrown if <paramref name="dedent"/> is negative.</exception>
        /// <exception cref="OptionsException">Thrown if <paramref name="indent"/> is negative.</exception>
        /// <exception cref="OptionsException">Thrown if <paramref name="collapse"/> is not in the range [0, 1].</exception>
        public Clipping(int startLine = _defaultStartLine,
            int endLine = _defaultEndLine,
            string region = default,
            string startString = default,
            string endString = default,
            int dedent = _defaultDedent,
            int indent = _defaultIndent,
            float collapse = _defaultCollapse,
            string before = default,
            string after = default) : base(startLine, endLine)
        {
            if (dedent < 0)
            {
                throw new OptionsException(nameof(Dedent), string.Format(Strings.OptionsException_Shared_ValueMustNotBeNegative, dedent));
            }

            if (indent < 0)
            {
                throw new OptionsException(nameof(Indent), string.Format(Strings.OptionsException_Shared_ValueMustNotBeNegative, indent));
            }

            if (collapse < 0 || collapse > 1)
            {
                throw new OptionsException(nameof(Collapse), string.Format(Strings.OptionsException_Shared_ValueMustBeWithinRange, collapse, "[0, 1]"));
            }

            Region = region;
            StartString = startString;
            EndString = endString;
            Dedent = dedent;
            Indent = indent;
            Collapse = collapse;
            Before = before;
            After = after;
        }

        /// <summary>
        /// Gets the name of the region that the <see cref="Clipping"/> contains.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Region { get; }

        /// <summary>
        /// Gets the substring that the line immediately preceding the <see cref="Clipping"/> contains.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string StartString { get; }

        /// <summary>
        /// Gets the substring that the line immediately after the <see cref="Clipping"/> contains.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string EndString { get; }

        /// <summary>
        /// Gets the number of leading whitespace characters to remove from each line in the <see cref="Clipping"/>.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(_defaultDedent)]
        public int Dedent { get; }

        /// <summary>
        /// Gets the number of leading whitespace characters to add to each line in the <see cref="Clipping"/>.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(_defaultDedent)]
        public int Indent { get; }

        /// <summary>
        /// Gets the proportion of leading whitespace characters (after dedenting) to keep.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(_defaultCollapse)]
        public float Collapse { get; }

        /// <summary>
        /// Gets the content to be prepended to the <see cref="Clipping"/>.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Before { get; }

        /// <summary>
        /// Gets the content to be appended to the <see cref="Clipping"/>.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string After { get; }

        /// <summary>
        /// Gets the <see cref="Clipping"/>'s normalized start and end strings.
        /// </summary>
        public (string normalizedStartString, string normalizedEndString) GetNormalizedStartAndEndStrings()
        {
            if (_startAndEndStringsNormalized)
            {
                return (_normalizedStartString, _normalizedEndString);
            }

            _startAndEndStringsNormalized = true;

            if (string.IsNullOrWhiteSpace(StartString))
            {
                _normalizedStartString = !string.IsNullOrWhiteSpace(Region) ? $"#region {Region}" : null;
            }
            else
            {
                _normalizedStartString = StartString;
            }

            if (string.IsNullOrWhiteSpace(EndString))
            {
                _normalizedEndString = !string.IsNullOrWhiteSpace(Region) ? "#endregion" : null;
            }
            else
            {
                _normalizedEndString = EndString;
            }

            return (_normalizedStartString, _normalizedEndString);
        }

        /// <summary>
        /// Checks for value equality between the <see cref="Clipping"/> and an object.
        /// </summary>
        /// <param name="obj">The object to check for value equality.</param>
        /// <returns>True if the <see cref="Clipping"/>'s value is equal to the object's value, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is Clipping clipping &&
                   base.Equals(obj) &&
                   Region == clipping.Region &&
                   StartString == clipping.StartString &&
                   EndString == clipping.EndString &&
                   Dedent == clipping.Dedent &&
                   Indent == clipping.Indent &&
                   Collapse == clipping.Collapse &&
                   Before == clipping.Before &&
                   After == clipping.After;
        }

        /// <summary>
        /// Returns the hash code for the object.
        /// </summary>
        /// <returns>The hash code for the object.</returns>
        public override int GetHashCode()
        {
            int hashCode = 1912135539;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Region);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StartString);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(EndString);
            hashCode = hashCode * -1521134295 + Dedent.GetHashCode();
            hashCode = hashCode * -1521134295 + Indent.GetHashCode();
            hashCode = hashCode * -1521134295 + Collapse.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Before);
            return hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(After);
        }
    }
}
