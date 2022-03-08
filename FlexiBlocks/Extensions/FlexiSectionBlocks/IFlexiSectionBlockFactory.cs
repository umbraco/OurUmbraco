using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// An abstraction for creating <see cref="FlexiSectionBlock"/>s.
    /// </summary>
    public interface IFlexiSectionBlockFactory
    {
        /// <summary>
        /// Creates a <see cref="FlexiSectionBlock"/>.
        /// </summary>
        /// <param name="level">The <see cref="FlexiSectionBlock"/>'s level.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiSectionBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiSectionBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="level"/> is less than 1 or greater than 6.</exception>
        /// <exception cref="OptionsException">Thrown if an option is invalid.</exception>
        FlexiSectionBlock Create(int level, BlockProcessor blockProcessor, BlockParser blockParser);
    }
}
