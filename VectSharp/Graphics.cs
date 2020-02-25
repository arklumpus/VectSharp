using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VectSharp
{
    internal static class Utils
    {
        public static Point RotatePoint(Point point, double angle)
        {
            return new Point(point.X * Math.Cos(angle) - point.Y * Math.Sin(angle), point.X * Math.Sin(angle) + point.Y * Math.Cos(angle));
        }
    }

    public enum TextBaselines { Top, Bottom, Middle, Baseline }
    public enum LineCaps { Butt = 0, Round = 1, Square = 2 }
    public enum LineJoins { Bevel = 2, Miter = 0, Round = 1 }

    /// <summary>
    /// Represents instructions on how to paint a dashed line.
    /// </summary>
    public struct LineDash
    {
        /// <summary>
        /// A solid (not dashed) line
        /// </summary>
        public static LineDash SolidLine = new LineDash(0, 0, 0);

        /// <summary>
        /// Length of the "on" (painted) segment.
        /// </summary>
        public double UnitsOn;

        /// <summary>
        /// Length of the "off" (not painted) segment.
        /// </summary>
        public double UnitsOff;

        /// <summary>
        /// Position in the dash pattern at which the line starts.
        /// </summary>
        public double Phase;

        /// <summary>
        /// Define a new line dash pattern.
        /// </summary>
        /// <param name="unitsOn">The length of the "on" (painted) segment.</param>
        /// <param name="unitsOff">The length of the "off" (not painted) segment.</param>
        /// <param name="phase">The position in the dash pattern at which the line starts.</param>
        public LineDash(double unitsOn, double unitsOff, double phase)
        {
            UnitsOn = unitsOn;
            UnitsOff = unitsOff;
            Phase = phase;
        }
    }

    /// <summary>
    /// Represents an RGB colour.
    /// </summary>
    public struct Colour : IEquatable<Colour>
    {
        /// <summary>
        /// Red component of the colour. Range: [0, 1].
        /// </summary>
        public double R;

        /// <summary>
        /// Green component of the colour. Range: [0, 1].
        /// </summary>
        public double G;

        /// <summary>
        /// Blue component of the colour. Range: [0, 1].
        /// </summary>
        public double B;

        /// <summary>
        /// Alpha component of the colour. Range: [0, 1].
        /// </summary>
        public double A;

        private Colour(double r, double g, double b, double a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Create a new colour from RGB (red, green and blue) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 1].</param>
        /// <param name="g">The green component of the colour. Range: [0, 1].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 1].</param>
        /// <returns>A colour struct witht the specified components and an alpha component of 1.</returns>
        public static Colour FromRgb(double r, double g, double b)
        {
            return new Colour(r, g, b, 1);
        }

        /// <summary>
        /// Create a new colour from RGB (red, green and blue) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <returns>A colour struct witht the specified components and an alpha component of 1.</returns>
        public static Colour FromRgb(byte r, byte g, byte b)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, 1);
        }

        /// <summary>
        /// Create a new colour from RGB (red, green and blue) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <returns>A colour struct witht the specified components and an alpha component of 1.</returns>
        public static Colour FromRgb(int r, int g, int b)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, 1);
        }

        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 1].</param>
        /// <param name="g">The green component of the colour. Range: [0, 1].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 1].</param>
        /// <param name="a">The alpha component of the colour. Range: [0, 1].</param>
        /// <returns>A colour struct witht the specified components.</returns>
        public static Colour FromRgba(double r, double g, double b, double a)
        {
            return new Colour(r, g, b, a);
        }

        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <param name="a">The alpha component of the colour. Range: [0, 255].</param>
        /// <returns>A colour struct witht the specified components.</returns>
        public static Colour FromRgba(byte r, byte g, byte b, byte a)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, a / 255.0);
        }

        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <param name="a">The alpha component of the colour. Range: [0, 1].</param>
        /// <returns>A colour struct witht the specified components.</returns>
        public static Colour FromRgba(byte r, byte g, byte b, double a)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, a);
        }
        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <param name="a">The alpha component of the colour. Range: [0, 255].</param>
        /// <returns>A colour struct witht the specified components.</returns>

        public static Colour FromRgba(int r, int g, int b, int a)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, a / 255.0);
        }

        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="r">The red component of the colour. Range: [0, 255].</param>
        /// <param name="g">The green component of the colour. Range: [0, 255].</param>
        /// <param name="b">The blue component of the colour. Range: [0, 255].</param>
        /// <param name="a">The alpha component of the colour. Range: [0, 1].</param>
        /// <returns>A colour struct witht the specified components.</returns>
        public static Colour FromRgba(int r, int g, int b, double a)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, a);


        }

        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="colour">A <see cref="System.ValueTuple{int, int, int, double}"/> containing component information for the colour. For r, g, and b, range: [0, 255]; for a, range: [0, 1].</param>
        /// <returns>A colour struct witht the specified components.</returns>
        public static Colour FromRgba((int r, int g, int b, double a) colour)
        {
            return new Colour(colour.r / 255.0, colour.g / 255.0, colour.b / 255.0, colour.a);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Colour))
            {
                return false;
            }
            else
            {
                return this.Equals((Colour)obj);
            }
        }

        public bool Equals(Colour col)
        {
            return col.R == this.R && col.G == this.G && col.B == this.B && col.A == this.A;
        }

        public static bool operator ==(Colour col1, Colour col2)
        {
            return col1.R == col2.R && col1.G == col2.G && col1.B == col2.B && col1.A == col2.A;
        }

        public static bool operator !=(Colour col1, Colour col2)
        {
            return col1.R != col2.R || col1.G != col2.G || col1.B != col2.B || col1.A != col2.A;
        }

        public override int GetHashCode()
        {
            return (int)(this.R * 255 + this.G * 255 * 255 + this.B * 255 * 255 * 255 + this.A * 255 * 255 * 255 * 255);
        }
    }

    /// <summary>
    /// Represents a typeface with a specific size.
    /// </summary>
    public class Font
    {
        /// <summary>
        /// Represents detailed information about the metrics of a text string when drawn with a certain font.
        /// </summary>
        public class DetailedFontMetrics
        {
            /// <summary>
            /// Width of the text (measured on the actual glyph outlines).
            /// </summary>
            public double Width { get; }

            /// <summary>
            /// Height of the text (measured on the actual glyph outlines).
            /// </summary>
            public double Height { get; }

            /// <summary>
            /// How much the leftmost glyph in the string overhangs the glyph origin on the left. Positive for glyphs that hang past the origin (e.g. italic 'f').
            /// </summary>
            public double LeftSideBearing { get; }

            /// <summary>
            /// How much the rightmost glyph in the string overhangs the glyph end on the right. Positive for glyphs that hang past the end (e.g. italic 'f').
            /// </summary>
            public double RightSideBearing { get; }

            /// <summary>
            /// Height of the tallest glyph in the string over the baseline. Always ≥ 0.
            /// </summary>
            public double Top { get; }

            /// <summary>
            /// Depth of the deepest glyph in the string below the baseline. Always ≤ 0.
            /// </summary>
            public double Bottom { get; }

            internal DetailedFontMetrics(double width, double height, double leftSideBearing, double rightSideBearing, double top, double bottom)
            {
                this.Width = width;
                this.Height = height;
                this.LeftSideBearing = leftSideBearing;
                this.RightSideBearing = rightSideBearing;
                this.Top = top;
                this.Bottom = bottom;
            }
        }

        /// <summary>
        /// Font size, in graphics units.
        /// </summary>
        public double FontSize { get; }

        /// <summary>
        /// Font typeface.
        /// </summary>
        public FontFamily FontFamily { get; }

        /// <summary>
        /// Create a new Font object, given the base typeface and the font size.
        /// </summary>
        /// <param name="fontFamily">Base typeface. See <see cref="FontFamily"/>.</param>
        /// <param name="fontSize">The font size, in graphics units.</param>
        public Font(FontFamily fontFamily, double fontSize)
        {
            this.FontFamily = fontFamily;
            this.FontSize = fontSize;
        }

        /// <summary>
        /// Maximum height over the baseline of the usual glyphs in the font (there may be glyphs taller than this). Always ≥ 0.
        /// </summary>
        public double Ascent
        {
            get
            {
                if (this.FontFamily.TrueTypeFile == null)
                {
                    return 0;
                }
                else
                {
                    return this.FontFamily.TrueTypeFile.Get1000EmAscent() * this.FontSize / 1000;
                }
            }
        }

        /// <summary>
        /// Maximum depth below the baseline of the usual glyphs in the font (there may be glyphs deeper than this). Always ≤ 0.
        /// </summary>
        public double Descent
        {
            get
            {
                if (this.FontFamily.TrueTypeFile == null)
                {
                    return 0;
                }
                else
                {
                    return this.FontFamily.TrueTypeFile.Get1000EmDescent() * this.FontSize / 1000;
                }
            }
        }

        /// <summary>
        /// Absolute maximum height over the baseline of the glyphs in the font. Always ≥ 0.
        /// </summary>
        public double YMax
        {
            get
            {
                if (this.FontFamily.TrueTypeFile == null)
                {
                    return 0;
                }
                else
                {
                    return this.FontFamily.TrueTypeFile.Get1000EmYMax() * this.FontSize / 1000;
                }
            }
        }

        /// <summary>
        /// Absolute maximum depth below the baseline of the glyphs in the font. Always ≤ 0.
        /// </summary>
        public double YMin
        {
            get
            {
                if (this.FontFamily.TrueTypeFile == null)
                {
                    return 0;
                }
                else
                {
                    return this.FontFamily.TrueTypeFile.Get1000EmYMin() * this.FontSize / 1000;
                }
            }
        }

        /// <summary>
        /// Measure the size of a text string when typeset with this font.
        /// </summary>
        /// <param name="text">The string to measure.</param>
        /// <returns>A <see cref="Size"/> object representing the width and height of the text.</returns>
        public Size MeasureText(string text)
        {
            if (this.FontFamily.TrueTypeFile != null)
            {
                double width = 0;
                double yMin = 0;
                double yMax = 0;

                for (int i = 0; i < text.Length; i++)
                {
                    width += this.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(text[i]) * this.FontSize / 1000;
                    TrueTypeFile.VerticalMetrics vMet = this.FontFamily.TrueTypeFile.Get1000EmGlyphVerticalMetrics(text[i]);

                    yMin = Math.Min(yMin, vMet.YMin * this.FontSize / 1000);
                    yMax = Math.Max(yMax, vMet.YMax * this.FontSize / 1000);
                }

                width -= this.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(text[0]).LeftSideBearing * this.FontSize / 1000;
                width -= this.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(text[text.Length - 1]).RightSideBearing * this.FontSize / 1000;

                return new Size(width, yMax - yMin);
            }
            else
            {
                return new Size(0, 0);
            }
        }

        /// <summary>
        /// Measure all the metrics of a text string when typeset with this font.
        /// </summary>
        /// <param name="text">The string to measure.</param>
        /// <returns>A <see cref="DetailedFontMetrics"/> object representing the metrics of the text.</returns>
        public DetailedFontMetrics MeasureTextAdvanced(string text)
        {
            if (this.FontFamily.TrueTypeFile != null)
            {
                double width = 0;
                double yMin = 0;
                double yMax = 0;

                for (int i = 0; i < text.Length; i++)
                {
                    width += this.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(text[i]) * this.FontSize / 1000;
                    TrueTypeFile.VerticalMetrics vMet = this.FontFamily.TrueTypeFile.Get1000EmGlyphVerticalMetrics(text[i]);

                    yMin = Math.Min(yMin, vMet.YMin * this.FontSize / 1000);
                    yMax = Math.Max(yMax, vMet.YMax * this.FontSize / 1000);
                }

                double lsb = this.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(text[0]).LeftSideBearing * this.FontSize / 1000;
                double rsb = this.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(text[text.Length - 1]).RightSideBearing * this.FontSize / 1000;

                width -= lsb;
                width -= rsb;

                return new DetailedFontMetrics(width, yMax - yMin, lsb, rsb, yMax, yMin);
            }
            else
            {
                return new DetailedFontMetrics(0, 0, 0, 0, 0, 0);
            }
        }
    }


    /// <summary>
    /// Represents a typeface.
    /// </summary>
    public class FontFamily
    {
        private static readonly Dictionary<string, Stream> manifestResources = new Dictionary<string, Stream>();

        private static Stream GetManifestResourceStream(string name)
        {
            if (!manifestResources.ContainsKey(name))
            {
                manifestResources.Add(name, typeof(FontFamily).Assembly.GetManifestResourceStream(name));
            }

            return manifestResources[name];
        }


        /// <summary>
        /// The names of the 14 standard families that are guaranteed to be displayed correctly.
        /// </summary>
        public static string[] StandardFamilies = new string[] { "Times-Roman", "Times-Bold", "Times-Italic", "Times-BoldItalic", "Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Helvetica-BoldOblique", "Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique", "Symbol", "ZapfDingbats" };

        /// <summary>
        /// The names of the resource streams pointing to the included TrueType font files for each of the standard 14 font families.
        /// </summary>
        public static string[] StandardFontFamilyResources = new string[]
        {
            "VectSharp.StandardFonts.NimbusRomNo9L-Reg.ttf", "VectSharp.StandardFonts.NimbusRomNo9L-Med.ttf", "VectSharp.StandardFonts.NimbusRomNo9L-RegIta.ttf", "VectSharp.StandardFonts.NimbusRomNo9L-MedIta.ttf",
            "VectSharp.StandardFonts.NimbusSanL-Reg.ttf", "VectSharp.StandardFonts.NimbusSanL-Bol.ttf",  "VectSharp.StandardFonts.NimbusSanL-RegIta.ttf", "VectSharp.StandardFonts.NimbusSanL-BolIta.ttf",
            "VectSharp.StandardFonts.NimbusMono-Regular.ttf", "VectSharp.StandardFonts.NimbusMono-Bold.ttf", "VectSharp.StandardFonts.NimbusMono-Oblique.ttf", "VectSharp.StandardFonts.NimbusMono-BoldOblique.ttf",
            "VectSharp.StandardFonts.StandardSymbolsPS.ttf", "VectSharp.StandardFonts.D050000L.ttf"
        };

        /// <summary>
        /// Whether this is one of the 14 standard font families or not.
        /// </summary>
        public bool IsStandardFamily { get; }

        /// <summary>
        /// The 14 standard font families.
        /// </summary>
        public enum StandardFontFamilies
        {
            TimesRoman, TimesBold, TimesItalic, TimesBoldItalic,
            Helvetica, HelveticaBold, HelveticaOblique, HelveticaBoldOblique,
            Courier, CourierBold, CourierOblique, CourierBoldOblique,
            Symbol, ZapfDingbats
        }

        /// <summary>
        /// Full path to the TrueType font file for this font family (or, if this is a standard font family, name of the font family).
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Parsed TrueType font file for this font family.
        /// See also: <seealso cref="VectSharp.TrueTypeFile"/>.
        /// </summary>
        public TrueTypeFile TrueTypeFile { get; }

        /// <summary>
        /// Whether this font is bold or not. This is set based on the information included in the OS/2 table of the TrueType file.
        /// </summary>
        public bool IsBold { get; }

        /// <summary>
        /// Whether this font is italic or oblique or not. This is set based on the information included in the OS/2 table of the TrueType file.
        /// </summary>
        public bool IsItalic { get; }

        /// <summary>
        /// Whether this font is oblique or not. This is set based on the information included in the OS/2 table of the TrueType file.
        /// </summary>
        public bool IsOblique { get; }

        /// <summary>
        /// Create a new <see cref="FontFamily"/>.
        /// </summary>
        /// <param name="fileName">The full path to the TrueType font file for this font family or the name of a standard font family.</param>
        public FontFamily(string fileName)
        {
            if (StandardFamilies.Contains(fileName))
            {
                IsStandardFamily = true;
            }
            else
            {
                IsStandardFamily = false;
            }

            FileName = fileName;

            if (IsStandardFamily)
            {
                TrueTypeFile = TrueTypeFile.CreateTrueTypeFile(GetManifestResourceStream(StandardFontFamilyResources[Array.IndexOf(StandardFamilies, fileName)]));
                this.IsBold = TrueTypeFile.IsBold();

                if (FileName == "Times-Italic" || FileName == "Times-BoldItalic" || FileName == "Helvetica-Oblique" || FileName == "Helvetica-BoldOblique" || FileName == "Courier-Oblique" || FileName == "Courier-BoldOblique")
                {
                    this.IsItalic = true;
                    this.IsOblique = (FileName == "Courier-Oblique" || FileName == "Courier-BoldOblique");
                }
                else
                {
                    this.IsItalic = false;
                    this.IsOblique = false;
                }
            }
            else
            {
                try
                {
                    TrueTypeFile = TrueTypeFile.CreateTrueTypeFile(fileName);
                    this.IsBold = TrueTypeFile.IsBold();
                    this.IsItalic = TrueTypeFile.IsItalic();
                    this.IsOblique = TrueTypeFile.IsOblique();
                }
                catch
                {
                    TrueTypeFile = null;
                }
            }
        }

        /// <summary>
        /// Create a new standard <see cref="FontFamily"/>.
        /// </summary>
        /// <param name="standardFontFamily">The standard font family.</param>
        public FontFamily(StandardFontFamilies standardFontFamily)
        {
            IsStandardFamily = true;

            FileName = StandardFamilies[(int)standardFontFamily];
            TrueTypeFile = TrueTypeFile.CreateTrueTypeFile(GetManifestResourceStream(StandardFontFamilyResources[(int)standardFontFamily]));
            this.IsBold = TrueTypeFile.IsBold();

            if (FileName == "Times-Italic" || FileName == "Times-BoldItalic" || FileName == "Helvetica-Oblique" || FileName == "Helvetica-BoldOblique" || FileName == "Courier-Oblique" || FileName == "Courier-BoldOblique")
            {
                this.IsItalic = true;
                this.IsOblique = (FileName == "Courier-Oblique" || FileName == "Courier-BoldOblique");
            }
            else
            {
                this.IsItalic = false;
                this.IsOblique = false;
            }
        }
    }


    /// <summary>
    /// Represents a point relative to an origin in the top-left corner.
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// Horizontal (x) coordinate, measured to the right of the origin.
        /// </summary>
        public double X;

        /// <summary>
        /// Vertical (y) coordinate, measured to the bottom of the origin.
        /// </summary>
        public double Y;

        /// <summary>
        /// Create a new <see cref="Point"/>.
        /// </summary>
        /// <param name="x">The horizontal (x) coordinate.</param>
        /// <param name="y">The vertical (y) coordinate.</param>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Represents the size of an object.
    /// </summary>
    public struct Size
    {
        /// <summary>
        /// Width of the object.
        /// </summary>
        public double Width;

        /// <summary>
        /// Height of the object.
        /// </summary>
        public double Height;

        /// <summary>
        /// Create a new <see cref="Size"/>.
        /// </summary>
        /// <param name="width">The width of the object.</param>
        /// <param name="height">The height of the object.</param>
        public Size(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    /// Types of <see cref="Segment"/>.
    /// </summary>
    public enum SegmentType
    {
        Move, Line, CubicBezier, Arc, Close
    }

    /// <summary>
    /// Represents a segment as part of a <see cref="GraphicsPath"/>.
    /// </summary>
    public abstract class Segment
    {

        /// <summary>
        /// The type of the <see cref="Segment"/>.
        /// </summary>
        public abstract SegmentType Type { get; }

        /// <summary>
        /// The points used to define the <see cref="Segment"/>.
        /// </summary>
        public Point[] Points { get; protected set; }

        /// <summary>
        /// The end point of the <see cref="Segment"/>.
        /// </summary>
        public virtual Point Point
        {
            get
            {
                return Points[Points.Length - 1];
            }
        }

        /// <summary>
        /// Creates a copy of the <see cref="Segment"/>.
        /// </summary>
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

    internal class ArcSegment : Segment
    {
        public override SegmentType Type => SegmentType.Arc;

        public Segment[] ToBezierSegments()
        {
            List<Segment> tbr = new List<Segment>();

            if (EndAngle - StartAngle <= Math.PI / 2)
            {
                tbr.AddRange(GetBezierSegment(Points[0].X, Points[0].Y, Radius, StartAngle, EndAngle, true));
            }
            else
            {
                int count = (int)Math.Ceiling(2 * (EndAngle - StartAngle) / Math.PI);
                double angle = StartAngle;

                for (int i = 0; i < count; i++)
                {
                    tbr.AddRange(GetBezierSegment(Points[0].X, Points[0].Y, Radius, angle, angle + (EndAngle - StartAngle) / count, i == 0));
                    angle += (EndAngle - StartAngle) / count;
                }
            }

            return tbr.ToArray();
        }

        const double k = 0.55191496;

        private static Segment[] GetBezierSegment(double cX, double cY, double radius, double startAngle, double endAngle, bool firstArc)
        {
            double phi = Math.PI / 4;

            double x1 = radius * Math.Cos(phi);
            double y1 = radius * Math.Sin(phi);

            double x4 = x1;
            double y4 = -y1;

            double x3 = x1 + k * radius * Math.Sin(phi);
            double y3 = y1 - k * radius * Math.Cos(phi);

            double x2 = x4 + k * radius * Math.Sin(phi);
            double y2 = y4 + k * radius * Math.Cos(phi);

            double u = 2 * (endAngle - startAngle) / Math.PI;

            double fx2 = (1 - u) * x4 + u * x2;
            double fy2 = (1 - u) * y4 + u * y2;

            double fx3 = (1 - u) * fx2 + u * ((1 - u) * x2 + u * x3);
            double fy3 = (1 - u) * fy2 + u * ((1 - u) * y2 + u * y3);

            double rX1 = cX + radius * Math.Cos(startAngle);
            double rY1 = cY + radius * Math.Sin(startAngle);

            double rX4 = cX + radius * Math.Cos(endAngle);
            double rY4 = cY + radius * Math.Sin(endAngle);

            Point rot2 = Utils.RotatePoint(new Point(fx2, fy2), phi + startAngle);
            Point rot3 = Utils.RotatePoint(new Point(fx3, fy3), phi + startAngle);

            List<Segment> tbr = new List<Segment>();

            if (firstArc)
            {
                tbr.Add(new LineSegment(rX1, rY1));
            }

            tbr.Add(new CubicBezierSegment(cX + rot2.X, cY + rot2.Y, cX + rot3.X, cY + rot3.Y, rX4, rY4));

            return tbr.ToArray();
        }
        public double Radius { get; }
        public double StartAngle { get; }
        public double EndAngle { get; }

        public ArcSegment(Point center, double radius, double startAngle, double endAngle)
        {
            this.Points = new Point[] { center };
            this.Radius = radius;
            this.StartAngle = startAngle;
            this.EndAngle = endAngle;
        }

        public ArcSegment(double centerX, double centerY, double radius, double startAngle, double endAngle)
        {
            this.Points = new Point[] { new Point(centerX, centerY) };
            this.Radius = radius;
            this.StartAngle = startAngle;
            this.EndAngle = endAngle;
        }

        public override Segment Clone()
        {
            return new ArcSegment(Point.X, Point.Y, Radius, StartAngle, EndAngle);
        }

        public override Point Point
        {
            get
            {
                return new Point(this.Points[0].X + Math.Cos(EndAngle) * Radius, this.Points[0].Y + Math.Sin(EndAngle) * Radius);
            }
        }
    }


    /// <summary>
    /// This interface should be implemented by classes intended to provide graphics output capability to a <see cref="Graphics"/> object.
    /// </summary>
    public interface IGraphicsContext
    {
        /// <summary>
        /// Width of the graphic surface.
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Height of the graphic surface.
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Save the current transform state (rotation, translation, scale). This should be implemented as a LIFO stack.
        /// </summary>
        void Save();

        /// <summary>
        /// Restore the previous transform state (rotation, translation, scale). This should be implemented as a LIFO stack.
        /// </summary>
        void Restore();

        /// <summary>
        /// Translate the coordinate system origin.
        /// </summary>
        /// <param name="x">The horizontal translation.</param>
        /// <param name="y">The vertical translation.</param>
        void Translate(double x, double y);

        /// <summary>
        /// Rotate the coordinate system around the origin.
        /// </summary>
        /// <param name="angle">The angle (in radians) by which to rotate the coordinate system.</param>
        void Rotate(double angle);

        /// <summary>
        /// Scale the coordinate system with respect to the origin.
        /// </summary>
        /// <param name="scaleX">The horizontal scale.</param>
        /// <param name="scaleY">The vertical scale.</param>
        void Scale(double scaleX, double scaleY);

        /// <summary>
        /// The current font.
        /// </summary>
        Font Font { get; set; }


        /// <summary>
        /// The current text baseline.
        /// </summary>
        TextBaselines TextBaseline { get; set; }

        /// <summary>
        /// Fill a text string using the current <see cref="Font"/> and <see cref="TextBaseline"/>.
        /// </summary>
        /// <param name="text">The string to draw.</param>
        /// <param name="x">The horizontal coordinate of the text origin.</param>
        /// <param name="y">The vertical coordinate of the text origin.</param>
        void FillText(string text, double x, double y);

        /// <summary>
        /// Stroke the outline of a text string using the current <see cref="Font"/> and <see cref="TextBaseline"/>.
        /// </summary>
        /// <param name="text">The string to draw.</param>
        /// <param name="x">The horizontal coordinate of the text origin.</param>
        /// <param name="y">The vertical coordinate of the text origin.</param>
        void StrokeText(string text, double x, double y);

        /// <summary>
        /// Change the current point without drawing a line from the previous point. If necessary, start a new figure.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the point.</param>
        /// <param name="y">The vertical coordinate of the point.</param>
        void MoveTo(double x, double y);

        /// <summary>
        /// Draw a line from the previous point to the specified point.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the point.</param>
        /// <param name="y">The vertical coordinate of the point.</param>
        void LineTo(double x, double y);

        /// <summary>
        /// Close the current figure.
        /// </summary>
        void Close();

        /// <summary>
        /// Stroke the current path using the current <see cref="StrokeStyle"/>, <see cref="LineWidth"/>, <see cref="LineCap"/>, <see cref="LineJoin"/> and <see cref="LineDash"/>.
        /// </summary>
        void Stroke();

        /// <summary>
        /// Current colour used to fill paths.
        /// </summary>
        Colour FillStyle { get; }

        /// <summary>
        /// Set the current <see cref="FillStyle"/>.
        /// </summary>
        /// <param name="style">A <see cref="System.ValueTuple{int, int, int, double}"/> containing component information for the colour. For r, g, and b, range: [0, 255]; for a, range: [0, 1].</param>
        void SetFillStyle((int r, int g, int b, double a) style);

        /// <summary>
        /// Set the current <see cref="FillStyle"/>.
        /// </summary>
        /// <param name="style">The new fill style.</param>
        void SetFillStyle(Colour style);

        /// <summary>
        /// Current colour used to stroke paths.
        /// </summary>
        Colour StrokeStyle { get; }

        /// <summary>
        /// Set the current <see cref="StrokeStyle"/>.
        /// </summary>
        /// <param name="style">A <see cref="System.ValueTuple{int, int, int, double}"/> containing component information for the colour. For r, g, and b, range: [0, 255]; for a, range: [0, 1].</param>
        void SetStrokeStyle((int r, int g, int b, double a) style);

        /// <summary>
        /// Set the current <see cref="StrokeStyle"/>.
        /// </summary>
        /// <param name="style">The new stroke style.</param>
        void SetStrokeStyle(Colour style);

        /// <summary>
        /// Add to the current figure a cubic Bezier from the current point to a destination point, with two control points.
        /// </summary>
        /// <param name="p1X">The horizontal coordinate of the first control point.</param>
        /// <param name="p1Y">The vertical coordinate of the first control point.</param>
        /// <param name="p2X">The horizontal coordinate of the second control point.</param>
        /// <param name="p2Y">The vertical coordinate of the second control point.</param>
        /// <param name="p3X">The horizontal coordinate of the destination point.</param>
        /// <param name="p3Y">The vertical coordinate of the destination point.</param>
        void CubicBezierTo(double p1X, double p1Y, double p2X, double p2Y, double p3X, double p3Y);

        /// <summary>
        /// Add a rectangle figure to the current path.
        /// </summary>
        /// <param name="x0">The horizontal coordinate of the top-left corner of the rectangle.</param>
        /// <param name="y0">The vertical coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of corner of the rectangle.</param>
        /// <param name="height">The height of corner of the rectangle.</param>
        void Rectangle(double x0, double y0, double width, double height);

        /// <summary>
        /// Fill the current path using the current <see cref="FillStyle"/>.
        /// </summary>
        void Fill();

        /// <summary>
        /// Current line width used to stroke paths.
        /// </summary>
        double LineWidth { get; set; }

        /// <summary>
        /// Current line cap used to stroke paths.
        /// </summary>
        LineCaps LineCap { set; }

        /// <summary>
        /// Current line join used to stroke paths.
        /// </summary>
        LineJoins LineJoin { set; }

        /// <summary>
        /// Set the current line dash pattern.
        /// </summary>
        /// <param name="dash">The line dash pattern.</param>
        void SetLineDash(LineDash dash);

        /// <summary>
        /// The current tag. How this can be used depends on each implementation.
        /// </summary>
        string Tag { get; set; }
    }

    /// <summary>
    /// Represents an abstract drawing surface.
    /// </summary>
    public class Graphics
    {
        internal List<IGraphicsAction> Actions = new List<IGraphicsAction>();

        /// <summary>
        /// Fill a <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> to fill.</param>
        /// <param name="fillColour">The <see cref="Colour"/> with which to fill the <see cref="GraphicsPath"/>.</param>
        /// <param name="tag">A tag to identify the filled path.</param>
        public void FillPath(GraphicsPath path, Colour fillColour, string tag = null)
        {
            Actions.Add(new PathAction(path, fillColour, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag));
        }


        /// <summary>
        /// Stroke a <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> to stroke.</param>
        /// <param name="strokeColour">The <see cref="Colour"/> with which to stroke the <see cref="GraphicsPath"/>.</param>
        /// <param name="lineWidth">The width of the line with which the path is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the path.</param>
        /// <param name="lineJoin">The line join to use to stroke the path.</param>
        /// <param name="lineDash">The line dash to use to stroke the path.</param>
        /// <param name="tag">A tag to identify the stroked path.</param>
        public void StrokePath(GraphicsPath path, Colour strokeColour, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new PathAction(path, null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Rotate the coordinate system around the origin.
        /// </summary>
        /// <param name="angle">The angle (in radians) by which to rotate the coordinate system.</param>
        public void Rotate(double angle)
        {
            Actions.Add(new TransformAction(angle));
        }

        /// <summary>
        /// Rotate the coordinate system around a pivot point.
        /// </summary>
        /// <param name="angle">The angle (in radians) by which to rotate the coordinate system.</param>
        /// <param name="pivot">The pivot around which the coordinate system is to be rotated.</param>
        public void RotateAt(double angle, Point pivot)
        {
            Actions.Add(new TransformAction(pivot));
            Actions.Add(new TransformAction(angle));
            Actions.Add(new TransformAction(new Point(-pivot.X, -pivot.Y)));
        }

        /// <summary>
        /// Translate the coordinate system origin.
        /// </summary>
        /// <param name="x">The horizontal translation.</param>
        /// <param name="y">The vertical translation.</param>
        public void Translate(double x, double y)
        {
            Actions.Add(new TransformAction(new Point(x, y)));
        }

        /// <summary>
        /// Translate the coordinate system origin.
        /// </summary>
        /// <param name="delta">The new origin point.</param>
        public void Translate(Point delta)
        {
            Actions.Add(new TransformAction(delta));
        }

        /// <summary>
        /// Scale the coordinate system with respect to the origin.
        /// </summary>
        /// <param name="scaleX">The horizontal scale.</param>
        /// <param name="scaleY">The vertical scale.</param>
        public void Scale(double scaleX, double scaleY)
        {
            Actions.Add(new TransformAction(new Size(scaleX, scaleY)));
        }

        /// <summary>
        /// Fill a rectangle.
        /// </summary>
        /// <param name="topLeft">The top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="fillColour">The colour with which to fill the rectangle.</param>
        /// <param name="tag">A tag to identify the filled rectangle.</param>
        public void FillRectangle(Point topLeft, Size size, Colour fillColour, string tag = null)
        {
            Actions.Add(new RectangleAction(topLeft, size, fillColour, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Fill a rectangle.
        /// </summary>
        /// <param name="leftX">The horizontal coordinate of the top-left corner of the rectangle.</param>
        /// <param name="topY">The vertical coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="fillColour">The colour with which to fill the rectangle.</param>
        /// <param name="tag">A tag to identify the filled rectangle.</param>
        public void FillRectangle(double leftX, double topY, double width, double height, Colour fillColour, string tag = null)
        {
            Actions.Add(new RectangleAction(new Point(leftX, topY), new Size(width, height), fillColour, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Stroke a rectangle.
        /// </summary>
        /// <param name="topLeft">The top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="strokeColour">The colour with which to stroke the rectangle.</param>
        /// <param name="lineWidth">The width of the line with which the rectangle is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the rectangle.</param>
        /// <param name="lineJoin">The line join to use to stroke the rectangle.</param>
        /// <param name="lineDash">The line dash to use to stroke the rectangle.</param>
        /// <param name="tag">A tag to identify the filled rectangle.</param>
        public void StrokeRectangle(Point topLeft, Size size, Colour strokeColour, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new RectangleAction(topLeft, size, null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Stroke a rectangle.
        /// </summary>
        /// <param name="leftX">The horizontal coordinate of the top-left corner of the rectangle.</param>
        /// <param name="topY">The vertical coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="strokeColour">The colour with which to stroke the rectangle.</param>
        /// <param name="lineWidth">The width of the line with which the rectangle is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the rectangle.</param>
        /// <param name="lineJoin">The line join to use to stroke the rectangle.</param>
        /// <param name="lineDash">The line dash to use to stroke the rectangle.</param>
        /// <param name="tag">A tag to identify the filled rectangle.</param>
        public void StrokeRectangle(double leftX, double topY, double width, double height, Colour strokeColour, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new RectangleAction(new Point(leftX, topY), new Size(width, height), null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Fill a text string.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="fillColour">The colour to use to fill the text.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <param name="tag">A tag to identify the filled text.</param>
        public void FillText(Point origin, string text, Font font, Colour fillColour, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            Actions.Add(new TextAction(origin, text, font, textBaseline, fillColour, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Fill a text string.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="fillColour">The colour to use to fill the text.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// <param name="tag">A tag to identify the filled text.</param>
        public void FillText(double originX, double originY, string text, Font font, Colour fillColour, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            Actions.Add(new TextAction(new Point(originX, originY), text, font, textBaseline, fillColour, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Stroke a text string.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="strokeColour">The colour with which to stroke the text.</param>
        /// <param name="lineWidth">The width of the line with which the text is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the text.</param>
        /// <param name="lineJoin">The line join to use to stroke the text.</param>
        /// <param name="lineDash">The line dash to use to stroke the text.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <param name="tag">A tag to identify the filled text.</param>
        public void StrokeText(Point origin, string text, Font font, Colour strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new TextAction(origin, text, font, textBaseline, null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Fill a text string.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="strokeColour">The colour with which to stroke the text.</param>
        /// <param name="lineWidth">The width of the line with which the text is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the text.</param>
        /// <param name="lineJoin">The line join to use to stroke the text.</param>
        /// <param name="lineDash">The line dash to use to stroke the text.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// <param name="tag">A tag to identify the filled text.</param>
        public void StrokeText(double originX, double originY, string text, Font font, Colour strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new TextAction(new Point(originX, originY), text, font, textBaseline, null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Measure a text string.
        /// See also <seealso cref="Font.MeasureText(string)"/> and <seealso cref="Font.MeasureTextAdvanced(string)"/>.
        /// </summary>
        /// <param name="text">The string to measure.</param>
        /// <param name="font">The font to use to measure the string.</param>
        /// <returns></returns>
        public Size MeasureText(string text, Font font)
        {
            return font.MeasureText(text);
        }

        /// <summary>
        /// Save the current transform state (rotation, translation, scale).
        /// </summary>
        public void Save()
        {
            Actions.Add(new StateAction(StateAction.StateActionTypes.Save));
        }

        /// <summary>
        /// Restore the previous transform state (rotation, translation scale).
        /// </summary>
        public void Restore()
        {
            Actions.Add(new StateAction(StateAction.StateActionTypes.Restore));
        }

        /// <summary>
        /// Copy the current graphics to an instance of a class implementing <see cref="IGraphicsContext"/>.
        /// </summary>
        /// <param name="destinationContext">The <see cref="IGraphicsContext"/> on which the graphics are to be copied.</param>
        public void CopyToIGraphicsContext(IGraphicsContext destinationContext)
        {
            for (int i = 0; i < this.Actions.Count; i++)
            {
                if (this.Actions[i] is RectangleAction)
                {
                    RectangleAction rec = this.Actions[i] as RectangleAction;

                    destinationContext.Tag = rec.Tag;
                    destinationContext.Rectangle(rec.TopLeft.X, rec.TopLeft.Y, rec.Size.Width, rec.Size.Height);

                    if (rec.Fill != null)
                    {
                        if (destinationContext.FillStyle != rec.Fill)
                        {
                            destinationContext.SetFillStyle((Colour)rec.Fill);
                        }
                        destinationContext.Fill();
                    }
                    else if (rec.Stroke != null)
                    {
                        if (destinationContext.StrokeStyle != rec.Stroke)
                        {
                            destinationContext.SetStrokeStyle((Colour)rec.Stroke);
                        }
                        if (destinationContext.LineWidth != rec.LineWidth)
                        {
                            destinationContext.LineWidth = rec.LineWidth;
                        }
                        destinationContext.SetLineDash(rec.LineDash);
                        destinationContext.LineCap = rec.LineCap;
                        destinationContext.LineJoin = rec.LineJoin;

                        destinationContext.Stroke();
                    }
                }
                else if (this.Actions[i] is PathAction)
                {
                    PathAction pth = this.Actions[i] as PathAction;

                    destinationContext.Tag = pth.Tag;

                    for (int j = 0; j < pth.Path.Segments.Count; j++)
                    {
                        switch (pth.Path.Segments[j].Type)
                        {
                            case SegmentType.Move:
                                destinationContext.MoveTo(pth.Path.Segments[j].Point.X, pth.Path.Segments[j].Point.Y);
                                break;
                            case SegmentType.Line:
                                destinationContext.LineTo(pth.Path.Segments[j].Point.X, pth.Path.Segments[j].Point.Y);
                                break;
                            case SegmentType.CubicBezier:
                                destinationContext.CubicBezierTo(pth.Path.Segments[j].Points[0].X, pth.Path.Segments[j].Points[0].Y, pth.Path.Segments[j].Points[1].X, pth.Path.Segments[j].Points[1].Y, pth.Path.Segments[j].Points[2].X, pth.Path.Segments[j].Points[2].Y);
                                break;
                            case SegmentType.Arc:
                                {
                                    ArcSegment seg = pth.Path.Segments[j] as ArcSegment;
                                    Segment[] segs = seg.ToBezierSegments();
                                    for (int k = 0; k < segs.Length; k++)
                                    {
                                        switch (segs[k].Type)
                                        {
                                            case SegmentType.Move:
                                                destinationContext.MoveTo(segs[k].Point.X, segs[k].Point.Y);
                                                break;
                                            case SegmentType.Line:
                                                destinationContext.LineTo(segs[k].Point.X, segs[k].Point.Y);
                                                break;
                                            case SegmentType.CubicBezier:
                                                destinationContext.CubicBezierTo(segs[k].Points[0].X, segs[k].Points[0].Y, segs[k].Points[1].X, segs[k].Points[1].Y, segs[k].Points[2].X, segs[k].Points[2].Y);
                                                break;
                                        }
                                    }
                                }
                                break;
                            case SegmentType.Close:
                                destinationContext.Close();
                                break;
                        }
                    }

                    if (pth.Fill != null)
                    {
                        if (destinationContext.FillStyle != pth.Fill)
                        {
                            destinationContext.SetFillStyle((Colour)pth.Fill);
                        }
                        destinationContext.Fill();
                    }
                    else if (pth.Stroke != null)
                    {
                        if (destinationContext.StrokeStyle != pth.Stroke)
                        {
                            destinationContext.SetStrokeStyle((Colour)pth.Stroke);
                        }
                        if (destinationContext.LineWidth != pth.LineWidth)
                        {
                            destinationContext.LineWidth = pth.LineWidth;
                        }
                        destinationContext.SetLineDash(pth.LineDash);
                        destinationContext.LineCap = pth.LineCap;
                        destinationContext.LineJoin = pth.LineJoin;

                        destinationContext.Stroke();
                    }
                }
                else if (this.Actions[i] is TextAction)
                {
                    TextAction txt = this.Actions[i] as TextAction;

                    destinationContext.Tag = txt.Tag;
                    if (destinationContext.TextBaseline != txt.TextBaseline)
                    {
                        destinationContext.TextBaseline = txt.TextBaseline;
                    }
                    destinationContext.Font = txt.Font;

                    if (txt.Fill != null)
                    {
                        if (destinationContext.FillStyle != txt.Fill)
                        {
                            destinationContext.SetFillStyle((Colour)txt.Fill);
                        }
                        destinationContext.FillText(txt.Text, txt.Origin.X, txt.Origin.Y);
                    }
                    else if (txt.Stroke != null)
                    {
                        if (destinationContext.StrokeStyle != txt.Stroke)
                        {
                            destinationContext.SetStrokeStyle((Colour)txt.Stroke);
                        }
                        if (destinationContext.LineWidth != txt.LineWidth)
                        {
                            destinationContext.LineWidth = txt.LineWidth;
                        }
                        destinationContext.SetLineDash(txt.LineDash);
                        destinationContext.LineCap = txt.LineCap;
                        destinationContext.LineJoin = txt.LineJoin;

                        destinationContext.StrokeText(txt.Text, txt.Origin.X, txt.Origin.Y);
                    }
                }
                else if (this.Actions[i] is TransformAction)
                {
                    TransformAction trf = this.Actions[i] as TransformAction;

                    if (trf.Delta != null)
                    {
                        destinationContext.Translate(((Point)trf.Delta).X, ((Point)trf.Delta).Y);
                    }
                    else if (trf.Angle != null)
                    {
                        destinationContext.Rotate((double)trf.Angle);
                    }
                    else if (trf.Scale != null)
                    {
                        destinationContext.Scale(((Size)trf.Scale).Width, ((Size)trf.Scale).Height);
                    }
                }
                else if (this.Actions[i] is StateAction)
                {
                    if (((StateAction)this.Actions[i]).StateActionType == StateAction.StateActionTypes.Save)
                    {
                        destinationContext.Save();
                    }
                    else
                    {
                        destinationContext.Restore();
                    }
                }
            }
        }
    }

    internal interface IGraphicsAction
    {

    }

    internal interface IPrintableAction
    {
        Colour? Fill { get; }
        Colour? Stroke { get; }
        double LineWidth { get; }
        LineCaps LineCap { get; }
        LineJoins LineJoin { get; }
        LineDash LineDash { get; }
        string Tag { get; }
    }

    internal class TransformAction : IGraphicsAction
    {
        public Point? Delta { get; } = null;

        public double? Angle { get; } = null;

        public Size? Scale { get; } = null;

        public TransformAction(Point delta)
        {
            this.Delta = delta;
        }

        public TransformAction(double angle)
        {
            this.Angle = angle;
        }

        public TransformAction(Size scale)
        {
            this.Scale = scale;
        }
    }

    internal class StateAction : IGraphicsAction
    {
        public enum StateActionTypes
        {
            Save, Restore
        }

        public StateActionTypes StateActionType { get; }

        public StateAction(StateActionTypes type)
        {
            this.StateActionType = type;
        }
    }

    internal class TextAction : IGraphicsAction, IPrintableAction
    {
        public Colour? Fill { get; }
        public Colour? Stroke { get; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public string Tag { get; }
        public string Text { get; }
        public Point Origin { get; }
        public TextBaselines TextBaseline { get; }
        public Font Font { get; }

        public TextAction(Point origin, string text, Font font, TextBaselines textBaseLine, Colour? fill, Colour? stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, string tag)
        {
            this.Origin = origin;
            this.Text = text;
            this.Font = font;
            this.TextBaseline = textBaseLine;
            this.Fill = fill;
            this.Stroke = stroke;
            this.LineCap = lineCap;
            this.LineJoin = lineJoin;
            this.LineWidth = lineWidth;
            this.Tag = tag;
            this.LineDash = lineDash;
        }
    }

    internal class RectangleAction : IGraphicsAction, IPrintableAction
    {
        public Colour? Fill { get; }
        public Colour? Stroke { get; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public string Tag { get; }
        public Point TopLeft { get; }
        public Size Size { get; }

        public RectangleAction(Point topLeft, Size size, Colour? fill, Colour? stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, string tag)
        {
            this.TopLeft = topLeft;
            this.Size = size;
            this.Fill = fill;
            this.Stroke = stroke;
            this.LineCap = lineCap;
            this.LineJoin = lineJoin;
            this.LineWidth = lineWidth;
            this.LineDash = lineDash;
            this.Tag = tag;
        }
    }

    internal class PathAction : IGraphicsAction, IPrintableAction
    {
        public GraphicsPath Path { get; }
        public Colour? Fill { get; }
        public Colour? Stroke { get; }
        public string Tag { get; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public PathAction(GraphicsPath path, Colour? fill, Colour? stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, string tag)
        {
            this.Path = path;
            this.Fill = fill;
            this.Stroke = stroke;
            this.LineCap = lineCap;
            this.LineJoin = lineJoin;
            this.LineWidth = lineWidth;
            this.LineDash = lineDash;
            this.Tag = tag;
        }
    }

    /// <summary>
    /// Represents a graphics path that can be filled or stroked.
    /// </summary>
    public class GraphicsPath
    {
        /// <summary>
        /// The segments that make up the path.
        /// </summary>
        public List<Segment> Segments { get; set; } = new List<Segment>();


        /// <summary>
        /// Move the current point without tracing a segment from the previous point.
        /// </summary>
        /// <param name="p">The new point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath MoveTo(Point p)
        {
            Segments.Add(new MoveSegment(p));
            return this;
        }

        /// <summary>
        /// Move the current point without tracing a segment from the previous point.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the new point.</param>
        /// <param name="y">The vertical coordinate of the new point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath MoveTo(double x, double y)
        {
            MoveTo(new Point(x, y));
            return this;
        }

        /// <summary>
        /// Move the current point and trace a segment from the previous point.
        /// </summary>
        /// <param name="p">The new point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath LineTo(Point p)
        {
            if (Segments.Count == 0)
            {
                Segments.Add(new MoveSegment(p));
            }
            else
            {
                Segments.Add(new LineSegment(p));
            }
            return this;
        }

        /// <summary>
        /// Move the current point and trace a segment from the previous point.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the new point.</param>
        /// <param name="y">The vertical coordinate of the new point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath LineTo(double x, double y)
        {
            LineTo(new Point(x, y));
            return this;
        }

        /// <summary>
        /// Trace an arc segment from a circle with the specified <paramref name="center"/> and <paramref name="radius"/>, starting at <paramref name="startAngle"/> and ending at <paramref name="endAngle"/>.
        /// The current point is updated to the end point of the arc.
        /// </summary>
        /// <param name="center">The center of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="startAngle">The start angle (in radians) of the arc.</param>
        /// <param name="endAngle">The end angle (in radians) of the arc.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath Arc(Point center, double radius, double startAngle, double endAngle)
        {
            if (Segments.Count == 0)
            {
                Segments.Add(new MoveSegment(center.X + radius * Math.Cos(startAngle), center.Y + radius * Math.Sin(startAngle)));
            }
            Segments.Add(new ArcSegment(center, radius, startAngle, endAngle));
            return this;
        }

        /// <summary>
        /// Trace an arc segment from a circle with the specified center and <paramref name="radius"/>, starting at <paramref name="startAngle"/> and ending at <paramref name="endAngle"/>.
        /// The current point is updated to the end point of the arc.
        /// </summary>
        /// <param name="centerX">The horizontal coordinate of the center of the arc.</param>
        /// <param name="centerY">The vertical coordinate of the center of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="startAngle">The start angle (in radians) of the arc.</param>
        /// <param name="endAngle">The end angle (in radians) of the arc.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath Arc(double centerX, double centerY, double radius, double startAngle, double endAngle)
        {
            Arc(new Point(centerX, centerY), radius, startAngle, endAngle);
            return this;
        }

        /// <summary>
        /// Trace a cubic Bezier curve from the current point to a destination point, with two control points.
        /// The current point is updated to the end point of the bezier curve.
        /// </summary>
        /// <param name="control1">The first control point.</param>
        /// <param name="control2">The second control point.</param>
        /// <param name="endPoint">The destination point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath CubicBezierTo(Point control1, Point control2, Point endPoint)
        {
            if (Segments.Count == 0)
            {
                Segments.Add(new MoveSegment(control1));
            }
            Segments.Add(new CubicBezierSegment(control1, control2, endPoint));
            return this;
        }

        /// <summary>
        /// Trace a cubic Bezier curve from the current point to a destination point, with two control points.
        /// The current point is updated to the end point of the bezier curve.
        /// </summary>
        /// <param name="control1X">The horizontal coordinate of the first control point.</param>
        /// <param name="control1Y">The vertical coordinate of the first control point.</param>
        /// <param name="control2X">The horizontal coordinate of the second control point.</param>
        /// <param name="control2Y">The vertical coordinate of the second control point.</param>
        /// <param name="endPointX">The horizontal coordinate of the destination point.</param>
        /// <param name="endPointY">The vertical coordinate of the destination point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath CubicBezierTo(double control1X, double control1Y, double control2X, double control2Y, double endPointX, double endPointY)
        {
            CubicBezierTo(new Point(control1X, control1Y), new Point(control2X, control2Y), new Point(endPointX, endPointY));
            return this;
        }

        /// <summary>
        /// Trace a segment from the current point to the start point of the figure and flag the figure as closed.
        /// </summary>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath Close()
        {
            Segments.Add(new CloseSegment());
            return this;
        }

        /// <summary>
        /// Add the contour of a text string to the current path.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath AddText(double originX, double originY, string text, Font font, TextBaselines textBaseline = TextBaselines.Top)
        {
            return AddText(new Point(originX, originY), text, font, textBaseline);
        }

        /// <summary>
        /// Add the contour of a text string to the current path.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath AddText(Point origin, string text, Font font, TextBaselines textBaseline = TextBaselines.Top)
        {
            Font.DetailedFontMetrics metrics = font.MeasureTextAdvanced(text);

            Point baselineOrigin = origin;

            switch (textBaseline)
            {
                case TextBaselines.Baseline:
                    baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y);
                    break;
                case TextBaselines.Top:
                    baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y + metrics.Top);
                    break;
                case TextBaselines.Bottom:
                    baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y + metrics.Bottom);
                    break;
                case TextBaselines.Middle:
                    baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y + (metrics.Top - metrics.Bottom) * 0.5 + metrics.Bottom);
                    break;
            }

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                TrueTypeFile.TrueTypePoint[][] glyphPaths = font.FontFamily.TrueTypeFile.GetGlyphPath(c, font.FontSize);

                for (int j = 0; j < glyphPaths.Length; j++)
                {
                    for (int k = 0; k < glyphPaths[j].Length; k++)
                    {
                        if (k == 0)
                        {
                            this.MoveTo(glyphPaths[j][k].X + baselineOrigin.X, -glyphPaths[j][k].Y + baselineOrigin.Y);
                        }
                        else
                        {
                            if (glyphPaths[j][k].IsOnCurve)
                            {
                                this.LineTo(glyphPaths[j][k].X + baselineOrigin.X, -glyphPaths[j][k].Y + baselineOrigin.Y);
                            }
                            else
                            {
                                Point startPoint = this.Segments.Last().Point;
                                Point quadCtrl = new Point(glyphPaths[j][k].X + baselineOrigin.X, -glyphPaths[j][k].Y + baselineOrigin.Y);
                                Point endPoint = new Point(glyphPaths[j][k + 1].X + baselineOrigin.X, -glyphPaths[j][k + 1].Y + baselineOrigin.Y);


                                Point ctrl1 = new Point(startPoint.X / 3 + 2 * quadCtrl.X / 3, startPoint.Y / 3 + 2 * quadCtrl.Y / 3);
                                Point ctrl2 = new Point(endPoint.X / 3 + 2 * quadCtrl.X / 3, endPoint.Y / 3 + 2 * quadCtrl.Y / 3);

                                this.CubicBezierTo(ctrl1, ctrl2, endPoint);

                                k++;
                            }
                        }
                    }

                    this.Close();
                }

                baselineOrigin.X += font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) * font.FontSize / 1000;
            }
            return this;
        }

        /// <summary>
        /// Adds a smooth spline composed of cubic bezier segments that pass through the specified points.
        /// </summary>
        /// <param name="points">The points through which the spline should pass.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath AddSmoothSpline(params Point[] points)
        {
            if (points.Length == 0)
            {
                return this;
            }
            else if (points.Length == 1)
            {
                return this.MoveTo(points[0]);
            }
            else if (points.Length == 2)
            {
                return this.MoveTo(points[0]).LineTo(points[1]);
            }

            Point[] smoothedSpline = SmoothSpline.SmoothSplines(points);

            this.MoveTo(smoothedSpline[0]);

            for (int i = 1; i < smoothedSpline.Length; i += 3)
            {
                this.CubicBezierTo(smoothedSpline[i], smoothedSpline[i + 1], smoothedSpline[i + 2]);
            }

            return this;
        }

    }

}
