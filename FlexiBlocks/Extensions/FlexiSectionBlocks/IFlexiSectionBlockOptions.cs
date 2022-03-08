namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiSectionBlock"/> options.
    /// </summary>
    public interface IFlexiSectionBlockOptions : IRenderedRootBlockOptions<IFlexiSectionBlockOptions>
    {
        /// <summary>
        /// Gets the <see cref="FlexiSectionBlock"/>'s root element's type.
        /// </summary>
        SectioningContentElement Element { get; }

        /// <summary>
        /// Gets the value specifying whether to generate an ID for the <see cref="FlexiSectionBlock"/>.
        /// </summary>
        bool GenerateID { get; }

        /// <summary>
        /// Gets the <see cref="FlexiSectionBlock"/>'s link icon as an HTML fragment.
        /// </summary>
        string LinkIcon { get; }

        /// <summary>
        /// Gets the value specifying whether the <see cref="FlexiSectionBlock"/> is reference-linkable.
        /// </summary>
        bool ReferenceLinkable { get; }

        /// <summary>
        /// Gets the <see cref="FlexiSectionBlock"/>'s rendering mode.
        /// </summary>
        FlexiSectionBlockRenderingMode RenderingMode { get; }
    }
}
