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

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace VectSharp
{
    /// <summary>
    /// Represents the position of the text.
    /// </summary>
    public enum Script
    {
        /// <summary>
        /// The text is normal text.
        /// </summary>
        Normal,

        /// <summary>
        /// The text is a superscript.
        /// </summary>
        Superscript,

        /// <summary>
        /// The text is a subscript.
        /// </summary>
        Subscript
    }

    /// <summary>
    /// Represents a run of text that should be drawn with the same style.
    /// </summary>
    public class FormattedText
    {
        /// <summary>
        /// Represents the text represented by this instance.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Represents the font that should be used to draw the text.
        /// </summary>
        public Font Font { get; }

        /// <summary>
        /// Represents the position of the text.
        /// </summary>
        public Script Script { get; }

        /// <summary>
        /// Represents the brush that should be used to draw the text. If this is null, the default brush is used.
        /// </summary>
        public Brush Brush { get; }

        /// <summary>
        /// Creates a new <see cref="FormattedText"/> instance with the specified <paramref name="text"/>, <paramref name="font"/>, <paramref name="script"/> position and <paramref name="brush"/>.
        /// </summary>
        /// <param name="text">The text that will be contained in the new <see cref="FormattedText"/>.</param>
        /// <param name="font">The font that will be used by the new <see cref="FormattedText"/>.</param>
        /// <param name="script">The script position of the new <see cref="FormattedText"/>.</param>
        /// <param name="brush">The brush that will be used by the new <see cref="FormattedText"/>.</param>
        public FormattedText(string text, Font font, Script script = Script.Normal, Brush brush = null)
        {
            this.Text = text;
            this.Font = font;
            this.Script = script;
            this.Brush = brush;
        }

        /// <summary>
        /// Parse the formatting information contained in a text string into a collection of <see cref="FormattedText"/> objects.
        /// </summary>
        /// <param name="text">
        /// The string containing formatting information. Format information is specified using HTML-like tags:
        /// <list type="bullet">
        /// <item><c>&lt;b&gt;&lt;/b&gt;</c> or <c>&lt;strong&gt;&lt;/strong&gt;</c> are used for bold text;</item>
        /// <item><c>&lt;i&gt;&lt;/i&gt;</c> or <c>&lt;em&gt;&lt;/em&gt;</c> are used for text in italics;</item>
        /// <item><c>&lt;u&gt;&lt;/u&gt;</c></item> are used for underlined text;
        /// <item><c>&lt;sup&gt;&lt;/sup&gt;</c> and <c>&lt;sub&gt;&lt;/sub&gt;</c> are used, respectively, for superscript and subscript text;</item>
        /// <item><c>&lt;#COLOUR&gt;&lt;/#&gt;</c> is used to specify the colour of the text, where <c>COLOUR</c> is a CSS colour string (e.g. <c>&lt;#red&gt;</c>, <c>&lt;#0080FF&gt;</c>, or <c>&lt;#rgba(128, 80, 52, 0.5)&gt;</c>).</item>
        /// </list></param>
        /// <param name="normalFont">The font that will be used for text that is neither bold nor italic.</param>
        /// <param name="boldFont">The font that will be used for text that is bold. Note that this does not necessarily have to be a bold font; this is just the font that is applied to text contained within <c>&lt;b&gt;&lt;/b&gt;</c> tags.</param>
        /// <param name="italicFont">The font that will be used for text that is in italics. Note that this does not necessarily have to be an italic font; this is just the font that is applied to text contained within <c>&lt;i&gt;&lt;/i&gt;</c> tags.</param>
        /// <param name="boldItalicFont">The font that will be used for text that is both in bold and in italics.</param>
        /// <param name="defaultBrush">The default <see cref="Brush"/> that will be used for text runs that do not specify a colour. If this is <see langword="null"/>, the default <see cref="Brush"/> will be the one specified in the painting call.</param>
        /// <returns>A lazy collection of <see cref="FormattedText"/> objects. Note that every enumeration of this collection causes the text to be parsed again; if you need to enumerate this collection more than once, you should probably convert it e.g. to a <see cref="List{T}"/>.</returns>
        public static IEnumerable<FormattedText> Format(string text, Font normalFont, Font boldFont, Font italicFont, Font boldItalicFont, Brush defaultBrush = null)
        {
            StringBuilder currentRun = new StringBuilder();
            int boldDepth = 0;
            int italicsDepth = 0;
            int underlineDepth = 0;
            int superscriptDepth = 0;
            int subscriptDepth = 0;
            Stack<Brush> brushes = new Stack<Brush>();
            brushes.Push(defaultBrush);

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != '<')
                {
                    currentRun.Append(text[i]);
                }
                else
                {
                    Tags tag = GetTag(text, i, out int tagEnd, out Brush tagBrush);

                    if (tag == Tags.None)
                    {
                        currentRun.Append(text[i]);
                    }
                    else
                    {
                        i = tagEnd;

                        string txt = currentRun.ToString();

                        switch (tag)
                        {
                            case Tags.BoldOpen:
                                if (!string.IsNullOrEmpty(txt))
                                {
                                    yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                }
                                currentRun.Clear();
                                boldDepth++;
                                break;
                            case Tags.BoldClose:
                                if (boldDepth > 0)
                                {
                                    if (!string.IsNullOrEmpty(txt))
                                    {
                                        yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                    }
                                    currentRun.Clear();
                                    boldDepth--;
                                }
                                break;
                            case Tags.UnderlineOpen:
                                if (!string.IsNullOrEmpty(txt))
                                {
                                    yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                }
                                currentRun.Clear();
                                underlineDepth++;
                                break;
                            case Tags.UnderlineClose:
                                if (underlineDepth > 0)
                                {
                                    if (!string.IsNullOrEmpty(txt))
                                    {
                                        yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                    }
                                    currentRun.Clear();
                                    underlineDepth--;
                                }
                                break;

                            case Tags.ItalicsOpen:
                                if (!string.IsNullOrEmpty(txt))
                                {
                                    yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                }
                                currentRun.Clear();
                                italicsDepth++;
                                break;
                            case Tags.ItalicsClose:
                                if (italicsDepth > 0)
                                {
                                    if (!string.IsNullOrEmpty(txt))
                                    {
                                        yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                    }
                                    currentRun.Clear();
                                    italicsDepth--;
                                }
                                break;

                            case Tags.SupOpen:
                                if (!string.IsNullOrEmpty(txt))
                                {
                                    yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                }
                                currentRun.Clear();
                                superscriptDepth++;
                                break;
                            case Tags.SupClose:
                                if (superscriptDepth > 0)
                                {
                                    if (!string.IsNullOrEmpty(txt))
                                    {
                                        yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                    }
                                    currentRun.Clear();
                                    superscriptDepth--;
                                }
                                break;

                            case Tags.SubOpen:
                                if (!string.IsNullOrEmpty(txt))
                                {
                                    yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                }
                                currentRun.Clear();
                                subscriptDepth++;
                                break;
                            case Tags.SubClose:
                                if (subscriptDepth > 0)
                                {
                                    if (!string.IsNullOrEmpty(txt))
                                    {
                                        yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                    }
                                    currentRun.Clear();
                                    subscriptDepth--;
                                }
                                break;
                            case Tags.ColourOpen:
                                if (!string.IsNullOrEmpty(txt))
                                {
                                    yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                }
                                currentRun.Clear();
                                brushes.Push(tagBrush);
                                break;
                            case Tags.ColourClose:
                                if (brushes.Count > 1)
                                {
                                    if (!string.IsNullOrEmpty(txt))
                                    {
                                        yield return new FormattedText(txt, GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
                                    }
                                    currentRun.Clear();
                                    brushes.Pop();
                                }
                                break;
                        }
                    }
                }
            }

            if (currentRun.Length > 0)
            {
                yield return new FormattedText(currentRun.ToString(), GetFont(boldDepth % 2 == 1, italicsDepth % 2 == 1, underlineDepth % 2 == 1, normalFont, boldFont, italicFont, boldItalicFont), superscriptDepth > subscriptDepth ? Script.Superscript : subscriptDepth > superscriptDepth ? Script.Subscript : Script.Normal, brushes.Peek());
            }
        }

        /// <summary>
        /// Parse the formatting information contained in a text string into a collection of <see cref="FormattedText"/> objects, using fonts from a standard font family.
        /// </summary>
        /// <param name="text">The string containing formatting information. Format information is specified using HTML-like tags:
        /// <list type="bullet">
        /// <item><c>&lt;b&gt;&lt;/b&gt;</c> or <c>&lt;strong&gt;&lt;/strong&gt;</c> are used for bold text;</item>
        /// <item><c>&lt;i&gt;&lt;/i&gt;</c> or <c>&lt;em&gt;&lt;/em&gt;</c> are used for text in italics;</item>
        /// <item><c>&lt;u&gt;&lt;/u&gt;</c></item> are used for underlined text;
        /// <item><c>&lt;sup&gt;&lt;/sup&gt;</c> and <c>&lt;sub&gt;&lt;/sub&gt;</c> are used, respectively, for superscript and subscript text;</item>
        /// <item><c>&lt;#COLOUR&gt;&lt;/#&gt;</c> is used to specify the colour of the text, where <c>COLOUR</c> is a CSS colour string (e.g. <c>&lt;#red&gt;</c>, <c>&lt;#0080FF&gt;</c>, or <c>&lt;#rgba(128, 80, 52, 0.5)&gt;</c>).</item>
        /// </list></param>
        /// <param name="fontFamily">The font family from which the fonts will be created. If this is a regular font family, the bold, italic and bold-italic versions of the font will be used for the formatted text. Otherwise, the relevant font styles will be toggled (e.g. if the supplied font family is bold, then regular text in the formatted string will be displayed as bold, while bold text in the formatted string will be displayed as regular text).</param>
        /// <param name="fontSize">The size of the fonts to use.</param>
        /// <param name="defaultUnderline">Determines whether text should be underlined by default. This is toggled by <c>&lt;u&gt;&lt;/u&gt;</c> tags.</param>
        /// <param name="defaultBrush">The default <see cref="Brush"/> that will be used for text runs that do not specify a colour. If this is <see langword="null"/>, the default <see cref="Brush"/> will be the one specified in the painting call.</param>
        /// <returns>A lazy collection of <see cref="FormattedText"/> objects. Note that every enumeration of this collection causes the text to be parsed again; if you need to enumerate this collection more than once, you should probably convert it e.g. to a <see cref="List{T}"/>.</returns>
        public static IEnumerable<FormattedText> Format(string text, FontFamily.StandardFontFamilies fontFamily, double fontSize, bool defaultUnderline = false, Brush defaultBrush = null)
        {
            Font normalFont = new Font(FontFamily.ResolveFontFamily(fontFamily), fontSize, defaultUnderline);

            Font boldFont = normalFont;
            Font italicFont = normalFont;
            Font boldItalicFont = normalFont;

            switch (fontFamily)
            {
                case FontFamily.StandardFontFamilies.Courier:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBold), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierOblique), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBoldOblique), fontSize, defaultUnderline);
                    break;
                case FontFamily.StandardFontFamilies.CourierBold:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Courier), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBoldOblique), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierOblique), fontSize, defaultUnderline);
                    break;
                case FontFamily.StandardFontFamilies.CourierOblique:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBoldOblique), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Courier), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBold), fontSize, defaultUnderline);
                    break;
                case FontFamily.StandardFontFamilies.CourierBoldOblique:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierOblique), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBold), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Courier), fontSize, defaultUnderline);
                    break;

                case FontFamily.StandardFontFamilies.Helvetica:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaOblique), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBoldOblique), fontSize, defaultUnderline);
                    break;
                case FontFamily.StandardFontFamilies.HelveticaBold:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBoldOblique), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaOblique), fontSize, defaultUnderline);
                    break;
                case FontFamily.StandardFontFamilies.HelveticaOblique:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBoldOblique), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), fontSize, defaultUnderline);
                    break;
                case FontFamily.StandardFontFamilies.HelveticaBoldOblique:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaOblique), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), fontSize, defaultUnderline);
                    break;

                case FontFamily.StandardFontFamilies.TimesRoman:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesBold), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesItalic), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesBoldItalic), fontSize, defaultUnderline);
                    break;
                case FontFamily.StandardFontFamilies.TimesBold:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesRoman), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesBoldItalic), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesItalic), fontSize, defaultUnderline);
                    break;
                case FontFamily.StandardFontFamilies.TimesItalic:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesBoldItalic), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesRoman), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesBold), fontSize, defaultUnderline);
                    break;
                case FontFamily.StandardFontFamilies.TimesBoldItalic:
                    boldFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesItalic), fontSize, defaultUnderline);
                    italicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesBold), fontSize, defaultUnderline);
                    boldItalicFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesRoman), fontSize, defaultUnderline);
                    break;
            }

            return Format(text, normalFont, boldFont, italicFont, boldItalicFont, defaultBrush);
        }

        private static Font GetFont(bool bold, bool italic, bool underlined, Font normalFont, Font boldFont, Font italicFont, Font boldItalicFont)
        {
            if (!bold && !italic)
            {
                if (underlined)
                {
                    return new Font(normalFont.FontFamily, normalFont.FontSize, normalFont.Underline == null);
                }
                else
                {
                    return normalFont;
                }
            }
            else if (bold && !italic)
            {
                if (underlined)
                {
                    return new Font(boldFont.FontFamily, boldFont.FontSize, boldFont.Underline == null);
                }
                else
                {
                    return boldFont;
                }
            }
            else if (!bold && italic)
            {
                if (underlined)
                {
                    return new Font(italicFont.FontFamily, italicFont.FontSize, italicFont.Underline == null);
                }
                else
                {
                    return italicFont;
                }
            }
            else
            {
                if (underlined)
                {
                    return new Font(boldItalicFont.FontFamily, boldItalicFont.FontSize, boldItalicFont.Underline == null);
                }
                else
                {
                    return boldItalicFont;
                }
            }
        }

        private enum Tags
        {
            BoldOpen,
            BoldClose,
            ItalicsOpen,
            ItalicsClose,
            UnderlineOpen,
            UnderlineClose,
            SupOpen,
            SupClose,
            SubOpen,
            SubClose,
            ColourOpen,
            ColourClose,
            None
        }

        private static Tags GetTag(string text, int start, out int tagEnd, out Brush tagBrush)
        {
            StringBuilder tag = new StringBuilder();
            bool closed = false;

            int i = start;

            for (; i < text.Length; i++)
            {
                tag.Append(text[i]);

                if (text[i] == '>')
                {
                    closed = true;
                    break;
                }
            }

            tagEnd = i;
            tagBrush = null;

            if (!closed)
            {
                return Tags.None;
            }
            else
            {
                string tagString = tag.Replace(" ", "").ToString().ToLowerInvariant();

                if (tagString == "<b>" || tagString == "<strong>")
                {
                    return Tags.BoldOpen;
                }
                else if (tagString == "</b>" || tagString == "</strong>")
                {
                    return Tags.BoldClose;
                }
                else if (tagString == "<i>" || tagString == "<em>")
                {
                    return Tags.ItalicsOpen;
                }
                else if (tagString == "</i>" || tagString == "</em>")
                {
                    return Tags.ItalicsClose;
                }
                else if (tagString == "<u>")
                {
                    return Tags.UnderlineOpen;
                }
                else if (tagString == "</u>")
                {
                    return Tags.UnderlineClose;
                }
                else if (tagString == "<sup>")
                {
                    return Tags.SupOpen;
                }
                else if (tagString == "</sup>")
                {
                    return Tags.SupClose;
                }
                else if (tagString == "<sub>")
                {
                    return Tags.SubOpen;
                }
                else if (tagString == "</sub>")
                {
                    return Tags.SubClose;
                }
                else if (tagString.StartsWith("<#"))
                {
                    string colour = tagString.Substring(1, tagString.Length - 2);

                    Colour? col = null;

                    try
                    {
                        col = Colour.FromCSSString(colour);
                    }
                    catch { }

                    if (col == null)
                    {
                        colour = tagString.Substring(2, tagString.Length - 3);

                        col = Colour.FromCSSString(colour);
                    }

                    if (col != null)
                    {
                        tagBrush = col.Value;
                        return Tags.ColourOpen;
                    }
                    else
                    {
                        return Tags.None;
                    }
                }
                else if (tagString == "</#>")
                {
                    return Tags.ColourClose;
                }
                else
                {
                    return Tags.None;
                }
            }
        }
    }

    /// <summary>
    /// Contains extension methods for collections of <see cref="FormattedText"/> objects.
    /// </summary>
    public static class FormattedTextExtensions
    {
        internal static Font.DetailedFontMetrics Measure(this IEnumerable<FormattedText> text, List<FormattedText> items, List<Font.DetailedFontMetrics> allMetrics)
        {
            double width = 0;
            double advanceWidth = 0;

            double lsb = 0;
            double rsb = 0;

            double top = 0;
            double bottom = 0;

            bool isFirst = true;

            foreach (FormattedText txt in text)
            {
                items?.Add(txt);

                if (txt.Script == Script.Normal)
                {
                    Font.DetailedFontMetrics metrics = txt.Font.MeasureTextAdvanced(txt.Text);
                    allMetrics?.Add(metrics);

                    top = Math.Max(top, metrics.Top);
                    bottom = Math.Min(bottom, metrics.Bottom);
                    rsb = metrics.RightSideBearing;

                    advanceWidth += metrics.AdvanceWidth;

                    if (!isFirst)
                    {
                        width += metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing;
                    }
                    else
                    {
                        width += metrics.Width + metrics.RightSideBearing;
                        lsb = metrics.LeftSideBearing;
                    }
                }
                else
                {
                    Font newFont = new Font(txt.Font.FontFamily, txt.Font.FontSize * 0.7);

                    Font.DetailedFontMetrics metrics = newFont.MeasureTextAdvanced(txt.Text);
                    allMetrics?.Add(metrics);

                    advanceWidth += metrics.AdvanceWidth;

                    if (txt.Script == Script.Subscript)
                    {
                        top = Math.Max(top, metrics.Top - txt.Font.FontSize * 0.14);
                        bottom = Math.Min(bottom, metrics.Bottom - txt.Font.FontSize * 0.14);
                    }
                    else if (txt.Script == Script.Superscript)
                    {
                        top = Math.Max(top, metrics.Top + txt.Font.FontSize * 0.33);
                        bottom = Math.Min(bottom, metrics.Bottom + txt.Font.FontSize * 0.33);
                    }

                    rsb = metrics.RightSideBearing;

                    if (!isFirst)
                    {
                        width += metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing;
                    }
                    else
                    {
                        width += metrics.Width + metrics.RightSideBearing;
                        lsb = metrics.LeftSideBearing;
                    }
                }

                if (isFirst)
                {
                    isFirst = false;
                }
            }

            width -= rsb;

            return new Font.DetailedFontMetrics(width, top - bottom, lsb, rsb, top, bottom, advanceWidth);
        }

        /// <summary>
        /// Measures a collection of <see cref="FormattedText"/> objects.
        /// </summary>
        /// <param name="text">The collection of <see cref="FormattedText"/> objects to be measured.</param>
        /// <returns>A <see cref="Font.DetailedFontMetrics"/> containing detailed measurements for the text obtained by composing the elements in the <see cref="FormattedText"/> collection.</returns>
        public static Font.DetailedFontMetrics Measure(this IEnumerable<FormattedText> text)
        {
            return text.Measure(null, null);
        }
    }

}
