using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiCardBlock"/>s from markdown.
    /// </summary>
    public class FlexiCardBlockParser : MultipartBlockParser<FlexiCardBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiCardBlockParser"/>.
        /// </summary>
        /// <param name="flexiCardBlockFactory">The factory for building <see cref="FlexiCardBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiCardBlockFactory"/> is <c>null</c>.</exception>
        public FlexiCardBlockParser(IMultipartBlockFactory<FlexiCardBlock> flexiCardBlockFactory) :
            base(flexiCardBlockFactory, "card", new PartType[] { PartType.Leaf, PartType.Container, PartType.Leaf })
        {
        }
    }
}
