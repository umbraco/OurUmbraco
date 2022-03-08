using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// <para>An abstraction for parsing <see cref="Block"/>s using <see cref="IProxyBlock"/>s.</para>
    /// </summary>
    /// <typeparam name="TMain">The type of <see cref="Block"/> this parser parsers.</typeparam>
    /// <typeparam name="TProxy">The type of <see cref="IProxyBlock"/> to collect data for the <typeparamref name="TMain"/>.</typeparam>
    public abstract class ProxyBlockParser<TMain, TProxy> : BlockParser<TProxy>
        where TMain : Block
        where TProxy : Block, IProxyBlock
    {
        /// <summary>
        /// Closes a <typeparamref name="TProxy"/>, optionally returning a <typeparamref name="TMain"/> to replace it with.
        /// </summary>
        /// <param name="blockProcessor">
        /// <para>The <see cref="BlockProcessor"/> processing the <typeparamref name="TProxy"/> to close. Never <c>null</c>.</para>
        /// </param>
        /// <param name="proxyBlock">
        /// <para>The <typeparamref name="TProxy"/> to close. Never <c>null</c>.</para>
        /// </param>
        /// <returns>
        /// Returns <c>null</c> if the <typeparamref name="TProxy"/> should be discarded with no <typeparamref name="TMain"/> replacement.
        /// Returns a replacement <typeparamref name="TMain"/> if the <typeparamref name="TProxy"/> is to be replaced.
        /// </returns>
        protected virtual TMain CloseProxy(BlockProcessor blockProcessor, TProxy proxyBlock)
        {
            // Discard the proxy block with no replacement by default
            return null;
        }

        /// <summary>
        /// Closes a <typeparamref name="TProxy"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <typeparamref name="TProxy"/> to close. Never <c>null</c>.</param>
        /// <param name="block">The <typeparamref name="TProxy"/> to close. Never <c>null</c>.</param>
        /// <returns>False if the block should be discarded, true otherwise.</returns>
        protected sealed override bool CloseBlock(BlockProcessor blockProcessor, TProxy block)
        {
            TMain proxyReplacement = CloseProxy(blockProcessor, block);

            if (proxyReplacement == null)
            {
                return false;
            }

            ContainerBlock parent = block.Parent;
            // TODO inefficient
            int index = parent.IndexOf(block);
            parent.RemoveAt(index);
            parent.Insert(index, proxyReplacement);

            return true;
        }
    }
}
