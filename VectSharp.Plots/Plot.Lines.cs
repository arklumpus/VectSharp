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
            /// Create a new line chart containing multiple lines.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="linePresentationAttributes">Presentation attributes for the line.</param>
            /// <param name="pointPresentationAttributes">Presentation attributes for the sampled points.</param>
            /// <param name="pointSizes">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElements">Symbols drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the line chart.</returns>
            public static Plot LineCharts(IReadOnlyList<IReadOnlyList<IReadOnlyList<double>>> data, bool smooth = false, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> linePresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> pointPresentationAttributes = null,
                IReadOnlyList<double> pointSizes = null,
                IReadOnlyList<IDataPointElement> dataPointElements = null,
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

                if (linePresentationAttributes == null)
                {
                    linePresentationAttributes = new PlotElementPresentationAttributes[] { new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(0, 114, 178)), Fill = null },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(213, 94, 0)), Fill = null },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(204, 121, 167)), Fill = null },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(230, 159, 0)), Fill = null },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(86, 180, 233)), Fill = null },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(0, 158, 115)), Fill = null },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(240, 228, 66)), Fill = null } };
                }

                if (pointPresentationAttributes == null)
                {
                    pointPresentationAttributes = (from el in linePresentationAttributes select new PlotElementPresentationAttributes() { Stroke = null, Fill = el.Stroke }).ToList();
                }

                if (dataPointElements == null)
                {
                    dataPointElements = new PathDataPointElement[] { new PathDataPointElement() };
                }

                List<IReadOnlyList<double>> allData = new List<IReadOnlyList<double>>();

                for (int i = 0; i < data.Count; i++)
                {
                    allData.AddRange(data[i]);
                }

                if (coordinateSystem == null)
                {
                    coordinateSystem = new LinearCoordinateSystem2D(allData, width, height);
                }

                if (titlePresentationAttributes == null)
                {
                    titlePresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 18) };
                }

                (double minX, double minY, double maxX, double maxY, double rangeX, double rangeY) = GetDataRange(allData);

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

                ContinuousAxis xAxis = new ContinuousAxis(marginBottomLeft[0] < marginBottomRight[0] ? marginBottomLeft : marginBottomRight, marginBottomLeft[0] < marginBottomRight[0] ? marginBottomRight : coordinateSystem.ToDataCoordinates(coordinateSystem.ToPlotCoordinates(marginBottomLeft) + new Point(-axisArrowSize - 7, 0)), coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = axisArrowSize };
                ContinuousAxis yAxis = new ContinuousAxis(marginBottomLeft[1] < marginTopLeft[1] ? marginBottomLeft : marginTopLeft, marginBottomLeft[1] < marginTopLeft[1] ? marginTopLeft : coordinateSystem.ToDataCoordinates(coordinateSystem.ToPlotCoordinates(marginBottomLeft) + new Point(0, axisArrowSize + 7)), coordinateSystem) { PresentationAttributes = axisPresentationAttributes, ArrowSize = axisArrowSize };

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

                List<IPlotElement> otherElements = new List<IPlotElement>();

                for (int i = 0; i < data.Count; i++)
                {
                    DataLine<IReadOnlyList<double>> line1 = new DataLine<IReadOnlyList<double>>(data[i], coordinateSystem) { PresentationAttributes = linePresentationAttributes[i % linePresentationAttributes.Count], Smooth = smooth };

                    otherElements.Add(line1);

                    if (pointSizes != null && pointSizes.Count > 0 && pointSizes[i % pointSizes.Count] > 0)
                    {
                        ScatterPoints<IReadOnlyList<double>> points1 = new ScatterPoints<IReadOnlyList<double>>(data[i], coordinateSystem) { PresentationAttributes = pointPresentationAttributes[i % pointPresentationAttributes.Count], DataPointElement = dataPointElements[i % dataPointElements.Count], Size = pointSizes[i % pointSizes.Count] };
                        otherElements.Add(points1);
                    }

                }

                TextLabel<IReadOnlyList<double>> titleLabel = new TextLabel<IReadOnlyList<double>>(title, coordinateSystem.ToDataCoordinates((coordinateSystem.ToPlotCoordinates(marginTopLeft) + coordinateSystem.ToPlotCoordinates(marginTopRight)) * 0.5 + new Point(0, -10)), coordinateSystem) { Baseline = TextBaselines.Bottom, PresentationAttributes = titlePresentationAttributes };

                Plot tbr = new Plot();
                tbr.AddPlotElements(xGrid, yGrid, xAxis, yAxis, xTicks, yTicks, xLabels, yLabels, xTitle, yTitle);

                tbr.AddPlotElements(otherElements);

                tbr.AddPlotElement(titleLabel);

                return tbr;
            }

            /// <summary>
            /// Create a new line chart.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="linePresentationAttributes">Presentation attributes for the line.</param>
            /// <param name="pointPresentationAttributes">Presentation attributes for the sampled points.</param>
            /// <param name="pointSize">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElement">Symbol drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the line chart.</returns>
            public static Plot LineChart(IReadOnlyList<IReadOnlyList<double>> data, bool smooth = false, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                PlotElementPresentationAttributes linePresentationAttributes = null,
                PlotElementPresentationAttributes pointPresentationAttributes = null,
                double pointSize = 0,
                IDataPointElement dataPointElement = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return LineCharts(new IReadOnlyList<IReadOnlyList<double>>[] { data }, smooth, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, linePresentationAttributes == null ? null : new PlotElementPresentationAttributes[] { linePresentationAttributes }, pointPresentationAttributes == null ? null : new PlotElementPresentationAttributes[] { pointPresentationAttributes }, new double[] { pointSize }, dataPointElement == null ? null : new IDataPointElement[] { dataPointElement }, coordinateSystem);
            }

            /// <summary>
            /// Create a new line chart.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="linePresentationAttributes">Presentation attributes for the line.</param>
            /// <param name="pointPresentationAttributes">Presentation attributes for the sampled points.</param>
            /// <param name="pointSize">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElement">Symbol drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the line chart.</returns>
            public static Plot LineChart(IReadOnlyList<(double, double)> data, bool smooth = false, double width = 350, double height = 250,
               PlotElementPresentationAttributes axisPresentationAttributes = null,
               double axisArrowSize = 3,
               PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
               PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
               string xAxisTitle = null,
               string yAxisTitle = null,
               string title = null,
               PlotElementPresentationAttributes titlePresentationAttributes = null,
               PlotElementPresentationAttributes gridPresentationAttributes = null,
               PlotElementPresentationAttributes linePresentationAttributes = null,
               PlotElementPresentationAttributes pointPresentationAttributes = null,
               double pointSize = 0,
               IDataPointElement dataPointElement = null,
               IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return LineCharts(new IReadOnlyList<IReadOnlyList<double>>[] { (from el in data select new double[] { el.Item1, el.Item2 }).ToArray() }, smooth, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, linePresentationAttributes == null ? null : new PlotElementPresentationAttributes[] { linePresentationAttributes }, pointPresentationAttributes == null ? null : new PlotElementPresentationAttributes[] { pointPresentationAttributes }, new double[] { pointSize }, dataPointElement == null ? null : new IDataPointElement[] { dataPointElement }, coordinateSystem);
            }

            /// <summary>
            /// Create a new line chart.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="linePresentationAttributes">Presentation attributes for the line.</param>
            /// <param name="pointPresentationAttributes">Presentation attributes for the sampled points.</param>
            /// <param name="pointSize">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElement">Symbol drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the line chart.</returns>
            public static Plot LineChart(IReadOnlyList<double> data, bool smooth = false, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                PlotElementPresentationAttributes linePresentationAttributes = null,
                PlotElementPresentationAttributes pointPresentationAttributes = null,
                double pointSize = 0,
                IDataPointElement dataPointElement = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                double[][] actualData = new double[data.Count][];

                for (int i = 0; i < data.Count; i++)
                {
                    actualData[i] = new double[] { i + 1, data[i] };
                }

                Plot plot = LineChart(actualData, smooth, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, linePresentationAttributes, pointPresentationAttributes, pointSize, dataPointElement, coordinateSystem);

                Plot tbr = new Plot();

                for (int i = 0; i < plot.PlotElements.Count; i++)
                {
                    if (i != 2 && i != 4 && i != 6)
                    {
                        tbr.AddPlotElement(plot.PlotElements[i]);
                    }
                }

                return tbr;
            }

            /// <summary>
            /// Create a new line chart containing two lines.
            /// </summary>
            /// <param name="data1">The data to plot for the first line.</param>
            /// <param name="data2">The data to plot for the second line.</param>
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
            /// <param name="line1PresentationAttributes">Presentation attributes for the first line.</param>
            /// <param name="point1PresentationAttributes">Presentation attributes for the sampled points for the first line.</param>
            /// <param name="point1Size">Size of the symbols drawn at the sampled points for the first line.</param>
            /// <param name="line2PresentationAttributes">Presentation attributes for the second line.</param>
            /// <param name="point2PresentationAttributes">Presentation attributes for the sampled points for the second line.</param>
            /// <param name="point2Size">Size of the symbols drawn at the sampled points for the second line.</param>
            /// <param name="dataPointElement1">Symbol drawn at the sampled points for the first line.</param>
            /// <param name="dataPointElement2">Symbol drawn at the sampled points for the second line.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the line chart.</returns>
            public static Plot LineCharts(IReadOnlyList<IReadOnlyList<double>> data1, IReadOnlyList<IReadOnlyList<double>> data2, bool smooth = false, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                PlotElementPresentationAttributes line1PresentationAttributes = null,
                PlotElementPresentationAttributes point1PresentationAttributes = null,
                double point1Size = 0,
                PlotElementPresentationAttributes line2PresentationAttributes = null,
                PlotElementPresentationAttributes point2PresentationAttributes = null,
                double point2Size = 0,
                IDataPointElement dataPointElement1 = null,
                IDataPointElement dataPointElement2 = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                if (line1PresentationAttributes == null)
                {
                    line1PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(0, 114, 178)), Fill = null };
                }

                if (point1PresentationAttributes == null)
                {
                    point1PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Fill = line1PresentationAttributes.Stroke };
                }

                if (line2PresentationAttributes == null)
                {
                    line2PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(213, 94, 0)), Fill = null };
                }

                if (point2PresentationAttributes == null)
                {
                    point2PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Fill = line2PresentationAttributes.Stroke };
                }

                if (dataPointElement1 == null)
                {
                    dataPointElement1 = new PathDataPointElement();
                }

                if (dataPointElement2 == null)
                {
                    dataPointElement2 = new PathDataPointElement();
                }

                return LineCharts(new IReadOnlyList<IReadOnlyList<double>>[] { data1, data2 }, smooth, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, new PlotElementPresentationAttributes[] { line1PresentationAttributes, line2PresentationAttributes }, new PlotElementPresentationAttributes[] { point1PresentationAttributes, point2PresentationAttributes }, new double[] { point1Size, point2Size }, new IDataPointElement[] { dataPointElement1, dataPointElement2 }, coordinateSystem);
            }

            /// <summary>
            /// Create a new line chart containing two lines.
            /// </summary>
            /// <param name="data1">The data to plot for the first line.</param>
            /// <param name="data2">The data to plot for the second line.</param>
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
            /// <param name="line1PresentationAttributes">Presentation attributes for the first line.</param>
            /// <param name="point1PresentationAttributes">Presentation attributes for the sampled points for the first line.</param>
            /// <param name="point1Size">Size of the symbols drawn at the sampled points for the first line.</param>
            /// <param name="line2PresentationAttributes">Presentation attributes for the second line.</param>
            /// <param name="point2PresentationAttributes">Presentation attributes for the sampled points for the second line.</param>
            /// <param name="point2Size">Size of the symbols drawn at the sampled points for the second line.</param>
            /// <param name="dataPointElement1">Symbol drawn at the sampled points for the first line.</param>
            /// <param name="dataPointElement2">Symbol drawn at the sampled points for the second line.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the line chart.</returns>
            public static Plot LineCharts(IReadOnlyList<double> data1, IReadOnlyList<double> data2, bool smooth = false, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                PlotElementPresentationAttributes line1PresentationAttributes = null,
                PlotElementPresentationAttributes point1PresentationAttributes = null,
                double point1Size = 0,
                PlotElementPresentationAttributes line2PresentationAttributes = null,
                PlotElementPresentationAttributes point2PresentationAttributes = null,
                double point2Size = 0,
                IDataPointElement dataPointElement1 = null,
                IDataPointElement dataPointElement2 = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                double[][] actualData1 = new double[data1.Count][];

                for (int i = 0; i < data1.Count; i++)
                {
                    actualData1[i] = new double[] { i + 1, data1[i] };
                }

                double[][] actualData2 = new double[data2.Count][];

                for (int i = 0; i < data2.Count; i++)
                {
                    actualData2[i] = new double[] { i + 1, data2[i] };
                }

                Plot plot = LineCharts(actualData1, actualData2, smooth, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, line1PresentationAttributes, point1PresentationAttributes, point1Size, line2PresentationAttributes, point2PresentationAttributes, point2Size, dataPointElement1, dataPointElement2, coordinateSystem);

                Plot tbr = new Plot();

                for (int i = 0; i < plot.PlotElements.Count; i++)
                {
                    if (i != 2 && i != 4 && i != 6)
                    {
                        tbr.AddPlotElement(plot.PlotElements[i]);
                    }
                }

                return tbr;
            }

            /// <summary>
            /// Create a new line chart containing multiple lines.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="linePresentationAttributes">Presentation attributes for the line.</param>
            /// <param name="pointPresentationAttributes">Presentation attributes for the sampled points.</param>
            /// <param name="pointSizes">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElements">Symbols drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the line chart.</returns>
            public static Plot LineCharts(IReadOnlyList<IReadOnlyList<(double, double)>> data, bool smooth = false, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> linePresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> pointPresentationAttributes = null,
                IReadOnlyList<double> pointSizes = null,
                IReadOnlyList<IDataPointElement> dataPointElements = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return LineCharts((from el in data select (from el2 in el select new double[] { el2.Item1, el2.Item2 }).ToArray()).ToArray(), smooth, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, linePresentationAttributes, pointPresentationAttributes, pointSizes, dataPointElements, coordinateSystem);
            }

            /// <summary>
            /// Create a new line chart containing multiple lines.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="linePresentationAttributes">Presentation attributes for the line.</param>
            /// <param name="pointPresentationAttributes">Presentation attributes for the sampled points.</param>
            /// <param name="pointSizes">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElements">Symbols drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the line chart.</returns>
            public static Plot LineCharts(IReadOnlyList<IReadOnlyList<double>> data, bool smooth = false, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                PlotElementPresentationAttributes gridPresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> linePresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> pointPresentationAttributes = null,
                IReadOnlyList<double> pointSizes = null,
                IReadOnlyList<IDataPointElement> dataPointElements = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                double[][][] actualData = new double[data.Count][][];

                for (int i = 0; i < data.Count; i++)
                {
                    actualData[i] = new double[data[i].Count][];

                    for (int j = 0; j < data[i].Count; j++)
                    {
                        actualData[i][j] = new double[] { j + 1, data[i][j] };
                    }
                }


                Plot plot = LineCharts(actualData, smooth, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, linePresentationAttributes, pointPresentationAttributes, pointSizes, dataPointElements, coordinateSystem);

                Plot tbr = new Plot();

                for (int i = 0; i < plot.PlotElements.Count; i++)
                {
                    if (i != 2 && i != 4 && i != 6)
                    {
                        tbr.AddPlotElement(plot.PlotElements[i]);
                    }
                }

                return tbr;
            }
        }
    }
}
