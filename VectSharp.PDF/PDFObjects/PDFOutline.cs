/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2024 Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// A PDF outline dictionary.
    /// </summary>
    public class PDFOutline : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("Outlines", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// First outline item.
        /// </summary>
        public PDFOutlineItem First { get; }

        /// <summary>
        /// Last outline item.
        /// </summary>
        public PDFOutlineItem Last { get; }

        /// <summary>
        /// Create a new <see cref="PDFOutline"/>.
        /// </summary>
        /// <param name="first">The first outline item.</param>
        /// <param name="last">The last outline item.</param>
        public PDFOutline(PDFOutlineItem first, PDFOutlineItem last)
        {
            this.First = first;
            this.Last = last;
        }
    }

    /// <summary>
    /// Represents a single PDF outline item.
    /// </summary>
    public class PDFOutlineItem : PDFDictionary
    {
        /// <summary>
        /// The title of the item.
        /// </summary>
        public PDFString Title { get; }

        /// <summary>
        /// The parent item (either another <see cref="PDFOutlineItem"/>, or the <see cref="PDFOutline"/> for top-level items).
        /// </summary>
        public PDFReferenceableObject Parent { get; set; }

        /// <summary>
        /// The previous outline item (or <see langword="null"/> for the first outline item).
        /// </summary>
        public PDFOutlineItem Prev { get; set; }

        /// <summary>
        /// The following outline item (or <see langword="null"/> for the last outline item).
        /// </summary>
        public PDFOutlineItem Next { get; set; }

        /// <summary>
        /// The first child item (or <see langword="null"/> for items without children).
        /// </summary>
        public PDFOutlineItem First { get; set; }

        /// <summary>
        /// The last child item (or <see langword="null"/> for items without children).
        /// </summary>
        public PDFOutlineItem Last { get; set; }

        /// <summary>
        /// The destination.
        /// </summary>
        public PDFArray<IPDFObject> Dest { get; }

        /// <summary>
        /// The colour for the outline item.
        /// </summary>
        public PDFArray<PDFDouble> C { get; }

        /// <summary>
        /// The font style for the outline item.
        /// </summary>
        public PDFUInt F { get; }

        /// <summary>
        /// Create a new <see cref="PDFOutlineItem"/> pointing to the specified destination.
        /// </summary>
        /// <param name="title">The text for the item.</param>
        /// <param name="destination">The destination page for the item.</param>
        /// <param name="destinationX">The X coordinate on the page.</param>
        /// <param name="destinationY">The Y coordinate on the page.</param>
        /// <param name="colour">The colour for the outline item.</param>
        /// <param name="bold">Whether the outline item should be highlighted in bold or not.</param>
        /// <param name="italic">Whether the outline item should be highlighted in italics or not.</param>
        public PDFOutlineItem(string title, PDFPage destination, double destinationX, double destinationY, Colour colour = default, bool bold = false, bool italic = false)
        {
            this.Title = new PDFString(title, PDFString.StringDelimiter.Brackets);
            this.Dest = new PDFArray<IPDFObject>(destination, new PDFString("XYZ", PDFString.StringDelimiter.StartingForwardSlash), new PDFDouble(destinationX), new PDFDouble(destinationY), new PDFDouble(0));

            this.C = new PDFArray<PDFDouble>(new PDFDouble(colour.R), new PDFDouble(colour.G), new PDFDouble(colour.B));
            this.F = new PDFUInt((bold ? 0b10U : 0b00U) | (italic ? 0b01U : 0b00U));
        }

        /// <summary>
        /// Create a new <see cref="PDFOutlineItem"/> that does not point to a destination.
        /// </summary>
        /// <param name="title">The text for the item.</param>
        /// <param name="colour">The colour for the outline item.</param>
        /// <param name="bold">Whether the outline item should be highlighted in bold or not.</param>
        /// <param name="italic">Whether the outline item should be highlighted in italics or not.</param>
        public PDFOutlineItem(string title, Colour colour = default, bool bold = false, bool italic = false)
        {
            this.Title = new PDFString(title, PDFString.StringDelimiter.Brackets);
            this.Dest = null;

            this.C = new PDFArray<PDFDouble>(new PDFDouble(colour.R), new PDFDouble(colour.G), new PDFDouble(colour.B));
            this.F = new PDFUInt((bold ? 0b10U : 0b00U) | (italic ? 0b01U : 0b00U));
        }
    }
}
