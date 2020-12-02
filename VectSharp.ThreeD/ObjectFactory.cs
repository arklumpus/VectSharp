using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectSharp.ThreeD
{
    public static class ObjectFactory
    {
        public static List<Element3D> CreateCube(Point3D center, double size, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            return CreateCuboid(center, size, size, size, fill, tag, zIndex);
        }

        public static List<Element3D> CreateCuboid(Point3D center, double sizeX, double sizeY, double sizeZ, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            Point3D A = new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5);
            Point3D B = new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5);
            Point3D C = new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5);
            Point3D D = new Point3D(center.X - sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5);

            Point3D E = new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z + sizeZ * 0.5);
            Point3D F = new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z + sizeZ * 0.5);
            Point3D G = new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z + sizeZ * 0.5);
            Point3D H = new Point3D(center.X - sizeX * 0.5, center.Y + sizeY * 0.5, center.Z + sizeZ * 0.5);


            List<Element3D> tbr = new List<Element3D>();

            tbr.AddRange(CreateRectangle(A, B, C, D, fill, tag, zIndex));
            tbr.AddRange(CreateRectangle(H, G, F, E, fill, tag, zIndex));

            tbr.AddRange(CreateRectangle(E, F, B, A, fill, tag, zIndex));
            tbr.AddRange(CreateRectangle(G, H, D, C, fill, tag, zIndex));

            tbr.AddRange(CreateRectangle(A, D, H, E, fill, tag, zIndex));
            tbr.AddRange(CreateRectangle(F, G, C, B, fill, tag, zIndex));

            return tbr;
        }

        public static List<Element3D> CreateRectangle(Point3D point1, Point3D point2, Point3D point3, Point3D point4, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            Triangle3DElement triangle1 = new Triangle3DElement(point1, point2, point3);
            triangle1.Fill.AddRange(fill);
            triangle1.Tag = tag;
            triangle1.ZIndex = zIndex;

            Triangle3DElement triangle2 = new Triangle3DElement(point1, point3, point4);
            triangle2.Fill.AddRange(fill);
            triangle2.Tag = tag;
            triangle2.ZIndex = zIndex;

            return new List<Element3D> { triangle1, triangle2 };
        }

        public static List<Element3D> CreateRectangle(Point3D point1, Point3D point2, Point3D point3, Point3D point4, NormalizedVector3D point1Normal, NormalizedVector3D point2Normal, NormalizedVector3D point3Normal, NormalizedVector3D point4Normal, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            Triangle3DElement triangle1 = new Triangle3DElement(point1, point2, point3, point1Normal, point2Normal, point3Normal);
            triangle1.Fill.AddRange(fill);
            triangle1.Tag = tag;
            triangle1.ZIndex = zIndex;

            Triangle3DElement triangle2 = new Triangle3DElement(point1, point3, point4, point1Normal, point3Normal, point4Normal);
            triangle2.Fill.AddRange(fill);
            triangle2.Tag = tag;
            triangle2.ZIndex = zIndex;

            return new List<Element3D> { triangle1, triangle2 };
        }

        public static List<Element3D> CreateSphere(Point3D center, double radius, int steps, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            List<Point3D> points = new List<Point3D>();

            for (int t = 0; t <= steps; t++)
            {
                for (int p = 0; p < steps * 2; p++)
                {
                    double theta = Math.PI / steps * t;
                    double phi = Math.PI / steps * p;

                    double x = center.X + radius * Math.Sin(theta) * Math.Cos(phi);
                    double y = center.Y + radius * Math.Sin(theta) * Math.Sin(phi);
                    double z = center.Z + radius * Math.Cos(theta);

                    points.Add(new Point3D(x, y, z));

                    if (t == 0 || t == steps)
                    {
                        break;
                    }
                }
            }

            List<Element3D> tbr = new List<Element3D>(4 * steps + (points.Count - 2 - 2 * steps) * 2);

            for (int i = 0; i < points.Count - 1; i++)
            {
                if (i == 0)
                {
                    for (int j = 0; j < 2 * steps; j++)
                    {
                        Point3D p1 = points[i];
                        Point3D p3 = points[i + 1 + j];
                        Point3D p2 = points[i + 1 + (j + 1) % (2 * steps)];

                        Triangle3DElement tri = new Triangle3DElement(p1, p2, p3, (center - p1).Normalize(), (center - p2).Normalize(), (center - p3).Normalize());
                        tri.Fill.AddRange(fill);
                        tri.Tag = tag;
                        tri.ZIndex = zIndex;
                        tbr.Add(tri);
                    }
                }
                else if (i >= points.Count - 1 - 2 * steps)
                {
                    Point3D p1 = points[i];
                    Point3D p3 = points[points.Count - 1];
                    Point3D p2 = points[points.Count - 1 - 2 * steps + (i - (points.Count - 1 - 2 * steps) + 1) % (2 * steps)];

                    Triangle3DElement tri = new Triangle3DElement(p1, p2, p3, (center - p1).Normalize(), (center - p2).Normalize(), (center - p3).Normalize());
                    tri.Fill.AddRange(fill);
                    tri.Tag = tag;
                    tri.ZIndex = zIndex;
                    tbr.Add(tri);
                }
                else
                {
                    if ((i - 1) % (2 * steps) < 2 * steps - 1)
                    {
                        Point3D p4 = points[i + 2 * steps];
                        Point3D p3 = points[i + 2 * steps + 1];
                        Point3D p2 = points[i + 1];
                        Point3D p1 = points[i];

                        tbr.AddRange(CreateRectangle(p1, p2, p3, p4, (center - p1).Normalize(), (center - p2).Normalize(), (center - p3).Normalize(), (center - p4).Normalize(), fill, tag, zIndex));
                    }
                    else
                    {
                        Point3D p4 = points[i + 2 * steps];
                        Point3D p3 = points[(i / (2 * steps)) * 2 * steps + 1];
                        Point3D p2 = points[(i / (2 * steps) - 1) * 2 * steps + 1];
                        Point3D p1 = points[i];

                        tbr.AddRange(CreateRectangle(p1, p2, p3, p4, (center - p1).Normalize(), (center - p2).Normalize(), (center - p3).Normalize(), (center - p4).Normalize(), fill, tag, zIndex));
                    }
                }
            }

            return tbr;
        }

        public static List<Element3D> CreateTetrahedron(Point3D center, double radius, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            Point3D tip = new Point3D(center.X, center.Y - radius, center.Z);
            Point3D base1 = new Point3D(Math.Sqrt(8.0 / 9) * radius + center.X, center.Y + radius / 3, center.Z);
            Point3D base2 = new Point3D(-Math.Sqrt(2.0 / 9) * radius + center.X, center.Y + radius / 3, center.Z + Math.Sqrt(2.0 / 3) * radius);
            Point3D base3 = new Point3D(-Math.Sqrt(2.0 / 9) * radius + center.X, center.Y + radius / 3, center.Z - Math.Sqrt(2.0 / 3) * radius);

            Triangle3DElement faceTriangle1 = new Triangle3DElement(tip, base2, base1) { Tag = tag, ZIndex = zIndex };
            faceTriangle1.Fill.AddRange(fill);

            Triangle3DElement faceTriangle2 = new Triangle3DElement(tip, base3, base2) { Tag = tag, ZIndex = zIndex };
            faceTriangle2.Fill.AddRange(fill);

            Triangle3DElement faceTriangle3 = new Triangle3DElement(tip, base1, base3) { Tag = tag, ZIndex = zIndex };
            faceTriangle3.Fill.AddRange(fill);

            Triangle3DElement baseTriangle = new Triangle3DElement(base1, base2, base3) { Tag = tag, ZIndex = zIndex };
            baseTriangle.Fill.AddRange(fill);

            return new List<Element3D>() { faceTriangle1, faceTriangle2, faceTriangle3, baseTriangle };
        }

        public static List<Element3D> CreatePolygon(GraphicsPath polygon2D, double triangulationResolution, Point3D origin, NormalizedVector3D xAxis, NormalizedVector3D yAxis, IEnumerable<IMaterial> fill, bool reverseTriangles, string tag = null, int zIndex = 0)
        {
            xAxis = (xAxis - yAxis * (xAxis * yAxis)).Normalize();

            List<GraphicsPath> triangles = polygon2D.Triangulate(triangulationResolution, true).ToList();

            double[,] matrix1 = Matrix3D.RotationToAlignAWithB(new NormalizedVector3D(0, 1, 0), yAxis);
            double[,] matrix2 = Matrix3D.RotationToAlignAWithB(((Vector3D)(matrix1 * new Point3D(1, 0, 0))).Normalize(), xAxis);

            List<Element3D> tbr = new List<Element3D>(triangles.Count);

            for (int i = 0; i < triangles.Count; i++)
            {
                Point p1 = triangles[i].Segments[0].Point;
                Point p2 = triangles[i].Segments[1].Point;
                Point p3 = triangles[i].Segments[2].Point;

                Point3D p13D = matrix2 * (matrix1 * new Point3D(p1.X, p1.Y, 0)) + (Vector3D)origin;
                Point3D p23D = matrix2 * (matrix1 * new Point3D(p2.X, p2.Y, 0)) + (Vector3D)origin;
                Point3D p33D = matrix2 * (matrix1 * new Point3D(p3.X, p3.Y, 0)) + (Vector3D)origin;

                Triangle3DElement t = !reverseTriangles ? new Triangle3DElement(p13D, p23D, p33D) : new Triangle3DElement(p13D, p33D, p23D);
                t.Fill.AddRange(fill);
                t.Tag = tag;
                t.ZIndex = zIndex;
                tbr.Add(t);
            }

            return tbr;
        }

        public static List<Element3D> CreatePrism(GraphicsPath polygonBase2D, double triangulationResolution, Point3D bottomOrigin, Point3D topOrigin, NormalizedVector3D baseXAxis, NormalizedVector3D baseYAxis, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            baseXAxis = (baseXAxis - baseYAxis * (baseXAxis * baseYAxis)).Normalize();

            List<Element3D> tbr = new List<Element3D>();

            bool orientation = (baseXAxis ^ baseYAxis) * (bottomOrigin - topOrigin) > 0;

            double[,] matrix1 = Matrix3D.RotationToAlignAWithB(new NormalizedVector3D(0, 1, 0), baseYAxis);
            double[,] matrix2 = Matrix3D.RotationToAlignAWithB(((Vector3D)(matrix1 * new Point3D(1, 0, 0))).Normalize(), baseXAxis);
            List<List<NormalizedVector3D>> normals = (from el2 in polygonBase2D.GetLinearisationPointsNormals(triangulationResolution) select (from el in el2 select ((Vector3D)(matrix2 * (matrix1 * new Point3D(el.X, el.Y, 0)))).Normalize()).ToList()).ToList();

            polygonBase2D = polygonBase2D.Linearise(triangulationResolution);

            tbr.AddRange(CreatePolygon(polygonBase2D, triangulationResolution, bottomOrigin, baseXAxis, baseYAxis, fill, orientation, tag, zIndex));
            tbr.AddRange(CreatePolygon(polygonBase2D, triangulationResolution, topOrigin, baseXAxis, baseYAxis, fill, !orientation, tag, zIndex));

            List<List<Point3D>> bottomPoints = (from el2 in polygonBase2D.GetPoints() select (from el in el2 select matrix2 * (matrix1 * new Point3D(el.X, el.Y, 0)) + (Vector3D)bottomOrigin).ToList()).ToList();
            List<List<Point3D>> topPoints = (from el2 in polygonBase2D.GetPoints() select (from el in el2 select matrix2 * (matrix1 * new Point3D(el.X, el.Y, 0)) + (Vector3D)topOrigin).ToList()).ToList();

            if (orientation)
            {
                for (int i = 0; i < bottomPoints.Count; i++)
                {
                    for (int j = 0; j < bottomPoints[i].Count - 1; j++)
                    {
                        tbr.AddRange(CreateRectangle(bottomPoints[i][j], bottomPoints[i][j + 1], topPoints[i][j + 1], topPoints[i][j], normals[i][j], normals[i][j + 1], normals[i][j + 1], normals[i][j], fill, tag, zIndex));
                    }
                }
            }
            else
            {
                for (int i = 0; i < bottomPoints.Count; i++)
                {
                    for (int j = 0; j < bottomPoints[i].Count - 1; j++)
                    {
                        tbr.AddRange(CreateRectangle(bottomPoints[i][j], topPoints[i][j], topPoints[i][j + 1], bottomPoints[i][j + 1], normals[i][j], normals[i][j], normals[i][j + 1], normals[i][j + 1], fill, tag, zIndex));
                    }
                }
            }

            return tbr;
        }

        public static List<Element3D> CreateWireframe(IEnumerable<Element3D> object3D, Colour colour, double thickness = 1, LineCaps lineCap = LineCaps.Butt, LineDash? lineDash = null, string tag = null, int zIndex = 0)
        {
            List<Element3D> tbr = new List<Element3D>();

            List<Point3D[]> addedLines = new List<Point3D[]>();

            void addLine(Point3D p1, Point3D p2)
            {
                for (int i = 0; i < addedLines.Count; i++)
                {
                    if ((addedLines[i][0].Equals(p1, 1e-4) && addedLines[i][1].Equals(p2, 1e-4)) || (addedLines[i][0].Equals(p2, 1e-4) && addedLines[i][1].Equals(p1, 1e-4)))
                    {
                        return;
                    }
                }

                tbr.Add(new Line3DElement(p1, p2) { Colour = colour, Thickness = thickness, LineCap = lineCap, LineDash = lineDash ?? LineDash.SolidLine, Tag = tag, ZIndex = zIndex });
                addedLines.Add(new Point3D[] { p1, p2 });
            }

            foreach (Element3D element in object3D)
            {
                if (element is Triangle3DElement triangle)
                {
                    addLine(triangle.Point1, triangle.Point2);
                    addLine(triangle.Point2, triangle.Point3);
                    addLine(triangle.Point3, triangle.Point1);
                }
            }

            return tbr;
        }

        public static List<Element3D> CreatePoints(IEnumerable<Element3D> object3D, Colour colour, double diameter = 1, string tag = null, int zIndex = 0)
        {
            List<Element3D> tbr = new List<Element3D>();

            List<Point3D> addedPoints = new List<Point3D>();

            void addPoint(Point3D p)
            {
                for (int i = 0; i < addedPoints.Count; i++)
                {
                    if (addedPoints[i].Equals(p, 1e-4))
                    {
                        return;
                    }
                }

                tbr.Add(new Point3DElement(p) { Colour = colour, Diameter = diameter, Tag = tag, ZIndex = zIndex });
                addedPoints.Add(p);
            }

            foreach (Element3D element in object3D)
            {
                if (!(element is Point3DElement))
                {
                    foreach (Point3D p in element)
                    {
                        addPoint(p);
                    }
                }
            }

            return tbr;
        }
    }
}
