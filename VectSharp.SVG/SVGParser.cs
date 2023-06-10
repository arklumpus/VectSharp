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

using ExCSS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using VectSharp.Filters;

namespace VectSharp.SVG
{
    /// <summary>
    /// Contains methods to read an SVG image file.
    /// </summary>
    public static class Parser
    {
        static Parser()
        {
            ParseImageURI = ParseSVGURI;
        }

        /// <summary>
        /// A function that takes as input an image URI and a boolean value indicating whether the image should be interpolated, and returns a <see cref="Page"/> object containing the image.
        /// By default, this is equal to <see cref="ParseSVGURI"/>, i.e. it is only able to parse SVG images. If you wish to enable the parsing of other formats, you should install the "VectSharp.MuPDFUtils" NuGet package
        /// and enable the parser in your program by doing something like:
        /// <code>VectSharp.SVG.Parser.ParseImageURI = VectSharp.MuPDFUtils.ImageURIParser.Parser(VectSharp.SVG.Parser.ParseSVGURI);</code>
        /// </summary>
        public static Func<string, bool, Page> ParseImageURI;

        /// <summary>
        /// Parses an SVG image URI.
        /// </summary>
        /// <param name="uri">The image URI to parse.</param>
        /// <param name="ignored">This value is ignored and is only needed for compatibility.</param>
        /// <returns>A <see cref="Page"/> containing the parsed SVG image, or null.</returns>
        public static Page ParseSVGURI(string uri, bool ignored = false)
        {
            if (uri.StartsWith("data:"))
            {
                string mimeType = uri.Substring(uri.IndexOf(":") + 1, uri.IndexOf(";") - uri.IndexOf(":") - 1);

                string type = uri.Substring(uri.IndexOf(";") + 1, uri.IndexOf(",") - uri.IndexOf(";") - 1);

                if (mimeType == "image/svg+xml")
                {
                    int offset = uri.IndexOf(",") + 1;

                    string data;

                    switch (type)
                    {
                        case "base64":
                            data = Encoding.UTF8.GetString(Convert.FromBase64String(uri.Substring(offset)));
                            break;
                        case "":
                        case "charset=utf-8":
                        case "utf-8":
                        case "utf8":
                            data = System.Web.HttpUtility.UrlDecode(uri.Substring(offset));
                            break;
                        case "charset=ascii":
                        case "ascii":
                            data = System.Web.HttpUtility.UrlDecode(uri.Substring(offset));
                            break;
                        default:
                            throw new InvalidDataException("Unknown data stream type!");
                    }

                    try
                    {
                        StringReader sr = new StringReader(data);
                        string firstLine = sr.ReadLine();
                        sr.Dispose();

                        if (firstLine.StartsWith("<?xml") && firstLine.EndsWith("?>"))
                        {
                            data = data.Substring(firstLine.Length + 1);
                        }
                    }
                    catch { }

                    return FromString(data);
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Parses SVG source into a <see cref="Page"/> containing the image represented by the code.
        /// </summary>
        /// <param name="svgSource">The SVG source code.</param>
        /// <returns>A <see cref="Page"/> containing the image represented by the <paramref name="svgSource"/>.</returns>
        public static Page FromString(string svgSource)
        {
            XmlDocument svgDoc = new XmlDocument();
            svgDoc.LoadXml(svgSource);

            Dictionary<string, FontFamily> embeddedFonts = new Dictionary<string, FontFamily>();

            StylesheetParser parser = new StylesheetParser();

            List<Stylesheet> styleSheets = new List<Stylesheet>();

            foreach (XmlNode styleNode in svgDoc.GetElementsByTagName("style"))
            {
                foreach (KeyValuePair<string, FontFamily> fnt in GetEmbeddedFonts(styleNode.InnerText))
                {
                    embeddedFonts.Add(fnt.Key, fnt.Value);
                }

                try
                {
                    Stylesheet sheet = parser.Parse(styleNode.InnerText);
                    styleSheets.Add(sheet);
                }
                catch { }
            }

            Dictionary<string, Brush> gradients = new Dictionary<string, Brush>();
            Dictionary<string, IFilter> filters = new Dictionary<string, IFilter>();
            Dictionary<string, XmlNode> masks = new Dictionary<string, XmlNode>();

            foreach (XmlNode definitionsNode in svgDoc.GetElementsByTagName("defs"))
            {
                foreach (KeyValuePair<string, Brush> fnt in GetGradients(definitionsNode, styleSheets))
                {
                    gradients.Add(fnt.Key, fnt.Value);
                }

                foreach (KeyValuePair<string, IFilter> filt in GetFilters(definitionsNode, styleSheets))
                {
                    filters.Add(filt.Key, filt.Value);
                }

                foreach (KeyValuePair<string, XmlNode> mask in GetMasks(definitionsNode, styleSheets))
                {
                    masks.Add(mask.Key, mask.Value);
                }
            }

            Graphics gpr = new Graphics();

            Size pageSize = InterpretSVGObject(svgDoc.GetElementsByTagName("svg")[0], gpr, new PresentationAttributes() { EmbeddedFonts = embeddedFonts }, styleSheets, gradients, filters, masks);

            Page pg = new Page(pageSize.Width, pageSize.Height);

            pg.Graphics = gpr;

            return pg;
        }

        /// <summary>
        /// Parses an SVG image file into a <see cref="Page"/> containing the image.
        /// </summary>
        /// <param name="fileName">The path to the SVG image file.</param>
        /// <returns>A <see cref="Page"/> containing the image represented by the file.</returns>
        public static Page FromFile(string fileName)
        {
            return FromString(File.ReadAllText(fileName));
        }

        /// <summary>
        /// Parses an stream containing SVG source code into a <see cref="Page"/> containing the image represented by the code.
        /// </summary>
        /// <param name="svgSourceStream">The stream containing SVG source code.</param>
        /// <returns>A <see cref="Page"/> containing the image represented by the <paramref name="svgSourceStream"/>.</returns>
        public static Page FromStream(Stream svgSourceStream)
        {
            using (StreamReader sr = new StreamReader(svgSourceStream))
            {
                return FromString(sr.ReadToEnd());
            }
        }

        private static Size InterpretSVGObject(XmlNode svgObject, Graphics gpr, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients, Dictionary<string, IFilter> filters, Dictionary<string, XmlNode> masks)
        {
            double[] viewBox = ParseListOfDoubles(svgObject.Attributes?["viewBox"]?.Value);

            double width, height, x, y;

            string widthAttribute = svgObject.Attributes?["width"]?.Value?.Replace("px", "")?.Replace("pt", "")?.Replace("mm", "")?.Replace("in", "")?.Replace("cm", "");

            if (!double.TryParse(widthAttribute, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out width)) { width = double.NaN; }

            string heightAttribute = svgObject.Attributes?["height"]?.Value?.Replace("px", "")?.Replace("pt", "")?.Replace("mm", "")?.Replace("in", "")?.Replace("cm", "");
            if (!double.TryParse(heightAttribute, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out height)) { height = double.NaN; }

            string xAttribute = svgObject.Attributes?["x"]?.Value;
            double.TryParse(xAttribute, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out x);

            string yAttribute = svgObject.Attributes?["y"]?.Value;
            double.TryParse(yAttribute, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out y);

            double scaleX = 1;
            double scaleY = 1;

            double postTranslateX = 0;
            double postTranslateY = 0;

            if (viewBox != null)
            {
                if (!double.IsNaN(width) && !double.IsNaN(height))
                {
                    scaleX = width / viewBox[2];
                    scaleY = height / viewBox[3];
                }
                else if (!double.IsNaN(width) && double.IsNaN(height))
                {
                    scaleX = width / viewBox[2];
                    scaleY = scaleX;
                    height = scaleY * viewBox[3];
                }
                else if (double.IsNaN(width) && !double.IsNaN(height))
                {
                    scaleY = height / viewBox[3];
                    scaleX = scaleY;
                    width = scaleX * viewBox[2];
                }
                else if (double.IsNaN(width) && double.IsNaN(height))
                {
                    width = viewBox[2];
                    height = viewBox[3];
                }

                postTranslateX = -viewBox[0];
                postTranslateY = -viewBox[1];
            }
            else
            {
                viewBox = new double[4];

                if (!double.IsNaN(width))
                {
                    viewBox[2] = width;
                }

                if (!double.IsNaN(height))
                {
                    viewBox[3] = height;
                }
            }

            double diagonal = Math.Sqrt(viewBox[2] * viewBox[2] + viewBox[3] * viewBox[3]) / Math.Sqrt(2);

            Size tbrSize = new Size(width, height);

            gpr.Save();
            gpr.Translate(x, y);
            gpr.Scale(scaleX, scaleY);
            gpr.Translate(postTranslateX, postTranslateY);

            attributes = InterpretPresentationAttributes(svgObject, attributes, viewBox[2], viewBox[3], diagonal, gpr, styleSheets, gradients);

            foreach (KeyValuePair<string, XmlNode> mask in masks)
            {
                Graphics maskGpr = new Graphics();
                InterpretGObject(mask.Value, maskGpr, viewBox[2], viewBox[3], diagonal, attributes, styleSheets, gradients, filters);

                filters.Add(mask.Key, new MaskFilter(maskGpr));
            }

            InterpretSVGChildren(svgObject, gpr, attributes, viewBox[2], viewBox[3], diagonal, styleSheets, gradients, filters);

            gpr.Restore();

            return tbrSize;
        }

        private static void InterpretSVGChildren(XmlNode svgObject, Graphics gpr, PresentationAttributes attributes, double width, double height, double diagonal, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients, Dictionary<string, IFilter> filters)
        {
            foreach (XmlNode child in svgObject.ChildNodes)
            {
                InterpretSVGElement(child, gpr, attributes, width, height, diagonal, styleSheets, gradients, filters);
            }
        }

        private static void InterpretSVGElement(XmlNode currObject, Graphics gpr, PresentationAttributes attributes, double width, double height, double diagonal, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients, Dictionary<string, IFilter> filters)
        {
            if (currObject.NodeType == XmlNodeType.EntityReference)
            {
                InterpretSVGChildren(currObject, gpr, attributes, width, height, diagonal, styleSheets, gradients, filters);
            }
            else if (currObject.Name.Equals("svg", StringComparison.OrdinalIgnoreCase))
            {
                InterpretSVGObject(currObject, gpr, attributes, styleSheets, gradients, filters, new Dictionary<string, XmlNode>());
            }
            else if (currObject.Name.Equals("line", StringComparison.OrdinalIgnoreCase))
            {
                InterpretLineObject(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);
            }
            else if (currObject.Name.Equals("circle", StringComparison.OrdinalIgnoreCase))
            {
                InterpretCircleObject(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);
            }
            else if (currObject.Name.Equals("ellipse", StringComparison.OrdinalIgnoreCase))
            {
                InterpretEllipseObject(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);
            }
            else if (currObject.Name.Equals("path", StringComparison.OrdinalIgnoreCase))
            {
                InterpretPathObject(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);
            }
            else if (currObject.Name.Equals("polyline", StringComparison.OrdinalIgnoreCase))
            {
                InterpretPolyLineObject(currObject, false, gpr, width, height, diagonal, attributes, styleSheets, gradients);
            }
            else if (currObject.Name.Equals("polygon", StringComparison.OrdinalIgnoreCase))
            {
                InterpretPolyLineObject(currObject, true, gpr, width, height, diagonal, attributes, styleSheets, gradients);
            }
            else if (currObject.Name.Equals("rect", StringComparison.OrdinalIgnoreCase))
            {
                InterpretRectObject(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);
            }
            else if (currObject.Name.Equals("use", StringComparison.OrdinalIgnoreCase))
            {
                InterpretUseObject(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients, filters);
            }
            else if (currObject.Name.Equals("g", StringComparison.OrdinalIgnoreCase) || currObject.Name.Equals("symbol", StringComparison.OrdinalIgnoreCase))
            {
                InterpretGObject(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients, filters);
            }
            else if (currObject.Name.Equals("text", StringComparison.OrdinalIgnoreCase))
            {
                double x = 0;
                double y = 0;

                InterpretTextObject(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients, ref x, ref y);
            }
            else if (currObject.Name.Equals("image", StringComparison.OrdinalIgnoreCase))
            {
                InterpretImageObject(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);
            }
        }

        private static void InterpretImageObject(XmlNode currObject, Graphics gpr, double width, double height, double diagonal, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients)
        {
            PresentationAttributes currAttributes = InterpretPresentationAttributes(currObject, attributes, width, height, diagonal, gpr, styleSheets, gradients);

            double x = ParseLengthOrPercentage(currObject.Attributes?["x"]?.Value, width, currAttributes.X);
            double y = ParseLengthOrPercentage(currObject.Attributes?["y"]?.Value, height, currAttributes.Y);

            double w = ParseLengthOrPercentage(currObject.Attributes?["width"]?.Value, width, currAttributes.Width);
            double h = ParseLengthOrPercentage(currObject.Attributes?["height"]?.Value, height, currAttributes.Height);

            bool interpolate = !(currObject.Attributes?["image-rendering"]?.Value == "pixelated" || currObject.Attributes?["image-rendering"]?.Value == "optimizeSpeed");

            string href = currObject.Attributes?["href"]?.Value;

            if (string.IsNullOrEmpty(href))
            {
                href = currObject.Attributes?["xlink:href"]?.Value;
            }

            bool hadClippingPath = ApplyClipPath(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);

            string tag = currObject.Attributes?["id"]?.Value;

            if (!string.IsNullOrEmpty(href) && w > 0 && h > 0)
            {
                Page image = ParseImageURI(href, interpolate);

                if (image != null)
                {
                    gpr.Save();

                    double scaleX = w / image.Width;
                    double scaleY = h / image.Height;

                    gpr.Scale(scaleX, scaleY);

                    gpr.DrawGraphics(x / scaleX, y / scaleY, image.Graphics, tag: tag);

                    gpr.Restore();
                }
                else
                {
                    gpr.StrokeRectangle(x, y, w, h, Colours.Red, 0.1, tag: tag);
                    gpr.StrokePath(new GraphicsPath().MoveTo(x, y).LineTo(x + w, y + h).MoveTo(x + w, y).LineTo(x, y + h), Colours.Red, 0.1, tag: tag);
                }
            }

            if (hadClippingPath)
            {
                gpr.Restore();
            }

            if (currAttributes.NeedsRestore)
            {
                gpr.Restore();
            }
        }

        private static string GetFirstAttributeValueIncludingAncestors(XmlNode currObject, string attribute)
        {
            string tbr = currObject.Attributes?[attribute]?.Value;

            if (tbr != null)
            {
                return tbr;
            }
            else
            {
                if (currObject.ParentNode != null && !currObject.ParentNode.Name.Equals("svg", StringComparison.OrdinalIgnoreCase))
                {
                    return GetFirstAttributeValueIncludingAncestors(currObject.ParentNode, attribute);
                }
                else
                {
                    return null;
                }
            }
        }

        private static void InterpretTextObject(XmlNode currObject, Graphics gpr, double width, double height, double diagonal, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients, ref double x, ref double y, double fontSize = double.NaN, string fontFamily = null, string textAlign = null)
        {
            PresentationAttributes currAttributes = InterpretPresentationAttributes(currObject, attributes, width, height, diagonal, gpr, styleSheets, gradients);

            x = ParseLengthOrPercentage(currObject.Attributes?["x"]?.Value, width, x);
            y = ParseLengthOrPercentage(currObject.Attributes?["y"]?.Value, height, y);

            double dx = ParseLengthOrPercentage(currObject.Attributes?["dx"]?.Value, width, 0);
            double dy = ParseLengthOrPercentage(currObject.Attributes?["dy"]?.Value, height, 0);

            x += dx;
            y += dy;

            fontFamily = GetFirstAttributeValueIncludingAncestors(currObject, "font-family") ?? fontFamily;
            fontSize = ParseLengthOrPercentage(GetFirstAttributeValueIncludingAncestors(currObject, "font-size"), width, fontSize);
            textAlign = GetFirstAttributeValueIncludingAncestors(currObject, "text-align") ?? textAlign;

            bool hadClippingPath = ApplyClipPath(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);

            string tag = currObject.Attributes?["id"]?.Value;

            if (currObject.ChildNodes.OfType<XmlNode>().Any(a => a.NodeType != XmlNodeType.Text))
            {
                foreach (XmlNode child in currObject.ChildNodes)
                {
                    InterpretTextObject(child, gpr, width, height, diagonal, currAttributes, styleSheets, gradients, ref x, ref y, fontSize, fontFamily, textAlign);
                }

                if (hadClippingPath)
                {
                    gpr.Restore();
                }

                if (currAttributes.NeedsRestore)
                {
                    gpr.Restore();
                }
            }
            else
            {
                string text = currObject.InnerText;

                if (!double.IsNaN(fontSize) && !string.IsNullOrEmpty(text))
                {
                    text = text.Replace("\u00A0", " ");

                    FontFamily parsedFontFamily = ParseFontFamily(fontFamily, currAttributes.EmbeddedFonts);
                    string fontWeight = GetFirstAttributeValueIncludingAncestors(currObject, "font-weight");
                    string fontStyle = GetFirstAttributeValueIncludingAncestors(currObject, "font-style");

                    bool isBold = false;
                    bool isItalic = false;

                    if (fontWeight != null && (fontWeight.Equals("bold", StringComparison.OrdinalIgnoreCase) || fontWeight.Equals("bolder", StringComparison.OrdinalIgnoreCase) || (int.TryParse(fontWeight, out int weight) && weight >= 500)))
                    {
                        isBold = true;
                    }

                    if (fontStyle != null && (fontStyle.Equals("italic", StringComparison.OrdinalIgnoreCase) || fontStyle.Equals("oblique", StringComparison.OrdinalIgnoreCase)))
                    {
                        isItalic = true;
                    }

                    if (isBold && !isItalic)
                    {
                        parsedFontFamily = GetBoldFontFamily(parsedFontFamily);
                    }
                    else if (isItalic && !isBold)
                    {
                        parsedFontFamily = GetItalicFontFamily(parsedFontFamily);
                    }
                    else if (isItalic && isBold)
                    {
                        parsedFontFamily = GetBoldItalicFontFamily(parsedFontFamily);
                    }


                    Font fnt = new Font(parsedFontFamily, fontSize);

                    double endX = x;

                    if (fnt.FontFamily.TrueTypeFile != null)
                    {
                        Font.DetailedFontMetrics metrics = fnt.MeasureTextAdvanced(text);
                        x += metrics.LeftSideBearing;

                        if (!string.IsNullOrEmpty(textAlign) && (textAlign.Equals("right", StringComparison.OrdinalIgnoreCase) || textAlign.Equals("end", StringComparison.OrdinalIgnoreCase)))
                        {
                            x -= metrics.AdvanceWidth;
                        }
                        else if (!string.IsNullOrEmpty(textAlign) && textAlign.Equals("center", StringComparison.OrdinalIgnoreCase))
                        {
                            x -= metrics.AdvanceWidth * 0.5;
                        }

                        endX += metrics.AdvanceWidth;
                    }

                    TextBaselines baseline = TextBaselines.Baseline;

                    string textBaseline = GetFirstAttributeValueIncludingAncestors(currObject, "alignment-baseline");

                    if (textBaseline != null)
                    {
                        if (textBaseline.Equals("text-bottom", StringComparison.OrdinalIgnoreCase) || textBaseline.Equals("bottom", StringComparison.OrdinalIgnoreCase))
                        {
                            baseline = TextBaselines.Bottom;
                        }
                        if (textBaseline.Equals("middle", StringComparison.OrdinalIgnoreCase) || textBaseline.Equals("central", StringComparison.OrdinalIgnoreCase) || textBaseline.Equals("center", StringComparison.OrdinalIgnoreCase))
                        {
                            baseline = TextBaselines.Middle;
                        }
                        if (textBaseline.Equals("text-top", StringComparison.OrdinalIgnoreCase) || textBaseline.Equals("top", StringComparison.OrdinalIgnoreCase) || textBaseline.Equals("hanging", StringComparison.OrdinalIgnoreCase))
                        {
                            baseline = TextBaselines.Top;
                        }
                    }

                    if (currAttributes.StrokeFirst)
                    {
                        if (currAttributes.Stroke != null)
                        {
                            Brush strokeColour = currAttributes.Stroke.MultiplyOpacity(currAttributes.Opacity * currAttributes.StrokeOpacity);
                            gpr.StrokeText(x, y, text, fnt, strokeColour, baseline, currAttributes.StrokeThickness, currAttributes.LineCap, currAttributes.LineJoin, currAttributes.LineDash, tag: tag);
                        }

                        if (currAttributes.Fill != null)
                        {
                            Brush fillColour = currAttributes.Fill.MultiplyOpacity(currAttributes.Opacity * currAttributes.FillOpacity);
                            gpr.FillText(x, y, text, fnt, fillColour, baseline, tag: tag);
                        }
                    }
                    else
                    {
                        if (currAttributes.Fill != null)
                        {
                            Brush fillColour = currAttributes.Fill.MultiplyOpacity(currAttributes.Opacity * currAttributes.FillOpacity);
                            gpr.FillText(x, y, text, fnt, fillColour, baseline, tag: tag);
                        }

                        if (currAttributes.Stroke != null)
                        {
                            Brush strokeColour = currAttributes.Stroke.MultiplyOpacity(currAttributes.Opacity * currAttributes.StrokeOpacity);
                            gpr.StrokeText(x, y, text, fnt, strokeColour, baseline, currAttributes.StrokeThickness, currAttributes.LineCap, currAttributes.LineJoin, currAttributes.LineDash, tag: tag);
                        }
                    }

                    x = endX;
                }

                if (hadClippingPath)
                {
                    gpr.Restore();
                }

                if (currAttributes.NeedsRestore)
                {
                    gpr.Restore();
                }
            }
        }

        private static string[] BoldPredicates = new string[] { "-Bold", "-bold", " Bold", " bold" };
        private static string[] ItalicPredicates = new string[] { "-Italic", "-italic", " Italic", " italic", "-Oblique", "-oblique", " Oblique", " oblique" };
        private static string[] BoldItalicPredicates = new string[] { "-BoldItalic", "-bolditalic", " BoldItalic", " bolditalic", " Bold Italic", " bold italic", "-BoldOblique", "-boldoblique", " BoldOblique", " boldoblique", " Bold Oblique", " bold oblique" };

        private static FontFamily GetBoldFontFamily(FontFamily fontFamily)
        {
            switch (fontFamily.FileName)
            {
                case "Times-Roman":
                case "Times-Bold":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesBold);
                case "Times-Italic":
                case "Times-BoldItalic":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesBoldItalic);
                case "Helvetica":
                case "Helvetica-Bold":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold);
                case "Helvetica-Oblique":
                case "Helvetica-BoldOblique":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBoldOblique);
                case "Courier":
                case "Courier-Bold":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBold);
                case "Courier-Oblique":
                case "Courier-BoldOblique":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBoldOblique);
                default:
                    foreach (string sr in BoldPredicates)
                    {
                        FontFamily attempt = FontFamily.ResolveFontFamily(fontFamily.FamilyName + sr);
                        if (attempt != null && attempt.TrueTypeFile != null)
                        {
                            return attempt;
                        }
                        attempt = FontFamily.ResolveFontFamily(fontFamily.FileName + sr);
                        if (attempt != null && attempt.TrueTypeFile != null)
                        {
                            return attempt;
                        }
                    }
                    return fontFamily;
            }
        }

        private static FontFamily GetItalicFontFamily(FontFamily fontFamily)
        {
            switch (fontFamily.FileName)
            {
                case "Times-Roman":
                case "Times-Italic":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesItalic);
                case "Times-Bold":
                case "Times-BoldItalic":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesBoldItalic);
                case "Helvetica":
                case "Helvetica-Oblique":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaOblique);
                case "Helvetica-Bold":
                case "Helvetica-BoldOblique":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBoldOblique);
                case "Courier":
                case "Courier-Oblique":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierOblique);
                case "Courier-Bold":
                case "Courier-BoldOblique":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBoldOblique);
                default:
                    foreach (string sr in ItalicPredicates)
                    {
                        FontFamily attempt = FontFamily.ResolveFontFamily(fontFamily.FamilyName + sr);
                        if (attempt != null && attempt.TrueTypeFile != null)
                        {
                            return attempt;
                        }
                        attempt = FontFamily.ResolveFontFamily(fontFamily.FileName + sr);
                        if (attempt != null && attempt.TrueTypeFile != null)
                        {
                            return attempt;
                        }
                    }
                    return fontFamily;
            }
        }

        private static FontFamily GetBoldItalicFontFamily(FontFamily fontFamily)
        {
            switch (fontFamily.FileName)
            {
                case "Times-Roman":
                case "Times-Italic":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesItalic);
                case "Times-Bold":
                case "Times-BoldItalic":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesBoldItalic);
                case "Helvetica":
                case "Helvetica-Oblique":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaOblique);
                case "Helvetica-Bold":
                case "Helvetica-BoldOblique":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBoldOblique);
                case "Courier":
                case "Courier-Oblique":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierOblique);
                case "Courier-Bold":
                case "Courier-BoldOblique":
                    return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBoldOblique);
                default:
                    foreach (string sr in BoldItalicPredicates)
                    {
                        FontFamily attempt = FontFamily.ResolveFontFamily(fontFamily.FamilyName + sr);
                        if (attempt != null && attempt.TrueTypeFile != null)
                        {
                            return attempt;
                        }
                        attempt = FontFamily.ResolveFontFamily(fontFamily.FileName + sr);
                        if (attempt != null && attempt.TrueTypeFile != null)
                        {
                            return attempt;
                        }
                    }
                    return fontFamily;
            }
        }

        private static FontFamily ParseFontFamily(string fontFamily, Dictionary<string, FontFamily> embeddedFonts)
        {
            string[] fontFamilies = Regexes.FontFamilySeparator.Split(fontFamily);

            foreach (string fam in fontFamilies)
            {
                string family = fam.Trim().Trim(',', '"').Trim();

                if (embeddedFonts.TryGetValue(family, out FontFamily tbr))
                {
                    return tbr;
                }

                List<(string, int)> matchedFamilies = new List<(string, int)>();

                for (int i = 0; i < FontFamily.StandardFamilies.Length; i++)
                {
                    if (family.StartsWith(FontFamily.StandardFamilies[i]))
                    {
                        matchedFamilies.Add((FontFamily.StandardFamilies[i], FontFamily.StandardFamilies[i].Length));
                    }
                }

                if (matchedFamilies.Count > 0)
                {
                    return FontFamily.ResolveFontFamily((from el in matchedFamilies orderby el.Item2 descending select el.Item1).First());
                }
                else
                {
                    if (family.Equals("serif", StringComparison.OrdinalIgnoreCase))
                    {
                        return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesRoman);
                    }
                    else if (family.Equals("sans-serif", StringComparison.OrdinalIgnoreCase))
                    {
                        return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
                    }
                    else if (family.Equals("monospace", StringComparison.OrdinalIgnoreCase))
                    {
                        return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Courier);
                    }
                    else if (family.Equals("cursive", StringComparison.OrdinalIgnoreCase))
                    {
                        return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesItalic);
                    }
                    else if (family.Equals("system-ui", StringComparison.OrdinalIgnoreCase))
                    {
                        return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
                    }
                    else if (family.Equals("ui-serif", StringComparison.OrdinalIgnoreCase))
                    {
                        return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesRoman);
                    }
                    else if (family.Equals("ui-sans-serif", StringComparison.OrdinalIgnoreCase))
                    {
                        return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
                    }
                    else if (family.Equals("ui-monospace", StringComparison.OrdinalIgnoreCase))
                    {
                        return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Courier);
                    }
                    else if (family.Equals("StandardSymbolsPS", StringComparison.OrdinalIgnoreCase))
                    {
                        return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Symbol);
                    }
                    else if (family.Equals("D050000L", StringComparison.OrdinalIgnoreCase))
                    {
                        return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.ZapfDingbats);
                    }
                }

                FontFamily parsedFamily = FontFamily.ResolveFontFamily(family);

                if (parsedFamily != null && parsedFamily.TrueTypeFile != null)
                {
                    return parsedFamily;
                }
            }

            return FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
        }

        private static void InterpretGObject(XmlNode currObject, Graphics gpr, double width, double height, double diagonal, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients, Dictionary<string, IFilter> filters)
        {
            PresentationAttributes currAttributes = InterpretPresentationAttributes(currObject, attributes, width, height, diagonal, gpr, styleSheets, gradients);

            bool hadClippingPath = ApplyClipPath(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);

            string filter = currObject.Attributes?["filter"]?.Value ?? currObject.Attributes?["mask"]?.Value;

            if (!string.IsNullOrEmpty(filter) && filter.StartsWith("url(#"))
            {
                filter = filter.Substring(5, filter.Length - 6);
            }

            if (!string.IsNullOrEmpty(filter) && filters.ContainsKey(filter))
            {
                Graphics filteredGraphics = new Graphics();

                InterpretSVGChildren(currObject, filteredGraphics, currAttributes, width, height, diagonal, styleSheets, gradients, filters);
                gpr.DrawGraphics(0, 0, filteredGraphics, filters[filter]);
            }
            else
            {
                InterpretSVGChildren(currObject, gpr, currAttributes, width, height, diagonal, styleSheets, gradients, filters);
            }

            if (hadClippingPath)
            {
                gpr.Restore();
            }

            if (currAttributes.NeedsRestore)
            {
                gpr.Restore();
            }
        }

        private static void InterpretUseObject(XmlNode currObject, Graphics gpr, double width, double height, double diagonal, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients, Dictionary<string, IFilter> filters)
        {
            double x, y, w, h;

            x = ParseLengthOrPercentage(currObject.Attributes?["x"]?.Value, width);
            y = ParseLengthOrPercentage(currObject.Attributes?["y"]?.Value, height);
            w = ParseLengthOrPercentage(currObject.Attributes?["width"]?.Value, width, double.NaN);
            h = ParseLengthOrPercentage(currObject.Attributes?["height"]?.Value, height, double.NaN);

            string id = currObject.Attributes?["href"]?.Value ?? currObject.Attributes?["xlink:href"]?.Value;

            if (id != null && id.StartsWith("#"))
            {
                id = id.Substring(1);

                XmlNode element = currObject.OwnerDocument.SelectSingleNode(string.Format("//*[@id='{0}']", id));

                if (element != null)
                {
                    XmlNode clone = element.Clone();

                    currObject.AppendChild(clone);


                    PresentationAttributes currAttributes = InterpretPresentationAttributes(currObject, attributes, width, height, diagonal, gpr, styleSheets, gradients);


                    gpr.Save();
                    gpr.Translate(x, y);

                    ((XmlElement)clone).SetAttribute("x", "0");
                    ((XmlElement)clone).SetAttribute("y", "0");

                    if (clone.Attributes?["viewBox"] != null)
                    {
                        ((XmlElement)clone).SetAttribute("width", w.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        ((XmlElement)clone).SetAttribute("height", h.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }

                    InterpretSVGElement(clone, gpr, currAttributes, width, height, diagonal, styleSheets, gradients, filters);

                    gpr.Restore();

                    if (currAttributes.NeedsRestore)
                    {
                        gpr.Restore();
                    }
                }
            }
        }

        private static bool ApplyClipPath(XmlNode currObject, Graphics gpr, double width, double height, double diagonal, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients)
        {
            string id = currObject.Attributes?["clip-path"]?.Value;

            if (id != null && id.StartsWith("url(#"))
            {
                id = id.Substring(5);
                id = id.Substring(0, id.Length - 1);

                XmlNode element = currObject.OwnerDocument.SelectSingleNode(string.Format("//*[@id='{0}']", id));

                if (element != null && element.ChildNodes.Count == 1 && element.ChildNodes[0].Name.Equals("path", StringComparison.OrdinalIgnoreCase))
                {
                    bool hasParentClipPath = ApplyClipPath(element, gpr, width, height, diagonal, attributes, styleSheets, gradients);

                    Graphics pathGraphics = new Graphics();
                    InterpretPathObject(element.ChildNodes[0], pathGraphics, width, height, diagonal, attributes, styleSheets, gradients);

                    PathTransformerGraphicsContext ptgc = new PathTransformerGraphicsContext();
                    pathGraphics.CopyToIGraphicsContext(ptgc);

                    if (!hasParentClipPath)
                    {
                        gpr.Save();
                    }

                    gpr.SetClippingPath(ptgc.CurrentPath);

                    return true;
                }
                else if (element != null && element.ChildNodes.Count == 1 && element.ChildNodes[0].Name.Equals("rect", StringComparison.OrdinalIgnoreCase))
                {
                    bool hasParentClipPath = ApplyClipPath(element, gpr, width, height, diagonal, attributes, styleSheets, gradients);

                    Graphics pathGraphics = new Graphics();
                    InterpretRectObject(element.ChildNodes[0], pathGraphics, width, height, diagonal, attributes, styleSheets, gradients);

                    PathTransformerGraphicsContext ptgc = new PathTransformerGraphicsContext();
                    pathGraphics.CopyToIGraphicsContext(ptgc);

                    if (!hasParentClipPath)
                    {
                        gpr.Save();
                    }

                    gpr.SetClippingPath(ptgc.CurrentPath);

                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        private static void InterpretRectObject(XmlNode currObject, Graphics gpr, double width, double height, double diagonal, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients)
        {
            double x, y, w, h, rx, ry;

            x = ParseLengthOrPercentage(currObject.Attributes?["x"]?.Value, width);
            y = ParseLengthOrPercentage(currObject.Attributes?["y"]?.Value, height);
            w = ParseLengthOrPercentage(currObject.Attributes?["width"]?.Value, width);
            h = ParseLengthOrPercentage(currObject.Attributes?["height"]?.Value, height);
            rx = ParseLengthOrPercentage(currObject.Attributes?["rx"]?.Value, width, double.NaN);
            ry = ParseLengthOrPercentage(currObject.Attributes?["ry"]?.Value, height, double.NaN);

            if (w > 0 && h > 0)
            {
                if (double.IsNaN(rx) && !double.IsNaN(ry))
                {
                    rx = ry;
                }
                else if (!double.IsNaN(rx) && double.IsNaN(ry))
                {
                    ry = rx;
                }

                if (double.IsNaN(rx))
                {
                    rx = 0;
                }

                if (double.IsNaN(ry))
                {
                    ry = 0;
                }

                rx = Math.Min(rx, w / 2);
                ry = Math.Min(ry, h / 2);

                GraphicsPath path = new GraphicsPath();

                path.MoveTo(x + rx, y);
                path.LineTo(x + w - rx, y);

                if (rx > 0 && ry > 0)
                {
                    path.EllipticalArc(rx, ry, 0, false, true, new Point(x + w, y + ry));
                }

                path.LineTo(x + w, y + h - ry);

                if (rx > 0 && ry > 0)
                {
                    path.EllipticalArc(rx, ry, 0, false, true, new Point(x + w - rx, y + h));
                }

                path.LineTo(x + rx, y + h);

                if (rx > 0 && ry > 0)
                {
                    path.EllipticalArc(rx, ry, 0, false, true, new Point(x, y + h - ry));
                }

                path.LineTo(x, y + ry);

                if (rx > 0 && ry > 0)
                {
                    path.EllipticalArc(rx, ry, 0, false, true, new Point(x + rx, y));
                }

                path.Close();

                PresentationAttributes currAttributes = InterpretPresentationAttributes(currObject, attributes, width, height, diagonal, gpr, styleSheets, gradients);

                bool hadClippingPath = ApplyClipPath(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);

                string tag = currObject.Attributes?["id"]?.Value;

                if (currAttributes.StrokeFirst)
                {
                    if (currAttributes.Stroke != null)
                    {
                        Brush strokeColour = currAttributes.Stroke.MultiplyOpacity(currAttributes.Opacity * currAttributes.StrokeOpacity);
                        gpr.StrokePath(path, strokeColour, currAttributes.StrokeThickness, currAttributes.LineCap, currAttributes.LineJoin, currAttributes.LineDash, tag: tag);
                    }

                    if (currAttributes.Fill != null)
                    {
                        Brush fillColour = currAttributes.Fill.MultiplyOpacity(currAttributes.Opacity * currAttributes.FillOpacity);
                        gpr.FillPath(path, fillColour, tag: tag);
                    }
                }
                else
                {
                    if (currAttributes.Fill != null)
                    {
                        Brush fillColour = currAttributes.Fill.MultiplyOpacity(currAttributes.Opacity * currAttributes.FillOpacity);
                        gpr.FillPath(path, fillColour, tag: tag);
                    }

                    if (currAttributes.Stroke != null)
                    {
                        Brush strokeColour = currAttributes.Stroke.MultiplyOpacity(currAttributes.Opacity * currAttributes.StrokeOpacity);
                        gpr.StrokePath(path, strokeColour, currAttributes.StrokeThickness, currAttributes.LineCap, currAttributes.LineJoin, currAttributes.LineDash, tag: tag);
                    }
                }

                if (hadClippingPath)
                {
                    gpr.Restore();
                }

                if (currAttributes.NeedsRestore)
                {
                    gpr.Restore();
                }
            }
        }

        private static void InterpretPolyLineObject(XmlNode currObject, bool isPolygon, Graphics gpr, double width, double height, double diagonal, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients)
        {
            string points = currObject.Attributes?["points"]?.Value;

            if (points != null)
            {
                double[] coordinates = ParseListOfDoubles(points);

                GraphicsPath path = new GraphicsPath();

                for (int i = 0; i < coordinates.Length; i += 2)
                {
                    path.LineTo(coordinates[i], coordinates[i + 1]);
                }

                if (isPolygon)
                {
                    path.Close();
                }

                PresentationAttributes currAttributes = InterpretPresentationAttributes(currObject, attributes, width, height, diagonal, gpr, styleSheets, gradients);

                bool hadClippingPath = ApplyClipPath(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);

                string tag = currObject.Attributes?["id"]?.Value;

                if (currAttributes.StrokeFirst)
                {
                    if (currAttributes.Stroke != null)
                    {
                        Brush strokeColour = currAttributes.Stroke.MultiplyOpacity(currAttributes.Opacity * currAttributes.StrokeOpacity);
                        gpr.StrokePath(path, strokeColour, currAttributes.StrokeThickness, currAttributes.LineCap, currAttributes.LineJoin, currAttributes.LineDash, tag: tag);
                    }

                    if (currAttributes.Fill != null)
                    {
                        Brush fillColour = currAttributes.Fill.MultiplyOpacity(currAttributes.Opacity * currAttributes.FillOpacity);
                        gpr.FillPath(path, fillColour, tag: tag);
                    }
                }
                else
                {
                    if (currAttributes.Fill != null)
                    {
                        Brush fillColour = currAttributes.Fill.MultiplyOpacity(currAttributes.Opacity * currAttributes.FillOpacity);
                        gpr.FillPath(path, fillColour, tag: tag);
                    }

                    if (currAttributes.Stroke != null)
                    {
                        Brush strokeColour = currAttributes.Stroke.MultiplyOpacity(currAttributes.Opacity * currAttributes.StrokeOpacity);
                        gpr.StrokePath(path, strokeColour, currAttributes.StrokeThickness, currAttributes.LineCap, currAttributes.LineJoin, currAttributes.LineDash, tag: tag);
                    }
                }

                if (hadClippingPath)
                {
                    gpr.Restore();
                }

                if (currAttributes.NeedsRestore)
                {
                    gpr.Restore();
                }
            }
        }

        private static void InterpretPathObject(XmlNode currObject, Graphics gpr, double width, double height, double diagonal, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients)
        {
            string d = currObject.Attributes?["d"]?.Value;

            if (d != null)
            {
                List<string> pathData = TokenisePathData(d);

                GraphicsPath path = new GraphicsPath();

                Point lastPoint = new Point();
                Point? figureStartPoint = null;

                char lastCommand = '\0';
                Point lastCtrlPoint = new Point();

                for (int i = 0; i < pathData.Count; i++)
                {
                    Point delta = new Point();

                    bool isAbsolute = char.IsUpper(pathData[i][0]);

                    if (!isAbsolute)
                    {
                        delta = lastPoint;
                    }

                    switch (pathData[i][0])
                    {
                        case 'M':
                        case 'm':
                            lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                            path.MoveTo(lastPoint);
                            figureStartPoint = lastPoint;
                            i += 2;
                            lastCommand = 'M';
                            while (i < pathData.Count - 1 && !char.IsLetter(pathData[i + 1][0]))
                            {
                                if (!isAbsolute)
                                {
                                    delta = lastPoint;
                                }
                                else
                                {
                                    delta = new Point();
                                }

                                lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                path.LineTo(lastPoint);

                                i += 2;
                                lastCommand = 'L';
                            }
                            break;
                        case 'L':
                        case 'l':
                            lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                            path.LineTo(lastPoint);
                            if (figureStartPoint == null)
                            {
                                figureStartPoint = lastPoint;
                            }
                            i += 2;
                            lastCommand = 'L';
                            while (i < pathData.Count - 1 && !char.IsLetter(pathData[i + 1][0]))
                            {
                                if (!isAbsolute)
                                {
                                    delta = lastPoint;
                                }
                                else
                                {
                                    delta = new Point();
                                }

                                lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                path.LineTo(lastPoint);

                                i += 2;
                            }
                            break;
                        case 'H':
                        case 'h':
                            lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), lastPoint.Y);
                            path.LineTo(lastPoint);
                            if (figureStartPoint == null)
                            {
                                figureStartPoint = lastPoint;
                            }
                            i++;
                            lastCommand = 'L';
                            while (i < pathData.Count - 1 && !char.IsLetter(pathData[i + 1][0]))
                            {
                                if (!isAbsolute)
                                {
                                    delta = lastPoint;
                                }
                                else
                                {
                                    delta = new Point();
                                }

                                lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), lastPoint.Y);
                                path.LineTo(lastPoint);

                                i++;
                            }
                            break;
                        case 'V':
                        case 'v':
                            lastPoint = new Point(lastPoint.X, delta.Y + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture));
                            path.LineTo(lastPoint);
                            if (figureStartPoint == null)
                            {
                                figureStartPoint = lastPoint;
                            }
                            i++;
                            lastCommand = 'L';
                            while (i < pathData.Count - 1 && !char.IsLetter(pathData[i + 1][0]))
                            {
                                if (!isAbsolute)
                                {
                                    delta = lastPoint;
                                }
                                else
                                {
                                    delta = new Point();
                                }

                                lastPoint = new Point(lastPoint.X, delta.Y + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture));
                                path.LineTo(lastPoint);

                                i++;
                            }
                            break;
                        case 'C':
                        case 'c':
                            {
                                Point ctrlPoint1 = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                i += 2;

                                Point ctrlPoint2 = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                i += 2;

                                lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                i += 2;

                                if (figureStartPoint == null)
                                {
                                    figureStartPoint = lastPoint;
                                }

                                path.CubicBezierTo(ctrlPoint1, ctrlPoint2, lastPoint);

                                lastCtrlPoint = ctrlPoint2;
                                lastCommand = 'C';

                                while (i < pathData.Count - 1 && !char.IsLetter(pathData[i + 1][0]))
                                {
                                    if (!isAbsolute)
                                    {
                                        delta = lastPoint;
                                    }
                                    else
                                    {
                                        delta = new Point();
                                    }

                                    ctrlPoint1 = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                    i += 2;

                                    ctrlPoint2 = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                    i += 2;

                                    lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                    i += 2;

                                    path.CubicBezierTo(ctrlPoint1, ctrlPoint2, lastPoint);
                                    lastCtrlPoint = ctrlPoint2;
                                }
                            }
                            break;
                        case 'S':
                        case 's':
                            {
                                Point ctrlPoint1;

                                if (lastCommand == 'C')
                                {
                                    ctrlPoint1 = new Point(2 * lastPoint.X - lastCtrlPoint.X, 2 * lastPoint.Y - lastCtrlPoint.Y);
                                }
                                else
                                {
                                    ctrlPoint1 = lastPoint;
                                }

                                Point ctrlPoint2 = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                i += 2;

                                lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                i += 2;

                                if (figureStartPoint == null)
                                {
                                    figureStartPoint = lastPoint;
                                }

                                path.CubicBezierTo(ctrlPoint1, ctrlPoint2, lastPoint);

                                lastCtrlPoint = ctrlPoint2;
                                lastCommand = 'C';

                                while (i < pathData.Count - 1 && !char.IsLetter(pathData[i + 1][0]))
                                {
                                    if (!isAbsolute)
                                    {
                                        delta = lastPoint;
                                    }
                                    else
                                    {
                                        delta = new Point();
                                    }

                                    ctrlPoint1 = new Point(2 * lastPoint.X - lastCtrlPoint.X, 2 * lastPoint.Y - lastCtrlPoint.Y);

                                    ctrlPoint2 = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                    i += 2;

                                    lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                    i += 2;

                                    path.CubicBezierTo(ctrlPoint1, ctrlPoint2, lastPoint);

                                    lastCtrlPoint = ctrlPoint2;
                                }
                            }
                            break;
                        case 'Q':
                        case 'q':
                            {
                                Point ctrlPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                i += 2;

                                Point actualCP1 = new Point(lastPoint.X + 2 * (ctrlPoint.X - lastPoint.X) / 3, lastPoint.Y + 2 * (ctrlPoint.Y - lastPoint.Y) / 3);

                                lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                i += 2;

                                Point actualCP2 = new Point(lastPoint.X + 2 * (ctrlPoint.X - lastPoint.X) / 3, lastPoint.Y + 2 * (ctrlPoint.Y - lastPoint.Y) / 3);

                                if (figureStartPoint == null)
                                {
                                    figureStartPoint = lastPoint;
                                }

                                path.CubicBezierTo(actualCP1, actualCP2, lastPoint);

                                lastCtrlPoint = ctrlPoint;
                                lastCommand = 'Q';

                                while (i < pathData.Count - 1 && !char.IsLetter(pathData[i + 1][0]))
                                {
                                    if (!isAbsolute)
                                    {
                                        delta = lastPoint;
                                    }
                                    else
                                    {
                                        delta = new Point();
                                    }

                                    ctrlPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                    i += 2;

                                    actualCP1 = new Point(lastPoint.X + 2 * (ctrlPoint.X - lastPoint.X) / 3, lastPoint.Y + 2 * (ctrlPoint.Y - lastPoint.Y) / 3);

                                    lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                    i += 2;

                                    actualCP2 = new Point(lastPoint.X + 2 * (ctrlPoint.X - lastPoint.X) / 3, lastPoint.Y + 2 * (ctrlPoint.Y - lastPoint.Y) / 3);

                                    path.CubicBezierTo(actualCP1, actualCP2, lastPoint);
                                    lastCtrlPoint = ctrlPoint;
                                }


                            }
                            break;
                        case 'T':
                        case 't':
                            {
                                Point ctrlPoint;

                                if (lastCommand == 'Q')
                                {
                                    ctrlPoint = new Point(2 * lastPoint.X - lastCtrlPoint.X, 2 * lastPoint.Y - lastCtrlPoint.Y);
                                }
                                else
                                {
                                    ctrlPoint = lastPoint;
                                }

                                Point actualCP1 = new Point(lastPoint.X + 2 * (ctrlPoint.X - lastPoint.X) / 3, lastPoint.Y + 2 * (ctrlPoint.Y - lastPoint.Y) / 3);

                                lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                i += 2;

                                Point actualCP2 = new Point(lastPoint.X + 2 * (ctrlPoint.X - lastPoint.X) / 3, lastPoint.Y + 2 * (ctrlPoint.Y - lastPoint.Y) / 3);

                                if (figureStartPoint == null)
                                {
                                    figureStartPoint = lastPoint;
                                }

                                path.CubicBezierTo(actualCP1, actualCP2, lastPoint);
                                lastCtrlPoint = ctrlPoint;
                                lastCommand = 'Q';

                                while (i < pathData.Count - 1 && !char.IsLetter(pathData[i + 1][0]))
                                {
                                    if (!isAbsolute)
                                    {
                                        delta = lastPoint;
                                    }
                                    else
                                    {
                                        delta = new Point();
                                    }

                                    ctrlPoint = new Point(2 * lastPoint.X - lastCtrlPoint.X, 2 * lastPoint.Y - lastCtrlPoint.Y);

                                    actualCP1 = new Point(lastPoint.X + 2 * (ctrlPoint.X - lastPoint.X) / 3, lastPoint.Y + 2 * (ctrlPoint.Y - lastPoint.Y) / 3);

                                    lastPoint = new Point(delta.X + double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                    i += 2;

                                    actualCP2 = new Point(lastPoint.X + 2 * (ctrlPoint.X - lastPoint.X) / 3, lastPoint.Y + 2 * (ctrlPoint.Y - lastPoint.Y) / 3);

                                    path.CubicBezierTo(actualCP1, actualCP2, lastPoint);

                                    lastCtrlPoint = ctrlPoint;
                                }
                            }
                            break;
                        case 'A':
                        case 'a':
                            {
                                Point startPoint = lastPoint;

                                if (figureStartPoint == null)
                                {
                                    figureStartPoint = lastPoint;
                                }

                                Point radii = new Point(double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                double angle = double.Parse(pathData[i + 3], System.Globalization.CultureInfo.InvariantCulture) * Math.PI / 180;
                                bool largeArcFlag = pathData[i + 4][0] == '1';
                                bool sweepFlag = pathData[i + 5][0] == '1';

                                lastPoint = new Point(delta.X + double.Parse(pathData[i + 6], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 7], System.Globalization.CultureInfo.InvariantCulture));
                                i += 7;

                                path.EllipticalArc(radii.X, radii.Y, angle, largeArcFlag, sweepFlag, lastPoint);

                                while (i < pathData.Count - 1 && !char.IsLetter(pathData[i + 1][0]))
                                {
                                    if (!isAbsolute)
                                    {
                                        delta = lastPoint;
                                    }
                                    else
                                    {
                                        delta = new Point();
                                    }

                                    startPoint = lastPoint;
                                    radii = new Point(double.Parse(pathData[i + 1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(pathData[i + 2], System.Globalization.CultureInfo.InvariantCulture));
                                    angle = double.Parse(pathData[i + 3], System.Globalization.CultureInfo.InvariantCulture) * Math.PI / 180;
                                    largeArcFlag = pathData[i + 4][0] == '1';
                                    sweepFlag = pathData[i + 5][0] == '1';

                                    lastPoint = new Point(delta.X + double.Parse(pathData[i + 6], System.Globalization.CultureInfo.InvariantCulture), delta.Y + double.Parse(pathData[i + 7], System.Globalization.CultureInfo.InvariantCulture));
                                    i += 7;

                                    path.EllipticalArc(radii.X, radii.Y, angle, largeArcFlag, sweepFlag, lastPoint);
                                }
                            }

                            break;
                        case 'Z':
                        case 'z':
                            path.Close();
                            lastPoint = figureStartPoint.Value;
                            figureStartPoint = null;
                            lastCommand = 'Z';
                            break;
                    }
                }

                PresentationAttributes currAttributes = InterpretPresentationAttributes(currObject, attributes, width, height, diagonal, gpr, styleSheets, gradients);

                bool hadClippingPath = ApplyClipPath(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);

                string tag = currObject.Attributes?["id"]?.Value;

                if (currAttributes.StrokeFirst)
                {
                    if (currAttributes.Stroke != null)
                    {
                        Brush strokeColour = currAttributes.Stroke.MultiplyOpacity(currAttributes.Opacity * currAttributes.StrokeOpacity);
                        gpr.StrokePath(path, strokeColour, currAttributes.StrokeThickness, currAttributes.LineCap, currAttributes.LineJoin, currAttributes.LineDash, tag: tag);
                    }

                    if (currAttributes.Fill != null)
                    {
                        Brush fillColour = currAttributes.Fill.MultiplyOpacity(currAttributes.Opacity * currAttributes.FillOpacity);
                        gpr.FillPath(path, fillColour, tag: tag);
                    }
                }
                else
                {
                    if (currAttributes.Fill != null)
                    {
                        Brush fillColour = currAttributes.Fill.MultiplyOpacity(currAttributes.Opacity * currAttributes.FillOpacity);
                        gpr.FillPath(path, fillColour, tag: tag);
                    }

                    if (currAttributes.Stroke != null)
                    {
                        Brush strokeColour = currAttributes.Stroke.MultiplyOpacity(currAttributes.Opacity * currAttributes.StrokeOpacity);
                        gpr.StrokePath(path, strokeColour, currAttributes.StrokeThickness, currAttributes.LineCap, currAttributes.LineJoin, currAttributes.LineDash, tag: tag);
                    }
                }

                if (hadClippingPath)
                {
                    gpr.Restore();
                }

                if (currAttributes.NeedsRestore)
                {
                    gpr.Restore();
                }
            }

        }

        private static List<string> TokenisePathData(string d)
        {
            List<string> tbr = new List<string>();

            string currToken = "";

            for (int i = 0; i < d.Length; i++)
            {
                char c = d[i];

                if (c >= '0' && c <= '9' || c == 'e' || c == 'E')
                {
                    currToken += c;
                }
                else if (c == '.')
                {
                    if (!currToken.Contains('.'))
                    {
                        currToken += c;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(currToken))
                        {
                            tbr.Add(currToken);
                        }
                        currToken = "" + c;
                    }
                }
                else if (c == '-' || c == '+')
                {
                    if (i > 0 && (d[i - 1] == 'e' || d[i - 1] == 'E'))
                    {
                        currToken += c;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(currToken))
                        {
                            tbr.Add(currToken);
                        }
                        currToken = "" + c;
                    }
                }
                else if (char.IsWhiteSpace(c) || c == ',')
                {
                    if (!string.IsNullOrEmpty(currToken))
                    {
                        tbr.Add(currToken);
                    }
                    currToken = "";
                }
                else if (i < d.Length - 2 && (c == 'N' || c == 'n') && (d[i + 1] == 'a' || d[i + 1] == 'A') && (d[i + 2] == 'N' || d[i + 2] == 'n'))
                {
                    if (!string.IsNullOrEmpty(currToken))
                    {
                        tbr.Add(currToken);
                    }
                    tbr.Add("NaN");
                    currToken = "";
                    i += 2;
                }
                else if ("MmLlHhVvCcSsQqTtAaZz".Contains(c))
                {
                    if (!string.IsNullOrEmpty(currToken))
                    {
                        tbr.Add(currToken);
                    }
                    tbr.Add(c.ToString());
                    currToken = "";
                }
            }

            if (!string.IsNullOrEmpty(currToken))
            {
                tbr.Add(currToken);
            }

            return tbr;
        }

        private static void InterpretCircleObject(XmlNode circleObject, Graphics gpr, double width, double height, double diagonal, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients)
        {
            double cx, cy, r;

            cx = ParseLengthOrPercentage(circleObject.Attributes?["cx"]?.Value, width);
            cy = ParseLengthOrPercentage(circleObject.Attributes?["cy"]?.Value, height);
            r = ParseLengthOrPercentage(circleObject.Attributes?["r"]?.Value, diagonal);

            string tag = circleObject.Attributes?["id"]?.Value;

            PresentationAttributes circleAttributes = InterpretPresentationAttributes(circleObject, attributes, width, height, diagonal, gpr, styleSheets, gradients);

            bool hadClippingPath = ApplyClipPath(circleObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);

            if (circleAttributes.StrokeFirst)
            {
                if (circleAttributes.Stroke != null)
                {
                    Brush strokeColour = circleAttributes.Stroke.MultiplyOpacity(circleAttributes.Opacity * circleAttributes.StrokeOpacity);
                    gpr.StrokePath(new GraphicsPath().Arc(cx, cy, r, 0, 2 * Math.PI).Close(), strokeColour, circleAttributes.StrokeThickness, circleAttributes.LineCap, circleAttributes.LineJoin, circleAttributes.LineDash, tag: tag);
                }

                if (circleAttributes.Fill != null)
                {
                    Brush fillColour = circleAttributes.Fill.MultiplyOpacity(circleAttributes.Opacity * circleAttributes.FillOpacity);
                    gpr.FillPath(new GraphicsPath().Arc(cx, cy, r, 0, 2 * Math.PI).Close(), fillColour, tag: tag);
                }
            }
            else
            {
                if (circleAttributes.Fill != null)
                {
                    Brush fillColour = circleAttributes.Fill.MultiplyOpacity(circleAttributes.Opacity * circleAttributes.FillOpacity);
                    gpr.FillPath(new GraphicsPath().Arc(cx, cy, r, 0, 2 * Math.PI).Close(), fillColour, tag: tag);
                }

                if (circleAttributes.Stroke != null)
                {
                    Brush strokeColour = circleAttributes.Stroke.MultiplyOpacity(circleAttributes.Opacity * circleAttributes.StrokeOpacity);
                    gpr.StrokePath(new GraphicsPath().Arc(cx, cy, r, 0, 2 * Math.PI).Close(), strokeColour, circleAttributes.StrokeThickness, circleAttributes.LineCap, circleAttributes.LineJoin, circleAttributes.LineDash, tag: tag);
                }
            }

            if (hadClippingPath)
            {
                gpr.Restore();
            }

            if (circleAttributes.NeedsRestore)
            {
                gpr.Restore();
            }
        }

        private static void InterpretEllipseObject(XmlNode currObject, Graphics gpr, double width, double height, double diagonal, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients)
        {
            double cx, cy, rx, ry;

            cx = ParseLengthOrPercentage(currObject.Attributes?["cx"]?.Value, width);
            cy = ParseLengthOrPercentage(currObject.Attributes?["cy"]?.Value, height);
            rx = ParseLengthOrPercentage(currObject.Attributes?["rx"]?.Value, width, double.NaN);
            ry = ParseLengthOrPercentage(currObject.Attributes?["ry"]?.Value, height, double.NaN);

            string tag = currObject.Attributes?["id"]?.Value;

            if (double.IsNaN(rx) && !double.IsNaN(ry))
            {
                rx = ry;
            }
            else if (!double.IsNaN(rx) && double.IsNaN(ry))
            {
                ry = rx;
            }

            if (rx > 0 && ry > 0)
            {

                PresentationAttributes currAttributes = InterpretPresentationAttributes(currObject, attributes, width, height, diagonal, gpr, styleSheets, gradients);

                bool hadClippingPath = ApplyClipPath(currObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);

                double r = Math.Min(rx, ry);

                gpr.Save();
                gpr.Translate(cx, cy);
                gpr.Scale(rx / r, ry / r);

                if (currAttributes.StrokeFirst)
                {
                    if (currAttributes.Stroke != null)
                    {
                        Brush strokeColour = currAttributes.Stroke.MultiplyOpacity(currAttributes.Opacity * currAttributes.StrokeOpacity);
                        gpr.StrokePath(new GraphicsPath().Arc(0, 0, r, 0, 2 * Math.PI).Close(), strokeColour, currAttributes.StrokeThickness, currAttributes.LineCap, currAttributes.LineJoin, currAttributes.LineDash, tag: tag);
                    }

                    if (currAttributes.Fill != null)
                    {
                        Brush fillColour = currAttributes.Fill.MultiplyOpacity(currAttributes.Opacity * currAttributes.FillOpacity);
                        gpr.FillPath(new GraphicsPath().Arc(0, 0, r, 0, 2 * Math.PI).Close(), fillColour, tag: tag);
                    }
                }
                else
                {
                    if (currAttributes.Fill != null)
                    {
                        Brush fillColour = currAttributes.Fill.MultiplyOpacity(currAttributes.Opacity * currAttributes.FillOpacity);
                        gpr.FillPath(new GraphicsPath().Arc(0, 0, r, 0, 2 * Math.PI).Close(), fillColour, tag: tag);
                    }

                    if (currAttributes.Stroke != null)
                    {
                        Brush strokeColour = currAttributes.Stroke.MultiplyOpacity(currAttributes.Opacity * currAttributes.StrokeOpacity);
                        gpr.StrokePath(new GraphicsPath().Arc(0, 0, r, 0, 2 * Math.PI).Close(), strokeColour, currAttributes.StrokeThickness, currAttributes.LineCap, currAttributes.LineJoin, currAttributes.LineDash, tag: tag);
                    }
                }

                gpr.Restore();

                if (hadClippingPath)
                {
                    gpr.Restore();
                }

                if (currAttributes.NeedsRestore)
                {
                    gpr.Restore();
                }
            }
        }

        private static void InterpretLineObject(XmlNode lineObject, Graphics gpr, double width, double height, double diagonal, PresentationAttributes attributes, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients)
        {
            double x1, x2, y1, y2;

            x1 = ParseLengthOrPercentage(lineObject.Attributes?["x1"]?.Value, width);
            y1 = ParseLengthOrPercentage(lineObject.Attributes?["y1"]?.Value, height);
            x2 = ParseLengthOrPercentage(lineObject.Attributes?["x2"]?.Value, width);
            y2 = ParseLengthOrPercentage(lineObject.Attributes?["y2"]?.Value, height);

            PresentationAttributes lineAttributes = InterpretPresentationAttributes(lineObject, attributes, width, height, diagonal, gpr, styleSheets, gradients);

            bool hadClippingPath = ApplyClipPath(lineObject, gpr, width, height, diagonal, attributes, styleSheets, gradients);

            string tag = lineObject.Attributes?["id"]?.Value;

            if (lineAttributes.Stroke != null)
            {
                Brush strokeColour = lineAttributes.Stroke.MultiplyOpacity(lineAttributes.Opacity * lineAttributes.StrokeOpacity);
                gpr.StrokePath(new GraphicsPath().MoveTo(x1, y1).LineTo(x2, y2), strokeColour, lineAttributes.StrokeThickness, lineAttributes.LineCap, lineAttributes.LineJoin, lineAttributes.LineDash, tag: tag);
            }

            if (hadClippingPath)
            {
                gpr.Restore();
            }

            if (lineAttributes.NeedsRestore)
            {
                gpr.Restore();
            }
        }

        private static void SetStyleAttributes(XmlNode obj, IEnumerable<Stylesheet> styleSheets)
        {
            string style = obj.Attributes?["style"]?.Value;

            string classes = obj.Attributes?["class"]?.Value;

            if (!string.IsNullOrEmpty(classes))
            {
                string[] splitClasses = classes.Split(' ');

                foreach (string className in splitClasses)
                {
                    if (!string.IsNullOrEmpty(className.Trim()))
                    {
                        foreach (Stylesheet sheet in styleSheets)
                        {
                            foreach (StyleRule rule in sheet.StyleRules)
                            {
                                if (rule.SelectorText.Contains("." + className))
                                {
                                    style = rule.Style.CssText + "; " + style;
                                }
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(style))
            {
                string[] splitStyle = style.Split(';');

                for (int i = 0; i < splitStyle.Length; i++)
                {
                    string[] styleCouple = splitStyle[i].Split(':');

                    if (styleCouple.Length == 2)
                    {
                        string styleName = styleCouple[0].Trim();
                        string styleValue = styleCouple[1].Trim();

                        ((XmlElement)obj).SetAttribute(styleName, styleValue);
                    }
                    else if (!string.IsNullOrWhiteSpace(splitStyle[i]))
                    {
                        throw new InvalidOperationException("The style specification is not valid: " + splitStyle[i]);
                    }
                }
            }
        }

        internal static PresentationAttributes InterpretPresentationAttributes(XmlNode obj, PresentationAttributes parentPresentationAttributes, double width, double height, double diagonal, Graphics gpr, IEnumerable<Stylesheet> styleSheets, Dictionary<string, Brush> gradients)
        {
            SetStyleAttributes(obj, styleSheets);

            PresentationAttributes tbr = parentPresentationAttributes.Clone();

            string stroke = obj.Attributes?["stroke"]?.Value;
            string strokeOpacity = obj.Attributes?["stroke-opacity"]?.Value;
            string fill = obj.Attributes?["fill"]?.Value;
            string fillOpacity = obj.Attributes?["fill-opacity"]?.Value;
            string currentColour = obj.Attributes?["colour"]?.Value;
            string strokeThickness = obj.Attributes?["stroke-width"]?.Value;
            string lineCap = obj.Attributes?["stroke-linecap"]?.Value;
            string lineJoin = obj.Attributes?["stroke-linejoin"]?.Value;
            string opacity = obj.Attributes?["opacity"]?.Value;
            string strokeDashArray = obj.Attributes?["stroke-dasharray"]?.Value;
            string strokeDashOffset = obj.Attributes?["stroke-dashoffset"]?.Value;
            string paintOrder = obj.Attributes?["paint-order"]?.Value;

            string xA = obj.Attributes?["x"]?.Value;
            string yA = obj.Attributes?["y"]?.Value;
            string wA = obj.Attributes?["width"]?.Value;
            string hA = obj.Attributes?["height"]?.Value;

            string transform = obj.Attributes?["transform"]?.Value;

            if (xA != null)
            {
                tbr.X = ParseLengthOrPercentage(xA, width);
            }

            if (yA != null)
            {
                tbr.Y = ParseLengthOrPercentage(yA, height);
            }

            if (wA != null)
            {
                tbr.Width = ParseLengthOrPercentage(wA, width);
            }

            if (hA != null)
            {
                tbr.Height = ParseLengthOrPercentage(hA, height);
            }

            if (stroke != null)
            {
                if (stroke.Trim().StartsWith("url("))
                {
                    string url = stroke.Trim().Substring(4);
                    if (url.EndsWith(")"))
                    {
                        url = url.Substring(0, url.Length - 1);
                    }

                    url = url.Trim();

                    if (url.StartsWith("#"))
                    {
                        url = url.Substring(1);
                        if (gradients.TryGetValue(url, out Brush brush))
                        {
                            tbr.Stroke = brush;
                        }
                    }
                    else
                    {
                        tbr.Stroke = null;
                    }
                }
                else
                {
                    tbr.Stroke = Colour.FromCSSString(stroke);
                }
            }

            if (strokeOpacity != null)
            {
                tbr.StrokeOpacity = ParseLengthOrPercentage(strokeOpacity, 1);
            }

            if (fill != null)
            {
                if (fill.Trim().StartsWith("url("))
                {
                    string url = fill.Trim().Substring(4);
                    if (url.EndsWith(")"))
                    {
                        url = url.Substring(0, url.Length - 1);
                    }

                    url = url.Trim();

                    if (url.StartsWith("#"))
                    {
                        url = url.Substring(1);
                        if (gradients.TryGetValue(url, out Brush brush))
                        {
                            tbr.Fill = brush;
                        }
                    }
                    else
                    {
                        tbr.Fill = null;
                    }
                }
                else
                {
                    tbr.Fill = Colour.FromCSSString(fill);
                }
            }

            if (fillOpacity != null)
            {
                tbr.FillOpacity = ParseLengthOrPercentage(fillOpacity, 1);
            }

            if (currentColour != null)
            {
                tbr.CurrentColour = Colour.FromCSSString(currentColour);
            }

            if (strokeThickness != null)
            {
                tbr.StrokeThickness = ParseLengthOrPercentage(strokeThickness, diagonal);
            }

            if (lineCap != null)
            {
                if (lineCap.Equals("butt", StringComparison.OrdinalIgnoreCase))
                {
                    tbr.LineCap = LineCaps.Butt;
                }
                else if (lineCap.Equals("round", StringComparison.OrdinalIgnoreCase))
                {
                    tbr.LineCap = LineCaps.Round;
                }
                else if (lineCap.Equals("square", StringComparison.OrdinalIgnoreCase))
                {
                    tbr.LineCap = LineCaps.Square;
                }
            }

            if (lineJoin != null)
            {
                if (lineJoin.Equals("bevel", StringComparison.OrdinalIgnoreCase))
                {
                    tbr.LineJoin = LineJoins.Bevel;
                }
                else if (lineJoin.Equals("miter", StringComparison.OrdinalIgnoreCase) || lineJoin.Equals("miter-clip", StringComparison.OrdinalIgnoreCase))
                {
                    tbr.LineJoin = LineJoins.Miter;
                }
                else if (lineJoin.Equals("round", StringComparison.OrdinalIgnoreCase))
                {
                    tbr.LineJoin = LineJoins.Round;
                }
            }

            if (opacity != null)
            {
                tbr.Opacity = ParseLengthOrPercentage(opacity, 1);
            }

            if (strokeDashArray != null)
            {
                if (strokeDashArray != "none")
                {
                    double[] parsedArray = ParseListOfDoubles(strokeDashArray);

                    tbr.LineDash = new LineDash(parsedArray[0], parsedArray.Length > 1 ? parsedArray[1] : parsedArray[0], tbr.LineDash.Phase);
                }
                else
                {
                    tbr.LineDash = LineDash.SolidLine;
                }
            }

            if (strokeDashOffset != null)
            {
                tbr.LineDash = new LineDash(tbr.LineDash.UnitsOn, tbr.LineDash.UnitsOff, ParseLengthOrPercentage(strokeDashOffset, diagonal));
            }

            if (paintOrder != null)
            {
                if (paintOrder.Equals("normal", StringComparison.OrdinalIgnoreCase))
                {
                    tbr.StrokeFirst = false;
                }
                else
                {
                    if (paintOrder.IndexOf("stroke", StringComparison.OrdinalIgnoreCase) >= 0 && (paintOrder.IndexOf("fill", StringComparison.OrdinalIgnoreCase) < 0 || paintOrder.IndexOf("fill", StringComparison.OrdinalIgnoreCase) > paintOrder.IndexOf("stroke", StringComparison.OrdinalIgnoreCase)))
                    {
                        tbr.StrokeFirst = true;
                    }
                    else
                    {
                        tbr.StrokeFirst = false;
                    }
                }
            }

            if (transform != null)
            {
                gpr.Save();
                tbr.NeedsRestore = true;

                string[] transforms = ParseListOfTransforms(transform);

                for (int i = 0; i < transforms.Length; i++)
                {
                    if (transforms[i].Equals("matrix", StringComparison.OrdinalIgnoreCase))
                    {
                        double a = ParseLengthOrPercentage(transforms[i + 1], 1);
                        double b = ParseLengthOrPercentage(transforms[i + 2], 1);
                        double c = ParseLengthOrPercentage(transforms[i + 3], 1);
                        double d = ParseLengthOrPercentage(transforms[i + 4], 1);
                        double e = ParseLengthOrPercentage(transforms[i + 5], 1);
                        double f = ParseLengthOrPercentage(transforms[i + 6], 1);

                        gpr.Transform(a, b, c, d, e, f);
                        i += 6;
                    }
                    else if (transforms[i].Equals("translate", StringComparison.OrdinalIgnoreCase))
                    {
                        double x = ParseLengthOrPercentage(transforms[i + 1], 1);

                        double y;

                        if (i < transforms.Length - 2 && !double.IsNaN(y = ParseLengthOrPercentage(transforms[i + 2], 1)))
                        {
                            gpr.Translate(x, y);
                            i += 2;
                        }
                        else
                        {
                            gpr.Translate(x, 0);
                            i++;
                        }
                    }
                    else if (transforms[i].Equals("scale", StringComparison.OrdinalIgnoreCase))
                    {
                        double x = ParseLengthOrPercentage(transforms[i + 1], 1);

                        double y;

                        if (i < transforms.Length - 2 && !double.IsNaN(y = ParseLengthOrPercentage(transforms[i + 2], 1)))
                        {
                            gpr.Scale(x, y);
                            i += 2;
                        }
                        else
                        {
                            gpr.Scale(x, x);
                            i++;
                        }
                    }
                    else if (transforms[i].Equals("rotate", StringComparison.OrdinalIgnoreCase))
                    {
                        double a = ParseLengthOrPercentage(transforms[i + 1], 1) * Math.PI / 180;

                        double x, y;

                        if (i < transforms.Length - 3 && !double.IsNaN(x = ParseLengthOrPercentage(transforms[i + 2], 1)) && !double.IsNaN(y = ParseLengthOrPercentage(transforms[i + 3], 1)))
                        {
                            gpr.RotateAt(a, new Point(x, y));
                            i += 2;
                        }
                        else
                        {
                            gpr.Rotate(a);
                            i++;
                        }
                    }
                    else if (transforms[i].Equals("skewX", StringComparison.OrdinalIgnoreCase))
                    {
                        double psi = ParseLengthOrPercentage(transforms[i + 1], 1) * Math.PI / 180;

                        gpr.Transform(1, 0, Math.Tan(psi), 1, 0, 0);

                        i++;
                    }
                    else if (transforms[i].Equals("skewY", StringComparison.OrdinalIgnoreCase))
                    {
                        double psi = ParseLengthOrPercentage(transforms[i + 1], 1) * Math.PI / 180;

                        gpr.Transform(1, Math.Tan(psi), 0, 1, 0, 0);

                        i++;
                    }
                }
            }

            return tbr;
        }

        private static double ParseLengthOrPercentage(string value, double total, double defaultValue = 0)
        {
            if (value != null)
            {
                if (value.Contains("%"))
                {
                    value = value.Replace("%", "");
                    return double.Parse(value, System.Globalization.CultureInfo.InvariantCulture) * total / 100;
                }
                else if (double.TryParse(value.Replace("px", "").Replace("pt", ""), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
                {
                    return result;
                }
                else
                {
                    string cleanedNumber = Regexes.NumberRegex.Match(value).Value;
                    return double.Parse(cleanedNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            else
            {
                return defaultValue;
            }
        }

        internal class PresentationAttributes
        {
            public Dictionary<string, FontFamily> EmbeddedFonts;

            public Brush Stroke = null;
            public double StrokeOpacity = 1;
            public Brush Fill = Colour.FromRgb(0, 0, 0);
            public double FillOpacity = 1;
            public Brush CurrentColour = null;
            public double StrokeThickness = 1;
            public LineCaps LineCap = LineCaps.Butt;
            public LineJoins LineJoin = LineJoins.Miter;
            public double Opacity = 1;
            public LineDash LineDash = new LineDash(0, 0, 0);
            public bool NeedsRestore = false;
            public bool StrokeFirst = false;
            public double X;
            public double Y;
            public double Width;
            public double Height;

            public PresentationAttributes Clone()
            {
                return new PresentationAttributes()
                {
                    EmbeddedFonts = this.EmbeddedFonts,

                    Stroke = this.Stroke,
                    StrokeOpacity = this.StrokeOpacity,
                    Fill = this.Fill,
                    FillOpacity = this.FillOpacity,
                    CurrentColour = this.CurrentColour,
                    StrokeThickness = this.StrokeThickness,
                    LineCap = this.LineCap,
                    LineJoin = this.LineJoin,
                    Opacity = this.Opacity,
                    LineDash = this.LineDash,
                    StrokeFirst = this.StrokeFirst,
                    X = this.X,
                    Y = this.Y,
                    Width = this.Width,
                    Height = this.Height
                };
            }
        }

        private static class Regexes
        {
            public static Regex ListSeparator = new Regex("[ \\t\\n\\r\\f]*,[ \\t\\n\\r\\f]*|[ \\t\\n\\r\\f]+", RegexOptions.Compiled);
            public static Regex FontFamilySeparator = new Regex("(?:^|,)(\"(?:[^\"])*\"|[^,]*)", RegexOptions.Compiled);
            public static Regex NumberRegex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?", RegexOptions.Compiled);
        }

        private static double[] ParseListOfDoubles(string value)
        {
            if (value == null)
            {
                return null;
            }

            string[] splitValue = Regexes.ListSeparator.Split(value);
            double[] tbr = new double[splitValue.Length];

            for (int i = 0; i < splitValue.Length; i++)
            {
                tbr[i] = double.Parse(splitValue[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            return tbr;
        }

        private static string[] ParseListOfTransforms(string value)
        {
            if (value == null)
            {
                return null;
            }

            string[] splitValue = Regexes.ListSeparator.Split(value.Replace("(", " ").Replace(")", " ").Trim());

            return splitValue;
        }

        private static List<KeyValuePair<string, FontFamily>> GetEmbeddedFonts(string styleBlock)
        {
            StringReader sr = new StringReader(styleBlock);

            List<KeyValuePair<string, FontFamily>> tbr = new List<KeyValuePair<string, FontFamily>>();

            while (sr.Peek() >= 0)
            {
                string token = ReadCSSToken(sr);

                if (token.Equals("@font-face", StringComparison.OrdinalIgnoreCase))
                {
                    List<string> tokens = new List<string>();

                    while (!token.Equals("}", StringComparison.OrdinalIgnoreCase))
                    {
                        token = ReadCSSToken(sr);
                        tokens.Add(token);
                    }

                    KeyValuePair<string, FontFamily>? fontFace = ParseFontFaceBlock(tokens);

                    if (fontFace != null)
                    {
                        tbr.Add(fontFace.Value);
                    }
                }
            }

            return tbr;
        }

        private static IEnumerable<KeyValuePair<string, IFilter>> GetFilters(XmlNode definitionsNode, List<Stylesheet> styleSheets)
        {
            Dictionary<string, IFilter> tbr = new Dictionary<string, IFilter>();

            foreach (XmlNode definition in definitionsNode.ChildNodes)
            {
                if (definition.Name.Equals("filter", StringComparison.OrdinalIgnoreCase))
                {
                    XmlElement filter = (XmlElement)definition;

                    string id = filter.GetAttribute("id");

                    List<ILocationInvariantFilter> filterElements = new List<ILocationInvariantFilter>();

                    foreach (XmlNode filterDefinition in definition.ChildNodes)
                    {
                        if (filterDefinition.Name.Equals("feGaussianBlur", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlElement actualFilter = (XmlElement)filterDefinition;

                            string stdDeviation = actualFilter.GetAttribute("stdDeviation");

                            filterElements.Add(new GaussianBlurFilter(double.Parse(stdDeviation, System.Globalization.CultureInfo.InvariantCulture)));
                        }
                        else if (filterDefinition.Name.Equals("feColorMatrix", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlElement actualFilter = (XmlElement)filterDefinition;

                            string type = actualFilter.GetAttribute("type");

                            if (type.Equals("matrix", StringComparison.OrdinalIgnoreCase))
                            {
                                string values = actualFilter.GetAttribute("values");
                                double[] parsedValues = (from el in System.Text.RegularExpressions.Regex.Split(values, "\\s") select double.Parse(el.Trim())).ToArray();

                                if (parsedValues.Length == 20)
                                {
                                    double[,] matrix = new double[5, 5];
                                    matrix[4, 4] = 1;

                                    for (int i = 0; i < 20; i++)
                                    {
                                        int y = i / 5;
                                        int x = i % 5;

                                        matrix[y, x] = parsedValues[i];
                                    }

                                    filterElements.Add(new ColourMatrixFilter(new ColourMatrix(matrix)));
                                }
                            }
                        }
                    }

                    if (filterElements.Count > 0)
                    {
                        if (filterElements.Count == 1)
                        {
                            tbr.Add(id, filterElements[0]);
                        }
                        else
                        {
                            tbr.Add(id, new CompositeLocationInvariantFilter(filterElements));
                        }
                    }
                }
            }

            return tbr;
        }

        private static IEnumerable<KeyValuePair<string, XmlNode>> GetMasks(XmlNode definitionsNode, List<Stylesheet> styleSheets)
        {
            Dictionary<string, XmlNode> tbr = new Dictionary<string, XmlNode>();

            foreach (XmlNode definition in definitionsNode.ChildNodes)
            {
                if (definition.Name.Equals("mask", StringComparison.OrdinalIgnoreCase))
                {
                    XmlElement mask = (XmlElement)definition;

                    string id = mask.GetAttribute("id");

                    tbr.Add(id, definition);
                }
            }

            return tbr;
        }

        private static IEnumerable<KeyValuePair<string, Brush>> GetGradients(XmlNode definitionsNode, List<Stylesheet> styleSheets)
        {
            Dictionary<string, Brush> tbr = new Dictionary<string, Brush>();
            Dictionary<string, XmlNodeList> stopLists = new Dictionary<string, XmlNodeList>();

            foreach (XmlNode definition in definitionsNode.ChildNodes)
            {
                if (definition.Name.Equals("linearGradient", StringComparison.OrdinalIgnoreCase))
                {
                    XmlElement gradient = (XmlElement)definition;

                    string id = gradient.GetAttribute("id");

                    double x1;
                    double y1;
                    double x2;
                    double y2;

                    if (!(gradient.HasAttribute("x1") && double.TryParse(gradient.GetAttribute("x1"), out x1)))
                    {
                        x1 = 0;
                    }

                    if (!(gradient.HasAttribute("y1") && double.TryParse(gradient.GetAttribute("y1"), out y1)))
                    {
                        y1 = 0;
                    }

                    if (!(gradient.HasAttribute("x2") && double.TryParse(gradient.GetAttribute("x2"), out x2)))
                    {
                        x2 = 0;
                    }

                    if (!(gradient.HasAttribute("y2") && double.TryParse(gradient.GetAttribute("y2"), out y2)))
                    {
                        y2 = 0;
                    }

                    List<GradientStop> gradientStops = new List<GradientStop>();

                    XmlNodeList childNodes;

                    if (gradient.HasAttribute("xlink:href"))
                    {
                        string refId = gradient.GetAttribute("xlink:href").Trim();

                        if (refId.StartsWith("#"))
                        {
                            refId = refId.Substring(1);

                            if (!stopLists.TryGetValue(refId, out childNodes))
                            {
                                childNodes = gradient.ChildNodes;
                            }
                        }
                        else
                        {
                            childNodes = gradient.ChildNodes;
                        }
                    }
                    else
                    {
                        childNodes = gradient.ChildNodes;
                    }

                    stopLists[id] = childNodes;

                    foreach (XmlNode stopNode in childNodes)
                    {
                        if (stopNode.Name.Equals("stop", StringComparison.OrdinalIgnoreCase))
                        {
                            SetStyleAttributes(stopNode, styleSheets);

                            XmlElement stop = (XmlElement)stopNode;

                            double offset = 0;
                            double opacity = 1;

                            if (stop.HasAttribute("offset"))
                            {
                                offset = ParseLengthOrPercentage(stop.GetAttribute("offset"), 1);
                            }

                            if (stop.HasAttribute("stop-opacity"))
                            {
                                opacity = ParseLengthOrPercentage(stop.GetAttribute("stop-opacity"), 1);
                            }

                            Colour stopColour = Colour.FromRgba(0, 0, 0, 0);

                            if (stop.HasAttribute("stop-color"))
                            {
                                stopColour = (Colour.FromCSSString(stop.GetAttribute("stop-color")) ?? stopColour).WithAlpha(opacity);
                            }

                            gradientStops.Add(new GradientStop(stopColour, offset));
                        }
                    }

                    if (gradient.HasAttribute("gradientTransform"))
                    {
                        string transform = gradient.GetAttribute("gradientTransform");
                        string[] transforms = ParseListOfTransforms(transform);

                        double[,] transformMatrix = MatrixUtils.Identity;

                        for (int i = 0; i < transforms.Length; i++)
                        {
                            if (transforms[i].Equals("matrix", StringComparison.OrdinalIgnoreCase))
                            {
                                double a = ParseLengthOrPercentage(transforms[i + 1], 1);
                                double b = ParseLengthOrPercentage(transforms[i + 2], 1);
                                double c = ParseLengthOrPercentage(transforms[i + 3], 1);
                                double d = ParseLengthOrPercentage(transforms[i + 4], 1);
                                double e = ParseLengthOrPercentage(transforms[i + 5], 1);
                                double f = ParseLengthOrPercentage(transforms[i + 6], 1);

                                transformMatrix = MatrixUtils.Multiply(transformMatrix, new double[,] { { a, c, e }, { b, d, f }, { 0, 0, 1 } });

                                i += 6;
                            }
                            else if (transforms[i].Equals("translate", StringComparison.OrdinalIgnoreCase))
                            {
                                double x = ParseLengthOrPercentage(transforms[i + 1], 1);

                                double y;

                                if (i < transforms.Length - 2 && !double.IsNaN(y = ParseLengthOrPercentage(transforms[i + 2], 1)))
                                {
                                    transformMatrix = MatrixUtils.Translate(transformMatrix, x, y);
                                    i += 2;
                                }
                                else
                                {
                                    transformMatrix = MatrixUtils.Translate(transformMatrix, x, 0);
                                    i++;
                                }
                            }
                            else if (transforms[i].Equals("scale", StringComparison.OrdinalIgnoreCase))
                            {
                                double x = ParseLengthOrPercentage(transforms[i + 1], 1);

                                double y;

                                if (i < transforms.Length - 2 && !double.IsNaN(y = ParseLengthOrPercentage(transforms[i + 2], 1)))
                                {
                                    transformMatrix = MatrixUtils.Scale(transformMatrix, x, y);
                                    i += 2;
                                }
                                else
                                {
                                    transformMatrix = MatrixUtils.Scale(transformMatrix, x, x);
                                    i++;
                                }
                            }
                            else if (transforms[i].Equals("rotate", StringComparison.OrdinalIgnoreCase))
                            {
                                double a = ParseLengthOrPercentage(transforms[i + 1], 1) * Math.PI / 180;

                                double x, y;

                                if (i < transforms.Length - 3 && !double.IsNaN(x = ParseLengthOrPercentage(transforms[i + 2], 1)) && !double.IsNaN(y = ParseLengthOrPercentage(transforms[i + 3], 1)))
                                {
                                    transformMatrix = MatrixUtils.Translate(transformMatrix, x, y);
                                    transformMatrix = MatrixUtils.Rotate(transformMatrix, a);
                                    transformMatrix = MatrixUtils.Translate(transformMatrix, -x, -y);
                                    i += 2;
                                }
                                else
                                {
                                    transformMatrix = MatrixUtils.Rotate(transformMatrix, a);
                                    i++;
                                }
                            }
                            else if (transforms[i].Equals("skewX", StringComparison.OrdinalIgnoreCase))
                            {
                                double psi = ParseLengthOrPercentage(transforms[i + 1], 1) * Math.PI / 180;

                                transformMatrix = MatrixUtils.Multiply(transformMatrix, new double[,] { { 1, Math.Tan(psi), 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
                                i++;
                            }
                            else if (transforms[i].Equals("skewY", StringComparison.OrdinalIgnoreCase))
                            {
                                double psi = ParseLengthOrPercentage(transforms[i + 1], 1) * Math.PI / 180;

                                transformMatrix = MatrixUtils.Multiply(transformMatrix, new double[,] { { 1, 0, 0 }, { Math.Tan(psi), 1, 0 }, { 0, 0, 1 } });
                                i++;
                            }
                        }

                        double[] start = MatrixUtils.Multiply(transformMatrix, new double[] { x1, y1 });
                        double[] end = MatrixUtils.Multiply(transformMatrix, new double[] { x2, y2 });

                        x1 = start[0];
                        y1 = start[1];
                        x2 = end[0];
                        y2 = end[1];
                    }

                    tbr.Add(id, new LinearGradientBrush(new Point(x1, y1), new Point(x2, y2), gradientStops));
                }
                else if (definition.Name.Equals("radialGradient", StringComparison.OrdinalIgnoreCase))
                {
                    XmlElement gradient = (XmlElement)definition;

                    string id = gradient.GetAttribute("id");

                    double cx;
                    double cy;
                    double r;

                    if (!(gradient.HasAttribute("cx") && double.TryParse(gradient.GetAttribute("cx"), out cx)))
                    {
                        cx = 0;
                    }

                    if (!(gradient.HasAttribute("cy") && double.TryParse(gradient.GetAttribute("cy"), out cy)))
                    {
                        cy = 0;
                    }

                    if (!(gradient.HasAttribute("r") && double.TryParse(gradient.GetAttribute("r"), out r)))
                    {
                        r = 0;
                    }

                    double fx;
                    double fy;

                    if (!(gradient.HasAttribute("fx") && double.TryParse(gradient.GetAttribute("fx"), out fx)))
                    {
                        fx = cx;
                    }

                    if (!(gradient.HasAttribute("fy") && double.TryParse(gradient.GetAttribute("fy"), out fy)))
                    {
                        fy = cy;
                    }

                    List<GradientStop> gradientStops = new List<GradientStop>();

                    XmlNodeList childNodes;

                    if (gradient.HasAttribute("xlink:href"))
                    {
                        string refId = gradient.GetAttribute("xlink:href").Trim();

                        if (refId.StartsWith("#"))
                        {
                            refId = refId.Substring(1);

                            if (!stopLists.TryGetValue(refId, out childNodes))
                            {
                                childNodes = gradient.ChildNodes;
                            }
                        }
                        else
                        {
                            childNodes = gradient.ChildNodes;
                        }
                    }
                    else
                    {
                        childNodes = gradient.ChildNodes;
                    }

                    stopLists[id] = childNodes;

                    foreach (XmlNode stopNode in childNodes)
                    {
                        if (stopNode.Name.Equals("stop", StringComparison.OrdinalIgnoreCase))
                        {
                            SetStyleAttributes(stopNode, styleSheets);

                            XmlElement stop = (XmlElement)stopNode;

                            double offset = 0;
                            double opacity = 1;

                            if (stop.HasAttribute("offset"))
                            {
                                offset = ParseLengthOrPercentage(stop.GetAttribute("offset"), 1);
                            }

                            if (stop.HasAttribute("stop-opacity"))
                            {
                                opacity = ParseLengthOrPercentage(stop.GetAttribute("stop-opacity"), 1);
                            }

                            Colour stopColour = Colour.FromRgba(0, 0, 0, 0);

                            if (stop.HasAttribute("stop-color"))
                            {
                                stopColour = (Colour.FromCSSString(stop.GetAttribute("stop-color")) ?? stopColour).WithAlpha(opacity);
                            }

                            gradientStops.Add(new GradientStop(stopColour, offset));
                        }
                    }

                    if (gradient.HasAttribute("gradientTransform"))
                    {
                        string transform = gradient.GetAttribute("gradientTransform");
                        string[] transforms = ParseListOfTransforms(transform);

                        double[,] transformMatrix = MatrixUtils.Identity;

                        for (int i = 0; i < transforms.Length; i++)
                        {
                            if (transforms[i].Equals("matrix", StringComparison.OrdinalIgnoreCase))
                            {
                                double a = ParseLengthOrPercentage(transforms[i + 1], 1);
                                double b = ParseLengthOrPercentage(transforms[i + 2], 1);
                                double c = ParseLengthOrPercentage(transforms[i + 3], 1);
                                double d = ParseLengthOrPercentage(transforms[i + 4], 1);
                                double e = ParseLengthOrPercentage(transforms[i + 5], 1);
                                double f = ParseLengthOrPercentage(transforms[i + 6], 1);

                                transformMatrix = MatrixUtils.Multiply(transformMatrix, new double[,] { { a, c, e }, { b, d, f }, { 0, 0, 1 } });

                                i += 6;
                            }
                            else if (transforms[i].Equals("translate", StringComparison.OrdinalIgnoreCase))
                            {
                                double x = ParseLengthOrPercentage(transforms[i + 1], 1);

                                double y;

                                if (i < transforms.Length - 2 && !double.IsNaN(y = ParseLengthOrPercentage(transforms[i + 2], 1)))
                                {
                                    transformMatrix = MatrixUtils.Translate(transformMatrix, x, y);
                                    i += 2;
                                }
                                else
                                {
                                    transformMatrix = MatrixUtils.Translate(transformMatrix, x, 0);
                                    i++;
                                }
                            }
                            else if (transforms[i].Equals("scale", StringComparison.OrdinalIgnoreCase))
                            {
                                double x = ParseLengthOrPercentage(transforms[i + 1], 1);

                                double y;

                                if (i < transforms.Length - 2 && !double.IsNaN(y = ParseLengthOrPercentage(transforms[i + 2], 1)))
                                {
                                    transformMatrix = MatrixUtils.Scale(transformMatrix, x, y);
                                    i += 2;
                                }
                                else
                                {
                                    transformMatrix = MatrixUtils.Scale(transformMatrix, x, x);
                                    i++;
                                }
                            }
                            else if (transforms[i].Equals("rotate", StringComparison.OrdinalIgnoreCase))
                            {
                                double a = ParseLengthOrPercentage(transforms[i + 1], 1) * Math.PI / 180;

                                double x, y;

                                if (i < transforms.Length - 3 && !double.IsNaN(x = ParseLengthOrPercentage(transforms[i + 2], 1)) && !double.IsNaN(y = ParseLengthOrPercentage(transforms[i + 3], 1)))
                                {
                                    transformMatrix = MatrixUtils.Translate(transformMatrix, x, y);
                                    transformMatrix = MatrixUtils.Rotate(transformMatrix, a);
                                    transformMatrix = MatrixUtils.Translate(transformMatrix, -x, -y);
                                    i += 2;
                                }
                                else
                                {
                                    transformMatrix = MatrixUtils.Rotate(transformMatrix, a);
                                    i++;
                                }
                            }
                            else if (transforms[i].Equals("skewX", StringComparison.OrdinalIgnoreCase))
                            {
                                double psi = ParseLengthOrPercentage(transforms[i + 1], 1) * Math.PI / 180;

                                transformMatrix = MatrixUtils.Multiply(transformMatrix, new double[,] { { 1, Math.Tan(psi), 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
                                i++;
                            }
                            else if (transforms[i].Equals("skewY", StringComparison.OrdinalIgnoreCase))
                            {
                                double psi = ParseLengthOrPercentage(transforms[i + 1], 1) * Math.PI / 180;

                                transformMatrix = MatrixUtils.Multiply(transformMatrix, new double[,] { { 1, 0, 0 }, { Math.Tan(psi), 1, 0 }, { 0, 0, 1 } });
                                i++;
                            }
                        }

                        double determinant = transformMatrix[0, 0] * (transformMatrix[1, 1] * transformMatrix[2, 2] - transformMatrix[1, 2] * transformMatrix[2, 1]) -
                            transformMatrix[0, 1] * (transformMatrix[1, 0] * transformMatrix[2, 2] - transformMatrix[1, 2] * transformMatrix[2, 0]) +
                            transformMatrix[0, 2] * (transformMatrix[1, 0] * transformMatrix[2, 1] - transformMatrix[1, 1] * transformMatrix[2, 0]);

                        double[] focus = MatrixUtils.Multiply(transformMatrix, new double[] { fx, fy });
                        double[] centre = MatrixUtils.Multiply(transformMatrix, new double[] { cx, cy });

                        fx = focus[0];
                        fy = focus[1];
                        cx = centre[0];
                        cy = centre[1];
                        r = r * Math.Sqrt(determinant);
                    }

                    tbr.Add(id, new RadialGradientBrush(new Point(fx, fy), new Point(cx, cy), r, gradientStops));
                }
            }


            return tbr;
        }

        private static KeyValuePair<string, FontFamily>? ParseFontFaceBlock(List<string> tokens)
        {
            int fontFamilyInd = tokens.IndexOf("font-family");
            string fontFamilyName = tokens[fontFamilyInd + 2].Trim().Trim('"').Trim();

            int srcInd = tokens.IndexOf("src");
            string src = tokens[srcInd + 2];

            string mimeType = src.Substring(src.IndexOf("data:") + 5);
            mimeType = mimeType.Substring(0, mimeType.IndexOf(";"));

            if (mimeType.Equals("font/ttf", StringComparison.OrdinalIgnoreCase) || mimeType.Equals("font/truetype", StringComparison.OrdinalIgnoreCase) || mimeType.Equals("application/x-font-ttf", StringComparison.OrdinalIgnoreCase))
            {
                src = src.Substring(src.IndexOf("base64,") + 7);
                src = src.TrimEnd(')').TrimEnd('\"').TrimEnd(')');
                byte[] fontBytes = Convert.FromBase64String(src);

                string tempFile = Path.GetTempFileName();

                File.WriteAllBytes(tempFile, fontBytes);

                FontFamily family = FontFamily.ResolveFontFamily(tempFile);
                return new KeyValuePair<string, FontFamily>(fontFamilyName, family);
            }

            return null;
        }

        private const string CSSDelimiters = ":;,{}";

        private static string ReadCSSToken(StringReader reader)
        {
            StringBuilder tbr = new StringBuilder();

            bool openQuotes = false;
            int openParentheses = 0;

            int c = reader.Read();
            if (c >= 0)
            {
                tbr.Append((char)c);

                if ((char)c == '"')
                {
                    openQuotes = !openQuotes;
                }

                if ((char)c == '(')
                {
                    openParentheses++;
                }
                if ((char)c == ')')
                {
                    openParentheses--;
                }


                while (c >= 0 && (!CSSDelimiters.Contains((char)c) || openQuotes || openParentheses > 0))
                {
                    c = reader.Read();
                    tbr.Append((char)c);
                    if ((char)c == '"')
                    {
                        openQuotes = !openQuotes;
                    }
                    if ((char)c == '(')
                    {
                        openParentheses++;
                    }
                    if ((char)c == ')')
                    {
                        openParentheses--;
                    }
                    c = reader.Peek();
                }
            }

            string val = tbr.ToString().Trim();

            return (string.IsNullOrEmpty(val) && c >= 0) ? ReadCSSToken(reader) : val;
        }
    }

    internal class PathTransformerGraphicsContext : IGraphicsContext
    {
        public GraphicsPath CurrentPath = new GraphicsPath();
        public double[,] TransformMatrix = MatrixUtils.Identity;

        private Stack<double[,]> transformMatrices;
        public PathTransformerGraphicsContext()
        {
            transformMatrices = new Stack<double[,]>();
            transformMatrices.Push((double[,])TransformMatrix.Clone());
        }

        public void CubicBezierTo(double p1X, double p1Y, double p2X, double p2Y, double p3X, double p3Y)
        {
            double[] p1 = MatrixUtils.Multiply(TransformMatrix, new double[] { p1X, p1Y });
            double[] p2 = MatrixUtils.Multiply(TransformMatrix, new double[] { p2X, p2Y });
            double[] p3 = MatrixUtils.Multiply(TransformMatrix, new double[] { p3X, p3Y });

            CurrentPath.CubicBezierTo(p1[0], p1[1], p2[0], p2[1], p3[0], p3[1]);
        }

        public void LineTo(double x, double y)
        {
            double[] p = MatrixUtils.Multiply(TransformMatrix, new double[] { x, y });
            CurrentPath.LineTo(p[0], p[1]);
        }

        public void MoveTo(double x, double y)
        {
            double[] p = MatrixUtils.Multiply(TransformMatrix, new double[] { x, y });
            CurrentPath.MoveTo(p[0], p[1]);
        }

        public void Close()
        {
            CurrentPath.Close();
        }

        public void Restore()
        {
            TransformMatrix = transformMatrices.Pop();
        }

        public void Rotate(double angle)
        {
            TransformMatrix = MatrixUtils.Rotate(TransformMatrix, angle);
        }

        public void Save()
        {
            transformMatrices.Push((double[,])TransformMatrix.Clone());
        }

        public void Scale(double scaleX, double scaleY)
        {
            TransformMatrix = MatrixUtils.Scale(TransformMatrix, scaleX, scaleY);
        }

        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            double[,] transfMatrix = new double[3, 3] { { a, c, e }, { b, d, f }, { 0, 0, 1 } };
            TransformMatrix = MatrixUtils.Multiply(TransformMatrix, transfMatrix);
        }

        public void Translate(double x, double y)
        {
            TransformMatrix = MatrixUtils.Translate(TransformMatrix, x, y);
        }



        public double Width => throw new NotImplementedException();

        public double Height => throw new NotImplementedException();

        public Font Font { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TextBaselines TextBaseline { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Brush FillStyle => Colours.Black;

        public Brush StrokeStyle => Colours.Black;

        public double LineWidth { get => 1; set { } }
        public LineCaps LineCap { set { } }
        public LineJoins LineJoin { set { } }
        public string Tag { get => null; set { } }

        public void DrawRasterImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image)
        {
            throw new NotImplementedException();
        }

        public void Fill()
        {

        }

        public void FillText(string text, double x, double y)
        {
            throw new NotImplementedException();
        }

        public void Rectangle(double x0, double y0, double width, double height)
        {
            MoveTo(x0, y0);
            LineTo(x0 + width, y0);
            LineTo(x0 + width, y0 + height);
            LineTo(x0, y0 + height);
            Close();
        }

        public void SetClippingPath()
        {
            throw new NotImplementedException();
        }

        public void SetFillStyle((int r, int g, int b, double a) style)
        {

        }

        public void SetFillStyle(Brush style)
        {

        }

        public void SetLineDash(LineDash dash)
        {

        }

        public void SetStrokeStyle((int r, int g, int b, double a) style)
        {

        }

        public void SetStrokeStyle(Brush style)
        {

        }

        public void Stroke()
        {

        }

        public void StrokeText(string text, double x, double y)
        {
            throw new NotImplementedException();
        }

        public void DrawFilteredGraphics(Graphics graphics, IFilter filter)
        {
            graphics.CopyToIGraphicsContext(this);
        }
    }
}
