using Markdig.Parsers;
using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// An abstraction representing a code block.
    /// </summary>
    public class FlexiCodeBlock : Block
    {
        /// <summary>
        /// Creates a <see cref="FlexiCodeBlock"/>.
        /// </summary>
        /// <param name="blockName">The <see cref="FlexiCodeBlock"/>'s BEM block name.</param>
        /// <param name="title">The <see cref="FlexiCodeBlock"/>'s title.</param>
        /// <param name="copyIcon">The <see cref="FlexiCodeBlock"/>'s copy icon as an HTML fragment.</param>
        /// <param name="renderHeader">The value specifying whether to render the <see cref="FlexiCodeBlock"/>'s header.</param>
        /// <param name="language">The programming langauge of the <see cref="FlexiCodeBlock"/>'s code.</param>
        /// <param name="code">The <see cref="FlexiCodeBlock"/>'s code.</param>
        /// <param name="codeNumLines">The number of lines the <see cref="FlexiCodeBlock"/>'s code spans.</param>
        /// <param name="syntaxHighlighter">The syntax highlighter to highlight the <see cref="FlexiCodeBlock"/>'s code with.</param>
        /// <param name="lineNumbers">The <see cref="NumberedLineRange"/>s specifying line numbers to render.</param>
        /// <param name="omittedLinesIcon">The <see cref="FlexiCodeBlock"/>'s omitted lines icon as an HTML fragment.</param>
        /// <param name="highlightedLines">The <see cref="LineRange"/>s specifying lines to highlight.</param>
        /// <param name="highlightedPhrases">The <see cref="PhraseGroup"/>s specifying phrases to highlight.</param>
        /// <param name="renderingMode">The <see cref="FlexiCodeBlock"/>'s rendering mode.</param>
        /// <param name="attributes">The HTML attributes for the <see cref="FlexiCodeBlock"/>'s root element.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiCodeBlock"/>.</param>
        public FlexiCodeBlock(
            string blockName,
            string title,
            string copyIcon,
            bool renderHeader,
            string language,
            string code,
            int codeNumLines,
            SyntaxHighlighter syntaxHighlighter,
            ReadOnlyCollection<NumberedLineRange> lineNumbers,
            string omittedLinesIcon,
            ReadOnlyCollection<LineRange> highlightedLines,
            ReadOnlyCollection<Phrase> highlightedPhrases,
            FlexiCodeBlockRenderingMode renderingMode,
            ReadOnlyDictionary<string, string> attributes,
            BlockParser blockParser) : base(blockParser)
        {
            BlockName = blockName;
            Title = title;
            CopyIcon = copyIcon;
            RenderHeader = renderHeader;
            Language = language;
            Code = code;
            CodeNumLines = codeNumLines;
            SyntaxHighlighter = syntaxHighlighter;
            LineNumbers = lineNumbers;
            OmittedLinesIcon = omittedLinesIcon;
            HighlightedLines = highlightedLines;
            HighlightedPhrases = highlightedPhrases;
            RenderingMode = renderingMode;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the <see cref="FlexiCodeBlock"/>'s BEM block name.
        /// </summary>
        public string BlockName { get; }

        /// <summary>
        /// Gets the <see cref="FlexiCodeBlock"/>'s title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the <see cref="FlexiCodeBlock"/>'s copy icon as an HTML fragment.
        /// </summary>
        public string CopyIcon { get; }

        /// <summary>
        /// Gets the value specifying whether to render the <see cref="FlexiCodeBlock"/>'s header.
        /// </summary>
        public bool RenderHeader { get; }

        /// <summary>
        /// Gets the programming langauge of the <see cref="FlexiCodeBlock"/>'s code.
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// Gets the syntax highlighter to highlight the <see cref="FlexiCodeBlock"/>'s code with.
        /// </summary>
        public SyntaxHighlighter SyntaxHighlighter { get; }

        /// <summary>
        /// Gets the <see cref="NumberedLineRange"/>s specifying line numbers to render.
        /// </summary>
        public ReadOnlyCollection<NumberedLineRange> LineNumbers { get; }

        /// <summary>
        /// Gets the <see cref="FlexiCodeBlock"/>'s omitted lines icon as an HTML fragment.
        /// </summary>
        public string OmittedLinesIcon { get; }

        /// <summary>
        /// Gets the <see cref="LineRange"/>s specifying lines to highlight.
        /// </summary>
        public ReadOnlyCollection<LineRange> HighlightedLines { get; }

        /// <summary>
        /// Gets the <see cref="Phrase"/>s to highlight.
        /// </summary>
        public ReadOnlyCollection<Phrase> HighlightedPhrases { get; }

        /// <summary>
        /// Gets the HTML attributes for the <see cref="FlexiCodeBlock"/>'s root element.
        /// </summary>
        public ReadOnlyDictionary<string, string> Attributes { get; }

        /// <summary>
        /// Gets the <see cref="FlexiCodeBlock"/>'s rendering mode.
        /// </summary>
        public FlexiCodeBlockRenderingMode RenderingMode { get; }

        /// <summary>
        /// Gets the <see cref="FlexiCodeBlock"/>'s code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the number of lines the <see cref="FlexiCodeBlock"/>'s code spans.
        /// </summary>
        public int CodeNumLines { get; }
    }
}
