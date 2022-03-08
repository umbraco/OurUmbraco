namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// The default implementation of <see cref="IBlockOptions{T}"/>.
    /// </summary>
    public abstract class BlockOptions<T> : IBlockOptions<T> where T : IBlockOptions<T>
    {
        /// <inheritdoc />
        public virtual T Clone()
        {
            return (T)MemberwiseClone();
        }
    }
}
