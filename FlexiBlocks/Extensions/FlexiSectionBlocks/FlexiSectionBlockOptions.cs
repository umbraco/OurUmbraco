using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiSectionBlockOptions"/>.</para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON in <see cref="FlexiOptionsBlock"/>s. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiSectionBlockOptions : RenderedRootBlockOptions<IFlexiSectionBlockOptions>, IFlexiSectionBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiSectionBlockOptions"/>.
        /// </summary>
        /// <param name="blockName">
        /// <para>The <see cref="FlexiSectionBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.</para>
        /// <para>In compliance with <a href="https://en.bem.info">BEM methodology</a>, this value is the <see cref="FlexiSectionBlock"/>'s root element's class as well as the prefix for all other classes in the block.</para>
        /// <para>This value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiSectionBlock"/>'s block name is "flexi-section".</para>
        /// <para>Defaults to "flexi-section".</para>
        /// </param>
        /// <param name="element">
        /// <para>The <see cref="FlexiSectionBlock"/>'s root element's type.</para> 
        /// <para>The element must be a <a href="https://html.spec.whatwg.org/#sectioning-content">sectioning content</a> element.</para>
        /// <para>Defaults to <see cref="SectioningContentElement.Section"/>.</para>
        /// </param>
        /// <param name="generateID">
        /// <para>The value specifying whether to generate an ID for the <see cref="FlexiSectionBlock"/>.</para>
        /// <para>The generated ID is assigned to the <see cref="FlexiSectionBlock"/>'s root element.</para>
        /// <para>The generated ID is the <see cref="FlexiSectionBlock"/>'s heading content in kebab-case (lowercase words joined by dashes). 
        /// For example, if the heading content is "Foo Bar Baz", the generated ID is "foo-bar-baz".</para>
        /// <para>If the generated ID is a duplicate of another <see cref="FlexiSectionBlock"/>'s ID, "-&lt;duplicate index&gt;" is appended. 
        /// For example, the second <see cref="FlexiSectionBlock"/> with heading content "Foo Bar Baz" will have ID "foo-bar-baz-1".</para>
        /// <para>The generated ID precedence over any ID specified in <paramref name="attributes"/>.</para>
        /// <para>Defaults to true.</para>
        /// </param>
        /// <param name="linkIcon">
        /// <para>The <see cref="FlexiSectionBlock"/>'s link icon as an HTML fragment.</para>
        /// <para>A class attribute with value "&lt;<paramref name="blockName"/>&gt;__link-icon" is added to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no link icon is rendered.</para>
        /// <para>Defaults to the <a href="https://material.io/tools/icons/?icon=link&amp;style=baseline">Material Design link icon</a>.</para>
        /// </param>
        /// <param name="referenceLinkable">
        /// <para>The value specifying whether the <see cref="FlexiSectionBlock"/> is <a href="https://spec.commonmark.org/0.28/#reference-link">reference-linkable</a>.</para>
        /// <para>If this value and <paramref name="generateID"/> are both true, the <see cref="FlexiSectionBlock"/> is reference-linkable. 
        /// Otherwise, it isn't.</para>
        /// <para>If a <see cref="FlexiSectionBlock"/> is reference-linkable, its <a href="https://spec.commonmark.org/0.28/#link-label">link label</a> content
        /// is its heading content. For example, "## Foo Bar Baz" can be linked to using "[Foo Bar Baz]".</para>
        /// <para>If a <see cref="FlexiSectionBlock"/>'s ID has "-&lt;duplicate index&gt;" appended (see <paramref name="generateID"/>), 
        /// you can link to it using "&lt;heading content&gt; &lt;duplicate index&gt;". 
        /// For example, the second "## Foo Bar baz" can be linked to using "[Foo Bar Baz 1]".</para>
        /// <para>Defaults to true.</para>
        /// </param>
        /// <param name="renderingMode">
        /// <para>The <see cref="FlexiSectionBlock"/>'s rendering mode.</para>
        /// <para>Defaults to <see cref="FlexiSectionBlockRenderingMode.Standard"/>.</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiSectionBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If classes are specified, they are appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If the <see cref="FlexiSectionBlock"/> has a generated ID, it takes precedence over any ID in this value.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiSectionBlockOptions(
            string blockName = "flexi-section",
            SectioningContentElement element = SectioningContentElement.Section,
            bool generateID = true,
            string linkIcon = MaterialDesignIcons.MATERIAL_DESIGN_LINK,
            bool referenceLinkable = true,
            FlexiSectionBlockRenderingMode renderingMode = FlexiSectionBlockRenderingMode.Standard,
            IDictionary<string, string> attributes = default) : base(blockName, attributes)
        {
            Element = element;
            GenerateID = generateID;
            LinkIcon = linkIcon;
            ReferenceLinkable = referenceLinkable;
            RenderingMode = renderingMode;
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public SectioningContentElement Element { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool GenerateID { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string LinkIcon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool ReferenceLinkable { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual FlexiSectionBlockRenderingMode RenderingMode { get; private set; }
    }
}
