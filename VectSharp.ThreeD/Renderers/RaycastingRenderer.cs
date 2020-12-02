using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VectSharp.ThreeD
{
    public class RaycastingRenderer : IRenderer
    {
        public enum AntiAliasings
        {
            None, Bilinear4X
        }

        private object RenderLock { get; } = new object();

        public int RenderWidth { get; }
        public int RenderHeight { get; }

        public RasterImage RenderedImage { get; private set; }

        private DisposableIntPtr renderedImageData;
        public DisposableIntPtr RenderedImageData => renderedImageData;

        public double Tolerance { get; set; } = 1e-4;

        private double[] ZBuffer { get; }
        private int[] ZIndexBuffer { get; }

        public bool InterpolateImage { get; set; } = true;

        public AntiAliasings AntiAliasing { get; set; } = AntiAliasings.None;



        public RaycastingRenderer(int renderWidth, int renderHeight)
        {
            this.RenderWidth = renderWidth;
            this.RenderHeight = renderHeight;

            IntPtr imageData = Marshal.AllocHGlobal(this.RenderWidth * this.RenderHeight * 4);

            renderedImageData = new DisposableIntPtr(imageData);

            this.ZBuffer = new double[this.RenderWidth * this.RenderHeight];
            this.ZIndexBuffer = new int[this.RenderWidth * this.RenderHeight];

            this.RenderedImage = new RasterImage(ref this.renderedImageData, this.RenderWidth, this.RenderHeight, true, this.InterpolateImage);
        }

        public Page Render(IScene scene, IEnumerable<ILightSource> lights, Camera camera)
        {
            Page pag = new Page(RenderWidth, RenderHeight);

            lock (RenderLock)
            {
                IEnumerable<Element3D> sceneElements = scene.SceneElements;

                Stopwatch sw = new Stopwatch();
                sw.Start();

                List<Element3D> nonCulled = new List<Element3D>();

                List<ILightSource> lightList = lights as List<ILightSource> ?? lights.ToList();
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

                foreach (Element3D element in sceneElements)
                {
                    if (!camera.IsCulled(element))
                    {
                        nonCulled.Add(element);
                    }
                }

                long cullTime = sw.ElapsedMilliseconds;
                sw.Restart();

                //Dictionary<Element3D, List<Element3D>> allDependencies = new Dictionary<Element3D, List<Element3D>>();

                foreach (Element3D el in nonCulled)
                {
                    //allDependencies[el] = new List<Element3D>();
                    el.SetProjection(camera);
                }

                long projectTime = sw.ElapsedMilliseconds;
                sw.Restart();

                long compareTime = sw.ElapsedMilliseconds;
                sw.Restart();

                /*List<Element3D> sortedElements = TopologicalSorter.Sort(nonCulled, (element, elements) =>
                {
                    List<Element3D> dependencies = allDependencies[element];
                    return dependencies;
                });*/

                long sortTime = sw.ElapsedMilliseconds;
                sw.Restart();

                unsafe
                {
                    byte* imageData = (byte*)this.renderedImageData.InternalPointer;

                    for (int i = 0; i < this.RenderWidth * this.RenderHeight * 4; i++)
                    {
                        imageData[i] = 0;
                    }

                    for (int i = 0; i < ZBuffer.Length; i++)
                    {
                        ZBuffer[i] = double.MaxValue;
                        ZIndexBuffer[i] = int.MinValue;
                    }

                    int totalPixels = this.RenderWidth * this.RenderHeight;

                    Parallel.For(0, totalPixels, i =>
                    {
                        double x = i % this.RenderWidth;
                        double y = i / this.RenderWidth;

                        //(triangle2D[i].X - camera.TopLeft.X) / camera.Size.Width * this.RenderWidth, (triangle2D[i].Y - camera.TopLeft.Y) / camera.Size.Height * this.RenderHeight

                        if (this.AntiAliasing == AntiAliasings.None)
                        {
                            Point point = new Point((x + 0.5) / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (y + 0.5) / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y);
                            (byte R, byte G, byte B, byte A) = GetPixelColor(point, nonCulled, camera, lightList, shadowers);

                            imageData[i * 4] = R;
                            imageData[i * 4 + 1] = G;
                            imageData[i * 4 + 2] = B;
                            imageData[i * 4 + 3] = A;
                        }
                        else if (this.AntiAliasing == AntiAliasings.Bilinear4X)
                        {
                            Point p1 = new Point((x + 0.3688) / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (y + 0.11177) / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y);
                            Point p2 = new Point((x + 0.8889) / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (y + 0.37069) / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y);
                            Point p3 = new Point((x + 0.62998) / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (y + 0.89079) / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y);
                            Point p4 = new Point((x + 0.10988) / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (y + 0.63187) / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y);

                            (byte R1, byte G1, byte B1, byte A1) = GetPixelColor(p1, nonCulled, camera, lightList, shadowers);
                            (byte R2, byte G2, byte B2, byte A2) = GetPixelColor(p2, nonCulled, camera, lightList, shadowers);
                            (byte R3, byte G3, byte B3, byte A3) = GetPixelColor(p3, nonCulled, camera, lightList, shadowers);
                            (byte R4, byte G4, byte B4, byte A4) = GetPixelColor(p4, nonCulled, camera, lightList, shadowers);



                            int totA = (A1 + A2 + A3 + A4);

                            if (totA > 0)
                            {
                                imageData[i * 4] = (byte)((R1 * A1 + R2 * A2 + R3 * A3 + R4 * A4) / totA);
                                imageData[i * 4 + 1] = (byte)((G1 * A1 + G2 * A2 + G3 * A3 + G4 * A4) / totA);
                                imageData[i * 4 + 2] = (byte)((B1 * A1 + B2 * A2 + B3 * A3 + B4 * A4) / totA);
                                imageData[i * 4 + 3] = (byte)((A1 + A2 + A3 + A4) / 4);
                            }
                        }
                    });
                }

                long drawTime = sw.ElapsedMilliseconds;

                sw.Stop();

                this.RenderedImage = new RasterImage(ref this.renderedImageData, this.RenderWidth, this.RenderHeight, true, this.InterpolateImage);

                pag.Graphics.DrawRasterImage(0, 0, RenderWidth, RenderHeight, this.RenderedImage);
            }

            return pag;
        }

        private (byte R, byte G, byte B, byte A) GetPixelColor(Point point, List<Element3D> elements, Camera camera, List<ILightSource> lights, List<Triangle3DElement> shadowers)
        {
            List<(Element3D element, double z, Point3D correspPoint)> hits = new List<(Element3D element, double z, Point3D correspPoint)>();

            foreach (Element3D element in elements)
            {
                if (element is Triangle3DElement triangle)
                {
                    Point[] projection = triangle.GetProjection();

                    if (Intersections2D.PointInTriangle(point, projection[0], projection[1], projection[2]))
                    {
                        Point3D correspPoint = camera.Deproject(point, triangle);

                        double z = camera.ZDepth(correspPoint);

                        hits.Add((triangle, z, correspPoint));
                    }
                }
                else if (element is Point3DElement pointElement)
                {
                    Point projection = pointElement.GetProjection()[0];

                    if ((point.X - projection.X) * (point.X - projection.X) + (point.Y - projection.Y) * (point.Y - projection.Y) <= pointElement.Diameter * pointElement.Diameter * 0.25)
                    {
                        double z = camera.ZDepth(pointElement.Point);

                        hits.Add((pointElement, z, pointElement.Point));
                    }
                }
                else if (element is Line3DElement line)
                {
                    Point[] line2D = line.GetProjection();

                    double lineLengthSq = (line2D[1].X - line2D[0].X) * (line2D[1].X - line2D[0].X) + (line2D[1].Y - line2D[0].Y) * (line2D[1].Y - line2D[0].Y);
                    double lineLength = Math.Sqrt(lineLengthSq);
                    double addedTerm = line2D[1].X * line2D[0].Y - line2D[1].Y * line2D[0].X;
                    double dy = line2D[1].Y - line2D[0].Y;
                    double dx = line2D[1].X - line2D[0].X;

                    double unitsOn = line.LineDash.UnitsOn / lineLength;
                    double unitsOff = line.LineDash.UnitsOff / lineLength;
                    double phase = line.LineDash.Phase / lineLength;

                    double dist = Math.Abs(dy * point.X - dx * point.Y + addedTerm) / lineLength;

                    double thickness = line.Thickness * 0.5;

                    if (dist < thickness)
                    {
                        (double t, Point pointOnLine) = Intersections2D.ProjectOnSegment(point, line2D[0], line2D[1]);

                        bool isIn = false;

                        if (line.LineCap == LineCaps.Butt)
                        {
                            if (t >= 0 && t <= 1)
                            {
                                isIn = true;
                            }
                        }
                        else if (line.LineCap == LineCaps.Square)
                        {
                            if (t >= -thickness / lineLength && t <= 1 + thickness / lineLength)
                            {
                                isIn = true;
                            }
                        }
                        else if (line.LineCap == LineCaps.Round)
                        {
                            if (t >= 0 && t <= 1)
                            {
                                isIn = true;
                            }
                            else if (t < -thickness / lineLength || t > 1 + thickness / lineLength)
                            {
                                isIn = false;
                            }
                            else if (t < 0)
                            {
                                double tipDist = (point.X - line2D[0].X) * (point.X - line2D[0].X) + (point.Y - line2D[0].Y) * (point.Y - line2D[0].Y);

                                if (tipDist <= thickness * thickness)
                                {
                                    isIn = true;
                                }
                            }
                            else if (t > 1)
                            {
                                double tipDist = (point.X - line2D[1].X) * (point.X - line2D[1].X) + (point.Y - line2D[1].Y) * (point.Y - line2D[1].Y);

                                if (tipDist <= thickness * thickness)
                                {
                                    isIn = true;
                                }
                            }
                        }

                        if (isIn && IsDashOn(unitsOn, unitsOff, phase, t))
                        {
                            Point3D correspPoint = camera.Deproject(point, line);

                            double z = camera.ZDepth(correspPoint);

                            hits.Add((line, z, correspPoint));
                        }
                    }
                }
            }

            hits.Sort((a, b) => Math.Sign(a.z - b.z));

            byte pixelR = 0;
            byte pixelG = 0;
            byte pixelB = 0;
            byte pixelA = 0;

            foreach ((Element3D element, double z, Point3D correspPoint) hit in hits)
            {
                byte R = 0;
                byte G = 0;
                byte B = 0;
                byte A = 0;

                if (hit.element is Triangle3DElement triangle)
                {
                    if (!triangle.ReceivesShadow || shadowers == null)
                    {
                        (R, G, B, A) = GetPixelColor(triangle, hit.correspPoint, camera, lights);
                    }
                    else
                    {
                        (R, G, B, A) = GetPixelColorWithShadow(triangle, lights, shadowers, hit.correspPoint, camera);
                    }
                }
                else if (hit.element is Point3DElement pointElement)
                {
                    R = (byte)(pointElement.Colour.R * 255);
                    G = (byte)(pointElement.Colour.G * 255);
                    B = (byte)(pointElement.Colour.B * 255);
                    A = (byte)(pointElement.Colour.A * 255);
                }
                else if (hit.element is Line3DElement lineElement)
                {
                    R = (byte)(lineElement.Colour.R * 255);
                    G = (byte)(lineElement.Colour.G * 255);
                    B = (byte)(lineElement.Colour.B * 255);
                    A = (byte)(lineElement.Colour.A * 255);
                }

                BlendBack(R, G, B, A, ref pixelR, ref pixelG, ref pixelB, ref pixelA);

                if (pixelA == 255)
                {
                    break;
                }
            }

            return (pixelR, pixelG, pixelB, pixelA);
        }

        private bool IsDashOn(double unitsOn, double unitsOff, double phase, double t)
        {
            if (unitsOff == 0)
            {
                return true;
            }
            else
            {
                t += phase;
                t = t % (unitsOn + unitsOff);
                while (t < 0)
                {
                    t += unitsOn + unitsOff;
                }

                return t <= unitsOn;
            }
        }


        private unsafe void FillTriangle(byte* imageData, Triangle3DElement triangle, Camera camera, List<ILightSource> lights)
        {
            Point[] triangle2D = triangle.GetProjection();

            int minX = int.MaxValue;
            int minY = int.MaxValue;

            int maxX = int.MinValue;
            int maxY = int.MinValue;

            for (int i = 0; i < triangle2D.Length; i++)
            {
                triangle2D[i] = new Point((triangle2D[i].X - camera.TopLeft.X) / camera.Size.Width * this.RenderWidth, (triangle2D[i].Y - camera.TopLeft.Y) / camera.Size.Height * this.RenderHeight);

                minX = Math.Min(minX, (int)triangle2D[i].X);
                minY = Math.Min(minY, (int)triangle2D[i].Y);

                maxX = Math.Max(maxX, (int)Math.Ceiling(triangle2D[i].X));
                maxY = Math.Max(maxY, (int)Math.Ceiling(triangle2D[i].Y));
            }

            minX = Math.Max(minX, 0);
            minY = Math.Max(minY, 0);

            maxX = Math.Min(maxX, this.RenderWidth - 1);
            maxY = Math.Min(maxY, this.RenderHeight - 1);

            int totalPixels = (maxX - minX + 1) * (maxY - minY + 1);

            Parallel.For(0, totalPixels, index =>
            {
                int y = index / (maxX - minX + 1) + minY;
                int x = index % (maxX - minX + 1) + minX;

                if (Intersections2D.PointInTriangle(x, y, triangle2D[0], triangle2D[1], triangle2D[2]))
                {
                    Point3D correspPoint = camera.Deproject(new Point((double)x / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (double)y / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y), triangle);

                    double zDepth = camera.ZDepth(correspPoint);

                    int prevZIndexBuffer = ZIndexBuffer[y * RenderWidth + x];

                    if (prevZIndexBuffer < triangle.ZIndex || (prevZIndexBuffer == triangle.ZIndex && ZBuffer[y * RenderWidth + x] > zDepth))
                    {
                        byte R = 0;
                        byte G = 0;
                        byte B = 0;
                        byte A = 0;

                        for (int i = 0; i < triangle.Fill.Count; i++)
                        {
                            Colour col = triangle.Fill[i].GetColour(correspPoint, triangle.GetNormalAt(correspPoint), camera, lights);

                            if (col.A == 1)
                            {
                                R = (byte)(col.R * 255);
                                G = (byte)(col.G * 255);
                                B = (byte)(col.B * 255);
                                A = (byte)(col.A * 255);
                            }
                            else
                            {
                                BlendFront(ref R, ref G, ref B, ref A, (byte)(col.R * 255), (byte)(col.G * 255), (byte)(col.B * 255), (byte)(col.A * 255));
                            }
                        }

                        if (A == 255)
                        {
                            imageData[y * RenderWidth * 4 + x * 4] = R;
                            imageData[y * RenderWidth * 4 + x * 4 + 1] = G;
                            imageData[y * RenderWidth * 4 + x * 4 + 2] = B;
                            imageData[y * RenderWidth * 4 + x * 4 + 3] = A;
                        }
                        else
                        {
                            BlendFront(ref imageData[y * RenderWidth * 4 + x * 4], ref imageData[y * RenderWidth * 4 + x * 4 + 1], ref imageData[y * RenderWidth * 4 + x * 4 + 2], ref imageData[y * RenderWidth * 4 + x * 4 + 3], R, G, B, A);
                        }

                        ZBuffer[y * RenderWidth + x] = zDepth;
                        ZIndexBuffer[y * RenderWidth + x] = triangle.ZIndex;
                    }
                    else if (imageData[y * RenderWidth * 4 + x * 4 + 3] < 255)
                    {
                        byte R = 0;
                        byte G = 0;
                        byte B = 0;
                        byte A = 0;

                        for (int i = 0; i < triangle.Fill.Count; i++)
                        {
                            Colour col = triangle.Fill[i].GetColour(correspPoint, triangle.GetNormalAt(correspPoint), camera, lights);

                            if (col.A == 1)
                            {
                                R = (byte)(col.R * 255);
                                G = (byte)(col.G * 255);
                                B = (byte)(col.B * 255);
                                A = (byte)(col.A * 255);
                            }
                            else
                            {
                                BlendFront(ref R, ref G, ref B, ref A, (byte)(col.R * 255), (byte)(col.G * 255), (byte)(col.B * 255), (byte)(col.A * 255));
                            }
                        }

                        BlendBack(R, G, B, A, ref imageData[y * RenderWidth * 4 + x * 4], ref imageData[y * RenderWidth * 4 + x * 4 + 1], ref imageData[y * RenderWidth * 4 + x * 4 + 2], ref imageData[y * RenderWidth * 4 + x * 4 + 3]);
                    }
                }
            });
        }

        private unsafe void DrawPoint(byte* imageData, Point3DElement point, Camera camera)
        {
            Point point2D = point.GetProjection()[0];

            point2D = new Point((point2D.X - camera.TopLeft.X) / camera.Size.Width * this.RenderWidth, (point2D.Y - camera.TopLeft.Y) / camera.Size.Height * this.RenderHeight);

            double radiusX = point.Diameter * 0.5 / camera.Size.Width * this.RenderWidth;
            double radiusY = point.Diameter * 0.5 / camera.Size.Height * this.RenderHeight;

            double radiusSquare = radiusX * radiusY;
            double radius = Math.Sqrt(radiusSquare);

            int minX = (int)Math.Floor(point2D.X - radiusX);
            int minY = (int)Math.Floor(point2D.Y - radiusY);

            int maxX = (int)Math.Ceiling(point2D.X + radiusX);
            int maxY = (int)Math.Ceiling(point2D.Y + radiusY);

            minX = Math.Max(minX, 0);
            minY = Math.Max(minY, 0);

            maxX = Math.Min(maxX, this.RenderWidth - 1);
            maxY = Math.Min(maxY, this.RenderHeight - 1);

            int totalPixels = (maxX - minX + 1) * (maxY - minY + 1);

            double zDepth = camera.ZDepth(point.Point);

            Parallel.For(0, totalPixels, index =>
            {
                int y = index / (maxX - minX + 1) + minY;
                int x = index % (maxX - minX + 1) + minX;

                double dist = (x - point2D.X) * (x - point2D.X) + (y - point2D.Y) * (y - point2D.Y);

                double howMuch = dist <= radiusSquare ? 1 : Math.Max(0, 1 - (Math.Sqrt(dist) - radius));

                if (howMuch > 0)
                {
                    int prevZIndexBuffer = ZIndexBuffer[y * RenderWidth + x];

                    if (prevZIndexBuffer < point.ZIndex || (prevZIndexBuffer == point.ZIndex && ZBuffer[y * RenderWidth + x] >= zDepth))
                    {
                        byte R = (byte)(point.Colour.R * 255);
                        byte G = (byte)(point.Colour.G * 255);
                        byte B = (byte)(point.Colour.B * 255);
                        byte A = (byte)(point.Colour.A * 255 * howMuch);

                        if (A == 255)
                        {
                            imageData[y * RenderWidth * 4 + x * 4] = R;
                            imageData[y * RenderWidth * 4 + x * 4 + 1] = G;
                            imageData[y * RenderWidth * 4 + x * 4 + 2] = B;
                            imageData[y * RenderWidth * 4 + x * 4 + 3] = A;
                        }
                        else
                        {
                            BlendFront(ref imageData[y * RenderWidth * 4 + x * 4], ref imageData[y * RenderWidth * 4 + x * 4 + 1], ref imageData[y * RenderWidth * 4 + x * 4 + 2], ref imageData[y * RenderWidth * 4 + x * 4 + 3], R, G, B, A);
                        }

                        ZBuffer[y * RenderWidth + x] = zDepth;
                        ZIndexBuffer[y * RenderWidth + x] = point.ZIndex;
                    }
                    else if (imageData[y * RenderWidth * 4 + x * 4 + 3] < 255)
                    {
                        byte R = (byte)(point.Colour.R * 255);
                        byte G = (byte)(point.Colour.G * 255);
                        byte B = (byte)(point.Colour.B * 255);
                        byte A = (byte)(point.Colour.A * 255 * howMuch);

                        BlendBack(R, G, B, A, ref imageData[y * RenderWidth * 4 + x * 4], ref imageData[y * RenderWidth * 4 + x * 4 + 1], ref imageData[y * RenderWidth * 4 + x * 4 + 2], ref imageData[y * RenderWidth * 4 + x * 4 + 3]);
                    }
                }
            });
        }

        private unsafe void DrawLine(byte* imageData, Line3DElement line, Camera camera)
        {
            Point[] line2D = line.GetProjection();

            int minX = int.MaxValue;
            int minY = int.MaxValue;

            int maxX = int.MinValue;
            int maxY = int.MinValue;

            for (int i = 0; i < line2D.Length; i++)
            {
                line2D[i] = new Point((line2D[i].X - camera.TopLeft.X) / camera.Size.Width * this.RenderWidth, (line2D[i].Y - camera.TopLeft.Y) / camera.Size.Height * this.RenderHeight);

                minX = Math.Min(minX, (int)line2D[i].X);
                minY = Math.Min(minY, (int)line2D[i].Y);

                maxX = Math.Max(maxX, (int)Math.Ceiling(line2D[i].X));
                maxY = Math.Max(maxY, (int)Math.Ceiling(line2D[i].Y));
            }



            double thicknessX = line.Thickness * 0.5 / camera.Size.Width * this.RenderWidth;
            double thicknessY = line.Thickness * 0.5 / camera.Size.Height * this.RenderHeight;
            double thicknessSquare = thicknessY * thicknessX;
            double thickness = Math.Sqrt(thicknessSquare);

            minX = (int)Math.Floor(minX - thicknessX);
            minY = (int)Math.Floor(minY - thicknessY);
            maxX = (int)Math.Ceiling(maxX + thicknessX);
            maxY = (int)Math.Ceiling(maxY + thicknessY);


            minX = Math.Max(minX, 0);
            minY = Math.Max(minY, 0);

            maxX = Math.Min(maxX, this.RenderWidth - 1);
            maxY = Math.Min(maxY, this.RenderHeight - 1);

            int totalPixels = (maxX - minX + 1) * (maxY - minY + 1);

            double lineLengthSq = (line2D[1].X - line2D[0].X) * (line2D[1].X - line2D[0].X) + (line2D[1].Y - line2D[0].Y) * (line2D[1].Y - line2D[0].Y);
            double lineLength = Math.Sqrt(lineLengthSq);
            double addedTerm = line2D[1].X * line2D[0].Y - line2D[1].Y * line2D[0].X;
            double dy = line2D[1].Y - line2D[0].Y;
            double dx = line2D[1].X - line2D[0].X;

            double unitsOn = (line.LineDash.UnitsOn / line.Thickness * thickness * 2) / lineLength;
            double unitsOff = (line.LineDash.UnitsOff / line.Thickness * thickness * 2) / lineLength;
            double phase = (line.LineDash.Phase / line.Thickness * thickness * 2) / lineLength;

            bool dashOn(double t)
            {
                if (unitsOff == 0)
                {
                    return true;
                }
                else
                {
                    t += phase;
                    t = t % (unitsOn + unitsOff);
                    while (t < 0)
                    {
                        t += unitsOn + unitsOff;
                    }

                    return t <= unitsOn;
                }
            };

            Parallel.For(0, totalPixels, index =>
            {
                int y = index / (maxX - minX + 1) + minY;
                int x = index % (maxX - minX + 1) + minX;

                double dist = Math.Abs(dy * x - dx * y + addedTerm) / lineLength;

                double howMuch = dist <= thickness ? 1 : Math.Max(0, 1 - (dist - thickness));

                if (howMuch > 0)
                {
                    (double t, Point pointOnLine) = Intersections2D.ProjectOnSegment(x, y, line2D[0], line2D[1]);

                    if (line.LineCap == LineCaps.Butt)
                    {
                        if (t < 0)
                        {
                            howMuch *= Math.Max(0, 1 + t * lineLength);
                        }
                        else if (t > 1)
                        {
                            howMuch *= Math.Max(0, 1 - (t - 1) * lineLength);
                        }
                    }
                    else if (line.LineCap == LineCaps.Square)
                    {
                        if (t < -thickness / lineLength)
                        {
                            howMuch *= Math.Max(0, 1 + (t + thickness / lineLength) * lineLength);
                        }
                        else if (t > 1 + thickness / lineLength)
                        {
                            howMuch *= Math.Max(0, 1 - (t - thickness / lineLength - 1) * lineLength);
                        }
                    }
                    else if (line.LineCap == LineCaps.Round)
                    {
                        if (t < -(thickness + 1) / lineLength || t > 1 + (thickness + 1) / lineLength)
                        {
                            howMuch = 0;
                        }
                        else if (t < 0)
                        {
                            double tipDist = (x - line2D[0].X) * (x - line2D[0].X) + (y - line2D[0].Y) * (y - line2D[0].Y);
                            howMuch *= tipDist <= thicknessSquare ? 1 : Math.Max(0, 1 - (Math.Sqrt(tipDist) - thickness));
                        }
                        else if (t > 1)
                        {
                            double tipDist = (x - line2D[1].X) * (x - line2D[1].X) + (y - line2D[1].Y) * (y - line2D[1].Y);
                            howMuch *= tipDist <= thicknessSquare ? 1 : Math.Max(0, 1 - (Math.Sqrt(tipDist) - thickness));
                        }
                    }

                    if (howMuch > 0 && dashOn(t))
                    {
                        Point3D correspPoint = camera.Deproject(new Point((double)pointOnLine.X / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (double)pointOnLine.Y / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y), line);

                        double zDepth = camera.ZDepth(correspPoint);

                        int prevZIndexBuffer = ZIndexBuffer[y * RenderWidth + x];

                        if (prevZIndexBuffer < line.ZIndex || (prevZIndexBuffer == line.ZIndex && ZBuffer[y * RenderWidth + x] >= zDepth))
                        {
                            byte R = (byte)(line.Colour.R * 255);
                            byte G = (byte)(line.Colour.G * 255);
                            byte B = (byte)(line.Colour.B * 255);
                            byte A = (byte)(line.Colour.A * 255 * howMuch);

                            if (A == 255)
                            {
                                imageData[y * RenderWidth * 4 + x * 4] = R;
                                imageData[y * RenderWidth * 4 + x * 4 + 1] = G;
                                imageData[y * RenderWidth * 4 + x * 4 + 2] = B;
                                imageData[y * RenderWidth * 4 + x * 4 + 3] = A;
                            }
                            else
                            {
                                BlendFront(ref imageData[y * RenderWidth * 4 + x * 4], ref imageData[y * RenderWidth * 4 + x * 4 + 1], ref imageData[y * RenderWidth * 4 + x * 4 + 2], ref imageData[y * RenderWidth * 4 + x * 4 + 3], R, G, B, A);
                            }

                            ZBuffer[y * RenderWidth + x] = zDepth;
                            ZIndexBuffer[y * RenderWidth + x] = line.ZIndex;
                        }
                        else if (imageData[y * RenderWidth * 4 + x * 4 + 3] < 255)
                        {
                            byte R = (byte)(line.Colour.R * 255);
                            byte G = (byte)(line.Colour.G * 255);
                            byte B = (byte)(line.Colour.B * 255);
                            byte A = (byte)(line.Colour.A * 255 * howMuch);

                            BlendBack(R, G, B, A, ref imageData[y * RenderWidth * 4 + x * 4], ref imageData[y * RenderWidth * 4 + x * 4 + 1], ref imageData[y * RenderWidth * 4 + x * 4 + 2], ref imageData[y * RenderWidth * 4 + x * 4 + 3]);
                        }
                    }
                }
            });
        }



        private static (byte R, byte G, byte B, byte A) GetPixelColorWithShadow(Triangle3DElement triangle, List<ILightSource> lights, IEnumerable<Triangle3DElement> shadowers, Point3D correspPoint, Camera camera)
        {
            List<ILightSource> pixelLights = new List<ILightSource>(lights.Count());
            for (int i = 0; i < lights.Count; i++)
            {
                if (!lights[i].CastsShadow)
                {
                    pixelLights.Add(lights[i]);
                }
                else
                {
                    if (!lights[i].IsObstructed(correspPoint, from el in shadowers where el != triangle select el))
                    {
                        pixelLights.Add(lights[i]);
                    }
                }
            }

            byte R = 0;
            byte G = 0;
            byte B = 0;
            byte A = 0;

            NormalizedVector3D normal = triangle.GetNormalAt(correspPoint);

            for (int i = 0; i < triangle.Fill.Count; i++)
            {
                Colour col = triangle.Fill[i].GetColour(correspPoint, normal, camera, pixelLights);

                if (col.A == 1)
                {
                    R = (byte)(col.R * 255);
                    G = (byte)(col.G * 255);
                    B = (byte)(col.B * 255);
                    A = (byte)(col.A * 255);
                }
                else
                {
                    BlendFront(ref R, ref G, ref B, ref A, (byte)(col.R * 255), (byte)(col.G * 255), (byte)(col.B * 255), (byte)(col.A * 255));
                }
            }

            return (R, G, B, A);
        }

        private static (byte R, byte G, byte B, byte A) GetPixelColor(Triangle3DElement triangle, Point3D correspPoint, Camera camera, List<ILightSource> lights)
        {
            NormalizedVector3D normal = triangle.GetNormalAt(correspPoint);

            byte R = 0;
            byte G = 0;
            byte B = 0;
            byte A = 0;

            for (int i = 0; i < triangle.Fill.Count; i++)
            {
                Colour col = triangle.Fill[i].GetColour(correspPoint, normal, camera, lights);

                if (col.A == 1)
                {
                    R = (byte)(col.R * 255);
                    G = (byte)(col.G * 255);
                    B = (byte)(col.B * 255);
                    A = (byte)(col.A * 255);
                }
                else
                {
                    BlendFront(ref R, ref G, ref B, ref A, (byte)(col.R * 255), (byte)(col.G * 255), (byte)(col.B * 255), (byte)(col.A * 255));
                }
            }

            return (R, G, B, A);
        }

        private static void BlendBack(byte backgroundR, byte backgroundG, byte backgroundB, byte backgroundA, ref byte sourceR, ref byte sourceG, ref byte sourceB, ref byte sourceA)
        {
            byte outA = (byte)(sourceA + backgroundA * (255 - sourceA) / 255);

            if (outA == 0)
            {
                sourceR = sourceG = sourceB = sourceA = 0;
            }
            else
            {
                sourceR = (byte)Math.Max(0, Math.Min(255, ((sourceR * sourceA + backgroundR * backgroundA * (255 - sourceA) / 255) / outA)));
                sourceG = (byte)Math.Max(0, Math.Min(255, ((sourceG * sourceA + backgroundG * backgroundA * (255 - sourceA) / 255) / outA)));
                sourceB = (byte)Math.Max(0, Math.Min(255, ((sourceB * sourceA + backgroundB * backgroundA * (255 - sourceA) / 255) / outA)));
                sourceA = outA;
            }
        }

        private static void BlendFront(ref byte backgroundR, ref byte backgroundG, ref byte backgroundB, ref byte backgroundA, byte sourceR, byte sourceG, byte sourceB, byte sourceA)
        {
            byte outA = (byte)(sourceA + backgroundA * (255 - sourceA) / 255);

            if (outA == 0)
            {
                backgroundR = backgroundG = backgroundB = backgroundA = 0;
            }
            else
            {
                backgroundR = (byte)Math.Max(0, Math.Min(255, ((sourceR * sourceA + backgroundR * backgroundA * (255 - sourceA) / 255) / outA)));
                backgroundG = (byte)Math.Max(0, Math.Min(255, ((sourceG * sourceA + backgroundG * backgroundA * (255 - sourceA) / 255) / outA)));
                backgroundB = (byte)Math.Max(0, Math.Min(255, ((sourceB * sourceA + backgroundB * backgroundA * (255 - sourceA) / 255) / outA)));
                backgroundA = outA;
            }
        }
    }
}
