namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiTabsBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiTabsBlocksExtensionOptions : ExtensionOptions<IFlexiTabsBlockOptions>, IFlexiTabsBlocksExtensionOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiTabsBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiTabsBlockOptions"/> for all <see cref="FlexiTabsBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiTabsBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiTabsBlocksExtensionOptions(IFlexiTabsBlockOptions defaultBlockOptions = null) :
            base(defaultBlockOptions ?? new FlexiTabsBlockOptions())
        {
        }

        /// <summary>
        /// Creates a <see cref="FlexiTabsBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiTabsBlocksExtensionOptions() : this(null)
        {
        }
    }
}
