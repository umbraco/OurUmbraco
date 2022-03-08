using Markdig.Syntax;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction representing options for a rendered <see cref="IBlock"/>.
    /// </summary>
    public interface IRenderedBlockOptions<T> : IBlockOptions<T> where T : IRenderedBlockOptions<T>
    {
        /// <summary>
        /// Gets the HTML attributes for the rendered <see cref="IBlock"/>'s root element.
        /// </summary>
        ReadOnlyDictionary<string, string> Attributes { get; }
    }
}
