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
    /// Represents a PDF action that opens a URI.
    /// </summary>
    public class PDFURIAction : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("Action", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// Type of action.
        /// </summary>
        public PDFString S { get; } = new PDFString("URI", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// Destination URI.
        /// </summary>
        public PDFString URI { get; }

        /// <summary>
        /// Creates a new <see cref="PDFURIAction"/> with the specified <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination"></param>
        public PDFURIAction(string destination)
        {
            this.URI = new PDFString(destination, PDFString.StringDelimiter.Brackets);
        }
    }

    /// <summary>
    /// Base class for PDF annotations.
    /// </summary>
    public abstract class PDFAnnotation : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("Annot", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// The annotation rectangle.
        /// </summary>
        public PDFArray<PDFDouble> Rect { get; }

        /// <summary>
        /// Optional content group membership dictionary for the annotation.
        /// </summary>
        public PDFOptionalContentGroupMembership OC { get; set; }

        /// <summary>
        /// Border style for the annotation.
        /// </summary>
        public PDFArray<IPDFObject> Border { get; }

        /// <summary>
        /// Colour for the annotation's border.
        /// </summary>
        public PDFArray<PDFDouble> C { get; }

        /// <summary>
        /// Create a new <see cref="PDFAnnotation"/> with the specified <paramref name="rect"/>.
        /// </summary>
        /// <param name="rect">The annotation rectangle.</param>
        /// <param name="borderWidth">Thickness of the annotation border.</param>
        /// <param name="borderDash">Dash style for the annotation border.</param>
        /// <param name="borderColour">Colour for the annotation border.</param>
        protected PDFAnnotation(Rectangle rect, double borderWidth, LineDash? borderDash, Colour borderColour)
        {
            this.Rect = new PDFArray<PDFDouble>(new PDFDouble(rect.Location.X), new PDFDouble(rect.Location.Y), new PDFDouble(rect.Location.X + rect.Size.Width), new PDFDouble(rect.Location.Y + rect.Size.Height));

            if (borderDash != null)
            {
                this.Border = new PDFArray<IPDFObject>(new PDFDouble(0), new PDFDouble(0), new PDFDouble(borderWidth), new PDFArray<PDFDouble>(new PDFDouble(borderDash.Value.UnitsOn), new PDFDouble(borderDash.Value.UnitsOff)));
            }
            else
            {
                this.Border = new PDFArray<IPDFObject>(new PDFDouble(0), new PDFDouble(0), new PDFDouble(borderWidth));
            }

            if (borderColour.A > 0)
            {
                this.C = new PDFArray<PDFDouble>(new PDFDouble(borderColour.R), new PDFDouble(borderColour.G), new PDFDouble(borderColour.B));
            }
            else
            {
                this.C = new PDFArray<PDFDouble>();
            }
        }
    }

    /// <summary>
    /// Base class for PDF link annotations.
    /// </summary>
    public abstract class PDFLinkAnnotation : PDFAnnotation
    {
        /// <summary>
        /// Annotation subtype.
        /// </summary>
        public PDFString Subtype { get; } = new PDFString("Link", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// Create a new <see cref="PDFLinkAnnotation"/> with the specified <paramref name="rect"/>.
        /// </summary>
        /// <param name="rect">The annotation rectangle.</param>
        /// <param name="borderWidth">Thickness of the annotation border.</param>
        /// <param name="borderDash">Dash style for the annotation border.</param>
        /// <param name="borderColour">Colour for the annotation border.</param>
        protected PDFLinkAnnotation(Rectangle rect, double borderWidth, LineDash? borderDash, Colour borderColour) : base(rect, borderWidth, borderDash, borderColour) { }
    }

    /// <summary>
    /// A PDF link annotation referring to a destination outside of the document.
    /// </summary>
    public class PDFExternalLinkAnnotation : PDFLinkAnnotation
    {
        /// <summary>
        /// The link destination.
        /// </summary>
        public PDFURIAction A { get; }

        /// <summary>
        /// Create a new <see cref="PDFExternalLinkAnnotation"/> linking the specified <paramref name="rect"/> to the external <paramref name="destination"/>.
        /// </summary>
        /// <param name="rect">The annotation rectangle.</param>
        /// <param name="destination">The external destination (e.g., a web URI).</param>
        /// <param name="borderWidth">Thickness of the annotation border.</param>
        /// <param name="borderDash">Dash style for the annotation border.</param>
        /// <param name="borderColour">Colour for the annotation border.</param>
        public PDFExternalLinkAnnotation(Rectangle rect, string destination, double borderWidth, LineDash? borderDash, Colour borderColour) : base(rect, borderWidth, borderDash, borderColour)
        {
            this.A = new PDFURIAction(destination);
        }
    }

    /// <summary>
    /// A PDF link annotation referring to a destination within the document.
    /// </summary>
    internal class PDFInternalLinkAnnotation : PDFLinkAnnotation
    {
        /// <summary>
        /// The link destination.
        /// </summary>
        public PDFArray<IPDFObject> Dest { get; }

        /// <summary>
        /// Create a new <see cref="PDFInternalLinkAnnotation"/> linking the specified <paramref name="rect"/> to the <paramref name="destinationX"/> and <paramref name="destinationY"/> coordinates on the <paramref name="destinationPage"/>.
        /// </summary>
        /// <param name="rect">The annotation rectangle.</param>
        /// <param name="destinationPage">The destination page.</param>
        /// <param name="destinationX">The X coordinate on the <paramref name="destinationPage"/>.</param>
        /// <param name="destinationY">The Y coordinate on the <paramref name="destinationPage"/>.</param>
        /// <param name="borderWidth">Thickness of the annotation border.</param>
        /// <param name="borderDash">Dash style for the annotation border.</param>
        /// <param name="borderColour">Colour for the annotation border.</param>
        public PDFInternalLinkAnnotation(Rectangle rect, PDFPage destinationPage, double destinationX, double destinationY, double borderWidth, LineDash? borderDash, Colour borderColour) : base(rect, borderWidth, borderDash, borderColour)
        {
            this.Dest = new PDFArray<IPDFObject>(destinationPage, new PDFString("XYZ", PDFString.StringDelimiter.StartingForwardSlash), new PDFDouble(destinationX), new PDFDouble(destinationY), new PDFDouble(0));
        }
    }
}
