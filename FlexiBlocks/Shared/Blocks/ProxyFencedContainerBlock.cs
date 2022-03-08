using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Represents a proxy fenced <see cref="ContainerBlock"/>.
    /// </summary>
    public class ProxyFencedContainerBlock : ProxyContainerBlock, IProxyFencedBlock
    {
        /// <summary>
        /// Creates a <see cref="ProxyFencedContainerBlock"/>.
        /// </summary>
        /// <param name="openingFenceIndent">The indent of the opening fence.</param>
        /// <param name="openingFenceCharCount">The number of characters in the opening fence.</param>
        /// <param name="mainTypeName">Type name of the fenced <see cref="ContainerBlock"/> the <see cref="ProxyFencedContainerBlock"/> is proxying for.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="ProxyFencedContainerBlock"/>.</param>
        public ProxyFencedContainerBlock(int openingFenceIndent, int openingFenceCharCount, string mainTypeName, BlockParser blockParser) : base(mainTypeName, blockParser)
        {
            OpeningFenceIndent = openingFenceIndent;
            OpeningFenceCharCount = openingFenceCharCount;
            IsBreakable = false; // Fenced blocks aren't breakable. They only close when we reach a closing fence or if an ancestor closes.
        }

        /// <inheritdoc />
        public int OpeningFenceIndent { get; }

        /// <inheritdoc />
        public int OpeningFenceCharCount { get; }
    }
}
