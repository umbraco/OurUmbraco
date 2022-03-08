using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction representing options for a root rendered <see cref="IBlock"/>.
    /// </summary>
    public interface IRenderedRootBlockOptions<T> : IRenderedBlockOptions<T> where T : IRenderedRootBlockOptions<T>
    {
        /// <summary>
        /// Gets the rendered <see cref="IBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.
        /// </summary>
        string BlockName { get; }
    }
}
