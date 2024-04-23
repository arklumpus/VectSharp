/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2024 Giorgio Bianchini, University of Bristol

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

using MathNet.Numerics.Statistics;
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
            /// Create a new swarm plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the swarms are parallel to the Y axis. If this is <see langword="false"/>, the swarms are parallel to the X axis.</param>
			/// <param name="showBoxPlots">If this is <see langword="true"/> (the default), a box plot is drawn on top of each swarm plot.</param>
			/// <param name="boxWidth">The width of the box plots, as a proportion of the available space for each swarm.</param>
            /// <param name="spacing">The spacing between consecutive swarms, in data space coordinates.</param>
            /// <param name="dataRangeMin">If this is not <see langword="null"/>, this value is used to override the default minimum value for the plot. Useful if you wish to create multiple plots with the same scale,
            /// even though the sampled values have different ranges.</param>
            /// <param name="dataRangeMax">If this is not <see langword="null"/>, this value is used to override the default maximum value for the plot. Useful if you wish to create multiple plots with the same scale,
            /// even though the sampled values have different ranges.</param>
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
            /// <param name="swarmSymbol">The symbol drawn at each data point.</param>
            /// <param name="swarmPointSize">The size of the symbol drawn at each data point.</param>
            /// <param name="swarmPointMargin">The margin between data point symbols. Set this to a negative value to compress the plot, or to a positive value to expand it.</param>
			/// <param name="swarmPresentationAttributes">Presentation attributes for the swarm data points.</param>
			/// <param name="boxPresentationAttributes">Presentation attributes for the box plots.</param>
			/// <param name="medianPresentationAttributes">Presentation attributes for the symbol drawn at the medians.</param>
            /// <param name="medianSymbol">The symbol drawn at the medians.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the swarm plot.</returns>
            public static Plot SwarmPlot<T>(IReadOnlyList<(T, IReadOnlyList<double>)> data, bool showBoxPlots = true, double boxWidth = 0.04, double spacing = 10, double? dataRangeMin = null, double? dataRangeMax = null, bool vertical = true, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                IDataPointElement swarmSymbol = null,
                double swarmPointSize = 2,
                double swarmPointMargin = 0.25,
                IReadOnlyList<PlotElementPresentationAttributes> swarmPresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> boxPresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> medianPresentationAttributes = null,
                IDataPointElement medianSymbol = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                if (swarmPresentationAttributes == null)
                {
                    if (showBoxPlots)
                    {
                        swarmPresentationAttributes = new PlotElementPresentationAttributes[] { new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(197, 235, 255)), Stroke = new SolidColourBrush(Colour.FromRgb(0, 114, 178)), LineWidth = 0 },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 233, 218)), Stroke = new SolidColourBrush(Colour.FromRgb(213, 94, 0)), LineWidth = 0 },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 222, 240)), Stroke = new SolidColourBrush(Colour.FromRgb(204, 121, 167)), LineWidth = 0 },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 242, 216)), Stroke = new SolidColourBrush(Colour.FromRgb(230, 159, 0)), LineWidth = 0 },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(214, 241, 255)), Stroke = new SolidColourBrush(Colour.FromRgb(86, 180, 233)), LineWidth = 0 },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(203, 255, 239)), Stroke = new SolidColourBrush(Colour.FromRgb(0, 158, 115)), LineWidth = 0 },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 249, 189)), Stroke = new SolidColourBrush(Colour.FromRgb(240, 228, 66)), LineWidth = 0 } };
                    }
                    else
                    {
                        swarmPresentationAttributes = new PlotElementPresentationAttributes[] { new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(0, 114, 178)), Stroke = null },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(213, 94, 0)), Stroke = null },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(204, 121, 167)), Stroke = null },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(230, 159, 0)), Stroke = null },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(86, 180, 233)), Stroke = null },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(0, 158, 115)), Stroke = null },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(240, 228, 66)), Stroke = null } };
                    }
                }

                if (boxPresentationAttributes == null)
                {
                    PlotElementPresentationAttributes[] temp = new PlotElementPresentationAttributes[swarmPresentationAttributes.Count];

                    for (int i = 0; i < swarmPresentationAttributes.Count; i++)
                    {
                        temp[i] = new PlotElementPresentationAttributes() { Stroke = swarmPresentationAttributes[i].Stroke, Fill = swarmPresentationAttributes[i].Stroke };
                    }

                    boxPresentationAttributes = temp;
                }

                if (medianPresentationAttributes == null)
                {
                    PlotElementPresentationAttributes[] temp = new PlotElementPresentationAttributes[swarmPresentationAttributes.Count];

                    for (int i = 0; i < swarmPresentationAttributes.Count; i++)
                    {
                        temp[i] = new PlotElementPresentationAttributes() { Stroke = null, Fill = swarmPresentationAttributes[i].Fill };
                    }

                    medianPresentationAttributes = temp;
                }

                if (medianSymbol == null)
                {
                    medianSymbol = new PathDataPointElement();
                }

                if (swarmSymbol == null)
                {
                    swarmSymbol = new PathDataPointElement();
                }

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

                if (titlePresentationAttributes == null)
                {
                    titlePresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 18) };
                }

                double[] medians = new double[data.Count];
                double[] firstQuartiles = new double[data.Count];
                double[] thirdQuartiles = new double[data.Count];
                double[] upperWhiskers = new double[data.Count];
                double[] lowerWhiskers = new double[data.Count];

                double minData = double.MaxValue;
                double maxData = double.MinValue;

                for (int i = 0; i < data.Count; i++)
                {
                    medians[i] = data[i].Item2.Median();
                    firstQuartiles[i] = data[i].Item2.LowerQuartile();
                    thirdQuartiles[i] = data[i].Item2.UpperQuartile();


                    upperWhiskers[i] = thirdQuartiles[i] + (thirdQuartiles[i] - firstQuartiles[i]) * 1.5;
                    lowerWhiskers[i] = firstQuartiles[i] - (thirdQuartiles[i] - firstQuartiles[i]) * 1.5;

                    double uW = double.MinValue;
                    double lW = double.MaxValue;

                    for (int j = 0; j < data[i].Item2.Count; j++)
                    {
                        if (data[i].Item2[j] <= upperWhiskers[i])
                        {
                            uW = Math.Max(uW, data[i].Item2[j]);
                        }

                        if (data[i].Item2[j] >= lowerWhiskers[i])
                        {
                            lW = Math.Min(lW, data[i].Item2[j]);
                        }

                        minData = Math.Min(minData, data[i].Item2[j]);
                        maxData = Math.Max(maxData, data[i].Item2[j]);
                    }

                    upperWhiskers[i] = uW;
                    lowerWhiskers[i] = lW;
                }

                double minX, maxX, minY, maxY;

                if (vertical)
                {
                    minX = 0;
                    maxX = spacing * data.Count;

                    minY = minData;
                    maxY = maxData;

                    if (dataRangeMin != null)
                    {
                        minY = dataRangeMin.Value;
                    }

                    if (dataRangeMax != null)
                    {
                        maxY = dataRangeMax.Value;
                    }
                }
                else
                {
                    minY = 0;
                    maxY = spacing * data.Count;

                    minX = minData;
                    maxX = maxData;

                    if (dataRangeMin != null)
                    {
                        minX = dataRangeMin.Value;
                    }

                    if (dataRangeMax != null)
                    {
                        maxX = dataRangeMax.Value;
                    }
                }

                if (coordinateSystem == null)
                {
                    coordinateSystem = new LinearCoordinateSystem2D(minX, maxX, minY, maxY, width, height);
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

                ContinuousAxis xAxis = new ContinuousAxis(marginBottomLeft, marginBottomRight, coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = vertical ? 0 : axisArrowSize };
                ContinuousAxis yAxis = new ContinuousAxis(marginBottomLeft, marginTopLeft, coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = vertical ? axisArrowSize : 0 };

                IPlotElement xTicks;
                IPlotElement yTicks;

                IPlotElement xLabels;
                IPlotElement yLabels;

                if (vertical)
                {
                    xTicks = new ScatterPoints<IReadOnlyList<double>>((from el in Enumerable.Range(0, data.Count) select new double[] { spacing * (el + 0.5), p3[1] }), coordinateSystem) { PresentationAttributes = axisPresentationAttributes, Size = 1, DataPointElement = new PathDataPointElement(new GraphicsPath().MoveTo(0, -3).LineTo(0, 3)) };
                    yTicks = new ContinuousAxisTicks(p6, p5, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };

                    xLabels = new DataLabels<IReadOnlyList<double>>((from el in Enumerable.Range(0, data.Count) select new double[] { spacing * (el + 0.5), p3[1] }), coordinateSystem) { Alignment = TextAnchors.Center, Baseline = TextBaselines.Top, Label = (i, _) => data[i].Item1, PresentationAttributes = axisLabelPresentationAttributes, Margin = (a, b) => new Point(0, 10) };
                    yLabels = new ContinuousAxisLabels(p6, p5, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Position = _ => -10, Alignment = TextAnchors.Right, Rotation = 0, IntervalCount = 5 };
                }
                else
                {
                    xTicks = new ContinuousAxisTicks(p3, p4, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };

                    yTicks = new ScatterPoints<IReadOnlyList<double>>((from el in Enumerable.Range(0, data.Count) select new double[] { p6[0], spacing * (el + 0.5) }), coordinateSystem) { PresentationAttributes = axisPresentationAttributes, Size = 1, DataPointElement = new PathDataPointElement(new GraphicsPath().MoveTo(-3, 0).LineTo(3, 0)) };

                    xLabels = new ContinuousAxisLabels(p3, p4, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Alignment = TextAnchors.Center, Baseline = TextBaselines.Top, Rotation = 0, IntervalCount = 5 };
                    yLabels = new DataLabels<IReadOnlyList<double>>((from el in Enumerable.Range(0, data.Count) select new double[] { p6[0], spacing * (el + 0.5) }), coordinateSystem) { Alignment = TextAnchors.Right, Baseline = TextBaselines.Middle, Label = (i, _) => data[i].Item1, PresentationAttributes = axisLabelPresentationAttributes, Margin = (a, b) => new Point(-10, 0) };
                }

                Graphics xLabelsSize = new Graphics();
                xLabels.Plot(xLabelsSize);
                double xLabelsHeight = xLabelsSize.GetBounds().Size.Height;

                Graphics yLabelsSize = new Graphics();
                yLabels.Plot(yLabelsSize);
                double yLabelsWidth = yLabelsSize.GetBounds().Size.Width;

                ContinuousAxisTitle xTitle = new ContinuousAxisTitle(xAxisTitle, marginBottomLeft, marginBottomRight, coordinateSystem, axisTitlePresentationAttributes) { Position = xLabelsHeight + 20, Alignment = TextAnchors.Center };
                ContinuousAxisTitle yTitle = new ContinuousAxisTitle(yAxisTitle, marginBottomLeft, marginTopLeft, coordinateSystem, axisTitlePresentationAttributes) { Position = -20 - yLabelsWidth, Baseline = TextBaselines.Bottom, Alignment = TextAnchors.Center };

                Plot plot = new Plot();

                if (vertical)
                {
                    Grid yGrid = new Grid(p5, p6, p7, p8, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };
                    plot.AddPlotElement(yGrid);
                }
                else
                {
                    Grid xGrid = new Grid(p1, p2, p3, p4, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };
                    plot.AddPlotElement(xGrid);
                }

                plot.AddPlotElements(xAxis, yAxis, xTicks, yTicks, xLabels, yLabels, xTitle, yTitle);

                double maxCount = (from el in data select el.Item2.Count).Max();

                for (int i = 0; i < data.Count; i++)
                {
                    if (vertical)
                    {
                        Swarm swarm = new Swarm(new double[] { spacing * (i + 0.5), 0 }, new double[] { 0, 1 }, data[i].Item2, coordinateSystem)
                        {
                            PointMargin = swarmPointMargin,
                            PointSize = swarmPointSize,
                            PresentationAttributes = swarmPresentationAttributes[i % swarmPresentationAttributes.Count],
                            SwarmPointElement = swarmSymbol
                        };
                        plot.AddPlotElement(swarm);

                        if (showBoxPlots)
                        {
                            BoxPlot box = new BoxPlot(new double[] { spacing * (i + 0.5), medians[i] }, new double[] { 0, 1 }, lowerWhiskers[i] - medians[i], firstQuartiles[i] - medians[i], thirdQuartiles[i] - medians[i], upperWhiskers[i] - medians[i], coordinateSystem)
                            {
                                BoxPresentationAttributes = boxPresentationAttributes[i % boxPresentationAttributes.Count],
                                WhiskersPresentationAttributes = boxPresentationAttributes[i % boxPresentationAttributes.Count],
                                Width = spacing * boxWidth * 0.5,
                                NotchSize = 0,
                                WhiskerWidth = 0,
                                CentreSymbolPresentationAttributes = medianPresentationAttributes[i % medianPresentationAttributes.Count],
                                CentreSymbol = medianSymbol
                            };
                            plot.AddPlotElement(box);
                        }
                    }
                    else
                    {
                        Swarm swarm = new Swarm(new double[] { 0, spacing * (i + 0.5) }, new double[] { 1, 0 }, data[i].Item2, coordinateSystem)
                        {
                            PointMargin = swarmPointMargin,
                            PointSize = swarmPointSize,
                            PresentationAttributes = swarmPresentationAttributes[i % swarmPresentationAttributes.Count],
                            SwarmPointElement = swarmSymbol
                        };
                        plot.AddPlotElement(swarm);

                        if (showBoxPlots)
                        {
                            BoxPlot box = new BoxPlot(new double[] { medians[i], spacing * (i + 0.5) }, new double[] { 1, 0 }, lowerWhiskers[i] - medians[i], firstQuartiles[i] - medians[i], thirdQuartiles[i] - medians[i], upperWhiskers[i] - medians[i], coordinateSystem)
                            {
                                BoxPresentationAttributes = boxPresentationAttributes[i % boxPresentationAttributes.Count],
                                WhiskersPresentationAttributes = boxPresentationAttributes[i % boxPresentationAttributes.Count],
                                Width = spacing * boxWidth * 0.5,
                                NotchSize = 0,
                                WhiskerWidth = 0,
                                CentreSymbolPresentationAttributes = medianPresentationAttributes[i % medianPresentationAttributes.Count],
                                CentreSymbol = medianSymbol
                            };

                            plot.AddPlotElement(box);
                        }
                    }
                }

                TextLabel<IReadOnlyList<double>> titleLabel = new TextLabel<IReadOnlyList<double>>(title, coordinateSystem.ToDataCoordinates(new Point((topLeft.X + topRight.X) * 0.5, (topLeft.Y + topRight.Y) * 0.5 - 20)), coordinateSystem) { Baseline = TextBaselines.Bottom, PresentationAttributes = titlePresentationAttributes };

                plot.AddPlotElement(titleLabel);

                return plot;
            }


            /// <summary>
            /// Create a new swarm plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the swarms are parallel to the Y axis. If this is <see langword="false"/>, the swarms are parallel to the X axis.</param>
			/// <param name="showBoxPlots">If this is <see langword="true"/> (the default), a box plot is drawn on top of each swarm plot.</param>
			/// <param name="boxWidth">The width of the box plots, as a proportion of the available space for each swarm.</param>
            /// <param name="spacing">The spacing between consecutive swarms, in data space coordinates.</param>
            /// <param name="dataRangeMin">If this is not <see langword="null"/>, this value is used to override the default minimum value for the plot. Useful if you wish to create multiple plots with the same scale,
            /// even though the sampled values have different ranges.</param>
            /// <param name="dataRangeMax">If this is not <see langword="null"/>, this value is used to override the default maximum value for the plot. Useful if you wish to create multiple plots with the same scale,
            /// even though the sampled values have different ranges.</param>
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
            /// <param name="swarmSymbol">The symbol drawn at each data point.</param>
            /// <param name="swarmPointSize">The size of the symbol drawn at each data point.</param>
            /// <param name="swarmPointMargin">The margin between data point symbols. Set this to a negative value to compress the plot, or to a positive value to expand it.</param>
			/// <param name="swarmPresentationAttributes">Presentation attributes for the swarm data points.</param>
			/// <param name="boxPresentationAttributes">Presentation attributes for the box plots.</param>
			/// <param name="medianPresentationAttributes">Presentation attributes for the symbol drawn at the medians.</param>
            /// <param name="medianSymbol">The symbol drawn at the medians.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the swarm plot.</returns>
            public static Plot SwarmPlot(IReadOnlyList<IReadOnlyList<double>> data, bool showBoxPlots = true, double boxWidth = 0.04, double spacing = 10, double? dataRangeMin = null, double? dataRangeMax = null, bool vertical = true, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                IDataPointElement swarmSymbol = null,
                double swarmPointSize = 2,
                double swarmPointMargin = 0.25,
                IReadOnlyList<PlotElementPresentationAttributes> swarmPresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> boxPresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> medianPresentationAttributes = null,
                IDataPointElement medianSymbol = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return SwarmPlot((from el in data select ((string)null, el)).ToArray(), showBoxPlots, boxWidth, spacing, dataRangeMin, dataRangeMax, vertical, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, swarmSymbol, swarmPointSize, swarmPointMargin, swarmPresentationAttributes, boxPresentationAttributes, medianPresentationAttributes, medianSymbol, coordinateSystem);
            }

            /// <summary>
            /// Create a new swarm plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the swarms are parallel to the Y axis. If this is <see langword="false"/>, the swarms are parallel to the X axis.</param>
            /// <param name="showBoxPlots">If this is <see langword="true"/> (the default), a box plot is drawn on top of each swarm plot.</param>
            /// <param name="boxWidth">The width of the box plots, as a proportion of the available space for each swarm.</param>
            /// <param name="spacing">The spacing between consecutive swarms, in data space coordinates.</param>
            /// <param name="dataRangeMin">If this is not <see langword="null"/>, this value is used to override the default minimum value for the plot. Useful if you wish to create multiple plots with the same scale,
            /// even though the sampled values have different ranges.</param>
            /// <param name="dataRangeMax">If this is not <see langword="null"/>, this value is used to override the default maximum value for the plot. Useful if you wish to create multiple plots with the same scale,
            /// even though the sampled values have different ranges.</param>
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
            /// <param name="swarmSymbol">The symbol drawn at each data point.</param>
            /// <param name="swarmPointSize">The size of the symbol drawn at each data point.</param>
            /// <param name="swarmPointMargin">The margin between data point symbols. Set this to a negative value to compress the plot, or to a positive value to expand it.</param>
            /// <param name="swarmPresentationAttributes">Presentation attributes for the swarm data points.</param>
            /// <param name="boxPresentationAttributes">Presentation attributes for the box plots.</param>
            /// <param name="medianPresentationAttributes">Presentation attributes for the symbol drawn at the medians.</param>
            /// <param name="medianSymbol">The symbol drawn at the medians.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the swarm plot.</returns>
            public static Plot SwarmPlot(IReadOnlyList<double> data, bool showBoxPlots = true, double boxWidth = 0.04, double spacing = 10, double? dataRangeMin = null, double? dataRangeMax = null, bool vertical = true, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                IDataPointElement swarmSymbol = null,
                double swarmPointSize = 2,
                double swarmPointMargin = 0.25,
                IReadOnlyList<PlotElementPresentationAttributes> swarmPresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> boxPresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> medianPresentationAttributes = null,
                IDataPointElement medianSymbol = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return SwarmPlot(new (string, IReadOnlyList<double>)[] { (null, data) }, showBoxPlots, boxWidth, spacing, dataRangeMin, dataRangeMax, vertical, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, swarmSymbol, swarmPointSize, swarmPointMargin, swarmPresentationAttributes, boxPresentationAttributes, medianPresentationAttributes, medianSymbol, coordinateSystem);
            }
        }
    }
}