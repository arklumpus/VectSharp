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

using Markdig.Helpers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;
using Markdig.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using VectSharp.Markdown.CSharpMath.VectSharp;
using System.Linq;
using Markdig.Extensions.Tables;

namespace VectSharp.Markdown
{
    public partial class MarkdownRenderer
    {
        private void RenderBlock(Block block, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction, bool spaceBefore, bool spaceAfter)
        {
            this.OnBlockRendering(ref context, ref graphics, ref block);

            if (block != null)
            {
                HtmlAttributes attributes = block.TryGetAttributes();

                string tag = null;

                if (attributes != null && !string.IsNullOrEmpty(attributes.Id))
                {
                    Point cursor = context.Cursor;

                    void reversibleNewPageAction(ref MarkdownContext currContext, ref Graphics currGraphics)
                    {
                        newPageAction(ref currContext, ref currGraphics);
                        cursor = currContext.Cursor;
                    }

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

                this.OnBlockRendered(ref context, ref graphics, block);
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

            context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y, context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X), this);
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

            context.CurrentLine ??= new Line(context.Font.Ascent);

            foreach (Inline inline in paragraph.Inline)
            {
                RenderInline(inline, ref context, ref graphics, newPageAction);
            }

            context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y, context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X), this);
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
                        context.CurrentLine ??= new Line(context.Font.Ascent);

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

                        context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y, context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X), this);
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
                        context.CurrentLine ??= new Line(context.Font.Ascent);

                        double maxX = context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X);

                        double minX = context.GetMinX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent);

                        context.Cursor = new Point(minX + context.Font.FontSize, context.Cursor.Y + context.Font.YMax);

                        if (index == 0)
                        {
                            context.CurrentLine.Fragments.Insert(0, new RectangleFragment(new Point(minX, context.Cursor.Y - context.Font.YMax - this.SpaceAfterLine * context.Font.FontSize), new Size(maxX - minX, this.SpaceAfterLine * context.Font.FontSize * 2), CodeBlockBackgroundColour, context.Tag));
                        }


                        RenderCodeBlockLine(line.ToString(), ref context, ref graphics, newPageAction);


                        context.Colour = Colours.Black;

                        context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y, context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X), this);
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
            context.CurrentLine ??= new Line(0);

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

            context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y, context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X), this);
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

            void newPageActionWithBlockquotes(ref MarkdownContext currContext, ref Graphics currGraphics)
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
            }

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
                    context.CurrentLine ??= new Line(context.Font.Ascent);

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
                        context.CurrentLine ??= new Line(context.Font.Ascent);

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
                        context.CurrentLine ??= new Line(context.Font.Ascent);

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

                context.CurrentLine ??= new Line(context.Font.Ascent);
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

                        context.CurrentLine ??= new Line(0);

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
                    context.CurrentLine.Render(ref graphics, ref context, newPageAction, this.PageSize.Height - this.Margins.Bottom - context.Translation.Y - context.MarginBottomRight.Y, context.GetMaxX(context.Cursor.Y - context.Font.Ascent, context.Cursor.Y - context.Font.Descent, this.PageSize.Width - this.Margins.Right - context.Translation.X - context.MarginBottomRight.X), this);
                    context.CurrentLine = null;

                    context.Cursor = new Point(0, context.Cursor.Y - context.Font.Descent + SpaceAfterLine * context.Font.FontSize);

                    context.Cursor = new Point(0, context.Cursor.Y + SpaceAfterParagraph * context.Font.FontSize);
                }
            }
        }

    }
}
