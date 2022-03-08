namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Fence trailing characters.
    /// </summary>
    public enum FenceTrailingCharacters
    {
        /// <summary>
        /// Fence trailing characters must be whitespace.
        /// </summary>
        Whitespace = 0,

        /// <summary>
        /// Fence trailing characters can be any characters. 
        /// </summary>
        All = 1,

        /// <summary>
        /// Fence trailing characters can be any characters other than the fence character.
        /// </summary>
        AllButFenceCharacter = 2
    }
}
