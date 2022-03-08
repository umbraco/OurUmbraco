using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiFigureBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiFigureBlockOptions"/>.</para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON in <see cref="FlexiOptionsBlock"/>s. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiFigureBlockOptions : RenderedRootBlockOptions<IFlexiFigureBlockOptions>, IFlexiFigureBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiFigureBlockOptions"/>.
        /// </summary>
        /// <param name="blockName">
        /// <para>The <see cref="FlexiFigureBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.</para>
        /// <para>In compliance with <a href="https://en.bem.info">BEM methodology</a>, this value is the <see cref="FlexiFigureBlock"/>'s root element's class as well as the prefix for all other classes in the block.</para>
        /// <para>This value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiFigureBlock"/>'s block name is "flexi-figure".</para>
        /// <para>Defaults to "flexi-figure".</para>
        /// </param>
        /// <param name="referenceLinkable">
        /// <para>The value specifying whether the <see cref="FlexiFigureBlock"/> is <a href="https://spec.commonmark.org/0.28/#reference-link">reference-linkable</a>.</para>
        /// <para>If this value is true and <paramref name="generateID"/> is true or an ID is specified in <paramref name="attributes"/>, 
        /// the <see cref="FlexiFigureBlock"/> is reference-linkable. Otherwise, it isn't.</para>
        /// <para>If a <see cref="FlexiFigureBlock"/> is reference-linkable, you can link to it using its name as label content. For example,
        /// the first <see cref="FlexiFigureBlock"/> in a document can be linked to using "[Figure 1]" or "[figure 1]" (label content is not case sensitive).</para>
        /// <para>Defaults to true.</para>
        /// </param>
        /// <param name="linkLabelContent">
        /// <para>The content of the <a href="https://spec.commonmark.org/0.28/#link-label">link label</a> for linking to the <see cref="FlexiFigureBlock"/>.</para>
        /// <para>If this value is not <c>null</c>, whitespace or an empty string, it is expected in place of the <see cref="FlexiFigureBlock"/>'s name as link label content.</para>
        /// <para>For example, if this value for the first <see cref="FlexiFigureBlock"/> in a document is "first", you'd link to it using "[first]" instead of "[figure 1]".</para>
        /// <para>Often, <see cref="FlexiFigureBlock"/> positions in a document aren't fixed. With custom link label content, reference-links to <see cref="FlexiFigureBlock"/>s do not need to be updated every time 
        /// positions change.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="generateID">
        /// <para>The value specifying whether to generate an ID for the <see cref="FlexiFigureBlock"/>.</para>
        /// <para>The generated ID is assigned to the <see cref="FlexiFigureBlock"/>'s root element.</para>
        /// <para>The generated ID is the <see cref="FlexiFigureBlock"/>'s generated name in kebab-case (lowercase words joined by dashes). For example,
        /// the generated ID of the first <see cref="FlexiFigureBlock"/> in a document is "figure-1".</para>
        /// <para>Any ID specified in <paramref name="attributes"/> takes precedence over the generated ID.</para>
        /// <para>Defaults to true.</para>
        /// </param>
        /// <param name="renderName">
        /// <para>The value specifying whether to render the <see cref="FlexiFigureBlock"/>'s name.</para>
        /// <para>If true, the <see cref="FlexiFigureBlock"/>'s name is rendered at the beginning of its caption followed by <c>. </c>. 
        /// For example, the caption of the first <see cref="FlexiFigureBlock"/> in a document will begin with "Figure 1. ".</para>
        /// <para>Defaults to true.</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiFigureBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If the class attribute is specified, its value is appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value has an ID value, it takes precedence over the generated ID.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiFigureBlockOptions(
            string blockName = "flexi-figure",
            bool referenceLinkable = true,
            string linkLabelContent = default,
            bool generateID = true,
            bool renderName = true,
            IDictionary<string, string> attributes = default) : base(blockName, attributes)
        {
            ReferenceLinkable = referenceLinkable;
            LinkLabelContent = linkLabelContent;
            GenerateID = generateID;
            RenderName = renderName;
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool ReferenceLinkable { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string LinkLabelContent { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool GenerateID { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool RenderName { get; private set; }
    }
}
