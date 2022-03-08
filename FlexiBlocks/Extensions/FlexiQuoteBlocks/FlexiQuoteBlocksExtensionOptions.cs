namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiQuoteBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiQuoteBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiQuoteBlocksExtensionOptions : ExtensionOptions<IFlexiQuoteBlockOptions>, IFlexiQuoteBlocksExtensionOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiQuoteBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiQuoteBlockOptions"/> for all <see cref="FlexiQuoteBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiQuoteBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiQuoteBlocksExtensionOptions(IFlexiQuoteBlockOptions defaultBlockOptions = null) :
            base(defaultBlockOptions ?? new FlexiQuoteBlockOptions())
        {
        }

        /// <summary>
        /// Creates a <see cref="FlexiQuoteBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiQuoteBlocksExtensionOptions() : this(null)
        {
        }
    }
}
