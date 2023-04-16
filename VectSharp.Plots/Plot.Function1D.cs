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
            /// Create a new plot for functions of one variable.
            /// </summary>
            /// <param name="functions">The functions to plot.</param>
            /// <param name="min">The minimum value of the function argument to plot.</param>
            /// <param name="max">The maximum value of the function argument to plot.</param>
            /// <param name="resolution">The distance between consecutive function samples.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the functions are plotted as y = f(x). If this is <see langword="false"/>, the functions are plotted as x = f(y).</param>
            /// <param name="smooth">If this is <see langword="false"/>, consecutive function samples are joined by a polyline. If this is <see langword="true"/>, they are joined by a smooth spline passing through them.</param>
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
            /// <param name="linePresentationAttributes">Presentation attributes for the function line.</param>
            /// <param name="pointPresentationAttributes">Presentation attributes for the symbols drawn at the sampled function points.</param>
            /// <param name="pointSize">Size of the symbols drawn at the sampled function points.</param>
            /// <param name="dataPointElements">Symbols drawn at the sampled function points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the bar chart.</returns>
            public static Plot Function(IReadOnlyList<Func<double, double>> functions, double min, double max, double resolution = double.NaN, bool vertical = true, bool smooth = false, double width = 350, double height = 250,
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
                IReadOnlyList<double> pointSize = null,
                IReadOnlyList<IDataPointElement> dataPointElements = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                if (double.IsNaN(resolution))
                {
                    if (coordinateSystem == null)
                    {
                        resolution = (max - min) / 500;
                    }
                    else
                    {
                        resolution = coordinateSystem.Resolution[0];
                    }
                }

                int steps = (int)Math.Ceiling((max - min) / resolution);

                List<double[]>[] data = new List<double[]>[functions.Count];

                for (int i = 0; i < functions.Count; i++)
                {
                    data[i] = new List<double[]>(steps + 1);

                    for (int j = 0; j <= steps; j++)
                    {
                        double x = min + (max - min) / steps * j;
                        double y = functions[i](x);

                        if (!double.IsNaN(y))
                        {
                            if (vertical)
                            {
                                data[i].Add(new double[] { x, y });
                            }
                            else
                            {
                                data[i].Add(new double[] { y, x });
                            }
                        }
                    }
                }

                return Create.LineCharts(data, smooth, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, linePresentationAttributes, pointPresentationAttributes, pointSize, dataPointElements, coordinateSystem);
            }

            /// <summary>
            /// Create a new plot for a function of one variable.
            /// </summary>
            /// <param name="function">The function to plot.</param>
            /// <param name="min">The minimum value of the function argument to plot.</param>
            /// <param name="max">The maximum value of the function argument to plot.</param>
            /// <param name="resolution">The distance between consecutive function samples.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the function is plotted as y = f(x). If this is <see langword="false"/>, the function is plotted as x = f(y).</param>
            /// <param name="smooth">If this is <see langword="false"/>, consecutive function samples are joined by a polyline. If this is <see langword="true"/>, they are joined by a smooth spline passing through them.</param>
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
            /// <param name="linePresentationAttributes">Presentation attributes for the function line.</param>
            /// <param name="pointPresentationAttributes">Presentation attributes for the symbols drawn at the sampled function points.</param>
            /// <param name="pointSize">Size of the symbols drawn at the sampled function points.</param>
            /// <param name="dataPointElement">Symbol drawn at the sampled function points.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the bar chart.</returns>
            public static Plot Function(Func<double, double> function, double min, double max, double resolution = double.NaN, bool vertical = true, bool smooth = false, double width = 350, double height = 250,
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
                return Function(new Func<double, double>[] { function }, min, max, resolution, vertical, smooth, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, linePresentationAttributes == null ? null : new PlotElementPresentationAttributes[] { linePresentationAttributes }, pointPresentationAttributes == null ? null : new PlotElementPresentationAttributes[] { pointPresentationAttributes }, new double[] { pointSize }, dataPointElement == null ? null : new IDataPointElement[] { dataPointElement }, coordinateSystem);
            }
        }
    }
}
