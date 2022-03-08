using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Represents a proxy <see cref="LeafBlock"/>.
    /// </summary>
    public class ProxyLeafBlock : LeafBlock, IProxyBlock
    {
        /// <summary>
        /// Creates a <see cref="ProxyLeafBlock"/>.
        /// </summary>
        /// <param name="mainTypeName">Type name of the <see cref="LeafBlock"/> this <see cref="ProxyLeafBlock"/> is proxying for.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="LeafBlock"/>.</param>
        public ProxyLeafBlock(string mainTypeName, BlockParser blockParser) : base(blockParser)
        {
            MainTypeName = mainTypeName;
        }

        /// <inheritdoc />
        public string MainTypeName { get; }
    }
}
