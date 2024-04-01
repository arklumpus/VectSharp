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
using VectSharp.PDF.PDFObjects;

namespace VectSharp.PDF
{
    /// <summary>
    /// Represents metadata for a PDF document.
    /// </summary>
    public class PDFMetadata
    {
        /// <summary>
        /// Indicates that no metadata should be included in PDF document.
        /// </summary>
        public bool ExcludeMetadata { get; set; } = false;
        /// <summary>
        /// The document title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The person who created the document.
        /// </summary>
        public string Author { get; set; } = Environment.UserName;

        /// <summary>
        /// The subject of the document.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Keywords associated with the document.
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// The program used to create the original version of the document (e.g., VectSharp).
        /// </summary>
        public string Creator { get; set; } = "VectSharp v" + typeof(Document).Assembly.GetName().Version.ToString(3);

        /// <summary>
        /// The program used to create the PDF document (e.g., VectSharp.PDF).
        /// </summary>
        public string Producer { get; set; } = "VectSharp.PDF v" + typeof(PDFMetadata).Assembly.GetName().Version.ToString(3);

        /// <summary>
        /// The date when the PDF file was created.
        /// </summary>
        public DateTime? CreationDate { get; set; } = DateTime.Now;

        /// <summary>
        /// The time zone where the PDF file was created.
        /// </summary>
        public TimeZoneInfo CreationDateTimeZone { get; set; } = TimeZoneInfo.Local;

        /// <summary>
        /// The date when the PDF file was last modified.
        /// </summary>
        public DateTime? ModificationDate { get; set; } = DateTime.Now;

        /// <summary>
        /// The time zone where the PDF file was last modified.
        /// </summary>
        public TimeZoneInfo ModificationDateTimeZone { get; set; } = TimeZoneInfo.Local;

        /// <summary>
        /// Custom metadata properties to include in the PDF document.
        /// </summary>
        public Dictionary<string, object> CustomProperties { get; set; }

        /// <summary>
        /// Create a new PDF metadata object.
        /// </summary>
        public PDFMetadata()
        {

        }

        internal PDFDocumentInfo ToPDFDocumentInfo()
        {
            if (this.ExcludeMetadata)
            {
                return null;
            }
            else
            {
                PDFDocumentInfo tbr = new PDFDocumentInfo();

                if (!string.IsNullOrEmpty(Title))
                {
                    tbr.Title = new PDFString(this.Title, PDFString.StringDelimiter.Brackets);
                }

                if (!string.IsNullOrEmpty(Author))
                {
                    tbr.Author = new PDFString(this.Author, PDFString.StringDelimiter.Brackets);
                }

                if (!string.IsNullOrEmpty(Subject))
                {
                    tbr.Subject = new PDFString(this.Subject, PDFString.StringDelimiter.Brackets);
                }

                if (!string.IsNullOrEmpty(Keywords))
                {
                    tbr.Keywords = new PDFString(this.Keywords, PDFString.StringDelimiter.Brackets);
                }

                if (!string.IsNullOrEmpty(Creator))
                {
                    tbr.Creator = new PDFString(this.Creator, PDFString.StringDelimiter.Brackets);
                }

                if (!string.IsNullOrEmpty(Producer))
                {
                    tbr.Producer = new PDFString(this.Producer, PDFString.StringDelimiter.Brackets);
                }

                if (this.CreationDate != null)
                {
                    tbr.CreationDate = new PDFDate(this.CreationDate.Value, this.CreationDateTimeZone ?? TimeZoneInfo.Utc);
                }

                if (this.ModificationDate != null)
                {
                    tbr.ModDate = new PDFDate(this.ModificationDate.Value, this.ModificationDateTimeZone ?? TimeZoneInfo.Utc);
                }

                foreach (KeyValuePair<string, object> kvp in this.CustomProperties)
                {
                    if (kvp.Value != null)
                    {
                        if (kvp.Value is string s)
                        {
                            tbr.CustomProperties[kvp.Key] = new PDFString(s, PDFString.StringDelimiter.Brackets);
                        }
                        else if (kvp.Value is double d)
                        {
                            tbr.CustomProperties[kvp.Key] = new PDFString(d.ToString(System.Globalization.CultureInfo.InvariantCulture), PDFString.StringDelimiter.Brackets);
                        }
                        else if (kvp.Value is DateTime t)
                        {
                            tbr.CustomProperties[kvp.Key] = new PDFDate(t, TimeZoneInfo.Utc);
                        }
                        else
                        {
                            tbr.CustomProperties[kvp.Key] = new PDFString(kvp.Value.ToString(), PDFString.StringDelimiter.Brackets);
                        }
                    }
                }

                return tbr;
            }
        }

        /// <summary>
        /// Returns an object that does not include any metadata in the PDF file.
        /// </summary>
        /// <returns>An object that does not include any metadata in the PDF file.</returns>
        public static PDFMetadata NoMetadata()
        {
            return new PDFMetadata() { ExcludeMetadata = true };
        }
    }
}
