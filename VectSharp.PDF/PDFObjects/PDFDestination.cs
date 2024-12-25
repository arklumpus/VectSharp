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

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// Represents an internal link or action destination.
    /// </summary>
    public class PDFDestination : PDFArray<IPDFObject>
    {
        /// <summary>
        /// Type of PDF destination.
        /// </summary>
        public enum PDFDestinationType
        {
            /// <summary>
            /// Specific coordinates and zoom level on a page.
            /// </summary>
            XYZ,

            /// <summary>
            /// Fit the specified page in the window.
            /// </summary>
            Fit,

            /// <summary>
            /// Fit the specified page horizontally (so that the page width is equal to the window width).
            /// </summary>
            FitH,

            /// <summary>
            /// Fit the specified page vertically (so that the page height is equal to the window height).
            /// </summary>
            FitV,

            /// <summary>
            /// Fit the specified rectangle in the window, so that the whole rectangle fits.
            /// </summary>
            FitR,

            /// <summary>
            /// Fit the bounding box of the specified page.
            /// </summary>
            FitB,

            /// <summary>
            /// Fit the bounding box of the specified page horizontally (so that the bounding box width is equal to the window width).
            /// </summary>
            FitBH,

            /// <summary>
            /// Fit the bounding box of the specified page vertically (so that the bounding box height is equal to the window height).
            /// </summary>
            FitBV
        }

        /// <summary>
        /// The destination page.
        /// </summary>
        public PDFPage Page => (PDFPage)Values[0];

        /// <summary>
        /// The type of destination.
        /// </summary>
        public PDFDestinationType Type
        {
            get
            {
                switch (((PDFString)Values[1]).Value)
                {
                    case "XYZ":
                        return PDFDestinationType.XYZ;
                    case "Fit":
                        return PDFDestinationType.Fit;
                    case "FitH":
                        return PDFDestinationType.FitH;
                    case "FitV":
                        return PDFDestinationType.FitV;
                    case "FitR":
                        return PDFDestinationType.FitR;
                    case "FitB":
                        return PDFDestinationType.FitB;
                    case "FitBH":
                        return PDFDestinationType.FitBH;
                    case "FitBV":
                        return PDFDestinationType.FitBV;
                    default:
                        throw new ArgumentException("Invalid PDF destination type: " + ((PDFString)Values[1]).Value);
                }
            }
        }

        /// <summary>
        /// The left coordinate of the destination, where applicable, or <see langword="null"/> otherwise.
        /// </summary>
        public PDFDouble Left
        {
            get
            {
                switch (Type)
                {
                    case PDFDestinationType.XYZ:
                    case PDFDestinationType.FitV:
                    case PDFDestinationType.FitR:
                    case PDFDestinationType.FitBV:
                        return (PDFDouble)Values[2];

                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// The right coordinate of the destination, where applicable, or <see langword="null"/> otherwise.
        /// </summary>
        public PDFDouble Right
        {
            get
            {
                switch (Type)
                {
                    case PDFDestinationType.FitR:
                        return (PDFDouble)Values[4];

                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// The bottom coordinate of the destination, where applicable, or <see langword="null"/> otherwise.
        /// </summary>
        public PDFDouble Bottom
        {
            get
            {
                switch (Type)
                {
                    case PDFDestinationType.FitR:
                        return (PDFDouble)Values[3];

                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// The top coordinate of the destination, where applicable, or <see langword="null"/> otherwise.
        /// </summary>
        public PDFDouble Top
        {
            get
            {
                switch (Type)
                {
                    case PDFDestinationType.XYZ:
                        return (PDFDouble)Values[3];

                    case PDFDestinationType.FitR:
                        return (PDFDouble)Values[5];

                    case PDFDestinationType.FitH:
                    case PDFDestinationType.FitBH:
                        return (PDFDouble)Values[2];

                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// The zoom level of the destination, where applicable, or <see langword="null"/> otherwise.
        /// </summary>
        public PDFDouble Zoom
        {
            get
            {
                switch (Type)
                {
                    case PDFDestinationType.XYZ:
                        return (PDFDouble)Values[4];

                    default:
                        return null;
                }
            }
        }

        private PDFDestination(params IPDFObject[] items) : base(items) { }

        /// <summary>
        /// Display the specified page, with the specified point at the upper-left corner of the window, and the specified zoom level. If any of <paramref name="left"/>, <paramref name="top"/>, or <paramref name="zoom"/> are <see langword="null"/>, the current value is left unchanged.
        /// </summary>
        /// <param name="page">The target page.</param>
        /// <param name="left">The left coordinate of the point.</param>
        /// <param name="top">The top coordinate of the point.</param>
        /// <param name="zoom">The zoom level.</param>
        /// <returns>The specified <see cref="PDFDestination"/>.</returns>
        public static PDFDestination XYZ(PDFPage page, PDFDouble left = null, PDFDouble top = null, PDFDouble zoom = null)
        {
            IPDFObject realLeft = left ?? new PDFString("null", PDFString.StringDelimiter.None);
            IPDFObject realTop = top ?? new PDFString("null", PDFString.StringDelimiter.None);

            if (zoom == null)
            {
                zoom = new PDFDouble(0);
            }

            return new PDFDestination(page, new PDFString("XYZ", PDFString.StringDelimiter.StartingForwardSlash), realLeft, realTop, zoom);
        }

        /// <summary>
        /// Display the specified page so that it fits completely within the window.
        /// </summary>
        /// <param name="page">The target page.</param>
        /// <returns>The specified <see cref="PDFDestination"/>.</returns>
        public static PDFDestination Fit(PDFPage page)
        {
            return new PDFDestination(page, new PDFString("Fit", PDFString.StringDelimiter.StartingForwardSlash));
        }

        /// <summary>
        /// Display the specified page so that its width fits completely within the window and with the specified <paramref name="top"/> coordinate at the top of the window. If <paramref name="top"/> is null, the current value is left unchanged.
        /// </summary>
        /// <param name="page">The target page.</param>
        /// <param name="top">The top coordinate of the page target.</param>
        /// <returns>The specified <see cref="PDFDestination"/>.</returns>
        public static PDFDestination FitH(PDFPage page, PDFDouble top = null)
        {
            IPDFObject realTop = top ?? new PDFString("null", PDFString.StringDelimiter.None);
            return new PDFDestination(page, new PDFString("FitH", PDFString.StringDelimiter.StartingForwardSlash), realTop);
        }

        /// <summary>
        /// Display the specified page so that its height fits completely within the window and with the specified <paramref name="left"/> coordinate at the left of the window. If <paramref name="left"/> is null, the current value is left unchanged.
        /// </summary>
        /// <param name="page">The target page.</param>
        /// <param name="left">The left coordinate of the page target.</param>
        /// <returns>The specified <see cref="PDFDestination"/>.</returns>
        public static PDFDestination FitV(PDFPage page, PDFDouble left = null)
        {
            IPDFObject realLeft = left ?? new PDFString("null", PDFString.StringDelimiter.None);
            return new PDFDestination(page, new PDFString("FitV", PDFString.StringDelimiter.StartingForwardSlash), realLeft);
        }

        /// <summary>
        /// Display the specified page so that the specified rectangle fits completely within the window.
        /// </summary>
        /// <param name="page">The target page.</param>
        /// <param name="left">The left coordinate of the rectangle.</param>
        /// <param name="bottom">The bottom coordinate of the rectangle. Note that (0,0) is the lower-left corner.</param>
        /// <param name="right">The right coordinate of the rectangle.</param>
        /// <param name="top">The top coordinate of the rectangle. Note that (0,0) is the lower-left corner.</param>
        /// <returns>The specified <see cref="PDFDestination"/>.</returns>
        public static PDFDestination FitR(PDFPage page, PDFDouble left, PDFDouble bottom, PDFDouble right, PDFDouble top)
        {
            return new PDFDestination(page, new PDFString("FitR", PDFString.StringDelimiter.StartingForwardSlash), left, bottom, right, top);
        }

        /// <summary>
        /// Display the specified page so that its bounding box fits completely within the window.
        /// </summary>
        /// <param name="page">The target page.</param>
        /// <returns>The specified <see cref="PDFDestination"/>.</returns>
        public static PDFDestination FitB(PDFPage page)
        {
            return new PDFDestination(page, new PDFString("FitB", PDFString.StringDelimiter.StartingForwardSlash));
        }

        /// <summary>
        /// Display the specified page so that the width of its bounding box fits completely within the window and with the specified <paramref name="top"/> coordinate at the top of the window. If <paramref name="top"/> is null, the current value is left unchanged.
        /// </summary>
        /// <param name="page">The target page.</param>
        /// <param name="top">The top coordinate of the page target.</param>
        /// <returns>The specified <see cref="PDFDestination"/>.</returns>
        public static PDFDestination FitBH(PDFPage page, PDFDouble top = null)
        {
            IPDFObject realTop = top ?? new PDFString("null", PDFString.StringDelimiter.None);
            return new PDFDestination(page, new PDFString("FitBH", PDFString.StringDelimiter.StartingForwardSlash), realTop);
        }

        /// <summary>
        /// Display the specified page so that the height of its bounding box fits completely within the window and with the specified <paramref name="left"/> coordinate at the left of the window. If <paramref name="left"/> is null, the current value is left unchanged.
        /// </summary>
        /// <param name="page">The target page.</param>
        /// <param name="left">The left coordinate of the page target.</param>
        /// <returns>The specified <see cref="PDFDestination"/>.</returns>
        public static PDFDestination FitBV(PDFPage page, PDFDouble left = null)
        {
            IPDFObject realLeft = left ?? new PDFString("null", PDFString.StringDelimiter.None);
            return new PDFDestination(page, new PDFString("FitBV", PDFString.StringDelimiter.StartingForwardSlash), realLeft);
        }
    }
}
