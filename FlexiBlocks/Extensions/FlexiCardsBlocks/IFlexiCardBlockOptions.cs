namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiCardsBlock"/> options.
    /// </summary>
    public interface IFlexiCardBlockOptions : IRenderedBlockOptions<IFlexiCardBlockOptions>
    {
        /// <summary>
        /// Gets the URL the <see cref="FlexiCardBlock"/> points to.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Gets the <see cref="FlexiCardBlock"/>'s background icon as an HTML fragment.
        /// </summary>
        string BackgroundIcon { get; }
    }
}
