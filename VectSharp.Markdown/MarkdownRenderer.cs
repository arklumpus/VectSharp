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

using VectSharp.Markdown.CSharpMath.VectSharp;
using Markdig;
using Markdig.Extensions;
using Markdig.Extensions.Tables;
using Markdig.Helpers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VectSharp.Markdown
{
    /// <summary>
    /// Renders Markdown documents into VectSharp graphics objects.
    /// </summary>
    public class MarkdownRenderer
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
        /// The colour for hypertext links-
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
        /// Determines whether page breaks should be treated as such in the source.
        /// </summary>
        public bool AllowPageBreak { get; set; } = true;

        internal MarkdownRenderer Clone()
        {
            return (MarkdownRenderer)this.MemberwiseClone();
        }

        private MathPainter MathPainter = new MathPainter() { DisplayErrorInline = false };

        /// <summary>
        /// Parses the supplied <paramref name="markdownSource"/> using all the supported extensions and renders the resulting document. Page breaks are disabled, and the document is rendered as a single page with the specified <paramref name="width"/>. The page will be cropped at the appropriate height to contain the entire document.
        /// </summary>
        /// <param name="markdownSource">The markdown source to parse.</param>
        /// <param name="width">The width of the page.</param>
        /// <param name="linkDestinations">When this method returns, this value will contain a dictionary used to associate graphic action tags to hyperlinks. This can be used to enable such links when rendering the <see cref="Page"/> to a file.</param>
        /// <param name="headingTree">When this method returns, this object will contain a list of the headings contained in the document and the corresponding tag. This can be used to create a document outline.</param>
        /// <returns>A <see cref="Page"/> containing a rendering of the supplied markdown document.</returns>
        public Page RenderSinglePage(string markdownSource, double width, out Dictionary<string, string> linkDestinations, out List<(int level, string heading, string tag)> headingTree)
        {
            MarkdownDocument document = Markdig.Markdown.Parse(markdownSource, new MarkdownPipelineBuilder().UseGridTables().UsePipeTables().UseEmphasisExtras().UseGenericAttributes().UseAutoIdentifiers().UseAutoLinks().UseTaskLists().UseListExtras().UseCitations().UseMathematics().UseSmartyPants().Build());

            return this.RenderSinglePage(document, width, out linkDestinations, out headingTree);
        }

        /// <summary>
        /// Renders the supplied <paramref name="markdownDocument"/>. Page breaks are disabled, and the document is rendered as a single page with the specified <paramref name="width"/>. The page will be cropped at the appropriate height to contain the entire document.
        /// </summary>
        /// <param name="markdownDocument">The markdown document to render.</param>
        /// <param name="width">The width of the page.</param>
        /// <param name="linkDestinations">When this method returns, this value will contain a dictionary used to associate graphic action tags to hyperlinks. This can be used to enable such links when rendering the <see cref="Page"/> to a file.</param>
        /// <param name="headingTree">When this method returns, this object will contain a list of the headings contained in the document and the corresponding tag. This can be used to create a document outline.</param>
        /// <returns>A <see cref="Page"/> containing a rendering of the supplied markdown document.</returns>
        public Page RenderSinglePage(MarkdownDocument markdownDocument, double width, out Dictionary<string, string> linkDestinations, out List<(int level, string heading, string tag)> headingTree)
        {
            Size prevPageSize = this.PageSize;
            bool allowPageBreak = this.AllowPageBreak;

            this.PageSize = new Size(width, double.PositiveInfinity);
            this.AllowPageBreak = false;

            Page pag = new Page(PageSize.Width, PageSize.Height) { Background = BackgroundColour };

            Graphics graphics = pag.Graphics;

            graphics.Save();

            graphics.Translate(Margins.Left, Margins.Top);

            void newPageAction(ref MarkdownContext mdContext, ref Graphics pageGraphics)
            {

            }

            MarkdownContext context = new MarkdownContext()
            {
                Font = new Font(RegularFontFamily, BaseFontSize),
                Cursor = new Point(0, 0),
                Colour = ForegroundColour,
                Underline = false,
                Translation = new Point(this.Margins.Left, this.Margins.Top),
                CurrentLine = null,
                ListDepth = 0,
                CurrentPage = pag
            };

            int index = 0;
            foreach (Block block in markdownDocument)
            {
                RenderBlock(block, ref context, ref graphics, newPageAction, index > 0, index < markdownDocument.Count - 1);
                index++;
            }

            if (context.CurrentLine != null)
            {
                context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
            }

            graphics.Restore();

            pag.Crop(new Point(0, 0), new Size(width, context.BottomRight.Y + this.Margins.Bottom));

            linkDestinations = context.LinkDestinations;
            headingTree = context.Headings;
            return pag;
        }

        /// <summary>
        /// Parses the supplied <paramref name="markdownSource"/> using all the supported extensions and renders the resulting document. The <see cref="Document"/> produced consists of one or more pages of the size specified in the <see cref="PageSize"/> of the current instance.
        /// </summary>
        /// <param name="markdownSource">The markdown source to parse.</param>
        /// <param name="linkDestinations">When this method returns, this value will contain a dictionary used to associate graphic action tags to hyperlinks. This can be used to enable such links when rendering the <see cref="Document"/> to a file.</param>
        /// <param name="headingTree">When this method returns, this object will contain a list of the headings contained in the document and the corresponding tag. This can be used to create a document outline.</param>
        /// <returns>A <see cref="Document"/> containing a rendering of the supplied markdown document, consisting of one or more pages of the size specified in the <see cref="PageSize"/> of the current instance.</returns>
        public Document Render(string markdownSource, out Dictionary<string, string> linkDestinations, out List<(int level, string heading, string tag)> headingTree)
        {
            MarkdownDocument document = Markdig.Markdown.Parse(markdownSource, new MarkdownPipelineBuilder().UseGridTables().UsePipeTables().UseEmphasisExtras().UseGenericAttributes().UseAutoIdentifiers().UseAutoLinks().UseTaskLists().UseListExtras().UseCitations().UseMathematics().UseSmartyPants().Build());

            return this.Render(document, out linkDestinations, out headingTree);
        }

        /// <summary>
        /// Renders the supplied <paramref name="mardownDocument"/>. The <see cref="Document"/> produced consists of one or more pages of the size specified in the <see cref="PageSize"/> of the current instance.
        /// </summary>
        /// <param name="mardownDocument">The markdown document to render.</param>
        /// <param name="linkDestinations">When this method returns, this object will contain a dictionary used to associate graphic action tags to hyperlinks. This can be used to enable such links when rendering the <see cref="Document"/> to a file.</param>
        /// <param name="headingTree">When this method returns, this object will contain a list of the headings contained in the document and the corresponding tag. This can be used to create a document outline.</param>
        /// <returns>A <see cref="Document"/> containing a rendering of the supplied markdown document, consisting of one or more pages of the size specified in the <see cref="PageSize"/> of the current instance.</returns>
        public Document Render(MarkdownDocument mardownDocument, out Dictionary<string, string> linkDestinations, out List<(int level, string heading, string tag)> headingTree)
        {
            Document doc = new Document();
            Page pag = new Page(PageSize.Width, PageSize.Height) { Background = BackgroundColour };
            doc.Pages.Add(pag);

            Graphics graphics = pag.Graphics;

            graphics.Translate(Margins.Left, Margins.Top);

            void newPageAction(ref MarkdownContext mdContext, ref Graphics pageGraphics)
            {
                Page newPag = new Page(PageSize.Width, PageSize.Height) { Background = BackgroundColour };
                doc.Pages.Add(newPag);

                newPag.Graphics.Translate(mdContext.Translation);
                mdContext.Cursor = new Point(0, 0);
                mdContext.ForbiddenAreasLeft.Clear();
                mdContext.ForbiddenAreasRight.Clear();

                pageGraphics = newPag.Graphics;
                mdContext.CurrentPage = newPag;
            }

            MarkdownContext context = new MarkdownContext()
            {
                Font = new Font(RegularFontFamily, BaseFontSize),
                Cursor = new Point(0, 0),
                Colour = ForegroundColour,
                Underline = false,
                Translation = new Point(Margins.Left, Margins.Top),
                CurrentLine = null,
                ListDepth = 0,
                CurrentPage = pag
            };

            int index = 0;

            foreach (Block block in mardownDocument)
            {
                RenderBlock(block, ref context, ref graphics, newPageAction, index > 0, index < mardownDocument.Count - 1);
                index++;
            }

            if (context.CurrentLine != null)
            {
                context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
            }

            linkDestinations = context.LinkDestinations;
            headingTree = context.Headings;

            return doc;
        }

        private Document RenderSubDocument(ContainerBlock document, ref MarkdownContext context)
        {
            Document doc = new Document();
            Page pag = new Page(PageSize.Width, PageSize.Height) { Background = BackgroundColour };
            doc.Pages.Add(pag);

            Graphics graphics = pag.Graphics;

            graphics.Save();

            graphics.Translate(Margins.Left, Margins.Top);

            void newPageAction(ref MarkdownContext mdContext, ref Graphics pageGraphics)
            {
                pageGraphics.Restore();
                Page newPag = new Page(PageSize.Width, PageSize.Height) { Background = BackgroundColour };
                doc.Pages.Add(newPag);

                newPag.Graphics.Save();
                newPag.Graphics.Translate(mdContext.Translation);
                mdContext.Cursor = new Point(0, 0);
                mdContext.ForbiddenAreasLeft.Clear();
                mdContext.ForbiddenAreasRight.Clear();

                pageGraphics = newPag.Graphics;
                mdContext.CurrentPage = newPag;
            }

            context.Translation = new Point(context.Translation.X + Margins.Left, context.Translation.Y + Margins.Top);
            context.CurrentPage = pag;
            context.ListDepth = 0;

            int index = 0;
            foreach (Block block in document)
            {
                RenderBlock(block, ref context, ref graphics, newPageAction, index > 0, index < document.Count - 1);
                index++;
            }

            if (context.CurrentLine != null)
            {
                context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
            }

            graphics.Restore();

            return doc;
        }

        internal delegate void NewPageAction(ref MarkdownContext context, ref Graphics graphics);

        private void RenderBlock(Block block, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction, bool spaceBefore, bool spaceAfter)
        {
            HtmlAttributes attributes = block.TryGetAttributes();

            string tag = null;

            if (attributes != null && !string.IsNullOrEmpty(attributes.Id))
            {
                Point cursor = context.Cursor;
                NewPageAction reversibleNewPageAction = (ref MarkdownContext currContext, ref Graphics currGraphics) =>
                {
                    newPageAction(ref currContext, ref currGraphics);
                    cursor = currContext.Cursor;
                };

                RenderHTMLBlock("<a name=\"" + attributes.Id + "\"></a>", false, ref context, ref graphics, reversibleNewPageAction, spaceBefore, spaceAfter);
                tag = context.InternalAnchors["#" + attributes.Id];
                context.Cursor = cursor;
            }

            if (block is LeafBlock leaf)
            {
                if (leaf is HeadingBlock heading)
                {
                    string headingText = RenderHeadingBlock(heading, ref context, ref graphics, newPageAction, spaceBefore, spaceAfter);

                    context.Headings.Add((heading.Level, headingText, tag));
                }
                else if (leaf is ParagraphBlock paragraph)
                {
                    RenderParagraphBlock(paragraph, ref context, ref graphics, newPageAction, spaceBefore, spaceAfter);
                }
                else if (leaf is CodeBlock code)
                {
                    if (block is Markdig.Extensions.Mathematics.MathBlock math)
                    {
                        StringBuilder mathBuilder = new StringBuilder();
                        foreach (StringLine line in math.Lines)
                        {
                            mathBuilder.Append(line.ToString());
                            mathBuilder.Append("\n");
                        }

                        byte[] svgData;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            MathPainter.LineStyle = global::CSharpMath.Atom.LineStyle.Display;
                            MathPainter.FontSize = (float)(context.Font.FontSize * MathFontScalingFactor / ImageMultiplier);
                            MathPainter.LaTeX = mathBuilder.ToString();
                            Page pag = MathPainter.DrawToPage();
                            VectSharp.SVG.SVGContextInterpreter.SaveAsSVG(pag, ms);
                            svgData = ms.ToArray();
                        }

                        string imageUri = "<img align=\"center\" src=\"data:image/svg+xml;base64," + Convert.ToBase64String(svgData) + "\">";
                        RenderHTMLBlock(imageUri, false, ref context, ref graphics, newPageAction, true, true);
                    }
                    else if (leaf is FencedCodeBlock fenced)
                    {
                        if (!string.IsNullOrEmpty(fenced.Info))
                        {
                            RenderFencedCodeBlock(fenced, ref context, ref graphics, newPageAction, spaceBefore, spaceAfter);
                        }
                        else
                        {
                            RenderCodeBlock(code, ref context, ref graphics, newPageAction, spaceBefore, spaceAfter);
                        }

                    }
                    else
                    {
                        RenderCodeBlock(code, ref context, ref graphics, newPageAction, spaceBefore, spaceAfter);
                    }
                }
                else if (leaf is HtmlBlock html)
                {
                    RenderHTMLBlock(html.Lines.ToString(), false, ref context, ref graphics, newPageAction, spaceBefore, spaceAfter);
                }
                else if (leaf is ThematicBreakBlock thematicBreak)
                {
                    RenderThematicBreakBlock(thematicBreak, ref context, ref graphics, newPageAction, spaceBefore, spaceAfter);
                }
                else if (leaf is LinkReferenceDefinition link)
                {
                    // Nothing to do (the links are correctly referenced by the parser)
                }

            }
            else if (block is ContainerBlock)
            {
                if (block is ListBlock list)
                {
                    RenderListBlock(list, ref context, ref graphics, newPageAction);
                }
                else if (block is ListItemBlock listItem)
                {
                    RenderListItemBlock(listItem, ref context, ref graphics, newPageAction);
                }
                else if (block is QuoteBlock quote)
                {
                    RenderQuoteBlock(quote, ref context, ref graphics, newPageAction);
                }
                else if (block is LinkReferenceDefinitionGroup linkGroup)
                {
                    // Nothing to render here
                }
                else if (block is Table table)
                {
                    RenderTable(table, ref context, ref graphics, newPageAction);
                }
            }
            else if (block is BlankLineBlock)
            {
                // Nothing to render here
            }
        }

        private string RenderHeadingBlock(HeadingBlock heading, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction, bool spaceBefore, bool spaceAfter)
        {
            MarkdownContext prevContext = context.Clone();

            context.Font = new Font(this.RegularFontFamily, BaseFontSize * HeaderFontSizeMultipliers[heading.Level - 1]);

            double minX = context.GetMinX(context.Cursor.Y + SpaceBeforeHeading * context.Font.FontSize, context.Cursor.Y + context.Font.Ascent + SpaceBeforeHeading * context.Font.FontSize - context.Font.Descent);

            if (spaceBefore)
            {
                context.Cursor = new Point(minX, context.Cursor.Y + context.Font.Ascent + SpaceBeforeHeading * context.Font.FontSize);
            }
            else
            {
                context.Cursor = new Point(minX, context.Cursor.Y + context.Font.Ascent);
            }

            if (context.CurrentLine == null)
            {
                context.CurrentLine = new Line(context.Font.Ascent);
            }
            else
            {
                double delta = context.Cursor.Y - (prevContext.Cursor.Y + prevContext.Font.Ascent + SpaceBeforeParagaph * prevContext.Font.FontSize);

                for (int i = 0; i < context.CurrentLine.Fragments.Count; i++)
                {
                    context.CurrentLine.Fragments[i].Translate(0, delta);
                }
            }

            StringBuilder headingText = new StringBuilder();

            foreach (Inline inline in heading.Inline)
            {
                headingText.Append(RenderInline(inline, ref context, ref graphics, newPageAction));
            }

            if (this.HeaderLineThicknesses[heading.Level - 1] > 0)
            {
                double lineY = context.Cursor.Y + context.Font.FontSize * 0.3;
                context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(minX, context.Cursor.Y + context.Font.FontSize * 0.3), new Point(context.GetMaxX(lineY, PageSize.Width - Margins.Right - context.Translation.X), lineY), this.HeaderLineColour, this.HeaderLineThicknesses[heading.Level - 1], context.Tag));
            }

            context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
            context.CurrentLine = null;

            if (this.HeaderLineThicknesses[heading.Level - 1] > 0)
            {
                double lineY = context.Cursor.Y + context.Font.FontSize * 0.3;
                context.Cursor = new Point(context.Cursor.X, lineY + this.HeaderLineThicknesses[heading.Level - 1]);
            }

            context.Cursor = new Point(0, context.Cursor.Y - context.Font.Descent + SpaceAfterLine * context.Font.FontSize);

            if (spaceAfter)
            {
                context.Cursor = new Point(0, context.Cursor.Y + SpaceAfterHeading * context.Font.FontSize);
            }

            prevContext.Cursor = context.Cursor;
            prevContext.BottomRight = context.BottomRight;
            prevContext.CurrentPage = context.CurrentPage;
            prevContext.CurrentLine = context.CurrentLine;

            context = prevContext;

            return headingText.ToString();
        }

        private void RenderParagraphBlock(ParagraphBlock paragraph, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction, bool spaceBefore, bool spaceAfter)
        {
            double minX = context.GetMinX(context.Cursor.Y + SpaceBeforeParagaph * context.Font.FontSize, context.Cursor.Y + context.Font.Ascent + SpaceBeforeParagaph * context.Font.FontSize - context.Font.Descent);

            if (spaceBefore)
            {
                context.Cursor = new Point(minX, context.Cursor.Y + context.Font.Ascent + SpaceBeforeParagaph * context.Font.FontSize);
            }
            else
            {
                context.Cursor = new Point(minX, context.Cursor.Y + context.Font.Ascent);
            }

            if (context.CurrentLine == null)
            {
                context.CurrentLine = new Line(context.Font.Ascent);
            }

            foreach (Inline inline in paragraph.Inline)
            {
                RenderInline(inline, ref context, ref graphics, newPageAction);
            }

            context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
            context.CurrentLine = null;

            context.Cursor = new Point(0, context.Cursor.Y - context.Font.Descent + SpaceAfterLine * context.Font.FontSize);

            if (spaceAfter)
            {
                context.Cursor = new Point(0, context.Cursor.Y + SpaceAfterParagraph * context.Font.FontSize);
            }
        }

        private void RenderFencedCodeBlock(FencedCodeBlock codeBlock, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction, bool spaceBefore, bool spaceAfter)
        {
            string info = codeBlock.Info;

            if (string.IsNullOrEmpty(info) || codeBlock.Lines.Count == 0)
            {
                RenderCodeBlock(codeBlock, ref context, ref graphics, newPageAction, spaceBefore, spaceAfter);
                return;
            }

            StringBuilder code = new StringBuilder();

            foreach (StringLine line in codeBlock.Lines)
            {
                code.Append(line.ToString());
                code.Append('\n');
            }

            List<List<FormattedString>> lines = this.SyntaxHighlighter(code.ToString(0, code.Length - 1), info);

            if (lines == null)
            {
                RenderCodeBlock(codeBlock, ref context, ref graphics, newPageAction, spaceBefore, spaceAfter);
                return;
            }

            MarkdownContext prevContext = context.Clone();

            context.Font = new Font(this.CodeFont, context.Font.FontSize);

            if (spaceBefore)
            {
                context.Cursor = new Point(0, context.Cursor.Y + SpaceBeforeParagaph * context.Font.FontSize);
            }

            int index = 0;

            if (codeBlock.Lines.Count > 0)
            {
                foreach (List<FormattedString> line in lines)
                {
                    if (index < codeBlock.Lines.Count)
                    {
                        if (context.CurrentLine == null)
                        {
                            context.CurrentLine = new Line(context.Font.Ascent);
                        }

                        double maxX = context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);

                        double minX = context.GetMinX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent);

                        context.Cursor = new Point(minX + context.Font.FontSize, context.Cursor.Y + context.Font.YMax);

                        if (index == 0)
                        {
                            context.CurrentLine.Fragments.Insert(0, new RectangleFragment(new Point(minX, context.Cursor.Y - context.Font.YMax - this.SpaceAfterLine * context.Font.FontSize), new Size(maxX - minX, this.SpaceAfterLine * context.Font.FontSize * 2), CodeBlockBackgroundColour, context.Tag));
                        }

                        foreach (FormattedString item in line)
                        {
                            context.Colour = item.Colour;

                            if (!item.IsBold && !item.IsItalic)
                            {
                                context.Font = new Font(this.CodeFont, context.Font.FontSize);
                            }
                            else if (item.IsBold && !item.IsItalic)
                            {
                                context.Font = new Font(this.CodeFontBold, context.Font.FontSize);
                            }
                            else if (item.IsBold && item.IsItalic)
                            {
                                context.Font = new Font(this.CodeFontBoldItalic, context.Font.FontSize);
                            }
                            else if (!item.IsBold && item.IsItalic)
                            {
                                context.Font = new Font(this.CodeFontItalic, context.Font.FontSize);
                            }

                            RenderCodeBlockLine(item.Text, ref context, ref graphics, newPageAction);
                        }


                        context.Colour = Colours.Black;

                        context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
                        context.CurrentLine = null;

                        context.Cursor = new Point(0, context.Cursor.Y - context.Font.YMin);
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            context.Cursor = new Point(0, context.Cursor.Y + SpaceAfterLine * context.Font.FontSize);

            if (spaceAfter)
            {
                context.Cursor = new Point(0, context.Cursor.Y + SpaceAfterParagraph * context.Font.FontSize);
            }

            prevContext.Cursor = context.Cursor;
            prevContext.BottomRight = context.BottomRight;
            prevContext.CurrentPage = context.CurrentPage;
            prevContext.CurrentLine = context.CurrentLine;

            context = prevContext;
        }


        private void RenderCodeBlock(CodeBlock codeBlock, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction, bool spaceBefore, bool spaceAfter)
        {
            MarkdownContext prevContext = context.Clone();

            context.Font = new Font(this.CodeFont, context.Font.FontSize);

            if (spaceBefore)
            {
                context.Cursor = new Point(0, context.Cursor.Y + SpaceBeforeParagaph * context.Font.FontSize);
            }

            int index = 0;

            if (codeBlock.Lines.Count > 0)
            {
                foreach (StringLine line in codeBlock.Lines)
                {
                    if (index < codeBlock.Lines.Count)
                    {
                        if (context.CurrentLine == null)
                        {
                            context.CurrentLine = new Line(context.Font.Ascent);
                        }

                        double maxX = context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);

                        double minX = context.GetMinX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent);

                        context.Cursor = new Point(minX + context.Font.FontSize, context.Cursor.Y + context.Font.YMax);

                        if (index == 0)
                        {
                            context.CurrentLine.Fragments.Insert(0, new RectangleFragment(new Point(minX, context.Cursor.Y - context.Font.YMax - this.SpaceAfterLine * context.Font.FontSize), new Size(maxX - minX, this.SpaceAfterLine * context.Font.FontSize * 2), CodeBlockBackgroundColour, context.Tag));
                        }


                        RenderCodeBlockLine(line.ToString(), ref context, ref graphics, newPageAction);


                        context.Colour = Colours.Black;

                        context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
                        context.CurrentLine = null;

                        context.Cursor = new Point(0, context.Cursor.Y - context.Font.YMin);
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            context.Cursor = new Point(0, context.Cursor.Y + SpaceAfterLine * context.Font.FontSize);

            if (spaceAfter)
            {
                context.Cursor = new Point(0, context.Cursor.Y + SpaceAfterParagraph * context.Font.FontSize);
            }

            prevContext.Cursor = context.Cursor;
            prevContext.BottomRight = context.BottomRight;
            prevContext.CurrentPage = context.CurrentPage;
            prevContext.CurrentLine = context.CurrentLine;

            context = prevContext;
        }

        private void RenderThematicBreakBlock(ThematicBreakBlock thematicBreak, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction, bool spaceBefore, bool spaceAfter)
        {
            if (context.CurrentLine == null)
            {
                context.CurrentLine = new Line(0);
            }

            if (spaceBefore)
            {
                context.Cursor = new Point(context.Cursor.X, context.Cursor.Y + SpaceBeforeParagaph * context.Font.FontSize + this.ThematicBreakThickness * 0.5);
            }
            else
            {
                context.Cursor = new Point(context.Cursor.X, context.Cursor.Y + this.ThematicBreakThickness * 0.5);
            }


            double maxX = context.GetMaxX(context.Cursor.Y, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);
            double minX = context.GetMinX(context.Cursor.Y);

            context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(minX, context.Cursor.Y), new Point(maxX, context.Cursor.Y), this.ThematicBreakLineColour, this.ThematicBreakThickness, context.Tag));

            context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
            context.CurrentLine = null;

            if (spaceAfter)
            {
                context.Cursor = new Point(context.Cursor.X, context.Cursor.Y + SpaceAfterParagraph * context.Font.FontSize + this.ThematicBreakThickness * 0.5);
            }
            else
            {
                context.Cursor = new Point(context.Cursor.X, context.Cursor.Y + this.ThematicBreakThickness * 0.5);
            }
        }

        private string RenderInline(Inline inline, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            HtmlAttributes attributes = inline.TryGetAttributes();

            if (attributes != null && !string.IsNullOrEmpty(attributes.Id))
            {
                Point cursor = context.Cursor;
                RenderHTMLBlock("<a name=\"" + attributes.Id + "\"></a>", true, ref context, ref graphics, newPageAction, false, false);
                context.Cursor = cursor;
            }

            if (inline is LeafInline)
            {
                if (inline is AutolinkInline autoLink)
                {
                    LinkInline link = new LinkInline((autoLink.IsEmail ? "mailto:" : "") + autoLink.Url, "");
                    link.AppendChild(new LiteralInline(autoLink.Url));

                    return RenderLinkInline(link, ref context, ref graphics, newPageAction);
                }
                else if (inline is CodeInline code)
                {
                    return RenderCodeInline(code, ref context, ref graphics, newPageAction);
                }
                else if (inline is HtmlEntityInline htmlEntity)
                {
                    return RenderLiteralInline(new LiteralInline(htmlEntity.Transcoded), ref context, ref graphics, newPageAction);
                }
                else if (inline is HtmlInline html)
                {
                    RenderHTMLBlock(html.Tag, true, ref context, ref graphics, newPageAction, true, true);
                    return "";
                }
                else if (inline is LineBreakInline lineBreak)
                {
                    RenderLineBreakInline(lineBreak.IsHard, false, ref context, ref graphics, newPageAction);
                    return "\n";
                }
                else if (inline is LiteralInline literal)
                {
                    return RenderLiteralInline(literal, ref context, ref graphics, newPageAction);
                }
                else if (inline is Markdig.Extensions.Mathematics.MathInline math)
                {
                    byte[] svgData;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        if (math.DelimiterCount == 1)
                        {
                            MathPainter.LineStyle = global::CSharpMath.Atom.LineStyle.Text;
                        }
                        else
                        {
                            MathPainter.LineStyle = global::CSharpMath.Atom.LineStyle.Display;
                        }

                        MathPainter.FontSize = (float)(context.Font.FontSize * MathFontScalingFactor / ImageMultiplier);

                        MathPainter.LaTeX = math.Content.ToString();
                        Page pag = MathPainter.DrawToPage();
                        VectSharp.SVG.SVGContextInterpreter.SaveAsSVG(pag, ms);
                        svgData = ms.ToArray();
                    }

                    string imageUri = "<img src=\"data:image/svg+xml;base64," + Convert.ToBase64String(svgData) + "\">";
                    RenderHTMLBlock(imageUri, true, ref context, ref graphics, newPageAction, true, true);
                    return math.Content.ToString();
                }
                else if (inline is Markdig.Extensions.SmartyPants.SmartyPant smartyPant)
                {
                    switch (smartyPant.Type)
                    {
                        case Markdig.Extensions.SmartyPants.SmartyPantType.LeftDoubleQuote:
                            return RenderLiteralInline(new LiteralInline("“"), ref context, ref graphics, newPageAction);
                        case Markdig.Extensions.SmartyPants.SmartyPantType.RightDoubleQuote:
                            return RenderLiteralInline(new LiteralInline("”"), ref context, ref graphics, newPageAction);
                        case Markdig.Extensions.SmartyPants.SmartyPantType.LeftQuote:
                            return RenderLiteralInline(new LiteralInline("‘"), ref context, ref graphics, newPageAction);
                        case Markdig.Extensions.SmartyPants.SmartyPantType.RightQuote:
                            return RenderLiteralInline(new LiteralInline("’"), ref context, ref graphics, newPageAction);
                        case Markdig.Extensions.SmartyPants.SmartyPantType.Dash2:
                            return RenderLiteralInline(new LiteralInline("–"), ref context, ref graphics, newPageAction);
                        case Markdig.Extensions.SmartyPants.SmartyPantType.Dash3:
                            return RenderLiteralInline(new LiteralInline("—"), ref context, ref graphics, newPageAction);
                        case Markdig.Extensions.SmartyPants.SmartyPantType.DoubleQuote:
                            return RenderLiteralInline(new LiteralInline("\""), ref context, ref graphics, newPageAction);
                        case Markdig.Extensions.SmartyPants.SmartyPantType.Ellipsis:
                            return RenderLiteralInline(new LiteralInline("…"), ref context, ref graphics, newPageAction);
                        case Markdig.Extensions.SmartyPants.SmartyPantType.LeftAngleQuote:
                            return RenderLiteralInline(new LiteralInline("«"), ref context, ref graphics, newPageAction);
                        case Markdig.Extensions.SmartyPants.SmartyPantType.Quote:
                            return RenderLiteralInline(new LiteralInline("'"), ref context, ref graphics, newPageAction);
                        case Markdig.Extensions.SmartyPants.SmartyPantType.RightAngleQuote:
                            return RenderLiteralInline(new LiteralInline("»"), ref context, ref graphics, newPageAction);
                        default:
                            return "";
                    }
                }
                else if (inline is Markdig.Extensions.TaskLists.TaskList)
                {
                    // Nothing to render here (the checkbox has already been rendered)
                    return "";
                }
                else
                {
                    return "";
                }
            }
            else if (inline is ContainerInline)
            {
                if (inline is DelimiterInline)
                {
                    // Nothing to render here
                    return "";
                }
                else if (inline is EmphasisInline emphasis)
                {
                    return RenderEmphasisInline(emphasis, ref context, ref graphics, newPageAction);
                }
                else if (inline is LinkInline link)
                {
                    return RenderLinkInline(link, ref context, ref graphics, newPageAction);
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        private void RenderLineBreakInline(bool isHard, bool isPageBreak, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            if (isHard)
            {
                context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
                context.CurrentLine = new Line(context.Font.Ascent);
                context.Cursor = new Point(0, context.Cursor.Y - context.Font.Descent + SpaceAfterLine * context.Font.FontSize + context.Font.Ascent);

                double minX = context.GetMinX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent);

                context.Cursor = new Point(minX, context.Cursor.Y);

                if (isPageBreak)
                {
                    newPageAction(ref context, ref graphics);
                }
            }
            else
            {
                double spaceWidth = context.Font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(' ') / 1000.0 * context.Font.FontSize;
                context.Cursor = new Point(context.Cursor.X + spaceWidth, context.Cursor.Y);
            }
        }

        private string RenderLiteralInline(LiteralInline literal, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            string text = literal.Content.ToString();

            double spaceWidth = context.Font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(' ') / 1000.0 * context.Font.FontSize;

            List<Word> words = Word.GetWords(text, context.Font, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X).ToList();

            double underlineStart = context.Cursor.X;
            double underlineEnd = context.Cursor.X;

            double currLineMaxX = context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);

            bool ignoreNextWhitespace = false;

            bool broken = false;

            for (int i = 0; i < words.Count; i++)
            {
                Word w = words[i];

                if (!string.IsNullOrEmpty(w.Text))
                {
                    if (!ignoreNextWhitespace)
                    {
                        context.Cursor = new Point(context.Cursor.X + spaceWidth * w.WhitespaceCount * (w.PrecedingWhitespace == '\t' ? 4 : 1), context.Cursor.Y);
                    }
                    else
                    {
                        ignoreNextWhitespace = false;
                    }

                    Font.DetailedFontMetrics wordMetrics = w.Metrics;

                    double finalX = context.Cursor.X + wordMetrics.Width + wordMetrics.RightSideBearing + wordMetrics.LeftSideBearing;

                    if (finalX <= currLineMaxX || broken)
                    {
                        context.CurrentLine.Fragments.Add(new TextFragment(new Point(context.Cursor.X + wordMetrics.LeftSideBearing, context.Cursor.Y), w.Text, context.Font, context.Colour, context.Tag));
                        context.Cursor = new Point(context.Cursor.X + wordMetrics.Width + wordMetrics.RightSideBearing + wordMetrics.LeftSideBearing, context.Cursor.Y);

                        broken = false;

                        if (context.Underline || context.StrikeThrough)
                        {
                            underlineEnd = context.Cursor.X;
                        }
                    }
                    else
                    {
                        if (context.Underline && underlineStart != underlineEnd)
                        {
                            context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y + context.Font.FontSize * 0.2), new Point(underlineEnd, context.Cursor.Y + context.Font.FontSize * 0.2), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
                        }
                        else if (context.StrikeThrough && underlineStart != underlineEnd)
                        {
                            context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), new Point(underlineEnd, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
                        }

                        context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);



                        context.CurrentLine = new Line(context.Font.Ascent);
                        context.Cursor = new Point(0, context.Cursor.Y - context.Font.Descent + SpaceAfterLine * context.Font.FontSize + context.Font.Ascent);
                        currLineMaxX = context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);

                        double minX = context.GetMinX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent);

                        context.Cursor = new Point(minX, context.Cursor.Y);

                        underlineStart = minX;
                        underlineEnd = minX;

                        i--;
                        ignoreNextWhitespace = true;
                        broken = true;
                    }
                }
                else
                {
                    context.Cursor = new Point(context.Cursor.X + spaceWidth * w.WhitespaceCount * (w.PrecedingWhitespace == '\t' ? 4 : 1), context.Cursor.Y);
                }
            }

            if (context.Underline && underlineStart != underlineEnd)
            {
                context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y + context.Font.FontSize * 0.2), new Point(underlineEnd, context.Cursor.Y + context.Font.FontSize * 0.2), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
            }
            else if (context.StrikeThrough && underlineStart != underlineEnd)
            {
                context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), new Point(underlineEnd, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
            }

            return text;
        }

        private string RenderCodeInline(CodeInline code, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            MarkdownContext prevContext = context.Clone();

            context.Font = new Font(this.CodeFont, context.Font.FontSize);

            double spaceWidth = context.Font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(' ') / 1000.0 * context.Font.FontSize;

            string text = code.Content.ToString();
            List<Word> words = Word.GetWords(text, context.Font, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X).ToList();

            double currLineMaxX = context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);

            context.Cursor = new Point(context.Cursor.X + this.CodeInlineMargin * context.Font.FontSize, context.Cursor.Y);


            double underlineStart = context.Cursor.X;
            double underlineEnd = context.Cursor.X;

            bool broken = false;

            for (int i = 0; i < words.Count; i++)
            {
                Word w = words[i];

                if (!string.IsNullOrEmpty(w.Text))
                {
                    context.Cursor = new Point(context.Cursor.X + spaceWidth * w.WhitespaceCount * (w.PrecedingWhitespace == '\t' ? 4 : 1), context.Cursor.Y);

                    Font.DetailedFontMetrics wordMetrics = w.Metrics;

                    double finalX = context.Cursor.X + wordMetrics.Width + wordMetrics.RightSideBearing + wordMetrics.LeftSideBearing;

                    if (finalX <= currLineMaxX || broken)
                    {
                        if (i == 0)
                        {
                            context.CurrentLine.Fragments.Add(new RectangleFragment(new Point(context.Cursor.X - this.CodeInlineMargin * context.Font.FontSize, context.Cursor.Y - context.Font.YMax), new Size(this.CodeInlineMargin * context.Font.FontSize, context.Font.YMax - context.Font.YMin), CodeInlineBackgroundColour, context.Tag));
                        }

                        context.CurrentLine.Fragments.Add(new RectangleFragment(new Point(context.Cursor.X - spaceWidth * w.WhitespaceCount * (w.PrecedingWhitespace == '\t' ? 4 : 1), context.Cursor.Y - context.Font.YMax), new Size(wordMetrics.Width + wordMetrics.LeftSideBearing * 2 + wordMetrics.RightSideBearing + spaceWidth * w.WhitespaceCount * (w.PrecedingWhitespace == '\t' ? 4 : 1), context.Font.YMax - context.Font.YMin), CodeInlineBackgroundColour, context.Tag));

                        context.CurrentLine.Fragments.Add(new TextFragment(new Point(context.Cursor.X + wordMetrics.LeftSideBearing, context.Cursor.Y), w.Text, context.Font, context.Colour, context.Tag));
                        context.Cursor = new Point(context.Cursor.X + wordMetrics.Width + wordMetrics.RightSideBearing + wordMetrics.LeftSideBearing, context.Cursor.Y);

                        broken = false;

                        if (context.Underline || context.StrikeThrough)
                        {
                            underlineEnd = context.Cursor.X;
                        }
                    }
                    else
                    {
                        if (context.Underline && underlineStart != underlineEnd)
                        {
                            context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y + context.Font.FontSize * 0.2), new Point(underlineEnd, context.Cursor.Y + context.Font.FontSize * 0.2), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
                        }
                        else if (context.StrikeThrough && underlineStart != underlineEnd)
                        {
                            context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), new Point(underlineEnd, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
                        }

                        context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);

                        context.CurrentLine = new Line(prevContext.Font.Ascent);

                        context.Cursor = new Point(0, context.Cursor.Y - prevContext.Font.Descent + SpaceAfterLine * prevContext.Font.FontSize + prevContext.Font.Ascent);


                        double minX = context.GetMinX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent);

                        context.Cursor = new Point(minX, context.Cursor.Y);

                        if (i == 0)
                        {
                            context.Cursor = new Point(context.Cursor.X + this.CodeInlineMargin * context.Font.FontSize, context.Cursor.Y);
                        }

                        underlineStart = minX;
                        underlineEnd = minX;

                        i--;
                        broken = true;
                    }
                }
            }


            if (context.Underline && underlineStart != underlineEnd)
            {
                context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y + context.Font.FontSize * 0.2), new Point(underlineEnd, context.Cursor.Y + context.Font.FontSize * 0.2), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
            }
            else if (context.StrikeThrough && underlineStart != underlineEnd)
            {
                context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), new Point(underlineEnd, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
            }

            context.CurrentLine.Fragments.Add(new RectangleFragment(new Point(context.Cursor.X, context.Cursor.Y - context.Font.YMax), new Size(this.CodeInlineMargin * context.Font.FontSize, context.Font.YMax - context.Font.YMin), CodeInlineBackgroundColour, context.Tag));

            context.Cursor = new Point(context.Cursor.X + this.CodeInlineMargin * context.Font.FontSize, context.Cursor.Y);

            prevContext.Cursor = context.Cursor;
            prevContext.BottomRight = context.BottomRight;
            prevContext.CurrentPage = context.CurrentPage;
            prevContext.CurrentLine = context.CurrentLine;

            context = prevContext;
            return text;
        }

        private void RenderCodeBlockLine(string text, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            double spaceWidth = context.Font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(' ') / 1000.0 * context.Font.FontSize;

            List<Word> words = Word.GetWords(text, context.Font, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.Font.FontSize * 2 - context.MarginBottomRight.X).ToList();

            double underlineStart = context.Cursor.X;
            double underlineEnd = context.Cursor.X;

            double minX = context.GetMinX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent);

            double currLineMaxX = context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X) - context.Font.FontSize;

            bool broken = false;

            for (int i = 0; i < words.Count; i++)
            {
                Word w = words[i];
                if (!string.IsNullOrEmpty(w.Text))
                {
                    context.Cursor = new Point(context.Cursor.X + spaceWidth * w.WhitespaceCount * (w.PrecedingWhitespace == '\t' ? 4 : 1), context.Cursor.Y);
                    Font.DetailedFontMetrics wordMetrics = w.Metrics;

                    double finalX = context.Cursor.X + wordMetrics.Width + wordMetrics.RightSideBearing + wordMetrics.LeftSideBearing;

                    double effW = wordMetrics.Width + wordMetrics.RightSideBearing + wordMetrics.LeftSideBearing;

                    double maxW = this.PageSize.Width - this.Margins.Right - context.Translation.X - context.Font.FontSize * 2 - spaceWidth * w.WhitespaceCount * (w.PrecedingWhitespace == '\t' ? 4 : 1) - context.MarginBottomRight.X;

                    if (finalX <= currLineMaxX || broken)
                    {
                        broken = false;
                        context.CurrentLine.Fragments.Add(new TextFragment(new Point(context.Cursor.X + wordMetrics.LeftSideBearing, context.Cursor.Y), w.Text, context.Font, context.Colour, context.Tag));
                        context.Cursor = new Point(context.Cursor.X + wordMetrics.Width + wordMetrics.RightSideBearing + wordMetrics.LeftSideBearing, context.Cursor.Y);

                        if (context.Underline || context.StrikeThrough)
                        {
                            underlineEnd = context.Cursor.X;
                        }
                    }
                    else
                    {
                        context.CurrentLine.Fragments.Insert(0, new RectangleFragment(new Point(minX, context.Cursor.Y - context.Font.YMax), new Size(currLineMaxX + context.Font.FontSize - minX, context.Font.YMax - context.Font.YMin + this.SpaceAfterLine * context.Font.FontSize), CodeBlockBackgroundColour, context.Tag));

                        if (context.Underline && underlineStart != underlineEnd)
                        {
                            context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y + context.Font.FontSize * 0.2), new Point(underlineEnd, context.Cursor.Y + context.Font.FontSize * 0.2), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
                        }
                        else if (context.StrikeThrough && underlineStart != underlineEnd)
                        {
                            context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), new Point(underlineEnd, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
                        }

                        context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);

                        underlineStart = 0;
                        underlineEnd = 0;

                        context.CurrentLine = new Line(context.Font.Ascent);

                        context.Cursor = new Point(0, context.Cursor.Y - context.Font.Descent + SpaceAfterLine * context.Font.FontSize + context.Font.Ascent);

                        currLineMaxX = context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X) - context.Font.FontSize;

                        minX = context.GetMinX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent);

                        context.Cursor = new Point(minX + context.Font.FontSize, context.Cursor.Y);

                        i--;
                        broken = true;
                    }
                }
                else
                {
                    context.Cursor = new Point(context.Cursor.X + spaceWidth * w.WhitespaceCount * (w.PrecedingWhitespace == '\t' ? 4 : 1), context.Cursor.Y);
                }
            }

            if (context.Underline && underlineStart != underlineEnd)
            {
                context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y + context.Font.FontSize * 0.2), new Point(underlineEnd, context.Cursor.Y + context.Font.FontSize * 0.2), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
            }
            else if (context.StrikeThrough && underlineStart != underlineEnd)
            {
                context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(underlineStart, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), new Point(underlineEnd, context.Cursor.Y - context.Font.Ascent * 0.5 - context.Font.Descent * 0.5), context.Colour, context.Font.FontSize * (context.Font.FontFamily.IsBold ? this.BoldUnderlineThickness : this.UnderlineThickness), context.Tag));
            }

            context.CurrentLine.Fragments.Insert(0, new RectangleFragment(new Point(minX, context.Cursor.Y - context.Font.YMax), new Size(currLineMaxX + context.Font.FontSize - minX, context.Font.YMax - context.Font.YMin + this.SpaceAfterLine * context.Font.FontSize), CodeBlockBackgroundColour, context.Tag));
        }

        private string RenderEmphasisInline(EmphasisInline emphasis, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            MarkdownContext prevContext = context.Clone();

            Point translationToUndo = new Point(0, 0);

            switch (emphasis.DelimiterChar)
            {
                case '*':
                case '_':
                    if (emphasis.DelimiterCount == 2)
                    {
                        if (context.Font.FontFamily == this.ItalicFontFamily)
                        {
                            context.Font = new Font(this.BoldItalicFontFamily, context.Font.FontSize);
                        }
                        else if (context.Font.FontFamily == this.BoldFontFamily)
                        {
                            context.Font = new Font(this.RegularFontFamily, context.Font.FontSize);
                        }
                        else if (context.Font.FontFamily == this.BoldItalicFontFamily)
                        {
                            context.Font = new Font(this.ItalicFontFamily, context.Font.FontSize);
                        }
                        else
                        {
                            context.Font = new Font(this.BoldFontFamily, context.Font.FontSize);
                        }
                    }
                    else if (emphasis.DelimiterCount == 3)
                    {
                        if (context.Font.FontFamily == this.ItalicFontFamily)
                        {
                            context.Font = new Font(this.BoldFontFamily, context.Font.FontSize);
                        }
                        else if (context.Font.FontFamily == this.BoldFontFamily)
                        {
                            context.Font = new Font(this.ItalicFontFamily, context.Font.FontSize);
                        }
                        else if (context.Font.FontFamily == this.BoldItalicFontFamily)
                        {
                            context.Font = new Font(this.RegularFontFamily, context.Font.FontSize);
                        }
                        else
                        {
                            context.Font = new Font(this.BoldItalicFontFamily, context.Font.FontSize);
                        }
                    }
                    else
                    {
                        if (context.Font.FontFamily == this.ItalicFontFamily)
                        {
                            context.Font = new Font(this.RegularFontFamily, context.Font.FontSize);
                        }
                        else if (context.Font.FontFamily == this.BoldFontFamily)
                        {
                            context.Font = new Font(this.BoldItalicFontFamily, context.Font.FontSize);
                        }
                        else if (context.Font.FontFamily == this.BoldItalicFontFamily)
                        {
                            context.Font = new Font(this.BoldFontFamily, context.Font.FontSize);
                        }
                        else
                        {
                            context.Font = new Font(this.ItalicFontFamily, context.Font.FontSize);
                        }
                    }
                    break;
                case '"':
                    if (emphasis.DelimiterCount == 2)
                    {
                        if (context.Font.FontFamily == this.ItalicFontFamily)
                        {
                            context.Font = new Font(this.RegularFontFamily, context.Font.FontSize);
                        }
                        else if (context.Font.FontFamily == this.BoldFontFamily)
                        {
                            context.Font = new Font(this.BoldItalicFontFamily, context.Font.FontSize);
                        }
                        else if (context.Font.FontFamily == this.BoldItalicFontFamily)
                        {
                            context.Font = new Font(this.BoldFontFamily, context.Font.FontSize);
                        }
                        else
                        {
                            context.Font = new Font(this.ItalicFontFamily, context.Font.FontSize);
                        }
                    }
                    break;
                case '~':
                    if (emphasis.DelimiterCount == 1)
                    {
                        //subscript;
                        context.Cursor = new Point(context.Cursor.X, context.Cursor.Y + context.Font.FontSize * this.SubscriptShift);
                        translationToUndo = new Point(translationToUndo.X, translationToUndo.Y + context.Font.FontSize * this.SubscriptShift);
                        context.Font = new Font(context.Font.FontFamily, context.Font.FontSize * this.SubSuperscriptFontSize);
                    }
                    else
                    {
                        //strikethrough
                        context.StrikeThrough = true;
                    }
                    break;
                case '^':
                    if (emphasis.DelimiterCount == 1)
                    {
                        //superscript
                        context.Cursor = new Point(context.Cursor.X, context.Cursor.Y - context.Font.FontSize * this.SuperscriptShift);
                        translationToUndo = new Point(translationToUndo.X, translationToUndo.Y - context.Font.FontSize * this.SuperscriptShift);
                        context.Font = new Font(context.Font.FontFamily, context.Font.FontSize * this.SubSuperscriptFontSize);
                    }
                    break;
                case '+':
                    context.Colour = this.InsertedColour;
                    break;
                case '=':
                    context.Colour = this.MarkedColour;
                    break;
            }

            StringBuilder text = new StringBuilder();

            foreach (Inline innerInline in emphasis)
            {
                text.Append(RenderInline(innerInline, ref context, ref graphics, newPageAction));
            }            

            if (translationToUndo.X != 0 || translationToUndo.Y != 0)
            {
                context.Cursor = new Point(context.Cursor.X - translationToUndo.X, context.Cursor.Y - translationToUndo.Y);
            }


            prevContext.Cursor = context.Cursor;
            prevContext.BottomRight = context.BottomRight;
            prevContext.CurrentPage = context.CurrentPage;
            prevContext.CurrentLine = context.CurrentLine;

            context = prevContext;

            return text.ToString();
        }

        private string RenderLinkInline(LinkInline link, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            if (!link.IsImage)
            {
                MarkdownContext prevContext = context.Clone();

                context.Colour = this.LinkColour;
                context.Underline = true;
                string tag = Guid.NewGuid().ToString("N");

                if (!link.Url.StartsWith("#"))
                {
                    if (Uri.TryCreate(this.BaseLinkUri, link.Url, out Uri uri))
                    {
                        context.LinkDestinations[tag] = this.LinkUriResolver(uri.ToString());
                    }
                    else
                    {
                        context.LinkDestinations[tag] = this.LinkUriResolver(link.Url);
                    }
                }
                else
                {
                    if (!context.InternalAnchors.TryGetValue(link.Url, out string anchor))
                    {
                        anchor = Guid.NewGuid().ToString("N");
                        context.InternalAnchors[link.Url] = anchor;
                    }

                    context.LinkDestinations[tag] = "#" + anchor;
                }

                context.Tag = tag;

                StringBuilder text = new StringBuilder();

                foreach (Inline innerInline in link)
                {
                    text.Append(RenderInline(innerInline, ref context, ref graphics, newPageAction));
                }

                prevContext.Cursor = context.Cursor;
                prevContext.BottomRight = context.BottomRight;
                prevContext.CurrentPage = context.CurrentPage;
                prevContext.CurrentLine = context.CurrentLine;

                context = prevContext;

                return text.ToString();
            }
            else
            {
                HtmlTag tag = HtmlTag.Parse("<img src=\"" + link.Url.Replace("\"", "\\\"") + "\">").FirstOrDefault();

                RenderHTMLImage(tag, true, ref context, ref graphics, newPageAction);

                return "";
            }
        }

        private void RenderListBlock(ListBlock list, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            MarkdownContext prevContext = context.Clone();

            context.ListDepth++;

            double minX = context.GetMinX(context.Cursor.Y + SpaceBeforeParagaph * context.Font.FontSize, context.Cursor.Y - context.Font.Descent + context.Font.Ascent + SpaceBeforeParagaph * context.Font.FontSize);

            if (context.CurrentLine != null)
            {
                foreach (LineFragment fragment in context.CurrentLine.Fragments)
                {
                    fragment.Translate(-minX - this.IndentWidth, 0);
                }
            }

            graphics.Translate(minX + this.IndentWidth, 0);
            context.Translation = new Point(context.Translation.X + minX + this.IndentWidth, context.Translation.Y);

            foreach (Block block in list)
            {
                RenderBlock(block, ref context, ref graphics, newPageAction, true, true);
            }

            graphics.Translate(-minX - this.IndentWidth, 0);
            context.Translation = new Point(context.Translation.X - minX - this.IndentWidth, context.Translation.Y);

            prevContext.Cursor = context.Cursor;
            prevContext.BottomRight = context.BottomRight;
            prevContext.CurrentPage = context.CurrentPage;
            prevContext.CurrentLine = context.CurrentLine;

            context = prevContext;
        }

        private void RenderQuoteBlock(QuoteBlock quote, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            MarkdownContext prevContext = context.Clone();

            double minX = context.GetMinX(context.Cursor.Y + SpaceBeforeParagaph * context.Font.FontSize, context.Cursor.Y - context.Font.Descent + context.Font.Ascent + SpaceBeforeParagaph * context.Font.FontSize);

            Graphics quoteGraphics = new Graphics();

            quoteGraphics.Translate(minX + this.QuoteBlockIndentWidth, 0);
            context.Translation = new Point(minX + this.QuoteBlockIndentWidth, 0);
            context.MarginBottomRight = new Point(context.MarginBottomRight.X + prevContext.Translation.X, context.MarginBottomRight.Y + prevContext.Translation.Y);

            double maxX = this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X;

            double startY = context.Cursor.Y + context.Font.Ascent - context.Font.YMax;

            Point currTranslation = context.Translation;

            Graphics parentGraphics = graphics;

            NewPageAction newPageActionWithBlockquotes = (ref MarkdownContext currContext, ref Graphics currGraphics) =>
            {
                double currEndY = currContext.Cursor.Y;

                double currMaxX = maxX;

                Graphics currBackgroundGraphics = new Graphics();

                currBackgroundGraphics.Save();

                currBackgroundGraphics.Translate(currContext.Translation);
                currBackgroundGraphics.FillRectangle(new Point(-this.QuoteBlockIndentWidth + this.QuoteBlockBarWidth, startY), new Size(currMaxX + this.QuoteBlockIndentWidth - this.QuoteBlockBarWidth, currEndY - startY), this.QuoteBlockBackgroundColour, tag: currContext.Tag);
                currBackgroundGraphics.FillRectangle(new Point(-this.QuoteBlockIndentWidth, startY), new Size(this.QuoteBlockBarWidth, currEndY - startY), this.QuoteBlockBarColour, tag: currContext.Tag);

                currBackgroundGraphics.Restore();

                currBackgroundGraphics.DrawGraphics(0, 0, currGraphics);

                parentGraphics.DrawGraphics(0, 0, currBackgroundGraphics);

                Point currContextTranslation = currContext.Translation;

                currContext.Translation = prevContext.Translation;

                newPageAction(ref currContext, ref parentGraphics);

                currContext.Translation = currContextTranslation;

                currGraphics = new Graphics();
                currGraphics.Translate(minX + this.QuoteBlockIndentWidth, 0);

                startY = currContext.Cursor.Y + currContext.Font.Ascent - currContext.Font.YMax;
            };

            int index = 0;

            foreach (Block block in quote)
            {
                RenderBlock(block, ref context, ref quoteGraphics, newPageActionWithBlockquotes, true, index < quote.Count - 1);
                index++;
            }

            double endY = context.Cursor.Y;

            Graphics backgroundGraphics = new Graphics();

            backgroundGraphics.Save();

            backgroundGraphics.Translate(context.Translation);
            backgroundGraphics.FillRectangle(new Point(-this.QuoteBlockIndentWidth + this.QuoteBlockBarWidth, startY), new Size(maxX + this.QuoteBlockIndentWidth - this.QuoteBlockBarWidth, endY - startY), this.QuoteBlockBackgroundColour, tag: context.Tag);
            backgroundGraphics.FillRectangle(new Point(-this.QuoteBlockIndentWidth, startY), new Size(this.QuoteBlockBarWidth, endY - startY), this.QuoteBlockBarColour, tag: context.Tag);

            backgroundGraphics.Restore();

            backgroundGraphics.DrawGraphics(0, 0, quoteGraphics);

            parentGraphics.DrawGraphics(0, 0, backgroundGraphics);

            graphics = parentGraphics;

            context.Translation = prevContext.Translation;

            if (!(quote.Parent is QuoteBlock) || quote.Parent.LastChild != quote)
            {
                context.Cursor = new Point(context.Cursor.X, context.Cursor.Y + SpaceAfterParagraph * context.Font.FontSize);
            }

            prevContext.Cursor = context.Cursor;
            prevContext.BottomRight = context.BottomRight;
            prevContext.CurrentPage = context.CurrentPage;
            prevContext.CurrentLine = context.CurrentLine;

            context = prevContext;
        }

        private void RenderListItemBlock(ListItemBlock listItem, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            MarkdownContext prevContext = context.Clone();

            bool isLoose = false;

            double startX = context.Cursor.X;
            double startY = context.Cursor.Y;

            if (listItem.Parent is ListBlock list)
            {
                if (list.IsOrdered)
                {
                    if (context.CurrentLine == null)
                    {
                        context.CurrentLine = new Line(context.Font.Ascent);
                    }

                    string bullet;

                    switch (list.BulletType)
                    {
                        case 'a':
                        case 'A':
                            bullet = ((char)((int)list.DefaultOrderedStart[0] + listItem.Order - 1)).ToString() + list.OrderedDelimiter;
                            break;
                        case 'i':
                            bullet = GetRomanNumeral(listItem.Order).ToLower() + list.OrderedDelimiter;
                            break;
                        case 'I':
                            bullet = GetRomanNumeral(listItem.Order) + list.OrderedDelimiter;
                            break;
                        default:
                            bullet = listItem.Order.ToString() + list.OrderedDelimiter;
                            break;
                    }

                    if (list.IsLoose)
                    {
                        isLoose = true;
                        context.CurrentLine.Fragments.Add(new TextFragment(new Point(context.Cursor.X - context.Font.MeasureText(bullet).Width - this.IndentWidth * 0.15, context.Cursor.Y + context.Font.Ascent + SpaceBeforeParagaph * context.Font.FontSize), bullet, context.Font, context.Colour, context.Tag));
                    }
                    else
                    {
                        isLoose = false;
                        context.CurrentLine.Fragments.Add(new TextFragment(new Point(context.Cursor.X - context.Font.MeasureText(bullet).Width - this.IndentWidth * 0.15, context.Cursor.Y + context.Font.Ascent), bullet, context.Font, context.Colour, context.Tag));
                    }
                }
                else
                {
                    if (listItem.Count > 0 && listItem[0] is ParagraphBlock paragraph && paragraph.Inline?.FirstChild is Markdig.Extensions.TaskLists.TaskList task)
                    {
                        if (context.CurrentLine == null)
                        {
                            context.CurrentLine = new Line(context.Font.Ascent);
                        }

                        Graphics bullet = new Graphics();
                        bullet.Scale(context.Font.FontSize, context.Font.FontSize);

                        if (task.Checked)
                        {
                            bullet.DrawGraphics(0, 0, this.TaskListCheckedBullet);
                        }
                        else
                        {
                            bullet.DrawGraphics(0, 0, this.TaskListUncheckedBullet);
                        }

                        if (list.IsLoose)
                        {
                            isLoose = true;
                            context.CurrentLine.Fragments.Add(new GraphicsFragment(new Point(context.Cursor.X - this.IndentWidth * 0.15, context.Cursor.Y + context.Font.Ascent + SpaceBeforeParagaph * context.Font.FontSize - context.Font.Descent * 0.5 - (context.Font.Ascent - context.Font.Descent) * 0.5), bullet, 0));
                        }
                        else
                        {
                            isLoose = false;
                            context.CurrentLine.Fragments.Add(new GraphicsFragment(new Point(context.Cursor.X - this.IndentWidth * 0.15, context.Cursor.Y + context.Font.Ascent - context.Font.Descent * 0.5 - (context.Font.Ascent - context.Font.Descent) * 0.5), bullet, 0));
                        }
                    }
                    else
                    {
                        if (context.CurrentLine == null)
                        {
                            context.CurrentLine = new Line(context.Font.Ascent);
                        }

                        Graphics bullet = new Graphics();
                        bullet.Scale(context.Font.FontSize, context.Font.FontSize);
                        this.Bullets[(context.ListDepth - 1) % this.Bullets.Count](bullet, context.Colour);

                        if (list.IsLoose)
                        {
                            isLoose = true;
                            context.CurrentLine.Fragments.Add(new GraphicsFragment(new Point(context.Cursor.X - this.IndentWidth * 0.15, context.Cursor.Y + context.Font.Ascent + SpaceBeforeParagaph * context.Font.FontSize - context.Font.Descent * 0.5 - (context.Font.Ascent - context.Font.Descent) * 0.5), bullet, 0));
                        }
                        else
                        {
                            isLoose = false;
                            context.CurrentLine.Fragments.Add(new GraphicsFragment(new Point(context.Cursor.X - this.IndentWidth * 0.15, context.Cursor.Y + context.Font.Ascent - context.Font.Descent * 0.5 - (context.Font.Ascent - context.Font.Descent) * 0.5), bullet, 0));
                        }
                    }
                }
            }

            foreach (Block block in listItem)
            {
                RenderBlock(block, ref context, ref graphics, newPageAction, isLoose || listItem == listItem.Parent[0], isLoose || listItem == listItem.Parent.LastChild);
            }

            prevContext.Cursor = context.Cursor;
            prevContext.BottomRight = context.BottomRight;
            prevContext.CurrentPage = context.CurrentPage;
            prevContext.CurrentLine = context.CurrentLine;

            context = prevContext;
        }

        private void RenderHTMLBlock(string html, bool isInline, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction, bool spaceBefore, bool spaceAfter)
        {
            if (!isInline)
            {
                double minX = context.GetMinX(context.Cursor.Y + SpaceBeforeParagaph * context.Font.FontSize, context.Cursor.Y + context.Font.Ascent + SpaceBeforeParagaph * context.Font.FontSize - context.Font.Descent);

                context.Cursor = new Point(minX, context.Cursor.Y + context.Font.Ascent + SpaceBeforeParagaph * context.Font.FontSize);

                if (context.CurrentLine == null)
                {
                    context.CurrentLine = new Line(context.Font.Ascent);
                }
            }

            foreach (HtmlTag tag in HtmlTag.Parse(html))
            {
                if (tag.Tag.Equals("img", StringComparison.OrdinalIgnoreCase) || tag.Tag.Equals("image", StringComparison.OrdinalIgnoreCase))
                {
                    RenderHTMLImage(tag, isInline, ref context, ref graphics, newPageAction);
                }
                else if (tag.Tag.Equals("br", StringComparison.OrdinalIgnoreCase))
                {
                    RenderLineBreakInline(true, tag.Attributes.TryGetValue("type", out string typeValue) && typeValue.Equals("page", StringComparison.OrdinalIgnoreCase) && this.AllowPageBreak, ref context, ref graphics, newPageAction);
                }
                else if (tag.Tag.Equals("a"))
                {
                    if (tag.Attributes.TryGetValue("name", out string anchorName))
                    {
                        if (!context.InternalAnchors.TryGetValue("#" + anchorName, out string anchor))
                        {
                            anchor = Guid.NewGuid().ToString("N");
                            context.InternalAnchors["#" + anchorName] = anchor;
                        }

                        if (context.CurrentLine == null)
                        {
                            context.CurrentLine = new Line(0);
                        }

                        double anchorHeight = this.BaseFontSize * Math.Max(1, this.HeaderFontSizeMultipliers.Max());

                        context.CurrentLine.Fragments.Add(new RectangleFragment(new Point(context.Cursor.X, context.Cursor.Y - anchorHeight), new Size(10, anchorHeight), Colour.FromRgba(0, 0, 0, 0), anchor));
                    }
                }
                else
                {

                }
            }

            if (!isInline)
            {
                if (context.CurrentLine != null)
                {
                    context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
                    context.CurrentLine = null;

                    context.Cursor = new Point(0, context.Cursor.Y - context.Font.Descent + SpaceAfterLine * context.Font.FontSize);

                    context.Cursor = new Point(0, context.Cursor.Y + SpaceAfterParagraph * context.Font.FontSize);
                }
            }
        }

        private void RenderHTMLImage(HtmlTag imgTag, bool isInline, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            if (imgTag != null)
            {
                if (imgTag.Attributes.TryGetValue("src", out string imageSrc))
                {
                    (string imageFile, bool wasDownloaded) = this.ImageUriResolver(imageSrc, this.BaseImageUri);

                    Page imagePage = null;

                    if (imageFile != null)
                    {
                        if (System.IO.Path.GetExtension(imageFile) == ".svg")
                        {
                            try
                            {
                                imagePage = VectSharp.SVG.Parser.FromFile(imageFile);
                            }
                            catch
                            {
                                imagePage = null;
                            }
                        }
                        else if (RasterImageLoader != null)
                        {
                            try
                            {
                                RasterImage raster = RasterImageLoader(imageFile);
                                imagePage = new Page(raster.Width, raster.Height);
                                imagePage.Graphics.DrawRasterImage(0, 0, raster, tag: context.Tag);
                            }
                            catch
                            {
                                imagePage = null;
                            }
                        }


                        if (wasDownloaded)
                        {
                            System.IO.File.Delete(imageFile);
                            System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(imageFile));
                        }
                    }
                    else if (imageSrc.StartsWith("data:"))
                    {
                        try
                        {
                            imagePage = VectSharp.SVG.Parser.ParseImageURI(imageSrc, false);
                        }
                        catch
                        {

                            imagePage = null;
                        }
                    }

                    if (imagePage != null)
                    {
                        double scaleX = 1;
                        double scaleY = 1;

                        bool hasWidth = false;
                        bool hasHeight = false;

                        if (imgTag.Attributes.TryGetValue("width", out string widthString) && double.TryParse(widthString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double width))
                        {
                            hasWidth = true;
                            scaleX = width * this.ImageUnitMultiplier / imagePage.Width;
                        }

                        if (imgTag.Attributes.TryGetValue("height", out string heightString) && double.TryParse(heightString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double height))
                        {
                            hasHeight = true;
                            scaleY = height * this.ImageUnitMultiplier / imagePage.Height;
                        }

                        if (hasWidth && !hasHeight)
                        {
                            scaleY = scaleX;
                        }
                        else if (hasHeight && !hasWidth)
                        {
                            scaleX = scaleY;
                        }

                        scaleX *= this.ImageMultiplier;
                        scaleY *= this.ImageMultiplier;

                        if (!isInline)
                        {
                            string alignValue;

                            if (!imgTag.Attributes.TryGetValue("align", out alignValue))
                            {
                                alignValue = null;
                            }

                            if (alignValue == "center")
                            {
                                if (context.CurrentLine != null)
                                {
                                    context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
                                    context.CurrentLine = null;
                                }

                                double minX = context.GetMinX(context.Cursor.Y + context.Translation.Y, context.Cursor.Y + context.Translation.Y + scaleY * imagePage.Height);
                                double maxX = context.GetMaxX(context.Cursor.Y + context.Translation.Y, context.Cursor.Y + context.Translation.Y + scaleY * imagePage.Height, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);

                                if (scaleX * imagePage.Width > maxX - minX)
                                {
                                    scaleX = (maxX - minX) / imagePage.Width;
                                    scaleY = scaleX;
                                }

                                double finalY = context.Cursor.Y + scaleY * imagePage.Height;

                                if (finalY + context.Translation.Y > this.PageSize.Height - this.Margins.Bottom + this.ImageMarginTolerance - context.MarginBottomRight.Y)
                                {
                                    newPageAction(ref context, ref graphics);
                                    finalY = context.Cursor.Y + scaleY * imagePage.Height;
                                }

                                graphics.Save();
                                graphics.Translate((minX + maxX - scaleX * imagePage.Width) * 0.5, context.Cursor.Y);
                                graphics.Scale(scaleX, scaleY);
                                graphics.SetClippingPath(0, 0, imagePage.Width, imagePage.Height);
                                graphics.DrawGraphics(0, 0, imagePage.Graphics);

                                graphics.Restore();

                                context.Cursor = new Point(0, finalY + SpaceAfterParagraph * context.Font.FontSize + SpaceAfterLine * context.Font.FontSize);
                            }
                            else if (alignValue == "right")
                            {
                                if (context.CurrentLine != null)
                                {
                                    context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
                                    context.CurrentLine = null;
                                }

                                double finalY = context.Cursor.Y + scaleY * imagePage.Height;

                                if (finalY + context.Translation.Y > this.PageSize.Height - this.Margins.Bottom + this.ImageMarginTolerance - context.MarginBottomRight.Y)
                                {
                                    newPageAction(ref context, ref graphics);
                                    finalY = context.Cursor.Y + scaleY * imagePage.Height;
                                }

                                graphics.Save();
                                graphics.Translate(this.PageSize.Width - this.Margins.Right - context.Translation.X - scaleX * imagePage.Width - context.MarginBottomRight.X, context.Cursor.Y);
                                graphics.Scale(scaleX, scaleY);
                                graphics.SetClippingPath(0, 0, imagePage.Width, imagePage.Height);

                                graphics.DrawGraphics(0, 0, imagePage.Graphics);

                                graphics.Restore();

                                context.ForbiddenAreasRight.Add((this.PageSize.Width - this.Margins.Right - scaleX * imagePage.Width - this.ImageSideMargin - context.MarginBottomRight.X, context.Cursor.Y + context.Translation.Y, context.Cursor.Y + context.Translation.Y + scaleY * imagePage.Height));
                            }
                            else if (alignValue == "left")
                            {
                                if (context.CurrentLine != null)
                                {
                                    context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
                                    context.CurrentLine = null;
                                }

                                double finalY = context.Cursor.Y + scaleY * imagePage.Height;

                                if (finalY + context.Translation.Y > this.PageSize.Height - this.Margins.Bottom + this.ImageMarginTolerance - context.MarginBottomRight.Y)
                                {
                                    newPageAction(ref context, ref graphics);
                                    finalY = context.Cursor.Y + scaleY * imagePage.Height;
                                }

                                graphics.Save();
                                graphics.Translate(0, context.Cursor.Y);
                                graphics.Scale(scaleX, scaleY);
                                graphics.SetClippingPath(0, 0, imagePage.Width, imagePage.Height);

                                graphics.DrawGraphics(0, 0, imagePage.Graphics);

                                graphics.Restore();

                                context.ForbiddenAreasLeft.Add((scaleX * imagePage.Width + context.Translation.X + this.ImageSideMargin, context.Cursor.Y + context.Translation.Y, context.Cursor.Y + context.Translation.Y + scaleY * imagePage.Height));
                            }
                            else
                            {
                                isInline = true;
                            }
                        }

                        if (isInline)
                        {
                            Graphics scaledImage = new Graphics();
                            scaledImage.Scale(scaleX, scaleY);
                            scaledImage.DrawGraphics(0, 0, imagePage.Graphics);

                            if (context.CurrentLine == null)
                            {
                                context.CurrentLine = new Line(context.Font.Ascent);

                                context.Cursor = new Point(0, context.Cursor.Y);
                                double minX = context.GetMinX(context.Cursor.Y - scaleY * imagePage.Height, context.Cursor.Y);
                                context.Cursor = new Point(minX, context.Cursor.Y);
                            }

                            double currLineMaxX = context.GetMaxX(context.Cursor.Y - scaleY * imagePage.Height, context.Cursor.Y, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);

                            double finalX = context.Cursor.X + imagePage.Width;

                            if (finalX > currLineMaxX)
                            {
                                context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);

                                context.CurrentLine = new Line(context.Font.Ascent);
                                context.Cursor = new Point(0, context.Cursor.Y - context.Font.Descent + SpaceAfterLine * context.Font.FontSize + context.Font.Ascent);
                                currLineMaxX = context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);

                                double minX = context.GetMinX(context.Cursor.Y - scaleY * imagePage.Height * scaleY, context.Cursor.Y);

                                context.Cursor = new Point(minX, context.Cursor.Y);
                            }

                            context.CurrentLine.Fragments.Add(new GraphicsFragment(new Point(context.Cursor.X, context.Cursor.Y - scaleY * imagePage.Height), scaledImage, scaleY * imagePage.Height));
                            context.Cursor = new Point(context.Cursor.X + scaleX * imagePage.Width, context.Cursor.Y);
                        }
                    }
                }
            }
        }

        private void RenderTable(Table table, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            if (table.Count > 0)
            {
                if (!table.IsValid())
                {
                    table.NormalizeUsingMaxWidth();
                    table.NormalizeUsingHeaderRow();
                }

                if (table.IsValid() && table.ColumnDefinitions?.Count > 0)
                {
                    bool isGridTable = false;

                    foreach (TableColumnDefinition def in table.ColumnDefinitions)
                    {
                        if (def.Width > 0)
                        {
                            isGridTable = true;
                            break;
                        }
                    }

                    if (isGridTable)
                    {
                        int maxColumns = 0;

                        foreach (TableRow row in table)
                        {
                            maxColumns = Math.Max(maxColumns, ((TableCell)row.Last()).ColumnIndex + 1);
                        }

                        if (table.ColumnDefinitions.Count > maxColumns)
                        {
                            table.ColumnDefinitions.RemoveRange(maxColumns, table.ColumnDefinitions.Count - maxColumns);
                        }
                    }
                    else
                    {
                        int maxColumns = 0;

                        foreach (TableRow row in table)
                        {
                            maxColumns = Math.Max(maxColumns, row.Count);
                        }

                        if (table.ColumnDefinitions.Count > maxColumns)
                        {
                            table.ColumnDefinitions.RemoveRange(maxColumns, table.ColumnDefinitions.Count - maxColumns);
                        }
                    }

                    double[] columnWidths = new double[table.ColumnDefinitions.Count];

                    if (table.ColumnDefinitions.Count == 0)
                    {
                        int columnCount = 0;
                        foreach (TableRow row in table)
                        {
                            columnCount = Math.Max(row.Count, columnCount);
                        }

                        columnWidths = new double[columnCount];
                    }

                    int missingColumns = columnWidths.Length;

                    for (int i = 0; i < columnWidths.Length; i++)
                    {
                        columnWidths[i] = double.NaN;
                    }

                    double remainingPerc = 1;

                    for (int i = 0; i < table.ColumnDefinitions.Count; i++)
                    {
                        if (table.ColumnDefinitions[i].Width > 0)
                        {
                            missingColumns--;
                            remainingPerc -= table.ColumnDefinitions[i].Width / 100.0;
                            columnWidths[i] = table.ColumnDefinitions[i].Width / 100.0;
                        }
                    }

                    if (missingColumns > 0)
                    {
                        remainingPerc /= missingColumns;
                        for (int i = 0; i < columnWidths.Length; i++)
                        {
                            if (double.IsNaN(columnWidths[i]))
                            {
                                columnWidths[i] = remainingPerc;
                            }
                        }
                    }

                    double maxX = context.GetMaxX(context.Cursor.Y, context.Cursor.Y, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);

                    for (int i = 0; i < columnWidths.Length; i++)
                    {
                        columnWidths[i] *= maxX;
                    }

                    int index = 0;

                    foreach (TableRow row in table)
                    {
                        RenderTableRow(row, columnWidths, index == table.Count - 1, ref context, ref graphics, newPageAction);
                        index++;
                    }

                    context.Cursor = new Point(context.Cursor.X, context.Cursor.Y + SpaceAfterParagraph * context.Font.FontSize);
                }
            }
        }

        private void RenderTableRow(TableRow row, double[] columnWidths, bool isLastRow, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            if (context.CurrentLine == null)
            {
                context.CurrentLine = new Line(0);
            }

            int index = 0;

            foreach (TableCell cell in row)
            {
                if (cell.ColumnIndex < 0)
                {
                    cell.ColumnIndex = index;
                    index += cell.ColumnSpan;
                }
                else
                {
                    index = cell.ColumnIndex + cell.ColumnSpan;
                }
            }

            double maxHeight = 0;
            double startX = context.Cursor.X;

            MarkdownContext prevContext = context.Clone();

            if (row.IsHeader)
            {
                context.Font = new Font(this.BoldFontFamily, context.Font.FontSize);
            }


            foreach (TableCell cell in row)
            {
                double cellWidth = 0;

                for (int i = 0; i < cell.ColumnSpan && cell.ColumnIndex + i < columnWidths.Length; i++)
                {
                    cellWidth += columnWidths[cell.ColumnIndex + i];
                }

                Page cellPage = RenderTableCell(cell, cellWidth, ref context, ref graphics, newPageAction);

                double prevMaxHeight = maxHeight;
                maxHeight = Math.Max(maxHeight, cellPage.Height);

                context.CurrentLine.Fragments.Add(new GraphicsFragment(new Point(context.Cursor.X, context.Cursor.Y - cellPage.Height), cellPage.Graphics, cellPage.Height));

                context.Cursor = new Point(context.Cursor.X + cellWidth, context.Cursor.Y);
            }

            if (this.TableVAlign == VerticalAlignment.Top)
            {
                for (int i = 0; i < context.CurrentLine.Fragments.Count; i++)
                {
                    context.CurrentLine.Fragments[i].Translate(0, -maxHeight + ((GraphicsFragment)context.CurrentLine.Fragments[i]).Ascent);
                }
            }
            else if (this.TableVAlign == VerticalAlignment.Middle)
            {
                for (int i = 0; i < context.CurrentLine.Fragments.Count; i++)
                {
                    context.CurrentLine.Fragments[i].Translate(0, (-maxHeight + ((GraphicsFragment)context.CurrentLine.Fragments[i]).Ascent) * 0.5);
                }
            }

            context.CurrentLine.Fragments.Add(new UnderlineFragment(new Point(startX, context.Cursor.Y), new Point(columnWidths.Sum() + startX, context.Cursor.Y), row.IsHeader ? this.TableHeaderRowSeparatorColour : this.TableRowSeparatorColour, row.IsHeader ? this.TableHeaderRowSeparatorThickness : this.TableHeaderSeparatorThickness, context.Tag));

            context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);
            context.CurrentLine = null;

            context.Cursor = new Point(startX, context.Cursor.Y);

            context.Cursor = new Point(startX, context.Cursor.Y + SpaceAfterLine * context.Font.FontSize + (row.IsHeader ? this.TableHeaderRowSeparatorThickness : this.TableHeaderSeparatorThickness));

            prevContext.Cursor = context.Cursor;
            prevContext.BottomRight = context.BottomRight;
            prevContext.CurrentPage = context.CurrentPage;
            prevContext.CurrentLine = context.CurrentLine;

            context = prevContext;
        }

        private Page RenderTableCell(TableCell cell, double cellWidth, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            MarkdownRenderer clonedRenderer = this.Clone();
            clonedRenderer.PageSize = new Size(cellWidth, double.PositiveInfinity);
            clonedRenderer.Margins = this.TableCellMargins;

            MarkdownContext clonedContext = context.Clone();
            clonedContext.Translation = new Point(0, 0);
            clonedContext.Cursor = new Point(0, 0);
            clonedContext.BottomRight = new Point(0, 0);
            clonedContext.CurrentLine = null;
            clonedContext.CurrentPage = null;
            clonedContext.ForbiddenAreasLeft = new List<(double MinX, double MinY, double MaxY)>();
            clonedContext.ForbiddenAreasRight = new List<(double MinX, double MinY, double MaxY)>();

            Page cellPage = clonedRenderer.RenderSubDocument(cell, ref clonedContext).Pages[0];

            cellPage.Crop(new Point(0, 0), new Size(cellWidth, clonedContext.BottomRight.Y + this.TableCellMargins.Bottom));

            return cellPage;
        }

        static (int, string)[] RomanNumbers = new (int, string)[] { (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"), (100, "C"), (90, "XC"), (50, "L"), (40, "XL"), (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I") };

        private static string GetRomanNumeral(int number)
        {
            StringBuilder tbr = new StringBuilder();

            for (int i = 0; i < RomanNumbers.Length; i++)
            {
                while (number >= RomanNumbers[i].Item1)
                {
                    tbr.Append(RomanNumbers[i].Item2);
                    number -= RomanNumbers[i].Item1;
                }
            }

            return tbr.ToString();
        }
    }
}