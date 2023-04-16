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
            /// Create a new doughnut chart containing multiple doughnuts.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="clockwise">If this is <see langword="false"/> (the default), the slices are drawn in anti-clockwise direction. If this is <see langword="true"/>, they are drawn in clockwise direction.</param>
            /// <param name="startAngle">The initial angle at which the slices are drawn.</param>
            /// <param name="innerRadius">The radius of the doughnut "hole".</param>
            /// <param name="width">The width of the plot.</param>
            /// <param name="height">The height of the plot.</param>
            /// <param name="title">Title for the plot.</param>
            /// <param name="titlePresentationAttributes">Presentation attributes for the plot title.</param>
            /// <param name="dataPresentationAttributes">Presentation attributes for the slices.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the doughnut chart.</returns>
            public static Plot DoughnutCharts(IReadOnlyList<IReadOnlyList<double>> data, bool clockwise = false, double startAngle = 0, double innerRadius = 0.5, double width = 350, double height = 250,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> dataPresentationAttributes = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {

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

                int rowCount = (int)Math.Round(Math.Sqrt(data.Count));

                int[] elementsPerRow = new int[rowCount];

                for (int i = 0; i < rowCount; i++)
                {
                    elementsPerRow[i] = data.Count / rowCount;

                    if (data.Count - (data.Count / rowCount) * rowCount > i)
                    {
                        elementsPerRow[i]++;
                    }
                }

                if (coordinateSystem == null)
                {
                    double pieWidth = 2.5 * elementsPerRow.Max() + 2;
                    double pieHeight = 2.5 * rowCount + 2;

                    double aspectRatio = pieWidth / pieHeight;
                    double targetAspectRatio = width / height;

                    if (targetAspectRatio > aspectRatio)
                    {
                        pieWidth = targetAspectRatio * pieHeight;
                    }
                    else
                    {
                        pieHeight = pieWidth / targetAspectRatio;
                    }

                    coordinateSystem = new LinearCoordinateSystem2D(-1, pieWidth - 1, -pieHeight + 1, 1, width, height);
                }

                if (titlePresentationAttributes == null)
                {
                    titlePresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 18) };
                }

                Plot tbr = new Plot();


                for (int i = 0; i < data.Count; i++)
                {
                    int column = i;
                    int row = 0;

                    for (int j = 0; j < rowCount; j++)
                    {
                        if (elementsPerRow[j] <= column)
                        {
                            column -= elementsPerRow[j];
                            row++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    Pie pie = new Pie(data[i], new double[] { column * 2.5 + 1.25 * (elementsPerRow.Max() - elementsPerRow[row]), -row * 2.5 }, new double[] { 1, 1 }, coordinateSystem)
                    {
                        PresentationAttributes = dataPresentationAttributes,
                        Clockwise = clockwise,
                        StartAngle = startAngle,
                        InnerRadius = new double[] { innerRadius, innerRadius }
                    };

                    tbr.AddPlotElement(pie);
                }

                Point top = coordinateSystem.ToPlotCoordinates(new double[] { (elementsPerRow.Max() - 1) * 0.5 * 2.5, 1 });
                double[] titleCentre = coordinateSystem.ToDataCoordinates(new Point(top.X, top.Y - 10));

                TextLabel<IReadOnlyList<double>> titleLabel = new TextLabel<IReadOnlyList<double>>(title, titleCentre, coordinateSystem) { Baseline = TextBaselines.Bottom, PresentationAttributes = titlePresentationAttributes };
                tbr.AddPlotElement(titleLabel);


                return tbr;
            }

            /// <summary>
            /// Create a new doughnut chart.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="clockwise">If this is <see langword="false"/> (the default), the slices are drawn in anti-clockwise direction. If this is <see langword="true"/>, they are drawn in clockwise direction.</param>
            /// <param name="startAngle">The initial angle at which the slices are drawn.</param>
            /// <param name="innerRadius">The radius of the doughnut "hole".</param>
            /// <param name="width">The width of the plot.</param>
            /// <param name="height">The height of the plot.</param>
            /// <param name="title">Title for the plot.</param>
            /// <param name="titlePresentationAttributes">Presentation attributes for the plot title.</param>
            /// <param name="dataPresentationAttributes">Presentation attributes for the slices.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the doughnut chart.</returns>
            public static Plot DoughnutChart(IReadOnlyList<double> data, bool clockwise = false, double startAngle = 0, double innerRadius = 0.5, double width = 350, double height = 250,
             string title = null,
             PlotElementPresentationAttributes titlePresentationAttributes = null,
             IReadOnlyList<PlotElementPresentationAttributes> dataPresentationAttributes = null,
             IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return DoughnutCharts(new IReadOnlyList<double>[] { data }, clockwise, startAngle, innerRadius, width, height, title, titlePresentationAttributes, dataPresentationAttributes, coordinateSystem);
            }


            /// <summary>
            /// Create a new pie chart.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="clockwise">If this is <see langword="false"/> (the default), the slices are drawn in anti-clockwise direction. If this is <see langword="true"/>, they are drawn in clockwise direction.</param>
            /// <param name="startAngle">The initial angle at which the slices are drawn.</param>
            /// <param name="width">The width of the plot.</param>
            /// <param name="height">The height of the plot.</param>
            /// <param name="title">Title for the plot.</param>
            /// <param name="titlePresentationAttributes">Presentation attributes for the plot title.</param>
            /// <param name="dataPresentationAttributes">Presentation attributes for the slices.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the pie chart.</returns>
            public static Plot PieChart(IReadOnlyList<double> data, bool clockwise = false, double startAngle = 0, double width = 350, double height = 250,
             string title = null,
             PlotElementPresentationAttributes titlePresentationAttributes = null,
             IReadOnlyList<PlotElementPresentationAttributes> dataPresentationAttributes = null,
             IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return DoughnutCharts(new IReadOnlyList<double>[] { data }, clockwise, startAngle, 0, width, height, title, titlePresentationAttributes, dataPresentationAttributes, coordinateSystem);
            }

            /// <summary>
            /// Create a new pie chart containing multiple pies.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="clockwise">If this is <see langword="false"/> (the default), the slices are drawn in anti-clockwise direction. If this is <see langword="true"/>, they are drawn in clockwise direction.</param>
            /// <param name="startAngle">The initial angle at which the slices are drawn.</param>
            /// <param name="width">The width of the plot.</param>
            /// <param name="height">The height of the plot.</param>
            /// <param name="title">Title for the plot.</param>
            /// <param name="titlePresentationAttributes">Presentation attributes for the plot title.</param>
            /// <param name="dataPresentationAttributes">Presentation attributes for the slices.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the pie chart.</returns>
            public static Plot PieCharts(IReadOnlyList<IReadOnlyList<double>> data, bool clockwise = false, double startAngle = 0, double width = 350, double height = 250,
               string title = null,
               PlotElementPresentationAttributes titlePresentationAttributes = null,
               IReadOnlyList<PlotElementPresentationAttributes> dataPresentationAttributes = null,
               IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {
                return DoughnutCharts(data, clockwise, startAngle, 0, width, height, title, titlePresentationAttributes, dataPresentationAttributes, coordinateSystem);
            }

            /// <summary>
            /// Create a new doughnut chart containing multiple concentric doughnuts.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="clockwise">If this is <see langword="false"/> (the default), the slices are drawn in anti-clockwise direction. If this is <see langword="true"/>, they are drawn in clockwise direction.</param>
            /// <param name="startAngle">The initial angle at which the slices are drawn.</param>
            /// <param name="innerRadius">The radius of the innermost doughnut "hole". If this is <see cref="double.NaN"/>, it is determined automatically based on the number of concentric doughnuts.</param>
            /// <param name="margin">The spacing between consecutive doughnut rings, ranging from 0 to 1.</param>
            /// <param name="width">The width of the plot.</param>
            /// <param name="height">The height of the plot.</param>
            /// <param name="title">Title for the plot.</param>
            /// <param name="titlePresentationAttributes">Presentation attributes for the plot title.</param>
            /// <param name="dataPresentationAttributes">Presentation attributes for the slices.</param>
            /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
            /// <returns>A <see cref="Plot"/> containing the doughnut chart.</returns>
            public static Plot StackedDoughnutChart(IReadOnlyList<IReadOnlyList<double>> data, bool clockwise = false, double startAngle = 0, double innerRadius = double.NaN, double margin = 0.1, double width = 350, double height = 250,
                string title = null,
                PlotElementPresentationAttributes titlePresentationAttributes = null,
                IReadOnlyList<PlotElementPresentationAttributes> dataPresentationAttributes = null,
                IContinuousInvertibleCoordinateSystem coordinateSystem = null)
            {

                if (dataPresentationAttributes == null)
                {
                    dataPresentationAttributes = new PlotElementPresentationAttributes[] { new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(0, 114, 178)), Fill = new SolidColourBrush(Colour.FromRgb(0, 114, 178)), LineWidth = 0.5 },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(213, 94, 0)), Fill = new SolidColourBrush(Colour.FromRgb(213, 94, 0)), LineWidth = 0.5 },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(204, 121, 167)), Fill = new SolidColourBrush(Colour.FromRgb(204, 121, 167)), LineWidth = 0.5 },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(230, 159, 0)), Fill = new SolidColourBrush(Colour.FromRgb(230, 159, 0)), LineWidth = 0.5 },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(86, 180, 233)), Fill = new SolidColourBrush(Colour.FromRgb(86, 180, 233)), LineWidth = 0.5 },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(0, 158, 115)), Fill = new SolidColourBrush(Colour.FromRgb(0, 158, 115)), LineWidth = 0.5 },
                                                                                       new PlotElementPresentationAttributes() { Stroke = new SolidColourBrush(Colour.FromRgb(240, 228, 66)), Fill = new SolidColourBrush(Colour.FromRgb(240, 228, 66)), LineWidth = 0.5 } };
                }

                if (double.IsNaN(innerRadius))
                {
                    innerRadius = Math.Max(0.1, Math.Pow(2, -data.Count));
                }

                if (coordinateSystem == null)
                {
                    double pieWidth = 2.5 + 2;
                    double pieHeight = 2.5 + 2;

                    double aspectRatio = pieWidth / pieHeight;
                    double targetAspectRatio = width / height;

                    if (targetAspectRatio > aspectRatio)
                    {
                        pieWidth = targetAspectRatio * pieHeight;
                    }
                    else
                    {
                        pieHeight = pieWidth / targetAspectRatio;
                    }

                    coordinateSystem = new LinearCoordinateSystem2D(-1, pieWidth - 1, -pieHeight + 1, 1, width, height);
                }

                if (titlePresentationAttributes == null)
                {
                    titlePresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 18) };
                }

                double actualMargin = margin * (1 - innerRadius) / data.Count;

                Plot tbr = new Plot();

                for (int i = 0; i < data.Count; i++)
                {

                    Pie pie = new Pie(data[i], new double[] { 0, 0 }, new double[] { innerRadius + (1 - innerRadius) * (i + 1) / data.Count - actualMargin * 0.5, innerRadius + (1 - innerRadius) * (i + 1) / data.Count - actualMargin * 0.5 }, coordinateSystem)
                    {
                        PresentationAttributes = dataPresentationAttributes,
                        Clockwise = clockwise,
                        StartAngle = startAngle,
                        InnerRadius = new double[] { innerRadius + (1 - innerRadius) * i / data.Count + actualMargin * 0.5, innerRadius + (1 - innerRadius) * i / data.Count + actualMargin * 0.5 }
                    };

                    tbr.AddPlotElement(pie);
                }

                Point top = coordinateSystem.ToPlotCoordinates(new double[] { 0, 1 });
                double[] titleCentre = coordinateSystem.ToDataCoordinates(new Point(top.X, top.Y - 10));

                TextLabel<IReadOnlyList<double>> titleLabel = new TextLabel<IReadOnlyList<double>>(title, titleCentre, coordinateSystem) { Baseline = TextBaselines.Bottom, PresentationAttributes = titlePresentationAttributes };
                tbr.AddPlotElement(titleLabel);


                return tbr;
            }
        }
    }
}
