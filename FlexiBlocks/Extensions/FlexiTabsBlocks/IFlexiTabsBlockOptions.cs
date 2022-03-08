namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiTabsBlock"/> options.
    /// </summary>
    public interface IFlexiTabsBlockOptions : IRenderedRootBlockOptions<IFlexiTabsBlockOptions>
    {
        /// <summary>
        /// Gets the default <see cref="IFlexiTabBlockOptions"/> for contained <see cref="FlexiTabBlock"/>s.
        /// </summary>
        IFlexiTabBlockOptions DefaultTabOptions { get; }
    }
}
