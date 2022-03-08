using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks
{
    /// <summary>
    /// A parser that parses <see cref="FlexiPictureBlock"/>s from markdown.
    /// </summary>
    public class FlexiPictureBlockParser : JsonBlockParser<FlexiPictureBlock, ProxyJsonBlock>
    {
        /// <summary>
        /// Creates a <see cref="FlexiPictureBlockParser"/>.
        /// </summary>
        /// <param name="flexiPixtureBlockFactory">The factory for building <see cref="FlexiPictureBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiPixtureBlockFactory"/> is <c>null</c>.</exception>
        public FlexiPictureBlockParser(IJsonBlockFactory<FlexiPictureBlock, ProxyJsonBlock> flexiPixtureBlockFactory) : base(flexiPixtureBlockFactory)
        {
            OpeningCharacters = new[] { 'p' };
        }
    }
}
