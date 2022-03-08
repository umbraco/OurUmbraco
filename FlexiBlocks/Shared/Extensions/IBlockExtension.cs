using Markdig;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for <see cref="IBlock"/> extensions.
    /// </summary>
    public interface IBlockExtension<T> : IMarkdownExtension where T : MarkdownObject, IBlock
    {
    }
}
