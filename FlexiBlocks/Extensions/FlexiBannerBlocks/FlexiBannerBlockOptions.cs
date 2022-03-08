using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiBannerBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiBannerBlockOptions"/>.</para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON in <see cref="FlexiOptionsBlock"/>s. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiBannerBlockOptions : RenderedRootBlockOptions<IFlexiBannerBlockOptions>, IFlexiBannerBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiBannerBlockOptions"/>.
        /// </summary>
        /// <param name="blockName">
        /// <para>The <see cref="FlexiBannerBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.</para>
        /// <para>In compliance with <a href="https://en.bem.info">BEM methodology</a>, this value is the <see cref="FlexiBannerBlock"/>'s root element's class as well as the prefix for all other classes in the block.</para>
        /// <para>This value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiBannerBlock"/>'s block name is "flexi-banner".</para>
        /// <para>Defaults to "flexi-banner".</para>
        /// </param>
        /// <param name="logoIcon">
        /// <para>The <see cref="FlexiBannerBlock"/>'s logo icon as an HTML fragment.</para>
        /// <para>A class attribute with value "&lt;<paramref name="blockName"/>&gt;__logo-icon" is added to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no logo icon is rendered.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="backgroundIcon">
        /// <para>The <see cref="FlexiBannerBlock"/>'s background icon as an HTML fragment.</para>
        /// <para>A class attribute with value "&lt;<paramref name="blockName"/>&gt;__background-icon" is added to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no background icon is rendered.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiBannerBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If the class attribute is specified, its value is appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiBannerBlockOptions(
            string blockName = "flexi-banner",
            string logoIcon = default,
            string backgroundIcon = default,
            IDictionary<string, string> attributes = default) : base(blockName, attributes)
        {
            LogoIcon = logoIcon;
            BackgroundIcon = backgroundIcon;
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string LogoIcon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string BackgroundIcon { get; private set; }
    }
}
