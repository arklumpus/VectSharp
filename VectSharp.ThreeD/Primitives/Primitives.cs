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

        /*public static bool CoplanarTrianglesOverlap(Point3D A1, Point3D B1, Point3D C1, Point3D A2, Point3D B2, Point3D C2, double[,] rotMatToPlane)
        {
            if (PointInTriangle(A1, A2, B2, C2) || PointInTriangle(B1, A2, B2, C2) || PointInTriangle(C1, A2, B2, C2) || PointInTriangle(A2, A1, B1, C1) || PointInTriangle(B2, A1, B1, C1) || PointInTriangle(C2, A1, B1, C1))
            {
                return true;
            }
            else
            {
                Point a1 = (rotMatToPlane * A1).DropZ();
                Point b1 = (rotMatToPlane * B1).DropZ();
                Point c1 = (rotMatToPlane * C1).DropZ();

                Point a2 = (rotMatToPlane * A2).DropZ();
                Point b2 = (rotMatToPlane * B2).DropZ();
                Point c2 = (rotMatToPlane * C2).DropZ();

                return Intersections2D.SegmentsOverlap(a1, b1, a2, b2) || Intersections2D.SegmentsOverlap(a1, b1, b2, c2) || Intersections2D.SegmentsOverlap(a1, b1, c2, a2) ||
                Intersections2D.SegmentsOverlap(b1, c1, a2, b2) || Intersections2D.SegmentsOverlap(b1, c1, b2, c2) || Intersections2D.SegmentsOverlap(b1, c1, c2, a2) ||
                Intersections2D.SegmentsOverlap(c1, a1, a2, b2) || Intersections2D.SegmentsOverlap(c1, a1, b2, c2) || Intersections2D.SegmentsOverlap(c1, a1, c2, a2);
            }
        }*/
    }

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

        public static bool PointInTriangle(int ptX, int ptY, Point A, Point B, Point C)
        {
            double signAB = (ptX - B.X) * (A.Y - B.Y) - (A.X - B.X) * (ptY - B.Y);
            double signBC = (ptX - C.X) * (B.Y - C.Y) - (B.X - C.X) * (ptY - C.Y);
            double signCA = (ptX - A.X) * (C.Y - A.Y) - (C.X - A.X) * (ptY - A.Y);

            return !((signAB < 0 || signBC < 0 || signCA < 0) && (signAB > 0 || signBC > 0 || signCA > 0));
        }

        /*  public static double HowMuchPointInTriangle(int ptX, int ptY, Point A, Point B, Point C)
          {
              double signAB = (ptX - B.X) * (A.Y - B.Y) - (A.X - B.X) * (ptY - B.Y);
              double signBC = (ptX - C.X) * (B.Y - C.Y) - (B.X - C.X) * (ptY - C.Y);
              double signCA = (ptX - A.X) * (C.Y - A.Y) - (C.X - A.X) * (ptY - A.Y);

              if ((signAB <= 0 && signBC <= 0 && signCA <= 0) || (signAB >= 0 && signBC >= 0 && signCA >= 0))
              {
                  return 1;
              }
              else
              {
                  int dominantSign = -Math.Sign(signAB) * Math.Sign(signBC) * Math.Sign(signCA);

                  if (Math.Sign(signAB) != dominantSign)
                  {
                      double distSq = signAB * signAB / ((A.X - B.X) * (A.X - B.X) + (A.Y - B.Y) * (A.Y - B.Y));

                      if (distSq < 1)
                      {
                          return 1 - Math.Sqrt(distSq);
                      }
                      else
                      {
                          return 0;
                      }
                  }
                  else if (Math.Sign(signBC) != dominantSign)
                  {
                      double distSq = signBC * signBC / ((C.X - B.X) * (C.X - B.X) + (C.Y - B.Y) * (C.Y - B.Y));

                      if (distSq < 1)
                      {
                          return 1 - Math.Sqrt(distSq);
                      }
                      else
                      {
                          return 0;
                      }
                  }
                  else //if (Math.Sign(signBC) != dominantSign)
                  {
                      double distSq = signCA * signCA / ((C.X - A.X) * (C.X - A.X) + (C.Y - A.Y) * (C.Y - A.Y));

                      if (distSq < 1)
                      {
                          return 1 - Math.Sqrt(distSq);
                      }
                      else
                      {
                          return 0;
                      }
                  }
              }
          }
        */
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

        public static bool SegmentsOverlap(Point l11, Point l12, Point l21, Point l22)
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
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /*
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
        */
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
        /*
        public static bool TrianglesOverlap(Point A1, Point B1, Point C1, Point A2, Point B2, Point C2)
        {
            return PointInTriangle(A1, A2, B2, C2) || PointInTriangle(B1, A2, B2, C2) || PointInTriangle(C1, A2, B2, C2) || PointInTriangle(A2, A1, B1, C1) || PointInTriangle(B2, A1, B1, C1) || PointInTriangle(C2, A1, B1, C1) ||
                SegmentsOverlap(A1, B1, A2, B2) || SegmentsOverlap(A1, B1, B2, C2) || SegmentsOverlap(A1, B1, C2, A2) ||
                SegmentsOverlap(B1, C1, A2, B2) || SegmentsOverlap(B1, C1, B2, C2) || SegmentsOverlap(B1, C1, C2, A2) ||
                SegmentsOverlap(C1, A1, A2, B2) || SegmentsOverlap(C1, A1, B2, C2) || SegmentsOverlap(C1, A1, C2, A2);
        }*/

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


    public readonly struct Point3D : IEquatable<Point3D>
    {
        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator Vector3D(Point3D point)
        {
            return new Vector3D(point.X, point.Y, point.Z);
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

        public Point DropZ()
        {
            return new Point(this.X, this.Y);
        }
    }


    public readonly struct Vector3D : IEquatable<Vector3D>
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

        public static Vector3D operator *(double times, Vector3D vector)
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

        public static Point3D operator +(Vector3D vector, Point3D point)
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

        public bool Equals(Vector3D other)
        {
            return other.X == this.X && other.Y == this.Y && other.Z == this.Z;
        }

        public bool Equals(Vector3D other, double tolerance)
        {
            return (Math.Abs(other.X - this.X) <= tolerance || Math.Abs((other.X - this.X) / (other.X + this.X)) <= tolerance * 0.5) && (Math.Abs(other.Y - this.Y) <= tolerance || Math.Abs((other.Y - this.Y) / (other.Y + this.Y)) <= tolerance * 0.5) && (Math.Abs(other.Z - this.Z) <= tolerance || Math.Abs((other.Z - this.Z) / (other.Z + this.Z)) <= tolerance * 0.5);
        }

        public override string ToString()
        {
            return this.X.ToString() + "; " + this.Y.ToString() + "; " + this.Z.ToString();
        }
    }

    public readonly struct NormalizedVector3D : IEquatable<NormalizedVector3D>
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

        public NormalizedVector3D(double x, double y, double z, bool normalize)
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

        public bool Equals(NormalizedVector3D other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        public NormalizedVector3D Reverse()
        {
            return new NormalizedVector3D(-X, -Y, -Z, false);
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
