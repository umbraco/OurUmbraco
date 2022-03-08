using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// An abstraction for <see cref="FlexiIncludeBlock"/> options.
    /// </summary>
    public interface IFlexiIncludeBlockOptions : IBlockOptions<IFlexiIncludeBlockOptions>
    {
        /// <summary>
        /// Gets the <see cref="FlexiIncludeBlock"/>'s source.
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Gets the <see cref="Clipping"/>s specifying content from the source to include.
        /// </summary>
        ReadOnlyCollection<Clipping> Clippings { get; }

        /// <summary>
        /// Gets the <see cref="FlexiIncludeBlock"/>'s type.
        /// </summary>
        FlexiIncludeType Type { get; }

        /// <summary>
        /// Gets the value specifying whether or not to cache the <see cref="FlexiIncludeBlock"/>'s content on disk.
        /// </summary>
        bool Cache { get; }

        /// <summary>
        /// Gets the directory to cache the <see cref="FlexiIncludeBlock"/>'s content in.
        /// </summary>
        string CacheDirectory { get; }
    }
}
