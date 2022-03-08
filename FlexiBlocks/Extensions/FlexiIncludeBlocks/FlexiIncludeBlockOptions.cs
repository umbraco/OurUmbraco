using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiIncludeBlockOptions"/></para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiIncludeBlockOptions : BlockOptions<IFlexiIncludeBlockOptions>, IFlexiIncludeBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiIncludeBlockOptions"/>. 
        /// </summary>
        /// <param name="source">
        /// <para>The <see cref="FlexiIncludeBlock"/>'s source.</para>
        /// <para>This value must either be a relative URI or an absolute URI with scheme file, http or https.</para>
        /// <para>If this value is a relative URI and the <see cref="FlexiIncludeBlock"/> is in root content, <see cref="IFlexiIncludeBlocksExtensionOptions.BaseUri"/>
        /// is used as the base URI.</para>
        /// <para>If this value is a relative URI and the <see cref="FlexiIncludeBlock"/> is in non-root content, the absolute URI of its containing source is used as the base URI.</para>
        /// <para>For example, consider standard Markdig usage: <c>string html = Markdown.ToHtml(rootContent, yourMarkdownPipeline);</c>.</para>
        /// <para>To Markdig, root content has no associated source, it is just a string containing markup.
        /// To work around this limitation, if the root content contains a <see cref="FlexiIncludeBlock"/> with a relative URI source like "../my/path/file1.md", <see cref="FlexiIncludeBlocksExtension"/> 
        /// uses <see cref="IFlexiIncludeBlocksExtensionOptions.BaseUri"/> to resolve the absolute URI of "file1.md".</para>
        /// <para>As such, <see cref="IFlexiIncludeBlocksExtensionOptions.BaseUri"/> is typically configured as the absolute URI of the root source.</para>
        /// <para>If "file1.md" contains a FlexiIncludeBlock with source "../my/path/file2.md", we use the previously resolved absolute URI of "file1.md" as the base URI to
        /// resolve the absolute URI of "file2.md".</para>
        /// <para>Note that retrieving content from remote sources can introduce security issues. As far as possible, retrieve remote content only from trusted or permanent links. For example,
        /// from <a href="https://help.github.com/articles/getting-permanent-links-to-files/">Github permalink</a>s. Additionally, consider sanitizing generated HTML.</para>
        /// <para>Defaults to <see cref="string.Empty"/>.</para>
        /// </param>
        /// <param name="clippings">
        /// <para>The <see cref="Clipping"/>s specifying content from the source to include.</para>
        /// <para>If this value is <c>null</c> or empty, all content from the source is included.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="type">
        /// <para>The <see cref="FlexiIncludeBlock"/>'s type.</para>
        /// <para>If this value is <see cref="FlexiIncludeType.Code"/>, the included content is rendered in a code block. 
        /// If this value is <see cref="FlexiIncludeType.Markdown"/>, the included content is processed as markdown.</para>
        /// <para>Defaults to <see cref="FlexiIncludeType.Code"/>.</para>
        /// </param>
        /// <param name="cache">
        /// <para>The value specifying whether to cache the <see cref="FlexiIncludeBlock"/>'s content on disk.</para>
        /// <para>If this value is true and the <see cref="FlexiIncludeBlock"/>'s source is remote, the source's content is cached on disk.</para>
        /// <para>Caching-on-disk slows down the first markdown-to-HTML run on a system, but significantly speeds up subsequent runs:</para>
        /// <para>If on-disk caching is enabled and content from remote source "x" is included, on the first run, all content in "x" is retrieved from a server and
        /// cached in memory as well as on disk.</para>
        /// <para>Subsequent requests to retrieve content from "x" during the same run will retrieve content from the in-memory cache.</para>
        /// <para>At the end of the first run, the in-memory cache is discarded when the process dies.</para>
        /// <para>For subsequent runs on the system, if content from "x" is included again, all content from "x" is retrieved from the on-disk cache, avoiding 
        /// round trips to a server.</para>
        /// <para>If you are only going to execute one run on a system (e.g when doing CI/CD), the run will take less time if on-disk caching is disabled.
        /// If you are doing multiple runs on a system, on-disk caching should be enabled.</para>
        /// <para>Defaults to <c>true</c>.</para>
        /// </param>
        /// <param name="cacheDirectory">
        /// <para>The directory to cache the <see cref="FlexiIncludeBlock"/>'s content in.</para>
        /// <para>This option is only relevant if caching on disk is enabled.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, a folder named "ContentCache" in the application's working directory is used instead.
        /// Note that the application's working directory is what <see cref="Directory.GetCurrentDirectory"/> returns.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiIncludeBlockOptions(string source = "",
            IList<Clipping> clippings = default,
            FlexiIncludeType type = FlexiIncludeType.Code,
            bool cache = true,
            string cacheDirectory = default)
        {
            Source = source;
            Clippings = clippings == null ? null :
                clippings is ReadOnlyCollection<Clipping> clippingsAsReadOnlyCollection ? clippingsAsReadOnlyCollection :
                new ReadOnlyCollection<Clipping>(clippings);
            Type = type;
            Cache = cache;
            CacheDirectory = cacheDirectory;
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Source { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public ReadOnlyCollection<Clipping> Clippings { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public FlexiIncludeType Type { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Cache { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string CacheDirectory { get; private set; }
    }
}
