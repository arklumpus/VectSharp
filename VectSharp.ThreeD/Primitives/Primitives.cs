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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectSharp.ThreeD
{
    internal static class Intersections3D
    {
        public static bool PointInTriangle(Point3D p, Point3D A, Point3D B, Point3D C)
        {
            Vector3D v2 = p - A;

            Vector3D v0 = B - A;
            Vector3D v1 = C - A;
            double d00 = v0 * v0;
            double d01 = v0 * v1;
            double d11 = v1 * v1;

            double invDenomin = 1.0 / (d00 * d11 - d01 * d01);

            double d20 = v2 * v0;
            double d21 = v2 * v1;

            double v = (d11 * d20 - d01 * d21) * invDenomin;
            double w = (d00 * d21 - d01 * d20) * invDenomin;
            double u = 1 - v - w;

            return u >= 0 && v >= 0 && w >= 0 && u <= 1 && v <= 1 && w <= 1;
        }
    }

    internal static class Intersections2D
    {
        public static bool AreEqual(Point p1, Point p2, double tolerance)
        {
            return Math.Abs(p1.X - p2.X) <= tolerance && Math.Abs(p1.Y - p2.Y) <= tolerance;
        }

        public static bool PointInTriangle(Point pt, Point A, Point B, Point C)
        {
            double signAB = (pt.X - B.X) * (A.Y - B.Y) - (A.X - B.X) * (pt.Y - B.Y);
            double signBC = (pt.X - C.X) * (B.Y - C.Y) - (B.X - C.X) * (pt.Y - C.Y);
            double signCA = (pt.X - A.X) * (C.Y - A.Y) - (C.X - A.X) * (pt.Y - A.Y);

            return !((signAB < 0 || signBC < 0 || signCA < 0) && (signAB > 0 || signBC > 0 || signCA > 0));
        }

        public static bool PointInTriangle(int ptX, int ptY, Point A, Point B, Point C)
        {
            double signAB = (ptX - B.X) * (A.Y - B.Y) - (A.X - B.X) * (ptY - B.Y);
            double signBC = (ptX - C.X) * (B.Y - C.Y) - (B.X - C.X) * (ptY - C.Y);
            double signCA = (ptX - A.X) * (C.Y - A.Y) - (C.X - A.X) * (ptY - A.Y);

            return !((signAB < 0 || signBC < 0 || signCA < 0) && (signAB > 0 || signBC > 0 || signCA > 0));
        }

        public static Point? IntersectionPoint(Point l11, Point l12, Point l21, Point l22)
        {
            double denomin = (l11.X - l12.X) * (l21.Y - l22.Y) - (l11.Y - l12.Y) * (l21.X - l22.X);

            if (denomin != 0)
            {
                double t = ((l11.X - l21.X) * (l21.Y - l22.Y) - (l11.Y - l21.Y) * (l21.X - l22.X));

                if ((denomin > 0 && t >= 0 && t <= denomin) || (denomin < 0 && t <= 0 && t >= denomin))
                {
                    double s = -((l11.X - l12.X) * (l11.Y - l21.Y) - (l11.Y - l12.Y) * (l11.X - l21.X));

                    if ((denomin > 0 && s >= 0 && s <= denomin) || (denomin < 0 && s <= 0 && s >= denomin))
                    {
                        t /= denomin;
                        return new Point(l11.X + t * (l12.X - l11.X), l11.Y + t * (l12.Y - l11.Y));
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static List<Point> IntersectTriangles(Point A1, Point B1, Point C1, Point A2, Point B2, Point C2, double tolerance)
        {
            List<Point> intersections = new List<Point>(6);

            void tryAdd(Point pt)
            {
                for (int i = 0; i < intersections.Count; i++)
                {
                    if (AreEqual(pt, intersections[i], tolerance))
                    {
                        return;
                    }
                }
                intersections.Add(pt);
            }

            void tryAddNull(Point? pt)
            {
                if (pt == null)
                {
                    return;
                }

                for (int i = 0; i < intersections.Count; i++)
                {
                    if (AreEqual(pt.Value, intersections[i], tolerance))
                    {
                        return;
                    }
                }
                intersections.Add(pt.Value);
            }

            bool A1in = PointInTriangle(A1, A2, B2, C2);
            bool B1in = PointInTriangle(B1, A2, B2, C2);
            bool C1in = PointInTriangle(C1, A2, B2, C2);

            bool A2in = PointInTriangle(A2, A1, B1, C1);
            bool B2in = PointInTriangle(B2, A1, B1, C1);
            bool C2in = PointInTriangle(C2, A1, B1, C1);

           
            if (A1in)
            {
                tryAdd(A1);
            }

            if (B1in)
            {
                tryAdd(B1);
            }

            if (C1in)
            {
                tryAdd(C1);
            }

            if (A2in)
            {
                tryAdd(A2);
            }

            if (B2in)
            {
                tryAdd(B2);
            }

            if (C2in)
            {
                tryAdd(C2);
            }


            tryAddNull(IntersectionPoint(A1, B1, A2, B2));
            tryAddNull(IntersectionPoint(A1, B1, B2, C2));
            tryAddNull(IntersectionPoint(A1, B1, C2, A2));

            tryAddNull(IntersectionPoint(B1, C1, A2, B2));
            tryAddNull(IntersectionPoint(B1, C1, B2, C2));
            tryAddNull(IntersectionPoint(B1, C1, C2, A2));

            tryAddNull(IntersectionPoint(C1, A1, A2, B2));
            tryAddNull(IntersectionPoint(C1, A1, B2, C2));
            tryAddNull(IntersectionPoint(C1, A1, C2, A2));

            return intersections;
        }
        
        static Point NaNPoint { get; } = new Point(double.NaN, double.NaN);

        public static Point IntersectFastWithTriangle(Point l11, Point l12, Point l21, Point l22, double tolerance)
        {
            double denomin = (l11.X - l12.X) * (l21.Y - l22.Y) - (l11.Y - l12.Y) * (l21.X - l22.X);

            if (denomin != 0)
            {
                double t, s;
                Point pt;

                t = ((l11.X - l21.X) * (l21.Y - l22.Y) - (l11.Y - l21.Y) * (l21.X - l22.X)) / denomin;
                s = -((l11.X - l12.X) * (l11.Y - l21.Y) - (l11.Y - l12.Y) * (l11.X - l21.X)) / denomin;

                if (s >= -tolerance && s <= 1 + tolerance)
                {
                    t = Math.Min(Math.Max(0, t), 1);

                    pt = new Point(l11.X + t * (l12.X - l11.X), l11.Y + t * (l12.Y - l11.Y));

                    return pt;
                }
                else
                {
                    return NaNPoint;
                }
            }
            else
            {
                return NaNPoint;
            }
        }

        public static (Point intersection, double t, double s)? Intersect(Point l11, Point l12, Point l21, Point l22)
        {
            Point v = new Point(l12.X - l11.X, l12.Y - l11.Y);
            double vLength = v.Modulus();
            v = new Point(v.X / vLength, v.Y / vLength);

            Point l = new Point(l22.X - l21.X, l22.Y - l21.Y);
            double lLength = l.Modulus();

            l = new Point(l.X / lLength, l.Y / lLength);

            if (v.X * l.Y - v.Y * l.X != 0)
            {
                double t2 = (l.X * (l11.Y - l21.Y) - l.Y * (l11.X - l21.X)) / (v.X * l.Y - v.Y * l.X);

                double s2 = -1;

                if (l.X != 0)
                {
                    s2 = (l11.X + t2 * v.X - l21.X) / l.X;
                }
                else if (l.Y != 0)
                {
                    s2 = (l11.Y + t2 * v.Y - l21.Y) / l.Y;
                }

                Point inters = new Point(l11.X + v.X * t2, l11.Y + v.Y * t2);

                return (inters, t2, s2);
            }
            else
            {
                return null;
            }
        }

        public static Point ProjectOnLine(Point p, Point l1, Point l2)
        {
            Point lineDir = new Point(l2.X - l1.X, l2.Y - l1.Y);
            lineDir = new Point(lineDir.X / lineDir.Modulus(), lineDir.Y / lineDir.Modulus());

            Point pDir = new Point(p.X - l1.X, p.Y - l1.Y);

            double dotProd = pDir.X * lineDir.X + pDir.Y * lineDir.Y;

            return new Point(l1.X + dotProd * lineDir.X, l1.Y + dotProd * lineDir.Y);
        }

        public static (double t, Point pt) ProjectOnSegment(int x, int y, Point l1, Point l2)
        {
            Point lineDir = new Point(l2.X - l1.X, l2.Y - l1.Y);
            Point pDir = new Point(x - l1.X, y - l1.Y);
            double lineLengthSq = lineDir.X * lineDir.X + lineDir.Y * lineDir.Y;
            double dot = pDir.X * lineDir.X + pDir.Y * lineDir.Y;

            double t = dot / lineLengthSq;

            return (t, new Point(l1.X + t * lineDir.X, l1.Y + t * lineDir.Y));
        }

        public static (double t, Point pt) ProjectOnSegment(Point p, Point l1, Point l2)
        {
            Point lineDir = new Point(l2.X - l1.X, l2.Y - l1.Y);
            Point pDir = new Point(p.X - l1.X, p.Y - l1.Y);
            double lineLengthSq = lineDir.X * lineDir.X + lineDir.Y * lineDir.Y;
            double dot = pDir.X * lineDir.X + pDir.Y * lineDir.Y;

            double t = dot / lineLengthSq;

            return (t, new Point(l1.X + t * lineDir.X, l1.Y + t * lineDir.Y));
        }

        public static List<Point> Intersect(Point l11, Point l12, Point A, Point B, Point C, double lineThickness, double tolerance)
        {
            double lineLength = new Point(l11.X - l12.X, l11.Y - l12.Y).Modulus();

            double AB = new Point(A.X - B.X, A.Y - B.Y).Modulus();
            double BC = new Point(B.X - C.X, B.Y - C.Y).Modulus();
            double CA = new Point(C.X - A.X, C.Y - A.Y).Modulus();

            (Point inters, double t, double s)? intersAB = Intersections2D.Intersect(l11, l12, A, B);
            (Point inters, double t, double s)? intersBC = Intersections2D.Intersect(l11, l12, B, C);
            (Point inters, double t, double s)? intersCA = Intersections2D.Intersect(l11, l12, C, A);

            bool ABtrue = intersAB != null && intersAB.Value.s >= -tolerance && intersAB.Value.s <= AB + tolerance;
            bool BCtrue = intersBC != null && intersBC.Value.s >= -tolerance && intersBC.Value.s <= BC + tolerance;
            bool CAtrue = intersCA != null && intersCA.Value.s >= -tolerance && intersCA.Value.s <= CA + tolerance;

            List<Point> intersections = new List<Point>(3);

            if (ABtrue)
            {
                Point inters;

                if (intersAB.Value.t >= -lineThickness && intersAB.Value.t <= lineLength + lineThickness)
                {
                    inters = intersAB.Value.inters;
                }
                else if (intersAB.Value.t < -lineThickness)
                {
                    inters = l11;
                }
                else
                {
                    inters = l12;
                }

                bool found = false;

                foreach (Point pt in intersections)
                {
                    if (AreEqual(pt, inters, tolerance))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    intersections.Add(inters);
                }
            }

            if (BCtrue)
            {
                Point inters;

                if (intersBC.Value.t >= -lineThickness && intersBC.Value.t <= lineLength + lineThickness)
                {
                    inters = intersBC.Value.inters;
                }
                else if (intersBC.Value.t < -lineThickness)
                {
                    inters = l11;
                }
                else
                {
                    inters = l12;
                }

                bool found = false;

                foreach (Point pt in intersections)
                {
                    if (AreEqual(pt, inters, tolerance))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    intersections.Add(inters);
                }
            }

            if (CAtrue)
            {
                Point inters;

                if (intersCA.Value.t >= -lineThickness && intersCA.Value.t <= lineLength + lineThickness)
                {
                    inters = intersCA.Value.inters;
                }
                else if (intersCA.Value.t < -lineThickness)
                {
                    inters = l11;
                }
                else
                {
                    inters = l12;
                }

                bool found = false;

                foreach (Point pt in intersections)
                {
                    if (AreEqual(pt, inters, tolerance))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    intersections.Add(inters);
                }
            }



            return intersections;
        }


        public static List<Point> Intersect(Point l11, Point l12, Point A, Point B, Point C, double tolerance)
        {
            Point intersAB = Intersections2D.IntersectFastWithTriangle(l11, l12, A, B, tolerance);
            Point intersBC = Intersections2D.IntersectFastWithTriangle(l11, l12, B, C, tolerance);
            Point intersCA = Intersections2D.IntersectFastWithTriangle(l11, l12, C, A, tolerance);

            List<Point> intersections = new List<Point>(3);

            if (!double.IsNaN(intersAB.X))
            {
                intersections.Add(intersAB);
            }

            if (!double.IsNaN(intersBC.X) && (intersections.Count == 0 || !AreEqual(intersections[0], intersBC, tolerance)))
            {
                intersections.Add(intersBC);
            }

            if (!double.IsNaN(intersCA.X) && (intersections.Count == 0 || (!AreEqual(intersections[0], intersCA, tolerance) && (intersections.Count == 1 || !AreEqual(intersections[1], intersCA, tolerance)))))
            {
                intersections.Add(intersCA);
            }

            return intersections;
        }
    }

    /// <summary>
    /// Represents a point in 3D coordinates.
    /// </summary>
    public readonly struct Point3D : IEquatable<Point3D>
    {
        /// <summary>
        /// The x coordinate of the point.
        /// </summary>
        public readonly double X;

        /// <summary>
        /// The y coordinate of the point.
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// The z coordinate of the point.
        /// </summary>
        public readonly double Z;

        /// <summary>
        /// Creates a new <see cref="Point3D"/>.
        /// </summary>
        /// <param name="x">The x coordinate of the point</param>
        /// <param name="y">The y coordinate of the point</param>
        /// <param name="z">The z coordinate of the point</param>
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Converts the <see cref="Point3D"/> into a <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point3D"/> to convert.</param>
        public static implicit operator Vector3D(Point3D point)
        {
            return new Vector3D(point.X, point.Y, point.Z);
        }

        /// <summary>
        /// Computes the vector difference between two <see cref="Point3D"/>s.
        /// </summary>
        /// <param name="endPoint">The first point.</param>
        /// <param name="startPoint">The second point.</param>
        /// <returns>A <see cref="Vector3D"/> point from <paramref name="startPoint"/> to <paramref name="endPoint"/>.</returns>
        public static Vector3D operator -(Point3D endPoint, Point3D startPoint)
        {
            return new Vector3D(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z);
        }
        
        /// <summary>
        /// Computes the product between a 3x3 <see cref="double"/> matrix and a <see cref="Point3D"/>.
        /// </summary>
        /// <param name="matrix">The 3x3 matrix that should applied to the <paramref name="point"/>.</param>
        /// <param name="point">The point to which the <paramref name="matrix"/> should be applied.</param>
        /// <returns>The product between <paramref name="matrix"/> and <paramref name="point"/>.</returns>
        public static Point3D operator *(double[,] matrix, Point3D point)
        {
            return new Point3D(matrix[0, 0] * point.X + matrix[0, 1] * point.Y + matrix[0, 2] * point.Z,
                matrix[1, 0] * point.X + matrix[1, 1] * point.Y + matrix[1, 2] * point.Z,
                matrix[2, 0] * point.X + matrix[2, 1] * point.Y + matrix[2, 2] * point.Z);
        }

        /// <summary>
        /// Converts the <see cref="Point3D"/> to a <see cref="string"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the <see cref="Point3D"/>.</returns>
        public override string ToString()
        {
            return this.X.ToString() + "; " + this.Y.ToString() + "; " + this.Z.ToString();
        }

        /// <summary>
        /// Indicates whether the <see cref="Point3D"/> is equal to another <see cref="Point3D"/>.
        /// </summary>
        /// <param name="other">A <see cref="Point3D"/> to compare with this <see cref="Point3D"/>.</param>
        /// <returns><see langword="true"/> if the two points are equal; otherwise, <see langword="false" />.</returns>
        public bool Equals(Point3D other)
        {
            return other.X == this.X && other.Y == this.Y && other.Z == this.Z;
        }

        /// <summary>
        /// Indicates whether the <see cref="Point3D"/> is equal to another <see cref="Point3D"/>, up to a specified <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="other">A <see cref="Point3D"/> to compare with this <see cref="Point3D"/>.</param>
        /// <param name="tolerance">The tolerance for the comparison.</param>
        /// <returns><see langword="true"/> if the two points are equal up to the specified <paramref name="tolerance"/>; otherwise, <see langword="false" />.</returns>
        public bool Equals(Point3D other, double tolerance)
        {
            return (Math.Abs(other.X - this.X) <= tolerance || Math.Abs((other.X - this.X) / (other.X + this.X)) <= tolerance * 0.5) && (Math.Abs(other.Y - this.Y) <= tolerance || Math.Abs((other.Y - this.Y) / (other.Y + this.Y)) <= tolerance * 0.5) && (Math.Abs(other.Z - this.Z) <= tolerance || Math.Abs((other.Z - this.Z) / (other.Z + this.Z)) <= tolerance * 0.5);
        }

        /// <summary>
        /// Returns a 2D <see cref="Point"/> obtained by dropping the z coordinate of this point.
        /// </summary>
        /// <returns>A 2D <see cref="Point"/> obtained by dropping the z coordinate of this point.</returns>
        public Point DropZ()
        {
            return new Point(this.X, this.Y);
        }
    }

    /// <summary>
    /// Represents a vector in 3D coordinates.
    /// </summary>
    public readonly struct Vector3D : IEquatable<Vector3D>
    {
        /// <summary>
        /// The x component of the vector.
        /// </summary>
        public readonly double X;

        /// <summary>
        /// The y component of the vector.
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// The z component of the vector.
        /// </summary>
        public readonly double Z;

        /// <summary>
        /// Creates a new <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="x">The x component of the vector.</param>
        /// <param name="y">The y component of the vector.</param>
        /// <param name="z">The z component of the vector.</param>
        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// The modulus of the <see cref="Vector3D"/>.
        /// </summary>
        public double Modulus => Math.Sqrt(X * X + Y * Y + Z * Z);

        /// <summary>
        /// Normalises the vector.
        /// </summary>
        /// <returns>A <see cref="NormalizedVector3D"/> corresponding to the normalisation of this vector.</returns>
        public NormalizedVector3D Normalize()
        {
            return new NormalizedVector3D(X, Y, Z);
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3D"/> to multiply</param>
        /// <param name="times">The scalar to multiply by.</param>
        /// <returns>A <see cref="Vector3D"/> with the same direction as the current vector, but with a modulus multiplied by <paramref name="times"/>.</returns>
        public static Vector3D operator *(Vector3D vector, double times)
        {
            return new Vector3D(vector.X * times, vector.Y * times, vector.Z * times);
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3D"/> to multiply</param>
        /// <param name="times">The scalar to multiply by.</param>
        /// <returns>A <see cref="Vector3D"/> with the same direction as the current vector, but with a modulus multiplied by <paramref name="times"/>.</returns>
        public static Vector3D operator *(double times, Vector3D vector)
        {
            return new Vector3D(vector.X * times, vector.Y * times, vector.Z * times);
        }

        /// <summary>
        /// Sums two <see cref="Vector3D"/>s.
        /// </summary>
        /// <param name="vector1">The first <see cref="Vector3D"/>.</param>
        /// <param name="vector2">The second <see cref="Vector3D"/>.</param>
        /// <returns>A <see cref="Vector3D"/> corresponding to the sum of the two <see cref="Vector3D"/>s.</returns>
        public static Vector3D operator +(Vector3D vector1, Vector3D vector2)
        {
            return new Vector3D(vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z);
        }

        /// <summary>
        /// Computes the dot product between two <see cref="Vector3D"/>s.
        /// </summary>
        /// <param name="vector1">The first <see cref="Vector3D"/>.</param>
        /// <param name="vector2">The second <see cref="Vector3D"/>.</param>
        /// <returns>The dot product between the two <see cref="Vector3D"/>s.</returns>
        public static double operator *(Vector3D vector1, Vector3D vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }

        /// <summary>
        /// Sums a <see cref="Vector3D"/> and a <see cref="Point3D"/>.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3D"/>.</param>
        /// <param name="point">The <see cref="Point3D"/>.</param>
        /// <returns>A <see cref="Point3D"/> corresponding to the sum of the <paramref name="vector"/> and the <paramref name="point"/>.</returns>
        public static Point3D operator +(Point3D point, Vector3D vector)
        {
            return new Point3D(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
        }

        /// <summary>
        /// Sums a <see cref="Vector3D"/> and a <see cref="Point3D"/>.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3D"/>.</param>
        /// <param name="point">The <see cref="Point3D"/>.</param>
        /// <returns>A <see cref="Point3D"/> corresponding to the sum of the <paramref name="vector"/> and the <paramref name="point"/>.</returns>
        public static Point3D operator +(Vector3D vector, Point3D point)
        {
            return new Point3D(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
        }

        /// <summary>
        /// Converts a <see cref="Vector3D"/> into a <see cref="Point3D"/>.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3D"/> to convert.</param>
        public static explicit operator Point3D(Vector3D vector)
        {
            return new Point3D(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Subtracts two <see cref="Vector3D"/>s.
        /// </summary>
        /// <param name="vector1">The first <see cref="Vector3D"/>.</param>
        /// <param name="vector2">The second <see cref="Vector3D"/>.</param>
        /// <returns>A <see cref="Vector3D"/> corresponding to the subtraction of the two <see cref="Vector3D"/>s.</returns>
        public static Vector3D operator -(Vector3D vector1, Vector3D vector2)
        {
            return new Vector3D(vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z);
        }

        /// <summary>
        /// Subtracts a <see cref="Vector3D"/> from a <see cref="Point3D"/>.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3D"/>.</param>
        /// <param name="point">The <see cref="Point3D"/>.</param>
        /// <returns>A <see cref="Point3D"/> corresponding to the subtraction of the <paramref name="vector"/> from the <paramref name="point"/>.</returns>
        public static Point3D operator -(Point3D point, Vector3D vector)
        {
            return new Point3D(point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z);
        }

        /// <summary>
        /// Computes the cross product between two <see cref="Vector3D"/>s. Note that this is anticommutative.
        /// </summary>
        /// <param name="vector1">The first <see cref="Vector3D"/>.</param>
        /// <param name="vector2">The second <see cref="Vector3D"/>.</param>
        /// <returns>The cross product between the two <see cref="Vector3D"/>s (which is a vector perpendicular to both).</returns>
        public static Vector3D operator ^(Vector3D vector1, Vector3D vector2)
        {
            return new Vector3D(vector1.Y * vector2.Z - vector1.Z * vector2.Y, vector1.Z * vector2.X - vector1.X * vector2.Z, vector1.X * vector2.Y - vector1.Y * vector2.X);
        }

        /// <summary>
        /// Indicates whether the <see cref="Vector3D"/> is equal to another <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="other">A <see cref="Vector3D"/> to compare with this <see cref="Vector3D"/>.</param>
        /// <returns><see langword="true"/> if the two vectors are equal; otherwise, <see langword="false" />.</returns>
        public bool Equals(Vector3D other)
        {
            return other.X == this.X && other.Y == this.Y && other.Z == this.Z;
        }

        /// <summary>
        /// Indicates whether the <see cref="Vector3D"/> is equal to another <see cref="Vector3D"/>, up to a specified <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="other">A <see cref="Vector3D"/> to compare with this <see cref="Vector3D"/>.</param>
        /// <param name="tolerance">The tolerance for the comparison.</param>
        /// <returns><see langword="true"/> if the two vectors are equal up to the specified <paramref name="tolerance"/>; otherwise, <see langword="false" />.</returns>
        public bool Equals(Vector3D other, double tolerance)
        {
            return (Math.Abs(other.X - this.X) <= tolerance || Math.Abs((other.X - this.X) / (other.X + this.X)) <= tolerance * 0.5) && (Math.Abs(other.Y - this.Y) <= tolerance || Math.Abs((other.Y - this.Y) / (other.Y + this.Y)) <= tolerance * 0.5) && (Math.Abs(other.Z - this.Z) <= tolerance || Math.Abs((other.Z - this.Z) / (other.Z + this.Z)) <= tolerance * 0.5);
        }

        /// <summary>
        /// Converts the <see cref="Vector3D"/> to a <see cref="string"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the <see cref="Vector3D"/>.</returns>
        public override string ToString()
        {
            return this.X.ToString() + "; " + this.Y.ToString() + "; " + this.Z.ToString();
        }
    }

    /// <summary>
    /// Represents a vector with modulus equal to 1 in 3D coordinates.
    /// </summary>
    public readonly struct NormalizedVector3D : IEquatable<NormalizedVector3D>
    {
        /// <summary>
        /// The x component of the vector.
        /// </summary>
        public readonly double X;

        /// <summary>
        /// The y component of the vector.
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// The z component of the vector.
        /// </summary>
        public readonly double Z;

        /// <summary>
        /// Creates a new <see cref="NormalizedVector3D"/>.
        /// </summary>
        /// <param name="x">The x component of the vector.</param>
        /// <param name="y">The y component of the vector.</param>
        /// <param name="z">The z component of the vector.</param>
        public NormalizedVector3D(double x, double y, double z)
        {
            double modulus = Math.Sqrt(x * x + y * y + z * z);

            X = x / modulus;
            Y = y / modulus;
            Z = z / modulus;
        }

        internal NormalizedVector3D(double x, double y, double z, bool normalize)
        {
            if (!normalize)
            {
                X = x;
                Y = y;
                Z = z;
            }
            else
            {
                double modulus = Math.Sqrt(x * x + y * y + z * z);

                X = x / modulus;
                Y = y / modulus;
                Z = z / modulus;
            }
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3D"/> to multiply</param>
        /// <param name="times">The scalar to multiply by.</param>
        /// <returns>A <see cref="Vector3D"/> with the same direction as the current vector, but with a modulus equal to <paramref name="times"/>.</returns>
        public static Vector3D operator *(NormalizedVector3D vector, double times)
        {
            return new Vector3D(vector.X * times, vector.Y * times, vector.Z * times);
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3D"/> to multiply</param>
        /// <param name="times">The scalar to multiply by.</param>
        /// <returns>A <see cref="Vector3D"/> with the same direction as the current vector, but with a modulus equal to <paramref name="times"/>.</returns>
        public static Vector3D operator *(double times, NormalizedVector3D vector)
        {
            return new Vector3D(vector.X * times, vector.Y * times, vector.Z * times);
        }

        /// <summary>
        /// Converts a <see cref="NormalizedVector3D"/> to a <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="vector">The <see cref="NormalizedVector3D"/> to convert.</param>
        public static implicit operator Vector3D(NormalizedVector3D vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Computes the dot product between two <see cref="NormalizedVector3D"/>s.
        /// </summary>
        /// <param name="vector1">The first <see cref="NormalizedVector3D"/>.</param>
        /// <param name="vector2">The second <see cref="NormalizedVector3D"/>.</param>
        /// <returns>The dot product between the two <see cref="NormalizedVector3D"/>s.</returns>
        public static double operator *(NormalizedVector3D vector1, NormalizedVector3D vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }

        /// <summary>
        /// Computes the cross product between two <see cref="NormalizedVector3D"/>s. Note that this is anticommutative.
        /// </summary>
        /// <param name="vector1">The first <see cref="NormalizedVector3D"/>.</param>
        /// <param name="vector2">The second <see cref="NormalizedVector3D"/>.</param>
        /// <returns>The cross product between the two <see cref="NormalizedVector3D"/>s (which is a vector perpendicular to both).</returns>
        public static Vector3D operator ^(NormalizedVector3D vector1, NormalizedVector3D vector2)
        {
            return new Vector3D(vector1.Y * vector2.Z - vector1.Z * vector2.Y, vector1.Z * vector2.X - vector1.X * vector2.Z, vector1.X * vector2.Y - vector1.Y * vector2.X);
        }

        /// <summary>
        /// Converts the <see cref="NormalizedVector3D"/> to a <see cref="string"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the <see cref="NormalizedVector3D"/>.</returns>
        public override string ToString()
        {
            return this.X.ToString() + "; " + this.Y.ToString() + "; " + this.Z.ToString();
        }

        /// <summary>
        /// Indicates whether the <see cref="NormalizedVector3D"/> is equal to another <see cref="NormalizedVector3D"/>.
        /// </summary>
        /// <param name="other">A <see cref="NormalizedVector3D"/> to compare with this <see cref="NormalizedVector3D"/>.</param>
        /// <returns><see langword="true"/> if the two vectors are equal; otherwise, <see langword="false" />.</returns>
        public bool Equals(NormalizedVector3D other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        /// <summary>
        /// Reverses the <see cref="NormalizedVector3D"/>. Equivalent to multiplying by -1, but avoids renormalisation.
        /// </summary>
        /// <returns>A <see cref="NormalizedVector3D"/> pointing in the opposite direction as this vector.</returns>
        public NormalizedVector3D Reverse()
        {
            return new NormalizedVector3D(-X, -Y, -Z, false);
        }
    }

    /// <summary>
    /// Represents a point in barycentric coordinates.
    /// </summary>
    public struct BarycentricPoint
    {
        /// <summary>
        /// The barycentric coordinate corresponding to the first vertex.
        /// </summary>
        public double U;

        /// <summary>
        /// The barycentric coordinate corresponding to the second vertex.
        /// </summary>
        public double V;

        /// <summary>
        /// The barycentric coordinate corresponding to the third vertex.
        /// </summary>
        public double W;

        /// <summary>
        /// Creates a new <see cref="BarycentricPoint"/> instance.
        /// </summary>
        /// <param name="u">The barycentric coordinate corresponding to the first vertex.</param>
        /// <param name="v">The barycentric coordinate corresponding to the second vertex.</param>
        /// <param name="w">The barycentric coordinate corresponding to the third vertex.</param>
        public BarycentricPoint(double u, double v, double w)
        {
            this.U = u;
            this.V = v;
            this.W = w;
        }
    }

   



}
