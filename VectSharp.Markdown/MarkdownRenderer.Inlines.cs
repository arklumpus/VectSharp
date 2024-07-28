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

using Markdig.Extensions.Emoji;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;
using Markdig.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;
using VectSharp.Markdown.CSharpMath.VectSharp;

namespace VectSharp.Markdown
{
    public partial class MarkdownRenderer
    {
        private string RenderInline(Inline inline, ref MarkdownContext context, ref Graphics graphics, NewPageAction newPageAction)
        {
            HtmlAttributes attributes = inline.TryGetAttributes();

            if (attributes != null && !string.IsNullOrEmpty(attributes.Id))
            {
                Point cursor = context.Cursor;
                RenderHTMLBlock("<a name=\"" + attributes.Id + "\"></a>", true, ref context, ref graphics, newPageAction, false, false);
                context.Cursor = cursor;
            }

            if (inline is EmojiInline emoji)
            {
                int headingLevel = 0;
                Block currBlock = emoji.Parent?.ParentBlock;
                while (currBlock != null)
                {
                    if (currBlock is HeadingBlock heading)
                    {
                        headingLevel = heading.Level;
                        break;
                    }
                    currBlock = currBlock.Parent;
                }

                inline = new LinkInline(emoji.Content.ToString() + "_heading:" + headingLevel.ToString(), "") { IsImage = true };
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
                    return smartyPant.Type switch
                    {
                        Markdig.Extensions.SmartyPants.SmartyPantType.LeftDoubleQuote => RenderLiteralInline(new LiteralInline("“"), ref context, ref graphics, newPageAction),
                        Markdig.Extensions.SmartyPants.SmartyPantType.RightDoubleQuote => RenderLiteralInline(new LiteralInline("”"), ref context, ref graphics, newPageAction),
                        Markdig.Extensions.SmartyPants.SmartyPantType.LeftQuote => RenderLiteralInline(new LiteralInline("‘"), ref context, ref graphics, newPageAction),
                        Markdig.Extensions.SmartyPants.SmartyPantType.RightQuote => RenderLiteralInline(new LiteralInline("’"), ref context, ref graphics, newPageAction),
                        Markdig.Extensions.SmartyPants.SmartyPantType.Dash2 => RenderLiteralInline(new LiteralInline("–"), ref context, ref graphics, newPageAction),
                        Markdig.Extensions.SmartyPants.SmartyPantType.Dash3 => RenderLiteralInline(new LiteralInline("—"), ref context, ref graphics, newPageAction),
                        Markdig.Extensions.SmartyPants.SmartyPantType.DoubleQuote => RenderLiteralInline(new LiteralInline("\""), ref context, ref graphics, newPageAction),
                        Markdig.Extensions.SmartyPants.SmartyPantType.Ellipsis => RenderLiteralInline(new LiteralInline("…"), ref context, ref graphics, newPageAction),
                        Markdig.Extensions.SmartyPants.SmartyPantType.LeftAngleQuote => RenderLiteralInline(new LiteralInline("«"), ref context, ref graphics, newPageAction),
                        Markdig.Extensions.SmartyPants.SmartyPantType.Quote => RenderLiteralInline(new LiteralInline("'"), ref context, ref graphics, newPageAction),
                        Markdig.Extensions.SmartyPants.SmartyPantType.RightAngleQuote => RenderLiteralInline(new LiteralInline("»"), ref context, ref graphics, newPageAction),
                        _ => "",
                    };
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
    }
}
