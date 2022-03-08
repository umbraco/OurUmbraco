using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// An abstraction for creating <see cref="FlexiTableBlock"/>s.
    /// </summary>
    public interface IFlexiTableBlockFactory
    {
        /// <summary>
        /// Creates a <see cref="FlexiTableBlock"/> from a <see cref="ProxyTableBlock"/>.
        /// </summary>
        /// <param name="proxyTableBlock">The <see cref="ProxyTableBlock"/> containing data for the <see cref="FlexiTableBlock"/>.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiTableBlock"/>.</param>
        /// <exception cref="OptionsException">Thrown if an option is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="proxyTableBlock"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        FlexiTableBlock Create(ProxyTableBlock proxyTableBlock, BlockProcessor blockProcessor);

        /// <summary>
        /// Creates a <see cref="ProxyTableBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiTableBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiTableBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        ProxyTableBlock CreateProxy(BlockProcessor blockProcessor, BlockParser blockParser);
    }
}
