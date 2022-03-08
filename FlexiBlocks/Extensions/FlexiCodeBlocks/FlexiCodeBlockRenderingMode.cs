namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// <see cref="FlexiCodeBlock"/> rendering modes.
    /// </summary>
    public enum FlexiCodeBlockRenderingMode
    {
        /// <summary>
        /// Render <see cref="FlexiCodeBlock"/>s with all features enabled, including icons, <a href="https://en.bem.info">BEM</a> classes, 
        /// line numbers, syntax highlighting and more.
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Render <see cref="FlexiCodeBlock"/>s the way <a href="https://spec.commonmark.org/0.28/#fenced-code-blocks">code blocks</a> are 
        /// rendered in <a href="https://spec.commonmark.org/">CommonMark Spec</a> examples, minus language classes.
        /// </summary>
        Classic
    }
}
