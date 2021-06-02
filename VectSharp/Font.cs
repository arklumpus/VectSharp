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

}
