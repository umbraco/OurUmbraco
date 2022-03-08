using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Represents a proxy <see cref="ContainerBlock"/>.
    /// </summary>
    public class ProxyContainerBlock : ContainerBlock, IProxyBlock
    {
        /// <summary>
        /// Creates a <see cref="ProxyContainerBlock"/>.
        /// </summary>
        /// <param name="mainTypeName">Type name of the <see cref="ContainerBlock"/> this <see cref="ProxyContainerBlock"/> is proxying for.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="ContainerBlock"/>.</param>
        public ProxyContainerBlock(string mainTypeName, BlockParser blockParser) : base(blockParser)
        {
            MainTypeName = mainTypeName;
        }

        /// <inheritdoc />
        public string MainTypeName { get; }
    }
}
