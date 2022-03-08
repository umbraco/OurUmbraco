namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiTableBlock"/> options.
    /// </summary>
    public interface IFlexiTableBlockOptions : IRenderedRootBlockOptions<IFlexiTableBlockOptions>
    {
        /// <summary>
        /// Gets the <see cref="FlexiTableBlock"/>'s type.
        /// </summary>
        FlexiTableType Type { get; }
    }
}
