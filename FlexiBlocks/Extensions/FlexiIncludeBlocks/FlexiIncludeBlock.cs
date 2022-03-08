using Markdig.Parsers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// Represents a block that includes content from a local or remote source.
    /// </summary>
    public class FlexiIncludeBlock : Block
    {
        /// <summary>
        /// Creates a <see cref="FlexiIncludeBlock"/>.
        /// </summary>
        /// <param name="source">The <see cref="FlexiIncludeBlock"/>'s source.</param>
        /// <param name="clippings">The <see cref="Clipping"/>s specifying content from the source to include.</param>
        /// <param name="type">The <see cref="FlexiIncludeBlock"/>'s type.</param>
        /// <param name="cacheDirectory">The directory to cache the <see cref="FlexiIncludeBlock"/>'s content in.</param>
        /// <param name="parentFlexiIncludeBlock">The <see cref="FlexiIncludeBlock"/>'s parent <see cref="FlexiIncludeBlock"/>.</param>
        /// <param name="containingSource">The URI of the source that contains the <see cref="FlexiIncludeBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="FlexiIncludeBlock"/>.</param>
        public FlexiIncludeBlock(Uri source,
            ReadOnlyCollection<Clipping> clippings,
            FlexiIncludeType type,
            string cacheDirectory,
            FlexiIncludeBlock parentFlexiIncludeBlock,
            string containingSource,
            BlockParser blockParser) : base(blockParser)
        {
            Source = source;
            Clippings = clippings;
            Type = type;
            CacheDirectory = cacheDirectory;
            ParentFlexiIncludeBlock = parentFlexiIncludeBlock;
            ContainingSource = containingSource;
            Children = new List<FlexiIncludeBlock>();
        }

        /// <summary>
        /// Gets the <see cref="FlexiIncludeBlock"/>'s source.
        /// </summary>
        public Uri Source { get; }

        /// <summary>
        /// Gets the <see cref="Clipping"/>s specifying content from the source to include.
        /// </summary>
        public ReadOnlyCollection<Clipping> Clippings { get; }

        /// <summary>
        /// Gets the <see cref="FlexiIncludeBlock"/>'s type.
        /// </summary>
        public FlexiIncludeType Type { get; }

        /// <summary>
        /// Gets the directory to cache the <see cref="FlexiIncludeBlock"/>'s content in.
        /// </summary>
        public string CacheDirectory { get; }

        /// <summary>
        /// Gets the <see cref="FlexiIncludeBlock"/>'s parent <see cref="FlexiIncludeBlock"/>.
        /// </summary>
        public FlexiIncludeBlock ParentFlexiIncludeBlock { get; }

        /// <summary>
        /// Gets the URI of the source that contains the <see cref="FlexiIncludeBlock"/>.
        /// </summary>
        public string ContainingSource { get; }

        /// <summary>
        /// Gets the <see cref="FlexiIncludeBlock"/>'s child <see cref="FlexiIncludeBlock"/>s.
        /// </summary>
        public List<FlexiIncludeBlock> Children { get; }
    }
}
