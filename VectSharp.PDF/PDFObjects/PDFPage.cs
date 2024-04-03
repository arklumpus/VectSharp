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

using System.Collections.Generic;

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// Represents a collection of <see cref="PDFPage"/> objects.
    /// </summary>
    public class PDFPages : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("Pages", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// Pages.
        /// </summary>
        public PDFArray<PDFPage> Kids { get; }

        /// <summary>
        /// Number of pages in the collection.
        /// </summary>
        public PDFInt Count { get; }

        /// <summary>
        /// Create a new <see cref="PDFPages"/> collection.
        /// </summary>
        /// <param name="pages">The pages contained in the collection.</param>
        public PDFPages(IEnumerable<PDFPage> pages)
        {
            this.Kids = new PDFArray<PDFPage>(pages);
            this.Count = new PDFInt(this.Kids.Values.Count);
        }
    }

    /// <summary>
    /// A PDF document catalog.
    /// </summary>
    public class PDFCatalog : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("Catalog", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// <see cref="PDFPages"/> object containing the pages of the document.
        /// </summary>
        public PDFPages Pages { get; }

        /// <summary>
        /// Document outline (table of contents).
        /// </summary>
        public PDFOutline Outlines { get; set; }

        /// <summary>
        /// Viewer preferences dictionary.
        /// </summary>
        public PDFRawDictionary ViewerPreferences { get; set; }

        /// <summary>
        /// Optional content properties.
        /// </summary>
        public PDFOptionalContentProperties OCProperties { get; set; }

        /// <summary>
        /// Page mode.
        /// </summary>
        public PDFString PageMode { get; set; }

        /// <summary>
        /// Create a new <see cref="PDFCatalog"/>.
        /// </summary>
        /// <param name="pages"><see cref="PDFPages"/> object containing the pages of the document.</param>
        public PDFCatalog(PDFPages pages)
        {
            this.Pages = pages;
        }
    }

    /// <summary>
    /// Represents a single PDF document page.
    /// </summary>
    public class PDFPage : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("Page", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// <see cref="PDFPages"/> object containing this page.
        /// </summary>
        public PDFPages Parent { get; set; }

        /// <summary>
        /// Bounding box for the page.
        /// </summary>
        public PDFArray<PDFDouble> MediaBox { get; }

        /// <summary>
        /// Page/document resource container.
        /// </summary>
        public PDFResources Resources { get; }

        /// <summary>
        /// Page content stream.
        /// </summary>
        public PDFStream Contents { get; }

        /// <summary>
        /// Page annotations.
        /// </summary>
        public PDFArray<PDFAnnotation> Annots { get; set; }

        /// <summary>
        /// Create a new <see cref="PDFPage"/>.
        /// </summary>
        /// <param name="width">Page width.</param>
        /// <param name="height">Page height.</param>
        /// <param name="resources">Page/document resource container.</param>
        /// <param name="pageContents">Page content stream.</param>
        public PDFPage(double width, double height, PDFResources resources, PDFStream pageContents)
        {
            this.MediaBox = new PDFArray<PDFDouble>(new PDFDouble(0), new PDFDouble(0), new PDFDouble(width), new PDFDouble(height));
            this.Resources = resources;
            this.Contents = pageContents;
            this.Annots = null;
        }

        /// <summary>
        /// Add an annotation to the page.
        /// </summary>
        /// <param name="annot">Annotation to add.</param>
        public void AddAnnotation(PDFAnnotation annot)
        {
            if (this.Annots == null)
            {
                this.Annots = new PDFArray<PDFAnnotation>();
            }

            this.Annots.Values.Add(annot);
        }
    }
}
