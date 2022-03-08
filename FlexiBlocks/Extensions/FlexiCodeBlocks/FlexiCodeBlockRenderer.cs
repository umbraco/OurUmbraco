using Jering.Web.SyntaxHighlighters.HighlightJS;
using Jering.Web.SyntaxHighlighters.Prism;
using Markdig.Renderers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// A renderer that renders <see cref="FlexiCodeBlock"/>s as HTML.
    /// </summary>
    public class FlexiCodeBlockRenderer : BlockRenderer<FlexiCodeBlock>
    {
        private readonly IPrismService _prismService;
        private readonly IHighlightJSService _highlightJSService;

        /// <summary>
        /// Creates a <see cref="FlexiCodeBlockRenderer"/>.
        /// </summary>
        /// <param name="prismService">The service that will handle syntax highlighting using Prism.</param>
        /// <param name="highlightJSService">The service that will handle syntax highlighting using HighlightJS.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="prismService"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="highlightJSService"/> is <c>null</c>.</exception>
        public FlexiCodeBlockRenderer(IPrismService prismService,
            IHighlightJSService highlightJSService)
        {
            _prismService = prismService ?? throw new ArgumentNullException(nameof(prismService));
            _highlightJSService = highlightJSService ?? throw new ArgumentNullException(nameof(highlightJSService));
        }

        /// <summary>
        /// Renders a <see cref="FlexiCodeBlock"/> as HTML.
        /// </summary>
        /// <param name="htmlRenderer">The renderer to write to.</param>
        /// <param name="block">The <see cref="FlexiCodeBlock"/> to render.</param>
        protected override void WriteBlock(HtmlRenderer htmlRenderer, FlexiCodeBlock block)
        {
            if (!htmlRenderer.EnableHtmlForBlock)
            {
                htmlRenderer.
                    WriteEscape(block.Code).
                    WriteLine();

                return;
            }

            if (block.RenderingMode == FlexiCodeBlockRenderingMode.Classic)
            {
                WriteClassic(htmlRenderer, block);
                return;
            }

            WriteStandard(htmlRenderer, block);
        }

        internal virtual void WriteClassic(HtmlRenderer htmlRenderer, FlexiCodeBlock flexiCodeBlock)
        {
            htmlRenderer.
                Write("<pre><code>");

            if (flexiCodeBlock.CodeNumLines > 0)
            {
                htmlRenderer.
                    WriteEscape(flexiCodeBlock.Code).
                    WriteLine();
            }

            htmlRenderer.
                WriteLine("</code></pre>").
                EnsureLine();
        }

        internal virtual void WriteStandard(HtmlRenderer htmlRenderer, FlexiCodeBlock flexiCodeBlock)
        {
            ReadOnlyDictionary<string, string> attributes = flexiCodeBlock.Attributes;
            ReadOnlyCollection<NumberedLineRange> lineNumbers = flexiCodeBlock.LineNumbers;
            ReadOnlyCollection<LineRange> highlightedLines = flexiCodeBlock.HighlightedLines;
            ReadOnlyCollection<Phrase> highlightedPhrases = flexiCodeBlock.HighlightedPhrases;
            int codeNumLines = flexiCodeBlock.CodeNumLines;
            string blockName = flexiCodeBlock.BlockName,
                title = flexiCodeBlock.Title,
                copyIcon = flexiCodeBlock.CopyIcon,
                language = flexiCodeBlock.Language,
                omittedLinesIcon = flexiCodeBlock.OmittedLinesIcon,
                code = flexiCodeBlock.Code;
            SyntaxHighlighter syntaxHighlighter = flexiCodeBlock.SyntaxHighlighter;
            bool hasTitle = !string.IsNullOrWhiteSpace(title),
                hasCopyIcon = !string.IsNullOrWhiteSpace(copyIcon),
                hasLanguage = !string.IsNullOrWhiteSpace(language),
                hasSyntaxHighlights = syntaxHighlighter != SyntaxHighlighter.None && hasLanguage,
                hasLineNumbers = lineNumbers?.Count > 0,
                hasOmittedLinesIcon = !string.IsNullOrWhiteSpace(omittedLinesIcon),
                hasHighlightedLines = highlightedLines?.Count > 0,
                hasHighlightedPhrases = highlightedPhrases?.Count > 0,
                hasHeader = flexiCodeBlock.RenderHeader;

            // Root element
            htmlRenderer.
                Write("<div class=\"").
                Write(blockName).
                WriteHasFeatureClass(hasTitle, blockName, "title").
                WriteHasFeatureClass(hasCopyIcon, blockName, "copy-icon").
                WriteHasFeatureClass(hasHeader, blockName, "header").
                WriteBlockKeyValueModifierClass(hasLanguage, blockName, "language", language).
                WriteHasFeatureClass(hasSyntaxHighlights, blockName, "syntax-highlights").
                WriteHasFeatureClass(hasLineNumbers, blockName, "line-numbers").
                WriteHasFeatureClass(hasOmittedLinesIcon, blockName, "omitted-lines-icon").
                WriteHasFeatureClass(hasHighlightedLines, blockName, "highlighted-lines").
                WriteHasFeatureClass(hasHighlightedPhrases, blockName, "highlighted-phrases").
                WriteAttributeValue(attributes, "class").
                Write('"').
                WriteAttributesExcept(attributes, "class").
                WriteLine(">");

            // Header
            if (hasHeader)
            {
                htmlRenderer.
                    WriteStartTagLine("header", blockName, "header").
                    WriteElementLine(hasTitle, "span", blockName, "title", title).
                    WriteStartTagLineWithAttributes("button", blockName, "copy-button", "aria-label=\"Copy code\"").
                    WriteHtmlFragmentLine(hasCopyIcon, copyIcon, blockName, "copy-icon").
                    WriteEndTagLine("button").
                    WriteEndTagLine("header");
            }

            // Code
            htmlRenderer.
                WriteStartTag("pre", blockName, "pre").
                WriteStartTag("code", blockName, "code");

            // Code - Syntax Highlighting
            if (hasSyntaxHighlights)
            {
                // All code up the stack from HighlightAsync calls ConfigureAwait(false), so there is no need to run these calls in the thread pool.
                // Use GetAwaiter and GetResult to avoid an AggregateException - https://blog.stephencleary.com/2014/12/a-tour-of-task-part-6-results.html
                code = (syntaxHighlighter == SyntaxHighlighter.HighlightJS ? _highlightJSService.HighlightAsync(code, language) : _prismService.HighlightAsync(code, language)).GetAwaiter().GetResult();
            }

            // If there is no need for code embellishements
            if (!hasLineNumbers && !hasHighlightedLines && !hasHighlightedPhrases)
            {
                if (code?.Length > 0)
                {
                    if (hasSyntaxHighlights)
                    {
                        htmlRenderer.Write(code); // Already escaped
                    }
                    else
                    {
                        htmlRenderer.WriteEscape(code);
                    }

                    htmlRenderer.
                        WriteLine(); // \n before </code> for consistency with CommonMark
                }

                htmlRenderer.
                    WriteLine("</code></pre>").
                    WriteLine("</div>");

                return;
            }

            // Code - Prepare to render line numbers
            bool currentLineHasLineNumber = false;
            NumberedLineRange currentLineNumberRange = default;
            int currentLineNumberToRender = 0,
                currentUnrenderedRangeFirstLineNumber = 1;
            List<NumberedLineRange>.Enumerator lineNumbersEnumerator = default;
            if (hasLineNumbers)
            {
                lineNumbersEnumerator = (List<NumberedLineRange>.Enumerator)lineNumbers.GetEnumerator();
                lineNumbersEnumerator.MoveNext();
                currentLineNumberRange = lineNumbersEnumerator.Current;
                currentLineNumberToRender = currentLineNumberRange.StartNumber;
            }

            // Code - Prepare to highlight lines
            bool currentLineIsHighlighted = false;
            LineRange currentHighlightedLineRange = default;
            List<LineRange>.Enumerator highlightedLinesEnumerator = default;
            if (hasHighlightedLines)
            {
                highlightedLinesEnumerator = (List<LineRange>.Enumerator)highlightedLines.GetEnumerator();
                highlightedLinesEnumerator.MoveNext();
                currentHighlightedLineRange = highlightedLinesEnumerator.Current;
                currentLineIsHighlighted = currentHighlightedLineRange.GetRelativePosition(1, codeNumLines) == 0;
            }

            // Code - Prepare to highlight phrases
            bool inHighlightedPhrase = false;
            int currentCodeCharIndex = 0; // Index ignoring HTML elements
            Phrase currentHighlightedPhrase = default;
            List<Phrase>.Enumerator highlightedPhrasesEnumerator = default;
            if (hasHighlightedPhrases)
            {
                highlightedPhrasesEnumerator = (List<Phrase>.Enumerator)highlightedPhrases.GetEnumerator();
                highlightedPhrasesEnumerator.MoveNext();
                currentHighlightedPhrase = highlightedPhrasesEnumerator.Current;
            }

            // Code - Write Embellished
            // We have to iterate over every character so we can write elements for highlighted phrases. 
            // Writing elements for highlighted phrases involves flattening intersecting elements, e.g <phrase>xxx<syntax>xxx</phrase>xxx</syntax>
            // is flattened to <phrase>xxx<syntax>xxx</syntax></phrase><syntax>xxx</syntax>. Flattening is the reason for the dense nature of the following code.
            // While dense, the following code is efficient, requiring only 1 pass and generating no intermediate strings/objects.
            var openElements = new Stack<Element>();
            var pendingElements = new Stack<Element>();
            TextWriter textWriter = htmlRenderer.Writer; // Faster to write chars directly
            bool previousLineHasLineElement = false, currentLineHasLineElement = hasLineNumbers || currentLineIsHighlighted;
            int codeLength = code.Length,
                currentLineNumber = 1,
                i = -1; // Starting from -1 is necessary for HandleLineStart
            char currentChar;
            HandleLineStart();
            for (i = 0; i < codeLength; i++)
            {
                currentChar = code[i];

                if (hasSyntaxHighlights && currentChar == '<') // Syntax element tags
                {
                    HandleSyntaxElementTag();
                    continue;
                }

                HandlePhraseStart();

                if (currentChar == '\r' || currentChar == '\n')
                {
                    HandleEndOfLineChar();
                }
                else if (!hasSyntaxHighlights) // If code has not been syntax highlighted, it may have unescaped characters
                {
                    WriteUnescaped();
                }
                else
                {
                    WriteEscaped();
                }

                HandlePhraseEnd();
                currentCodeCharIndex++;
            }
            HandleLineEnd();

            htmlRenderer.
                WriteLine().
                WriteLine("</code></pre>").
                WriteLine("</div>");

            void HandleSyntaxElementTag()
            {
                if (code[i + 1] == '/')
                {
                    WriteEndTag();
                    Element lastElement = openElements.Pop();
                    if (lastElement.Type != 0) // Last element is a highlighted phrase element, only one such element can be open at a time
                    {
                        WriteEndTag();
                        WriteStartTag("__highlighted-phrase");
                        openElements.Pop();
                        openElements.Push(lastElement);
                    }

                    i += 6; // Skip end tag
                }
                else
                {
                    HandlePhraseStart(); // If a phrase starts at the current index, write the phrase start element first to minimize splitting

                    for (int j = i + 15; j < codeLength; j++) // 16 is the min number of characters in a start tag: <span class="x">
                    {
                        if (code[j] == '>')
                        {
                            int length = j - i + 1;
                            openElements.Push(new Element(i, length));
                            htmlRenderer.Write(code, i, length);
                            i = j;
                            break;
                        }
                    }
                }
            }

            void HandlePhraseStart()
            {
                if (hasHighlightedPhrases &&
                    !inHighlightedPhrase &&
                    currentHighlightedPhrase?.Start == currentCodeCharIndex)
                {
                    inHighlightedPhrase = true;
                    openElements.Push(new Element(1));
                    WriteStartTag("__highlighted-phrase");
                }
            }

            void HandlePhraseEnd()
            {
                if (inHighlightedPhrase &&
                    currentHighlightedPhrase?.End == currentCodeCharIndex)
                {
                    // Find next phrase
                    do
                    {
                        if (highlightedPhrasesEnumerator.MoveNext())
                        {
                            currentHighlightedPhrase = highlightedPhrasesEnumerator.Current;
                        }
                        else
                        {
                            currentHighlightedPhrase = null;
                            break;
                        }
                    }
                    while (currentHighlightedPhrase.End <= currentCodeCharIndex); // If two phrases have the same start index, the longer one is ordered before the shorter one

                    // Write end
                    if (currentHighlightedPhrase == null || currentHighlightedPhrase.Start > currentCodeCharIndex + 1) // Ignore overlapping and adjacent phrases so they get combined
                    {
                        inHighlightedPhrase = false;

                        if (hasSyntaxHighlights)
                        {
                            // If syntax elements end at the same code index, close them first so that we don't end up with empty elements
                            int nextIndex = i + 1;
                            while (nextIndex < codeLength && code[nextIndex] == '<' && code[nextIndex + 1] == '/')
                            {
                                WriteEndTag();
                                if (openElements.Pop().Type == 1)
                                {
                                    return;
                                }
                                i += 7;
                                nextIndex = i + 1;
                            }
                        }

                        while (openElements.Count > 0)
                        {
                            WriteEndTag();
                            Element element = openElements.Pop();
                            if (element.Type == 1)
                            {
                                break;
                            }
                            else
                            {
                                pendingElements.Push(element);
                            }
                        }

                        while (pendingElements.Count > 0)
                        {
                            Element element = pendingElements.Pop();
                            htmlRenderer.Write(code, element.StartIndex, element.Length);
                            openElements.Push(element);
                        }
                    }
                }
            }

            void HandleEndOfLineChar()
            {
                int nextIndex;
                if (currentChar == '\r' && (nextIndex = i + 1) < codeLength && code[nextIndex] == '\n')
                {
                    HandlePhraseEnd(); // If a phrase ends at \r, allow it to end

                    i = nextIndex;
                    currentCodeCharIndex++;
                }

                HandleLineEnd();
                textWriter.WriteLine();
                HandleLineStart();
            }

            void HandleLineStart()
            {
                // Prefix element
                if (hasLineNumbers)
                {
                    WriteStartTag("__line-prefix");

                    int relativePosition = -1;
                    if (currentLineNumberRange != null)
                    {
                        relativePosition = currentLineNumberRange.GetRelativePosition(currentLineNumber, codeNumLines);
                        if (relativePosition == -1)
                        {
                            currentUnrenderedRangeFirstLineNumber = currentLineNumberToRender;
                            if (lineNumbersEnumerator.MoveNext())
                            {
                                currentLineNumberRange = lineNumbersEnumerator.Current;
                                currentLineNumberToRender = currentLineNumberRange.StartNumber;
                                relativePosition = currentLineNumberRange.GetRelativePosition(currentLineNumber, codeNumLines);
                            }
                            else
                            {
                                currentLineNumberRange = null;
                            }
                        }
                    }

                    if (currentLineHasLineNumber = relativePosition == 0)
                    {
                        textWriter.Write(currentLineNumberToRender++);
                    }
                    else
                    {
                        htmlRenderer.WriteHtmlFragment(hasOmittedLinesIcon, omittedLinesIcon, blockName, "omitted-lines-icon");
                    }

                    WriteEndTag();
                }

                // Write line element start tag
                if (currentLineHasLineElement)
                {
                    textWriter.Write("<span class=\"");
                    textWriter.Write(blockName);
                    textWriter.Write("__line");

                    if (currentLineIsHighlighted)
                    {
                        textWriter.Write(' ');
                        textWriter.Write(blockName);
                        textWriter.Write("__line_highlighted");
                    }

                    bool representsOmittedLines = hasLineNumbers && !currentLineHasLineNumber;
                    if (representsOmittedLines)
                    {
                        textWriter.Write(' ');
                        textWriter.Write(blockName);
                        textWriter.Write("__line_omitted-lines");
                    }

                    textWriter.Write("\">");

                    openElements.Push(new Element(2));

                    // Write omitted lines notice
                    char nextChar;
                    int nextIndex;
                    if (representsOmittedLines &&
                        // These conditions check whether the line is empty
                        ((nextIndex = i + 1) == codeLength ||
                        (nextChar = code[nextIndex]) == '\n' ||
                        nextChar == '\r'))
                    {
                        // TODO if currentUnrenderedRangeFirstLineNumber > currentLineNumberToRender.
                        // Also, consider making notices customizable through FlexiCodeBlockOptions
                        int currentUnrenderedRangeLastLineNumber = currentLineNumberToRender - 1;
                        if (currentUnrenderedRangeFirstLineNumber == currentUnrenderedRangeLastLineNumber)
                        {
                            textWriter.Write("Line {0} omitted for brevity", currentUnrenderedRangeLastLineNumber);
                        }
                        else
                        {
                            object firstArg, secondArg;
                            if (currentLineNumberRange == null) // Till end of document
                            {
                                firstArg = currentUnrenderedRangeFirstLineNumber;
                                secondArg = "the end";
                            }
                            else
                            {
                                firstArg = currentUnrenderedRangeFirstLineNumber;
                                secondArg = currentUnrenderedRangeLastLineNumber;
                            }

                            textWriter.Write("Lines {0} to {1} omitted for brevity", firstArg, secondArg);
                        }
                    }
                }

                // Reopen pending elements
                if (currentLineHasLineElement || previousLineHasLineElement)
                {
                    while (pendingElements.Count > 0)
                    {
                        Element element = pendingElements.Pop();
                        if (element.Type == 0)
                        {
                            htmlRenderer.Write(code, element.StartIndex, element.Length);
                        }
                        else
                        {
                            WriteStartTag("__highlighted-phrase");
                        }

                        openElements.Push(element);
                    }
                }
            }

            void HandleLineEnd()
            {
                // Increment line number
                currentLineNumber++;

                // Update currentLineIsHighlighted
                if (currentHighlightedLineRange != null)
                {
                    int relativePosition = currentHighlightedLineRange.GetRelativePosition(currentLineNumber, codeNumLines);
                    if (relativePosition == -1)
                    {
                        if (highlightedLinesEnumerator.MoveNext())
                        {
                            currentHighlightedLineRange = highlightedLinesEnumerator.Current;
                            relativePosition = currentHighlightedLineRange.GetRelativePosition(currentLineNumber, codeNumLines);
                        }
                        else
                        {
                            currentHighlightedLineRange = null;
                        }
                    }
                    currentLineIsHighlighted = relativePosition == 0;
                }

                // Line end tags
                previousLineHasLineElement = currentLineHasLineElement;
                currentLineHasLineElement = hasLineNumbers || currentLineIsHighlighted;
                if (previousLineHasLineElement || currentLineHasLineElement)
                {
                    while (openElements.Count > 0)
                    {
                        WriteEndTag();
                        Element element = openElements.Pop();
                        if (element.Type == 2)
                        {
                            break; // Line element is always the last 
                        }
                        else
                        {
                            pendingElements.Push(element);
                        }
                    }
                }
            }

            void WriteStartTag(string element)
            {
                textWriter.Write("<span class=\"");
                textWriter.Write(blockName);
                textWriter.Write(element);
                textWriter.Write("\">");
            }

            void WriteEndTag()
            {
                textWriter.Write("</span>");
            }

            void WriteUnescaped()
            {
                switch (currentChar)
                {
                    case '<':
                        textWriter.Write("&lt;");
                        break;
                    case '>':
                        textWriter.Write("&gt;");
                        break;
                    case '&':
                        textWriter.Write("&amp;");
                        break;
                    case '"':
                        textWriter.Write("&quot;");
                        break;
                    default:
                        textWriter.Write(currentChar);
                        break;
                }
            }

            void WriteEscaped()
            {
                if (currentChar == '&')
                {
                    switch (code[i + 1])
                    {
                        case 'l':
                            textWriter.Write("&lt;");
                            i += 3;
                            break;
                        case 'g':
                            textWriter.Write("&gt;");
                            i += 3;
                            break;
                        case 'a':
                            textWriter.Write("&amp;");
                            i += 4;
                            break;
                        case 'q':
                            textWriter.Write("&quot;");
                            i += 5;
                            break;
                    }
                }
                else
                {
                    textWriter.Write(currentChar);
                }
            }
        }

        private readonly struct Element
        {
            public readonly int StartIndex,
                Length,
                Type; // 0 == syntax, 1 == highlighted phrase, 2 == line

            public Element(int startIndex, int length)
            {
                StartIndex = startIndex;
                Length = length;
                Type = 0;
            }

            public Element(int type)
            {
                Type = type;
                StartIndex = default;
                Length = default;
            }
        }
    }
}
