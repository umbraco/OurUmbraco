using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiTabBlock"/>s from markdown.
    /// </summary>
    public class FlexiTabBlockParser : MultipartBlockParser<FlexiTabBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiTabBlockParser"/>.
        /// </summary>
        /// <param name="flexiTabBlockFactory">The factory for building <see cref="FlexiTabBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiTabBlockFactory"/> is <c>null</c>.</exception>
        public FlexiTabBlockParser(IMultipartBlockFactory<FlexiTabBlock> flexiTabBlockFactory) :
            base(flexiTabBlockFactory, "tab", new PartType[] { PartType.Leaf, PartType.Container })
        {
        }
    }
}
