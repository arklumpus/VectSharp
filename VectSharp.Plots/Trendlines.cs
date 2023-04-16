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

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearRegression;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VectSharp.Plots
{
    /// <summary>
    /// A plot element that draws a linear trendline with equation <c>y = a * x + b</c>.
    /// </summary>
    public class LinearTrendLine : IPlotElement
    {
        /// <summary>
        /// The slope of the trendline (a).
        /// </summary>
        public double Slope { get; set; }

        /// <summary>
        /// The intercept of the trendline (b).
        /// </summary>
        public double Intercept { get; set; }

        /// <summary>
        /// The minimum X value for which the trendline is plotted.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// The minimum Y value for which the trendline is plotted.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// The maximum X value for which the trendline is plotted.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// The maximum Y value for which the trendline is plotted.
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// Presentation attributes for the trendline.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { LineDash = new LineDash(5, 5, 0), Stroke = Colour.FromRgb(180, 180, 180) };
        
        /// <summary>
        /// A tag to identify the trendline in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Create a new <see cref="LinearTrendLine"/> instance, specifying the equation parameters.
        /// </summary>
        /// <param name="slope">The slope of the trendline (a).</param>
        /// <param name="intercept">The intercept of the trendline (b).</param>
        /// <param name="minX">The minimum X value for which the trendline is plotted.</param>
        /// <param name="minY">The minimum Y value for which the trendline is plotted.</param>
        /// <param name="maxX">The maximum X value for which the trendline is plotted.</param>
        /// <param name="maxY">The maximum Y value for which the trendline is plotted.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public LinearTrendLine(double slope, double intercept, double minX, double minY, double maxX, double maxY, IContinuousCoordinateSystem coordinateSystem)
        {
            Slope = slope;
            Intercept = intercept;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            CoordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Create a new <see cref="LinearTrendLine"/> instance, determining the equation parameters by running a regression.
        /// </summary>
        /// <param name="data">The data that will be used to determine the equation parameters.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="fixedIntercept">If this is <see langword="null"/>, the intercept (b) is determined during the regression; otherwise, it is fixed to the specified value.</param>
        public LinearTrendLine(IReadOnlyList<IReadOnlyList<double>> data, IContinuousCoordinateSystem coordinateSystem, double? fixedIntercept = null)
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;
            MaxX = double.MinValue;
            MaxY = double.MinValue;

            double[] x = new double[data.Count];
            double[] y = new double[data.Count];

            for (int i = 0; i < data.Count; i++)
            {
                MinX = Math.Min(MinX, data[i][0]);
                MinY = Math.Min(MinY, data[i][1]);
                MaxX = Math.Max(MaxX, data[i][0]);
                MaxY = Math.Max(MaxY, data[i][1]);

                x[i] = data[i][0];

                if (fixedIntercept == null)
                {
                    y[i] = data[i][1];
                }
                else
                {
                    y[i] = data[i][1] - fixedIntercept.Value;
                }
            }

            if (fixedIntercept == null)
            {
                (Intercept, Slope) = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(x, y);
            }
            else
            {
                Slope = MathNet.Numerics.LinearRegression.SimpleRegression.FitThroughOrigin(x, y);
                Intercept = fixedIntercept.Value;
            }

            this.CoordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Create a new <see cref="LinearTrendLine"/> instance, determining the equation parameters by running a regression.
        /// </summary>
        /// <param name="data">The data that will be used to determine the equation parameters.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="fixedIntercept">If this is <see langword="null"/>, the intercept (b) is determined during the regression; otherwise, it is fixed to the specified value.</param>
        public LinearTrendLine(IReadOnlyList<(double, double)> data, IContinuousCoordinateSystem coordinateSystem, double? fixedIntercept = null) : this((from el in data select new double[] { el.Item1, el.Item2 }).ToArray(), coordinateSystem, fixedIntercept) { }
        
        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            double y0 = Slope * MinX + Intercept;
            double x0 = (MinY - Intercept) / Slope;

            double y1 = Slope * MaxX + Intercept;
            double x1 = (MaxY - Intercept) / Slope;

            HashSet<(double, double)> points = new HashSet<(double, double)>();

            if (y0 >= MinY && y0 <= MaxY)
            {
                points.Add((MinX, y0));
            }

            if (x0 >= MinX && x0 <= MaxX)
            {
                points.Add((x0, MinY));
            }

            if (y1 >= MinY && y1 <= MaxY)
            {
                points.Add((MaxX, y1));
            }

            if (x1 >= MinX && x1 <= MaxX)
            {
                points.Add((x1, MaxY));
            }

            if (points.Count == 2)
            {
                double[][] uniquePoints = new double[points.Count][];

                int index = 0;

                foreach ((double x, double y) in points)
                {
                    uniquePoints[index] = new double[] { x, y };
                    index++;
                }

                GraphicsPath path;

                if (CoordinateSystem.IsLinear || CoordinateSystem.IsDirectionStraight(new double[] { uniquePoints[1][0] - uniquePoints[0][0], uniquePoints[1][1] - uniquePoints[0][1] }))
                {
                    Point p1 = CoordinateSystem.ToPlotCoordinates(uniquePoints[0]);
                    Point p2 = CoordinateSystem.ToPlotCoordinates(uniquePoints[1]);

                    path = new GraphicsPath().MoveTo(p1).LineTo(p2);
                }
                else
                {
                    double[] direction = new double[] { uniquePoints[1][0] - uniquePoints[0][0], uniquePoints[1][1] - uniquePoints[0][1] };
                    double mod = Math.Sqrt(direction[0] * direction[0] + direction[1] * direction[1]);
                    double[] increment = new double[] { direction[0] / mod * Math.Min(CoordinateSystem.Resolution[0], CoordinateSystem.Resolution[1]), direction[1] / mod * Math.Min(CoordinateSystem.Resolution[0], CoordinateSystem.Resolution[1]) };

                    int count = (int)Math.Ceiling(Math.Max(direction[0] / increment[0], direction[1] / increment[1]));

                    path = new GraphicsPath();

                    for (int i = 0; i <= count; i++)
                    {
                        double[] pt = new double[] { uniquePoints[0][0] + increment[0] * i, uniquePoints[0][1] + increment[1] * i };

                        if (i == 0)
                        {
                            path.MoveTo(CoordinateSystem.ToPlotCoordinates(pt));
                        }
                        else
                        {
                            path.LineTo(CoordinateSystem.ToPlotCoordinates(pt));
                        }
                    }
                }

                Point topLeft = CoordinateSystem.ToPlotCoordinates(new double[] { MinX, MinY });
                Point topRight = CoordinateSystem.ToPlotCoordinates(new double[] { MaxX, MinY });
                Point bottomRight = CoordinateSystem.ToPlotCoordinates(new double[] { MaxX, MaxY });
                Point bottomLeft = CoordinateSystem.ToPlotCoordinates(new double[] { MinX, MaxY });

                double minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
                double minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
                double maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
                double maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

                target.Save();
                target.SetClippingPath(minX, minY, maxX - minX, maxY - minY);

                target.StrokePath(path, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, Tag);
                target.Restore();
            }

        }
    }

    /// <summary>
    /// A plot element that draws an exponential trendline with equation <c>y = b * Exp(a * x)</c>.
    /// </summary>
    public class ExponentialTrendLine : IPlotElement
    {
        /// <summary>
        /// The slope of the trendline (a).
        /// </summary>
        public double Slope { get; set; }

        /// <summary>
        /// The intercept of the trendline (b).
        /// </summary>
        public double Intercept { get; set; }

        /// <summary>
        /// The minimum X value for which the trendline is plotted.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// The minimum Y value for which the trendline is plotted.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// The maximum X value for which the trendline is plotted.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// The maximum Y value for which the trendline is plotted.
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// Presentation attributes for the trendline.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { LineDash = new LineDash(5, 5, 0), Stroke = Colour.FromRgb(180, 180, 180) };

        /// <summary>
        /// A tag to identify the trendline in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Create a new <see cref="ExponentialTrendLine"/> instance, specifying the equation parameters.
        /// </summary>
        /// <param name="slope">The slope of the trendline (a).</param>
        /// <param name="intercept">The intercept of the trendline (b).</param>
        /// <param name="minX">The minimum X value for which the trendline is plotted.</param>
        /// <param name="minY">The minimum Y value for which the trendline is plotted.</param>
        /// <param name="maxX">The maximum X value for which the trendline is plotted.</param>
        /// <param name="maxY">The maximum Y value for which the trendline is plotted.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public ExponentialTrendLine(double slope, double intercept, double minX, double minY, double maxX, double maxY, IContinuousCoordinateSystem coordinateSystem)
        {
            Slope = slope;
            Intercept = intercept;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            CoordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Create a new <see cref="ExponentialTrendLine"/> instance, determining the equation parameters by running a regression.
        /// </summary>
        /// <param name="data">The data that will be used to determine the equation parameters.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="fixedIntercept">If this is <see langword="null"/>, the intercept (b) is determined during the regression; otherwise, it is fixed to the specified value.</param>
        public ExponentialTrendLine(IReadOnlyList<IReadOnlyList<double>> data, IContinuousCoordinateSystem coordinateSystem, double? fixedIntercept = null)
        {
            if (fixedIntercept <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fixedIntercept), fixedIntercept, "The intercept must be greater than 0!");
            }

            MinX = double.MaxValue;
            MinY = double.MaxValue;
            MaxX = double.MinValue;
            MaxY = double.MinValue;

            double[] x = new double[data.Count];
            double[] y = new double[data.Count];

            double shift = 0;

            if (fixedIntercept != null)
            {
                shift = Math.Log(fixedIntercept.Value);
            }

            for (int i = 0; i < data.Count; i++)
            {
                MinX = Math.Min(MinX, data[i][0]);
                MinY = Math.Min(MinY, data[i][1]);
                MaxX = Math.Max(MaxX, data[i][0]);
                MaxY = Math.Max(MaxY, data[i][1]);

                x[i] = data[i][0];
                y[i] = Math.Log(data[i][1]) - shift;
            }

            if (fixedIntercept == null)
            {
                (double k, double b) = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(x, y);
                Intercept = Math.Exp(k);
                Slope = b;
            }
            else
            {
                Slope = MathNet.Numerics.LinearRegression.SimpleRegression.FitThroughOrigin(x, y);
                Intercept = fixedIntercept.Value;
            }

            this.CoordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Create a new <see cref="ExponentialTrendLine"/> instance, determining the equation parameters by running a regression.
        /// </summary>
        /// <param name="data">The data that will be used to determine the equation parameters.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="fixedIntercept">If this is <see langword="null"/>, the intercept (b) is determined during the regression; otherwise, it is fixed to the specified value.</param>
        public ExponentialTrendLine(IReadOnlyList<(double, double)> data, IContinuousCoordinateSystem coordinateSystem, double? fixedIntercept = null) : this((from el in data select new double[] { el.Item1, el.Item2 }).ToArray(), coordinateSystem, fixedIntercept) { }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            double y0 = Math.Exp(Slope * MinX) * Intercept;
            double x0 = (Math.Log(MinY) - Math.Log(Intercept)) / Slope;

            double y1 = Math.Exp(Slope * MaxX) * Intercept;
            double x1 = (Math.Log(MaxY) - Math.Log(Intercept)) / Slope;

            HashSet<(double, double)> points = new HashSet<(double, double)>();

            if (y0 >= MinY && y0 <= MaxY)
            {
                points.Add((MinX, y0));
            }

            if (x0 >= MinX && x0 <= MaxX)
            {
                points.Add((x0, MinY));
            }

            if (y1 >= MinY && y1 <= MaxY)
            {
                points.Add((MaxX, y1));
            }

            if (x1 >= MinX && x1 <= MaxX)
            {
                points.Add((x1, MaxY));
            }

            if (points.Count == 2)
            {
                double[][] uniquePoints = new double[points.Count][];

                int index = 0;

                foreach ((double x, double y) in points)
                {
                    uniquePoints[index] = new double[] { x, y };
                    index++;
                }

                double startX = Math.Min(uniquePoints[0][0], uniquePoints[1][0]);
                double endX = Math.Max(uniquePoints[0][0], uniquePoints[1][0]);

                int count = (int)Math.Ceiling(Math.Max((endX - startX) / CoordinateSystem.Resolution[0], Math.Abs(uniquePoints[1][1] - uniquePoints[0][1]) / CoordinateSystem.Resolution[1]));

                GraphicsPath path = new GraphicsPath();

                for (int i = 0; i <= count; i++)
                {
                    double[] pt = new double[] { startX + (endX - startX) / count * i, Math.Exp(Slope * (startX + (endX - startX) / count * i)) * Intercept };

                    if (i == 0)
                    {
                        path.MoveTo(CoordinateSystem.ToPlotCoordinates(pt));
                    }
                    else
                    {
                        path.LineTo(CoordinateSystem.ToPlotCoordinates(pt));
                    }
                }

                Point topLeft = CoordinateSystem.ToPlotCoordinates(new double[] { MinX, MinY });
                Point topRight = CoordinateSystem.ToPlotCoordinates(new double[] { MaxX, MinY });
                Point bottomRight = CoordinateSystem.ToPlotCoordinates(new double[] { MaxX, MaxY });
                Point bottomLeft = CoordinateSystem.ToPlotCoordinates(new double[] { MinX, MaxY });

                double minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
                double minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
                double maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
                double maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

                target.Save();
                target.SetClippingPath(minX, minY, maxX - minX, maxY - minY);

                target.StrokePath(path, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, Tag);
                target.Restore();
            }

        }
    }

    /// <summary>
    /// A plot element that draws a logarithmic trendline with equation <c>y = a * Ln(x) + b</c>.
    /// </summary>
    public class LogarithmicTrendLine : IPlotElement
    {
        /// <summary>
        /// The slope of the trendline (a).
        /// </summary>
        public double Slope { get; set; }

        /// <summary>
        /// The intercept of the trendline (b).
        /// </summary>
        public double Intercept { get; set; }

        /// <summary>
        /// The minimum X value for which the trendline is plotted.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// The minimum Y value for which the trendline is plotted.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// The maximum X value for which the trendline is plotted.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// The maximum Y value for which the trendline is plotted.
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// Presentation attributes for the trendline.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { LineDash = new LineDash(5, 5, 0), Stroke = Colour.FromRgb(180, 180, 180) };

        /// <summary>
        /// A tag to identify the trendline in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Create a new <see cref="LogarithmicTrendLine"/> instance, specifying the equation parameters.
        /// </summary>
        /// <param name="slope">The slope of the trendline (a).</param>
        /// <param name="intercept">The intercept of the trendline (b).</param>
        /// <param name="minX">The minimum X value for which the trendline is plotted.</param>
        /// <param name="minY">The minimum Y value for which the trendline is plotted.</param>
        /// <param name="maxX">The maximum X value for which the trendline is plotted.</param>
        /// <param name="maxY">The maximum Y value for which the trendline is plotted.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public LogarithmicTrendLine(double slope, double intercept, double minX, double minY, double maxX, double maxY, IContinuousCoordinateSystem coordinateSystem)
        {
            Slope = slope;
            Intercept = intercept;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            CoordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Create a new <see cref="LogarithmicTrendLine"/> instance, determining the equation parameters by running a regression.
        /// </summary>
        /// <param name="data">The data that will be used to determine the equation parameters.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="fixedIntercept">If this is <see langword="null"/>, the intercept (b) is determined during the regression; otherwise, it is fixed to the specified value.</param>
        public LogarithmicTrendLine(IReadOnlyList<IReadOnlyList<double>> data, IContinuousCoordinateSystem coordinateSystem, double? fixedIntercept = null)
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;
            MaxX = double.MinValue;
            MaxY = double.MinValue;

            double[] x = new double[data.Count];
            double[] y = new double[data.Count];

            double shift = 0;

            if (fixedIntercept != null)
            {
                shift = fixedIntercept.Value;
            }

            for (int i = 0; i < data.Count; i++)
            {
                MinX = Math.Min(MinX, data[i][0]);
                MinY = Math.Min(MinY, data[i][1]);
                MaxX = Math.Max(MaxX, data[i][0]);
                MaxY = Math.Max(MaxY, data[i][1]);

                x[i] = Math.Log(data[i][0]);
                y[i] = data[i][1] - shift;
            }

            if (fixedIntercept == null)
            {
                (Intercept, Slope) = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(x, y);
            }
            else
            {
                Slope = MathNet.Numerics.LinearRegression.SimpleRegression.FitThroughOrigin(x, y);
                Intercept = fixedIntercept.Value;
            }

            this.CoordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Create a new <see cref="LogarithmicTrendLine"/> instance, determining the equation parameters by running a regression.
        /// </summary>
        /// <param name="data">The data that will be used to determine the equation parameters.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="fixedIntercept">If this is <see langword="null"/>, the intercept (b) is determined during the regression; otherwise, it is fixed to the specified value.</param>
        public LogarithmicTrendLine(IReadOnlyList<(double, double)> data, IContinuousCoordinateSystem coordinateSystem, double? fixedIntercept = null) : this((from el in data select new double[] { el.Item1, el.Item2 }).ToArray(), coordinateSystem, fixedIntercept) { }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            double y0 = Slope * Math.Log(MinX) + Intercept;
            double x0 = Math.Exp((MinY - Intercept) / Slope);

            double y1 = Slope * Math.Log(MaxX) + Intercept;
            double x1 = Math.Exp((MaxY - Intercept) / Slope);

            HashSet<(double, double)> points = new HashSet<(double, double)>();

            if (y0 >= MinY && y0 <= MaxY)
            {
                points.Add((MinX, y0));
            }

            if (x0 >= MinX && x0 <= MaxX)
            {
                points.Add((x0, MinY));
            }

            if (y1 >= MinY && y1 <= MaxY)
            {
                points.Add((MaxX, y1));
            }

            if (x1 >= MinX && x1 <= MaxX)
            {
                points.Add((x1, MaxY));
            }

            if (points.Count == 2)
            {
                double[][] uniquePoints = new double[points.Count][];

                int index = 0;

                foreach ((double x, double y) in points)
                {
                    uniquePoints[index] = new double[] { x, y };
                    index++;
                }

                double startX = Math.Min(uniquePoints[0][0], uniquePoints[1][0]);
                double endX = Math.Max(uniquePoints[0][0], uniquePoints[1][0]);

                int count = (int)Math.Ceiling(Math.Max((endX - startX) / CoordinateSystem.Resolution[0], Math.Abs(uniquePoints[1][1] - uniquePoints[0][1]) / CoordinateSystem.Resolution[1]));

                GraphicsPath path = new GraphicsPath();

                for (int i = 0; i <= count; i++)
                {
                    double[] pt = new double[] { startX + (endX - startX) / count * i, Slope * Math.Log(startX + (endX - startX) / count * i) + Intercept };

                    if (i == 0)
                    {
                        path.MoveTo(CoordinateSystem.ToPlotCoordinates(pt));
                    }
                    else
                    {
                        path.LineTo(CoordinateSystem.ToPlotCoordinates(pt));
                    }
                }

                Point topLeft = CoordinateSystem.ToPlotCoordinates(new double[] { MinX, MinY });
                Point topRight = CoordinateSystem.ToPlotCoordinates(new double[] { MaxX, MinY });
                Point bottomRight = CoordinateSystem.ToPlotCoordinates(new double[] { MaxX, MaxY });
                Point bottomLeft = CoordinateSystem.ToPlotCoordinates(new double[] { MinX, MaxY });

                double minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
                double minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
                double maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
                double maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

                target.Save();
                target.SetClippingPath(minX, minY, maxX - minX, maxY - minY);

                target.StrokePath(path, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, Tag);
                target.Restore();
            }

        }
    }

    /// <summary>
    /// A plot element that draws a polynomial trendline with equation <c>y = a0 + a1 * x + a2 * x^2 + ... + aN * x^N</c>.
    /// </summary>
    public class PolynomialTrendLine : IPlotElement
    {
        /// <summary>
        /// The coefficients (<c>a0 ... aN</c>).
        /// </summary>
        public double[] Coefficients { get; set; }
        /// <summary>
        /// The minimum X value for which the trendline is plotted.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// The minimum Y value for which the trendline is plotted.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// The maximum X value for which the trendline is plotted.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// The maximum Y value for which the trendline is plotted.
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// Presentation attributes for the trendline.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { LineDash = new LineDash(5, 5, 0), Stroke = Colour.FromRgb(180, 180, 180) };

        /// <summary>
        /// A tag to identify the trendline in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Create a new <see cref="LinearTrendLine"/> instance, specifying the coefficients.
        /// </summary>
        /// <param name="coefficients">The coefficients (<c>a0 ... aN</c>).</param>
        /// <param name="minX">The minimum X value for which the trendline is plotted.</param>
        /// <param name="minY">The minimum Y value for which the trendline is plotted.</param>
        /// <param name="maxX">The maximum X value for which the trendline is plotted.</param>
        /// <param name="maxY">The maximum Y value for which the trendline is plotted.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public PolynomialTrendLine(double[] coefficients, double minX, double minY, double maxX, double maxY, IContinuousCoordinateSystem coordinateSystem)
        {
            Coefficients = coefficients;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            CoordinateSystem = coordinateSystem;
        }

        private static double[] FitPolynomialWithFixedIntercept(double[] x, double[] y, int order, double fixedIntercept)
        {
            Matrix<double> v = new DenseMatrix(x.Length, order);

            for (int i = 0; i < v.RowCount; i++)
            {
                for (int j = 0; j < order; j++)
                {
                    v[i, j] = Math.Pow(x[i], j + 1);
                }
            }

            double[] coeffs = MultipleRegression.QR(v, Vector<double>.Build.Dense(y)).ToArray();
            double[] tbr = new double[coeffs.Length + 1];
            tbr[0] = fixedIntercept;
            for (int i = 0; i < coeffs.Length; i++)
            {
                tbr[i + 1] = coeffs[i];
            }

            return tbr;
        }

        /// <summary>
        /// Create a new <see cref="PolynomialTrendLine"/> instance, determining the coefficients by running a regression.
        /// </summary>
        /// <param name="data">The data that will be used to determine the coefficients.</param>
        /// <param name="order">The order of the polynomial (<c>N</c>). This must be ≥ 2.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="fixedIntercept">If this is <see langword="null"/>, the intercept (a0) is determined during the regression; otherwise, it is fixed to the specified value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="order"/> is &lt; 2.</exception>
        public PolynomialTrendLine(IReadOnlyList<IReadOnlyList<double>> data, int order, IContinuousCoordinateSystem coordinateSystem,  double? fixedIntercept = null)
        {
            if (order < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(order), order, "The polynomial order must be 2 or greater!");
            }

            MinX = double.MaxValue;
            MinY = double.MaxValue;
            MaxX = double.MinValue;
            MaxY = double.MinValue;

            double[] x = new double[data.Count];
            double[] y = new double[data.Count];

            double shift = 0;

            if (fixedIntercept != null)
            {
                shift = fixedIntercept.Value;
            }

            for (int i = 0; i < data.Count; i++)
            {
                MinX = Math.Min(MinX, data[i][0]);
                MinY = Math.Min(MinY, data[i][1]);
                MaxX = Math.Max(MaxX, data[i][0]);
                MaxY = Math.Max(MaxY, data[i][1]);

                x[i] = data[i][0];
                y[i] = data[i][1] - shift;
            }

            if (fixedIntercept == null)
            {
                Coefficients = MathNet.Numerics.Fit.Polynomial(x, y, order);
            }
            else
            {
                Coefficients = FitPolynomialWithFixedIntercept(x, y, order, fixedIntercept.Value);
            }

            this.CoordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Create a new <see cref="PolynomialTrendLine"/> instance, determining the coefficients by running a regression.
        /// </summary>
        /// <param name="data">The data that will be used to determine the coefficients.</param>
        /// <param name="order">The order of the polynomial (<c>N</c>). This must be ≥ 2.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="fixedIntercept">If this is <see langword="null"/>, the intercept (a0) is determined during the regression; otherwise, it is fixed to the specified value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="order"/> is &lt; 2.</exception>
        public PolynomialTrendLine(IReadOnlyList<(double, double)> data, int order, IContinuousCoordinateSystem coordinateSystem,  double? fixedIntercept = null) : this((from el in data select new double[] { el.Item1, el.Item2 }).ToArray(), order, coordinateSystem, fixedIntercept) { }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            double startX = Math.Min(MinX, MaxX);
            double endX = Math.Max(MinX, MaxX);

            int count = (int)Math.Ceiling((endX - startX) / CoordinateSystem.Resolution[0]);

            GraphicsPath path = new GraphicsPath();

            for (int i = 0; i <= count; i++)
            {
                double[] pt = new double[] { startX + (endX - startX) / count * i, Polynomial.Evaluate(startX + (endX - startX) / count * i, Coefficients) };

                if (i == 0)
                {
                    path.MoveTo(CoordinateSystem.ToPlotCoordinates(pt));
                }
                else
                {
                    path.LineTo(CoordinateSystem.ToPlotCoordinates(pt));
                }
            }

            Point topLeft = CoordinateSystem.ToPlotCoordinates(new double[] { MinX, MinY });
            Point topRight = CoordinateSystem.ToPlotCoordinates(new double[] { MaxX, MinY });
            Point bottomRight = CoordinateSystem.ToPlotCoordinates(new double[] { MaxX, MaxY });
            Point bottomLeft = CoordinateSystem.ToPlotCoordinates(new double[] { MinX, MaxY });

            double minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
            double minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
            double maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
            double maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

            target.Save();
            target.SetClippingPath(minX, minY, maxX - minX, maxY - minY);

            target.StrokePath(path, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, Tag);
            target.Restore();
        }
    }

    /// <summary>
    /// A plot element that draws a power law trendline with equation <c>y = b * x^a</c>.
    /// </summary>
    public class PowerLawTrendLine : IPlotElement
    {
        /// <summary>
        /// The slope of the trendline (a).
        /// </summary>
        public double Slope { get; set; }

        /// <summary>
        /// The intercept of the trendline (b).
        /// </summary>
        public double Intercept { get; set; }

        /// <summary>
        /// The minimum X value for which the trendline is plotted.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// The minimum Y value for which the trendline is plotted.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// The maximum X value for which the trendline is plotted.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// The maximum Y value for which the trendline is plotted.
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// Presentation attributes for the trendline.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { LineDash = new LineDash(5, 5, 0), Stroke = Colour.FromRgb(180, 180, 180) };

        /// <summary>
        /// A tag to identify the trendline in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Create a new <see cref="PowerLawTrendLine"/> instance, specifying the equation parameters.
        /// </summary>
        /// <param name="slope">The slope of the trendline (a).</param>
        /// <param name="intercept">The intercept of the trendline (b).</param>
        /// <param name="minX">The minimum X value for which the trendline is plotted.</param>
        /// <param name="minY">The minimum Y value for which the trendline is plotted.</param>
        /// <param name="maxX">The maximum X value for which the trendline is plotted.</param>
        /// <param name="maxY">The maximum Y value for which the trendline is plotted.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
		public PowerLawTrendLine(double slope, double intercept, double minX, double minY, double maxX, double maxY, IContinuousCoordinateSystem coordinateSystem)
        {
            Slope = slope;
            Intercept = intercept;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            CoordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Create a new <see cref="PowerLawTrendLine"/> instance, determining the equation parameters by running a regression.
        /// </summary>
        /// <param name="data">The data that will be used to determine the equation parameters.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public PowerLawTrendLine(IReadOnlyList<IReadOnlyList<double>> data, IContinuousCoordinateSystem coordinateSystem)
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;
            MaxX = double.MinValue;
            MaxY = double.MinValue;

            double[] x = new double[data.Count];
            double[] y = new double[data.Count];

            for (int i = 0; i < data.Count; i++)
            {
                MinX = Math.Min(MinX, data[i][0]);
                MinY = Math.Min(MinY, data[i][1]);
                MaxX = Math.Max(MaxX, data[i][0]);
                MaxY = Math.Max(MaxY, data[i][1]);

                x[i] = Math.Log(data[i][0]);
                y[i] = Math.Log(data[i][1]);
            }

            (double intercept, double slope) = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(x, y);

            Slope = slope;
            Intercept = Math.Exp(intercept);

            this.CoordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Create a new <see cref="PowerLawTrendLine"/> instance, determining the equation parameters by running a regression.
        /// </summary>
        /// <param name="data">The data that will be used to determine the equation parameters.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public PowerLawTrendLine(IReadOnlyList<(double, double)> data, IContinuousCoordinateSystem coordinateSystem) : this((from el in data select new double[] { el.Item1, el.Item2 }).ToArray(), coordinateSystem) { }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            double y0 = Intercept * Math.Pow(MinX, Slope);
            double x0 = Math.Pow(MinY / Intercept, 1 / Slope);

            double y1 = Intercept * Math.Pow(MaxX, Slope);
            double x1 = Math.Pow(MaxY / Intercept, 1 / Slope);

            HashSet<(double, double)> points = new HashSet<(double, double)>();

            if (y0 >= MinY && y0 <= MaxY)
            {
                points.Add((MinX, y0));
            }

            if (x0 >= MinX && x0 <= MaxX)
            {
                points.Add((x0, MinY));
            }

            if (y1 >= MinY && y1 <= MaxY)
            {
                points.Add((MaxX, y1));
            }

            if (x1 >= MinX && x1 <= MaxX)
            {
                points.Add((x1, MaxY));
            }

            if (points.Count == 2)
            {
                double[][] uniquePoints = new double[points.Count][];

                int index = 0;

                foreach ((double x, double y) in points)
                {
                    uniquePoints[index] = new double[] { x, y };
                    index++;
                }

                double startX = Math.Min(uniquePoints[0][0], uniquePoints[1][0]);
                double endX = Math.Max(uniquePoints[0][0], uniquePoints[1][0]);

                int count = (int)Math.Ceiling(Math.Max((endX - startX) / CoordinateSystem.Resolution[0], Math.Abs(uniquePoints[1][1] - uniquePoints[0][1]) / CoordinateSystem.Resolution[1]));

                GraphicsPath path = new GraphicsPath();

                for (int i = 0; i <= count; i++)
                {
                    double[] pt = new double[] { startX + (endX - startX) / count * i, Intercept * Math.Pow(startX + (endX - startX) / count * i, Slope) };

                    if (i == 0)
                    {
                        path.MoveTo(CoordinateSystem.ToPlotCoordinates(pt));
                    }
                    else
                    {
                        path.LineTo(CoordinateSystem.ToPlotCoordinates(pt));
                    }
                }

                Point topLeft = CoordinateSystem.ToPlotCoordinates(new double[] { MinX, MinY });
                Point topRight = CoordinateSystem.ToPlotCoordinates(new double[] { MaxX, MinY });
                Point bottomRight = CoordinateSystem.ToPlotCoordinates(new double[] { MaxX, MaxY });
                Point bottomLeft = CoordinateSystem.ToPlotCoordinates(new double[] { MinX, MaxY });

                double minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
                double minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
                double maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
                double maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

                target.Save();
                target.SetClippingPath(minX, minY, maxX - minX, maxY - minY);

                target.StrokePath(path, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, Tag);
                target.Restore();
            }

        }
    }

    /// <summary>
    /// A plot element that draws a moving average trendline.
    /// </summary>
    public class MovingAverageTrendLine : IPlotElement
    {
        /// <summary>
        /// The data used to compute the moving average. These must be sorted in a sensible way.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<double>> Data { get; set; }

        /// <summary>
        /// The weight function. This should accept two parameters: an <see langword="int"/> representing the difference in 
        /// index between the point being weighted and the "focus point", and a <see langword="double"/>[] representing the
        /// difference in coordinates between the two points. It should return a <see langword="double"/> representing the
        /// weight (it does not need to be normalised).
        /// </summary>
        public Func<int, double[], double> Weight { get; set; } = (int i, double[] d) => 1;

        /// <summary>
        /// The number of points that are averaged to obtain the value for each point. This must be ≥ 0.
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Presentation attributes for the trendline.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { LineDash = new LineDash(5, 5, 0), Stroke = Colour.FromRgb(180, 180, 180) };
        
        /// <summary>
        /// A tag to identify the trendline in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<IReadOnlyList<double>> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Create a new <see cref="MovingAverageTrendLine"/> instance from the specified data.
        /// </summary>
        /// <param name="data">The data used to compute the moving average. These must be sorted in a sensible way.</param>
        /// <param name="period">The number of points that are averaged to obtain the value for each point. This must be ≥ 0.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="period"/> is &lt; 0.</exception>
        public MovingAverageTrendLine(IReadOnlyList<IReadOnlyList<double>> data, int period, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem)
        {
            if (period < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(period), period, "The period of the moving average must be greater than or equal to zero!");
            }

            this.Data = data;
            this.Period = period;
            CoordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Create a new <see cref="MovingAverageTrendLine"/> instance from the specified data.
        /// </summary>
        /// <param name="data">The data used to compute the moving average. These must be sorted in a sensible way.</param>
        /// <param name="period">The number of points that are averaged to obtain the value for each point. This must be ≥ 0.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="period"/> is &lt; 0.</exception>
        public MovingAverageTrendLine(IReadOnlyList<(double, double)> data, int period, IContinuousCoordinateSystem coordinateSystem) : this((from el in data select new double[] { el.Item1, el.Item2 }).ToArray(), period, coordinateSystem) { }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            GraphicsPath path = new GraphicsPath();

            for (int i = 0; i < Data.Count; i++)
            {
                double count = 0;

                double[] point = new double[Data[i].Count];

                for (int k = -Period; k <= Period; k++)
                {
                    if (i + k >= 0 && i + k < Data.Count)
                    {
                        double[] diff = new double[Data[i].Count];

                        for (int j = 0; j < Data[i].Count; j++)
                        {
                            diff[j] = Data[i + k][j] - Data[i][j];
                        }

                        double weighting = Weight(k, diff);

                        count += weighting;

                        for (int j = 0; j < point.Length; j++)
                        {
                            point[j] += Data[i + k][j] * weighting;
                        }
                    }
                }

                for (int j = 0; j < point.Length; j++)
                {
                    point[j] /= count;
                }

                if (i == 0)
                {
                    path.MoveTo(CoordinateSystem.ToPlotCoordinates(point));
                }
                else
                {
                    path.LineTo(CoordinateSystem.ToPlotCoordinates(point));
                }
            }

            target.StrokePath(path, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, Tag);

        }
    }
}
