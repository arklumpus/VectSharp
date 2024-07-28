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

using Markdig;
using Markdig.Extensions.Emoji;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VectSharp.Markdown
{
    public partial class MarkdownRenderer
	{
		/// <summary>
		/// The base font size to use when rendering the document. This will be the size of regular elements, and the size of header elements will be expressed as a multiple of this.
		/// </summary>
		public double BaseFontSize { get; set; } = 9.71424;

		/// <summary>
		/// Scaling factor for the font size to use when rendering math.
		/// </summary>
		public double MathFontScalingFactor { get; set; } = 0.85;

		/// <summary>
		/// The font size for elements at each header level. The values in this array will be multiplied by the <see cref="BaseFontSize"/>.
		/// </summary>
		public double[] HeaderFontSizeMultipliers { get; } = new double[]
		{
			28 / 12.0, 22 / 12.0, 16 / 12.0, 14 / 12.0, 13 / 12.0, 12 / 12.0
		};

		/// <summary>
		/// The thickness of the separator line after a header of each level. A value of 0 disables the line after headers of that level.
		/// </summary>
		public double[] HeaderLineThicknesses { get; } = new double[] { 1, 1, 0, 0, 0, 0 };

		/// <summary>
		/// The thickness of thematic break lines.
		/// </summary>
		public double ThematicBreakThickness { get; set; } = 2;

		/// <summary>
		/// The font family for regular text.
		/// </summary>
		public FontFamily RegularFontFamily { get; set; } = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);

		/// <summary>
		/// The font family for bold text.
		/// </summary>
		public FontFamily BoldFontFamily { get; set; } = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold);

		/// <summary>
		/// The font family for italic text.
		/// </summary>
		public FontFamily ItalicFontFamily { get; set; } = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaOblique);

		/// <summary>
		/// The font family for bold italic text.
		/// </summary>
		public FontFamily BoldItalicFontFamily { get; set; } = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBoldOblique);

		/// <summary>
		/// The font family for code elements.
		/// </summary>
		public FontFamily CodeFont { get; set; } = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Courier);

		/// <summary>
		/// The font family for bold code elements.
		/// </summary>
		public FontFamily CodeFontBold { get; set; } = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBold);

		/// <summary>
		/// The font family for italic code elements.
		/// </summary>
		public FontFamily CodeFontItalic { get; set; } = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierOblique);

		/// <summary>
		/// The font family for bold italic code elements.
		/// </summary>
		public FontFamily CodeFontBoldItalic { get; set; } = FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBoldOblique);

		/// <summary>
		/// The thickness of underlines. This value will be multiplied by the font size of the element being underlined.
		/// </summary>
		public double UnderlineThickness { get; set; } = 0.075;

		/// <summary>
		/// The thickness of underlines for bold text. This value will be multiplied by the font size of the element being underlined.
		/// </summary>
		public double BoldUnderlineThickness { get; set; } = 0.15;

		/// <summary>
		/// The margins of the page.
		/// </summary>
		public Margins Margins { get; set; } = new Margins(55, 55, 55, 55);

		/// <summary>
		/// The margins for table cells.
		/// </summary>
		public Margins TableCellMargins { get; set; } = new Margins(5, 0, 5, 0);

		/// <summary>
		/// Defines the options for the vertical alignment of table cells.
		/// </summary>
		public enum VerticalAlignment
		{
			/// <summary>
			/// Table cells will be aligned at the top of their row.
			/// </summary>
			Top,

			/// <summary>
			/// Table cells will be aligned in the middle of their row.
			/// </summary>
			Middle,

			/// <summary>
			/// Table cells will be aligned at the bottom of their row.
			/// </summary>
			Bottom
		}

		/// <summary>
		/// The vertical alignment of table cells.
		/// </summary>
		public VerticalAlignment TableVAlign { get; set; } = VerticalAlignment.Middle;

		/// <summary>
		/// The size of the page.
		/// </summary>
		public Size PageSize { get; set; } = new Size(595, 842);

		/// <summary>
		/// The space before each text paragraph. This value will be multiplied by the <see cref="BaseFontSize"/>.
		/// </summary>
		public double SpaceBeforeParagaph { get; set; } = 0;

		/// <summary>
		/// The space after each text paragraph. This value will be multiplied by the <see cref="BaseFontSize"/>.
		/// </summary>
		public double SpaceAfterParagraph { get; set; } = 0.75;

		/// <summary>
		/// The space after each line of text. This value will be multiplied by the <see cref="BaseFontSize"/>.
		/// </summary>
		public double SpaceAfterLine { get; set; } = 0.25;

		/// <summary>
		/// The space before each heading. This value will be multiplied by the font size of the heading.
		/// </summary>
		public double SpaceBeforeHeading { get; set; } = 0.25;

		/// <summary>
		/// The space after each heading. This value will be multiplied by the font size of the heading.
		/// </summary>
		public double SpaceAfterHeading { get; set; } = 0.25;

		/// <summary>
		/// The margin at the left and right of code inlines. This value will be multiplied by the current font size.
		/// </summary>
		public double CodeInlineMargin { get; set; } = 0.25;

		/// <summary>
		/// The indentation width used for list items.
		/// </summary>
		public double IndentWidth { get; set; } = 40;

		/// <summary>
		/// The indentation width used for block quotes.
		/// </summary>
		public double QuoteBlockIndentWidth { get; set; } = 30;

		/// <summary>
		/// The thickness of the bar to the left of block quotes.
		/// </summary>
		public double QuoteBlockBarWidth { get; set; } = 5;

		/// <summary>
		/// The font size for subscripts and superscripts. This value will be multiplied by the current font size.
		/// </summary>
		public double SubSuperscriptFontSize { get; set; } = 0.7;

		/// <summary>
		/// The upwards shift in the baseline for superscript elements. This value will be multiplied by the current font size.
		/// </summary>
		public double SuperscriptShift { get; set; } = 0.33;

		/// <summary>
		/// The downwards shift in the baseline for subscript elements. This value will be multiplied by the current font size.
		/// </summary>
		public double SubscriptShift { get; set; } = 0.14;

		/// <summary>
		/// The base uri for resolving relative image addresses.
		/// </summary>
		public string BaseImageUri { get; set; } = "";

		/// <summary>
		/// A method used to resolve (possibly remote) image uris into local file paths. The first argument of the method should be the image uri and the second argument the base uri used to resolve relative links. The method should return a tuple containing the path of the local file and a boolean value indicating whether the file has been fetched from a remote location and should be deleted after the program has finished using it.
		/// </summary>
		public Func<string, string, (string, bool)> ImageUriResolver { get; set; } = HTTPUtils.ResolveImageURI;

		/// <summary>
		/// A method used to resolve emojis. The argument of the method should be an emoji uri of the form "emoji://{name}_heading:{level}", where "{name}" is the name of the emoji and "{level}" is the heading level, or "unicode://{unicode}_heading:{level}", where "{unicode}" is a unicode surrogate pair. The method should return a tuple containing the path of a local file containing the rendered emoji and a boolean value indicating whether the file should be deleted after the program has finished using it.
		/// </summary>
		public Func<string, (string, bool)> EmojiUriResolver { get; set; }

		/// <summary>
		/// The base uri for resolving links.
		/// </summary>
		public Uri BaseLinkUri { get; set; } = new Uri("about:blank");

		/// <summary>
		/// A method used to resolve link addresses. The argument of the method should be the absolute link, and the method should return the resolved address. This can be used to "redirect" links to a different target.
		/// </summary>
		public Func<string, string> LinkUriResolver { get; set; } = a => a;

		/// <summary>
		/// A method used to a load raster image from a local file. The argument of the method should be the path of a local image file, and the method should return a RasterImage representing that file. For example, this can be achieved using the <c>RasterImageFile</c> class from the <c>VectSharp.MuPDFUtils</c> package. If this is <see langword="null" />, only SVG images will be included in the document.
		/// </summary>
		public Func<string, RasterImage> RasterImageLoader { get; set; } = null;

		/// <summary>
		/// The size of images (as defined in the image's width and height attributes) will be multiplied by this value to determine the actual size of the image on the page. This has no effect on images without a width or height attribute.
		/// </summary>
		public double ImageUnitMultiplier { get; set; } = 0.60714;

		/// <summary>
		/// The size of images will be multiplied by this value to determine the actual size of the image on the page. For images that have a width or height attribute, this will be applied in addition to the <see cref="ImageUnitMultiplier"/>. For images without width and height, only this multiplier will be applied.
		/// </summary>
		public double ImageMultiplier { get; set; } = 1;

		/// <summary>
		/// The margin on the right of left-aligned images and on the left of right-aligned images.
		/// </summary>
		public double ImageSideMargin { get; set; } = 10;

		/// <summary>
		/// Images will be allowed to extend into the page bottom margin area by this amount before triggering a page break. This should be smaller than the bottom margin, otherwise images risk being cut off by the page boundary.
		/// </summary>
		public double ImageMarginTolerance { get; set; } = 25;

		/// <summary>
		/// A method used for syntax highlighting. The first argument should be the source code to highlight, while the second parameter is the name of the language to use for the highlight. The method should return a list of lists of <see cref="FormattedString"/>s, with each list of <see cref="FormattedString"/>s representing a line. For each code block, if the method returns <see langword="null" />, no syntax highlighting is used.
		/// </summary>
		public Func<string, string, List<List<FormattedString>>> SyntaxHighlighter { get; set; } = VectSharp.Markdown.SyntaxHighlighter.GetSyntaxHighlightedLines;

		/// <summary>
		/// Bullet points used for unordered lists. Each element of this list corresponds to the bullet for each level of list indentation. If the list indentation is greater than the number of elements in this list, the bullet points will be reused cyclically.
		/// Each element of this list is a method taking two arguments: the first is the <see cref="Graphics"/> object on which the bullet point should be drawn, while the second is the colour in which it should be painted. The method should draw the bullet point centered around the origin. The size of the bullet point will be multiplied by the current font size.
		/// </summary>
		public List<Action<Graphics, Colour>> Bullets { get; } = new List<Action<Graphics, Colour>>()
		{
			(graphics, colour) =>
			{
				graphics.FillPath(new GraphicsPath().Arc(-0.5, 0, 0.25, 0, 2 * Math.PI), colour);
			},

			(graphics, colour) =>
			{
				graphics.StrokePath(new GraphicsPath().Arc(-0.5, 0, 0.25, 0, 2 * Math.PI), colour, 0.1);
			},

			(graphics, colour) =>
			{
				graphics.StrokeRectangle(-0.75, -0.25, 0.5, 0.5, colour, 0.1);
			},
		};

		/// <summary>
		/// The bullet used for unchecked task list items.
		/// </summary>
		public Graphics TaskListUncheckedBullet { get; set; } = new Func<Graphics>(() =>
		{
			Graphics tbr = new Graphics();

			GraphicsPath checkboxPath = new GraphicsPath().MoveTo(-0.7, -0.4).LineTo(-0.3, -0.4).Arc(-0.3, -0.2, 0.2, 3 * Math.PI / 2, 2 * Math.PI).LineTo(-0.1, 0.2).Arc(-0.3, 0.2, 0.2, 0, Math.PI / 2).LineTo(-0.7, 0.4).Arc(-0.7, 0.2, 0.2, Math.PI / 2, Math.PI).LineTo(-0.9, -0.2).Arc(-0.7, -0.2, 0.2, Math.PI, 3 * Math.PI / 2).Close();
			tbr.FillPath(checkboxPath, Colour.FromRgb(240, 246, 249));
			tbr.StrokePath(checkboxPath, Colour.FromRgb(0, 114, 178), 0.075);

			return tbr;
		})();

		/// <summary>
		/// The bullet used for checked task list items.
		/// </summary>
		public Graphics TaskListCheckedBullet { get; set; } = new Func<Graphics>(() =>
		{
			Graphics tbr = new Graphics();

			GraphicsPath checkboxPath = new GraphicsPath().MoveTo(-0.7, -0.4).LineTo(-0.3, -0.4).Arc(-0.3, -0.2, 0.2, 3 * Math.PI / 2, 2 * Math.PI).LineTo(-0.1, 0.2).Arc(-0.3, 0.2, 0.2, 0, Math.PI / 2).LineTo(-0.7, 0.4).Arc(-0.7, 0.2, 0.2, Math.PI / 2, Math.PI).LineTo(-0.9, -0.2).Arc(-0.7, -0.2, 0.2, Math.PI, 3 * Math.PI / 2).Close();
			tbr.FillPath(checkboxPath, Colour.FromRgb(240, 246, 249));
			tbr.StrokePath(checkboxPath, Colour.FromRgb(0, 114, 178), 0.075);

			GraphicsPath tickpath = new GraphicsPath().MoveTo(-0.75, -0.1).LineTo(-0.5, 0.15).LineTo(-0.1, -0.4);

			tbr.StrokePath(new GraphicsPath().MoveTo(-0.5, 0.15).LineTo(-0.1, -0.4), Colour.FromRgb(240, 246, 249), 0.3, LineCaps.Round);
			tbr.StrokePath(tickpath, Colour.FromRgb(0, 158, 115), 0.2, LineCaps.Round);

			return tbr;
		})();

		/// <summary>
		/// The foreground colour for text elements.
		/// </summary>
		public Colour ForegroundColour { get; set; } = Colours.Black;

		/// <summary>
		/// The background colour for the page.
		/// </summary>
		public Colour BackgroundColour { get; set; } = Colours.White;

		/// <summary>
		/// The colour of the line below headers.
		/// </summary>
		public Colour HeaderLineColour { get; set; } = Colour.FromRgb(180, 180, 180);

		/// <summary>
		/// The colour for thematic break lines.
		/// </summary>
		public Colour ThematicBreakLineColour { get; set; } = Colour.FromRgb(180, 180, 200);

		/// <summary>
		/// The colour for hypertext links.
		/// </summary>
		public Colour LinkColour { get; set; } = Colour.FromRgb(25, 140, 191);

		/// <summary>
		/// The background colour for code inlines.
		/// </summary>
		public Colour CodeInlineBackgroundColour { get; set; } = Colour.FromRgb(240, 240, 240);

		/// <summary>
		/// The background colour for code blocks.
		/// </summary>
		public Colour CodeBlockBackgroundColour { get; set; } = Colour.FromRgb(240, 240, 245);

		/// <summary>
		/// The colour for the bar to the left of block quotes.
		/// </summary>
		public Colour QuoteBlockBarColour { get; set; } = Colour.FromRgb(75, 152, 220);

		/// <summary>
		/// The background colour for block quotes.
		/// </summary>
		public Colour QuoteBlockBackgroundColour { get; set; } = Colour.FromRgb(240, 240, 255);

		/// <summary>
		/// The colour for text that has been styled as "inserted".
		/// </summary>
		public Colour InsertedColour { get; set; } = Colour.FromRgb(0, 158, 115);

		/// <summary>
		/// The colour for text that has been styled as "marked".
		/// </summary>
		public Colour MarkedColour { get; set; } = Colour.FromRgb(213, 94, 0);

		/// <summary>
		/// The colour for the line separating the table header row from normal rows.
		/// </summary>
		public Colour TableHeaderRowSeparatorColour { get; set; } = Colours.Black;

		/// <summary>
		/// The colour for lines separating table rows from each other.
		/// </summary>
		public Colour TableRowSeparatorColour { get; set; } = Colour.FromRgb(180, 180, 180);

		/// <summary>
		/// The thickness of the line separating the table header row from normal rows.
		/// </summary>
		public double TableHeaderRowSeparatorThickness { get; set; } = 2;

		/// <summary>
		/// The thickness of lines separating table rows from each other.
		/// </summary>
		public double TableHeaderSeparatorThickness { get; set; } = 1;

		/// <summary>
		/// Determines whether page breaks should be treated as such in the source.
		/// </summary>
		public bool AllowPageBreak { get; set; } = true;

		/// <summary>
		/// Emoji mapping that transforms emojis (e.g. ":something:") into URLs (e.g., "emoji://something").
		/// </summary>
		public static EmojiMapping EmojiURLMapping { get; } = new EmojiMapping(new Dictionary<string, string>(EmojiMapping.GetDefaultEmojiShortcodeToUnicode().Select(x => new KeyValuePair<string, string>(x.Key, "emoji://" + x.Key.Trim(':')))), EmojiMapping.GetDefaultSmileyToEmojiShortcode());

		/// <summary>
		/// Markdown pipeline builder used to parse markdown source.
		/// </summary>
		public MarkdownPipelineBuilder MarkdownPipelineBuilder { get; set; } = new MarkdownPipelineBuilder().UseGridTables().UsePipeTables().UseEmphasisExtras().UseGenericAttributes().UseAutoIdentifiers().UseAutoLinks().UseTaskLists().UseListExtras().UseCitations().UseMathematics().UseSmartyPants().UseEmojiAndSmiley(EmojiURLMapping).UseUnicodeEmoji();

	}
}
