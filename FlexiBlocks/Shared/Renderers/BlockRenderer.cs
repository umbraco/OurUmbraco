using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// <para>An abstraction for rendering <see cref="Block"/>s.</para> 
    /// 
    /// <para>This class's primary functionality is consistent exception handling.</para>
    /// 
    /// <para>Exceptions thrown by implementers are wrapped in <see cref="BlockException"/>s with the locations of offending markdown.</para>
    /// 
    /// <para>Without this class's approach to exception handling, exceptions thrown by renderers bubble up through Markdig and
    /// user facing applications with no information on locations of offending markdown.</para>
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Block"/> this renderer renders.</typeparam>
    public abstract class BlockRenderer<T> : HtmlObjectRenderer<T> where T : MarkdownObject, IBlock
    {
        /// <summary>
        /// Renders a <see cref="Block"/> as HTML.
        /// </summary>
        /// <param name="htmlRenderer">The renderer to write to. Never <c>null</c>.</param>
        /// <param name="block">The <see cref="Block"/> to render. Never <c>null</c>.</param>
        protected abstract void WriteBlock(HtmlRenderer htmlRenderer, T block);

        /// <summary>
        /// Renders a <see cref="Block"/> as HTML.
        /// </summary>
        /// <param name="renderer">The renderer to write to.</param>
        /// <param name="obj">The <see cref="Block"/> to render.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="renderer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is <c>null</c>.</exception>
        protected sealed override void Write(HtmlRenderer renderer, T obj)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            try
            {
                renderer.EnsureLine();

                WriteBlock(renderer, obj);
            }
            catch (Exception exception) when ((exception as BlockException)?.Context != BlockExceptionContext.Block)
            {
                throw new BlockException(obj as Block, innerException: exception);
            }
        }
    }
}
