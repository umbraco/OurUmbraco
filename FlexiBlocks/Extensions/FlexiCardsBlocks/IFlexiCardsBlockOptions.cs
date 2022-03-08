namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiCardsBlock"/> options.
    /// </summary>
    public interface IFlexiCardsBlockOptions : IRenderedRootBlockOptions<IFlexiCardsBlockOptions>
    {
        /// <summary>
        /// Gets the display size of contained <see cref="FlexiCardBlock"/>s.
        /// </summary>
        FlexiCardBlockSize CardSize { get; }

        /// <summary>
        /// Gets the default <see cref="IFlexiCardBlockOptions"/> for contained <see cref="FlexiCardBlock"/>s.
        /// </summary>
        IFlexiCardBlockOptions DefaultCardOptions { get; }
    }
}
