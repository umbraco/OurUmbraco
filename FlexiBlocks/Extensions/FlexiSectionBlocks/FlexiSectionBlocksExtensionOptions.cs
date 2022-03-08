namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiSectionBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiSectionBlocksExtensionOptions : ExtensionOptions<IFlexiSectionBlockOptions>, IFlexiSectionBlocksExtensionOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiSectionBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiSectionBlockOptions"/> for all <see cref="FlexiSectionBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiSectionBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiSectionBlocksExtensionOptions(IFlexiSectionBlockOptions defaultBlockOptions) :
            base(defaultBlockOptions ?? new FlexiSectionBlockOptions())
        {
        }

        /// <summary>
        /// Creates a <see cref="FlexiSectionBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiSectionBlocksExtensionOptions() : this(null)
        {
        }
    }
}
