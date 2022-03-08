using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiBannerBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiBannerBlock"/>s from markdown.
    /// </summary>
    public class FlexiBannerBlockParser : MultipartBlockParser<FlexiBannerBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiBannerBlockParser"/>.
        /// </summary>
        /// <param name="flexiBannerBlockFactory">The factory for building <see cref="FlexiBannerBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiBannerBlockFactory"/> is <c>null</c>.</exception>
        public FlexiBannerBlockParser(IMultipartBlockFactory<FlexiBannerBlock> flexiBannerBlockFactory) :
            base(flexiBannerBlockFactory, "banner", new PartType[] { PartType.Leaf, PartType.Leaf })
        {
        }
    }
}
