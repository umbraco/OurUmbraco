using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for proxy fenced <see cref="IBlock"/>s.
    /// </summary>
    public interface IProxyFencedBlock : IProxyBlock
    {
        /// <summary>
        /// Gets the indentation of the fenced <see cref="IBlock"/>'s opening fence.
        /// </summary>
        int OpeningFenceIndent { get; }

        /// <summary>
        /// Gets the character count of the fenced <see cref="IBlock"/>'s opening fence.
        /// </summary>
        int OpeningFenceCharCount { get; }
    }
}
