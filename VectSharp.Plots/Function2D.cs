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

using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace VectSharp.Plots
{
    /// <summary>
    /// Represents a function of two variables that has been sampled in some points.
    /// </summary>
    public class Function2DGrid
    {
        /// <summary>
        /// Describes the arrangement of the points that have been sampled.
        /// </summary>
        public enum GridType
        {
            /// <summary>
            /// Points have been sampled along a rectangular grid.
            /// </summary>
            Rectangular,

            /// <summary>
            /// Points have been sampled along a grid composed of hexagons with a diagonal
            /// parallel to the horizontal.
            /// </summary>
            HexagonHorizontal,

            /// <summary>
            /// Points have been sampled along a grid composed of hexagons with a diagonal
            /// parallel to the vertical.
            /// </summary>
            HexagonVertical,

            /// <summary>
            /// Points have been sampled without any particular criterion.
            /// </summary>
            Irregular
        }

        /// <summary>
        /// The points where the function has been sampled.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<double>> DataPoints { get; }

        /// <summary>
        /// The minimum X value that has been sampled.
        /// </summary>
        public double MinX { get; }

        /// <summary>
        /// The maximum X value that has been sampled.
        /// </summary>
        public double MaxX { get; }

        /// <summary>
        /// The minimum Y value that has been sampled.
        /// </summary>
        public double MinY { get; }

        /// <summary>
        /// The maximum Y value that has been sampled.
        /// </summary>
        public double MaxY { get; }

        /// <summary>
        /// The minimum value that has been obtained for the function.
        /// </summary>
        public double MinZ { get; }

        /// <summary>
        /// The maximum value that has been obtained for the function.
        /// </summary>
        public double MaxZ { get; }
        
        /// <summary>
        /// If the function has been sampled along a regular grid, the number of steps
        /// between <see cref="MinX"/> and <see cref="MaxX"/> on the X axis.
        /// </summary>
        public int StepsX { get; private set; }

        /// <summary>
        /// If the function has been sampled along a regular grid, the number of steps
        /// between <see cref="MinY"/> and <see cref="MaxY"/> on the X axis.
        /// </summary>
        public int StepsY { get; private set; }

        /// <summary>
        /// The type of grid.
        /// </summary>
        public GridType Type { get; private set; }

        /// <summary>
        /// Create a new <see cref="Function2DGrid"/> from a list of sampled values.
        /// </summary>
        /// <param name="dataPoints">The sampled values. Each element of this <see cref="IReadOnlyList{T}"/> should
        /// be a collection of three values: the X coordinate of the sampled point, the Y coordinate of the sampled
        /// point, and the value of the function at that point.</param>
        public Function2DGrid(IReadOnlyList<IReadOnlyList<double>> dataPoints)
        {
            this.Type = GridType.Irregular;
            this.DataPoints = dataPoints.ToArray();

            MinX = double.MaxValue;
            MaxX = double.MinValue;
            MinY = double.MaxValue;
            MaxY = double.MinValue;
            MinZ = double.MaxValue;
            MaxZ = double.MinValue;

            this.StepsX = 0;
            this.StepsY = 0;

            for (int i = 0; i < dataPoints.Count; i++)
            {
                MinX = Math.Min(MinX, dataPoints[i][0]);
                MinY = Math.Min(MinY, dataPoints[i][1]);
                MinZ = Math.Min(MinZ, dataPoints[i][2]);

                MaxX = Math.Max(MaxX, dataPoints[i][0]);
                MaxY = Math.Max(MaxY, dataPoints[i][1]);
                MaxZ = Math.Max(MaxZ, dataPoints[i][2]);
            }
        }

        /// <summary>
        /// Create a new <see cref="Function2DGrid"/> by sampling the provided function.
        /// </summary>
        /// <param name="function">The function to sample.</param>
        /// <param name="minX">The minimum X value at which the function should be sampled.</param>
        /// <param name="minY">The minimum Y value at which the function should be sampled.</param>
        /// <param name="maxX">The maximum X value at which the function should be sampled.</param>
        /// <param name="maxY">The maximum Y value at which the function should be sampled.</param>
        /// <param name="stepsX">If <paramref name="type"/> is not <see cref="GridType.Irregular"/>,
        /// the number of steps between <paramref name="minX"/> and <paramref name="maxX"/> on the X axis.
        /// Otherwise, the number of sampled points is determined by multiplying <paramref name="stepsX"/> and
        /// <paramref name="stepsY"/> together.</param>
        /// <param name="stepsY">If <paramref name="type"/> is not <see cref="GridType.Irregular"/>,
        /// the number of steps between <paramref name="minY"/> and <paramref name="maxY"/> on the Y axis.
        /// Otherwise, the number of sampled points is determined by multiplying <paramref name="stepsX"/> and
        /// <paramref name="stepsY"/> together.</param>
        /// <param name="type">The strategy used to select points to sample. If this is <see cref="GridType.Irregular"/>,
        /// uniformly distributed random points between (<paramref name="minX"/>, <paramref name="minY"/>) and
        /// (<paramref name="maxX"/>, <paramref name="maxY"/>) are sampled.</param>
        public Function2DGrid(Func<double[], double> function, double minX, double minY, double maxX, double maxY, int stepsX, int stepsY, GridType type)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            StepsX = stepsX;
            StepsY = stepsY;
            Type = type;
            MinZ = double.MaxValue;
            MaxZ = double.MinValue;

            if (type == GridType.HexagonHorizontal)
            {
                List<double[]> dataPoints = new List<double[]>((stepsX + 1) * (stepsY + 1));

                for (int y = 0; y <= stepsY; y++)
                {
                    for (int x = 0; x <= stepsX; x++)
                    {
                        if (x % 2 == 0 || y < stepsY)
                        {

                            double realY;

                            if (x % 2 == 0)
                            {
                                realY = minY + (maxY - minY) / stepsY * y;
                            }
                            else
                            {
                                realY = minY + (maxY - minY) / stepsY * (y + 0.5);
                            }

                            double realX = minX + (maxX - minX) / stepsX * x;
                            double z = function(new double[] { realX, realY });

                            if (!double.IsNaN(z))
                            {
                                MinZ = Math.Min(MinZ, z);
                                MaxZ = Math.Max(MaxZ, z);

                                dataPoints.Add(new double[] { realX, realY, z });
                            }
                            else
                            {
                                Type = GridType.Irregular;
                            }
                        }
                    }
                }

                this.DataPoints = dataPoints;
            }
            else if (type == GridType.HexagonVertical)
            {
                List<double[]> dataPoints = new List<double[]>((stepsX + 1) * (stepsY + 1));

                for (int y = 0; y <= stepsY; y++)
                {
                    double realY = minY + (maxY - minY) / stepsY * y;

                    for (int x = 0; x <= stepsX; x++)
                    {
                        if (y % 2 == 0 || x < stepsX)
                        {

                            double realX;

                            if (y % 2 == 0)
                            {
                                realX = minX + (maxX - minX) / stepsX * x;
                            }
                            else
                            {
                                realX = minX + (maxX - minX) / stepsX * (x + 0.5);
                            }

                            double z = function(new double[] { realX, realY });

                            if (!double.IsNaN(z))
                            {
                                MinZ = Math.Min(MinZ, z);
                                MaxZ = Math.Max(MaxZ, z);

                                dataPoints.Add(new double[] { realX, realY, z });
                            }
                            else
                            {
                                Type = GridType.Irregular;
                            }
                        }
                    }
                }

                this.DataPoints = dataPoints;
            }
            else if (type == GridType.Rectangular)
            {
                List<double[]> dataPoints = new List<double[]>((stepsX + 1) * (stepsY + 1));

                for (int y = 0; y <= stepsY; y++)
                {
                    double realY = minY + (maxY - minY) / stepsY * y;
                    for (int x = 0; x <= stepsX; x++)
                    {
                        double realX = minX + (maxX - minX) / stepsX * x;
                        double z = function(new double[] { realX, realY });

                        if (!double.IsNaN(z))
                        {
                            MinZ = Math.Min(MinZ, z);
                            MaxZ = Math.Max(MaxZ, z);

                            dataPoints.Add(new double[] { realX, realY, z });
                        }
                        else
                        {
                            Type = GridType.Irregular;
                        }
                    }
                }

                this.DataPoints = dataPoints;
            }
            else //type == GridType.Irregular
            {
                double[] xs = ContinuousUniform.Samples(minX, maxX).Take((stepsX + 1) * (stepsY + 1)).ToArray();
                double[] ys = ContinuousUniform.Samples(minY, maxY).Take((stepsX + 1) * (stepsY + 1)).ToArray();

                List<double[]> dataPoints = new List<double[]>((stepsX + 1) * (stepsY + 1));

                for (int i = 0; i < (stepsX + 1) * (stepsY + 1); i++)
                {
                    double z = function(new double[] { xs[i], ys[i] });

                    if (!double.IsNaN(z))
                    {
                        MinZ = Math.Min(MinZ, z);
                        MaxZ = Math.Max(MaxZ, z);

                        dataPoints.Add(new double[] { xs[i], ys[i], z });
                    }
                }

                this.DataPoints = dataPoints;
            }
        }

        /// <summary>
        /// Converts a hexagonal grid into a rectangular grid.
        /// </summary>
        /// <returns>If <see cref="Type"/> is <see cref="GridType.Rectangular"/>, this method returns the current instance. <br/>
        /// If <see cref="Type"/> is <see cref="GridType.HexagonHorizontal"/> or <see cref="GridType.HexagonVertical"/>, a 
        /// new <see cref="Function2DGrid"/> with <see cref="Type"/> equal to <see cref="GridType.Rectangular"/> is returned.
        /// The sampled points in this grid are obtained by performing a bilinear interpolation on the sampled points from
        /// this grid. The returned grid will always be "denser" than the current instance.<br/>
        /// If <see cref="Type"/> is <see cref="GridType.Irregular"/>, an <see cref="InvalidOperationException"/> is thrown.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Type"/> is <see cref="GridType.Irregular"/>.</exception>
        public Function2DGrid ToRectangular()
        {
            if (this.Type == GridType.Rectangular)
            {
                return this;
            }
            else if (this.Type == GridType.HexagonHorizontal)
            {
                IReadOnlyList<double>[] data = new IReadOnlyList<double>[(2 * this.StepsY + 1) * (this.StepsX + 1)];

                int newStepsX = this.StepsX;
                int newStepsY = this.StepsY * 2;

                for (int i = 0; i < this.DataPoints.Count; i++)
                {
                    int x = (int)Math.Round((this.DataPoints[i][0] - this.MinX) / (this.MaxX - this.MinX) * newStepsX);
                    int y = (int)Math.Round((this.DataPoints[i][1] - this.MinY) / (this.MaxY - this.MinY) * newStepsY);

                    data[y * (newStepsX + 1) + x] = this.DataPoints[i];
                }

                for (int y = 0; y <= newStepsY; y++)
                {
                    for (int x = 0; x <= newStepsX; x++)
                    {
                        if (data[y * (newStepsX + 1) + x] == null)
                        {
                            double xVal = this.MinX + (this.MaxX - this.MinX) / newStepsX * x;
                            double yVal = this.MinY + (this.MaxY - this.MinY) / newStepsY * y;

                            double zVal;

                            if (x > 0)
                            {
                                if (y > 0)
                                {
                                    if (x < newStepsX)
                                    {
                                        if (y < newStepsY)
                                        {
                                            zVal = (data[(y - 1) * (newStepsX + 1) + x][2] + data[(y + 1) * (newStepsX + 1) + x][2] + data[y * (newStepsX + 1) + (x - 1)][2] + data[y * (newStepsX + 1) + (x + 1)][2]) * 0.25;
                                        }
                                        else
                                        {
                                            zVal = (data[y * (newStepsX + 1) + (x - 1)][2] + data[y * (newStepsX + 1) + (x + 1)][2]) * 0.5;
                                        }
                                    }
                                    else
                                    {
                                        if (y < newStepsY)
                                        {
                                            zVal = (data[(y - 1) * (newStepsX + 1) + x][2] + data[(y + 1) * (newStepsX + 1) + x][2]) * 0.5;
                                        }
                                        else
                                        {
                                            zVal = (data[y * (newStepsX + 1) + (x - 1)][2] + data[(y - 1) * (newStepsX + 1) + x][2]) * 0.5;
                                        }
                                    }
                                }
                                else
                                {
                                    if (x < newStepsX)
                                    {
                                        zVal = (data[y * (newStepsX + 1) + (x - 1)][2] + data[y * (newStepsX + 1) + (x + 1)][2]) * 0.5;
                                    }
                                    else
                                    {
                                        zVal = (data[y * (newStepsX + 1) + (x - 1)][2] + data[(y + 1) * (newStepsX + 1) + x][2]) * 0.5;
                                    }
                                }
                            }
                            else
                            {
                                if (y > 0)
                                {
                                    if (y < newStepsY)
                                    {
                                        zVal = (data[(y - 1) * (newStepsX + 1) + x][2] + data[(y + 1) * (newStepsX + 1) + x][2]) * 0.5;
                                    }
                                    else
                                    {
                                        zVal = (data[y * (newStepsX + 1) + (x + 1)][2] + data[(y - 1) * (newStepsX + 1) + x][2]) * 0.5;
                                    }
                                }
                                else
                                {
                                    zVal = (data[y * (newStepsX + 1) + (x + 1)][2] + data[(y + 1) * (newStepsX + 1) + x][2]) * 0.5;
                                }
                            }

                            data[y * (newStepsX + 1) + x] = new double[] { xVal, yVal, zVal };
                        }
                    }
                }

                return new Function2DGrid(data) { Type = GridType.Rectangular, StepsX = newStepsX, StepsY = newStepsY };
            }
            else if (this.Type == GridType.HexagonVertical)
            {
                IReadOnlyList<double>[] data = new IReadOnlyList<double>[(this.StepsY + 1) * (2 * this.StepsX + 1)];

                int newStepsX = this.StepsX * 2;
                int newStepsY = this.StepsY;

                for (int i = 0; i < this.DataPoints.Count; i++)
                {
                    int x = (int)Math.Round((this.DataPoints[i][0] - this.MinX) / (this.MaxX - this.MinX) * newStepsX);
                    int y = (int)Math.Round((this.DataPoints[i][1] - this.MinY) / (this.MaxY - this.MinY) * newStepsY);

                    data[y * (newStepsX + 1) + x] = this.DataPoints[i];
                }

                for (int y = 0; y <= newStepsY; y++)
                {
                    for (int x = 0; x <= newStepsX; x++)
                    {
                        if (data[y * (newStepsX + 1) + x] == null)
                        {
                            double xVal = this.MinX + (this.MaxX - this.MinX) / newStepsX * x;
                            double yVal = this.MinY + (this.MaxY - this.MinY) / newStepsY * y;

                            double zVal;

                            if (x > 0)
                            {
                                if (y > 0)
                                {
                                    if (x < newStepsX)
                                    {
                                        if (y < newStepsY)
                                        {
                                            zVal = (data[(y - 1) * (newStepsX + 1) + x][2] + data[(y + 1) * (newStepsX + 1) + x][2] + data[y * (newStepsX + 1) + (x - 1)][2] + data[y * (newStepsX + 1) + (x + 1)][2]) * 0.25;
                                        }
                                        else
                                        {
                                            zVal = (data[y * (newStepsX + 1) + (x - 1)][2] + data[y * (newStepsX + 1) + (x + 1)][2]) * 0.5;
                                        }
                                    }
                                    else
                                    {
                                        if (y < newStepsY)
                                        {
                                            zVal = (data[(y - 1) * (newStepsX + 1) + x][2] + data[(y + 1) * (newStepsX + 1) + x][2]) * 0.5;
                                        }
                                        else
                                        {
                                            zVal = (data[y * (newStepsX + 1) + (x - 1)][2] + data[(y - 1) * (newStepsX + 1) + x][2]) * 0.5;
                                        }
                                    }
                                }
                                else
                                {
                                    if (x < newStepsX)
                                    {
                                        zVal = (data[y * (newStepsX + 1) + (x - 1)][2] + data[y * (newStepsX + 1) + (x + 1)][2]) * 0.5;
                                    }
                                    else
                                    {
                                        zVal = (data[y * (newStepsX + 1) + (x - 1)][2] + data[(y + 1) * (newStepsX + 1) + x][2]) * 0.5;
                                    }
                                }
                            }
                            else
                            {
                                if (y > 0)
                                {
                                    if (y < newStepsY)
                                    {
                                        zVal = (data[(y - 1) * (newStepsX + 1) + x][2] + data[(y + 1) * (newStepsX + 1) + x][2]) * 0.5;
                                    }
                                    else
                                    {
                                        zVal = (data[y * (newStepsX + 1) + (x + 1)][2] + data[(y - 1) * (newStepsX + 1) + x][2]) * 0.5;
                                    }
                                }
                                else
                                {
                                    zVal = (data[y * (newStepsX + 1) + (x + 1)][2] + data[(y + 1) * (newStepsX + 1) + x][2]) * 0.5;
                                }
                            }

                            data[y * (newStepsX + 1) + x] = new double[] { xVal, yVal, zVal };
                        }
                    }
                }

                return new Function2DGrid(data) { Type = GridType.Rectangular, StepsX = newStepsX, StepsY = newStepsY };
            }
            else
            {
                throw new InvalidOperationException("Cannot convert an irregular grid into a rectangular grid!");
            }
        }
    }

    /// <summary>
    /// A plot element that plots a function of two variables.
    /// </summary>
    public class Function2D : IPlotElement
    {
        /// <summary>
        /// Describes the kind of plots that can be produced.
        /// </summary>
        public enum PlotType
        {
            /// <summary>
            /// A symbol is drawn at each sampled point, whose
            /// colour depends on the value of the function at that point.
            /// </summary>
            SampledPoints,

            /// <summary>
            /// The plot area is tessellated with cells whose colour
            /// depends on the value of the function at a point within the cell.
            /// </summary>
            Tessellation,

            /// <summary>
            /// A rasterised tessellation is created and then stretched and interpolated
            /// to fill the plot area.
            /// </summary>
            Raster
        }

        /// <summary>
        /// The symbol to draw at the sampled points.
        /// </summary>
        public IDataPointElement SampledPointElement { get; set; } = new PathDataPointElement();

        /// <summary>
        /// Resolution on the X axis for the rasterised tesselation.
        /// </summary>
        public int RasterResolutionX { get; set; } = 512;

        /// <summary>
        /// Resolution on the Y axis for the rasterised tesselation.
        /// </summary>
        public int RasterResolutionY { get; set; } = 512;

        /// <summary>
        /// The function to plot.
        /// </summary>
        public Function2DGrid Function { get; set; }
        
        /// <summary>
        /// The kind of plot that is produced.
        /// </summary>
        public PlotType Type { get; set; } = PlotType.SampledPoints;
        
        /// <summary>
        /// A function associating sampled function values to a <see cref="Colour"/>. You should
        /// set this to a function accepting a single <see langword="double"/> argument ranging between
        /// 0 and 1, and returning the corresponding colour. The default value returns black 0 and white for 1.
        /// </summary>
        public Func<double, Colour> Colouring { get; set; } = x => { x = Math.Max(0, Math.Min(1, x)); return Colour.FromRgb(x, x, x); };
        
        /// <summary>
        /// A tag to identify the function in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousInvertibleCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => this.CoordinateSystem;

        /// <summary>
        /// Create a new <see cref="Function2D"/> instance.
        /// </summary>
        /// <param name="function">The function to plot.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Function2D(Function2DGrid function, IContinuousInvertibleCoordinateSystem coordinateSystem)
        {
            this.Function = function;
            this.CoordinateSystem = coordinateSystem;
        }

        /// <inheritdoc/>
        public unsafe void Plot(Graphics target)
        {
            if (Type == PlotType.SampledPoints)
            {
                Point topLeft = CoordinateSystem.ToPlotCoordinates(new double[] { Function.MinX, Function.MaxY });
                Point bottomRight = CoordinateSystem.ToPlotCoordinates(new double[] { Function.MaxX, Function.MinY });

                double size = Math.Min(Math.Abs(topLeft.X - bottomRight.X), Math.Abs(topLeft.Y - bottomRight.Y)) * 0.0075;

                if (Function.Type != Function2DGrid.GridType.Irregular)
                {
                    size = Math.Min(Math.Abs(topLeft.X - bottomRight.X) / Function.StepsX, Math.Abs(topLeft.Y - bottomRight.Y) / Function.StepsY) * 0.4;
                }

                for (int i = 0; i < Function.DataPoints.Count; i++)
                {
                    Point pt = CoordinateSystem.ToPlotCoordinates(Function.DataPoints[i]);
                    Colour col = Colouring((Function.DataPoints[i][2] - Function.MinZ) / (Function.MaxZ - Function.MinZ));

                    string tag = Tag;
                    if (!string.IsNullOrEmpty(tag) && target.UseUniqueTags)
                    {
                        tag += "@" + i.ToString();
                    }

                    target.Save();
                    target.Translate(pt);
                    target.Scale(size, size);

                    SampledPointElement.Plot(target, new PlotElementPresentationAttributes() { Fill = col, Stroke = null }, tag);

                    target.Restore();
                }
            }
            else if (Type == PlotType.Tessellation || (Type == PlotType.Raster && Function.Type == Function2DGrid.GridType.Irregular))
            {
                double width = (Function.MaxX - Function.MinX) / Function.StepsX;
                double height = (Function.MaxY - Function.MinY) / Function.StepsY;

                Point topLeft = CoordinateSystem.ToPlotCoordinates(new double[] { Function.MinX, Function.MaxY });
                Point topRight = CoordinateSystem.ToPlotCoordinates(new double[] { Function.MaxX, Function.MaxY });
                Point bottomRight = CoordinateSystem.ToPlotCoordinates(new double[] { Function.MaxX, Function.MinY });
                Point bottomLeft = CoordinateSystem.ToPlotCoordinates(new double[] { Function.MinX, Function.MinY });


                Graphics strokes = new Graphics();
                Graphics fills = new Graphics();

                bool anyTransparent = false;

                if (Function.Type != Function2DGrid.GridType.Irregular)
                {
                    for (int i = 0; i < Function.DataPoints.Count; i++)
                    {
                        Colour col = Colouring((Function.DataPoints[i][2] - Function.MinZ) / (Function.MaxZ - Function.MinZ));

                        if (col.A != 1)
                        {
                            anyTransparent = true;
                        }

                        if (Function.Type == Function2DGrid.GridType.Rectangular)
                        {
                            Point p1 = CoordinateSystem.ToPlotCoordinates(new double[] { Function.DataPoints[i][0] - width * 0.5, Function.DataPoints[i][1] - height * 0.5 });
                            Point p2 = CoordinateSystem.ToPlotCoordinates(new double[] { Function.DataPoints[i][0] - width * 0.5, Function.DataPoints[i][1] + height * 0.5 });
                            Point p3 = CoordinateSystem.ToPlotCoordinates(new double[] { Function.DataPoints[i][0] + width * 0.5, Function.DataPoints[i][1] + height * 0.5 });
                            Point p4 = CoordinateSystem.ToPlotCoordinates(new double[] { Function.DataPoints[i][0] + width * 0.5, Function.DataPoints[i][1] - height * 0.5 });

                            GraphicsPath pth = new GraphicsPath().MoveTo(p1).LineTo(p2).LineTo(p3).LineTo(p4).Close();

                            string tag = Tag;
                            string strokeTag = Tag;
                            if (!string.IsNullOrEmpty(tag) && target.UseUniqueTags)
                            {
                                tag += "@" + i.ToString();
                                strokeTag += "@stroke" + i.ToString();
                            }

                            strokes.StrokePath(pth, col, 0.5, tag: strokeTag);
                            fills.FillPath(pth, col, tag);
                        }
                        else if (Function.Type == Function2DGrid.GridType.HexagonHorizontal)
                        {
                            double rY = height * 0.57735026919;
                            double rX = width * 2 / 3;

                            Point[] points = new Point[6];

                            for (int j = 0; j < 6; j++)
                            {
                                points[j] = CoordinateSystem.ToPlotCoordinates(new double[] { Function.DataPoints[i][0] + rX * Math.Cos(Math.PI / 3 * j), Function.DataPoints[i][1] + rY * Math.Sin(Math.PI / 3 * j) });
                            }

                            GraphicsPath pth = new GraphicsPath().MoveTo(points[0]).LineTo(points[1]).LineTo(points[2]).LineTo(points[3]).LineTo(points[4]).LineTo(points[5]).Close();

                            string tag = Tag;
                            string strokeTag = Tag;
                            if (!string.IsNullOrEmpty(tag) && target.UseUniqueTags)
                            {
                                tag += "@" + i.ToString();
                                strokeTag += "@stroke" + i.ToString();
                            }

                            strokes.StrokePath(pth, col, 0.5, tag: strokeTag);
                            fills.FillPath(pth, col, tag);
                        }
                        else if (Function.Type == Function2DGrid.GridType.HexagonVertical)
                        {
                            double rX = width * 0.57735026919;
                            double rY = height * 2 / 3;

                            Point[] points = new Point[6];

                            for (int j = 0; j < 6; j++)
                            {
                                points[j] = CoordinateSystem.ToPlotCoordinates(new double[] { Function.DataPoints[i][0] + rX * Math.Sin(Math.PI / 3 * j), Function.DataPoints[i][1] + rY * Math.Cos(Math.PI / 3 * j) });
                            }

                            GraphicsPath pth = new GraphicsPath().MoveTo(points[0]).LineTo(points[1]).LineTo(points[2]).LineTo(points[3]).LineTo(points[4]).LineTo(points[5]).Close();

                            string tag = Tag;
                            string strokeTag = Tag;
                            if (!string.IsNullOrEmpty(tag) && target.UseUniqueTags)
                            {
                                tag += "@" + i.ToString();
                                strokeTag += "@stroke" + i.ToString();
                            }

                            strokes.StrokePath(pth, col, 0.5, tag: strokeTag);
                            fills.FillPath(pth, col, tag);
                        }
                    }
                }
                else
                {
                    double[][] plotCoordinates = new double[Function.DataPoints.Count][];

                    double minX = double.MaxValue;
                    double minY = double.MaxValue;
                    double maxX = double.MinValue;
                    double maxY = double.MinValue;

                    for (int i = 0; i < Function.DataPoints.Count; i++)
                    {
                        Point pt = CoordinateSystem.ToPlotCoordinates(Function.DataPoints[i]);
                        minX = Math.Min(minX, pt.X);
                        minY = Math.Min(minY, pt.Y);
                        maxX = Math.Max(maxX, pt.X);
                        maxY = Math.Max(maxY, pt.Y);
                        plotCoordinates[i] = new double[] { pt.X, pt.Y };
                    }

                    List<double[][]> cells = Voronoi.Voronoi.GetVoronoiCells(plotCoordinates, minX, minY, maxX, maxY);

                    for (int j = 0; j < cells.Count; j++)
                    {
                        Colour col = Colouring((Function.DataPoints[j][2] - Function.MinZ) / (Function.MaxZ - Function.MinZ));

                        if (col.A != 1)
                        {
                            anyTransparent = true;
                        }

                        GraphicsPath path = new GraphicsPath();

                        for (int k = 0; k < cells[j].Length; k++)
                        {
                            path.LineTo(cells[j][k][0], cells[j][k][1]);
                        }

                        path.Close();

                        string tag = Tag;
                        string strokeTag = Tag;
                        if (!string.IsNullOrEmpty(tag) && target.UseUniqueTags)
                        {
                            tag += "@" + j.ToString();
                            strokeTag += "@stroke" + j.ToString();
                        }

                        strokes.StrokePath(path, col, 0.5, tag: strokeTag);
                        fills.FillPath(path, col, tag);

                    }
                }

                target.Save();
                target.SetClippingPath(new GraphicsPath().MoveTo(topLeft).LineTo(topRight).LineTo(bottomRight).LineTo(bottomLeft).Close());

                if (Type != PlotType.Raster)
                {
                    if (!anyTransparent)
                    {
                        target.DrawGraphics(0, 0, strokes);
                    }
                    
                    target.DrawGraphics(0, 0, fills);
                }
                else
                {
                    double x1 = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
                    double x0 = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));

                    double y1 = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));
                    double y0 = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));

                    double scaleX = RasterResolutionX / (x1 - x0);
                    double scaleY = RasterResolutionY / (y1 - y0);

                    Graphics toBeRasterised = new Graphics();
                    toBeRasterised.Scale(scaleX, scaleY);

                    if (!anyTransparent)
                    {
                        toBeRasterised.DrawGraphics(0, 0, strokes);
                    }
                    
                    toBeRasterised.DrawGraphics(0, 0, fills);

                    if (toBeRasterised.TryRasterise(new Rectangle(x0 * scaleX, y0 * scaleY, (x1 - x0) * scaleX, (y1 - y0) * scaleY), 1, true, out RasterImage image))
                    {
                        target.DrawRasterImage(x0, y0, x1 - x0, y1 - y0, image, Tag);
                    }
                }
                target.Restore();

            }
            else if (Type == PlotType.Raster)
            {
                Point topLeft = CoordinateSystem.ToPlotCoordinates(new double[] { Function.MinX, Function.MaxY });
                Point topRight = CoordinateSystem.ToPlotCoordinates(new double[] { Function.MaxX, Function.MaxY });
                Point bottomRight = CoordinateSystem.ToPlotCoordinates(new double[] { Function.MaxX, Function.MinY });
                Point bottomLeft = CoordinateSystem.ToPlotCoordinates(new double[] { Function.MinX, Function.MinY });

                double width = Math.Abs(topLeft.X - bottomRight.X);
                double height = Math.Abs(topLeft.Y - bottomRight.Y);

                RasterImage image = null;

                Function2DGrid function = Function;

                if (Function.Type != Function2DGrid.GridType.Rectangular)
                {
                    function = Function.ToRectangular();
                }


                double[,] imageData = new double[function.StepsX + 1, function.StepsY + 1];

                int sampleWidth = function.StepsX + 1;
                int sampleHeight = function.StepsY + 1;

                for (int i = 0; i < function.DataPoints.Count; i++)
                {
                    int x = (int)Math.Round((function.DataPoints[i][0] - function.MinX) / (function.MaxX - function.MinX) * function.StepsX);
                    int y = (int)Math.Round((function.DataPoints[i][1] - function.MinY) / (function.MaxY - function.MinY) * function.StepsY);

                    imageData[x, y] = (function.DataPoints[i][2] - function.MinZ) / (function.MaxZ - function.MinZ);
                }

                IntPtr imageAddr = Marshal.AllocHGlobal(RasterResolutionX * RasterResolutionY * 4);
                DisposableIntPtr disp = new DisposableIntPtr(imageAddr);

                unsafe
                {
                    byte* imageBytes = (byte*)imageAddr;

                    for (int y = 0; y < RasterResolutionY; y++)
                    {
                        for (int x = 0; x < RasterResolutionX; x++)
                        {
                            double realX = (double)x / (RasterResolutionX - 1);
                            double realY = 1 - (double)y / (RasterResolutionY - 1);

                            double intensity00 = imageData[(int)Math.Floor(realX * (sampleWidth - 1)), (int)Math.Floor(realY * (sampleHeight - 1))];
                            double intensity01 = imageData[(int)Math.Floor(realX * (sampleWidth - 1)), Math.Min(sampleHeight - 1, (int)Math.Floor(realY * (sampleHeight - 1)) + 1)];
                            double intensity10 = imageData[Math.Min(sampleWidth - 1, (int)Math.Floor(realX * (sampleWidth - 1)) + 1), (int)Math.Floor(realY * (sampleHeight - 1))];
                            double intensity11 = imageData[Math.Min(sampleWidth - 1, (int)Math.Floor(realX * (sampleWidth - 1)) + 1), Math.Min(sampleHeight - 1, (int)Math.Floor(realY * (sampleHeight - 1)) + 1)];

                            double fracX = realX * (sampleWidth - 1) - Math.Floor(realX * (sampleWidth - 1));
                            double fracY = realY * (sampleHeight - 1) - Math.Floor(realY * (sampleHeight - 1));

                            double intensityX0 = intensity00 * (1 - fracX) + intensity10 * fracX;
                            double intensityX1 = intensity01 * (1 - fracX) + intensity11 * fracX;
                            double intensity = intensityX0 * (1 - fracY) + intensityX1 * fracY;

                            Colour col = Colouring(intensity);

                            imageBytes[(y * RasterResolutionX + x) * 4] = (byte)(col.R * 255);
                            imageBytes[(y * RasterResolutionX + x) * 4 + 1] = (byte)(col.G * 255);
                            imageBytes[(y * RasterResolutionX + x) * 4 + 2] = (byte)(col.B * 255);
                            imageBytes[(y * RasterResolutionX + x) * 4 + 3] = (byte)(col.A * 255);
                        }
                    }
                }

                image = new RasterImage(ref disp, RasterResolutionX, RasterResolutionY, true, true);


                if (image != null)
                {
                    target.Save();
                    target.SetClippingPath(new GraphicsPath().MoveTo(topLeft).LineTo(topRight).LineTo(bottomRight).LineTo(bottomLeft).Close());

                    target.DrawRasterImage(topLeft, new Size(width, height), image, Tag);

                    target.Restore();
                }
            }
        }
    }
}

