using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectSharp.ThreeD
{
    public interface ICamera
    {
        Point TopLeft { get; }
        Size Size { get; }

        double ScaleFactor { get; }

        Point3D ViewPoint { get; }

        Point Project(Point3D point);

        int Compare(IElement3D element1, IElement3D element2, Graphics gpr);

        bool IsCulled(IElement3D element);
    }

    public class PerspectiveCamera : ICamera
    {
        public double ScaleFactor { get; }

        public Point3D Position { get; private set; }

        public Point3D ViewPoint => Position;

        public NormalizedVector3D Direction { get; private set; }
        public double Distance { get; }

        public double[,] RotationMatrix { get; private set; }

        public double[,] CameraRotationMatrix { get; private set; }

        public Point3D OrbitOrigin { get; private set; }

        private Point3D Origin { get; set; }

        private NormalizedVector3D RotationReference { get; set; }

        public Point TopLeft { get; }
        public Size Size { get; }

        public PerspectiveCamera(Point3D position, NormalizedVector3D direction, double distance, Size viewSize, double scaleFactor)
        {
            this.Position = position;
            this.Direction = direction;
            this.Distance = distance;

            this.Origin = position + direction * distance;
            this.RotationMatrix = Matrix3D.RotationToAlignWithZ(direction);

            this.ScaleFactor = scaleFactor;

            this.TopLeft = new Point(-viewSize.Width * 0.5 * ScaleFactor, -viewSize.Height * 0.5 * ScaleFactor);
            this.Size = new Size(viewSize.Width * ScaleFactor, viewSize.Height * ScaleFactor);

            this.OrbitOrigin = position + direction * ((Vector3D)position).Modulus;

            if (new Vector3D(0, 1, 0) * this.Direction < 1)
            {
                this.RotationReference = (new Vector3D(0, 1, 0) - (new Vector3D(0, 1, 0) * this.Direction) * this.Direction).Normalize();
            }
            else
            {
                this.RotationReference = (new Vector3D(0, 0, 1) - (new Vector3D(0, 0, 1) * this.Direction) * this.Direction).Normalize();
            }

            Point3D rotatedY = this.RotationMatrix * (Point3D)(Vector3D)this.RotationReference;
            double rotationAngle = Math.PI / 2 - Math.Atan2(rotatedY.Y, rotatedY.X);
            this.CameraRotationMatrix = Matrix3D.RotationAroundAxis(new NormalizedVector3D(0, 0, 1), rotationAngle);
        }

        public Point Project(Point3D point)
        {
            //Vector3D toOrigin = point - this.Origin;
            Vector3D cameraDirection = (this.Position - point).Normalize();

            //Point3D projectedPoint = (Point3D)(toOrigin + cameraDirection * (toOrigin * cameraDirection));

            //Point3D planePoint = point + cameraDirection * (((this.Origin - point) * this.Direction) / (cameraDirection * this.Direction));

            Point3D projectedPoint = (Point3D)(point + cameraDirection * (((this.Origin - point) * this.Direction) / (cameraDirection * this.Direction)) - this.Origin);

            //Vector3D vectPoint = (Vector3D)projectedPoint;

            /* Point3D rotatedZ = this.RotationMatrix * new Point3D(0, 0, 1);

             double deRotation = Math.Atan2(rotatedZ.Y, rotatedZ.X);*/

            //Point3D rotatedY = this.RotationMatrix * (Point3D)(Vector3D)this.RotationReference;
            //Point3D rotatedY = this.RotationMatrix * (Point3D)(Vector3D)(new Vector3D(0, 1, 0) - (new Vector3D(0, 1, 0) * this.Direction) * this.Direction).Normalize();

            //double rotationAngle = Math.PI / 2 - Math.Atan2(rotatedY.Y, rotatedY.X);

            //Point3D rotatedPoint = Matrix3D.RotationAroundAxis(new NormalizedVector3D(0, 0, 1), rotationAngle) * (this.RotationMatrix * projectedPoint);

            Point3D rotatedPoint = this.CameraRotationMatrix * (this.RotationMatrix * projectedPoint);

            return new Point(rotatedPoint.X * ScaleFactor, rotatedPoint.Y * ScaleFactor);

            /*Point3D rotatedPoint = this.RotationMatrix * (Point3D)toOrigin;

            return new Point(rotatedPoint.X / (point - this.Origin).Modulus * this.Distance, rotatedPoint.Y / (point - this.Origin).Modulus * this.Distance);*/
        }

        internal Point3D Deproject(Point point, Line3D line)
        {
            Point3D rotatedPoint = new Point3D(point.X / ScaleFactor, point.Y / ScaleFactor, 0);

            Point3D projectedPoint = RotationMatrix.Inverse() * (CameraRotationMatrix.Inverse() * rotatedPoint);
            Point3D cameraPlanePoint = projectedPoint + (Vector3D)this.Origin;

            NormalizedVector3D v = (cameraPlanePoint - this.Position).Normalize();
            NormalizedVector3D l = (line[1] - line[0]).Normalize();

            double t;

            if (v.X * l.Y - v.Y * l.X != 0)
            {
                t = (l.X * (this.Position.Y - line[0].Y) - l.Y * (this.Position.X - line[0].X)) / (v.X * l.Y - v.Y * l.X);
            }
            else if (v.Z * l.Y - v.Y * l.Z != 0)
            {
                t = (l.Z * (this.Position.Y - line[0].Y) - l.Y * (this.Position.Z - line[0].Z)) / (v.Z * l.Y - v.Y * l.Z);
            }
            else if (v.Z * l.X - v.X * l.Z != 0)
            {
                t = (l.Z * (this.Position.X - line[0].X) - l.X * (this.Position.Z - line[0].Z)) / (v.Z * l.X - v.X * l.Z);
            }
            else
            {
                throw new Exception("The lines do not intersect!");
            }

            Point3D pt = this.Position + v * t;
            return pt;
        }

        internal Point3D Deproject(Point point, Triangle triangle)
        {
            Point3D rotatedPoint = new Point3D(point.X / ScaleFactor, point.Y / ScaleFactor, 0);

            Point3D projectedPoint = RotationMatrix.Inverse() * (CameraRotationMatrix.Inverse() * rotatedPoint);
            Point3D cameraPlanePoint = projectedPoint + (Vector3D)this.Origin;

            Point3D centroid = (Point3D)(((Vector3D)triangle[0] + triangle[1] + triangle[2]) * (1.0 / 3.0));

            Vector3D l = (cameraPlanePoint - this.Position).Normalize();

            double d = ((centroid - this.Position) * triangle.ActualNormal) / (l * triangle.ActualNormal);

            Point3D pt = this.Position + l * d;
            return pt;
        }

        public int Compare(IElement3D element1, IElement3D element2, Graphics gpr)
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
                        double dist1 = (pt1[0] - this.Position).Modulus;
                        double dist2 = (pt2[0] - this.Position).Modulus;

                        if (Math.Abs(dist1 - dist2) < ThreeDGraphics.Tolerance)
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
                else if (element2 is Line3D line)
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

                        double dist1 = (pt1[0] - this.Position).Modulus;
                        double dist2 = (pt - this.Position).Modulus;

                        if (Math.Abs(dist1 - dist2) < ThreeDGraphics.Tolerance)
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
                else if (element2 is Triangle triangle)
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

                        double dist1 = (pt1[0] - this.Position).Modulus;
                        double dist2 = (pt - this.Position).Modulus;

                        if (Math.Abs(dist1 - dist2) < ThreeDGraphics.Tolerance)
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
            else if (element1 is Line3D line1)
            {
                if (element2 is Point3DElement)
                {
                    return -Compare(element2, element1, gpr);
                }
                else if (element2 is Line3D line2)
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

                            double dist1 = (point1 - this.Position).Modulus;
                            double dist2 = (point2 - this.Position).Modulus;

                            if (Math.Abs(dist1 - dist2) < ThreeDGraphics.Tolerance)
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
                else if (element2 is Triangle triangle)
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

                    List<Point> interss1 = Intersections2D.Intersect(l11, l12, A, B, C, line1.Thickness, ThreeDGraphics.Tolerance);
                    List<Point> interss2 = Intersections2D.Intersect(l21, l22, A, B, C, ThreeDGraphics.Tolerance);
                    List<Point> interss3 = Intersections2D.Intersect(l31, l32, A, B, C, ThreeDGraphics.Tolerance);

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

                            double dist1 = (pt3D1 - this.Position).Modulus;
                            double dist2 = (pt3D2 - this.Position).Modulus;

                            int currSign = -Math.Sign(dist1 - dist2);

                            if (Math.Abs(dist1 - dist2) < ThreeDGraphics.Tolerance)
                            {
                                currSign = 0;
                            }

                            if (sign == 0 || currSign == sign || currSign == 0)
                            {
                                sign = currSign;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Line intersects with triangle!");
                                return 0;
                            }
                        }

                        /*if (sign == 0)
                        {
                            return 1;
                        }*/

                        return sign;

                        /* Point inters1 = interss[0];
                         Point inters2 = interss[1];

                         Point3D inters13D = this.Deproject(inters1, line1);
                         Point3D inters23D = this.Deproject(inters2, line1);

                         Point3D inters1Triangle = this.Deproject(inters1, triangle);
                         Point3D inters2Triangle = this.Deproject(inters2, triangle);


                         double dist11 = (inters13D - this.Position).Modulus;
                         double dist12 = (inters23D - this.Position).Modulus;

                         double dist21 = (inters1Triangle - this.Position).Modulus;
                         double dist22 = (inters2Triangle - this.Position).Modulus;

                         int sign1 = -Math.Sign(dist11 - dist21);
                         int sign2 = -Math.Sign(dist12 - dist22);

                         if (Math.Abs(dist11 - dist21) < Tolerance)
                         {
                             sign1 = 0;
                         }

                         if (Math.Abs(dist12 - dist22) < Tolerance)
                         {
                             sign2 = 0;
                         }

                         if (sign1 == sign2 && sign1 == 0)
                         {
                             return 1;
                         }
                         else if (sign1 == sign2 || sign2 == 0)
                         {
                             return sign1;
                         }
                         else if (sign1 == 0)
                         {
                             return sign2;
                         }
                         else
                         {
                             System.Diagnostics.Debug.WriteLine("Line intersects with triangle!");
                             return 0;
                         }*/
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
            else if (element1 is Triangle triangle1)
            {
                if (element2 is Point3DElement || element2 is Line3D)
                {
                    return -Compare(element2, element1, gpr);
                }
                else if (element2 is Triangle triangle2)
                {


                    Point[] triangle1Projection = triangle1.GetProjection();
                    Point[] triangle2Projection = triangle2.GetProjection();

                    /*Point A1 = this.Project(triangle1[0]);
                    Point B1 = this.Project(triangle1[1]);
                    Point C1 = this.Project(triangle1[2]);

                    Point A2 = this.Project(triangle2[0]);
                    Point B2 = this.Project(triangle2[1]);
                    Point C2 = this.Project(triangle2[2]);*/

                    Point A1 = triangle1Projection[0];
                    Point B1 = triangle1Projection[1];
                    Point C1 = triangle1Projection[2];

                    Point A2 = triangle2Projection[0];
                    Point B2 = triangle2Projection[1];
                    Point C2 = triangle2Projection[2];

                    /*

                    List<Point> intersections = new List<Point>();


                    List<Point>[] allIntersections = new List<Point>[]
                    {
                        //Intersections2D.Intersect(A2, B2, A1, B1, C1, Tolerance),
                        //Intersections2D.Intersect(B2, C2, A1, B1, C1, 0),
                        //Intersections2D.Intersect(C2, A2, A1, B1, C1, Tolerance),
                        //Intersections2D.Intersect(A1, B1, A2, B2, C2, Tolerance),
                        //Intersections2D.Intersect(B1, C1, A2, B2, C2, Tolerance),
                        //Intersections2D.Intersect(C1, A1, A2, B2, C2, Tolerance)
                    };

                    foreach (List<Point> points in allIntersections)
                    {
                        if (points != null)
                        {
                            foreach (Point point in points)
                            {
                                bool found = false;
                                foreach (Point existing in intersections)
                                {
                                    if (Intersections2D.AreEqual(existing, point, Tolerance))
                                    {
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    intersections.Add(point);
                                }    
                            }
                        }
                    }*/

                    List<Point> intersections = Intersections2D.IntersectTriangles(A1, B1, C1, A2, B2, C2, ThreeDGraphics.Tolerance);

                    /*for (int i = 0; i < intersections.Count; i++)
                    {
                        gpr.FillRectangle(intersections[i].X - 1, intersections[i].Y - 1, 2, 2, Colours.Green);
                    }*/

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

                        //gpr.FillRectangle(pt.X - 0.5, pt.Y - 0.5, 1, 1, Colours.Orange);

                        Point3D pt3D1 = this.Deproject(pt, triangle1);
                        Point3D pt3D2 = this.Deproject(pt, triangle2);

                        double dist1 = (pt3D1 - this.Position).Modulus;
                        double dist2 = (pt3D2 - this.Position).Modulus;

                        if (double.IsNaN(dist1 - dist2))
                        {
                            return 0;
                        }


                        int sign = -Math.Sign(dist1 - dist2);

                        if (Math.Abs(dist1 - dist2) < ThreeDGraphics.Tolerance)
                        {
                            sign = 0;
                        }

                        return sign;

                        /*int sign = 0;

                        foreach (Point pt in intersections)
                        {
                            Point3D pt3D1 = this.Deproject(pt, triangle1);
                            Point3D pt3D2 = this.Deproject(pt, triangle2);

                            double dist1 = (pt3D1 - this.Position).Modulus;
                            double dist2 = (pt3D2 - this.Position).Modulus;

                            int currSign = -Math.Sign(dist1 - dist2);

                            if (Math.Abs(dist1 - dist2) < Tolerance)
                            {
                                currSign = 0;
                            }

                            if (sign == 0 || currSign == sign || currSign == 0)
                            {
                                sign = currSign;
                            }
                            else
                            {
                                //System.Diagnostics.Debug.WriteLine("Two triangles intersect!");
                                return 0;
                            }
                        }

                        return sign;*/
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


            /*

            double maxDist = 0;

            foreach (Point3D point in element)
            {
                maxDist = Math.Max(maxDist, (point - this.Position).Modulus);
            }

            return maxDist;*/
        }

        public bool IsCulled(IElement3D element)
        {
            if (element is Point3DElement)
            {
                return (element[0] - this.Origin) * (this.Position - this.Origin) >= 0;
            }
            else if (element is Line3D)
            {
                foreach (Point3D pt in element)
                {
                    if ((pt - this.Origin) * (this.Position - this.Origin) < 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (element is Triangle triangle)
            {
                bool found = false;

                foreach (Point3D pt in element)
                {
                    if ((pt - this.Origin) * (this.Position - this.Origin) < 0)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    return true;
                }
                else
                {
                    return triangle.ActualNormal * (triangle.Centroid - this.Position) < 0;
                }
            }
            else
            {
                return true;
            }
        }

        public void Orbit(double theta, double phi)
        {
            Vector3D vect = this.Position - this.OrbitOrigin;

            /*double[,] rotationMatrix = Matrix3D.RotationToAlignWithZ(vect.Normalize());

            if (rotationMatrix[0, 0] > 1e-4)
            {
                theta *= -1;
            }

            double r = vect.Modulus;

            Point3D newVect = Matrix3D.RotationAroundY(theta) * new Point3D(0, 0, r);

            this.Position = this.OrbitOrigin + (Vector3D)(rotationMatrix.Inverse() * newVect);*/

            //double[,] rotationMatrix = this.RotationMatrix;

            //NormalizedVector3D yAxis = ((Vector3D)(rotationMatrix.Inverse() * new Point3D(0, 1, 0))).Normalize();

            NormalizedVector3D yAxis = new NormalizedVector3D(0, 1, 0);
            NormalizedVector3D xAxis = (this.Direction ^ (Vector3D)yAxis).Normalize();

            phi *= Math.Sign(Math.Atan2(xAxis.Z, xAxis.X));
            theta *= Math.Sign(yAxis * this.RotationReference);

            double[,] thetaRotation = Matrix3D.RotationAroundAxis(yAxis, theta);

            //double[,] rotationMatrix2 = Matrix3D.RotationToAlignWithZ(this.Direction, false);

            //Vector3D xDir = thetaRotation * (rotationMatrix2 * new Point3D(1, 0, 0));

            //NormalizedVector3D xAxis = (xDir - (xDir * yAxis) * yAxis).Normalize();
            //NormalizedVector3D xAxis = ((Vector3D)(thetaRotation * new Point3D(0, 0, 1))).Normalize();

            /*if (Math.Abs((Vector3D)yAxis * this.Direction) == 1)
            {
                phi = 0;
            }*/

            //double scal = (Vector3D)yAxis * this.Direction;

            //System.Diagnostics.Debug.WriteLine(scal.ToString() + "\t" + ang.ToString());

            double[,] phiRotation = Matrix3D.RotationAroundAxis(xAxis, phi);

            Vector3D rotatedVect = (Vector3D)(phiRotation * (thetaRotation * (Point3D)vect));

            this.Position = this.OrbitOrigin + rotatedVect;
            this.Direction = (this.OrbitOrigin - this.Position).Normalize();
            this.RotationReference = ((Vector3D)(phiRotation * (thetaRotation * (Point3D)(Vector3D)this.RotationReference))).Normalize();

            this.Origin = this.Position + this.Direction * this.Distance;
            this.RotationMatrix = Matrix3D.RotationToAlignWithZ(this.Direction);

            Point3D rotatedY = this.RotationMatrix * (Point3D)(Vector3D)this.RotationReference;
            double rotationAngle = Math.PI / 2 - Math.Atan2(rotatedY.Y, rotatedY.X);
            this.CameraRotationMatrix = Matrix3D.RotationAroundAxis(new NormalizedVector3D(0, 0, 1), rotationAngle);
        }


        public void Zoom(double amount)
        {
            this.Position = this.Position + this.Direction * amount;
            this.Origin = this.Position + this.Direction * this.Distance;
        }

        public void Pan(double X, double Y)
        {
            Vector3D delta = this.RotationMatrix.Inverse() * new Point3D(-X / ScaleFactor, -Y / ScaleFactor, 0);

            this.Position = this.Position + delta;
            this.Origin = this.Position + this.Direction * this.Distance;

            this.OrbitOrigin = this.OrbitOrigin + delta;
        }
    }
}
