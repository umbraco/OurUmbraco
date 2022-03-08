namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiQuoteBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiQuoteBlock"/> options.
    /// </summary>
    public interface IFlexiQuoteBlockOptions : IRenderedRootBlockOptions<IFlexiQuoteBlockOptions>
    {
        /// <summary>
        /// Gets the <see cref="FlexiQuoteBlock"/>'s icon as an HTML fragment.
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// Gets the index of the link in the <see cref="FlexiQuoteBlock"/>'s citation that points to the work where its quote comes from.
        /// </summary>
        int CiteLink { get; }
    }
}
