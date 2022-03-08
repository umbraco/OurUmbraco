namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// <see cref="FlexiSectionBlock"/> rendering modes.
    /// </summary>
    public enum FlexiSectionBlockRenderingMode
    {
        /// <summary>
        /// Render <see cref="FlexiSectionBlock"/>s with all features enabled, including link icon, <a href="https://en.bem.info">BEM</a> classes,
        /// and more.
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Render <see cref="FlexiSectionBlock"/>s the way <a href="https://spec.commonmark.org/0.28/#atx-headings">ATX headings</a> are 
        /// rendered in <a href="https://spec.commonmark.org/">CommonMark Spec</a> examples.
        /// </summary>
        Classic
    }
}
