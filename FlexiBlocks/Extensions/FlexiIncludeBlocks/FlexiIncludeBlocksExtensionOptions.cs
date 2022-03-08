using Newtonsoft.Json;
using System.IO;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiIncludeBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiIncludeBlocksExtensionOptions : ExtensionOptions<IFlexiIncludeBlockOptions>, IFlexiIncludeBlocksExtensionOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiIncludeBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiIncludeBlockOptions"/> for all <see cref="FlexiIncludeBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiIncludeBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="baseUri">
        /// <para>The base URI for <see cref="FlexiIncludeBlock"/>s in root content.</para>
        /// <para>If this value is <c>null</c>, the application's working directory is used as the base URI.
        /// Note that the application's working directory is what <see cref="Directory.GetCurrentDirectory"/> returns.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiIncludeBlocksExtensionOptions(IFlexiIncludeBlockOptions defaultBlockOptions = null, string baseUri = null) :
            base(defaultBlockOptions ?? new FlexiIncludeBlockOptions())
        {
            BaseUri = baseUri;
        }

        /// <summary>
        /// Creates a <see cref="FlexiIncludeBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiIncludeBlocksExtensionOptions() : this(null, null)
        {
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string BaseUri { get; private set; }
    }
}
