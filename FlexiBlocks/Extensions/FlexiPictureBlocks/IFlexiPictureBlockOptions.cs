namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiPictureBlock"/> options.
    /// </summary>
    public interface IFlexiPictureBlockOptions : IMediaBlockOptions<IFlexiPictureBlockOptions>
    {
        /// <summary>
        /// Gets the <see cref="FlexiPictureBlock"/>'s alt text.
        /// </summary>
        string Alt { get; }

        /// <summary>
        /// Gets the value specifying whether the<see cref="FlexiPictureBlock"/> loads lazily.
        /// </summary>
        bool Lazy { get; }
    }
}
