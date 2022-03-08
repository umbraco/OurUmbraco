using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiTableBlockOptions"/>.</para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON in <see cref="FlexiOptionsBlock"/>s. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiTableBlockOptions : RenderedRootBlockOptions<IFlexiTableBlockOptions>, IFlexiTableBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiTableBlockOptions"/>.
        /// </summary>
        /// <param name="blockName">
        /// <para>The <see cref="FlexiTableBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.</para>
        /// <para>In compliance with <a href="https://en.bem.info">BEM methodology</a>, this value is the <see cref="FlexiTableBlock"/>'s root element's class as well as the prefix for all other classes in the block.</para>
        /// <para>This value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiTableBlock"/>'s block name is "flexi-table".</para>
        /// <para>Defaults to "flexi-table".</para>
        /// </param>
        /// <param name="type">
        /// <para>The <see cref="FlexiTableBlock"/>'s type.</para>
        /// <para>This value is used in the root element's default <a href="https://en.bem.info/methodology/quick-start/#modifier">modifier class</a>, 
        /// "&lt;<paramref name="blockName"/>&gt;_type_&lt;<paramref name="type"/>&gt;".</para>
        /// <para>This value affects the structure of generated HTML.</para>
        /// <para>Defaults to <see cref="FlexiTableType.Cards"/>.</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiTableBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If classes are specified, they are appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiTableBlockOptions(
            string blockName = "flexi-table",
            FlexiTableType type = FlexiTableType.Cards,
            IDictionary<string, string> attributes = default) : base(blockName, attributes)
        {
            Type = type;
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public FlexiTableType Type { get; private set; }
    }
}
