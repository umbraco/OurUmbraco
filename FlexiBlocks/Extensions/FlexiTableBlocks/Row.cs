using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// Represents a table row.
    /// </summary>
    public class Row : IList<Cell>
    {
        private readonly List<Cell> _cells;

        /// <summary>
        /// Creates a <see cref="Row"/>.
        /// </summary>
        public Row()
        {
            _cells = new List<Cell>();
            IsOpen = true;
            IsHeaderRow = false;
        }

        /// <summary>
        /// Gets or sets the value indicating whether the <see cref="Row"/> is a header row.
        /// </summary>
        public bool IsHeaderRow { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the <see cref="Row"/> is open.
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Cell"/> at the specified index.
        /// </summary>
        public Cell this[int index] { get => _cells[index]; set => _cells[index] = value; }

        /// <summary>
        /// Gets the number of <see cref="Cell"/>s contained in the <see cref="Row"/>.
        /// </summary>
        public int Count => _cells.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Row"/> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds an object to the end of the <see cref="Row"/>.
        /// </summary>
        public void Add(Cell item)
        {
            _cells.Add(item);
        }

        /// <summary>
        /// Removes all <see cref="Cell"/>s from the <see cref="Row"/>.
        /// </summary>
        public void Clear()
        {
            _cells.Clear();
        }

        /// <summary>
        /// Determines whether a <see cref="Cell"/> is in the <see cref="Row"/>.
        /// </summary>
        public bool Contains(Cell item)
        {
            return _cells.Contains(item);
        }

        /// <summary>
        /// Copies the <see cref="Row"/> or a portion of it to an array.
        /// </summary>
        public void CopyTo(Cell[] array, int arrayIndex)
        {
            _cells.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        public IEnumerator<Cell> GetEnumerator()
        {
            return ((IList<Cell>)_cells).GetEnumerator();
        }

        /// <summary>
        /// Returns the zero-based index of the first occurrence of a value in the <see cref="Row"/> or in a portion of it.
        /// </summary>
        public int IndexOf(Cell item)
        {
            return _cells.IndexOf(item);
        }

        /// <summary>
        /// Inserts a <see cref="Cell"/> into the <see cref="Row"/> at the specified index.
        /// </summary>
        public void Insert(int index, Cell item)
        {
            _cells.Insert(index, item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="Row"/>.
        /// </summary>
        public bool Remove(Cell item)
        {
            return _cells.Remove(item);
        }

        /// <summary>
        /// Removes the <see cref="Cell"/> at the specified index of the <see cref="Row"/>.
        /// </summary>
        public void RemoveAt(int index)
        {
            _cells.RemoveAt(index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Cell>)_cells).GetEnumerator();
        }

        /// <summary>
        /// Checks for value equality between the <see cref="Row"/> and an object.
        /// </summary>
        /// <param name="obj">The object to check for value equality.</param>
        /// <returns>True if the <see cref="Row"/>'s value is equal to the object's value, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is Row row &&
                   _cells.SequenceEqual(row._cells) &&
                   IsHeaderRow == row.IsHeaderRow &&
                   IsOpen == row.IsOpen &&
                   Count == row.Count;
        }

        /// <summary>
        /// WARNING: always returns 0. This type is mutable - it is not suitable for use as the key type in a dictionary and
        /// instances of this type should not be stored in a hashset.
        /// </summary>
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
