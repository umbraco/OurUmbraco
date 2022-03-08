using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiCodeBlockOptions"/>.</para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON in <see cref="FlexiOptionsBlock"/>s. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiCodeBlockOptions : RenderedRootBlockOptions<IFlexiCodeBlockOptions>, IFlexiCodeBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiCodeBlockOptions"/>.
        /// </summary>
        /// <param name="blockName">
        /// <para>The <see cref="FlexiCodeBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.</para>
        /// <para>In compliance with <a href="https://en.bem.info">BEM methodology</a>, this value is the <see cref="FlexiCodeBlock"/>'s root element's class as well as the prefix for all other classes in the block.</para>
        /// <para>This value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiCodeBlock"/>'s block name is "flexi-code".</para>
        /// <para>Defaults to "flexi-code".</para>
        /// </param>
        /// <param name="title">
        /// <para>The <see cref="FlexiCodeBlock"/>'s title.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no title is rendered.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>    
        /// <param name="copyIcon">
        /// <para>The <see cref="FlexiCodeBlock"/>'s copy button icon as an HTML fragment.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__copy-icon" is assigned to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no copy icon is rendered.</para>
        /// <para>Defaults to a copy file icon.</para>
        /// </param>
        /// <param name="renderHeader">
        /// <para>The value specifying whether to render the <see cref="FlexiCodeBlock"/>'s header.</para>
        /// <para>If <c>false</c>, the header element, which contains the <see cref="FlexiCodeBlock"/>'s title and copy button, is not rendered.</para>
        /// <para>Defaults to <c>true</c>.</para>
        /// </param>
        /// <param name="language">
        /// <para>The programming language of the <see cref="FlexiCodeBlock"/>'s code.</para>
        /// <para>If <paramref name="syntaxHighlighter"/> is not <see cref="SyntaxHighlighter.None"/>, this value is passed to the chosen syntax highlighter.</para>
        /// <para>Therefore, this value must be a language alias supported by the chosen syntax highlighter.</para>
        /// <para><a href="https://prismjs.com/index.html#languages-list">Valid language aliases for Prism</a>.</para>
        /// <para><a href="https://github.com/highlightjs/highlight.js/tree/master/src/languages">Valid language aliases for HighlightJS</a>.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__code_language_&lt;language&gt;" is assigned to the <see cref="FlexiCodeBlock"/>'s root element.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, syntax highlighting is disabled and no language class is assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="syntaxHighlighter">
        /// <para>The syntax highlighter to highlight the <see cref="FlexiCodeBlock"/>'s code with.</para>
        /// <para>If this value is <see cref="SyntaxHighlighter.None"/>, or <paramref name="language"/> is <c>null</c>, whitespace or an empty string,
        /// syntax highlighting is disabled.</para>
        /// <para>Syntax highlighting requires <a href="https://nodejs.org/en/">Node.js</a> to be installed and on the path environment variable.</para>
        /// <para>Defaults to <see cref="SyntaxHighlighter.Prism"/>.</para>
        /// </param>
        /// <param name="lineNumbers">
        /// <para>The <see cref="NumberedLineRange"/>s specifying line numbers to render.</para>
        /// <para>If line numbers are specified for some but not all lines, an omitted lines icon is rendered for each line with no line number. You can customize
        /// the icon by specifying <paramref name="omittedLinesIcon"/>.</para>
        /// <para>If line numbers are specified for some but not all lines, an omitted lines notice is inserted into each empty line with no line number. 
        /// The notice "line {0} omitted for brevity" is inserted if a single line is omitted and the notice "lines {0} to {1} omitted for brevity", 
        /// is inserted if multiple lines are omitted.</para>
        /// <para>Contained ranges must not overlap.</para>
        /// <para>If this value is <c>null</c>, no line numbers are rendered.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="omittedLinesIcon">
        /// <para>The <see cref="FlexiCodeBlock"/>'s omitted lines icon as an HTML fragment.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__omitted-lines-icon" is assigned to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no omitted lines icons are rendered.</para>
        /// <para>Defaults to a vertical ellipsis icon.</para>
        /// </param>
        /// <param name="highlightedLines">
        /// <para>The <see cref="LineRange"/>s specifying lines to highlight.</para>
        /// <para>If this value is <c>null</c>, no lines are highlighted.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="highlightedPhrases">
        /// <para>The <see cref="PhraseGroup"/>s specifying phrases to highlight.</para>
        /// <para>If the regex expression of a <see cref="PhraseGroup"/> has groups, only groups are highlighted, entire matches are not highlighted.</para>
        /// <para>If the regex expression of a <see cref="PhraseGroup"/> has no groups, entire matches are highlighted.</para>
        /// <para>If this value is <c>null</c>, no phrases are highlighted.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="renderingMode">
        /// <para>The <see cref="FlexiCodeBlock"/>'s rendering mode.</para>
        /// <para>Defaults to <see cref="FlexiCodeBlockRenderingMode.Standard"/>.</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiCodeBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If classes are specified, they are appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiCodeBlockOptions(
            string blockName = "flexi-code",
            string title = default,
            string copyIcon = CustomIcons.CUSTOM_COPY,
            bool renderHeader = true,
            string language = default,
            SyntaxHighlighter syntaxHighlighter = SyntaxHighlighter.Prism,
            IList<NumberedLineRange> lineNumbers = default,
            string omittedLinesIcon = CustomIcons.CUSTOM_MORE_VERT,
            IList<LineRange> highlightedLines = default,
            IList<PhraseGroup> highlightedPhrases = default,
            FlexiCodeBlockRenderingMode renderingMode = FlexiCodeBlockRenderingMode.Standard,
            IDictionary<string, string> attributes = default) : base(blockName, attributes)
        {
            Title = title;
            CopyIcon = copyIcon;
            RenderHeader = renderHeader;
            Language = language;
            SyntaxHighlighter = syntaxHighlighter;
            LineNumbers = lineNumbers == null ? null :
                lineNumbers is ReadOnlyCollection<NumberedLineRange> lineNumbersAsReadOnlyDictionary ? lineNumbersAsReadOnlyDictionary :
                new ReadOnlyCollection<NumberedLineRange>(lineNumbers);
            OmittedLinesIcon = omittedLinesIcon;
            HighlightedLines = highlightedLines == null ? null :
                highlightedLines is ReadOnlyCollection<LineRange> highlightedLinesAsReadOnlyDictionary ? highlightedLinesAsReadOnlyDictionary :
                new ReadOnlyCollection<LineRange>(highlightedLines);
            HighlightedPhrases = highlightedPhrases == null ? null :
                highlightedPhrases is ReadOnlyCollection<PhraseGroup> highlightedPhrasesAsReadOnlyDictionary ? highlightedPhrasesAsReadOnlyDictionary :
                new ReadOnlyCollection<PhraseGroup>(highlightedPhrases);
            RenderingMode = renderingMode;
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual string Title { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual string CopyIcon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual bool RenderHeader { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual string Language { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual SyntaxHighlighter SyntaxHighlighter { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual ReadOnlyCollection<NumberedLineRange> LineNumbers { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual string OmittedLinesIcon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual ReadOnlyCollection<LineRange> HighlightedLines { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual ReadOnlyCollection<PhraseGroup> HighlightedPhrases { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual FlexiCodeBlockRenderingMode RenderingMode { get; private set; }
    }
}
