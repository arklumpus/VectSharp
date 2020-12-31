using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// A static class containing methods to create complex 3D objects.
    /// </summary>
    public static class ObjectFactory
    {
        /// <summary>
        /// Creates a cube.
        /// </summary>
        /// <param name="center">The centre of the cube.</param>
        /// <param name="size">The length of each side of the cube.</param>
        /// <param name="fill">A collection of materials that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="tag">A tag that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="zIndex">A z-index that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <returns>A list of <see cref="Triangle3DElement"/>s that constitute the cube.</returns>
        public static List<Element3D> CreateCube(Point3D center, double size, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            return CreateCuboid(center, size, size, size, fill, tag, zIndex);
        }

        /// <summary>
        /// Creates a cuboid.
        /// </summary>
        /// <param name="center">The centre of the cube.</param>
        /// <param name="sizeX">The length of the sides of the cube parallel to the x axis.</param>
        /// <param name="sizeY">The length of the sides of the cube parallel to the y axis.</param>
        /// <param name="sizeZ">The length of the sides of the cube parallel to the z axis.</param>
        /// <param name="fill">A collection of materials that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="tag">A tag that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="zIndex">A z-index that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <returns>A list of <see cref="Triangle3DElement"/>s that constitute the cuboid.</returns>
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

        /// <summary>
        /// Creates a quadrilater. All the vertices need not be coplanar.
        /// </summary>
        /// <param name="point1">The first vertex of the quadrilater.</param>
        /// <param name="point2">The second vertex of the quadrilater.</param>
        /// <param name="point3">The third vertex of the quadrilater.</param>
        /// <param name="point4">The fourth vertex of the quadrilater.</param>
        /// <param name="fill">A collection of materials that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="tag">A tag that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="zIndex">A z-index that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <returns>A list containing two <see cref="Triangle3DElement"/>s representing the quadrilater.</returns>
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

        /// <summary>
        /// Creates a quadrilater, specifying the vertex normals at the four vertices. All the vertices need not be coplanar.
        /// </summary>
        /// <param name="point1">The first vertex of the quadrilater.</param>
        /// <param name="point2">The second vertex of the quadrilater.</param>
        /// <param name="point3">The third vertex of the quadrilater.</param>
        /// <param name="point4">The fourth vertex of the quadrilater.</param>
        /// <param name="point1Normal">The vertex normal at the first vertex of the quadrilater.</param>
        /// <param name="point2Normal">The vertex normal at the second vertex of the quadrilater.</param>
        /// <param name="point3Normal">The vertex normal at the third vertex of the quadrilater.</param>
        /// <param name="point4Normal">The vertex normal at the fourth vertex of the quadrilater.</param>
        /// <param name="fill">A collection of materials that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="tag">A tag that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="zIndex">A z-index that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <returns>A list containing two <see cref="Triangle3DElement"/>s representing the quadrilater.</returns>
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

        /// <summary>
        /// Creates a sphere.
        /// </summary>
        /// <param name="center">The centre of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <param name="steps">The number of meridians and parallels to use when generating the sphere.</param>
        /// <param name="fill">A collection of materials that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="tag">A tag that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="zIndex">A z-index that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <returns>A list of <see cref="Triangle3DElement"/>s that constitute the sphere.</returns>
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

        /// <summary>
        /// Creates a tetrahedron inscribed in a sphere.
        /// </summary>
        /// <param name="center">The centre of the tetrahedron.</param>
        /// <param name="radius">The radius of the sphere in which the tetrahedron is inscribed.</param>
        /// <param name="fill">A collection of materials that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="tag">A tag that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="zIndex">A z-index that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <returns>A list of <see cref="Triangle3DElement"/>s that constitute the sphere.</returns>
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

        /// <summary>
        /// Creates a flat polygon.
        /// </summary>
        /// <param name="polygon2D">A 2D <see cref="GraphicsPath"/> representing the polygon.</param>
        /// <param name="triangulationResolution">The resolution that will be used to linearise curve segments in the <see cref="GraphicsPath"/>.</param>
        /// <param name="origin">A <see cref="Point3D"/> that will correspond to the origin of the 2D reference system.</param>
        /// <param name="xAxis">A <see cref="NormalizedVector3D"/> that will correspond to the x axis of the 2D reference system. This will be orthonormalised to the <paramref name="yAxis"/>.</param>
        /// <param name="yAxis">A <see cref="NormalizedVector3D"/> that will correspond to the y axis of the 2D reference system.</param>
        /// <param name="reverseTriangles">Indicates whether the order of the points (and thus the normals) of all the triangles returned by this method should be reversed.</param>
        /// <param name="fill">A collection of materials that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="tag">A tag that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="zIndex">A z-index that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <returns>A list of <see cref="Triangle3DElement"/>s that constitute the polygon.</returns>
        public static List<Element3D> CreatePolygon(GraphicsPath polygon2D, double triangulationResolution, Point3D origin, NormalizedVector3D xAxis, NormalizedVector3D yAxis, bool reverseTriangles, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            xAxis = (xAxis - yAxis * (xAxis * yAxis)).Normalize();

            List<GraphicsPath> triangles = polygon2D.Triangulate(triangulationResolution, true).ToList();

            List<Element3D> tbr = new List<Element3D>(triangles.Count);

            for (int i = 0; i < triangles.Count; i++)
            {
                Point p1 = triangles[i].Segments[0].Point;
                Point p2 = triangles[i].Segments[1].Point;
                Point p3 = triangles[i].Segments[2].Point;

                Point3D p13D = origin + xAxis * p1.X + yAxis * p1.Y;
                Point3D p23D = origin + xAxis * p2.X + yAxis * p2.Y;
                Point3D p33D = origin + xAxis * p3.X + yAxis * p3.Y;

                Triangle3DElement t = !reverseTriangles ? new Triangle3DElement(p13D, p23D, p33D) : new Triangle3DElement(p13D, p33D, p23D);
                t.Fill.AddRange(fill);
                t.Tag = tag;
                t.ZIndex = zIndex;
                tbr.Add(t);
            }

            return tbr;
        }

        /// <summary>
        /// Creates a prism with the specified base.
        /// </summary>
        /// <param name="polygonBase2D">A 2D <see cref="GraphicsPath"/> representing the base of the prism.</param>
        /// <param name="triangulationResolution">The resolution that will be used to linearise curve segments in the <see cref="GraphicsPath"/>.</param>
        /// <param name="bottomOrigin">A <see cref="Point3D"/> that will correspond to the origin of the 2D reference system of the bottom base.</param>
        /// <param name="topOrigin">A <see cref="Point3D"/> that will correspond to the origin of the 2D reference system of the top base.</param>
        /// <param name="baseXAxis">A <see cref="NormalizedVector3D"/> that will correspond to the x axis of the 2D reference system of the bases. This will be orthonormalised to the <paramref name="baseYAxis"/>.</param>
        /// <param name="baseYAxis">A <see cref="NormalizedVector3D"/> that will correspond to the y axis of the 2D reference system of the bases.</param>
        /// <param name="fill">A collection of materials that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="tag">A tag that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <param name="zIndex">A z-index that will be applied to the <see cref="Triangle3DElement"/>s returned by this method.</param>
        /// <returns>A list of <see cref="Triangle3DElement"/>s that constitute the prism.</returns>
        public static List<Element3D> CreatePrism(GraphicsPath polygonBase2D, double triangulationResolution, Point3D bottomOrigin, Point3D topOrigin, NormalizedVector3D baseXAxis, NormalizedVector3D baseYAxis, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            baseXAxis = (baseXAxis - baseYAxis * (baseXAxis * baseYAxis)).Normalize();

            List<Element3D> tbr = new List<Element3D>();

            bool orientation = (baseXAxis ^ baseYAxis) * (bottomOrigin - topOrigin) > 0;

            double[,] matrix1 = Matrix3D.RotationToAlignAWithB(new NormalizedVector3D(0, 1, 0), baseYAxis);
            double[,] matrix2 = Matrix3D.RotationToAlignAWithB(((Vector3D)(matrix1 * new Point3D(1, 0, 0))).Normalize(), baseXAxis);

            List<List<NormalizedVector3D>> normals = (from el2 in polygonBase2D.GetLinearisationPointsNormals(triangulationResolution) select (from el in el2 select (el.X * baseXAxis + el.Y * baseYAxis).Normalize()).ToList()).ToList();

            polygonBase2D = polygonBase2D.Linearise(triangulationResolution);

            tbr.AddRange(CreatePolygon(polygonBase2D, triangulationResolution, bottomOrigin, baseXAxis, baseYAxis, orientation, fill, tag, zIndex));
            tbr.AddRange(CreatePolygon(polygonBase2D, triangulationResolution, topOrigin, baseXAxis, baseYAxis, !orientation, fill, tag, zIndex));

            List<List<Point3D>> bottomPoints = (from el2 in polygonBase2D.GetPoints() select (from el in el2 select (Point3D)(el.X * baseXAxis + el.Y * baseYAxis + (Vector3D)bottomOrigin)).ToList()).ToList();
            List<List<Point3D>> topPoints = (from el2 in polygonBase2D.GetPoints() select (from el in el2 select (Point3D)(el.X * baseXAxis + el.Y * baseYAxis + (Vector3D)topOrigin)).ToList()).ToList();

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

        /// <summary>
        /// Creates a wireframe from a collection of <see cref="Element3D"/>s.
        /// </summary>
        /// <param name="object3D">The collection of <see cref="Element3D"/>s. <see cref="Line3DElement"/>s and <see cref="Point3DElement"/>s are ignored.</param>
        /// <param name="colour">The colour of the <see cref="Line3DElement"/>s returned by this method.</param>
        /// <param name="thickness">The thickness of the <see cref="Line3DElement"/>s returned by this method.</param>
        /// <param name="lineCap">The line cap of the <see cref="Line3DElement"/>s returned by this method.</param>
        /// <param name="lineDash">The line dash of the <see cref="Line3DElement"/>s returned by this method.</param>
        /// <param name="tag">A tag that will be applied to the <see cref="Line3DElement"/>s returned by this method.</param>
        /// <param name="zIndex">A z-index that will be applied to the <see cref="Line3DElement"/>s returned by this method.</param>
        /// <returns>A list of <see cref="Line3DElement"/>s that constitute the wireframe.</returns>
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

        /// <summary>
        /// Obtains a list of <see cref="Point3DElement"/> corresponding to the vertices of a list of <see cref="Element3D"/>s.
        /// </summary>
        /// <param name="object3D">The collection of <see cref="Element3D"/>s. <see cref="Point3DElement"/>s are ignored.</param>
        /// <param name="colour">The colour of the <see cref="Point3DElement"/>s returned by this method.</param>
        /// <param name="diameter">The diameter of the <see cref="Point3DElement"/>s returned by this method.</param>
        /// <param name="tag">A tag that will be applied to the <see cref="Point3DElement"/>s returned by this method.</param>
        /// <param name="zIndex">A z-index that will be applied to the <see cref="Point3DElement"/>s returned by this method.</param>
        /// <returns>A list of <see cref="Point3DElement"/>s corresponding to the vertices of the <see cref="Element3D"/>s.</returns>
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
