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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace VectSharp.PDF
{
    internal enum SegmentType
    {
        Move, Line, CubicBezier, Arc, Close
    }

    internal abstract class Segment
    {
        public abstract SegmentType Type { get; }
        public Point[] Points { get; protected set; }

        public virtual Point Point
        {
            get
            {
                return Points[Points.Length - 1];
            }
        }

        public abstract Segment Clone();
    }

    internal class MoveSegment : Segment
    {
        public override SegmentType Type => SegmentType.Move;

        public MoveSegment(Point p)
        {
            this.Points = new Point[] { p };
        }

        public MoveSegment(double x, double y)
        {
            this.Points = new Point[] { new Point(x, y) };
        }

        public override Segment Clone()
        {
            return new MoveSegment(this.Point);
        }
    }

    internal class LineSegment : Segment
    {
        public override SegmentType Type => SegmentType.Line;

        public LineSegment(Point p)
        {
            this.Points = new Point[] { p };
        }

        public LineSegment(double x, double y)
        {
            this.Points = new Point[] { new Point(x, y) };
        }

        public override Segment Clone()
        {
            return new LineSegment(this.Point);
        }
    }

    internal class CloseSegment : Segment
    {
        public override SegmentType Type => SegmentType.Close;

        public CloseSegment() { }

        public override Segment Clone()
        {
            return new CloseSegment();
        }
    }

    internal class CubicBezierSegment : Segment
    {
        public override SegmentType Type => SegmentType.CubicBezier;
        public CubicBezierSegment(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            Points = new Point[] { new Point(x1, y1), new Point(x2, y2), new Point(x3, y3) };
        }

        public CubicBezierSegment(Point p1, Point p2, Point p3)
        {
            Points = new Point[] { p1, p2, p3 };
        }

        public override Segment Clone()
        {
            return new CubicBezierSegment(Points[0], Points[1], Points[2]);
        }
    }

    internal interface IFigure
    {
        Colour? Fill { get; }
        Colour? Stroke { get; }
        double LineWidth { get; }
        LineCaps LineCap { get; }
        LineJoins LineJoin { get; }
        LineDash LineDash { get; }
        bool IsClipping { get; }
        string Tag { get; }
    }

    internal class TransformFigure : IFigure
    {
        public enum TransformTypes
        {
            Transform, Save, Restore
        }

        public TransformTypes TransformType { get; }

        public Colour? Fill { get; }
        public Colour? Stroke { get; }

        public bool IsClipping { get; }
        public double LineWidth { get; }

        public double[,] TransformationMatrix { get; }

        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }
        public string Tag { get; }

        public TransformFigure(TransformTypes type, double[,] transformationMatrix, string tag)
        {
            this.TransformType = type;
            this.TransformationMatrix = transformationMatrix;
            this.Tag = tag;
        }
    }

    internal class RasterImageFigure : IFigure
    {

        public Colour? Fill { get; }
        public Colour? Stroke { get; }

        public bool IsClipping { get; }
        public double LineWidth { get; }

        public double[,] TransformationMatrix { get; }

        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }

        public RasterImage Image { get; }

        public string Tag { get; }

        public RasterImageFigure(RasterImage image, string tag)
        {
            this.Image = image;
            this.Tag = tag;
        }
    }

    internal class PathFigure : IFigure
    {
        public Colour? Fill { get; }
        public Colour? Stroke { get; }
        public bool IsClipping { get; }
        public double LineWidth { get; }

        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }
        public Segment[] Segments { get; }

        public string Tag { get; }
        public PathFigure(IEnumerable<Segment> segments, Colour? fill, Colour? stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, bool isClipping, string tag)
        {
            List<Segment> segs = new List<Segment>();

            foreach (Segment s in segments)
            {
                segs.Add(s.Clone());
            }

            this.Segments = segs.ToArray();

            Fill = fill;
            Stroke = stroke;
            LineWidth = lineWidth;
            LineCap = lineCap;
            LineJoin = lineJoin;
            LineDash = lineDash;
            IsClipping = isClipping;
            this.Tag = tag;
        }
    }

    internal class TextFigure : IFigure
    {
        public Colour? Fill { get; }
        public Colour? Stroke { get; }
        public double LineWidth { get; }
        public bool IsClipping { get; }
        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }

        public string Text { get; }

        public Font Font { get; }

        public Point Position { get; }

        public TextBaselines TextBaseline { get; }

        public string Tag { get; }

        public TextFigure(string text, Font font, Point position, TextBaselines textBaseline, Colour? fill, Colour? stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, string tag)
        {
            Text = text;
            Font = font;
            Position = position;
            TextBaseline = textBaseline;

            Fill = fill;
            Stroke = stroke;
            LineWidth = lineWidth;
            LineCap = lineCap;
            LineJoin = lineJoin;
            LineDash = lineDash;
            this.Tag = tag;
        }
    }

    internal class PDFFontDescriptor
    {
        private static readonly Random TagGenerator = new Random();
        private static readonly List<string> TagCache = new List<string>();

        public string FontName { get; }
        public string FontFamily { get; }
        public uint Flags { get; }
        public double[] FontBBox { get; }
        public int ItalicAngle => 0;
        public double Ascent { get; }
        public double Descent { get; }
        public double CapHeight { get { return Ascent; } }
        public int StemV => 80;
        public int StemH => 80;

        public PDFFontDescriptor(TrueTypeFile ttf, bool isSubset, bool isSymbolic)
        {
            this.Ascent = ttf.Get1000EmAscent();
            this.Descent = ttf.Get1000EmDescent();

            this.FontBBox = new double[] { ttf.Get1000EmXMin(), ttf.Get1000EmYMin(), ttf.Get1000EmXMax(), ttf.Get1000EmYMax() };

            bool fixedPitch = ttf.IsFixedPitch();

            bool serif = ttf.IsSerif();

            bool script = ttf.IsScript();

            bool italic = ttf.IsBold();

            bool allCap = false;

            bool smallCap = false;

            bool forceBold = false;

            this.Flags = (fixedPitch ? 1U : 0) | (serif ? 1U << 1 : 0) | (isSymbolic ? 1U << 2 : 0) | (script ? 1U << 3 : 0) | (!isSymbolic ? 1U << 5 : 0) | (italic ? 1U << 6 : 0) | (allCap ? 1U << 16 : 0) | (smallCap ? 1U << 17 : 0) | (forceBold ? 1U << 18 : 0);

            this.FontName = ttf.GetFontName();

            this.FontFamily = ttf.GetFontFamilyName();

            if (string.IsNullOrEmpty(this.FontFamily))
            {
                this.FontFamily = FontName;
            }

            if (isSubset)
            {
                string randString = "";

                while (randString.Length == 0 || TagCache.Contains(randString))
                {
                    randString = "";
                    for (int i = 0; i < 6; i++)
                    {
                        randString += (char)TagGenerator.Next(65, 91);
                    }
                }

                this.FontName = randString + "+" + this.FontName;
            }
        }
    }

    internal class PDFContext : IGraphicsContext
    {
        public string Tag { get; set; }
        public double Width { get; }
        public double Height { get; }


        private List<Segment> _currentFigure;

        internal List<IFigure> _figures;

        private Colour _strokeStyle;
        private Colour _fillStyle;
        private LineDash _lineDash;

        private readonly bool _textToPaths;

        public PDFContext(double width, double height, Colour background, bool textToPaths)
        {
            this.Width = width;
            this.Height = height;

            _currentFigure = new List<Segment>();
            _figures = new List<IFigure>();

            _strokeStyle = Colour.FromRgb(0, 0, 0);
            _fillStyle = Colour.FromRgb(0, 0, 0);
            LineWidth = 1;

            LineCap = LineCaps.Butt;
            LineJoin = LineJoins.Miter;
            _lineDash = new LineDash(0, 0, 0);

            _textToPaths = textToPaths;

            Font = new Font(new FontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);

            TextBaseline = TextBaselines.Top;

            this.Translate(0, height);
            this.Scale(1, -1);

            this.Rectangle(0, 0, width, height);
            this.SetFillStyle(background);
            this.Fill();

            this.SetFillStyle(Colour.FromRgb(0, 0, 0));
        }


        public void MoveTo(double x, double y)
        {
            _currentFigure.Add(new MoveSegment(x, y));
        }

        public void LineTo(double x, double y)
        {
            _currentFigure.Add(new LineSegment(x, y));
        }

        public void Close()
        {
            _currentFigure.Add(new CloseSegment());
        }

        public void Rectangle(double x0, double y0, double width, double height)
        {
            MoveTo(x0, y0);
            LineTo(x0 + width, y0);
            LineTo(x0 + width, y0 + height);
            LineTo(x0, y0 + height);
            Close();
        }
        public void SetStrokeStyle((int r, int g, int b, double a) style)
        {
            _strokeStyle = Colour.FromRgba(style.r, style.g, style.b, style.a);
        }

        public void SetStrokeStyle(Colour style)
        {
            _strokeStyle = style;
        }

        public void SetFillStyle((int r, int g, int b, double a) style)
        {
            _fillStyle = Colour.FromRgba(style.r, style.g, style.b, style.a);
        }

        public void SetFillStyle(Colour style)
        {
            _fillStyle = style;
        }


        public Colour FillStyle { get { return _fillStyle; } }
        public Colour StrokeStyle { get { return _strokeStyle; } }

        public double LineWidth { get; set; }

        public LineCaps LineCap { get; set; }
        public LineJoins LineJoin { get; set; }

        public void Fill()
        {
            _figures.Add(new PathFigure(_currentFigure, _fillStyle, null, 0, LineCaps.Butt, LineJoins.Bevel, new LineDash(0, 0, 0), false, this.Tag));
            _currentFigure = new List<Segment>();
        }

        public void Stroke()
        {
            _figures.Add(new PathFigure(_currentFigure, null, _strokeStyle, LineWidth, LineCap, LineJoin, _lineDash, false, this.Tag));
            _currentFigure = new List<Segment>();
        }

        public void SetClippingPath()
        {
            _figures.Add(new PathFigure(_currentFigure, null, null, 0, LineCaps.Butt, LineJoins.Bevel, new LineDash(0, 0, 0), true, this.Tag));
            _currentFigure = new List<Segment>();
        }

        public void CubicBezierTo(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            _currentFigure.Add(new CubicBezierSegment(x1, y1, x2, y2, x3, y3));
        }

        public void SetLineDash(LineDash dash)
        {
            _lineDash = dash;
        }

        public Font Font { get; set; }

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

        public void FillText(string text, double x, double y)
        {
            if (!_textToPaths)
            {
                _figures.Add(new TextFigure(text, Font, new Point(x, y), TextBaseline, _fillStyle, null, 0, LineCaps.Butt, LineJoins.Miter, new LineDash(0, 0, 0), this.Tag));
            }
            else
            {
                PathText(text, x, y);
                Fill();
            }
        }

        public TextBaselines TextBaseline { get; set; }

        public void Restore()
        {
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Restore, null, this.Tag));
        }

        public void Rotate(double angle)
        {
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { Math.Cos(angle), Math.Sin(angle), 0 }, { -Math.Sin(angle), Math.Cos(angle), 0 }, { 0, 0, 1 } }, this.Tag));
        }

        public void Save()
        {
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Save, null, this.Tag));
        }


        public void StrokeText(string text, double x, double y)
        {
            if (!_textToPaths)
            {
                _figures.Add(new TextFigure(text, Font, new Point(x, y), TextBaseline, null, _strokeStyle, LineWidth, LineCap, LineJoin, _lineDash, this.Tag));
            }
            else
            {
                PathText(text, x, y);
                Stroke();
            }
        }

        public void Translate(double x, double y)
        {
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { 1, 0, x }, { 0, 1, y }, { 0, 0, 1 } }, this.Tag));
        }

        public void Scale(double scaleX, double scaleY)
        {
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { scaleX, 0, 0 }, { 0, scaleY, 0 }, { 0, 0, 1 } }, this.Tag));
        }

        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { a, b, e }, { c, d, f }, { 0, 0, 1 } }, this.Tag));
        }

        public void DrawRasterImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image)
        {
            Save();

            MoveTo(destinationX, destinationY);
            LineTo(destinationX + destinationWidth, destinationY);
            LineTo(destinationX + destinationWidth, destinationY + destinationHeight);
            LineTo(destinationX, destinationY + destinationHeight);
            Close();
            SetClippingPath();

            double sourceRectX = (double)sourceX / image.Width;
            double sourceRectY = 1 - (double)sourceY / image.Height;
            double sourceRectWidth = (double)sourceWidth / image.Width;
            double sourceRectHeight = -(double)sourceHeight / image.Height;

            double scaleX = destinationWidth / sourceRectWidth;
            double scaleY = destinationHeight / sourceRectHeight;

            double translationX = destinationX / scaleX - sourceRectX;
            double translationY = destinationY / scaleY - sourceRectY;


            Scale(scaleX, scaleY);
            Translate(translationX, translationY);

            _figures.Add(new RasterImageFigure(image, this.Tag));

            Restore();
        }
    }

    /// <summary>
    /// Contains methods to render a <see cref="Document"/> as a PDF document.
    /// </summary>
    public static class PDFContextInterpreter
    {
        private static string EscapeStringForPDF(string str)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];

                if (CP1252Chars.Contains(ch))
                {
                    if ((int)ch < 128)
                    {
                        if (!"\n\r\t\b\f()\\".Contains(ch))
                        {
                            sb.Append(ch);
                        }
                        else
                        {
                            switch (ch)
                            {
                                case '\n':
                                    sb.Append("\\n");
                                    break;
                                case '\r':
                                    sb.Append("\\r");
                                    break;
                                case '\t':
                                    sb.Append("\\t");
                                    break;
                                case '\b':
                                    sb.Append("\\b");
                                    break;
                                case '\f':
                                    sb.Append("\\f");
                                    break;
                                case '\\':
                                    sb.Append("\\\\");
                                    break;
                                case '(':
                                    sb.Append("\\(");
                                    break;
                                case ')':
                                    sb.Append("\\)");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        string octal = Convert.ToString((int)ch, 8);
                        while (octal.Length < 3)
                        {
                            octal = "0" + octal;
                        }
                        sb.Append("\\" + octal);
                    }
                }
                else
                {
                    sb.Append('?');
                }
            }
            return sb.ToString();
        }


        private static string EscapeSymbolStringForPDF(string str, Dictionary<char, int> glyphIndices)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                sb.Append((glyphIndices[str[i]]).ToString("X4"));
            }
            return sb.ToString();
        }

        private static Dictionary<string, FontFamily> GetFontFamilies(PDFContext[] pdfContexts)
        {
            Dictionary<string, FontFamily> tbr = new Dictionary<string, FontFamily>();

            foreach (PDFContext ctx in pdfContexts)
            {
                foreach (IFigure act in ctx._figures)
                {
                    if (act is TextFigure figure && !tbr.ContainsKey(figure.Font.FontFamily.FileName))
                    {
                        tbr.Add(figure.Font.FontFamily.FileName, new FontFamily(figure.Font.FontFamily.FileName));
                    }
                }
            }

            return tbr;
        }

        private static Dictionary<string, HashSet<char>> GetUsedChars(PDFContext[] pdfContexts)
        {
            Dictionary<string, HashSet<char>> tbr = new Dictionary<string, HashSet<char>>();

            foreach (PDFContext ctx in pdfContexts)
            {
                foreach (IFigure act in ctx._figures)
                {
                    if (act is TextFigure figure && !tbr.ContainsKey(figure.Font.FontFamily.FileName))
                    {
                        tbr.Add(figure.Font.FontFamily.FileName, new HashSet<char>(figure.Text));
                    }
                    else if (act is TextFigure figure1)
                    {
                        string txt = figure1.Text;
                        for (int i = 0; i < txt.Length; i++)
                        {
                            tbr[figure1.Font.FontFamily.FileName].Add(txt[i]);
                        }
                    }
                }
            }

            return tbr;
        }


        private static double[] GetAlphas(PDFContext[] pdfContexts)
        {
            HashSet<double> tbr = new HashSet<double>();

            tbr.Add(1);

            foreach (PDFContext ctx in pdfContexts)
            {
                foreach (IFigure act in ctx._figures)
                {
                    if (act.Stroke.HasValue)
                    {
                        tbr.Add((double)act.Stroke?.A);
                    }

                    if (act.Fill.HasValue)
                    {
                        tbr.Add((double)act.Fill?.A);
                    }
                }
            }

            return tbr.ToArray();
        }

        private static Dictionary<string, RasterImage> GetAllImages(PDFContext[] pdfContexts)
        {
            Dictionary<string, RasterImage> tbr = new Dictionary<string, RasterImage>();

            foreach (PDFContext ctx in pdfContexts)
            {
                foreach (IFigure act in ctx._figures)
                {
                    if (act is RasterImageFigure figure && !tbr.ContainsKey(figure.Image.Id))
                    {
                        tbr.Add(figure.Image.Id, figure.Image);
                    }
                }
            }

            return tbr;
        }


        private static readonly char[] CP1252Chars = new char[] { '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', '\u0008', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F', '\u0020', '\u0021', '\u0022', '\u0023', '\u0024', '\u0025', '\u0026', '\u0027', '\u0028', '\u0029', '\u002A', '\u002B', '\u002C', '\u002D', '\u002E', '\u002F', '\u0030', '\u0031', '\u0032', '\u0033', '\u0034', '\u0035', '\u0036', '\u0037', '\u0038', '\u0039', '\u003A', '\u003B', '\u003C', '\u003D', '\u003E', '\u003F', '\u0040', '\u0041', '\u0042', '\u0043', '\u0044', '\u0045', '\u0046', '\u0047', '\u0048', '\u0049', '\u004A', '\u004B', '\u004C', '\u004D', '\u004E', '\u004F', '\u0050', '\u0051', '\u0052', '\u0053', '\u0054', '\u0055', '\u0056', '\u0057', '\u0058', '\u0059', '\u005A', '\u005B', '\u005C', '\u005D', '\u005E', '\u005F', '\u0060', '\u0061', '\u0062', '\u0063', '\u0064', '\u0065', '\u0066', '\u0067', '\u0068', '\u0069', '\u006A', '\u006B', '\u006C', '\u006D', '\u006E', '\u006F', '\u0070', '\u0071', '\u0072', '\u0073', '\u0074', '\u0075', '\u0076', '\u0077', '\u0078', '\u0079', '\u007A', '\u007B', '\u007C', '\u007D', '\u007E', '\u007F', '\u20AC', '\u25A1', '\u201A', '\u0192', '\u201E', '\u0000', '\u2020', '\u2021', '\u02C6', '\u2030', '\u0160', '\u2039', '\u0152', '\u25A1', '\u017D', '\u25A1', '\u25A1', '\u0000', '\u0000', '\u0000', '\u0000', '\u2022', '\u0000', '\u0000', '\u02DC', '\u2122', '\u0161', '\u203A', '\u0153', '\u25A1', '\u017E', '\u0178', '\u00A0', '\u00A1', '\u00A2', '\u00A3', '\u00A4', '\u00A5', '\u00A6', '\u00A7', '\u00A8', '\u00A9', '\u00AA', '\u00AB', '\u00AC', '\u00AD', '\u00AE', '\u00AF', '\u0000', '\u00B1', '\u00B2', '\u00B3', '\u00B4', '\u00B5', '\u00B6', '\u00B7', '\u00B8', '\u00B9', '\u00BA', '\u00BB', '\u00BC', '\u00BD', '\u00BE', '\u00BF', '\u00C0', '\u00C1', '\u00C2', '\u00C3', '\u00C4', '\u00C5', '\u00C6', '\u00C7', '\u00C8', '\u00C9', '\u00CA', '\u00CB', '\u00CC', '\u00CD', '\u00CE', '\u00CF', '\u00D0', '\u00D1', '\u00D2', '\u00D3', '\u00D4', '\u00D5', '\u00D6', '\u00D7', '\u00D8', '\u00D9', '\u00DA', '\u00DB', '\u00DC', '\u00DD', '\u00DE', '\u00DF', '\u00E0', '\u00E1', '\u00E2', '\u00E3', '\u00E4', '\u00E5', '\u00E6', '\u00E7', '\u00E8', '\u00E9', '\u00EA', '\u00EB', '\u00EC', '\u00ED', '\u00EE', '\u00EF', '\u00F0', '\u00F1', '\u00F2', '\u00F3', '\u00F4', '\u00F5', '\u00F6', '\u00F7', '\u00F8', '\u00F9', '\u00FA', '\u00FB', '\u00FC', '\u00FD', '\u00FE', '\u00FF' };

        /// <summary>
        /// Save the document to a PDF file.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to save.</param>
        /// <param name="fileName">The full path to the file to save. If it exists, it will be overwritten.</param>
        /// <param name="textOption">Defines whether the used fonts should be included in the file.</param>
        /// <param name="compressStreams">Indicates whether the streams in the PDF file should be compressed.</param>
        /// <param name="linkDestinations">A dictionary associating element tags to link targets. If this is provided, objects that have been drawn with a tag contained in the dictionary will become hyperlink to the destination specified in the dictionary. If the destination starts with a hash (#), it is interpreted as the tag of another object in the current document; otherwise, it is interpreted as an external URI.</param>
        public static void SaveAsPDF(this Document document, string fileName, TextOptions textOption = TextOptions.SubsetFonts, bool compressStreams = true, Dictionary<string, string> linkDestinations = null)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                document.SaveAsPDF(stream, textOption, compressStreams, linkDestinations);
            }
        }

        /// <summary>
        /// Defines whether the used fonts should be included in the file.
        /// </summary>
        public enum TextOptions
        {
            /// <summary>
            /// Embeds subsetted font files containing only the glyphs for the characters that have been used.
            /// </summary>
            SubsetFonts,

            /// <summary>
            /// Does not embed any font file and converts all text items into paths.
            /// </summary>
            ConvertIntoPaths
        }



        /// <summary>
        /// Save the document to a PDF stream.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to save.</param>
        /// <param name="stream">The stream to which the PDF data will be written.</param>
        /// <param name="textOption">Defines whether the used fonts should be included in the file.</param>
        /// <param name="compressStreams">Indicates whether the streams in the PDF file should be compressed.</param>
        /// <param name="linkDestinations">A dictionary associating element tags to link targets. If this is provided, objects that have been drawn with a tag contained in the dictionary will become hyperlink to the destination specified in the dictionary. If the destination starts with a hash (#), it is interpreted as the tag of another object in the current document; otherwise, it is interpreted as an external URI.</param>
        public static void SaveAsPDF(this Document document, Stream stream, TextOptions textOption = TextOptions.SubsetFonts, bool compressStreams = true, Dictionary<string, string> linkDestinations = null)
        {
            if (linkDestinations == null)
            {
                linkDestinations = new Dictionary<string, string>();
            }

            long position = 0;

            List<long> objectPositions = new List<long>();

            int objectNum = 1;
            string currObject = "";

            int resourceObject = -1;

            StreamWriter sw = new StreamWriter(stream, Encoding.GetEncoding("ISO-8859-1"), 1024, true);

            //Header
            sw.Write("%PDF-1.4\n");
            position += 9;

            PDFContext[] pageContexts = new PDFContext[document.Pages.Count];

            for (int i = 0; i < document.Pages.Count; i++)
            {
                pageContexts[i] = new PDFContext(document.Pages[i].Width, document.Pages[i].Height, document.Pages[i].Background, textOption == TextOptions.ConvertIntoPaths);
                document.Pages[i].Graphics.CopyToIGraphicsContext(pageContexts[i]);
            }

            Dictionary<string, FontFamily> allFontFamilies = GetFontFamilies(pageContexts);
            Dictionary<string, HashSet<char>> usedChars = GetUsedChars(pageContexts);
            Dictionary<string, int> fontObjectNums = new Dictionary<string, int>();
            Dictionary<string, string> symbolFontIDs = new Dictionary<string, string>();
            Dictionary<string, string> nonSymbolFontIDs = new Dictionary<string, string>();
            Dictionary<string, Dictionary<char, int>> symbolGlyphIndices = new Dictionary<string, Dictionary<char, int>>();
            double[] alphas = GetAlphas(pageContexts);
            Dictionary<string, RasterImage> allImages = GetAllImages(pageContexts);
            Dictionary<string, int> imageObjectNums = new Dictionary<string, int>();

            int fontId = 1;

            foreach (KeyValuePair<string, FontFamily> kvp in allFontFamilies)
            {
                List<char> nonSymbol = new List<char>();
                List<char> symbol = new List<char>();

                foreach (char c in usedChars[kvp.Key])
                {
                    if (CP1252Chars.Contains(c))
                    {
                        nonSymbol.Add(c);
                    }
                    else
                    {
                        symbol.Add(c);
                    }
                }

                //Font
                if (((kvp.Value.IsStandardFamily && kvp.Value.FileName != "Symbol" && kvp.Value.FileName != "ZapfDingbats") && symbol.Count == 0) || kvp.Value.TrueTypeFile == null)
                {
                    fontObjectNums.Add("nonsymbol: " + kvp.Key, objectNum);
                    fontObjectNums.Add("symbol: " + kvp.Key, objectNum);
                    nonSymbolFontIDs.Add(kvp.Key, "F" + fontId.ToString());
                    symbolFontIDs.Add(kvp.Key, "F" + fontId.ToString());
                    objectPositions.Add(position);
                    currObject = objectNum.ToString() + " 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /" + kvp.Key + " >>\nendobj\n";
                    sw.Write(currObject);
                    position += currObject.Length;
                    objectNum++;
                    fontId++;
                }
                else
                {
                    int fontFileInd = objectNum;

                    TrueTypeFile subsettedFont = kvp.Value.TrueTypeFile.SubsetFont(new string(usedChars[kvp.Key].ToArray()));

                    Stream compressedStream;

                    if (!compressStreams)
                    {
                        compressedStream = subsettedFont.FontStream;
                    }
                    else
                    {
                        compressedStream = ZLibCompress(subsettedFont.FontStream);
                    }

                    long length = compressedStream.Length;

                    objectPositions.Add(position);
                    currObject = objectNum.ToString() + " 0 obj\n<< /Length " + length.ToString() + " /Length1 " + subsettedFont.FontStream.Length.ToString();

                    if (compressStreams)
                    {
                        currObject += " /Filter [ /FlateDecode ]";
                    }

                    currObject += " >>\nstream\n";
                    sw.Write(currObject);
                    position += currObject.Length;
                    sw.Flush();

                    compressedStream.Seek(0, SeekOrigin.Begin);
                    compressedStream.CopyTo(stream);

                    position += length;
                    currObject = "endstream\nendobj\n";
                    sw.Write(currObject);
                    position += currObject.Length;
                    objectNum++;


                    if (nonSymbol.Count > 0)
                    {
                        PDFFontDescriptor desc = new PDFFontDescriptor(subsettedFont, true, false);

                        int fontDescriptorInd = objectNum;
                        objectPositions.Add(position);

                        currObject = objectNum.ToString() + " 0 obj\n<< /Type /FontDescriptor /FontName /" + desc.FontName + " /FontFamily (" + EscapeStringForPDF(desc.FontFamily) + ") /Flags " + desc.Flags.ToString();
                        currObject += " /FontBBox [ " + desc.FontBBox[0].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + desc.FontBBox[1].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + desc.FontBBox[2].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + desc.FontBBox[3].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ] /ItalicAngle " + desc.ItalicAngle.ToString();
                        currObject += " /Ascent " + desc.Ascent.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " /Descent " + desc.Descent.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " /CapHeight " + desc.CapHeight.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " /StemV " + desc.StemV.ToString() + " /StemH " + desc.StemH.ToString() + " /FontFile2 " + fontFileInd.ToString() + " 0 R >>\nendobj\n";
                        sw.Write(currObject);
                        position += currObject.Length;
                        objectNum++;


                        fontObjectNums.Add("nonsymbol: " + kvp.Key, objectNum);
                        nonSymbolFontIDs.Add(kvp.Key, "F" + fontId.ToString());
                        objectPositions.Add(position);

                        int firstChar = (from el in nonSymbol select Array.IndexOf(CP1252Chars, el)).Min();
                        int lastChar = (from el in nonSymbol select Array.IndexOf(CP1252Chars, el)).Max();

                        currObject = objectNum.ToString() + " 0 obj\n<< /Type /Font /Subtype /TrueType /BaseFont /" + desc.FontName + " /FirstChar " + firstChar.ToString() + " /LastChar " + lastChar.ToString() + " /FontDescriptor " + fontDescriptorInd.ToString() + " 0 R /Encoding /WinAnsiEncoding /Widths [ ";

                        for (int i = firstChar; i <= lastChar; i++)
                        {
                            if (nonSymbol.Contains(CP1252Chars[i]))
                            {
                                currObject += subsettedFont.Get1000EmGlyphWidth(CP1252Chars[i]).ToString() + " ";
                            }
                            else
                            {
                                currObject += "0 ";
                            }
                        }

                        currObject += "] >>\nendobj\n";
                        sw.Write(currObject);
                        position += currObject.Length;
                        objectNum++;
                        fontId++;
                    }


                    if (symbol.Count > 0)
                    {
                        PDFFontDescriptor desc = new PDFFontDescriptor(subsettedFont, true, true);

                        int fontDescriptorInd = objectNum;
                        objectPositions.Add(position);

                        currObject = objectNum.ToString() + " 0 obj\n<< /Type /FontDescriptor /FontName /" + desc.FontName + " /FontFamily (" + EscapeStringForPDF(desc.FontFamily) + ") /Flags " + desc.Flags.ToString();
                        currObject += " /FontBBox [ " + desc.FontBBox[0].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + desc.FontBBox[1].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + desc.FontBBox[2].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + desc.FontBBox[3].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ] /ItalicAngle " + desc.ItalicAngle.ToString();
                        currObject += " /Ascent " + desc.Ascent.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " /Descent " + desc.Descent.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " /CapHeight " + desc.CapHeight.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " /StemV " + desc.StemV.ToString() + " /StemH " + desc.StemH.ToString() + " /FontFile2 " + fontFileInd.ToString() + " 0 R >>\nendobj\n";
                        sw.Write(currObject);
                        position += currObject.Length;
                        objectNum++;


                        Dictionary<char, int> glyphIndices = new Dictionary<char, int>();

                        for (int i = 0; i < symbol.Count; i++)
                        {
                            glyphIndices.Add(symbol[i], subsettedFont.GetGlyphIndex(symbol[i]));
                        }

                        symbolGlyphIndices.Add(kvp.Key, glyphIndices);

                        int descendantFontInd = objectNum;
                        objectPositions.Add(position);
                        currObject = objectNum.ToString() + " 0 obj\n<< /Type /Font /Subtype /CIDFontType2 /BaseFont /" + desc.FontName + " /CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> /FontDescriptor " + fontDescriptorInd.ToString() + " 0 R ";
                        currObject += "/W [ ";

                        for (int i = 0; i < symbol.Count; i++)
                        {
                            currObject += glyphIndices[symbol[i]].ToString() + " [ ";
                            currObject += subsettedFont.Get1000EmGlyphWidth(symbol[i]).ToString() + " ] ";
                        }

                        currObject += "] >>\nendobj\n";
                        sw.Write(currObject);
                        position += currObject.Length;
                        objectNum++;


                        string toUnicodeStream = "/CIDInit /ProcSet findresource begin\n12 dict begin\nbegincmap\n/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def\n";
                        toUnicodeStream += "/CMapName /Adobe-Identity-UCS def\n/CMapType 2 def\n1 begincodespacerange\n<0000> <ffff>\nendcodespacerange\n1 beginbfchar\n";
                        for (int i = 0; i < symbol.Count; i++)
                        {
                            toUnicodeStream += "<" + glyphIndices[symbol[i]].ToString("X4") + "> <" + ((int)symbol[i]).ToString("X4") + ">\n";
                        }
                        toUnicodeStream += "endbfchar\nendcmap\nCmapName currentdict /CMap defineresource pop\nend\nend\n";

                        MemoryStream uncompressedUnicode = new MemoryStream();

                        using (StreamWriter usw = new StreamWriter(uncompressedUnicode, Encoding.ASCII, 1024, true))
                        {
                            usw.Write(toUnicodeStream);
                        }

                        uncompressedUnicode.Seek(0, SeekOrigin.Begin);

                        MemoryStream compressedToUnicode;

                        if (!compressStreams)
                        {
                            compressedToUnicode = uncompressedUnicode;
                        }
                        else
                        {
                            compressedToUnicode = ZLibCompress(uncompressedUnicode);
                        }

                        long unicodeLength = compressedToUnicode.Length;

                        int toUnicodeInd = objectNum;
                        objectPositions.Add(position);
                        currObject = objectNum.ToString() + " 0 obj\n<< /Length " + unicodeLength;

                        if (compressStreams)
                        {
                            currObject += " /Filter [ /FlateDecode ]";
                        }

                        currObject += " >>\nstream\n";

                        sw.Write(currObject);
                        position += currObject.Length;
                        sw.Flush();

                        compressedToUnicode.WriteTo(stream);
                        position += unicodeLength;

                        currObject = "endstream\nendobj\n";
                        sw.Write(currObject);
                        position += currObject.Length;
                        objectNum++;


                        fontObjectNums.Add("symbol: " + kvp.Key, objectNum);
                        symbolFontIDs.Add(kvp.Key, "F" + fontId.ToString());
                        objectPositions.Add(position);
                        currObject = objectNum.ToString() + " 0 obj\n<< /Type /Font /Subtype /Type0 /BaseFont /" + desc.FontName + " /Encoding /Identity-H /DescendantFonts [ " + descendantFontInd.ToString() + " 0 R ] /ToUnicode " + toUnicodeInd.ToString() + " 0 R >>\nendobj\n";
                        sw.Write(currObject);
                        position += currObject.Length;
                        objectNum++;
                        fontId++;
                    }
                }
            }

            foreach (KeyValuePair<string, RasterImage> img in allImages)
            {
                RasterImage image = img.Value;
                int stride = image.Width * (image.HasAlpha ? 4 : 3);

                string filter = "";

                if (image.HasAlpha)
                {
                    objectPositions.Add(position);

                    filter = "";
                    MemoryStream alphaStream = new MemoryStream();

                    unsafe
                    {
                        byte* dataPointer = (byte*)image.ImageDataAddress;

                        if (compressStreams)
                        {
                            filter = "/FlateDecode";

                            for (int y = 0; y < image.Height; y++)
                            {
                                for (int x = 0; x < image.Width; x++)
                                {
                                    dataPointer += 3;
                                    alphaStream.WriteByte(*dataPointer);
                                    dataPointer++;
                                }
                            }

                            alphaStream.Seek(0, SeekOrigin.Begin);
                            MemoryStream compressed = ZLibCompress(alphaStream);
                            alphaStream.Dispose();
                            alphaStream = compressed;
                        }
                        else
                        {
                            filter = "/ASCIIHexDecode";

                            using (StreamWriter imageWriter = new StreamWriter(alphaStream, Encoding.ASCII, 1024, true))
                            {
                                for (int y = 0; y < image.Height; y++)
                                {
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        dataPointer += 3;
                                        imageWriter.Write((*dataPointer).ToString("X2"));
                                        dataPointer++;
                                    }
                                }
                            }
                        }
                    }

                    currObject = objectNum.ToString() + " 0 obj\n<< /Type /XObject /Subtype /Image /Width " + image.Width.ToString() + " /Height " + image.Height.ToString() + " /ColorSpace /DeviceGray /BitsPerComponent 8 /Interpolate " + (image.Interpolate ? "true" : "false") + " /Filter " + filter + " /Length " + alphaStream.Length + " >>\nstream\n";

                    sw.Write(currObject);
                    position += currObject.Length;
                    sw.Flush();

                    alphaStream.Seek(0, SeekOrigin.Begin);
                    alphaStream.CopyTo(stream);
                    position += alphaStream.Length;

                    currObject = "\nendstream\nendobj\n";
                    sw.Write(currObject);
                    position += currObject.Length;
                    objectNum++;

                    alphaStream.Dispose();

                }

                objectPositions.Add(position);
                int imageObjectNum = objectNum;

                filter = "";
                MemoryStream imageStream = new MemoryStream();

                unsafe
                {
                    byte* dataPointer = (byte*)image.ImageDataAddress;

                    if (compressStreams)
                    {
                        filter = "/FlateDecode";

                        if (image.HasAlpha)
                        {
                            for (int y = 0; y < image.Height; y++)
                            {
                                for (int x = 0; x < image.Width; x++)
                                {
                                    imageStream.WriteByte(*dataPointer);
                                    dataPointer++;
                                    imageStream.WriteByte(*dataPointer);
                                    dataPointer++;
                                    imageStream.WriteByte(*dataPointer);
                                    dataPointer++;
                                    dataPointer++;
                                }
                            }
                        }
                        else
                        {
                            for (int y = 0; y < image.Height; y++)
                            {
                                for (int x = 0; x < image.Width; x++)
                                {
                                    imageStream.WriteByte(*dataPointer);
                                    dataPointer++;
                                    imageStream.WriteByte(*dataPointer);
                                    dataPointer++;
                                    imageStream.WriteByte(*dataPointer);
                                    dataPointer++;
                                }
                            }
                        }

                        imageStream.Seek(0, SeekOrigin.Begin);
                        MemoryStream compressed = ZLibCompress(imageStream);
                        imageStream.Dispose();
                        imageStream = compressed;
                    }
                    else
                    {
                        filter = "/ASCIIHexDecode";

                        using (StreamWriter imageWriter = new StreamWriter(imageStream, Encoding.ASCII, 1024, true))
                        {
                            if (image.HasAlpha)
                            {
                                for (int y = 0; y < image.Height; y++)
                                {
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        imageWriter.Write((*dataPointer).ToString("X2"));
                                        dataPointer++;
                                        imageWriter.Write((*dataPointer).ToString("X2"));
                                        dataPointer++;
                                        imageWriter.Write((*dataPointer).ToString("X2"));
                                        dataPointer++;
                                        dataPointer++;
                                    }
                                }
                            }
                            else
                            {
                                for (int y = 0; y < image.Height; y++)
                                {
                                    for (int x = 0; x < image.Width; x++)
                                    {
                                        imageWriter.Write((*dataPointer).ToString("X2"));
                                        dataPointer++;
                                        imageWriter.Write((*dataPointer).ToString("X2"));
                                        dataPointer++;
                                        imageWriter.Write((*dataPointer).ToString("X2"));
                                        dataPointer++;
                                    }
                                }
                            }
                        }
                    }
                }

                currObject = objectNum.ToString() + " 0 obj\n<< /Type /XObject /Subtype /Image /Width " + image.Width.ToString() + " /Height " + image.Height.ToString() + " /ColorSpace /DeviceRGB /BitsPerComponent 8 /Interpolate " + (image.Interpolate ? "true" : "false") + " /Filter " + filter + " /Length " + imageStream.Length + (image.HasAlpha ? " /SMask " + (objectNum - 1) + " 0 R" : "") + " >>\nstream\n";

                sw.Write(currObject);
                position += currObject.Length;
                sw.Flush();

                imageStream.Seek(0, SeekOrigin.Begin);
                imageStream.CopyTo(stream);
                position += imageStream.Length;

                currObject = "\nendstream\nendobj\n";
                sw.Write(currObject);
                position += currObject.Length;
                objectNum++;

                imageStream.Dispose();

                imageObjectNums.Add(img.Key, imageObjectNum);
            }

            if (allFontFamilies.Count > 0)
            {

                //Fonts
                objectPositions.Add(position);
                int fontListObject = objectNum;
                currObject = objectNum.ToString() + " 0 obj\n<< ";
                foreach (KeyValuePair<string, string> kvp in nonSymbolFontIDs)
                {
                    currObject += "/" + kvp.Value + " " + fontObjectNums["nonsymbol: " + kvp.Key].ToString() + " 0 R ";
                }
                foreach (KeyValuePair<string, string> kvp in symbolFontIDs)
                {
                    currObject += "/" + kvp.Value + " " + fontObjectNums["symbol: " + kvp.Key].ToString() + " 0 R ";
                }
                currObject += ">>\nendobj\n";
                sw.Write(currObject);
                position += currObject.Length;
                objectNum++;


                //Resources
                objectPositions.Add(position);
                resourceObject = objectNum;
                currObject = objectNum.ToString() + " 0 obj\n<< /Font " + fontListObject.ToString() + " 0 R";

                if (alphas.Length > 0)
                {
                    currObject += " /ExtGState <<\n";

                    for (int i = 0; i < alphas.Length; i++)
                    {
                        currObject += "/a" + i.ToString() + " << /CA " + alphas[i].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " /ca " + alphas[i].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " >>\n";
                    }

                    currObject += ">>";
                }

                if (imageObjectNums.Count > 0)
                {
                    currObject += " /XObject <<";

                    foreach (KeyValuePair<string, int> kvp in imageObjectNums)
                    {
                        currObject += " /Img" + kvp.Value.ToString() + " " + kvp.Value.ToString() + " 0 R";
                    }

                    currObject += " >>";
                }

                currObject += " >>\nendobj\n";
                sw.Write(currObject);
                position += currObject.Length;
                objectNum++;
            }
            else
            {
                //Resources
                objectPositions.Add(position);
                resourceObject = objectNum;
                currObject = objectNum.ToString() + " 0 obj\n<<";

                if (alphas.Length > 0)
                {
                    currObject += " /ExtGState <<\n";

                    for (int i = 0; i < alphas.Length; i++)
                    {
                        currObject += "/a" + i.ToString() + " << /CA " + alphas[i].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " /ca " + alphas[i].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " >>\n";
                    }

                    currObject += ">>";
                }

                if (imageObjectNums.Count > 0)
                {
                    currObject += " /XObject <<";

                    foreach (KeyValuePair<string, int> kvp in imageObjectNums)
                    {
                        currObject += " /Img" + kvp.Value.ToString() + " " + kvp.Value.ToString() + " 0 R";
                    }

                    currObject += " >>";
                }

                currObject += " >>\nendobj\n";
                sw.Write(currObject);
                position += currObject.Length;
                objectNum++;
            }

            int[] pageContentInd = new int[document.Pages.Count];


            List<(string, List<(double, double, double, double)>)>[] taggedObjectRectsByPage = new List<(string, List<(double, double, double, double)>)>[document.Pages.Count];
            Dictionary<string, int>[] taggedObjectRectsIndicesByPage = new Dictionary<string, int>[document.Pages.Count];

            for (int pageInd = 0; pageInd < document.Pages.Count; pageInd++)
            {
                taggedObjectRectsByPage[pageInd] = new List<(string, List<(double, double, double, double)>)>();
                taggedObjectRectsIndicesByPage[pageInd] = new Dictionary<string, int>();

                double[,] transformationMatrix = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
                Stack<double[,]> savedStates = new Stack<double[,]>();

                MemoryStream contentStream = new MemoryStream();

                using (StreamWriter ctW = new StreamWriter(contentStream, Encoding.ASCII, 1024, true))
                {
                    for (int i = 0; i < pageContexts[pageInd]._figures.Count; i++)
                    {
                        bool isTransform = pageContexts[pageInd]._figures[i] is TransformFigure;

                        bool isClip = pageContexts[pageInd]._figures[i] is PathFigure pathFig && pathFig.IsClipping;

                        if (!string.IsNullOrEmpty(pageContexts[pageInd]._figures[i].Tag) && !isTransform && !isClip)
                        {
                            (double, double, double, double) boundingRect = MeasureFigure(pageContexts[pageInd]._figures[i], ref transformationMatrix, savedStates);

                            if (!taggedObjectRectsIndicesByPage[pageInd].TryGetValue(pageContexts[pageInd]._figures[i].Tag, out int index))
                            {
                                taggedObjectRectsByPage[pageInd].Add((pageContexts[pageInd]._figures[i].Tag, new List<(double, double, double, double)> { boundingRect }));
                                taggedObjectRectsIndicesByPage[pageInd][pageContexts[pageInd]._figures[i].Tag] = taggedObjectRectsByPage[pageInd].Count - 1;
                            }
                            else
                            {
                                (string, List<(double, double, double, double)>) previousRect = taggedObjectRectsByPage[pageInd][index];
                                taggedObjectRectsByPage[pageInd][index].Item2.Add(boundingRect);
                            }
                        }
                        else if (isTransform)
                        {
                            MeasureFigure(pageContexts[pageInd]._figures[i], ref transformationMatrix, savedStates);
                        }

                        ctW.Write(FigureAsPDFString(pageContexts[pageInd]._figures[i], nonSymbolFontIDs, symbolFontIDs, symbolGlyphIndices, alphas, imageObjectNums));
                    }
                }

                //Contents
                objectPositions.Add(position);
                contentStream.Seek(0, SeekOrigin.Begin);

                MemoryStream compressedStream;

                if (!compressStreams)
                {
                    compressedStream = contentStream;
                }
                else
                {
                    compressedStream = ZLibCompress(contentStream);
                }

                long streamLength = compressedStream.Length;

                pageContentInd[pageInd] = objectNum;
                currObject = objectNum.ToString() + " 0 obj\n<< /Length " + streamLength.ToString(System.Globalization.CultureInfo.InvariantCulture);

                if (compressStreams)
                {
                    currObject += " /Filter [ /FlateDecode ]";
                }

                currObject += " >>\nstream\n";

                sw.Write(currObject);
                sw.Flush();

                position += currObject.Length;
                compressedStream.WriteTo(stream);
                position += streamLength;

                compressedStream.Dispose();

                currObject = "endstream\nendobj\n";
                sw.Write(currObject);
                position += currObject.Length;

                objectNum++;
            }

            //Catalog
            objectPositions.Add(position);
            int rootObject = objectNum;
            currObject = objectNum.ToString() + " 0 obj\n<< /Type /Catalog /Pages " + (objectNum + 1).ToString() + " 0 R >>\nendobj\n";
            sw.Write(currObject);
            position += currObject.Length;
            objectNum++;

            objectPositions.Add(position);
            int pageParent = objectNum;
            objectNum++;

            List<int> pageObjectNums = new List<int>();

            //We do not have enough information to resolve all relative links yet (we need the object number for all the pages).
            List<(int annotationObjectNum, (double, double, double, double) annotationOrigin, int annotationDestinationPage, (double, double, double, double) annotationDestination)> postponedAnnotations = new List<(int, (double, double, double, double), int, (double, double, double, double))>();

            //Page
            for (int i = 0; i < document.Pages.Count; i++)
            {
                List<int> annotationsToInclude = new List<int>();

                //Annotations
                for (int j = 0; j < taggedObjectRectsByPage[i].Count; j++)
                {
                    if (linkDestinations.TryGetValue(taggedObjectRectsByPage[i][j].Item1, out string destination))
                    {
                        if (destination.StartsWith("#"))
                        {
                            //Leave these for later, once we have computed the object number for all the pages. But we need to include the annotation number in the page, so we start processing the annotation now.
                            for (int k = 0; k < taggedObjectRectsIndicesByPage.Length; k++)
                            {
                                if (taggedObjectRectsIndicesByPage[k].TryGetValue(destination.Substring(1), out int index))
                                {
                                    for (int l = 0; l < taggedObjectRectsByPage[i][j].Item2.Count; l++)
                                    {
                                        objectPositions.Add(position);
                                        annotationsToInclude.Add(objectNum);
                                        postponedAnnotations.Add((objectNum, taggedObjectRectsByPage[i][j].Item2[l], k, taggedObjectRectsByPage[k][index].Item2[0]));
                                        objectNum++;
                                    }

                                    //Only consider the first match for the local destination.
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int l = 0; l < taggedObjectRectsByPage[i][j].Item2.Count; l++)
                            {
                                objectPositions.Add(position);
                                currObject = objectNum.ToString() + " 0 obj\n<< /Type /Annot /Subtype /Link /Rect [" + taggedObjectRectsByPage[i][j].Item2[l].Item1.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + taggedObjectRectsByPage[i][j].Item2[l].Item2.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + taggedObjectRectsByPage[i][j].Item2[l].Item3.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + taggedObjectRectsByPage[i][j].Item2[l].Item4.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + "] " +
                                    "/A << /Type /Action /S /URI /URI (" + destination + ") >> >>\nendobj\n";
                                sw.Write(currObject);
                                annotationsToInclude.Add(objectNum);
                                objectNum++;
                                position += currObject.Length;
                            }
                        }
                    }
                }

                //Page
                objectPositions.Add(position);
                currObject = objectNum.ToString() + " 0 obj\n<< /Type /Page /Parent " + pageParent.ToString() + " 0 R /MediaBox [0 0 " + document.Pages[i].Width.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + document.Pages[i].Height.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + "] /Resources " + resourceObject.ToString() + " 0 R /Contents " + pageContentInd[i].ToString() + " 0 R ";

                if (annotationsToInclude.Count > 0)
                {
                    StringBuilder annotations = new StringBuilder();
                    annotations.Append("/Annots [ ");
                    for (int j = 0; j < annotationsToInclude.Count; j++)
                    {
                        annotations.Append(annotationsToInclude[j].ToString() + " 0 R ");
                    }
                    annotations.Append("] ");
                    currObject += annotations.ToString();
                }

                currObject += ">>\nendobj\n";

                sw.Write(currObject);
                pageObjectNums.Add(objectNum);
                objectNum++;
                position += currObject.Length;
            }

            //Now we have enough information for the postponed annotations.
            foreach ((int annotationObjectNum, (double, double, double, double) annotationOrigin, int annotationDestinationPage, (double, double, double, double) annotationDestination) in postponedAnnotations)
            {
                objectPositions[annotationObjectNum - 1] = position;
                currObject = annotationObjectNum.ToString() + " 0 obj\n<< /Type /Annot /Subtype /Link /Rect [" + annotationOrigin.Item1.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + annotationOrigin.Item2.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + annotationOrigin.Item3.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + annotationOrigin.Item4.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + "] " +
                    "/Dest [ " + pageObjectNums[annotationDestinationPage].ToString() + " 0 R /XYZ " + annotationDestination.Item1.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + annotationDestination.Item4.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " 0 ] >>\nendobj\n";
                sw.Write(currObject);
                position += currObject.Length;
            }

            //Pages
            objectPositions[pageParent - 1] = position;
            currObject = pageParent.ToString() + " 0 obj\n<< /Type /Pages /Kids [ ";
            for (int i = 0; i < document.Pages.Count; i++)
            {
                //currObject += (pageParent + i + 1).ToString() + " 0 R ";
                currObject += pageObjectNums[i].ToString() + " 0 R ";
            }
            currObject += "] /Count " + document.Pages.Count + " >>\nendobj\n\n";
            sw.Write(currObject);
            position += currObject.Length;

            //XRef
            sw.Write("xref\n0 " + (objectPositions.Count + 1).ToString() + "\n0000000000 65535 f \n");
            for (int i = 0; i < objectPositions.Count; i++)
            {
                sw.Write(objectPositions[i].ToString("0000000000", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
            }

            //Trailer
            sw.Write("trailer\n<< /Size " + (objectPositions.Count + 1).ToString() + " /Root " + rootObject.ToString() + " 0 R >>\nstartxref\n" + position.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + "\n%%EOF\n");

            sw.Flush();
            sw.Dispose();
        }

        private static (double, double, double, double) MeasureFigure(IFigure figure, ref double[,] transformationMatrix, Stack<double[,]> savedStates)
        {
            if (figure is PathFigure)
            {
                PathFigure fig = figure as PathFigure;

                double minX = double.MaxValue;
                double maxX = double.MinValue;
                double minY = double.MaxValue;
                double maxY = double.MinValue;

                for (int i = 0; i < fig.Segments.Length; i++)
                {
                    switch (fig.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            {
                                Point pt = transformationMatrix.Multiply(fig.Segments[i].Point);
                                minX = Math.Min(minX, pt.X);
                                minY = Math.Min(minY, pt.Y);
                                maxX = Math.Max(maxX, pt.X);
                                maxY = Math.Max(maxY, pt.Y);
                            }
                            break;
                        case SegmentType.Line:
                            {
                                Point pt = transformationMatrix.Multiply(fig.Segments[i].Point);
                                minX = Math.Min(minX, pt.X);
                                minY = Math.Min(minY, pt.Y);
                                maxX = Math.Max(maxX, pt.X);
                                maxY = Math.Max(maxY, pt.Y);
                            }
                            break;
                        case SegmentType.CubicBezier:
                            for (int j = 0; j < fig.Segments[i].Points.Length; j++)
                            {
                                Point pt = transformationMatrix.Multiply(fig.Segments[i].Points[j]);
                                minX = Math.Min(minX, pt.X);
                                minY = Math.Min(minY, pt.Y);
                                maxX = Math.Max(maxX, pt.X);
                                maxY = Math.Max(maxY, pt.Y);
                            }
                            break;
                        case SegmentType.Close:
                            break;
                    }
                }

                return (minX, minY, maxX, maxY);
            }
            else if (figure is TextFigure)
            {
                TextFigure fig = figure as TextFigure;

                double realX = fig.Position.X;

                if (fig.Font.FontFamily.TrueTypeFile != null)
                {
                    realX = fig.Position.X - fig.Font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(fig.Text[0]).LeftSideBearing * fig.Font.FontSize / 1000;
                }

                double yMax = 0;
                double yMin = 0;

                if (fig.Font.FontFamily.TrueTypeFile != null)
                {
                    for (int i = 0; i < fig.Text.Length; i++)
                    {
                        TrueTypeFile.VerticalMetrics vMet = fig.Font.FontFamily.TrueTypeFile.Get1000EmGlyphVerticalMetrics(fig.Text[i]);
                        yMin = Math.Min(yMin, vMet.YMin * fig.Font.FontSize / 1000);
                        yMax = Math.Max(yMax, vMet.YMax * fig.Font.FontSize / 1000);
                    }
                }

                double realY = fig.Position.Y;

                if (fig.TextBaseline == TextBaselines.Bottom)
                {
                    realY -= yMax;
                }
                else if (fig.TextBaseline == TextBaselines.Top)
                {
                    realY -= yMin;
                }
                else if (fig.TextBaseline == TextBaselines.Middle)
                {
                    realY -= (yMax + yMin) * 0.5;
                }
                else if (fig.TextBaseline == TextBaselines.Baseline)
                {
                    realY -= yMax + yMin;
                }

                Font.DetailedFontMetrics metrics = fig.Font.MeasureTextAdvanced(fig.Text);

                Point corner1 = new Point(fig.Position.X - metrics.LeftSideBearing, realY + metrics.Top);
                Point corner2 = new Point(fig.Position.X + metrics.Width, realY + metrics.Top);
                Point corner3 = new Point(fig.Position.X + metrics.Width, realY + metrics.Bottom);
                Point corner4 = new Point(fig.Position.X - metrics.LeftSideBearing, realY + metrics.Bottom);

                corner1 = transformationMatrix.Multiply(corner1);
                corner2 = transformationMatrix.Multiply(corner2);
                corner3 = transformationMatrix.Multiply(corner3);
                corner4 = transformationMatrix.Multiply(corner4);

                return (Math.Min(corner1.X, Math.Min(corner2.X, Math.Min(corner3.X, corner4.X))), Math.Min(corner1.Y, Math.Min(corner2.Y, Math.Min(corner3.Y, corner4.Y))), Math.Max(corner1.X, Math.Max(corner2.X, Math.Max(corner3.X, corner4.X))), Math.Max(corner1.Y, Math.Max(corner2.Y, Math.Max(corner3.Y, corner4.Y))));
            }
            else if (figure is TransformFigure transf)
            {
                if (transf.TransformType == TransformFigure.TransformTypes.Transform)
                {
                    transformationMatrix = transformationMatrix.Multiply(transf.TransformationMatrix);
                }
                else if (transf.TransformType == TransformFigure.TransformTypes.Save)
                {
                    savedStates.Push(transformationMatrix);
                }
                else if (transf.TransformType == TransformFigure.TransformTypes.Restore)
                {
                    transformationMatrix = savedStates.Pop();
                }

                return (0, 0, 0, 0);
            }
            else if (figure is RasterImageFigure)
            {
                Point corner1 = new Point(0, 0);
                Point corner2 = new Point(0, 1);
                Point corner3 = new Point(1, 1);
                Point corner4 = new Point(1, 0);

                corner1 = transformationMatrix.Multiply(corner1);
                corner2 = transformationMatrix.Multiply(corner2);
                corner3 = transformationMatrix.Multiply(corner3);
                corner4 = transformationMatrix.Multiply(corner4);

                return (Math.Min(corner1.X, Math.Min(corner2.X, Math.Min(corner3.X, corner4.X))), Math.Min(corner1.Y, Math.Min(corner2.Y, Math.Min(corner3.Y, corner4.Y))), Math.Max(corner1.X, Math.Max(corner2.X, Math.Max(corner3.X, corner4.X))), Math.Max(corner1.Y, Math.Max(corner2.Y, Math.Max(corner3.Y, corner4.Y))));
            }
            else
            {
                return (0, 0, 0, 0);
            }
        }

        private static double[,] Multiply(this double[,] matrix, double[,] matrix2)
        {
            double[,] tbr = new double[3, 3];

            tbr[0, 0] = matrix[0, 0] * matrix2[0, 0] - matrix[0, 1] * matrix2[1, 0] + matrix[0, 2] * matrix2[2, 0];
            tbr[0, 1] = -matrix[0, 0] * matrix2[0, 1] + matrix[0, 1] * matrix2[1, 1] + matrix[0, 2] * matrix2[2, 1];
            tbr[0, 2] = matrix[0, 0] * matrix2[0, 2] + matrix[0, 1] * matrix2[1, 2] + matrix[0, 2] * matrix2[2, 2];

            tbr[1, 0] = matrix[1, 0] * matrix2[0, 0] - matrix[1, 1] * matrix2[1, 0] + matrix[1, 2] * matrix2[2, 0];
            tbr[1, 1] = -matrix[1, 0] * matrix2[0, 1] + matrix[1, 1] * matrix2[1, 1] + matrix[1, 2] * matrix2[2, 1];
            tbr[1, 2] = matrix[1, 0] * matrix2[0, 2] + matrix[1, 1] * matrix2[1, 2] + matrix[1, 2] * matrix2[2, 2];

            tbr[2, 0] = matrix[2, 0] * matrix2[0, 0] - matrix[2, 1] * matrix2[1, 0] + matrix[2, 2] * matrix2[2, 0];
            tbr[2, 1] = -matrix[2, 0] * matrix2[0, 1] + matrix[2, 1] * matrix2[1, 1] + matrix[2, 2] * matrix2[2, 1];
            tbr[2, 2] = matrix[2, 0] * matrix2[0, 2] + matrix[2, 1] * matrix2[1, 2] + matrix[2, 2] * matrix2[2, 2];

            return tbr;
        }

        private static Point Multiply(this double[,] matrix, Point point)
        {
            double[] transPt = new double[3];

            transPt[0] = matrix[0, 0] * point.X + matrix[0, 1] * point.Y + matrix[0, 2];
            transPt[1] = matrix[1, 0] * point.X + matrix[1, 1] * point.Y + matrix[1, 2];
            transPt[2] = matrix[2, 0] * point.X + matrix[2, 1] * point.Y + matrix[2, 2];

            return new Point(transPt[0] / transPt[2], transPt[1] / transPt[2]);
        }

        private static string FigureAsPDFString(IFigure figure, Dictionary<string, string> nonSymbolFontIds, Dictionary<string, string> symbolFontIds, Dictionary<string, Dictionary<char, int>> symbolGlyphIndices, double[] alphas, Dictionary<string, int> imageObjectNums)
        {

            StringBuilder sb = new StringBuilder();

            if (figure.Fill != null)
            {
                sb.Append(figure.Fill?.R.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + figure.Fill?.G.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + figure.Fill?.B.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " rg\n");
                sb.Append("/a" + Array.IndexOf(alphas, figure.Fill?.A).ToString() + " gs\n");
            }

            if (figure.Stroke != null)
            {
                sb.Append(figure.Stroke?.R.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + figure.Stroke?.G.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + figure.Stroke?.B.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " RG\n");
                sb.Append("/a" + Array.IndexOf(alphas, figure.Stroke?.A).ToString() + " gs\n");
                sb.Append(figure.LineWidth.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " w\n");
                sb.Append(((int)figure.LineCap).ToString() + " J\n");
                sb.Append(((int)figure.LineJoin).ToString() + " j\n");
                if (figure.LineDash.UnitsOff != 0 || figure.LineDash.UnitsOn != 0)
                {
                    sb.Append("[ " + figure.LineDash.UnitsOn.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + figure.LineDash.UnitsOff.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ] " + figure.LineDash.Phase.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " d\n");
                }
                else
                {
                    sb.Append("[] 0 d\n");
                }
            }

            if (figure is PathFigure)
            {
                PathFigure fig = figure as PathFigure;

                for (int i = 0; i < fig.Segments.Length; i++)
                {
                    switch (fig.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            {
                                sb.Append(fig.Segments[i].Point.X.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + fig.Segments[i].Point.Y.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " m ");
                            }
                            break;
                        case SegmentType.Line:
                            {
                                sb.Append(fig.Segments[i].Point.X.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + fig.Segments[i].Point.Y.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " l ");
                            }
                            break;
                        case SegmentType.CubicBezier:
                            for (int j = 0; j < fig.Segments[i].Points.Length; j++)
                            {
                                sb.Append(fig.Segments[i].Points[j].X.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + fig.Segments[i].Points[j].Y.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                            }
                            sb.Append("c ");
                            break;
                        case SegmentType.Close:
                            sb.Append("h ");
                            break;
                    }
                }

                if (fig.IsClipping)
                {
                    sb.Append("W n\n");
                }
                else
                {
                    if (fig.Fill != null)
                    {
                        sb.Append("f*\n");
                    }

                    if (fig.Stroke != null)
                    {
                        sb.Append("S\n");
                    }
                }
            }
            else if (figure is TextFigure)
            {
                TextFigure fig = figure as TextFigure;

                List<(string txt, bool isSymbolic)> segments = new List<(string txt, bool isSymbolic)>();

                StringBuilder currSeg = new StringBuilder();
                bool currSymbolic = false;

                for (int i = 0; i < fig.Text.Length; i++)
                {
                    if (CP1252Chars.Contains(fig.Text[i]))
                    {
                        if (!currSymbolic)
                        {
                            currSeg.Append(fig.Text[i]);
                        }
                        else
                        {
                            if (currSeg.Length > 0)
                            {
                                segments.Add((currSeg.ToString(), currSymbolic));
                            }

                            currSeg = new StringBuilder();
                            currSymbolic = false;
                            currSeg.Append(fig.Text[i]);
                        }
                    }
                    else
                    {
                        if (currSymbolic)
                        {
                            currSeg.Append(fig.Text[i]);
                        }
                        else
                        {
                            if (currSeg.Length > 0)
                            {
                                segments.Add((currSeg.ToString(), currSymbolic));
                            }

                            currSeg = new StringBuilder();
                            currSymbolic = true;
                            currSeg.Append(fig.Text[i]);
                        }
                    }
                }

                if (currSeg.Length > 0)
                {
                    segments.Add((currSeg.ToString(), currSymbolic));
                }



                double realX = fig.Position.X;

                if (fig.Font.FontFamily.TrueTypeFile != null)
                {
                    realX = fig.Position.X - fig.Font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(fig.Text[0]).LeftSideBearing * fig.Font.FontSize / 1000;
                }

                double yMax = 0;
                double yMin = 0;

                if (fig.Font.FontFamily.TrueTypeFile != null)
                {
                    for (int i = 0; i < fig.Text.Length; i++)
                    {
                        TrueTypeFile.VerticalMetrics vMet = fig.Font.FontFamily.TrueTypeFile.Get1000EmGlyphVerticalMetrics(fig.Text[i]);
                        yMin = Math.Min(yMin, vMet.YMin * fig.Font.FontSize / 1000);
                        yMax = Math.Max(yMax, vMet.YMax * fig.Font.FontSize / 1000);
                    }
                }

                double realY = fig.Position.Y;

                if (fig.TextBaseline == TextBaselines.Bottom)
                {
                    realY -= yMax;
                }
                else if (fig.TextBaseline == TextBaselines.Top)
                {
                    realY -= yMin;
                }
                else if (fig.TextBaseline == TextBaselines.Middle)
                {
                    realY -= (yMax + yMin) * 0.5;
                }
                else if (fig.TextBaseline == TextBaselines.Baseline)
                {
                    realY -= yMax + yMin;
                }

                double middleY = realY + (yMax + yMin) * 0.5;



                sb.Append("q\n1 0 0 1 0 " + (middleY).ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " cm\n");
                sb.Append("1 0 0 -1 0 0 cm\n");
                sb.Append("1 0 0 1 0 " + (-middleY).ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " cm\n");

                sb.Append("BT\n");

                if (figure.Stroke != null && figure.Fill != null)
                {
                    sb.Append("2 Tr\n");
                }
                else if (figure.Stroke != null)
                {
                    sb.Append("1 Tr\n");
                }
                else if (figure.Fill != null)
                {
                    sb.Append("0 Tr\n");
                }

                sb.Append(realX.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + realY.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " Td\n");

                for (int i = 0; i < segments.Count; i++)
                {
                    if (!segments[i].isSymbolic)
                    {
                        sb.Append("/" + nonSymbolFontIds[fig.Font.FontFamily.FileName] + " " + fig.Font.FontSize.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " Tf\n");
                        sb.Append("(" + EscapeStringForPDF(segments[i].txt) + ") Tj\n");
                    }
                    else
                    {
                        sb.Append("/" + symbolFontIds[fig.Font.FontFamily.FileName] + " " + fig.Font.FontSize.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " Tf\n");
                        sb.Append("<" + EscapeSymbolStringForPDF(segments[i].txt, symbolGlyphIndices[fig.Font.FontFamily.FileName]) + "> Tj\n");
                    }
                }

                sb.Append("ET\nQ\n");
            }
            else if (figure is TransformFigure transf)
            {
                if (transf.TransformType == TransformFigure.TransformTypes.Transform)
                {
                    sb.Append(transf.TransformationMatrix[0, 0].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                    sb.Append(transf.TransformationMatrix[0, 1].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                    sb.Append(transf.TransformationMatrix[1, 0].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                    sb.Append(transf.TransformationMatrix[1, 1].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                    sb.Append(transf.TransformationMatrix[0, 2].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                    sb.Append(transf.TransformationMatrix[1, 2].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " cm\n");
                }
                else if (transf.TransformType == TransformFigure.TransformTypes.Save)
                {
                    sb.Append("q\n");
                }
                else if (transf.TransformType == TransformFigure.TransformTypes.Restore)
                {
                    sb.Append("Q\n");
                }
            }
            else if (figure is RasterImageFigure fig)
            {
                sb.Append("/a" + Array.IndexOf(alphas, 1).ToString() + " gs\n");

                int imageNum = imageObjectNums[fig.Image.Id];

                sb.Append("/Img" + imageNum.ToString() + " Do\n");
            }

            return sb.ToString();
        }

        internal static MemoryStream ZLibCompress(Stream contentStream)
        {
            MemoryStream compressedStream = new MemoryStream();
            compressedStream.Write(new byte[] { 0x78, 0x01 }, 0, 2);

            using (DeflateStream deflate = new DeflateStream(compressedStream, CompressionLevel.Optimal, true))
            {
                contentStream.CopyTo(deflate);
            }
            contentStream.Seek(0, SeekOrigin.Begin);

            uint checksum = Adler32(contentStream);

            compressedStream.Write(new byte[] { (byte)((checksum >> 24) & 255), (byte)((checksum >> 16) & 255), (byte)((checksum >> 8) & 255), (byte)(checksum & 255) }, 0, 4);

            compressedStream.Seek(0, SeekOrigin.Begin);

            return compressedStream;
        }

        internal static uint Adler32(Stream contentStream)
        {
            uint s1 = 1;
            uint s2 = 0;

            int readByte;

            while ((readByte = contentStream.ReadByte()) >= 0)
            {
                s1 = (s1 + (byte)readByte) % 65521U;
                s2 = (s2 + s1) % 65521U;
            }

            return (s2 << 16) + s1;
        }
    }
}
