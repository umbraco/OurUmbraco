using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiVideoBlock"/>s from markdown.
    /// </summary>
    public class FlexiVideoBlockParser : JsonBlockParser<FlexiVideoBlock, ProxyJsonBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiVideoBlockParser"/>.
        /// </summary>
        /// <param name="flexiPictureBlockFactory">The factory for building <see cref="FlexiVideoBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiPictureBlockFactory"/> is <c>null</c>.</exception>
        public FlexiVideoBlockParser(IJsonBlockFactory<FlexiVideoBlock, ProxyJsonBlock> flexiPictureBlockFactory) : base(flexiPictureBlockFactory)
        {
            OpeningCharacters = new[] { 'v' };
        }
    }
}
