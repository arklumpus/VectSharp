using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VectSharp.ThreeD
{
    public interface IMaterial
    {
        void Fill(Graphics graphics, IEnumerable<Point> points, Point3D centroid, NormalizedVector3D normal, IEnumerable<Point3D> points3D, Point3D viewPoint, ICamera camera, IEnumerable<ILightSource> lightSources, double scaleFactor, string tag);
    }

    public class ColourMaterial : IMaterial
    {
        public Colour Colour { get; }

        public double OverFill { get; set; } = 0;

        public ColourMaterial(Colour colour)
        {
            this.Colour = colour;
        }

        public void Fill(Graphics graphics, IEnumerable<Point> points, Point3D centroid, NormalizedVector3D normal, IEnumerable<Point3D> points3D, Point3D viewPoint, ICamera camera, IEnumerable<ILightSource> lightSources, double scaleFactor, string tag)
        {
            GraphicsPath path = new GraphicsPath();

            if (OverFill > 0)
            {
                double overfill = OverFill * scaleFactor;

                double meanX = 0;
                double meanY = 0;

                int count = 0;

                foreach (Point pt in points)
                {
                    meanX += pt.X;
                    meanY += pt.Y;
                    count++;
                }

                meanX /= count;
                meanY /= count;

                Point centroid2D = new Point(meanX, meanY);

                Point prevPoint = new Point();
                Point firstPoint = new Point();

                foreach (Point pt in points)
                {
                    if (path.Segments.Count == 0)
                    {
                        path.MoveTo(pt);
                        prevPoint = pt;
                        firstPoint = pt;
                    }
                    else
                    {
                        Point meanPoint = Intersections2D.ProjectOnLine(centroid2D, prevPoint, pt);

                        Point dir = new Point(meanPoint.X - meanX, meanPoint.Y - meanY);
                        double length = dir.Modulus();

                        Point newMeanPoint = new Point(meanX + dir.X * (length + overfill) / length, meanY + dir.Y * (length + overfill) / length);

                        path.LineTo(prevPoint.X - meanPoint.X + newMeanPoint.X, prevPoint.Y - meanPoint.Y + newMeanPoint.Y);

                        path.LineTo(pt.X - meanPoint.X + newMeanPoint.X, pt.Y - meanPoint.Y + newMeanPoint.Y);

                        path.LineTo(pt);

                        prevPoint = pt;
                    }
                }

                {
                    Point meanPoint = Intersections2D.ProjectOnLine(centroid2D, prevPoint, firstPoint);

                    Point dir = new Point(meanPoint.X - meanX, meanPoint.Y - meanY);
                    double length = dir.Modulus();

                    Point newMeanPoint = new Point(meanX + dir.X * (length + overfill) / length, meanY + dir.Y * (length + overfill) / length);

                    path.LineTo(prevPoint.X - meanPoint.X + newMeanPoint.X, prevPoint.Y - meanPoint.Y + newMeanPoint.Y);

                    path.LineTo(firstPoint.X - meanPoint.X + newMeanPoint.X, firstPoint.Y - meanPoint.Y + newMeanPoint.Y);
                }
            }
            else
            {
                foreach (Point pt in points)
                {
                    path.LineTo(pt);
                }
            }

            path.Close();

            graphics.FillPath(path, Colour, tag: tag);
        }
    }

    public class PhongMaterial : IMaterial
    {
        public Colour Colour { get; }

        private (double L, double a, double b) LabColour;
        private (double H, double S, double L) LabBlackHSL;
        private (double H, double S, double L) LabWhiteHSL;
        private double IntensityExponent;
        private double TotalLength;

        public double OverFill { get; set; } = 0;

        public double AmbientReflectionCoefficient { get; set; } = 1;
        public double DiffuseReflectionCoefficient { get; set; } = 1;
        public double SpecularReflectionCoefficient { get; set; } = 1;
        public double SpecularShininess { get; set; } = 1;
        public PhongMaterial(Colour colour)
        {
            this.Colour = colour;
            this.LabColour = colour.ToLab();
            this.LabBlackHSL = Colour.FromLab(0, LabColour.a, LabColour.b).ToHSL();
            this.LabWhiteHSL = Colour.FromLab(1, LabColour.a, LabColour.b).ToHSL();
            this.TotalLength = 1 + LabBlackHSL.L + (1 - LabWhiteHSL.L);
            this.IntensityExponent = Math.Log((LabBlackHSL.L + LabColour.L) / TotalLength) / Math.Log(0.5);
        }

        private Colour GetScaledColour(double intensity)
        {
            intensity = Math.Max(Math.Min(intensity, 1), 0);

            double pos = Math.Pow(intensity, IntensityExponent) * TotalLength;

            if (pos <= LabBlackHSL.L)
            {
                return Colour.FromHSL(LabBlackHSL.H, LabBlackHSL.S, pos);
            }
            else if (pos >= 1 + LabBlackHSL.L)
            {
                return Colour.FromHSL(LabWhiteHSL.H, LabWhiteHSL.S, LabWhiteHSL.L + pos - 1 - LabBlackHSL.L);
            }
            else
            {
                return Colour.FromLab(pos - LabBlackHSL.L, LabColour.a, LabColour.b);
            }
        }

        public void Fill(Graphics graphics, IEnumerable<Point> points, Point3D centroid, NormalizedVector3D normal, IEnumerable<Point3D> points3D, Point3D viewPoint, ICamera camera, IEnumerable<ILightSource> lightSources, double scaleFactor, string tag)
        {
            GraphicsPath path = new GraphicsPath();

            if (OverFill > 0)
            {
                double overfill = OverFill * scaleFactor;

                double meanX = 0;
                double meanY = 0;

                int count = 0;

                foreach (Point pt in points)
                {
                    meanX += pt.X;
                    meanY += pt.Y;
                    count++;
                }

                meanX /= count;
                meanY /= count;

                Point centroid2D = new Point(meanX, meanY);

                Point prevPoint = new Point();
                Point firstPoint = new Point();

                foreach (Point pt in points)
                {
                    if (path.Segments.Count == 0)
                    {
                        path.MoveTo(pt);
                        prevPoint = pt;
                        firstPoint = pt;
                    }
                    else
                    {
                        Point meanPoint = Intersections2D.ProjectOnLine(centroid2D, prevPoint, pt);

                        Point dir = new Point(meanPoint.X - meanX, meanPoint.Y - meanY);
                        double length = dir.Modulus();

                        Point newMeanPoint = new Point(meanX + dir.X * (length + overfill) / length, meanY + dir.Y * (length + overfill) / length);

                        path.LineTo(prevPoint.X - meanPoint.X + newMeanPoint.X, prevPoint.Y - meanPoint.Y + newMeanPoint.Y);

                        path.LineTo(pt.X - meanPoint.X + newMeanPoint.X, pt.Y - meanPoint.Y + newMeanPoint.Y);

                        path.LineTo(pt);

                        prevPoint = pt;
                    }
                }

                {
                    Point meanPoint = Intersections2D.ProjectOnLine(centroid2D, prevPoint, firstPoint);

                    Point dir = new Point(meanPoint.X - meanX, meanPoint.Y - meanY);
                    double length = dir.Modulus();

                    Point newMeanPoint = new Point(meanX + dir.X * (length + overfill) / length, meanY + dir.Y * (length + overfill) / length);

                    path.LineTo(prevPoint.X - meanPoint.X + newMeanPoint.X, prevPoint.Y - meanPoint.Y + newMeanPoint.Y);

                    path.LineTo(firstPoint.X - meanPoint.X + newMeanPoint.X, firstPoint.Y - meanPoint.Y + newMeanPoint.Y);
                }
            }
            else
            {
                foreach (Point pt in points)
                {
                    path.LineTo(pt);
                }
            }

            path.Close();

            /*NormalizedVector3D lightDirection = new NormalizedVector3D(0.5, 1.5, 1);

            double intensity = lightDirection * normal;*/

            double intensity = 0;

            foreach (ILightSource light in lightSources)
            {
                (double lightIntensity, NormalizedVector3D lightDirection) = light.GetLightAt(centroid);

                if (double.IsNaN(lightDirection.X))
                {
                    intensity += lightIntensity * AmbientReflectionCoefficient;
                }
                else
                {
                    double dotProd = lightDirection * normal;

                    intensity += lightIntensity * Math.Max(0, dotProd) * DiffuseReflectionCoefficient;

                    if (dotProd > 0)
                    {
                        NormalizedVector3D mirroredDirection = (lightDirection - 2 * dotProd * normal).Normalize();

                        NormalizedVector3D cameraDirection = (viewPoint - centroid).Normalize();

                        double dotProd2 = mirroredDirection * cameraDirection;

                        if (dotProd2 >= 0)
                        {
                            intensity += lightIntensity * Math.Pow(dotProd2, SpecularShininess) * SpecularReflectionCoefficient;
                        }
                    }
                }
            }

            graphics.FillPath(path, GetScaledColour(intensity), tag: tag);
        }
    }

    public class SimpleTexturedMaterial : IMaterial
    {
        public Graphics Texture { get; }

        public Point Point1 { get; }
        public Point Point2 { get; }
        public Point Point3 { get; }

        internal Point GetPoint(int index)
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
                    throw new IndexOutOfRangeException();
            }
        }

        public double OverFill { get; set; } = 0;

        public SimpleTexturedMaterial(Graphics texture, Point point1, Point point2, Point point3)
        {
            this.Texture = texture;
            this.Point1 = point1;
            this.Point2 = point2;
            this.Point3 = point3;
        }

        private static (double, double, double) ComputeBarycentric(Point p, Point a, Point b, Point c)
        {
            double denom = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
            double u = ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / denom;
            double v = ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / denom;
            double w = 1 - u - v;

            return (u, v, w);
        }

        private static Point3D ComputeCartesian((double u, double v, double w) barycentric, Point3D a, Point3D b, Point3D c)
        {
            return new Point3D(barycentric.u * a.X + barycentric.v * b.X + barycentric.w * c.X, barycentric.u * a.Y + barycentric.v * b.Y + barycentric.w * c.Y, barycentric.u * a.Z + barycentric.v * b.Z + barycentric.w * c.Z);
        }

        public void Fill(Graphics graphics, IEnumerable<Point> points, Point3D centroid, NormalizedVector3D normal, IEnumerable<Point3D> points3D, Point3D viewPoint, ICamera camera, IEnumerable<ILightSource> lightSources, double scaleFactor, string tag)
        {
            GraphicsPath clippingPath = new GraphicsPath();

            if (OverFill > 0)
            {
                double overfill = OverFill * scaleFactor;

                double meanX = 0;
                double meanY = 0;

                int count = 0;

                foreach (Point pt in points)
                {
                    meanX += pt.X;
                    meanY += pt.Y;
                    count++;
                }

                meanX /= count;
                meanY /= count;

                Point centroid2D = new Point(meanX, meanY);

                Point prevPoint = new Point();
                Point firstPoint = new Point();

                foreach (Point pt in points)
                {
                    if (clippingPath.Segments.Count == 0)
                    {
                        clippingPath.MoveTo(pt);
                        prevPoint = pt;
                        firstPoint = pt;
                    }
                    else
                    {
                        Point meanPoint = Intersections2D.ProjectOnLine(centroid2D, prevPoint, pt);

                        Point dir = new Point(meanPoint.X - meanX, meanPoint.Y - meanY);
                        double length = dir.Modulus();

                        Point newMeanPoint = new Point(meanX + dir.X * (length + overfill) / length, meanY + dir.Y * (length + overfill) / length);

                        clippingPath.LineTo(prevPoint.X - meanPoint.X + newMeanPoint.X, prevPoint.Y - meanPoint.Y + newMeanPoint.Y);

                        clippingPath.LineTo(pt.X - meanPoint.X + newMeanPoint.X, pt.Y - meanPoint.Y + newMeanPoint.Y);

                        clippingPath.LineTo(pt);

                        prevPoint = pt;
                    }
                }

                {
                    Point meanPoint = Intersections2D.ProjectOnLine(centroid2D, prevPoint, firstPoint);

                    Point dir = new Point(meanPoint.X - meanX, meanPoint.Y - meanY);
                    double length = dir.Modulus();

                    Point newMeanPoint = new Point(meanX + dir.X * (length + overfill) / length, meanY + dir.Y * (length + overfill) / length);

                    clippingPath.LineTo(prevPoint.X - meanPoint.X + newMeanPoint.X, prevPoint.Y - meanPoint.Y + newMeanPoint.Y);

                    clippingPath.LineTo(firstPoint.X - meanPoint.X + newMeanPoint.X, firstPoint.Y - meanPoint.Y + newMeanPoint.Y);
                }
            }
            else
            {
                foreach (Point pt in points)
                {
                    clippingPath.LineTo(pt);
                }
            }

            clippingPath.Close();

            graphics.Save();
            graphics.SetClippingPath(clippingPath);

            Graphics transformedTexture = Texture.Transform(pt => camera.Project(ComputeCartesian(ComputeBarycentric(pt, this.Point1, this.Point2, this.Point3), points3D.ElementAt(0), points3D.ElementAt(1), points3D.ElementAt(2))), 15);

            graphics.DrawGraphics(0, 0, transformedTexture);

            graphics.Restore();

            //graphics.FillPath(clippingPath, Colour, tag: tag);
        }
    }


    public static class TextureFactory
    {
        public static void Apply(SimpleTexturedMaterial texture, Triangle referenceTriangle, IEnumerable<IElement3D> elements)
        {
            List<Triangle> triangles = (from el in elements let tr = el as Triangle where tr != null select tr).ToList();
            Apply(texture, referenceTriangle, triangles);
        }

        private static Point Multiply(double[,] matrix, Point pt)
        {
            double x = matrix[0, 0] * pt.X + matrix[0, 1] * pt.Y + matrix[0, 2];
            double y = matrix[1, 0] * pt.X + matrix[1, 1] * pt.Y + matrix[1, 2];
            double z = matrix[2, 0] * pt.X + matrix[2, 1] * pt.Y + matrix[2, 2];

            return new Point(x / z, y / z);
        }

        public static int[] Apply(SimpleTexturedMaterial texture, Triangle referenceTriangle, IList<Triangle> triangles)
        {
            List<int>[] edges = new List<int>[triangles.Count];
            List<int> rootEdges = new List<int>();

            for (int i = 0; i < triangles.Count; i++)
            {
                edges[i] = new List<int>();
            }

            for (int i = 0; i < triangles.Count; i++)
            {
                if (referenceTriangle.IsAdjacentTo(triangles[i]))
                {
                    rootEdges.Add(i);
                }

                for (int j = i + 1; j < triangles.Count; j++)
                {
                    if (triangles[i].IsAdjacentTo(triangles[j]))
                    {
                        edges[i].Add(j);
                        edges[j].Add(i);
                    }
                }
            }

            bool[] visited = new bool[triangles.Count];
            int[] distances = new int[triangles.Count];
            int[] parents = new int[triangles.Count];
            int[] sortedTriangles = new int[triangles.Count];

            int sortPos = 0;

            Queue<int> queue = new Queue<int>();
            queue.Enqueue(-1);

            while (queue.Count > 0)
            {
                int v = queue.Dequeue();

                if (v < 0)
                {
                    for (int i = 0; i < rootEdges.Count; i++)
                    {
                        if (!visited[rootEdges[i]])
                        {
                            visited[rootEdges[i]] = true;
                            parents[rootEdges[i]] = -1;
                            queue.Enqueue(rootEdges[i]);
                            sortedTriangles[sortPos] = rootEdges[i];
                            sortPos++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < edges[v].Count; i++)
                    {
                        if (!visited[edges[v][i]])
                        {
                            distances[edges[v][i]] = distances[v] + 1;
                            visited[edges[v][i]] = true;
                            parents[edges[v][i]] = v;
                            queue.Enqueue(edges[v][i]);
                            sortedTriangles[sortPos] = edges[v][i];
                            sortPos++;
                        }
                    }
                }
            }

            SimpleTexturedMaterial[] textures = new SimpleTexturedMaterial[triangles.Count];

            for (int i = 0; i < sortedTriangles.Length; i++)
            {
                if (parents[sortedTriangles[i]] < 0)
                {
                    Triangle parentTriangle = referenceTriangle;

                    int[] commonSide = parentTriangle.GetCommonSide(triangles[sortedTriangles[i]]);

                    if (commonSide != null)
                    {
                        int minInd = Math.Min(commonSide[0], commonSide[1]);
                        int maxInd = Math.Max(commonSide[0], commonSide[1]);

                        Point p1_2D = texture.GetPoint(minInd);
                        Point p2_2D = texture.GetPoint(maxInd);

                        Point3D p1_3D = parentTriangle[minInd];
                        Point3D p2_3D = parentTriangle[maxInd];

                        Point3D p3_3D_parent = parentTriangle[3 - commonSide[0] - commonSide[1]];
                        Point p3_2D_parent = texture.GetPoint(3 - commonSide[0] - commonSide[1]);

                        int[] commonSideChild = triangles[sortedTriangles[i]].GetCommonSide(parentTriangle);
                        Point3D p3_3D = triangles[sortedTriangles[i]][3 - commonSideChild[0] - commonSideChild[1]];

                        int p0Child = triangles[sortedTriangles[i]][0].Equals(p1_3D) ? 0 : triangles[sortedTriangles[i]][0].Equals(p2_3D) ? 1 : 2;
                        int p1Child = triangles[sortedTriangles[i]][1].Equals(p1_3D) ? 0 : triangles[sortedTriangles[i]][1].Equals(p2_3D) ? 1 : 2;
                        int p2Child = triangles[sortedTriangles[i]][2].Equals(p1_3D) ? 0 : triangles[sortedTriangles[i]][2].Equals(p2_3D) ? 1 : 2;

                        double angle = Math.Atan2((triangles[sortedTriangles[i]].ActualNormal ^ parentTriangle.ActualNormal).Modulus, parentTriangle.ActualNormal * triangles[sortedTriangles[i]].ActualNormal);

                        Point3D p3_3D_cand1 = p1_3D + (Vector3D)(Matrix3D.RotationAroundAxis((p2_3D - p1_3D).Normalize(), angle) * (Point3D)(p3_3D - p1_3D));
                        Point3D p3_3D_cand2 = p1_3D + (Vector3D)(Matrix3D.RotationAroundAxis((p2_3D - p1_3D).Normalize(), -angle) * (Point3D)(p3_3D - p1_3D));

                        double prod = (p3_3D_cand1 - ((Vector3D)p1_3D + p2_3D) * 0.5).Normalize() * triangles[sortedTriangles[i]].ActualNormal;

                        if (prod < 0)
                        {
                            p3_3D = p3_3D_cand1;
                        }
                        else
                        {
                            p3_3D = p3_3D_cand2;
                        }

                        double[,] rotationMatrix = Matrix3D.RotationToAlignWithZ(parentTriangle.ActualNormal);

                        p1_3D = rotationMatrix * p1_3D;
                        p2_3D = rotationMatrix * p2_3D;
                        p3_3D_parent = rotationMatrix * p3_3D_parent;
                        p3_3D = rotationMatrix * p3_3D;

                        double angle_2D = Math.Atan2((p2_2D.X - p1_2D.X) * (p2_3D.Y - p1_3D.Y) - (p2_2D.Y - p1_2D.Y) * (p2_3D.X - p1_3D.X), (p2_2D.X - p1_2D.X) * (p2_3D.X - p1_3D.X) + (p2_2D.Y - p1_2D.Y) * (p2_3D.Y - p1_3D.Y));

                        double[,] rotationMatrix_2D = new double[3, 3]
                        {
                            {Math.Cos(angle_2D), -Math.Sin(angle_2D), 0 },
                            {Math.Sin(angle_2D), Math.Cos(angle_2D), 0 },
                            {0, 0, 1 }
                        };

                        Point p1_3D_2D = Multiply(rotationMatrix_2D, new Point(p1_3D.X, p1_3D.Y));
                        Point p2_3D_2D = Multiply(rotationMatrix_2D, new Point(p2_3D.X, p2_3D.Y));
                        Point p3_3D_2D_parent = Multiply(rotationMatrix_2D, new Point(p3_3D_parent.X, p3_3D_parent.Y));
                        Point p3_3D_2D = Multiply(rotationMatrix_2D, new Point(p3_3D.X, p3_3D.Y));


                        double scaleX;
                        double scaleY;

                        if (Math.Abs(p3_3D_2D_parent.X - p1_3D_2D.X) > ThreeDGraphics.Tolerance && Math.Abs(p3_2D_parent.X - p1_2D.X) > ThreeDGraphics.Tolerance)
                        {
                            scaleX = (p3_2D_parent.X - p1_2D.X) / (p3_3D_2D_parent.X - p1_3D_2D.X);
                        }
                        else if (Math.Abs(p2_3D_2D.X - p1_3D_2D.X) > ThreeDGraphics.Tolerance && Math.Abs(p2_2D.X - p1_2D.X) > ThreeDGraphics.Tolerance)
                        {
                            scaleX = (p2_2D.X - p1_2D.X) / (p2_3D_2D.X - p1_3D_2D.X);
                        }
                        else
                        {
                            scaleX = (p3_2D_parent.X - p2_2D.X) / (p3_3D_2D_parent.X - p2_3D_2D.X);
                        }

                        if (Math.Abs(p3_3D_2D_parent.Y - p1_3D_2D.Y) > ThreeDGraphics.Tolerance && Math.Abs(p3_2D_parent.Y - p1_2D.Y) > ThreeDGraphics.Tolerance)
                        {
                            scaleY = (p3_2D_parent.Y - p1_2D.Y) / (p3_3D_2D_parent.Y - p1_3D_2D.Y);
                        }
                        else if (Math.Abs(p2_3D_2D.Y - p1_3D_2D.Y) > ThreeDGraphics.Tolerance && Math.Abs(p2_2D.Y - p1_2D.Y) > ThreeDGraphics.Tolerance)
                        {
                            scaleY = (p2_2D.Y - p1_2D.Y) / (p2_3D_2D.Y - p1_3D_2D.Y);
                        }
                        else
                        {
                            scaleY = (p3_2D_parent.Y - p2_2D.Y) / (p3_3D_2D_parent.Y - p2_3D_2D.Y);
                        }

                        Point p3_2D = new Point(p1_2D.X + (p3_3D_2D.X - p1_3D_2D.X) * scaleX, p1_2D.Y + (p3_3D_2D.Y - p1_3D_2D.Y) * scaleY);


                        SimpleTexturedMaterial text = new SimpleTexturedMaterial(texture.Texture, p0Child == 0 ? p1_2D : p0Child == 1 ? p2_2D : p3_2D, p1Child == 0 ? p1_2D : p1Child == 1 ? p2_2D : p3_2D, p2Child == 0 ? p1_2D : p2Child == 1 ? p2_2D : p3_2D) { OverFill = texture.OverFill };
                        triangles[sortedTriangles[i]].Fill.Add(text);
                        textures[sortedTriangles[i]] = text;
                    }
                }
                else
                {
                    Triangle parentTriangle = triangles[parents[sortedTriangles[i]]];
                    SimpleTexturedMaterial parentTexture = textures[parents[sortedTriangles[i]]];

                    int[] commonSide = parentTriangle.GetCommonSide(triangles[sortedTriangles[i]]);

                    if (commonSide != null)
                    {
                        int minInd = Math.Min(commonSide[0], commonSide[1]);
                        int maxInd = Math.Max(commonSide[0], commonSide[1]);

                        Point p1_2D = parentTexture.GetPoint(minInd);
                        Point p2_2D = parentTexture.GetPoint(maxInd);

                        Point3D p1_3D = parentTriangle[minInd];
                        Point3D p2_3D = parentTriangle[maxInd];

                        Point3D p3_3D_parent = parentTriangle[3 - commonSide[0] - commonSide[1]];
                        Point p3_2D_parent = parentTexture.GetPoint(3 - commonSide[0] - commonSide[1]);

                        int[] commonSideChild = triangles[sortedTriangles[i]].GetCommonSide(parentTriangle);
                        Point3D p3_3D = triangles[sortedTriangles[i]][3 - commonSideChild[0] - commonSideChild[1]];

                        int p0Child = triangles[sortedTriangles[i]][0].Equals(p1_3D) ? 0 : triangles[sortedTriangles[i]][0].Equals(p2_3D) ? 1 : 2;
                        int p1Child = triangles[sortedTriangles[i]][1].Equals(p1_3D) ? 0 : triangles[sortedTriangles[i]][1].Equals(p2_3D) ? 1 : 2;
                        int p2Child = triangles[sortedTriangles[i]][2].Equals(p1_3D) ? 0 : triangles[sortedTriangles[i]][2].Equals(p2_3D) ? 1 : 2;

                        double angle = Math.Atan2((triangles[sortedTriangles[i]].ActualNormal ^ parentTriangle.ActualNormal).Modulus, parentTriangle.ActualNormal * triangles[sortedTriangles[i]].ActualNormal);

                        Point3D p3_3D_cand1 = p1_3D + (Vector3D)(Matrix3D.RotationAroundAxis((p2_3D - p1_3D).Normalize(), angle) * (Point3D)(p3_3D - p1_3D));
                        Point3D p3_3D_cand2 = p1_3D + (Vector3D)(Matrix3D.RotationAroundAxis((p2_3D - p1_3D).Normalize(), -angle) * (Point3D)(p3_3D - p1_3D));

                        double prod = (p3_3D_cand1 - ((Vector3D)p1_3D + p2_3D) * 0.5).Normalize() * triangles[sortedTriangles[i]].ActualNormal;

                        if (prod < 0)
                        {
                            p3_3D = p3_3D_cand1;
                        }
                        else
                        {
                            p3_3D = p3_3D_cand2;
                        }

                        double[,] rotationMatrix = Matrix3D.RotationToAlignWithZ(parentTriangle.ActualNormal);

                        p1_3D = rotationMatrix * p1_3D;
                        p2_3D = rotationMatrix * p2_3D;
                        p3_3D_parent = rotationMatrix * p3_3D_parent;
                        p3_3D = rotationMatrix * p3_3D;

                        double angle_2D = Math.Atan2((p2_2D.X - p1_2D.X) * (p2_3D.Y - p1_3D.Y) - (p2_2D.Y - p1_2D.Y) * (p2_3D.X - p1_3D.X), (p2_2D.X - p1_2D.X) * (p2_3D.X - p1_3D.X) + (p2_2D.Y - p1_2D.Y) * (p2_3D.Y - p1_3D.Y));

                        double[,] rotationMatrix_2D = new double[3, 3]
                        {
                            {Math.Cos(angle_2D), -Math.Sin(angle_2D), 0 },
                            {Math.Sin(angle_2D), Math.Cos(angle_2D), 0 },
                            {0, 0, 1 }
                        };

                        Point p1_3D_2D = Multiply(rotationMatrix_2D, new Point(p1_3D.X, p1_3D.Y));
                        Point p2_3D_2D = Multiply(rotationMatrix_2D, new Point(p2_3D.X, p2_3D.Y));
                        Point p3_3D_2D_parent = Multiply(rotationMatrix_2D, new Point(p3_3D_parent.X, p3_3D_parent.Y));
                        Point p3_3D_2D = Multiply(rotationMatrix_2D, new Point(p3_3D.X, p3_3D.Y));


                        double scaleX;
                        double scaleY;

                        if (Math.Abs(p3_3D_2D_parent.X - p1_3D_2D.X) > ThreeDGraphics.Tolerance && Math.Abs(p3_2D_parent.X - p1_2D.X) > ThreeDGraphics.Tolerance)
                        {
                            scaleX = (p3_2D_parent.X - p1_2D.X) / (p3_3D_2D_parent.X - p1_3D_2D.X);
                        }
                        else if (Math.Abs(p2_3D_2D.X - p1_3D_2D.X) > ThreeDGraphics.Tolerance && Math.Abs(p2_2D.X - p1_2D.X) > ThreeDGraphics.Tolerance)
                        {
                            scaleX = (p2_2D.X - p1_2D.X) / (p2_3D_2D.X - p1_3D_2D.X);
                        }
                        else
                        {
                            scaleX = (p3_2D_parent.X - p2_2D.X) / (p3_3D_2D_parent.X - p2_3D_2D.X);
                        }

                        if (Math.Abs(p3_3D_2D_parent.Y - p1_3D_2D.Y) > ThreeDGraphics.Tolerance && Math.Abs(p3_2D_parent.Y - p1_2D.Y) > ThreeDGraphics.Tolerance)
                        {
                            scaleY = (p3_2D_parent.Y - p1_2D.Y) / (p3_3D_2D_parent.Y - p1_3D_2D.Y);
                        }
                        else if (Math.Abs(p2_3D_2D.Y - p1_3D_2D.Y) > ThreeDGraphics.Tolerance && Math.Abs(p2_2D.Y - p1_2D.Y) > ThreeDGraphics.Tolerance)
                        {
                            scaleY = (p2_2D.Y - p1_2D.Y) / (p2_3D_2D.Y - p1_3D_2D.Y);
                        }
                        else
                        {
                            scaleY = (p3_2D_parent.Y - p2_2D.Y) / (p3_3D_2D_parent.Y - p2_3D_2D.Y);
                        }

                        Point p3_2D = new Point(p1_2D.X + (p3_3D_2D.X - p1_3D_2D.X) * scaleX, p1_2D.Y + (p3_3D_2D.Y - p1_3D_2D.Y) * scaleY);

                        SimpleTexturedMaterial text = new SimpleTexturedMaterial(texture.Texture, p0Child == 0 ? p1_2D : p0Child == 1 ? p2_2D : p3_2D, p1Child == 0 ? p1_2D : p1Child == 1 ? p2_2D : p3_2D, p2Child == 0 ? p1_2D : p2Child == 1 ? p2_2D : p3_2D) { OverFill = texture.OverFill };
                        triangles[sortedTriangles[i]].Fill.Add(text);
                        textures[sortedTriangles[i]] = text;
                    }
                }
            }

            return sortedTriangles;
        }

        private static bool EqualWithTolerance(double val1, double val2, double tolerance)
        {
            return Math.Abs(val1 - val2) <= tolerance || Math.Abs((val1 - val2) / (val1 + val2)) <= tolerance * 0.5;
        }

        public static int[] Apply(Graphics texture, Point3D origin, NormalizedVector3D xAxis, NormalizedVector3D yAxis, double scaleX, double scaleY, IEnumerable<IElement3D> elements, double overFill = 0)
        {
            List<Triangle> triangles = (from el in elements let tr = el as Triangle where tr != null select tr).ToList();
            return Apply(texture, origin, xAxis, yAxis, scaleX, scaleY, triangles, overFill);
        }

        public static int[] Apply(Graphics texture, Point3D origin, NormalizedVector3D xAxis, NormalizedVector3D yAxis, double scaleX, double scaleY, IList<Triangle> triangles, double overFill = 0)
        {
            xAxis = (xAxis - yAxis * (xAxis * yAxis)).Normalize();

            NormalizedVector3D normal = (xAxis ^ yAxis).Normalize();

            int closestTriangle = -1;
            double minDistance1 = double.MaxValue;
            double minDistance2 = double.MaxValue;
            double minDistance3 = double.MaxValue;

            List<double> distances = new List<double>(3) { double.NaN, double.NaN, double.NaN };
            for (int i = 0; i < triangles.Count; i++)
            {
                if (triangles[i].ActualNormal * normal > 0)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        distances[j] = Math.Abs((triangles[i][j] - origin) * normal);
                    }

                    distances.Sort();

                    if (EqualWithTolerance(distances[0], minDistance1, ThreeDGraphics.Tolerance))
                    {
                        if (EqualWithTolerance(distances[1], minDistance2, ThreeDGraphics.Tolerance))
                        {
                            if (!EqualWithTolerance(distances[2], minDistance3, ThreeDGraphics.Tolerance) && distances[2] < minDistance3)
                            {
                                minDistance1 = distances[0];
                                minDistance2 = distances[1];
                                minDistance3 = distances[2];
                                closestTriangle = i;
                            }
                        }
                        else if (distances[1] < minDistance2)
                        {
                            minDistance1 = distances[0];
                            minDistance2 = distances[1];
                            minDistance3 = distances[2];
                            closestTriangle = i;
                        }
                    }
                    else if (distances[0] < minDistance1)
                    {
                        minDistance1 = distances[0];
                        minDistance2 = distances[1];
                        minDistance3 = distances[2];
                        closestTriangle = i;
                    }
                }
            }

            for (int j = 0; j < 3; j++)
            {
                distances[j] = (triangles[closestTriangle][j] - origin) * normal;
            }

            int[] sortedVertices = (from el in Enumerable.Range(0, 3) orderby distances[el] ascending select el).ToArray();

            Point3D p1 = triangles[closestTriangle][sortedVertices[0]];
            p1 = (Point3D)(p1 - normal * (((Vector3D)p1 - origin) * normal));

            Point3D p2 = triangles[closestTriangle][sortedVertices[1]];
            p2 = (Point3D)(p2 - normal * (((Vector3D)p2 - origin) * normal));
            p2 = p1 + (p2 - p1).Normalize() * (triangles[closestTriangle][sortedVertices[1]] - triangles[closestTriangle][sortedVertices[0]]).Modulus;

            double[,] rotationMatrix1 = Matrix3D.RotationToAlignAWithB((triangles[closestTriangle][sortedVertices[1]] - triangles[closestTriangle][sortedVertices[0]]).Normalize(), (p2 - p1).Normalize());


            Vector3D p2p = rotationMatrix1 * (Point3D)(triangles[closestTriangle][sortedVertices[1]] - triangles[closestTriangle][sortedVertices[0]]);
            Vector3D p3p = rotationMatrix1 * (Point3D)(triangles[closestTriangle][sortedVertices[2]] - triangles[closestTriangle][sortedVertices[0]]);

            NormalizedVector3D normalP = (p2p ^ p3p).Normalize();

            double[,] rotationMatrix2 = Matrix3D.RotationToAlignAWithB(normalP, normal);

            Point3D p3 = p1 + (Vector3D)(rotationMatrix2 * (Point3D)p3p);

            Point[] points2D = new Point[] { new Point((p1 - origin) * xAxis * scaleX, (p1 - origin) * yAxis * scaleY), new Point((p2 - origin) * xAxis * scaleX, (p2 - origin) * yAxis * scaleY), new Point((p3 - origin) * xAxis * scaleX, (p3 - origin) * yAxis * scaleY) };

            SimpleTexturedMaterial text = new SimpleTexturedMaterial(texture, points2D[sortedVertices[0]], points2D[sortedVertices[1]], points2D[sortedVertices[2]]) { OverFill = overFill };

            return Apply(text, triangles[closestTriangle], triangles);
        }
    }

}
