using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.ContextObjects
{
    /// <summary>
    /// The default implementation of <see cref="IContextObjectsService"/>.
    /// </summary>
    public class ContextObjectsService : IContextObjectsService
    {
        /// <inheritdoc />
        public virtual bool TryGetContextObject(object key, BlockProcessor blockProcessor, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            value = null;
            if (blockProcessor.Context?.Properties.TryGetValue(key, out value) == true) // This allows overriding by providing MarkdownParserContext.Context
            {
                return true;
            }

            // Try to retrieve from context objects store
            if (TryGetFromContextObjectsStore(key, blockProcessor, out value))
            {
                blockProcessor.Context?.Properties.Add(key, value);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public virtual bool TryAddContextObject(object key, object value, BlockProcessor blockProcessor)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            if (blockProcessor.Context != null)
            {
                blockProcessor.Context.Properties[key] = value;
                return true;
            }

            ContextObjectsStore contextObjectsStore = TryGetContextObjectsStore(blockProcessor);
            if (contextObjectsStore != null)
            {
                contextObjectsStore[key] = value;
                return true;
            }

            return false;
        }

        internal virtual ContextObjectsStore TryGetContextObjectsStore(BlockProcessor blockProcessor)
        {
            BlockParser[] globalParsers = blockProcessor.Parsers.GlobalParsers;

            if (globalParsers == null)
            {
                return null;
            }

            for (int i = globalParsers.Length - 1; i > -1; i--)
            {
                if (globalParsers[i] is ContextObjectsStore contextObjectsStore)
                {
                    return contextObjectsStore;
                }
            }

            return null;
        }

        internal virtual bool TryGetFromContextObjectsStore(object key, BlockProcessor blockProcessor, out object value)
        {
            value = null;

            return TryGetContextObjectsStore(blockProcessor)?.TryGetValue(key, out value) == true;
        }
    }
}
