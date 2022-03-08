using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiTabsBlockOptions"/>.</para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON in <see cref="FlexiOptionsBlock"/>s. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiTabsBlockOptions : RenderedRootBlockOptions<IFlexiTabsBlockOptions>, IFlexiTabsBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiTabsBlockOptions"/>.
        /// </summary>
        /// <param name="blockName">
        /// <para>The <see cref="FlexiTabsBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.</para>
        /// <para>In compliance with <a href="https://en.bem.info">BEM methodology</a>, this value is the <see cref="FlexiTabsBlock"/>'s root element's class as well as the prefix for all other classes in the block.</para>
        /// <para>This value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiTabsBlock"/>'s block name is "flexi-tabs".</para>
        /// <para>Defaults to "flexi-tabs".</para>
        /// </param>
        /// <param name="defaultTabOptions">
        /// <para>The default <see cref="IFlexiTabBlockOptions"/> for contained <see cref="FlexiTabBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiTabBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiTabsBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If the class attribute is specified, its value is appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiTabsBlockOptions(
            string blockName = "flexi-tabs",
            IFlexiTabBlockOptions defaultTabOptions = default,
            IDictionary<string, string> attributes = default) : base(blockName, attributes)
        {
            DefaultTabOptions = defaultTabOptions ?? new FlexiTabBlockOptions();
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public IFlexiTabBlockOptions DefaultTabOptions { get; private set; }

        /// <summary>
        /// Returns a shallow clone with a shallow clone of <see cref="DefaultTabOptions"/>.
        /// </summary>
        public override IFlexiTabsBlockOptions Clone()
        {
            // The default Clone method creates a shallow clone. Shallow clones are fine for options types with only value type properties.
            // DefaultTabOptions is a reference type property, so we have to manually clone it.
            return new FlexiTabsBlockOptions(BlockName, DefaultTabOptions.Clone(), Attributes);
        }
    }
}
