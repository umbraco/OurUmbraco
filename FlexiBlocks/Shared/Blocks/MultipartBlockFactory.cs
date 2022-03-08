using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// <para>An abstraction for creating multipart <see cref="Block"/>s.</para>
    /// 
    /// <para>Implements <see cref="IMultipartBlockFactory{T}"/>.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MultipartBlockFactory<T> : IMultipartBlockFactory<T> where T : Block
    {
        private readonly PlainBlockParser _plainBlockParser;

        /// <summary>
        /// Creates a <see cref="MultipartBlockFactory{T}"/>.
        /// </summary>
        /// <param name="plainBlockParser">The <see cref="BlockParser"/> for parsing plain <see cref="Block"/>s.</param>
        protected MultipartBlockFactory(PlainBlockParser plainBlockParser)
        {
            _plainBlockParser = plainBlockParser ?? throw new ArgumentNullException(nameof(plainBlockParser));
        }

        /// <inheritdoc />
        public abstract T Create(BlockProcessor blockProcessor, BlockParser blockParser);

        /// <inheritdoc />
        public virtual Block CreatePart(PartType partType, BlockProcessor blockProcessor)
        {
            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            Block result;
            if (partType == PartType.Container)
            {
                result = new PlainContainerBlock(_plainBlockParser);
            }
            else
            {
                result = new PlainLeafBlock(_plainBlockParser);
            }

            result.Column = blockProcessor.Column;
            result.Span = new SourceSpan(blockProcessor.Start, 0); // End updated by PlainBlockParser
            // Line is assigned by BlockProcessor

            return result;
        }
    }
}
