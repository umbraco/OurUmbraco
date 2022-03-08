using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Represents a proxy fenced <see cref="LeafBlock"/>.
    /// </summary>
    public class ProxyFencedLeafBlock : ProxyLeafBlock, IProxyFencedBlock
    {
        /// <summary>
        /// Creates a <see cref="ProxyFencedLeafBlock"/>.
        /// </summary>
        /// <param name="openingFenceIndent">The indent of the opening fence.</param>
        /// <param name="openingFenceCharCount">The number of characters in the opening fence.</param>
        /// <param name="mainTypeName">Type name of the fenced <see cref="LeafBlock"/> the <see cref="ProxyFencedLeafBlock"/> is proxying for.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="ProxyFencedLeafBlock"/>.</param>
        public ProxyFencedLeafBlock(int openingFenceIndent, int openingFenceCharCount, string mainTypeName, BlockParser blockParser) : base(mainTypeName, blockParser)
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
