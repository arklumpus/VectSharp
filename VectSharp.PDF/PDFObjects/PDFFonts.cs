/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2024 Giorgio Bianchini

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// PDF font descriptor object.
    /// </summary>
    public class PDFFontDescriptor : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("FontDescriptor", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Name of the font described by this object.
        /// </summary>
        public PDFString FontName { get; }
        
        /// <summary>
        /// Name of the font family described by this object.
        /// </summary>
        public PDFString FontFamily { get; }
        
        /// <summary>
        /// Font descriptor flags.
        /// </summary>
        public PDFUInt Flags { get; }

        /// <summary>
        /// Font bounding box.
        /// </summary>
        public PDFArray<PDFDouble> FontBBox { get; }
        
        /// <summary>
        /// Font italic angle slope.
        /// </summary>
        public PDFDouble ItalicAngle { get; }

        /// <summary>
        /// The font ascent.
        /// </summary>
        public PDFDouble Ascent { get; }

        /// <summary>
        /// The font descent.
        /// </summary>
        public PDFDouble Descent { get; }

        /// <summary>
        /// The height of capital letters in the font.
        /// </summary>
        public PDFDouble CapHeight { get; }

        /// <summary>
        /// Thickness of vertical stems. Arbitrarily set to 80.
        /// </summary>
        public PDFDouble StemV { get; } = new PDFDouble(80);

        /// <summary>
        /// Thickness of horizontal stems. Arbitrarily set to 80.
        /// </summary>
        public PDFDouble StemH { get; } = new PDFDouble(80);
        
        /// <summary>
        /// <see cref="PDFTTFStream"/> containing the subsetted TTF file.
        /// </summary>
        public PDFTTFStream FontFile2 { get; }

        /// <summary>
        /// Create a new <see cref="PDFFontDescriptor"/> from the specified <see cref="TrueTypeFile"/>.
        /// </summary>
        /// <param name="ttf">The subsetted <see cref="TrueTypeFile"/>.</param>
        /// <param name="isSymbolic">Indicates whether this font descriptor should describe the symbolic or non-symbolic part of the font.</param>
        /// <param name="ttfStream">The <see cref="PDFTTFStream"/> containing the <paramref name="ttf"/> contents.</param>
        /// <param name="subsetTag">A unique tag identifying this font descriptor. It must consist of exactly six uppercase letters.</param>
        public PDFFontDescriptor(TrueTypeFile ttf, bool isSymbolic, PDFTTFStream ttfStream, string subsetTag)
        {
            string fontName = subsetTag + "+" + ttf.GetFontName();
            this.FontName = new PDFString(fontName, PDFString.StringDelimiter.StartingForwardSlash);

            string fontFamily = ttf.GetFontFamilyName();
            if (string.IsNullOrEmpty(fontFamily))
            {
                fontFamily = fontName;
            }
            this.FontFamily = new PDFString(fontFamily, PDFString.StringDelimiter.Brackets);

            double italicAngle = ttf.GetItalicAngle();

            if (double.IsNaN(italicAngle))
            {
                italicAngle = 0;
            }
            
            this.ItalicAngle = new PDFDouble(italicAngle);

            bool fixedPitch = ttf.IsFixedPitch();
            bool serif = ttf.IsSerif();
            bool script = ttf.IsScript();
            bool italic = ttf.IsBold();
            bool allCap = false;
            bool smallCap = false;
            bool forceBold = false;
            this.Flags = new PDFUInt((fixedPitch ? 1U : 0) | (serif ? 1U << 1 : 0) | (isSymbolic ? 1U << 2 : 0) | (script ? 1U << 3 : 0) | (!isSymbolic ? 1U << 5 : 0) | (italic ? 1U << 6 : 0) | (allCap ? 1U << 16 : 0) | (smallCap ? 1U << 17 : 0) | (forceBold ? 1U << 18 : 0));
            this.FontBBox = new PDFArray<PDFDouble>(new PDFDouble(ttf.Get1000EmXMin()), new PDFDouble(ttf.Get1000EmYMin()), new PDFDouble(ttf.Get1000EmXMax()), new PDFDouble(ttf.Get1000EmYMax()));
            this.Ascent = new PDFDouble(ttf.Get1000EmAscent());
            this.Descent = new PDFDouble(ttf.Get1000EmDescent());
            this.CapHeight = new PDFDouble(ttf.Get1000EmAscent());
            this.FontFile2 = ttfStream;
        }
    }

    /// <summary>
    /// Base class for PDF fonts.
    /// </summary>
    public abstract class PDFFont : PDFDictionary
    {
        /// <summary>
        /// The name used to refer to the font within content streams.
        /// </summary>
        public string FontReferenceName { get; }

        /// <summary>
        /// The object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("Font", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Font subtype.
        /// </summary>
        public PDFString Subtype { get; protected set; }
        
        /// <summary>
        /// PostScript name of the font.
        /// </summary>
        public PDFString BaseFont { get; protected set; }

        /// <summary>
        /// Create a new <see cref="PDFFont"/> with the specified <paramref name="fontReferenceName"/>.
        /// </summary>
        /// <param name="fontReferenceName">The name used to refer to the font within content streams.</param>
        protected PDFFont(string fontReferenceName)
        {
            this.FontReferenceName = fontReferenceName;
        }
    }

    /// <summary>
    /// A PDF stream containing a <see cref="TrueTypeFile"/>.
    /// </summary>
    public class PDFTTFStream : PDFStream
    {
        /// <summary>
        /// Create a new <see cref="PDFTTFStream"/> from the specified <see cref="TrueTypeFile"/>.
        /// </summary>
        /// <param name="subsettedFont">The subsetted <see cref="TrueTypeFile"/> to include in the <see cref="PDFTTFStream"/>.</param>
        /// <param name="compressStream">Determines whether the stream should be compressed.</param>
        public PDFTTFStream(TrueTypeFile subsettedFont, bool compressStream) : base(subsettedFont.FontStream, compressStream) { }
    }

    /// <summary>
    /// A PDF stream describing a mapping of character ranges to their Unicode counterpart.
    /// </summary>
    public class PDFToUnicodeStream : PDFStream
    {
        private static MemoryStream CreateStream(List<char> symbol, Dictionary<char, int> glyphIndices)
        {
            MemoryStream ms = new MemoryStream();

            using (StreamWriter sw = new StreamWriter(ms, Encoding.GetEncoding("ISO-8859-1"), 1024, true))
            {
                sw.NewLine = "\n";

                sw.WriteLine("/CIDInit /ProcSet findresource begin");
                sw.WriteLine("12 dict begin");
                sw.WriteLine("begincmap");
                sw.WriteLine("/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def");
                sw.WriteLine("/CMapName /Adobe-Identity-UCS def");
                sw.WriteLine("/CMapType 2 def");
                sw.WriteLine("1 begincodespacerange");
                sw.WriteLine("<0000> <ffff>");
                sw.WriteLine("endcodespacerange");
                sw.WriteLine("1 beginbfchar");

                for (int i = 0; i < symbol.Count; i++)
                {
                    sw.WriteLine("<" + glyphIndices[symbol[i]].ToString("X4") + "> <" + ((int)symbol[i]).ToString("X4") + ">");
                }

                sw.WriteLine("endbfchar");
                sw.WriteLine("endcmap");
                sw.WriteLine("CmapName currentdict /CMap defineresource pop");
                sw.WriteLine("end");
                sw.WriteLine("end");
            }

            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }

        /// <summary>
        /// Create a new <see cref="PDFToUnicodeStream"/> for the specified characters.
        /// </summary>
        /// <param name="symbol">The characters described by this stream.</param>
        /// <param name="glyphIndices">A dictionary associating each character to its index in the font.</param>
        /// <param name="compressStream">Determines whether the stream should be compressed.</param>
        public PDFToUnicodeStream(List<char> symbol, Dictionary<char, int> glyphIndices, bool compressStream) : base(CreateStream(symbol, glyphIndices), compressStream) { }
    }

    /// <summary>
    /// Represents a Type1 PDF font.
    /// </summary>
    public class PDFType1Font : PDFFont
    {
        /// <summary>
        /// Create a new <see cref="PDFType1Font"/> from the specified <paramref name="familyName"/>, using the specified <paramref name="fontReferenceName"/>.
        /// </summary>
        /// <param name="familyName">The font family name. It should be one of the 14 standard fonts.</param>
        /// <param name="fontReferenceName">The name used to refer to the font within content streams.</param>
        public PDFType1Font(string familyName, string fontReferenceName) : base(fontReferenceName)
        {
            this.Subtype = new PDFString("Type1", PDFString.StringDelimiter.StartingForwardSlash);
            this.BaseFont = new PDFString(familyName.Replace(" ", "-"), PDFString.StringDelimiter.StartingForwardSlash);
        }
    }

    /// <summary>
    /// Represents a TrueType font embedded in a PDF document.
    /// </summary>
    public class PDFTrueTypeFont : PDFFont
    {
        /// <summary>
        /// The first character included in the subsetted font.
        /// </summary>
        public PDFInt FirstChar { get; }

        /// <summary>
        /// The last character included in the subsetted font.
        /// </summary>
        public PDFInt LastChar { get; }

        /// <summary>
        /// The font descriptor.
        /// </summary>
        public PDFFontDescriptor FontDescriptor { get; }

        /// <summary>
        /// The font encoding.
        /// </summary>
        public PDFString Encoding { get; } = new PDFString("WinAnsiEncoding", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Widths of the glyphs included in the font.
        /// </summary>
        public PDFArray<PDFDouble> Widths { get; }

        /// <summary>
        /// Create a new <see cref="PDFTrueTypeFont"/> from the specified <paramref name="subsettedFont"/>.
        /// </summary>
        /// <param name="chars">The characters included in the font.</param>
        /// <param name="fontDescriptor">The font descriptor.</param>
        /// <param name="subsettedFont">The subsetted <see cref="TrueTypeFile"/>.</param>
        /// <param name="fontReferenceName">The name used to refer to the font within content streams.</param>
        public PDFTrueTypeFont(List<char> chars, PDFFontDescriptor fontDescriptor, TrueTypeFile subsettedFont, string fontReferenceName) : base(fontReferenceName)
        {
            this.Subtype = new PDFString("TrueType", PDFString.StringDelimiter.StartingForwardSlash);
            this.BaseFont = fontDescriptor.FontName;

            int firstChar = (from el in chars select Array.IndexOf(PDFContextInterpreter.CP1252Chars, el)).Min();
            int lastChar = (from el in chars select Array.IndexOf(PDFContextInterpreter.CP1252Chars, el)).Max();

            this.FirstChar = new PDFInt(firstChar);
            this.LastChar = new PDFInt(lastChar);
            this.FontDescriptor = fontDescriptor;
            this.Widths = new PDFArray<PDFDouble>(Enumerable.Range(firstChar, lastChar - firstChar + 1).Select(i => new PDFDouble(chars.Contains(PDFContextInterpreter.CP1252Chars[i]) ? subsettedFont.Get1000EmGlyphWidth(PDFContextInterpreter.CP1252Chars[i]) : 0)));
        }
    }

    /// <summary>
    /// PDF character collection.
    /// </summary>
    public class PDFCIDSystemInfo : PDFDictionary
    {
        /// <summary>
        /// CID registry.
        /// </summary>
        public PDFString Registry { get; } = new PDFString("Adobe", PDFString.StringDelimiter.Brackets);

        /// <summary>
        /// CID ordering.
        /// </summary>
        public PDFString Ordering { get; } = new PDFString("Identity", PDFString.StringDelimiter.Brackets);

        /// <summary>
        /// CID supplement.
        /// </summary>
        public PDFInt Supplement { get; } = new PDFInt(0);
    }

    /// <summary>
    /// PDF CIDFontType2 font.
    /// </summary>
    public class PDFCIDFontType2Font : PDFFont
    {
        /// <summary>
        /// Indices of glyphs in the font file.
        /// </summary>
        public Dictionary<char, int> GlyphIndices { get; }

        /// <summary>
        /// PDF character collection.
        /// </summary>
        public PDFCIDSystemInfo CIDSystemInfo { get; } = new PDFCIDSystemInfo();

        /// <summary>
        /// Font descriptor.
        /// </summary>
        public PDFFontDescriptor FontDescriptor { get; }
        
        /// <summary>
        /// Glyph widths.
        /// </summary>
        public PDFArray<PDFValueObject> W { get; }

        /// <summary>
        /// Create a new <see cref="PDFCIDFontType2Font"/> describing the specified <paramref name="symbol"/> characters.
        /// </summary>
        /// <param name="symbol">The characters described by this font.</param>
        /// <param name="fontDescriptor">The font descriptor.</param>
        /// <param name="subsettedFont">The subsetted <see cref="TrueTypeFile"/> containing the glyph data.</param>
        /// <param name="fontReferenceName">The name used to refer to the font within content streams.</param>
        public PDFCIDFontType2Font(List<char> symbol, PDFFontDescriptor fontDescriptor, TrueTypeFile subsettedFont, string fontReferenceName) : base(fontReferenceName)
        {
            GlyphIndices = new Dictionary<char, int>();

            for (int i = 0; i < symbol.Count; i++)
            {
                GlyphIndices.Add(symbol[i], subsettedFont.GetGlyphIndex(symbol[i]));
            }

            this.Subtype = new PDFString("CIDFontType2", PDFString.StringDelimiter.StartingForwardSlash);
            this.BaseFont = fontDescriptor.FontName;
            this.FontDescriptor = fontDescriptor;

            this.W = new PDFArray<PDFValueObject>(symbol.SelectMany(x => new PDFValueObject[] { new PDFInt(GlyphIndices[x]), new PDFArray<PDFDouble>(new PDFDouble(subsettedFont.Get1000EmGlyphWidth(x))) }));
        }
    }

    /// <summary>
    /// A PDF Type0 font.
    /// </summary>
    public class PDFType0Font : PDFFont
    {
        /// <summary>
        /// Font encoding.
        /// </summary>
        public PDFString Encoding { get; } = new PDFString("Identity-H", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Descendant font.
        /// </summary>
        public PDFArray<PDFCIDFontType2Font> DescendantFonts { get; }
        
        /// <summary>
        /// Stream mapping glyphs to Unicode characters.
        /// </summary>
        public PDFToUnicodeStream ToUnicode { get; }

        /// <summary>
        /// Create a new <see cref="PDFType0Font"/>.
        /// </summary>
        /// <param name="fontDescriptor">The font descriptor.</param>
        /// <param name="descendantFont">The descendant font.</param>
        /// <param name="toUnicode">The stream mapping glyphs to Unicode characters.</param>
        /// <param name="fontReferenceName">The name used to refer to the font within content streams.</param>
        public PDFType0Font(PDFFontDescriptor fontDescriptor, PDFCIDFontType2Font descendantFont, PDFToUnicodeStream toUnicode, string fontReferenceName) : base(fontReferenceName)
        {
            this.Subtype = new PDFString("Type0", PDFString.StringDelimiter.StartingForwardSlash);
            this.BaseFont = fontDescriptor.FontName;
            this.DescendantFonts = new PDFArray<PDFCIDFontType2Font>(descendantFont);
            this.ToUnicode = toUnicode;
        }
    }
}