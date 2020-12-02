using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectSharp.ThreeD
{
    public abstract class Camera
    {
        public abstract Point TopLeft { get; protected set; }
        public abstract Size Size { get; protected set; }

        public abstract double ScaleFactor { get; }

        public abstract Point3D ViewPoint { get; }

        public abstract Point Project(Point3D point);

        public abstract Point3D Deproject(Point point, Line3DElement element);
        public abstract Point3D Deproject(Point point, Triangle3DElement element);

        public virtual int Compare(Element3D element1, Element3D element2)
        {
            if (element1.ZIndex != element2.ZIndex)
            {
                return element1.ZIndex - element2.ZIndex;
            }

            if (element1 is Point3DElement pt1)
            {
                Point proj1 = this.Project(pt1[0]);

                if (element2 is Point3DElement pt2)
                {
                    Point proj2 = this.Project(pt2[0]);

                    if ((proj1.X - proj2.X) * (proj1.X - proj2.X) + (proj1.Y - proj2.Y) * (proj1.Y - proj2.Y) < (pt1.Diameter * 0.5 + pt2.Diameter * 0.5) * (pt1.Diameter * 0.5 + pt2.Diameter * 0.5))
                    {
                        double dist1 = this.ZDepth(pt1[0]);
                        double dist2 = this.ZDepth(pt2[0]);

                        if (Math.Abs(dist1 - dist2) < Scene.Tolerance)
                        {
                            return 0;
                        }

                        return -Math.Sign(dist1 - dist2);
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (element2 is Line3DElement line)
                {
                    Point p0 = proj1;

                    Point p1 = this.Project(line[0]);
                    Point p2 = this.Project(line[1]);

                    double distSq = ((p2.Y - p1.Y) * p0.X - (p2.X - p1.X) * p0.Y + p2.X * p1.Y - p2.Y * p1.X) * ((p2.Y - p1.Y) * p0.X - (p2.X - p1.X) * p0.Y + p2.X * p1.Y - p2.Y * p1.X) / ((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));

                    double maxDistSq = (pt1.Diameter * 0.5 + line.Thickness * 0.5) * (pt1.Diameter * 0.5 + line.Thickness * 0.5);

                    Point v1 = new Point(p0.X - p1.X, p0.Y - p1.Y);
                    Point v2 = new Point(p2.X - p1.X, p2.Y - p1.Y);

                    double v2Length = v2.Modulus();

                    Point e2 = new Point(v2.X / v2Length, v2.Y / v2Length);

                    double dotProd = v1.X * e2.X + v1.Y * e2.Y;

                    if (dotProd >= -Math.Sqrt(maxDistSq) && dotProd <= v2Length + Math.Sqrt(maxDistSq) && distSq < maxDistSq)
                    {
                        Point pointOnLine = new Point(p1.X + dotProd * e2.X, p1.Y + dotProd * e2.Y);

                        Point3D pt = this.Deproject(pointOnLine, line);

                        double dist1 = this.ZDepth(pt1[0]);
                        double dist2 = this.ZDepth(pt);

                        if (Math.Abs(dist1 - dist2) < Scene.Tolerance)
                        {
                            return 0;
                        }

                        return -Math.Sign(dist1 - dist2);
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (element2 is Triangle3DElement triangle)
                {
                    Point p0 = proj1;

                    Point p1 = this.Project(triangle[0]);
                    Point p2 = this.Project(triangle[1]);
                    Point p3 = this.Project(triangle[2]);

                    double area = 0.5 * (-p2.Y * p3.X + p1.Y * (-p2.X + p3.X) + p1.X * (p2.Y - p3.Y) + p2.X * p3.Y);

                    double s = 1 / (2 * area) * (p1.Y * p3.X - p1.X * p3.Y + (p3.Y - p1.Y) * p0.X + (p1.X - p3.X) * p0.Y);
                    double t = 1 / (2 * area) * (p1.X * p2.Y - p1.Y * p2.X + (p1.Y - p2.Y) * p0.X + (p2.X - p1.X) * p0.Y);

                    if (s >= 0 && t >= 0 && 1 - s - t >= 0)
                    {
                        Point3D pt = this.Deproject(p0, triangle);

                        double dist1 = this.ZDepth(pt1[0]);
                        double dist2 = this.ZDepth(pt);

                        if (Math.Abs(dist1 - dist2) < Scene.Tolerance)
                        {
                            return 0;
                        }

                        return -Math.Sign(dist1 - dist2);
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (element1 is Line3DElement line1)
            {
                if (element2 is Point3DElement)
                {
                    return -Compare(element2, element1);
                }
                else if (element2 is Line3DElement line2)
                {
                    Point l11 = this.Project(line1[0]);
                    Point l12 = this.Project(line1[1]);

                    Point l21 = this.Project(line2[0]);
                    Point l22 = this.Project(line2[1]);


                    (Point inters, double t, double s)? intersection = Intersections2D.Intersect(l11, l12, l21, l22);

                    if (intersection != null)
                    {
                        double vLength = new Point(l12.X - l11.X, l12.Y - l11.Y).Modulus();
                        double lLength = new Point(l22.X - l21.X, l22.Y - l21.Y).Modulus();

                        if (intersection?.t >= -line1.Thickness && intersection?.t <= vLength + line1.Thickness && intersection?.s >= -line1.Thickness && intersection?.s <= lLength + line1.Thickness)
                        {
                            Point3D point1 = this.Deproject(intersection.Value.inters, line1);
                            Point3D point2 = this.Deproject(intersection.Value.inters, line2);

                            double dist1 = this.ZDepth(point1);
                            double dist2 = this.ZDepth(point2);

                            if (Math.Abs(dist1 - dist2) < Scene.Tolerance)
                            {
                                return 0;
                            }

                            return -Math.Sign(dist1 - dist2);
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (element2 is Triangle3DElement triangle)
                {
                    Point l11 = this.Project(line1[0]);
                    Point l12 = this.Project(line1[1]);

                    Point lineDirPerp = new Point(l12.Y - l11.Y, l11.X - l12.X);
                    lineDirPerp = new Point(lineDirPerp.X / lineDirPerp.Modulus(), lineDirPerp.Y / lineDirPerp.Modulus());

                    Point l21 = new Point(l11.X + lineDirPerp.X * line1.Thickness * 0.5, l11.Y + lineDirPerp.Y * line1.Thickness * 0.5);
                    Point l22 = new Point(l12.X + lineDirPerp.X * line1.Thickness * 0.5, l12.Y + lineDirPerp.Y * line1.Thickness * 0.5);

                    Point l31 = new Point(l11.X - lineDirPerp.X * line1.Thickness * 0.5, l11.Y - lineDirPerp.Y * line1.Thickness * 0.5);
                    Point l32 = new Point(l12.X - lineDirPerp.X * line1.Thickness * 0.5, l12.Y - lineDirPerp.Y * line1.Thickness * 0.5);


                    Point A = this.Project(triangle[0]);
                    Point B = this.Project(triangle[1]);
                    Point C = this.Project(triangle[2]);

                    List<Point> interss1 = Intersections2D.Intersect(l11, l12, A, B, C, line1.Thickness, Scene.Tolerance);
                    List<Point> interss2 = Intersections2D.Intersect(l21, l22, A, B, C, Scene.Tolerance);
                    List<Point> interss3 = Intersections2D.Intersect(l31, l32, A, B, C, Scene.Tolerance);

                    List<Point> interss = new List<Point>();

                    if (interss1 != null && interss1.Count > 0)
                    {
                        interss.AddRange(interss1);
                    }

                    if (interss2 != null && interss2.Count > 0)
                    {
                        interss.AddRange(from el in interss2 select Intersections2D.ProjectOnLine(el, l11, l12));
                    }

                    if (interss3 != null && interss3.Count > 0)
                    {
                        interss.AddRange(from el in interss3 select Intersections2D.ProjectOnLine(el, l11, l12));
                    }

                    if (interss != null && interss.Count > 0)
                    {

                        int sign = 0;

                        foreach (Point pt in interss)
                        {
                            Point3D pt3D1 = this.Deproject(pt, line1);
                            Point3D pt3D2 = this.Deproject(pt, triangle);

                            double dist1 = this.ZDepth(pt3D1);
                            double dist2 = this.ZDepth(pt3D2);

                            int currSign = -Math.Sign(dist1 - dist2);

                            if (Math.Abs(dist1 - dist2) < Scene.Tolerance)
                            {
                                currSign = 0;
                            }

                            if (sign == 0 || currSign == sign || currSign == 0)
                            {
                                sign = currSign;
                            }
                            else
                            {
                                //Line intersects with triangle
                                return 0;
                            }
                        }

                        return sign;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (element1 is Triangle3DElement triangle1)
            {
                if (element2 is Point3DElement || element2 is Line3DElement)
                {
                    return -Compare(element2, element1);
                }
                else if (element2 is Triangle3DElement triangle2)
                {
                    Point[] triangle1Projection = triangle1.GetProjection();
                    Point[] triangle2Projection = triangle2.GetProjection();

                    Point A1 = triangle1Projection[0];
                    Point B1 = triangle1Projection[1];
                    Point C1 = triangle1Projection[2];

                    Point A2 = triangle2Projection[0];
                    Point B2 = triangle2Projection[1];
                    Point C2 = triangle2Projection[2];

                    List<Point> intersections = Intersections2D.IntersectTriangles(A1, B1, C1, A2, B2, C2, Scene.Tolerance);

                    if (intersections.Count >= 3)
                    {
                        double meanX = 0;
                        double meanY = 0;

                        foreach (Point p in intersections)
                        {
                            meanX += p.X;
                            meanY += p.Y;
                        }

                        meanX /= intersections.Count;
                        meanY /= intersections.Count;

                        Point pt = new Point(meanX, meanY);


                        Point3D pt3D1 = this.Deproject(pt, triangle1);
                        Point3D pt3D2 = this.Deproject(pt, triangle2);

                        double dist1 = this.ZDepth(pt3D1);
                        double dist2 = this.ZDepth(pt3D2);

                        if (double.IsNaN(dist1 - dist2))
                        {
                            return 0;
                        }


                        int sign = -Math.Sign(dist1 - dist2);

                        if (Math.Abs(dist1 - dist2) < Scene.Tolerance)
                        {
                            sign = 0;
                        }

                        return sign;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public abstract double ZDepth(Point3D point);

        public abstract bool IsCulled(Element3D element);
    }

    public interface ICameraWithControls
    {
        void Zoom(double amount);
        void Pan(double x, double y);
        void Orbit(double theta, double phi);
    }

    public abstract class CameraWithControls : Camera, ICameraWithControls
    {
        public abstract void Zoom(double amount);
        public abstract void Pan(double x, double y);
        public abstract void Orbit(double theta, double phi);
    }

}
