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
using Markdig.Extensions.Emoji;

namespace VectSharp.Markdown
{
    /// <summary>
    /// Renders Markdown documents into VectSharp graphics objects.
    /// </summary>
    public partial class MarkdownRenderer
    {

        /// <summary>
        /// Create a new <see cref="MarkdownRenderer"/>;
        /// </summary>
        public MarkdownRenderer()
        {
            this.EmojiUriResolver = x => HTTPUtils.ResolveEmojiUsingOpenMoji(x, this);
        }

        internal MarkdownRenderer Clone()
        {
            return (MarkdownRenderer)this.MemberwiseClone();
        }

        private MathPainter MathPainter = new MathPainter() { DisplayErrorInline = false };

        internal delegate void NewPageAction(ref MarkdownContext context, ref Graphics graphics);

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
            MarkdownDocument document = Markdig.Markdown.Parse(markdownSource, this.MarkdownPipelineBuilder.Build());

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

            static void newPageAction(ref MarkdownContext mdContext, ref Graphics pageGraphics)
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

            context.CurrentLine?.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);

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
            MarkdownDocument document = Markdig.Markdown.Parse(markdownSource, this.MarkdownPipelineBuilder.Build());

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

            context.CurrentLine?.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y);

            linkDestinations = context.LinkDestinations;
            headingTree = context.Headings;

            return doc;
        }

        private void RenderHTMLImage(HtmlTag imgTag, bool isInline, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            if (imgTag != null)
            {
                if (imgTag.Attributes.TryGetValue("src", out string imageSrc))
                {
                    string imageFile;
                    bool wasDownloaded;

                    if (imageSrc.StartsWith("emoji://") || imageSrc.StartsWith("unicode://"))
                    {
                        (imageFile, wasDownloaded) = this.EmojiUriResolver(imageSrc);
                    }
                    else
                    {
                        (imageFile, wasDownloaded) = this.ImageUriResolver(imageSrc, this.BaseImageUri);
                    }

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
                            if (!imgTag.Attributes.TryGetValue("align", out string alignValue))
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