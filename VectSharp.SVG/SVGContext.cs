/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020  Giorgio Bianchini
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace VectSharp.SVG
{
    internal static class MatrixUtils
    {
        public static double[] Multiply(double[,] matrix, double[] vector)
        {
            double[] tbr = new double[2];

            tbr[0] = matrix[0, 0] * vector[0] + matrix[0, 1] * vector[1] + matrix[0, 2];
            tbr[1] = matrix[1, 0] * vector[0] + matrix[1, 1] * vector[1] + matrix[1, 2];

            return tbr;
        }

        public static double[,] Multiply(double[,] matrix1, double[,] matrix2)
        {
            double[,] tbr = new double[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        tbr[i, j] += matrix1[i, k] * matrix2[k, j];
                    }
                }
            }

            return tbr;
        }

        public static double[,] Rotate(double[,] matrix, double angle)
        {
            double[,] rotationMatrix = new double[3, 3];
            rotationMatrix[0, 0] = Math.Cos(angle);
            rotationMatrix[0, 1] = -Math.Sin(angle);
            rotationMatrix[1, 0] = Math.Sin(angle);
            rotationMatrix[1, 1] = Math.Cos(angle);
            rotationMatrix[2, 2] = 1;

            return Multiply(matrix, rotationMatrix);
        }

        public static double[,] Translate(double[,] matrix, double x, double y)
        {
            double[,] translationMatrix = new double[3, 3];
            translationMatrix[0, 0] = 1;
            translationMatrix[0, 2] = x;
            translationMatrix[1, 1] = 1;
            translationMatrix[1, 2] = y;
            translationMatrix[2, 2] = 1;

            return Multiply(matrix, translationMatrix);
        }

        public static double[,] Scale(double[,] matrix, double scaleX, double scaleY)
        {
            double[,] scaleMatrix = new double[3, 3];
            scaleMatrix[0, 0] = scaleX;
            scaleMatrix[1, 1] = scaleY;
            scaleMatrix[2, 2] = 1;

            return Multiply(matrix, scaleMatrix);
        }

        public static double[,] Identity = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
    }

    internal class SVGFigure
    {
        public Point StartPoint { get; set; }
        public Point CurrentPoint { get; set; }

        public string Data { get; set; }
        public int PointCount { get; set; } = 0;
    }

    internal class SVGPathObject
    {
        public List<SVGFigure> Figures { get; set; } = new List<SVGFigure>();
    }


    internal class SVGContext : IGraphicsContext
    {
        public Dictionary<string, FontFamily> UsedFontFamilies;
        public Dictionary<string, HashSet<char>> UsedChars;


        public const string SVGNamespace = "http://www.w3.org/2000/svg";

        public double Width { get; }

        public double Height { get; }

        public Font Font { get; set; }
        public TextBaselines TextBaseline { get; set; }

        public Colour FillStyle { get; private set; }

        public Colour StrokeStyle { get; private set; }

        public double LineWidth { get; set; }
        public LineCaps LineCap { private get; set; }
        public LineJoins LineJoin { private get; set; }

        private LineDash _lineDash;

        private SVGPathObject currentPath;
        private SVGFigure currentFigure;

        public XmlDocument Document;
        private XmlElement currentElement;

        public string Tag { get; set; }

        private double[,] _transform;
        private Stack<double[,]> states;

        private bool TextToPaths = false;

        public SVGContext(double width, double height, bool textToPaths)
        {
            this.Width = width;
            this.Height = height;

            currentPath = new SVGPathObject();
            currentFigure = new SVGFigure();

            StrokeStyle = Colour.FromRgba(0, 0, 0, 0);
            FillStyle = Colour.FromRgb(0, 0, 0);
            LineWidth = 1;

            LineCap = LineCaps.Butt;
            LineJoin = LineJoins.Miter;
            _lineDash = new LineDash(0, 0, 0);

            Font = new Font(new FontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);

            TextBaseline = TextBaselines.Top;

            _transform = new double[3, 3];

            _transform[0, 0] = 1;
            _transform[1, 1] = 1;
            _transform[2, 2] = 1;

            states = new Stack<double[,]>();

            UsedFontFamilies = new Dictionary<string, FontFamily>();
            UsedChars = new Dictionary<string, HashSet<char>>();

            Document = new XmlDocument();

            Document.InsertBefore(Document.CreateXmlDeclaration("1.0", "UTF-8", null), Document.DocumentElement);

            currentElement = Document.CreateElement(null, "svg", SVGNamespace);
            currentElement.SetAttribute("viewBox", "0 0 " + width.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + height.ToString(System.Globalization.CultureInfo.InvariantCulture));
            currentElement.SetAttribute("version", "1.1");
            Document.AppendChild(currentElement);

            this.TextToPaths = textToPaths;
        }


        private void PathText(string text, double x, double y)
        {
            GraphicsPath textPath = new GraphicsPath().AddText(x, y, text, Font, TextBaseline);

            for (int j = 0; j < textPath.Segments.Count; j++)
            {
                switch (textPath.Segments[j].Type)
                {
                    case VectSharp.SegmentType.Move:
                        this.MoveTo(textPath.Segments[j].Point.X, textPath.Segments[j].Point.Y);
                        break;
                    case VectSharp.SegmentType.Line:
                        this.LineTo(textPath.Segments[j].Point.X, textPath.Segments[j].Point.Y);
                        break;
                    case VectSharp.SegmentType.CubicBezier:
                        this.CubicBezierTo(textPath.Segments[j].Points[0].X, textPath.Segments[j].Points[0].Y, textPath.Segments[j].Points[1].X, textPath.Segments[j].Points[1].Y, textPath.Segments[j].Points[2].X, textPath.Segments[j].Points[2].Y);
                        break;
                    case VectSharp.SegmentType.Close:
                        this.Close();
                        break;
                }
            }
        }

        public void Close()
        {
            if (currentFigure.PointCount > 0)
            {
                currentFigure.Data += "Z ";
                currentPath.Figures.Add(currentFigure);

                currentFigure = new SVGFigure();
            }
        }

        public void CubicBezierTo(double p1X, double p1Y, double p2X, double p2Y, double p3X, double p3Y)
        {
            if (currentFigure.PointCount == 0)
            {
                currentFigure.StartPoint = new Point(p1X, p1Y);
            }

            currentFigure.CurrentPoint = new Point(p3X, p3Y);
            currentFigure.Data += "C " + p1X.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + p1Y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", " +
                p2X.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + p2Y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", " +
                p3X.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + p3Y.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ";
            currentFigure.PointCount += 3;
        }

        public void Fill()
        {
            if (currentFigure.PointCount > 0)
            {
                currentPath.Figures.Add(currentFigure);
            }

            XmlElement path = Document.CreateElement("path", SVGNamespace);
            path.SetAttribute("d", currentPath.Figures.Aggregate("", (a, b) => a + b.Data));
            path.SetAttribute("stroke", "none");
            path.SetAttribute("fill", FillStyle.ToCSSString(false));
            path.SetAttribute("fill-opacity", FillStyle.A.ToString(System.Globalization.CultureInfo.InvariantCulture));
            path.SetAttribute("transform", "matrix(" + _transform[0, 0].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + _transform[1, 0].ToString(System.Globalization.CultureInfo.InvariantCulture) +
                "," + _transform[0, 1].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + _transform[1, 1].ToString(System.Globalization.CultureInfo.InvariantCulture) +
                "," + _transform[0, 2].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + _transform[1, 2].ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");

            if (!string.IsNullOrEmpty(Tag))
            {
                path.SetAttribute("id", Tag);
            }

            currentElement.AppendChild(path);

            currentPath = new SVGPathObject();
            currentFigure = new SVGFigure();

        }

        public void FillText(string text, double x, double y)
        {
            if (!TextToPaths)
            {

                if (!UsedFontFamilies.ContainsKey(Font.FontFamily.FileName))
                {
                    UsedFontFamilies.Add(Font.FontFamily.FileName, Font.FontFamily);
                    UsedChars.Add(Font.FontFamily.FileName, new HashSet<char>());
                }

                UsedChars[Font.FontFamily.FileName].UnionWith(text);

                Font.DetailedFontMetrics metrics = Font.MeasureTextAdvanced(text);

                double[,] currTransform = null;

                switch (TextBaseline)
                {
                    case TextBaselines.Baseline:
                        currTransform = MatrixUtils.Translate(_transform, x - metrics.LeftSideBearing, y);
                        break;
                    case TextBaselines.Top:
                        currTransform = MatrixUtils.Translate(_transform, x - metrics.LeftSideBearing, y + metrics.Top);
                        break;
                    case TextBaselines.Bottom:
                        currTransform = MatrixUtils.Translate(_transform, x - metrics.LeftSideBearing, y + metrics.Bottom);
                        break;
                    case TextBaselines.Middle:
                        currTransform = MatrixUtils.Translate(_transform, x - metrics.LeftSideBearing, y + (metrics.Top + metrics.Bottom) * 0.5);
                        break;
                    default:
                        currTransform = MatrixUtils.Translate(_transform, x - metrics.LeftSideBearing, y);
                        break;
                }

                XmlElement textElement = Document.CreateElement("text", SVGNamespace);

                textElement.SetAttribute("stroke", "none");
                textElement.SetAttribute("fill", FillStyle.ToCSSString(false));
                textElement.SetAttribute("fill-opacity", FillStyle.A.ToString(System.Globalization.CultureInfo.InvariantCulture));
                textElement.SetAttribute("transform", "matrix(" + currTransform[0, 0].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + currTransform[1, 0].ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    "," + currTransform[0, 1].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + currTransform[1, 1].ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    "," + currTransform[0, 2].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + currTransform[1, 2].ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");

                textElement.SetAttribute("x", "0");
                textElement.SetAttribute("y", "0");
                textElement.SetAttribute("font-size", Font.FontSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
                textElement.SetAttribute("font-family", Font.FontFamily.FileName);

                if (Font.FontFamily.IsBold)
                {
                    textElement.SetAttribute("font-weight", "bold");
                }
                else
                {
                    textElement.SetAttribute("font-weight", "regular");
                }

                if (Font.FontFamily.IsItalic)
                {
                    textElement.SetAttribute("font-style", "italic");
                }
                else
                {
                    textElement.SetAttribute("font-style", "normal");
                }

                if (Font.FontFamily.IsOblique)
                {
                    textElement.SetAttribute("font-style", "oblique");
                }

                textElement.InnerText = text;

                if (!string.IsNullOrEmpty(Tag))
                {
                    textElement.SetAttribute("id", Tag);
                }

                currentElement.AppendChild(textElement);

            }
            else
            {
                PathText(text, x, y);
                Fill();
            }
        }

        public void LineTo(double x, double y)
        {
            if (currentFigure.PointCount == 0)
            {
                currentFigure.StartPoint = new Point(x, y);
            }

            currentFigure.CurrentPoint = new Point(x, y);
            currentFigure.Data += "L " + x.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + y.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ";
            currentFigure.PointCount++;
        }

        public void MoveTo(double x, double y)
        {
            if (currentFigure.PointCount > 0)
            {
                currentPath.Figures.Add(currentFigure);
            }

            currentFigure = new SVGFigure();
            currentFigure.CurrentPoint = new Point(x, y);
            currentFigure.StartPoint = new Point(x, y);
            currentFigure.Data += "M " + x.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + y.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ";
            currentFigure.PointCount = 1;
        }

        public void Rectangle(double x0, double y0, double width, double height)
        {
            MoveTo(x0, y0);
            LineTo(x0 + width, y0);
            LineTo(x0 + width, y0 + height);
            LineTo(x0, y0 + height);
            Close();
        }

        public void Restore()
        {
            _transform = states.Pop();
            currentPath = new SVGPathObject();
            currentFigure = new SVGFigure();
        }

        public void Rotate(double angle)
        {
            _transform = MatrixUtils.Rotate(_transform, angle);
            currentPath = new SVGPathObject(); currentPath = new SVGPathObject();
            currentFigure = new SVGFigure();
        }

        public void Save()
        {
            states.Push((double[,])_transform.Clone());
        }

        public void Scale(double scaleX, double scaleY)
        {
            _transform = MatrixUtils.Scale(_transform, scaleX, scaleY);
            currentPath = new SVGPathObject();
            currentFigure = new SVGFigure();
        }

        public void SetFillStyle((int r, int g, int b, double a) style)
        {
            FillStyle = Colour.FromRgba(style);
        }

        public void SetFillStyle(Colour style)
        {
            FillStyle = style;
        }

        public void SetLineDash(LineDash dash)
        {
            _lineDash = dash;
        }

        public void SetStrokeStyle((int r, int g, int b, double a) style)
        {
            StrokeStyle = Colour.FromRgba(style);
        }

        public void SetStrokeStyle(Colour style)
        {
            StrokeStyle = style;
        }

        public void Stroke()
        {
            if (currentFigure.PointCount > 0)
            {
                currentPath.Figures.Add(currentFigure);
            }

            XmlElement path = Document.CreateElement("path", SVGNamespace);
            path.SetAttribute("d", currentPath.Figures.Aggregate("", (a, b) => a + b.Data));
            path.SetAttribute("stroke", StrokeStyle.ToCSSString(false));
            path.SetAttribute("stroke-opacity", StrokeStyle.A.ToString(System.Globalization.CultureInfo.InvariantCulture));
            path.SetAttribute("stroke-width", LineWidth.ToString(System.Globalization.CultureInfo.InvariantCulture));

            switch (LineCap)
            {
                case LineCaps.Butt:
                    path.SetAttribute("stroke-linecap", "butt");
                    break;
                case LineCaps.Round:
                    path.SetAttribute("stroke-linecap", "round");
                    break;
                case LineCaps.Square:
                    path.SetAttribute("stroke-linecap", "square");
                    break;
            }

            switch (LineJoin)
            {
                case LineJoins.Bevel:
                    path.SetAttribute("stroke-linejoin", "bevel");
                    break;
                case LineJoins.Round:
                    path.SetAttribute("stroke-linejoin", "round");
                    break;
                case LineJoins.Miter:
                    path.SetAttribute("stroke-linejoin", "miter");
                    break;
            }

            if (_lineDash.Phase != 0 || _lineDash.UnitsOn != 0 || _lineDash.UnitsOff != 0)
            {
                path.SetAttribute("stroke-dasharray", _lineDash.UnitsOn.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + _lineDash.UnitsOff.ToString(System.Globalization.CultureInfo.InvariantCulture));
                path.SetAttribute("stroke-dashoffset", _lineDash.Phase.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            path.SetAttribute("fill", "none");
            path.SetAttribute("transform", "matrix(" + _transform[0, 0].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + _transform[1, 0].ToString(System.Globalization.CultureInfo.InvariantCulture) +
                "," + _transform[0, 1].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + _transform[1, 1].ToString(System.Globalization.CultureInfo.InvariantCulture) +
                "," + _transform[0, 2].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + _transform[1, 2].ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");

            if (!string.IsNullOrEmpty(Tag))
            {
                path.SetAttribute("id", Tag);
            }

            currentElement.AppendChild(path);

            currentPath = new SVGPathObject();
            currentFigure = new SVGFigure();
        }

        public void StrokeText(string text, double x, double y)
        {
            if (!TextToPaths)
            {
                if (!UsedFontFamilies.ContainsKey(Font.FontFamily.FileName))
                {
                    UsedFontFamilies.Add(Font.FontFamily.FileName, Font.FontFamily);
                    UsedChars.Add(Font.FontFamily.FileName, new HashSet<char>());
                }

                UsedChars[Font.FontFamily.FileName].UnionWith(text);

                Font.DetailedFontMetrics metrics = Font.MeasureTextAdvanced(text);

                double[,] currTransform = null;

                switch (TextBaseline)
                {
                    case TextBaselines.Baseline:
                        currTransform = MatrixUtils.Translate(_transform, x - metrics.LeftSideBearing, y);
                        break;
                    case TextBaselines.Top:
                        currTransform = MatrixUtils.Translate(_transform, x - metrics.LeftSideBearing, y + metrics.Top);
                        break;
                    case TextBaselines.Bottom:
                        currTransform = MatrixUtils.Translate(_transform, x - metrics.LeftSideBearing, y + metrics.Bottom);
                        break;
                    case TextBaselines.Middle:
                        currTransform = MatrixUtils.Translate(_transform, x - metrics.LeftSideBearing, y + (metrics.Top + metrics.Bottom) * 0.5);
                        break;
                    default:
                        currTransform = MatrixUtils.Translate(_transform, x - metrics.LeftSideBearing, y);
                        break;
                }

                XmlElement textElement = Document.CreateElement("text", SVGNamespace);

                textElement.SetAttribute("stroke", StrokeStyle.ToCSSString(false));
                textElement.SetAttribute("stroke-opacity", StrokeStyle.A.ToString(System.Globalization.CultureInfo.InvariantCulture));
                textElement.SetAttribute("stroke-width", LineWidth.ToString(System.Globalization.CultureInfo.InvariantCulture));

                switch (LineCap)
                {
                    case LineCaps.Butt:
                        textElement.SetAttribute("stroke-linecap", "butt");
                        break;
                    case LineCaps.Round:
                        textElement.SetAttribute("stroke-linecap", "round");
                        break;
                    case LineCaps.Square:
                        textElement.SetAttribute("stroke-linecap", "square");
                        break;
                }

                switch (LineJoin)
                {
                    case LineJoins.Bevel:
                        textElement.SetAttribute("stroke-linejoin", "bevel");
                        break;
                    case LineJoins.Round:
                        textElement.SetAttribute("stroke-linejoin", "round");
                        break;
                    case LineJoins.Miter:
                        textElement.SetAttribute("stroke-linejoin", "miter");
                        break;
                }

                if (_lineDash.Phase != 0 || _lineDash.UnitsOn != 0 || _lineDash.UnitsOff != 0)
                {
                    textElement.SetAttribute("stroke-dasharray", _lineDash.UnitsOn.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + _lineDash.UnitsOff.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    textElement.SetAttribute("stroke-dashoffset", _lineDash.Phase.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                textElement.SetAttribute("fill", "none");
                textElement.SetAttribute("transform", "matrix(" + currTransform[0, 0].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + currTransform[1, 0].ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    "," + currTransform[0, 1].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + currTransform[1, 1].ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    "," + currTransform[0, 2].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + currTransform[1, 2].ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");

                textElement.SetAttribute("x", "0");
                textElement.SetAttribute("y", "0");
                textElement.SetAttribute("font-size", Font.FontSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
                textElement.SetAttribute("font-family", Font.FontFamily.FileName);

                if (Font.FontFamily.IsBold)
                {
                    textElement.SetAttribute("font-weight", "bold");
                }
                else
                {
                    textElement.SetAttribute("font-weight", "regular");
                }

                if (Font.FontFamily.IsItalic)
                {
                    textElement.SetAttribute("font-style", "italic");
                }
                else
                {
                    textElement.SetAttribute("font-style", "normal");
                }

                if (Font.FontFamily.IsOblique)
                {
                    textElement.SetAttribute("font-style", "oblique");
                }

                textElement.InnerText = text;

                if (!string.IsNullOrEmpty(Tag))
                {
                    textElement.SetAttribute("id", Tag);
                }

                currentElement.AppendChild(textElement);
            }
            else
            {
                PathText(text, x, y);
                Stroke();
            }
        }

        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            double[,] transfMatrix = new double[3, 3] { { a, c, e }, { b, d, f }, { 0, 0, 1 } };
            _transform = MatrixUtils.Multiply(_transform, transfMatrix);

            currentPath = new SVGPathObject();
            currentFigure = new SVGFigure();
        }

        public void Translate(double x, double y)
        {
            _transform = MatrixUtils.Translate(_transform, x, y);

            currentPath = new SVGPathObject();
            currentFigure = new SVGFigure();
        }
    }


    /// <summary>
    /// Contains methods to render a <see cref="Page"/> as an SVG file.
    /// </summary>
    public static class SVGContextInterpreter
    {

        /// <summary>
        /// Render the page to an SVG file.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="fileName">The full path to the file to save. If it exists, it will be overwritten.</param>
        /// <param name="textOption">Defines whether the used fonts should be included in the file.</param>
        public static void SaveAsSVG(this Page page, string fileName, TextOptions textOption = TextOptions.SubsetFonts)
        {
            using (FileStream sr = new FileStream(fileName, FileMode.Create))
            {
                page.SaveAsSVG(sr, textOption);
            }
        }

        /// <summary>
        /// Defines whether the used fonts should be included in the file.
        /// </summary>
        public enum TextOptions
        {
            /// <summary>
            /// Embeds the full font files.
            /// </summary>
            EmbedFonts,

            /// <summary>
            /// Embeds subsetted font files containing only the glyphs for the characters that have been used.
            /// </summary>
            SubsetFonts,

            /// <summary>
            /// Does not embed any font file and converts all text items into paths.
            /// </summary>
            ConvertIntoPaths,

            /// <summary>
            /// Does not embed any font file, but still encodes text items as such.
            /// </summary>
            DoNotEmbed
        }

        /// <summary>
        /// Render the page to an SVG stream.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="stream">The stream to which the SVG data will be written.</param>
        /// <param name="textOption">Defines whether the used fonts should be included in the file.</param>
        public static void SaveAsSVG(this Page page, Stream stream, TextOptions textOption = TextOptions.SubsetFonts)
        {
            bool textToPaths = textOption == TextOptions.ConvertIntoPaths;

            SVGContext ctx = new SVGContext(page.Width, page.Height, textToPaths);

            ctx.Rectangle(0, 0, page.Width, page.Height);
            ctx.SetFillStyle(page.Background);
            ctx.Fill();
            ctx.SetFillStyle((0, 0, 0, 1));

            page.Graphics.CopyToIGraphicsContext(ctx);

            if (!textToPaths && textOption != TextOptions.DoNotEmbed)
            {
                bool subsetFonts = textOption == TextOptions.SubsetFonts;

                StringBuilder cssFonts = new StringBuilder();

                Dictionary<string, string> newFontFamilies = new Dictionary<string, string>();

                foreach (KeyValuePair<string, FontFamily> kvp in ctx.UsedFontFamilies)
                {
                    TrueTypeFile subsettedFont;

                    if (subsetFonts)
                    {
                        newFontFamilies[kvp.Key] = kvp.Value.TrueTypeFile.GetFontFamilyName() + "-" + Guid.NewGuid().ToString();
                        subsettedFont = kvp.Value.TrueTypeFile.SubsetFont(new string(ctx.UsedChars[kvp.Key].ToArray()));
                    }
                    else
                    {
                        newFontFamilies[kvp.Key] = kvp.Value.TrueTypeFile.GetFontName();
                        subsettedFont = kvp.Value.TrueTypeFile;
                    }

                    byte[] fontBytes;

                    using (MemoryStream fontStream = new MemoryStream((int)subsettedFont.FontStream.Length))
                    {
                        subsettedFont.FontStream.Seek(0, SeekOrigin.Begin);
                        subsettedFont.FontStream.CopyTo(fontStream);

                        fontBytes = fontStream.ToArray();
                    }


                    cssFonts.Append("\n\t\t@font-face\n\t\t{\t\t\tfont-family: \"" + newFontFamilies[kvp.Key] + "\";\n\t\t\tsrc: url(\"data:font/ttf;charset=utf-8;base64,");
                    cssFonts.Append(Convert.ToBase64String(fontBytes));
                    cssFonts.Append("\");\n\t\t}\n");
                }

                XmlElement style = ctx.Document.CreateElement("style", SVGContext.SVGNamespace);
                style.InnerText = cssFonts.ToString();

                XmlNode svgElement = ctx.Document.GetElementsByTagName("svg")[0];

                svgElement.InsertBefore(style, svgElement.FirstChild);

                foreach (XmlNode text in ctx.Document.GetElementsByTagName("text"))
                {
                    string fontFamily = text.Attributes["font-family"].Value;

                    string fallbackFontFamily = "";

                    switch (fontFamily)
                    {
                        case "Helvetica":
                        case "Helvetica-Bold":
                        case "Helvetica-Oblique":
                        case "Helvetica-BoldOblique":
                            fallbackFontFamily = "sans-serif";
                            break;

                        case "Times-Roman":
                        case "Times-Bold":
                        case "Times-Italic":
                        case "Times-BoldItalic":
                            fallbackFontFamily = "serif";
                            break;

                        case "Courier":
                        case "Courier-Bold":
                        case "Courier-Oblique":
                        case "Courier-BoldOblique":
                            fallbackFontFamily = "monospace";
                            break;

                        default:
                            if (ctx.UsedFontFamilies[fontFamily].TrueTypeFile.IsFixedPitch())
                            {
                                fallbackFontFamily = "monospace";
                            }
                            else if (ctx.UsedFontFamilies[fontFamily].TrueTypeFile.IsScript() || ctx.UsedFontFamilies[fontFamily].TrueTypeFile.IsSerif())
                            {
                                fallbackFontFamily = "serif";
                            }
                            else
                            {
                                fallbackFontFamily = "sans-serif";
                            }
                            break;
                    }


                    if (!string.IsNullOrEmpty(fallbackFontFamily))
                    {
                        ((XmlElement)text).SetAttribute("font-family", newFontFamilies[fontFamily] + ", " + fallbackFontFamily);
                    }
                    else
                    {
                        ((XmlElement)text).SetAttribute("font-family", newFontFamilies[fontFamily]);
                    }

                }
            }
            else if (!textToPaths && textOption == TextOptions.DoNotEmbed)
            {
                foreach (XmlNode text in ctx.Document.GetElementsByTagName("text"))
                {
                    string fontFamily = text.Attributes["font-family"].Value;

                    string newFontFamily = ctx.UsedFontFamilies[fontFamily].TrueTypeFile.GetFontFamilyName();

                    switch (fontFamily)
                    {
                        case "Helvetica":
                        case "Helvetica-Bold":
                        case "Helvetica-Oblique":
                        case "Helvetica-BoldOblique":
                            newFontFamily = newFontFamily + ", sans-serif";
                            break;

                        case "Times-Roman":
                        case "Times-Bold":
                        case "Times-Italic":
                        case "Times-BoldItalic":
                            newFontFamily = newFontFamily + ", serif";
                            break;

                        case "Courier":
                        case "Courier-Bold":
                        case "Courier-Oblique":
                        case "Courier-BoldOblique":
                            newFontFamily = newFontFamily + ", monospace";
                            break;

                        default:
                            if (ctx.UsedFontFamilies[fontFamily].TrueTypeFile.IsFixedPitch())
                            {
                                newFontFamily = newFontFamily + ", monospace";
                            }
                            else if (ctx.UsedFontFamilies[fontFamily].TrueTypeFile.IsScript())
                            {
                                newFontFamily = newFontFamily + ", cursive";
                            }
                            else if (ctx.UsedFontFamilies[fontFamily].TrueTypeFile.IsSerif())
                            {
                                newFontFamily = newFontFamily + ", serif";
                            }
                            else
                            {
                                newFontFamily = newFontFamily + ", sans-serif";
                            }
                            break;
                    }

                    ((XmlElement)text).SetAttribute("font-family", newFontFamily);
                }
            }

            ctx.Document.DocumentElement.SetAttribute("style", "font-synthesis: none;");

            ctx.Document.Save(stream);
        }
    }
}
