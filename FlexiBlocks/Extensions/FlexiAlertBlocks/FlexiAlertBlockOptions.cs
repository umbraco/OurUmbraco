using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiAlertBlockOptions"/>.</para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON in <see cref="FlexiOptionsBlock"/>s. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiAlertBlockOptions : RenderedRootBlockOptions<IFlexiAlertBlockOptions>, IFlexiAlertBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiAlertBlockOptions"/>.
        /// </summary>
        /// <param name="blockName">
        /// <para>The <see cref="FlexiAlertBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.</para>
        /// <para>In compliance with <a href="https://en.bem.info">BEM methodology</a>, this value is the <see cref="FlexiAlertBlock"/>'s root element's class as well as the prefix for all other classes in the block.</para>
        /// <para>This value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiAlertBlock"/>'s block name is "flexi-alert".</para>
        /// <para>Defaults to "flexi-alert".</para>
        /// </param>
        /// <param name="type">
        /// <para>The <see cref="FlexiAlertBlock"/>'s type.</para>
        /// <para>This value is used in the root element's default <a href="https://en.bem.info/methodology/quick-start/#modifier">modifier class</a>, 
        /// "&lt;<paramref name="blockName"/>&gt;_type_&lt;<paramref name="type"/>&gt;".</para>
        /// <para>As such, this value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>This value is also used to retrieve an icon if <paramref name="icon"/> is <c>null</c>, whitespace or an empty string.</para>
        /// <para>Icons for custom types can be defined in <see cref="IFlexiAlertBlocksExtensionOptions.Icons"/>. The default implementation of <see cref="IFlexiAlertBlocksExtensionOptions.Icons"/>
        /// contains icons for types "info", "warning" and "critical-warning".</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiAlertBlock"/>'s type is "info".</para>
        /// <para>Defaults to "info".</para>
        /// </param>
        /// <param name="icon">
        /// <para>The <see cref="FlexiAlertBlock"/>'s icon as an HTML fragment.</para>
        /// <para>A class attribute with value "&lt;<paramref name="blockName"/>&gt;__icon" is added to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, an attempt is made to retrieve an icon for the <see cref="FlexiAlertBlock"/>'s type from 
        /// <see cref="IFlexiAlertBlocksExtensionOptions.Icons"/>, failing which no icon is rendered.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiAlertBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If the class attribute is specified, its value is appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiAlertBlockOptions(
            string blockName = "flexi-alert",
            string type = "info",
            string icon = default,
            IDictionary<string, string> attributes = default) : base(blockName, attributes)
        {
            Type = type;
            Icon = icon;
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual string Type { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual string Icon { get; private set; }
    }
}
