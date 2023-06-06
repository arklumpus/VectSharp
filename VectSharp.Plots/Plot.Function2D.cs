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
    partial class Plot
    {
        partial class Create
        {
            /// <summary>
            /// Create a new plot for a function of two variables.
            /// </summary>
            /// <param name="function">The function grid to plot.</param>
            /// <param name="plotType">The type of plot to produce.</param>
            /// <param name="rasterResolutionX">If <paramref name="plotType"/> is <see cref="Function2D.PlotType.Raster"/>, the resolution of the rasterised image on the X axis.</param>
            /// <param name="rasterResolutionY">If <paramref name="plotType"/> is <see cref="Function2D.PlotType.Raster"/>, the resolution of the rasterised image on the Y axis.</param>
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
            /// <param name="pointElement">If <paramref name="plotType"/> is <see cref="Function2D.PlotType.SampledPoints"/>, the symbol drawn at the sampled function points.</param>
            /// <param name="colouring">Function used to associate function values to <see cref="Colour"/>s. You should set this to a function accepting a single argument ranging from 0 to 1 and returning the corresponding <see cref="Colour"/>. You can also use a <see cref="GradientStops"/> object.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the bar chart.</returns>
            public static Plot Function(Function2DGrid function, Function2D.PlotType plotType = Function2D.PlotType.Tessellation, int rasterResolutionX = 512, int rasterResolutionY = 512, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                IDataPointElement pointElement = null,
                Func<double, Colour> colouring = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {

                if (axisPresentationAttributes == null)
                {
                    axisPresentationAttributes = new PlotElementPresentationAttributes();
                }

                if (axisLabelPresentationAttributes == null)
                {
                    axisLabelPresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null };
                }

                if (axisTitlePresentationAttributes == null)
                {
                    axisTitlePresentationAttributes = new PlotElementPresentationAttributes() { Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 14), Stroke = null };
                }

                if (pointElement == null)
                {
                    pointElement = new PathDataPointElement();
                }

                if (coordinateSystem == null)
                {
                    coordinateSystem = new LinearCoordinateSystem2D(function.DataPoints, width, height);
                }

                if (titlePresentationAttributes == null)
                {
                    titlePresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 18) };
                }

                if (colouring == null)
                {
                    colouring = Gradients.Viridis;
                }

                (double minX, double minY, double maxX, double maxY, double rangeX, double rangeY) = GetDataRange(function.DataPoints);

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

                //Grid xGrid = new Grid(p1, p2, p3, p4, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };
                //Grid yGrid = new Grid(p5, p6, p7, p8, coordinateSystem) { IntervalCount = 5, PresentationAttributes = gridPresentationAttributes };

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

                TextLabel<IReadOnlyList<double>> titleLabel = new TextLabel<IReadOnlyList<double>>(title, coordinateSystem.ToDataCoordinates(new Point((topLeft.X + topRight.X) * 0.5, (topLeft.Y + topRight.Y) * 0.5 - 20)), coordinateSystem) { Baseline = TextBaselines.Bottom, PresentationAttributes = titlePresentationAttributes };

                Function2D functionPlot = new Function2D(function, coordinateSystem)
                {
                    RasterResolutionX = rasterResolutionX,
                    RasterResolutionY = rasterResolutionY,
                    SampledPointElement = pointElement,
                    Type = plotType,
                    Colouring = colouring
                };

                Plot tbr = new Plot();
                tbr.AddPlotElements(functionPlot, xAxis, yAxis, xTicks, yTicks, xLabels, yLabels, xTitle, yTitle);

                tbr.AddPlotElement(titleLabel);
                
                return tbr;
            }

            /// <summary>
            /// Create a new plot for a function of two variables.
            /// </summary>
            /// <param name="function">The function to plot.</param>
            /// <param name="minX">The minimum value of the first function argument to plot.</param>
            /// <param name="minY">The minimum value of the second function argument to plot.</param>
            /// <param name="maxX">The maximum value of the first function argument to plot.</param>
            /// <param name="maxY">The maximum value of the second function argument to plot.</param>
            /// <param name="resolutionX">The distance between consecutive function samples on the X axis.</param>
            /// <param name="resolutionY">The distance between consecutive function samples on the Y axis.</param>
            /// <param name="gridType">The type of grid along which to sample the function. If it is <see langword="null" />, this is determined automatically.</param>
            /// <param name="plotType">The type of plot to produce.</param>
            /// <param name="rasterResolutionX">If <paramref name="plotType"/> is <see cref="Function2D.PlotType.Raster"/>, the resolution of the rasterised image on the X axis.</param>
            /// <param name="rasterResolutionY">If <paramref name="plotType"/> is <see cref="Function2D.PlotType.Raster"/>, the resolution of the rasterised image on the Y axis.</param>
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
            /// <param name="pointElement">If <paramref name="plotType"/> is <see cref="Function2D.PlotType.SampledPoints"/>, the symbol drawn at the sampled function points.</param>
            /// <param name="colouring">Function used to associate function values to <see cref="Colour"/>s. You should set this to a function accepting a single argument ranging from 0 to 1 and returning the corresponding <see cref="Colour"/>. You can also use a <see cref="GradientStops"/> object.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the bar chart.</returns>
            public static Plot Function(Func<double[], double> function, double minX, double minY, double maxX, double maxY, double resolutionX = double.NaN, double resolutionY = double.NaN, Function2DGrid.GridType? gridType = null, Function2D.PlotType plotType = Function2D.PlotType.Tessellation, int rasterResolutionX = 512, int rasterResolutionY = 512, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                IDataPointElement pointElement = null,
                Func<double, Colour> colouring = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                if (double.IsNaN(resolutionX) && double.IsNaN(resolutionY))
                {
                    double ratio = width / height;

                    double stepsY = Math.Sqrt(4096 / ratio);
                    double stepsX = ratio * stepsY;

                    resolutionX = (maxX - minX) / stepsX;
                    resolutionY = (maxY - minY) / stepsY;
                }
                else if (double.IsNaN(resolutionX) && !double.IsNaN(resolutionY))
                {
                    double ratio = width / height;

                    double stepsY = (maxY - minY) / resolutionY;
                    double stepsX = ratio * stepsY;

                    resolutionX = (maxX - minX) / stepsX;
                }
                else if (!double.IsNaN(resolutionX) && double.IsNaN(resolutionY))
                {
                    double ratio = width / height;

                    double stepsX = (maxX - minX) / resolutionX;
                    double stepsY = stepsX / ratio;

                    resolutionY = (maxY - minY) / stepsY;
                }

                int stepsCountX = (int)Math.Floor((maxX - minX) / resolutionX);
                int stepsCountY = (int)Math.Floor((maxY - minY) / resolutionY);

                if (gridType == null)
                {
                    switch (plotType)
                    {
                        case Function2D.PlotType.SampledPoints:
                            gridType = Function2DGrid.GridType.HexagonHorizontal;
                            break;

                        case Function2D.PlotType.Tessellation:
                            if ((stepsCountX + 1) * (stepsCountY + 1) <= 4096)
                            {
                                gridType = Function2DGrid.GridType.Irregular;
                            }
                            else
                            {
                                gridType = Function2DGrid.GridType.HexagonHorizontal;
                            }
                            break;

                        case Function2D.PlotType.Raster:
                            gridType = Function2DGrid.GridType.Rectangular;
                            break;
                    }
                }

                Function2DGrid grid = new Function2DGrid(function, minX, minY, maxX, maxY, stepsCountX, stepsCountY, gridType.Value);

                return Function(grid, plotType, rasterResolutionX, rasterResolutionY, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, pointElement, colouring, coordinateSystem);
            }

            /// <summary>
            /// Create a new plot for a function of two variables.
            /// </summary>
            /// <param name="function">The function to plot.</param>
            /// <param name="minX">The minimum value of the first function argument to plot.</param>
            /// <param name="minY">The minimum value of the second function argument to plot.</param>
            /// <param name="maxX">The maximum value of the first function argument to plot.</param>
            /// <param name="maxY">The maximum value of the second function argument to plot.</param>
            /// <param name="resolutionX">The distance between consecutive function samples on the X axis.</param>
            /// <param name="resolutionY">The distance between consecutive function samples on the Y axis.</param>
            /// <param name="gridType">The type of grid along which to sample the function. If it is <see langword="null" />, this is determined automatically.</param>
            /// <param name="plotType">The type of plot to produce.</param>
            /// <param name="rasterResolutionX">If <paramref name="plotType"/> is <see cref="Function2D.PlotType.Raster"/>, the resolution of the rasterised image on the X axis.</param>
            /// <param name="rasterResolutionY">If <paramref name="plotType"/> is <see cref="Function2D.PlotType.Raster"/>, the resolution of the rasterised image on the Y axis.</param>
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
            /// <param name="pointElement">If <paramref name="plotType"/> is <see cref="Function2D.PlotType.SampledPoints"/>, the symbol drawn at the sampled function points.</param>
            /// <param name="colouring">Function used to associate function values to <see cref="Colour"/>s. You should set this to a function accepting a single argument ranging from 0 to 1 and returning the corresponding <see cref="Colour"/>. You can also use a <see cref="GradientStops"/> object.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the bar chart.</returns>
            public static Plot Function(Func<double, double, double> function, double minX, double minY, double maxX, double maxY, double resolutionX = double.NaN, double resolutionY = double.NaN, Function2DGrid.GridType? gridType = null, Function2D.PlotType plotType = Function2D.PlotType.Tessellation, int rasterResolutionX = 512, int rasterResolutionY = 512, double width = 350, double height = 250,
                PlotElementPresentationAttributes axisPresentationAttributes = null,
                double axisArrowSize = 3,
                PlotElementPresentationAttributes axisLabelPresentationAttributes = null,
                PlotElementPresentationAttributes axisTitlePresentationAttributes = null,
                string xAxisTitle = null,
                string yAxisTitle = null,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                IDataPointElement pointElement = null,
                Func<double, Colour> colouring = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return Function(x => function(x[0], x[1]), minX, minY, maxX, maxY, resolutionX, resolutionY, gridType, plotType, rasterResolutionX, rasterResolutionY, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, pointElement, colouring, coordinateSystem);
            }
        }
    }
}
