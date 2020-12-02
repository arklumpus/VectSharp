using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectSharp.ThreeD
{
    public class VectorRenderer : IRenderer
    {
        public enum ResamplingTimes { BeforeSorting, AfterSorting }
        public double ResamplingMaxSize { get; set; } = double.NaN;
        public ResamplingTimes ResamplingTime { get; set; } = ResamplingTimes.BeforeSorting;

        public double DefaultOverFill { get; set; } = 0;

        private object RenderLock = new object();

        public VectorRenderer()
        {

        }

        public Page Render(IScene scene, IEnumerable<ILightSource> lights, Camera camera)
        {
            Page tbr = new Page(1, 1);

            Graphics gpr = tbr.Graphics;

            lock (RenderLock)
                lock (scene.SceneLock)
                {
                    List<Element3D> sceneElements = new List<Element3D>((scene.SceneElements as IList<Element3D>)?.Count ?? 4);

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    foreach (Element3D element in scene.SceneElements)
                    {
                        if (!(element is Triangle3DElement))
                        {
                            sceneElements.Add(element);
                        }
                        else if (element is IVectorRendererTriangle3DElement)
                        {
                            sceneElements.Add(element);
                        }
                        else if (element is Triangle3DElement triangle)
                        {
                            sceneElements.Add(new VectorRendererTriangle3DElement(triangle) { OverFill = DefaultOverFill });
                        }
                    }

                    List<Element3D> nonCulled = new List<Element3D>();

                    foreach (Element3D element in sceneElements)
                    {
                        if (!camera.IsCulled(element))
                        {
                            if (ResamplingTime == ResamplingTimes.AfterSorting || double.IsNaN(ResamplingMaxSize))
                            {
                                nonCulled.Add(element);
                            }
                            else
                            {
                                nonCulled.AddRange(Resample(camera, element, ResamplingMaxSize));
                            }
                        }
                    }

                    long cullTime = sw.ElapsedMilliseconds;
                    sw.Restart();

                    Dictionary<Element3D, List<Element3D>> allDependencies = new Dictionary<Element3D, List<Element3D>>();

                    foreach (Element3D el in nonCulled)
                    {
                        allDependencies[el] = new List<Element3D>();
                        el.SetProjection(camera);
                    }

                    long projectTime = sw.ElapsedMilliseconds;

                    IList<ILightSource> lightList = lights as IList<ILightSource> ?? lights.ToList();
                    bool anyShadows = false;
                    for (int i = 0; i < lightList.Count; i++)
                    {
                        if (lightList[i].CastsShadow)
                        {
                            anyShadows = true;
                            break;
                        }
                    }

                    List<Triangle3DElement> shadowers = null;

                    if (anyShadows)
                    {
                        shadowers = new List<Triangle3DElement>();

                        foreach (Element3D element in sceneElements)
                        {
                            if (element is Triangle3DElement triangle)
                            {
                                if (triangle.CastsShadow)
                                {
                                    shadowers.Add(triangle);
                                }
                            }
                        }
                    }
                    sw.Restart();

                    int[] comparisons = new int[nonCulled.Count * (nonCulled.Count - 1) / 2];

                    Parallel.For(0, comparisons.Length, k =>
                    {
                        int i = (int)Math.Floor((2 * nonCulled.Count - 1 - Math.Sqrt((2 * nonCulled.Count - 1) * (2 * nonCulled.Count - 1) - 8 * k)) / 2);
                        int j = k - i * (nonCulled.Count - 1) + i * (i - 1) / 2 + i + 1;
                        comparisons[k] = camera.Compare(nonCulled[i], nonCulled[j]);
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

                    List<Element3D> sortedElements = TopologicalSorter.Sort(nonCulled, (element, elements) =>
                    {
                        List<Element3D> dependencies = allDependencies[element];
                        return dependencies;
                    });

                    if (ResamplingTime == ResamplingTimes.AfterSorting && !double.IsNaN(ResamplingMaxSize))
                    {
                        List<Element3D> resampledElements = new List<Element3D>();

                        foreach (Element3D element in sortedElements)
                        {
                            List<Element3D> resampled = Resample(camera, element, ResamplingMaxSize).ToList();
                            resampledElements.AddRange(resampled);
                        }

                        sortedElements = resampledElements;

                        Parallel.For(0, sortedElements.Count, i =>
                        {
                            sortedElements[i].SetProjection(camera);
                        });
                    }

                    long sortTime = sw.ElapsedMilliseconds;
                    sw.Restart();

                    foreach (Element3D element in sortedElements)
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
                            else if (element is Line3DElement line)
                            {
                                GraphicsPath path = new GraphicsPath();

                                foreach (Point pt in element.GetProjection())
                                {
                                    path.LineTo(pt);
                                }

                                gpr.StrokePath(path, line.Colour, line.Thickness, lineCap: line.LineCap, lineDash: line.LineDash, tag: line.Tag);
                            }
                            else if (element is Triangle3DElement triangle && element is IVectorRendererTriangle3DElement vectorRendererTriangle)
                            {
                                IList<ILightSource> triangleLights;

                                if (!anyShadows)
                                {
                                    triangleLights = lightList;
                                }
                                else
                                {
                                    triangleLights = new List<ILightSource>();

                                    foreach (ILightSource light in lightList)
                                    {
                                        if (!light.CastsShadow || !light.IsObstructed(triangle.Centroid, from el in shadowers where el != triangle && vectorRendererTriangle.Parent != el select el))
                                        {
                                            triangleLights.Add(light);
                                        }
                                    }
                                }


                                foreach (IMaterial fill in triangle.Fill)
                                {
                                    FillTriangle(gpr, element.GetProjection(), fill.GetColour(triangle.Centroid, triangle.Normal, camera, triangleLights), vectorRendererTriangle.OverFill, camera.ScaleFactor, triangle.Tag);
                                }
                            }
                        }
                    }

                    long drawTime = sw.ElapsedMilliseconds;

                    sw.Stop();

                    //gpr.FillText(camera.TopLeft, cullTime.ToString() + " / " + projectTime.ToString() + " / " + compareTime.ToString() + " / " + sortTime.ToString() + " / " + drawTime.ToString(), new Font(new FontFamily(FontFamily.StandardFontFamilies.Helvetica), 12 * 10 / camera.ScaleFactor), Colours.Black);

                    tbr.Crop(camera.TopLeft, camera.Size);
                }

            return tbr;
        }

        private void FillTriangle(Graphics graphics, IEnumerable<Point> triangleProjection, Colour colour, double overFill, double scaleFactor, string tag)
        {
            GraphicsPath path = new GraphicsPath();

            if (overFill > 0)
            {
                double overfill = overFill * scaleFactor;

                double meanX = 0;
                double meanY = 0;

                int count = 0;

                foreach (Point pt in triangleProjection)
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

                foreach (Point pt in triangleProjection)
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
                foreach (Point pt in triangleProjection)
                {
                    path.LineTo(pt);
                }
            }

            path.Close();

            graphics.FillPath(path, colour, tag: tag);
        }

        private IEnumerable<Element3D> Resample(Camera camera, Element3D element, double maxSize)
        {
            if (element is Point3DElement)
            {
                yield return element;
            }
            else if (element is Line3DElement line)
            {
                Point p1 = camera.Project(line.Point1);
                Point p2 = camera.Project(line.Point2);

                if ((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) > maxSize)
                {
                    Point3D half = (Point3D)(((Vector3D)line.Point1 + (Vector3D)line.Point2) * 0.5);

                    Line3DElement line1 = new Line3DElement(line.Point1, half) { Colour = line.Colour, LineCap = line.LineCap, LineDash = line.LineDash, Tag = line.Tag, Thickness = line.Thickness, ZIndex = line.ZIndex };
                    Line3DElement line2 = new Line3DElement(half, line.Point2) { Colour = line.Colour, LineCap = line.LineCap, LineDash = line.LineDash, Tag = line.Tag, Thickness = line.Thickness, ZIndex = line.ZIndex };

                    foreach (Element3D el in Resample(camera, line1, maxSize))
                    {
                        yield return el;
                    }

                    foreach (Element3D el in Resample(camera, line2, maxSize))
                    {
                        yield return el;
                    }
                }
                else
                {
                    yield return element;
                }
            }
            else if (element is Triangle3DElement triangle && element is IVectorRendererTriangle3DElement vectorRendererTriangle)
            {
                Point p1 = camera.Project(triangle.Point1);
                Point p2 = camera.Project(triangle.Point2);
                Point p3 = camera.Project(triangle.Point3);

                double area = 0.5 * Math.Abs((p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X));

                if (area > maxSize)
                {
                    Point3D proj12 = (Point3D)((triangle.Point2 - triangle.Point1) * 0.5 + triangle.Point1);
                    Point3D proj23 = (Point3D)((triangle.Point3 - triangle.Point2) * 0.5 + triangle.Point2);
                    Point3D proj31 = (Point3D)((triangle.Point1 - triangle.Point3) * 0.5 + triangle.Point3);

                    NormalizedVector3D proj12Normal = (triangle.Point1Normal * 0.5 + triangle.Point2Normal * 0.5).Normalize();
                    NormalizedVector3D proj23Normal = (triangle.Point2Normal * 0.5 + triangle.Point3Normal * 0.5).Normalize();
                    NormalizedVector3D proj31Normal = (triangle.Point3Normal * 0.5 + triangle.Point1Normal * 0.5).Normalize();

                    VectorRendererTriangle3DElement t1 = new VectorRendererTriangle3DElement(triangle.Point1, proj12, proj31, triangle.Point1Normal, proj12Normal, proj31Normal) { Tag = triangle.Tag, ZIndex = triangle.ZIndex, OverFill = vectorRendererTriangle.OverFill, CastsShadow = triangle.CastsShadow, ReceivesShadow = triangle.ReceivesShadow };
                    t1.Fill.AddRange(triangle.Fill);
                    t1.Parent = vectorRendererTriangle.Parent ?? triangle;
                    VectorRendererTriangle3DElement t2 = new VectorRendererTriangle3DElement(triangle.Point2, proj23, proj12, triangle.Point2Normal, proj23Normal, proj12Normal) { Tag = triangle.Tag, ZIndex = triangle.ZIndex, OverFill = vectorRendererTriangle.OverFill, CastsShadow = triangle.CastsShadow, ReceivesShadow = triangle.ReceivesShadow };
                    t2.Fill.AddRange(triangle.Fill);
                    t2.Parent = vectorRendererTriangle.Parent ?? triangle;
                    VectorRendererTriangle3DElement t3 = new VectorRendererTriangle3DElement(triangle.Point3, proj31, proj23, triangle.Point3Normal, proj31Normal, proj23Normal) { Tag = triangle.Tag, ZIndex = triangle.ZIndex, OverFill = vectorRendererTriangle.OverFill, CastsShadow = triangle.CastsShadow, ReceivesShadow = triangle.ReceivesShadow };
                    t3.Fill.AddRange(triangle.Fill);
                    t3.Parent = vectorRendererTriangle.Parent ?? triangle;
                    VectorRendererTriangle3DElement t4 = new VectorRendererTriangle3DElement(proj12, proj23, proj31, proj12Normal, proj23Normal, proj31Normal) { Tag = triangle.Tag, ZIndex = triangle.ZIndex, OverFill = vectorRendererTriangle.OverFill, CastsShadow = triangle.CastsShadow, ReceivesShadow = triangle.ReceivesShadow };
                    t4.Fill.AddRange(triangle.Fill);
                    t4.Parent = vectorRendererTriangle.Parent ?? triangle;

                    foreach (Element3D el in Resample(camera, t1, maxSize))
                    {
                        yield return el;
                    }

                    foreach (Element3D el in Resample(camera, t2, maxSize))
                    {
                        yield return el;
                    }

                    foreach (Element3D el in Resample(camera, t3, maxSize))
                    {
                        yield return el;
                    }

                    foreach (Element3D el in Resample(camera, t4, maxSize))
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

    public interface IVectorRendererTriangle3DElement
    {
        double OverFill { get; }
        Triangle3DElement Parent { get; }
    }

    public class VectorRendererTriangle3DElement : Triangle3DElement, IVectorRendererTriangle3DElement
    {
        public virtual double OverFill { get; set; } = 0;
        public virtual Triangle3DElement Parent { get; set; } = null;

        public VectorRendererTriangle3DElement(Point3D point1, Point3D point2, Point3D point3) : base(point1, point2, point3) { }
        public VectorRendererTriangle3DElement(Point3D point1, Point3D point2, Point3D point3, NormalizedVector3D point1Normal, NormalizedVector3D point2Normal, NormalizedVector3D point3Normal) : base(point1, point2, point3, point1Normal, point2Normal, point3Normal) { }

        public VectorRendererTriangle3DElement(Triangle3DElement triangle) : this(triangle.Point1, triangle.Point2, triangle.Point3, triangle.Point1Normal, triangle.Point2Normal, triangle.Point3Normal)
        {
            this.CastsShadow = triangle.CastsShadow;
            this.Fill = triangle.Fill;
            this.ReceivesShadow = triangle.ReceivesShadow;
            this.Tag = triangle.Tag;
            this.ZIndex = triangle.ZIndex;
        }
    }
}
