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
        partial class Create
        {
            /// <summary>
            /// Create a new area chart.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the highlighted area goes from the X axis up to the sampled values. If this is <see langword="false"/>, the highlighted area goes from the Y axis to the sampled values.</param>
            /// <param name="smooth">If this is <see langword="false"/> (the default), the sampled values are joined by a polyline. If this is <see langword="true"/>, they are joined by a smooth spline passing through them.</param>
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
            /// <returns>A <see cref="Plot"/> containing the area chart.</returns>
            public static Plot AreaChart(IReadOnlyList<IReadOnlyList<double>> data, bool vertical = true, bool smooth = false, double width = 350, double height = 250,
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
                    dataPresentationAttributes = new PlotElementPresentationAttributes()
                    {
                        Fill = new SolidColourBrush(Colour.FromRgb(197, 235, 255)),
                        Stroke = new SolidColourBrush(Colour.FromRgb(0, 114, 178))
                    };
                }

                if (titlePresentationAttributes == null)
                {
                    titlePresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 18) };
                }

                double baselineValue = 0;

                if (coordinateSystem is LogarithmicCoordinateSystem2D || (!vertical && coordinateSystem is LinLogCoordinateSystem2D) || (vertical && coordinateSystem is LogLinCoordinateSystem2D))
                {
                    baselineValue = 1;
                }

                Func<IReadOnlyList<double>, IReadOnlyList<double>> getBaseline;

                if (vertical)
                {
                    getBaseline = x => new double[] { x[0], baselineValue };
                }
                else
                {
                    getBaseline = x => new double[] { baselineValue, x[1] };
                }

                IReadOnlyList<double>[] allData = new IReadOnlyList<double>[data.Count * 2];

                for (int i = 0; i < data.Count; i++)
                {
                    allData[i] = data[i];
                    allData[i + data.Count] = getBaseline(data[i]);
                }

                (double minX, double minY, double maxX, double maxY, double rangeX, double rangeY) = GetDataRange(allData);

                if (coordinateSystem == null)
                {
                    coordinateSystem = new LinearCoordinateSystem2D(allData, width, height);
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

                Grid xGrid = new Grid(p1, p2, p3, p4, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };
                Grid yGrid = new Grid(p5, p6, p7, p8, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };

                ContinuousAxis xAxis = new ContinuousAxis(marginBottomLeft, marginBottomRight, coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = axisArrowSize };
                ContinuousAxis yAxis = new ContinuousAxis(marginBottomLeft, marginTopLeft, coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = axisArrowSize };

                ContinuousAxisTicks xTicks = new ContinuousAxisTicks(p3, p4, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };
                ContinuousAxisTicks yTicks = new ContinuousAxisTicks(p6, p5, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };

                ContinuousAxisLabels xLabels = new ContinuousAxisLabels(p3, p4, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Alignment = TextAnchors.Center, Baseline = TextBaselines.Top, Rotation = 0, IntervalCount = 5 };
                ContinuousAxisLabels yLabels = new ContinuousAxisLabels(p6, p5, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Position = _ => -10, Alignment = TextAnchors.Right, Rotation = 0, IntervalCount = 5 };

                Graphics xLabelsSize = new Graphics();
                xLabels.Plot(xLabelsSize);
                double xLabelsHeight = xLabelsSize.GetBounds().Size.Height;

                Graphics yLabelsSize = new Graphics();
                yLabels.Plot(yLabelsSize);
                double yLabelsWidth = yLabelsSize.GetBounds().Size.Width;

                ContinuousAxisTitle xTitle = new ContinuousAxisTitle(xAxisTitle, marginBottomLeft, marginBottomRight, coordinateSystem, axisTitlePresentationAttributes) { Position = xLabelsHeight + 20, Alignment = TextAnchors.Center };
                ContinuousAxisTitle yTitle = new ContinuousAxisTitle(yAxisTitle, marginBottomLeft, marginTopLeft, coordinateSystem, axisTitlePresentationAttributes) { Position = -20 - yLabelsWidth, Baseline = TextBaselines.Bottom, Alignment = TextAnchors.Center };

                Area<IReadOnlyList<double>> area = new Area<IReadOnlyList<double>>(data, getBaseline, coordinateSystem) { PresentationAttributes = dataPresentationAttributes, Smooth = smooth };

                TextLabel<IReadOnlyList<double>> titleLabel = new TextLabel<IReadOnlyList<double>>(title, coordinateSystem.ToDataCoordinates(new Point((topLeft.X + topRight.X) * 0.5, (topLeft.Y + topRight.Y) * 0.5 - 20)), coordinateSystem) { Baseline = TextBaselines.Bottom, PresentationAttributes = titlePresentationAttributes };

                Plot tbr = new Plot();
                tbr.AddPlotElements(xGrid, yGrid, xAxis, yAxis, xTicks, yTicks, xLabels, yLabels, xTitle, yTitle, area, titleLabel);

                return tbr;
            }


            /// <summary>
            /// Create a new stacked area chart.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the highlighted area goes from the X axis up to the sampled values. If this is <see langword="false"/>, the highlighted area goes from the Y axis to the sampled values.</param>
            /// <param name="smooth">If this is <see langword="false"/> (the default), the sampled values are joined by a polyline. If this is <see langword="true"/>, they are joined by a smooth spline passing through them.</param>
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
            /// <returns>A <see cref="Plot"/> containing the stacked area chart.</returns>
            public static Plot StackedAreaChart(IReadOnlyList<IReadOnlyList<double>> data, bool vertical = true, bool smooth = false, double width = 350, double height = 250,
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
                    dataPresentationAttributes = new PlotElementPresentationAttributes[] { new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(197, 235, 255)), Stroke = new SolidColourBrush(Colour.FromRgb(0, 114, 178)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 233, 218)), Stroke = new SolidColourBrush(Colour.FromRgb(213, 94, 0)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 222, 240)), Stroke = new SolidColourBrush(Colour.FromRgb(204, 121, 167)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 242, 216)), Stroke = new SolidColourBrush(Colour.FromRgb(230, 159, 0)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(214, 241, 255)), Stroke = new SolidColourBrush(Colour.FromRgb(86, 180, 233)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(203, 255, 239)), Stroke = new SolidColourBrush(Colour.FromRgb(0, 158, 115)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 249, 189)), Stroke = new SolidColourBrush(Colour.FromRgb(240, 228, 66)) } };
                }

                if (titlePresentationAttributes == null)
                {
                    titlePresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 18) };
                }

                double baselineValue = 0;

                if (coordinateSystem is LogarithmicCoordinateSystem2D || (!vertical && coordinateSystem is LinLogCoordinateSystem2D) || (vertical && coordinateSystem is LogLinCoordinateSystem2D))
                {
                    baselineValue = 1;
                }

                Func<IReadOnlyList<double>, IReadOnlyList<double>> getBaseline;

                if (vertical)
                {
                    getBaseline = x => new double[] { x[0], baselineValue };
                }
                else
                {
                    getBaseline = x => new double[] { baselineValue, x[1] };
                }

                List<IReadOnlyList<double>> allData = new List<IReadOnlyList<double>>(data.Count * data[0].Count);

                allData.AddRange(data);

                List<List<IReadOnlyList<double>>> groupedData = new List<List<IReadOnlyList<double>>>(data[0].Count - 1);

                List<Dictionary<double, double>> baselines = new List<Dictionary<double, double>>(data[0].Count - 2);

                for (int i = 0; i < data.Count; i++)
                {
                    allData.Add(getBaseline(data[i]));
                }

                for (int i = 0; i < data[0].Count - 1; i++)
                {
                    if (vertical)
                    {
                        groupedData.Add(new List<IReadOnlyList<double>>(from el in data select new double[] { el[0], el[i + 1] }));

                        if (i > 0)
                        {
                            Dictionary<double, double> baseline = new Dictionary<double, double>(data.Count);

                            for (int j = 0; j < data.Count; j++)
                            {
                                baseline[data[j][0]] = data[j][i];
                            }

                            baselines.Add(baseline);
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            groupedData.Add(new List<IReadOnlyList<double>>(from el in data select new double[] { el[0], el[1] }));
                        }
                        else
                        {
                            groupedData.Add(new List<IReadOnlyList<double>>(from el in data select new double[] { el[i + 1], el[1] }));

                            Dictionary<double, double> baseline = new Dictionary<double, double>(data.Count);

                            if (i == 1)
                            {
                                for (int j = 0; j < data.Count; j++)
                                {
                                    baseline[data[j][1]] = data[j][0];
                                }
                            }
                            else
                            {
                                for (int j = 0; j < data.Count; j++)
                                {
                                    baseline[data[j][1]] = data[j][i];
                                }
                            }

                            baselines.Add(baseline);
                        }
                    }

                    allData.AddRange(groupedData[groupedData.Count - 1]);
                }

                (double minX, double minY, double maxX, double maxY, double rangeX, double rangeY) = GetDataRange(allData);

                if (coordinateSystem == null)
                {
                    coordinateSystem = new LinearCoordinateSystem2D(allData, width, height);
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

                Grid xGrid = new Grid(p1, p2, p3, p4, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };
                Grid yGrid = new Grid(p5, p6, p7, p8, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };

                ContinuousAxis xAxis = new ContinuousAxis(marginBottomLeft, marginBottomRight, coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = axisArrowSize };
                ContinuousAxis yAxis = new ContinuousAxis(marginBottomLeft, marginTopLeft, coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = axisArrowSize };

                ContinuousAxisTicks xTicks = new ContinuousAxisTicks(p3, p4, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };
                ContinuousAxisTicks yTicks = new ContinuousAxisTicks(p6, p5, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };

                ContinuousAxisLabels xLabels = new ContinuousAxisLabels(p3, p4, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Alignment = TextAnchors.Center, Baseline = TextBaselines.Top, Rotation = 0, IntervalCount = 5 };
                ContinuousAxisLabels yLabels = new ContinuousAxisLabels(p6, p5, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Position = _ => -10, Alignment = TextAnchors.Right, Rotation = 0, IntervalCount = 5 };

                Graphics xLabelsSize = new Graphics();
                xLabels.Plot(xLabelsSize);
                double xLabelsHeight = xLabelsSize.GetBounds().Size.Height;

                Graphics yLabelsSize = new Graphics();
                yLabels.Plot(yLabelsSize);
                double yLabelsWidth = yLabelsSize.GetBounds().Size.Width;

                ContinuousAxisTitle xTitle = new ContinuousAxisTitle(xAxisTitle, marginBottomLeft, marginBottomRight, coordinateSystem, axisTitlePresentationAttributes) { Position = xLabelsHeight + 20, Alignment = TextAnchors.Center };
                ContinuousAxisTitle yTitle = new ContinuousAxisTitle(yAxisTitle, marginBottomLeft, marginTopLeft, coordinateSystem, axisTitlePresentationAttributes) { Position = -20 - yLabelsWidth, Baseline = TextBaselines.Bottom, Alignment = TextAnchors.Center };


                List<IPlotElement> areas = new List<IPlotElement>(groupedData.Count);
                List<IPlotElement> lines = new List<IPlotElement>(groupedData.Count);

                for (int i = 0; i < groupedData.Count; i++)
                {
                    if (i == 0)
                    {
                        Area<IReadOnlyList<double>> area = new Area<IReadOnlyList<double>>(groupedData[i], getBaseline, coordinateSystem) { PresentationAttributes = new PlotElementPresentationAttributes(dataPresentationAttributes[i % dataPresentationAttributes.Count]) { Stroke = null }, Smooth = smooth };
                        DataLine<IReadOnlyList<double>> line = new DataLine<IReadOnlyList<double>>(groupedData[i], coordinateSystem) { PresentationAttributes = new PlotElementPresentationAttributes(dataPresentationAttributes[i % dataPresentationAttributes.Count]) { Fill = null }, Smooth = smooth };
                        areas.Add(area);
                        lines.Add(line);
                    }
                    else
                    {
                        int index = i;

                        if (vertical)
                        {
                            Area<IReadOnlyList<double>> area = new Area<IReadOnlyList<double>>(groupedData[i], x => new double[] { x[0], baselines[index - 1][x[0]] }, coordinateSystem) { PresentationAttributes = new PlotElementPresentationAttributes(dataPresentationAttributes[i % dataPresentationAttributes.Count]) { Stroke = null }, Smooth = smooth };
                            DataLine<IReadOnlyList<double>> line = new DataLine<IReadOnlyList<double>>(groupedData[i], coordinateSystem) { PresentationAttributes = new PlotElementPresentationAttributes(dataPresentationAttributes[i % dataPresentationAttributes.Count]) { Fill = null }, Smooth = smooth };
                            areas.Add(area);
                            lines.Add(line);
                        }
                        else
                        {
                            Area<IReadOnlyList<double>> area = new Area<IReadOnlyList<double>>(groupedData[i], x => new double[] { baselines[index - 1][x[1]], x[1] }, coordinateSystem) { PresentationAttributes = new PlotElementPresentationAttributes(dataPresentationAttributes[i % dataPresentationAttributes.Count]) { Stroke = null }, Smooth = smooth };
                            DataLine<IReadOnlyList<double>> line = new DataLine<IReadOnlyList<double>>(groupedData[i], coordinateSystem) { PresentationAttributes = new PlotElementPresentationAttributes(dataPresentationAttributes[i % dataPresentationAttributes.Count]) { Fill = null }, Smooth = smooth };
                            areas.Add(area);
                            lines.Add(line);
                        }
                    }

                }

                TextLabel<IReadOnlyList<double>> titleLabel = new TextLabel<IReadOnlyList<double>>(title, coordinateSystem.ToDataCoordinates(new Point((topLeft.X + topRight.X) * 0.5, (topLeft.Y + topRight.Y) * 0.5 - 20)), coordinateSystem) { Baseline = TextBaselines.Bottom, PresentationAttributes = titlePresentationAttributes };

                Plot tbr = new Plot();
                tbr.AddPlotElements(xGrid, yGrid, xAxis, yAxis, xTicks, yTicks, xLabels, yLabels, xTitle, yTitle);
                tbr.AddPlotElements(areas);
                tbr.AddPlotElements(lines);
                tbr.AddPlotElement(titleLabel);

                return tbr;
            }
        }
    }
}
