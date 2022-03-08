using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for creating JSON <see cref="Block"/>s.
    /// </summary>
    /// <typeparam name="TMain">The type of JSON <see cref="Block"/> created.</typeparam>
    /// <typeparam name="TProxy">The type of <see cref="IProxyJsonBlock"/> to collect data for the JSON <see cref="Block"/>.</typeparam>
    public interface IJsonBlockFactory<TMain, TProxy>
        where TMain : Block
        where TProxy : LeafBlock, IProxyJsonBlock
    {
        /// <summary>
        /// Creates a <typeparamref name="TProxy"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <typeparamref name="TProxy"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <typeparamref name="TProxy"/>.</param>
        TProxy CreateProxyJsonBlock(BlockProcessor blockProcessor, BlockParser blockParser);

        /// <summary>
        /// Creates a <typeparamref name="TMain"/>.
        /// </summary>
        /// <param name="proxyJsonBlock">The <typeparamref name="TProxy"/> containing data for the <typeparamref name="TMain"/>.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <typeparamref name="TMain"/>.</param>
        TMain Create(TProxy proxyJsonBlock, BlockProcessor blockProcessor);
    }
}
