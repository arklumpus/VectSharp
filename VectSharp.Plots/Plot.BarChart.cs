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
            /// Create a new bar chart.
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
            /// <returns>A <see cref="Plot"/> containing the bar chart.</returns>
            public static Plot BarChart(IReadOnlyList<double> data, bool vertical = true, double margin = 0.25, double width = 350, double height = 250,
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
                double[][] changedData = new double[data.Count][];

                if (vertical)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        changedData[i] = new double[] { i, data[i] };
                    }
                }
                else
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        changedData[i] = new double[] { data[i], i };
                    }
                }

                Plot barChart = Histogram(changedData, vertical, margin, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, coordinateSystem);

                if (vertical)
                {
                    ((ContinuousAxis)barChart.PlotElements[1]).ArrowSize = 0;
                }
                else
                {
                    ((ContinuousAxis)barChart.PlotElements[2]).ArrowSize = 0;
                }

                Plot tbr = new Plot();

                for (int i = 0; i < barChart.PlotElements.Count; i++)
                {
                    if ((vertical && i != 5 && i != 3 && i != 1) || (!vertical && i != 6 && i != 4 && i != 2))
                    {
                        tbr.AddPlotElement(barChart.PlotElements[i]);
                    }
                }

                return tbr;
            }

            /// <summary>
            /// Create a new bar chart.
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
            /// <returns>A <see cref="Plot"/> containing the bar chart.</returns>
            public static Plot BarChart<T>(IReadOnlyList<(T, double)> data, bool vertical = true, double margin = 0.25, double width = 350, double height = 250,
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
                return BarChart(data, double.NaN, double.NaN, vertical, margin, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, coordinateSystem);
            }


            private static Plot BarChart<T>(IReadOnlyList<(T, double)> data, double overrideMax, double overrideMin, bool vertical = true, double margin = 0.25, double width = 350, double height = 250,
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
                double[][] changedData = new double[data.Count][];

                if (vertical)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        changedData[i] = new double[] { i, data[i].Item2 };
                    }
                }
                else
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        changedData[i] = new double[] { data[i].Item2, i };
                    }
                }

                Plot barChart = Histogram(changedData, overrideMax, overrideMin, vertical, margin, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, coordinateSystem);

                if (vertical)
                {
                    ((ContinuousAxis)barChart.PlotElements[1]).ArrowSize = 0;
                }
                else
                {
                    ((ContinuousAxis)barChart.PlotElements[2]).ArrowSize = 0;
                }

                Plot tbr = new Plot();

                double yLabelsWidth = 0;
                double xLabelsHeight = 0;

                for (int i = 0; i < barChart.PlotElements.Count; i++)
                {
                    if ((vertical && i != 5) || (!vertical && i != 6))
                    {
                        tbr.AddPlotElement(barChart.PlotElements[i]);
                    }
                    else
                    {
                        if (vertical)
                        {
                            ContinuousAxis xAxis = barChart.GetFirst<ContinuousAxis>();

                            IReadOnlyList<double>[] labelData = new IReadOnlyList<double>[data.Count];

                            for (int j = 0; j < data.Count; j++)
                            {
                                labelData[j] = new double[] { j, xAxis.StartPoint[1] * (1 - (double)j / (data.Count - 1)) + xAxis.EndPoint[1] * j / (data.Count - 1) };
                            }

                            DataLabels<IReadOnlyList<double>> xLabels = new Plots.DataLabels<IReadOnlyList<double>>(labelData, barChart.GetFirst<ICoordinateSystem<IReadOnlyList<double>>>()) { Baseline = TextBaselines.Top, Margin = (a, b) => new Point(0, 10), Label = (index, _) => data[index].Item1 };

                            Graphics xLabelsSize = new Graphics();
                            xLabels.Plot(xLabelsSize);
                            xLabelsHeight = xLabelsSize.GetBounds().Size.Height;

                            tbr.AddPlotElement(xLabels);
                        }
                        else
                        {
                            ContinuousAxis yAxis = barChart.GetAll<ContinuousAxis>().ElementAt(1);

                            IReadOnlyList<double>[] labelData = new IReadOnlyList<double>[data.Count];

                            for (int j = 0; j < data.Count; j++)
                            {
                                labelData[j] = new double[] { yAxis.StartPoint[0] * (1 - (double)j / (data.Count - 1)) + yAxis.EndPoint[0] * j / (data.Count - 1), j };
                            }

                            DataLabels<IReadOnlyList<double>> yLabels = new Plots.DataLabels<IReadOnlyList<double>>(labelData, barChart.GetFirst<ICoordinateSystem<IReadOnlyList<double>>>()) { Baseline = TextBaselines.Middle, Alignment = TextAnchors.Right, Margin = (a, b) => new Point(-10, 0), Label = (index, _) => data[index].Item1 };

                            Graphics yLabelsSize = new Graphics();
                            yLabels.Plot(yLabelsSize);
                            yLabelsWidth = yLabelsSize.GetBounds().Size.Width;

                            tbr.AddPlotElement(yLabels);
                        }
                    }
                }

                if (vertical)
                {
                    tbr.GetAll<ContinuousAxisTitle>().ToArray()[0].Position = xLabelsHeight + 20;
                }
                else
                {
                    tbr.GetAll<ContinuousAxisTitle>().ToArray()[1].Position = -20 - yLabelsWidth;
                }

                return tbr;
            }

            /// <summary>
            /// Create a new stacked bar chart.
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
            /// <returns>A <see cref="Plot"/> containing the stacked bar chart.</returns>
            public static Plot StackedBarChart(IReadOnlyList<IReadOnlyList<double>> data, bool vertical = true, double margin = 0.25, double width = 350, double height = 250,
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
                double[][] changedData = new double[data.Count][];

                if (vertical)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        changedData[i] = new double[data[i].Count + 1];

                        changedData[i][0] = i;

                        for (int j = 0; j < data[i].Count; j++)
                        {
                            changedData[i][j + 1] = data[i][j];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        changedData[i] = new double[data[i].Count + 1];

                        changedData[i][1] = i;

                        changedData[i][0] = data[i][0];

                        for (int j = 1; j < data[i].Count; j++)
                        {
                            changedData[i][j + 1] = data[i][j];
                        }
                    }
                }

                Plot barChart = StackedHistogram(changedData, vertical, margin, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, coordinateSystem);

                if (vertical)
                {
                    ((ContinuousAxis)barChart.PlotElements[1]).ArrowSize = 0;
                }
                else
                {
                    ((ContinuousAxis)barChart.PlotElements[2]).ArrowSize = 0;
                }

                Plot tbr = new Plot();

                for (int i = 0; i < barChart.PlotElements.Count; i++)
                {
                    if ((vertical && i != 5 && i != 3 && i != 1) || (!vertical && i != 6 && i != 4 && i != 2))
                    {
                        tbr.AddPlotElement(barChart.PlotElements[i]);
                    }

                }

                return tbr;
            }

            /// <summary>
            /// Create a new stacked bar chart.
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
            /// <returns>A <see cref="Plot"/> containing the stacked bar chart.</returns>
            public static Plot StackedBarChart<T>(IReadOnlyList<(T, IReadOnlyList<double>)> data, bool vertical = true, double margin = 0.25, double width = 350, double height = 250,
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
                double[][] changedData = new double[data.Count][];

                if (vertical)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        changedData[i] = new double[data[i].Item2.Count + 1];

                        changedData[i][0] = i;

                        for (int j = 0; j < data[i].Item2.Count; j++)
                        {
                            changedData[i][j + 1] = data[i].Item2[j];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        changedData[i] = new double[data[i].Item2.Count + 1];

                        changedData[i][1] = i;

                        changedData[i][0] = data[i].Item2[0];

                        for (int j = 1; j < data[i].Item2.Count; j++)
                        {
                            changedData[i][j + 1] = data[i].Item2[j];
                        }
                    }
                }

                Plot barChart = StackedHistogram(changedData, vertical, margin, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, coordinateSystem);

                if (vertical)
                {
                    ((ContinuousAxis)barChart.PlotElements[1]).ArrowSize = 0;
                }
                else
                {
                    ((ContinuousAxis)barChart.PlotElements[2]).ArrowSize = 0;
                }

                Plot tbr = new Plot();

                double xLabelsHeight = 0;
                double yLabelsWidth = 0;

                for (int i = 0; i < barChart.PlotElements.Count; i++)
                {
                    if ((vertical && i != 5) || (!vertical && i != 6))
                    {
                        tbr.AddPlotElement(barChart.PlotElements[i]);
                    }
                    else
                    {
                        if (vertical)
                        {
                            ContinuousAxis xAxis = barChart.GetFirst<ContinuousAxis>();

                            IReadOnlyList<double>[] labelData = new IReadOnlyList<double>[data.Count];

                            for (int j = 0; j < data.Count; j++)
                            {
                                if (data.Count > 1)
                                {
                                    labelData[j] = new double[] { j, xAxis.StartPoint[1] * (1 - (double)j / (data.Count - 1)) + xAxis.EndPoint[1] * j / (data.Count - 1) };
                                }
                                else
                                {
                                    labelData[j] = new double[] { j, xAxis.StartPoint[1] * 0.5 + xAxis.EndPoint[1] * 0.5 };
                                }
                            }

                            DataLabels<IReadOnlyList<double>> xLabels = new Plots.DataLabels<IReadOnlyList<double>>(labelData, barChart.GetFirst<ICoordinateSystem<IReadOnlyList<double>>>()) { Baseline = TextBaselines.Top, Margin = (a, b) => new Point(0, 10), Label = (index, _) => data[index].Item1 };

                            Graphics xLabelsSize = new Graphics();
                            xLabels.Plot(xLabelsSize);
                            xLabelsHeight = xLabelsSize.GetBounds().Size.Height;


                            tbr.AddPlotElement(xLabels);
                        }
                        else
                        {
                            ContinuousAxis yAxis = barChart.GetAll<ContinuousAxis>().ElementAt(1);

                            IReadOnlyList<double>[] labelData = new IReadOnlyList<double>[data.Count];

                            for (int j = 0; j < data.Count; j++)
                            {
                                if (data.Count > 1)
                                {
                                    labelData[j] = new double[] { yAxis.StartPoint[0] * (1 - (double)j / (data.Count - 1)) + yAxis.EndPoint[0] * j / (data.Count - 1), j };
                                }
                                else
                                {
                                    labelData[j] = new double[] { yAxis.StartPoint[0] * 0.5 + yAxis.EndPoint[0] * 0.5, j };
                                }
                            }

                            DataLabels<IReadOnlyList<double>> yLabels = new Plots.DataLabels<IReadOnlyList<double>>(labelData, barChart.GetFirst<ICoordinateSystem<IReadOnlyList<double>>>()) { Baseline = TextBaselines.Middle, Alignment = TextAnchors.Right, Margin = (a, b) => new Point(-10, 0), Label = (index, _) => data[index].Item1 };

                            Graphics yLabelsSize = new Graphics();
                            yLabels.Plot(yLabelsSize);
                            yLabelsWidth = yLabelsSize.GetBounds().Size.Width;

                            tbr.AddPlotElement(yLabels);
                        }
                    }
                }

                if (vertical)
                {
                    tbr.GetAll<ContinuousAxisTitle>().ToArray()[0].Position = xLabelsHeight + 20;
                }
                else
                {
                    tbr.GetAll<ContinuousAxisTitle>().ToArray()[1].Position = -20 - yLabelsWidth;
                }


                return tbr;
            }

            /// <summary>
            /// Create a new clustered bar chart.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the bars go from the X axis up to the sampled values. If this is <see langword="false"/>, the bars go from the Y axis to the sampled values.</param>
            /// <param name="interClusterMargin">Spacing between consecutive bar clusters. This should be a value between 0 and 1.</param>
            /// <param name="intraClusterMargin">Spacing between consecutive bars within the same clusters. This should be a value between 0 and 1.</param>
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
            /// <returns>A <see cref="Plot"/> containing the bar chart.</returns>
            public static Plot ClusteredBarChart(IReadOnlyList<IReadOnlyList<double>> data, bool vertical = true, double interClusterMargin = 0.25, double intraClusterMargin = 0, double width = 350, double height = 250,
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

                ClusteredBars bars;

                if (baselineValue == 0)
                {
                    bars = new ClusteredBars(data, coordinateSystem, vertical) { PresentationAttributes = dataPresentationAttributes, InterClusterMargin = interClusterMargin, IntraClusterMargin = intraClusterMargin };
                }
                else
                {
                    bars = new ClusteredBars(data, new Comparison<IReadOnlyList<double>>(vertical ? new Func<IReadOnlyList<double>, IReadOnlyList<double>, int>((x, y) => Math.Sign(x[0] - y[0])) : (x, y) => Math.Sign(x[1] - y[1])), vertical ? new Func<IReadOnlyList<double>, IReadOnlyList<double>>(pt => new double[] { pt[0], baselineValue }) : pt => new double[] { baselineValue, pt[1] }, coordinateSystem) { PresentationAttributes = dataPresentationAttributes, InterClusterMargin = interClusterMargin, IntraClusterMargin = intraClusterMargin };
                }

                double minX, minY, maxX, maxY, rangeX, rangeY;

                {
                    minX = double.MaxValue;
                    minY = double.MaxValue;

                    maxX = double.MinValue;
                    maxY = double.MinValue;

                    for (int i = 0; i < data.Count; i++)
                    {
                        minX = Math.Min(minX, data[i][0]);
                        minY = Math.Min(minY, data[i][1]);

                        maxX = Math.Max(maxX, data[i][0]);
                        maxY = Math.Max(maxY, data[i][1]);

                        if (vertical)
                        {
                            for (int j = 2; j < data[i].Count; j++)
                            {
                                minY = Math.Min(minY, data[i][j]);
                                maxY = Math.Max(maxY, data[i][j]);
                            }
                        }
                        else
                        {
                            for (int j = 2; j < data[i].Count; j++)
                            {
                                minX = Math.Min(minX, data[i][j]);
                                maxX = Math.Max(maxX, data[i][j]);
                            }
                        }
                    }

                    rangeX = maxX - minX;
                    rangeY = maxY - minY;
                }

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
                        else if (bars.Data.Count == 1)
                        {
                            minX = Math.Min(minX, -0.5);
                            maxX = Math.Max(maxX, 0.5);
                            dataMinX = minX;
                            dataMaxX = maxX;

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
                        else if (bars.Data.Count == 1)
                        {
                            minY = Math.Min(minY, -0.5);
                            maxY = Math.Max(maxY, 0.5);
                            dataMinY = minY;
                            dataMaxY = maxY;

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
                        else if (bars.Data.Count == 1)
                        {
                            minX = Math.Min(minX, -0.5);
                            maxX = Math.Max(maxX, 0.5);
                            dataMinX = minX;
                            dataMaxX = maxX;

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
                        else if (bars.Data.Count == 1)
                        {
                            minY = Math.Min(minY, -0.5);
                            maxY = Math.Max(maxY, 0.5);
                            dataMinY = minY;
                            dataMaxY = maxY;

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
            /// Create a new clustered bar chart.
            /// </summary>
            /// <param name="data">The data to plot.</param>
            /// <param name="vertical">If this is <see langword="true"/> (the default), the bars go from the X axis up to the sampled values. If this is <see langword="false"/>, the bars go from the Y axis to the sampled values.</param>
            /// <param name="interClusterMargin">Spacing between consecutive bar clusters. This should be a value between 0 and 1.</param>
            /// <param name="intraClusterMargin">Spacing between consecutive bars within the same clusters. This should be a value between 0 and 1.</param>
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
            /// <returns>A <see cref="Plot"/> containing the bar chart.</returns>
            public static Plot ClusteredBarChart<T>(IReadOnlyList<(T, IReadOnlyList<double>)> data, bool vertical = true, double interClusterMargin = 0.25, double intraClusterMargin = 0, double width = 350, double height = 250,
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
                double[][] changedData = new double[data.Count][];

                if (vertical)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        changedData[i] = new double[data[i].Item2.Count + 1];

                        changedData[i][0] = i;

                        for (int j = 0; j < data[i].Item2.Count; j++)
                        {
                            changedData[i][j + 1] = data[i].Item2[j];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        changedData[i] = new double[data[i].Item2.Count + 1];

                        changedData[i][1] = i;

                        changedData[i][0] = data[i].Item2[0];

                        for (int j = 1; j < data[i].Item2.Count; j++)
                        {
                            changedData[i][j + 1] = data[i].Item2[j];
                        }
                    }
                }

                Plot barChart = ClusteredBarChart(changedData, vertical, interClusterMargin, intraClusterMargin, width, height, axisPresentationAttributes, axisArrowSize, axisLabelPresentationAttributes, axisTitlePresentationAttributes, xAxisTitle, yAxisTitle, title, titlePresentationAttributes, gridPresentationAttributes, dataPresentationAttributes, coordinateSystem);

                if (vertical)
                {
                    ((ContinuousAxis)barChart.PlotElements[1]).ArrowSize = 0;
                }
                else
                {
                    ((ContinuousAxis)barChart.PlotElements[2]).ArrowSize = 0;
                }

                Plot tbr = new Plot();

                double xLabelsHeight = 0;
                double yLabelsWidth = 0;

                for (int i = 0; i < barChart.PlotElements.Count; i++)
                {
                    if ((vertical && i != 5) || (!vertical && i != 6))
                    {
                        tbr.AddPlotElement(barChart.PlotElements[i]);
                    }
                    else
                    {
                        if (vertical)
                        {
                            ContinuousAxis xAxis = barChart.GetFirst<ContinuousAxis>();

                            IReadOnlyList<double>[] labelData = new IReadOnlyList<double>[data.Count];

                            for (int j = 0; j < data.Count; j++)
                            {
                                if (data.Count > 1)
                                {
                                    labelData[j] = new double[] { j, xAxis.StartPoint[1] * (1 - (double)j / (data.Count - 1)) + xAxis.EndPoint[1] * j / (data.Count - 1) };
                                }
                                else
                                {
                                    labelData[j] = new double[] { j, xAxis.StartPoint[1] * 0.5 + xAxis.EndPoint[1] * 0.5 };
                                }
                            }

                            DataLabels<IReadOnlyList<double>> xLabels = new Plots.DataLabels<IReadOnlyList<double>>(labelData, barChart.GetFirst<ICoordinateSystem<IReadOnlyList<double>>>()) { Baseline = TextBaselines.Top, Margin = (a, b) => new Point(0, 10), Label = (index, _) => data[index].Item1 };

                            Graphics xLabelsSize = new Graphics();
                            xLabels.Plot(xLabelsSize);
                            xLabelsHeight = xLabelsSize.GetBounds().Size.Height;

                            tbr.AddPlotElement(xLabels);
                        }
                        else
                        {
                            ContinuousAxis yAxis = barChart.GetAll<ContinuousAxis>().ElementAt(1);

                            IReadOnlyList<double>[] labelData = new IReadOnlyList<double>[data.Count];

                            for (int j = 0; j < data.Count; j++)
                            {
                                if (data.Count > 1)
                                {
                                    labelData[j] = new double[] { yAxis.StartPoint[0] * 0.5 + yAxis.EndPoint[0] * 0.5, j };
                                }
                                else
                                {
                                    labelData[j] = new double[] { yAxis.StartPoint[0] * 0.5 + yAxis.EndPoint[0] * 0.5, j };
                                }
                            }

                            DataLabels<IReadOnlyList<double>> yLabels = new Plots.DataLabels<IReadOnlyList<double>>(labelData, barChart.GetFirst<ICoordinateSystem<IReadOnlyList<double>>>()) { Baseline = TextBaselines.Middle, Alignment = TextAnchors.Right, Margin = (a, b) => new Point(-10, 0), Label = (index, _) => data[index].Item1 };

                            Graphics yLabelsSize = new Graphics();
                            yLabels.Plot(yLabelsSize);
                            yLabelsWidth = yLabelsSize.GetBounds().Size.Width;

                            tbr.AddPlotElement(yLabels);
                        }
                    }

                }

                if (vertical)
                {
                    tbr.GetAll<ContinuousAxisTitle>().ToArray()[0].Position = xLabelsHeight + 20;
                }
                else
                {
                    tbr.GetAll<ContinuousAxisTitle>().ToArray()[1].Position = -20 - yLabelsWidth;
                }

                return tbr;
            }
        }
    }
}
