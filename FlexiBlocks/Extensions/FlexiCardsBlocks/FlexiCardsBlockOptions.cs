using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiCardsBlockOptions"/>.</para>
    /// 
    /// <para>Initialization-wise, this class is primarily populated from JSON in <see cref="FlexiOptionsBlock"/>s. Hence the Newtonsoft.JSON attributes. 
    /// Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    /// 
    /// <para>This class is immutable as long as <see cref="DefaultCardOptions"/> is immutable.</para>
    /// </summary>
    public class FlexiCardsBlockOptions : RenderedRootBlockOptions<IFlexiCardsBlockOptions>, IFlexiCardsBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiCardBlockOptions"/>.
        /// </summary>
        /// <param name="blockName">
        /// <para>The <see cref="FlexiCardsBlock"/>'s <a href="https://en.bem.info/methodology/naming-convention/#block-name">BEM block name</a>.</para>
        /// <para>In compliance with <a href="https://en.bem.info">BEM methodology</a>, this value is the <see cref="FlexiCardsBlock"/>'s root element's class as well as the prefix for all other classes in the block.</para>
        /// <para>This value should contain only valid <a href="https://www.w3.org/TR/CSS21/syndata.html#characters">CSS class characters</a>.</para>
        /// <para>If this value is <c>null</c>, whitespace or an empty string, the <see cref="FlexiCardsBlock"/>'s block name is "flexi-cards".</para>
        /// <para>Defaults to "flexi-cards".</para>
        /// </param>
        /// <param name="cardSize">
        /// <para>The display size of contained <see cref="FlexiCardBlock"/>s.</para>
        /// <para>A class attribute with value "&lt;<paramref name="blockName"/>&gt;_size_&lt;<paramref name="cardSize"/>&gt;" is added to the <see cref="FlexiCardsBlock"/>'s root element.</para>
        /// <para>Defaults to <see cref="FlexiCardBlockSize.Small"/>.</para>
        /// </param>
        /// <param name="defaultCardOptions">
        /// <para>The default <see cref="IFlexiCardBlockOptions"/> for contained <see cref="FlexiCardBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiCardBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiCardsBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If the class attribute is specified, its value is appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiCardsBlockOptions(
            string blockName = "flexi-cards",
            FlexiCardBlockSize cardSize = FlexiCardBlockSize.Small,
            IFlexiCardBlockOptions defaultCardOptions = default,
            IDictionary<string, string> attributes = default) : base(blockName, attributes)
        {
            CardSize = cardSize;
            DefaultCardOptions = defaultCardOptions ?? new FlexiCardBlockOptions();
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public FlexiCardBlockSize CardSize { get; private set; }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public IFlexiCardBlockOptions DefaultCardOptions { get; private set; }

        /// <summary>
        /// Returns a shallow clone with a shallow clone of <see cref="DefaultCardOptions"/>.
        /// </summary>
        public override IFlexiCardsBlockOptions Clone()
        {
            // The default Clone method creates a shallow clone. Shallow clones are fine for options types with only value type properties.
            // DefaultCardOptions is a reference type property, so we have to manually clone it.
            return new FlexiCardsBlockOptions(BlockName, CardSize, DefaultCardOptions.Clone(), Attributes);
        }
    }
}
