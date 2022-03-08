namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiFigureBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiFigureBlock"/> options.
    /// </summary>
    public interface IFlexiFigureBlockOptions : IRenderedRootBlockOptions<IFlexiFigureBlockOptions>
    {
        /// <summary>
        /// Gets the value specifying whether the <see cref="FlexiFigureBlock"/> is <a href="https://spec.commonmark.org/0.28/#reference-link">reference-linkable</a>.
        /// </summary>
        bool ReferenceLinkable { get; }

        /// <summary>
        /// Gets the content of the <a href="https://spec.commonmark.org/0.28/#link-label">link label</a> for linking to the <see cref="FlexiFigureBlock"/>.
        /// </summary>
        string LinkLabelContent { get; }

        /// <summary>
        ///  Gets the value specifying whether to generate an ID for the <see cref="FlexiFigureBlock"/>.
        /// </summary>
        bool GenerateID { get; }

        /// <summary>
        /// Gets the value specifying whether to render the <see cref="FlexiFigureBlock"/>'s name.
        /// </summary>
        bool RenderName { get; }
    }
}
