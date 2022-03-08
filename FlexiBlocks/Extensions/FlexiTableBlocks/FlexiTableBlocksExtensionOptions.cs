namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiTableBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiTableBlocksExtensionOptions : ExtensionOptions<IFlexiTableBlockOptions>, IFlexiTableBlocksExtensionOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiTableBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiTableBlockOptions"/> for all <see cref="FlexiTableBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiTableBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiTableBlocksExtensionOptions(IFlexiTableBlockOptions defaultBlockOptions = null) :
            base(defaultBlockOptions ?? new FlexiTableBlockOptions())
        {
        }

        /// <summary>
        /// Creates a <see cref="FlexiTableBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiTableBlocksExtensionOptions() : this(null)
        {
        }
    }
}
