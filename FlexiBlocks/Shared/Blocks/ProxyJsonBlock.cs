using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Represents a proxy JSON <see cref="LeafBlock"/>.
    /// </summary>
    public class ProxyJsonBlock : ProxyLeafBlock, IProxyJsonBlock
    {
        /// <summary>
        /// Creates a <see cref="ProxyJsonBlock"/>.
        /// </summary>
        /// <param name="mainTypeName">Type name of the JSON <see cref="LeafBlock"/> this <see cref="ProxyJsonBlock"/> is proxying for.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the JSON <see cref="LeafBlock"/>.</param>
        public ProxyJsonBlock(string mainTypeName, BlockParser blockParser) : base(mainTypeName, blockParser)
        {
        }

        /// <inheritdoc />
        public int NumOpenObjects { get; set; }

        /// <inheritdoc />
        public bool WithinString { get; set; }
    }
}
