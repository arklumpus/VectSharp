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

namespace VectSharp
{
    /// <summary>
    /// Represents a graphics path that can be filled or stroked.
    /// </summary>
    public class GraphicsPath
    {
        /// <summary>
        /// The segments that make up the path.
        /// </summary>
        public List<Segment> Segments { get; set; } = new List<Segment>();


        /// <summary>
        /// Move the current point without tracing a segment from the previous point.
        /// </summary>
        /// <param name="p">The new point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath MoveTo(Point p)
        {
            cachedLength = double.NaN;
            cachedBounds = Rectangle.NaN;
            Segments.Add(new MoveSegment(p));
            return this;
        }

        /// <summary>
        /// Move the current point without tracing a segment from the previous point.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the new point.</param>
        /// <param name="y">The vertical coordinate of the new point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath MoveTo(double x, double y)
        {
            MoveTo(new Point(x, y));
            return this;
        }

        /// <summary>
        /// Move the current point and trace a segment from the previous point.
        /// </summary>
        /// <param name="p">The new point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath LineTo(Point p)
        {
            cachedLength = double.NaN;
            cachedBounds = Rectangle.NaN;

            if (Segments.Count == 0)
            {
                Segments.Add(new MoveSegment(p));
            }
            else
            {
                Segments.Add(new LineSegment(p));
            }
            return this;
        }

        /// <summary>
        /// Move the current point and trace a segment from the previous point.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the new point.</param>
        /// <param name="y">The vertical coordinate of the new point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath LineTo(double x, double y)
        {
            LineTo(new Point(x, y));
            return this;
        }

        /// <summary>
        /// Trace an arc segment from a circle with the specified <paramref name="center"/> and <paramref name="radius"/>, starting at <paramref name="startAngle"/> and ending at <paramref name="endAngle"/>.
        /// The current point is updated to the end point of the arc.
        /// </summary>
        /// <param name="center">The center of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="startAngle">The start angle (in radians) of the arc.</param>
        /// <param name="endAngle">The end angle (in radians) of the arc.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath Arc(Point center, double radius, double startAngle, double endAngle)
        {
            cachedLength = double.NaN;
            cachedBounds = Rectangle.NaN;

            if (Segments.Count == 0)
            {
                Segments.Add(new MoveSegment(center.X + radius * Math.Cos(startAngle), center.Y + radius * Math.Sin(startAngle)));
            }
            Segments.Add(new ArcSegment(center, radius, startAngle, endAngle));
            return this;
        }

        /// <summary>
        /// Trace an arc segment from a circle with the specified center and <paramref name="radius"/>, starting at <paramref name="startAngle"/> and ending at <paramref name="endAngle"/>.
        /// The current point is updated to the end point of the arc.
        /// </summary>
        /// <param name="centerX">The horizontal coordinate of the center of the arc.</param>
        /// <param name="centerY">The vertical coordinate of the center of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="startAngle">The start angle (in radians) of the arc.</param>
        /// <param name="endAngle">The end angle (in radians) of the arc.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath Arc(double centerX, double centerY, double radius, double startAngle, double endAngle)
        {
            Arc(new Point(centerX, centerY), radius, startAngle, endAngle);
            return this;
        }

        /// <summary>
        /// Trace an arc from an ellipse with the specified radii, rotated by <paramref name="axisAngle"/> with respect to the x-axis, starting at the current point and ending at the <paramref name="endPoint"/>.
        /// </summary>
        /// <param name="radiusX">The horizontal radius of the ellipse.</param>
        /// <param name="radiusY">The vertical radius of the ellipse.</param>
        /// <param name="axisAngle">The angle of the horizontal axis of the ellipse with respect to the horizontal axis.</param>
        /// <param name="largeArc">Determines whether the large or the small arc is drawn.</param>
        /// <param name="sweepClockwise">Determines whether the clockwise or anticlockwise arc is drawn.</param>
        /// <param name="endPoint">The end point of the arc.</param>
        /// <returns></returns>
        public GraphicsPath EllipticalArc(double radiusX, double radiusY, double axisAngle, bool largeArc, bool sweepClockwise, Point endPoint)
        {
            if (radiusX == 0 || radiusY == 0)
            {
                return this.LineTo(endPoint);
            }
            else
            {
                radiusX = Math.Abs(radiusX);
                radiusY = Math.Abs(radiusY);
            }

            double x1 = 0;
            double y1 = 0;

            if (this.Segments.Count > 0)
            {
                for (int i = this.Segments.Count - 1; i >= 0; i--)
                {
                    if (this.Segments[i].Type != SegmentType.Close)
                    {
                        x1 = this.Segments[i].Point.X;
                        y1 = this.Segments[i].Point.Y;
                        break;
                    }
                }
            }

            double x2 = endPoint.X;
            double y2 = endPoint.Y;

            double x1P = Math.Cos(axisAngle) * (x1 - x2) * 0.5 + Math.Sin(axisAngle) * (y1 - y2) * 0.5;

            if (Math.Abs(x1P) < 1e-7)
            {
                x1P = 0;
            }

            double y1P = -Math.Sin(axisAngle) * (x1 - x2) * 0.5 + Math.Cos(axisAngle) * (y1 - y2) * 0.5;

            if (Math.Abs(y1P) < 1e-7)
            {
                y1P = 0;
            }

            double lambda = x1P * x1P / (radiusX * radiusX) + y1P * y1P / (radiusY * radiusY);

            if (lambda > 1)
            {
                double sqrtLambda = Math.Sqrt(lambda);
                radiusX *= sqrtLambda;
                radiusY *= sqrtLambda;
            }

            double sqrtTerm = (largeArc != sweepClockwise ? 1 : -1) * Math.Sqrt(Math.Max(0, (radiusX * radiusX * radiusY * radiusY - radiusX * radiusX * y1P * y1P - radiusY * radiusY * x1P * x1P) / (radiusX * radiusX * y1P * y1P + radiusY * radiusY * x1P * x1P)));

            double cXP = sqrtTerm * radiusX * y1P / radiusY;
            double cYP = -sqrtTerm * radiusY * x1P / radiusX;

            double cX = Math.Cos(axisAngle) * cXP - Math.Sin(axisAngle) * cYP + (x1 + x2) * 0.5;
            double cY = Math.Sin(axisAngle) * cXP + Math.Cos(axisAngle) * cYP + (y1 + y2) * 0.5;

            double theta1 = AngleVectors(1, 0, (x1P - cXP) / radiusX, (y1P - cYP) / radiusY);
            double deltaTheta = AngleVectors((x1P - cXP) / radiusX, (y1P - cYP) / radiusY, (-x1P - cXP) / radiusX, (-y1P - cYP) / radiusY) % (2 * Math.PI);

            if (!sweepClockwise && deltaTheta > 0)
            {
                deltaTheta -= 2 * Math.PI;
            }
            else if (sweepClockwise && deltaTheta < 0)
            {
                deltaTheta += 2 * Math.PI;
            }

            double r = Math.Min(radiusX, radiusY);

            ArcSegment arc = new ArcSegment(0, 0, r, theta1, theta1 + deltaTheta);

            Segment[] segments = arc.ToBezierSegments();

            for (int i = 0; i < segments.Length; i++)
            {
                for (int j = 0; j < segments[i].Points.Length; j++)
                {
                    double newX = segments[i].Points[j].X * radiusX / r;
                    double newY = segments[i].Points[j].Y * radiusY / r;

                    segments[i].Points[j] = new Point(newX * Math.Cos(axisAngle) - newY * Math.Sin(axisAngle) + cX, newX * Math.Sin(axisAngle) + newY * Math.Cos(axisAngle) + cY);
                }
            }

            cachedLength = double.NaN;
            cachedBounds = Rectangle.NaN;

            this.Segments.AddRange(segments);

            return this;
        }

        private static double AngleVectors(double uX, double uY, double vX, double vY)
        {
            double tbr = Math.Acos((uX * vX + uY * vY) / Math.Sqrt((uX * uX + uY * uY) * (vX * vX + vY * vY)));
            double sign = Math.Sign(uX * vY - uY * vX);
            if (sign != 0)
            {
                tbr *= sign;
            }
            return tbr;
        }


        /// <summary>
        /// Trace a cubic Bezier curve from the current point to a destination point, with two control points.
        /// The current point is updated to the end point of the Bezier curve.
        /// </summary>
        /// <param name="control1">The first control point.</param>
        /// <param name="control2">The second control point.</param>
        /// <param name="endPoint">The destination point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath CubicBezierTo(Point control1, Point control2, Point endPoint)
        {
            cachedLength = double.NaN;
            cachedBounds = Rectangle.NaN;

            if (Segments.Count == 0)
            {
                Segments.Add(new MoveSegment(control1));
            }
            Segments.Add(new CubicBezierSegment(control1, control2, endPoint));
            return this;
        }

        /// <summary>
        /// Trace a cubic Bezier curve from the current point to a destination point, with two control points.
        /// The current point is updated to the end point of the Bezier curve.
        /// </summary>
        /// <param name="control1X">The horizontal coordinate of the first control point.</param>
        /// <param name="control1Y">The vertical coordinate of the first control point.</param>
        /// <param name="control2X">The horizontal coordinate of the second control point.</param>
        /// <param name="control2Y">The vertical coordinate of the second control point.</param>
        /// <param name="endPointX">The horizontal coordinate of the destination point.</param>
        /// <param name="endPointY">The vertical coordinate of the destination point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath CubicBezierTo(double control1X, double control1Y, double control2X, double control2Y, double endPointX, double endPointY)
        {
            CubicBezierTo(new Point(control1X, control1Y), new Point(control2X, control2Y), new Point(endPointX, endPointY));
            return this;
        }

        /// <summary>
        /// Trace a quadratic Bezier curve from the current point to a destination point, with a single control point.
        /// The current point is updated to the end point of the Bezier curve.
        /// </summary>
        /// <param name="control">The control point.</param>
        /// <param name="endPoint">The destination point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath QuadraticBezierTo(Point control, Point endPoint)
        {
            Point currentPoint = control;

            if (Segments.Count > 0)
            {
                for (int i = Segments.Count - 1; i >= 0; i--)
                {
                    if (Segments[i].Type != SegmentType.Close)
                    {
                        currentPoint = Segments[i].Point;
                        break;
                    }
                }
            }

            Point control1 = new Point((currentPoint.X + 2 * control.X) / 3, (currentPoint.Y + 2 * control.Y) / 3);
            Point control2 = new Point((endPoint.X + 2 * control.X) / 3, (endPoint.Y + 2 * control.Y) / 3);

            CubicBezierTo(control1, control2, endPoint);

            return this;
        }

        /// <summary>
        /// Trace a quadratic Bezier curve from the current point to a destination point, with a single control point.
        /// The current point is updated to the end point of the Bezier curve.
        /// </summary>
        /// <param name="controlX">The horizontal coordinate of the control point.</param>
        /// <param name="controlY">The vertical coordinate of the control point.</param>
        /// <param name="endPointX">The horizontal coordinate of the destination point.</param>
        /// <param name="endPointY">The vertical coordinate of the destination point.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath QuadraticBezierTo(double controlX, double controlY, double endPointX, double endPointY)
        {
            QuadraticBezierTo(new Point(controlX, controlY), new Point(endPointX, endPointY));
            return this;
        }

        /// <summary>
        /// Trace a segment from the current point to the start point of the figure and flag the figure as closed.
        /// </summary>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath Close()
        {
            cachedLength = double.NaN;
            cachedBounds = Rectangle.NaN;
            Segments.Add(new CloseSegment());
            return this;
        }

        /// <summary>
        /// Add the contour of a text string to the current path.
        /// </summary>
        /// <param name="originX">The horizontal coordinate of the text origin.</param>
        /// <param name="originY">The vertical coordinate of the text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="textBaseline">The text baseline (determines what <paramref name="originY"/> represents).</param>
        /// /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath AddText(double originX, double originY, string text, Font font, TextBaselines textBaseline = TextBaselines.Top)
        {
            return AddText(new Point(originX, originY), text, font, textBaseline);
        }

        /// <summary>
        /// Add the contour of a text string to the current path.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath AddText(Point origin, string text, Font font, TextBaselines textBaseline = TextBaselines.Top)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Font.DetailedFontMetrics metrics = font.MeasureTextAdvanced(text);

                Point baselineOrigin = origin;

                switch (textBaseline)
                {
                    case TextBaselines.Baseline:
                        baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y);
                        break;
                    case TextBaselines.Top:
                        baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y + metrics.Top);
                        break;
                    case TextBaselines.Bottom:
                        baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y + metrics.Bottom);
                        break;
                    case TextBaselines.Middle:
                        baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y + (metrics.Top - metrics.Bottom) * 0.5 + metrics.Bottom);
                        break;
                }

                Point currentGlyphPlacementDelta = new Point();
                Point currentGlyphAdvanceDelta = new Point();
                Point nextGlyphPlacementDelta = new Point();
                Point nextGlyphAdvanceDelta = new Point();

                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];

                    if (Font.EnableKerning && i < text.Length - 1)
                    {
                        currentGlyphPlacementDelta = nextGlyphPlacementDelta;
                        currentGlyphAdvanceDelta = nextGlyphAdvanceDelta;
                        nextGlyphAdvanceDelta = new Point();
                        nextGlyphPlacementDelta = new Point();

                        TrueTypeFile.PairKerning kerning = font.FontFamily.TrueTypeFile.Get1000EmKerning(c, text[i + 1]);

                        if (kerning != null)
                        {
                            currentGlyphPlacementDelta = new Point(currentGlyphPlacementDelta.X + kerning.Glyph1Placement.X, currentGlyphPlacementDelta.Y + kerning.Glyph1Placement.Y);
                            currentGlyphAdvanceDelta = new Point(currentGlyphAdvanceDelta.X + kerning.Glyph1Advance.X, currentGlyphAdvanceDelta.Y + kerning.Glyph1Advance.Y);

                            nextGlyphPlacementDelta = new Point(nextGlyphPlacementDelta.X + kerning.Glyph2Placement.X, nextGlyphPlacementDelta.Y + kerning.Glyph2Placement.Y);
                            nextGlyphAdvanceDelta = new Point(nextGlyphAdvanceDelta.X + kerning.Glyph2Advance.X, nextGlyphAdvanceDelta.Y + kerning.Glyph2Advance.Y);
                        }
                    }

                    TrueTypeFile.TrueTypePoint[][] glyphPaths = font.FontFamily.TrueTypeFile.GetGlyphPath(c, font.FontSize);

                    for (int j = 0; j < glyphPaths.Length; j++)
                    {
                        for (int k = 0; k < glyphPaths[j].Length; k++)
                        {
                            if (k == 0)
                            {
                                this.MoveTo(glyphPaths[j][k].X + baselineOrigin.X + currentGlyphPlacementDelta.X, -glyphPaths[j][k].Y + baselineOrigin.Y + currentGlyphPlacementDelta.Y);
                            }
                            else
                            {
                                if (glyphPaths[j][k].IsOnCurve)
                                {
                                    this.LineTo(glyphPaths[j][k].X + baselineOrigin.X + currentGlyphPlacementDelta.X, -glyphPaths[j][k].Y + baselineOrigin.Y + currentGlyphPlacementDelta.Y);
                                }
                                else
                                {
                                    Point startPoint = this.Segments.Last().Point;
                                    Point quadCtrl = new Point(glyphPaths[j][k].X + baselineOrigin.X + currentGlyphPlacementDelta.X, -glyphPaths[j][k].Y + baselineOrigin.Y + currentGlyphPlacementDelta.Y);
                                    Point endPoint = new Point(glyphPaths[j][k + 1].X + baselineOrigin.X + currentGlyphPlacementDelta.X, -glyphPaths[j][k + 1].Y + baselineOrigin.Y + currentGlyphPlacementDelta.Y);


                                    Point ctrl1 = new Point(startPoint.X / 3 + 2 * quadCtrl.X / 3, startPoint.Y / 3 + 2 * quadCtrl.Y / 3);
                                    Point ctrl2 = new Point(endPoint.X / 3 + 2 * quadCtrl.X / 3, endPoint.Y / 3 + 2 * quadCtrl.Y / 3);

                                    this.CubicBezierTo(ctrl1, ctrl2, endPoint);

                                    k++;
                                }
                            }
                        }

                        this.Close();
                    }

                    baselineOrigin.X += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                    baselineOrigin.Y += (currentGlyphAdvanceDelta.Y) * font.FontSize / 1000;
                }
            }
            return this;
        }

        /// <summary>
        /// Add the contour of a text string flowing along a <see cref="GraphicsPath"/> to the current path.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> along which the text will flow.</param>
        /// <param name="text">The string to draw.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="reference">The (relative) starting point on the path starting from which the text should be drawn (0 is the start of the path, 1 is the end of the path).</param>
        /// <param name="anchor">The anchor in the text string that will correspond to the point specified by the <paramref name="reference"/>.</param>
        /// <param name="textBaseline">The text baseline (determines which the position of the text in relation to the <paramref name="path"/>.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath AddTextOnPath(GraphicsPath path, string text, Font font, double reference = 0, TextAnchors anchor = TextAnchors.Left, TextBaselines textBaseline = TextBaselines.Top)
        {
            if (!string.IsNullOrEmpty(text))
            {
                cachedLength = double.NaN;
                cachedBounds = Rectangle.NaN;

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

                    GraphicsPath glyphPath = new GraphicsPath();

                    switch (textBaseline)
                    {
                        case TextBaselines.Top:
                            if (i > 0)
                            {
                                glyphPath.AddText(new Point(metrics.LeftSideBearing, fullMetrics.Top), c, font, textBaseline: TextBaselines.Baseline);
                            }
                            else
                            {
                                glyphPath.AddText(new Point(0, fullMetrics.Top), c, font, textBaseline: TextBaselines.Baseline);
                            }
                            break;
                        case TextBaselines.Baseline:
                            if (i > 0)
                            {
                                glyphPath.AddText(new Point(metrics.LeftSideBearing, 0), c, font, textBaseline: TextBaselines.Baseline);
                            }
                            else
                            {
                                glyphPath.AddText(new Point(0, 0), c, font, textBaseline: TextBaselines.Baseline);
                            }
                            break;
                        case TextBaselines.Bottom:
                            if (i > 0)
                            {
                                glyphPath.AddText(new Point(metrics.LeftSideBearing, fullMetrics.Bottom), c, font, textBaseline: TextBaselines.Baseline);
                            }
                            else
                            {
                                glyphPath.AddText(new Point(0, fullMetrics.Bottom), c, font, textBaseline: TextBaselines.Baseline);
                            }
                            break;
                        case TextBaselines.Middle:
                            if (i > 0)
                            {
                                glyphPath.AddText(new Point(metrics.LeftSideBearing, fullMetrics.Bottom + fullMetrics.Height / 2), c, font, textBaseline: TextBaselines.Baseline);
                            }
                            else
                            {
                                glyphPath.AddText(new Point(0, fullMetrics.Bottom + fullMetrics.Height / 2), c, font, textBaseline: TextBaselines.Baseline);
                            }
                            break;
                    }

                    double angle = Math.Atan2(tangent.Y, tangent.X);

                    for (int j = 0; j < glyphPath.Segments.Count; j++)
                    {
                        if (glyphPath.Segments[j].Points != null)
                        {
                            for (int k = 0; k < glyphPath.Segments[j].Points.Length; k++)
                            {
                                double newX = glyphPath.Segments[j].Points[k].X * Math.Cos(angle) - glyphPath.Segments[j].Points[k].Y * Math.Sin(angle) + origin.X;
                                double newY = glyphPath.Segments[j].Points[k].X * Math.Sin(angle) + glyphPath.Segments[j].Points[k].Y * Math.Cos(angle) + origin.Y;

                                glyphPath.Segments[j].Points[k] = new Point(newX, newY);
                            }
                        }

                        this.Segments.Add(glyphPath.Segments[j]);
                    }

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

            return this;
        }



        /// <summary>
        /// Add the contour of the underline of the specified text string to the current path.
        /// </summary>
        /// <param name="origin">The text origin. See <paramref name="textBaseline"/>.</param>
        /// <param name="text">The string whose underline will be drawn.</param>
        /// <param name="font">The font with which to draw the text.</param>
        /// <param name="textBaseline">The text baseline (determines what the vertical component of <paramref name="origin"/> represents).</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath AddTextUnderline(Point origin, string text, Font font, TextBaselines textBaseline = TextBaselines.Top)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (font.Underline == null)
                {
                    return this;
                }

                Font.DetailedFontMetrics metrics = font.MeasureTextAdvanced(text);

                double italicAngle = font.FontFamily.TrueTypeFile?.GetItalicAngle() ?? 0;

                if (double.IsNaN(italicAngle))
                {
                    italicAngle = 0;
                }

                Point baselineOrigin = origin;

                switch (textBaseline)
                {
                    case TextBaselines.Baseline:
                        baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y);
                        break;
                    case TextBaselines.Top:
                        baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y + metrics.Top);
                        break;
                    case TextBaselines.Bottom:
                        baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y + metrics.Bottom);
                        break;
                    case TextBaselines.Middle:
                        baselineOrigin = new Point(origin.X - metrics.LeftSideBearing, origin.Y + (metrics.Top - metrics.Bottom) * 0.5 + metrics.Bottom);
                        break;
                }

                if (!font.Underline.SkipDescenders)
                {
                    double italicShift;

                    if (!font.Underline.FollowItalicAngle || italicAngle == 0)
                    {
                        italicShift = 0;
                    }
                    else
                    {
                        italicShift = font.Underline.Thickness * font.FontSize * Math.Tan(italicAngle / 180.0 * Math.PI);
                    }

                    if (font.Underline.LineCap == LineCaps.Butt)
                    {
                        if (!font.Underline.FollowItalicAngle || italicAngle == 0)
                        {
                            this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.Close();
                        }
                        else
                        {
                            this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.Close();
                        }
                    }
                    else if (font.Underline.LineCap == LineCaps.Square)
                    {
                        if (!font.Underline.FollowItalicAngle || italicAngle == 0)
                        {
                            this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing - font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width + font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width + font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing - font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.Close();
                        }
                        else
                        {
                            this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing - font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width + font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width + font.Underline.Thickness * font.FontSize * 0.5 + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing - font.Underline.Thickness * font.FontSize * 0.5 + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.Close();
                        }
                    }
                    else if (font.Underline.LineCap == LineCaps.Round)
                    {
                        if (!font.Underline.FollowItalicAngle || italicAngle == 0)
                        {
                            this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.Arc(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize * 0.5, font.Underline.Thickness * font.FontSize * 0.5, -Math.PI / 2, Math.PI / 2);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.Arc(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize * 0.5, font.Underline.Thickness * font.FontSize * 0.5, Math.PI / 2, 3 * Math.PI / 2);
                            this.Close();
                        }
                        else
                        {
                            this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing - italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            this.CubicBezierTo(baselineOrigin.X + metrics.LeftSideBearing + metrics.Width + font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize,
                                baselineOrigin.X + metrics.LeftSideBearing + metrics.Width + italicShift + font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize,
                                baselineOrigin.X + metrics.LeftSideBearing + metrics.Width + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);

                            this.LineTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.CubicBezierTo(baselineOrigin.X + metrics.LeftSideBearing - font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize,
                                baselineOrigin.X + metrics.LeftSideBearing - italicShift - font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize,
                                baselineOrigin.X + metrics.LeftSideBearing - italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize);

                            this.Close();
                        }
                    }

                    return this;
                }
                else
                {
                    if (font.Underline.LineCap == LineCaps.Butt)
                    {
                        double italicShift;

                        if (!font.Underline.FollowItalicAngle || italicAngle == 0)
                        {
                            italicShift = 0;
                        }
                        else
                        {
                            italicShift = font.Underline.Thickness * font.FontSize * Math.Tan(italicAngle / 180.0 * Math.PI);
                        }

                        bool started = false;

                        double currX = baselineOrigin.X;
                        double underlineStartX = baselineOrigin.X + metrics.LeftSideBearing;
                        double currUnderlineX = underlineStartX - metrics.LeftSideBearing;

                        Point currentGlyphPlacementDelta = new Point();
                        Point currentGlyphAdvanceDelta = new Point();
                        Point nextGlyphPlacementDelta = new Point();
                        Point nextGlyphAdvanceDelta = new Point();

                        for (int i = 0; i < text.Length; i++)
                        {
                            char c = text[i];

                            if (Font.EnableKerning && i < text.Length - 1)
                            {
                                currentGlyphPlacementDelta = nextGlyphPlacementDelta;
                                currentGlyphAdvanceDelta = nextGlyphAdvanceDelta;
                                nextGlyphAdvanceDelta = new Point();
                                nextGlyphPlacementDelta = new Point();

                                TrueTypeFile.PairKerning kerning = font.FontFamily.TrueTypeFile.Get1000EmKerning(c, text[i + 1]);

                                if (kerning != null)
                                {
                                    currentGlyphPlacementDelta = new Point(currentGlyphPlacementDelta.X + kerning.Glyph1Placement.X, currentGlyphPlacementDelta.Y + kerning.Glyph1Placement.Y);
                                    currentGlyphAdvanceDelta = new Point(currentGlyphAdvanceDelta.X + kerning.Glyph1Advance.X, currentGlyphAdvanceDelta.Y + kerning.Glyph1Advance.Y);

                                    nextGlyphPlacementDelta = new Point(nextGlyphPlacementDelta.X + kerning.Glyph2Placement.X, nextGlyphPlacementDelta.Y + kerning.Glyph2Placement.Y);
                                    nextGlyphAdvanceDelta = new Point(nextGlyphAdvanceDelta.X + kerning.Glyph2Advance.X, nextGlyphAdvanceDelta.Y + kerning.Glyph2Advance.Y);
                                }
                            }

                            double[] intersections = font.FontFamily.TrueTypeFile.Get1000EmUnderlineIntersections(c, font.Underline.Position * 1000, font.Underline.Thickness * 1000);

                            if (intersections != null)
                            {
                                intersections[0] = intersections[0] * font.FontSize / 1000;
                                intersections[1] = intersections[1] * font.FontSize / 1000;

                                if (currX + intersections[0] - font.Underline.Thickness * font.FontSize >= underlineStartX)
                                {
                                    if (!started)
                                    {
                                        started = true;
                                        this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize).LineTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                                    }

                                    this.LineTo(currX + intersections[0] - font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                                    this.LineTo(currX + intersections[0] - font.Underline.Thickness * font.FontSize + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                                    this.Close();
                                }

                                started = true;

                                this.MoveTo(currX + intersections[1] + font.Underline.Thickness * font.FontSize + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                                this.LineTo(currX + intersections[1] + font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize);

                                underlineStartX = currX + intersections[1] + font.Underline.Thickness * font.FontSize;
                                currUnderlineX = Math.Max(currX + intersections[1] + font.Underline.Thickness * font.FontSize, currX + (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000 - font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(c).RightSideBearing * font.FontSize / 1000);
                            }
                            else if (i == text.Length - 1)
                            {
                                if (c != ' ')
                                {
                                    currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000 - font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(c).RightSideBearing * font.FontSize / 1000;
                                }
                                else
                                {
                                    currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                                }
                            }
                            else
                            {
                                currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                            }

                            currX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                        }

                        if (!started)
                        {
                            started = true;
                            this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize).LineTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                        }

                        this.LineTo(currUnderlineX, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                        this.LineTo(currUnderlineX + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                        this.LineTo(underlineStartX + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                        this.Close();
                    }
                    else if (font.Underline.LineCap == LineCaps.Square)
                    {
                        double italicShift;

                        if (!font.Underline.FollowItalicAngle || italicAngle == 0)
                        {
                            italicShift = 0;
                        }
                        else
                        {
                            italicShift = font.Underline.Thickness * font.FontSize * Math.Tan(italicAngle / 180.0 * Math.PI);
                        }

                        bool started = false;

                        double currX = baselineOrigin.X;
                        double underlineStartX = baselineOrigin.X + metrics.LeftSideBearing;
                        double currUnderlineX = underlineStartX - metrics.LeftSideBearing;

                        Point currentGlyphPlacementDelta = new Point();
                        Point currentGlyphAdvanceDelta = new Point();
                        Point nextGlyphPlacementDelta = new Point();
                        Point nextGlyphAdvanceDelta = new Point();

                        for (int i = 0; i < text.Length; i++)
                        {
                            char c = text[i];

                            if (Font.EnableKerning && i < text.Length - 1)
                            {
                                currentGlyphPlacementDelta = nextGlyphPlacementDelta;
                                currentGlyphAdvanceDelta = nextGlyphAdvanceDelta;
                                nextGlyphAdvanceDelta = new Point();
                                nextGlyphPlacementDelta = new Point();

                                TrueTypeFile.PairKerning kerning = font.FontFamily.TrueTypeFile.Get1000EmKerning(c, text[i + 1]);

                                if (kerning != null)
                                {
                                    currentGlyphPlacementDelta = new Point(currentGlyphPlacementDelta.X + kerning.Glyph1Placement.X, currentGlyphPlacementDelta.Y + kerning.Glyph1Placement.Y);
                                    currentGlyphAdvanceDelta = new Point(currentGlyphAdvanceDelta.X + kerning.Glyph1Advance.X, currentGlyphAdvanceDelta.Y + kerning.Glyph1Advance.Y);

                                    nextGlyphPlacementDelta = new Point(nextGlyphPlacementDelta.X + kerning.Glyph2Placement.X, nextGlyphPlacementDelta.Y + kerning.Glyph2Placement.Y);
                                    nextGlyphAdvanceDelta = new Point(nextGlyphAdvanceDelta.X + kerning.Glyph2Advance.X, nextGlyphAdvanceDelta.Y + kerning.Glyph2Advance.Y);
                                }
                            }

                            double[] intersections = font.FontFamily.TrueTypeFile.Get1000EmUnderlineIntersections(c, font.Underline.Position * 1000, font.Underline.Thickness * 1000);

                            if (intersections != null)
                            {
                                intersections[0] = intersections[0] * font.FontSize / 1000;
                                intersections[1] = intersections[1] * font.FontSize / 1000;

                                if (currX + intersections[0] - font.Underline.Thickness * font.FontSize >= underlineStartX)
                                {
                                    if (!started)
                                    {
                                        started = true;
                                        this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing + italicShift - font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize).LineTo(baselineOrigin.X + metrics.LeftSideBearing - font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                                    }
                                    this.LineTo(currX + intersections[0] - font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                                    this.LineTo(currX + intersections[0] - font.Underline.Thickness * font.FontSize + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);

                                    this.Close();
                                }

                                started = true;

                                this.MoveTo(currX + intersections[1] + font.Underline.Thickness * font.FontSize + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                                this.LineTo(currX + intersections[1] + font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize);

                                underlineStartX = currX + intersections[1] + font.Underline.Thickness * font.FontSize;
                                currUnderlineX = Math.Max(currX + intersections[1] + font.Underline.Thickness * font.FontSize, currX + (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000 - font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(c).RightSideBearing * font.FontSize / 1000);
                            }
                            else if (i == text.Length - 1)
                            {
                                if (c != ' ')
                                {
                                    currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000 - font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(c).RightSideBearing * font.FontSize / 1000;
                                }
                                else
                                {
                                    currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                                }
                            }
                            else
                            {
                                currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                            }

                            currX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                        }

                        if (!started)
                        {
                            started = true;
                            this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing + italicShift - font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize).LineTo(baselineOrigin.X + metrics.LeftSideBearing - font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                        }

                        this.LineTo(currUnderlineX + font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                        this.LineTo(currUnderlineX + italicShift + font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                        this.LineTo(underlineStartX + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                        this.Close();
                    }
                    else if (font.Underline.LineCap == LineCaps.Round)
                    {
                        if (!font.Underline.FollowItalicAngle || italicAngle == 0)
                        {
                            bool started = false;

                            double currX = baselineOrigin.X;
                            double underlineStartX = baselineOrigin.X + metrics.LeftSideBearing;
                            double currUnderlineX = underlineStartX - metrics.LeftSideBearing;

                            Point currentGlyphPlacementDelta = new Point();
                            Point currentGlyphAdvanceDelta = new Point();
                            Point nextGlyphPlacementDelta = new Point();
                            Point nextGlyphAdvanceDelta = new Point();

                            for (int i = 0; i < text.Length; i++)
                            {
                                char c = text[i];

                                if (Font.EnableKerning && i < text.Length - 1)
                                {
                                    currentGlyphPlacementDelta = nextGlyphPlacementDelta;
                                    currentGlyphAdvanceDelta = nextGlyphAdvanceDelta;
                                    nextGlyphAdvanceDelta = new Point();
                                    nextGlyphPlacementDelta = new Point();

                                    TrueTypeFile.PairKerning kerning = font.FontFamily.TrueTypeFile.Get1000EmKerning(c, text[i + 1]);

                                    if (kerning != null)
                                    {
                                        currentGlyphPlacementDelta = new Point(currentGlyphPlacementDelta.X + kerning.Glyph1Placement.X, currentGlyphPlacementDelta.Y + kerning.Glyph1Placement.Y);
                                        currentGlyphAdvanceDelta = new Point(currentGlyphAdvanceDelta.X + kerning.Glyph1Advance.X, currentGlyphAdvanceDelta.Y + kerning.Glyph1Advance.Y);

                                        nextGlyphPlacementDelta = new Point(nextGlyphPlacementDelta.X + kerning.Glyph2Placement.X, nextGlyphPlacementDelta.Y + kerning.Glyph2Placement.Y);
                                        nextGlyphAdvanceDelta = new Point(nextGlyphAdvanceDelta.X + kerning.Glyph2Advance.X, nextGlyphAdvanceDelta.Y + kerning.Glyph2Advance.Y);
                                    }
                                }

                                double[] intersections = font.FontFamily.TrueTypeFile.Get1000EmUnderlineIntersections(c, font.Underline.Position * 1000, font.Underline.Thickness * 1000);

                                if (intersections != null)
                                {
                                    intersections[0] = intersections[0] * font.FontSize / 1000;
                                    intersections[1] = intersections[1] * font.FontSize / 1000;

                                    if (currX + intersections[0] - font.Underline.Thickness * font.FontSize >= underlineStartX)
                                    {
                                        if (!started)
                                        {
                                            started = true;
                                            this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                                            this.Arc(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize * 0.5, font.Underline.Thickness * font.FontSize * 0.5, Math.PI / 2, 3 * Math.PI / 2);
                                        }

                                        this.LineTo(currX + intersections[0] - font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                                        this.LineTo(currX + intersections[0] - font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);

                                        this.Close();
                                    }

                                    started = true;

                                    this.MoveTo(currX + intersections[1] + font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                                    this.LineTo(currX + intersections[1] + font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize);

                                    underlineStartX = currX + intersections[1] + font.Underline.Thickness * font.FontSize;
                                    currUnderlineX = Math.Max(currX + intersections[1] + font.Underline.Thickness * font.FontSize, currX + (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000 - font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(c).RightSideBearing * font.FontSize / 1000);
                                }
                                else if (i == text.Length - 1)
                                {
                                    if (c != ' ')
                                    {
                                        currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000 - font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(c).RightSideBearing * font.FontSize / 1000;
                                    }
                                    else
                                    {
                                        currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                                    }
                                }
                                else
                                {
                                    currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                                }

                                currX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                            }

                            if (!started)
                            {
                                started = true;
                                this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                                this.Arc(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize * 0.5, font.Underline.Thickness * font.FontSize * 0.5, Math.PI / 2, 3 * Math.PI / 2);
                            }

                            this.LineTo(currUnderlineX, baselineOrigin.Y + font.Underline.Position * font.FontSize);

                            this.Arc(currUnderlineX, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize * 0.5, font.Underline.Thickness * font.FontSize * 0.5, -Math.PI / 2, Math.PI / 2);

                            this.LineTo(underlineStartX, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.Close();
                        }
                        else
                        {
                            double italicShift = font.Underline.Thickness * font.FontSize * Math.Tan(italicAngle / 180.0 * Math.PI);

                            bool started = false;

                            double currX = baselineOrigin.X;
                            double underlineStartX = baselineOrigin.X + metrics.LeftSideBearing;
                            double currUnderlineX = underlineStartX - metrics.LeftSideBearing;

                            Point currentGlyphPlacementDelta = new Point();
                            Point currentGlyphAdvanceDelta = new Point();
                            Point nextGlyphPlacementDelta = new Point();
                            Point nextGlyphAdvanceDelta = new Point();

                            for (int i = 0; i < text.Length; i++)
                            {
                                char c = text[i];

                                if (Font.EnableKerning && i < text.Length - 1)
                                {
                                    currentGlyphPlacementDelta = nextGlyphPlacementDelta;
                                    currentGlyphAdvanceDelta = nextGlyphAdvanceDelta;
                                    nextGlyphAdvanceDelta = new Point();
                                    nextGlyphPlacementDelta = new Point();

                                    TrueTypeFile.PairKerning kerning = font.FontFamily.TrueTypeFile.Get1000EmKerning(c, text[i + 1]);

                                    if (kerning != null)
                                    {
                                        currentGlyphPlacementDelta = new Point(currentGlyphPlacementDelta.X + kerning.Glyph1Placement.X, currentGlyphPlacementDelta.Y + kerning.Glyph1Placement.Y);
                                        currentGlyphAdvanceDelta = new Point(currentGlyphAdvanceDelta.X + kerning.Glyph1Advance.X, currentGlyphAdvanceDelta.Y + kerning.Glyph1Advance.Y);

                                        nextGlyphPlacementDelta = new Point(nextGlyphPlacementDelta.X + kerning.Glyph2Placement.X, nextGlyphPlacementDelta.Y + kerning.Glyph2Placement.Y);
                                        nextGlyphAdvanceDelta = new Point(nextGlyphAdvanceDelta.X + kerning.Glyph2Advance.X, nextGlyphAdvanceDelta.Y + kerning.Glyph2Advance.Y);
                                    }
                                }

                                double[] intersections = font.FontFamily.TrueTypeFile.Get1000EmUnderlineIntersections(c, font.Underline.Position * 1000, font.Underline.Thickness * 1000);

                                if (intersections != null)
                                {
                                    intersections[0] = intersections[0] * font.FontSize / 1000;
                                    intersections[1] = intersections[1] * font.FontSize / 1000;

                                    if (currX + intersections[0] - font.Underline.Thickness * font.FontSize >= underlineStartX)
                                    {
                                        if (!started)
                                        {
                                            started = true;

                                            this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);

                                            this.CubicBezierTo(baselineOrigin.X + metrics.LeftSideBearing - font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize,
                                                baselineOrigin.X + metrics.LeftSideBearing - italicShift - font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize,
                                                baselineOrigin.X + metrics.LeftSideBearing - italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                                        }

                                        this.LineTo(currX + intersections[0] - font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                                        this.LineTo(currX + intersections[0] - font.Underline.Thickness * font.FontSize + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);

                                        this.Close();
                                    }

                                    started = true;

                                    this.MoveTo(currX + intersections[1] + font.Underline.Thickness * font.FontSize + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                                    this.LineTo(currX + intersections[1] + font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize);

                                    underlineStartX = currX + intersections[1] + font.Underline.Thickness * font.FontSize;
                                    currUnderlineX = Math.Max(currX + intersections[1] + font.Underline.Thickness * font.FontSize, currX + (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000 - font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(c).RightSideBearing * font.FontSize / 1000);
                                }
                                else if (i == text.Length - 1)
                                {
                                    if (c != ' ')
                                    {
                                        currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000 - font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(c).RightSideBearing * font.FontSize / 1000;
                                    }
                                    else
                                    {
                                        currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                                    }
                                }
                                else
                                {
                                    currUnderlineX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                                }

                                currX += (font.FontFamily.TrueTypeFile.Get1000EmGlyphWidth(c) + currentGlyphAdvanceDelta.X) * font.FontSize / 1000;
                            }

                            if (!started)
                            {
                                started = true;

                                this.MoveTo(baselineOrigin.X + metrics.LeftSideBearing, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);

                                this.CubicBezierTo(baselineOrigin.X + metrics.LeftSideBearing - font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize,
                                    baselineOrigin.X + metrics.LeftSideBearing - italicShift - font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize,
                                    baselineOrigin.X + metrics.LeftSideBearing - italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize);
                            }

                            this.LineTo(currUnderlineX, baselineOrigin.Y + font.Underline.Position * font.FontSize);

                            //this.LineTo(currUnderlineX + italicShift + font.Underline.Thickness * font.FontSize * 0.5, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.CubicBezierTo(currUnderlineX + font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize,
                                currUnderlineX + italicShift + font.Underline.Thickness * font.FontSize, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize,
                                currUnderlineX + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);

                            this.LineTo(underlineStartX + italicShift, baselineOrigin.Y + font.Underline.Position * font.FontSize + font.Underline.Thickness * font.FontSize);
                            this.Close();
                        }
                    }
                }
            }
            return this;
        }


        /// <summary>
        /// Adds a smooth spline composed of cubic bezier segments that pass through the specified points.
        /// </summary>
        /// <param name="points">The points through which the spline should pass.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath AddSmoothSpline(params Point[] points)
        {
            if (points.Length == 0)
            {
                return this;
            }
            else if (points.Length == 1)
            {
                return this.LineTo(points[0]);
            }
            else if (points.Length == 2)
            {
                return this.LineTo(points[0]).LineTo(points[1]);
            }

            Point[] smoothedSpline = SmoothSpline.SmoothSplines(points);

            this.LineTo(smoothedSpline[0]);

            for (int i = 1; i < smoothedSpline.Length; i += 3)
            {
                this.CubicBezierTo(smoothedSpline[i], smoothedSpline[i + 1], smoothedSpline[i + 2]);
            }

            return this;
        }

        /// <summary>
        /// Adds another <see cref="GraphicsPath"/> to the current <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The existing <see cref="GraphicsPath"/> that should be added to the current <see cref="GraphicsPath"/>.</param>
        /// <returns>The <see cref="GraphicsPath"/>, to allow for chained calls.</returns>
        public GraphicsPath AddPath(GraphicsPath path)
        {
            this.Segments.AddRange(path.Segments);
            return this;
        }

        private double cachedLength = double.NaN;

        /// <summary>
        /// Measures the length of the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <returns>The length of the <see cref="GraphicsPath"/></returns>
        public double MeasureLength()
        {
            if (double.IsNaN(cachedLength))
            {
                cachedLength = 0;
                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                cachedLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                cachedLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                cachedLength += this.Segments[i].Measure(figureStartPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Close:
                            cachedLength += Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));
                            currPoint = figureStartPoint;
                            break;
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                cachedLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                cachedLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                    }
                }
            }

            return cachedLength;
        }

        /// <summary>
        /// Gets the point at the relative position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="position">The position on the <see cref="GraphicsPath"/> (0 is the start of the path, 1 is the end of the path).</param>
        /// <returns>The point at the specified position.</returns>
        public Point GetPointAtRelative(double position)
        {
            return GetPointAtAbsolute(position * this.MeasureLength());
        }

        /// <summary>
        /// Gets the point at the absolute position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="length">The distance to the point from the start of the <see cref="GraphicsPath"/>.</param>
        /// <returns>The point at the specified position.</returns>
        public Point GetPointAtAbsolute(double length)
        {
            double pathLength = this.MeasureLength();

            if (length >= 0 && length <= pathLength)
            {
                double currLen = 0;

                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else if (segLength > 0)
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetPointAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else if (segLength > 0)
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetPointAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currPoint = figureStartPoint;

                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else if (segLength > 0)
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetPointAt(currPoint, pos);
                                }
                            }
                            break;
                        case SegmentType.Close:
                            {
                                double segLength = Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = figureStartPoint;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return new Point(currPoint.X * (1 - pos) + figureStartPoint.X * pos, currPoint.Y * (1 - pos) + figureStartPoint.Y * pos);
                                }
                            }
                            break;
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else if (segLength > 0)
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetPointAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else if (segLength > 0)
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetPointAt(currPoint, pos);
                                }
                            }
                            break;
                    }
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
            else if (length > pathLength)
            {
                double currLength = 0;

                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count - 1; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currLength += this.Segments[i].Measure(figureStartPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Close:
                            currLength += Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));
                            currPoint = figureStartPoint;
                            break;
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                    }
                }

                switch (this.Segments[this.Segments.Count - 1].Type)
                {
                    case SegmentType.Arc:
                    case SegmentType.CubicBezier:
                    case SegmentType.Line:
                        {
                            double pos = 1 + (length - pathLength) / this.Segments[this.Segments.Count - 1].Measure(currPoint);
                            return this.Segments[this.Segments.Count - 1].GetPointAt(currPoint, pos);
                        }
                    case SegmentType.Move:
                        return currPoint;
                    case SegmentType.Close:
                        return this.GetPointAtAbsolute(length - pathLength);
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
            else
            {
                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetPointAt(currPoint, pos);
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetPointAt(currPoint, pos);
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currPoint = figureStartPoint;

                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetPointAt(currPoint, pos);
                            }
                        case SegmentType.Close:
                            {
                                double segLength = Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));
                                double pos = length / segLength;
                                return new Point(currPoint.X * (1 - pos) + figureStartPoint.X * pos, currPoint.Y * (1 - pos) + figureStartPoint.Y * pos);
                            }
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetPointAt(currPoint, pos);
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetPointAt(currPoint, pos);
                            }
                    }
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
        }

        /// <summary>
        /// Gets the tangent to the point at the relative position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="position">The position on the <see cref="GraphicsPath"/> (0 is the start of the path, 1 is the end of the path).</param>
        /// <returns>The tangent to the point at the specified position.</returns>
        public Point GetTangentAtRelative(double position)
        {
            return GetTangentAtAbsolute(position * this.MeasureLength());
        }

        /// <summary>
        /// Gets the tangent to the point at the absolute position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="length">The distance to the point from the start of the <see cref="GraphicsPath"/>.</param>
        /// <returns>The tangent to the point at the specified position.</returns>
        public Point GetTangentAtAbsolute(double length)
        {
            double pathLength = this.MeasureLength();

            if (length >= 0 && length <= pathLength)
            {
                double currLen = 0;

                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetTangentAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetTangentAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currPoint = figureStartPoint;

                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetTangentAt(currPoint, pos);
                                }
                            }
                            break;
                        case SegmentType.Close:
                            {
                                double segLength = Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = figureStartPoint;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return new Point(figureStartPoint.X - currPoint.X, figureStartPoint.Y - currPoint.Y).Normalize();
                                }
                            }
                            break;
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetTangentAt(currPoint, pos);
                                }
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                double segLength = this.Segments[i].Measure(currPoint);

                                if (currLen + segLength < length)
                                {
                                    currLen += segLength;
                                    currPoint = this.Segments[i].Point;
                                }
                                else
                                {
                                    double pos = (length - currLen) / segLength;
                                    return this.Segments[i].GetTangentAt(currPoint, pos);
                                }
                            }
                            break;
                    }
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
            else if (length > pathLength)
            {
                double currLength = 0;

                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count - 1; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currLength += this.Segments[i].Measure(figureStartPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Close:
                            currLength += Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));
                            currPoint = figureStartPoint;
                            break;
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                currLength += this.Segments[i].Measure(currPoint);
                                currPoint = this.Segments[i].Point;
                            }
                            break;
                    }
                }

                switch (this.Segments[this.Segments.Count - 1].Type)
                {
                    case SegmentType.Arc:
                    case SegmentType.CubicBezier:
                    case SegmentType.Line:
                        {
                            double pos = 1 + (length - pathLength) / this.Segments[this.Segments.Count - 1].Measure(currPoint);
                            return this.Segments[this.Segments.Count - 1].GetTangentAt(currPoint, pos);
                        }
                    case SegmentType.Move:
                        return new Point();
                    case SegmentType.Close:
                        return this.GetTangentAtAbsolute(length - pathLength);
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
            else
            {
                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetTangentAt(currPoint, pos);
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            break;
                        case SegmentType.Arc:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetTangentAt(currPoint, pos);
                            }
                            else
                            {
                                ArcSegment seg = (ArcSegment)this.Segments[i];
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currPoint = figureStartPoint;

                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetTangentAt(currPoint, pos);
                            }
                        case SegmentType.Close:
                            {
                                double segLength = Math.Sqrt((currPoint.X - figureStartPoint.X) * (currPoint.X - figureStartPoint.X) + (currPoint.Y - figureStartPoint.Y) * (currPoint.Y - figureStartPoint.Y));
                                double pos = length / segLength;
                                return new Point(figureStartPoint.X - currPoint.X, figureStartPoint.Y - currPoint.Y).Normalize();
                            }
                        case SegmentType.CubicBezier:
                            if (i > 0)
                            {
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetTangentAt(currPoint, pos);
                            }
                            else
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                                double segLength = this.Segments[i].Measure(currPoint);
                                double pos = length / segLength;
                                return this.Segments[i].GetTangentAt(currPoint, pos);
                            }
                    }
                }

                throw new InvalidOperationException("Unexpected code path!");
            }
        }

        /// <summary>
        /// Gets the normal to the point at the absolute position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="length">The distance to the point from the start of the <see cref="GraphicsPath"/>.</param>
        /// <returns>The normal to the point at the specified position.</returns>
        public Point GetNormalAtAbsolute(double length)
        {
            Point tangent = this.GetTangentAtAbsolute(length);
            return new Point(-tangent.Y, tangent.X);
        }

        /// <summary>
        /// Gets the normal to the point at the relative position specified on the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="position">The position on the <see cref="GraphicsPath"/> (0 is the start of the path, 1 is the end of the path).</param>
        /// <returns>The normal to the point at the specified position.</returns>
        public Point GetNormalAtRelative(double position)
        {
            Point tangent = this.GetTangentAtRelative(position);
            return new Point(-tangent.Y, tangent.X);
        }

        /// <summary>
        /// Linearises a <see cref="GraphicsPath"/>, replacing curve segments with series of line segments that approximate them.
        /// </summary>
        /// <param name="resolution">The absolute length between successive samples in curve segments.</param>
        /// <returns>A <see cref="GraphicsPath"/> composed only of linear segments that approximates the current <see cref="GraphicsPath"/>.</returns>
        public GraphicsPath Linearise(double resolution)
        {
            if (!(resolution > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The resolution must be greater than 0!");
            }

            GraphicsPath tbr = new GraphicsPath();

            Point?[] previousPoints = new Point?[this.Segments.Count];

            previousPoints[0] = null;

            for (int i = 0; i < this.Segments.Count - 1; i++)
            {
                if (this.Segments[i].Type != SegmentType.Close)
                {
                    previousPoints[i + 1] = this.Segments[i].Point;
                }
                else
                {
                    previousPoints[i + 1] = previousPoints[i];
                }
            }

            Segment[][] linearisedSegments = new Segment[this.Segments.Count][];

            System.Threading.Tasks.Parallel.For(0, this.Segments.Count, i =>
            {
                linearisedSegments[i] = this.Segments[i].Linearise(previousPoints[i], resolution).ToArray();
            });

            int total = 0;

            for (int i = 0; i < linearisedSegments.Length; i++)
            {
                total += linearisedSegments[i].Length;
            }

            tbr.Segments.Capacity = total;

            for (int i = 0; i < linearisedSegments.Length; i++)
            {
                tbr.Segments.AddRange(linearisedSegments[i]);
            }

            return tbr;
        }

        /// <summary>
        /// Discretises a <see cref="GraphicsPath"/>, replacing curve segments with series of line segments that approximate them and ensuring that all line segments are shorter than the specified <paramref name="resolution"/>.
        /// </summary>
        /// <param name="resolution">The maximum length (in absolute units) of line segments in the resulting <see cref="GraphicsPath"/>.</param>
        /// <returns>A <see cref="GraphicsPath"/> composed only of linear segments that are shorter than <paramref name="resolution"/> and approximate the current <see cref="GraphicsPath"/>.</returns>
        public GraphicsPath Discretise(double resolution)
        {
            if (!(resolution > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The resolution must be greater than 0!");
            }

            GraphicsPath tbr = new GraphicsPath();

            Point? previousPoint = null;

            foreach (Segment seg in this.Segments)
            {
                tbr.Segments.AddRange(seg.Linearise(previousPoint, resolution));

                if (seg.Type != SegmentType.Close)
                {
                    previousPoint = seg.Point;
                }
            }

            return Graphics.ReduceMaximumLength(tbr, resolution);
        }


        /// <summary>
        /// Flattens a <see cref="GraphicsPath"/>, replacing curve segments with series of line segments that approximate them, ensuring the specified maximum deviation from the original path.
        /// </summary>
        /// <param name="flatness">The maximum deviation from the original path.</param>
        /// <returns>A <see cref="GraphicsPath"/> composed only of linear segments that approximates the current <see cref="GraphicsPath"/>.</returns>
        public GraphicsPath Flatten(double flatness)
        {
            if (!(flatness > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(flatness), flatness, "The flatness must be greater than 0!");
            }

            GraphicsPath tbr = new GraphicsPath();

            Point?[] previousPoints = new Point?[this.Segments.Count];

            previousPoints[0] = null;

            for (int i = 0; i < this.Segments.Count - 1; i++)
            {
                if (this.Segments[i].Type != SegmentType.Close)
                {
                    previousPoints[i + 1] = this.Segments[i].Point;
                }
                else
                {
                    previousPoints[i + 1] = previousPoints[i];
                }
            }

            Segment[][] flattenedSegments = new Segment[this.Segments.Count][];

            System.Threading.Tasks.Parallel.For(0, this.Segments.Count, i =>
            {
                flattenedSegments[i] = this.Segments[i].Flatten(previousPoints[i], flatness).ToArray();
            });

            int total = 0;

            for (int i = 0; i < flattenedSegments.Length; i++)
            {
                total += flattenedSegments[i].Length;
            }

            tbr.Segments.Capacity = total;

            for (int i = 0; i < flattenedSegments.Length; i++)
            {
                tbr.Segments.AddRange(flattenedSegments[i]);
            }

            return tbr;
        }

        /// <summary>
        /// Gets a collection of the end points of all the segments in the <see cref="GraphicsPath"/>, divided by figure.
        /// </summary>
        /// <returns>A collection of the end points of all the segments in the <see cref="GraphicsPath"/>, divided by figure.</returns>
        public IEnumerable<List<Point>> GetPoints()
        {
            Point startPoint = new Point();

            List<Point> currFigure = null;
            bool returned = true;

            foreach (Segment seg in this.Segments)
            {
                if (seg.Type != SegmentType.Close)
                {
                    Point currPoint = seg.Point;
                    if (seg.Type == SegmentType.Move)
                    {
                        if (!returned)
                        {
                            yield return currFigure;
                        }

                        startPoint = currPoint;
                        currFigure = new List<Point>();
                        returned = false;
                    }
                    currFigure.Add(currPoint);
                }
                else
                {
                    currFigure.Add(startPoint);
                    yield return currFigure;
                    returned = true;
                }
            }

            if (!returned)
            {
                yield return currFigure;
            }
        }

        /// <summary>
        /// Gets a collection of all the figures in the <see cref="GraphicsPath"/>, returned as individual <see cref="GraphicsPath"/>s.
        /// </summary>
        /// <returns>A collection of all the figures in the <see cref="GraphicsPath"/>, returned as individual <see cref="GraphicsPath"/>s.</returns>
        public IEnumerable<GraphicsPath> GetFigures()
        {
            GraphicsPath currFigure = null;
            bool returned = true;

            foreach (Segment seg in this.Segments)
            {
                if (seg.Type != SegmentType.Close)
                {
                    Point currPoint = seg.Point;
                    if (seg.Type == SegmentType.Move)
                    {
                        if (!returned)
                        {
                            yield return currFigure;
                        }

                        currFigure = new GraphicsPath();
                        returned = false;
                    }

                    currFigure.Segments.Add(seg);
                }
                else
                {
                    currFigure.Close();
                    yield return currFigure;
                    returned = true;
                }
            }

            if (!returned)
            {
                yield return currFigure;
            }
        }


        /// <summary>
        /// Gets a collection of the tangents at the end point of the segments in which the <see cref="GraphicsPath"/> would be linearised, divided by figure.
        /// </summary>
        /// <param name="resolution">The absolute length between successive samples in curve segments.</param>
        /// <returns>A collection of the tangents at the end point of the segments in which the <see cref="GraphicsPath"/> would be linearised, divided by figure.</returns>
        public IEnumerable<List<Point>> GetLinearisationPointsNormals(double resolution)
        {
            if (!(resolution > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The resolution must be greater than 0!");
            }

            Point previousPoint = new Point();
            Point startPoint = new Point();

            List<Point> currFigure = null;
            bool returned = true;

            for (int i = 0; i < this.Segments.Count; i++)
            {
                Segment seg = this.Segments[i];

                if (seg.Type != SegmentType.Close)
                {
                    Point currPoint = seg.Point;
                    if (seg.Type == SegmentType.Move)
                    {
                        if (!returned)
                        {
                            yield return currFigure;
                        }

                        startPoint = currPoint;
                        currFigure = new List<Point>();
                        returned = false;

                        if (i < this.Segments.Count - 1 && this.Segments[i + 1].Type != SegmentType.Move)
                        {
                            Point tangent = this.Segments[i + 1].GetTangentAt(seg.Point, 0);

                            currFigure.Add(new Point(-tangent.Y, tangent.X));
                        }
                        else
                        {
                            currFigure.Add(new Point());
                        }
                    }
                    else
                    {
                        foreach (Point tangent in seg.GetLinearisationTangents(previousPoint, resolution))
                        {
                            currFigure.Add(new Point(-tangent.Y, tangent.X));
                        }
                    }

                    previousPoint = currPoint;
                }
                else
                {
                    Point normal;

                    if (!startPoint.IsEqual(previousPoint, 1e-4))
                    {
                        Point tangent = new Point(startPoint.X - previousPoint.X, startPoint.Y - previousPoint.Y).Normalize();
                        normal = new Point(-tangent.Y, tangent.X);
                    }
                    else
                    {
                        normal = currFigure[currFigure.Count - 1];
                    }

                    currFigure.Add(normal);
                    currFigure[0] = new Point((currFigure[1].X + normal.X) * 0.5, (currFigure[1].Y + normal.Y) * 0.5).Normalize();

                    yield return currFigure;
                    returned = true;
                }
            }

            if (!returned)
            {
                yield return currFigure;
            }

        }


        private enum VertexType
        {
            Start, End, Regular, Split, Merge
        };

        /// <summary>
        /// Divides a <see cref="GraphicsPath"/> into triangles.
        /// </summary>
        /// <param name="resolution">The resolution that will be used to linearise curve segments in the <see cref="GraphicsPath"/>.</param>
        /// <param name="clockwise">If this is <see langword="true"/>, the triangles will have their vertices in a clockwise order, otherwise they will be in anticlockwise order.</param>
        /// <returns>A collection of distinct <see cref="GraphicsPath"/>s, each representing one triangle.</returns>
        public IEnumerable<GraphicsPath> Triangulate(double resolution, bool clockwise)
        {
            double shiftAmount = 0.01 * resolution;

            if (!(resolution > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The resolution must be greater than 0!");
            }

            GraphicsPath linearisedPath = this.Linearise(resolution);

            List<Point> vertices = new List<Point>();
            List<List<int>> vertexEdges = new List<List<int>>();
            List<(int, int)> edges = new List<(int, int)>();
            int lastStartingPoint = -1;
            int lastSegmentEnd = -1;
            double area = 0;

            foreach (Segment seg in linearisedPath.Segments)
            {
                if (seg is MoveSegment)
                {
                    vertices.Add(seg.Point);
                    vertexEdges.Add(new List<int>(2));
                    lastStartingPoint = vertices.Count - 1;
                    lastSegmentEnd = vertices.Count - 1;
                }
                else if (seg is LineSegment)
                {
                    if (!vertices[lastSegmentEnd].IsEqual(seg.Point, 1e-4))
                    {
                        vertices.Add(seg.Point);
                        vertexEdges.Add(new List<int>(2));
                        edges.Add((lastSegmentEnd, vertices.Count - 1));
                        area += (seg.Point.X - vertices[lastSegmentEnd].X) * (seg.Point.Y + vertices[lastSegmentEnd].Y);
                        vertexEdges[lastSegmentEnd].Add(edges.Count - 1);
                        vertexEdges[vertices.Count - 1].Add(edges.Count - 1);
                        lastSegmentEnd = vertices.Count - 1;
                    }
                }
                else if (seg is CloseSegment)
                {
                    if (!vertices[lastSegmentEnd].IsEqual(vertices[lastStartingPoint], 1e-4))
                    {
                        edges.Add((lastSegmentEnd, lastStartingPoint));
                        area += (vertices[lastStartingPoint].X - vertices[lastSegmentEnd].X) * (vertices[lastStartingPoint].Y + vertices[lastSegmentEnd].Y);
                        vertexEdges[lastSegmentEnd].Add(edges.Count - 1);
                        vertexEdges[lastStartingPoint].Add(edges.Count - 1);
                    }
                    else
                    {
                        vertices.RemoveAt(lastSegmentEnd);
                        vertexEdges.RemoveAt(lastSegmentEnd);

                        for (int i = 0; i < edges.Count; i++)
                        {
                            if (edges[i].Item1 == lastSegmentEnd)
                            {
                                edges[i] = (lastStartingPoint, edges[i].Item2);
                                vertexEdges[lastStartingPoint].Add(i);
                            }
                            else if (edges[i].Item2 == lastSegmentEnd)
                            {
                                edges[i] = (edges[i].Item1, lastStartingPoint);
                                vertexEdges[lastStartingPoint].Add(i);
                            }
                        }
                    }

                    lastStartingPoint = -1;
                    lastSegmentEnd = -1;
                }
            }

            if (vertices.Count < 3)
            {
                yield break;
            }

            bool isAntiClockwise = area > 0;

            int compareVertices(Point a, Point b)
            {
                if (a.Y - b.Y != 0)
                {
                    return Math.Sign(a.Y - b.Y);
                }
                else
                {
                    return Math.Sign(a.X - b.X);
                }
            }

            Dictionary<double, int> yCoordinates = new Dictionary<double, int>();
            Dictionary<double, int> yShiftCount = new Dictionary<double, int>();

            foreach (Point pt in vertices)
            {
                if (yCoordinates.ContainsKey(pt.Y))
                {
                    yCoordinates[pt.Y]++;
                }
                else
                {
                    yCoordinates[pt.Y] = 1;
                    yShiftCount[pt.Y] = 0;
                }
            }

            HashSet<double> yS = new HashSet<double>(from el in yCoordinates select el.Key);

            for (int i = 0; i < vertices.Count; i++)
            {
                if (yCoordinates[vertices[i].Y] > 1)
                {
                    int shiftCount = yShiftCount[vertices[i].Y];

                    double targetCoordinate;

                    do
                    {
                        shiftCount++;

                        targetCoordinate = vertices[i].Y + (2 * (shiftCount % 2) - 1) * (1 - Math.Pow(0.5, (shiftCount - 1) / 2 + 1)) * shiftAmount;
                    }
                    while (yS.Contains(targetCoordinate));

                    yS.Add(targetCoordinate);
                    yCoordinates[vertices[i].Y]--;
                    yShiftCount[vertices[i].Y] = shiftCount;
                    vertices[i] = new Point(vertices[i].X, targetCoordinate);
                }

            }

            Queue<int> sortedVertices = new Queue<int>(Enumerable.Range(0, vertices.Count).OrderBy(i => vertices[i], Comparer<Point>.Create(compareVertices)));

            VertexType[] vertexTypes = new VertexType[vertices.Count];

            List<(int, int)> exploredEdges = new List<(int, int)>();
            List<int> helpers = new List<int>();
            List<(int, int)> diagonals = new List<(int, int)>();
            int[] nexts = new int[vertices.Count];
            int[] prevs = new int[vertices.Count];

            while (sortedVertices.Count > 0)
            {
                int vertex = sortedVertices.Dequeue();

                Point pt = vertices[vertex];

                (int, int) edge1 = edges[vertexEdges[vertex][0]];
                (int, int) edge2 = edges[vertexEdges[vertex][1]];

                int neighbour1 = edge1.Item1 != vertex ? edge1.Item1 : edge1.Item2;
                int neighbour2 = edge2.Item1 != vertex ? edge2.Item1 : edge2.Item2;

                int minNeighbour = Math.Min(neighbour1, neighbour2);
                int maxNeighbour = Math.Max(neighbour1, neighbour2);

                int prev, next;

                if (vertex - minNeighbour == 1 && maxNeighbour - vertex == 1)
                {
                    prev = minNeighbour;
                    next = maxNeighbour;
                }
                else if ((minNeighbour - vertex == 1 && maxNeighbour - vertex > 1) || vertex - maxNeighbour == 1 && vertex - minNeighbour > 1)
                {
                    prev = maxNeighbour;
                    next = minNeighbour;
                }
                else
                {
                    throw new InvalidOperationException("Could not make sense of the ordering of the vertices!");
                }

                nexts[vertex] = next;
                prevs[vertex] = prev;

                Point prevPoint = vertices[prev];
                Point nextPoint = vertices[next];

                double angle = Math.Atan2(prevPoint.Y - pt.Y, prevPoint.X - pt.X) - Math.Atan2(nextPoint.Y - pt.Y, nextPoint.X - pt.X);

                if (angle < 0)
                {
                    angle += 2 * Math.PI;
                }

                VertexType vertexType;

                if (prevPoint.Y >= pt.Y && nextPoint.Y >= pt.Y && angle < Math.PI)
                {
                    vertexType = VertexType.Start;
                }
                else if (prevPoint.Y >= pt.Y && nextPoint.Y >= pt.Y && angle > Math.PI)
                {
                    vertexType = VertexType.Split;
                }
                else if (prevPoint.Y <= pt.Y && nextPoint.Y <= pt.Y && angle < Math.PI)
                {
                    vertexType = VertexType.End;
                }
                else if (prevPoint.Y <= pt.Y && nextPoint.Y <= pt.Y && angle > Math.PI)
                {
                    vertexType = VertexType.Merge;
                }
                else
                {
                    vertexType = VertexType.Regular;
                }

                vertexTypes[vertex] = vertexType;

                //gpr.FillText(vertices[vertex], vertex.ToString(), new Font(new FontFamily(FontFamily.StandardFontFamilies.Helvetica), 4), Colours.Orange);

                if (vertexType == VertexType.Start)
                {
                    exploredEdges.Add((prev, vertex));
                    helpers.Add(vertex);

                    //gpr.StrokeRectangle(pt.X - 1, pt.Y - 1, 2, 2, Colours.Green, 0.25);
                }
                else if (vertexType == VertexType.End)
                {
                    int eiM1 = -1;

                    for (int i = exploredEdges.Count - 1; i >= 0; i--)
                    {
                        if (exploredEdges[i].Item1 == vertex && exploredEdges[i].Item2 == next)
                        {
                            eiM1 = i;
                            break;
                        }
                    }

                    if (eiM1 >= 0)
                    {
                        if (vertexTypes[helpers[eiM1]] == VertexType.Merge)
                        {
                            diagonals.Add((helpers[eiM1], vertex));
                        }

                        exploredEdges.RemoveAt(eiM1);
                        helpers.RemoveAt(eiM1);
                    }
                }
                else if (vertexType == VertexType.Split)
                {
                    (int, int) ej = (-1, -1);
                    int ejIndex = -1;

                    double xJ = double.MinValue;

                    for (int i = 0; i < exploredEdges.Count; i++)
                    {
                        if ((vertices[exploredEdges[i].Item1].Y <= pt.Y && vertices[exploredEdges[i].Item2].Y >= pt.Y) || (vertices[exploredEdges[i].Item1].Y >= pt.Y && vertices[exploredEdges[i].Item2].Y <= pt.Y))
                        {
                            double dy = pt.Y - vertices[exploredEdges[i].Item1].Y;
                            double dx = dy * (vertices[exploredEdges[i].Item2].X - vertices[exploredEdges[i].Item1].X) / (vertices[exploredEdges[i].Item2].Y - vertices[exploredEdges[i].Item1].Y);

                            double x = dx + vertices[exploredEdges[i].Item1].X;

                            if (x < pt.X && x >= xJ)
                            {
                                xJ = x;
                                ej = exploredEdges[i];
                                ejIndex = i;
                            }
                        }
                    }

                    if (ejIndex >= 0)
                    {
                        diagonals.Add((helpers[ejIndex], vertex));

                        helpers[ejIndex] = vertex;

                        exploredEdges.Add((prev, vertex));
                        helpers.Add(vertex);
                    }
                }
                else if (vertexType == VertexType.Merge)
                {
                    int eiM1 = -1;

                    for (int i = exploredEdges.Count - 1; i >= 0; i--)
                    {
                        if (exploredEdges[i].Item1 == vertex && exploredEdges[i].Item2 == next)
                        {
                            eiM1 = i;
                            break;
                        }
                    }

                    if (eiM1 >= 0)
                    {
                        if (vertexTypes[helpers[eiM1]] == VertexType.Merge)
                        {
                            diagonals.Add((helpers[eiM1], vertex));
                        }

                        exploredEdges.RemoveAt(eiM1);
                        helpers.RemoveAt(eiM1);
                    }

                    (int, int) ej = (-1, -1);
                    int ejIndex = -1;

                    double xJ = double.MinValue;

                    for (int i = 0; i < exploredEdges.Count; i++)
                    {
                        if ((vertices[exploredEdges[i].Item1].Y <= pt.Y && vertices[exploredEdges[i].Item2].Y >= pt.Y) || (vertices[exploredEdges[i].Item1].Y >= pt.Y && vertices[exploredEdges[i].Item2].Y <= pt.Y))
                        {
                            double dy = pt.Y - vertices[exploredEdges[i].Item1].Y;
                            double dx = dy * (vertices[exploredEdges[i].Item2].X - vertices[exploredEdges[i].Item1].X) / (vertices[exploredEdges[i].Item2].Y - vertices[exploredEdges[i].Item1].Y);

                            double x = dx + vertices[exploredEdges[i].Item1].X;

                            if (x < pt.X && x >= xJ)
                            {
                                xJ = x;
                                ej = exploredEdges[i];
                                ejIndex = i;
                            }
                        }
                    }

                    if (ejIndex >= 0)
                    {
                        if (vertexTypes[helpers[ejIndex]] == VertexType.Merge)
                        {
                            diagonals.Add((helpers[ejIndex], vertex));
                        }

                        helpers[ejIndex] = vertex;
                    }
                }
                else if (vertexType == VertexType.Regular)
                {
                    if ((isAntiClockwise && (prevPoint.Y < pt.Y || pt.Y < nextPoint.Y)) || (!isAntiClockwise && (prevPoint.Y > pt.Y || pt.Y > nextPoint.Y)))
                    {
                        int eiM1 = -1;

                        for (int i = exploredEdges.Count - 1; i >= 0; i--)
                        {
                            if (exploredEdges[i].Item1 == vertex && exploredEdges[i].Item2 == next)
                            {
                                eiM1 = i;
                                break;
                            }
                        }

                        if (eiM1 >= 0)
                        {
                            if (vertexTypes[helpers[eiM1]] == VertexType.Merge)
                            {
                                diagonals.Add((helpers[eiM1], vertex));
                            }

                            exploredEdges.RemoveAt(eiM1);
                            helpers.RemoveAt(eiM1);
                        }

                        exploredEdges.Add((prev, vertex));
                        helpers.Add(vertex);
                    }
                    else
                    {
                        (int, int) ej = (-1, -1);
                        int ejIndex = -1;

                        double xJ = double.MinValue;

                        for (int i = 0; i < exploredEdges.Count; i++)
                        {
                            if ((vertices[exploredEdges[i].Item1].Y <= pt.Y && vertices[exploredEdges[i].Item2].Y >= pt.Y) || (vertices[exploredEdges[i].Item1].Y >= pt.Y && vertices[exploredEdges[i].Item2].Y <= pt.Y))
                            {
                                double dy = pt.Y - vertices[exploredEdges[i].Item1].Y;
                                double dx = dy * (vertices[exploredEdges[i].Item2].X - vertices[exploredEdges[i].Item1].X) / (vertices[exploredEdges[i].Item2].Y - vertices[exploredEdges[i].Item1].Y);

                                double x = dx + vertices[exploredEdges[i].Item1].X;

                                if (x < pt.X && x >= xJ)
                                {
                                    xJ = x;
                                    ej = exploredEdges[i];
                                    ejIndex = i;
                                }
                            }
                        }

                        if (ejIndex >= 0)
                        {
                            if (vertexTypes[helpers[ejIndex]] == VertexType.Merge)
                            {
                                diagonals.Add((helpers[ejIndex], vertex));
                            }

                            helpers[ejIndex] = vertex;
                        }
                    }
                }
            }

            for (int i = diagonals.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < edges.Count; j++)
                {
                    if (CompareEdges(diagonals[i], edges[j]))
                    {
                        diagonals.RemoveAt(i);
                        break;
                    }
                }
            }

            List<List<(int, int)>> polygons = SplitPolygons(edges, diagonals, vertices, isAntiClockwise ? prevs : nexts);

            int[] directions = new int[vertices.Count];

            int ind = 0;

            foreach (List<(int, int)> polygon in polygons)
            {
                foreach (GraphicsPath pth in TriangulateMonotone(vertices, polygon, directions, clockwise ? -1 : 1))
                {
                    yield return pth;
                }

                ind++;
            }
        }

        private static bool CompareEdges((int, int) edge1, (int, int) edge2)
        {
            return (edge1.Item1 == edge2.Item1 && edge1.Item2 == edge2.Item2) || (edge1.Item1 == edge2.Item2 && edge1.Item2 == edge2.Item1);
        }

        private static List<List<(int, int)>> SplitPolygons(List<(int, int)> edges, List<(int, int)> diagonals, List<Point> vertices, int[] nexts)
        {
            List<List<(int, int)>> polygons = new List<List<(int, int)>>();

            List<int>[] outPaths = new List<int>[vertices.Count];

            for (int i = 0; i < edges.Count; i++)
            {
                if (outPaths[edges[i].Item1] == null)
                {
                    outPaths[edges[i].Item1] = new List<int>();
                }

                if (outPaths[edges[i].Item2] == null)
                {
                    outPaths[edges[i].Item2] = new List<int>();
                }

                outPaths[edges[i].Item1].Add(edges[i].Item2);
                outPaths[edges[i].Item2].Add(edges[i].Item1);
            }

            for (int i = 0; i < diagonals.Count; i++)
            {
                if (outPaths[diagonals[i].Item1] == null)
                {
                    outPaths[diagonals[i].Item1] = new List<int>();
                }

                if (outPaths[diagonals[i].Item2] == null)
                {
                    outPaths[diagonals[i].Item2] = new List<int>();
                }

                outPaths[diagonals[i].Item1].Add(diagonals[i].Item2);
                outPaths[diagonals[i].Item1].Add(diagonals[i].Item2);
                outPaths[diagonals[i].Item2].Add(diagonals[i].Item1);
                outPaths[diagonals[i].Item2].Add(diagonals[i].Item1);
            }

            List<int> activeVertices = (from el in Enumerable.Range(0, outPaths.Length) where outPaths[el] != null && outPaths[el].Count > 0 select el).ToList();

            int[] newNexts = new int[nexts.Length];

            for (int i = 0; i < nexts.Length; i++)
            {
                if (activeVertices.Contains(i))
                {
                    int currNext = nexts[i];
                    while (!outPaths[i].Contains(currNext))
                    {
                        currNext = nexts[currNext];
                    }
                    newNexts[i] = currNext;
                }
                else
                {
                    newNexts[i] = -1;
                }
            }

            while (activeVertices.Count > 0)
            {
                List<(int, int)> polygon = new List<(int, int)>();

                int startPoint = activeVertices.Min();

                if (!outPaths[startPoint].Contains(newNexts[startPoint]))
                {
                    throw new InvalidOperationException("Missing edge!");
                }

                int prevVertex = startPoint;
                int currVertex = newNexts[startPoint];

                outPaths[startPoint].Remove(currVertex);
                outPaths[currVertex].Remove(startPoint);
                polygon.Add((startPoint, currVertex));

                while (currVertex != startPoint)
                {
                    Point currPoint = vertices[currVertex];
                    Point prevPoint = vertices[prevVertex];

                    double angleIncoming = Math.Atan2(prevPoint.Y - currPoint.Y, prevPoint.X - currPoint.X);
                    if (angleIncoming < 0)
                    {
                        angleIncoming += 2 * Math.PI;
                    }

                    double maxAngle = double.MinValue;
                    int candidateVertex = -1;

                    for (int i = 0; i < outPaths[currVertex].Count; i++)
                    {
                        double angleI = Math.Atan2(vertices[outPaths[currVertex][i]].Y - currPoint.Y, vertices[outPaths[currVertex][i]].X - currPoint.X);
                        if (angleI < 0)
                        {
                            angleI += 2 * Math.PI;
                        }
                        angleI -= angleIncoming;
                        if (angleI < 0)
                        {
                            angleI += 2 * Math.PI;
                        }

                        if (angleI > maxAngle)
                        {
                            candidateVertex = outPaths[currVertex][i];
                            maxAngle = angleI;
                        }
                    }

                    outPaths[currVertex].Remove(candidateVertex);
                    outPaths[candidateVertex].Remove(currVertex);
                    polygon.Add((currVertex, candidateVertex));

                    prevVertex = currVertex;
                    currVertex = candidateVertex;
                }

                polygons.Add(polygon);
                activeVertices = (from el in Enumerable.Range(0, outPaths.Length) where outPaths[el] != null && outPaths[el].Count > 0 select el).ToList();

                if (activeVertices.Contains(currVertex))
                {
                    int currNext = newNexts[currVertex];
                    while (!outPaths[currVertex].Contains(currNext))
                    {
                        currNext = newNexts[currNext];
                    }
                    newNexts[currVertex] = currNext;
                }

                if (activeVertices.Contains(prevVertex))
                {
                    int currNext = newNexts[prevVertex];
                    while (!outPaths[prevVertex].Contains(currNext))
                    {
                        currNext = newNexts[currNext];
                    }
                    newNexts[prevVertex] = currNext;
                }
            }

            return polygons;
        }

        private static IEnumerable<GraphicsPath> TriangulateMonotone(List<Point> vertices, List<(int, int)> edges, int[] directions, int targetSign)
        {
            int getDirection((int, int) edge)
            {
                if (vertices[edge.Item2].Y != vertices[edge.Item1].Y)
                {
                    return Math.Sign(vertices[edge.Item2].Y - vertices[edge.Item1].Y);
                }
                else
                {
                    for (int i = 0; i < edges.Count; i++)
                    {
                        if (edges[i].Item2 == edge.Item1)
                        {
                            return getDirection(edges[i]);
                        }
                    }
                }

                throw new InvalidOperationException("Unknown edge direction!");
            }


            for (int i = 0; i < edges.Count; i++)
            {
                directions[edges[i].Item1] = getDirection(edges[i]);
            }

            int[] sortedVertices = (from el in edges select el.Item1).OrderBy(a => a, Comparer<int>.Create((a, b) =>
            {
                if (vertices[a].Y != vertices[b].Y)
                {
                    return Math.Sign(vertices[a].Y - vertices[b].Y);
                }
                else
                {
                    return Math.Sign(vertices[a].X - vertices[b].X);
                }
            })).ToArray();

            List<(int, int)> diagonals = new List<(int, int)>();

            Stack<int> stack = new Stack<int>();

            stack.Push(sortedVertices[0]);
            stack.Push(sortedVertices[1]);

            for (int i = 2; i < sortedVertices.Length - 1; i++)
            {
                int onTop = stack.Peek();

                if (directions[sortedVertices[i]] != directions[onTop])
                {
                    while (stack.Count > 1)
                    {
                        int v = stack.Pop();

                        diagonals.Add((v, sortedVertices[i]));
                    }

                    stack.Pop();

                    stack.Push(sortedVertices[i - 1]);
                    stack.Push(sortedVertices[i]);
                }
                else
                {
                    int lastPopped = stack.Pop();

                    bool shouldContinue = true;

                    while (shouldContinue && stack.Count > 0)
                    {
                        int currVert = stack.Peek();

                        double areaSign = Math.Sign((vertices[currVert].X - vertices[lastPopped].X) * (vertices[sortedVertices[i]].Y - vertices[lastPopped].Y) - (vertices[currVert].Y - vertices[lastPopped].Y) * (vertices[sortedVertices[i]].X - vertices[lastPopped].X));

                        double dirSign = Math.Sign(vertices[currVert].X - vertices[lastPopped].X);

                        if (areaSign * dirSign > 0)
                        {
                            lastPopped = stack.Pop();

                            diagonals.Add((currVert, sortedVertices[i]));
                        }
                        else
                        {
                            shouldContinue = false;
                        }
                    }

                    stack.Push(lastPopped);
                    stack.Push(sortedVertices[i]);
                }
            }

            stack.Pop();

            while (stack.Count > 1)
            {
                int v = stack.Pop();

                diagonals.Add((v, sortedVertices[sortedVertices.Length - 1]));
            }

            List<int>[] connections = new List<int>[vertices.Count];

            for (int i = 0; i < edges.Count; i++)
            {
                connections[edges[i].Item1] = new List<int>();
            }

            for (int i = 0; i < edges.Count; i++)
            {
                connections[edges[i].Item1].Add(edges[i].Item2);
                connections[edges[i].Item2].Add(edges[i].Item1);
            }

            for (int i = 0; i < diagonals.Count; i++)
            {
                connections[diagonals[i].Item1].Add(diagonals[i].Item2);
                connections[diagonals[i].Item1].Add(diagonals[i].Item2);

                connections[diagonals[i].Item2].Add(diagonals[i].Item1);
                connections[diagonals[i].Item2].Add(diagonals[i].Item1);
            }

            int totalTriangles = (edges.Count + diagonals.Count * 2) / 3;

            List<List<(int, int)>> polygons = new List<List<(int, int)>>();

            while (polygons.Count < totalTriangles)
            {
                int p1 = -1;
                int p2 = -1;

                for (int i = 0; i < connections.Length; i++)
                {
                    if (connections[i] != null && connections[i].Count > 0)
                    {
                        p1 = i;
                        p2 = connections[i][0];

                        connections[i].Remove(p2);
                        connections[p2].Remove(i);
                        break;
                    }
                }

                int p3 = -1;

                for (int i = 0; i < connections[p1].Count; i++)
                {
                    if (connections[connections[p1][i]].Contains(p2))
                    {
                        p3 = connections[p1][i];
                        connections[p1].Remove(p3);
                        connections[p2].Remove(p3);
                        connections[p3].Remove(p1);
                        connections[p3].Remove(p2);
                        break;
                    }
                }

                int sign = Math.Sign((vertices[p1].X - vertices[p2].X) * (vertices[p3].Y - vertices[p2].Y) - (vertices[p1].Y - vertices[p2].Y) * (vertices[p3].X - vertices[p2].X));

                if (sign == targetSign)
                {
                    polygons.Add(new List<(int, int)>() { (p1, p2), (p2, p3), (p3, p1) });
                }
                else
                {
                    polygons.Add(new List<(int, int)>() { (p1, p3), (p3, p2), (p2, p1) });
                }
            }

            foreach (List<(int, int)> polygon in polygons)
            {
                GraphicsPath polygonPath = new GraphicsPath();
                foreach ((int, int) edge in polygon)
                {
                    if (polygonPath.Segments.Count == 0)
                    {
                        polygonPath.MoveTo(vertices[edge.Item1]);
                    }

                    polygonPath.LineTo(vertices[edge.Item2]);
                }

                yield return polygonPath;
            }
        }

        /// <summary>
        /// Transforms all of the <see cref="Point"/>s in the <see cref="GraphicsPath"/> with an arbitrary transformation function.
        /// </summary>
        /// <param name="transformationFunction">An arbitrary transformation function.</param>
        /// <returns>A new <see cref="GraphicsPath"/> in which all points have been replaced using the <paramref name="transformationFunction"/>.</returns>
        public GraphicsPath Transform(Func<Point, Point> transformationFunction)
        {
            GraphicsPath tbr = new GraphicsPath();

            foreach (Segment seg in this.Segments)
            {
                tbr.Segments.AddRange(seg.Transform(transformationFunction));
            }

            return tbr;
        }


        private Rectangle cachedBounds = Rectangle.NaN;

        /// <summary>
        /// Compute the rectangular bounds of the path.
        /// </summary>
        /// <returns>The smallest <see cref="Rectangle"/> that contains the path.</returns>
        public Rectangle GetBounds()
        {
            if (double.IsNaN(cachedBounds.Location.X) || double.IsNaN(cachedBounds.Location.Y) || double.IsNaN(cachedBounds.Size.Width) || double.IsNaN(cachedBounds.Size.Height))
            {
                Point min = new Point(double.MaxValue, double.MaxValue);
                Point max = new Point(double.MinValue, double.MinValue);

                Point currPoint = new Point();
                Point figureStartPoint = new Point();

                for (int i = 0; i < this.Segments.Count; i++)
                {
                    switch (this.Segments[i].Type)
                    {
                        case SegmentType.Move:
                            currPoint = this.Segments[i].Point;
                            figureStartPoint = this.Segments[i].Point;
                            min = Point.Min(min, this.Segments[i].Point);
                            max = Point.Max(max, this.Segments[i].Point);
                            break;
                        case SegmentType.Line:
                            if (i > 0)
                            {
                                currPoint = this.Segments[i].Point;
                            }
                            else
                            {
                                currPoint = this.Segments[i].Point;
                                figureStartPoint = this.Segments[i].Point;
                            }
                            min = Point.Min(min, this.Segments[i].Point);
                            max = Point.Max(max, this.Segments[i].Point);
                            break;
                        case SegmentType.Arc:
                            ArcSegment seg = (ArcSegment)this.Segments[i];

                            if (i == 0)
                            {
                                figureStartPoint = new Point(seg.Points[0].X + Math.Cos(seg.StartAngle) * seg.Radius, seg.Points[0].Y + Math.Sin(seg.StartAngle) * seg.Radius);
                                currPoint = figureStartPoint;

                                min = Point.Min(min, currPoint);
                                max = Point.Max(max, currPoint);
                            }

                            double theta1 = seg.StartAngle;
                            double theta2 = seg.EndAngle;

                            if (theta1 > theta2)
                            {
                                (theta2, theta1) = (theta1, theta2);
                            }

                            while (theta1 < 0)
                            {
                                theta1 += 2 * Math.PI;
                                theta2 += 2 * Math.PI;
                            }

                            theta1 /= 2 * Math.PI;
                            theta2 /= 2 * Math.PI;

                            int minAngle = (int)Math.Min(0, Math.Floor(theta1)) * 4;
                            int maxAngle = (int)Math.Max(1, Math.Ceiling(theta2)) * 4;

                            theta1 *= 4;
                            theta2 *= 4;

                            bool right = false;
                            bool bottom = false;
                            bool left = false;
                            bool top = false;

                            for (int j = minAngle; j < maxAngle; j++)
                            {
                                if (theta1 <= j && theta2 >= j)
                                {
                                    switch (j % 4)
                                    {
                                        case 0:
                                            right = true;
                                            break;
                                        case 1:
                                            bottom = true;
                                            break;
                                        case 2:
                                            left = true;
                                            break;
                                        case 3:
                                            top = true;
                                            break;
                                    }
                                }
                            }

                            if (right)
                            {
                                Point p = new Point(seg.Points[0].X + seg.Radius, seg.Points[0].Y);
                                min = Point.Min(min, p);
                                max = Point.Max(max, p);
                            }

                            if (bottom)
                            {
                                Point p = new Point(seg.Points[0].X, seg.Points[0].Y + seg.Radius);
                                min = Point.Min(min, p);
                                max = Point.Max(max, p);
                            }

                            if (left)
                            {
                                Point p = new Point(seg.Points[0].X - seg.Radius, seg.Points[0].Y);
                                min = Point.Min(min, p);
                                max = Point.Max(max, p);
                            }

                            if (top)
                            {
                                Point p = new Point(seg.Points[0].X, seg.Points[0].Y - seg.Radius);
                                min = Point.Min(min, p);
                                max = Point.Max(max, p);
                            }

                            min = Point.Min(min, this.Segments[i].Point);
                            max = Point.Max(max, this.Segments[i].Point);

                            currPoint = this.Segments[i].Point;
                            break;
                        case SegmentType.Close:
                            currPoint = figureStartPoint;
                            break;
                        case SegmentType.CubicBezier:
                            if (i == 0)
                            {
                                currPoint = this.Segments[i].Points[0];
                                figureStartPoint = this.Segments[i].Points[0];
                            }

                            min = Point.Min(min, this.Segments[i].Point);
                            max = Point.Max(max, this.Segments[i].Point);

                            {
                                double aX = -currPoint.X + 3 * this.Segments[i].Points[0].X - 3 * this.Segments[i].Points[1].X + this.Segments[i].Point.X;
                                double bX = 2 * (currPoint.X - 2 * this.Segments[i].Points[0].X + this.Segments[i].Points[1].X);
                                double cX = this.Segments[i].Points[0].X - currPoint.X;

                                double t1X;
                                double t2X;

                                if (Math.Abs(aX) < 1e-5)
                                {
                                    if (Math.Abs(bX) >= 1e-5)
                                    {
                                        t1X = -cX / bX;
                                        t2X = t1X;
                                    }
                                    else
                                    {
                                        t1X = double.NaN;
                                        t2X = double.NaN;
                                    }
                                }
                                else
                                {
                                    double delta = bX * bX - 4 * aX * cX;

                                    if (delta >= -1e-5)
                                    {
                                        delta = Math.Max(delta, 0);

                                        delta = Math.Sqrt(delta);
                                        t1X = (-bX + delta) / (2 * aX);
                                        t2X = (-bX - delta) / (2 * aX);
                                    }
                                    else
                                    {
                                        t1X = double.NaN;
                                        t2X = double.NaN;
                                    }
                                }

                                if (t1X >= 0 && t1X <= 1)
                                {
                                    Point p1 = ((CubicBezierSegment)this.Segments[i]).GetBezierPointAt(currPoint, t1X);
                                    min = Point.Min(min, p1);
                                    max = Point.Max(max, p1);
                                }

                                if (t2X >= 0 && t2X <= 1)
                                {
                                    Point p2 = ((CubicBezierSegment)this.Segments[i]).GetBezierPointAt(currPoint, t2X);
                                    min = Point.Min(min, p2);
                                    max = Point.Max(max, p2);
                                }
                            }

                            {
                                double aY = -currPoint.Y + 3 * this.Segments[i].Points[0].Y - 3 * this.Segments[i].Points[1].Y + this.Segments[i].Point.Y;
                                double bY = 2 * (currPoint.Y - 2 * this.Segments[i].Points[0].Y + this.Segments[i].Points[1].Y);
                                double cY = this.Segments[i].Points[0].Y - currPoint.Y;

                                double t1Y;
                                double t2Y;

                                if (Math.Abs(aY) < 1e-5)
                                {
                                    if (Math.Abs(bY) >= 1e-5)
                                    {
                                        t1Y = -cY / bY;
                                        t2Y = t1Y;
                                    }
                                    else
                                    {
                                        t1Y = double.NaN;
                                        t2Y = double.NaN;
                                    }
                                }
                                else
                                {
                                    double delta = bY * bY - 4 * aY * cY;

                                    if (delta >= -1e-5)
                                    {
                                        delta = Math.Max(delta, 0);

                                        delta = Math.Sqrt(delta);
                                        t1Y = (-bY + delta) / (2 * aY);
                                        t2Y = (-bY - delta) / (2 * aY);
                                    }
                                    else
                                    {
                                        t1Y = double.NaN;
                                        t2Y = double.NaN;
                                    }
                                }

                                if (t1Y >= 0 && t1Y <= 1)
                                {
                                    Point p1 = ((CubicBezierSegment)this.Segments[i]).GetBezierPointAt(currPoint, t1Y);
                                    min = Point.Min(min, p1);
                                    max = Point.Max(max, p1);
                                }

                                if (t2Y >= 0 && t2Y <= 1)
                                {
                                    Point p2 = ((CubicBezierSegment)this.Segments[i]).GetBezierPointAt(currPoint, t2Y);
                                    min = Point.Min(min, p2);
                                    max = Point.Max(max, p2);
                                }
                            }

                            currPoint = this.Segments[i].Point;
                            break;
                    }
                }

                cachedBounds = new Rectangle(min, max);
            }

            return cachedBounds;
        }

        internal GraphicsPath ConvertArcsToBeziers()
        {
            GraphicsPath tbr = new GraphicsPath();

            foreach (Segment seg in this.Segments)
            {
                if (seg is ArcSegment arc)
                {
                    tbr.Segments.AddRange(arc.ToBezierSegments());
                }
                else
                {
                    tbr.Segments.Add(seg);
                }
            }

            return tbr;
        }

        private static Point IntersectLines(Point p1, Point d1, Point p3, Point d3)
        {
            Point p2 = p1 + d1;
            Point p4 = p3 + d3;
            double denom = d1.X * d3.Y - d1.Y * d3.X;

            double x = (-(p1.X * p2.Y - p1.Y * p2.X) * d3.X + d1.X * (p3.X * p4.Y - p3.Y * p4.X)) / denom;
            double y = (-(p1.X * p2.Y - p1.Y * p2.X) * d3.Y + d1.Y * (p3.X * p4.Y - p3.Y * p4.X)) / denom;

            return new Point(x, y);
        }

        /// <summary>
        /// Reverses the <see cref="GraphicsPath"/>.
        /// </summary>
        /// <returns>The reversed <see cref="GraphicsPath"/>.</returns>
        public GraphicsPath Reverse()
        {
            GraphicsPath tbr = new GraphicsPath();

            foreach (GraphicsPath figure in this.GetFigures())
            {
                Point currPoint = new Point();
                Point startPoint = new Point();
                bool started = false;

                List<(int type, Point p1, Point p2, Point p3, Point p4)> invertedSegments = new List<(int type, Point p1, Point p2, Point p3, Point p4)>();

                for (int i = 0; i < figure.Segments.Count; i++)
                {
                    if (figure.Segments[i].Type == SegmentType.Move)
                    {
                        currPoint = figure.Segments[i].Point;
                        startPoint = currPoint;
                        started = true;
                    }
                    else if (figure.Segments[i].Type == SegmentType.Line)
                    {
                        invertedSegments.Add((0, figure.Segments[i].Point, currPoint, new Point(), new Point()));
                        currPoint = figure.Segments[i].Point;

                        if (!started)
                        {
                            started = true;
                            startPoint = currPoint;
                        }
                    }
                    else if (figure.Segments[i].Type == SegmentType.Arc && figure.Segments[i] is ArcSegment arc)
                    {
                        invertedSegments.Add((1, figure.Segments[i].Points[0], new Point(arc.Radius, 0), new Point(arc.EndAngle, arc.StartAngle), new Point()));
                        currPoint = figure.Segments[i].GetPointAt(currPoint, 1);

                        if (!started)
                        {
                            started = true;
                            startPoint = figure.Segments[i].GetPointAt(currPoint, 0);
                        }
                    }
                    else if (figure.Segments[i].Type == SegmentType.CubicBezier)
                    {
                        invertedSegments.Add((2, figure.Segments[i].Points[2], figure.Segments[i].Points[1], figure.Segments[i].Points[0], currPoint));
                        currPoint = figure.Segments[i].GetPointAt(currPoint, 1);

                        if (!started)
                        {
                            started = true;
                            startPoint = figure.Segments[i].GetPointAt(currPoint, 0);
                        }
                    }
                    else if (figure.Segments[i].Type == SegmentType.Close)
                    {
                        invertedSegments.Add((3, startPoint, currPoint, new Point(), new Point()));
                    }
                }

                bool isClosed = false;

                for (int i = invertedSegments.Count - 1; i >= 0; i--)
                {
                    if (invertedSegments[i].type == 3)
                    {
                        tbr.MoveTo(invertedSegments[i].p1);
                        tbr.LineTo(invertedSegments[i].p2);
                        isClosed = true;
                    }
                    else if (invertedSegments[i].type == 0)
                    {
                        if (i == invertedSegments.Count - 1)
                        {
                            tbr.MoveTo(invertedSegments[i].p1);
                        }

                        tbr.LineTo(invertedSegments[i].p2);
                    }
                    else if (invertedSegments[i].type == 1)
                    {
                        tbr.Arc(invertedSegments[i].p1, invertedSegments[i].p2.X, invertedSegments[i].p3.X, invertedSegments[i].p3.Y);
                    }
                    else if (invertedSegments[i].type == 2)
                    {
                        if (i == invertedSegments.Count - 1)
                        {
                            tbr.MoveTo(invertedSegments[i].p1);
                        }

                        tbr.CubicBezierTo(invertedSegments[i].p2, invertedSegments[i].p3, invertedSegments[i].p4);
                    }
                }

                if (isClosed)
                {
                    tbr.Close();
                }
            }

            return tbr;
        }

        /// <summary>
        /// Returns a <see cref="GraphicsPath"/> representing the stroke of the current <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="lineWidth">The thickness of the stroke.</param>
        /// <param name="lineCap">The line cap used in the stroke.</param>
        /// <param name="lineJoin">The line join used in the stroke.</param>
        /// <returns>A <see cref="GraphicsPath"/> representing the stroke of the current <see cref="GraphicsPath"/>.</returns>
        public GraphicsPath GetStroke(double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter)
        {
            double flatness = lineWidth * 0.0001;

            GraphicsPath tbr = new GraphicsPath();

            foreach (GraphicsPath figure in this.GetFigures())
            {
                GraphicsPath half1 = new GraphicsPath();

                ProcessHalfStroke(figure, half1, lineWidth, lineJoin, flatness, true);

                tbr.AddPath(half1);

                GraphicsPath half2 = new GraphicsPath();
                ProcessHalfStroke(figure, half2, lineWidth, lineJoin, flatness, false);

                half2 = half2.Reverse();

                if (figure.Segments[figure.Segments.Count - 1].Type != SegmentType.Close && half1.Segments.Count > 0 && half2.Segments.Count > 0)
                {
                    if (lineCap == LineCaps.Butt)
                    {
                        if (half2.Segments[0].Type == SegmentType.Move)
                        {
                            half2.Segments[0] = new LineSegment(half2.Segments[0].Point);
                        }

                        tbr.AddPath(half2);
                        tbr.Close();
                    }
                    else if (lineCap == LineCaps.Square)
                    {
                        if (half2.Segments[0].Type == SegmentType.Move)
                        {
                            half2.Segments[0] = new LineSegment(half2.Segments[0].Point);
                        }

                        Point cap1 = tbr.GetPointAtRelative(1);
                        Point cap2 = half2.GetPointAtRelative(0);

                        Point tangent = this.GetTangentAtRelative(1);

                        tbr.LineTo(cap1 + tangent * lineWidth * 0.5);
                        tbr.LineTo(cap2 + tangent * lineWidth * 0.5);

                        tbr.AddPath(half2);

                        Point cap3 = tbr.GetPointAtRelative(0);
                        Point cap4 = half2.GetPointAtRelative(1);

                        Point tangent2 = this.GetTangentAtRelative(0) * -1;

                        tbr.LineTo(cap4 + tangent2 * lineWidth * 0.5);
                        tbr.LineTo(cap3 + tangent2 * lineWidth * 0.5);

                        tbr.Close();
                    }
                    else if (lineCap == LineCaps.Round)
                    {
                        if (half2.Segments[0].Type == SegmentType.Move)
                        {
                            half2.Segments[0] = new LineSegment(half2.Segments[0].Point);
                        }

                        Point cap1 = tbr.GetPointAtRelative(1);
                        Point cap2 = half2.GetPointAtRelative(0);

                        Point center = this.GetPointAtRelative(1);

                        double ang1 = (Math.Atan2(cap1.Y - center.Y, cap1.X - center.X) + 2 * Math.PI) % (2 * Math.PI);
                        double ang2 = (Math.Atan2(cap2.Y - center.Y, cap2.X - center.X) + 2 * Math.PI) % (2 * Math.PI);

                        tbr.Arc(center, lineWidth * 0.5, ang1, ang2);
                        

                        tbr.AddPath(half2);

                        Point cap3 = tbr.GetPointAtRelative(0);
                        Point cap4 = half2.GetPointAtRelative(1);

                        Point center2 = this.GetPointAtRelative(0);

                        double ang3 = (Math.Atan2(cap4.Y - center2.Y, cap4.X - center2.X) + 2 * Math.PI) % (2 * Math.PI);
                        double ang4 = (Math.Atan2(cap3.Y - center2.Y, cap3.X - center2.X) + 2 * Math.PI) % (2 * Math.PI);

                        tbr.Arc(center2, lineWidth * 0.5, ang3, ang4);

                        tbr.Close();
                    }
                }
                else
                {
                    tbr.AddPath(half2);
                }
            }

            return RemoveDuplicates(tbr);
        }

        // Adapted from https://www.particleincell.com/2013/cubic-line-intersection/
        private static double[] CubicRoots(double a, double b, double c, double d)
        {
            double A = b / a;
            double B = c / a;
            double C = d / a;

            double Q = (3 * B - A * A) / 9;
            double R = (9 * A * B - 27 * C - 2 * A * A * A) / 54;
            double D = Q * Q * Q + R * R; // polynomial discriminant

            if (Math.Abs(D) < Tolerance)
            {
                D = 0;
            }

            double[] t = new double[3];

            if (D >= 0)                                 // complex or duplicate roots
            {
                var S = Math.Sign(R + Math.Sqrt(D)) * Math.Pow(Math.Abs(R + Math.Sqrt(D)), (1.0 / 3));
                var T = Math.Sign(R - Math.Sqrt(D)) * Math.Pow(Math.Abs(R - Math.Sqrt(D)), (1.0 / 3));

                t[0] = -A / 3 + (S + T);                    // real root
                t[1] = -A / 3 - (S + T) / 2;                  // real part of complex root
                t[2] = -A / 3 - (S + T) / 2;                  // real part of complex root
                double Im = Math.Abs(Math.Sqrt(3) * (S - T) / 2);    // complex part of root pair   

                /*discard complex roots*/
                if (Im > Tolerance)
                {
                    t[1] = -1;
                    t[2] = -1;
                }

            }
            else                                          // distinct real roots
            {
                var th = Math.Acos(R / Math.Sqrt(-Math.Pow(Q, 3)));

                t[0] = 2 * Math.Sqrt(-Q) * Math.Cos(th / 3) - A / 3;
                t[1] = 2 * Math.Sqrt(-Q) * Math.Cos((th + 2 * Math.PI) / 3) - A / 3;
                t[2] = 2 * Math.Sqrt(-Q) * Math.Cos((th + 4 * Math.PI) / 3) - A / 3;
            }

            return new HashSet<double>(t.Select(x => Math.Abs(x) <= Tolerance ? 0 : Math.Abs(x - 1) <= Tolerance ? 1 : x).Where(x => x >= 0 && x <= 1)).ToArray();
        }

        /// <summary>
        /// Determines whether the specified <paramref name="point"/> falls within the current <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> being analysed.</param>
        /// <param name="fillRule">The rule to use to determine whether a point is inside or outside of a the <see cref="GraphicsPath"/>.</param>
        /// <returns><see langword="true"/> if the <paramref name="point"/> is inside the <see cref="GraphicsPath"/>, or <see langword="false"/> if it is outside. If the point lies exactly on the <see cref="GraphicsPath"/> (or very close to it), the result may be either <see langword="true"/> or <see langword="false"/>, depending on numerical approximations.</returns>
        public bool ContainsPoint(Point point, FillRule fillRule)
        {
            return ContainsPointInternal(point, fillRule == FillRule.EvenOdd) > 0;
        }

        private GraphicsPath MonotoniseOnY()
        {
            GraphicsPath tbr = new GraphicsPath();
            Point startPoint = new Point();
            Point currPoint = new Point();


            foreach (Segment seg in this.Segments)
            {
                switch (seg.Type)
                {
                    case SegmentType.Move:
                        tbr.Segments.Add(seg);
                        currPoint = seg.Point;
                        startPoint = seg.Point;
                        break;

                    case SegmentType.Line:
                        tbr.Segments.Add(seg);
                        currPoint = seg.Point;
                        break;

                    case SegmentType.Close:
                        tbr.Segments.Add(seg);
                        currPoint = startPoint;
                        break;

                    case SegmentType.Arc:
                        foreach (Segment seg2 in ((ArcSegment)seg).ToBezierSegments())
                        {
                            if (seg2 is CubicBezierSegment cub)
                            {
                                tbr.Segments.AddRange(cub.MonotoniseOnY(currPoint));
                                currPoint = tbr.Segments[tbr.Segments.Count - 1].Point;
                            }
                            else
                            {
                                tbr.Segments.Add(seg2);
                                currPoint = tbr.Segments[tbr.Segments.Count - 1].Point;
                            }
                        }
                        break;
                    case SegmentType.CubicBezier:
                        tbr.Segments.AddRange(((CubicBezierSegment)seg).MonotoniseOnY(currPoint));
                        currPoint = tbr.Segments[tbr.Segments.Count - 1].Point;
                        break;
                }
            }

            return tbr;
        }

        private GraphicsPath Monotonise(double flatness)
        {
            GraphicsPath tbr = new GraphicsPath();
            Point startPoint = new Point();
            Point currPoint = new Point();

            foreach (Segment seg in this.Segments)
            {
                switch (seg.Type)
                {
                    case SegmentType.Move:
                        tbr.Segments.Add(seg);
                        currPoint = seg.Point;
                        startPoint = seg.Point;
                        break;

                    case SegmentType.Line:
                        tbr.Segments.Add(seg);
                        currPoint = seg.Point;
                        break;

                    case SegmentType.Close:
                        tbr.Segments.Add(seg);
                        currPoint = startPoint;
                        break;

                    case SegmentType.Arc:
                        foreach (Segment seg2 in ((ArcSegment)seg).ToBezierSegments())
                        {
                            if (seg2 is CubicBezierSegment cub)
                            {
                                tbr.Segments.AddRange(cub.BreakAtInflectionPoints(currPoint, flatness));
                                currPoint = tbr.Segments[tbr.Segments.Count - 1].Point;
                            }
                            else
                            {
                                tbr.Segments.Add(seg2);
                                currPoint = tbr.Segments[tbr.Segments.Count - 1].Point;
                            }
                        }
                        break;
                    case SegmentType.CubicBezier:
                        tbr.Segments.AddRange(((CubicBezierSegment)seg).BreakAtInflectionPoints(currPoint, flatness));
                        currPoint = tbr.Segments[tbr.Segments.Count - 1].Point;
                        break;
                }
            }

            return tbr;
        }

        private int ContainsPointInternal(Point point, bool evenOdd)
        {
            int countsAbs = 0;
            int counts = 0;

            foreach (GraphicsPath figure in this.MonotoniseOnY().GetFigures())
            {
                if (figure.Segments[figure.Segments.Count - 1].Type == SegmentType.Close)
                {
                    if (figure.Segments[0].Type != SegmentType.Move)
                    {
                        throw new InvalidOperationException("Assertion failed: the figure does not start with a move segment!");
                    }

                    (int, int)[] pointDirections = new (int, int)[figure.Segments.Count - 1];

                    Point startPoint = new Point();
                    Point currPoint = new Point();
                    int index = 0;

                    for (int i = 0; i < figure.Segments.Count; i++)
                    {
                        if (figure.Segments[i].Type == SegmentType.Move)
                        {
                            currPoint = figure.Segments[i].Point;
                            startPoint = currPoint;
                        }
                        else if (figure.Segments[i].Type == SegmentType.Line)
                        {
                            int direction = Math.Sign(figure.Segments[i].Point.Y - currPoint.Y);

                            pointDirections[index] = (pointDirections[index].Item1, direction);
                            pointDirections[index + 1] = (direction, pointDirections[index + 1].Item2);

                            currPoint = figure.Segments[i].Point;
                            index++;
                        }
                        else if (figure.Segments[i].Type == SegmentType.Arc)
                        {
                            throw new InvalidOperationException("Path not properly monotonised!");
                        }
                        else if (figure.Segments[i].Type == SegmentType.CubicBezier)
                        {
                            int direction = Math.Sign(figure.Segments[i].Point.Y - currPoint.Y);

                            pointDirections[index] = (pointDirections[index].Item1, direction);
                            pointDirections[index + 1] = (direction, pointDirections[index + 1].Item2);

                            currPoint = figure.Segments[i].Point;
                            index++;
                        }
                        else if (figure.Segments[i].Type == SegmentType.Close)
                        {
                            int direction = Math.Sign(startPoint.Y - currPoint.Y);

                            pointDirections[index] = (pointDirections[index].Item1, direction);
                            pointDirections[0] = (direction, pointDirections[0].Item2);
                        }
                    }

                    int[] directions = new int[pointDirections.Length * 2];

                    for (int i = 0; i < pointDirections.Length; i++)
                    {
                        directions[2 * i] = pointDirections[i].Item1;
                        directions[2 * i + 1] = pointDirections[i].Item2;
                    }

                    for (int i = 0; i < directions.Length; i++)
                    {
                        if (directions[i] == 0)
                        {
                            int nextDir = 0;

                            int j = 0;
                            while (directions[(i + j) % directions.Length] == 0)
                            {
                                j++;
                            }
                            nextDir = directions[(i + j) % directions.Length];

                            int prevDir = 0;

                            j = 0;
                            while (directions[(i - j) >= 0 ? (i - j) : (i - j + directions.Length)] == 0)
                            {
                                j++;
                            }
                            prevDir = directions[(i - j) >= 0 ? (i - j) : (i - j + directions.Length)];

                            if (prevDir == nextDir)
                            {
                                directions[i] = prevDir;
                            }
                        }
                    }

                    for (int i = 0; i < pointDirections.Length; i++)
                    {
                        pointDirections[i] = (directions[2 * i], directions[2 * i + 1]);
                    }

                    startPoint = new Point();
                    currPoint = new Point();
                    index = 0;

                    for (int i = 0; i < figure.Segments.Count; i++)
                    {
                        if (figure.Segments[i].Type == SegmentType.Move)
                        {
                            currPoint = figure.Segments[i].Point;
                            startPoint = currPoint;
                        }
                        else if (figure.Segments[i].Type == SegmentType.Line)
                        {
                            if (currPoint.Y != figure.Segments[i].Point.Y)
                            {
                                double t = (point.Y - currPoint.Y) / (figure.Segments[i].Point.Y - currPoint.Y);

                                if (t >= 0 && t <= 1)
                                {
                                    double x = currPoint.X + (figure.Segments[i].Point.X - currPoint.X) * t;

                                    if (x >= point.X)
                                    {
                                        if (t == 0)
                                        {
                                            if (!((pointDirections[index].Item1 == -1 && pointDirections[index].Item2 == -1) || (pointDirections[index].Item1 == 0 && pointDirections[index].Item2 == 0)))
                                            {
                                                countsAbs++;
                                                counts += pointDirections[index].Item2;
                                            }
                                        }
                                        else if (t == 1)
                                        {
                                            if (!((pointDirections[index + 1].Item1 == 1 && pointDirections[index + 1].Item2 == 1) || (pointDirections[index + 1].Item1 == 0 && pointDirections[index + 1].Item2 == 0)))
                                            {
                                                countsAbs++;
                                                counts += pointDirections[index + 1].Item1;
                                            }
                                        }
                                        else
                                        {
                                            countsAbs++;
                                            counts += Math.Sign(figure.Segments[i].Point.Y - currPoint.Y);
                                        }
                                    }
                                }
                            }
                            /* else if (currPoint.Y == point.Y)
                             {
                                 if (currPoint.X >= point.X)
                                 {
                                     if (!((pointDirections[index].Item1 == -1 && pointDirections[index].Item2 == -1) || (pointDirections[index].Item1 == 0 && pointDirections[index].Item2 == 0)))
                                     {
                                         countsAbs++;
                                         counts += pointDirections[index].Item2;
                                     }
                                 }

                                 if (figure.Segments[i].Point.X >= point.X)
                                 {
                                     if (!((pointDirections[index + 1].Item1 == 1 && pointDirections[index + 1].Item2 == 1) || (pointDirections[index + 1].Item1 == 0 && pointDirections[index + 1].Item2 == 0)))
                                     {
                                         countsAbs++;
                                         counts += pointDirections[index + 1].Item1;
                                     }
                                 }
                             }*/

                            currPoint = figure.Segments[i].Point;
                            index++;
                        }
                        else if (figure.Segments[i].Type == SegmentType.CubicBezier)
                        {
                            double a = -currPoint.Y + 3 * figure.Segments[i].Points[0].Y - 3 * figure.Segments[i].Points[1].Y + figure.Segments[i].Points[2].Y;
                            double b = 3 * currPoint.Y - 6 * figure.Segments[i].Points[0].Y + 3 * figure.Segments[i].Points[1].Y;
                            double c = -3 * currPoint.Y + 3 * figure.Segments[i].Points[0].Y;


                            double d = currPoint.Y - point.Y;

                            double[] roots = CubicRoots(a, b, c, d);

                            if (roots.Length == 1)
                            {
                                double t = roots[0];
                                double x = (1 - t) * (1 - t) * (1 - t) * currPoint.X + figure.Segments[i].Points[0].X * t * (1 - t) * (1 - t) * 3 + figure.Segments[i].Points[1].X * t * t * (1 - t) * 3 + figure.Segments[i].Points[2].X * t * t * t;

                                if (x >= point.X)
                                {
                                    if (t == 0)
                                    {
                                        if (!((pointDirections[index].Item1 == -1 && pointDirections[index].Item2 == -1) || (pointDirections[index].Item1 == 0 && pointDirections[index].Item2 == 0)))
                                        {
                                            countsAbs++;
                                            counts += pointDirections[index].Item2;
                                        }
                                    }
                                    else if (t == 1)
                                    {
                                        if (!((pointDirections[index + 1].Item1 == 1 && pointDirections[index + 1].Item2 == 1) || (pointDirections[index + 1].Item1 == 0 && pointDirections[index + 1].Item2 == 0)))
                                        {
                                            countsAbs++;
                                            counts += pointDirections[index + 1].Item1;
                                        }
                                    }
                                    else
                                    {
                                        countsAbs++;
                                        counts += Math.Sign(figure.Segments[i].Point.Y - currPoint.Y);
                                    }
                                }
                            }
                            else if (roots.Length != 0)
                            {
                                throw new InvalidOperationException("Path not properly monotonised!");
                            }

                            currPoint = figure.Segments[i].Point;
                            index++;
                        }
                        else if (figure.Segments[i].Type == SegmentType.Close)
                        {
                            if (currPoint.Y != startPoint.Y)
                            {
                                double t = (point.Y - currPoint.Y) / (startPoint.Y - currPoint.Y);

                                if (t >= 0 && t <= 1)
                                {
                                    double x = currPoint.X + (startPoint.X - currPoint.X) * t;

                                    if (x >= point.X)
                                    {
                                        if (t == 0)
                                        {
                                            if (!((pointDirections[index].Item1 == -1 && pointDirections[index].Item2 == -1) || (pointDirections[index].Item1 == 0 && pointDirections[index].Item2 == 0)))
                                            {
                                                countsAbs++;
                                                counts += pointDirections[index].Item2;
                                            }
                                        }
                                        else if (t == 1)
                                        {
                                            if (!((pointDirections[0].Item1 == 1 && pointDirections[0].Item2 == 1) || (pointDirections[0].Item1 == 0 && pointDirections[0].Item2 == 0)))
                                            {
                                                countsAbs++;
                                                counts += pointDirections[0].Item1;
                                            }
                                        }
                                        else
                                        {
                                            countsAbs++;
                                            counts += Math.Sign(startPoint.Y - currPoint.Y);
                                        }
                                    }
                                }
                            }
                            /* else if (currPoint.Y == point.Y)
                             {
                                 if (currPoint.X >= point.X)
                                 {
                                     if (!((pointDirections[index].Item1 == -1 && pointDirections[index].Item2 == -1) || (pointDirections[index].Item1 == 0 && pointDirections[index].Item2 == 0)))
                                     {
                                         countsAbs++;
                                         counts += pointDirections[index].Item2;
                                     }
                                 }

                                 if (startPoint.X >= point.X)
                                 {
                                     if (!((pointDirections[0].Item1 == 1 && pointDirections[0].Item2 == 1) || (pointDirections[0].Item1 == 0 && pointDirections[0].Item2 == 0)))
                                     {
                                         countsAbs++;
                                         counts += pointDirections[0].Item1;
                                     }
                                 }
                             }*/
                        }
                    }
                }
            }

            if (evenOdd)
            {
                return countsAbs % 2;
            }
            else
            {
                return counts != 0 ? 1 : 0;
            }
        }

        private static void ProcessHalfStroke(GraphicsPath figure, GraphicsPath tbr, double lineWidth, LineJoins lineJoin, double flatness, bool positive)
        {
            Point currPoint = new Point();
            Point currTangent = new Point(double.NaN, double.NaN);
            double sign = positive ? 1 : -1;

            bool started = false;
            Point startPoint = new Point();

            bool isClosed = false;

            figure = RemoveDuplicates(figure.Monotonise(flatness));

            foreach (Segment seg in figure.Segments)
            {
                if (seg.Type == SegmentType.Move)
                {
                    currPoint = seg.Point;
                    currTangent = new Point(double.NaN, double.NaN);
                    startPoint = currPoint;
                    started = true;
                }
                else if (seg.Type == SegmentType.Line)
                {
                    Point segStartPoint = currPoint;
                    Point segTangent = seg.GetTangentAt(segStartPoint, 0);
                    Point segEndPoint = seg.Point;

                    if (!double.IsNaN(currTangent.X) && !double.IsNaN(currTangent.Y) && (currTangent.X != segTangent.X && currTangent.Y != segTangent.Y) && !double.IsNaN(segTangent.X) && !double.IsNaN(segTangent.Y))
                    {
                        Point p = currPoint;
                        Point pALeft = currPoint - new Point(-currTangent.Y, currTangent.X) * lineWidth * 0.5;
                        Point pARight = currPoint + new Point(-currTangent.Y, currTangent.X) * lineWidth * 0.5;

                        Point pBLeft = currPoint - new Point(-segTangent.Y, segTangent.X) * lineWidth * 0.5;
                        Point pBRight = currPoint + new Point(-segTangent.Y, segTangent.X) * lineWidth * 0.5;

                        Point p3_1 = IntersectLines(pALeft, currTangent, pBLeft, segTangent);
                        Point p3_2 = IntersectLines(pARight, currTangent, pBRight, segTangent);

                        double crossProduct = currTangent.X * segTangent.Y - currTangent.Y * segTangent.X;

                        if (crossProduct > 0 && !positive)
                        {
                            if (lineJoin == LineJoins.Round)
                            {
                                tbr.Arc(p, lineWidth * 0.5, Math.Atan2(pALeft.Y - p.Y, pALeft.X - p.X), Math.Atan2(pBLeft.Y - p.Y, pBLeft.X - p.X));
                            }
                            else if (lineJoin == LineJoins.Miter)
                            {
                                tbr.LineTo(p3_1);
                            }
                        }
                        else if (crossProduct < 0 && positive)
                        {
                            if (lineJoin == LineJoins.Round)
                            {
                                tbr.Arc(p, lineWidth * 0.5, Math.Atan2(pARight.Y - p.Y, pARight.X - p.X), Math.Atan2(pBRight.Y - p.Y, pBRight.X - p.X));
                            }
                            else if (lineJoin == LineJoins.Miter)
                            {
                                tbr.LineTo(p3_2);
                            }
                        }
                    }

                    if (!double.IsNaN(segTangent.X) && !double.IsNaN(segTangent.Y))
                    {
                        tbr.LineTo(currPoint + sign * lineWidth * 0.5 * new Point(-segTangent.Y, segTangent.X));
                        tbr.LineTo(seg.Point + sign * lineWidth * 0.5 * new Point(-segTangent.Y, segTangent.X));
                    }

                    currPoint = seg.Point;
                    currTangent = segTangent;

                    if (!started)
                    {
                        startPoint = currPoint;
                        started = true;
                    }
                }
                else if (seg.Type == SegmentType.Arc && seg is ArcSegment arc)
                {
                    Point segStartPoint = currPoint;
                    Point segTangent = seg.GetTangentAt(segStartPoint, 0);
                    Point segEndPoint = seg.Point;

                    if (!double.IsNaN(currTangent.X) && !double.IsNaN(currTangent.Y) && (currTangent.X != segTangent.X && currTangent.Y != segTangent.Y) && !double.IsNaN(segTangent.X) && !double.IsNaN(segTangent.Y))
                    {
                        Point p = currPoint;
                        Point pALeft = currPoint - new Point(-currTangent.Y, currTangent.X) * lineWidth * 0.5;
                        Point pARight = currPoint + new Point(-currTangent.Y, currTangent.X) * lineWidth * 0.5;

                        Point pBLeft = currPoint - new Point(-segTangent.Y, segTangent.X) * lineWidth * 0.5;
                        Point pBRight = currPoint + new Point(-segTangent.Y, segTangent.X) * lineWidth * 0.5;

                        Point p3_1 = IntersectLines(pALeft, currTangent, pBLeft, segTangent);
                        Point p3_2 = IntersectLines(pARight, currTangent, pBRight, segTangent);

                        double crossProduct = currTangent.X * segTangent.Y - currTangent.Y * segTangent.X;
                        if (crossProduct > 0 && !positive)
                        {
                            if (lineJoin == LineJoins.Round)
                            {
                                tbr.Arc(p, lineWidth * 0.5, Math.Atan2(pALeft.Y - p.Y, pALeft.X - p.X), Math.Atan2(pBLeft.Y - p.Y, pBLeft.X - p.X));
                            }
                            else if (lineJoin == LineJoins.Miter)
                            {
                                tbr.LineTo(p3_1);
                            }
                        }
                        else if (crossProduct < 0 && positive)
                        {
                            if (lineJoin == LineJoins.Round)
                            {
                                tbr.Arc(p, lineWidth * 0.5, Math.Atan2(pARight.Y - p.Y, pARight.X - p.X), Math.Atan2(pBRight.Y - p.Y, pBRight.X - p.X));
                            }
                            else if (lineJoin == LineJoins.Miter)
                            {
                                tbr.LineTo(p3_2);
                            }
                        }
                    }

                    tbr.LineTo(currPoint + sign * lineWidth * 0.5 * new Point(-segTangent.Y, segTangent.X));
                    tbr.Arc(arc.Points[0], arc.Radius - sign * lineWidth * 0.5 * Math.Sign(arc.EndAngle - arc.StartAngle), arc.StartAngle, arc.EndAngle);

                    currPoint = seg.Point;
                    currTangent = seg.GetTangentAt(segStartPoint, 1);

                    if (!started)
                    {
                        startPoint = currPoint;
                        started = true;
                    }
                }
                else if (seg.Type == SegmentType.CubicBezier)
                {
                    Point segStartPoint = currPoint;
                    Point segTangent = seg.GetTangentAt(segStartPoint, 0);
                    Point segEndPoint = seg.Point;

                    if (!double.IsNaN(currTangent.X) && !double.IsNaN(currTangent.Y) && (currTangent.X != segTangent.X && currTangent.Y != segTangent.Y) && !double.IsNaN(segTangent.X) && !double.IsNaN(segTangent.Y))
                    {
                        Point p = currPoint;
                        Point pALeft = currPoint - new Point(-currTangent.Y, currTangent.X) * lineWidth * 0.5;
                        Point pARight = currPoint + new Point(-currTangent.Y, currTangent.X) * lineWidth * 0.5;

                        Point pBLeft = currPoint - new Point(-segTangent.Y, segTangent.X) * lineWidth * 0.5;
                        Point pBRight = currPoint + new Point(-segTangent.Y, segTangent.X) * lineWidth * 0.5;

                        Point p3_1 = IntersectLines(pALeft, currTangent, pBLeft, segTangent);
                        Point p3_2 = IntersectLines(pARight, currTangent, pBRight, segTangent);

                        double crossProduct = currTangent.X * segTangent.Y - currTangent.Y * segTangent.X;

                        if (crossProduct > 1e-7 && !positive)
                        {
                            if (lineJoin == LineJoins.Round)
                            {
                                tbr.Arc(p, lineWidth * 0.5, Math.Atan2(pALeft.Y - p.Y, pALeft.X - p.X), Math.Atan2(pBLeft.Y - p.Y, pBLeft.X - p.X));
                            }
                            else if (lineJoin == LineJoins.Miter)
                            {
                                tbr.LineTo(p3_1);
                            }
                        }
                        else if (crossProduct < -1e-7 && positive)
                        {
                            if (lineJoin == LineJoins.Round)
                            {
                                tbr.Arc(p, lineWidth * 0.5, Math.Atan2(pARight.Y - p.Y, pARight.X - p.X), Math.Atan2(pBRight.Y - p.Y, pBRight.X - p.X));
                            }
                            else if (lineJoin == LineJoins.Miter)
                            {
                                tbr.LineTo(p3_2);
                            }
                        }
                    }

                    if (!double.IsNaN(segTangent.X) && !double.IsNaN(segTangent.Y))
                    {
                        tbr.LineTo(currPoint + sign * lineWidth * 0.5 * new Point(-segTangent.Y, segTangent.X));
                    }
                    
                    List<(Point point, Point tangent)> newPoints = seg.FlattenForOffsetAndGetTangents(currPoint, sign * lineWidth, flatness).ToList();

                    for (int i = 0; i < newPoints.Count; i++)
                    {
                        Point pt = newPoints[i].point + sign * lineWidth * 0.5 * new Point(-newPoints[i].tangent.Y, newPoints[i].tangent.X);

                        if (!double.IsNaN(pt.X) && !double.IsNaN(pt.Y))
                        {
                            tbr.LineTo(pt);
                        }
                    }

                    if ((currPoint.X != newPoints[newPoints.Count - 1].point.X || currPoint.Y != newPoints[newPoints.Count - 1].point.Y) || double.IsNaN(currTangent.X) || double.IsNaN(currTangent.Y))
                    {
                        currPoint = newPoints[newPoints.Count - 1].point;
                        currTangent = newPoints[newPoints.Count - 1].tangent;
                    }                        

                    if (!started)
                    {
                        startPoint = currPoint;
                        started = true;
                    }
                }
                else if (seg.Type == SegmentType.Close)
                {
                    Point segStartPoint = currPoint;
                    Point segTangent = new Point(startPoint.X - currPoint.X, startPoint.Y - currPoint.Y).Normalize();
                    Point segEndPoint = startPoint;

                    if (!currPoint.IsEqual(startPoint, Tolerance))
                    {
                        if (!double.IsNaN(currTangent.X) && !double.IsNaN(currTangent.Y) && (currTangent.X != segTangent.X && currTangent.Y != segTangent.Y))
                        {
                            Point p = currPoint;
                            Point pALeft = currPoint - new Point(-currTangent.Y, currTangent.X) * lineWidth * 0.5;
                            Point pARight = currPoint + new Point(-currTangent.Y, currTangent.X) * lineWidth * 0.5;

                            Point pBLeft = currPoint - new Point(-segTangent.Y, segTangent.X) * lineWidth * 0.5;
                            Point pBRight = currPoint + new Point(-segTangent.Y, segTangent.X) * lineWidth * 0.5;

                            Point p3_1 = IntersectLines(pALeft, currTangent, pBLeft, segTangent);
                            Point p3_2 = IntersectLines(pARight, currTangent, pBRight, segTangent);

                            double crossProduct = currTangent.X * segTangent.Y - currTangent.Y * segTangent.X;

                            if (crossProduct > 0 && !positive)
                            {
                                if (lineJoin == LineJoins.Round)
                                {
                                    tbr.Arc(p, lineWidth * 0.5, Math.Atan2(pALeft.Y - p.Y, pALeft.X - p.X), Math.Atan2(pBLeft.Y - p.Y, pBLeft.X - p.X));
                                }
                                else if (lineJoin == LineJoins.Miter)
                                {
                                    tbr.LineTo(p3_1);
                                }
                            }
                            else if (crossProduct < 0 && positive)
                            {
                                if (lineJoin == LineJoins.Round)
                                {
                                    tbr.Arc(p, lineWidth * 0.5, Math.Atan2(pARight.Y - p.Y, pARight.X - p.X), Math.Atan2(pBRight.Y - p.Y, pBRight.X - p.X));
                                }
                                else if (lineJoin == LineJoins.Miter)
                                {
                                    tbr.LineTo(p3_2);
                                }
                            }
                        }


                        tbr.LineTo(currPoint + sign * lineWidth * 0.5 * new Point(-segTangent.Y, segTangent.X));

                        tbr.LineTo(startPoint + sign * lineWidth * 0.5 * new Point(-segTangent.Y, segTangent.X));

                        currTangent = segTangent;
                    }

                    currPoint = startPoint;


                    isClosed = true;

                    started = false;
                }
            }


            if (isClosed)
            {
                Point segTangent = figure.GetTangentAtRelative(0);

                if (!double.IsNaN(currTangent.X) && !double.IsNaN(currTangent.Y) && (currTangent.X != segTangent.X && currTangent.Y != segTangent.Y) && !double.IsNaN(segTangent.X) && !double.IsNaN(segTangent.Y))
                {
                    Point p = currPoint;
                    Point pALeft = currPoint - new Point(-currTangent.Y, currTangent.X) * lineWidth * 0.5;
                    Point pARight = currPoint + new Point(-currTangent.Y, currTangent.X) * lineWidth * 0.5;

                    Point pBLeft = currPoint - new Point(-segTangent.Y, segTangent.X) * lineWidth * 0.5;
                    Point pBRight = currPoint + new Point(-segTangent.Y, segTangent.X) * lineWidth * 0.5;

                    Point p3_1 = IntersectLines(pALeft, currTangent, pBLeft, segTangent);
                    Point p3_2 = IntersectLines(pARight, currTangent, pBRight, segTangent);

                    double crossProduct = currTangent.X * segTangent.Y - currTangent.Y * segTangent.X;

                    if (crossProduct > 0 && !positive)
                    {
                        if (lineJoin == LineJoins.Round)
                        {
                            double ang1 = Math.Atan2(pALeft.Y - p.Y, pALeft.X - p.X);
                            double ang2 = Math.Atan2(pBLeft.Y - p.Y, pBLeft.X - p.X);

                            if (ang1 < 0)
                            {
                                ang1 += 2 * Math.PI;
                            }

                            if (ang2 < 0)
                            {
                                ang2 += 2 * Math.PI;
                            }

                            tbr.Arc(p, lineWidth * 0.5, ang1, ang2);
                        }
                        else if (lineJoin == LineJoins.Miter)
                        {
                            tbr.LineTo(p3_1);
                        }
                    }
                    else if (crossProduct < 0 && positive)
                    {
                        if (lineJoin == LineJoins.Round)
                        {
                            double ang1 = Math.Atan2(pARight.Y - p.Y, pARight.X - p.X);
                            double ang2 = Math.Atan2(pBRight.Y - p.Y, pBRight.X - p.X);

                            if (ang1 < 0)
                            {
                                ang1 += 2 * Math.PI;
                            }

                            if (ang2 < 0)
                            {
                                ang2 += 2 * Math.PI;
                            }

                            tbr.Arc(p, lineWidth * 0.5, ang1, ang2);
                        }
                        else if (lineJoin == LineJoins.Miter)
                        {
                            tbr.LineTo(p3_2);
                        }
                    }
                }


                tbr.Close();
            }
        }

        internal static double Tolerance = 1e-7;

        private static GraphicsPath RemoveDuplicates(GraphicsPath path)
        {
            Point currentPoint = new Point();
            Point startPoint = new Point();
            bool started = false;

            GraphicsPath tbr = new GraphicsPath();

            GraphicsPath figure = null;

            for (int i = 0; i < path.Segments.Count; i++)
            {
                if (path.Segments[i].Type == SegmentType.Move)
                {
                    if (figure != null)
                    {
                        tbr.AddPath(figure);
                    }

                    figure = new GraphicsPath();

                    currentPoint = path.Segments[i].Point;
                    figure.MoveTo(currentPoint);
                    startPoint = currentPoint;
                    started = true;
                }
                else if (path.Segments[i].Type == SegmentType.Line)
                {
                    if (figure == null)
                    {
                        figure = new GraphicsPath();
                    }

                    if (currentPoint.X != path.Segments[i].Point.X || currentPoint.Y != path.Segments[i].Point.Y)
                    {
                        currentPoint = path.Segments[i].Point;
                        figure.LineTo(currentPoint);
                    }

                    if (!started)
                    {
                        startPoint = currentPoint;
                        started = true;
                    }
                }
                else if (path.Segments[i].Type == SegmentType.Arc && path.Segments[i] is ArcSegment arc)
                {
                    if (figure == null)
                    {
                        figure = new GraphicsPath();
                    }

                    Point endPoint = arc.GetPointAt(currentPoint, 1);

                    if (arc.StartAngle != arc.EndAngle)
                    {
                        currentPoint = endPoint;
                        figure.Arc(arc.Points[0], arc.Radius, arc.StartAngle, arc.EndAngle);
                    }
                    else if (endPoint.X != currentPoint.X || endPoint.Y != currentPoint.Y)
                    {
                        currentPoint = endPoint;
                        figure.LineTo(endPoint);
                    }

                    if (!started)
                    {
                        startPoint = currentPoint;
                        started = true;
                    }
                }
                else if (path.Segments[i].Type == SegmentType.CubicBezier)
                {
                    if (figure == null)
                    {
                        figure = new GraphicsPath();
                    }

                    if (!currentPoint.IsEqual(path.Segments[i].Points[0], Tolerance) || !currentPoint.IsEqual(path.Segments[i].Points[1], Tolerance) || !currentPoint.IsEqual(path.Segments[i].Points[2], Tolerance))
                    {
                        currentPoint = path.Segments[i].Points[2];
                        figure.CubicBezierTo(path.Segments[i].Points[0], path.Segments[i].Points[1], path.Segments[i].Points[2]);
                    }
                    
                    if (!started)
                    {
                        startPoint = currentPoint;
                        started = true;
                    }
                }
                else if (path.Segments[i].Type == SegmentType.Close)
                {
                    if (currentPoint.X != startPoint.X || currentPoint.Y != startPoint.Y)
                    {
                        currentPoint = startPoint;
                        figure.Close();

                        tbr.AddPath(figure);
                        figure = null;
                    }
                    else
                    {
                        if (figure.Segments[figure.Segments.Count - 1].Type == SegmentType.Line)
                        {
                            figure.Segments.RemoveAt(figure.Segments.Count - 1);
                        }
                        else if (figure.Segments[0].Type == SegmentType.Move && figure.Segments[1].Type == SegmentType.Line)
                        {
                            figure.Segments[0] = new MoveSegment(figure.Segments[1].Point);
                            figure.Segments.RemoveAt(1);
                        }

                        currentPoint = startPoint;
                        figure.Close();

                        tbr.AddPath(figure);
                        figure = null;
                    }

                    started = false;
                }
            }

            if (figure != null)
            {
                tbr.AddPath(figure);
            }

            return tbr;
        }
    }
}
