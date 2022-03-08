namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiBannerBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiBannerBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiBannerBlocksExtensionOptions : ExtensionOptions<IFlexiBannerBlockOptions>, IFlexiBannerBlocksExtensionOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiBannerBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiBannerBlockOptions"/> for all <see cref="FlexiBannerBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiBannerBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiBannerBlocksExtensionOptions(IFlexiBannerBlockOptions defaultBlockOptions = null) :
            base(defaultBlockOptions ?? new FlexiBannerBlockOptions())
        {
        }

        /// <summary>
        /// Creates a <see cref="FlexiBannerBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiBannerBlocksExtensionOptions() : this(null)
        {
        }
    }
}
