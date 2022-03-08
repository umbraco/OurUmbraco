using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// An abstraction for creating <see cref="FlexiCodeBlock"/>s.
    /// </summary>
    public interface IFlexiCodeBlockFactory : IFencedBlockFactory<FlexiCodeBlock, ProxyFencedLeafBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiCodeBlock"/> from a <see cref="ProxyLeafBlock"/>.
        /// </summary>
        /// <param name="proxyLeafBlock">The <see cref="ProxyLeafBlock"/> containing data for the <see cref="FlexiCodeBlock"/>.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiCodeBlock"/>.</param>
        /// <exception cref="OptionsException">Thrown if an option is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="proxyLeafBlock"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        FlexiCodeBlock Create(ProxyLeafBlock proxyLeafBlock, BlockProcessor blockProcessor);

        /// <summary>
        /// Creates a <see cref="ProxyLeafBlock"/> from indented code.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="ProxyLeafBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="ProxyLeafBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        ProxyLeafBlock CreateProxyLeafBlock(BlockProcessor blockProcessor, BlockParser blockParser);
    }
}
