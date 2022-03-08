namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiBannerBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiBannerBlock"/> options.
    /// </summary>
    public interface IFlexiBannerBlockOptions : IRenderedRootBlockOptions<IFlexiBannerBlockOptions>
    {
        /// <summary>
        /// Gets the <see cref="FlexiBannerBlock"/>'s logo icon as an HTML fragment.
        /// </summary>
        string LogoIcon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiBannerBlock"/>'s background icon as an HTML fragment.
        /// </summary>
        string BackgroundIcon { get; }
    }
}
