using Markdig.Parsers;
using System.Collections;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.ContextObjects
{
    /// <summary>
    /// A store of context objects.
    /// </summary>
    public class ContextObjectsStore : BlockParser, IDictionary<object, object>
    {
        private readonly Dictionary<object, object> _contextObjects = new Dictionary<object, object>();

        /// <summary>
        /// <para>Do nothing.</para>
        /// <para>This method will not get called as long as this parser comes after <see cref="ParagraphBlockParser"/> in
        /// <paramref name="processor"/>'s list of global parsers.</para>
        /// </summary>
        /// <param name="processor">Unused.</param>
        public override BlockState TryOpen(BlockProcessor processor)
        {
            return BlockState.None;
        }

        /// <inheritdoc />
        public object this[object key] { get => _contextObjects[key]; set => _contextObjects[key] = value; }

        /// <inheritdoc />
        public ICollection<object> Keys => _contextObjects.Keys;

        /// <inheritdoc />
        public ICollection<object> Values => _contextObjects.Values;

        /// <inheritdoc />
        public int Count => _contextObjects.Count;

        /// <inheritdoc />
        public bool IsReadOnly => ((IDictionary<object, object>)_contextObjects).IsReadOnly;

        /// <inheritdoc />
        public void Add(object key, object value)
        {
            _contextObjects.Add(key, value);
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<object, object> item)
        {
            ((IDictionary<object, object>)_contextObjects).Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _contextObjects.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<object, object> item)
        {
            return ((IDictionary<object, object>)_contextObjects).Contains(item);
        }

        /// <inheritdoc />
        public bool ContainsKey(object key)
        {
            return _contextObjects.ContainsKey(key);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            ((IDictionary<object, object>)_contextObjects).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            return _contextObjects.GetEnumerator();
        }

        /// <inheritdoc />
        public bool Remove(object key)
        {
            return _contextObjects.Remove(key);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<object, object> item)
        {
            return _contextObjects.Remove(item);
        }

        /// <inheritdoc />
        public bool TryGetValue(object key, out object value)
        {
            return _contextObjects.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _contextObjects.GetEnumerator();
        }
    }
}
