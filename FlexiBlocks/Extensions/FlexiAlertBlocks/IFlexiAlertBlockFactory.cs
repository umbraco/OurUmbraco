using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks
{
    /// <summary>
    /// An abstraction for creating <see cref="FlexiAlertBlock"/>s.
    /// </summary>
    public interface IFlexiAlertBlockFactory
    {
        /// <summary>
        /// Creates a <see cref="FlexiAlertBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiAlertBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiAlertBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        FlexiAlertBlock Create(BlockProcessor blockProcessor, BlockParser blockParser);
    }
}
