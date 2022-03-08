using Markdig;
using Markdig.Renderers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.ContextObjects
{
    /// <summary>
    /// A Markdig extension for context objects.
    /// </summary>
    public class ContextObjectsExtension : IMarkdownExtension
    {
        /// <summary>
        /// Creates a <see cref="ContextObjectsExtension"/>.
        /// </summary>
        /// <param name="contextObjectsStore">The context objects store.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="contextObjectsStore"/> is <c>null</c>.</exception>
        public ContextObjectsExtension(ContextObjectsStore contextObjectsStore)
        {
            ContextObjectsStore = contextObjectsStore ?? throw new ArgumentNullException(nameof(contextObjectsStore));
        }

        /// <summary>
        /// The context objects store.
        /// </summary>
        public virtual ContextObjectsStore ContextObjectsStore { get; }

        /// <summary>
        /// Registers the <see cref="ContextObjectsStore"/> if one isn't already registered.
        /// </summary>
        /// <param name="pipeline">The pipeline builder to register the store for.</param>
        public virtual void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.BlockParsers.Contains<ContextObjectsStore>())
            {
                pipeline.BlockParsers.Add(ContextObjectsStore);
            }
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <param name="pipeline">Unused.</param>
        /// <param name="renderer">Unused.</param>
        public virtual void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            // Do nothing
        }
    }
}
