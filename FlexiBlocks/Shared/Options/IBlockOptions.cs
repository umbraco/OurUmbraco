using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction representing options for an <see cref="IBlock"/>.
    /// </summary>
    public interface IBlockOptions<T> where T : IBlockOptions<T>
    {
        /// <summary>
        /// Returns a shallow clone.
        /// </summary>
        T Clone();
    }
}
