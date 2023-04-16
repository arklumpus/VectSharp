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
    /// <summary>
    /// A plot element that draws a violin plot.
    /// </summary>
    public class Violin : IPlotElement
    {
        /// <summary>
        /// The sides on which a violin can be drawn.
        /// </summary>
        public enum ViolinSide
        {
            /// <summary>
            /// Draw the violin only on the left side.
            /// </summary>
            Left,

            /// <summary>
            /// Draw the violin only on the right side.
            /// </summary>
            Right,

            /// <summary>
            /// Draw the violin on both left and right sides.
            /// </summary>
            Both
        }

        /// <summary>
        /// The position of the origin of the violin (e.g., the 0 in data space coordinates).
        /// </summary>
        public IReadOnlyList<double> Position { get; set; }

        /// <summary>
        /// The direction along which the violin is drawn, in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> Direction { get; set; }
        
        /// <summary>
        /// The width of the violin in data space coordinates.
        /// </summary>
        public double Width { get; set; } = 10;
        
        /// <summary>
        /// Determines whether the violin is smoothed or not.
        /// </summary>
        public bool Smooth { get; set; } = true;
        
        /// <summary>
        /// Determines on which side(s) the violin is drawn.
        /// </summary>
        public ViolinSide Sides { get; set; } = ViolinSide.Both;

        /// <summary>
        /// The values whose distribution is displayed by the violin plot.
        /// </summary>
        public IReadOnlyList<double> Data { get; set; }

        /// <summary>
        /// Presentation attributes for the violin plot.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { Fill = Colours.White };

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<IReadOnlyList<double>> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => this.CoordinateSystem;

        /// <summary>
        /// A tag to identify the violin in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Create a new <see cref="Violin"/> instance.
        /// </summary>
        /// <param name="position">The position of the origin of the violin (e.g., the 0 in data space coordinates).</param>
        /// <param name="direction">The direction along which the violin is drawn, in data space coordinates.</param>
        /// <param name="data">The values whose distribution is displayed by the violin plot.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Violin(IReadOnlyList<double> position, IReadOnlyList<double> direction, IReadOnlyList<double> data, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem)
        {
            this.Position = position;
            this.Direction = direction;
            this.Data = data;
            this.CoordinateSystem = coordinateSystem;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {

            double min = double.MaxValue;
            double max = double.MinValue;

            for (int i = 0; i < Data.Count; i++)
            {
                min = Math.Min(min, Data[i]);
                max = Math.Max(max, Data[i]);
            }

            (double _, double _, double iqr) = Plots.Plot.IQR(Data);
            double h2 = 2 * iqr / Math.Pow(Data.Count, 1.0 / 3.0);
            int binCount = Math.Max(1, (int)Math.Ceiling((max - min) / h2));

            int[] bins = new int[binCount];

            if (max > min)
            {
                for (int i = 0; i < Data.Count; i++)
                {
                    int index = (int)Math.Min(binCount - 1, Math.Floor((Data[i] - min) / (max - min) * binCount));

                    bins[index]++;
                }
            }
            else
            {
                bins[0] = Data.Count;
            }

            double binMax = bins.Max();

            double[] perpDirection = new double[] { Direction[1], -Direction[0] };

            List<Point> points = new List<Point>();
            List<Point> pointsOpposite = new List<Point>();

            Point topPoint = CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * min, Position[1] + Direction[1] * min });
            Point bottomPoint = CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * max, Position[1] + Direction[1] * max });

            for (int i = 0; i < bins.Length; i++)
            {
                double binStart = min + (max - min) / binCount * i;
                double binEnd = min + (max - min) / binCount * (i + 1);

                double x = (binStart + binEnd) * 0.5;

                if (i == 0)
                {
                    if (bins.Length > 1 && bins[i + 1] > bins[i])
                    {
                        points.Add(CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * binStart, Position[1] + Direction[1] * binStart }));
                        pointsOpposite.Add(CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * binStart, Position[1] + Direction[1] * binStart }));
                    }
                    else
                    {
                        points.Add(CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * binStart + perpDirection[0] * bins[i] / binMax * Width, Position[1] + Direction[1] * binStart + perpDirection[1] * bins[i] / binMax * Width }));
                        pointsOpposite.Add(CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * binStart - perpDirection[0] * bins[i] / binMax * Width, Position[1] + Direction[1] * binStart - perpDirection[1] * bins[i] / binMax * Width }));
                    }
                }

                points.Add(CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * x + perpDirection[0] * bins[i] / binMax * Width, Position[1] + Direction[1] * x + perpDirection[1] * bins[i] / binMax * Width }));
                pointsOpposite.Add(CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * x - perpDirection[0] * bins[i] / binMax * Width, Position[1] + Direction[1] * x - perpDirection[1] * bins[i] / binMax * Width }));

                if (i == bins.Length - 1)
                {
                    if (bins.Length > 1 && bins[i - 1] > bins[i])
                    {
                        points.Add(CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * binEnd, Position[1] + Direction[1] * binEnd }));
                        pointsOpposite.Add(CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * binEnd, Position[1] + Direction[1] * binEnd }));
                    }
                    else
                    {
                        points.Add(CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * binEnd + perpDirection[0] * bins[i] / binMax * Width, Position[1] + Direction[1] * binEnd + perpDirection[1] * bins[i] / binMax * Width }));
                        pointsOpposite.Add(CoordinateSystem.ToPlotCoordinates(new double[] { Position[0] + Direction[0] * binEnd - perpDirection[0] * bins[i] / binMax * Width, Position[1] + Direction[1] * binEnd - perpDirection[1] * bins[i] / binMax * Width }));
                    }
                }
            }

            pointsOpposite.Reverse();

            GraphicsPath path = new GraphicsPath();

            if (Sides == ViolinSide.Left || Sides == ViolinSide.Both)
            {
                if (Smooth)
                {
                    path.AddSmoothSpline(pointsOpposite.ToArray());
                }
                else
                {
                    for (int i= 0; i < pointsOpposite.Count; i++)
                    {
                        if (i > 0)
                        {
                            path.LineTo(pointsOpposite[i]);
                        }
                        else
                        {
                            path.MoveTo(pointsOpposite[i]);
                        }
                    }
                }
            }
            else
            {
                path.MoveTo(bottomPoint).LineTo(topPoint);
            }

            if (Sides == ViolinSide.Right || Sides == ViolinSide.Both)
            {
                if (Smooth)
                {
                    path.AddSmoothSpline(points.ToArray());
                }
                else
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        path.LineTo(points[i]);
                    }
                }
            }
            else
            {
                path.LineTo(topPoint).LineTo(bottomPoint);
            }

            path.Close();

            string tag = Tag;

            string strokeTag = Tag;

            if (!string.IsNullOrEmpty(Tag) && target.UseUniqueTags)
            {
                strokeTag += "@stroke";
            }


            if (PresentationAttributes.Fill != null)
            {
                target.FillPath(path, PresentationAttributes.Fill, tag);
            }

            if (PresentationAttributes.Stroke != null)
            {
                target.StrokePath(path, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, strokeTag);
            }
        }
    }
}
