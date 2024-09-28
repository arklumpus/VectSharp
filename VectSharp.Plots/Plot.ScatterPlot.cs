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
            /// Create a new scatter plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="dataPresentationAttributes">Presentation attributes for the data.</param>
            /// <param name="pointSizes">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElements">Symbols drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the scatter plot.</returns>
            public static Plot ScatterPlot(IReadOnlyList<IReadOnlyList<IReadOnlyList<double>>> data, double width = 350, double height = 250,
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

                if (dataPointElements == null)
                {
                    dataPointElements = new PathDataPointElement[] { new PathDataPointElement() };
                }

                if (pointSizes == null)
                {
                    pointSizes = new double[] { 2 };
                }

                List<IReadOnlyList<double>> allData = data.Aggregate(new List<IReadOnlyList<double>>(), (a, b) => { a.AddRange(b); return a; });

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

                Plot tbr = new Plot();
                tbr.AddPlotElements(xGrid, yGrid, xAxis, yAxis, xTicks, yTicks, xLabels, yLabels, xTitle, yTitle);

                for (int i = 0; i < data.Count; i++)
                {
                    ScatterPoints<IReadOnlyList<double>> points = new ScatterPoints<IReadOnlyList<double>>(data[i], coordinateSystem) { PresentationAttributes = dataPresentationAttributes[i % dataPresentationAttributes.Count], Size = pointSizes[i % pointSizes.Count], DataPointElement = dataPointElements[i % dataPointElements.Count] };
                    tbr.AddPlotElement(points);
                }


                TextLabel<IReadOnlyList<double>> titleLabel = new TextLabel<IReadOnlyList<double>>(title, coordinateSystem.ToDataCoordinates((coordinateSystem.ToPlotCoordinates(marginTopLeft) + coordinateSystem.ToPlotCoordinates(marginTopRight)) * 0.5 + new Point(0, -10)), coordinateSystem) { Baseline = TextBaselines.Bottom, PresentationAttributes = titlePresentationAttributes };

                tbr.AddPlotElement(titleLabel);

                return tbr;
            }

            /// <summary>
            /// Create a new scatter plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="dataPresentationAttributes">Presentation attributes for the data.</param>
            /// <param name="pointSize">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElement">Symbol drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the scatter plot.</returns>
            public static Plot ScatterPlot(IReadOnlyList<IReadOnlyList<double>> data, double width = 350, double height = 250,
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
                double pointSize = 2,
                IDataPointElement dataPointElement = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return ScatterPlot(new IReadOnlyList<IReadOnlyList<double>>[] { data }, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes == null ? null : new PlotElementPresentationAttributes[] { dataPresentationAttributes }, new double[] { pointSize }, dataPointElement == null ? null : new IDataPointElement[] { dataPointElement }, coordinateSystem);
            }

            /// <summary>
            /// Create a new scatter plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="dataPresentationAttributes">Presentation attributes for the data.</param>
            /// <param name="pointSizes">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElements">Symbols drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the scatter plot.</returns>
            public static Plot ScatterPlot(IReadOnlyList<IReadOnlyList<(double, double)>> data, double width = 350, double height = 250,
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
               IReadOnlyList<double> pointSizes = null,
               IReadOnlyList<IDataPointElement> dataPointElements = null,
               IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return ScatterPlot((from el in data select (from el2 in el select new double[] { el2.Item1, el2.Item2 }).ToArray()).ToArray(), width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, pointSizes, dataPointElements, coordinateSystem);
            }

            /// <summary>
            /// Create a new scatter plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="dataPresentationAttributes">Presentation attributes for the data.</param>
            /// <param name="pointSize">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElement">Symbol drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the scatter plot.</returns>
            public static Plot ScatterPlot(IReadOnlyList<(double, double)> data, double width = 350, double height = 250,
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
            double pointSize = 2,
            IDataPointElement dataPointElement = null,
            IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return ScatterPlot((from el in data select new double[] { el.Item1, el.Item2 }).ToArray(), width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, pointSize, dataPointElement, coordinateSystem);
            }

            /// <summary>
            /// Create a new scatter plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="dataPresentationAttributes">Presentation attributes for the data.</param>
            /// <param name="pointSize">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElement">Symbol drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the scatter plot.</returns>
            public static Plot ScatterPlot(IReadOnlyList<Point> data, double width = 350, double height = 250,
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
                double pointSize = 2,
                IDataPointElement dataPointElement = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return ScatterPlot((from el in data select new double[] { el.X, el.Y }).ToArray(), width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, pointSize, dataPointElement, coordinateSystem);
            }

            /// <summary>
            /// Create a new scatter plot.
            /// </summary>
            /// <param name="data">The data to plot.</param>
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
            /// <param name="dataPresentationAttributes">Presentation attributes for the data.</param>
            /// <param name="pointSizes">Size of the symbols drawn at the sampled points.</param>
            /// <param name="dataPointElements">Symbols drawn at the sampled points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the scatter plot.</returns>
            public static Plot ScatterPlot(IReadOnlyList<IReadOnlyList<Point>> data, double width = 350, double height = 250,
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
               IReadOnlyList<double> pointSizes = null,
               IReadOnlyList<IDataPointElement> dataPointElements = null,
               IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return ScatterPlot((from el in data select (from el2 in el select new double[] { el2.X, el2.Y }).ToArray()).ToArray(), width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, pointSizes, dataPointElements, coordinateSystem);
            }
        }
    }
}
