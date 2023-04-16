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
    /// <summary>
    /// Represents a coordinate system.
    /// </summary>
    public interface ICoordinateSystem
    {

    }

    /// <summary>
    /// Represents a coordinate system transforming data points of type <typeparamref name="T"/> into plot <see cref="Point"/>s.
    /// </summary>
    /// <typeparam name="T">The type of data elements.</typeparam>
    public interface ICoordinateSystem<T> : ICoordinateSystem
    {
        /// <summary>
        /// Transform the specified <paramref name="dataPoint"/> into a plot <see cref="Point"/>.
        /// </summary>
        /// <param name="dataPoint">The data point whose coordinates should be determined.</param>
        /// <returns>A <see cref="Point"/> representing the <paramref name="dataPoint"/> in plot space.</returns>
        Point ToPlotCoordinates(T dataPoint);
    }

    /// <summary>
    /// Represents a coordinate system tranforming data points of type <typeparamref name="T"/> into <see langword="double"/>s.
    /// </summary>
    /// <typeparam name="T">The type of data elements.</typeparam>
    public interface ICoordinateSystem1D<T>
    {
        /// <summary>
        /// Transform the specified <paramref name="dataPoint"/> into a <see langword="double"/>.
        /// </summary>
        /// <param name="dataPoint">The data point whose coordinates should be determined.</param>
        /// <returns>A <see langword="double"/> representing the <paramref name="dataPoint"/> in plot space.</returns>
        double ToPlotCoordinates(T dataPoint);
    }

    /// <summary>
    /// A coordinate system using a custom method to transform data points.
    /// </summary>
    /// <typeparam name="T">The type of data elements.</typeparam>
    public class CoordinateSystem<T> : ICoordinateSystem<T>
    {
        /// <summary>
        /// The method used to transform the data elements into plot <see cref="Point"/>s.
        /// </summary>
        public Func<T, Point> CoordinateFunction { get; set; }

        /// <summary>
        /// Create a new <see cref="CoordinateSystem{T}"/> using the specified method.
        /// </summary>
        /// <param name="coordinateFunction">The method used to transform the data elements into plot <see cref="Point"/>s.</param>
        public CoordinateSystem(Func<T, Point> coordinateFunction)
        {
            this.CoordinateFunction = coordinateFunction;
        }

        /// <inheritdoc/>
        public Point ToPlotCoordinates(T dataPoint)
        {
            return CoordinateFunction(dataPoint);
        }
    }

    /// <summary>
    /// Represents a coordinate system performing continuous transformations.
    /// </summary>
    public interface IContinuousCoordinateSystem : ICoordinateSystem<IReadOnlyList<double>>
    {
        /// <summary>
        /// Gets whether the current coordinate system is linear along all directions.
        /// </summary>
        bool IsLinear { get; }

        /// <summary>
        /// Determines whether points aligned along <paramref name="direction"/> in data space are also aligned along some line in plot space.
        /// </summary>
        /// <param name="direction">The direction in data space along which the points should be aligned.</param>
        /// <returns><see langword="true"/> if any two points aligned along <paramref name="direction"/> in data space are also aligned along some line in plot space, <see langword="false"/> otherwise.</returns>
        bool IsDirectionStraight(IReadOnlyList<double> direction);

        /// <summary>
        /// The maximum difference between two points in data space that appear arbitrarily close in plot space, or some approximation.
        /// </summary>
        double[] Resolution { get; }

        /// <summary>
        /// Gets a data element that is arbitrarily close to the specified <paramref name="point"/>, along the specified <paramref name="direction"/>.
        /// </summary>
        /// <param name="point">The point close to which the returned data element should be.</param>
        /// <param name="direction">The direction (in data space) along which the returned point should be.</param>
        /// <returns>A data element that is arbitrarily close to the specified <paramref name="point"/>, along the specified <paramref name="direction"/>.</returns>
        double[] GetAround(IReadOnlyList<double> point, IReadOnlyList<double> direction);
    }

    /// <summary>
    /// Represents a coordinate system performing continuous invertible transformations.
    /// </summary>
    public interface IContinuousInvertibleCoordinateSystem : IContinuousCoordinateSystem
    {
        /// <summary>
        /// Transform a point in plot space back into data space.
        /// </summary>
        /// <param name="plotPoint">The point in plot space.</param>
        /// <returns>The point in data space corresponding to the specified point in plot space.</returns>
        double[] ToDataCoordinates(Point plotPoint);
    }

    /// <summary>
    /// Represents a linear coordinate system.
    /// </summary>
    public class LinearCoordinateSystem2D : IContinuousInvertibleCoordinateSystem
    {
        /// <summary>
        /// The minimum X value.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// The maximum X value.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// The minimum Y value.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// The maximum Y value.
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// The X scale.
        /// </summary>
        public double ScaleX { get; set; }

        /// <summary>
        /// The y scale.
        /// </summary>
        public double ScaleY { get; set; }
        
        /// <inheritdoc/>
        public bool IsLinear => true;

        /// <inheritdoc/>
        public bool IsDirectionStraight(IReadOnlyList<double> direction) => true;

        private double[] resolution = null;

        /// <inheritdoc/>
        public double[] Resolution
        {
            get
            {
                return new double[] { (MaxX - MinX) * 0.01, (MaxY - MinY) * 0.01 };
            }

            set
            {
                resolution = value;
            }
        }

        /// <summary>
        /// Creates a new <see cref="LinearCoordinateSystem2D"/> manually specifying the parameter values.
        /// </summary>
        /// <param name="minX">The minimum X value.</param>
        /// <param name="maxX">The maximum X value.</param>
        /// <param name="minY">The minimum Y value.</param>
        /// <param name="maxY">The maximum Y value.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LinearCoordinateSystem2D(double minX, double maxX, double minY, double maxY, double scaleX, double scaleY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <summary>
        /// Creates a new <see cref="LinearCoordinateSystem2D"/> determining the value range from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data from which the value range should be determined.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LinearCoordinateSystem2D(IReadOnlyList<IReadOnlyList<double>> data, double scaleX = 350, double scaleY = 250)
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;

            MaxX = double.MinValue;
            MaxY = double.MinValue;

            for (int i = 0; i < data.Count; i++)
            {
                MinX = Math.Min(MinX, data[i][0]);
                MinY = Math.Min(MinY, data[i][1]);

                MaxX = Math.Max(MaxX, data[i][0]);
                MaxY = Math.Max(MaxY, data[i][1]);
            }

            double rangeX = MaxX - MinX;
            double rangeY = MaxY - MinY;

            MinX -= rangeX * 0.1;
            MaxX += rangeX * 0.1;

            MinY -= rangeY * 0.1;
            MaxY += rangeY * 0.1;

            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <summary>
        /// Creates a new <see cref="LinearCoordinateSystem2D"/> determining the value range from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data from which the value range should be determined.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LinearCoordinateSystem2D(double[,] data, double scaleX = 350, double scaleY = 250)
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;

            MaxX = double.MinValue;
            MaxY = double.MinValue;

            for (int i = 0; i < data.GetLength(0); i++)
            {
                MinX = Math.Min(MinX, data[i, 0]);
                MinY = Math.Min(MinY, data[i, 1]);

                MaxX = Math.Max(MaxX, data[i, 0]);
                MaxY = Math.Max(MaxY, data[i, 1]);
            }

            double rangeX = MaxX - MinX;
            double rangeY = MaxY - MinY;

            MinX -= rangeX * 0.1;
            MaxX += rangeX * 0.1;

            MinY -= rangeY * 0.1;
            MaxY += rangeY * 0.1;

            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <inheritdoc/>
        public double[] ToDataCoordinates(Point plotPoint)
        {
            return new double[] { plotPoint.X / ScaleX * (MaxX - MinX) + MinX, (ScaleY - plotPoint.Y) / ScaleY * (MaxY - MinY) + MinY };
        }

        /// <inheritdoc/>
        public Point ToPlotCoordinates(IReadOnlyList<double> dataPoint)
        {
            return new Point((dataPoint[0] - MinX) / (MaxX - MinX) * ScaleX, ScaleY - (dataPoint[1] - MinY) / (MaxY - MinY) * ScaleY);
        }

        /// <summary>
        /// Transforms the specified <paramref name="dataPoint"/> into a plot point.
        /// </summary>
        /// <param name="dataPoint">The data whose plot coordinates should be determined.</param>
        /// <returns>A <see cref="Point"/> representing the <paramref name="dataPoint"/> in plot space.</returns>
        public Point ToPlotCoordinates(Point dataPoint)
        {
            return new Point((dataPoint.X - MinX) / (MaxX - MinX) * ScaleX, ScaleY - (dataPoint.Y - MinY) / (MaxY - MinY) * ScaleY);
        }

        /// <inheritdoc/>
        public double[] GetAround(IReadOnlyList<double> point, IReadOnlyList<double> direction)
        {
            return new double[] { point[0] + direction[0] * this.Resolution[0], point[1] + direction[1] * this.Resolution[1] };
        }
    }

    /// <summary>
    /// Represents a logarithmic coordinate system.
    /// </summary>
    public class LogarithmicCoordinateSystem2D : IContinuousInvertibleCoordinateSystem
    {
        /// <summary>
        /// The minimum X value (in logarithmic scale).
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// The maximum X value (in logarithmic scale).
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// The minimum Y value (in logarithmic scale).
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// The maximum Y value (in logarithmic scale).
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// The X scale.
        /// </summary>
        public double ScaleX { get; set; }

        /// <summary>
        /// The y scale.
        /// </summary>
        public double ScaleY { get; set; }

        /// <inheritdoc/>
        public bool IsLinear => false;

        /// <inheritdoc/>
        public bool IsDirectionStraight(IReadOnlyList<double> direction)
        {
            if (direction[0] == 0 || direction[1] == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private double[] resolution = null;

        /// <inheritdoc/>
        public double[] Resolution
        {
            get
            {
                if (resolution == null)
                {
                    return new double[] { (Math.Exp(MaxX) - Math.Exp(MinX)) * 0.01, (Math.Exp(MaxY) - Math.Exp(MinY)) * 0.01 };
                }
                else
                {
                    return resolution;
                }
            }

            set
            {
                resolution = value;
            }
        }

        /// <summary>
        /// Creates a new <see cref="LogarithmicCoordinateSystem2D"/> manually specifying the parameter values.
        /// </summary>
        /// <param name="minX">The minimum X value.</param>
        /// <param name="maxX">The maximum X value.</param>
        /// <param name="minY">The minimum Y value.</param>
        /// <param name="maxY">The maximum Y value.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LogarithmicCoordinateSystem2D(double minX, double maxX, double minY, double maxY, double scaleX, double scaleY)
        {
            MinX = Math.Log(minX);
            MaxX = Math.Log(maxX);
            MinY = Math.Log(minY);
            MaxY = Math.Log(maxY);
            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <summary>
        /// Creates a new <see cref="LogarithmicCoordinateSystem2D"/> determining the value range from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data from which the value range should be determined.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LogarithmicCoordinateSystem2D(IReadOnlyList<IReadOnlyList<double>> data, double scaleX = 350, double scaleY = 250)
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;

            MaxX = double.MinValue;
            MaxY = double.MinValue;

            for (int i = 0; i < data.Count; i++)
            {
                MinX = Math.Min(MinX, data[i][0]);
                MinY = Math.Min(MinY, data[i][1]);

                MaxX = Math.Max(MaxX, data[i][0]);
                MaxY = Math.Max(MaxY, data[i][1]);
            }

            MinX = Math.Log(MinX);
            MinY = Math.Log(MinY);
            MaxX = Math.Log(MaxX);
            MaxY = Math.Log(MaxY);

            double rangeX = MaxX - MinX;
            double rangeY = MaxY - MinY;

            MinX -= rangeX * 0.1;
            MaxX += rangeX * 0.1;

            MinY -= rangeY * 0.1;
            MaxY += rangeY * 0.1;

            ScaleX = scaleX;
            ScaleY = scaleY;
        }


        /// <summary>
        /// Creates a new <see cref="LogarithmicCoordinateSystem2D"/> determining the value range from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data from which the value range should be determined.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LogarithmicCoordinateSystem2D(double[,] data, double scaleX = 350, double scaleY = 250)
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;

            MaxX = double.MinValue;
            MaxY = double.MinValue;

            for (int i = 0; i < data.GetLength(0); i++)
            {
                MinX = Math.Min(MinX, data[i, 0]);
                MinY = Math.Min(MinY, data[i, 1]);

                MaxX = Math.Max(MaxX, data[i, 0]);
                MaxY = Math.Max(MaxY, data[i, 1]);
            }

            MinX = Math.Log(MinX);
            MinY = Math.Log(MinY);
            MaxX = Math.Log(MaxX);
            MaxY = Math.Log(MaxY);

            double rangeX = MaxX - MinX;
            double rangeY = MaxY - MinY;

            MinX -= rangeX * 0.1;
            MaxX += rangeX * 0.1;

            MinY -= rangeY * 0.1;
            MaxY += rangeY * 0.1;

            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <inheritdoc/>
        public double[] ToDataCoordinates(Point plotPoint)
        {
            return new double[] { Math.Exp(plotPoint.X / ScaleX * (MaxX - MinX) + MinX), Math.Exp((ScaleY - plotPoint.Y) / ScaleY * (MaxY - MinY) + MinY) };
        }

        /// <inheritdoc/>
        public Point ToPlotCoordinates(IReadOnlyList<double> dataPoint)
        {
            return new Point((Math.Log(dataPoint[0]) - MinX) / (MaxX - MinX) * ScaleX, ScaleY - (Math.Log(dataPoint[1]) - MinY) / (MaxY - MinY) * ScaleY);
        }

        /// <summary>
        /// Transforms the specified <paramref name="dataPoint"/> into a plot point.
        /// </summary>
        /// <param name="dataPoint">The data whose plot coordinates should be determined.</param>
        /// <returns>A <see cref="Point"/> representing the <paramref name="dataPoint"/> in plot space.</returns>
        public Point ToPlotCoordinates(Point dataPoint)
        {
            return new Point((Math.Log(dataPoint.X) - MinX) / (MaxX - MinX) * ScaleX, ScaleY - (Math.Log(dataPoint.Y) - MinY) / (MaxY - MinY) * ScaleY);
        }

        /// <inheritdoc/>
        public double[] GetAround(IReadOnlyList<double> point, IReadOnlyList<double> direction)
        {
            return new double[] { Math.Exp(Math.Log(point[0]) + direction[0] * (MaxX - MinX) * 0.01), Math.Exp(Math.Log(point[1]) + direction[1] * (MaxY - MinY) * 0.01) };
        }
    }

    /// <summary>
    /// Represents a semi-logarithmic coordinate system with a logarithmic transformation on the Y axis.
    /// </summary>
    public class LogLinCoordinateSystem2D : IContinuousInvertibleCoordinateSystem
    {
        /// <summary>
        /// The minimum X value.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// The maximum X value.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// The minimum Y value (in logarithmic scale).
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// The maximum Y value (in logarithmic scale).
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// The X scale.
        /// </summary>
        public double ScaleX { get; set; }

        /// <summary>
        /// The y scale.
        /// </summary>
        public double ScaleY { get; set; }

        /// <inheritdoc/>
        public bool IsLinear => false;

        /// <inheritdoc/>
        public bool IsDirectionStraight(IReadOnlyList<double> direction)
        {
            if (direction[0] == 0 || direction[1] == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private double[] resolution = null;

        /// <inheritdoc/>
        public double[] Resolution
        {
            get
            {
                if (resolution == null)
                {
                    return new double[] { (MaxX - MinX) * 0.01, (Math.Exp(MaxY) - Math.Exp(MinY)) * 0.01 };
                }
                else
                {
                    return resolution;
                }
            }

            set
            {
                resolution = value;
            }
        }

        /// <summary>
        /// Creates a new <see cref="LogLinCoordinateSystem2D"/> manually specifying the parameter values.
        /// </summary>
        /// <param name="minX">The minimum X value.</param>
        /// <param name="maxX">The maximum X value.</param>
        /// <param name="minY">The minimum Y value.</param>
        /// <param name="maxY">The maximum Y value.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LogLinCoordinateSystem2D(double minX, double maxX, double minY, double maxY, double scaleX, double scaleY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = Math.Log(minY);
            MaxY = Math.Log(maxY);
            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <summary>
        /// Creates a new <see cref="LogLinCoordinateSystem2D"/> determining the value range from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data from which the value range should be determined.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LogLinCoordinateSystem2D(IReadOnlyList<IReadOnlyList<double>> data, double scaleX = 350, double scaleY = 250)
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;

            MaxX = double.MinValue;
            MaxY = double.MinValue;

            for (int i = 0; i < data.Count; i++)
            {
                MinX = Math.Min(MinX, data[i][0]);
                MinY = Math.Min(MinY, data[i][1]);

                MaxX = Math.Max(MaxX, data[i][0]);
                MaxY = Math.Max(MaxY, data[i][1]);
            }

            MinX = MinX;
            MinY = Math.Log(MinY);
            MaxX = MaxX;
            MaxY = Math.Log(MaxY);

            double rangeX = MaxX - MinX;
            double rangeY = MaxY - MinY;

            MinX -= rangeX * 0.1;
            MaxX += rangeX * 0.1;

            MinY -= rangeY * 0.1;
            MaxY += rangeY * 0.1;

            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <summary>
        /// Creates a new <see cref="LogLinCoordinateSystem2D"/> determining the value range from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data from which the value range should be determined.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LogLinCoordinateSystem2D(double[,] data, double scaleX = 350, double scaleY = 250)
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;

            MaxX = double.MinValue;
            MaxY = double.MinValue;

            for (int i = 0; i < data.GetLength(0); i++)
            {
                MinX = Math.Min(MinX, data[i, 0]);
                MinY = Math.Min(MinY, data[i, 1]);

                MaxX = Math.Max(MaxX, data[i, 0]);
                MaxY = Math.Max(MaxY, data[i, 1]);
            }

            MinX = MinX;
            MinY = Math.Log(MinY);
            MaxX = MaxX;
            MaxY = Math.Log(MaxY);

            double rangeX = MaxX - MinX;
            double rangeY = MaxY - MinY;

            MinX -= rangeX * 0.1;
            MaxX += rangeX * 0.1;

            MinY -= rangeY * 0.1;
            MaxY += rangeY * 0.1;

            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <inheritdoc/>
        public double[] ToDataCoordinates(Point plotPoint)
        {
            return new double[] { plotPoint.X / ScaleX * (MaxX - MinX) + MinX, Math.Exp((ScaleY - plotPoint.Y) / ScaleY * (MaxY - MinY) + MinY) };
        }

        /// <inheritdoc/>
        public Point ToPlotCoordinates(IReadOnlyList<double> dataPoint)
        {
            return new Point((dataPoint[0] - MinX) / (MaxX - MinX) * ScaleX, ScaleY - (Math.Log(dataPoint[1]) - MinY) / (MaxY - MinY) * ScaleY);
        }

        /// <summary>
        /// Transforms the specified <paramref name="dataPoint"/> into a plot point.
        /// </summary>
        /// <param name="dataPoint">The data whose plot coordinates should be determined.</param>
        /// <returns>A <see cref="Point"/> representing the <paramref name="dataPoint"/> in plot space.</returns>
        public Point ToPlotCoordinates(Point dataPoint)
        {
            return new Point((dataPoint.X - MinX) / (MaxX - MinX) * ScaleX, ScaleY - (Math.Log(dataPoint.Y) - MinY) / (MaxY - MinY) * ScaleY);
        }

        /// <inheritdoc/>
        public double[] GetAround(IReadOnlyList<double> point, IReadOnlyList<double> direction)
        {
            return new double[] { point[0] + direction[0] * (MaxX - MinX) * 0.01, Math.Exp(Math.Log(point[1]) + direction[1] * (MaxY - MinY) * 0.01) };
        }
    }

    /// <summary>
    /// Represents a semi-logarithmic coordinate system with a logarithmic transformation on the X axis.
    /// </summary>
    public class LinLogCoordinateSystem2D : IContinuousInvertibleCoordinateSystem
    {
        /// <summary>
        /// The minimum X value.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// The maximum X value.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// The minimum Y value (in logarithmic scale).
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// The maximum Y value (in logarithmic scale).
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// The X scale.
        /// </summary>
        public double ScaleX { get; set; }

        /// <summary>
        /// The y scale.
        /// </summary>
        public double ScaleY { get; set; }

        /// <inheritdoc/>
        public bool IsLinear => false;

        /// <inheritdoc/>
        public bool IsDirectionStraight(IReadOnlyList<double> direction)
        {
            if (direction[0] == 0 || direction[1] == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private double[] resolution = null;

        /// <inheritdoc/>
        public double[] Resolution
        {
            get
            {
                if (resolution == null)
                {
                    return new double[] { (MaxX - MinX) * 0.01, (Math.Exp(MaxY) - Math.Exp(MinY)) * 0.01 };
                }
                else
                {
                    return resolution;
                }
            }

            set
            {
                resolution = value;
            }
        }

        /// <summary>
        /// Creates a new <see cref="LinLogCoordinateSystem2D"/> manually specifying the parameter values.
        /// </summary>
        /// <param name="minX">The minimum X value.</param>
        /// <param name="maxX">The maximum X value.</param>
        /// <param name="minY">The minimum Y value.</param>
        /// <param name="maxY">The maximum Y value.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LinLogCoordinateSystem2D(double minX, double maxX, double minY, double maxY, double scaleX, double scaleY)
        {
            MinX = Math.Log(minX);
            MaxX = Math.Log(maxX);
            MinY = minY;
            MaxY = maxY;
            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <summary>
        /// Creates a new <see cref="LinLogCoordinateSystem2D"/> determining the value range from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data from which the value range should be determined.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LinLogCoordinateSystem2D(IReadOnlyList<IReadOnlyList<double>> data, double scaleX = 350, double scaleY = 250)
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;

            MaxX = double.MinValue;
            MaxY = double.MinValue;

            for (int i = 0; i < data.Count; i++)
            {
                MinX = Math.Min(MinX, data[i][0]);
                MinY = Math.Min(MinY, data[i][1]);

                MaxX = Math.Max(MaxX, data[i][0]);
                MaxY = Math.Max(MaxY, data[i][1]);
            }

            MinX = Math.Log(MinX);
            MinY = MinY;
            MaxX = Math.Log(MaxX);
            MaxY = MaxY;

            double rangeX = MaxX - MinX;
            double rangeY = MaxY - MinY;

            MinX -= rangeX * 0.1;
            MaxX += rangeX * 0.1;

            MinY -= rangeY * 0.1;
            MaxY += rangeY * 0.1;

            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <summary>
        /// Creates a new <see cref="LinLogCoordinateSystem2D"/> determining the value range from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data from which the value range should be determined.</param>
        /// <param name="scaleX">The X scale.</param>
        /// <param name="scaleY">The Y scale.</param>
        public LinLogCoordinateSystem2D(double[,] data, double scaleX = 350, double scaleY = 250)
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;

            MaxX = double.MinValue;
            MaxY = double.MinValue;

            for (int i = 0; i < data.GetLength(0); i++)
            {
                MinX = Math.Min(MinX, data[i, 0]);
                MinY = Math.Min(MinY, data[i, 1]);

                MaxX = Math.Max(MaxX, data[i, 0]);
                MaxY = Math.Max(MaxY, data[i, 1]);
            }

            MinX = Math.Log(MinX);
            MinY = MinY;
            MaxX = Math.Log(MaxX);
            MaxY = MaxY;

            double rangeX = MaxX - MinX;
            double rangeY = MaxY - MinY;

            MinX -= rangeX * 0.1;
            MaxX += rangeX * 0.1;

            MinY -= rangeY * 0.1;
            MaxY += rangeY * 0.1;

            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        /// <inheritdoc/>
        public double[] ToDataCoordinates(Point plotPoint)
        {
            return new double[] { Math.Exp(plotPoint.X / ScaleX * (MaxX - MinX) + MinX), (ScaleY - plotPoint.Y) / ScaleY * (MaxY - MinY) + MinY };
        }

        /// <inheritdoc/>
        public Point ToPlotCoordinates(IReadOnlyList<double> dataPoint)
        {
            return new Point((Math.Log(dataPoint[0]) - MinX) / (MaxX - MinX) * ScaleX, ScaleY - (dataPoint[1] - MinY) / (MaxY - MinY) * ScaleY);
        }

        /// <summary>
        /// Transforms the specified <paramref name="dataPoint"/> into a plot point.
        /// </summary>
        /// <param name="dataPoint">The data whose plot coordinates should be determined.</param>
        /// <returns>A <see cref="Point"/> representing the <paramref name="dataPoint"/> in plot space.</returns>
        public Point ToPlotCoordinates(Point dataPoint)
        {
            return new Point((Math.Log(dataPoint.X) - MinX) / (MaxX - MinX) * ScaleX, ScaleY - (dataPoint.Y - MinY) / (MaxY - MinY) * ScaleY);
        }

        /// <inheritdoc/>
        public double[] GetAround(IReadOnlyList<double> point, IReadOnlyList<double> direction)
        {
            return new double[] { Math.Exp(Math.Log(point[0]) + direction[0] * (MaxX - MinX) * 0.01), point[1] + direction[1] * (MaxY - MinY) * 0.01 };
        }
    }

    /// <summary>
    /// Represents a 1-D linear coordinate system.
    /// </summary>
    public class LinearCoordinateSystem1D : ICoordinateSystem1D<double>
    {
        /// <summary>
        /// The minimum value.
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// The maximum value.
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// The scale factor.
        /// </summary>
        public double Scale { get; set; }

        /// <summary>
        /// Creates a new <see cref="LinearCoordinateSystem1D"/>, manually specifying the parameters.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="scale">The scale factor.</param>
        public LinearCoordinateSystem1D(double min, double max, double scale)
        {
            Min = min;
            Max = max;
            Scale = scale;
        }

        /// <summary>
        /// Creates a new <see cref="LinearCoordinateSystem1D"/> determining the value range from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data from which the value range should be determined.</param>
        /// <param name="scale">The scale factor.</param>
        public LinearCoordinateSystem1D(IReadOnlyList<double> data, double scale = 350)
        {
            Min = double.MaxValue;
            Min = double.MaxValue;

            for (int i = 0; i < data.Count; i++)
            {
                Min = Math.Min(Min, data[i]);
                Max = Math.Max(Max, data[i]);
            }

            double range = Max - Min;

            Min -= range * 0.1;
            Max += range * 0.1;

            Scale = scale;
        }

        /// <inheritdoc/>
        public double ToPlotCoordinates(double dataPoint)
        {
            return (dataPoint - Min) / (Max - Min) * Scale;
        }
    }

    /// <summary>
    /// Represents a 1-D logarithmic coordinate system.
    /// </summary>
    public class LogarithmicCoordinateSystem1D : ICoordinateSystem1D<double>
    {
        /// <summary>
        /// The minimum value (in logarithmic scale).
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// The maximum value (in logarithmic scale).
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// The scale factor.
        /// </summary>
        public double Scale { get; set; }

        /// <summary>
        /// Creates a new <see cref="LogarithmicCoordinateSystem1D"/>, manually specifying the parameters.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="scale">The scale factor.</param>
        public LogarithmicCoordinateSystem1D(double min, double max, double scale)
        {
            Min = Math.Log(min);
            Max = Math.Log(max);
            Scale = scale;
        }

        /// <summary>
        /// Creates a new <see cref="LogarithmicCoordinateSystem1D"/> determining the value range from the <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data from which the value range should be determined.</param>
        /// <param name="scale">The scale factor.</param>
        public LogarithmicCoordinateSystem1D(IReadOnlyList<double> data, double scale = 350)
        {
            Min = double.MaxValue;
            Min = double.MaxValue;

            for (int i = 0; i < data.Count; i++)
            {
                Min = Math.Min(Min, data[i]);
                Max = Math.Max(Max, data[i]);
            }

            Min = Math.Log(Min);
            Max = Math.Log(Max);

            double range = Max - Min;

            Min -= range * 0.1;
            Max += range * 0.1;

            Scale = scale;
        }

        /// <inheritdoc/>
        public double ToPlotCoordinates(double dataPoint)
        {
            return (Math.Log(dataPoint) - Min) / (Max - Min) * Scale;
        }
    }

    /// <summary>
    /// A coordinate system using a custom method to transform data points.
    /// </summary>
    /// <typeparam name="T">The type of data elements.</typeparam>
    public class CoordinateSystem1D<T> : ICoordinateSystem1D<T>
    {
        /// <summary>
        /// The method used to transform data points.
        /// </summary>
        public Func<T, double> CoordinateFunction { get; set; }

        /// <summary>
        /// Creates a new <see cref="CoordinateSystem1D{T}"/> using the specified <paramref name="coordinateFunction"/>.
        /// </summary>
        /// <param name="coordinateFunction">The method used to transform data points.</param>
        public CoordinateSystem1D(Func<T, double> coordinateFunction)
        {
            this.CoordinateFunction = coordinateFunction;
        }

        /// <inheritdoc/>
        public double ToPlotCoordinates(T dataPoint)
        {
            return CoordinateFunction(dataPoint);
        }
    }

    /// <summary>
    /// Represents a categorical 1-D coordinate system.
    /// </summary>
    /// <typeparam name="T">The type of data elements.</typeparam>
    public class CategoricalCoordinateSystem1D<T> : ICoordinateSystem1D<T>
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> storing the coordinates.
        /// </summary>
        public Dictionary<T, double> Coordinates { get; set; }

        /// <summary>
        /// Creates a new <see cref="CategoricalCoordinateSystem1D{T}"/> using the coordinates specified in the supplied <paramref name="coordinates"/> <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="coordinates"></param>
        public CategoricalCoordinateSystem1D(Dictionary<T, double> coordinates)
        {
            this.Coordinates = coordinates;
        }

        /// <summary>
        /// Creates a new <see cref="CategoricalCoordinateSystem1D{T}"/> assigning equally-spaced coordinates to all different values of <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data containing discrete parameter values that should be assigned to different coordinates.</param>
        /// <param name="scale">The maximum coordinate.</param>
        public CategoricalCoordinateSystem1D(IReadOnlyList<T> data, double scale = 350)
        {
            Coordinates = new Dictionary<T, double>();

            for (int i = 0; i < data.Count; i++)
            {
                if (!Coordinates.ContainsKey(data[i]))
                {
                    Coordinates[data[i]] = Coordinates.Count;
                } 
            }

            if (Coordinates.Count > 1)
            {
                foreach (KeyValuePair<T, double> kvp in Coordinates)
                {
                    Coordinates[kvp.Key] = kvp.Value / (Coordinates.Count - 1) * scale; ;
                }
            }
        }

        /// <inheritdoc/>
        public double ToPlotCoordinates(T dataPoint)
        {
            return Coordinates[dataPoint];
        }
    }

    /// <summary>
    /// Combines two <see cref="ICoordinateSystem1D{T}"/>s to produce a <see cref="ICoordinateSystem{T}"/>.
    /// </summary>
    /// <typeparam name="T1">The type for the first coordinate system.</typeparam>
    /// <typeparam name="T2">The type for the second coordinate system.</typeparam>
    public class CompositeCoordinateSystem2D<T1, T2> : ICoordinateSystem<(T1, T2)>
    {
        /// <summary>
        /// The first coordinate system.
        /// </summary>
        public ICoordinateSystem1D<T1> CoordinateSystemX { get; set; }

        /// <summary>
        /// The second coordinate system.
        /// </summary>
        public ICoordinateSystem1D<T2> CoordinateSystemY { get; set; }

        /// <summary>
        /// Creates a new <see cref="CompositeCoordinateSystem2D{T1, T2}"/> from two 1-D coordinate system.
        /// </summary>
        /// <param name="coordinateSystemX">The first coordinate system.</param>
        /// <param name="coordinateSystemY">The second coordinate system.</param>
        public CompositeCoordinateSystem2D(ICoordinateSystem1D<T1> coordinateSystemX, ICoordinateSystem1D<T2> coordinateSystemY)
        {
            this.CoordinateSystemX = coordinateSystemX;
            this.CoordinateSystemY = coordinateSystemY;
        }

        /// <inheritdoc/>
        public Point ToPlotCoordinates((T1, T2) dataPoint)
        {
            return new Point(CoordinateSystemX.ToPlotCoordinates(dataPoint.Item1), CoordinateSystemY.ToPlotCoordinates(dataPoint.Item2));
        }
    }
}
