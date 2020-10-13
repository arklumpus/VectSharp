using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace VectSharp.ThreeD
{
    public class ThreeDGraphics
    {
        public static double Tolerance = 1e-4;

        internal List<IElement3D> SceneElements;

        public ThreeDGraphics()
        {
            this.SceneElements = new List<IElement3D>();
        }

        public IElement3D AddPoint(Point3D point, Colour colour, string tag = null, int zIndex = 0)
        {
            Point3DElement el = new Point3DElement(point);
            el.Colour = colour;
            el.Tag = tag;
            el.ZIndex = zIndex;
            this.SceneElements.Add(el);
            return el;
        }

        public List<IElement3D> AddCube(Point3D center, double sizeX, double sizeY, double sizeZ, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            /*this.SceneElements.Add(new Line3D(new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5), new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5)) { Colour = Colours.MediumVioletRed, LineCap = LineCaps.Round });
            this.SceneElements.Add(new Line3D(new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5), new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5)) { Colour = Colours.MediumVioletRed, LineCap = LineCaps.Round });
            this.SceneElements.Add(new Line3D(new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5), new Point3D(center.X - sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5)) { Colour = Colours.MediumVioletRed, LineCap = LineCaps.Round });
            this.SceneElements.Add(new Line3D(new Point3D(center.X - sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5), new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5)) { Colour = Colours.MediumVioletRed, LineCap = LineCaps.Round });

            this.SceneElements.Add(new Line3D(new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5), new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z + sizeZ * 0.5)) { LineCap = LineCaps.Round });
            this.SceneElements.Add(new Line3D(new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z + sizeZ * 0.5), new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5)) { LineCap = LineCaps.Round });
            this.SceneElements.Add(new Line3D(new Point3D(center.X - sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5), new Point3D(center.X - sizeX * 0.5, center.Y + sizeY * 0.5, center.Z + sizeZ * 0.5)) { LineCap = LineCaps.Round });
            this.SceneElements.Add(new Line3D(new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z + sizeZ * 0.5), new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5)) { LineCap = LineCaps.Round });

            this.SceneElements.Add(new Line3D(new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z + sizeZ * 0.5), new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z + sizeZ * 0.5)) { Colour = Colours.Coral, LineCap = LineCaps.Round, LineDash = new LineDash(2, 4, 0) });
            this.SceneElements.Add(new Line3D(new Point3D(center.X - sizeX * 0.5, center.Y + sizeY * 0.5, center.Z + sizeZ * 0.5), new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z + sizeZ * 0.5)) { Colour = Colours.Coral, LineCap = LineCaps.Round, LineDash = new LineDash(2, 4, 0) });
            this.SceneElements.Add(new Line3D(new Point3D(center.X - sizeX * 0.5, center.Y + sizeY * 0.5, center.Z + sizeZ * 0.5), new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z + sizeZ * 0.5)) { Colour = Colours.Coral, LineCap = LineCaps.Round, LineDash = new LineDash(2, 4, 0) });
            this.SceneElements.Add(new Line3D(new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z + sizeZ * 0.5), new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z + sizeZ * 0.5)) { Colour = Colours.Coral, LineCap = LineCaps.Round, LineDash = new LineDash(2, 4, 0) });
            
            this.SceneElements.Add(new Line3D(new Point3D(center.X - sizeX * 0.15, center.Y - sizeY * 0.15, center.Z - sizeZ * 0.55), new Point3D(center.X + sizeX * 0.15, center.Y + sizeY * 0.15, center.Z - sizeZ * 0.55)) { Colour = Colours.CornflowerBlue, LineCap = LineCaps.Round, Thickness = 3 });
            this.SceneElements.Add(new Line3D(new Point3D(center.X + sizeX * 0.15, center.Y - sizeY * 0.15, center.Z - sizeZ * 0.55), new Point3D(center.X - sizeX * 0.15, center.Y + sizeY * 0.15, center.Z - sizeZ * 0.55)) { Colour = Colours.CornflowerBlue, LineCap = LineCaps.Round, Thickness = 3 });
            

            */
            /*this.SceneElements.Add(new Line3D(new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5), new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5)) { Colour = Colours.Bisque, LineCap = LineCaps.Round, Thickness = 0.5 });
            this.SceneElements.Add(new Line3D(new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5), new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5)) { Colour = Colours.Bisque, LineCap = LineCaps.Round, Thickness = 0.5 });
            this.SceneElements.Add(new Line3D(new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5), new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5)) { Colour = Colours.Bisque, LineCap = LineCaps.Round, Thickness = 0.5 });*/

            Point3D A = new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5);
            Point3D B = new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z - sizeZ * 0.5);
            Point3D C = new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5);
            Point3D D = new Point3D(center.X - sizeX * 0.5, center.Y + sizeY * 0.5, center.Z - sizeZ * 0.5);

            Point3D E = new Point3D(center.X - sizeX * 0.5, center.Y - sizeY * 0.5, center.Z + sizeZ * 0.5);
            Point3D F = new Point3D(center.X + sizeX * 0.5, center.Y - sizeY * 0.5, center.Z + sizeZ * 0.5);
            Point3D G = new Point3D(center.X + sizeX * 0.5, center.Y + sizeY * 0.5, center.Z + sizeZ * 0.5);
            Point3D H = new Point3D(center.X - sizeX * 0.5, center.Y + sizeY * 0.5, center.Z + sizeZ * 0.5);


            List<IElement3D> tbr = new List<IElement3D>();

            tbr.AddRange(this.AddRectangle(A, B, C, D, fill));
            tbr.AddRange(this.AddRectangle(H, G, F, E, fill));

            tbr.AddRange(this.AddRectangle(E, F, B, A, fill));
            tbr.AddRange(this.AddRectangle(G, H, D, C, fill));

            tbr.AddRange(this.AddRectangle(A, D, H, E, fill));
            tbr.AddRange(this.AddRectangle(F, G, C, B, fill));

            return tbr;

            //Triangle triangle2 = new Triangle(new Point3D(center.X - sizeX * 0.4, center.Y - sizeY * 0.4, center.Z - sizeZ * 0.4), new Point3D(center.X + sizeX * 0.4, center.Y - sizeY * 0.4, center.Z - sizeZ * 0.4), new Point3D(center.X + sizeX * 0.4, center.Y + sizeY * 0.4, center.Z - sizeZ * 0.4));

            /*Triangle triangle2 = new Triangle(new Point3D(center.X - sizeX * 0.4, center.Y - sizeY * 0.4, center.Z - sizeZ * 0.6), new Point3D(center.X + sizeX * 0.4, center.Y - sizeY * 0.4, center.Z - sizeZ * 0.6), new Point3D(center.X + sizeX * 0.4, center.Y + sizeY * 0.4, center.Z - sizeZ * 0.6));
            triangle2.Fill.Add(new ColourFillProider(Colours.LightSalmon));
            this.SceneElements.Add(triangle2);*/

            /*this.SceneElements.Add(new Point3DElement(new Point3D(0, 0, 0)) { Colour = Colours.Red, Diameter = 2 });

            this.SceneElements.Add(new Point3DElement(new Point3D(center.X + sizeX * 0.15, center.Y - sizeY * 0.15, center.Z - sizeZ * 0.51)) { Colour = Colours.BlueViolet, Diameter = 2 });*/

        }

        public IElement3D[] AddRectangle(Point3D point1, Point3D point2, Point3D point3, Point3D point4, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            Triangle triangle1 = new Triangle(point1, point2, point3);
            triangle1.Fill.AddRange(fill);
            triangle1.Tag = tag;
            triangle1.ZIndex = zIndex;
            this.SceneElements.Add(triangle1);

            Triangle triangle2 = new Triangle(point1, point3, point4);
            triangle2.Fill.AddRange(fill);
            triangle2.Tag = tag;
            triangle2.ZIndex = zIndex;
            this.SceneElements.Add(triangle2);

            return new IElement3D[] { triangle1, triangle2 };
        }


        public IElement3D[] AddRectangle(Point3D point1, Point3D point2, Point3D point3, Point3D point4, NormalizedVector3D point1Normal, NormalizedVector3D point2Normal, NormalizedVector3D point3Normal, NormalizedVector3D point4Normal, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            Triangle triangle1 = new Triangle(point1, point2, point3, point1Normal, point2Normal, point3Normal);
            triangle1.Fill.AddRange(fill);
            triangle1.Tag = tag;
            triangle1.ZIndex = zIndex;
            this.SceneElements.Add(triangle1);

            Triangle triangle2 = new Triangle(point1, point3, point4, point1Normal, point3Normal, point4Normal);
            triangle2.Fill.AddRange(fill);
            triangle2.Tag = tag;
            triangle2.ZIndex = zIndex;
            this.SceneElements.Add(triangle2);

            return new IElement3D[] { triangle1, triangle2 };
        }

        public List<IElement3D> AddSphere(Point3D center, double radius, int steps, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
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

                    //this.SceneElements.Add(new Point3DElement(new Point3D(x, y, z)) { Colour = Colours.Black, Diameter = 0.5, ZIndex = zIndex });

                    if (t == 0 || t == steps)
                    {
                        break;
                    }
                }
            }

            List<IElement3D> tbr = new List<IElement3D>(4 * steps + (points.Count - 2 - 2 * steps) * 2);

            for (int i = 0; i < points.Count - 1; i++)
            {
                if (i == 0)
                {
                    for (int j = 0; j < 2 * steps; j++)
                    {
                        Point3D p1 = points[i];
                        Point3D p2 = points[i + 1 + j];
                        Point3D p3 = points[i + 1 + (j + 1) % (2 * steps)];

                        Triangle tri = new Triangle(p1, p2, p3, (p1 - center).Normalize(), (p2 - center).Normalize(), (p3 - center).Normalize());
                        tri.Fill.AddRange(fill);
                        tri.Tag = tag;
                        tri.ZIndex = zIndex;
                        this.SceneElements.Add(tri);
                        tbr.Add(tri);
                    }
                }
                else if (i >= points.Count - 1 - 2 * steps)
                {
                    Point3D p1 = points[i];
                    Point3D p2 = points[points.Count - 1];
                    Point3D p3 = points[points.Count - 1 - 2 * steps + (i - (points.Count - 1 - 2 * steps) + 1) % (2 * steps)];

                    Triangle tri = new Triangle(p1, p2, p3, (p1 - center).Normalize(), (p2 - center).Normalize(), (p3 - center).Normalize());
                    tri.Fill.AddRange(fill);
                    tri.Tag = tag;
                    tri.ZIndex = zIndex;
                    this.SceneElements.Add(tri);
                    tbr.Add(tri);
                }
                else
                {
                    //this.SceneElements.Add(new Point3DElement(points[i]) { Colour = Colours.Black, Diameter = 0.5, ZIndex = zIndex });

                    //this.SceneElements.Add(new Point3DElement(points[((i - 1) / (2 * steps)) * 2 * steps + (i + 1) % (2 * steps)]) { Colour = Colours.Red, Diameter = 0.5, ZIndex = zIndex });

                    //this.SceneElements.Add(new Point3DElement(points[i + 2 * steps]) { Colour = Colours.Blue, Diameter = 0.5, ZIndex = zIndex });

                    //this.SceneElements.Add(new Point3DElement(points[((i - 1) / (2 * steps)) * 2 * steps + (i + 1) % (2 * steps) + 2 * steps]) { Colour = Colours.Red, Diameter = 0.5, ZIndex = zIndex });

                    if ((i - 1) % (2 * steps) < 2 * steps - 1)
                    {
                        Point3D p1 = points[i + 2 * steps];
                        Point3D p2 = points[i + 2 * steps + 1];
                        Point3D p3 = points[i + 1];
                        Point3D p4 = points[i];

                        tbr.AddRange(AddRectangle(p1, p2, p3, p4, (p1 - center).Normalize(), (p2 - center).Normalize(), (p3 - center).Normalize(), (p4 - center).Normalize(), fill, tag, zIndex));
                    }
                    else
                    {
                        Point3D p1 = points[i + 2 * steps];
                        Point3D p2 = points[(i / (2 * steps)) * 2 * steps + 1];
                        Point3D p3 = points[(i / (2 * steps) - 1) * 2 * steps + 1];
                        Point3D p4 = points[i];

                        tbr.AddRange(AddRectangle(p1, p2, p3, p4, (p1 - center).Normalize(), (p2 - center).Normalize(), (p3 - center).Normalize(), (p4 - center).Normalize(), fill, tag, zIndex));
                    }

                    //AddRectangle(points[i], points[((i - 1) / (2 * steps)) * 2 * steps + (i + 1) % (2 * steps)], points[((i - 1) / (2 * steps)) * 2 * steps + (i + 1) % (2 * steps) + 2 * steps], points[i + 2 * steps], fill, tag, zIndex);
                }
            }

            return tbr;
        }

        public void AddPolygon(GraphicsPath polygon2D, double triangulationResolution, Point3D origin, NormalizedVector3D xAxis, NormalizedVector3D yAxis, IEnumerable<IMaterial> fill, bool reverseTriangles, string tag = null, int zIndex = 0)
        {
            xAxis = (xAxis - yAxis * (xAxis * yAxis)).Normalize();

            List<GraphicsPath> triangles = polygon2D.Triangulate(triangulationResolution, true).ToList();

            double[,] matrix1 = Matrix3D.RotationToAlignAWithB(new NormalizedVector3D(0, 1, 0), yAxis);
            double[,] matrix2 = Matrix3D.RotationToAlignAWithB(((Vector3D)(matrix1 * new Point3D(1, 0, 0))).Normalize(), xAxis);

            for (int i = 0; i < triangles.Count; i++)
            {
                Point p1 = triangles[i].Segments[0].Point;
                Point p2 = triangles[i].Segments[1].Point;
                Point p3 = triangles[i].Segments[2].Point;

                Point3D p13D = matrix2 * (matrix1 * new Point3D(p1.X, p1.Y, 0)) + (Vector3D)origin;
                Point3D p23D = matrix2 * (matrix1 * new Point3D(p2.X, p2.Y, 0)) + (Vector3D)origin;
                Point3D p33D = matrix2 * (matrix1 * new Point3D(p3.X, p3.Y, 0)) + (Vector3D)origin;

                Triangle t = !reverseTriangles ? new Triangle(p13D, p23D, p33D) : new Triangle(p13D, p33D, p23D);
                t.Fill.AddRange(fill);
                t.Tag = tag;
                t.ZIndex = zIndex;
                this.SceneElements.Add(t);
            }
        }

        public void AddPrism(GraphicsPath polygonBase2D, double triangulationResolution, Point3D bottomOrigin, Point3D topOrigin, NormalizedVector3D baseXAxis, NormalizedVector3D baseYAxis, IEnumerable<IMaterial> fill, string tag = null, int zIndex = 0)
        {
            baseXAxis = (baseXAxis - baseYAxis * (baseXAxis * baseYAxis)).Normalize();

            bool orientation = (baseXAxis ^ baseYAxis) * (bottomOrigin - topOrigin) > 0;

            double[,] matrix1 = Matrix3D.RotationToAlignAWithB(new NormalizedVector3D(0, 1, 0), baseYAxis);
            double[,] matrix2 = Matrix3D.RotationToAlignAWithB(((Vector3D)(matrix1 * new Point3D(1, 0, 0))).Normalize(), baseXAxis);
            List<List<NormalizedVector3D>> normals = (from el2 in polygonBase2D.GetLinearisationPointsNormals(triangulationResolution) select (from el in el2 select ((Vector3D)(matrix2 * (matrix1 * new Point3D(el.X, el.Y, 0))) ).Normalize()).ToList()).ToList();

            polygonBase2D = polygonBase2D.Linearise(triangulationResolution);

            AddPolygon(polygonBase2D, triangulationResolution, bottomOrigin, baseXAxis, baseYAxis, fill, orientation, tag, zIndex);
            AddPolygon(polygonBase2D, triangulationResolution, topOrigin, baseXAxis, baseYAxis, fill, !orientation, tag, zIndex);

            List<List<Point3D>> bottomPoints = (from el2 in polygonBase2D.GetPoints() select (from el in el2 select matrix2 * (matrix1 * new Point3D(el.X, el.Y, 0)) + (Vector3D)bottomOrigin).ToList()).ToList();
            List<List<Point3D>> topPoints = (from el2 in polygonBase2D.GetPoints() select (from el in el2 select matrix2 * (matrix1 * new Point3D(el.X, el.Y, 0)) + (Vector3D)topOrigin).ToList()).ToList();


            if (orientation)
            {
                for (int i = 0; i < bottomPoints.Count; i++)
                {
                    for (int j = 0; j < bottomPoints[i].Count - 1; j++)
                    {
                        //AddRectangle(bottomPoints[i][j], bottomPoints[i][j + 1], topPoints[i][j + 1], topPoints[i][j], fill, tag, zIndex);

                        AddRectangle(bottomPoints[i][j], bottomPoints[i][j + 1], topPoints[i][j + 1], topPoints[i][j], normals[i][j], normals[i][j + 1], normals[i][j + 1], normals[i][j], fill, tag, zIndex);

                        /*Line3D line = new Line3D(topPoints[i][j], topPoints[i][j] + (Vector3D)normals[i][j]) { Colour = Colours.Red, ZIndex = 1 };
                        this.SceneElements.Add(line);*/
                    }
                }
            }
            else
            {
                for (int i = 0; i < bottomPoints.Count; i++)
                {
                    for (int j = 0; j < bottomPoints[i].Count - 1; j++)
                    {
                        //AddRectangle(bottomPoints[i][j], topPoints[i][j], topPoints[i][j + 1], bottomPoints[i][j + 1], fill, tag, zIndex);

                        AddRectangle(bottomPoints[i][j], topPoints[i][j], topPoints[i][j + 1], bottomPoints[i][j + 1], normals[i][j], normals[i][j], normals[i][j + 1], normals[i][j + 1], fill, tag, zIndex);

                        /*Line3D line = new Line3D(topPoints[i][j], topPoints[i][j] + (Vector3D)normals[i][j]) { Colour = Colours.Red, ZIndex = 1 };
                        this.SceneElements.Add(line);*/
                    }
                }
            }
        }

        object renderLock = new object();


        public Page Render(ICamera camera, IEnumerable<ILightSource> lightSources, double resamplingMaxSize = double.NaN)
        {
            Page tbr = new Page(1, 1);

            Graphics gpr = tbr.Graphics;

            lock (renderLock)
            {

                /* SceneElements.Sort((a, b) =>
                 {*/
                /*double zA = camera.GetZIndex(a);
                double zB = camera.GetZIndex(b);

                if (zA != zB)
                {
                    return Math.Sign(zB - zA);
                }
                else
                {
                    if ((a is Triangle && (b is Line3D || b is Point3DElement)) || (a is Line3D && b is Point3DElement))
                    {
                        return -1;
                    }
                    else if ((b is Triangle && (a is Line3D || a is Point3DElement)) || (b is Line3D && a is Point3DElement))
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }*/
                /*    return camera.Compare(a, b, gpr);
                });*/

                Stopwatch sw = new Stopwatch();
                sw.Start();


                List<IElement3D> nonCulled = new List<IElement3D>();

                for (int i = 0; i < SceneElements.Count; i++)
                {
                    if (!camera.IsCulled(SceneElements[i]))
                    {
                        if (double.IsNaN(resamplingMaxSize))
                        {
                            nonCulled.Add(SceneElements[i]);
                        }
                        else
                        {
                            nonCulled.AddRange(Resample(camera, SceneElements[i], resamplingMaxSize));
                        }
                    }
                }

                long cullTime = sw.ElapsedMilliseconds;
                sw.Restart();

                Dictionary<IElement3D, List<IElement3D>> allDependencies = new Dictionary<IElement3D, List<IElement3D>>();

                foreach (IElement3D el in nonCulled)
                {
                    allDependencies[el] = new List<IElement3D>();
                    el.SetProjection(camera);
                }

                long projectTime = sw.ElapsedMilliseconds;
                sw.Restart();

                int[] comparisons = new int[nonCulled.Count * (nonCulled.Count - 1) / 2];

                /*for (int i = 0; i < nonCulled.Count; i++)
                {
                    for (int j = i + 1; j < nonCulled.Count; j++)
                    {
                        int k = i * (nonCulled.Count - 1) - i * (i - 1) / 2 + (j - i - 1);
                    }
                }*/

                /*for (int k = 0; k < comparisons.Length; k++)
                {
                    int i = (int)Math.Floor((2 * nonCulled.Count - 1 - Math.Sqrt((2 * nonCulled.Count - 1) * (2 * nonCulled.Count - 1) - 8 * k)) / 2);
                    int j = k - i * (nonCulled.Count - 1) + i * (i - 1) / 2 + i + 1;
                    comparisons[k] = camera.Compare(nonCulled[i], nonCulled[j], gpr);
                }*/

                Parallel.For(0, comparisons.Length, k =>
                {
                    int i = (int)Math.Floor((2 * nonCulled.Count - 1 - Math.Sqrt((2 * nonCulled.Count - 1) * (2 * nonCulled.Count - 1) - 8 * k)) / 2);
                    int j = k - i * (nonCulled.Count - 1) + i * (i - 1) / 2 + i + 1;
                    comparisons[k] = camera.Compare(nonCulled[i], nonCulled[j], gpr);
                });

                for (int i = 0; i < nonCulled.Count; i++)
                {
                    for (int j = i + 1; j < nonCulled.Count; j++)
                    {
                        int k = i * (nonCulled.Count - 1) - i * (i - 1) / 2 + (j - i - 1);
                        int comparison = comparisons[k];

                        if (comparison < 0)
                        {
                            allDependencies[nonCulled[j]].Add(nonCulled[i]);
                        }
                        else if (comparison > 0)
                        {
                            allDependencies[nonCulled[i]].Add(nonCulled[j]);
                        }
                    }
                }

                long compareTime = sw.ElapsedMilliseconds;
                sw.Restart();

                List<IElement3D> sortedElements = TopologicalSorter.Sort(nonCulled, (element, elements) =>
                {
                    /*List<IElement3D> dependencies = new List<IElement3D>();
                    foreach (IElement3D elem in elements)
                    {
                        if (elem != element)
                        {
                            if (camera.Compare(elem, element, gpr) < 0)
                            {
                                dependencies.Add(elem);
                            }
                        }
                    }*/

                    List<IElement3D> dependencies = allDependencies[element];

                    /*string deps = "";

                    if (dependencies.Count > 0)
                    {
                        deps = (from el in dependencies select el.Tag).Aggregate((a, b) => a + ", " + b);
                    }

                    System.Diagnostics.Debug.WriteLine(element.Tag + ": " + deps);
                    */
                    return dependencies;
                });

                long sortTime = sw.ElapsedMilliseconds;
                sw.Restart();

                /* double minX = double.MaxValue;
                 double maxX = double.MinValue;
                 double minY = double.MaxValue;
                 double maxY = double.MinValue;
                */

                //gpr.Rotate(2.9441970937399127 - Math.PI);

                foreach (IElement3D element in sortedElements)
                {
                    if (!camera.IsCulled(element))
                    {
                        if (element is Point3DElement point)
                        {
                            Point pt = element.GetProjection()[0];

                            GraphicsPath path = new GraphicsPath();
                            path.Arc(pt, point.Diameter * 0.5, 0, 2 * Math.PI);

                            gpr.FillPath(path, point.Colour, tag: point.Tag);
                        }
                        else if (element is Line3D line)
                        {
                            GraphicsPath path = new GraphicsPath();

                            foreach (Point pt in element.GetProjection())
                            {
                                path.LineTo(pt);
                            }

                            gpr.StrokePath(path, line.Colour, line.Thickness, lineCap: line.LineCap, lineDash: line.LineDash, tag: line.Tag);
                        }
                        else if (element is Triangle triangle)
                        {
                            foreach (IMaterial fill in triangle.Fill)
                            {
                                fill.Fill(gpr, element.GetProjection(), triangle.Centroid, triangle.Normal, triangle, camera.ViewPoint, camera, lightSources, camera.ScaleFactor, triangle.Tag);
                            }
                        }
                    }
                }

                long drawTime = sw.ElapsedMilliseconds;

                sw.Stop();

                /* double width = maxX - minX;
                 double height = maxY - minY;*/

                gpr.FillText(camera.TopLeft, cullTime.ToString() + " / " + projectTime.ToString() + " / " + compareTime.ToString() + " / " + sortTime.ToString() + " / " + drawTime.ToString(), new Font(new FontFamily(FontFamily.StandardFontFamilies.Helvetica), 12), Colours.Black);

                tbr.Crop(camera.TopLeft, camera.Size);
            }

            return tbr;
        }

        private IEnumerable<IElement3D> Resample(ICamera camera, IElement3D element, double maxSize)
        {
            if (element is Point3DElement)
            {
                yield return element;
            }
            else if (element is Line3D line)
            {
                Point p1 = camera.Project(line.Point1);
                Point p2 = camera.Project(line.Point2);

                if ((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) > maxSize)
                {
                    Point3D half = (Point3D)(((Vector3D)line.Point1 + line.Point2) * 0.5);

                    Line3D line1 = new Line3D(line.Point1, half) { Colour = line.Colour, LineCap = line.LineCap, LineDash = line.LineDash, Tag = line.Tag, Thickness = line.Thickness, ZIndex = line.ZIndex };
                    Line3D line2 = new Line3D(half, line.Point2) { Colour = line.Colour, LineCap = line.LineCap, LineDash = line.LineDash, Tag = line.Tag, Thickness = line.Thickness, ZIndex = line.ZIndex };

                    foreach (IElement3D el in Resample(camera, line1, maxSize))
                    {
                        yield return el;
                    }

                    foreach (IElement3D el in Resample(camera, line2, maxSize))
                    {
                        yield return el;
                    }
                }
                else
                {
                    yield return element;
                }
            }
            else if (element is Triangle triangle)
            {
                Point p1 = camera.Project(triangle.Point1);
                Point p2 = camera.Project(triangle.Point2);
                Point p3 = camera.Project(triangle.Point3);

                double area = 0.5 * Math.Abs((p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X));

                if (area > maxSize)
                {
                    /*double prod12 = (triangle.Centroid - triangle.Point1) * (triangle.Point2 - triangle.Point1);
                    double prod23 = (triangle.Centroid - triangle.Point2) * (triangle.Point3 - triangle.Point2);
                    double prod31 = (triangle.Centroid - triangle.Point3) * (triangle.Point1 - triangle.Point3);

                    prod12 /= (triangle.Point2 - triangle.Point1).Modulus;
                    prod23 /= (triangle.Point3 - triangle.Point2).Modulus;
                    prod31 /= (triangle.Point1 - triangle.Point3).Modulus;

                    Point3D proj12 = (Point3D)((triangle.Point2 - triangle.Point1) * prod12 + triangle.Point1);
                    Point3D proj23 = (Point3D)((triangle.Point3 - triangle.Point2) * prod23 + triangle.Point2);
                    Point3D proj31 = (Point3D)((triangle.Point1 - triangle.Point3) * prod31 + triangle.Point3);

                    NormalizedVector3D proj12Normal = (triangle.Point1Normal * (1 - prod12) + triangle.Point2Normal * prod12).Normalize();
                    NormalizedVector3D proj23Normal = (triangle.Point2Normal * (1 - prod23) + triangle.Point3Normal * prod23).Normalize();
                    NormalizedVector3D proj31Normal = (triangle.Point3Normal * (1 - prod31) + triangle.Point1Normal * prod31).Normalize();*/

                    Point3D proj12 = (Point3D)((triangle.Point2 - triangle.Point1) * 0.5 + triangle.Point1);
                    Point3D proj23 = (Point3D)((triangle.Point3 - triangle.Point2) * 0.5 + triangle.Point2);
                    Point3D proj31 = (Point3D)((triangle.Point1 - triangle.Point3) * 0.5 + triangle.Point3);

                    NormalizedVector3D proj12Normal = (triangle.Point1Normal * 0.5 + triangle.Point2Normal * 0.5).Normalize();
                    NormalizedVector3D proj23Normal = (triangle.Point2Normal * 0.5 + triangle.Point3Normal * 0.5).Normalize();
                    NormalizedVector3D proj31Normal = (triangle.Point3Normal * 0.5 + triangle.Point1Normal * 0.5).Normalize();

                    Triangle t1 = new Triangle(triangle.Point1, proj12, proj31, triangle.Point1Normal, proj12Normal, proj31Normal) { Tag = triangle.Tag, ZIndex = triangle.ZIndex };
                    t1.Fill.AddRange(triangle.Fill);
                    Triangle t2 = new Triangle(triangle.Point2, proj23, proj12, triangle.Point2Normal, proj23Normal, proj12Normal) { Tag = triangle.Tag, ZIndex = triangle.ZIndex };
                    t2.Fill.AddRange(triangle.Fill);
                    Triangle t3 = new Triangle(triangle.Point3, proj31, proj23, triangle.Point3Normal, proj31Normal, proj23Normal) { Tag = triangle.Tag, ZIndex = triangle.ZIndex };
                    t3.Fill.AddRange(triangle.Fill);
                    Triangle t4 = new Triangle(proj12, proj23, proj31, proj12Normal, proj23Normal, proj31Normal) { Tag = triangle.Tag, ZIndex = triangle.ZIndex };
                    t4.Fill.AddRange(triangle.Fill);

                    foreach (IElement3D el in Resample(camera, t1, maxSize))
                    {
                        yield return el;
                    }

                    foreach (IElement3D el in Resample(camera, t2, maxSize))
                    {
                        yield return el;
                    }

                    foreach (IElement3D el in Resample(camera, t3, maxSize))
                    {
                        yield return el;
                    }

                    foreach (IElement3D el in Resample(camera, t4, maxSize))
                    {
                        yield return el;
                    }
                }
                else
                {
                    yield return element;
                }
            }
        }
    }
}
