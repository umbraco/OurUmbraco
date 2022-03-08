using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    // TODO these comments are here because of spec generator limitations
    // <para>The default implementation of <see cref="IFlexiCardBlockOptions"/>.</para>
    // 
    // <para>Initialization-wise, this class is primarily populated from JSON in <see cref="FlexiOptionsBlock"/>s. Hence the Newtonsoft.JSON attributes. 
    // Developers can also manually instantiate this class, typically for use as extension-wide default options.</para>
    // 
    // <para>This class is immutable.</para>
    /// <summary>
    /// Options for a <see cref="FlexiTabBlock"/>.
    /// </summary>
    public class FlexiTabBlockOptions : RenderedBlockOptions<IFlexiTabBlockOptions>, IFlexiTabBlockOptions
    {
        /// <summary>
        /// Creates a <see cref="FlexiTabBlockOptions"/>.
        /// </summary>
        /// <param name="attributes">
        /// <para>The HTML attributes for the <see cref="FlexiTabBlock"/>'s root element.</para>
        /// <para>Attribute names must be lowercase.</para>
        /// <para>If the class attribute is specified, its value is appended to default classes. This facilitates <a href="https://en.bem.info/methodology/quick-start/#mix">BEM mixes</a>.</para>
        /// <para>If this value is <c>null</c>, default classes are still assigned to the root element.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiTabBlockOptions(IDictionary<string, string> attributes = default) : base(attributes)
        {
        }
    }
}
