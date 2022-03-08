using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction representing an extension's options.
    /// </summary>
    public interface IExtensionOptions<T> where T : IBlockOptions<T>
    {
        /// <summary>
        /// Gets the default <see cref="IBlockOptions{T}"/> for <see cref="IBlock"/>s created by the extension.
        /// </summary>
        T DefaultBlockOptions { get; }
    }
}
