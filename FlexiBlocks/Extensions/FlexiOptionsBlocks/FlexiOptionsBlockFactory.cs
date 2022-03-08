using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks
{
    /// <summary>
    /// The implementation of <see cref="IJsonBlockFactory{TMain, TProxy}"/> for creating <see cref="FlexiOptionsBlock"/>s.
    /// </summary>
    public class FlexiOptionsBlockFactory : IJsonBlockFactory<FlexiOptionsBlock, ProxyJsonBlock>
    {
        /// <summary>
        /// The key for storing the most recently created <see cref="FlexiOptionsBlock"/>.
        /// </summary>
        public const string PENDING_FLEXI_OPTIONS_BLOCK = "pendingFlexiOptionsBlock";

        /// <summary>
        /// Creates a <see cref="ProxyJsonBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="ProxyJsonBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="ProxyJsonBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        public ProxyJsonBlock CreateProxyJsonBlock(BlockProcessor blockProcessor, BlockParser blockParser)
        {
            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            return new ProxyJsonBlock(nameof(FlexiOptionsBlock), blockParser)
            {
                Column = blockProcessor.Column,
                Span = { Start = blockProcessor.Start } // JsonBlockParser.ParseLine will update the span's end
                // Line is assigned by BlockProcessor
            };
        }

        /// <summary>
        /// Creates a <see cref="FlexiOptionsBlock"/>.
        /// </summary>
        /// <param name="proxyJsonBlock">The <see cref="ProxyJsonBlock"/> containing data for the <see cref="FlexiOptionsBlock"/>.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiOptionsBlock"/>.</param>
        /// <exception cref="BlockException">Thrown if there is an unconsumed <see cref="FlexiOptionsBlock"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="proxyJsonBlock"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        public FlexiOptionsBlock Create(ProxyJsonBlock proxyJsonBlock, BlockProcessor blockProcessor)
        {
            if (proxyJsonBlock == null)
            {
                throw new ArgumentNullException(nameof(proxyJsonBlock));
            }

            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            if (blockProcessor.Document.GetData(PENDING_FLEXI_OPTIONS_BLOCK) is FlexiOptionsBlock pendingFlexiOptionsBlock)
            {
                // There is an unconsumed FlexiOptionsBlock
                throw new BlockException(pendingFlexiOptionsBlock, Strings.BlockException_FlexiOptionsBlockParser_UnconsumedBlock);
            }

            var flexiOptionsBlock = new FlexiOptionsBlock(proxyJsonBlock.Parser)
            {
                Lines = proxyJsonBlock.Lines,
                Line = proxyJsonBlock.Line,
                Column = proxyJsonBlock.Column,
                Span = proxyJsonBlock.Span
            };

            // Save the options block to document data. There are two reasons for this. Firstly, it's easy to detect if an options block goes unused.
            // Secondly, the options block does not need to be a sibling of the block that consumes it. This can occur
            // when extensions like FlexiSections are used. If a container block only ends when a new container block
            // is encountered, an options block can end up being a child of the container block that precedes the container block the options apply to.
            // Searching through the tree of blocks is a brittle approach. This simple approach is relatively robust.
            blockProcessor.Document.SetData(PENDING_FLEXI_OPTIONS_BLOCK, flexiOptionsBlock);

            return null; // Block is already in Document, it does not need to be added to tree
        }
    }
}
