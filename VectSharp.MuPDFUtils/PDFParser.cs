/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2024  Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using MuPDFCore;
using System;
using System.Collections.Generic;
using System.IO;
using VectSharp.SVG;

namespace VectSharp.MuPDFUtils
{
    /// <summary>
    /// Contains methods to import PDF documents as vectors.
    /// </summary>
    public static class PDFParser
    {
        private static Page FromMuPDFDocument(MuPDFContext context, MuPDFDocument document, int pageNumber, SVGCreationOptions.TextOption textOption, bool includeAnnotations, Dictionary<string, MuPDFLinkDestination> linkDestinations)
        {
            string tempFile = Path.GetTempFileName();

            try
            {
                MuPDFDocument.Create.SVGDocument(context, tempFile, document.Pages[pageNumber], new SVGCreationOptions() { TextRendering = textOption, IncludeAnnotations = includeAnnotations });
                Parser.ParseImageURI = ImageURIParser.Parser(Parser.ParseSVGURI);

                Page parsedPage = Parser.FromFile(tempFile);

                if (linkDestinations != null)
                {
                    foreach (MuPDFLink link in document.Pages[pageNumber].Links)
                    {
                        if (link.IsVisible)
                        {
                            string tag = Guid.NewGuid().ToString("N");
                            parsedPage.Graphics.FillRectangle(link.ActiveArea.X0, link.ActiveArea.Y0, link.ActiveArea.Width, link.ActiveArea.Height, Colour.FromRgba(0, 0, 0, 0), tag: tag);
                            linkDestinations.Add(tag, link.Destination);
                        }
                    }
                }

                return parsedPage;
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        private static Dictionary<string, string> SetUpLinks(Page page, int pageNumber, Dictionary<string, MuPDFLinkDestination> linkDestinations)
        {
            Dictionary<string, string> tbr = new Dictionary<string, string>();

            foreach (KeyValuePair<string, MuPDFLinkDestination> link in linkDestinations)
            {
                if (link.Value.Type == MuPDFLinkDestination.DestinationType.External)
                {
                    tbr.Add(link.Key, (link.Value as MuPDFExternalLinkDestination).Uri);
                }
                else if (link.Value.Type == MuPDFLinkDestination.DestinationType.Internal)
                {
                    MuPDFInternalLinkDestination intern = link.Value as MuPDFInternalLinkDestination;
                    if (intern.PageNumber == pageNumber)
                    {
                        string tag = Guid.NewGuid().ToString("N");

                        switch (intern.InternalType)
                        {
                            case MuPDFInternalLinkDestination.InternalDestinationType.XYZoom:
                                page.Graphics.FillRectangle(intern.X, intern.Y, 1, 1, Colour.FromRgba(0, 0, 0, 0), tag);
                                break;
                            case MuPDFInternalLinkDestination.InternalDestinationType.FitRectangle:
                                page.Graphics.FillRectangle(intern.X, intern.Y, intern.Width, intern.Height, Colour.FromRgba(0, 0, 0, 0), tag);
                                break;
                            case MuPDFInternalLinkDestination.InternalDestinationType.Fit:
                            case MuPDFInternalLinkDestination.InternalDestinationType.FitBoundingBox:
                                page.Graphics.FillRectangle(0, 0, page.Width, page.Height, Colour.FromRgba(0, 0, 0, 0), tag);
                                break;
                            case MuPDFInternalLinkDestination.InternalDestinationType.FitWidth:
                            case MuPDFInternalLinkDestination.InternalDestinationType.FitBoundingBoxWidth:
                                page.Graphics.FillRectangle(0, intern.Y, page.Width, page.Height - intern.Y, Colour.FromRgba(0, 0, 0, 0), tag);
                                break;
                            case MuPDFInternalLinkDestination.InternalDestinationType.FitHeight:
                            case MuPDFInternalLinkDestination.InternalDestinationType.FitBoundingBoxHeight:
                                page.Graphics.FillRectangle(intern.X, 0, page.Width - intern.X, page.Height, Colour.FromRgba(0, 0, 0, 0), tag);
                                break;
                        }

                        tbr.Add(link.Key, "#" + tag);
                    }
                }
            }

            return tbr;
        }

        private static Dictionary<string, string> SetUpLinks(IReadOnlyList<Page> pages, Dictionary<string, MuPDFLinkDestination> linkDestinations)
        {
            Dictionary<string, string> tbr = new Dictionary<string, string>();

            foreach (KeyValuePair<string, MuPDFLinkDestination> link in linkDestinations)
            {
                if (link.Value.Type == MuPDFLinkDestination.DestinationType.External)
                {
                    tbr.Add(link.Key, (link.Value as MuPDFExternalLinkDestination).Uri);
                }
                else if (link.Value.Type == MuPDFLinkDestination.DestinationType.Internal)
                {
                    MuPDFInternalLinkDestination intern = link.Value as MuPDFInternalLinkDestination;
                    Page page = pages[intern.PageNumber];

                    string tag = Guid.NewGuid().ToString("N");

                    switch (intern.InternalType)
                    {
                        case MuPDFInternalLinkDestination.InternalDestinationType.XYZoom:
                            page.Graphics.FillRectangle(intern.X, intern.Y, 1, 1, Colour.FromRgba(0, 0, 0, 0), tag);
                            break;
                        case MuPDFInternalLinkDestination.InternalDestinationType.FitRectangle:
                            page.Graphics.FillRectangle(intern.X, intern.Y, intern.Width, intern.Height, Colour.FromRgba(0, 0, 0, 0), tag);
                            break;
                        case MuPDFInternalLinkDestination.InternalDestinationType.Fit:
                        case MuPDFInternalLinkDestination.InternalDestinationType.FitBoundingBox:
                            page.Graphics.FillRectangle(0, 0, page.Width, page.Height, Colour.FromRgba(0, 0, 0, 0), tag);
                            break;
                        case MuPDFInternalLinkDestination.InternalDestinationType.FitWidth:
                        case MuPDFInternalLinkDestination.InternalDestinationType.FitBoundingBoxWidth:
                            page.Graphics.FillRectangle(0, intern.Y, page.Width, page.Height - intern.Y, Colour.FromRgba(0, 0, 0, 0), tag);
                            break;
                        case MuPDFInternalLinkDestination.InternalDestinationType.FitHeight:
                        case MuPDFInternalLinkDestination.InternalDestinationType.FitBoundingBoxHeight:
                            page.Graphics.FillRectangle(intern.X, 0, page.Width - intern.X, page.Height, Colour.FromRgba(0, 0, 0, 0), tag);
                            break;
                    }

                    tbr.Add(link.Key, "#" + tag);
                }
            }

            return tbr;
        }


        /// <summary>
        /// Import the specified page from a document.
        /// </summary>
        /// <param name="fileName">The path to the document file.</param>
        /// <param name="pageNumber">The page number (starting at 0).</param>
        /// <param name="textOption">This parameter determines whether text elements are rendered as paths (which provides more accurate results) or as text.</param>
        /// <param name="includeAnnotations">Whether annotations (e.g., signatures) should be included in the imported document.</param>
        /// <returns>A <see cref="Page"/> containing a representation of the specified page from the document.</returns>
        public static Page FromFile(string fileName, int pageNumber, SVGCreationOptions.TextOption textOption = SVGCreationOptions.TextOption.TextAsPath, bool includeAnnotations = true)
        {
            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument document = new MuPDFDocument(context, fileName))
                {
                    return FromMuPDFDocument(context, document, pageNumber, textOption, includeAnnotations, null);
                }
            }
        }

        /// <summary>
        /// Import the specified page from a document.
        /// </summary>
        /// <param name="fileName">The path to the document file.</param>
        /// <param name="pageNumber">The page number (starting at 0).</param>
        /// <param name="linkDestinations">When this method returns, this variable will contain a dictionary used to associate graphic action tags to hyperlinks. This can be used to enable such links when rendering the <see cref="Page"/> to a file.</param>
        /// <param name="textOption">This parameter determines whether text elements are rendered as paths (which provides more accurate results) or as text.</param>
        /// <param name="includeAnnotations">Whether annotations (e.g., signatures) should be included in the imported document.</param>
        /// <returns>A <see cref="Page"/> containing a representation of the specified page from the document.</returns>
        public static Page FromFile(string fileName, int pageNumber, out Dictionary<string, string> linkDestinations, SVGCreationOptions.TextOption textOption = SVGCreationOptions.TextOption.TextAsPath, bool includeAnnotations = true)
        {
            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument document = new MuPDFDocument(context, fileName))
                {
                    Dictionary<string, MuPDFLinkDestination> dests = new Dictionary<string, MuPDFLinkDestination>();
                    Page pag = FromMuPDFDocument(context, document, pageNumber, textOption, includeAnnotations, dests);
                    linkDestinations = SetUpLinks(pag, pageNumber, dests);
                    return pag;
                }
            }
        }

        /// <summary>
        /// Import the specified page from a document.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the document to import.</param>
        /// <param name="pageNumber">The page number (starting at 0).</param>
        /// <param name="fileType">The type of document contained in the <see cref="Stream"/>.</param>
        /// <param name="textOption">This parameter determines whether text elements are rendered as paths (which provides more accurate results) or as text.</param>
        /// <param name="includeAnnotations">Whether annotations (e.g., signatures) should be included in the imported document.</param>
        /// <returns>A <see cref="Page"/> containing a representation of the specified page from the document.</returns>
        public static Page FromStream(Stream stream, int pageNumber, InputFileTypes fileType = InputFileTypes.PDF, SVGCreationOptions.TextOption textOption = SVGCreationOptions.TextOption.TextAsPath, bool includeAnnotations = true)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument document = new MuPDFDocument(context, ref ms, fileType))
                {
                    return FromMuPDFDocument(context, document, pageNumber, textOption, includeAnnotations, null);
                }
            }
        }

        /// <summary>
        /// Import the specified page from a document.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the document to import.</param>
        /// <param name="pageNumber">The page number (starting at 0).</param>
        /// <param name="linkDestinations">When this method returns, this variable will contain a dictionary used to associate graphic action tags to hyperlinks. This can be used to enable such links when rendering the <see cref="Page"/> to a file.</param>
        /// <param name="fileType">The type of document contained in the <see cref="Stream"/>.</param>
        /// <param name="textOption">This parameter determines whether text elements are rendered as paths (which provides more accurate results) or as text.</param>
        /// <param name="includeAnnotations">Whether annotations (e.g., signatures) should be included in the imported document.</param>
        /// <returns>A <see cref="Page"/> containing a representation of the specified page from the document.</returns>
        public static Page FromStream(Stream stream, int pageNumber, out Dictionary<string, string> linkDestinations, InputFileTypes fileType = InputFileTypes.PDF, SVGCreationOptions.TextOption textOption = SVGCreationOptions.TextOption.TextAsPath, bool includeAnnotations = true)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument document = new MuPDFDocument(context, ref ms, fileType))
                {
                    Dictionary<string, MuPDFLinkDestination> dests = new Dictionary<string, MuPDFLinkDestination>();
                    Page pag = FromMuPDFDocument(context, document, pageNumber, textOption, includeAnnotations, dests);
                    linkDestinations = SetUpLinks(pag, pageNumber, dests);
                    return pag;
                }
            }
        }

        /// <summary>
        /// Import all pages from a document.
        /// </summary>
        /// <param name="fileName">The path to the document file.</param>
        /// <param name="textOption">This parameter determines whether text elements are rendered as paths (which provides more accurate results) or as text.</param>
        /// <param name="includeAnnotations">Whether annotations (e.g., signatures) should be included in the imported document.</param>
        /// <returns>A <see cref="Document"/> containing a representation of every page from the document.</returns>
        public static Document FromFile(string fileName, SVGCreationOptions.TextOption textOption = SVGCreationOptions.TextOption.TextAsPath, bool includeAnnotations = true)
        {
            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument document = new MuPDFDocument(context, fileName))
                {
                    Document doc = new Document();

                    for (int i = 0; i < document.Pages.Count; i++)
                    {
                        doc.Pages.Add(FromMuPDFDocument(context, document, i, textOption, includeAnnotations, null));
                    }

                    return doc;
                }
            }
        }

        /// <summary>
        /// Import all pages from a document.
        /// </summary>
        /// <param name="fileName">The path to the document file.</param>
        /// <param name="linkDestinations">When this method returns, this variable will contain a dictionary used to associate graphic action tags to hyperlinks. This can be used to enable such links when rendering the <see cref="Page"/> to a file.</param>
        /// <param name="textOption">This parameter determines whether text elements are rendered as paths (which provides more accurate results) or as text.</param>
        /// <param name="includeAnnotations">Whether annotations (e.g., signatures) should be included in the imported document.</param>
        /// <returns>A <see cref="Document"/> containing a representation of every page from the document.</returns>
        public static Document FromFile(string fileName, out Dictionary<string, string> linkDestinations, SVGCreationOptions.TextOption textOption = SVGCreationOptions.TextOption.TextAsPath, bool includeAnnotations = true)
        {
            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument document = new MuPDFDocument(context, fileName))
                {
                    Document doc = new Document();

                    Dictionary<string, MuPDFLinkDestination> dest = new Dictionary<string, MuPDFLinkDestination>();

                    for (int i = 0; i < document.Pages.Count; i++)
                    {
                        doc.Pages.Add(FromMuPDFDocument(context, document, i, textOption, includeAnnotations, dest));
                    }

                    linkDestinations = SetUpLinks(doc.Pages, dest);

                    return doc;
                }
            }
        }

        /// <summary>
        /// Import all pages from a document.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the document to import.</param>
        /// <param name="fileType">The type of document contained in the <see cref="Stream"/>.</param>
        /// <param name="textOption">This parameter determines whether text elements are rendered as paths (which provides more accurate results) or as text.</param>
        /// <param name="includeAnnotations">Whether annotations (e.g., signatures) should be included in the imported document.</param>
        /// <returns>A <see cref="Document"/> containing a representation of every page from the document.</returns>
        public static Document FromStream(Stream stream, InputFileTypes fileType = InputFileTypes.PDF, SVGCreationOptions.TextOption textOption = SVGCreationOptions.TextOption.TextAsPath, bool includeAnnotations = true)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument document = new MuPDFDocument(context, ref ms, fileType))
                {
                    Document doc = new Document();

                    for (int i = 0; i < document.Pages.Count; i++)
                    {
                        doc.Pages.Add(FromMuPDFDocument(context, document, i, textOption, includeAnnotations, null));
                    }

                    return doc;
                }
            }
        }

        /// <summary>
        /// Import all pages from a document.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the document to import.</param>
        /// <param name="linkDestinations">When this method returns, this variable will contain a dictionary used to associate graphic action tags to hyperlinks. This can be used to enable such links when rendering the <see cref="Page"/> to a file.</param>
        /// <param name="fileType">The type of document contained in the <see cref="Stream"/>.</param>
        /// <param name="textOption">This parameter determines whether text elements are rendered as paths (which provides more accurate results) or as text.</param>
        /// <param name="includeAnnotations">Whether annotations (e.g., signatures) should be included in the imported document.</param>
        /// <returns>A <see cref="Document"/> containing a representation of every page from the document.</returns>
        public static Document FromStream(Stream stream, out Dictionary<string, string> linkDestinations, InputFileTypes fileType = InputFileTypes.PDF, SVGCreationOptions.TextOption textOption = SVGCreationOptions.TextOption.TextAsPath, bool includeAnnotations = true)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument document = new MuPDFDocument(context, ref ms, fileType))
                {
                    Document doc = new Document();

                    Dictionary<string, MuPDFLinkDestination> dest = new Dictionary<string, MuPDFLinkDestination>();

                    for (int i = 0; i < document.Pages.Count; i++)
                    {
                        doc.Pages.Add(FromMuPDFDocument(context, document, i, textOption, includeAnnotations, dest));
                    }

                    linkDestinations = SetUpLinks(doc.Pages, dest);

                    return doc;
                }
            }
        }
    }
}
