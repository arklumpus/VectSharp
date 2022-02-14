/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2022 Giorgio Bianchini

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

using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace VectSharp.ImageSharpUtils
{
    /// <summary>
    /// Provides a method to parse an image URI into a page.
    /// </summary>
    public static class ImageURIParser
    {
        /// <summary>
        /// Parses an image URI into a page. This is intended to replace the default image URI interpreter in <c>VectSharp.SVG.Parser.ParseImageURI</c>. To do this, use something like:
        /// <code>VectSharp.SVG.Parser.ParseImageURI = VectSharp.ImageSharpUtils.ImageURIParser.Parser(VectSharp.SVG.Parser.ParseSVGURI);</code>
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

                        GCHandle handle = GCHandle.Alloc(parsed, GCHandleType.Pinned);

                        RasterImageStream img = new RasterImageStream(handle.AddrOfPinnedObject(), parsed.Length, interpolate: interpolate);

                        handle.Free();

                        Page pag = new Page(img.Width, img.Height);

                        pag.Graphics.DrawRasterImage(0, 0, img);

                        return pag;
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
