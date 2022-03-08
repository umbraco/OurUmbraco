using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiFigureBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiFigureBlock"/>s from markdown.
    /// </summary>
    public class FlexiFigureBlockParser : MultipartBlockParser<FlexiFigureBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiFigureBlockParser"/>.
        /// </summary>
        /// <param name="flexiFigureBlockFactory">The factory for building <see cref="FlexiFigureBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiFigureBlockFactory"/> is <c>null</c>.</exception>
        public FlexiFigureBlockParser(IMultipartBlockFactory<FlexiFigureBlock> flexiFigureBlockFactory) :
            base(flexiFigureBlockFactory, "figure", new PartType[] { PartType.Container, PartType.Leaf })
        {
        }
    }
}
