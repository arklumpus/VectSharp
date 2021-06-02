/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020  Giorgio Bianchini
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
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
        public abstract IEnumerable<Segment> Transform(Func<Point, Point> transformationFunction);
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

        private Point GetBezierPointAt(Point previousPoint, double position)
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
    }


}
