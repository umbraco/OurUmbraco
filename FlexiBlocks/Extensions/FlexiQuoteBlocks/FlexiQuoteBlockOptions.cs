using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiQuoteBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiQuoteBlockOptions"/>.</para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON in <see cref="FlexiOptionsBlock"/>s. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiQuoteBlockOptions : RenderedRootBlockOptions<IFlexiQuoteBlockOptions>, IFlexiQuoteBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiQuoteBlockOptions"/>.
        /// </summary>
        /// <param name="blockName">
        /// <para>The <see cref="FlexiQuoteBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.</para>
        /// <para>In compliance with <a href="https://en.bem.info">BEM methodology</a>, this value is the <see cref="FlexiQuoteBlock"/>'s root element's class as well as the prefix for all other classes in the block.</para>
        /// <para>This value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiQuoteBlock"/>'s block name is "flexi-quote".</para>
        /// <para>Defaults to "flexi-quote".</para>
        /// </param>
        /// <param name="icon">
        /// <para>The <see cref="FlexiQuoteBlock"/>'s icon as an HTML fragment.</para>
        /// <para>A class attribute with value "&lt;<paramref name="blockName"/>&gt;__icon" is added to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no icon is rendered.</para>
        /// <para>Defaults to an opening quotation mark icon.</para>
        /// </param>
        /// <param name="citeLink">
        /// <para>The index of the link in the <see cref="FlexiQuoteBlock"/>'s citation that points to the work where its quote comes from.</para>
        /// <para>The link's URL is assigned to the <see cref="FlexiQuoteBlock"/>'s blockquote element's cite attribute,
        /// in compliance with <a href="https://html.spec.whatwg.org/multipage/grouping-content.html#the-blockquote-element">HTML specifications</a>.</para>
        /// <para>If this value is <c>-n</c>, the link is the nth last link. For example, if this value is <c>-2</c>, the link is the 2nd last link.</para>
        /// <para>If the <see cref="FlexiQuoteBlock"/> has at least one link in its citation, this value must be within the logical range of indices.</para>
        /// <para>Defaults to <c>-1</c> (the last link).</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiQuoteBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If the class attribute is specified, its value is appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiQuoteBlockOptions(
            string blockName = "flexi-quote",
            string icon = CustomIcons.CUSTOM_QUOTE,
            int citeLink = -1,
            IDictionary<string, string> attributes = default) : base(blockName, attributes)
        {
            Icon = icon;
            CiteLink = citeLink;
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Icon { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int CiteLink { get; private set; }
    }
}
