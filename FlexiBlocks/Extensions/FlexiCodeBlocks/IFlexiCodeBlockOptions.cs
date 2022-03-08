using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiCodeBlock"/> options.
    /// </summary>
    public interface IFlexiCodeBlockOptions : IRenderedRootBlockOptions<IFlexiCodeBlockOptions>
    {
        /// <summary>
        /// Gets the <see cref="FlexiCodeBlock"/>'s title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the <see cref="FlexiCodeBlock"/>'s copy icon as an HTML fragment.
        /// </summary>
        string CopyIcon { get; }

        /// <summary>
        /// Gets the programming language of the <see cref="FlexiCodeBlock"/>'s code.
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Gets the syntax highlighter to highlight the <see cref="FlexiCodeBlock"/>'s code with.
        /// </summary>
        SyntaxHighlighter SyntaxHighlighter { get; }

        /// <summary>
        /// Gets the <see cref="NumberedLineRange"/>s specifying line numbers to render.
        /// </summary>
        ReadOnlyCollection<NumberedLineRange> LineNumbers { get; }

        /// <summary>
        /// Gets the <see cref="FlexiCodeBlock"/>'s omitted lines icon as an HTML fragment.
        /// </summary>
        string OmittedLinesIcon { get; }

        /// <summary>
        /// Gets the <see cref="LineRange"/>s specifying lines to highlight.
        /// </summary>
        ReadOnlyCollection<LineRange> HighlightedLines { get; }

        /// <summary>
        /// Gets the <see cref="PhraseGroup"/>s specifying phrases to highlight.
        /// </summary>
        ReadOnlyCollection<PhraseGroup> HighlightedPhrases { get; }

        /// <summary>
        /// Gets the <see cref="FlexiCodeBlock"/>'s rendering mode.
        /// </summary>
        FlexiCodeBlockRenderingMode RenderingMode { get; }

        /// <summary>
        /// Gets the value specifying whether to render the <see cref="FlexiCodeBlock"/>'s header.
        /// </summary>
        bool RenderHeader { get; }
    }
}
