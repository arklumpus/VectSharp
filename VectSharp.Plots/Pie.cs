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
    /// A plot element that draws a pie or a doughnut.
    /// </summary>
    public class Pie : IPlotElement
    {
        /// <summary>
        /// The data in the pie chart. The values do not need to be normalised.
        /// </summary>
        public IReadOnlyList<double> Data { get; set; }

        /// <summary>
        /// The centre of the pie/doughnut, in data coordinates.
        /// </summary>
        public IReadOnlyList<double> Centre { get; set; }

        /// <summary>
        /// The outer radius of the pie/doughnut, in data coordinates.
        /// </summary>
        public IReadOnlyList<double> OuterRadius { get; set; }

        /// <summary>
        /// The inner radius of the doughnut, in data coordinates. Set to [0, 0] for a pie chart.
        /// </summary>
        public IReadOnlyList<double> InnerRadius { get; set; }

        /// <summary>
        /// The initial angle starting from which the pie slices are drawn.
        /// </summary>
        public double StartAngle { get; set; } = 0;

        /// <summary>
        /// Determines whether the slices are drawn in clockwise or anti-clockwise fashion.
        /// </summary>
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<IReadOnlyList<double>> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes for the slices. An element from this collection is used for each slice in
        /// the pie/doughnut; if there are more slices than elements in this collection, the presentation attributes
        /// are wrapped.
        /// </summary>
        public IReadOnlyList<PlotElementPresentationAttributes> PresentationAttributes { get; set; } = new PlotElementPresentationAttributes[] { new PlotElementPresentationAttributes() };
        
        /// <summary>
        /// A tag to identify the pie/doughnut in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Create a new <see cref="Pie"/> instance drawing a pie chart.
        /// </summary>
        /// <param name="data">The data in the pie chart. The values do not need to be normalised.</param>
        /// <param name="centre">The centre of the pie, in data coordinates.</param>
        /// <param name="radius">The radius of the pie, in data coordinates.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Pie(IReadOnlyList<double> data, IReadOnlyList<double> centre, IReadOnlyList<double> radius, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem)
        {
            Data = data;
            Centre = centre;
            OuterRadius = radius;
            InnerRadius = new double[] { 0, 0 };
            CoordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Create a new <see cref="Pie"/> instance drawing a doughnut chart.
        /// </summary>
        /// <param name="data">The data in the doughnut chart. The values do not need to be normalised.</param>
        /// <param name="centre">The centre of the doughnut, in data coordinates.</param>
        /// <param name="innerRadius">The inner radius of the doughnut, in data coordinates.</param>
        /// <param name="outerRadius">The outer radius of the doughnut, in data coordinates.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Pie(IReadOnlyList<double> data, IReadOnlyList<double> centre, IReadOnlyList<double> innerRadius, IReadOnlyList<double> outerRadius, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem)
        {
            Data = data;
            Centre = centre;
            OuterRadius = outerRadius;
            InnerRadius = innerRadius;
            CoordinateSystem = coordinateSystem;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            double total = Data.Sum();

            double currAngle = StartAngle;

            double minAngle = Data.Where(x => x > 0).Min() / total * 2 * Math.PI;

            minAngle = Math.Min(minAngle, 1);
            minAngle = Math.Max(minAngle, 0.1);

            for (int i = 0; i < Data.Count; i++)
            {
                double endAngle = currAngle + Data[i] / total * 2 * Math.PI;

                GraphicsPath pth = new GraphicsPath();

                if (InnerRadius[0] != 0 || InnerRadius[1] != 0)
                {
                    Point[] innerCircle = new Point[(int)((endAngle - currAngle) / minAngle * 10) + 1];

                    Point[] outerCircle = new Point[(int)((endAngle - currAngle) / minAngle * 10) + 1];

                    double step = (endAngle - currAngle) / (innerCircle.Length - 1);

                    for (int j = 0; j < innerCircle.Length; j++)
                    {
                        double currAng = currAngle + step * j;

                        if (Clockwise)
                        {
                            currAng *= -1;
                        }

                        innerCircle[j] = CoordinateSystem.ToPlotCoordinates(new double[] { Centre[0] + InnerRadius[0] * Math.Cos(currAng), Centre[1] + InnerRadius[1] * Math.Sin(currAng) });
                        outerCircle[outerCircle.Length - 1 - j] = CoordinateSystem.ToPlotCoordinates(new double[] { Centre[0] + OuterRadius[0] * Math.Cos(currAng), Centre[1] + OuterRadius[1] * Math.Sin(currAng) });
                    }

                    currAngle = endAngle;

                    pth.AddSmoothSpline(innerCircle);

                    pth.LineTo(outerCircle[0]);
                    pth.AddSmoothSpline(outerCircle);

                    pth.Close();
                }
                else
                {
                    Point[] outerCircle = new Point[(int)((endAngle - currAngle) / minAngle * 10) + 1];

                    double step = (endAngle - currAngle) / (outerCircle.Length - 1);

                    for (int j = 0; j < outerCircle.Length; j++)
                    {
                        double currAng = currAngle + step * j;

                        if (Clockwise)
                        {
                            currAng *= -1;
                        }

                        outerCircle[outerCircle.Length - 1 - j] = CoordinateSystem.ToPlotCoordinates(new double[] { Centre[0] + OuterRadius[0] * Math.Cos(currAng), Centre[1] + OuterRadius[1] * Math.Sin(currAng) });
                    }

                    currAngle = endAngle;

                    pth.MoveTo(CoordinateSystem.ToPlotCoordinates(Centre));

                    pth.LineTo(outerCircle[0]);
                    pth.AddSmoothSpline(outerCircle);

                    pth.Close();
                }

                PlotElementPresentationAttributes attributes = PresentationAttributes[i % PresentationAttributes.Count];

                string tag = Tag;
                string strokeTag = tag;

                if (target.UseUniqueTags && !string.IsNullOrEmpty(tag))
                {
                    tag += "@" + i.ToString();
                    strokeTag += "@" + i.ToString() + "_stroke";
                }

                if (attributes.Fill != null)
                {
                    target.FillPath(pth, attributes.Fill, tag);
                }

                if (attributes.Stroke != null)
                {
                    target.StrokePath(pth, attributes.Stroke, attributes.LineWidth, attributes.LineCap, attributes.LineJoin, attributes.LineDash, strokeTag);
                }
            }
        }
    }
}
