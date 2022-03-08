using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for creating fenced <see cref="ContainerBlock"/>s with children of a single type.
    /// </summary>
    /// <typeparam name="TMain">The type of fenced <see cref="ContainerBlock"/> created.</typeparam>
    /// <typeparam name="TProxy">The type of <see cref="ProxyFencedContainerBlock"/> to collect data for the fenced <see cref="ContainerBlock"/>.</typeparam>
    /// <typeparam name="TChild">The type of <see cref="Block"/> the fenced <see cref="ContainerBlock"/> may contain.</typeparam>
    public abstract class CollectionBlockFactory<TMain, TProxy, TChild> : IFencedBlockFactory<TMain, TProxy>
        where TMain : ContainerBlock
        where TProxy : ContainerBlock, IProxyFencedBlock
        where TChild : Block
    {
        /// <inheritdoc />
        public abstract TMain Create(TProxy proxyFencedBlock, BlockProcessor blockProcessor);

        /// <inheritdoc />
        public abstract TProxy CreateProxyFencedBlock(int openingFenceIndent, int openingFenceCharCount, BlockProcessor blockProcessor, BlockParser blockParser);

        /// <summary>
        /// Moves children from a <typeparamref name="TProxy"/> to a <typeparamref name="TMain"/>.
        /// </summary>
        /// <param name="source">The <typeparamref name="TProxy"/> to move children from.</param>
        /// <param name="target">The <typeparamref name="TMain"/> to move children to.</param>
        /// <exception cref="BlockException">Thrown if <paramref name="source"/> has a child that isn't a <typeparamref name="TChild"/>.</exception>
        protected virtual void MoveChildren(TProxy source, TMain target)
        {
            // TODO not efficient
            int numChildren = source.Count;
            for (int i = numChildren - 1; i > -1; i--)
            {
                if (!(source[i] is TChild childBlock))
                {
                    throw new BlockException(target,
                        string.Format(Strings.BlockException_Shared_BlockMustOnlyContainASpecificTypeOfBlock,
                            typeof(TMain).Name,
                            typeof(TChild).Name,
                            source[i].GetType().Name));
                }
                source.RemoveAt(i);
                target.Insert(0, childBlock);
            }
        }
    }
}
