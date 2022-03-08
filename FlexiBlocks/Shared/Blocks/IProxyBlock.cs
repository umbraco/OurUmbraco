using Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// <para>An abstraction for proxy blocks.</para>
    /// <para>We can only build some blocks after parsing. For example, we can only validate options for a <see cref="FlexiCodeBlock"/> when we know what code contains.</para>
    /// <para>To build only after parsing, we use proxy blocks to accumulate data. Proxy blocks are then passed to block factories.</para>
    /// <para>Apart from allowing us to build after parsing, proxy blocks allow for less repeated logic in blocks. For example, instead of every fenced <see cref="IBlock"/> having an <see cref="IProxyFencedBlock.OpeningFenceCharCount"/> property (redundant after building),
    /// fenced <see cref="IBlock"/>s are built using a shared proxy type with the required property.</para>
    /// </summary>
    public interface IProxyBlock : IBlock
    {
        /// <summary>
        ///  Type name of the <see cref="IBlock"/> this <see cref="IProxyBlock"/> is proxying for.
        /// </summary>
        string MainTypeName { get; }
    }
}
