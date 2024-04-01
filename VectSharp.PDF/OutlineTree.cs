using System.Collections;
using System.Collections.Generic;

namespace VectSharp.PDF
{
    /// <summary>
    /// Represents an outline tree (table of contents) for a PDF document.
    /// </summary>
    public class OutlineTree : IList<OutlineTreeNode>
    {
        /// <summary>
        /// The top-level items in the outline.
        /// </summary>
        public List<OutlineTreeNode> TopLevelItems { get; }

        /// <inheritdoc/>
        public int Count => ((ICollection<OutlineTreeNode>)TopLevelItems).Count;

        /// <inheritdoc/>
        public bool IsReadOnly => ((ICollection<OutlineTreeNode>)TopLevelItems).IsReadOnly;

        /// <inheritdoc/>
        public OutlineTreeNode this[int index] { get => ((IList<OutlineTreeNode>)TopLevelItems)[index]; set => ((IList<OutlineTreeNode>)TopLevelItems)[index] = value; }

        /// <summary>
        /// Create a new <see cref="OutlineTree"/>.
        /// </summary>
        public OutlineTree()
        {
            this.TopLevelItems = new List<OutlineTreeNode>();
        }

        /// <inheritdoc/>
        public int IndexOf(OutlineTreeNode item)
        {
            return ((IList<OutlineTreeNode>)TopLevelItems).IndexOf(item);
        }

        /// <inheritdoc/>
        public void Insert(int index, OutlineTreeNode item)
        {
            ((IList<OutlineTreeNode>)TopLevelItems).Insert(index, item);
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            ((IList<OutlineTreeNode>)TopLevelItems).RemoveAt(index);
        }

        /// <inheritdoc/>
        public void Add(OutlineTreeNode item)
        {
            ((ICollection<OutlineTreeNode>)TopLevelItems).Add(item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            ((ICollection<OutlineTreeNode>)TopLevelItems).Clear();
        }

        /// <inheritdoc/>
        public bool Contains(OutlineTreeNode item)
        {
            return ((ICollection<OutlineTreeNode>)TopLevelItems).Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(OutlineTreeNode[] array, int arrayIndex)
        {
            ((ICollection<OutlineTreeNode>)TopLevelItems).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(OutlineTreeNode item)
        {
            return ((ICollection<OutlineTreeNode>)TopLevelItems).Remove(item);
        }

        /// <inheritdoc/>
        public IEnumerator<OutlineTreeNode> GetEnumerator()
        {
            return ((IEnumerable<OutlineTreeNode>)TopLevelItems).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)TopLevelItems).GetEnumerator();
        }
    }

    /// <summary>
    /// An item in an outline tree.
    /// </summary>
    public class OutlineTreeNode : IList<OutlineTreeNode>
    {
        /// <summary>
        /// Children of this item (can be empty).
        /// </summary>
        public List<OutlineTreeNode> Children { get; }

        /// <summary>
        /// Text for this item.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Tag of the graphics item that should be focused when the user clicks on the outline item.
        /// </summary>
        public string DestinationTag { get; set; }

        /// <summary>
        /// Colour for the outline item (the alpha component is ignored).
        /// </summary>
        public Colour Colour { get; set; }

        /// <summary>
        /// Indicates whether the outline item should be highlighted in bold or not.
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Indicates whether the outline item should be highlighted in italics or not.
        /// </summary>
        public bool Italic { get; set; }

        /// <inheritdoc/>
        public int Count => ((ICollection<OutlineTreeNode>)Children).Count;

        /// <inheritdoc/>
        public bool IsReadOnly => ((ICollection<OutlineTreeNode>)Children).IsReadOnly;

        /// <inheritdoc/>
        public OutlineTreeNode this[int index] { get => ((IList<OutlineTreeNode>)Children)[index]; set => ((IList<OutlineTreeNode>)Children)[index] = value; }

        /// <summary>
        /// Create a new <see cref="OutlineTreeNode"/>.
        /// </summary>
        /// <param name="title">The text for the new outline item.</param>
        /// <param name="destinationTag">Tag of the graphics item that should be focused when the user clicks on the outline item.</param>
        /// <param name="colour">The colour for the outline item.</param>
        /// <param name="bold">Whether the outline item should be highlighted in bold or not.</param>
        /// <param name="italic">Whether the outline item should be highlighted in italics or not.</param>
        public OutlineTreeNode(string title, string destinationTag = null, Colour colour = default, bool bold = false, bool italic = false)
        {
            this.Title = title;
            this.Children = new List<OutlineTreeNode>();
            this.DestinationTag = destinationTag;
            Colour = colour;
            Bold = bold;
            Italic = italic;
        }

        /// <inheritdoc/>
        public int IndexOf(OutlineTreeNode item)
        {
            return ((IList<OutlineTreeNode>)Children).IndexOf(item);
        }

        /// <inheritdoc/>
        public void Insert(int index, OutlineTreeNode item)
        {
            ((IList<OutlineTreeNode>)Children).Insert(index, item);
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            ((IList<OutlineTreeNode>)Children).RemoveAt(index);
        }

        /// <inheritdoc/>
        public void Add(OutlineTreeNode item)
        {
            ((ICollection<OutlineTreeNode>)Children).Add(item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            ((ICollection<OutlineTreeNode>)Children).Clear();
        }

        /// <inheritdoc/>
        public bool Contains(OutlineTreeNode item)
        {
            return ((ICollection<OutlineTreeNode>)Children).Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(OutlineTreeNode[] array, int arrayIndex)
        {
            ((ICollection<OutlineTreeNode>)Children).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(OutlineTreeNode item)
        {
            return ((ICollection<OutlineTreeNode>)Children).Remove(item);
        }

        /// <inheritdoc/>
        public IEnumerator<OutlineTreeNode> GetEnumerator()
        {
            return ((IEnumerable<OutlineTreeNode>)Children).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Children).GetEnumerator();
        }
    }
}
