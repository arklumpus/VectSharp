using System;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.ThreeD
{
    internal static class Matrix3D
    {
        public static readonly double[,] Identity = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

        public static double[,] RotationToAlignWithZ(NormalizedVector3D vector, bool preferY = true)
        {
            if (vector.Z != -1)
            {
                Vector3D v = new Vector3D(vector.Y, -vector.X, 0);
                double c = 1 / (1 + vector.Z);

                return new double[3, 3]
                {
                    {1 - v.Y * v.Y * c, v.X * v.Y * c, v.Y },
                    {v.X * v.Y * c, 1 - v.X * v.X * c, -v.X },
                    {-v.Y, v.X, 1 - v.X * v.X * c- v.Y * v.Y * c }
                };
            }
            else
            {
                if (!preferY)
                {
                    return new double[3, 3]
                    {
                        { 1, 0, 0 },
                        { 0, -1, 0 },
                        { 0, 0, -1 }
                    };
                }
                else
                {
                    return new double[3, 3]
                    {
                        {-1, 0, 0 },
                        {0, 1, 0 },
                        {0, 0, -1 }
                    };
                }
            }
        }

        public static double[,] RotationToAlignAWithB(NormalizedVector3D a, NormalizedVector3D b)
        {
            double c = a * b;

            if (c != -1)
            {
                Vector3D v = a ^ b;

                c = 1 / (1 + c);

                return new double[3, 3]
                {
                    { 1 - (v.Y * v.Y - v.Z * v.Z) * c, (v.X * v.Y) * c - v.Z, (v.X * v.Z) * c + v.Y},
                    { (v.X * v.Y) * c + v.Z,  1 + (- v.X * v.X - v.Z * v.Z) * c, (v.Y * v.Z) * c - v.X },
                    { (v.X * v.Z) * c - v.Y, (v.Y * v.Z) * c + v.X, 1 + (- v.X * v.X - v.Y * v.Y) * c }
                };
            }
            else
            {
                if (a.X != 0 || a.Z != 0)
                {
                    NormalizedVector3D p = new NormalizedVector3D(-a.Z, 0, a.X);

                    return RotationAroundAxis(p, Math.PI);
                }
                else //a.Y != 0
                {
                    NormalizedVector3D p = new NormalizedVector3D(0, a.Z, -a.Y);

                    return RotationAroundAxis(p, Math.PI);
                }
            }
        }

        public static double[,] RotationAroundY(double theta)
        {
            return new double[3, 3]
            {
                { Math.Cos(theta), 0, Math.Sin(theta) },
                { 0, 1, 0 },
                { -Math.Sin(theta), 0, Math.Cos(theta) }
            };
        }

        public static double[,] RotationAroundAxis(NormalizedVector3D axis, double theta)
        {
            double cos = Math.Cos(theta);
            double sin = Math.Sin(theta);

            return new double[3, 3]
            {
                { cos + axis.X * axis.X * (1 - cos), axis.X * axis.Y * (1 - cos) - axis.Z * sin, axis.X * axis.Z * (1 - cos) + axis.Y * sin },
                { axis.Y * axis.X * (1 - cos) + axis.Z * sin, cos + axis.Y * axis.Y * (1 - cos), axis.Y * axis.Z * (1 - cos) - axis.X * sin },
                { axis.Z * axis.X * (1 - cos) - axis.Y * sin, axis.Z * axis.Y * (1 - cos) + axis.X * sin, cos + axis.Z * axis.Z * (1 - cos) }
            };
        }

        public static double Determinant(this double[,] matrix)
        {
            return matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1]) - matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[2, 0] * matrix[1, 2]) + matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);
        }

        public static double Trace(this double[,] matrix)
        {
            return matrix[0, 0] + matrix[1, 1] + matrix[2, 2];
        }

        public static double[,] Inverse(this double[,] matrix)
        {
            double det = matrix.Determinant();

            if (det != 0)
            {
                double A = matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1];
                double B = -(matrix[1, 0] * matrix[2, 2] - matrix[2, 0] * matrix[1, 2]);
                double C = matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0];

                double D = -(matrix[0, 1] * matrix[2, 2] - matrix[0, 2] * matrix[2, 1]);
                double E = matrix[0, 0] * matrix[2, 2] - matrix[0, 2] * matrix[2, 0];
                double F = -(matrix[0, 0] * matrix[2, 1] - matrix[2, 0] * matrix[0, 1]);

                double G = matrix[0, 1] * matrix[1, 2] - matrix[0, 2] * matrix[1, 1];
                double H = -(matrix[0, 0] * matrix[1, 2] - matrix[1, 0] * matrix[0, 2]);
                double I = matrix[0, 0] * matrix[1, 1] - matrix[1, 0] * matrix[0, 1];


                return new double[3, 3]
                {
                    { A / det, D / det, G / det },
                    { B / det, E / det, H / det },
                    { C / det, F / det, I / det }
                };
            }
            else
            {
                throw new ArgumentException("The matrix is not invertible!");
            }
        }
    }

    /// <summary>
    /// Represents a transformation in 3D coordinates.
    /// </summary>
    public class Transform3D
    {
        /// <summary>
        /// The matrix representation of the transformation.
        /// </summary>
        public double[,] Matrix { get; }

        /// <summary>
        /// Creates a new <see cref="Transform3D"/> from the specified <paramref name="matrix"/>.
        /// </summary>
        /// <param name="matrix"></param>
        public Transform3D(double[,] matrix)
        {
            this.Matrix = matrix;
        }

        /// <summary>
        /// Applies the transformation to the specified <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point3D"/> to which the transformation should be applied.</param>
        /// <returns>A <see cref="Point3D"/> corresponding to the transformed <paramref name="point"/>.</returns>
        public Point3D Apply(Point3D point)
        {
            double x = this.Matrix[0, 0] * point.X + this.Matrix[0, 1] * point.Y + this.Matrix[0, 2] * point.Z + this.Matrix[0, 3];
            double y = this.Matrix[1, 0] * point.X + this.Matrix[1, 1] * point.Y + this.Matrix[1, 2] * point.Z + this.Matrix[1, 3];
            double z = this.Matrix[2, 0] * point.X + this.Matrix[2, 1] * point.Y + this.Matrix[2, 2] * point.Z + this.Matrix[2, 3];
            double t = this.Matrix[3, 0] * point.X + this.Matrix[3, 1] * point.Y + this.Matrix[3, 2] * point.Z + this.Matrix[3, 3];

            x /= t;
            y /= t;
            z /= t;

            return new Point3D(x, y, z);
        }

        private Vector3D Apply(Vector3D vector)
        {
            double x = this.Matrix[0, 0] * vector.X + this.Matrix[0, 1] * vector.Y + this.Matrix[0, 2] * vector.Z + this.Matrix[0, 3];
            double y = this.Matrix[1, 0] * vector.X + this.Matrix[1, 1] * vector.Y + this.Matrix[1, 2] * vector.Z + this.Matrix[1, 3];
            double z = this.Matrix[2, 0] * vector.X + this.Matrix[2, 1] * vector.Y + this.Matrix[2, 2] * vector.Z + this.Matrix[2, 3];
            double t = this.Matrix[3, 0] * vector.X + this.Matrix[3, 1] * vector.Y + this.Matrix[3, 2] * vector.Z + this.Matrix[3, 3];

            x /= t;
            y /= t;
            z /= t;

            return new Vector3D(x, y, z);
        }

        /// <summary>
        /// Combines two <see cref="Transform3D"/>s, by multiplying the corresponding matrices.
        /// </summary>
        /// <param name="other">The other <see cref="Transform3D"/> to combine with this instance.</param>
        /// <returns>A <see cref="Transform3D"/> corresponding to applying first the <paramref name="other" /> transformation, and then this transformation.</returns>
        public Transform3D Combine(Transform3D other)
        {
            double m00 = this.Matrix[0, 0] * other.Matrix[0, 0] + this.Matrix[0, 1] * other.Matrix[1, 0] + this.Matrix[0, 2] * other.Matrix[2, 0] + this.Matrix[0, 3] * other.Matrix[3, 0];
            double m01 = this.Matrix[0, 0] * other.Matrix[0, 1] + this.Matrix[0, 1] * other.Matrix[1, 1] + this.Matrix[0, 2] * other.Matrix[2, 1] + this.Matrix[0, 3] * other.Matrix[3, 1];
            double m02 = this.Matrix[0, 0] * other.Matrix[0, 2] + this.Matrix[0, 1] * other.Matrix[1, 2] + this.Matrix[0, 2] * other.Matrix[2, 2] + this.Matrix[0, 3] * other.Matrix[3, 2];
            double m03 = this.Matrix[0, 0] * other.Matrix[0, 3] + this.Matrix[0, 1] * other.Matrix[1, 3] + this.Matrix[0, 2] * other.Matrix[2, 3] + this.Matrix[0, 3] * other.Matrix[3, 3];

            double m10 = this.Matrix[1, 0] * other.Matrix[0, 0] + this.Matrix[1, 1] * other.Matrix[1, 0] + this.Matrix[1, 2] * other.Matrix[2, 0] + this.Matrix[1, 3] * other.Matrix[3, 0];
            double m11 = this.Matrix[1, 0] * other.Matrix[0, 1] + this.Matrix[1, 1] * other.Matrix[1, 1] + this.Matrix[1, 2] * other.Matrix[2, 1] + this.Matrix[1, 3] * other.Matrix[3, 1];
            double m12 = this.Matrix[1, 0] * other.Matrix[0, 2] + this.Matrix[1, 1] * other.Matrix[1, 2] + this.Matrix[1, 2] * other.Matrix[2, 2] + this.Matrix[1, 3] * other.Matrix[3, 2];
            double m13 = this.Matrix[1, 0] * other.Matrix[0, 3] + this.Matrix[1, 1] * other.Matrix[1, 3] + this.Matrix[1, 2] * other.Matrix[2, 3] + this.Matrix[1, 3] * other.Matrix[3, 3];

            double m20 = this.Matrix[2, 0] * other.Matrix[0, 0] + this.Matrix[2, 1] * other.Matrix[1, 0] + this.Matrix[2, 2] * other.Matrix[2, 0] + this.Matrix[2, 3] * other.Matrix[3, 0];
            double m21 = this.Matrix[2, 0] * other.Matrix[0, 1] + this.Matrix[2, 1] * other.Matrix[1, 1] + this.Matrix[2, 2] * other.Matrix[2, 1] + this.Matrix[2, 3] * other.Matrix[3, 1];
            double m22 = this.Matrix[2, 0] * other.Matrix[0, 2] + this.Matrix[2, 1] * other.Matrix[1, 2] + this.Matrix[2, 2] * other.Matrix[2, 2] + this.Matrix[2, 3] * other.Matrix[3, 2];
            double m23 = this.Matrix[2, 0] * other.Matrix[0, 3] + this.Matrix[2, 1] * other.Matrix[1, 3] + this.Matrix[2, 2] * other.Matrix[2, 3] + this.Matrix[2, 3] * other.Matrix[3, 3];

            double m30 = this.Matrix[3, 0] * other.Matrix[0, 0] + this.Matrix[3, 1] * other.Matrix[1, 0] + this.Matrix[3, 2] * other.Matrix[2, 0] + this.Matrix[3, 3] * other.Matrix[3, 0];
            double m31 = this.Matrix[3, 0] * other.Matrix[0, 1] + this.Matrix[3, 1] * other.Matrix[1, 1] + this.Matrix[3, 2] * other.Matrix[2, 1] + this.Matrix[3, 3] * other.Matrix[3, 1];
            double m32 = this.Matrix[3, 0] * other.Matrix[0, 2] + this.Matrix[3, 1] * other.Matrix[1, 2] + this.Matrix[3, 2] * other.Matrix[2, 2] + this.Matrix[3, 3] * other.Matrix[3, 2];
            double m33 = this.Matrix[3, 0] * other.Matrix[0, 3] + this.Matrix[3, 1] * other.Matrix[1, 3] + this.Matrix[3, 2] * other.Matrix[2, 3] + this.Matrix[3, 3] * other.Matrix[3, 3];

            return new Transform3D(new double[,] { { m00, m01, m02, m03 }, { m10, m11, m12, m13 }, { m20, m21, m22, m23 }, { m30, m31, m32, m33 } });
        }

        /// <summary>
        /// Computes the inverse transformation.
        /// </summary>
        /// <returns>A <see cref="Transform3D"/> that, when combined with the current transformation, results in the identity transformation.</returns>
        public Transform3D Inverse()
        {
            double a = this.Matrix[0, 0];
            double b = this.Matrix[0, 1];
            double c = this.Matrix[0, 2];
            double d = this.Matrix[0, 3];

            double e = this.Matrix[1, 0];
            double f = this.Matrix[1, 1];
            double g = this.Matrix[1, 2];
            double h = this.Matrix[1, 3];

            double i = this.Matrix[2, 0];
            double j = this.Matrix[2, 1];
            double k = this.Matrix[2, 2];
            double l = this.Matrix[2, 3];

            double m = this.Matrix[3, 0];
            double n = this.Matrix[3, 1];
            double o = this.Matrix[3, 2];
            double p = this.Matrix[3, 3];

            double[,] matrix = new double[4, 4];


            matrix[0, 0] = (f * k * p - f * l * o - g * j * p + g * l * n + h * j * o - h * k * n) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[0, 1] = -(b * k * p - b * l * o - c * j * p + c * l * n + d * j * o - d * k * n) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[0, 2] = (b * g * p - b * h * o - c * f * p + c * h * n + d * f * o - d * g * n) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[0, 3] = -(b * g * l - b * h * k - c * f * l + c * h * j + d * f * k - d * g * j) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[1, 0] = -(e * k * p - e * l * o - g * i * p + g * l * m + h * i * o - h * k * m) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[1, 1] = (a * k * p - a * l * o - c * i * p + c * l * m + d * i * o - d * k * m) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[1, 2] = -(a * g * p - a * h * o - c * e * p + c * h * m + d * e * o - d * g * m) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[1, 3] = (a * g * l - a * h * k - c * e * l + c * h * i + d * e * k - d * g * i) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[2, 0] = (e * j * p - e * l * n - f * i * p + f * l * m + h * i * n - h * j * m) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[2, 1] = -(a * j * p - a * l * n - b * i * p + b * l * m + d * i * n - d * j * m) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[2, 2] = (a * f * p - a * h * n - b * e * p + b * h * m + d * e * n - d * f * m) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[2, 3] = -(a * f * l - a * h * j - b * e * l + b * h * i + d * e * j - d * f * i) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[3, 0] = -(e * j * o - e * k * n - f * i * o + f * k * m + g * i * n - g * j * m) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[3, 1] = (a * j * o - a * k * n - b * i * o + b * k * m + c * i * n - c * j * m) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[3, 2] = -(a * f * o - a * g * n - b * e * o + b * g * m + c * e * n - c * f * m) / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m);
            matrix[3, 3] = 0.1e1 / (a * f * k * p - a * f * l * o - a * g * j * p + a * g * l * n + a * h * j * o - a * h * k * n - b * e * k * p + b * e * l * o + b * g * i * p - b * g * l * m - b * h * i * o + b * h * k * m + c * e * j * p - c * e * l * n - c * f * i * p + c * f * l * m + c * h * i * n - c * h * j * m - d * e * j * o + d * e * k * n + d * f * i * o - d * f * k * m - d * g * i * n + d * g * j * m) * (a * f * k - a * g * j - b * e * k + b * g * i + c * e * j - c * f * i);

            return new Transform3D(matrix);
        }
        
        /// <summary>
        /// A transformation that leaves points unchanged.
        /// </summary>
        public static Transform3D Identity { get; } = new Transform3D(new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });

        /// <summary>
        /// Creates a new translation transformation.
        /// </summary>
        /// <param name="x">The amount to translate with respect to the x axis.</param>
        /// <param name="y">The amount to translate with respect to the y axis.</param>
        /// <param name="z">The amount to translate with respect to the z axis.</param>
        /// <returns>A <see cref="Transform3D"/> corresponding to the specified translation.</returns>
        public static Transform3D Translate(double x, double y, double z)
        {
            return new Transform3D(new double[,] { { 1, 0, 0, x }, { 0, 1, 0, y }, { 0, 0, 1, z }, { 0, 0, 0, 1 } });
        }

        /// <summary>
        /// Creates a new scaling transformation.
        /// </summary>
        /// <param name="x">The amount to scale with respect to the x axis.</param>
        /// <param name="y">The amount to scale with respect to the y axis.</param>
        /// <param name="z">The amount to scale with respect to the z axis.</param>
        /// <returns>A <see cref="Transform3D"/> corresponding to the specified scaling.</returns>
        public static Transform3D Scale(double x, double y, double z)
        {
            return new Transform3D(new double[,] { { x, 0, 0, 0 }, { 0, y, 0, 0 }, { 0, 0, z, 0 }, { 0, 0, 0, 1 } });
        }

        /// <summary>
        /// Creates a new rotation transformation that aligns <paramref name="a"/> with <paramref name="b"/>.
        /// </summary>
        /// <param name="a">The vector that should be aligned with <paramref name="b"/>.</param>
        /// <param name="b">The reference vector.</param>
        /// <returns>A <see cref="Transform3D"/> such that applying this transform to <paramref name="a"/> returns <paramref name="b"/> and the determinant of the transformation is 1.</returns>
        public static Transform3D RotationToAlignAWithB(NormalizedVector3D a, NormalizedVector3D b)
        {
            double[,] matrix3x3 = Matrix3D.RotationToAlignAWithB(a, b);
            return new Transform3D(new double[,] { { matrix3x3[0, 0], matrix3x3[0, 1], matrix3x3[0, 2], 0 }, { matrix3x3[1, 0], matrix3x3[1, 1], matrix3x3[1, 2], 0 }, { matrix3x3[2, 0], matrix3x3[2, 1], matrix3x3[2, 2], 0 }, { 0, 0, 0, 1 } });
        }

        /// <summary>
        /// Creates a new transformation corresponding to a rotation around a specified axis.
        /// </summary>
        /// <param name="axis">The axis around which to rotate.</param>
        /// <param name="theta">The rotation angle in radians.</param>
        /// <returns>A <see cref="Transform3D"/> corresponding to the specified rotation.</returns>
        public static Transform3D RotationAlongAxis(NormalizedVector3D axis, double theta)
        {
            double[,] matrix3x3 = Matrix3D.RotationAroundAxis(axis, theta);
            return new Transform3D(new double[,] { { matrix3x3[0, 0], matrix3x3[0, 1], matrix3x3[0, 2], 0 }, { matrix3x3[1, 0], matrix3x3[1, 1], matrix3x3[1, 2], 0 }, { matrix3x3[2, 0], matrix3x3[2, 1], matrix3x3[2, 2], 0 }, { 0, 0, 0, 1 } });
        }

        /// <summary>
        /// Applies the transformation to a <see cref="Triangle3DElement"/>.
        /// </summary>
        /// <param name="triangle">The <see cref="Triangle3DElement"/> to which the transformation should be applied.</param>
        /// <returns>A <see cref="Triangle3DElement"/> corresponding to a triangle in which the transformation has been applied to the points from <paramref name="triangle" />. Properties are preserved between the two elements.</returns>
        public Triangle3DElement Apply(Triangle3DElement triangle)
        {
            Point3D p1 = this.Apply(triangle.Point1);
            Point3D p2 = this.Apply(triangle.Point2);
            Point3D p3 = this.Apply(triangle.Point3);

            if (!triangle.IsFlat)
            {
                Point3D p1Ref = triangle.Point1 + (Vector3D)triangle.Point1Normal;
                Point3D p2Ref = triangle.Point2 + (Vector3D)triangle.Point2Normal;
                Point3D p3Ref = triangle.Point3 + (Vector3D)triangle.Point3Normal;

                p1Ref = this.Apply(p1Ref);
                p2Ref = this.Apply(p2Ref);
                p3Ref = this.Apply(p3Ref);

                NormalizedVector3D n1 = (p1Ref - p1).Normalize();
                NormalizedVector3D n2 = (p2Ref - p2).Normalize();
                NormalizedVector3D n3 = (p3Ref - p3).Normalize();

                Triangle3DElement tbr = new Triangle3DElement(p1, p2, p3, n1, n2, n3) { CastsShadow = triangle.CastsShadow, ReceivesShadow = triangle.ReceivesShadow, Tag = triangle.Tag, ZIndex = triangle.ZIndex };

                tbr.Fill.AddRange(triangle.Fill);

                return tbr;
            }
            else
            {
                Triangle3DElement tbr = new Triangle3DElement(p1, p2, p3) { CastsShadow = triangle.CastsShadow, ReceivesShadow = triangle.ReceivesShadow, Tag = triangle.Tag, ZIndex = triangle.ZIndex };

                tbr.Fill.AddRange(triangle.Fill);

                return tbr;
            }
        }

        /// <summary>
        /// Applies the transformation to a <see cref="Line3DElement"/>.
        /// </summary>
        /// <param name="line">The <see cref="Line3DElement"/> to which the transformation should be applied.</param>
        /// <returns>A <see cref="Line3DElement"/> corresponding to a line in which the transformation has been applied to the points from <paramref name="line" />. Properties are preserved between the two elements.</returns>
        public Line3DElement Apply(Line3DElement line)
        {
            Point3D p1 = this.Apply(line.Point1);
            Point3D p2 = this.Apply(line.Point2);

            Line3DElement tbr = new Line3DElement(p1, p2) { Colour = line.Colour, LineCap = line.LineCap, LineDash = line.LineDash, Thickness = line.Thickness, Tag = line.Tag, ZIndex = line.ZIndex };

            return tbr;
        }

        /// <summary>
        /// Applies the transformation to a <see cref="Point3DElement"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point3DElement"/> to which the transformation should be applied.</param>
        /// <returns>A <see cref="Point3DElement"/> with coordinates corresponding to the transformation of <paramref name="point"/>. Properties are preserved between the two elements.</returns>
        public Point3DElement Apply(Point3DElement point)
        {
            Point3D p1 = this.Apply(point.Point);

            Point3DElement tbr = new Point3DElement(p1) { Colour = point.Colour, Tag = point.Tag, ZIndex = point.ZIndex, Diameter = point.Diameter };

            return tbr;
        }

        /// <summary>
        /// Applies the transformation to a collection of <see cref="Element3D"/>s.
        /// </summary>
        /// <param name="items">The <see cref="Element3D"/>s to which the transformation should be applied.</param>
        /// <returns>A lazy collection of <see cref="Element3D"/>s to which the transformation has been applied. You may have to store this e.g. in a <see cref="List{T}"/> depending on your needs.</returns>
        public IEnumerable<Element3D> Apply(IEnumerable<Element3D> items)
        {
            foreach (Element3D element in items)
            {
                if (element is Point3DElement point)
                {
                    yield return this.Apply(point);
                }
                else if (element is Line3DElement line)
                {
                    yield return this.Apply(line);
                }
                else if (element is Triangle3DElement triangle)
                {
                    yield return this.Apply(triangle);
                }
            }
        }

        /// <summary>
        /// Applies a transformation to the specified <paramref name="point"/>.
        /// </summary>
        /// <param name="transform">The transformation to apply.</param>
        /// <param name="point">The <see cref="Point3D"/> to which the transformation should be applied.</param>
        /// <returns>A <see cref="Point3D"/> corresponding to the transformed <paramref name="point"/>.</returns>
        public static Point3D operator *(Transform3D transform, Point3D point)
        {
            return transform.Apply(point);
        }

        /// <summary>
        /// Applies a transformation to the specified <paramref name="vector"/>. You most likely do not want to use this operator!
        /// </summary>
        /// <param name="transform">The transformation to apply.</param>
        /// <param name="vector">The <see cref="Point3D"/> to which the transformation should be applied.</param>
        /// <returns>A <see cref="Point3D"/> corresponding to the transformed <paramref name="vector"/>.</returns>
        public static Vector3D operator *(Transform3D transform, Vector3D vector)
        {
            return transform.Apply(vector);
        }

        /// <summary>
        /// Combines two <see cref="Transform3D"/>s, by multiplying the corresponding matrices.
        /// </summary>
        /// <param name="transform1">The first <see cref="Transform3D"/> to combine.</param>
        /// <param name="transform2">The second <see cref="Transform3D"/> to combine.</param>
        /// <returns>A <see cref="Transform3D"/> corresponding to applying first <paramref name="transform2" /> and then <paramref name="transform1"/>.</returns>
        public static Transform3D operator *(Transform3D transform1, Transform3D transform2)
        {
            return transform1.Combine(transform2);
        }

        /// <summary>
        /// Applies a transformation to a <see cref="Point3DElement"/>.
        /// </summary>
        /// <param name="transform">The transformation to apply.</param>
        /// <param name="point">The <see cref="Point3DElement"/> to which the transformation should be applied.</param>
        /// <returns>A <see cref="Point3DElement"/> with coordinates corresponding to the transformation of <paramref name="point"/>. Properties are preserved between the two elements.</returns>
        public static Point3DElement operator *(Transform3D transform, Point3DElement point)
        {
            return transform.Apply(point);
        }

        /// <summary>
        /// Applies a transformation to a <see cref="Line3DElement"/>.
        /// </summary>
        /// <param name="transform">The transformation to apply.</param>
        /// <param name="line">The <see cref="Line3DElement"/> to which the transformation should be applied.</param>
        /// <returns>A <see cref="Line3DElement"/> corresponding to a line in which the transformation has been applied to the points from <paramref name="line" />. Properties are preserved between the two elements.</returns>
        public static Line3DElement operator *(Transform3D transform, Line3DElement line)
        {
            return transform.Apply(line);
        }

        /// <summary>
        /// Applies a transformation to a <see cref="Triangle3DElement"/>.
        /// </summary>
        /// <param name="transform">The transformation to apply.</param>
        /// <param name="triangle">The <see cref="Triangle3DElement"/> to which the transformation should be applied.</param>
        /// <returns>A <see cref="Triangle3DElement"/> corresponding to a triangle in which the transformation has been applied to the points from <paramref name="triangle" />. Properties are preserved between the two elements.</returns>
        public static Triangle3DElement operator *(Transform3D transform, Triangle3DElement triangle)
        {
            return transform.Apply(triangle);
        }

        /// <summary>
        /// Applies a transformation to a collection of <see cref="Element3D"/>s.
        /// </summary>
        /// <param name="transform">The transformation to apply.</param>
        /// <param name="elements">The <see cref="Element3D"/>s to which the transformation should be applied.</param>
        /// <returns>A lazy collection of <see cref="Element3D"/>s to which the transformation has been applied. You may have to store this e.g. in a <see cref="List{T}"/> depending on your needs.</returns>
        public static IEnumerable<Element3D> operator *(Transform3D transform, IEnumerable<Element3D> elements)
        {
            return transform.Apply(elements);
        }
    }
}
