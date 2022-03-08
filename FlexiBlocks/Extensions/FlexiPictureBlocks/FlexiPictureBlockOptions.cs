using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiPictureBlockOptions"/></para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiPictureBlockOptions : RenderedRootBlockOptions<IFlexiPictureBlockOptions>, IFlexiPictureBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiPictureBlockOptions"/>. 
        /// </summary>
        /// <param name="blockName">
        /// <para>The <see cref="FlexiPictureBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.</para>
        /// <para>In compliance with <a href="https://en.bem.info">BEM methodology</a>, this value is the <see cref="FlexiPictureBlock"/>'s root element's class as well as the prefix for all other classes in the block.</para>
        /// <para>This value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiPictureBlock"/>'s block name is "flexi-picture".</para>
        /// <para>Defaults to "flexi-picture".</para>
        /// </param>
        /// <param name="src">
        /// <para>The <see cref="FlexiPictureBlock"/>'s source URI.</para>
        /// <para>This value is assigned to the img element's data-src attribute if <paramref name="lazy"/> is true, otherwise it is assigned to the img element's src attribute.</para>
        /// <para>This value is required and must be a valid URI pointing to a file.</para>
        /// </param>
        /// <param name="alt">
        /// <para>The <see cref="FlexiPictureBlock"/>'s alt text.</para>
        /// <para>This value is assigned to the img element's alt attribute.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the alt attribute is not rendered.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="lazy">
        /// <para>The value specifying whether the <see cref="FlexiPictureBlock"/> loads lazily.</para>
        /// <para>If this value is <c>true</c>, <paramref name="src"/> is assigned to the img element's data-src attribute. 
        /// On the client, the data-src attribute's value is copied to the src attribute when the <see cref="FlexiPictureBlock"/> is almost visible.
        /// Browsers automatically begin loading the img element once its src attribute is set.</para>
        /// <para>If this value is <c>false</c>, <paramref name="src"/> is assigned to the img element's src attribute.</para>
        /// <para>This value should be false if the <see cref="FlexiPictureBlock"/>s is immediately visible on page load (above-the-fold) and true otherwise.</para>
        /// <para>Benefits of lazy loading include <a href="https://developers.google.com/web/fundamentals/performance/lazy-loading-guidance/images-and-video#why_lazy_load_images_or_video_instead_of_just_loading_them">
        /// reducing initial page load time, initial page weight, and system resource usage</a>.</para>
        /// <para><a href="https://web.dev/native-lazy-loading">Chrome recently implemented native lazy loading</a>, unfortunately native lazy loading isn't widely supported across browsers, so we don't support native lazy loading yet.</para>
        /// <para>Defaults to <c>true</c>.</para>
        /// </param>
        /// <param name="width">
        /// <para>The <see cref="FlexiPictureBlock"/>'s width.</para>
        /// <para>If this value is larger than 0, it is assigned to width style properties of several elements.</para>
        /// <para>If this value and <paramref name="height"/> are both larger than 0, they're used to calculate the <see cref="FlexiPictureBlock"/>'s aspect ratio,
        /// which is assigned to a padding-bottom style property.</para>
        /// <para>The width and padding-bottom style properties <a href="https://www.voorhoede.nl/en/blog/say-no-to-image-reflow/">ensure that there is no reflow on img element load</a>.</para>
        /// <para>The <a href="https://github.com/WICG/intrinsicsize-attribute/issues/16">CSS Working Group</a> have proposed a solution to content reflow on img element loads. 
        /// Unfortunately, the solution isn't widely supported, so we do not support it yet.</para>
        /// <para>If this value is larger than 0, it takes precedence over any width retrieved by file operations.</para>
        /// <para>Defaults to 0.</para>
        /// </param>
        /// <param name="height">
        /// <para>The <see cref="FlexiPictureBlock"/>'s height.</para>
        /// <para>If this value and <paramref name="width"/> are both larger than 0, they're used to calculate the <see cref="FlexiPictureBlock"/>'s aspect ratio,
        /// which is assigned to a padding-bottom style property.</para>
        /// <para>The padding-bottom style property <a href="https://www.voorhoede.nl/en/blog/say-no-to-image-reflow/">helps ensure that there is no reflow
        /// on img element load</a>.</para>
        /// <para>The <a href="https://github.com/WICG/intrinsicsize-attribute/issues/16">CSS Working Group</a> have proposed a solution to content reflow on img element loads. 
        /// Unfortunately, the solution isn't widely supported, so we do not support it yet.</para>
        /// <para>If this value is larger than 0, it takes precedence over any height retrieved by file operations.</para>
        /// <para>Defaults to 0.</para>
        /// </param>
        /// <param name="exitFullscreenIcon">
        /// <para>The <see cref="FlexiPictureBlock"/>'s exit fullscreen icon as an HTML fragment.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__exit-fullscreen-icon" is assigned to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no exit fullscreen icon is rendered.</para>
        /// <para>Defaults to the <a href="https://material.io/tools/icons/?icon=clear&amp;style=baseline">Material Design clear icon</a>.</para>
        /// </param>
        /// <param name="errorIcon">
        /// <para>The <see cref="FlexiPictureBlock"/>'s error icon as an HTML fragment.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__error-icon" is assigned to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no error icon is rendered.</para>
        /// <para>Defaults to the <a href="https://material.io/tools/icons/?icon=error&amp;style=baseline">Material Design error icon</a>.</para>
        /// </param>
        /// <param name="spinner">
        /// <para>The <see cref="FlexiPictureBlock"/>'s spinner as an HTML fragment.</para>
        /// <para>The class "&lt;<paramref name="blockName"/>&gt;__spinner" is assigned to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no spinner is rendered.</para>
        /// <para>Defaults to a simple spinner.</para>
        /// </param>
        /// <param name="enableFileOperations">
        /// <para>The value specifying whether file operations are enabled for the <see cref="FlexiPictureBlock"/>.</para>
        /// <para>If this value is <c>true</c> and
        /// <see cref="IFlexiPictureBlocksExtensionOptions.LocalMediaDirectory"/> is not <c>null</c>, whitespace or an empty string and 
        /// either <paramref name="width"/> or <paramref name="height"/> is less than or equal to 0,
        /// <see cref="IFlexiPictureBlocksExtensionOptions.LocalMediaDirectory"/> is searched recursively for a file with <paramref name="src"/>'s file name,
        /// and the necessary file operations are performed on the file.</para>
        /// <para>Defaults to true.</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiPictureBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If classes are specified, they are appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiPictureBlockOptions(
            string blockName = "flexi-picture",
            string src = default,
            string alt = default,
            bool lazy = true,
            double width = default,
            double height = default,
            string exitFullscreenIcon = MaterialDesignIcons.MATERIAL_DESIGN_CLEAR,
            string errorIcon = MaterialDesignIcons.MATERIAL_DESIGN_ERROR,
            string spinner = CustomMarkup.CUSTOM_SPINNER,
            bool enableFileOperations = true,
            IDictionary<string, string> attributes = default) : base(blockName, attributes)
        {
            Src = src;
            Alt = alt;
            Lazy = lazy;
            Width = width;
            Height = height;
            ExitFullscreenIcon = exitFullscreenIcon;
            ErrorIcon = errorIcon;
            Spinner = spinner;
            EnableFileOperations = enableFileOperations;
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Src { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Alt { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Lazy { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public double Width { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public double Height { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string ExitFullscreenIcon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string ErrorIcon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Spinner { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool EnableFileOperations { get; private set; }
    }
}
