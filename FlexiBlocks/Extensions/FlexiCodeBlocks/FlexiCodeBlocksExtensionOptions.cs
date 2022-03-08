namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiCodeBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiCodeBlocksExtensionOptions : ExtensionOptions<IFlexiCodeBlockOptions>, IFlexiCodeBlocksExtensionOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiCodeBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiCodeBlockOptions"/> for all <see cref="FlexiCodeBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiCodeBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiCodeBlocksExtensionOptions(IFlexiCodeBlockOptions defaultBlockOptions = null) :
            base(defaultBlockOptions ?? new FlexiCodeBlockOptions())
        {
        }

        /// <summary>
        /// Creates a <see cref="FlexiCodeBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiCodeBlocksExtensionOptions() : this(null)
        {
        }
    }
}
