namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Part types, see: <a href="https://spec.commonmark.org/0.28/#container-blocks-and-leaf-blocks">Container blocks and leaf blocks</a>.
    /// </summary>
    public enum PartType
    {
        /// <summary>
        /// Container.
        /// </summary>
        Container = 0,

        /// <summary>
        /// Leaf.
        /// </summary>
        Leaf
    }
}
