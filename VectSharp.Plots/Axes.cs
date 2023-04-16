/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2023 Giorgio Bianchini, University of Bristol

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

namespace VectSharp.Plots
{
    /// <summary>
    /// A plot element that draws an axis line and an arrow.
    /// </summary>
    public class ContinuousAxis : IPlotElement
    {
        /// <summary>
        /// The starting point of the axis (i.e., the blunt end of the arrow), expressed in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> StartPoint { get; set; }

        /// <summary>
        /// The ending point of the axis (i.e., the pointy end of the arrow), expressed in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> EndPoint { get; set; }

        /// <summary>
        /// The size of the arrow at the end of the axis, expressed in plot space coordinates.
        /// </summary>
        public double ArrowSize { get; set; } = 3;

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes determining the appearance of the axis.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes();

        /// <summary>
        /// A tag to identify the axis in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Creates a new <see cref="ContinuousAxis"/> instance.
        /// </summary>
        /// <param name="startPoint">The starting point of the axis (i.e., the blunt end of the arrow), expressed in data space coordinates.</param>
        /// <param name="endPoint">The ending point of the axis (i.e., the pointy end of the arrow), expressed in data space coordinates.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public ContinuousAxis(IReadOnlyList<double> startPoint, IReadOnlyList<double> endPoint, IContinuousCoordinateSystem coordinateSystem)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.CoordinateSystem = coordinateSystem;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            Point startPoint = CoordinateSystem.ToPlotCoordinates(StartPoint);
            Point endPoint = CoordinateSystem.ToPlotCoordinates(EndPoint);

            double[] direction = new double[StartPoint.Count];

            for (int i = 0; i < direction.Length; i++)
            {
                direction[i] = EndPoint[i] - StartPoint[i];
            }

            double angle;

            GraphicsPath path;

            if (CoordinateSystem.IsLinear || CoordinateSystem.IsDirectionStraight(direction))
            {
                path = new GraphicsPath().MoveTo(startPoint).LineTo(endPoint);
                angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
            }
            else
            {
                angle = 0;

                path = new GraphicsPath();

                int count = 0;

                for (int i = 0; i < direction.Length; i++)
                {
                    count = Math.Max(count, (int)Math.Ceiling(direction[i] / CoordinateSystem.Resolution[i]));
                }

                Point lastPoint = new Point();

                for (int i = 0; i <= count; i++)
                {
                    double[] pt = new double[StartPoint.Count];

                    for (int j = 0; j < pt.Length; j++)
                    {
                        pt[j] = StartPoint[j] + direction[j] / count * i;
                    }

                    Point point = CoordinateSystem.ToPlotCoordinates(pt);
                    path.LineTo(point);

                    if (i == count)
                    {
                        angle = Math.Atan2(point.Y - lastPoint.Y, point.X - lastPoint.X);
                    }
                    else
                    {
                        lastPoint = point;
                    }
                }
            }

            target.StrokePath(path, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, tag: Tag);

            if (ArrowSize > 0)
            {
                target.Save();
                target.Translate(endPoint);

                target.Rotate(angle);

                GraphicsPath arrowPath = new GraphicsPath().MoveTo(-ArrowSize * PresentationAttributes.LineWidth, -ArrowSize * PresentationAttributes.LineWidth).LineTo(ArrowSize * PresentationAttributes.LineWidth, 0).LineTo(-ArrowSize * PresentationAttributes.LineWidth, ArrowSize * PresentationAttributes.LineWidth).Close();

                string fillTag = Tag;
                string strokeTag = Tag;

                if (target.UseUniqueTags && !string.IsNullOrEmpty(Tag))
                {
                    fillTag += "@arrowFill";
                    strokeTag += "@arrowStroke";
                }

                if (PresentationAttributes.Fill != null)
                {
                    target.FillPath(arrowPath, PresentationAttributes.Fill, tag: fillTag);
                }

                target.StrokePath(arrowPath, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, tag: strokeTag);

                target.Restore();
            }
        }
    }

    /// <summary>
    /// A plot element that draws equally spaced ticks on an axis.
    /// </summary>
    public class ContinuousAxisTicks : IPlotElement
    {
        /// <summary>
        /// The starting point of the axis, expressed in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> StartPoint { get; set; }

        /// <summary>
        /// The ending point of the axis, expressed in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> EndPoint { get; set; }

        /// <summary>
        /// The number of intervals between ticks. Note that the number of ticks will be one greater than this.
        /// </summary>
        public double IntervalCount { get; set; } = 10;

        /// <summary>
        /// The size of the ticks "above" the axis. What "above" means depends on the orientation of the axis.
        /// This should be set to a function accepting an <see langword="int"/> argument representing the index of the tick, and
        /// return a <see langword="double"/> representing the size of the tick in plot coordinates.
        /// </summary>
        public Func<int, double> SizeAbove { get; set; } = i => i % 2 == 0 ? 3 : 2;

        /// <summary>
        /// The size of the ticks "below" the axis. What "below" means depends on the orientation of the axis.
        /// This should be set to a function accepting an <see langword="int"/> argument representing the index of the tick, and
        /// return a <see langword="double"/> representing the size of the tick in plot coordinates.
        /// </summary>
        public Func<int, double> SizeBelow { get; set; } = i => i % 2 == 0 ? 3 : 2;

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes determining the appearance of the ticks.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes();

        /// <summary>
        /// A tag to identify the ticks in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Creates a new <see cref="ContinuousAxisTicks"/> instance.
        /// </summary>
        /// <param name="startPoint">The starting point of the axis, expressed in data space coordinates.</param>
        /// <param name="endPoint">The ending point of the axis, expressed in data space coordinates.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public ContinuousAxisTicks(IReadOnlyList<double> startPoint, IReadOnlyList<double> endPoint, IContinuousCoordinateSystem coordinateSystem)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.CoordinateSystem = coordinateSystem;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            double[] direction = new double[StartPoint.Count];
            double directionMod = 0;

            for (int i = 0; i < direction.Length; i++)
            {
                direction[i] = EndPoint[i] - StartPoint[i];
                directionMod += direction[i] * direction[i];
            }

            directionMod = Math.Sqrt(directionMod);

            GraphicsPath tickPath = new GraphicsPath();

            double[] normDir = new double[StartPoint.Count];
            double[] invDir = new double[StartPoint.Count];

            for (int i = 0; i < direction.Length; i++)
            {
                normDir[i] = direction[i] / directionMod;
                invDir[i] = -normDir[i];
            }

            for (int i = 0; i <= IntervalCount; i++)
            {
                double[] pt = new double[StartPoint.Count];

                if (CoordinateSystem is IContinuousInvertibleCoordinateSystem inv && inv.IsDirectionStraight(direction))
                {
                    Point start = inv.ToPlotCoordinates(StartPoint);
                    Point end = inv.ToPlotCoordinates(EndPoint);

                    pt = inv.ToDataCoordinates(new Point(start.X * (1 - i / IntervalCount) + end.X * i / IntervalCount, start.Y * (1 - i / IntervalCount) + end.Y * i / IntervalCount));
				}
                else
                {
					for (int j = 0; j < pt.Length; j++)
					{
						pt[j] = StartPoint[j] + direction[j] / IntervalCount * i;
					}
				}


                double[] prevPt = CoordinateSystem.GetAround(pt, invDir);
                double[] nextPt = CoordinateSystem.GetAround(pt, normDir);

                Point point = CoordinateSystem.ToPlotCoordinates(pt);
                Point prevPoint = CoordinateSystem.ToPlotCoordinates(prevPt);
                Point nextPoint = CoordinateSystem.ToPlotCoordinates(nextPt);

                Point deriv = new Point(nextPoint.X - prevPoint.X, nextPoint.Y - prevPoint.Y);
                double derivMod = deriv.Modulus();
                deriv = new Point(deriv.X / derivMod, deriv.Y / derivMod);
                Point perp = new Point(-deriv.Y, deriv.X);

                double sizeAbove = SizeAbove(i);
                double sizeBelow = SizeBelow(i);

                if (sizeAbove + sizeBelow > 0)
                {
                    tickPath.MoveTo(point.X - sizeAbove * perp.X, point.Y - sizeAbove * perp.Y);
                    tickPath.LineTo(point.X + sizeBelow * perp.X, point.Y + sizeBelow * perp.Y);
                }
            }

            target.StrokePath(tickPath, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, tag: Tag);
        }

    }

    /// <summary>
    /// A plot element that draws equally spaced labels on an axis.
    /// </summary>
    public class ContinuousAxisLabels : IPlotElement
    {
        /// <summary>
        /// The starting point of the axis, expressed in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> StartPoint { get; set; }

        /// <summary>
        /// The ending point of the axis, expressed in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> EndPoint { get; set; }

        /// <summary>
        /// The number of intervals between labels. Note that the number of labels will be one greater than this.
        /// </summary>
        public double IntervalCount { get; set; } = 10;

        /// <summary>
        /// The distance of the label anchor from the point on the axis. 
        /// This should be set to a function accepting an <see langword="int"/> argument representing the index of the label, and
        /// return a <see langword="double"/> representing the distance of the label from the axis, in plot space coordinates.
        /// </summary>
        public Func<int, double> Position { get; set; } = _ => 10;

        /// <summary>
        /// A function used to determine what text to draw at each label. You should set this to a
        /// function accepting two parameters: an <see cref="IReadOnlyList{T}"/> of <see langword="double"/>s,
        /// representing the coordinates of the point the label refers to, and an <see langword="int"/>
        /// representing the index of the label. The function should return an <see cref="IEnumerable{T}" /> of
        /// <see cref="FormattedText" /> objects, representing the text that will be drawn.
        /// </summary>
        public Func<IReadOnlyList<double>, int, IEnumerable<FormattedText>> TextFormat { get; set; }
        
        /// <summary>
        /// The orientation of the label with respect to the horizontal. If this is <see langword="null"/>, the
        /// labels will be perpendicular to the axis.
        /// </summary>
        public double? Rotation { get; set; } = null;

        private IEnumerable<FormattedText> DefaultTextFormat(IReadOnlyList<double> point)
        {
            double[] direction = new double[StartPoint.Count];
            double directionMod = 0;

            double maxDirection = 0;
            int maxDirectionIndex = 0;

            for (int i = 0; i < direction.Length; i++)
            {
                direction[i] = EndPoint[i] - StartPoint[i];
                directionMod += direction[i] * direction[i];

                if (Math.Abs(direction[i]) > maxDirection)
                {
                    maxDirection = Math.Abs(direction[i]);
                    maxDirectionIndex = i;
                }
            }

            directionMod = Math.Sqrt(directionMod);

            if (maxDirection / directionMod >= 0.9)
            {
                double range = Math.Abs((EndPoint[maxDirectionIndex] - StartPoint[maxDirectionIndex]) / IntervalCount);

                string formatString;

                if (range >= 10)
                {
                    formatString = "0";
                }
                else if (range >= 1)
                {
                    formatString = "0.0";
                }
                else
                {
                    formatString = "0." + new string('0', -(int)Math.Floor(Math.Log10(range)) + 1);
                }

                if (PresentationAttributes.Font.FontFamily.IsStandardFamily)
                {
                    int i = Array.IndexOf(FontFamily.StandardFamilies, PresentationAttributes.Font.FontFamily.FamilyName.Replace(" ", "-"));

                    return FormattedText.Format(point[maxDirectionIndex].ToString(formatString, System.Globalization.CultureInfo.InvariantCulture), (FontFamily.StandardFontFamilies)i, PresentationAttributes.Font.FontSize);
                }
                else
                {
                    return FormattedText.Format(point[maxDirectionIndex].ToString(formatString, System.Globalization.CultureInfo.InvariantCulture), PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font);
                }
            }
            else
            {
                double pointMod = 0;

                for (int i = 0; i < point.Count; i++)
                {
                    pointMod += (point[i] - StartPoint[i]) * (point[i] - StartPoint[i]);
                }

                if (PresentationAttributes.Font.FontFamily.IsStandardFamily)
                {
                    int i = Array.IndexOf(FontFamily.StandardFamilies, PresentationAttributes.Font.FontFamily.FamilyName.Replace(" ", "-"));

                    return FormattedText.Format((pointMod / directionMod).ToString("0%", System.Globalization.CultureInfo.InvariantCulture), (FontFamily.StandardFontFamilies)i, PresentationAttributes.Font.FontSize);
                }
                else
                {
                    return FormattedText.Format((pointMod / directionMod).ToString("0%", System.Globalization.CultureInfo.InvariantCulture), PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font);
                }
            }
        }

        /// <summary>
        /// The baseline for the label anchor.
        /// </summary>
        public TextBaselines Baseline { get; set; } = TextBaselines.Middle;

        /// <summary>
        /// The alignment for the label anchor.
        /// </summary>
        public TextAnchors Alignment { get; set; } = TextAnchors.Left;

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes determining the appearance of the labels.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes();

        /// <summary>
        /// A tag to identify the labels in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Create a new <see cref="ContinuousAxisLabels"/> instance.
        /// </summary>
        /// <param name="startPoint">The starting point of the axis, expressed in data space coordinates.</param>
        /// <param name="endPoint">The ending point of the axis, expressed in data space coordinates.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public ContinuousAxisLabels(IReadOnlyList<double> startPoint, IReadOnlyList<double> endPoint, IContinuousCoordinateSystem coordinateSystem)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.CoordinateSystem = coordinateSystem;
            this.TextFormat = (p, i) => this.DefaultTextFormat(p);
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            double[] direction = new double[StartPoint.Count];
            double directionMod = 0;

            for (int i = 0; i < direction.Length; i++)
            {
                direction[i] = EndPoint[i] - StartPoint[i];
                directionMod += direction[i] * direction[i];
            }

            directionMod = Math.Sqrt(directionMod);

            double[] normDir = new double[StartPoint.Count];
            double[] invDir = new double[StartPoint.Count];

            for (int i = 0; i < direction.Length; i++)
            {
                normDir[i] = direction[i] / directionMod;
                invDir[i] = -normDir[i];
            }

            for (int i = 0; i <= IntervalCount; i++)
            {
                double[] pt = new double[StartPoint.Count];

				if (CoordinateSystem is IContinuousInvertibleCoordinateSystem inv && inv.IsDirectionStraight(direction))
				{
					Point start = inv.ToPlotCoordinates(StartPoint);
					Point end = inv.ToPlotCoordinates(EndPoint);

					pt = inv.ToDataCoordinates(new Point(start.X * (1 - i / IntervalCount) + end.X * i / IntervalCount, start.Y * (1 - i / IntervalCount) + end.Y * i / IntervalCount));
				}
				else
				{
					for (int j = 0; j < pt.Length; j++)
					{
						pt[j] = StartPoint[j] + direction[j] / IntervalCount * i;
					}
				}


				double[] prevPt = CoordinateSystem.GetAround(pt, invDir);
                double[] nextPt = CoordinateSystem.GetAround(pt, normDir);

                IEnumerable<FormattedText> text = TextFormat(pt, i);

                if (text != null)
                {
                    Point point = CoordinateSystem.ToPlotCoordinates(pt);
                    Point prevPoint = CoordinateSystem.ToPlotCoordinates(prevPt);
                    Point nextPoint = CoordinateSystem.ToPlotCoordinates(nextPt);

                    Point deriv = new Point(nextPoint.X - prevPoint.X, nextPoint.Y - prevPoint.Y);
                    double derivMod = deriv.Modulus();
                    deriv = new Point(deriv.X / derivMod, deriv.Y / derivMod);
                    Point perp = new Point(-deriv.Y, deriv.X);

                    double position = Position(i);

                    target.Save();
                    target.Translate(point.X + position * perp.X, point.Y + position * perp.Y);

                    if (Rotation == null)
                    {
                        target.Rotate(Math.Atan2(perp.Y, perp.X));
                    }
                    else
                    {
                        target.Rotate(Rotation ?? 0);
                    }


                    double x = Alignment == TextAnchors.Left ? 0 : Alignment == TextAnchors.Right ? -text.Measure().Width : -text.Measure().Width * 0.5;

                    string fillTag = Tag;
                    string strokeTag = Tag;

                    if (!string.IsNullOrEmpty(Tag) && target.UseUniqueTags)
                    {
                        fillTag = fillTag + "@" + i;
                        strokeTag = strokeTag + "@stroke" + i;
                    }

                    target.FillText(x, 0, text, PresentationAttributes.Fill, Baseline, fillTag);

                    if (PresentationAttributes.Stroke != null)
                    {
                        target.StrokeText(x, 0, text, PresentationAttributes.Stroke, Baseline, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, strokeTag);
                    }

                    target.Restore();
                }
            }
        }
    }

    /// <summary>
    /// A plot element that draws a title for an axis.
    /// </summary>
    public class ContinuousAxisTitle : IPlotElement
    {
        /// <summary>
        /// The starting point of the axis, expressed in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> StartPoint { get; set; }

        /// <summary>
        /// The ending point of the axis, expressed in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> EndPoint { get; set; }

        /// <summary>
        /// The distance between the title and the axis, in plot space coordinates.
        /// </summary>
        public double Position { get; set; } = 30;

        /// <summary>
        /// The axis title to draw.
        /// </summary>
        public IEnumerable<FormattedText> Title { get; set; }

        /// <summary>
        /// If the axis is not a straight line (e.g., because the coordinate system is not linear),
        /// if this is <see langword="true"/> the title will follow the shape of the axis. If this
        /// is <see langword="false"/>, the title will always be drawn on a straight line.
        /// </summary>
        public bool FollowAxis { get; set; } = true;

        /// <summary>
        /// Orientation of the title with respect to the horizontal. If this is <see langword="null"/>,
        /// the title is parallel to the axis.
        /// </summary>
        public double? Rotation { get; set; } = null;

        /// <summary>
        /// The baseline for the title anchor.
        /// </summary>
        public TextBaselines Baseline { get; set; } = TextBaselines.Middle;

        /// <summary>
        /// The alignment for the title anchor.
        /// </summary>
        public TextAnchors Alignment { get; set; } = TextAnchors.Left;

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes determining the appearance of the title.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 14), Stroke = null, Fill = Colours.Black };

        /// <summary>
        /// A tag to identify the labels in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Create a new <see cref="ContinuousAxisTitle"/> instance.
        /// </summary>
        /// <param name="title">The title to draw on the axis.</param>
        /// <param name="startPoint">The starting point of the axis, expressed in data space coordinates.</param>
        /// <param name="endPoint">The ending point of the axis, expressed in data space coordinates.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public ContinuousAxisTitle(IEnumerable<FormattedText> title, IReadOnlyList<double> startPoint, IReadOnlyList<double> endPoint, IContinuousCoordinateSystem coordinateSystem)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.CoordinateSystem = coordinateSystem;
            this.Title = title;
        }

        /// <summary>
        /// Create a new <see cref="ContinuousAxisTitle"/> instance.
        /// </summary>
        /// <param name="title">The title to draw on the axis.</param>
        /// <param name="startPoint">The starting point of the axis, expressed in data space coordinates.</param>
        /// <param name="endPoint">The ending point of the axis, expressed in data space coordinates.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="presentationAttributes">Presentation attributes determining the appearance of the title.</param>
        public ContinuousAxisTitle(string title, IReadOnlyList<double> startPoint, IReadOnlyList<double> endPoint, IContinuousCoordinateSystem coordinateSystem, PlotElementPresentationAttributes presentationAttributes = null)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.CoordinateSystem = coordinateSystem;

            if (presentationAttributes != null)
            {
                this.PresentationAttributes = presentationAttributes;
            }

            if (title != null)
            {
                if (PresentationAttributes.Font.FontFamily.IsStandardFamily)
                {
                    int i = Array.IndexOf(FontFamily.StandardFamilies, PresentationAttributes.Font.FontFamily.FamilyName.Replace(" ", "-"));

                    this.Title = FormattedText.Format(title, (FontFamily.StandardFontFamilies)i, PresentationAttributes.Font.FontSize);
                }
                else
                {
                    this.Title = FormattedText.Format(title, PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font);
                }
            }
            else
            {
                Title = null;
            }
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            if (Title != null)
            {
                double[] direction = new double[StartPoint.Count];
                double directionMod = 0;

                for (int i = 0; i < direction.Length; i++)
                {
                    direction[i] = EndPoint[i] - StartPoint[i];
                    directionMod += direction[i] * direction[i];
                }

                if (!FollowAxis || CoordinateSystem.IsLinear || CoordinateSystem.IsDirectionStraight(direction))
                {
                    directionMod = Math.Sqrt(directionMod);

                    double[] normDir = new double[StartPoint.Count];
                    double[] invDir = new double[StartPoint.Count];

                    for (int i = 0; i < direction.Length; i++)
                    {
                        normDir[i] = direction[i] / directionMod;
                        invDir[i] = -normDir[i];
                    }

                    double[] pt = new double[StartPoint.Count];

					if (CoordinateSystem is IContinuousInvertibleCoordinateSystem inv && inv.IsDirectionStraight(direction))
					{
						Point start = inv.ToPlotCoordinates(StartPoint);
						Point end = inv.ToPlotCoordinates(EndPoint);

						pt = inv.ToDataCoordinates(new Point(start.X * 0.5 + end.X * 0.5, start.Y * 0.5 + end.Y * 0.5));
					}
					else
					{
						for (int j = 0; j < pt.Length; j++)
						{
							pt[j] = StartPoint[j] + direction[j] * 0.5;
						}
					}

					double[] prevPt = CoordinateSystem.GetAround(pt, invDir);
                    double[] nextPt = CoordinateSystem.GetAround(pt, normDir);

                    Point point = CoordinateSystem.ToPlotCoordinates(pt);
                    Point prevPoint = CoordinateSystem.ToPlotCoordinates(prevPt);
                    Point nextPoint = CoordinateSystem.ToPlotCoordinates(nextPt);

                    Point deriv = new Point(nextPoint.X - prevPoint.X, nextPoint.Y - prevPoint.Y);
                    double derivMod = deriv.Modulus();
                    deriv = new Point(deriv.X / derivMod, deriv.Y / derivMod);
                    Point perp = new Point(-deriv.Y, deriv.X);

                    target.Save();
                    target.Translate(point.X + Position * perp.X, point.Y + Position * perp.Y);

                    if (Rotation == null)
                    {
                        target.Rotate(Math.Atan2(perp.Y, perp.X) - Math.PI / 2);
                    }
                    else
                    {
                        target.Rotate((Rotation ?? 0) - Math.PI / 2);
                    }


                    double x = Alignment == TextAnchors.Left ? 0 : Alignment == TextAnchors.Right ? -Title.Measure().Width : -Title.Measure().Width * 0.5;

                    string fillTag = Tag;
                    string strokeTag = Tag;

                    if (!string.IsNullOrEmpty(Tag) && target.UseUniqueTags)
                    {
                        strokeTag = strokeTag + "@stroke";
                    }

                    target.FillText(x, 0, Title, PresentationAttributes.Fill, Baseline, fillTag);

                    if (PresentationAttributes.Stroke != null)
                    {
                        target.StrokeText(x, 0, Title, PresentationAttributes.Stroke, Baseline, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, strokeTag);
                    }

                    target.Restore();
                }
                else
                {
                    GraphicsPath path;

                    path = new GraphicsPath();

                    int count = 0;

                    for (int i = 0; i < direction.Length; i++)
                    {
                        count = Math.Max(count, (int)Math.Ceiling(direction[i] / CoordinateSystem.Resolution[i]));
                    }

                    double[] normDir = new double[StartPoint.Count];
                    double[] invDir = new double[StartPoint.Count];

                    for (int i = 0; i < direction.Length; i++)
                    {
                        normDir[i] = direction[i] / directionMod;
                        invDir[i] = -normDir[i];
                    }

                    for (int i = 0; i <= count; i++)
                    {
                        double[] pt = new double[StartPoint.Count];

                        for (int j = 0; j < pt.Length; j++)
                        {
                            pt[j] = StartPoint[j] + direction[j] / count * i;
                        }

                        double[] prevPt = CoordinateSystem.GetAround(pt, invDir);
                        double[] nextPt = CoordinateSystem.GetAround(pt, normDir);


                        Point point = CoordinateSystem.ToPlotCoordinates(pt);
                        Point prevPoint = CoordinateSystem.ToPlotCoordinates(prevPt);
                        Point nextPoint = CoordinateSystem.ToPlotCoordinates(nextPt);

                        Point deriv = new Point(nextPoint.X - prevPoint.X, nextPoint.Y - prevPoint.Y);
                        double derivMod = deriv.Modulus();
                        deriv = new Point(deriv.X / derivMod, deriv.Y / derivMod);
                        Point perp = new Point(-deriv.Y, deriv.X);

                        path.LineTo(point.X + perp.X * Position, point.Y + perp.Y * Position);
                    }

                    string fillTag = Tag;
                    string strokeTag = Tag;

                    if (!string.IsNullOrEmpty(Tag) && target.UseUniqueTags)
                    {
                        strokeTag = strokeTag + "@stroke";
                    }

                    target.FillTextOnPath(path, Title.GetText(), PresentationAttributes.Font, PresentationAttributes.Fill, 0.5, Alignment, Baseline, fillTag);

                    if (PresentationAttributes.Stroke != null)
                    {
                        target.StrokeTextOnPath(path, Title.GetText(), PresentationAttributes.Font, PresentationAttributes.Fill, 0.5, Alignment, Baseline, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, strokeTag);
                    }
                }
            }
        }
    }

    /// <summary>
    /// A plot element that draws a grid.
    /// </summary>
    public class Grid : IPlotElement
    {
        /// <summary>
        /// The starting point for the first side of the grid.
        /// </summary>
        public IReadOnlyList<double> Side1Start { get; set; }

        /// <summary>
        /// The ending point for the first side of the grid.
        /// </summary>
        public IReadOnlyList<double> Side1End { get; set; }

        /// <summary>
        /// The starting point for the second side of the grid.
        /// </summary>
        public IReadOnlyList<double> Side2Start { get; set; }

        /// <summary>
        /// The ending point for the second side of the grid.
        /// </summary>
        public IReadOnlyList<double> Side2End { get; set; }

        /// <summary>
        /// The number of intervals between grid lines. Note that the number of grid lines will be one greater than this.
        /// </summary>
        public int IntervalCount { get; set; } = 10;

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes determining the appearance of the grid.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(220, 220, 220)) };

        /// <summary>
        /// A tag to identify the grid in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Create a new <see cref="Grid"/> instance.
        /// </summary>
        /// <param name="side1Start">The starting point for the first side of the grid.</param>
        /// <param name="side1End">The ending point for the first side of the grid.</param>
        /// <param name="side2Start">The starting point for the second side of the grid.</param>
        /// <param name="side2End">The ending point for the second side of the grid.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Grid(IReadOnlyList<double> side1Start, IReadOnlyList<double> side1End, IReadOnlyList<double> side2Start, IReadOnlyList<double> side2End, IContinuousCoordinateSystem coordinateSystem)
        {
            this.Side1Start = side1Start;
            this.Side1End = side1End;
            this.Side2Start = side2Start;
            this.Side2End = side2End;
            this.CoordinateSystem = coordinateSystem;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            IReadOnlyList<double> StartPoint1 = Side1Start;
            IReadOnlyList<double> EndPoint1 = Side1End;
            IReadOnlyList<double> StartPoint2 = Side2Start;
            IReadOnlyList<double> EndPoint2 = Side2End;

            double[] direction1 = new double[StartPoint1.Count];

            for (int i = 0; i < direction1.Length; i++)
            {
                direction1[i] = EndPoint1[i] - StartPoint1[i];
            }

            double[] direction2 = new double[StartPoint2.Count];

            for (int i = 0; i < direction2.Length; i++)
            {
                direction2[i] = EndPoint2[i] - StartPoint2[i];
            }

            GraphicsPath gridPath = new GraphicsPath();

            for (int i = 0; i <= IntervalCount; i++)
            {
                double[] pt1 = new double[StartPoint1.Count];
                double[] pt2 = new double[StartPoint2.Count];

				if (CoordinateSystem is IContinuousInvertibleCoordinateSystem inv)
				{
                    if (inv.IsDirectionStraight(direction1))
                    {
                        Point start = inv.ToPlotCoordinates(StartPoint1);
                        Point end = inv.ToPlotCoordinates(EndPoint1);

                        pt1 = inv.ToDataCoordinates(new Point(start.X * (1 - (double)i / IntervalCount) + end.X * i / IntervalCount, start.Y * (1 - (double)i / IntervalCount) + end.Y * i / IntervalCount));
                    }
                    else
                    {
						for (int j = 0; j < pt1.Length; j++)
						{
							pt1[j] = StartPoint1[j] + direction1[j] / IntervalCount * i;
						}
					}

					if (inv.IsDirectionStraight(direction2))
					{
						Point start = inv.ToPlotCoordinates(StartPoint2);
						Point end = inv.ToPlotCoordinates(EndPoint2);

						pt2 = inv.ToDataCoordinates(new Point(start.X * (1 - (double)i / IntervalCount) + end.X * i / IntervalCount, start.Y * (1 - (double)i / IntervalCount) + end.Y * i / IntervalCount));
					}
					else
					{
						for (int j = 0; j < pt1.Length; j++)
						{
							pt2[j] = StartPoint2[j] + direction2[j] / IntervalCount * i;
						}
					}
				}
				else
				{
					for (int j = 0; j < pt1.Length; j++)
					{
						pt1[j] = StartPoint1[j] + direction1[j] / IntervalCount * i;
						pt2[j] = StartPoint2[j] + direction2[j] / IntervalCount * i;
					}
				}

				


                Point point1 = CoordinateSystem.ToPlotCoordinates(pt1);
                Point point2 = CoordinateSystem.ToPlotCoordinates(pt2);


                gridPath.MoveTo(point1).LineTo(point2);
            }

            target.StrokePath(gridPath, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, tag: Tag);
        }
    }
}
