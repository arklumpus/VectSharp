﻿using System;
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
                    Font newFont = new Font(txt.Font.FontFamily, txt.Font.FontSize * 0.7);

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
                    Font newFont = new Font(txt.Font.FontFamily, txt.Font.FontSize * 0.7);

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
    }
}