using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        public double LineWidth { get; }

        public double[,] TransformationMatrix { get; }

        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }
        public Segment[] Segments { get; }

        public TransformFigure(TransformTypes type, double[,] transformationMatrix)
        {
            this.TransformType = type;
            this.TransformationMatrix = transformationMatrix;
        }
    }

    internal class PathFigure : IFigure
    {
        public Colour? Fill { get; }
        public Colour? Stroke { get; }
        public double LineWidth { get; }

        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }
        public Segment[] Segments { get; }

        public PathFigure(IEnumerable<Segment> segments, Colour? fill, Colour? stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash)
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
        }
    }

    internal class TextFigure : IFigure
    {
        public Colour? Fill { get; }
        public Colour? Stroke { get; }
        public double LineWidth { get; }

        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }

        public string Text { get; }

        public Font Font { get; }

        public Point Position { get; }

        public TextBaselines TextBaseline { get; }

        public TextFigure(string text, Font font, Point position, TextBaselines textBaseline, Colour? fill, Colour? stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash)
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

        private bool _textToPaths;

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
            _figures.Add(new PathFigure(_currentFigure, _fillStyle, null, 0, LineCaps.Butt, LineJoins.Bevel, new LineDash(0, 0, 0)));
            _currentFigure = new List<Segment>();
        }

        public void Stroke()
        {
            _figures.Add(new PathFigure(_currentFigure, null, _strokeStyle, LineWidth, LineCap, LineJoin, _lineDash));
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
                _figures.Add(new TextFigure(text, Font, new Point(x, y), TextBaseline, _fillStyle, null, 0, LineCaps.Butt, LineJoins.Miter, new LineDash(0, 0, 0))); _figures.Add(new TextFigure(text, Font, new Point(x, y), TextBaseline, _fillStyle, null, 0, LineCaps.Butt, LineJoins.Miter, new LineDash(0, 0, 0)));
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
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Restore, null));
        }

        public void Rotate(double angle)
        {
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { Math.Cos(angle), Math.Sin(angle), 0 }, { -Math.Sin(angle), Math.Cos(angle), 0 }, { 0, 0, 1 } }));
        }

        public void Save()
        {
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Save, null));
        }


        public void StrokeText(string text, double x, double y)
        {
            if (!_textToPaths)
            {
                _figures.Add(new TextFigure(text, Font, new Point(x, y), TextBaseline, null, _strokeStyle, LineWidth, LineCap, LineJoin, _lineDash));
            }
            else
            {
                PathText(text, x, y);
                Stroke();
            }
        }

        public void Translate(double x, double y)
        {
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { 1, 0, x }, { 0, 1, y }, { 0, 0, 1 } }));
        }

        public void Scale(double scaleX, double scaleY)
        {
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { scaleX, 0, 0 }, { 0, scaleY, 0 }, { 0, 0, 1 } }));
        }

        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { a, b, e }, { c, d, f }, { 0, 0, 1 } }));
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
                    if (act is TextFigure && !tbr.ContainsKey(((TextFigure)act).Font.FontFamily.FileName))
                    {
                        tbr.Add(((TextFigure)act).Font.FontFamily.FileName, new FontFamily(((TextFigure)act).Font.FontFamily.FileName));
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
                    if (act is TextFigure && !tbr.ContainsKey(((TextFigure)act).Font.FontFamily.FileName))
                    {
                        tbr.Add(((TextFigure)act).Font.FontFamily.FileName, new HashSet<char>(((TextFigure)act).Text));
                    }
                    else if (act is TextFigure)
                    {
                        string txt = ((TextFigure)act).Text;
                        for (int i = 0; i < txt.Length; i++)
                        {
                            tbr[((TextFigure)act).Font.FontFamily.FileName].Add(txt[i]);
                        }
                    }
                }
            }

            return tbr;
        }


        private static double[] GetAlphas(PDFContext[] pdfContexts)
        {
            HashSet<double> tbr = new HashSet<double>();

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


        private static readonly char[] CP1252Chars = new char[] { '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', '\u0008', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F', '\u0020', '\u0021', '\u0022', '\u0023', '\u0024', '\u0025', '\u0026', '\u0027', '\u0028', '\u0029', '\u002A', '\u002B', '\u002C', '\u002D', '\u002E', '\u002F', '\u0030', '\u0031', '\u0032', '\u0033', '\u0034', '\u0035', '\u0036', '\u0037', '\u0038', '\u0039', '\u003A', '\u003B', '\u003C', '\u003D', '\u003E', '\u003F', '\u0040', '\u0041', '\u0042', '\u0043', '\u0044', '\u0045', '\u0046', '\u0047', '\u0048', '\u0049', '\u004A', '\u004B', '\u004C', '\u004D', '\u004E', '\u004F', '\u0050', '\u0051', '\u0052', '\u0053', '\u0054', '\u0055', '\u0056', '\u0057', '\u0058', '\u0059', '\u005A', '\u005B', '\u005C', '\u005D', '\u005E', '\u005F', '\u0060', '\u0061', '\u0062', '\u0063', '\u0064', '\u0065', '\u0066', '\u0067', '\u0068', '\u0069', '\u006A', '\u006B', '\u006C', '\u006D', '\u006E', '\u006F', '\u0070', '\u0071', '\u0072', '\u0073', '\u0074', '\u0075', '\u0076', '\u0077', '\u0078', '\u0079', '\u007A', '\u007B', '\u007C', '\u007D', '\u007E', '\u007F', '\u20AC', '\u25A1', '\u201A', '\u0192', '\u201E', '\u2026', '\u2020', '\u2021', '\u02C6', '\u2030', '\u0160', '\u2039', '\u0152', '\u25A1', '\u017D', '\u25A1', '\u25A1', '\u2018', '\u2019', '\u201C', '\u201D', '\u2022', '\u2013', '\u2014', '\u02DC', '\u2122', '\u0161', '\u203A', '\u0153', '\u25A1', '\u017E', '\u0178', '\u00A0', '\u00A1', '\u00A2', '\u00A3', '\u00A4', '\u00A5', '\u00A6', '\u00A7', '\u00A8', '\u00A9', '\u00AA', '\u00AB', '\u00AC', '\u00AD', '\u00AE', '\u00AF', '\u00B0', '\u00B1', '\u00B2', '\u00B3', '\u00B4', '\u00B5', '\u00B6', '\u00B7', '\u00B8', '\u00B9', '\u00BA', '\u00BB', '\u00BC', '\u00BD', '\u00BE', '\u00BF', '\u00C0', '\u00C1', '\u00C2', '\u00C3', '\u00C4', '\u00C5', '\u00C6', '\u00C7', '\u00C8', '\u00C9', '\u00CA', '\u00CB', '\u00CC', '\u00CD', '\u00CE', '\u00CF', '\u00D0', '\u00D1', '\u00D2', '\u00D3', '\u00D4', '\u00D5', '\u00D6', '\u00D7', '\u00D8', '\u00D9', '\u00DA', '\u00DB', '\u00DC', '\u00DD', '\u00DE', '\u00DF', '\u00E0', '\u00E1', '\u00E2', '\u00E3', '\u00E4', '\u00E5', '\u00E6', '\u00E7', '\u00E8', '\u00E9', '\u00EA', '\u00EB', '\u00EC', '\u00ED', '\u00EE', '\u00EF', '\u00F0', '\u00F1', '\u00F2', '\u00F3', '\u00F4', '\u00F5', '\u00F6', '\u00F7', '\u00F8', '\u00F9', '\u00FA', '\u00FB', '\u00FC', '\u00FD', '\u00FE', '\u00FF' };

        /// <summary>
        /// Save the document to a PDF file.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to save.</param>
        /// <param name="fileName">The full path to the file to save. If it exists, it will be overwritten.</param>
        /// <param name="textOption">Defines whether the used fonts should be included in the file.</param>
        /// <param name="compressStreams">Indicates whether the streams in the PDF file should be compressed.</param>
        public static void SaveAsPDF(this Document document, string fileName, TextOptions textOption = TextOptions.SubsetFonts, bool compressStreams = true)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                document.SaveAsPDF(stream, textOption, compressStreams);
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
        public static void SaveAsPDF(this Document document, Stream stream, TextOptions textOption = TextOptions.SubsetFonts, bool compressStreams = true)
        {
            long position = 0;

            List<long> objectPositions = new List<long>();

            int objectNum = 1;
            string currObject = "";

            int resourceObject = -1;

            StreamWriter sw = new StreamWriter(stream, Encoding.UTF8, 1024, true);

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

                        int firstChar = (from el in nonSymbol select (int)el).Min();
                        int lastChar = (from el in nonSymbol select (int)el).Max();

                        currObject = objectNum.ToString() + " 0 obj\n<< /Type /Font /Subtype /TrueType /BaseFont /" + desc.FontName + " /FirstChar " + firstChar.ToString() + " /LastChar " + lastChar.ToString() + " /FontDescriptor " + fontDescriptorInd.ToString() + " 0 R /Encoding /WinAnsiEncoding /Widths [ ";

                        for (int i = firstChar; i <= lastChar; i++)
                        {
                            if (nonSymbol.Contains((char)i))
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

                currObject += " >>\nendobj\n";
                sw.Write(currObject);
                position += currObject.Length;
                objectNum++;
            }

            int[] pageContentInd = new int[document.Pages.Count];

            for (int pageInd = 0; pageInd < document.Pages.Count; pageInd++)
            {
                MemoryStream contentStream = new MemoryStream();



                using (StreamWriter ctW = new StreamWriter(contentStream, Encoding.ASCII, 1024, true))
                {
                    for (int i = 0; i < pageContexts[pageInd]._figures.Count; i++)
                    {
                        ctW.Write(FigureAsPDFString(pageContexts[pageInd]._figures[i], nonSymbolFontIDs, symbolFontIDs, symbolGlyphIndices, alphas));
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
                if (!compressStreams)
                {
                    streamLength--;
                }

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

            //Pages
            objectPositions.Add(position);
            int pageParent = objectNum;
            currObject = objectNum.ToString() + " 0 obj\n<< /Type /Pages /Kids [ ";
            for (int i = 0; i < document.Pages.Count; i++)
            {
                currObject += (objectNum + i + 1).ToString() + " 0 R ";
            }
            currObject += "] /Count " + document.Pages.Count + " >>\nendobj\n";
            sw.Write(currObject);
            position += currObject.Length;
            objectNum++;

            //Page
            for (int i = 0; i < document.Pages.Count; i++)
            {
                objectPositions.Add(position);
                currObject = objectNum.ToString() + " 0 obj\n<< /Type /Page /Parent " + pageParent.ToString() + " 0 R /MediaBox [0 0 " + document.Pages[i].Width.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + document.Pages[i].Height.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + "] /Resources " + resourceObject.ToString() + " 0 R /Contents " + pageContentInd[i].ToString() + " 0 R >>\nendobj\n";
                sw.Write(currObject);
                objectNum++;
                position += currObject.Length;
            }

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

        private static string FigureAsPDFString(IFigure figure, Dictionary<string, string> nonSymbolFontIds, Dictionary<string, string> symbolFontIds, Dictionary<string, Dictionary<char, int>> symbolGlyphIndices, double[] alphas)
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

                if (fig.Fill != null)
                {
                    sb.Append("f\n");
                }

                if (fig.Stroke != null)
                {
                    sb.Append("S\n");
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
