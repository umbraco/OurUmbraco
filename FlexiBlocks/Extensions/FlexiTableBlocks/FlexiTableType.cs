namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// <see cref="FlexiTableBlock"/> types.
    /// </summary>
    public enum FlexiTableType
    {
        /// <summary>
        /// Each row is displayed as a card when table width is limited.
        /// </summary>
        Cards = 0,

        /// <summary>
        /// Table is inverted and titles are fixed when table width is limited.
        /// </summary>
        FixedTitles,

        /// <summary>
        /// Table layout remains the same when table width is limited.
        /// </summary>
        Unresponsive
    }
}
