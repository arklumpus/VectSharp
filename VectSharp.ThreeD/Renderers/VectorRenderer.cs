/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2022 Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Renders a scene to a 2D vector image using the painter's algorithm.
    /// </summary>
    public class VectorRenderer : IRenderer
    {
        /// <summary>
        /// Indicates when should the resampling happen.
        /// </summary>
        public enum ResamplingTimes
        {
            /// <summary>
            /// The resampling should happen before the sorting step.
            /// </summary>
            BeforeSorting, 

            /// <summary>
            /// The resampling should happen after the sorting step.
            /// </summary>
            AfterSorting
        }

        /// <summary>
        /// Determines the maximum area for triangles and maximum length for lines during the resampling step. Setting this to <see cref="double.NaN"/> disables resampling.
        /// </summary>
        public double ResamplingMaxSize { get; set; } = double.NaN;

        /// <summary>
        /// Determines when the resampling happens.
        /// </summary>
        public ResamplingTimes ResamplingTime { get; set; } = ResamplingTimes.AfterSorting;

        /// <summary>
        /// Determines whether lines are resampled alongside triangles or not.
        /// </summary>
        public bool ResampleLines { get; set; } = true;

        /// <summary>
        /// Determines the default overfill value for triangles.
        /// </summary>
        public double DefaultOverFill { get; set; } = 0;

        private object RenderLock = new object();

        /// <summary>
        /// Creates a new <see cref="VectorRenderer"/>.
        /// </summary>
        public VectorRenderer()
        {

        }

        /// <inheritdoc/>
        public Page Render(IScene scene, IEnumerable<ILightSource> lights, Camera camera)
        {
            Page tbr = new Page(1, 1);

            Graphics gpr = tbr.Graphics;

            lock (RenderLock)
                lock (scene.SceneLock)
                {
                    List<Element3D> sceneElements = new List<Element3D>((scene.SceneElements as IList<Element3D>)?.Count ?? 4);

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

                    Dictionary<Element3D, List<Element3D>> allDependencies = new Dictionary<Element3D, List<Element3D>>();

                    foreach (Element3D el in nonCulled)
                    {
                        allDependencies[el] = new List<Element3D>();
                        el.SetProjection(camera);
                    }

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

                    List<double> noObstructions = new List<double>(lightList.Count);
                    for (int i = 0; i < lightList.Count; i++)
                    {
                        noObstructions.Add(0);
                    };

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

                    sbyte[] comparisons = new sbyte[nonCulled.Count * (nonCulled.Count - 1) / 2];

                    Parallel.For(0, comparisons.Length, k =>
                    {
                        int i = (int)Math.Floor((2 * nonCulled.Count - 1 - Math.Sqrt((2 * nonCulled.Count - 1) * (2 * nonCulled.Count - 1) - 8 * k)) / 2);
                        int j = k - i * (nonCulled.Count - 1) + i * (i - 1) / 2 + i + 1;
                        comparisons[k] = (sbyte)camera.Compare(nonCulled[i], nonCulled[j]);
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
                                List<double> triangleObstructions;

                                if (!anyShadows)
                                {
                                    triangleObstructions = noObstructions;
                                }
                                else
                                {
                                    triangleObstructions = new List<double>();

                                    for (int i = 0; i < lightList.Count; i++)
                                    {
                                        if (!lightList[i].CastsShadow)
                                        {
                                            triangleObstructions.Add(0);
                                        }
                                        else
                                        {
                                            triangleObstructions.Add(lightList[i].GetObstruction(triangle.Centroid, from el in shadowers where el != triangle && vectorRendererTriangle.Parent != el select el));
                                        }
                                    }
                                }

                                foreach (IMaterial fill in triangle.Fill)
                                {
                                    FillTriangle(gpr, element.GetProjection(), fill.GetColour(triangle.Centroid, triangle.Normal, camera, lightList, triangleObstructions), vectorRendererTriangle.OverFill, camera.ScaleFactor, triangle.Tag);
                                }
                            }
                        }
                    }

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
                if (this.ResampleLines)
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

    /// <summary>
    /// Represents an extension adding properties to a <see cref="Triangle3DElement"/>.
    /// </summary>
    public interface IVectorRendererTriangle3DElement
    {
        /// <summary>
        /// The amount of overfill that should be applied to the triangle.
        /// </summary>
        double OverFill { get; }

        /// <summary>
        /// If the triangle is the result of resampling, this property holds a reference to the original triangle.
        /// </summary>
        Triangle3DElement Parent { get; }
    }

    /// <summary>
    /// Represents a <see cref="Triangle3DElement"/> extended to implement additional properties.
    /// </summary>
    public class VectorRendererTriangle3DElement : Triangle3DElement, IVectorRendererTriangle3DElement
    {
        /// <inheritdoc/>
        public virtual double OverFill { get; set; } = 0;

        /// <inheritdoc/>
        public virtual Triangle3DElement Parent { get; set; } = null;

        /// <inheritdoc/>
        public VectorRendererTriangle3DElement(Point3D point1, Point3D point2, Point3D point3) : base(point1, point2, point3) { }
        
        /// <inheritdoc/>
        public VectorRendererTriangle3DElement(Point3D point1, Point3D point2, Point3D point3, NormalizedVector3D point1Normal, NormalizedVector3D point2Normal, NormalizedVector3D point3Normal) : base(point1, point2, point3, point1Normal, point2Normal, point3Normal) { }

        /// <summary>
        /// Creates a new <see cref="VectorRendererTriangle3DElement"/> based on the specified base <paramref name="triangle"/>.
        /// </summary>
        /// <param name="triangle">The base <see cref="Triangle3DElement"/> from which all property values will be copied.</param>
        public VectorRendererTriangle3DElement(Triangle3DElement triangle) : this(triangle.Point1, triangle.Point2, triangle.Point3, triangle.Point1Normal, triangle.Point2Normal, triangle.Point3Normal)
        {
            this.CastsShadow = triangle.CastsShadow;
            this.Fill = triangle.Fill;
            this.ReceivesShadow = triangle.ReceivesShadow;
            this.Tag = triangle.Tag;
            this.ZIndex = triangle.ZIndex;
        }
    }

    internal static class TopologicalSorter
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
                // Cyclic dependency. Ignore it and try your best.
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
