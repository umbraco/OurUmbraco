namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiFigureBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiFigureBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiFigureBlocksExtensionOptions : ExtensionOptions<IFlexiFigureBlockOptions>, IFlexiFigureBlocksExtensionOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiFigureBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiFigureBlockOptions"/> for all <see cref="FlexiFigureBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiFigureBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiFigureBlocksExtensionOptions(IFlexiFigureBlockOptions defaultBlockOptions = null) :
            base(defaultBlockOptions ?? new FlexiFigureBlockOptions())
        {
        }

        /// <summary>
        /// Creates a <see cref="FlexiFigureBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiFigureBlocksExtensionOptions() : this(null)
        {
        }
    }
}
