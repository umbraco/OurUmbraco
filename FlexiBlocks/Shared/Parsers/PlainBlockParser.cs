using Markdig.Parsers;
using Markdig.Syntax;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// A parser for plain <see cref="Block"/>s.
    /// </summary>
    public class PlainBlockParser : BlockParser<Block>
    {
        /// <summary>
        /// Does nothing. <see cref="PlainBlockParser"/> should never be registered to a markdown pipeline, so this method should
        /// never be called.
        /// </summary>
        protected override BlockState TryOpenBlock(BlockProcessor blockProcessor)
        {
            return BlockState.None;
        }

        /// <summary>
        /// Always continues. Plain <see cref="Block"/>s have no state other than their contents, they should typically
        /// be closed by a parent block's parser, e.g <see cref="MultipartBlockParser{T}"/>.
        /// </summary>
        protected override BlockState TryContinueBlock(BlockProcessor blockProcessor, Block block)
        {
            return BlockState.Continue;
        }
    }
}
