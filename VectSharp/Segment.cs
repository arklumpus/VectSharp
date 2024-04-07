/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2022 Giorgio Bianchini

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
using System.Diagnostics.Contracts;
using System.Linq;

namespace VectSharp
{

    /// <summary>
    /// Represents a segment as part of a <see cref="GraphicsPath"/>.
    /// </summary>
    public abstract class Segment
    {

        /// <summary>
        /// The type of the <see cref="Segment"/>.
        /// </summary>
        public abstract SegmentType Type { get; }

        /// <summary>
        /// The points used to define the <see cref="Segment"/>.
        /// </summary>
        public Point[] Points { get; protected set; }

        /// <summary>
        /// The end point of the <see cref="Segment"/>.
        /// </summary>
        public virtual Point Point
        {
            get
            {
                return Points[Points.Length - 1];
            }
        }

        /// <summary>
        /// Creates a copy of the <see cref="Segment"/>.
        /// </summary>
        /// <returns>A copy of the <see cref="Segment"/>.</returns>
        [Pure]
        public abstract Segment Clone();

        /// <summary>
        /// Computes the length of the <see cref="Segment"/>.
        /// </summary>
        /// <param name="previousPoint">The point from which the <see cref="Segment"/> starts (i.e. the endpoint of the previous <see cref="Segment"/>).</param>
        /// <returns>The length of the segment.</returns>
        public abstract double Measure(Point previousPoint);

        /// <summary>
        /// Gets the point on the <see cref="Segment"/> at the specified (relative) <paramref name="position"/>).
        /// </summary>
        /// <param name="previousPoint">The point from which the <see cref="Segment"/> starts (i.e. the endpoint of the previous <see cref="Segment"/>).</param>
        /// <param name="position">The relative position on the <see cref="Segment"/> (0 is the start of the <see cref="Segment"/>, 1 is the end of the <see cref="Segment"/>).</param>
        /// <returns>The point at the specified position.</returns>
        public abstract Point GetPointAt(Point previousPoint, double position);

        /// <summary>
        /// Gets the tangent to the <see cref="Segment"/> at the specified (relative) <paramref name="position"/>).
        /// </summary>
        /// <param name="previousPoint">The point from which the <see cref="Segment"/> starts (i.e. the endpoint of the previous <see cref="Segment"/>).</param>
        /// <param name="position">The relative position on the <see cref="Segment"/> (0 is the start of the <see cref="Segment"/>, 1 is the end of the <see cref="Segment"/>).</param>
        /// <returns>The tangent to the point at the specified position.</returns>
        public abstract Point GetTangentAt(Point previousPoint, double position);

        /// <summary>
        /// Transform the segment into a series of linear segments. Segments that are already linear are not changed.
        /// </summary>
        /// <param name="previousPoint">The point from which the <see cref="Segment"/> starts (i.e. the endpoint of the previous <see cref="Segment"/>).</param>
        /// <param name="resolution">The absolute length between successive samples in curve segments.</param>
        /// <returns>A collection of linear segments that approximate the current segment.</returns>
        [Pure]
        public abstract IEnumerable<Segment> Linearise(Point? previousPoint, double resolution);

        /// <summary>
        /// Gets the tanget at the points at which the segment would be linearised.
        /// </summary>
        /// <param name="previousPoint">The point from which the <see cref="Segment"/> starts (i.e. the endpoint of the previous <see cref="Segment"/>).</param>
        /// <param name="resolution">The absolute length between successive samples in curve segments.</param>
        /// <returns>A collection of tangents at the points in which the segment would be linearised.</returns>
        public abstract IEnumerable<Point> GetLinearisationTangents(Point? previousPoint, double resolution);

        /// <summary>
        /// Applies an arbitrary transformation to all of the points of the <see cref="Segment"/>.
        /// </summary>
        /// <param name="transformationFunction">An arbitrary transformation function.</param>
        /// <returns>A collection of <see cref="Segment"/>s that have been transformed according to the <paramref name="transformationFunction"/>.</returns>
        [Pure]
        public abstract IEnumerable<Segment> Transform(Func<Point, Point> transformationFunction);

        /// <summary>
        /// Flattens the <see cref="Segment"/>, replacing curve segments with series of line segments that approximate them, ensuring the specified maximum deviation from the original path.
        /// </summary>
        /// <param name="previousPoint">The point from which the <see cref="Segment"/> starts (i.e. the endpoint of the previous <see cref="Segment"/>).</param>
        /// <param name="flatness">The maximum deviation from the original path.</param>
        /// <returns>A collection of <see cref="Segment"/>s composed only of linear segments that approximates the current <see cref="Segment"/>.</returns>
        [Pure]
        public abstract IEnumerable<Segment> Flatten(Point? previousPoint, double flatness);

        /// <summary>
        /// Flattens the <see cref="Segment"/>, replacing curve segments with series of line segments that approximate them, ensuring the specified maximum deviation from the original path, assuming that the <see cref="Segment"/> will be drawn with the specified <paramref name="offset"/>.
        /// </summary>
        /// <param name="previousPoint">The point from which the <see cref="Segment"/> starts (i.e. the endpoint of the previous <see cref="Segment"/>).</param>
        /// <param name="offset">The offset that will be used to draw the <see cref="Segment"/> (e.g., the line width of the stroke).</param>
        /// <param name="flatness">The maximum deviation from the original path.</param>
        /// <returns>A collection of tuples where the first element is a <see cref="Point"/> representing the end-point of a linear segment, and the second element is a <see cref="Point"/> containing the value of the tangent to the original <see cref="Segment"/> at that point.</returns>
        public abstract IEnumerable<(Point point, Point tangent)> FlattenForOffsetAndGetTangents(Point? previousPoint, double offset, double flatness);
    }

    internal class MoveSegment : Segment
    {
        public override SegmentType Type => SegmentType.Move;

        public MoveSegment(Point p)
        {
            this.Points = new Point[] { p };
        }

        public MoveSegment(double x, double y)
        {
            this.Points = new Point[] { new Point(x, y) };
        }

        public override Segment Clone()
        {
            return new MoveSegment(this.Point);
        }

        public override double Measure(Point previousPoint)
        {
            return 0;
        }

        public override Point GetPointAt(Point previousPoint, double position)
        {
            throw new InvalidOperationException();
        }

        public override Point GetTangentAt(Point previousPoint, double position)
        {
            throw new InvalidOperationException();
        }

        public override IEnumerable<Segment> Linearise(Point? previousPoint, double resolution)
        {
            yield return new MoveSegment(this.Point);
        }

        public override IEnumerable<Point> GetLinearisationTangents(Point? previousPoint, double resolution)
        {
            throw new InvalidOperationException();
        }

        public override IEnumerable<Segment> Transform(Func<Point, Point> transformationFunction)
        {
            yield return new MoveSegment(transformationFunction(this.Point));
        }

        public override IEnumerable<Segment> Flatten(Point? previousPoint, double flatness)
        {
            yield return new MoveSegment(this.Point);
        }

        public override IEnumerable<(Point point, Point tangent)> FlattenForOffsetAndGetTangents(Point? previousPoint, double offset, double flatness)
        {
            throw new InvalidOperationException();
        }
    }

    internal class LineSegment : Segment
    {
        public override SegmentType Type => SegmentType.Line;

        public LineSegment(Point p)
        {
            this.Points = new Point[] { p };
        }

        public LineSegment(double x, double y)
        {
            this.Points = new Point[] { new Point(x, y) };
        }

        public override Segment Clone()
        {
            return new LineSegment(this.Point);
        }

        private double cachedLength = double.NaN;

        public override double Measure(Point previousPoint)
        {
            if (double.IsNaN(cachedLength))
            {
                cachedLength = Math.Sqrt((this.Point.X - previousPoint.X) * (this.Point.X - previousPoint.X) + (this.Point.Y - previousPoint.Y) * (this.Point.Y - previousPoint.Y));
            }

            return cachedLength;
        }

        public override Point GetPointAt(Point previousPoint, double position)
        {
            return new Point(previousPoint.X * (1 - position) + this.Point.X * position, previousPoint.Y * (1 - position) + this.Point.Y * position);
        }

        public override Point GetTangentAt(Point previousPoint, double position)
        {
            return new Point(this.Point.X - previousPoint.X, this.Point.Y - previousPoint.Y).Normalize();
        }

        public override IEnumerable<Segment> Linearise(Point? previousPoint, double resolution)
        {
            yield return new LineSegment(this.Point);
        }

        public override IEnumerable<Point> GetLinearisationTangents(Point? previousPoint, double resolution)
        {
            yield return this.GetTangentAt(previousPoint.Value, 1);
        }

        public override IEnumerable<Segment> Transform(Func<Point, Point> transformationFunction)
        {
            yield return new LineSegment(transformationFunction(this.Point));
        }

        public override IEnumerable<Segment> Flatten(Point? previousPoint, double flatness)
        {
            yield return new LineSegment(this.Point);
        }

        public override IEnumerable<(Point point, Point tangent)> FlattenForOffsetAndGetTangents(Point? previousPoint, double offset, double flatness)
        {
            if (previousPoint != null)
            {
                yield return (previousPoint.Value, this.GetTangentAt(previousPoint.Value, 1));
            }

            yield return (this.Point, this.GetTangentAt(previousPoint.Value, 1));
        }
    }

    internal class CloseSegment : Segment
    {
        public override SegmentType Type => SegmentType.Close;

        public CloseSegment() { }

        public override Segment Clone()
        {
            return new CloseSegment();
        }

        public override double Measure(Point previousPoint)
        {
            return 0;
        }

        public override Point GetPointAt(Point previousPoint, double position)
        {
            throw new InvalidOperationException();
        }

        public override Point GetTangentAt(Point previousPoint, double position)
        {
            throw new InvalidOperationException();
        }

        public override IEnumerable<Segment> Linearise(Point? previousPoint, double resolution)
        {
            yield return new CloseSegment();
        }

        public override IEnumerable<Point> GetLinearisationTangents(Point? previousPoint, double resolution)
        {
            throw new InvalidOperationException();
        }

        public override IEnumerable<Segment> Transform(Func<Point, Point> transformationFunction)
        {
            yield return new CloseSegment();
        }

        public override IEnumerable<Segment> Flatten(Point? previousPoint, double flatness)
        {
            yield return new CloseSegment();
        }

        public override IEnumerable<(Point point, Point tangent)> FlattenForOffsetAndGetTangents(Point? previousPoint, double offset, double flatness)
        {
            throw new InvalidOperationException();
        }
    }

    internal class CubicBezierSegment : Segment
    {
        public override SegmentType Type => SegmentType.CubicBezier;
        public CubicBezierSegment(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            Points = new Point[] { new Point(x1, y1), new Point(x2, y2), new Point(x3, y3) };
        }

        public CubicBezierSegment(Point p1, Point p2, Point p3)
        {
            Points = new Point[] { p1, p2, p3 };
        }

        public override Segment Clone()
        {
            return new CubicBezierSegment(Points[0], Points[1], Points[2]);
        }

        private double cachedLength = double.NaN;
        private int cachedSegments = -1;

        public override double Measure(Point previousPoint)
        {
            if (double.IsNaN(cachedLength))
            {
                int segments = 16;
                double prevLength = 0;
                double currLength = Measure(previousPoint, segments);

                while (currLength > 0.00001 && Math.Abs(currLength - prevLength) / currLength > 0.0001)
                {
                    segments *= 2;
                    prevLength = currLength;
                    currLength = Measure(previousPoint, segments);
                }

                cachedSegments = segments;

                cachedLength = currLength;
            }

            return cachedLength;
        }

        public Point GetBezierPointAt(Point previousPoint, double position)
        {
            if (position <= 1 && position >= 0)
            {
                return new Point(
                this.Points[2].X * position * position * position + 3 * this.Points[1].X * position * position * (1 - position) + 3 * this.Points[0].X * position * (1 - position) * (1 - position) + previousPoint.X * (1 - position) * (1 - position) * (1 - position),
                this.Points[2].Y * position * position * position + 3 * this.Points[1].Y * position * position * (1 - position) + 3 * this.Points[0].Y * position * (1 - position) * (1 - position) + previousPoint.Y * (1 - position) * (1 - position) * (1 - position)
                );
            }
            else if (position > 1)
            {
                Point tangent = GetBezierTangentAt(previousPoint, 1);

                double excessLength = (position - 1) * this.Measure(previousPoint);

                return new Point(this.Point.X + tangent.X * excessLength, this.Point.Y + tangent.Y * excessLength);
            }
            else
            {
                Point tangent = GetBezierTangentAt(previousPoint, 0);

                return new Point(previousPoint.X + tangent.X * position * this.Measure(previousPoint), previousPoint.Y + tangent.Y * position * this.Measure(previousPoint));
            }
        }

        public override Point GetPointAt(Point previousPoint, double position)
        {
            double t = GetTFromPosition(previousPoint, position);
            return this.GetBezierPointAt(previousPoint, t);
        }

        public override Point GetTangentAt(Point previousPoint, double position)
        {
            double t = GetTFromPosition(previousPoint, position);
            return this.GetBezierTangentAt(previousPoint, t);
        }

        private double Measure(Point startPoint, int segments)
        {
            double delta = 1.0 / segments;

            double tbr = 0;

            for (int i = 1; i < segments; i++)
            {
                Point p1 = GetBezierPointAt(startPoint, delta * (i - 1));
                Point p2 = GetBezierPointAt(startPoint, delta * i);

                tbr += Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            }

            return tbr;
        }

        private double Measure(Point startPoint, int segments, double maxT)
        {
            double delta = maxT / segments;

            double tbr = 0;

            for (int i = 1; i < segments; i++)
            {
                Point p1 = GetBezierPointAt(startPoint, delta * (i - 1));
                Point p2 = GetBezierPointAt(startPoint, delta * i);

                tbr += Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            }

            return tbr;
        }

        private Point GetBezierTangentAt(Point previousPoint, double position)
        {
            if (position <= 1 && position >= 0)
            {
                if (position == 0 && previousPoint.IsEqual(this.Points[0], GraphicsPath.Tolerance))
                {
                    return (this.Points[1] - previousPoint).Normalize();
                }
                else if (position == 1 && this.Points[2].IsEqual(this.Points[1], GraphicsPath.Tolerance))
                {
                    return (this.Points[2] - this.Points[0]).Normalize();
                }
                else
                {
                    return new Point(
                        3 * this.Points[2].X * position * position +
                        3 * this.Points[1].X * position * (2 - 3 * position) +
                        3 * this.Points[0].X * (3 * position * position - 4 * position + 1) +
                        -3 * previousPoint.X * (1 - position) * (1 - position),

                        3 * this.Points[2].Y * position * position +
                        3 * this.Points[1].Y * position * (2 - 3 * position) +
                        3 * this.Points[0].Y * (3 * position * position - 4 * position + 1) +
                        -3 * previousPoint.Y * (1 - position) * (1 - position)).Normalize();
                }
            }
            else if (position > 1)
            {
                return GetBezierTangentAt(previousPoint, 1);
            }
            else
            {
                return GetBezierTangentAt(previousPoint, 0);
            }
        }

        private double GetTFromPosition(Point previousPoint, double position)
        {
            if (position <= 0 || position >= 1)
            {
                return position;
            }
            else
            {
                double length = this.Measure(previousPoint);

                double lowerBound = 0;
                double upperBound = 0.5;

                double lowerPos = 0;
                double upperPos = Measure(previousPoint, (int)Math.Ceiling(this.cachedSegments * upperBound), upperBound) / length;

                if (upperPos < position)
                {
                    lowerBound = upperBound;
                    lowerPos = upperPos;

                    upperBound = 1;
                    upperPos = 1;
                }

                while (Math.Min(upperPos - position, position - lowerPos) > 0.001)
                {
                    double mid = (lowerBound + upperBound) * 0.5;
                    double midPos = Measure(previousPoint, (int)Math.Ceiling(this.cachedSegments * mid), mid) / length;

                    if (midPos > position)
                    {
                        upperBound = mid;
                        upperPos = midPos;
                    }
                    else
                    {
                        lowerBound = mid;
                        lowerPos = midPos;
                    }
                }

                return lowerBound + (position - lowerPos) / (upperPos - lowerPos) * (upperBound - lowerBound);
            }
        }

        public override IEnumerable<Segment> Linearise(Point? previousPoint, double resolution)
        {
            double length = this.Measure(previousPoint.Value);
            int segmentCount = (int)Math.Ceiling(length / resolution);

            for (int i = 0; i < segmentCount; i++)
            {
                yield return new LineSegment(this.GetPointAt(previousPoint.Value, (double)(i + 1) / segmentCount));
            }
        }

        public override IEnumerable<Point> GetLinearisationTangents(Point? previousPoint, double resolution)
        {
            double length = this.Measure(previousPoint.Value);
            int segmentCount = (int)Math.Ceiling(length / resolution);

            for (int i = 0; i < segmentCount; i++)
            {
                yield return this.GetTangentAt(previousPoint.Value, (double)(i + 1) / segmentCount);
            }
        }

        public override IEnumerable<Segment> Transform(Func<Point, Point> transformationFunction)
        {
            yield return new CubicBezierSegment(transformationFunction(this.Points[0]), transformationFunction(this.Points[1]), transformationFunction(this.Points[2]));
        }

        private (CubicBezierSegment first, CubicBezierSegment second) Subdivide(Point previousPoint, double t)
        {
            Point p0 = previousPoint + t * (this.Points[0] - previousPoint);
            Point p1 = this.Points[0] + t * (this.Points[1] - this.Points[0]);
            Point p2 = this.Points[1] + t * (this.Points[2] - this.Points[1]);
            Point p0_2 = p0 + t * (p1 - p0);
            Point p1_2 = p1 + t * (p2 - p1);
            Point p0_3 = p0_2 + t * (p1_2 - p0_2);

            return (new CubicBezierSegment(p0, p0_2, p0_3), new CubicBezierSegment(p1_2, p2, this.Points[2]));
        }

        // Based on https://doi.org/10.1016/j.cag.2005.08.002
        private IEnumerable<Segment> FlattenPrivate(Point? previousPoint, double flatness)
        {
            Point startingPoint;

            startingPoint = previousPoint ?? this.Points[0];

            double s2 = ((this.Points[1].X - startingPoint.X) * (this.Points[0].Y - startingPoint.Y) - (this.Points[1].Y - startingPoint.Y) * (this.Points[0].X - startingPoint.X)) / Math.Sqrt((this.Points[0].X - startingPoint.X) * (this.Points[0].X - startingPoint.X) + (this.Points[0].Y - startingPoint.Y) * (this.Points[0].Y - startingPoint.Y));

            double t = 2 * Math.Sqrt(flatness / (3 * Math.Abs(s2)));

            if (t < 1)
            {
                (CubicBezierSegment first, CubicBezierSegment second) = this.Subdivide(startingPoint, t);

                yield return new LineSegment(first.Points[2]);

                foreach (Segment seg in second.FlattenPrivate(first.Points[2], flatness))
                {
                    yield return seg;
                }
            }
            else
            {
                yield return new LineSegment(this.Points[2]);
            }
        }

        // Based on https://doi.org/10.1016/j.cag.2005.08.002
        public override IEnumerable<(Point point, Point tangent)> FlattenForOffsetAndGetTangents(Point? previousPoint, double offset, double flatness)
        {
            return FlattenForOffsetAndGetTangents(previousPoint, offset, flatness, false);
        }

        // Based on https://doi.org/10.1016/j.cag.2005.08.002
        private IEnumerable<(Point point, Point tangent)> FlattenForOffsetAndGetTangents(Point? previousPoint, double offset, double flatness, bool skipFirst)
        {
            Point startingPoint;

            startingPoint = previousPoint ?? this.Points[0];

            if (!skipFirst)
            {
                yield return (startingPoint, this.GetBezierTangentAt(startingPoint, 0));
            }

            double r1sq = (this.Points[0].X - startingPoint.X) * (this.Points[0].X - startingPoint.X) + (this.Points[0].Y - startingPoint.Y) * (this.Points[0].Y - startingPoint.Y);
            double s2 = ((this.Points[1].X - startingPoint.X) * (this.Points[0].Y - startingPoint.Y) - (this.Points[1].Y - startingPoint.Y) * (this.Points[0].X - startingPoint.X)) / Math.Sqrt((this.Points[0].X - startingPoint.X) * (this.Points[0].X - startingPoint.X) + (this.Points[0].Y - startingPoint.Y) * (this.Points[0].Y - startingPoint.Y));

            if (double.IsNaN(s2))
            {
                s2 = ((this.Points[2].X - startingPoint.X) * (this.Points[1].Y - startingPoint.Y) - (this.Points[2].Y - startingPoint.Y) * (this.Points[1].X - startingPoint.X)) / Math.Sqrt((this.Points[1].X - startingPoint.X) * (this.Points[1].X - startingPoint.X) + (this.Points[1].Y - startingPoint.Y) * (this.Points[1].Y - startingPoint.Y));
            }

            if (double.IsNaN(s2))
            {
                s2 = 1;
            }

            double t = 2 * Math.Sqrt(Math.Abs(flatness / (3 * Math.Abs(s2) * (1 - offset * s2 / (3 * r1sq)))));

            if (r1sq < 1e-7)
            {
                t = 2 * Math.Sqrt(Math.Abs(flatness / (3 * Math.Abs(s2))));

                if (this.Points[1].X == startingPoint.X && this.Points[1].Y == startingPoint.Y && this.Points[2].X == startingPoint.X && this.Points[2].Y == startingPoint.Y)
                {
                    yield return (this.Point, new Point(double.NaN, double.NaN));
                    yield break;
                }
            }

            if (t < 1)
            {
                (CubicBezierSegment first, CubicBezierSegment second) = this.Subdivide(startingPoint, t);

                yield return (first.Points[2], this.GetBezierTangentAt(startingPoint, t));

                foreach ((Point, Point) p in second.FlattenForOffsetAndGetTangents(first.Points[2], offset, flatness, true))
                {
                    yield return p;
                }
            }
            else
            {
                yield return (this.Points[2], this.GetBezierTangentAt(startingPoint, 1));
            }
        }

        internal IEnumerable<CubicBezierSegment> MonotoniseOnY(Point previousPoint)
        {
            double a = previousPoint.Y;
            double b = this.Points[0].Y;
            double c = this.Points[1].Y;
            double d = this.Points[2].Y;

            if ((a + 3 * c != 3 * b + d))
            {
                double t1 = ((-6 * a + 12 * b - 6 * c) + Math.Sqrt((6 * a - 12 * b + 6 * c) * (6 * a - 12 * b + 6 * c) - 4 * (3 * b - 3 * a) * (-3 * a + 9 * b - 9 * c + 3 * d))) / (2 * (-3 * a + 9 * b - 9 * c + 3 * d));
                double t2 = ((-6 * a + 12 * b - 6 * c) - Math.Sqrt((6 * a - 12 * b + 6 * c) * (6 * a - 12 * b + 6 * c) - 4 * (3 * b - 3 * a) * (-3 * a + 9 * b - 9 * c + 3 * d))) / (2 * (-3 * a + 9 * b - 9 * c + 3 * d));

                if (t1 > 0 && t1 < 1 && t2 > 0 && t2 < 1)
                {
                    (CubicBezierSegment seg1, CubicBezierSegment seg2) = this.Subdivide(previousPoint, Math.Min(t1, t2));

                    yield return seg1;

                    foreach (CubicBezierSegment seg in seg2.MonotoniseOnY(seg1.Point))
                    {
                        yield return seg;
                    }
                }
                else if (t1 > 0 && t1 < 1)
                {
                    (CubicBezierSegment seg1, CubicBezierSegment seg2) = this.Subdivide(previousPoint, t1);

                    yield return seg1;
                    yield return seg2;
                }
                else if (t2 > 0 && t2 < 1)
                {
                    (CubicBezierSegment seg1, CubicBezierSegment seg2) = this.Subdivide(previousPoint, t2);

                    yield return seg1;
                    yield return seg2;
                }
                else
                {
                    yield return this;
                }
            }
            else
            {
                double t = (a - b) / (2 * (a - 2 * b + c));

                if (t > 0 && t < 1)
                {
                    (CubicBezierSegment seg1, CubicBezierSegment seg2) = this.Subdivide(previousPoint, t);

                    yield return seg1;
                    yield return seg2;
                }
                else
                {
                    yield return this;
                }
            }
        }

        // Based on https://doi.org/10.1016/j.cag.2005.08.002
        public override IEnumerable<Segment> Flatten(Point? previousPoint, double flatness)
        {
            Point currPoint = previousPoint ?? this.Points[0];

            foreach (Segment seg in this.BreakAtInflectionPoints(previousPoint, flatness))
            {
                if (seg is CubicBezierSegment cub)
                {
                    foreach (Segment seg2 in cub.FlattenPrivate(currPoint, flatness))
                    {
                        yield return seg2;
                    }
                }
                else
                {
                    yield return seg;
                }
                currPoint = seg.Point;
            }
        }

        // Based on https://doi.org/10.1016/j.cag.2005.08.002
        internal IEnumerable<Segment> BreakAtInflectionPoints(Point? previousPoint, double flatness)
        {
            Point startingPoint;

            startingPoint = previousPoint ?? this.Points[0];

            double ax = -startingPoint.X + 3 * this.Points[0].X - 3 * this.Points[1].X + this.Points[2].X;
            double ay = -startingPoint.Y + 3 * this.Points[0].Y - 3 * this.Points[1].Y + this.Points[2].Y;
            double bx = 3 * startingPoint.X - 6 * this.Points[0].X + 3 * this.Points[1].X;
            double by = 3 * startingPoint.Y - 6 * this.Points[0].Y + 3 * this.Points[1].Y;
            double cx = -3 * startingPoint.X + 3 * this.Points[0].X;
            double cy = -3 * startingPoint.Y + 3 * this.Points[0].Y;

            double tcusp = -0.5 * (ay * cx - ax * cy) / (ay * bx - ax * by);

            double t1 = tcusp - Math.Sqrt(tcusp * tcusp - (by * cx - bx * cy) / (ay * bx - ax * by) / 3);
            double t2 = tcusp + Math.Sqrt(tcusp * tcusp - (by * cx - bx * cy) / (ay * bx - ax * by) / 3);

            List<double> validInflectionPoints = new List<double>();

            if (t1 >= 0 && t1 <= 1)
            {
                validInflectionPoints.Add(t1);
            }

            if (t2 >= 0 && t2 <= 1 && (validInflectionPoints.Count == 0 || t2 - validInflectionPoints[0] > 1e-5))
            {
                validInflectionPoints.Add(t2);
            }

            if (validInflectionPoints.Count == 0)
            {
                yield return this;
            }
            else if (validInflectionPoints.Count == 1)
            {
                (CubicBezierSegment first, CubicBezierSegment second) = this.Subdivide(startingPoint, validInflectionPoints[0]);

                double s3 = Math.Abs(((second.Points[2].X - first.Point.X) * (second.Points[0].Y - first.Point.Y) - (second.Points[2].Y - first.Point.Y) * (second.Points[0].X - first.Point.X)) / Math.Sqrt((second.Points[0].X - first.Point.X) * (second.Points[0].X - first.Point.X) + (second.Points[0].Y - first.Point.Y) * (second.Points[0].Y - first.Point.Y)));

                double tf = Math.Pow(flatness / s3, 1.0 / 3);

                if (double.IsNaN(tf) || double.IsInfinity(tf))
                {
                    tf = 0;
                }

                double tm = Math.Max(0, validInflectionPoints[0] - tf * (1 - validInflectionPoints[0]));
                double tp = Math.Min(validInflectionPoints[0] + tf * (1 - validInflectionPoints[0]), 1);

                if (tm > 0 && tp < 1)
                {
                    (first, _) = this.Subdivide(startingPoint, tm);

                    CubicBezierSegment temp;
                    (temp, second) = this.Subdivide(startingPoint, tp);

                    yield return first;

                    if (tm != tp)
                    {
                        yield return new LineSegment(temp.Point);
                    }

                    yield return second;
                }
                else if (tm == 0 && tp < 1)
                {
                    (first, second) = this.Subdivide(startingPoint, tp);

                    yield return new LineSegment(first.Point);

                    yield return second;
                }
                else if (tm > 0 && tp == 1)
                {
                    (first, _) = this.Subdivide(startingPoint, tm);

                    yield return first;

                    if (tm != tp)
                    {
                        yield return new LineSegment(this.Point);
                    }
                }
                else
                {
                    yield return new LineSegment(this.Point);
                }
            }
            else if (validInflectionPoints.Count == 2)
            {
                (CubicBezierSegment first, CubicBezierSegment second) = this.Subdivide(startingPoint, validInflectionPoints[0]);

                double s3 = Math.Abs(((second.Points[2].X - first.Point.X) * (second.Points[0].Y - first.Point.Y) - (second.Points[2].Y - first.Point.Y) * (second.Points[0].X - first.Point.X)) / Math.Sqrt((second.Points[0].X - first.Point.X) * (second.Points[0].X - first.Point.X) + (second.Points[0].Y - first.Point.Y) * (second.Points[0].Y - first.Point.Y)));

                double tf = Math.Pow(flatness / s3, 1.0 / 3);

                if (double.IsNaN(tf) || double.IsInfinity(tf))
                {
                    tf = 0;
                }

                double t1m = Math.Max(validInflectionPoints[0] - tf * (1 - validInflectionPoints[0]), 0);
                double t1p = Math.Min(validInflectionPoints[0] + tf * (1 - validInflectionPoints[0]), validInflectionPoints[1]);


                (first, second) = this.Subdivide(startingPoint, validInflectionPoints[1]);

                s3 = Math.Abs(((second.Points[2].X - first.Point.X) * (second.Points[0].Y - first.Point.Y) - (second.Points[2].Y - first.Point.Y) * (second.Points[0].X - first.Point.X)) / Math.Sqrt((second.Points[0].X - first.Point.X) * (second.Points[0].X - first.Point.X) + (second.Points[0].Y - first.Point.Y) * (second.Points[0].Y - first.Point.Y)));

                tf = Math.Pow(flatness / s3, 1.0 / 3);

                if (double.IsNaN(tf) || double.IsInfinity(tf))
                {
                    tf = 0;
                }

                double t2m = Math.Max(validInflectionPoints[1] - tf * (1 - validInflectionPoints[1]), t1p);
                double t2p = Math.Min(validInflectionPoints[1] + tf * (1 - validInflectionPoints[1]), 1);

                if (t1m > 0)
                {
                    (first, _) = this.Subdivide(startingPoint, t1m);
                    yield return first;
                }

                if (t1p > t1m)
                {
                    CubicBezierSegment temp;
                    (temp, _) = this.Subdivide(startingPoint, t1p);
                    yield return new LineSegment(temp.Point);
                }

                if (t2m > t1p)
                {
                    CubicBezierSegment temp, third;
                    (temp, second) = this.Subdivide(startingPoint, t1p);
                    (third, _) = second.Subdivide(temp.Point, (t2m - t1p) / (1 - t1p));
                    yield return third;
                }

                if (t2p > t2m)
                {
                    CubicBezierSegment temp;
                    (temp, second) = this.Subdivide(startingPoint, t1p);
                    (temp, _) = second.Subdivide(temp.Point, (t2p - t1p) / (1 - t1p));
                    yield return new LineSegment(temp.Point);
                }

                if (t2p < 1)
                {
                    CubicBezierSegment temp, fourth;
                    (temp, second) = this.Subdivide(startingPoint, t1p);
                    (_, fourth) = second.Subdivide(temp.Point, (t2p - t1p) / (1 - t1p));
                    yield return fourth;
                }
            }
        }
    }

    internal class ArcSegment : Segment
    {
        public override SegmentType Type => SegmentType.Arc;

        public Segment[] ToBezierSegments()
        {
            List<Segment> tbr = new List<Segment>();

            if (EndAngle > StartAngle)
            {
                if (EndAngle - StartAngle <= Math.PI / 2)
                {
                    tbr.AddRange(GetBezierSegment(Points[0].X, Points[0].Y, Radius, StartAngle, EndAngle, true));
                }
                else
                {
                    int count = (int)Math.Ceiling(2 * (EndAngle - StartAngle) / Math.PI);
                    double angle = StartAngle;

                    for (int i = 0; i < count; i++)
                    {
                        tbr.AddRange(GetBezierSegment(Points[0].X, Points[0].Y, Radius, angle, angle + (EndAngle - StartAngle) / count, i == 0));
                        angle += (EndAngle - StartAngle) / count;
                    }
                }
            }
            else if (EndAngle < StartAngle)
            {
                Point startPoint = new Point(Points[0].X + Radius * Math.Cos(EndAngle), Points[0].Y + Radius * Math.Sin(EndAngle));
                if (StartAngle - EndAngle <= Math.PI / 2)
                {
                    tbr.AddRange(GetBezierSegment(Points[0].X, Points[0].Y, Radius, EndAngle, StartAngle, true));
                }
                else
                {
                    int count = (int)Math.Ceiling(2 * (StartAngle - EndAngle) / Math.PI);
                    double angle = EndAngle;

                    for (int i = 0; i < count; i++)
                    {
                        tbr.AddRange(GetBezierSegment(Points[0].X, Points[0].Y, Radius, angle, angle + (StartAngle - EndAngle) / count, i == 0));
                        angle += (StartAngle - EndAngle) / count;
                    }
                }

                return ReverseSegments(tbr, startPoint).ToArray();
            }

            return tbr.ToArray();
        }

        private static Segment[] ReverseSegments(IReadOnlyList<Segment> originalSegments, Point startPoint)
        {
            List<Segment> tbr = new List<Segment>(originalSegments.Count);

            for (int i = originalSegments.Count - 1; i >= 0; i--)
            {
                switch (originalSegments[i].Type)
                {
                    case SegmentType.Line:
                        if (i > 0)
                        {
                            tbr.Add(new LineSegment(originalSegments[i - 1].Point));
                        }
                        else
                        {
                            tbr.Add(new LineSegment(startPoint));
                        }
                        break;
                    case SegmentType.CubicBezier:
                        CubicBezierSegment originalSegment = (CubicBezierSegment)originalSegments[i];
                        if (i > 0)
                        {
                            tbr.Add(new CubicBezierSegment(originalSegment.Points[1], originalSegment.Points[0], originalSegments[i - 1].Point));
                        }
                        else
                        {
                            tbr.Add(new CubicBezierSegment(originalSegment.Points[1], originalSegment.Points[0], startPoint));
                        }
                        break;
                }
            }

            return tbr.ToArray();
        }

        const double k = 0.55191496;

        private static Segment[] GetBezierSegment(double cX, double cY, double radius, double startAngle, double endAngle, bool firstArc)
        {
            double phi = Math.PI / 4;

            double x1 = radius * Math.Cos(phi);
            double y1 = radius * Math.Sin(phi);

            double x4 = x1;
            double y4 = -y1;

            double x3 = x1 + k * radius * Math.Sin(phi);
            double y3 = y1 - k * radius * Math.Cos(phi);

            double x2 = x4 + k * radius * Math.Sin(phi);
            double y2 = y4 + k * radius * Math.Cos(phi);

            double u = 2 * (endAngle - startAngle) / Math.PI;

            double fx2 = (1 - u) * x4 + u * x2;
            double fy2 = (1 - u) * y4 + u * y2;

            double fx3 = (1 - u) * fx2 + u * ((1 - u) * x2 + u * x3);
            double fy3 = (1 - u) * fy2 + u * ((1 - u) * y2 + u * y3);

            double rX1 = cX + radius * Math.Cos(startAngle);
            double rY1 = cY + radius * Math.Sin(startAngle);

            double rX4 = cX + radius * Math.Cos(endAngle);
            double rY4 = cY + radius * Math.Sin(endAngle);

            Point rot2 = Utils.RotatePoint(new Point(fx2, fy2), phi + startAngle);
            Point rot3 = Utils.RotatePoint(new Point(fx3, fy3), phi + startAngle);

            List<Segment> tbr = new List<Segment>();

            if (firstArc)
            {
                tbr.Add(new LineSegment(rX1, rY1));
            }

            tbr.Add(new CubicBezierSegment(cX + rot2.X, cY + rot2.Y, cX + rot3.X, cY + rot3.Y, rX4, rY4));

            return tbr.ToArray();
        }
        public double Radius { get; }
        public double StartAngle { get; }
        public double EndAngle { get; }

        public ArcSegment(Point center, double radius, double startAngle, double endAngle)
        {
            this.Points = new Point[] { center };
            this.Radius = radius;
            this.StartAngle = startAngle;
            this.EndAngle = endAngle;
        }

        public ArcSegment(double centerX, double centerY, double radius, double startAngle, double endAngle)
        {
            this.Points = new Point[] { new Point(centerX, centerY) };
            this.Radius = radius;
            this.StartAngle = startAngle;
            this.EndAngle = endAngle;
        }

        public override Segment Clone()
        {
            return new ArcSegment(Point.X, Point.Y, Radius, StartAngle, EndAngle);
        }

        public override Point Point
        {
            get
            {
                return new Point(this.Points[0].X + Math.Cos(EndAngle) * Radius, this.Points[0].Y + Math.Sin(EndAngle) * Radius);
            }
        }


        private double cachedLength = double.NaN;

        public override double Measure(Point previousPoint)
        {
            if (double.IsNaN(cachedLength))
            {
                Point arcStartPoint = new Point(this.Points[0].X + Math.Cos(StartAngle) * Radius, this.Points[0].Y + Math.Sin(StartAngle) * Radius);

                cachedLength = Radius * Math.Abs(EndAngle - StartAngle) + Math.Sqrt((arcStartPoint.X - previousPoint.X) * (arcStartPoint.X - previousPoint.X) + (arcStartPoint.Y - previousPoint.Y) * (arcStartPoint.Y - previousPoint.Y));
            }

            return cachedLength;
        }

        public override Point GetPointAt(Point previousPoint, double position)
        {
            double totalLength = this.Measure(previousPoint);
            double arcLength = Radius * Math.Abs(EndAngle - StartAngle);

            double preArc = (totalLength - arcLength) / totalLength;

            if (position < preArc)
            {
                if (position >= 0)
                {
                    double relPos = position / preArc;
                    Point arcStartPoint = new Point(this.Points[0].X + Math.Cos(StartAngle) * Radius, this.Points[0].Y + Math.Sin(StartAngle) * Radius);

                    return new Point(previousPoint.X * (1 - relPos) + arcStartPoint.X * relPos, previousPoint.Y * (1 - relPos) + arcStartPoint.Y * relPos);
                }
                else
                {
                    Point arcStartPoint = new Point(this.Points[0].X + Math.Cos(StartAngle) * Radius, this.Points[0].Y + Math.Sin(StartAngle) * Radius);
                    Point tangent = GetTangentAt(previousPoint, 0);
                    double excessLength = position * this.Measure(previousPoint);
                    return new Point(arcStartPoint.X + tangent.X * excessLength, arcStartPoint.Y + tangent.Y * excessLength);
                }
            }
            else
            {
                double relPos = position - preArc / (1 - preArc);

                if (relPos <= 1)
                {
                    double angle = StartAngle * (1 - relPos) + EndAngle * relPos;
                    return new Point(this.Points[0].X + Radius * Math.Cos(angle), this.Points[0].Y + Radius * Math.Sin(angle));
                }
                else
                {
                    Point arcEndPoint = this.Point;
                    Point tangent = GetTangentAt(previousPoint, 1);
                    double excessLength = (position - 1) * this.Measure(previousPoint);
                    return new Point(arcEndPoint.X + tangent.X * excessLength, arcEndPoint.Y + tangent.Y * excessLength);
                }
            }
        }

        public override Point GetTangentAt(Point previousPoint, double position)
        {
            double totalLength = this.Measure(previousPoint);
            double arcLength = Radius * Math.Abs(EndAngle - StartAngle);

            double preArc = (totalLength - arcLength) / totalLength;

            if (position < preArc)
            {
                Point arcStartPoint = new Point(this.Points[0].X + Math.Cos(StartAngle) * Radius, this.Points[0].Y + Math.Sin(StartAngle) * Radius);
                Point tang = new Point((arcStartPoint.X - previousPoint.X) * Math.Sign(EndAngle - StartAngle), (arcStartPoint.Y - previousPoint.Y) * Math.Sign(EndAngle - StartAngle)).Normalize();

                if (tang.Modulus() > 0.001)
                {
                    return tang.Normalize();
                }
                else
                {
                    return this.GetTangentAt(previousPoint, 0);
                }
            }
            else
            {
                double relPos = position - preArc / (1 - preArc);

                if (relPos <= 1)
                {
                    double angle = StartAngle * (1 - relPos) + EndAngle * relPos;
                    return new Point(-Math.Sin(angle) * Math.Sign(EndAngle - StartAngle), Math.Cos(angle) * Math.Sign(EndAngle - StartAngle));
                }
                else
                {
                    return new Point(-Math.Sin(EndAngle) * Math.Sign(EndAngle - StartAngle), Math.Cos(EndAngle) * Math.Sign(EndAngle - StartAngle));
                }
            }

        }

        public override IEnumerable<Segment> Linearise(Point? previousPoint, double resolution)
        {
            double length = this.Measure(previousPoint.Value);
            int segmentCount = (int)Math.Ceiling(length / resolution);

            for (int i = 0; i < segmentCount; i++)
            {
                yield return new LineSegment(this.GetPointAt(previousPoint.Value, (double)(i + 1) / segmentCount));
            }
        }
        public override IEnumerable<Point> GetLinearisationTangents(Point? previousPoint, double resolution)
        {
            double length = this.Measure(previousPoint.Value);
            int segmentCount = (int)Math.Ceiling(length / resolution);

            for (int i = 0; i < segmentCount; i++)
            {
                yield return this.GetTangentAt(previousPoint.Value, (double)(i + 1) / segmentCount);
            }
        }

        public override IEnumerable<Segment> Transform(Func<Point, Point> transformationFunction)
        {
            foreach (Segment seg in this.ToBezierSegments())
            {
                foreach (Segment seg2 in seg.Transform(transformationFunction))
                {
                    yield return seg2;
                }
            }
        }

        public override IEnumerable<Segment> Flatten(Point? previousPoint, double flatness)
        {
            double length = this.Measure(previousPoint.Value);
            double resolution = 2 * Math.Sqrt(flatness * (2 * this.Radius - flatness));

            int segmentCount = (int)Math.Ceiling(length / resolution);

            for (int i = 0; i < segmentCount; i++)
            {
                yield return new LineSegment(this.GetPointAt(previousPoint.Value, (double)(i + 1) / segmentCount));
            }
        }

        public override IEnumerable<(Point point, Point tangent)> FlattenForOffsetAndGetTangents(Point? previousPoint, double offset, double flatness)
        {
            double length = this.Measure(previousPoint.Value);
            double resolution = 2 * Math.Sqrt(flatness * (2 * (this.Radius + offset) - flatness));

            int segmentCount = (int)Math.Ceiling(length / resolution);

            yield return (this.GetPointAt(previousPoint.Value, 0), this.GetTangentAt(previousPoint.Value, 0));

            for (int i = 0; i < segmentCount; i++)
            {
                yield return (this.GetPointAt(previousPoint.Value, (double)(i + 1) / segmentCount), this.GetTangentAt(previousPoint.Value, (double)(i + 1) / segmentCount));
            }
        }
    }


}
