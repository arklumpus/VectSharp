/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020  Giorgio Bianchini

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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace VectSharp.MuPDFUtils
{
    /// <summary>
    /// Provides a method to parse an image URI into a page.
    /// </summary>
    public static class ImageURIParser
    {
        /// <summary>
        /// Parses an image URI into a page. This is intended to replace the default image URI interpreter in <c>VectSharp.SVG.Parser.ParseImageURI</c>. To do this, use something like:
        /// <code>VectSharp.SVG.Parser.ParseImageURI = VectSharp.MuPDFUtils.ImageURIParser.Parser(VectSharp.SVG.Parser.ParseSVGURI);</code>
        /// </summary>
        /// <param name="parseSVG">A function to parse an SVG image uri into a page. You should pass <c>VectSharp.SVG.Parser.ParseSVGURI</c> as this argument.</param>
        /// <returns>A function to parse an image URI into a page.</returns>
        public static Func<string, bool, Page> Parser(Func<string, bool, Page> parseSVG)
        {
            return (string uri, bool interpolate) =>
            {
                if (uri.StartsWith("data:"))
                {
                    string mimeType = uri.Substring(uri.IndexOf(":") + 1, uri.IndexOf(";") - uri.IndexOf(":") - 1);

                    string type = uri.Substring(uri.IndexOf(";") + 1, uri.IndexOf(",") - uri.IndexOf(";") - 1);

                    if (mimeType != "image/svg+xml")
                    {
                        int offset = uri.IndexOf(",") + 1;

                        byte[] parsed;

                        bool isVector = false;

                        InputFileTypes fileType;

                        switch (mimeType)
                        {
                            case "image/png":
                                fileType = InputFileTypes.PNG;
                                break;
                            case "image/jpeg":
                            case "image/jpg":
                                fileType = InputFileTypes.JPEG;
                                break;
                            case "image/gif":
                                fileType = InputFileTypes.GIF;
                                break;
                            case "image/bmp":
                                fileType = InputFileTypes.BMP;
                                break;
                            case "image/tiff":
                            case "image/tif":
                                fileType = InputFileTypes.TIFF;
                                break;
                            case "application/oxps":
                            case "application/vnd.ms-xpsdocument":
                                fileType = InputFileTypes.XPS;
                                isVector = true;
                                break;
                            case "application/x-cbz":
                                fileType = InputFileTypes.CBZ;
                                break;
                            case "application/epub+zip":
                                fileType = InputFileTypes.EPUB;
                                isVector = true;
                                break;
                            case "text/fb2+xml":
                                fileType = InputFileTypes.FB2;
                                break;
                            case "image/x-portable-anymap":
                                fileType = InputFileTypes.PNM;
                                break;
                            case "image/x-portable-arbitrarymap":
                                fileType = InputFileTypes.PAM;
                                break;
                            case "application/pdf":
                                fileType = InputFileTypes.PDF;
                                isVector = true;
                                break;
                            default:
                                fileType = InputFileTypes.PDF;
                                break;
                        }

                        string substring = uri.Substring(offset);


                        switch (type)
                        {
                            case "base64":
                                parsed = Convert.FromBase64String(uri.Substring(offset));
                                break;
                            case "":
                                parsed = (from el in System.Web.HttpUtility.UrlDecode(uri.Substring(offset)) select (byte)el).ToArray();
                                break;
                            default:
                                throw new InvalidDataException("Unknown data stream type!");
                        }

                        if (!isVector)
                        {
                            GCHandle handle = GCHandle.Alloc(parsed, GCHandleType.Pinned);

                            RasterImageStream img = new RasterImageStream(handle.AddrOfPinnedObject(), parsed.Length, fileType, interpolate: interpolate);

                            handle.Free();

                            Page pag = new Page(img.Width, img.Height);

                            pag.Graphics.DrawRasterImage(0, 0, img);

                            return pag;
                        }
                        else
                        {
                            string tempFile = Path.GetTempFileName();

                            using (MuPDFContext context = new MuPDFContext())
                            {
                                using (MuPDFDocument document = new MuPDFDocument(context, parsed, fileType))
                                {
                                    MuPDFDocument.CreateDocument(context, tempFile, DocumentOutputFileTypes.SVG, document.Pages[0]);
                                }
                            }

                            string tbr = "data:image/svg+xml;," + System.Web.HttpUtility.UrlEncode(File.ReadAllText(tempFile));

                            File.Delete(tempFile);

                            return parseSVG(tbr, interpolate);
                        }
                    }
                    else
                    {
                        return parseSVG(uri, interpolate);
                    }
                }

                return null;
            };
        }
    }
}
