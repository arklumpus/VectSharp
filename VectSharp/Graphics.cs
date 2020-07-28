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

    /// <summary>
    /// Represent text baselines.
    /// </summary>
    public enum TextBaselines
    {
        /// <summary>
        /// The current vertical coordinate determines where the top of the text string will be placed.
        /// </summary>
        Top,

        /// <summary>
        /// The current vertical coordinate determines where the bottom of the text string will be placed.
        /// </summary>
        Bottom,

        /// <summary>
        /// The current vertical coordinate determines where the middle of the text string will be placed.
        /// </summary>
        Middle,

        /// <summary>
        /// The current vertical coordinate determines where the baseline of the text string will be placed.
        /// </summary>
        Baseline
    }

    /// <summary>
    /// Represents text anchors.
    /// </summary>
    public enum TextAnchors
    {
        /// <summary>
        /// The current coordinate will determine the position of the left side of the text string.
        /// </summary>
        Left,

        /// <summary>
        /// The current coordinate will determine the position of the center of the text string.
        /// </summary>
        Center,

        /// <summary>
        /// The current coordinate will determine the position of the right side of the text string.
        /// </summary>
        Right
    }

    /// <summary>
    /// Represents line caps.
    /// </summary>
    public enum LineCaps
    {
        /// <summary>
        /// The ends of the line are squared off at the endpoints.
        /// </summary>
        Butt = 0,

        /// <summary>
        /// The ends of the lines are rounded.
        /// </summary>
        Round = 1,

        /// <summary>
        /// The ends of the lines are squared off by adding an half square box at each end.
        /// </summary>
        Square = 2
    }

    /// <summary>
    /// Represents line joining options.
    /// </summary>
    public enum LineJoins
    {
        /// <summary>
        /// Consecutive segments are joined by straight corners.
        /// </summary>
        Bevel = 2,

        /// <summary>
        /// Consecutive segments are joined by extending their outside edges until they meet.
        /// </summary>
        Miter = 0,

        /// <summary>
        /// Consecutive segments are joined by arc segments.
        /// </summary>
        Round = 1
    }

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
    public partial struct Colour : IEquatable<Colour>
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
        /// <returns>A <see cref="Colour"/> struct with the specified components and an alpha component of 1.</returns>
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
        /// <returns>A <see cref="Colour"/> struct with the specified components and an alpha component of 1.</returns>
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
        /// <returns>A <see cref="Colour"/> struct with the specified components and an alpha component of 1.</returns>
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
        /// <returns>A <see cref="Colour"/> struct with the specified components.</returns>
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
        /// <returns>A <see cref="Colour"/><see cref="Colour"/> struct with the specified components.</returns>
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
        /// <returns>A <see cref="Colour"/> struct with the specified components.</returns>
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
        /// <returns>A <see cref="Colour"/> struct with the specified components.</returns>
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
        /// <returns>A <see cref="Colour"/> struct with the specified components.</returns>
        public static Colour FromRgba(int r, int g, int b, double a)
        {
            return new Colour(r / 255.0, g / 255.0, b / 255.0, a);
        }

        /// <summary>
        /// Create a new colour from RGBA (red, green, blue and alpha) values.
        /// </summary>
        /// <param name="colour">A <see cref="ValueTuple{Int32, Int32, Int32, Double}"/> containing component information for the colour. For r, g, and b, range: [0, 255]; for a, range: [0, 1].</param>
        /// <returns>A <see cref="Colour"/> struct with the specified components.</returns>
        public static Colour FromRgba((int r, int g, int b, double a) colour)
        {
            return new Colour(colour.r / 255.0, colour.g / 255.0, colour.b / 255.0, colour.a);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool Equals(Colour col)
        {
            return col.R == this.R && col.G == this.G && col.B == this.B && col.A == this.A;
        }

        /// <inheritdoc/>
        public static bool operator ==(Colour col1, Colour col2)
        {
            return col1.R == col2.R && col1.G == col2.G && col1.B == col2.B && col1.A == col2.A;
        }

        /// <inheritdoc/>
        public static bool operator !=(Colour col1, Colour col2)
        {
            return col1.R != col2.R || col1.G != col2.G || col1.B != col2.B || col1.A != col2.A;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (int)(this.R * 255 + this.G * 255 * 255 + this.B * 255 * 255 * 255 + this.A * 255 * 255 * 255 * 255);
        }

        /// <summary>
        /// Convert the <see cref="Colour"/> object into a hex string that is constituted by a "#" followed by two-digit hexadecimal representations of the red, green and blue components of the colour (in the range 0x00 - 0xFF).
        /// Optionally also includes opacity (alpha channel) data.
        /// </summary>
        /// <param name="includeAlpha">Whether two additional hex digits representing the colour's opacity (alpha channel) should be included in the string.</param>
        /// <returns>A hex colour string.</returns>
        public string ToCSSString(bool includeAlpha)
        {
            if (includeAlpha)
            {
                return "#" + ((int)Math.Round(this.R * 255)).ToString("X2") + ((int)Math.Round(this.G * 255)).ToString("X2") + ((int)Math.Round(this.B * 255)).ToString("X2") + ((int)Math.Round(this.A * 255)).ToString("X2");
            }
            else
            {
                return "#" + ((int)Math.Round(this.R * 255)).ToString("X2") + ((int)Math.Round(this.G * 255)).ToString("X2") + ((int)Math.Round(this.B * 255)).ToString("X2");
            }
        }

        /// <summary>
        /// Convert a CSS colour string into a <see cref="Colour"/> object.
        /// </summary>
        /// <param name="cssString">The CSS colour string. In addition to 148 standard colour names (case-insensitive), #RGB, #RGBA, #RRGGBB and #RRGGBBAA hex strings and rgb(r, g, b) and rgba(r, g, b, a) functional colour notations are supported.</param>
        /// <returns></returns>
        public static Colour? FromCSSString(string cssString)
        {
            if (cssString.StartsWith("#"))
            {
                cssString = cssString.Substring(1);

                if (cssString.Length == 3)
                {
                    byte r = byte.Parse(cssString.Substring(0, 1) + cssString.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                    byte g = byte.Parse(cssString.Substring(1, 1) + cssString.Substring(1, 1), System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(cssString.Substring(2, 1) + cssString.Substring(2, 1), System.Globalization.NumberStyles.HexNumber);

                    return Colour.FromRgb(r, g, b);
                }
                else if (cssString.Length == 4)
                {
                    byte r = byte.Parse(cssString.Substring(0, 1) + cssString.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                    byte g = byte.Parse(cssString.Substring(1, 1) + cssString.Substring(1, 1), System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(cssString.Substring(2, 1) + cssString.Substring(2, 1), System.Globalization.NumberStyles.HexNumber);
                    byte a = byte.Parse(cssString.Substring(3, 1) + cssString.Substring(3, 1), System.Globalization.NumberStyles.HexNumber);

                    return Colour.FromRgba(r, g, b, a);
                }
                else if (cssString.Length == 6)
                {
                    byte r = byte.Parse(cssString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    byte g = byte.Parse(cssString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(cssString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

                    return Colour.FromRgb(r, g, b);
                }
                else if (cssString.Length == 8)
                {
                    byte r = byte.Parse(cssString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    byte g = byte.Parse(cssString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(cssString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    byte a = byte.Parse(cssString.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

                    return Colour.FromRgba(r, g, b, a);
                }
                else
                {
                    return null;
                }
            }
            else if (cssString.StartsWith("rgb(") || cssString.StartsWith("rgba("))
            {
                try
                {
                    cssString = cssString.Substring(cssString.IndexOf("(") + 1).Replace(")", "").Replace(" ", "");
                    string[] splitCssString = cssString.Split(',');

                    double R = ParseColourValueOrPercentage(splitCssString[0]);
                    double G = ParseColourValueOrPercentage(splitCssString[1]);
                    double B = ParseColourValueOrPercentage(splitCssString[2]);

                    double A = 1;

                    if (splitCssString.Length == 4)
                    {
                        A = double.Parse(splitCssString[3], System.Globalization.CultureInfo.InvariantCulture);
                    }

                    return Colour.FromRgba(R, G, B, A);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if (StandardColours.TryGetValue(cssString, out Colour tbr))
                {
                    return tbr;
                }
                else
                {
                    return null;
                }
            }
        }

        private static double ParseColourValueOrPercentage(string value)
        {
            if (int.TryParse(value, out int tbr))
            {
                return tbr / 255.0;
            }
            else if (value.Contains("%"))
            {
                return double.Parse(value.Replace("%", ""), System.Globalization.CultureInfo.InvariantCulture) / 100.0;
            }
            else
            {
                return double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Create a new <see cref="Colour"/> with the same RGB components as the <paramref name="original"/> <see cref="Colour"/>, but with the specified <paramref name="alpha"/>.
        /// </summary>
        /// <param name="original">The original <see cref="Colour"/> from which the RGB components will be taken.</param>
        /// <param name="alpha">The alpha component of the new <see cref="Colour"/>.</param>
        /// <returns>A <see cref="Colour"/> struct with the same RGB components as the <paramref name="original"/> <see cref="Colour"/> and the specified <paramref name="alpha"/>.</returns>
        public static Colour WithAlpha(Colour original, double alpha)
        {
            return Colour.FromRgba(original.R, original.G, original.B, alpha);
        }

        /// <summary>
        /// Create a new <see cref="Colour"/> with the same RGB components as the <paramref name="original"/> <see cref="Colour"/>, but with the specified <paramref name="alpha"/>.
        /// </summary>
        /// <param name="original">The original <see cref="Colour"/> from which the RGB components will be taken.</param>
        /// <param name="alpha">The alpha component of the new <see cref="Colour"/>.</param>
        /// <returns>A <see cref="Colour"/> struct with the same RGB components as the <paramref name="original"/> <see cref="Colour"/> and the specified <paramref name="alpha"/>.</returns>
        public static Colour WithAlpha(Colour original, byte alpha)
        {
            return Colour.FromRgba(original.R, original.G, original.B, (double)alpha / 255.0);
        }

        /// <summary>
        /// Create a new <see cref="Colour"/> with the same RGB components as the current <see cref="Colour"/>, but with the specified <paramref name="alpha"/>.
        /// </summary>
        /// <param name="alpha">The alpha component of the new <see cref="Colour"/>.</param>
        /// <returns>A <see cref="Colour"/> struct with the same RGB components as the current <see cref="Colour"/> and the specified <paramref name="alpha"/>.</returns>
        public Colour WithAlpha(double alpha)
        {
            return Colour.FromRgba(this.R, this.G, this.B, alpha);
        }

        /// <summary>
        /// Create a new <see cref="Colour"/> with the same RGB components as the current <see cref="Colour"/>, but with the specified <paramref name="alpha"/>.
        /// </summary>
        /// <param name="alpha">The alpha component of the new <see cref="Colour"/>.</param>
        /// <returns>A <see cref="Colour"/> struct with the same RGB components as the current <see cref="Colour"/> and the specified <paramref name="alpha"/>.</returns>
        public Colour WithAlpha(byte alpha)
        {
            return Colour.FromRgba(this.R, this.G, this.B, (double)alpha / 255.0);
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
            /// Height of the tallest glyph in the string over the baseline. Always &gt;= 0.
            /// </summary>
            public double Top { get; }

            /// <summary>
            /// Depth of the deepest glyph in the string below the baseline. Always &lt;= 0.
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
        /// Maximum height over the baseline of the usual glyphs in the font (there may be glyphs taller than this). Always &gt;= 0.
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
        /// Maximum depth below the baseline of the usual glyphs in the font (there may be glyphs deeper than this). Always &lt;= 0.
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
        /// Absolute maximum height over the baseline of the glyphs in the font. Always &gt;= 0.
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
        /// Absolute maximum depth below the baseline of the glyphs in the font. Always &lt;= 0.
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
            /// <summary>
            /// Serif normal regular face.
            /// </summary>
            TimesRoman,

            /// <summary>
            /// Serif bold regular face.
            /// </summary>
            TimesBold,

            /// <summary>
            /// Serif normal italic face.
            /// </summary>
            TimesItalic,

            /// <summary>
            /// Serif bold italic face.
            /// </summary>
            TimesBoldItalic,

            /// <summary>
            /// Sans-serif normal regular face.
            /// </summary>
            Helvetica,

            /// <summary>
            /// Sans-serif bold regular face.
            /// </summary>
            HelveticaBold,

            /// <summary>
            /// Sans-serif normal oblique face.
            /// </summary>
            HelveticaOblique,

            /// <summary>
            /// Sans-serif bold oblique face.
            /// </summary>
            HelveticaBoldOblique,

            /// <summary>
            /// Monospace normal regular face.
            /// </summary>
            Courier,

            /// <summary>
            /// Monospace bold regular face.
            /// </summary>
            CourierBold,

            /// <summary>
            /// Monospace normal oblique face.
            /// </summary>
            CourierOblique,

            /// <summary>
            /// Monospace bold oblique face.
            /// </summary>
            CourierBoldOblique,

            /// <summary>
            /// Symbol font.
            /// </summary>
            Symbol,

            /// <summary>
            /// Dingbat font.
            /// </summary>
            ZapfDingbats
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
        /// Create a new <see cref="FontFamily"/>.
        /// </summary>
        /// <param name="ttfStream">A stream containing a file in TTF format.</param>
        public FontFamily(Stream ttfStream)
        {
            IsStandardFamily = false;

            TrueTypeFile = TrueTypeFile.CreateTrueTypeFile(ttfStream);

            FileName = TrueTypeFile.GetFontFamilyName();
            this.IsBold = TrueTypeFile.IsBold();
            this.IsItalic = TrueTypeFile.IsItalic();
            this.IsOblique = TrueTypeFile.IsOblique();
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

        /// <summary>
        /// Computes the modulus of the vector represented by the <see cref="Point"/>.
        /// </summary>
        /// <returns>The modulus of the vector represented by the <see cref="Point"/>.</returns>
        public double Modulus()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        /// <summary>
        /// Normalises a <see cref="Point"/>.
        /// </summary>
        /// <returns>The normalised <see cref="Point"/>.</returns>
        public Point Normalize()
        {
            double mod = Modulus();
            return new Point(X / mod, Y / mod);
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
        /// <summary>
        /// The segment represents a move from the current point to a new point.
        /// </summary>
        Move,

        /// <summary>
        /// The segment represents a straight line from the current point to a new point.
        /// </summary>
        Line,

        /// <summary>
        /// The segment represents a cubic bezier curve from the current point to a new point.
        /// </summary>
        CubicBezier,

        /// <summary>
        /// The segment represents a circular arc from the current point to a new point.
        /// </summary>
        Arc,

        /// <summary>
        /// The segment represents the closing segment of a figure.
        /// </summary>
        Close
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
        /// <returns>A copy of the <see cref="Segment"/>.</returns>
        public abstract Segment Clone();

        /// <summary>
        /// Computes the length of the <see cref="Segment"/>.
        /// </summary>
        /// <param name="previousPoint">The point from which the <see cref="Segment"/> starts (i.e. the endpoint of the previous <see cref="Segment"/>).</param>
        /// <returns>The length of the segment.</returns>
        public abstract double Measure(Point previousPoint);

        /// <summary>
        /// Gets the point on the <see cref="Segment"/> at the specified (relative) <paramref name="position"/>).
        /// </summary>
        /// <param name="previousPoint">The point from which the <see cref="Segment"/> starts (i.e. the endpoint of the previous <see cref="Segment"/>).</param>
        /// <param name="position">The relative position on the <see cref="Segment"/> (0 is the start of the <see cref="Segment"/>, 1 is the end of the <see cref="Segment"/>).</param>
        /// <returns>The point at the specified position.</returns>
        public abstract Point GetPointAt(Point previousPoint, double position);

        /// <summary>
        /// Gets the tangent to the <see cref="Segment"/> at the specified (relative) <paramref name="position"/>).
        /// </summary>
        /// <param name="previousPoint">The point from which the <see cref="Segment"/> starts (i.e. the endpoint of the previous <see cref="Segment"/>).</param>
        /// <param name="position">The relative position on the <see cref="Segment"/> (0 is the start of the <see cref="Segment"/>, 1 is the end of the <see cref="Segment"/>).</param>
        /// <returns>The tangent to the point at the specified position.</returns>
        public abstract Point GetTangentAt(Point previousPoint, double position);
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

        public override double Measure(Point previousPoint)
        {
            return 0;
        }

        public override Point GetPointAt(Point previousPoint, double position)
        {
            throw new InvalidOperationException();
        }

        public override Point GetTangentAt(Point previousPoint, double position)
        {
            throw new InvalidOperationException();
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

        private double cachedLength = double.NaN;

        public override double Measure(Point previousPoint)
        {
            if (double.IsNaN(cachedLength))
            {
                cachedLength = Math.Sqrt((this.Point.X - previousPoint.X) * (this.Point.X - previousPoint.X) + (this.Point.Y - previousPoint.Y) * (this.Point.Y - previousPoint.Y));
            }

            return cachedLength;
        }

        public override Point GetPointAt(Point previousPoint, double position)
        {
            return new Point(previousPoint.X * (1 - position) + this.Point.X * position, previousPoint.Y * (1 - position) + this.Point.Y * position);
        }

        public override Point GetTangentAt(Point previousPoint, double position)
        {
            return new Point(this.Point.X - previousPoint.X, this.Point.Y - previousPoint.Y).Normalize();
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

        public override double Measure(Point previousPoint)
        {
            return 0;
        }

        public override Point GetPointAt(Point previousPoint, double position)
        {
            throw new InvalidOperationException();
        }

        public override Point GetTangentAt(Point previousPoint, double position)
        {
            throw new InvalidOperationException();
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

        private double cachedLength = double.NaN;
        private int cachedSegments = -1;

        public override double Measure(Point previousPoint)
        {
            if (double.IsNaN(cachedLength))
            {
                int segments = 16;
                double prevLength = 0;
                double currLength = Measure(previousPoint, segments);

                while (currLength > 0.00001 && Math.Abs(currLength - prevLength) / currLength > 0.0001)
                {
                    segments *= 2;
                    prevLength = currLength;
                    currLength = Measure(previousPoint, segments);
                }

                cachedSegments = segments;

                cachedLength = currLength;
            }

            return cachedLength;
        }

        private Point GetBezierPointAt(Point previousPoint, double position)
        {
            if (position <= 1 && position >= 0)
            {
                return new Point(
                this.Points[2].X * position * position * position + 3 * this.Points[1].X * position * position * (1 - position) + 3 * this.Points[0].X * position * (1 - position) * (1 - position) + previousPoint.X * (1 - position) * (1 - position) * (1 - position),
                this.Points[2].Y * position * position * position + 3 * this.Points[1].Y * position * position * (1 - position) + 3 * this.Points[0].Y * position * (1 - position) * (1 - position) + previousPoint.Y * (1 - position) * (1 - position) * (1 - position)
                );
            }
            else if (position > 1)
            {
                Point tangent = GetBezierTangentAt(previousPoint, 1);

                double excessLength = (position - 1) * this.Measure(previousPoint);

                return new Point(this.Point.X + tangent.X * excessLength, this.Point.Y + tangent.Y * excessLength);
            }
            else
            {
                Point tangent = GetBezierTangentAt(previousPoint, 0);

                return new Point(previousPoint.X + tangent.X * position * this.Measure(previousPoint), previousPoint.Y + tangent.Y * position * this.Measure(previousPoint));
            }
        }

        public override Point GetPointAt(Point previousPoint, double position)
        {
            double t = GetTFromPosition(previousPoint, position);
            return this.GetBezierPointAt(previousPoint, t);
        }

        public override Point GetTangentAt(Point previousPoint, double position)
        {
            double t = GetTFromPosition(previousPoint, position);
            return this.GetBezierTangentAt(previousPoint, t);
        }

        private double Measure(Point startPoint, int segments)
        {
            double delta = 1.0 / segments;

            double tbr = 0;

            for (int i = 1; i < segments; i++)
            {
                Point p1 = GetBezierPointAt(startPoint, delta * (i - 1));
                Point p2 = GetBezierPointAt(startPoint, delta * i);

                tbr += Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            }

            return tbr;
        }

        private double Measure(Point startPoint, int segments, double maxT)
        {
            double delta = maxT / segments;

            double tbr = 0;

            for (int i = 1; i < segments; i++)
            {
                Point p1 = GetBezierPointAt(startPoint, delta * (i - 1));
                Point p2 = GetBezierPointAt(startPoint, delta * i);

                tbr += Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            }

            return tbr;
        }

        private Point GetBezierTangentAt(Point previousPoint, double position)
        {
            if (position <= 1 && position >= 0)
            {
                return new Point(
                    3 * this.Points[2].X * position * position +
                    3 * this.Points[1].X * position * (2 - 3 * position) +
                    3 * this.Points[0].X * (3 * position * position - 4 * position + 1) +
                    -3 * previousPoint.X * (1 - position) * (1 - position),

                    3 * this.Points[2].Y * position * position +
                    3 * this.Points[1].Y * position * (2 - 3 * position) +
                    3 * this.Points[0].Y * (3 * position * position - 4 * position + 1) +
                    -3 * previousPoint.Y * (1 - position) * (1 - position)).Normalize();
            }
            else if (position > 1)
            {
                return GetBezierTangentAt(previousPoint, 1);
            }
            else
            {
                return GetBezierTangentAt(previousPoint, 0);
            }
        }

        private double GetTFromPosition(Point previousPoint, double position)
        {
            if (position <= 0 || position >= 1)
            {
                return position;
            }
            else
            {
                double length = this.Measure(previousPoint);

                double lowerBound = 0;
                double upperBound = 0.5;

                double lowerPos = 0;
                double upperPos = Measure(previousPoint, (int)Math.Ceiling(this.cachedSegments * upperBound), upperBound) / length;

                if (upperPos < position)
                {
                    lowerBound = upperBound;
                    lowerPos = upperPos;

                    upperBound = 1;
                    upperPos = 1;
                }

                while (Math.Min(upperPos - position, position - lowerPos) > 0.001)
                {
                    double mid = (lowerBound + upperBound) * 0.5;
                    double midPos = Measure(previousPoint, (int)Math.Ceiling(this.cachedSegments * mid), mid) / length;

                    if (midPos > position)
                    {
                        upperBound = mid;
                        upperPos = midPos;
                    }
                    else
                    {
                        lowerBound = mid;
                        lowerPos = midPos;
                    }
                }

                return lowerBound + (position - lowerPos) / (upperPos - lowerPos) * (upperBound - lowerBound);
            }
        }
    }

    internal class ArcSegment : Segment
    {
        public override SegmentType Type => SegmentType.Arc;

        public Segment[] ToBezierSegments()
        {
            List<Segment> tbr = new List<Segment>();

            if (EndAngle > StartAngle)
            {
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
            }
            else if (EndAngle < StartAngle)
            {
                Point startPoint = new Point(Points[0].X + Radius * Math.Cos(EndAngle), Points[0].Y + Radius * Math.Sin(EndAngle));
                if (StartAngle - EndAngle <= Math.PI / 2)
                {
                    tbr.AddRange(GetBezierSegment(Points[0].X, Points[0].Y, Radius, EndAngle, StartAngle, true));
                }
                else
                {
                    int count = (int)Math.Ceiling(2 * (StartAngle - EndAngle) / Math.PI);
                    double angle = EndAngle;

                    for (int i = 0; i < count; i++)
                    {
                        tbr.AddRange(GetBezierSegment(Points[0].X, Points[0].Y, Radius, angle, angle + (StartAngle - EndAngle) / count, i == 0));
                        angle += (StartAngle - EndAngle) / count;
                    }
                }

                return ReverseSegments(tbr, startPoint).ToArray();
            }

            return tbr.ToArray();
        }

        private static Segment[] ReverseSegments(IReadOnlyList<Segment> originalSegments, Point startPoint)
        {
            List<Segment> tbr = new List<Segment>(originalSegments.Count);

            for (int i = originalSegments.Count - 1; i >= 0; i--)
            {
                switch (originalSegments[i].Type)
                {
                    case SegmentType.Line:
                        if (i > 0)
                        {
                            tbr.Add(new LineSegment(originalSegments[i - 1].Point));
                        }
                        else
                        {
                            tbr.Add(new LineSegment(startPoint));
                        }
                        break;
                    case SegmentType.CubicBezier:
                        CubicBezierSegment originalSegment = (CubicBezierSegment)originalSegments[i];
                        if (i > 0)
                        {
                            tbr.Add(new CubicBezierSegment(originalSegment.Points[1], originalSegment.Points[0], originalSegments[i - 1].Point));
                        }
                        else
                        {
                            tbr.Add(new CubicBezierSegment(originalSegment.Points[1], originalSegment.Points[0], startPoint));
                        }
                        break;
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


        private double cachedLength = double.NaN;

        public override double Measure(Point previousPoint)
        {
            if (double.IsNaN(cachedLength))
            {
                Point arcStartPoint = new Point(this.Points[0].X + Math.Cos(StartAngle) * Radius, this.Points[0].Y + Math.Sin(StartAngle) * Radius);

                cachedLength = Radius * Math.Abs(EndAngle - StartAngle) + Math.Sqrt((arcStartPoint.X - previousPoint.X) * (arcStartPoint.X - previousPoint.X) + (arcStartPoint.Y - previousPoint.Y) * (arcStartPoint.Y - previousPoint.Y));
            }

            return cachedLength;
        }

        public override Point GetPointAt(Point previousPoint, double position)
        {
            double totalLength = this.Measure(previousPoint);
            double arcLength = Radius * Math.Abs(EndAngle - StartAngle);

            double preArc = (totalLength - arcLength) / totalLength;

            if (position < preArc)
            {
                if (position >= 0)
                {
                    double relPos = position / preArc;
                    Point arcStartPoint = new Point(this.Points[0].X + Math.Cos(StartAngle) * Radius, this.Points[0].Y + Math.Sin(StartAngle) * Radius);

                    return new Point(previousPoint.X * (1 - relPos) + arcStartPoint.X * relPos, previousPoint.Y * (1 - relPos) + arcStartPoint.Y * relPos);
                }
                else
                {
                    Point arcStartPoint = new Point(this.Points[0].X + Math.Cos(StartAngle) * Radius, this.Points[0].Y + Math.Sin(StartAngle) * Radius);
                    Point tangent = GetTangentAt(previousPoint, 0);
                    double excessLength = position * this.Measure(previousPoint);
                    return new Point(arcStartPoint.X + tangent.X * excessLength, arcStartPoint.Y + tangent.Y * excessLength);
                }
            }
            else
            {
                double relPos = position - preArc / (1 - preArc);

                if (relPos <= 1)
                {
                    double angle = StartAngle * (1 - relPos) + EndAngle * relPos;
                    return new Point(this.Points[0].X + Radius * Math.Cos(angle), this.Points[0].Y + Radius * Math.Sin(angle));
                }
                else
                {
                    Point arcEndPoint = this.Point;
                    Point tangent = GetTangentAt(previousPoint, 1);
                    double excessLength = (position - 1) * this.Measure(previousPoint);
                    return new Point(arcEndPoint.X + tangent.X * excessLength, arcEndPoint.Y + tangent.Y * excessLength);
                }
            }
        }

        public override Point GetTangentAt(Point previousPoint, double position)
        {
            double totalLength = this.Measure(previousPoint);
            double arcLength = Radius * Math.Abs(EndAngle - StartAngle);

            double preArc = (totalLength - arcLength) / totalLength;

            if (position < preArc)
            {
                Point arcStartPoint = new Point(this.Points[0].X + Math.Cos(StartAngle) * Radius, this.Points[0].Y + Math.Sin(StartAngle) * Radius);
                Point tang = new Point((arcStartPoint.X - previousPoint.X) * Math.Sign(EndAngle - StartAngle), (arcStartPoint.Y - previousPoint.Y) * Math.Sign(EndAngle - StartAngle)).Normalize();

                if (tang.Modulus() > 0.001)
                {
                    return tang.Normalize();
                }
                else
                {
                    return this.GetTangentAt(previousPoint, 0);
                }
            }
            else
            {
                double relPos = position - preArc / (1 - preArc);

                if (relPos <= 1)
                {
                    double angle = StartAngle * (1 - relPos) + EndAngle * relPos;
                    return new Point(-Math.Sin(angle) * Math.Sign(EndAngle - StartAngle), Math.Cos(angle) * Math.Sign(EndAngle - StartAngle));
                }
                else
                {
                    return new Point(-Math.Sin(EndAngle) * Math.Sign(EndAngle - StartAngle), Math.Cos(EndAngle) * Math.Sign(EndAngle - StartAngle));
                }
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
        /// Transform the coordinate system with the specified transformation matrix [ [a, c, e], [b, d, f], [0, 0, 1] ].
        /// </summary>
        /// <param name="a">The first element of the first column.</param>
        /// <param name="b">The second element of the first column.</param>
        /// <param name="c">The first element of the second column.</param>
        /// <param name="d">The second element of the second column.</param>
        /// <param name="e">The first element of the third column.</param>
        /// <param name="f">The second element of the third column.</param>
        void Transform(double a, double b, double c, double d, double e, double f);

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
        /// Set the current clipping path as the intersection of the previous clipping path and the current path.
        /// </summary>
        void SetClippingPath();

        /// <summary>
        /// Current colour used to fill paths.
        /// </summary>
        Colour FillStyle { get; }

        /// <summary>
        /// Set the current <see cref="FillStyle"/>.
        /// </summary>
        /// <param name="style">A <see cref="ValueTuple{Int32, Int32, Int32, Double}"/> containing component information for the colour. For r, g, and b, range: [0, 255]; for a, range: [0, 1].</param>
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
        /// <param name="style">A <see cref="ValueTuple{Int32, Int32, Int32, Double}"/> containing component information for the colour. For r, g, and b, range: [0, 255]; for a, range: [0, 1].</param>
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

        /// <summary>
        /// Draw a raster image.
        /// </summary>
        /// <param name="sourceX">The horizontal coordinate of the top-left corner of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceY">The vertical coordinate of the top-left corner of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceWidth">The width of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceHeight">The height of the rectangle delimiting the source area of the image.</param>
        /// <param name="destinationX">The horizontal coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationY">The vertical coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationWidth">The width of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationHeight">The height of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        void DrawRasterImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image);
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
            Actions.Add(new PathAction(path, fillColour, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag, false));
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
            Actions.Add(new PathAction(path, null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag, false));
        }

        /// <summary>
        /// Intersect the current clipping path with the specified <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> to intersect with the current clipping path.</param>
        public void SetClippingPath(GraphicsPath path)
        {
            Actions.Add(new PathAction(path, null, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, null, true));
        }

        /// <summary>
        /// Intersect the current clipping path with the specified rectangle.
        /// </summary>
        /// <param name="leftX">The horizontal coordinate of the top-left corner of the rectangle.</param>
        /// <param name="topY">The vertical coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public void SetClippingPath(double leftX, double topY, double width, double height)
        {
            SetClippingPath(new Point(leftX, topY), new Size(width, height));
        }

        /// <summary>
        /// Intersect the current clipping path with the specified rectangle.
        /// </summary>
        /// <param name="topLeft">The top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        public void SetClippingPath(Point topLeft, Size size)
        {
            Actions.Add(new PathAction(new GraphicsPath().MoveTo(topLeft).LineTo(topLeft.X + size.Width, topLeft.Y).LineTo(topLeft.X + size.Width, topLeft.Y + size.Height).LineTo(topLeft.X, topLeft.Y + size.Height).Close(), null, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, null, true));
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
        /// Transform the coordinate system with the specified transformation matrix [ [a, c, e], [b, d, f], [0, 0, 1] ].
        /// </summary>
        /// <param name="a">The first element of the first column.</param>
        /// <param name="b">The second element of the first column.</param>
        /// <param name="c">The first element of the second column.</param>
        /// <param name="d">The second element of the second column.</param>
        /// <param name="e">The first element of the third column.</param>
        /// <param name="f">The second element of the third column.</param>
        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            double[,] matrix = new double[,] { { a, c, e }, { b, d, f }, { 0, 0, 1 } };
            Actions.Add(new TransformAction(matrix));
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
        /// Draw a raster image.
        /// </summary>
        /// <param name="sourceX">The horizontal coordinate of the top-left corner of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceY">The vertical coordinate of the top-left corner of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceWidth">The width of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceHeight">The height of the rectangle delimiting the source area of the image.</param>
        /// <param name="destinationX">The horizontal coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationY">The vertical coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationWidth">The width of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationHeight">The height of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        /// <param name="tag">A tag to identify the drawn image.</param>
        public void DrawRasterImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image, string tag = null)
        {
            Actions.Add(new RasterImageAction(sourceX, sourceY, sourceWidth, sourceHeight, destinationX, destinationY, destinationWidth, destinationHeight, image, tag));
        }

        /// <summary>
        /// Draw a raster image.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="y">The vertical coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        /// <param name="tag">A tag to identify the drawn image.</param>
        public void DrawRasterImage(double x, double y, RasterImage image, string tag = null)
        {
            DrawRasterImage(0, 0, image.Width, image.Height, x, y, image.Width, image.Height, image, tag);
        }

        /// <summary>
        /// Draw a raster image.
        /// </summary>
        /// <param name="position">The the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        /// <param name="tag">A tag to identify the drawn image.</param>
        public void DrawRasterImage(Point position, RasterImage image, string tag = null)
        {
            DrawRasterImage(0, 0, image.Width, image.Height, position.X, position.Y, image.Width, image.Height, image, tag);
        }

        /// <summary>
        /// Draw a raster image.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="y">The vertical coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="width">The width of the rectangle delimiting the destination area of the image.</param>
        /// <param name="height">The height of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        /// <param name="tag">A tag to identify the drawn image.</param>
        public void DrawRasterImage(double x, double y, double width, double height, RasterImage image, string tag = null)
        {
            DrawRasterImage(0, 0, image.Width, image.Height, x, y, width, height, image, tag);
        }

        /// <summary>
        /// Draw a raster image.
        /// </summary>
        /// <param name="position">The the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="size">The size of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        /// <param name="tag">A tag to identify the drawn image.</param>
        public void DrawRasterImage(Point position, Size size, RasterImage image, string tag = null)
        {
            DrawRasterImage(0, 0, image.Width, image.Height, position.X, position.Y, size.Width, size.Height, image, tag);
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
        /// <param name="tag">A tag to identify the stroked text.</param>
        public void StrokeText(Point origin, string text, Font font, Colour strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new TextAction(origin, text, font, textBaseline, null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Stroke a text string.
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
        /// <param name="tag">A tag to identify the stroked text.</param>
        public void StrokeText(double originX, double originY, string text, Font font, Colour strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new TextAction(new Point(originX, originY), text, font, textBaseline, null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Fill a text string along a <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> along which the text will flow.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="fillColour">The colour to use to fill the text.</param>
        /// <param name="reference">The (relative) starting point on the path starting from which the text should be drawn (0 is the start of the path, 1 is the end of the path).</param>
        /// <param name="anchor">The anchor in the text string that will correspond to the point specified by the <paramref name="reference"/>.</param>
        /// <param name="textBaseline">The text baseline (determines which the position of the text in relation to the <paramref name="path"/>.</param>
        /// <param name="tag">A tag to identify the filled text.</param>
        public void FillTextOnPath(GraphicsPath path, string text, Font font, Colour fillColour, double reference = 0, TextAnchors anchor = TextAnchors.Left, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            double currDelta = 0;
            double pathLength = path.MeasureLength();

            Font.DetailedFontMetrics fullMetrics = font.MeasureTextAdvanced(text);

            switch (anchor)
            {
                case TextAnchors.Left:
                    break;
                case TextAnchors.Center:
                    currDelta = -fullMetrics.Width * 0.5 / pathLength;
                    break;
                case TextAnchors.Right:
                    currDelta = -fullMetrics.Width / pathLength;
                    break;
            }

            for (int i = 0; i < text.Length; i++)
            {
                string c = text.Substring(i, 1);

                Font.DetailedFontMetrics metrics = font.MeasureTextAdvanced(c);

                Point origin = path.GetPointAtRelative(reference + currDelta);

                Point tangent = path.GetTangentAtRelative(reference + currDelta + (metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing) / pathLength * 0.5);

                this.Save();

                this.Translate(origin);
                this.Rotate(Math.Atan2(tangent.Y, tangent.X));

                switch (textBaseline)
                {
                    case TextBaselines.Top:
                        if (i > 0)
                        {
                            this.FillText(new Point(metrics.LeftSideBearing, fullMetrics.Top), c, font, fillColour, textBaseline: TextBaselines.Baseline, tag);
                        }
                        else
                        {
                            this.FillText(new Point(0, fullMetrics.Top), c, font, fillColour, textBaseline: TextBaselines.Baseline, tag);
                        }
                        break;
                    case TextBaselines.Baseline:
                        if (i > 0)
                        {
                            this.FillText(new Point(metrics.LeftSideBearing, 0), c, font, fillColour, textBaseline: TextBaselines.Baseline, tag);
                        }
                        else
                        {
                            this.FillText(new Point(0, 0), c, font, fillColour, textBaseline: TextBaselines.Baseline, tag);
                        }
                        break;
                    case TextBaselines.Bottom:
                        if (i > 0)
                        {
                            this.FillText(new Point(metrics.LeftSideBearing, fullMetrics.Bottom), c, font, fillColour, textBaseline: TextBaselines.Baseline, tag);
                        }
                        else
                        {
                            this.FillText(new Point(0, fullMetrics.Bottom), c, font, fillColour, textBaseline: TextBaselines.Baseline, tag);
                        }
                        break;
                    case TextBaselines.Middle:
                        if (i > 0)
                        {
                            this.FillText(new Point(metrics.LeftSideBearing, fullMetrics.Bottom + fullMetrics.Height / 2), c, font, fillColour, textBaseline: TextBaselines.Baseline, tag);
                        }
                        else
                        {
                            this.FillText(new Point(0, fullMetrics.Bottom + fullMetrics.Height / 2), c, font, fillColour, textBaseline: TextBaselines.Baseline, tag);
                        }
                        break;
                }

                this.Restore();

                if (i > 0)
                {
                    currDelta += (metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing) / pathLength;
                }
                else
                {
                    currDelta += (metrics.Width + metrics.RightSideBearing) / pathLength;
                }
            }
        }

        /// <summary>
        /// Stroke a text string along a <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> along which the text will flow.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="strokeColour">The colour with which to stroke the text.</param>
        /// <param name="lineWidth">The width of the line with which the text is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the text.</param>
        /// <param name="lineJoin">The line join to use to stroke the text.</param>
        /// <param name="lineDash">The line dash to use to stroke the text.</param>
        /// <param name="reference">The (relative) starting point on the path starting from which the text should be drawn (0 is the start of the path, 1 is the end of the path).</param>
        /// <param name="anchor">The anchor in the text string that will correspond to the point specified by the <paramref name="reference"/>.</param>
        /// <param name="textBaseline">The text baseline (determines which the position of the text in relation to the <paramref name="path"/>.</param>
        /// <param name="tag">A tag to identify the stroked text.</param>
        public void StrokeTextOnPath(GraphicsPath path, string text, Font font, Colour strokeColour, double reference = 0, TextAnchors anchor = TextAnchors.Left, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            double currDelta = 0;
            double pathLength = path.MeasureLength();

            Font.DetailedFontMetrics fullMetrics = font.MeasureTextAdvanced(text);

            switch (anchor)
            {
                case TextAnchors.Left:
                    break;
                case TextAnchors.Center:
                    currDelta = -fullMetrics.Width * 0.5 / pathLength;
                    break;
                case TextAnchors.Right:
                    currDelta = -fullMetrics.Width / pathLength;
                    break;
            }

            for (int i = 0; i < text.Length; i++)
            {
                string c = text.Substring(i, 1);

                Font.DetailedFontMetrics metrics = font.MeasureTextAdvanced(c);

                Point origin = path.GetPointAtRelative(reference + currDelta);

                Point tangent = path.GetTangentAtRelative(reference + currDelta + (metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing) / pathLength * 0.5);

                this.Save();

                this.Translate(origin);
                this.Rotate(Math.Atan2(tangent.Y, tangent.X));

                switch (textBaseline)
                {
                    case TextBaselines.Top:
                        if (i > 0)
                        {
                            this.StrokeText(new Point(metrics.LeftSideBearing, fullMetrics.Top), c, font, strokeColour, textBaseline: TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                        }
                        else
                        {
                            this.StrokeText(new Point(0, fullMetrics.Top), c, font, strokeColour, textBaseline: TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                        }
                        break;
                    case TextBaselines.Baseline:
                        if (i > 0)
                        {
                            this.StrokeText(new Point(metrics.LeftSideBearing, 0), c, font, strokeColour, textBaseline: TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                        }
                        else
                        {
                            this.StrokeText(new Point(0, 0), c, font, strokeColour, textBaseline: TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                        }
                        break;
                    case TextBaselines.Bottom:
                        if (i > 0)
                        {
                            this.StrokeText(new Point(metrics.LeftSideBearing, fullMetrics.Bottom), c, font, strokeColour, textBaseline: TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                        }
                        else
                        {
                            this.StrokeText(new Point(0, fullMetrics.Bottom), c, font, strokeColour, textBaseline: TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                        }
                        break;
                    case TextBaselines.Middle:
                        if (i > 0)
                        {
                            this.StrokeText(new Point(metrics.LeftSideBearing, fullMetrics.Bottom + fullMetrics.Height / 2), c, font, strokeColour, textBaseline: TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                        }
                        else
                        {
                            this.StrokeText(new Point(0, fullMetrics.Bottom + fullMetrics.Height / 2), c, font, strokeColour, textBaseline: TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                        }
                        break;
                }

                this.Restore();

                if (i > 0)
                {
                    currDelta += (metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing) / pathLength;
                }
                else
                {
                    currDelta += (metrics.Width + metrics.RightSideBearing) / pathLength;
                }
            }
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

                    if (pth.IsClipping)
                    {
                        destinationContext.SetClippingPath();
                    }
                    else
                    {
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
                    else if (trf.Matrix != null)
                    {
                        destinationContext.Transform(trf.Matrix[0, 0], trf.Matrix[1, 0], trf.Matrix[0, 1], trf.Matrix[1, 1], trf.Matrix[0, 2], trf.Matrix[1, 2]);
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
                else if (this.Actions[i] is RasterImageAction)
                {
                    RasterImageAction img = this.Actions[i] as RasterImageAction;
                    destinationContext.DrawRasterImage(img.SourceX, img.SourceY, img.SourceWidth, img.SourceHeight, img.DestinationX, img.DestinationY, img.DestinationWidth, img.DestinationHeight, img.Image);
                }
            }
        }

        /// <summary>
        /// Draws a <see cref="Graphics"/> object on the current <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="origin">The point at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="graphics">The <see cref="Graphics"/> object to draw on the current <see cref="Graphics"/> object.</param>
        public void DrawGraphics(Point origin, Graphics graphics)
        {
            this.Save();
            this.Translate(origin);

            this.Actions.AddRange(graphics.Actions);

            this.Restore();
        }

        /// <summary>
        /// Draws a <see cref="Graphics"/> object on the current <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="originX">The horizontal coordinate at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="originY">The vertical coordinate at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="graphics">The <see cref="Graphics"/> object to draw on the current <see cref="Graphics"/> object.</param>
        public void DrawGraphics(double originX, double originY, Graphics graphics)
        {
            this.DrawGraphics(new Point(originX, originY), graphics);
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

        public double[,] Matrix { get; } = null;

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

        public TransformAction(double[,] matrix)
        {
            this.Matrix = matrix;
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
        public bool IsClipping { get; }
        public PathAction(GraphicsPath path, Colour? fill, Colour? stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, string tag, bool isClipping)
        {
            this.Path = path;
            this.Fill = fill;
            this.Stroke = stroke;
            this.LineCap = lineCap;
            this.LineJoin = lineJoin;
            this.LineWidth = lineWidth;
            this.LineDash = lineDash;
            this.Tag = tag;
            this.IsClipping = isClipping;
        }
    }

    internal class RasterImageAction : IGraphicsAction, IPrintableAction
    {
        public Colour? Fill { get; }
        public Colour? Stroke { get; }
        public string Tag { get; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public int SourceX { get; }
        public int SourceY { get; }
        public int SourceWidth { get; }
        public int SourceHeight { get; }
        public double DestinationX { get; }
        public double DestinationY { get; }
        public double DestinationWidth { get; }
        public double DestinationHeight { get; }
        public RasterImage Image { get; }

        public RasterImageAction(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image, string tag)
        {
            this.SourceX = sourceX;
            this.SourceY = sourceY;
            this.SourceWidth = sourceWidth;
            this.SourceHeight = sourceHeight;

            this.DestinationX = destinationX;
            this.DestinationY = destinationY;
            this.DestinationWidth = destinationWidth;
            this.DestinationHeight = destinationHeight;

            this.Image = image;
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
        /// Trace an arc from an ellipse with the specified radii, rotated by <paramref name="axisAngle"/> with respect to the x-axis, starting at the current point and ending at the <paramref name="endPoint"/>.
        /// </summary>
        /// <param name="radiusX">The horizontal radius of the ellipse.</param>
        /// <param name="radiusY">The vertical radius of the ellipse.</param>
        /// <param name="axisAngle">The angle of the horizontal axis of the ellipse with respect to the horizontal axis.</param>
        /// <param name="largeArc">Determines whether the large or the small arc is drawn.</param>
        /// <param name="sweepClockwise">Determines whether the clockwise or counterclockwise arc is drawn.</param>
        /// <param name="endPoint">The end point of the arc.</param>
        /// <returns></returns>
        public GraphicsPath EllipticalArc(double radiusX, double radiusY, double axisAngle, bool largeArc, bool sweepClockwise, Point endPoint)
        {
            double x1 = 0;
            double y1 = 0;

            if (this.Segments.Count > 0)
            {
                for (int i = this.Segments.Count - 1; i >= 0; i--)
                {
                    if (this.Segments[i].Type != SegmentType.Close)
                    {
                        x1 = this.Segments[i].Point.X;
                        y1 = this.Segments[i].Point.Y;
                        break;
                    }
                }
            }

            double x2 = endPoint.X;
            double y2 = endPoint.Y;

            double x1P = Math.Cos(axisAngle) * (x1 - x2) * 0.5 + Math.Sin(axisAngle) * (y1 - y2) * 0.5;

            if (Math.Abs(x1P) < 1e-7)
            {
                x1P = 0;
            }

            double y1P = -Math.Sin(axisAngle) * (x1 - x2) * 0.5 + Math.Cos(axisAngle) * (y1 - y2) * 0.5;

            if (Math.Abs(y1P) < 1e-7)
            {
                y1P = 0;
            }

            double sqrtTerm = (largeArc != sweepClockwise ? 1 : -1) * Math.Sqrt((radiusX * radiusX * radiusY * radiusY - radiusX * radiusX * y1P * y1P - radiusY * radiusY * x1P * x1P) / (radiusX * radiusX * y1P * y1P + radiusY * radiusY * x1P * x1P));

            double cXP = sqrtTerm * radiusX * y1P / radiusY;
            double cYP = -sqrtTerm * radiusY * x1P / radiusX;

            double cX = Math.Cos(axisAngle) * cXP - Math.Sin(axisAngle) * cYP + (x1 + x2) * 0.5;
            double cY = Math.Sin(axisAngle) * cXP + Math.Cos(axisAngle) * cYP + (y1 + y2) * 0.5;

            double theta1 = AngleVectors(1, 0, (x1P - cXP) / radiusX, (y1P - cYP) / radiusY);
            double deltaTheta = AngleVectors((x1P - cXP) / radiusX, (y1P - cYP) / radiusY, (-x1P - cXP) / radiusX, (-y1P - cYP) / radiusY) % (2 * Math.PI);

            if (!sweepClockwise && deltaTheta > 0)
            {
                deltaTheta -= 2 * Math.PI;
            }
            else if (sweepClockwise && deltaTheta < 0)
            {
                deltaTheta += 2 * Math.PI;
            }

            double r = Math.Min(radiusX, radiusY);

            ArcSegment arc = new ArcSegment(0, 0, r, theta1, theta1 + deltaTheta);

            Segment[] segments = arc.ToBezierSegments();

            for (int i = 0; i < segments.Length; i++)
            {
                for (int j = 0; j < segments[i].Points.Length; j++)
                {
                    double newX = segments[i].Points[j].X * radiusX / r;
                    double newY = segments[i].Points[j].Y * radiusY / r;

                    segments[i].Points[j] = new Point(newX * Math.Cos(axisAngle) - newY * Math.Sin(axisAngle) + cX, newX * Math.Sin(axisAngle) + newY * Math.Cos(axisAngle) + cY);
                }
            }

            this.Segments.AddRange(segments);

            return this;
        }

        private static double AngleVectors(double uX, double uY, double vX, double vY)
        {
            double tbr = Math.Acos((uX * vX + uY * vY) / Math.Sqrt((uX * uX + uY * uY) * (vX * vX + vY * vY)));
            double sign = Math.Sign(uX * vY - uY * vX);
            if (sign != 0)
            {
                tbr *= sign;
            }
            return tbr;
        }


        /// <summary>
        /// Trace a cubic Bezier curve from the current point to a destination point, with two control points.
        /// The current point is updated to the end point of the Bezier curve.
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
        /// The current point is updated to the end point of the Bezier curve.
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
        /// Add the contour of a text string flowing along a <see cref="GraphicsPath"/> to the current path.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> along which the text will flow.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="reference">The (relative) starting point on the path starting from which the text should be drawn (0 is the start of the path, 1 is the end of the path).</param>
        /// <param name="anchor">The anchor in the text string that will correspond to the point specified by the <paramref name="reference"/>.</param>
        /// <param name="textBaseline">The text baseline (determines which the position of the text in relation to the <paramref name="path"/>.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath AddTextOnPath(GraphicsPath path, string text, Font font, double reference = 0, TextAnchors anchor = TextAnchors.Left, TextBaselines textBaseline = TextBaselines.Top)
        {
            double currDelta = 0;
            double pathLength = path.MeasureLength();

            Font.DetailedFontMetrics fullMetrics = font.MeasureTextAdvanced(text);

            switch (anchor)
            {
                case TextAnchors.Left:
                    break;
                case TextAnchors.Center:
                    currDelta = -fullMetrics.Width * 0.5 / pathLength;
                    break;
                case TextAnchors.Right:
                    currDelta = -fullMetrics.Width / pathLength;
                    break;
            }

            for (int i = 0; i < text.Length; i++)
            {
                string c = text.Substring(i, 1);

                Font.DetailedFontMetrics metrics = font.MeasureTextAdvanced(c);

                Point origin = path.GetPointAtRelative(reference + currDelta);

                Point tangent = path.GetTangentAtRelative(reference + currDelta + (metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing) / pathLength * 0.5);

                GraphicsPath glyphPath = new GraphicsPath();

                switch (textBaseline)
                {
                    case TextBaselines.Top:
                        if (i > 0)
                        {
                            glyphPath.AddText(new Point(metrics.LeftSideBearing, fullMetrics.Top), c, font, textBaseline: TextBaselines.Baseline);
                        }
                        else
                        {
                            glyphPath.AddText(new Point(0, fullMetrics.Top), c, font, textBaseline: TextBaselines.Baseline);
                        }
                        break;
                    case TextBaselines.Baseline:
                        if (i > 0)
                        {
                            glyphPath.AddText(new Point(metrics.LeftSideBearing, 0), c, font, textBaseline: TextBaselines.Baseline);
                        }
                        else
                        {
                            glyphPath.AddText(new Point(0, 0), c, font, textBaseline: TextBaselines.Baseline);
                        }
                        break;
                    case TextBaselines.Bottom:
                        if (i > 0)
                        {
                            glyphPath.AddText(new Point(metrics.LeftSideBearing, fullMetrics.Bottom), c, font, textBaseline: TextBaselines.Baseline);
                        }
                        else
                        {
                            glyphPath.AddText(new Point(0, fullMetrics.Bottom), c, font, textBaseline: TextBaselines.Baseline);
                        }
                        break;
                    case TextBaselines.Middle:
                        if (i > 0)
                        {
                            glyphPath.AddText(new Point(metrics.LeftSideBearing, fullMetrics.Bottom + fullMetrics.Height / 2), c, font, textBaseline: TextBaselines.Baseline);
                        }
                        else
                        {
                            glyphPath.AddText(new Point(0, fullMetrics.Bottom + fullMetrics.Height / 2), c, font, textBaseline: TextBaselines.Baseline);
                        }
                        break;
                }

                double angle = Math.Atan2(tangent.Y, tangent.X);

                for (int j = 0; j < glyphPath.Segments.Count; j++)
                {
                    if (glyphPath.Segments[j].Points != null)
                    {
                        for (int k = 0; k < glyphPath.Segments[j].Points.Length; k++)
                        {
                            double newX = glyphPath.Segments[j].Points[k].X * Math.Cos(angle) - glyphPath.Segments[j].Points[k].Y * Math.Sin(angle) + origin.X;
                            double newY = glyphPath.Segments[j].Points[k].X * Math.Sin(angle) + glyphPath.Segments[j].Points[k].Y * Math.Cos(angle) + origin.Y;

                            glyphPath.Segments[j].Points[k] = new Point(newX, newY);
                        }
                    }

                    this.Segments.Add(glyphPath.Segments[j]);
                }

                if (i > 0)
                {
                    currDelta += (metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing) / pathLength;
                }
                else
                {
                    currDelta += (metrics.Width + metrics.RightSideBearing) / pathLength;
                }
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
                return this.LineTo(points[0]);
            }
            else if (points.Length == 2)
            {
                return this.LineTo(points[0]).LineTo(points[1]);
            }

            Point[] smoothedSpline = SmoothSpline.SmoothSplines(points);

            this.LineTo(smoothedSpline[0]);

            for (int i = 1; i < smoothedSpline.Length; i += 3)
            {
                this.CubicBezierTo(smoothedSpline[i], smoothedSpline[i + 1], smoothedSpline[i + 2]);
            }

            return this;
        }

        private double cachedLength = double.NaN;

        /// <summary>
        /// Measures the length of the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <returns>The length of the <see cref="GraphicsPath"/></returns>
        public double MeasureLength()
        {
            if (double.IsNaN(cachedLength))
            {
                cachedLength = 0;
                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                cachedLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                cachedLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                cachedLength += this.Segments[i].Measure(figureStartPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Close:
                            cachedLength += Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));
                            currPoint = figureStartPoint;
                            break;
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                cachedLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                cachedLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                    }
                }
            }

            return cachedLength;
        }

        /// <summary>
        /// Gets the point at the relative position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="position">The position on the <see cref="GraphicsPath"/> (0 is the start of the path, 1 is the end of the path).</param>
        /// <returns>The point at the specified position.</returns>
        public Point GetPointAtRelative(double position)
        {
            return GetPointAtAbsolute(position * this.MeasureLength());
        }

        /// <summary>
        /// Gets the point at the absolute position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="length">The distance to the point from the start of the <see cref="GraphicsPath"/>.</param>
        /// <returns>The point at the specified position.</returns>
        public Point GetPointAtAbsolute(double length)
        {
            double pathLength = this.MeasureLength();

            if (length >= 0 && length <= pathLength)
            {
                double currLen = 0;

                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetPointAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetPointAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currPoint = figureStartPoint;

                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetPointAt(currPoint, pos);
                                }
                            }
                            break;
                        case SegmentType.Close:
                            {
                                double segLength = Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return new Point(currPoint.X * (1 - pos) + figureStartPoint.X * pos, currPoint.Y * (1 - pos) + figureStartPoint.Y * pos);
                                }
                            }
                            break;
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetPointAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetPointAt(currPoint, pos);
                                }
                            }
                            break;
                    }
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
            else if (length > pathLength)
            {
                double currLength = 0;

                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count - 1; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currLength += this.Segments[i].Measure(figureStartPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Close:
                            currLength += Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));
                            currPoint = figureStartPoint;
                            break;
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                    }
                }

                switch (this.Segments[this.Segments.Count - 1].Type)
                {
                    case SegmentType.Arc:
                    case SegmentType.CubicBezier:
                    case SegmentType.Line:
                        {
                            double pos = 1 + (length - pathLength) / this.Segments[this.Segments.Count - 1].Measure(currPoint);
                            return this.Segments[this.Segments.Count - 1].GetPointAt(currPoint, pos);
                        }
                    case SegmentType.Move:
                        return currPoint;
                    case SegmentType.Close:
                        return this.GetPointAtAbsolute(length - pathLength);
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
            else
            {
                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetPointAt(currPoint, pos);
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetPointAt(currPoint, pos);
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currPoint = figureStartPoint;

                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetPointAt(currPoint, pos);
                            }
                        case SegmentType.Close:
                            {
                                double segLength = Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));
                                double pos = length / segLength;
                                return new Point(currPoint.X * (1 - pos) + figureStartPoint.X * pos, currPoint.Y * (1 - pos) + figureStartPoint.Y * pos);
                            }
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetPointAt(currPoint, pos);
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetPointAt(currPoint, pos);
                            }
                    }
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
        }

        /// <summary>
        /// Gets the tangent to the point at the relative position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="position">The position on the <see cref="GraphicsPath"/> (0 is the start of the path, 1 is the end of the path).</param>
        /// <returns>The tangent to the point at the specified position.</returns>
        public Point GetTangentAtRelative(double position)
        {
            return GetTangentAtAbsolute(position * this.MeasureLength());
        }

        /// <summary>
        /// Gets the tangent to the point at the absolute position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="length">The distance to the point from the start of the <see cref="GraphicsPath"/>.</param>
        /// <returns>The tangent to the point at the specified position.</returns>
        public Point GetTangentAtAbsolute(double length)
        {
            double pathLength = this.MeasureLength();

            if (length >= 0 && length <= pathLength)
            {
                double currLen = 0;

                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetTangentAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetTangentAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currPoint = figureStartPoint;

                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetTangentAt(currPoint, pos);
                                }
                            }
                            break;
                        case SegmentType.Close:
                            {
                                double segLength = Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return new Point(figureStartPoint.X - currPoint.X, figureStartPoint.Y - currPoint.Y).Normalize();
                                }
                            }
                            break;
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetTangentAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetTangentAt(currPoint, pos);
                                }
                            }
                            break;
                    }
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
            else if (length > pathLength)
            {
                double currLength = 0;

                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count - 1; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currLength += this.Segments[i].Measure(figureStartPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Close:
                            currLength += Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));
                            currPoint = figureStartPoint;
                            break;
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                    }
                }

                switch (this.Segments[this.Segments.Count - 1].Type)
                {
                    case SegmentType.Arc:
                    case SegmentType.CubicBezier:
                    case SegmentType.Line:
                        {
                            double pos = 1 + (length - pathLength) / this.Segments[this.Segments.Count - 1].Measure(currPoint);
                            return this.Segments[this.Segments.Count - 1].GetTangentAt(currPoint, pos);
                        }
                    case SegmentType.Move:
                        return new Point();
                    case SegmentType.Close:
                        return this.GetTangentAtAbsolute(length - pathLength);
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
            else
            {
                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetTangentAt(currPoint, pos);
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetTangentAt(currPoint, pos);
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currPoint = figureStartPoint;

                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetTangentAt(currPoint, pos);
                            }
                        case SegmentType.Close:
                            {
                                double segLength = Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));
                                double pos = length / segLength;
                                return new Point(figureStartPoint.X - currPoint.X, figureStartPoint.Y - currPoint.Y).Normalize();
                            }
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetTangentAt(currPoint, pos);
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetTangentAt(currPoint, pos);
                            }
                    }
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
        }

        /// <summary>
        /// Gets the normal to the point at the absolute position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="length">The distance to the point from the start of the <see cref="GraphicsPath"/>.</param>
        /// <returns>The normal to the point at the specified position.</returns>
        public Point GetNormalAtAbsolute(double length)
        {
            Point tangent = this.GetTangentAtAbsolute(length);
            return new Point(-tangent.Y, tangent.X);
        }

        /// <summary>
        /// Gets the normal to the point at the relative position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="position">The position on the <see cref="GraphicsPath"/> (0 is the start of the path, 1 is the end of the path).</param>
        /// <returns>The normal to the point at the specified position.</returns>
        public Point GetNormalAtRelative(double position)
        {
            Point tangent = this.GetTangentAtRelative(position);
            return new Point(-tangent.Y, tangent.X);
        }
    }

}
