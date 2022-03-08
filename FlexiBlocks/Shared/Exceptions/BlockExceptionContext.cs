namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Represents possible contexts of a <see cref="BlockException"/>.
    /// </summary>
    public enum BlockExceptionContext
    {
        /// <summary>
        /// No context.
        /// </summary>
        None = 0,

        /// <summary>
        /// Line of offending markdown is known but block at line is unknown.
        /// </summary>
        Line,

        /// <summary>
        /// Offending block is known.
        /// </summary>
        Block
    }
}
