using Newtonsoft.Json;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiPictureBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiPictureBlocksExtensionOptions : ExtensionOptions<IFlexiPictureBlockOptions>, IFlexiPictureBlocksExtensionOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiPictureBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiPictureBlockOptions"/> for all <see cref="FlexiPictureBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiPictureBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="localMediaDirectory">
        /// <para>The local directory to search for image files in.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, file operations are disabled for all <see cref="FlexiPictureBlock"/>s.</para>
        /// <para>This value must be an absolute URI with the file scheme (points to a local directory).</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiPictureBlocksExtensionOptions(IFlexiPictureBlockOptions defaultBlockOptions = null, string localMediaDirectory = null) :
            base(defaultBlockOptions ?? new FlexiPictureBlockOptions())
        {
            LocalMediaDirectory = localMediaDirectory;
        }

        /// <summary>
        /// Creates a <see cref="FlexiPictureBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiPictureBlocksExtensionOptions() : this(null, null)
        {
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string LocalMediaDirectory { get; private set; }
    }
}
