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

using System;
using System.Collections.Generic;
using System.IO;

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// Represents a PDF document trailer.
    /// </summary>
    public class PDFTrailer : PDFDictionary
    {
        /// <summary>
        /// The number of objects in the cross-reference table.
        /// </summary>
        public PDFInt Size { get; }

        /// <summary>
        /// The root catalog object.
        /// </summary>
        public PDFCatalog Root { get; }

        /// <summary>
        /// The document information dictionary.
        /// </summary>
        public PDFDocumentInfo Info { get; }

        /// <summary>
        /// Create a new <see cref="PDFTrailer"/>.
        /// </summary>
        /// <param name="size">The number of objects in the cross-reference table.</param>
        /// <param name="root">The root catalog object.</param>
        /// <param name="info">The document information dictionary.</param>
        public PDFTrailer(int size, PDFCatalog root, PDFDocumentInfo info)
        {
            this.Size = new PDFInt(size);
            this.Root = root;
            this.Info = info;
        }
    }

    /// <summary>
    /// Represents a document information dictionary.
    /// </summary>
    public class PDFDocumentInfo : PDFDictionary
    {
        /// <summary>
        /// The document title.
        /// </summary>
        public PDFString Title { get; set; }

        /// <summary>
        /// The person who created the document.
        /// </summary>
        public PDFString Author { get; set; }

        /// <summary>
        /// The subject of the document.
        /// </summary>
        public PDFString Subject { get; set; }

        /// <summary>
        /// Keywords associated with the document.
        /// </summary>
        public PDFString Keywords { get; set; }

        /// <summary>
        /// The program used to create the original version of the document (e.g., VectSharp).
        /// </summary>
        public PDFString Creator { get; set; }

        /// <summary>
        /// The program used to create the PDF document (e.g., VectSharp.PDF).
        /// </summary>
        public PDFString Producer { get; set; }

        /// <summary>
        /// The date when the PDF file was created.
        /// </summary>
        public PDFDate CreationDate { get; set; }

        /// <summary>
        /// The date when the PDF file was last modified.
        /// </summary>
        public PDFDate ModDate { get; set; }

        /// <summary>
        /// Additional custom properties.
        /// </summary>
        public Dictionary<string, IPDFObject> CustomProperties { get; } = new Dictionary<string, IPDFObject>();

        /// <summary>
        /// Create a new <see cref="PDFDocumentInfo"/> object.
        /// </summary>
        public PDFDocumentInfo()
        {

        }

        /// <inheritdoc/>
        public override void FullWrite(Stream stream, StreamWriter writer)
        {
            Dictionary<string, Func<PDFDictionary, IPDFObject>> getters = Getters[this.GetType().FullName];

            writer.Write("<<");
            foreach (KeyValuePair<string, Func<PDFDictionary, IPDFObject>> kvp in getters)
            {
                IPDFObject val = kvp.Value(this);

                if (val != null)
                {
                    writer.Write(" /");
                    writer.Write(kvp.Key);
                    writer.Write(" ");
                    writer.Flush();
                    val.Write(stream, writer);
                }
            }

            foreach (KeyValuePair<string, IPDFObject> kvp in CustomProperties)
            {
                writer.Write(" /");
                writer.Write(kvp.Key);
                writer.Write(" ");

                writer.Flush();
                kvp.Value.Write(stream, writer);
            }

            writer.Write(" >>");
            writer.Flush();
        }
    }
}
