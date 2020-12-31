using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Renders a scene using ray casting.
    /// </summary>
    public class RaycastingRenderer : IRenderer
    {
        /// <summary>
        /// Levels of anti-aliasing.
        /// </summary>
        public enum AntiAliasings
        {
            /// <summary>
            /// No anti-aliasing is performed.
            /// </summary>
            None,

            /// <summary>
            /// Bilinear anti-aliasing with four samples per pixel is performed.
            /// </summary>
            Bilinear4X
        }

        private object RenderLock { get; } = new object();

        /// <summary>
        /// The width of the <see cref="RenderedImage"/>.
        /// </summary>
        public int RenderWidth { get; }

        /// <summary>
        /// The height of the <see cref="RenderedImage"/>.
        /// </summary>
        public int RenderHeight { get; }

        /// <summary>
        /// The last rendered image.
        /// </summary>
        public RasterImage RenderedImage { get; private set; }

        private DisposableIntPtr renderedImageData;

        /// <summary>
        /// A <see cref="DisposableIntPtr"/> to the data contained in the <see cref="RenderedImage"/>.
        /// </summary>
        public DisposableIntPtr RenderedImageData => renderedImageData;

        /// <summary>
        /// Indicates whether the <see cref="RenderedImage"/> should be interpolated when drawn at a different resolution or not.
        /// </summary>
        public bool InterpolateImage { get; set; } = true;

        /// <summary>
        /// Determines the level of anti-aliasing to use when rendering the scene.
        /// </summary>
        public AntiAliasings AntiAliasing { get; set; } = AntiAliasings.None;

        /// <summary>
        /// An event called multiple times during the rendering of the image.
        /// </summary>
        public event EventHandler<RaycastingRendererProgressEventArgs> Progress;

        /// <summary>
        /// Creates a new <see cref="RaycastingRenderer"/>.
        /// </summary>
        /// <param name="renderWidth">The width of the <see cref="RenderedImage"/>.</param>
        /// <param name="renderHeight">The height of the <see cref="RenderedImage"/>.</param>
        public RaycastingRenderer(int renderWidth, int renderHeight)
        {
            this.RenderWidth = renderWidth;
            this.RenderHeight = renderHeight;

            IntPtr imageData = Marshal.AllocHGlobal(this.RenderWidth * this.RenderHeight * 4);

            renderedImageData = new DisposableIntPtr(imageData);

            this.RenderedImage = new RasterImage(ref this.renderedImageData, this.RenderWidth, this.RenderHeight, true, this.InterpolateImage);
        }

        /// <inheritdoc/>
        public Page Render(IScene scene, IEnumerable<ILightSource> lights, Camera camera)
        {
            Page pag = new Page(camera.Size.Width, camera.Size.Height);

            lock (RenderLock)
            {
                lock (scene.SceneLock)
                {
                    IEnumerable<Element3D> sceneElements = scene.SceneElements;

                    Camera[] cameras;

                    if (camera is IBlurrableCamera blurrableCamera)
                    {
                        cameras = blurrableCamera.GetCameras();
                    }
                    else
                    {
                        cameras = new Camera[] { camera };
                    }

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

                    List<Element3D>[] nonCulled = new List<Element3D>[cameras.Length];

                    for (int i = 0; i < cameras.Length; i++)
                    {
                        nonCulled[i] = new List<Element3D>();
                        foreach (Element3D element in sceneElements)
                        {
                            if (!cameras[i].IsCulled(element))
                            {
                                nonCulled[i].Add(element);
                            }
                        }
                    }

                    if (cameras.Length == 1)
                    {
                        foreach (Element3D el in nonCulled[0])
                        {
                            el.SetProjection(cameras[0]);
                        }
                    }

                    unsafe
                    {
                        byte* imageData = (byte*)this.renderedImageData.InternalPointer;

                        for (int i = 0; i < this.RenderWidth * this.RenderHeight * 4; i++)
                        {
                            imageData[i] = 0;
                        }

                        int totalPixels = this.RenderWidth * this.RenderHeight;

                        object progressLock = new object();
                        int progress = 0;

                        if (cameras.Length == 1)
                        {

                            Parallel.For(0, totalPixels, i =>
                            {
                                double x = i % this.RenderWidth;
                                double y = i / this.RenderWidth;

                                if (this.AntiAliasing == AntiAliasings.None)
                                {
                                    Point point = new Point((x + 0.5) / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (y + 0.5) / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y);
                                    (byte R, byte G, byte B, byte A) = GetPixelColor(point, nonCulled[0], camera, lightList, shadowers, noObstructions, false);

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

                                    (byte R1, byte G1, byte B1, byte A1) = GetPixelColor(p1, nonCulled[0], camera, lightList, shadowers, noObstructions, false);
                                    (byte R2, byte G2, byte B2, byte A2) = GetPixelColor(p2, nonCulled[0], camera, lightList, shadowers, noObstructions, false);
                                    (byte R3, byte G3, byte B3, byte A3) = GetPixelColor(p3, nonCulled[0], camera, lightList, shadowers, noObstructions, false);
                                    (byte R4, byte G4, byte B4, byte A4) = GetPixelColor(p4, nonCulled[0], camera, lightList, shadowers, noObstructions, false);



                                    int totA = (A1 + A2 + A3 + A4);

                                    if (totA > 0)
                                    {
                                        imageData[i * 4] = (byte)((R1 * A1 + R2 * A2 + R3 * A3 + R4 * A4) / totA);
                                        imageData[i * 4 + 1] = (byte)((G1 * A1 + G2 * A2 + G3 * A3 + G4 * A4) / totA);
                                        imageData[i * 4 + 2] = (byte)((B1 * A1 + B2 * A2 + B3 * A3 + B4 * A4) / totA);
                                        imageData[i * 4 + 3] = (byte)((A1 + A2 + A3 + A4) / 4);
                                    }
                                }

                                if (i % Math.Max(1, (totalPixels / 1000)) == 0)
                                {
                                    lock (progressLock)
                                    {
                                        progress++;

                                        double currProgress = progress / 1000.0;

                                        this.Progress?.Invoke(this, new RaycastingRendererProgressEventArgs(currProgress));
                                    }
                                }
                            });
                        }
                        else
                        {
                            Parallel.For(0, totalPixels, i =>
                            {
                                double x = i % this.RenderWidth;
                                double y = i / this.RenderWidth;

                                if (this.AntiAliasing == AntiAliasings.None)
                                {
                                    Point point = new Point((x + 0.5) / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (y + 0.5) / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y);

                                    GetPixelColourFromCameras(cameras, point, nonCulled, lightList, shadowers, noObstructions, ref imageData[i * 4], ref imageData[i * 4 + 1], ref imageData[i * 4 + 2], ref imageData[i * 4 + 3]);
                                }
                                else if (this.AntiAliasing == AntiAliasings.Bilinear4X)
                                {
                                    Point p1 = new Point((x + 0.3688) / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (y + 0.11177) / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y);
                                    Point p2 = new Point((x + 0.8889) / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (y + 0.37069) / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y);
                                    Point p3 = new Point((x + 0.62998) / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (y + 0.89079) / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y);
                                    Point p4 = new Point((x + 0.10988) / this.RenderWidth * camera.Size.Width + camera.TopLeft.X, (y + 0.63187) / this.RenderHeight * camera.Size.Height + camera.TopLeft.Y);

                                    byte R1 = 0, G1 = 0, B1 = 0, A1 = 0;
                                    byte R2 = 0, G2 = 0, B2 = 0, A2 = 0;
                                    byte R3 = 0, G3 = 0, B3 = 0, A3 = 0;
                                    byte R4 = 0, G4 = 0, B4 = 0, A4 = 0;

                                    GetPixelColourFromCameras(cameras, p1, nonCulled, lightList, shadowers, noObstructions, ref R1, ref G1, ref B1, ref A1);
                                    GetPixelColourFromCameras(cameras, p2, nonCulled, lightList, shadowers, noObstructions, ref R2, ref G2, ref B2, ref A2);
                                    GetPixelColourFromCameras(cameras, p3, nonCulled, lightList, shadowers, noObstructions, ref R3, ref G3, ref B3, ref A3);
                                    GetPixelColourFromCameras(cameras, p4, nonCulled, lightList, shadowers, noObstructions, ref R4, ref G4, ref B4, ref A4);

                                    int totA = (A1 + A2 + A3 + A4);

                                    if (totA > 0)
                                    {
                                        imageData[i * 4] = (byte)((R1 * A1 + R2 * A2 + R3 * A3 + R4 * A4) / totA);
                                        imageData[i * 4 + 1] = (byte)((G1 * A1 + G2 * A2 + G3 * A3 + G4 * A4) / totA);
                                        imageData[i * 4 + 2] = (byte)((B1 * A1 + B2 * A2 + B3 * A3 + B4 * A4) / totA);
                                        imageData[i * 4 + 3] = (byte)((A1 + A2 + A3 + A4) / 4);
                                    }
                                }

                                if (i % Math.Max(1, (totalPixels / 1000)) == 0)
                                {
                                    lock (progressLock)
                                    {
                                        progress++;

                                        double currProgress = progress / 1000.0;

                                        this.Progress?.Invoke(this, new RaycastingRendererProgressEventArgs(currProgress));
                                    }
                                }
                            });
                        }
                    }

                    this.RenderedImage = new RasterImage(ref this.renderedImageData, this.RenderWidth, this.RenderHeight, true, this.InterpolateImage);

                    pag.Graphics.Save();
                    pag.Graphics.Scale(camera.Size.Width / RenderWidth, camera.Size.Height / RenderHeight);

                    pag.Graphics.DrawRasterImage(0, 0, RenderWidth, RenderHeight, this.RenderedImage);

                    pag.Graphics.Restore();
                }
            }

            return pag;
        }

        private void GetPixelColourFromCameras(Camera[] cameras, Point point, List<Element3D>[] nonCulled, List<ILightSource> lightList, List<Triangle3DElement> shadowers, List<double> noObstructions, ref byte R, ref byte G, ref byte B, ref byte A)
        {
            byte[] pixelColours = new byte[4 * cameras.Length];
            for (int j = 0; j < cameras.Length; j++)
            {
                (pixelColours[j * 4], pixelColours[j * 4 + 1], pixelColours[j * 4 + 2], pixelColours[j * 4 + 3]) = GetPixelColor(point, nonCulled[j], cameras[j], lightList, shadowers, noObstructions, true);
            }

            AverageColours(pixelColours, cameras.Length, ref R, ref G, ref B, ref A);
        }
        private void AverageColours(byte[] colours, int colourCount, ref byte R, ref byte G, ref byte B, ref byte A)
        {
            int totA = 0;
            int totR = 0;
            int totG = 0;
            int totB = 0;

            for (int i = 0; i < colourCount; i++)
            {
                totR += colours[i * 4] * colours[i * 4 + 3];
                totG += colours[i * 4 + 1] * colours[i * 4 + 3];
                totB += colours[i * 4 + 2] * colours[i * 4 + 3];
                totA += colours[i * 4 + 3];
            }

            if (totA > 0)
            {
                R = (byte)(totR / totA);
                G = (byte)(totG / totA);
                B = (byte)(totB / totA);
                A = (byte)(totA / colourCount);
            }
        }

        private (byte R, byte G, byte B, byte A) GetPixelColor(Point point, List<Element3D> elements, Camera camera, List<ILightSource> lights, List<Triangle3DElement> shadowers, List<double> noObstructions, bool reproject)
        {
            List<(Element3D element, double z, Point3D correspPoint)> hits = new List<(Element3D element, double z, Point3D correspPoint)>();

            foreach (Element3D element in elements)
            {
                if (element is Triangle3DElement triangle)
                {
                    Point[] projection;

                    if (reproject)
                    {
                        projection = new Point[] { camera.Project(triangle[0]), camera.Project(triangle[1]), camera.Project(triangle[2]) };
                    }
                    else
                    {
                        projection = triangle.GetProjection();
                    }


                    if (Intersections2D.PointInTriangle(point, projection[0], projection[1], projection[2]))
                    {
                        Point3D correspPoint = camera.Deproject(point, triangle);

                        double z = camera.ZDepth(correspPoint);

                        hits.Add((triangle, z, correspPoint));
                    }
                }
                else if (element is Point3DElement pointElement)
                {
                    Point projection;

                    if (reproject)
                    {
                        projection = camera.Project(pointElement[0]);
                    }
                    else
                    {
                        projection = pointElement.GetProjection()[0];

                    }

                    if ((point.X - projection.X) * (point.X - projection.X) + (point.Y - projection.Y) * (point.Y - projection.Y) <= pointElement.Diameter * pointElement.Diameter * 0.25)
                    {
                        double z = camera.ZDepth(pointElement.Point);

                        hits.Add((pointElement, z, pointElement.Point));
                    }
                }
                else if (element is Line3DElement line)
                {
                    Point[] line2D;

                    if (reproject)
                    {
                        line2D = new Point[] { camera.Project(line[0]), camera.Project(line[1]) };
                    }
                    else
                    {
                        line2D = line.GetProjection();
                    }

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
                            Point3D correspPoint = camera.Deproject(pointOnLine, line);

                            double z = camera.ZDepth(correspPoint);

                            hits.Add((line, z, correspPoint));
                        }
                    }
                }
            }

            hits.Sort((a, b) =>
            {
                if (a.element.ZIndex == b.element.ZIndex)
                {
                    return Math.Sign(a.z - b.z);
                }
                else
                {
                    return Math.Sign(b.element.ZIndex - a.element.ZIndex);
                }
            });

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
                        (R, G, B, A) = GetPixelColor(triangle, hit.correspPoint, camera, lights, noObstructions);
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

        private static (byte R, byte G, byte B, byte A) GetPixelColorWithShadow(Triangle3DElement triangle, List<ILightSource> lights, IEnumerable<Triangle3DElement> shadowers, Point3D correspPoint, Camera camera)
        {
            List<double> pixelObstructions = new List<double>(lights.Count);

            for (int i = 0; i < lights.Count; i++)
            {
                if (!lights[i].CastsShadow)
                {
                    pixelObstructions.Add(0);
                }
                else
                {
                    pixelObstructions.Add(lights[i].GetObstruction(correspPoint, from el in shadowers where el != triangle select el));
                }
            }


            byte R = 0;
            byte G = 0;
            byte B = 0;
            byte A = 0;

            NormalizedVector3D normal = triangle.GetNormalAt(correspPoint);

            for (int i = 0; i < triangle.Fill.Count; i++)
            {
                Colour col = triangle.Fill[i].GetColour(correspPoint, normal, camera, lights, pixelObstructions);

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

        private static (byte R, byte G, byte B, byte A) GetPixelColor(Triangle3DElement triangle, Point3D correspPoint, Camera camera, List<ILightSource> lights, List<double> obstructions)
        {
            NormalizedVector3D normal = triangle.GetNormalAt(correspPoint);

            byte R = 0;
            byte G = 0;
            byte B = 0;
            byte A = 0;

            for (int i = 0; i < triangle.Fill.Count; i++)
            {
                Colour col = triangle.Fill[i].GetColour(correspPoint, normal, camera, lights, obstructions);

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

    /// <summary>
    /// Represents the current progress of a ray casting rendering pass.
    /// </summary>
    public class RaycastingRendererProgressEventArgs : EventArgs
    {
        /// <summary>
        /// The current progress. Should be between 0 and 1, but can be greater than 1 due to rounding errors with low-resolution renderings.
        /// </summary>
        public double Progress { get; }

        /// <summary>
        /// Creates a new <see cref="RaycastingRendererProgressEventArgs"/>.
        /// </summary>
        /// <param name="progress">The current progress.</param>
        public RaycastingRendererProgressEventArgs(double progress) : base()
        {
            this.Progress = progress;
        }
    }
}
