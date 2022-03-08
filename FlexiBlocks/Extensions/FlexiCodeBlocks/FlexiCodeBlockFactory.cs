using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// The default implementation of <see cref="IFlexiCodeBlockFactory"/>.
    /// </summary>
    public class FlexiCodeBlockFactory : IFlexiCodeBlockFactory
    {
        private readonly IOptionsService<IFlexiCodeBlockOptions, IFlexiCodeBlocksExtensionOptions> _optionsService;

        /// <summary>
        /// Creates a <see cref="FlexiCodeBlockFactory"/>.
        /// </summary>
        /// <param name="optionsService">The service for creating <see cref="IFlexiCodeBlockOptions"/> and <see cref="IFlexiCodeBlocksExtensionOptions"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsService"/> is <c>null</c>.</exception>
        public FlexiCodeBlockFactory(IOptionsService<IFlexiCodeBlockOptions, IFlexiCodeBlocksExtensionOptions> optionsService)
        {
            _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
        }

        /// <summary>
        /// Creates a <see cref="ProxyFencedLeafBlock"/> from fenced code.
        /// </summary>
        /// <param name="openingFenceIndent">The indent of the opening fence.</param>
        /// <param name="openingFenceCharCount">The number of characters in the opening fence.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="ProxyFencedLeafBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="ProxyFencedLeafBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        public ProxyFencedLeafBlock CreateProxyFencedBlock(int openingFenceIndent, int openingFenceCharCount, BlockProcessor blockProcessor, BlockParser blockParser)
        {
            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            return new ProxyFencedLeafBlock(openingFenceIndent, openingFenceCharCount, nameof(FlexiCodeBlock), blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, blockProcessor.Line.End)
                // Line is assigned by BlockProcessor
            };
        }

        /// <summary>
        /// Creates a <see cref="FlexiCodeBlock"/> from a <see cref="ProxyFencedLeafBlock"/>.
        /// </summary>
        /// <param name="proxyFencedBlock">The <see cref="ProxyFencedLeafBlock"/> containing data for the <see cref="FlexiCodeBlock"/>.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiCodeBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="proxyFencedBlock"/> is <c>null</c>.</exception>
        /// <exception cref="OptionsException">Thrown if an option is invalid.</exception>
        public FlexiCodeBlock Create(ProxyFencedLeafBlock proxyFencedBlock, BlockProcessor blockProcessor)
        {
            return CreateCore(proxyFencedBlock, blockProcessor);
        }

        /// <inheritdoc />
        public ProxyLeafBlock CreateProxyLeafBlock(BlockProcessor blockProcessor, BlockParser blockParser)
        {
            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            return new ProxyLeafBlock(nameof(FlexiCodeBlock), blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, blockProcessor.Line.End)
                // Line is assigned by BlockProcessor
            };
        }

        /// <inheritdoc />
        public FlexiCodeBlock Create(ProxyLeafBlock proxyLeafBlock, BlockProcessor blockProcessor)
        {
            return CreateCore(proxyLeafBlock, blockProcessor);
        }

        internal virtual FlexiCodeBlock CreateCore(ProxyLeafBlock proxyLeafBlock, BlockProcessor blockProcessor)
        {
            if (proxyLeafBlock == null)
            {
                throw new ArgumentNullException(nameof(proxyLeafBlock));
            }

            (IFlexiCodeBlockOptions flexiCodeBlockOptions, IFlexiCodeBlocksExtensionOptions _) = _optionsService.CreateOptions(blockProcessor);

            // Code
            StringLineGroup lines = proxyLeafBlock.Lines;
            string code = proxyLeafBlock.Lines.ToString();
            int codeNumLines = lines.Count;

            // Block name
            string blockName = ResolveBlockName(flexiCodeBlockOptions.BlockName);

            // Syntax highlighter
            SyntaxHighlighter syntaxHighlighter = flexiCodeBlockOptions.SyntaxHighlighter;
            ValidateSyntaxHighlighter(syntaxHighlighter);

            // Line numbers
            ReadOnlyCollection<NumberedLineRange> lineNumbers = TryCreateSortedLineRanges(flexiCodeBlockOptions.LineNumbers, codeNumLines);
            ValidateSortedLineNumbers(lineNumbers, codeNumLines);

            // Highlighted lines
            ReadOnlyCollection<LineRange> highlightedLines = TryCreateSortedLineRanges(flexiCodeBlockOptions.HighlightedLines, codeNumLines);

            // Highlighted phrases
            ReadOnlyCollection<Phrase> highlightedPhrases = ResolveHighlightedPhrases(code, flexiCodeBlockOptions.HighlightedPhrases);

            // Rendering mode
            FlexiCodeBlockRenderingMode renderingMode = flexiCodeBlockOptions.RenderingMode;
            ValidateRenderingMode(renderingMode);

            return new FlexiCodeBlock(
                blockName,
                flexiCodeBlockOptions.Title,
                flexiCodeBlockOptions.CopyIcon,
                flexiCodeBlockOptions.RenderHeader,
                flexiCodeBlockOptions.Language,
                code,
                codeNumLines,
                syntaxHighlighter,
                lineNumbers,
                flexiCodeBlockOptions.OmittedLinesIcon,
                highlightedLines,
                highlightedPhrases,
                renderingMode,
                flexiCodeBlockOptions.Attributes,
                proxyLeafBlock.Parser)
            {
                Line = proxyLeafBlock.Line,
                Column = proxyLeafBlock.Column,
                Span = proxyLeafBlock.Span
            };
        }

        internal virtual string ResolveBlockName(string blockName)
        {
            return string.IsNullOrWhiteSpace(blockName) ? "flexi-code" : blockName;
        }

        internal virtual void ValidateSyntaxHighlighter(SyntaxHighlighter syntaxHighlighter)
        {
            if (!Enum.IsDefined(typeof(SyntaxHighlighter), syntaxHighlighter))
            {
                throw new OptionsException(nameof(IFlexiCodeBlockOptions.SyntaxHighlighter),
                        string.Format(Strings.OptionsException_Shared_ValueMustBeAValidEnumValue,
                            syntaxHighlighter,
                            nameof(SyntaxHighlighter)));
            }
        }

        // TODO This method throws if a LineRange has invalid start or end values, wrap in OptionsException?
        internal virtual ReadOnlyCollection<T> TryCreateSortedLineRanges<T>(ReadOnlyCollection<T> lineRanges, int codeNumLines)
            where T : LineRange
        {
            if (lineRanges == null)
            {
                return null;
            }

            int numLineRanges = lineRanges.Count;
            if (numLineRanges < 1)
            {
                return null;
            }

            bool sort = false;
            var lineRangeComparer = new LineRangeComparer(codeNumLines); // TODO consider pooling
            T currentNumberedLineRange = lineRanges[0];
            for (int i = 1; i < numLineRanges; i++)
            {
                T nextLineRange = lineRanges[i];
                int compareResult = lineRangeComparer.Compare(currentNumberedLineRange, nextLineRange);
                if (compareResult > 0)
                {
                    sort = true;
                    break;
                }
                currentNumberedLineRange = nextLineRange;
            }

            if (sort)
            {
                var newLineRanges = new T[numLineRanges]; // TODO consider pooling
                lineRanges.CopyTo(newLineRanges, 0);
                Array.Sort(newLineRanges, lineRangeComparer);
                return new ReadOnlyCollection<T>(newLineRanges);
            }

            return lineRanges;
        }

        internal virtual void ValidateSortedLineNumbers(ReadOnlyCollection<NumberedLineRange> lineNumbers, int codeNumLines)
        {
            if (lineNumbers != null)
            {
                NumberedLineRange currentNumberedLineRange = lineNumbers[0];
                (int currentNormalizedStartLine, int currentNormalizedEndLine) = currentNumberedLineRange.GetNormalizedStartAndEndLines(codeNumLines);
                int currentStartNumber = currentNumberedLineRange.StartNumber;
                int lastNormalizedEndLine = currentNormalizedEndLine;
                int lastEndNumber = currentStartNumber + (currentNormalizedEndLine - currentNormalizedStartLine);

                int numNumberedLineRanges = lineNumbers.Count;
                for (int i = 1; i < numNumberedLineRanges; i++)
                {
                    currentNumberedLineRange = lineNumbers[i];
                    (currentNormalizedStartLine, currentNormalizedEndLine) = currentNumberedLineRange.GetNormalizedStartAndEndLines(codeNumLines);
                    currentStartNumber = currentNumberedLineRange.StartNumber;

                    if (currentNormalizedStartLine <= lastNormalizedEndLine || currentStartNumber <= lastEndNumber)
                    {
                        throw new OptionsException(nameof(IFlexiCodeBlockOptions.LineNumbers),
                            string.Format(Strings.OptionsException_FlexiCodeBlocks_OverlappingLineNumbers,
                                lineNumbers[i - 1],
                                currentNumberedLineRange));
                    }

                    lastNormalizedEndLine = currentNormalizedEndLine;
                    lastEndNumber = currentStartNumber + (currentNormalizedEndLine - currentNormalizedStartLine);
                }
            }
        }

        internal virtual ReadOnlyCollection<Phrase> ResolveHighlightedPhrases(string code, ReadOnlyCollection<PhraseGroup> highlightedPhrases)
        {
            if (highlightedPhrases == null)
            {
                return null;
            }

            int numPhraseGroups = highlightedPhrases.Count;
            if (numPhraseGroups == 0)
            {
                return null;
            }

            if (code.Length == 0)
            {
                return null;
            }

            var result = new List<Phrase>(); // We don't know how many phrases will be created from regex matches
            for (int i = 0; i < numPhraseGroups; i++)
            {
                highlightedPhrases[i].GetPhrases(code, result);
            }

            if (result.Count == 0) // This can happen if phrase groups have no matches
            {
                return null;
            }

            result.Sort();

            return new ReadOnlyCollection<Phrase>(result);
        }

        internal virtual void ValidateRenderingMode(FlexiCodeBlockRenderingMode renderingMode)
        {
            if (!Enum.IsDefined(typeof(FlexiCodeBlockRenderingMode), renderingMode))
            {
                throw new OptionsException(nameof(IFlexiCodeBlockOptions.RenderingMode),
                    string.Format(Strings.OptionsException_Shared_ValueMustBeAValidEnumValue,
                        renderingMode,
                        nameof(FlexiCodeBlockRenderingMode)));
            }
        }
    }
}
