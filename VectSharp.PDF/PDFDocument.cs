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
using System.Text;
using VectSharp.PDF.PDFObjects;

namespace VectSharp.PDF
{
    /// <summary>
    /// A low-level representation of a PDF document.
    /// </summary>
    public class PDFDocument
    {
        /// <summary>
        /// Objects contained in the PDF document.
        /// </summary>
        public IList<PDFReferenceableObject> Contents { get; }

        /// <summary>
        /// PDF specification version. This is only included in the header and not enforced.
        /// </summary>
        public string PDFVersion { get; set; } = "1.4";

        /// <summary>
        /// Gets or sets the PDF catalog object.
        /// </summary>
        public PDFCatalog Catalog
        {
            get
            {
                bool found = false;
                PDFCatalog tbr = null;

                foreach (PDFReferenceableObject obj in Contents)
                {
                    if (obj is PDFCatalog cat)
                    {
                        if (!found)
                        {
                            found = true;
                            tbr = cat;
                        }
                        else
                        {
                            throw new ArgumentException("The document contains multiple catalog objects!");
                        }
                    }
                }

                if (found)
                {
                    return tbr;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The document does not contain a catalog object!");
                }
            }

            set
            {
                bool found = false;
                int catalogIndex = -1;

                for (int i = 0; i < Contents.Count; i++)
                {
                    if (Contents[i] is PDFCatalog)
                    {
                        if (!found)
                        {
                            found = true;
                            catalogIndex = i;
                        }
                        else
                        {
                            throw new ArgumentException("The document contains multiple catalog objects!");
                        }
                    }
                }

                if (found)
                {
                    Contents[catalogIndex] = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The document does not contain a catalog object!");
                }
            }
        }

        /// <summary>
        /// Gets or sets the PDF document info object.
        /// </summary>
        public PDFDocumentInfo Info
        {
            get
            {
                bool found = false;
                PDFDocumentInfo tbr = null;

                foreach (PDFReferenceableObject obj in Contents)
                {
                    if (obj is PDFDocumentInfo info)
                    {
                        if (!found)
                        {
                            found = true;
                            tbr = info;
                        }
                        else
                        {
                            throw new ArgumentException("The document contains multiple document info objects!");
                        }
                    }
                }

                if (found)
                {
                    return tbr;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                bool found = false;
                int infoIndex = -1;

                for (int i = 0; i < Contents.Count; i++)
                {
                    if (Contents[i] is PDFDocumentInfo)
                    {
                        if (!found)
                        {
                            found = true;
                            infoIndex = i;
                        }
                        else
                        {
                            throw new ArgumentException("The document contains multiple document info objects!");
                        }
                    }
                }
                if (value != null)
                {
                    if (found)
                    {
                        Contents[infoIndex] = value;
                    }
                    else
                    {
                        Contents.Add(value);
                    }
                }
                else if (found)
                {
                    Contents.RemoveAt(infoIndex);
                }
            }
        }

        /// <summary>
        /// Gets the document page list.
        /// </summary>
        public List<PDFPage> Pages
        {
            get => this.Catalog.Pages.Kids.Values;
        }

        /// <summary>
        /// Default document encoding.
        /// </summary>
        public static readonly Encoding DefaultEncoding = Encoding.GetEncoding("ISO-8859-1");

        /// <summary>
        /// Write the PDF document to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream on which the PDF document should be written.</param>
        /// <param name="encoding">The document encoding (default: ISO 8859-1).</param>
        public void Write(Stream stream, Encoding encoding = default)
        {
            if (encoding == null)
            {
                encoding = DefaultEncoding;
            }

            for (int i = 0; i < this.Contents.Count; i++)
            {
                this.Contents[i].ObjectNumber = i + 1;
            }

            long[] objectPositions = new long[this.Contents.Count];

            StreamWriter sw = new StreamWriter(stream, encoding, 1024, true);

            //Header
            sw.Write("%PDF-" + this.PDFVersion + "\n");
            sw.Flush();

            for (int i = 0; i < this.Contents.Count; i++)
            {
                objectPositions[i] = stream.Position;
                sw.Write(this.Contents[i].ObjectNumber.ToString() + " " + this.Contents[i].Generation.ToString() + " obj\n");
                sw.Flush();
                this.Contents[i].FullWrite(stream, sw);
                sw.Write("\nendobj\n");
                sw.Flush();
            }

            long startXref = stream.Position;

            //XRef
            sw.Write("\nxref\n0 " + (objectPositions.Length + 1).ToString() + "\n0000000000 65535 f \n");
            for (int i = 0; i < objectPositions.Length; i++)
            {
                sw.Write(objectPositions[i].ToString("0000000000", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
            }

            //Trailer

            sw.Write("trailer\n");
            sw.Flush();
            PDFTrailer trailer = new PDFTrailer(objectPositions.Length + 1, this.Catalog, this.Info);
            trailer.Write(stream, sw);
            
            sw.Write("\nstartxref\n" + startXref.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + "\n%%EOF\n");

            sw.Flush();
            sw.Dispose();
        }

        /// <summary>
        /// Write the PDF document to the specified file.
        /// </summary>
        /// <param name="file">The file on which the PDF document should be written.</param>
        /// <param name="encoding">The document encoding (default: ISO 8859-1).</param>
        public void Write(string file, Encoding encoding = default)
        {
            using (FileStream fs = new FileStream(file, FileMode.Create))
            {
                this.Write(fs, encoding);
            }
        }

        /// <summary>
        /// Create a new <see cref="PDFDocument"/> containing the specified <paramref name="pdfObjects"/>.
        /// </summary>
        /// <param name="pdfObjects">The objects contained in the PDF document.</param>
        public PDFDocument(IList<PDFReferenceableObject> pdfObjects)
        {
            this.Contents = pdfObjects;
        }
    }
}
