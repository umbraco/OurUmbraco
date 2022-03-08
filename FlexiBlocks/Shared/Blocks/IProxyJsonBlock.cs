namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for proxy JSON blocks.
    /// </summary>
    public interface IProxyJsonBlock : IProxyBlock
    {
        /// <summary>
        /// Gets or sets the number of open objects in the JSON parsed to far.
        /// </summary>
        int NumOpenObjects { get; set; }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the JSON parsed so far ends within a string.
        /// </summary>
        bool WithinString { get; set; }
    }
}
