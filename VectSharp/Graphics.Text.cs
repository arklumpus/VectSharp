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
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace VectSharp
{
    public partial class Graphics
    {
        /// <summary>
        /// Fill a text string.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="fillColour">The <see cref="Brush"/> to use to fill the text.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <param name="tag">A tag to identify the filled text.</param>
        public void FillText(Point origin, string text, Font font, Brush fillColour, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            Actions.Add(new TextAction(origin, text, font, textBaseline, fillColour, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag));

            if (font.Underline != null)
            {
                FillTextUnderline(origin, text, font, fillColour, textBaseline, tag);
            }
        }

        /// <summary>
        /// Fill a text string.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="fillColour">The <see cref="Brush"/> to use to fill the text.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// <param name="tag">A tag to identify the filled text.</param>
        public void FillText(double originX, double originY, string text, Font font, Brush fillColour, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            Actions.Add(new TextAction(new Point(originX, originY), text, font, textBaseline, fillColour, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag));

            if (font.Underline != null)
            {
                FillTextUnderline(originX, originY, text, font, fillColour, textBaseline, tag);
            }
        }

        /// <summary>
        /// Stroke a text string.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="strokeColour">The <see cref="Brush"/> with which to stroke the text.</param>
        /// <param name="lineWidth">The width of the line with which the text is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the text.</param>
        /// <param name="lineJoin">The line join to use to stroke the text.</param>
        /// <param name="lineDash">The line dash to use to stroke the text.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <param name="tag">A tag to identify the stroked text.</param>
        public void StrokeText(Point origin, string text, Font font, Brush strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new TextAction(origin, text, font, textBaseline, null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag));

            if (font.Underline != null)
            {
                StrokeTextUnderline(origin, text, font, strokeColour, textBaseline, lineWidth, lineCap, lineJoin, lineDash, tag);
            }
        }

        /// <summary>
        /// Stroke a text string.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="strokeColour">The <see cref="Brush"/> with which to stroke the text.</param>
        /// <param name="lineWidth">The width of the line with which the text is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the text.</param>
        /// <param name="lineJoin">The line join to use to stroke the text.</param>
        /// <param name="lineDash">The line dash to use to stroke the text.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// <param name="tag">A tag to identify the stroked text.</param>
        public void StrokeText(double originX, double originY, string text, Font font, Brush strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new TextAction(new Point(originX, originY), text, font, textBaseline, null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag));

            if (font.Underline != null)
            {
                StrokeTextUnderline(originX, originY, text, font, strokeColour, textBaseline, lineWidth, lineCap, lineJoin, lineDash, tag);
            }
        }

        /// <summary>
        /// Fill a text string along a <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> along which the text will flow.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="fillColour">The <see cref="Brush"/> to use to fill the text.</param>
        /// <param name="reference">The (relative) starting point on the path starting from which the text should be drawn (0 is the start of the path, 1 is the end of the path).</param>
        /// <param name="anchor">The anchor in the text string that will correspond to the point specified by the <paramref name="reference"/>.</param>
        /// <param name="textBaseline">The text baseline (determines which the position of the text in relation to the <paramref name="path"/>.</param>
        /// <param name="tag">A tag to identify the filled text.</param>
        public void FillTextOnPath(GraphicsPath path, string text, Font font, Brush fillColour, double reference = 0, TextAnchors anchor = TextAnchors.Left, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            if (font.Underline != null)
            {
                font = new Font(font.FontFamily, font.FontSize, false);
            }

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

            Point currentGlyphPlacementDelta = new Point();
            Point currentGlyphAdvanceDelta = new Point();
            Point nextGlyphPlacementDelta = new Point();
            Point nextGlyphAdvanceDelta = new Point();

            for (int i = 0; i < text.Length; i++)
            {
                string c = text.Substring(i, 1);

                if (Font.EnableKerning && i < text.Length - 1)
                {
                    currentGlyphPlacementDelta = nextGlyphPlacementDelta;
                    currentGlyphAdvanceDelta = nextGlyphAdvanceDelta;
                    nextGlyphAdvanceDelta = new Point();
                    nextGlyphPlacementDelta = new Point();

                    TrueTypeFile.PairKerning kerning = font.FontFamily.TrueTypeFile.Get1000EmKerning(text[i], text[i + 1]);

                    if (kerning != null)
                    {
                        currentGlyphPlacementDelta = new Point(currentGlyphPlacementDelta.X + kerning.Glyph1Placement.X, currentGlyphPlacementDelta.Y + kerning.Glyph1Placement.Y);
                        currentGlyphAdvanceDelta = new Point(currentGlyphAdvanceDelta.X + kerning.Glyph1Advance.X, currentGlyphAdvanceDelta.Y + kerning.Glyph1Advance.Y);

                        nextGlyphPlacementDelta = new Point(nextGlyphPlacementDelta.X + kerning.Glyph2Placement.X, nextGlyphPlacementDelta.Y + kerning.Glyph2Placement.Y);
                        nextGlyphAdvanceDelta = new Point(nextGlyphAdvanceDelta.X + kerning.Glyph2Advance.X, nextGlyphAdvanceDelta.Y + kerning.Glyph2Advance.Y);
                    }
                }

                Font.DetailedFontMetrics metrics = font.MeasureTextAdvanced(c);

                Point origin = path.GetPointAtRelative(reference + currDelta + currentGlyphPlacementDelta.X * font.FontSize / 1000);

                Point tangent = path.GetTangentAtRelative(reference + currDelta + currentGlyphPlacementDelta.X * font.FontSize / 1000 + (metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing) / pathLength * 0.5);

                origin = new Point(origin.X - tangent.Y * currentGlyphPlacementDelta.Y * font.FontSize / 1000, origin.Y + tangent.X * currentGlyphPlacementDelta.Y * font.FontSize / 1000);

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
                    currDelta += (metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing + currentGlyphAdvanceDelta.X * font.FontSize / 1000) / pathLength;
                }
                else
                {
                    currDelta += (metrics.Width + metrics.RightSideBearing + currentGlyphAdvanceDelta.X * font.FontSize / 1000) / pathLength;
                }
            }
        }

        /// <summary>
        /// Stroke a text string along a <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> along which the text will flow.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="strokeColour">The <see cref="Brush"/> with which to stroke the text.</param>
        /// <param name="lineWidth">The width of the line with which the text is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the text.</param>
        /// <param name="lineJoin">The line join to use to stroke the text.</param>
        /// <param name="lineDash">The line dash to use to stroke the text.</param>
        /// <param name="reference">The (relative) starting point on the path starting from which the text should be drawn (0 is the start of the path, 1 is the end of the path).</param>
        /// <param name="anchor">The anchor in the text string that will correspond to the point specified by the <paramref name="reference"/>.</param>
        /// <param name="textBaseline">The text baseline (determines which the position of the text in relation to the <paramref name="path"/>.</param>
        /// <param name="tag">A tag to identify the stroked text.</param>
        public void StrokeTextOnPath(GraphicsPath path, string text, Font font, Brush strokeColour, double reference = 0, TextAnchors anchor = TextAnchors.Left, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            if (font.Underline != null)
            {
                font = new Font(font.FontFamily, font.FontSize, false);
            }

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

            Point currentGlyphPlacementDelta = new Point();
            Point currentGlyphAdvanceDelta = new Point();
            Point nextGlyphPlacementDelta = new Point();
            Point nextGlyphAdvanceDelta = new Point();

            for (int i = 0; i < text.Length; i++)
            {
                string c = text.Substring(i, 1);

                if (Font.EnableKerning && i < text.Length - 1)
                {
                    currentGlyphPlacementDelta = nextGlyphPlacementDelta;
                    currentGlyphAdvanceDelta = nextGlyphAdvanceDelta;
                    nextGlyphAdvanceDelta = new Point();
                    nextGlyphPlacementDelta = new Point();

                    TrueTypeFile.PairKerning kerning = font.FontFamily.TrueTypeFile.Get1000EmKerning(text[i], text[i + 1]);

                    if (kerning != null)
                    {
                        currentGlyphPlacementDelta = new Point(currentGlyphPlacementDelta.X + kerning.Glyph1Placement.X, currentGlyphPlacementDelta.Y + kerning.Glyph1Placement.Y);
                        currentGlyphAdvanceDelta = new Point(currentGlyphAdvanceDelta.X + kerning.Glyph1Advance.X, currentGlyphAdvanceDelta.Y + kerning.Glyph1Advance.Y);

                        nextGlyphPlacementDelta = new Point(nextGlyphPlacementDelta.X + kerning.Glyph2Placement.X, nextGlyphPlacementDelta.Y + kerning.Glyph2Placement.Y);
                        nextGlyphAdvanceDelta = new Point(nextGlyphAdvanceDelta.X + kerning.Glyph2Advance.X, nextGlyphAdvanceDelta.Y + kerning.Glyph2Advance.Y);
                    }
                }

                Font.DetailedFontMetrics metrics = font.MeasureTextAdvanced(c);

                Point origin = path.GetPointAtRelative(reference + currDelta + currentGlyphPlacementDelta.X * font.FontSize / 1000);

                Point tangent = path.GetTangentAtRelative(reference + currDelta + currentGlyphPlacementDelta.X * font.FontSize / 1000 + (metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing) / pathLength * 0.5);

                origin = new Point(origin.X - tangent.Y * currentGlyphPlacementDelta.Y * font.FontSize / 1000, origin.Y + tangent.X * currentGlyphPlacementDelta.Y * font.FontSize / 1000);

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
                    currDelta += (metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing + currentGlyphAdvanceDelta.X * font.FontSize / 1000) / pathLength;
                }
                else
                {
                    currDelta += (metrics.Width + metrics.RightSideBearing + currentGlyphAdvanceDelta.X * font.FontSize / 1000) / pathLength;
                }
            }
        }

        /// <summary>
        /// Fill a formatted text string.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The <see cref="FormattedText"/> to draw.</param>
        /// <param name="fillColour">The default <see cref="Brush"/> to use to fill the text. This can be overridden by each <paramref name="text"/> element.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <param name="tag">A tag to identify the filled text.</param>
        public void FillText(Point origin, IEnumerable<FormattedText> text, Brush fillColour, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            List<FormattedText> enumeratedText = new List<FormattedText>();
            List<Font.DetailedFontMetrics> allMetrics = new List<Font.DetailedFontMetrics>();

            Font.DetailedFontMetrics fullMetrics = text.Measure(enumeratedText, allMetrics);

            Point baselineOrigin = origin;

            switch (textBaseline)
            {
                case TextBaselines.Baseline:
                    baselineOrigin = origin;
                    break;
                case TextBaselines.Top:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Top);
                    break;
                case TextBaselines.Bottom:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Bottom);
                    break;
                case TextBaselines.Middle:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Top * 0.5 + fullMetrics.Bottom * 0.5);
                    break;
            }

            for (int i = 0; i < enumeratedText.Count; i++)
            {
                FormattedText txt = enumeratedText[i];

                if (txt.Script == Script.Normal)
                {
                    Font.DetailedFontMetrics metrics = allMetrics[i];

                    if (i > 0)
                    {
                        FillText(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y, txt.Text, txt.Font, txt.Brush ?? fillColour, TextBaselines.Baseline, tag);
                    }
                    else
                    {
                        FillText(baselineOrigin, txt.Text, txt.Font, txt.Brush ?? fillColour, TextBaselines.Baseline, tag);
                    }

                    if (i > 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing, baselineOrigin.Y);
                    }
                    else
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing, baselineOrigin.Y);
                    }
                }
                else
                {
                    Font newFont = new Font(txt.Font.FontFamily, txt.Font.FontSize * 0.7, txt.Font.Underline);

                    Font.DetailedFontMetrics metrics = allMetrics[i];

                    if (i == 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X - metrics.LeftSideBearing, baselineOrigin.Y);
                    }

                    if (txt.Script == Script.Subscript)
                    {
                        FillText(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + txt.Font.FontSize * 0.14, txt.Text, newFont, txt.Brush ?? fillColour, TextBaselines.Baseline, tag);
                    }
                    else if (txt.Script == Script.Superscript)
                    {
                        FillText(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y - txt.Font.FontSize * 0.33, txt.Text, newFont, txt.Brush ?? fillColour, TextBaselines.Baseline, tag);
                    }


                    if (i > 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing, baselineOrigin.Y);
                    }
                    else
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing, baselineOrigin.Y);
                    }
                }
            }
        }

        /// <summary>
        /// Fill a formatted text string.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The <see cref="FormattedText"/> to draw.</param>
        /// <param name="fillColour">The default <see cref="Brush"/> to use to fill the text. This can be overridden by each <paramref name="text"/> element.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// <param name="tag">A tag to identify the filled text.</param>
        public void FillText(double originX, double originY, IEnumerable<FormattedText> text, Brush fillColour, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            FillText(new Point(originX, originY), text, fillColour, textBaseline, tag);
        }

        /// <summary>
        /// Stroke a formatted text string.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The <see cref="FormattedText"/> to draw.</param>
        /// <param name="strokeColour">The default <see cref="Brush"/> with which to stroke the text.</param>
        /// <param name="lineWidth">The width of the line with which the text is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the text.</param>
        /// <param name="lineJoin">The line join to use to stroke the text.</param>
        /// <param name="lineDash">The line dash to use to stroke the text.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <param name="tag">A tag to identify the stroked text.</param>
        public void StrokeText(Point origin, IEnumerable<FormattedText> text, Brush strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            List<FormattedText> enumeratedText = new List<FormattedText>();
            List<Font.DetailedFontMetrics> allMetrics = new List<Font.DetailedFontMetrics>();

            Font.DetailedFontMetrics fullMetrics = text.Measure(enumeratedText, allMetrics);

            Point baselineOrigin = origin;

            switch (textBaseline)
            {
                case TextBaselines.Baseline:
                    baselineOrigin = origin;
                    break;
                case TextBaselines.Top:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Top);
                    break;
                case TextBaselines.Bottom:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Bottom);
                    break;
                case TextBaselines.Middle:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Top * 0.5 + fullMetrics.Bottom * 0.5);
                    break;
            }

            for (int i = 0; i < enumeratedText.Count; i++)
            {
                FormattedText txt = enumeratedText[i];

                if (txt.Script == Script.Normal)
                {
                    Font.DetailedFontMetrics metrics = allMetrics[i];

                    if (i > 0)
                    {
                        StrokeText(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y, txt.Text, txt.Font, txt.Brush ?? strokeColour, TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                    }
                    else
                    {
                        StrokeText(baselineOrigin, txt.Text, txt.Font, txt.Brush ?? strokeColour, TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                    }

                    if (i > 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing, baselineOrigin.Y);
                    }
                    else
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing, baselineOrigin.Y);
                    }
                }
                else
                {
                    Font newFont = new Font(txt.Font.FontFamily, txt.Font.FontSize * 0.7, txt.Font.Underline);

                    Font.DetailedFontMetrics metrics = allMetrics[i];

                    if (i == 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X - metrics.LeftSideBearing, baselineOrigin.Y);
                    }

                    if (txt.Script == Script.Subscript)
                    {
                        StrokeText(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + txt.Font.FontSize * 0.14, txt.Text, newFont, txt.Brush ?? strokeColour, TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                    }
                    else if (txt.Script == Script.Superscript)
                    {
                        StrokeText(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y - txt.Font.FontSize * 0.33, txt.Text, newFont, txt.Brush ?? strokeColour, TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                    }


                    if (i > 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing, baselineOrigin.Y);
                    }
                    else
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing, baselineOrigin.Y);
                    }
                }
            }
        }

        /// <summary>
        /// Stroke a formatted text string.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The <see cref="FormattedText"/> to draw.</param>
        /// <param name="strokeColour">The default <see cref="Brush"/> with which to stroke the text.</param>
        /// <param name="lineWidth">The width of the line with which the text is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the text.</param>
        /// <param name="lineJoin">The line join to use to stroke the text.</param>
        /// <param name="lineDash">The line dash to use to stroke the text.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// <param name="tag">A tag to identify the stroked text.</param>
        public void StrokeText(double originX, double originY, IEnumerable<FormattedText> text, Brush strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            StrokeText(new Point(originX, originY), text, strokeColour, textBaseline, lineWidth, lineCap, lineJoin, lineDash, tag);
        }

        /// <summary>
        /// Measure a text string.
        /// See also <seealso cref="Font.MeasureText(string)"/> and <seealso cref="Font.MeasureTextAdvanced(string)"/>.
        /// </summary>
        /// <param name="text">The string to measure.</param>
        /// <param name="font">The font to use to measure the string.</param>
        /// <returns>The size of the measured <paramref name="text"/>.</returns>
        public Size MeasureText(string text, Font font)
        {
            return font.MeasureText(text);
        }

        /// <summary>
        /// Measure a formatted text string.
        /// See also <seealso cref="FormattedTextExtensions.Measure(IEnumerable{FormattedText})"/>.
        /// </summary>
        /// <param name="text">The collection of <see cref="FormattedText"/> objects to measure.</param>
        /// <returns>The size of the measured <paramref name="text"/>.</returns>
        public Size MeasureText(IEnumerable<FormattedText> text)
        {
            Font.DetailedFontMetrics metrics = text.Measure();

            return new Size(metrics.Width, metrics.Height);
        }

        /// <summary>
        /// Fills the underline of the specified text string.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string whose underline will be draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="fillColour">The <see cref="Brush"/> to use to fill the underline.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// <param name="tag">A tag to identify the filled underline.</param>
        public void FillTextUnderline(double originX, double originY, string text, Font font, Brush fillColour, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            FillTextUnderline(new Point(originX, originY), text, font, fillColour, textBaseline, tag);
        }

        /// <summary>
        /// Fills the underline of the specified text string.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string whose underline will be draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="fillColour">The <see cref="Brush"/> to use to fill the underline.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <param name="tag">A tag to identify the filled underline.</param>
        public void FillTextUnderline(Point origin, string text, Font font, Brush fillColour, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            GraphicsPath underline = new GraphicsPath().AddTextUnderline(origin, text, font, textBaseline);
            FillPath(underline, fillColour, tag);
        }

        /// <summary>
        /// Stroke the underline of the specified text string.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string whose underline will be drawn.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="strokeColour">The <see cref="Brush"/> with which to stroke the underline.</param>
        /// <param name="lineWidth">The width of the line with which the underline is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the underline.</param>
        /// <param name="lineJoin">The line join to use to stroke the underline.</param>
        /// <param name="lineDash">The line dash to use to stroke the underline.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// <param name="tag">A tag to identify the stroked underline.</param>
        public void StrokeTextUnderline(double originX, double originY, string text, Font font, Brush strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            StrokeTextUnderline(new Point(originX, originY), text, font, strokeColour, textBaseline, lineWidth, lineCap, lineJoin, lineDash, tag);
        }

        /// <summary>
        /// Stroke the underline of the specified text string.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string whose underline will be drawn.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="strokeColour">The <see cref="Brush"/> with which to stroke the underline.</param>
        /// <param name="lineWidth">The width of the line with which the underline is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the underline.</param>
        /// <param name="lineJoin">The line join to use to stroke the underline.</param>
        /// <param name="lineDash">The line dash to use to stroke the underline.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <param name="tag">A tag to identify the stroked underline.</param>
        public void StrokeTextUnderline(Point origin, string text, Font font, Brush strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            GraphicsPath underline = new GraphicsPath().AddTextUnderline(origin, text, font, textBaseline);
            StrokePath(underline, strokeColour, lineWidth, lineCap, lineJoin, lineDash, tag);
        }


        /// <summary>
        /// Fill the underline of the specified formatted text string.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The <see cref="FormattedText"/> whose underline will be drawn.</param>
        /// <param name="fillColour">The default <see cref="Brush"/> to use to fill the underline. This can be overridden by each <paramref name="text"/> element.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// <param name="tag">A tag to identify the filled underlined.</param>
        public void FillTextUnderline(double originX, double originY, IEnumerable<FormattedText> text, Brush fillColour, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            FillTextUnderline(new Point(originX, originY), text, fillColour, textBaseline, tag);
        }

        /// <summary>
        /// Fill the underline of the specified formatted text string.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The <see cref="FormattedText"/> whose underline will be drawn.</param>
        /// <param name="fillColour">The default <see cref="Brush"/> to use to fill the underline. This can be overridden by each <paramref name="text"/> element.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <param name="tag">A tag to identify the filled underlined.</param>
        public void FillTextUnderline(Point origin, IEnumerable<FormattedText> text, Brush fillColour, TextBaselines textBaseline = TextBaselines.Top, string tag = null)
        {
            List<FormattedText> enumeratedText = new List<FormattedText>();
            List<Font.DetailedFontMetrics> allMetrics = new List<Font.DetailedFontMetrics>();

            Font.DetailedFontMetrics fullMetrics = text.Measure(enumeratedText, allMetrics);

            Point baselineOrigin = origin;

            switch (textBaseline)
            {
                case TextBaselines.Baseline:
                    baselineOrigin = origin;
                    break;
                case TextBaselines.Top:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Top);
                    break;
                case TextBaselines.Bottom:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Bottom);
                    break;
                case TextBaselines.Middle:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Top * 0.5 + fullMetrics.Bottom * 0.5);
                    break;
            }

            for (int i = 0; i < enumeratedText.Count; i++)
            {
                FormattedText txt = enumeratedText[i];

                if (txt.Script == Script.Normal)
                {
                    Font.DetailedFontMetrics metrics = allMetrics[i];

                    if (i > 0)
                    {
                        FillTextUnderline(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y, txt.Text, txt.Font, txt.Brush ?? fillColour, TextBaselines.Baseline, tag);
                    }
                    else
                    {
                        FillTextUnderline(baselineOrigin, txt.Text, txt.Font, txt.Brush ?? fillColour, TextBaselines.Baseline, tag);
                    }

                    if (i > 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing, baselineOrigin.Y);
                    }
                    else
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing, baselineOrigin.Y);
                    }
                }
                else
                {
                    Font newFont = new Font(txt.Font.FontFamily, txt.Font.FontSize * 0.7, txt.Font.Underline);

                    Font.DetailedFontMetrics metrics = allMetrics[i];

                    if (i == 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X - metrics.LeftSideBearing, baselineOrigin.Y);
                    }

                    if (txt.Script == Script.Subscript)
                    {
                        FillTextUnderline(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + txt.Font.FontSize * 0.14, txt.Text, newFont, txt.Brush ?? fillColour, TextBaselines.Baseline, tag);
                    }
                    else if (txt.Script == Script.Superscript)
                    {
                        FillTextUnderline(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y - txt.Font.FontSize * 0.33, txt.Text, newFont, txt.Brush ?? fillColour, TextBaselines.Baseline, tag);
                    }


                    if (i > 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing, baselineOrigin.Y);
                    }
                    else
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing, baselineOrigin.Y);
                    }
                }
            }
        }

        /// <summary>
        /// Stroke the underline of the specified formatted text string.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The <see cref="FormattedText"/> to draw.</param>
        /// <param name="strokeColour">The default <see cref="Brush"/> with which to stroke the underline.</param>
        /// <param name="lineWidth">The width of the line with which the underline is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the underline.</param>
        /// <param name="lineJoin">The line join to use to stroke the underline.</param>
        /// <param name="lineDash">The line dash to use to stroke the underline.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// <param name="tag">A tag to identify the stroked underline.</param>
        public void StrokeTextUnderline(double originX, double originY, IEnumerable<FormattedText> text, Brush strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            StrokeTextUnderline(new Point(originX, originY), text, strokeColour, textBaseline, lineWidth, lineCap, lineJoin, lineDash, tag);
        }

        /// <summary>
        /// Stroke the underline of the specified formatted text string.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The <see cref="FormattedText"/> to draw.</param>
        /// <param name="strokeColour">The default <see cref="Brush"/> with which to stroke the underline.</param>
        /// <param name="lineWidth">The width of the line with which the underline is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the underline.</param>
        /// <param name="lineJoin">The line join to use to stroke the underline.</param>
        /// <param name="lineDash">The line dash to use to stroke the underline.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <param name="tag">A tag to identify the stroked underline.</param>
        public void StrokeTextUnderline(Point origin, IEnumerable<FormattedText> text, Brush strokeColour, TextBaselines textBaseline = TextBaselines.Top, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            List<FormattedText> enumeratedText = new List<FormattedText>();
            List<Font.DetailedFontMetrics> allMetrics = new List<Font.DetailedFontMetrics>();

            Font.DetailedFontMetrics fullMetrics = text.Measure(enumeratedText, allMetrics);

            Point baselineOrigin = origin;

            switch (textBaseline)
            {
                case TextBaselines.Baseline:
                    baselineOrigin = origin;
                    break;
                case TextBaselines.Top:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Top);
                    break;
                case TextBaselines.Bottom:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Bottom);
                    break;
                case TextBaselines.Middle:
                    baselineOrigin = new Point(origin.X, origin.Y + fullMetrics.Top * 0.5 + fullMetrics.Bottom * 0.5);
                    break;
            }

            for (int i = 0; i < enumeratedText.Count; i++)
            {
                FormattedText txt = enumeratedText[i];

                if (txt.Script == Script.Normal)
                {
                    Font.DetailedFontMetrics metrics = allMetrics[i];

                    if (i > 0)
                    {
                        StrokeTextUnderline(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y, txt.Text, txt.Font, txt.Brush ?? strokeColour, TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                    }
                    else
                    {
                        StrokeTextUnderline(baselineOrigin, txt.Text, txt.Font, txt.Brush ?? strokeColour, TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                    }

                    if (i > 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing, baselineOrigin.Y);
                    }
                    else
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing, baselineOrigin.Y);
                    }
                }
                else
                {
                    Font newFont = new Font(txt.Font.FontFamily, txt.Font.FontSize * 0.7, txt.Font.Underline);

                    Font.DetailedFontMetrics metrics = allMetrics[i];

                    if (i == 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X - metrics.LeftSideBearing, baselineOrigin.Y);
                    }

                    if (txt.Script == Script.Subscript)
                    {
                        StrokeTextUnderline(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + txt.Font.FontSize * 0.14, txt.Text, newFont, txt.Brush ?? strokeColour, TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                    }
                    else if (txt.Script == Script.Superscript)
                    {
                        StrokeTextUnderline(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y - txt.Font.FontSize * 0.33, txt.Text, newFont, txt.Brush ?? strokeColour, TextBaselines.Baseline, lineWidth, lineCap, lineJoin, lineDash, tag);
                    }


                    if (i > 0)
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing + metrics.LeftSideBearing, baselineOrigin.Y);
                    }
                    else
                    {
                        baselineOrigin = new Point(baselineOrigin.X + metrics.Width + metrics.RightSideBearing, baselineOrigin.Y);
                    }
                }
            }
        }
    }
}
