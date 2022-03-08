namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiAlertBlock"/> options.
    /// </summary>
    public interface IFlexiAlertBlockOptions : IRenderedRootBlockOptions<IFlexiAlertBlockOptions>
    {
        /// <summary>
        /// Gets the <see cref="FlexiAlertBlock"/>'s icon as an HTML fragment.
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// Gets the <see cref="FlexiAlertBlock"/>'s type.
        /// </summary>
        string Type { get; }
    }
}
