using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiVideoBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiVideoBlocksExtensionOptions : ExtensionOptions<IFlexiVideoBlockOptions>, IFlexiVideoBlocksExtensionOptions
    {
        private static readonly ReadOnlyDictionary<string, string> _defaultMimeTypes = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".mp4", "video/mp4" },
                { ".webm", "video/webm"},
                { ".ogg", "video/ogg"}
            }
        );

        /// <summary>
        /// Creates a <see cref="FlexiVideoBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiVideoBlockOptions"/> for all <see cref="FlexiVideoBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiVideoBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="localMediaDirectory">
        /// <para>The local directory to search for video files in.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, file operations are disabled for all <see cref="FlexiVideoBlock"/>s.</para>
        /// <para>This value must be an absolute URI with the file scheme (points to a local directory).</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="mimeTypes">
        /// <para>A map of MIME types to file extensions.</para>
        /// <para>If this value is <c>null</c>, a map of MIME types for file extensions ".mp4", ".webm" and ".ogg" is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiVideoBlocksExtensionOptions(IFlexiVideoBlockOptions defaultBlockOptions = null,
            string localMediaDirectory = null,
            IDictionary<string, string> mimeTypes = null) :
            base(defaultBlockOptions ?? new FlexiVideoBlockOptions())
        {
            LocalMediaDirectory = localMediaDirectory;
            MimeTypes = mimeTypes is ReadOnlyDictionary<string, string> ? mimeTypes as ReadOnlyDictionary<string, string> :
                mimeTypes != null ? new ReadOnlyDictionary<string, string>(mimeTypes) :
                _defaultMimeTypes;
        }

        /// <summary>
        /// Creates a <see cref="FlexiVideoBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiVideoBlocksExtensionOptions() : this(null, null)
        {
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string LocalMediaDirectory { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public ReadOnlyDictionary<string, string> MimeTypes { get; private set; }
    }
}
