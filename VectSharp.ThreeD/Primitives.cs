using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectSharp.ThreeD
{
    internal static class Intersections2D
    {
        public static bool AreEqual(Point p1, Point p2, double tolerance)
        {
            return Math.Abs(p1.X - p2.X) <= tolerance && Math.Abs(p1.Y - p2.Y) <= tolerance;
        }

        /* public static (Point intersection, double t, double s)? IntersectFast(Point l11, Point l12, Point l21, Point l22)
         {
             double denomin = (l11.X - l12.X) * (l21.Y - l22.Y) - (l11.Y - l12.Y) * (l21.X - l22.X);

             double t, s;
             Point pt;

             if (denomin != 0)
             {
                 t = ((l11.X - l21.X) * (l21.Y - l22.Y) - (l11.Y - l21.Y) * (l21.X - l22.X)) / denomin;
                 s = -((l11.X - l12.X) * (l11.Y - l21.Y) - (l11.Y - l12.Y) * (l11.X - l21.X)) / denomin;

                 pt = new Point(l11.X + t * (l12.X - l11.X), l11.Y + t * (l12.Y - l11.Y));

                 return (pt, t, s);
             }
             else
             {
                 return null;
             }
         }
        */

        public static bool PointInTriangle(Point pt, Point A, Point B, Point C)
        {
            double signAB = (pt.X - B.X) * (A.Y - B.Y) - (A.X - B.X) * (pt.Y - B.Y);
            double signBC = (pt.X - C.X) * (B.Y - C.Y) - (B.X - C.X) * (pt.Y - C.Y);
            double signCA = (pt.X - A.X) * (C.Y - A.Y) - (C.X - A.X) * (pt.Y - A.Y);

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

        public static Point? IntersectLineWithTriangle(Point l1, Point l2, Point A, Point B, Point C)
        {
            Point? pt = IntersectionPoint(l1, l2, A, B);

            if (pt != null)
            {
                return pt;
            }
            else
            {
                pt = IntersectionPoint(l1, l2, B, C);
                if (pt != null)
                {
                    return pt;
                }
                else
                {
                    return IntersectionPoint(l1, l2, C, A);
                }
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

            /*if (A1in && !B1in && !C1in)
            {
                tryAdd(A1);
                tryAddNull(IntersectLineWithTriangle(A1, B1, A2, B2, C2));
                tryAddNull(IntersectLineWithTriangle(A1, C1, A2, B2, C2));
            }
            else if (!A1in && B1in && !C1in)
            {
                tryAdd(B1);
                tryAddNull(IntersectLineWithTriangle(A1, B1, A2, B2, C2));
                tryAddNull(IntersectLineWithTriangle(B1, C1, A2, B2, C2));
            }
            else if (!A1in && !B1in && C1in)
            {
                tryAdd(C1);
                tryAddNull(IntersectLineWithTriangle(A1, C1, A2, B2, C2));
                tryAddNull(IntersectLineWithTriangle(B1, C1, A2, B2, C2));
            }
            else if (A1in && B1in && !C1in)
            {
                tryAdd(A1);
                tryAdd(B1);
                tryAddNull(IntersectLineWithTriangle(A1, C1, A2, B2, C2));
                tryAddNull(IntersectLineWithTriangle(B1, C1, A2, B2, C2));
            }
            else if (A1in && !B1in && C1in)
            {
                tryAdd(A1);
                tryAdd(C1);
                tryAddNull(IntersectLineWithTriangle(A1, B1, A2, B2, C2));
                tryAddNull(IntersectLineWithTriangle(B1, C1, A2, B2, C2));
            }
            else if (!A1in && B1in && C1in)
            {
                tryAdd(B1);
                tryAdd(C1);
                tryAddNull(IntersectLineWithTriangle(A1, B1, A2, B2, C2));
                tryAddNull(IntersectLineWithTriangle(C1, A1, A2, B2, C2));
            }
            else if (A1in && B1in & C1in)
            {
                tryAdd(A1);
                tryAdd(B1);
                tryAdd(C1);
                return intersections;
            }*/

            bool A2in = PointInTriangle(A2, A1, B1, C1);
            bool B2in = PointInTriangle(B2, A1, B1, C1);
            bool C2in = PointInTriangle(C2, A1, B1, C1);

            /*if (A2in && !B2in && !C2in)
            {
                tryAdd(A2);
                tryAddNull(IntersectLineWithTriangle(A2, B2, A1, B1, C1));
                tryAddNull(IntersectLineWithTriangle(A2, C2, A1, B1, C1));
            }
            else if (!A2in && B2in && !C2in)
            {
                tryAdd(B2);
                tryAddNull(IntersectLineWithTriangle(A2, B2, A1, B1, C1));
                tryAddNull(IntersectLineWithTriangle(B2, C2, A1, B1, C1));
            }
            else if (!A2in && !B2in && C2in)
            {
                tryAdd(C2);
                tryAddNull(IntersectLineWithTriangle(A2, C2, A1, B1, C1));
                tryAddNull(IntersectLineWithTriangle(B2, C2, A1, B1, C1));
            }
            else if (A2in && B2in && !C2in)
            {
                tryAdd(A2);
                tryAdd(B2);
                tryAddNull(IntersectLineWithTriangle(A2, C2, A1, B1, C1));
                tryAddNull(IntersectLineWithTriangle(B2, C2, A1, B1, C1));
            }
            else if (A2in && !B2in && C2in)
            {
                tryAdd(A2);
                tryAdd(C2);
                tryAddNull(IntersectLineWithTriangle(A2, B2, A1, B1, C1));
                tryAddNull(IntersectLineWithTriangle(B2, C2, A1, B1, C1));
            }
            else if (!A2in && B2in && C2in)
            {
                tryAdd(B2);
                tryAdd(C2);
                tryAddNull(IntersectLineWithTriangle(A2, B2, A1, B1, C1));
                tryAddNull(IntersectLineWithTriangle(C2, A2, A1, B1, C1));
            }
            else if (A2in && B2in & C2in)
            {
                tryAdd(A2);
                tryAdd(B2);
                tryAdd(C2);
                return intersections;
            }

            if (!A1in && !B1in && !C1in && !A2in && !B2in && !C2in)
            {*/
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
            //}

            return intersections;
        }


        static Point NaNPoint = new Point(double.NaN, double.NaN);

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

            /*bool ABtrue = intersAB != null && intersAB.Value.s >= -tolerance && intersAB.Value.s <= 1 + tolerance;
            bool BCtrue = intersBC != null && intersBC.Value.s >= -tolerance && intersBC.Value.s <= 1 + tolerance;
            bool CAtrue = intersCA != null && intersCA.Value.s >= -tolerance && intersCA.Value.s <= 1 + tolerance;
            
            List<Point> intersections = new List<Point>(3);

            if (ABtrue)
            {
                Point inters;

                if (intersAB.Value.t >= -tolerance && intersAB.Value.t <= 1 + tolerance)
                {
                    inters = intersAB.Value.inters;
                }
                else if (intersAB.Value.t < -tolerance)
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

                if (intersBC.Value.t >= -tolerance && intersBC.Value.t <= 1 + tolerance)
                {
                    inters = intersBC.Value.inters;
                }
                else if (intersBC.Value.t < -tolerance)
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

                if (intersCA.Value.t >= -tolerance && intersCA.Value.t <= 1 + tolerance)
                {
                    inters = intersCA.Value.inters;
                }
                else if (intersCA.Value.t < -tolerance)
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
            }*/

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

    public static class Matrix3D
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
                Vector3D v = new Vector3D(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

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

    public readonly struct Point3D : IEquatable<Point3D>
    {
        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        public readonly bool Is2D;

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;

            Is2D = double.IsNaN(z);
        }

        public static implicit operator Vector3D(Point3D point)
        {
            if (!point.Is2D)
            {
                return new Vector3D(point.X, point.Y, point.Z);
            }
            else
            {
                throw new NotImplementedException("The point is a 2D point!");
            }
        }

        public static Vector3D operator -(Point3D endPoint, Point3D startPoint)
        {
            return new Vector3D(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z);
        }

        public static Point3D operator *(double[,] matrix, Point3D point)
        {
            return new Point3D(matrix[0, 0] * point.X + matrix[0, 1] * point.Y + matrix[0, 2] * point.Z,
                matrix[1, 0] * point.X + matrix[1, 1] * point.Y + matrix[1, 2] * point.Z,
                matrix[2, 0] * point.X + matrix[2, 1] * point.Y + matrix[2, 2] * point.Z);
        }

        public override string ToString()
        {
            return this.X.ToString() + "; " + this.Y.ToString() + "; " + this.Z.ToString();
        }

        public bool Equals(Point3D other)
        {
            return other.X == this.X && other.Y == this.Y && other.Z == this.Z;
        }

        public bool Equals(Point3D other, double tolerance)
        {
            return (Math.Abs(other.X - this.X) <= tolerance || Math.Abs((other.X - this.X) / (other.X + this.X)) <= tolerance * 0.5) && (Math.Abs(other.Y - this.Y) <= tolerance || Math.Abs((other.Y - this.Y) / (other.Y + this.Y)) <= tolerance * 0.5) && (Math.Abs(other.Z - this.Z) <= tolerance || Math.Abs((other.Z - this.Z) / (other.Z + this.Z)) <= tolerance * 0.5);
        }
    }

    public readonly struct Vector3D
    {
        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Modulus => Math.Sqrt(X * X + Y * Y + Z * Z);

        public NormalizedVector3D Normalize()
        {
            return new NormalizedVector3D(X, Y, Z);
        }

        public static Vector3D operator *(Vector3D vector, double times)
        {
            return new Vector3D(vector.X * times, vector.Y * times, vector.Z * times);
        }

        public static Vector3D operator +(Vector3D vector1, Vector3D vector2)
        {
            return new Vector3D(vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z);
        }

        public static double operator *(Vector3D vector1, Vector3D vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }

        public static Point3D operator +(Point3D point, Vector3D vector)
        {
            return new Point3D(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
        }

        public static explicit operator Point3D(Vector3D vector)
        {
            return new Point3D(vector.X, vector.Y, vector.Z);
        }

        public static Vector3D operator -(Vector3D vector1, Vector3D vector2)
        {
            return new Vector3D(vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z);
        }

        public static Vector3D operator ^(Vector3D vector1, Vector3D vector2)
        {
            return new Vector3D(vector1.Y * vector2.Z - vector1.Z * vector2.Y, vector1.Z * vector2.X - vector1.X * vector2.Z, vector1.X * vector2.Y - vector1.Y * vector2.X);
        }

        public override string ToString()
        {
            return this.X.ToString() + "; " + this.Y.ToString() + "; " + this.Z.ToString();
        }
    }

    public readonly struct NormalizedVector3D
    {
        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        public NormalizedVector3D(double x, double y, double z)
        {
            double modulus = Math.Sqrt(x * x + y * y + z * z);

            X = x / modulus;
            Y = y / modulus;
            Z = z / modulus;
        }

        public static Vector3D operator *(NormalizedVector3D vector, double times)
        {
            return new Vector3D(vector.X * times, vector.Y * times, vector.Z * times);
        }

        public static Vector3D operator *(double times, NormalizedVector3D vector)
        {
            return new Vector3D(vector.X * times, vector.Y * times, vector.Z * times);
        }

        public static implicit operator Vector3D(NormalizedVector3D vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }

        public static double operator *(NormalizedVector3D vector1, NormalizedVector3D vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }

        public static Vector3D operator ^(NormalizedVector3D vector1, NormalizedVector3D vector2)
        {
            return new Vector3D(vector1.Y * vector2.Z - vector1.Z * vector2.Y, vector1.Z * vector2.X - vector1.X * vector2.Z, vector1.X * vector2.Y - vector1.Y * vector2.X);
        }

        public override string ToString()
        {
            return this.X.ToString() + "; " + this.Y.ToString() + "; " + this.Z.ToString();
        }
    }

    public interface IElement3D : IReadOnlyList<Point3D>
    {
        string Tag { get; set; }
        int ZIndex { get; set; }
        void SetProjection(ICamera camera);
        Point[] GetProjection();
    }

    public class Triangle : IElement3D
    {
        public Point3D Point1 { get; }
        public Point3D Point2 { get; }
        public Point3D Point3 { get; }

        public List<IMaterial> Fill { get; }

        public string Tag { get; set; }

        public Point3D Centroid { get; }

        public NormalizedVector3D Normal { get; }
        public NormalizedVector3D ActualNormal { get; }

        public NormalizedVector3D Point1Normal { get; }
        public NormalizedVector3D Point2Normal { get; }
        public NormalizedVector3D Point3Normal { get; }

        public int ZIndex { get; set; } = 0;

        public Triangle(Point3D point1, Point3D point2, Point3D point3)
        {
            this.Point1 = point1;
            this.Point2 = point2;
            this.Point3 = point3;
            this.Fill = new List<IMaterial>();

            this.Normal = ((this.Point2 - this.Point1) ^ (this.Point3 - this.Point1)).Normalize();
            this.ActualNormal = this.Normal;
            this.Centroid = (Point3D)(((Vector3D)point1 + point2 + point3) * (1.0 / 3));

            this.Point1Normal = this.Normal;
            this.Point2Normal = this.Normal;
            this.Point3Normal = this.Normal;
        }

        public Triangle(Point3D point1, Point3D point2, Point3D point3, NormalizedVector3D point1Normal, NormalizedVector3D point2Normal, NormalizedVector3D point3Normal)
        {
            this.Point1 = point1;
            this.Point2 = point2;
            this.Point3 = point3;
            this.Fill = new List<IMaterial>();

            this.ActualNormal = ((this.Point2 - this.Point1) ^ (this.Point3 - this.Point1)).Normalize();
            this.Normal = ((Vector3D)point1Normal + point2Normal + point3Normal).Normalize();
            this.Centroid = (Point3D)(((Vector3D)point1 + point2 + point3) * (1.0 / 3));

            this.Point1Normal = point1Normal;
            this.Point2Normal = point2Normal;
            this.Point3Normal = point3Normal;
        }

        private Point[] Projection;

        public void SetProjection(ICamera camera)
        {
            this.Projection = new Point[]
            {
                camera.Project(this.Point1),
                camera.Project(this.Point2),
                camera.Project(this.Point3)
            };
        }

        public Point[] GetProjection()
        {
            return this.Projection;
        }

        public Point3D this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Point1;
                    case 1:
                        return Point2;
                    case 2:
                        return Point3;
                    default:
                        throw new IndexOutOfRangeException("The index must be between 0 and 2!");
                }
            }
        }

        public int Count => 3;

        public IEnumerator<Point3D> GetEnumerator()
        {
            return new TriangleEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TriangleEnumerator(this);
        }

        public class TriangleEnumerator : IEnumerator<Point3D>
        {
            public TriangleEnumerator(Triangle triangle)
            {
                this.Triangle = triangle;
            }

            private readonly Triangle Triangle;
            private int Position = -1;

            public Point3D Current => Triangle[Position];

            object IEnumerator.Current => Triangle[Position];

            public bool MoveNext()
            {
                this.Position++;
                return Position < 3;
            }

            public void Reset()
            {
                Position = -1;
            }

            public void Dispose()
            {

            }
        }

        public bool IsAdjacentTo(Triangle triangle2)
        {
            int equalCount = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (this[i].Equals(triangle2[j], ThreeDGraphics.Tolerance))
                    {
                        equalCount++;

                        if (equalCount >= 2)
                        {
                            return true;
                        }

                        break;
                    }
                }

                if (i == 1 && equalCount == 0)
                {
                    return false;
                }
            }

            return false;
        }

        public int[] GetCommonSide(Triangle triangle2)
        {
            int[] tbr = new int[2];
            int equalCount = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (this[i].Equals(triangle2[j], ThreeDGraphics.Tolerance))
                    {
                        tbr[equalCount] = i;
                        equalCount++;

                        if (equalCount == 2)
                        {
                            return tbr;
                        }

                        break;
                    }
                }

                if (i == 1 && equalCount == 0)
                {
                    return null;
                }
            }

            return null;
        }
    }


    internal class Line3D : IElement3D
    {
        public Point3D Point1 { get; set; }
        public Point3D Point2 { get; set; }
        public Colour Colour { get; set; } = Colour.FromRgb(0, 0, 0);
        public LineCaps LineCap { get; set; }
        public string Tag { get; set; }
        public LineDash LineDash { get; set; }
        public double Thickness { get; set; } = 1;
        public int ZIndex { get; set; } = 0;

        public Line3D(Point3D point1, Point3D point2)
        {
            this.Point1 = point1;
            this.Point2 = point2;
        }

        private Point[] Projection;

        public void SetProjection(ICamera camera)
        {
            this.Projection = new Point[]
            {
                camera.Project(this.Point1),
                camera.Project(this.Point2)
            };
        }

        public Point[] GetProjection()
        {
            return this.Projection;
        }

        public Point3D this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Point1;
                    case 1:
                        return Point2;
                    default:
                        throw new IndexOutOfRangeException("The index must be between 0 and 1!");
                }
            }
        }

        public int Count => 2;

        public IEnumerator<Point3D> GetEnumerator()
        {
            return new LineEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new LineEnumerator(this);
        }

        public class LineEnumerator : IEnumerator<Point3D>
        {
            public LineEnumerator(Line3D line)
            {
                this.Line = line;
            }

            private readonly Line3D Line;
            private int Position = -1;

            public Point3D Current => Line[Position];

            object IEnumerator.Current => Line[Position];

            public bool MoveNext()
            {
                this.Position++;
                return Position < 2;
            }

            public void Reset()
            {
                Position = -1;
            }

            public void Dispose()
            {

            }
        }
    }


    internal class Point3DElement : IElement3D
    {
        public Point3D Point;
        public Colour Colour { get; set; } = Colours.Black;
        public double Diameter { get; set; } = 1;
        public string Tag { get; set; }
        public int ZIndex { get; set; } = 0;

        public Point3DElement(Point3D point)
        {
            this.Point = point;
        }

        private Point[] Projection;

        public void SetProjection(ICamera camera)
        {
            this.Projection = new Point[]
            {
                camera.Project(this.Point),
            };
        }

        public Point[] GetProjection()
        {
            return this.Projection;
        }

        public Point3D this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Point;
                    default:
                        throw new IndexOutOfRangeException("The index must be equal to 0!");
                }
            }
        }

        public int Count => 1;

        public IEnumerator<Point3D> GetEnumerator()
        {
            return new PointEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PointEnumerator(this);
        }

        public class PointEnumerator : IEnumerator<Point3D>
        {
            public PointEnumerator(Point3DElement point)
            {
                this.Point = point;
            }

            private readonly Point3DElement Point;
            private int Position = -1;

            public Point3D Current => Point[Position];

            object IEnumerator.Current => Point[Position];

            public bool MoveNext()
            {
                this.Position++;
                return Position < 1;
            }

            public void Reset()
            {
                Position = -1;
            }

            public void Dispose()
            {

            }
        }
    }

    public static class TopologicalSorter
    {
        // Adapted from https://www.codeproject.com/Articles/869059/Topological-sorting-in-Csharp
        public static List<T> Sort<T>(IEnumerable<T> source, Func<T, IEnumerable<T>, IEnumerable<T>> getDependencies)
        {
            var sorted = new List<T>();
            var visited = new Dictionary<T, bool>();

            foreach (var item in source)
            {
                Visit(item, source, getDependencies, sorted, visited);
            }

            return sorted;
        }

        public static void Visit<T>(T item, IEnumerable<T> source, Func<T, IEnumerable<T>, IEnumerable<T>> getDependencies, List<T> sorted, Dictionary<T, bool> visited)
        {
            bool inProcess;
            var alreadyVisited = visited.TryGetValue(item, out inProcess);

            if (alreadyVisited)
            {
                /*if (inProcess)
                {
                    throw new ArgumentException("Cyclic dependency found.");
                }*/
            }
            else
            {
                visited[item] = true;

                var dependencies = getDependencies(item, source);
                if (dependencies != null)
                {
                    foreach (var dependency in dependencies)
                    {
                        Visit(dependency, source, getDependencies, sorted, visited);
                    }
                }

                visited[item] = false;
                sorted.Add(item);
            }
        }
    }



}
