using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    // TODO these comments are here because of spec generator limitations
    // <para>The default implementation of <see cref="IFlexiCardBlockOptions"/>.</para>
    // 
    // <para>Initialization-wise, this class is primarily populated from JSON in <see cref="FlexiOptionsBlock"/>s. Hence the Newtonsoft.JSON attributes. 
    // Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    // 
    // <para>This class is immutable.</para>
    /// <summary>
    /// Options for a <see cref="FlexiCardBlock"/>.
    /// </summary>
    public class FlexiCardBlockOptions : RenderedBlockOptions<IFlexiCardBlockOptions>, IFlexiCardBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiCardBlockOptions"/>.
        /// </summary>
        /// <param name="url">
        /// <para>The URL the <see cref="FlexiCardBlock"/> points to.</para>
        /// <para>If this value is not <c>null</c>, whitespace or an empty string, the <see cref="FlexiCardBlock"/>'s outermost element is an <c>&lt;a&gt;</c>
        /// with this value as its <c>href</c>.</para>
        /// <para>Otherwise, the <see cref="FlexiCardsBlock"/>'s outermost element is a <c>&lt;div&gt;</c>.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="backgroundIcon">
        /// <para>The <see cref="FlexiCardBlock"/>'s background icon as an HTML fragment.</para>
        /// <para>A class attribute with value "&lt;parent block name&gt;__card_background-icon" where &lt;parent block name&gt; is the block name of 
        /// the <see cref="FlexiCardBlock"/>'s parent <see cref="FlexiCardsBlock"/>, is added to this fragment's first start tag.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, no background icon is rendered.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiCardBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If the class attribute is specified, its value is appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiCardBlockOptions(string url = default,
            string backgroundIcon = default,
            IDictionary<string, string> attributes = default) : base(attributes)
        {
            Url = url;
            BackgroundIcon = backgroundIcon;
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Url { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string BackgroundIcon { get; private set; }
    }
}
