using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for creating multipart <see cref="Block"/>s.
    /// </summary>
    /// <typeparam name="T">The type of multipart <see cref="Block"/> created.</typeparam>
    public interface IMultipartBlockFactory<T>
    {
        /// <summary>
        /// Creates a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <typeparamref name="T"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <typeparamref name="T"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        T Create(BlockProcessor blockProcessor, BlockParser blockParser);

        /// <summary>
        /// Creates a <see cref="PlainContainerBlock"/> or a <see cref="PlainLeafBlock"/>.
        /// </summary>
        /// <param name="partType">The type of plain <see cref="Block"/> to create.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the plain <see cref="Block"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        Block CreatePart(PartType partType, BlockProcessor blockProcessor);
    }
}
