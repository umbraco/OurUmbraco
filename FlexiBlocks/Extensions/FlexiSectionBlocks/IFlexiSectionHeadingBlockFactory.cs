using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// An abstraction for creating <see cref="FlexiSectionHeadingBlock"/>s.
    /// </summary>
    public interface IFlexiSectionHeadingBlockFactory
    {
        /// <summary>
        /// Creates a <see cref="FlexiSectionHeadingBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiSectionHeadingBlock"/>.</param>
        /// <param name="flexiSectionBlockOptions">The options for the <see cref="FlexiSectionHeadingBlock"/>'s parent <see cref="FlexiSectionBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiSectionHeadingBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        FlexiSectionHeadingBlock Create(BlockProcessor blockProcessor, IFlexiSectionBlockOptions flexiSectionBlockOptions, BlockParser blockParser);
    }
}
