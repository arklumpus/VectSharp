/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2024 Giorgio Bianchini, University of Bristol

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
    /// A plot element that draws a swarm plot.
    /// </summary>
    public class Swarm : IPlotElement
    {
        /// <summary>
        /// The position of the origin of the swarm (e.g., the 0 in data space coordinates).
        /// </summary>
        public IReadOnlyList<double> Position { get; set; }

        /// <summary>
        /// The direction along which the swarm is drawn, in data space coordinates.
        /// </summary>
        public IReadOnlyList<double> Direction { get; set; }

        /// <summary>
        /// The values whose distribution is displayed by the swarm plot.
        /// </summary>
        public IReadOnlyList<double> Data { get; }

        /// <summary>
        /// The size of the points in the swarm plot, in plot coordinates.
        /// </summary>
        public double PointSize { get; set; } = 2;

        /// <summary>
        /// The minimum margin between points in the swarm plot, in plot coordinates.
        /// </summary>
        public double PointMargin { get; set; } = 0.25;

        /// <summary>
        /// The <see cref="IDataPointElement"/> used to draw the swarm points.
        /// </summary>
        public IDataPointElement SwarmPointElement { get; set; } = new PathDataPointElement();

        /// <summary>
        /// Presentation attributes for the swarm plot.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { Fill = Colours.White };

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public IContinuousInvertibleCoordinateSystem CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => this.CoordinateSystem;

        /// <summary>
        /// A tag to identify the swarm in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Create a new <see cref="Swarm"/> instance.
        /// </summary>
        /// <param name="position">The position of the origin of the swarm (e.g., the 0 in data space coordinates).</param>
        /// <param name="direction">The direction along which the swarm is drawn, in data space coordinates.</param>
        /// <param name="data">The values whose distribution is displayed by the swarm plot.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Swarm(IReadOnlyList<double> position, IReadOnlyList<double> direction, IEnumerable<double> data, IContinuousInvertibleCoordinateSystem coordinateSystem)
        {
            this.Position = position;
            this.Direction = direction;
            this.Data = data.OrderBy(x => x).ToArray();
            this.CoordinateSystem = coordinateSystem;
        }

        private static (Point, double) GetMinDistance(Point pt, List<Point> plottedPoints)
        {
            double minDist = double.MaxValue;
            Point minPoint = new Point();

            for (int i = 0; i < plottedPoints.Count; i++)
            {
                double dist = (plottedPoints[i] - pt).Modulus();
                if (dist < minDist)
                {
                    minDist = dist;
                    minPoint = plottedPoints[i];
                }
            }

            return (minPoint, minDist);
        }

        private IReadOnlyList<double> ProposePoint(IReadOnlyList<double> pt, List<Point> plottedPoints, IReadOnlyList<double> perpendicularDirection)
        {
            Point projPt = CoordinateSystem.ToPlotCoordinates(pt);

            (Point closestPoint, double minDist) = GetMinDistance(projPt, plottedPoints);

            while (minDist < PointSize * 2 + PointMargin * PointSize)
            {
                Point localPerpDirectionPt = (CoordinateSystem.ToPlotCoordinates(CoordinateSystem.GetAround(CoordinateSystem.ToDataCoordinates(projPt), perpendicularDirection)) - projPt).Normalize();
                double parallDist = (projPt - closestPoint).Select((x, i) => x * localPerpDirectionPt[i]).Sum();
                projPt = projPt + localPerpDirectionPt * (-parallDist + Math.Sqrt(parallDist * parallDist - (projPt - closestPoint).Select(x => x * x).Sum() + (PointSize * 2 + PointMargin * PointSize) * (PointSize * 2 + PointMargin * PointSize))) * 1.0001;
                (closestPoint, minDist) = GetMinDistance(projPt, plottedPoints);
            }

            return CoordinateSystem.ToDataCoordinates(projPt);
        }

        private static double[] GetPerpendicularVector(IReadOnlyList<double> v)
        {
            double[] tbr = new double[v.Count];

            int m = -1;

            for (int i = 0; i < v.Count; i++)
            {
                if (v[i] != 0)
                {
                    m = i; break;
                }
            }

            if (m < 0)
            {
                return tbr;
            }

            int n = m == 0 ? 1 : 0;

            double mod = Math.Sqrt(v[m] * v[m] + v[n] * v[n]);

            tbr[m] = -v[n] / mod;
            tbr[n] = v[m] / mod;

            return tbr;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            List<Point> plottedPoints = new List<Point>();

            double[] perpDirection1 = GetPerpendicularVector(this.Direction);
            double[] perpDirection2 = perpDirection1.Select(x => -x).ToArray();

            for (int i = 0; i < Data.Count; i++)
            {
                double[] point = new double[this.Position.Count];

                for (int j = 0; j < this.Position.Count; j++)
                {
                    point[j] = this.Position[j] + Data[i] * this.Direction[j];
                }

                IReadOnlyList<double> point1 = ProposePoint(point, plottedPoints, perpDirection1);
                IReadOnlyList<double> point2 = ProposePoint(point, plottedPoints, perpDirection2);

                Point proj = CoordinateSystem.ToPlotCoordinates(point);
                Point pt1 = CoordinateSystem.ToPlotCoordinates(point1);
                Point pt2 = CoordinateSystem.ToPlotCoordinates(point2);

                Point pt = (pt1 - proj).Modulus() < (pt2 - proj).Modulus() ? pt1 : pt2;

                plottedPoints.Add(pt);

                string tag = (Tag == null || !target.UseUniqueTags) ? this.Tag : (this.Tag + "@" + i.ToString());

                target.Save();
                target.Translate(pt);
                target.Scale(PointSize, PointSize);
                SwarmPointElement.Plot(target, this.PresentationAttributes, tag);
                target.Restore();
            }
        }
    }
}
