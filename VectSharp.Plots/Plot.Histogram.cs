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
using System.Linq;

namespace VectSharp.Plots
{
    partial class Plot
    {
        /// <summary>
        /// Describes the kind of normalisation that is performed.
        /// </summary>
        public enum NormalisationMode
        {
            /// <summary>
            /// No normalisation is performed.
            /// </summary>
            None,

            /// <summary>
            /// Values are normalised so that the maximum of each distribution corresponds to the same value.
            /// </summary>
            Maximum,

            /// <summary>
            /// Values are normalised so that the area covered by each distribution is the same.
            /// </summary>
            Area
        }

        internal static (double q1, double q3, double iqr) IQR(IEnumerable<double> values)
        {
            double[] values2 = (from el in values orderby el ascending select el).ToArray();

            double q1, q3;

            if (values2.Length < 4)
            {
                q1 = values2[0];
                q3 = values2[values2.Length - 1];
            }
            else if (values2.Length == 4)
            {
                q1 = (values2[0] + values2[1]) * 0.5;
                q3 = (values2[2] + values2[3]) * 0.5;
            }
            else if (values2.Length % 2 == 0)
            {
                q1 = values2.Length % 4 == 0 ? (values2[values2.Length / 4] + values2[values2.Length / 4 + 1]) * 0.5 : values2[values2.Length / 4];
                q3 = values2.Length % 4 == 0 ? (values2[3 * values2.Length / 4] + values2[3 * values2.Length / 4 + 1]) * 0.5 : values2[3 * values2.Length / 4];
            }
            else if (values2.Length % 4 == 1)
            {
                int n = (values2.Length - 1) / 4;
                q1 = 0.75 * values2[n - 1] + 0.25 * values2[n];
                q3 = 0.25 * values2[3 * n] + 0.75 * values2[3 * n + 1];
            }
            else
            {
                int n = (values2.Length - 3) / 4;
                q1 = 0.75 * values2[n] + 0.25 * values2[n + 1];
                q3 = 0.25 * values2[3 * n + 1] + 0.75 * values2[3 * n + 2];
            }

            return (q1, q3, q3 - q1);
        }

        partial class Create
        {
            private static Plot Histogram(IReadOnlyList<IReadOnlyList<double>> data, double overrideMax, double overrideMin, bool vertical = true, double margin = 0, double width = 350, double height = 250,
                   PlotElementPresentationAttributes axisPresentationAttributes = null,
                   double axisArrowSize = 3,
                   PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                   PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                   string xAxisTitle = null,
                   string yAxisTitle = null,
                   string title = null,
                   PlotElementPresentationAttributes titlePresentationAttributes = null,
                   PlotElementPresentationAttributes gridPresentationAttributes = null,
                   PlotElementPresentationAttributes dataPresentationAttributes = null,
                   IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                if (axisPresentationAttributes == null)
                {
                    axisPresentationAttributes = new PlotElementPresentationAttributes();
                }

                if (gridPresentationAttributes == null)
                {
                    gridPresentationAttributes = new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(220, 220, 220)) };
                }

                if (axisLabelPresentationAttributes == null)
                {
                    axisLabelPresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null };
                }

                if (axisTitlePresentationAttributes == null)
                {
                    axisTitlePresentationAttributes = new PlotElementPresentationAttributes() { Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 14), Stroke = null };
                }

                if (dataPresentationAttributes == null)
                {
                    dataPresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Fill = new SolidColourBrush(Colour.FromRgb(0, 114, 178)) };
                }

                double baselineValue = 0;

                if (coordinateSystem is LogarithmicCoordinateSystem2D || (!vertical && coordinateSystem is LinLogCoordinateSystem2D) || (vertical && coordinateSystem is LogLinCoordinateSystem2D))
                {
                    baselineValue = 1;
                }

                Bars bars;

                if (baselineValue == 0)
                {
                    bars = new Bars(data, coordinateSystem, vertical) { PresentationAttributes = dataPresentationAttributes, Margin = margin };
                }
                else
                {
                    bars = new Bars(data, new Comparison<IReadOnlyList<double>>(vertical ? new Func<IReadOnlyList<double>, IReadOnlyList<double>, int>((x, y) => Math.Sign(x[0] - y[0])) : (x, y) => Math.Sign(x[1] - y[1])), vertical ? new Func<IReadOnlyList<double>, IReadOnlyList<double>>(pt => new double[] { pt[0], baselineValue }) : pt => new double[] { baselineValue, pt[1] }, coordinateSystem) { PresentationAttributes = dataPresentationAttributes, Margin = margin };
                }

                (double minX, double minY, double maxX, double maxY, double rangeX, double rangeY) = GetDataRange(data);

                double dataMinX = minX;
                double dataMinY = minY;
                double dataMaxX = maxX;
                double dataMaxY = maxY;

                if (coordinateSystem == null)
                {
                    LinearCoordinateSystem2D linCoords = new LinearCoordinateSystem2D(data, width, height);

                    if (vertical)
                    {
                        minY = Math.Min(minY, 0);
                        maxY = Math.Max(maxY, 0);

                        if (!double.IsNaN(overrideMax))
                        {
                            maxY = overrideMax;
                        }

                        if (!double.IsNaN(overrideMin))
                        {
                            minY = overrideMin;
                        }

                        rangeY = maxY - minY;

                        linCoords.MinY = minY - rangeY * 0.1;
                        linCoords.MaxY = maxY + rangeY * 0.1;


                        if (bars.Data.Count >= 2)
                        {
                            IReadOnlyList<double> item0 = bars.Data.ElementAt(0);
                            IReadOnlyList<double> item1 = bars.Data.ElementAt(1);

                            IReadOnlyList<double> itemN = bars.Data.ElementAt(bars.Data.Count - 1);
                            IReadOnlyList<double> itemN1 = bars.Data.ElementAt(bars.Data.Count - 2);

                            minX = Math.Min(minX, 1.5 * item0[0] - item1[0] * 0.5);
                            maxX = Math.Max(maxX, 1.5 * itemN[0] - itemN1[0] * 0.5);

                            rangeX = maxX - minX;
                            linCoords.MinX = minX;
                            linCoords.MaxX = maxX;
                        }
                    }
                    else
                    {
                        minX = Math.Min(minX, 0);
                        maxX = Math.Max(maxX, 0);

                        if (!double.IsNaN(overrideMax))
                        {
                            maxX = overrideMax;
                        }

                        if (!double.IsNaN(overrideMin))
                        {
                            minX = overrideMin;
                        }

                        rangeX = maxX - minX;

                        linCoords.MinX = minX - rangeX * 0.1;
                        linCoords.MaxX = maxX + rangeX * 0.1;


                        if (bars.Data.Count >= 2)
                        {
                            IReadOnlyList<double> item0 = bars.Data.ElementAt(0);
                            IReadOnlyList<double> item1 = bars.Data.ElementAt(1);

                            IReadOnlyList<double> itemN = bars.Data.ElementAt(bars.Data.Count - 1);
                            IReadOnlyList<double> itemN1 = bars.Data.ElementAt(bars.Data.Count - 2);

                            minY = Math.Min(minY, 1.5 * item0[1] - item1[1] * 0.5);
                            maxY = Math.Max(maxY, 1.5 * itemN[1] - itemN1[1] * 0.5);

                            rangeY = maxY - minY;
                            linCoords.MinY = minY;
                            linCoords.MaxY = maxY;
                        }
                    }

                    coordinateSystem = linCoords;
                    bars.CoordinateSystem = coordinateSystem;
                }
                else
                {
                    if (vertical)
                    {
                        minY = Math.Min(minY, baselineValue);
                        rangeY = maxY - minY;

                        if (bars.Data.Count >= 2)
                        {
                            IReadOnlyList<double> item0 = bars.Data.ElementAt(0);
                            IReadOnlyList<double> item1 = bars.Data.ElementAt(1);

                            IReadOnlyList<double> itemN = bars.Data.ElementAt(bars.Data.Count - 1);
                            IReadOnlyList<double> itemN1 = bars.Data.ElementAt(bars.Data.Count - 2);

                            minX = Math.Min(minX, 1.5 * item0[0] - item1[0] * 0.5);
                            maxX = Math.Max(maxX, 1.5 * itemN[0] - itemN1[0] * 0.5);

                            rangeX = maxX - minX;
                        }
                    }
                    else
                    {
                        minX = Math.Min(minX, baselineValue);
                        rangeX = maxX - minX;

                        if (bars.Data.Count >= 2)
                        {
                            IReadOnlyList<double> item0 = bars.Data.ElementAt(0);
                            IReadOnlyList<double> item1 = bars.Data.ElementAt(1);

                            IReadOnlyList<double> itemN = bars.Data.ElementAt(bars.Data.Count - 1);
                            IReadOnlyList<double> itemN1 = bars.Data.ElementAt(bars.Data.Count - 2);

                            minY = Math.Min(minY, 1.5 * item0[1] - item1[1] * 0.5);
                            maxY = Math.Max(maxY, 1.5 * itemN[1] - itemN1[1] * 0.5);

                            rangeY = maxY - minY;
                        }
                    }
                }



                if (titlePresentationAttributes == null)
                {
                    titlePresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 18) };
                }

                Point topLeft = coordinateSystem.ToPlotCoordinates(new double[] { minX, maxY });
                Point topRight = coordinateSystem.ToPlotCoordinates(new double[] { maxX, maxY });
                Point bottomRight = coordinateSystem.ToPlotCoordinates(new double[] { maxX, minY });
                Point bottomLeft = coordinateSystem.ToPlotCoordinates(new double[] { minX, minY });

                double[] marginTopLeft = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10));
                double[] marginTopRight = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10));
                double[] marginBottomRight = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10));
                double[] marginBottomLeft = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10));

                double[] p1 = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)), Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10));
                double[] p2 = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)), Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10));
                double[] p3 = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)), Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10));
                double[] p4 = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)), Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10));


                double[] p5 = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y))));
                double[] p6 = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y))));
                double[] p7 = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y))));
                double[] p8 = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y))));

                if (vertical)
                {
                    Point p1p = new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)), Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10);
                    Point p1p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMinX, dataMaxY });

                    Point p2p = new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)), Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10);
                    Point p2p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMaxX, dataMaxY });

                    Point p3p = new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)), Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10);
                    Point p3p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMinX, 0 });

                    Point p4p = new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)), Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10);
                    Point p4p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMaxX, 0 });

                    p1 = coordinateSystem.ToDataCoordinates(new Point(p1p2.X, p1p.Y));
                    p2 = coordinateSystem.ToDataCoordinates(new Point(p2p2.X, p2p.Y));
                    p3 = coordinateSystem.ToDataCoordinates(new Point(p3p2.X, p3p.Y));
                    p4 = coordinateSystem.ToDataCoordinates(new Point(p4p2.X, p4p.Y));
                }
                else
                {
                    Point p5p = new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)));
                    Point p5p2 = coordinateSystem.ToPlotCoordinates(new double[] { 0, dataMaxY });

                    Point p6p = new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)));
                    Point p6p2 = coordinateSystem.ToPlotCoordinates(new double[] { 0, dataMinY });

                    Point p7p = new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)));
                    Point p7p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMaxX, dataMaxY });

                    Point p8p = new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)));
                    Point p8p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMinX, dataMinY });

                    p5 = coordinateSystem.ToDataCoordinates(new Point(p5p.X, p5p2.Y));
                    p6 = coordinateSystem.ToDataCoordinates(new Point(p6p.X, p6p2.Y));
                    p7 = coordinateSystem.ToDataCoordinates(new Point(p7p2.X, p7p.Y));
                    p8 = coordinateSystem.ToDataCoordinates(new Point(p8p2.X, p8p.Y));
                }

                Grid grid;

                if (vertical)
                {
                    grid = new Grid(p5, p6, p7, p8, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };
                }
                else
                {
                    grid = new Grid(p1, p2, p3, p4, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };
                }

                ContinuousAxis xAxis = new ContinuousAxis(marginBottomLeft[0] < marginBottomRight[0] ? marginBottomLeft : marginBottomRight, marginBottomLeft[0] < marginBottomRight[0] ? marginBottomRight : vertical ? marginBottomLeft : coordinateSystem.ToDataCoordinates(coordinateSystem.ToPlotCoordinates(marginBottomLeft) + new Point(-axisArrowSize - 7, 0)), coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = axisArrowSize };
                ContinuousAxis yAxis = new ContinuousAxis(marginBottomLeft[1] < marginTopLeft[1] ? marginBottomLeft : marginTopLeft, marginBottomLeft[1] < marginTopLeft[1] ? marginTopLeft : !vertical ? marginBottomLeft : coordinateSystem.ToDataCoordinates(coordinateSystem.ToPlotCoordinates(marginBottomLeft) + new Point(0, axisArrowSize + 7)), coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = axisArrowSize };

                int every = (int)Math.Ceiling((double)data.Count / 5);
                int shift = (data.Count - every * (data.Count / every - 1) - 1) / 2;

                ContinuousAxisTicks xTicks;
                ContinuousAxisTicks yTicks;

                if (vertical)
                {
                    xTicks = new ContinuousAxisTicks(p3, p4, coordinateSystem) { PresentationAttributes = axisPresentationAttributes, IntervalCount = data.Count - 1, SizeAbove = i => (i - shift) % every == 0 ? 3 : 2, SizeBelow = i => (i - shift) % every == 0 ? 3 : 2 };
                    yTicks = new ContinuousAxisTicks(p6, p5, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };
                }
                else
                {
                    xTicks = new ContinuousAxisTicks(p3, p4, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };
                    yTicks = new ContinuousAxisTicks(p6, p5, coordinateSystem) { PresentationAttributes = axisPresentationAttributes, IntervalCount = data.Count - 1, SizeAbove = i => (i - shift) % every == 0 ? 3 : 2, SizeBelow = i => (i - shift) % every == 0 ? 3 : 2 };
                }

                ContinuousAxisLabels xLabels;
                ContinuousAxisLabels yLabels;

                if (vertical)
                {
                    xLabels = new ContinuousAxisLabels(p3, p4, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Alignment = TextAnchors.Center, Baseline = TextBaselines.Top, Rotation = 0, IntervalCount = data.Count - 1 };
                    yLabels = new ContinuousAxisLabels(p6, p5, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Position = _ => -10, Alignment = TextAnchors.Right, Rotation = 0, IntervalCount = 5 };

                    Func<IReadOnlyList<double>, int, IEnumerable<FormattedText>> originalFormatter = xLabels.TextFormat;
                    xLabels.TextFormat = (x, i) => (i - shift) % every == 0 ? originalFormatter(x, i) : null;
                }
                else
                {
                    xLabels = new ContinuousAxisLabels(p3, p4, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Alignment = TextAnchors.Center, Baseline = TextBaselines.Top, Rotation = 0, IntervalCount = 5 };
                    yLabels = new ContinuousAxisLabels(p6, p5, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Position = _ => -10, Alignment = TextAnchors.Right, Rotation = 0, IntervalCount = data.Count - 1 };

                    Func<IReadOnlyList<double>, int, IEnumerable<FormattedText>> originalFormatter = yLabels.TextFormat;
                    yLabels.TextFormat = (x, i) => (i - shift) % every == 0 ? originalFormatter(x, i) : null;
                }

                Graphics xLabelsSize = new Graphics();
                xLabels.Plot(xLabelsSize);
                double xLabelsHeight = xLabelsSize.GetBounds().Size.Height;

                Graphics yLabelsSize = new Graphics();
                yLabels.Plot(yLabelsSize);
                double yLabelsWidth = yLabelsSize.GetBounds().Size.Width;

                ContinuousAxisTitle xTitle = new ContinuousAxisTitle(xAxisTitle, marginBottomLeft, marginBottomRight, coordinateSystem, axisTitlePresentationAttributes) { Position = xLabelsHeight + 20, Alignment = TextAnchors.Center };
                ContinuousAxisTitle yTitle = new ContinuousAxisTitle(yAxisTitle, marginBottomLeft, marginTopLeft, coordinateSystem, axisTitlePresentationAttributes) { Position = -20 - yLabelsWidth, Baseline = TextBaselines.Bottom, Alignment = TextAnchors.Center };

                TextLabel<IReadOnlyList<double>> titleLabel = new TextLabel<IReadOnlyList<double>>(title, coordinateSystem.ToDataCoordinates((coordinateSystem.ToPlotCoordinates(marginTopLeft) + coordinateSystem.ToPlotCoordinates(marginTopRight)) * 0.5 + new Point(0, -10)), coordinateSystem) { Baseline = TextBaselines.Bottom, PresentationAttributes = titlePresentationAttributes };

                Plot tbr = new Plot();
                tbr.AddPlotElements(grid, xAxis, yAxis, xTicks, yTicks, xLabels, yLabels, xTitle, yTitle, bars, titleLabel);

                return tbr;
            }

            /// <summary>
            /// Create a new histogram.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the bars go from the X axis up to the sampled values. If this is <see langword="false"/>, the bars go from the Y axis to the sampled values.</param>
            /// <param name="margin">Spacing between consecutive bars. This should be a value between 0 and 1.</param>
            /// <param name="width">The width of the plot.</param>
            /// <param name="height">The height of the plot.</param>
            /// <param name="axisPresentationAttributes">Presentation attributes for the axes.</param>
            /// <param name="axisArrowSize">Size of the arrow at the end of each axis.</param>
            /// <param name="axisLabelPresentationAttributes">Presentation attributes for the axis labels.</param>
            /// <param name="axisTitlePresentationAttributes">Presentation attributes for the axis titles.</param>
            /// <param name="xAxisTitle">Title for the X axis.</param>
            /// <param name="yAxisTitle">Title for the Y axis.</param>
            /// <param name="title">Title for the plot.</param>
            /// <param name="titlePresentationAttributes">Presentation attributes for the plot title.</param>
            /// <param name="gridPresentationAttributes">Presentation attributes for the grid.</param>
            /// <param name="dataPresentationAttributes">Presentation attributes for the plotted data.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the histogram.</returns>
            public static Plot Histogram(IReadOnlyList<IReadOnlyList<double>> data, bool vertical = true, double margin = 0, double width = 350, double height = 250,
                   PlotElementPresentationAttributes axisPresentationAttributes = null,
                   double axisArrowSize = 3,
                   PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                   PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                   string xAxisTitle = null,
                   string yAxisTitle = null,
                   string title = null,
                   PlotElementPresentationAttributes titlePresentationAttributes = null,
                   PlotElementPresentationAttributes gridPresentationAttributes = null,
                   PlotElementPresentationAttributes dataPresentationAttributes = null,
                   IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return Histogram(data, double.NaN, double.NaN, vertical, margin, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, coordinateSystem);
            }

            /// <summary>
            /// Create a new histogram.
            /// </summary>
            /// <param name="data">The data whose distribution will be plotted.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the bars go from the X axis up to the sampled values. If this is <see langword="false"/>, the bars go from the Y axis to the sampled values.</param>
            /// <param name="margin">Spacing between consecutive bars. This should be a value between 0 and 1.</param>
            /// <param name="binCount">The number of bins to use. If this is &lt; 2, the number of bins is determined automatically using the Freedman-Diaconis rule.</param>
            /// <param name="underflow">Values smaller than this will be excluded from the main plot and moved into an underflow bin.</param>
            /// <param name="overflow">Values larger than this will be excluded from the main plot and moved into an overflow bin.</param>
            /// <param name="width">The width of the plot.</param>
            /// <param name="height">The height of the plot.</param>
            /// <param name="axisPresentationAttributes">Presentation attributes for the axes.</param>
            /// <param name="axisArrowSize">Size of the arrow at the end of each axis.</param>
            /// <param name="axisLabelPresentationAttributes">Presentation attributes for the axis labels.</param>
            /// <param name="axisTitlePresentationAttributes">Presentation attributes for the axis titles.</param>
            /// <param name="xAxisTitle">Title for the X axis.</param>
            /// <param name="yAxisTitle">Title for the Y axis.</param>
            /// <param name="title">Title for the plot.</param>
            /// <param name="titlePresentationAttributes">Presentation attributes for the plot title.</param>
            /// <param name="gridPresentationAttributes">Presentation attributes for the grid.</param>
            /// <param name="dataPresentationAttributes">Presentation attributes for the plotted data.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the histogram.</returns>
            public static Plot Histogram(IReadOnlyList<double> data, bool vertical = true, double margin = 0, int binCount = -1, double underflow = double.NegativeInfinity, double overflow = double.PositiveInfinity, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                PlotElementPresentationAttributes dataPresentationAttributes = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                if (dataPresentationAttributes == null)
                {
                    dataPresentationAttributes = new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(0, 114, 178)), LineWidth = 0.5, Fill = new SolidColourBrush(Colour.FromRgb(0, 114, 178)) };
                }

                int underflowCount = 0;
                int overflowCount = 0;

                List<double> binnableData = new List<double>(data.Count);

                double min = double.MaxValue;
                double max = double.MinValue;

                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i] > underflow && data[i] < overflow)
                    {
                        min = Math.Min(min, data[i]);
                        max = Math.Max(max, data[i]);
                        binnableData.Add(data[i]);
                    }
                    else if (data[i] <= underflow)
                    {
                        underflowCount++;
                    }
                    else if (data[i] >= overflow)
                    {
                        overflowCount++;
                    }
                }

                if (binCount <= 1)
                {
                    (double _, double _, double iqr) = IQR(binnableData);
                    double h2 = 2 * iqr / Math.Pow(binnableData.Count, 1.0 / 3.0);

                    if (h2 > 0)
                    {
                        binCount = Math.Max(2, (int)Math.Ceiling((max - min) / h2));
                    }
                    else
                    {
                        binCount = 2;
                    }
                }

                int[] bins = new int[binCount];

                if (max > min)
                {
                    for (int i = 0; i < binnableData.Count; i++)
                    {
                        int index = (int)Math.Min(binCount - 1, Math.Floor((binnableData[i] - min) / (max - min) * binCount));

                        bins[index]++;
                    }
                }
                else
                {
                    bins[0] = binnableData.Count;
                }

                (string, double)[] binnedData = new (string, double)[bins.Length];

                for (int i = 0; i < bins.Length; i++)
                {
                    double binStart = min + (max - min) / binCount * i;
                    double binEnd = min + (max - min) / binCount * (i + 1);

                    string formatString;

                    if (binEnd - binStart >= 10)
                    {
                        formatString = "0";
                    }
                    else if (binEnd - binStart >= 1)
                    {
                        formatString = "0.0";
                    }
                    else if (binEnd == binStart)
                    {
                        formatString = "0." + new string('0', -(int)Math.Floor(Math.Log10(Math.Abs(binEnd))) + 1);
                    }
                    else
                    {
                        formatString = "0." + new string('0', -(int)Math.Floor(Math.Log10(binEnd - binStart)) + 1);
                    }

                    string binLabel = "[" + binStart.ToString(formatString, System.Globalization.CultureInfo.InvariantCulture) + ", " + binEnd.ToString(formatString, System.Globalization.CultureInfo.InvariantCulture);

                    if (i < bins.Length - 1)
                    {
                        binLabel += ")";
                    }
                    else
                    {
                        binLabel += "]";
                    }

                    binnedData[i] = (binLabel, bins[i]);
                }

                double maxY = Math.Max(Math.Max(underflowCount, overflowCount), bins.Max());

                Plot plot = Create.BarChart(binnedData, maxY, double.NaN, vertical, margin, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, coordinateSystem);

                if (vertical)
                {
                    DataLabels<IReadOnlyList<double>> xLabels = plot.GetFirst<DataLabels<IReadOnlyList<double>>>();

                    xLabels.Alignment = TextAnchors.Left;

                    xLabels.Rotation = (a, b) => Math.PI / 6;

                    xLabels.Baseline = TextBaselines.Middle;

                    Func<int, IReadOnlyList<double>, object> prevLabels = xLabels.Label;

                    xLabels.Label = (i, x) =>
                    {
                        if (i == 0 || i == bins.Length - 1 || i == Math.Floor(bins.Length * 0.5) || i == Math.Floor(bins.Length * 0.25) || i == Math.Floor(bins.Length * 0.75))
                        {
                            return prevLabels(i, x);
                        }
                        else
                        {
                            return null;
                        }
                    };

                    plot.GetFirst<ContinuousAxisTicks>().SizeAbove = i => (i == 0 || i == bins.Length - 1 || i == Math.Floor(bins.Length * 0.5) || i == Math.Floor(bins.Length * 0.25) || i == Math.Floor(bins.Length * 0.75)) ? 3 : 2;
                    plot.GetFirst<ContinuousAxisTicks>().SizeBelow = i => (i == 0 || i == bins.Length - 1 || i == Math.Floor(bins.Length * 0.5) || i == Math.Floor(bins.Length * 0.25) || i == Math.Floor(bins.Length * 0.75)) ? 3 : 2;

                    Graphics xLabelsSize = new Graphics();
                    xLabels.Plot(xLabelsSize);
                    double xLabelsHeight = xLabelsSize.GetBounds().Size.Height;

                    plot.GetFirst<ContinuousAxisTitle>().Position = xLabelsHeight + 20;

                }
                else
                {
                    DataLabels<IReadOnlyList<double>> xLabels = plot.GetFirst<DataLabels<IReadOnlyList<double>>>();

                    Func<int, IReadOnlyList<double>, object> prevLabels = xLabels.Label;

                    xLabels.Label = (i, x) =>
                    {
                        if (i == 0 || i == bins.Length - 1 || i == Math.Floor(bins.Length * 0.5) || i == Math.Floor(bins.Length * 0.25) || i == Math.Floor(bins.Length * 0.75))
                        {
                            return prevLabels(i, x);
                        }
                        else
                        {
                            return null;
                        }
                    };

                    plot.GetAll<ContinuousAxisTicks>().ElementAt(1).SizeAbove = i => (i == 0 || i == bins.Length - 1 || i == Math.Floor(bins.Length * 0.5) || i == Math.Floor(bins.Length * 0.25) || i == Math.Floor(bins.Length * 0.75)) ? 3 : 2;
                    plot.GetAll<ContinuousAxisTicks>().ElementAt(1).SizeBelow = i => (i == 0 || i == bins.Length - 1 || i == Math.Floor(bins.Length * 0.5) || i == Math.Floor(bins.Length * 0.25) || i == Math.Floor(bins.Length * 0.75)) ? 3 : 2;
                }

                if (underflowCount > 0)
                {
                    if (vertical)
                    {
                        ContinuousAxis xAxis = plot.GetFirst<ContinuousAxis>();
                        xAxis.StartPoint = new double[] { xAxis.StartPoint[0] - 1.5, xAxis.StartPoint[1] };

                        ContinuousAxis yAxis = plot.GetAll<ContinuousAxis>().ElementAt(1);

                        yAxis.StartPoint = new double[] { yAxis.StartPoint[0] - 1.5, yAxis.StartPoint[1] };
                        yAxis.EndPoint = new double[] { yAxis.EndPoint[0] - 1.5, yAxis.EndPoint[1] };

                        Grid grid = plot.GetFirst<Grid>();
                        grid.Side1Start = new double[] { grid.Side1Start[0] - 1.5, grid.Side1Start[1] };
                        grid.Side1End = new double[] { grid.Side1End[0] - 1.5, grid.Side1End[1] };

                        ContinuousAxisTicks yTicks = plot.GetAll<ContinuousAxisTicks>().ElementAt(1);
                        yTicks.StartPoint = new double[] { yTicks.StartPoint[0] - 1.5, yTicks.StartPoint[1] };
                        yTicks.EndPoint = new double[] { yTicks.EndPoint[0] - 1.5, yTicks.EndPoint[1] };

                        ContinuousAxisLabels yLabels = plot.GetFirst<ContinuousAxisLabels>();
                        yLabels.StartPoint = new double[] { yLabels.StartPoint[0] - 1.5, yLabels.StartPoint[1] };
                        yLabels.EndPoint = new double[] { yLabels.EndPoint[0] - 1.5, yLabels.EndPoint[1] };

                        ContinuousAxisTitle yTitle = plot.GetAll<ContinuousAxisTitle>().ElementAt(1);
                        yTitle.StartPoint = yAxis.StartPoint;
                        yTitle.EndPoint = yAxis.EndPoint;

                    }
                    else
                    {
                        ContinuousAxis xAxis = plot.GetAll<ContinuousAxis>().ElementAt(1);
                        xAxis.StartPoint = new double[] { xAxis.StartPoint[0], xAxis.StartPoint[1] - 1.5 };

                        ContinuousAxis yAxis = plot.GetFirst<ContinuousAxis>();

                        yAxis.StartPoint = new double[] { yAxis.StartPoint[0], yAxis.StartPoint[1] - 1.5 };
                        yAxis.EndPoint = new double[] { yAxis.EndPoint[0], yAxis.EndPoint[1] - 1.5 };

                        Grid grid = plot.GetFirst<Grid>();
                        grid.Side2Start = new double[] { grid.Side2Start[0], grid.Side2Start[1] - 1.5 };
                        grid.Side2End = new double[] { grid.Side2End[0], grid.Side2End[1] - 1.5 };

                        ContinuousAxisTicks yTicks = plot.GetFirst<ContinuousAxisTicks>();
                        yTicks.StartPoint = new double[] { yTicks.StartPoint[0], yTicks.StartPoint[1] - 1.5 };
                        yTicks.EndPoint = new double[] { yTicks.EndPoint[0], yTicks.EndPoint[1] - 1.5 };

                        ContinuousAxisLabels yLabels = plot.GetFirst<ContinuousAxisLabels>();
                        yLabels.StartPoint = new double[] { yLabels.StartPoint[0], yLabels.StartPoint[1] - 1.5 };
                        yLabels.EndPoint = new double[] { yLabels.EndPoint[0], yLabels.EndPoint[1] - 1.5 };

                        ContinuousAxisTitle yTitle = plot.GetFirst<ContinuousAxisTitle>();
                        yTitle.StartPoint = yAxis.StartPoint;
                        yTitle.EndPoint = yAxis.EndPoint;
                    }
                }

                if (overflowCount > 0)
                {
                    if (vertical)
                    {
                        ContinuousAxis xAxis = plot.GetFirst<ContinuousAxis>();
                        xAxis.EndPoint = new double[] { xAxis.EndPoint[0] + 1.5, xAxis.EndPoint[1] };

                        Grid grid = plot.GetFirst<Grid>();
                        grid.Side2Start = new double[] { grid.Side2Start[0] + 1.5, grid.Side2Start[1] };
                        grid.Side2End = new double[] { grid.Side2End[0] + 1.5, grid.Side2End[1] };
                    }
                    else
                    {
                        ContinuousAxis xAxis = plot.GetAll<ContinuousAxis>().ElementAt(1);
                        xAxis.EndPoint = new double[] { xAxis.EndPoint[0], xAxis.EndPoint[1] + 1.5 };

                        Grid grid = plot.GetFirst<Grid>();
                        grid.Side1Start = new double[] { grid.Side1Start[0], grid.Side1Start[1] + 1.5 };
                        grid.Side1End = new double[] { grid.Side1End[0], grid.Side1End[1] + 1.5 };

                        TextLabel<IReadOnlyList<double>> titleLabel = plot.GetFirst<TextLabel<IReadOnlyList<double>>>();
                        titleLabel.Position = new double[] { titleLabel.Position[0], titleLabel.Position[1] + 1.5 };
                    }
                }

                if (overflowCount > 0 || underflowCount > 0)
                {
                    DataLabels<IReadOnlyList<double>> xLabels = plot.GetFirst<DataLabels<IReadOnlyList<double>>>();

                    List<IReadOnlyList<double>> xLabelData = xLabels.Data.ToList();

                    List<IReadOnlyList<double>> barData = new List<IReadOnlyList<double>>();

                    if (underflowCount > 0)
                    {
                        if (vertical)
                        {
                            xLabelData.Add(plot.GetFirst<Bars>().GetBaseline(new double[] { -1.5, 0 }));

                            barData.Add(new double[] { -1.5, underflowCount });
                            barData.Add(plot.GetFirst<Bars>().GetBaseline(new double[] { -0.5, 0 }));
                        }
                        else
                        {
                            xLabelData.Add(plot.GetFirst<Bars>().GetBaseline(new double[] { 0, -1.5 }));

                            barData.Add(new double[] { underflowCount, -1.5 });
                            barData.Add(plot.GetFirst<Bars>().GetBaseline(new double[] { 0, -0.5 }));
                        }
                    }

                    if (overflowCount > 0)
                    {
                        if (vertical)
                        {
                            xLabelData.Add(plot.GetFirst<Bars>().GetBaseline(new double[] { binCount + 0.5, 0 }));

                            barData.Add(plot.GetFirst<Bars>().GetBaseline(new double[] { binCount - 0.5, 0 }));
                            barData.Add(new double[] { binCount + 0.5, overflowCount });
                        }
                        else
                        {
                            xLabelData.Add(plot.GetFirst<Bars>().GetBaseline(new double[] { 0, binCount + 0.5 }));

                            barData.Add(plot.GetFirst<Bars>().GetBaseline(new double[] { 0, binCount - 0.5 }));
                            barData.Add(new double[] { overflowCount, binCount + 0.5 });
                        }
                    }

                    Bars overUnderBars = new Bars(barData, plot.GetFirst<ICoordinateSystem<IReadOnlyList<double>>>(), vertical) { PresentationAttributes = dataPresentationAttributes, GetBaseline = plot.GetFirst<Bars>().GetBaseline };

                    plot.AddPlotElement(overUnderBars);

                    Func<int, IReadOnlyList<double>, object> prevLabel = xLabels.Label;
                    Func<int, IReadOnlyList<double>, Point> prevMargin = xLabels.Margin;

                    if (vertical)
                    {
                        xLabels.Label = (i, x) =>
                        {
                            if (x[0] == -1.5)
                            {
                                return "(-∞, " + underflow.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";
                            }
                            else if (x[0] == binCount + 0.5)
                            {
                                return "[" + overflow.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", +∞)";
                            }
                            else
                            {
                                return prevLabel(i, x);
                            }
                        };

                        xLabels.Margin = (i, x) =>
                        {
                            if (x[0] == -1.5 || x[0] == binCount + 0.5)
                            {
                                return new Point(0, 20);
                            }
                            else
                            {
                                return prevMargin(i, x);
                            }
                        };
                    }
                    else
                    {
                        xLabels.Label = (i, x) =>
                        {
                            if (x[1] == -1.5)
                            {
                                return "(-∞, " + underflow.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";
                            }
                            else if (x[1] == binCount + 0.5)
                            {
                                return "[" + overflow.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", +∞)";
                            }
                            else
                            {
                                return prevLabel(i, x);
                            }
                        };

                        xLabels.Margin = (i, x) =>
                        {
                            if (x[1] == -1.5 || x[1] == binCount + 0.5)
                            {
                                return new Point(-20, 0);
                            }
                            else
                            {
                                return prevMargin(i, x);
                            }
                        };

                    }

                    xLabels.Data = xLabelData;

                    double ground = 0;

                    if (vertical)
                    {
                        ground = plot.GetFirst<ContinuousAxisTicks>().StartPoint[1];
                    }
                    else
                    {
                        ground = plot.GetAll<ContinuousAxisTicks>().ElementAt(1).StartPoint[0];
                    }

                    double[] tickStart = null;
                    double[] tickEnd = null;

                    if (overflowCount > 0 && underflowCount > 0)
                    {
                        if (vertical)
                        {
                            tickStart = new double[] { -1.5, ground };
                            tickEnd = new double[] { binCount + 0.5, ground };
                        }
                        else
                        {
                            tickStart = new double[] { ground, -1.5 };
                            tickEnd = new double[] { ground, binCount + 0.5 };
                        }
                    }
                    else if (overflowCount > 0)
                    {
                        if (vertical)
                        {
                            tickStart = new double[] { binCount + 0.5, ground };
                            tickEnd = new double[] { binCount + 0.50000001, ground };
                        }
                        else
                        {
                            tickStart = new double[] { ground, binCount + 0.5 };
                            tickEnd = new double[] { ground, binCount + 0.50000001 };
                        }
                    }
                    else if (underflowCount > 0)
                    {
                        if (vertical)
                        {
                            tickStart = new double[] { -1.5, ground };
                            tickEnd = new double[] { -1.50000001, ground };
                        }
                        else
                        {
                            tickStart = new double[] { ground, -1.5 };
                            tickEnd = new double[] { ground, -1.50000001 };
                        }
                    }

                    ContinuousAxisTicks overUnderTicks = new ContinuousAxisTicks(tickStart, tickEnd, plot.GetFirst<IContinuousCoordinateSystem>()) { IntervalCount = 1, SizeAbove = i => 3, SizeBelow = i => 3 };

                    plot.AddPlotElement(overUnderTicks);
                }

                return plot;
            }

            private static Plot StackedHistogram(IReadOnlyList<IReadOnlyList<double>> data, bool vertical = true, double margin = 0.25, double width = 350, double height = 250,
            PlotElementPresentationAttributes axisPresentationAttributes = null,
            double axisArrowSize = 3,
            PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
            PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
            string xAxisTitle = null,
            string yAxisTitle = null,
            string title = null,
            PlotElementPresentationAttributes titlePresentationAttributes = null,
            PlotElementPresentationAttributes gridPresentationAttributes = null,
            IReadOnlyList<PlotElementPresentationAttributes> dataPresentationAttributes = null,
            IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                if (axisPresentationAttributes == null)
                {
                    axisPresentationAttributes = new PlotElementPresentationAttributes();
                }

                if (gridPresentationAttributes == null)
                {
                    gridPresentationAttributes = new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(220, 220, 220)) };
                }

                if (axisLabelPresentationAttributes == null)
                {
                    axisLabelPresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null };
                }

                if (axisTitlePresentationAttributes == null)
                {
                    axisTitlePresentationAttributes = new PlotElementPresentationAttributes() { Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 14), Stroke = null };
                }

                if (dataPresentationAttributes == null)
                {
                    dataPresentationAttributes = new PlotElementPresentationAttributes[] { new PlotElementPresentationAttributes() { Stroke = null, Fill = new SolidColourBrush(Colour.FromRgb(0, 114, 178)) },
                                                                                       new PlotElementPresentationAttributes() { Stroke = null, Fill = new SolidColourBrush(Colour.FromRgb(213, 94, 0)) },
                                                                                       new PlotElementPresentationAttributes() { Stroke = null, Fill = new SolidColourBrush(Colour.FromRgb(204, 121, 167)) },
                                                                                       new PlotElementPresentationAttributes() { Stroke = null, Fill = new SolidColourBrush(Colour.FromRgb(230, 159, 0)) },
                                                                                       new PlotElementPresentationAttributes() { Stroke = null, Fill = new SolidColourBrush(Colour.FromRgb(86, 180, 233)) },
                                                                                       new PlotElementPresentationAttributes() { Stroke = null, Fill = new SolidColourBrush(Colour.FromRgb(0, 158, 115)) },
                                                                                       new PlotElementPresentationAttributes() { Stroke = null, Fill = new SolidColourBrush(Colour.FromRgb(240, 228, 66)) } };
                }

                double baselineValue = 0;

                if (coordinateSystem is LogarithmicCoordinateSystem2D || (!vertical && coordinateSystem is LinLogCoordinateSystem2D) || (vertical && coordinateSystem is LogLinCoordinateSystem2D))
                {
                    baselineValue = 1;
                }

                StackedBars bars;

                if (baselineValue == 0)
                {
                    bars = new StackedBars(data, coordinateSystem, vertical) { PresentationAttributes = dataPresentationAttributes, Margin = margin };
                }
                else
                {
                    bars = new StackedBars(data, new Comparison<IReadOnlyList<double>>(vertical ? new Func<IReadOnlyList<double>, IReadOnlyList<double>, int>((x, y) => Math.Sign(x[0] - y[0])) : (x, y) => Math.Sign(x[1] - y[1])), vertical ? new Func<IReadOnlyList<double>, IReadOnlyList<double>>(pt => new double[] { pt[0], baselineValue }) : pt => new double[] { baselineValue, pt[1] }, coordinateSystem) { PresentationAttributes = dataPresentationAttributes, Margin = margin, Vertical = vertical };
                }

                double[][] actualData = new double[data.Count][];

                for (int i = 0; i < data.Count; i++)
                {
                    if (vertical)
                    {
                        double sum = 0;

                        for (int j = 1; j < data[i].Count; j++)
                        {
                            sum += data[i][j];
                        }

                        actualData[i] = new double[] { data[i][0], sum };
                    }
                    else
                    {
                        double sum = data[i][0];

                        for (int j = 2; j < data[i].Count; j++)
                        {
                            sum += data[i][j];
                        }

                        actualData[i] = new double[] { sum, data[i][1] };
                    }
                }


                (double minX, double minY, double maxX, double maxY, double rangeX, double rangeY) = GetDataRange(actualData);

                double dataMinX = minX;
                double dataMinY = minY;
                double dataMaxX = maxX;
                double dataMaxY = maxY;

                if (coordinateSystem == null)
                {
                    LinearCoordinateSystem2D linCoords = new LinearCoordinateSystem2D(data, width, height);

                    if (vertical)
                    {
                        minY = Math.Min(minY, 0);
                        maxY = Math.Max(maxY, 0);
                        rangeY = maxY - minY;

                        linCoords.MinY = minY - rangeY * 0.1;
                        linCoords.MaxY = maxY + rangeY * 0.1;


                        if (bars.Data.Count >= 2)
                        {
                            IReadOnlyList<double> item0 = bars.Data.ElementAt(0);
                            IReadOnlyList<double> item1 = bars.Data.ElementAt(1);

                            IReadOnlyList<double> itemN = bars.Data.ElementAt(bars.Data.Count - 1);
                            IReadOnlyList<double> itemN1 = bars.Data.ElementAt(bars.Data.Count - 2);

                            minX = Math.Min(minX, 1.5 * item0[0] - item1[0] * 0.5);
                            maxX = Math.Max(maxX, 1.5 * itemN[0] - itemN1[0] * 0.5);

                            rangeX = maxX - minX;
                            linCoords.MinX = minX;
                            linCoords.MaxX = maxX;
                        }
                    }
                    else
                    {
                        minX = Math.Min(minX, 0);
                        maxX = Math.Max(maxX, 0);
                        rangeX = maxX - minX;

                        linCoords.MinX = minX - rangeX * 0.1;
                        linCoords.MaxX = maxX + rangeX * 0.1;

                        if (bars.Data.Count >= 2)
                        {
                            IReadOnlyList<double> item0 = bars.Data.ElementAt(0);
                            IReadOnlyList<double> item1 = bars.Data.ElementAt(1);

                            IReadOnlyList<double> itemN = bars.Data.ElementAt(bars.Data.Count - 1);
                            IReadOnlyList<double> itemN1 = bars.Data.ElementAt(bars.Data.Count - 2);

                            minY = Math.Min(minY, 1.5 * item0[1] - item1[1] * 0.5);
                            maxY = Math.Max(maxY, 1.5 * itemN[1] - itemN1[1] * 0.5);

                            rangeY = maxY - minY;
                            linCoords.MinY = minY;
                            linCoords.MaxY = maxY;
                        }
                    }

                    coordinateSystem = linCoords;
                    bars.CoordinateSystem = coordinateSystem;
                }
                else
                {
                    if (vertical)
                    {
                        minY = Math.Min(minY, baselineValue);
                        rangeY = maxY - minY;

                        if (bars.Data.Count >= 2)
                        {
                            IReadOnlyList<double> item0 = bars.Data.ElementAt(0);
                            IReadOnlyList<double> item1 = bars.Data.ElementAt(1);

                            IReadOnlyList<double> itemN = bars.Data.ElementAt(bars.Data.Count - 1);
                            IReadOnlyList<double> itemN1 = bars.Data.ElementAt(bars.Data.Count - 2);

                            minX = Math.Min(minX, 1.5 * item0[0] - item1[0] * 0.5);
                            maxX = Math.Max(maxX, 1.5 * itemN[0] - itemN1[0] * 0.5);

                            rangeX = maxX - minX;
                        }
                    }
                    else
                    {
                        minX = Math.Min(minX, baselineValue);
                        rangeX = maxX - minX;

                        if (bars.Data.Count >= 2)
                        {
                            IReadOnlyList<double> item0 = bars.Data.ElementAt(0);
                            IReadOnlyList<double> item1 = bars.Data.ElementAt(1);

                            IReadOnlyList<double> itemN = bars.Data.ElementAt(bars.Data.Count - 1);
                            IReadOnlyList<double> itemN1 = bars.Data.ElementAt(bars.Data.Count - 2);

                            minY = Math.Min(minY, 1.5 * item0[1] - item1[1] * 0.5);
                            maxY = Math.Max(maxY, 1.5 * itemN[1] - itemN1[1] * 0.5);

                            rangeY = maxY - minY;
                        }
                    }
                }



                if (titlePresentationAttributes == null)
                {
                    titlePresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 18) };
                }

                Point topLeft = coordinateSystem.ToPlotCoordinates(new double[] { minX, maxY });
                Point topRight = coordinateSystem.ToPlotCoordinates(new double[] { maxX, maxY });
                Point bottomRight = coordinateSystem.ToPlotCoordinates(new double[] { maxX, minY });
                Point bottomLeft = coordinateSystem.ToPlotCoordinates(new double[] { minX, minY });

                double[] marginTopLeft = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10));
                double[] marginTopRight = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10));
                double[] marginBottomRight = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10));
                double[] marginBottomLeft = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10));

                double[] p1 = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)), Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10));
                double[] p2 = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)), Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10));
                double[] p3 = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)), Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10));
                double[] p4 = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)), Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10));


                double[] p5 = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y))));
                double[] p6 = coordinateSystem.ToDataCoordinates(new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y))));
                double[] p7 = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y))));
                double[] p8 = coordinateSystem.ToDataCoordinates(new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y))));

                if (vertical)
                {
                    Point p1p = new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)), Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10);
                    Point p1p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMinX, dataMaxY });

                    Point p2p = new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)), Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)) - 10);
                    Point p2p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMaxX, dataMaxY });

                    Point p3p = new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)), Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10);
                    Point p3p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMinX, 0 });

                    Point p4p = new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)), Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) + 10);
                    Point p4p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMaxX, 0 });

                    p1 = coordinateSystem.ToDataCoordinates(new Point(p1p2.X, p1p.Y));
                    p2 = coordinateSystem.ToDataCoordinates(new Point(p2p2.X, p2p.Y));
                    p3 = coordinateSystem.ToDataCoordinates(new Point(p3p2.X, p3p.Y));
                    p4 = coordinateSystem.ToDataCoordinates(new Point(p4p2.X, p4p.Y));
                }
                else
                {
                    Point p5p = new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)));
                    Point p5p2 = coordinateSystem.ToPlotCoordinates(new double[] { 0, dataMaxY });

                    Point p6p = new Point(Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)) - 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)));
                    Point p6p2 = coordinateSystem.ToPlotCoordinates(new double[] { 0, dataMinY });

                    Point p7p = new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)));
                    Point p7p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMaxX, dataMaxY });

                    Point p8p = new Point(Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) + 10, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)));
                    Point p8p2 = coordinateSystem.ToPlotCoordinates(new double[] { dataMinX, dataMinY });

                    p5 = coordinateSystem.ToDataCoordinates(new Point(p5p.X, p5p2.Y));
                    p6 = coordinateSystem.ToDataCoordinates(new Point(p6p.X, p6p2.Y));
                    p7 = coordinateSystem.ToDataCoordinates(new Point(p7p2.X, p7p.Y));
                    p8 = coordinateSystem.ToDataCoordinates(new Point(p8p2.X, p8p.Y));
                }

                Grid grid;

                if (vertical)
                {
                    grid = new Grid(p5, p6, p7, p8, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };
                }
                else
                {
                    grid = new Grid(p1, p2, p3, p4, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };
                }

                ContinuousAxis xAxis = new ContinuousAxis(marginBottomLeft[0] < marginBottomRight[0] ? marginBottomLeft : marginBottomRight, marginBottomLeft[0] < marginBottomRight[0] ? marginBottomRight : vertical ? marginBottomLeft : coordinateSystem.ToDataCoordinates(coordinateSystem.ToPlotCoordinates(marginBottomLeft) + new Point(-axisArrowSize - 7, 0)), coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = axisArrowSize };
                ContinuousAxis yAxis = new ContinuousAxis(marginBottomLeft[1] < marginTopLeft[1] ? marginBottomLeft : marginTopLeft, marginBottomLeft[1] < marginTopLeft[1] ? marginTopLeft : !vertical ? marginBottomLeft : coordinateSystem.ToDataCoordinates(coordinateSystem.ToPlotCoordinates(marginBottomLeft) + new Point(0, axisArrowSize + 7)), coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = axisArrowSize };

                int every = (int)Math.Ceiling((double)data.Count / 5);
                int shift = (data.Count - every * (data.Count / every - 1) - 1) / 2;

                ContinuousAxisTicks xTicks;
                ContinuousAxisTicks yTicks;

                if (vertical)
                {
                    xTicks = new ContinuousAxisTicks(p3, p4, coordinateSystem) { PresentationAttributes = axisPresentationAttributes, IntervalCount = data.Count - 1, SizeAbove = i => (i - shift) % every == 0 ? 3 : 2, SizeBelow = i => (i - shift) % every == 0 ? 3 : 2 };
                    yTicks = new ContinuousAxisTicks(p6, p5, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };
                }
                else
                {
                    xTicks = new ContinuousAxisTicks(p3, p4, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };
                    yTicks = new ContinuousAxisTicks(p6, p5, coordinateSystem) { PresentationAttributes = axisPresentationAttributes, IntervalCount = data.Count - 1, SizeAbove = i => (i - shift) % every == 0 ? 3 : 2, SizeBelow = i => (i - shift) % every == 0 ? 3 : 2 };
                }

                ContinuousAxisLabels xLabels;
                ContinuousAxisLabels yLabels;

                if (vertical)
                {
                    xLabels = new ContinuousAxisLabels(p3, p4, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Alignment = TextAnchors.Center, Baseline = TextBaselines.Top, Rotation = 0, IntervalCount = data.Count - 1 };
                    yLabels = new ContinuousAxisLabels(p6, p5, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Position = _ => -10, Alignment = TextAnchors.Right, Rotation = 0, IntervalCount = 5 };

                    Func<IReadOnlyList<double>, int, IEnumerable<FormattedText>> originalFormatter = xLabels.TextFormat;
                    xLabels.TextFormat = (x, i) => (i - shift) % every == 0 ? originalFormatter(x, i) : null;
                }
                else
                {
                    xLabels = new ContinuousAxisLabels(p3, p4, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Alignment = TextAnchors.Center, Baseline = TextBaselines.Top, Rotation = 0, IntervalCount = 5 };
                    yLabels = new ContinuousAxisLabels(p6, p5, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Position = _ => -10, Alignment = TextAnchors.Right, Rotation = 0, IntervalCount = data.Count - 1 };

                    Func<IReadOnlyList<double>, int, IEnumerable<FormattedText>> originalFormatter = yLabels.TextFormat;
                    yLabels.TextFormat = (x, i) => (i - shift) % every == 0 ? originalFormatter(x, i) : null;
                }

                Graphics xLabelsSize = new Graphics();
                xLabels.Plot(xLabelsSize);
                double xLabelsHeight = xLabelsSize.GetBounds().Size.Height;

                Graphics yLabelsSize = new Graphics();
                yLabels.Plot(yLabelsSize);
                double yLabelsWidth = yLabelsSize.GetBounds().Size.Width;

                ContinuousAxisTitle xTitle = new ContinuousAxisTitle(xAxisTitle, marginBottomLeft, marginBottomRight, coordinateSystem, axisTitlePresentationAttributes) { Position = xLabelsHeight + 20, Alignment = TextAnchors.Center };
                ContinuousAxisTitle yTitle = new ContinuousAxisTitle(yAxisTitle, marginBottomLeft, marginTopLeft, coordinateSystem, axisTitlePresentationAttributes) { Position = -20 - yLabelsWidth, Baseline = TextBaselines.Bottom, Alignment = TextAnchors.Center };

                TextLabel<IReadOnlyList<double>> titleLabel = new TextLabel<IReadOnlyList<double>>(title, coordinateSystem.ToDataCoordinates((coordinateSystem.ToPlotCoordinates(marginTopLeft) + coordinateSystem.ToPlotCoordinates(marginTopRight)) * 0.5 + new Point(0, -10)), coordinateSystem) { Baseline = TextBaselines.Bottom, PresentationAttributes = titlePresentationAttributes };

                Plot tbr = new Plot();
                tbr.AddPlotElements(grid, xAxis, yAxis, xTicks, yTicks, xLabels, yLabels, xTitle, yTitle, bars, titleLabel);

                return tbr;
            }

            /// <summary>
            /// Create a new distribution plot.
            /// </summary>
            /// <param name="data">The data whose distribution will be plotted.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the distribution goes from the X axis up to the sampled values. If this is <see langword="false"/>, the distribution goes from the Y axis to the sampled values.</param>
            /// <param name="binCount">The number of bins to use. If this is &lt; 2, the number of bins is determined automatically using the Freedman-Diaconis rule.</param>
            /// <param name="smooth">If this is <see langword="false"/> (the default), the values are joined by a polyline. If this is <see langword="true"/>, the values are joined by a smooth spline passing through all of them.</param>
            /// <param name="width">The width of the plot.</param>
            /// <param name="height">The height of the plot.</param>
            /// <param name="axisPresentationAttributes">Presentation attributes for the axes.</param>
            /// <param name="axisArrowSize">Size of the arrow at the end of each axis.</param>
            /// <param name="axisLabelPresentationAttributes">Presentation attributes for the axis labels.</param>
            /// <param name="axisTitlePresentationAttributes">Presentation attributes for the axis titles.</param>
            /// <param name="xAxisTitle">Title for the X axis.</param>
            /// <param name="yAxisTitle">Title for the Y axis.</param>
            /// <param name="title">Title for the plot.</param>
            /// <param name="titlePresentationAttributes">Presentation attributes for the plot title.</param>
            /// <param name="gridPresentationAttributes">Presentation attributes for the grid.</param>
            /// <param name="dataPresentationAttributes">Presentation attributes for the plotted data.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the distribution plot.</returns>
            public static Plot Distribution(IReadOnlyList<double> data, bool vertical = true, int binCount = -1, bool smooth = false, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                PlotElementPresentationAttributes dataPresentationAttributes = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                if (dataPresentationAttributes == null)
                {
                    dataPresentationAttributes = new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(0, 114, 178)), LineWidth = 2, Fill = new SolidColourBrush(Colour.FromRgb(0, 114, 178).WithAlpha(0.5)) };
                }

                double min = double.MaxValue;
                double max = double.MinValue;

                for (int i = 0; i < data.Count; i++)
                {
                    min = Math.Min(min, data[i]);
                    max = Math.Max(max, data[i]);
                }

                if (binCount <= 1)
                {
                    (double _, double _, double iqr) = IQR(data);
                    double h2 = 2 * iqr / Math.Pow(data.Count, 1.0 / 3.0);

                    if (h2 > 0)
                    {
                        binCount = Math.Max(2, (int)Math.Ceiling((max - min) / h2));
                    }
                    else
                    {
                        binCount = 2;
                    }
                }

                int[] bins = new int[binCount];

                if (max > min)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        int index = (int)Math.Min(binCount - 1, Math.Floor((data[i] - min) / (max - min) * binCount));

                        bins[index]++;
                    }
                }
                else
                {
                    bins[0] = data.Count;
                }

                double[][] binnedData = new double[bins.Length][];

                for (int i = 0; i < bins.Length; i++)
                {
                    double binStart = min + (max - min) / binCount * i;
                    double binEnd = min + (max - min) / binCount * (i + 1);

                    if (vertical)
                    {
                        binnedData[i] = new double[] { (binStart + binEnd) * 0.5, bins[i] };
                    }
                    else
                    {
                        binnedData[i] = new double[] { bins[i], (binStart + binEnd) * 0.5 };
                    }
                }

                Plot plot = Create.AreaChart(binnedData, vertical, smooth, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, coordinateSystem);

                return plot;
            }

            /// <summary>
            /// Create a new stacked distribution plot.
            /// </summary>
            /// <param name="data">The data whose distribution will be plotted.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the distributions go from the X axis up to the sampled values. If this is <see langword="false"/>, the distributions go from the Y axis to the sampled values.</param>
            /// <param name="binCount">The number of bins to use. If this is &lt; 2, the number of bins is determined automatically using the Freedman-Diaconis rule.</param>
            /// <param name="smooth">If this is <see langword="false"/> (the default), the values are joined by a polyline. If this is <see langword="true"/>, the values are joined by a smooth spline passing through all of them.</param>
            /// <param name="normalisationMode">The kind of normalisation to use to make the distributions comparable.</param>
            /// <param name="width">The width of the plot.</param>
            /// <param name="height">The height of the plot.</param>
            /// <param name="axisPresentationAttributes">Presentation attributes for the axes.</param>
            /// <param name="axisArrowSize">Size of the arrow at the end of each axis.</param>
            /// <param name="axisLabelPresentationAttributes">Presentation attributes for the axis labels.</param>
            /// <param name="axisTitlePresentationAttributes">Presentation attributes for the axis titles.</param>
            /// <param name="xAxisTitle">Title for the X axis.</param>
            /// <param name="yAxisTitle">Title for the Y axis.</param>
            /// <param name="title">Title for the plot.</param>
            /// <param name="titlePresentationAttributes">Presentation attributes for the plot title.</param>
            /// <param name="gridPresentationAttributes">Presentation attributes for the grid.</param>
            /// <param name="dataPresentationAttributes">Presentation attributes for the plotted data.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the stacked distribution plot.</returns>
            public static Plot StackedDistribution(IReadOnlyList<IReadOnlyList<double>> data, bool vertical = true, int binCount = -1, bool smooth = false, NormalisationMode normalisationMode = NormalisationMode.Area, double width = 350, double height = 250,
               PlotElementPresentationAttributes axisPresentationAttributes = null,
               double axisArrowSize = 3,
               PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
               PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
               string xAxisTitle = null,
               string yAxisTitle = null,
               string title = null,
               PlotElementPresentationAttributes titlePresentationAttributes = null,
               PlotElementPresentationAttributes gridPresentationAttributes = null,
               IReadOnlyList<PlotElementPresentationAttributes> dataPresentationAttributes = null,
               IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                if (dataPresentationAttributes == null)
                {
                    dataPresentationAttributes = new PlotElementPresentationAttributes[] { new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(0, 114, 178).WithAlpha(0.5)), Stroke = new SolidColourBrush(Colour.FromRgb(0, 114, 178)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(213, 94, 0).WithAlpha(0.5)), Stroke = new SolidColourBrush(Colour.FromRgb(213, 94, 0)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(204, 121, 167).WithAlpha(0.5)), Stroke = new SolidColourBrush(Colour.FromRgb(204, 121, 167)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(230, 159, 0).WithAlpha(0.5)), Stroke = new SolidColourBrush(Colour.FromRgb(230, 159, 0)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(86, 180, 233).WithAlpha(0.5)), Stroke = new SolidColourBrush(Colour.FromRgb(86, 180, 233)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(0, 158, 115).WithAlpha(0.5)), Stroke = new SolidColourBrush(Colour.FromRgb(0, 158, 115)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(240, 228, 66).WithAlpha(0.5)), Stroke = new SolidColourBrush(Colour.FromRgb(240, 228, 66)) } };
                }

                bool wasCoordinateSystemNull = coordinateSystem == null;

                double[][][] allBinnedData = new double[data.Count][][];

                double overallMinX = double.MaxValue;
                double overallMaxX = double.MinValue;

                double overallMaxY = double.MinValue;

                double minX0 = double.MaxValue;
                double maxX0 = double.MinValue;
                double maxY0 = double.MaxValue;

                for (int j = 0; j < data.Count; j++)
                {

                    double min = double.MaxValue;
                    double max = double.MinValue;

                    for (int i = 0; i < data[j].Count; i++)
                    {
                        min = Math.Min(min, data[j][i]);
                        max = Math.Max(max, data[j][i]);
                    }

                    if (binCount <= 1)
                    {
                        (double _, double _, double iqr) = IQR(data[j]);
                        double h2 = 2 * iqr / Math.Pow(data[j].Count, 1.0 / 3.0);

                        if (h2 > 0)
                        {
                            binCount = Math.Max(2, (int)Math.Ceiling((max - min) / h2));
                        }
                        else
                        {
                            binCount = 2;
                        }
                    }

                    int[] bins = new int[binCount];

                    if (max > min)
                    {
                        for (int i = 0; i < data[j].Count; i++)
                        {
                            int index = (int)Math.Min(binCount - 1, Math.Floor((data[j][i] - min) / (max - min) * binCount));

                            bins[index]++;
                        }
                    }
                    else
                    {
                        bins[0] = data[j].Count;
                    }

                    double[][] binnedData = new double[bins.Length][];

                    double maxBin = 1;

                    if (normalisationMode == NormalisationMode.Maximum)
                    {
                        maxBin = bins.Max();
                    }
                    else if (normalisationMode == NormalisationMode.None)
                    {
                        maxBin = 1;
                    }
                    else if (normalisationMode == NormalisationMode.Area)
                    {
                        maxBin = (max - min) / binCount * data[j].Count;
                    }

                    for (int i = 0; i < bins.Length; i++)
                    {
                        double binStart = min + (max - min) / binCount * i;
                        double binEnd = min + (max - min) / binCount * (i + 1);

                        if (vertical)
                        {
                            binnedData[i] = new double[] { (binStart + binEnd) * 0.5, bins[i] / maxBin };
                        }
                        else
                        {
                            binnedData[i] = new double[] { bins[i] / maxBin, (binStart + binEnd) * 0.5 };
                        }

                        overallMaxY = Math.Max(overallMaxY, bins[i] / maxBin);
                        overallMaxX = Math.Max(overallMaxX, (binStart + binEnd) * 0.5);
                        overallMinX = Math.Min(overallMinX, (binStart + binEnd) * 0.5);
                    }

                    if (j == 0)
                    {
                        maxX0 = overallMaxX;
                        minX0 = overallMinX;
                        maxY0 = overallMaxY;
                    }

                    allBinnedData[j] = binnedData;
                }

                List<double[]> joinedBinnedData = new List<double[]>((from el in allBinnedData select el.Length).Sum());

                for (int i = 0; i < allBinnedData.Length; i++)
                {
                    joinedBinnedData.AddRange(allBinnedData[i]);
                }

                Plot plot = Create.AreaChart(joinedBinnedData, vertical, smooth, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes[0], coordinateSystem);

                Func<IReadOnlyList<double>, IReadOnlyList<double>> getBaseline = plot.GetFirst<Area<IReadOnlyList<double>>>().GetBaseline;

                plot.RemovePlotElement(plot.GetFirst<Area<IReadOnlyList<double>>>());

                ICoordinateSystem<IReadOnlyList<double>> currCoordinateSystem = plot.GetFirst<ICoordinateSystem<IReadOnlyList<double>>>();

                for (int i = 0; i < allBinnedData.Length; i++)
                {
                    Area<IReadOnlyList<double>> area = new Area<IReadOnlyList<double>>(allBinnedData[i], getBaseline, currCoordinateSystem) { Smooth = smooth, PresentationAttributes = dataPresentationAttributes[i % dataPresentationAttributes.Count] };
                    plot.AddPlotElement(area);
                }

                return plot;
            }
        }
    }
}
