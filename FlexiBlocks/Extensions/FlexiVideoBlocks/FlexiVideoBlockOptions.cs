using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiVideoBlockOptions"/></para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiVideoBlockOptions : RenderedRootBlockOptions<IFlexiVideoBlockOptions>, IFlexiVideoBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiVideoBlockOptions"/>. 
        /// </summary>
        /// <param name="blockName">
        /// <para>The <see cref="FlexiVideoBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.</para>
        /// <para>In compliance with <a href="https://en.bem.info">BEM methodology</a>, this value is the <see cref="FlexiVideoBlock"/>'s root element's class as well as the prefix for all other classes in the block.</para>
        /// <para>This value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiVideoBlock"/>'s block name is "flexi-video".</para>
        /// <para>Defaults to "flexi-video".</para>
        /// </param>
        /// <param name="src">
        /// <para>The <see cref="FlexiVideoBlock"/>'s source URI.</para>
        /// <para>All <see cref="FlexiVideoBlock"/>s are loaded lazily. Therefore, this value is assigned to the source element's data-src attribute.
        /// On the client, the data-src attribute's value is copied to the src attribute when the <see cref="FlexiVideoBlock"/> is almost visible
        /// and loading of the <see cref="FlexiVideoBlock"/> is started.</para>
        /// <para>Benefits of lazy loading include <a href="https://developers.google.com/web/fundamentals/performance/lazy-loading-guidance/images-and-video#why_lazy_load_images_or_video_instead_of_just_loading_them">
        /// reducing initial page load time, initial page weight, and system resource usage</a>.</para>
        /// <para><a href="https://web.dev/native-lazy-loading">Chrome recently implemented native lazy loading</a>, unfortunately native lazy loading isn't widely supported across browsers, so we don't support native lazy loading yet.</para>
        /// <para>This value is required and must be a valid URI pointing to a file.</para>
        /// </param>
        /// <param name="type">
        /// <para>The <see cref="FlexiVideoBlock"/>'s MIME type.</para>
        /// <para>This value is assigned to the source element's type attribute.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, an attempt is made to retrieve a MIME type from 
        /// <see cref="IFlexiVideoBlocksExtensionOptions.MimeTypes"/> using <paramref name="src"/>'s file extension, failing which the type attribute is not rendered.</para>
        /// <para>MIME types for file extensions can be specified in <see cref="IFlexiVideoBlocksExtensionOptions.MimeTypes"/>. The default implementation of <see cref="IFlexiVideoBlocksExtensionOptions.MimeTypes"/>
        /// contains MIME types for ".mp4", ".webm" and ".ogg".</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="width">
        /// <para>The <see cref="FlexiVideoBlock"/>'s width.</para>
        /// <para>If this value is larger than 0, it is assigned to width style properties of several elements.</para>
        /// <para>If this value and <paramref name="height"/> are both larger than 0, they're used to calculate the <see cref="FlexiVideoBlock"/>'s aspect ratio,
        /// which is assigned to a padding-bottom style property.</para>
        /// <para>The width and padding-bottom style properties <a href="https://www.voorhoede.nl/en/blog/say-no-to-image-reflow/">ensure that there is no reflow on video element load</a>.</para>
        /// <para>The <a href="https://github.com/WICG/intrinsicsize-attribute/issues/16">CSS Working Group</a> have proposed a solution to content reflow on video element loads. 
        /// Unfortunately, the solution isn't widely supported, so we do not support it yet.</para>
        /// <para>If this value is larger than 0, it takes precedence over any width retrieved by file operations.</para>
        /// <para>Defaults to 0.</para>
        /// </param>
        /// <param name="height">
        /// <para>The <see cref="FlexiVideoBlock"/>'s height.</para>
        /// <para>If this value and <paramref name="width"/> are both larger than 0, they're used to calculate the <see cref="FlexiVideoBlock"/>'s aspect ratio,
        /// which is assigned to a padding-bottom style property.</para>
        /// <para>The padding-bottom style property <a href="https://www.voorhoede.nl/en/blog/say-no-to-image-reflow/">helps ensure that there is no reflow
        /// on video element load</a>.</para>
        /// <para>The <a href="https://github.com/WICG/intrinsicsize-attribute/issues/16">CSS Working Group</a> have proposed a solution to content reflow on video element loads. 
        /// Unfortunately, the solution isn't widely supported, so we do not support it yet.</para>
        /// <para>If this value is larger than 0, it takes precedence over any height retrieved by file operations.</para>
        /// <para>Defaults to 0.</para>
        /// </param>
        /// <param name="duration">
        /// <para>The <see cref="FlexiVideoBlock"/>'s duration.</para>
        /// <para>If this value is larger than 0, it is rendered in a span next to the <see cref="FlexiVideoBlock"/>'s progress bar.
        /// Prefilling the duration span allows end users to know the video's length before it loads.</para>
        /// <para>If this value is less than or equal to 0, the duration span's content is "0:00". On the client, once the browser
        /// knows how long the video is, the duration span's contents are updated.</para>
        /// <para>If this value is larger than 0, it takes precedence over any duration retrieved by file operations.</para>
        /// <para>Defaults to 0.</para>
        /// </param>
        /// <param name="generatePoster">
        /// <para>The value specifying whether to generate a poster for the <see cref="FlexiVideoBlock"/>.</para>
        /// <para>If this value and <paramref name="enableFileOperations"/> are true and <see cref="IFlexiVideoBlocksExtensionOptions.LocalMediaDirectory"/> is not null,
        /// whitespace or an empty string, the video's first frame is extracted for use as a poster.</para>
        /// <para>The generated poster is named "&lt;video file name less extension&gt;__poster.png" and placed in the same directory as the video file.</para>
        /// <para>"&lt;<paramref name="src"/> less extension&gt;__poster.png" is assigned to the video element's poster attribute.</para>
        /// <para>A poster allows end users to know what the video is about before it loads.</para>
        /// <para>Therefore, this value should be true if the <see cref="FlexiVideoBlock"/> is immediately visible on page load (above-the-fold) but doesn't 
        /// have a custom poster specified using <paramref name="poster"/>.
        /// Otherwise, this value should be false.</para>
        /// <para>Poster generation requires <a href="https://www.ffmpeg.org/">FFmpeg</a> to be installed and on the path environment variable.</para>
        /// <para>Defaults to <c>false</c>.</para>
        /// </param>
        /// <param name="poster">
        /// <para>The <see cref="FlexiVideoBlock"/>'s poster URI.</para>
        /// <para>If this value is not <c>null</c>, whitespace or an empty string, it is assigned to the video element's poster attribute.</para>
        /// <para>This value takes precedence over any generated poster.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="spinner">
        /// <para>The <see cref="FlexiVideoBlock"/>'s spinner as an HTML fragment.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__spinner" is assigned to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no spinner is rendered.</para>
        /// <para>Defaults to a simple spinner.</para>
        /// </param>
        /// <param name="playIcon">
        /// <para>The <see cref="FlexiVideoBlock"/>'s play icon as an HTML fragment.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__play-icon" is assigned to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no play icon is rendered.</para>
        /// <para>Defaults to the <a href="https://material.io/tools/icons/?icon=play_arrow&amp;style=baseline">Material Design play arrow icon</a>.</para>
        /// </param>
        /// <param name="pauseIcon">
        /// <para>The <see cref="FlexiVideoBlock"/>'s pause icon as an HTML fragment.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__pause-icon" is assigned to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no pause icon is rendered.</para>
        /// <para>Defaults to a pause icon.</para>
        /// </param>
        /// <param name="fullscreenIcon">
        /// <para>The <see cref="FlexiVideoBlock"/>'s fullscreen icon as an HTML fragment.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__fullscreen-icon" is assigned to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no fullscreen icon is rendered.</para>
        /// <para>Defaults to a fullscreen icon.</para>
        /// </param>
        /// <param name="exitFullscreenIcon">
        /// <para>The <see cref="FlexiVideoBlock"/>'s exit fullscreen icon as an HTML fragment.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__exit-fullscreen-icon" is assigned to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no error icon is rendered.</para>
        /// <para>Defaults to an exit fullscreen icon.</para>
        /// </param>
        /// <param name="errorIcon">
        /// <para>The <see cref="FlexiVideoBlock"/>'s error icon as an HTML fragment.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__error-icon" is assigned to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no error icon is rendered.</para>
        /// <para>Defaults to the <a href="https://material.io/tools/icons/?icon=error&amp;style=baseline">Material Design error icon</a>.</para>
        /// </param>
        /// <param name="enableFileOperations">
        /// <para>The value specifying whether file operations are enabled for the <see cref="FlexiVideoBlock"/>.</para>
        /// <para>If this value is <c>true</c> and
        /// <see cref="IFlexiVideoBlocksExtensionOptions.LocalMediaDirectory"/> is not <c>null</c>, whitespace or an empty string and 
        /// <paramref name="width"/>, <paramref name="height"/> or <paramref name="duration"/> is less than or equal to 0 or we need to generate a poster,
        /// <see cref="IFlexiVideoBlocksExtensionOptions.LocalMediaDirectory"/> is searched recursively for a file with <paramref name="src"/>'s file name,
        /// and the necessary file operations are performed on the file.</para>
        /// <para>Defaults to true.</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiVideoBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If classes are specified, they are appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiVideoBlockOptions(
            string blockName = "flexi-video",
            string src = default,
            string type = default,
            double width = default,
            double height = default,
            double duration = default,
            bool generatePoster = false,
            string poster = default,
            string spinner = CustomMarkup.CUSTOM_SPINNER,
            string playIcon = MaterialDesignIcons.MATERIAL_DESIGN_PLAY_ARROW,
            string pauseIcon = CustomIcons.CUSTOM_PAUSE,
            string fullscreenIcon = CustomIcons.CUSTOM_FULLSCREEN,
            string exitFullscreenIcon = CustomIcons.CUSTOM_FULLSCREEN_EXIT,
            string errorIcon = MaterialDesignIcons.MATERIAL_DESIGN_ERROR,
            bool enableFileOperations = true,
            IDictionary<string, string> attributes = default) : base(blockName, attributes)
        {
            Src = src;
            Type = type;
            Width = width;
            Height = height;
            Duration = duration;
            GeneratePoster = generatePoster;
            Poster = poster;
            Spinner = spinner;
            PlayIcon = playIcon;
            PauseIcon = pauseIcon;
            FullscreenIcon = fullscreenIcon;
            ExitFullscreenIcon = exitFullscreenIcon;
            ErrorIcon = errorIcon;
            EnableFileOperations = enableFileOperations;
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Src { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Type { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public double Width { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public double Height { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public double Duration { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool GeneratePoster { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Poster { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Spinner { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string PlayIcon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string PauseIcon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string FullscreenIcon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string ExitFullscreenIcon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string ErrorIcon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool EnableFileOperations { get; private set; }
    }
}
