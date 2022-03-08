using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for creating fenced <see cref="Block"/>s.
    /// </summary>
    /// <typeparam name="TMain">The type of fenced <see cref="Block"/> created.</typeparam>
    /// <typeparam name="TProxy">The type of <see cref="IProxyFencedBlock"/> to collect data for the fenced <see cref="Block"/>.</typeparam>
    public interface IFencedBlockFactory<TMain, TProxy>
        where TMain : Block
        where TProxy : Block, IProxyFencedBlock
    {
        /// <summary>
        /// Creates a <typeparamref name="TProxy"/>.
        /// </summary>
        /// <param name="openingFenceIndent">The indent of the opening fence.</param>
        /// <param name="openingFenceCharCount">The number of characters in the opening fence.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <typeparamref name="TProxy"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <typeparamref name="TProxy"/>.</param>
        TProxy CreateProxyFencedBlock(int openingFenceIndent,
            int openingFenceCharCount,
            BlockProcessor blockProcessor,
            BlockParser blockParser);

        /// <summary>
        /// Creates a <typeparamref name="TMain"/>.
        /// </summary>
        /// <param name="proxyFencedBlock">The <see cref="IProxyFencedBlock"/> containing data for the <typeparamref name="TMain"/>.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <typeparamref name="TMain"/>.</param>
        TMain Create(TProxy proxyFencedBlock, BlockProcessor blockProcessor);
    }
}
