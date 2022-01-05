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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Renders a scene by rasterisation.
    /// </summary>
    public class RasterRenderer : IRenderer
    {
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

        private double[] ZBuffer { get; }
        private int[] ZIndexBuffer { get; }

        /// <summary>
        /// Indicates whether the <see cref="RenderedImage"/> should be interpolated when drawn at a different resolution or not.
        /// </summary>
        public bool InterpolateImage { get; set; } = true;

        /// <summary>
        /// Creates a new <see cref="RasterRenderer"/>.
        /// </summary>
        /// <param name="renderWidth">The width of the rendered image.</param>
        /// <param name="renderHeight">The height of the rendered image.</param>
        public RasterRenderer(int renderWidth, int renderHeight)
        {
            this.RenderWidth = renderWidth;
            this.RenderHeight = renderHeight;

            IntPtr imageData = Marshal.AllocHGlobal(this.RenderWidth * this.RenderHeight * 4);

            renderedImageData = new DisposableIntPtr(imageData);

            this.ZBuffer = new double[this.RenderWidth * this.RenderHeight];
            this.ZIndexBuffer = new int[this.RenderWidth * this.RenderHeight];

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



                    foreach (Element3D element in sceneElements)
                    {
                        if (!camera.IsCulled(element))
                        {
                            nonCulled.Add(element);
                        }
                    }

                    foreach (Element3D el in nonCulled)
                    {
                        el.SetProjection(camera);
                    }

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

                        foreach (Element3D element in nonCulled)
                        {
                            if (!camera.IsCulled(element))
                            {
                                if (element is Point3DElement point)
                                {
                                    DrawPoint(imageData, point, camera);
                                }
                                else if (element is Line3DElement line)
                                {
                                    DrawLine(imageData, line, camera);
                                }
                                else if (element is Triangle3DElement triangle)
                                {
                                    if (!anyShadows || !triangle.ReceivesShadow)
                                    {
                                        FillTriangle(imageData, triangle, camera, lightList, noObstructions);
                                    }
                                    else
                                    {
                                        FillTriangleWithShadow(imageData, triangle, camera, lightList, shadowers);
                                    }
                                }
                            }
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



        private unsafe void FillTriangle(byte* imageData, Triangle3DElement triangle, Camera camera, List<ILightSource> lights, List<double> obstructions)
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
                            Colour col = triangle.Fill[i].GetColour(correspPoint, triangle.GetNormalAt(correspPoint), camera, lights, obstructions);

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
                            Colour col = triangle.Fill[i].GetColour(correspPoint, triangle.GetNormalAt(correspPoint), camera, lights, obstructions);

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
                    if (y * RenderWidth + x < ZIndexBuffer.Length)
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


        private unsafe void FillTriangleWithShadow(byte* imageData, Triangle3DElement triangle, Camera camera, List<ILightSource> lights, List<Triangle3DElement> shadowers)
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

            List<Triangle3DElement> otherShadowers = new List<Triangle3DElement>(shadowers.Count);

            for (int i = 0; i < shadowers.Count; i++)
            {
                if (shadowers[i] != triangle)
                {
                    otherShadowers.Add(shadowers[i]);
                }
            }

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
                        (byte R, byte G, byte B, byte A) = GetPixelColorWithShadow(triangle, lights, otherShadowers, x, y, correspPoint, camera);

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
                        (byte R, byte G, byte B, byte A) = GetPixelColorWithShadow(triangle, lights, otherShadowers, x, y, correspPoint, camera);

                        BlendBack(R, G, B, A, ref imageData[y * RenderWidth * 4 + x * 4], ref imageData[y * RenderWidth * 4 + x * 4 + 1], ref imageData[y * RenderWidth * 4 + x * 4 + 2], ref imageData[y * RenderWidth * 4 + x * 4 + 3]);
                    }
                }
            });
        }

        private static (byte R, byte G, byte B, byte A) GetPixelColorWithShadow(Triangle3DElement triangle, List<ILightSource> lights, IEnumerable<Triangle3DElement> shadowers, int x, int y, Point3D correspPoint, Camera camera)
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
                    pixelObstructions.Add(lights[i].GetObstruction(correspPoint, shadowers));
                }
            }


            byte R = 0;
            byte G = 0;
            byte B = 0;
            byte A = 0;

            for (int i = 0; i < triangle.Fill.Count; i++)
            {
                Colour col = triangle.Fill[i].GetColour(correspPoint, triangle.GetNormalAt(correspPoint), camera, lights, pixelObstructions);

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
