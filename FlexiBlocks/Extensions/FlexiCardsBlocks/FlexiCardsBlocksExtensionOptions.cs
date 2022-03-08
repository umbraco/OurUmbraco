namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiCardsBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiCardsBlocksExtensionOptions : ExtensionOptions<IFlexiCardsBlockOptions>, IFlexiCardsBlocksExtensionOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiCardsBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiCardsBlockOptions"/> for all <see cref="FlexiCardsBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiCardsBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiCardsBlocksExtensionOptions(IFlexiCardsBlockOptions defaultBlockOptions = null) :
            base(defaultBlockOptions ?? new FlexiCardsBlockOptions())
        {
        }

        /// <summary>
        /// Creates a <see cref="FlexiCardsBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiCardsBlocksExtensionOptions() : this(null)
        {
        }
    }
}
