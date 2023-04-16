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

using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VectSharp.Plots
{
    partial class Plot
    {
        /// <summary>
        /// Describes the types of whiskers for a box plot.
        /// </summary>
        public enum WhiskerType
        {
            /// <summary>
            /// The whiskers extend from the minimum sampled value to the maximum sampled value.
            /// </summary>
            FullRange,
            
            /// <summary>
            /// The whiskers extend from (Q1 - 1.5 * IQR) to (Q3 + 1.5 * IQR).
            /// </summary>
            IQR_1_5,

            /// <summary>
            /// The whiskers extend from (mean - 2 * standard deviation) to (mean + 2 * standard deviation).
            /// </summary>
            StandardDeviation
        }

        partial class Create
        {
            /// <summary>
            /// Create a new box plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the boxes are parallel to the Y axis. If this is <see langword="false"/>, the boxes are parallel to the X axis.</param>
            /// <param name="whiskerType">The type of whiskers to use in the plot.</param>
            /// <param name="useNotches">If this is <see langword="true"/> (the default), the box plot has notches. Otherwise, it does not.</param>
            /// <param name="proportionalWidth">If this is <see langword="false"/> (the default), all the boxes have the same width. Otherwise, the width of each box is proportional to the number of samples.</param>
            /// <param name="showOutliers">If this is <see langword="true"/> (the default), a symbol is drawn four outlier points that fall outside of the interval between the whiskers.</param>
            /// <param name="boxWidth">The width of the boxes in data space coordinates.</param>
            /// <param name="spacing">The spacing between consecutive boxes (ranging from 0 to 1).</param>
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
            /// <param name="boxPresentationAttributes">Presentation attributes for the boxes.</param>
            /// <param name="outlierPointElement">The symbol drawn at outlier points.</param>
            /// <param name="outlierPresentationAttributes">Presentation attributes for the outlier points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the box plot.</returns>
            public static Plot BoxPlot<T>(IReadOnlyList<(T, IReadOnlyList<double>)> data, WhiskerType whiskerType = WhiskerType.IQR_1_5, bool useNotches = true, bool proportionalWidth = false, bool showOutliers = true, double boxWidth = 10, double spacing = 0.1, double? dataRangeMin = null, double? dataRangeMax = null, bool vertical = true, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> boxPresentationAttributes = null,
                IDataPointElement outlierPointElement = null,
                IReadOnlyList<PlotElementPresentationAttributes> outlierPresentationAttributes = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                if (boxPresentationAttributes == null)
                {
                    boxPresentationAttributes = new PlotElementPresentationAttributes[] { new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(197, 235, 255)), Stroke = new SolidColourBrush(Colour.FromRgb(0, 114, 178)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 233, 218)), Stroke = new SolidColourBrush(Colour.FromRgb(213, 94, 0)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 222, 240)), Stroke = new SolidColourBrush(Colour.FromRgb(204, 121, 167)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 242, 216)), Stroke = new SolidColourBrush(Colour.FromRgb(230, 159, 0)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(214, 241, 255)), Stroke = new SolidColourBrush(Colour.FromRgb(86, 180, 233)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(203, 255, 239)), Stroke = new SolidColourBrush(Colour.FromRgb(0, 158, 115)) },
                                                                                       new PlotElementPresentationAttributes() { Fill = new SolidColourBrush(Colour.FromRgb(255, 249, 189)), Stroke = new SolidColourBrush(Colour.FromRgb(240, 228, 66)) } };
                }

                if (outlierPresentationAttributes == null)
                {
                    PlotElementPresentationAttributes[] temp = new PlotElementPresentationAttributes[boxPresentationAttributes.Count];

                    for (int i = 0; i < boxPresentationAttributes.Count; i++)
                    {
                        temp[i] = new PlotElementPresentationAttributes() { Stroke = null, Fill = boxPresentationAttributes[i].Stroke };
                    }

                    outlierPresentationAttributes = temp;
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

                if (outlierPointElement == null)
                {
                    outlierPointElement = new PathDataPointElement();
                }

                double[] medians = new double[data.Count];
                double[] firstQuartiles = new double[data.Count];
                double[] thirdQuartiles = new double[data.Count];
                double[] upperWhiskers = new double[data.Count];
                double[] lowerWhiskers = new double[data.Count];
                double[] notches = new double[data.Count];

                double minData = double.MaxValue;
                double maxData = double.MinValue;

                for (int i = 0; i < data.Count; i++)
                {
                    medians[i] = data[i].Item2.Median();
                    firstQuartiles[i] = data[i].Item2.LowerQuartile();
                    thirdQuartiles[i] = data[i].Item2.UpperQuartile();

                    if (useNotches)
                    {
                        notches[i] = 1.58 * (thirdQuartiles[i] - firstQuartiles[i]) / Math.Sqrt(data[i].Item2.Count);
                    }
                    else
                    {
                        notches[i] = 0;
                    }

                    if (whiskerType == WhiskerType.FullRange)
                    {
                        double uW = double.MinValue;
                        double lW = double.MaxValue;

                        for (int j = 0; j < data[i].Item2.Count; j++)
                        {
                            uW = Math.Max(uW, data[i].Item2[j]);
                            lW = Math.Min(lW, data[i].Item2[j]);

                            minData = Math.Min(minData, data[i].Item2[j]);
                            maxData = Math.Max(maxData, data[i].Item2[j]);
                        }

                        upperWhiskers[i] = uW;
                        lowerWhiskers[i] = lW;
                    }
                    else if (whiskerType == WhiskerType.IQR_1_5)
                    {
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
                    else if (whiskerType == WhiskerType.StandardDeviation)
                    {
                        double stdDev = data[i].Item2.StandardDeviation();
                        double mean = data[i].Item2.Mean();

                        upperWhiskers[i] = mean + 2 * stdDev;
                        lowerWhiskers[i] = mean - 2 * stdDev;

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
                }

                if (!showOutliers)
                {
                    minData = lowerWhiskers.Minimum();
                    maxData = upperWhiskers.Maximum();
                }

                double minX, maxX, minY, maxY;

                if (vertical)
                {
                    minX = 0;
                    maxX = boxWidth * (data.Count + spacing * (data.Count - 1));

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
                    maxY = boxWidth * (data.Count + spacing * (data.Count - 1));

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
                    //xTicks = new ContinuousAxisTicks(new double[] { 0.5 * boxWidth, p3[1] }, new double[] { ((data.Count - 1) * (spacing + 1) + 0.5) * boxWidth, p4[1] }, coordinateSystem) { PresentationAttributes = axisPresentationAttributes, IntervalCount = data.Count - 1, SizeAbove = _ => 3, SizeBelow = _ => 3 };

                    xTicks = new ScatterPoints<IReadOnlyList<double>>((from el in Enumerable.Range(0, data.Count) select new double[] { (el * (spacing + 1) + 0.5) * boxWidth, p3[1] }), coordinateSystem) { PresentationAttributes = axisPresentationAttributes, Size = 1, DataPointElement = new PathDataPointElement(new GraphicsPath().MoveTo(0, -3).LineTo(0, 3)) };
                    yTicks = new ContinuousAxisTicks(p6, p5, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };

                    xLabels = new DataLabels<IReadOnlyList<double>>((from el in Enumerable.Range(0, data.Count) select new double[] { (el * (spacing + 1) + 0.5) * boxWidth, p3[1] }), coordinateSystem) { Alignment = TextAnchors.Center, Baseline = TextBaselines.Top, Label = (i, _) => data[i].Item1, PresentationAttributes = axisLabelPresentationAttributes, Margin = (a, b) => new Point(0, 10) };
                    yLabels = new ContinuousAxisLabels(p6, p5, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Position = _ => -10, Alignment = TextAnchors.Right, Rotation = 0, IntervalCount = 5 };
                }
                else
                {
                    xTicks = new ContinuousAxisTicks(p3, p4, coordinateSystem) { PresentationAttributes = axisPresentationAttributes };

                    yTicks = new ScatterPoints<IReadOnlyList<double>>((from el in Enumerable.Range(0, data.Count) select new double[] { p6[0], (el * (spacing + 1) + 0.5) * boxWidth }), coordinateSystem) { PresentationAttributes = axisPresentationAttributes, Size = 1, DataPointElement = new PathDataPointElement(new GraphicsPath().MoveTo(-3, 0).LineTo(3, 0)) };

                    xLabels = new ContinuousAxisLabels(p3, p4, coordinateSystem) { PresentationAttributes = axisLabelPresentationAttributes, Alignment = TextAnchors.Center, Baseline = TextBaselines.Top, Rotation = 0, IntervalCount = 5 };
                    yLabels = new DataLabels<IReadOnlyList<double>>((from el in Enumerable.Range(0, data.Count) select new double[] { p6[0], (el * (spacing + 1) + 0.5) * boxWidth }), coordinateSystem) { Alignment = TextAnchors.Right, Baseline = TextBaselines.Middle, Label = (i, _) => data[i].Item1, PresentationAttributes = axisLabelPresentationAttributes, Margin = (a, b) => new Point(-10, 0) };
                }

                Graphics xLabelsSize = new Graphics();
                xLabels.Plot(xLabelsSize);
                double xLabelsHeight = xLabelsSize.GetBounds().Size.Height;

                Graphics yLabelsSize = new Graphics();
                yLabels.Plot(yLabelsSize);
                double yLabelsWidth = yLabelsSize.GetBounds().Size.Width;

                ContinuousAxisTitle xTitle = new ContinuousAxisTitle(xAxisTitle, marginBottomLeft, marginBottomRight, coordinateSystem, axisTitlePresentationAttributes) { Position = xLabelsHeight + 20, Baseline = TextBaselines.Top, Alignment = TextAnchors.Center };
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
                        BoxPlot box = new BoxPlot(new double[] { (i * (spacing + 1) + 0.5) * boxWidth, medians[i] }, new double[] { 0, 1 }, lowerWhiskers[i] - medians[i], firstQuartiles[i] - medians[i], thirdQuartiles[i] - medians[i], upperWhiskers[i] - medians[i], coordinateSystem)
                        {
                            BoxPresentationAttributes = boxPresentationAttributes[i % boxPresentationAttributes.Count],
                            WhiskersPresentationAttributes = boxPresentationAttributes[i % boxPresentationAttributes.Count],
                            Width = proportionalWidth ? (boxWidth * 0.5 * data[i].Item2.Count / maxCount) : (boxWidth * 0.5),
                            NotchSize = notches[i]
                        };

                        plot.AddPlotElement(box);
                    }
                    else
                    {
                        BoxPlot box = new BoxPlot(new double[] { medians[i], (i * (spacing + 1) + 0.5) * boxWidth }, new double[] { 1, 0 }, lowerWhiskers[i] - medians[i], firstQuartiles[i] - medians[i], thirdQuartiles[i] - medians[i], upperWhiskers[i] - medians[i], coordinateSystem)
                        {
                            BoxPresentationAttributes = boxPresentationAttributes[i % boxPresentationAttributes.Count],
                            WhiskersPresentationAttributes = boxPresentationAttributes[i % boxPresentationAttributes.Count],
                            Width = proportionalWidth ? (boxWidth * 0.5 * data[i].Item2.Count / maxCount) : (boxWidth * 0.5),
                            NotchSize = notches[i]
                        };

                        plot.AddPlotElement(box);
                    }
                }

                if (showOutliers)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        double[][] outliers;

                        if (vertical)
                        {
                            outliers = (from el in data[i].Item2 where el > upperWhiskers[i] || el < lowerWhiskers[i] select new double[] { (i * (spacing + 1) + 0.5) * boxWidth, el }).ToArray();
                        }
                        else
                        {
                            outliers = (from el in data[i].Item2 where el > upperWhiskers[i] || el < lowerWhiskers[i] select new double[] { el, (i * (spacing + 1) + 0.5) * boxWidth }).ToArray();
                        }

                        if (outliers.Length > 0)
                        {
                            ScatterPoints<IReadOnlyList<double>> outlierPoints = new ScatterPoints<IReadOnlyList<double>>(outliers, coordinateSystem)
                            {
                                PresentationAttributes = outlierPresentationAttributes[i % outlierPresentationAttributes.Count],
                                Size = 3,
                                DataPointElement= outlierPointElement
                            };

                            plot.AddPlotElement(outlierPoints);
                        }
                    }
                }

                TextLabel<IReadOnlyList<double>> titleLabel = new TextLabel<IReadOnlyList<double>>(title, coordinateSystem.ToDataCoordinates(new Point((topLeft.X + topRight.X) * 0.5, (topLeft.Y + topRight.Y) * 0.5 - 20)), coordinateSystem) { Baseline = TextBaselines.Bottom, PresentationAttributes = titlePresentationAttributes };

                plot.AddPlotElement(titleLabel);

                return plot;
            }

            /// <summary>
            /// Create a new box plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the boxes are parallel to the Y axis. If this is <see langword="false"/>, the boxes are parallel to the X axis.</param>
            /// <param name="whiskerType">The type of whiskers to use in the plot.</param>
            /// <param name="useNotches">If this is <see langword="true"/> (the default), the box plot has notches. Otherwise, it does not.</param>
            /// <param name="proportionalWidth">If this is <see langword="false"/> (the default), all the boxes have the same width. Otherwise, the width of each box is proportional to the number of samples.</param>
            /// <param name="showOutliers">If this is <see langword="true"/> (the default), a symbol is drawn four outlier points that fall outside of the interval between the whiskers.</param>
            /// <param name="boxWidth">The width of the boxes in data space coordinates.</param>
            /// <param name="spacing">The spacing between consecutive boxes (ranging from 0 to 1).</param>
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
            /// <param name="boxPresentationAttributes">Presentation attributes for the boxes.</param>
            /// <param name="outlierPointElement">The symbol drawn at outlier points.</param>
            /// <param name="outlierPresentationAttributes">Presentation attributes for the outlier points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the box plot.</returns>
            public static Plot BoxPlot(IReadOnlyList<IReadOnlyList<double>> data, WhiskerType whiskerType = WhiskerType.IQR_1_5, bool useNotches = true, bool proportionalWidth = false, bool showOutliers = true, double boxWidth = 10, double spacing = 0.1, double? dataRangeMin = null, double? dataRangeMax = null, bool vertical = true, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> boxPresentationAttributes = null,
                IDataPointElement outlierPointElement = null,
                IReadOnlyList<PlotElementPresentationAttributes> outlierPresentationAttributes = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return BoxPlot((from el in data select ((string)null, el)).ToArray(), whiskerType, useNotches, proportionalWidth, showOutliers, boxWidth, spacing, dataRangeMin, dataRangeMax, vertical, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, boxPresentationAttributes, outlierPointElement, outlierPresentationAttributes, coordinateSystem);
            }

            /// <summary>
            /// Create a new box plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the box is parallel to the Y axis. If this is <see langword="false"/>, the box is parallel to the X axis.</param>
            /// <param name="whiskerType">The type of whiskers to use in the plot.</param>
            /// <param name="useNotches">If this is <see langword="true"/> (the default), the box plot has notches. Otherwise, it does not.</param>
            /// <param name="showOutliers">If this is <see langword="true"/> (the default), a symbol is drawn four outlier points that fall outside of the interval between the whiskers.</param>
            /// <param name="boxWidth">The width of the box in data space coordinates.</param>
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
            /// <param name="boxPresentationAttributes">Presentation attributes for the box.</param>
            /// <param name="outlierPointElement">The symbol drawn at outlier points.</param>
            /// <param name="outlierPresentationAttributes">Presentation attributes for the outlier points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the box plot.</returns>
            public static Plot BoxPlot(IReadOnlyList<double> data, WhiskerType whiskerType = WhiskerType.IQR_1_5, bool useNotches = true, bool showOutliers = true, double boxWidth = 10, double? dataRangeMin = null, double? dataRangeMax = null, bool vertical = true, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                PlotElementPresentationAttributes boxPresentationAttributes = null,
                IDataPointElement outlierPointElement = null,
                PlotElementPresentationAttributes outlierPresentationAttributes = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return BoxPlot(new (string, IReadOnlyList<double>)[] { (null, data) }, whiskerType, useNotches, false, showOutliers, boxWidth, 0, dataRangeMin, dataRangeMax, vertical, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, boxPresentationAttributes == null ? null : new PlotElementPresentationAttributes[] { boxPresentationAttributes }, outlierPointElement, outlierPresentationAttributes == null ? null : new PlotElementPresentationAttributes[] { outlierPresentationAttributes }, coordinateSystem);
            }
        }
    }
}
