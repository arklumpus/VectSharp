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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Collections.Generic;
using SixLabors.ImageSharp.Advanced;
using System.IO;
using VectSharp.Filters;
using SixLabors.ImageSharp.Formats.Gif;
using System.Threading.Tasks;

namespace VectSharp.Raster.ImageSharp
{
    internal static class MatrixUtils
    {
        public static System.Numerics.Matrix3x2 ToMatrix(this double[,] matrix)
        {
            return new System.Numerics.Matrix3x2((float)matrix[0, 0], (float)matrix[1, 0], (float)matrix[0, 1], (float)matrix[1, 1], (float)matrix[0, 2], (float)matrix[1, 2]);
        }

        public static double[] Multiply(double[,] matrix, double[] vector)
        {
            double[] tbr = new double[2];

            tbr[0] = matrix[0, 0] * vector[0] + matrix[0, 1] * vector[1] + matrix[0, 2];
            tbr[1] = matrix[1, 0] * vector[0] + matrix[1, 1] * vector[1] + matrix[1, 2];

            return tbr;
        }

        public static Point Multiply(this double[,] matrix, Point vector)
        {
            return new Point(matrix[0, 0] * vector.X + matrix[0, 1] * vector.Y + matrix[0, 2], matrix[1, 0] * vector.X + matrix[1, 1] * vector.Y + matrix[1, 2]);
        }

        public static double[,] Multiply(double[,] matrix1, double[,] matrix2)
        {
            double[,] tbr = new double[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        tbr[i, j] += matrix1[i, k] * matrix2[k, j];
                    }
                }
            }

            return tbr;
        }

        public static double[,] Rotate(double[,] matrix, double angle)
        {
            double[,] rotationMatrix = new double[3, 3];
            rotationMatrix[0, 0] = Math.Cos(angle);
            rotationMatrix[0, 1] = -Math.Sin(angle);
            rotationMatrix[1, 0] = Math.Sin(angle);
            rotationMatrix[1, 1] = Math.Cos(angle);
            rotationMatrix[2, 2] = 1;

            return Multiply(matrix, rotationMatrix);
        }

        public static double[,] Translate(double[,] matrix, double x, double y)
        {
            double[,] translationMatrix = new double[3, 3];
            translationMatrix[0, 0] = 1;
            translationMatrix[0, 2] = x;
            translationMatrix[1, 1] = 1;
            translationMatrix[1, 2] = y;
            translationMatrix[2, 2] = 1;

            return Multiply(matrix, translationMatrix);
        }

        public static double[,] Scale(double[,] matrix, double scaleX, double scaleY)
        {
            double[,] scaleMatrix = new double[3, 3];
            scaleMatrix[0, 0] = scaleX;
            scaleMatrix[1, 1] = scaleY;
            scaleMatrix[2, 2] = 1;

            return Multiply(matrix, scaleMatrix);
        }

        public static double[,] Identity = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
        public static double[,] Invert(double[,] m)
        {
            double[,] tbr = new double[3, 3];

            tbr[0, 0] = (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]) / (m[0, 0] * m[1, 1] * m[2, 2] - m[0, 0] * m[1, 2] * m[2, 1] - m[1, 0] * m[0, 1] * m[2, 2] + m[2, 0] * m[0, 1] * m[1, 2] + m[1, 0] * m[0, 2] * m[2, 1] - m[2, 0] * m[0, 2] * m[1, 1]);
            tbr[0, 1] = -(m[0, 1] * m[2, 2] - m[0, 2] * m[2, 1]) / (m[0, 0] * m[1, 1] * m[2, 2] - m[0, 0] * m[1, 2] * m[2, 1] - m[1, 0] * m[0, 1] * m[2, 2] + m[2, 0] * m[0, 1] * m[1, 2] + m[1, 0] * m[0, 2] * m[2, 1] - m[2, 0] * m[0, 2] * m[1, 1]);
            tbr[0, 2] = (m[0, 1] * m[1, 2] - m[0, 2] * m[1, 1]) / (m[0, 0] * m[1, 1] * m[2, 2] - m[0, 0] * m[1, 2] * m[2, 1] - m[1, 0] * m[0, 1] * m[2, 2] + m[2, 0] * m[0, 1] * m[1, 2] + m[1, 0] * m[0, 2] * m[2, 1] - m[2, 0] * m[0, 2] * m[1, 1]);
            tbr[1, 0] = -(m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) / (m[0, 0] * m[1, 1] * m[2, 2] - m[0, 0] * m[1, 2] * m[2, 1] - m[1, 0] * m[0, 1] * m[2, 2] + m[2, 0] * m[0, 1] * m[1, 2] + m[1, 0] * m[0, 2] * m[2, 1] - m[2, 0] * m[0, 2] * m[1, 1]);
            tbr[1, 1] = (m[0, 0] * m[2, 2] - m[0, 2] * m[2, 0]) / (m[0, 0] * m[1, 1] * m[2, 2] - m[0, 0] * m[1, 2] * m[2, 1] - m[1, 0] * m[0, 1] * m[2, 2] + m[2, 0] * m[0, 1] * m[1, 2] + m[1, 0] * m[0, 2] * m[2, 1] - m[2, 0] * m[0, 2] * m[1, 1]);
            tbr[1, 2] = -(m[0, 0] * m[1, 2] - m[0, 2] * m[1, 0]) / (m[0, 0] * m[1, 1] * m[2, 2] - m[0, 0] * m[1, 2] * m[2, 1] - m[1, 0] * m[0, 1] * m[2, 2] + m[2, 0] * m[0, 1] * m[1, 2] + m[1, 0] * m[0, 2] * m[2, 1] - m[2, 0] * m[0, 2] * m[1, 1]);
            tbr[2, 0] = (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]) / (m[0, 0] * m[1, 1] * m[2, 2] - m[0, 0] * m[1, 2] * m[2, 1] - m[1, 0] * m[0, 1] * m[2, 2] + m[2, 0] * m[0, 1] * m[1, 2] + m[1, 0] * m[0, 2] * m[2, 1] - m[2, 0] * m[0, 2] * m[1, 1]);
            tbr[2, 1] = -(m[0, 0] * m[2, 1] - m[0, 1] * m[2, 0]) / (m[0, 0] * m[1, 1] * m[2, 2] - m[0, 0] * m[1, 2] * m[2, 1] - m[1, 0] * m[0, 1] * m[2, 2] + m[2, 0] * m[0, 1] * m[1, 2] + m[1, 0] * m[0, 2] * m[2, 1] - m[2, 0] * m[0, 2] * m[1, 1]);
            tbr[2, 2] = (m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0]) / (m[0, 0] * m[1, 1] * m[2, 2] - m[0, 0] * m[1, 2] * m[2, 1] - m[1, 0] * m[0, 1] * m[2, 2] + m[2, 0] * m[0, 1] * m[1, 2] + m[1, 0] * m[0, 2] * m[2, 1] - m[2, 0] * m[0, 2] * m[1, 1]);

            return tbr;
        }

        public static double Determinant(double[,] matrix)
        {
            return (matrix[0, 0] * matrix[1, 1] - matrix[1, 0] * matrix[0, 1]) * matrix[2, 2] - (matrix[0, 0] * matrix[1, 2] - matrix[1, 0] * matrix[0, 2]) * matrix[2, 1] + (matrix[0, 1] * matrix[1, 2] - matrix[1, 1] * matrix[0, 2]) * matrix[2, 0];
        }
    }

    internal class ImageSharpContext : IGraphicsContext
    {
        public Image<SixLabors.ImageSharp.PixelFormats.Rgba32> Image { get; }
        private Image Buffer { get; set; }
        private Image ClipBuffer { get; }

        public double Width { get; private set; }

        public double Height { get; private set; }

        public int IntWidth { get; private set; }
        public int IntHeight { get; private set; }

        public Font Font { get; set; }
        public TextBaselines TextBaseline { get; set; }

        public Brush FillStyle { get; set; }

        public Brush StrokeStyle { get; set; }

        public double LineWidth { get; set; }
        public LineCaps LineCap { get; set; }
        public LineJoins LineJoin { get; set; }

        public LineDash Dash { get; set; }
        public string Tag { get; set; }

        private double[,] _transform;
        private readonly Stack<double[,]> states;

        private readonly Stack<Image> clips;
        private Image CurrentClip;

        private List<IDisposable> disposables;

        PathBuilder currentPath;
        bool figureInitialised;
        Point currentPoint;
        double scaleFactor;

        public ImageSharpContext(double width, double height, double scaleFactor, Colour backgroundColour)
        {
            currentPath = new PathBuilder();
            figureInitialised = false;
            currentPoint = new Point();

            IntWidth = (int)(width * scaleFactor);
            IntHeight = (int)(height * scaleFactor);

            this.scaleFactor = Math.Sqrt(IntWidth / width * IntHeight / height);

            _transform = new double[3, 3];

            _transform[0, 0] = scaleFactor;
            _transform[1, 1] = scaleFactor;
            _transform[2, 2] = 1;

            states = new Stack<double[,]>();

            Width = width;
            Height = height;

            this.Image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(IntWidth, IntHeight);
            this.Image.Mutate(x => x.Fill(backgroundColour.ToImageSharpColor()));

            this.Buffer = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(IntWidth, IntHeight);
            this.Buffer.Mutate(x => x.Clear(Color.FromRgba(0, 0, 0, 0)));

            this.ClipBuffer = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(IntWidth, IntHeight);
            this.ClipBuffer.Mutate(x => x.Clear(Color.FromRgba(0, 0, 0, 0)));

            this.clips = new Stack<Image>();
            this.CurrentClip = null;

            disposables = new List<IDisposable>();
        }

        public void DisposeAllExceptImage()
        {
            for (int i = 0; i < disposables.Count; i++)
            {
                disposables[i].Dispose();
                this.CurrentClip?.Dispose();
                this.Buffer.Dispose();
                this.ClipBuffer.Dispose();

            }
        }

        public void Close()
        {
            this.currentPath.CloseFigure();
            this.figureInitialised = false;
        }

        public void CubicBezierTo(double p1X, double p1Y, double p2X, double p2Y, double p3X, double p3Y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref p1X, ref p1Y, ref p2X, ref p2Y, ref p3X, ref p3Y);

            if (figureInitialised)
            {
                Point p1 = new Point(p1X, p1Y);
                Point p2 = new Point(p2X, p2Y);
                Point p3 = new Point(p3X, p3Y);
                currentPath.AddCubicBezier(currentPoint.ToPointF(1), p1.ToPointF(1), p2.ToPointF(1), p3.ToPointF(1));
                currentPoint = p3;
            }
            else
            {
                currentPath.StartFigure();
                currentPoint = new Point(p3X, p3Y);
                figureInitialised = true;
            }
        }

        public void DrawRasterImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image)
        {
            Image sourceImage;

            unsafe
            {
                if (image.HasAlpha)
                {
                    ReadOnlySpan<byte> data = new ReadOnlySpan<byte>((void*)image.ImageDataAddress, image.Width * image.Height * 4);
                    sourceImage = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(data, image.Width, image.Height);
                }
                else
                {
                    ReadOnlySpan<byte> data = new ReadOnlySpan<byte>((void*)image.ImageDataAddress, image.Width * image.Height * 3);
                    sourceImage = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgb24>(data, image.Width, image.Height);
                }
            }

            DrawImage(sourceX, sourceY, sourceWidth, sourceHeight, destinationX, destinationY, destinationWidth, destinationHeight, image.Interpolate, sourceImage);
        }

        internal void DrawImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, bool interpolate, Image sourceImage)
        {
            sourceImage.Mutate(x => x.Crop(new SixLabors.ImageSharp.Rectangle(sourceX, sourceY, sourceWidth, sourceHeight)));

            Point targetPoint = _transform.Multiply(new Point(destinationX, destinationY));
            Point targetPointX = _transform.Multiply(new Point(destinationX + destinationWidth, destinationY));
            Point targetPointY = _transform.Multiply(new Point(destinationX, destinationY + destinationHeight));
            Point targetPointXY = _transform.Multiply(new Point(destinationX + destinationWidth, destinationY + destinationHeight));

            double minX = Math.Min(Math.Min(targetPoint.X, targetPointX.X), Math.Min(targetPointY.X, targetPointXY.X));
            double minY = Math.Min(Math.Min(targetPoint.Y, targetPointX.Y), Math.Min(targetPointY.Y, targetPointXY.Y));

            double maxX = Math.Max(Math.Max(targetPoint.X, targetPointX.X), Math.Max(targetPointY.X, targetPointXY.X));
            double maxY = Math.Max(Math.Max(targetPoint.Y, targetPointX.Y), Math.Max(targetPointY.Y, targetPointXY.Y));

            double[,] currTransform = MatrixUtils.Multiply(MatrixUtils.Translate(_transform, destinationX, destinationY), MatrixUtils.Scale(MatrixUtils.Identity, destinationWidth / sourceImage.Width, destinationHeight / sourceImage.Height));

            Point origin = currTransform.Multiply(new Point(0, 0));

            double[,] translation = MatrixUtils.Translate(MatrixUtils.Identity, -minX, -minY);

            double[,] centeredTransform = MatrixUtils.Multiply(translation, currTransform);

            SixLabors.ImageSharp.Processing.Processors.Transforms.IResampler resampler;

            if (interpolate)
            {
                resampler = KnownResamplers.Bicubic;
            }
            else
            {
                resampler = KnownResamplers.NearestNeighbor;
            }

            sourceImage.Mutate(x => x.Transform(new SixLabors.ImageSharp.Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), centeredTransform.ToMatrix(), new SixLabors.ImageSharp.Size((int)Math.Round(maxX - minX), (int)Math.Round(maxY - minY)), resampler));

            if (this.CurrentClip == null)
            {
                this.Image.Mutate(x => x.CheckedDrawImage(sourceImage, new SixLabors.ImageSharp.Point((int)Math.Round(minX), (int)Math.Round(minY)), 1));
            }
            else
            {
                this.ClipBuffer.Mutate(x => x.Clear(Color.FromRgba(0, 0, 0, 0)));
                this.ClipBuffer.Mutate(x => x.CheckedDrawImage(sourceImage, new SixLabors.ImageSharp.Point((int)Math.Round(minX), (int)Math.Round(minY)), 1));

                GraphicsOptions opt = new GraphicsOptions() { AlphaCompositionMode = SixLabors.ImageSharp.PixelFormats.PixelAlphaCompositionMode.SrcIn };

                this.Buffer.Dispose();

                this.Buffer = this.CurrentClip.Clone(x => x.CheckedDrawImage(this.ClipBuffer, opt));

                this.Image.Mutate(x => x.CheckedDrawImage(this.Buffer, 1));
            }
        }

        public void Fill(FillRule fillRule)
        {
            IPath path = this.currentPath.Build();

            ShapeOptions shapeOptions = new ShapeOptions();

            switch (fillRule)
            {
                case FillRule.NonZeroWinding:
                    shapeOptions.IntersectionRule = IntersectionRule.NonZero;
                    break;
                case FillRule.EvenOdd:
                    shapeOptions.IntersectionRule = IntersectionRule.EvenOdd;
                    break;

            }

            if (this.CurrentClip == null)
            {
                this.Image.Mutate(x => x.Fill(new DrawingOptions() { Transform = _transform.ToMatrix(), ShapeOptions = shapeOptions }, this.FillStyle.ToImageSharpBrush(_transform), path));
            }
            else
            {
                this.ClipBuffer.Mutate(x => x.Clear(Color.FromRgba(0, 0, 0, 0)));
                this.ClipBuffer.Mutate(x => x.Fill(new DrawingOptions() { Transform = _transform.ToMatrix(), ShapeOptions = shapeOptions }, this.FillStyle.ToImageSharpBrush(_transform), path));

                GraphicsOptions opt = new GraphicsOptions() { AlphaCompositionMode = SixLabors.ImageSharp.PixelFormats.PixelAlphaCompositionMode.SrcIn };

                this.Buffer.Dispose();

                this.Buffer = this.CurrentClip.Clone(x => x.CheckedDrawImage(this.ClipBuffer, opt));

                this.Image.Mutate(x => x.CheckedDrawImage(this.Buffer, 1));
            }

            this.currentPath = new PathBuilder();
            this.figureInitialised = false;
        }

        public void FillText(string text, double x, double y)
        {
            PathText(text, x, y);
            Fill(FillRule.NonZeroWinding);
        }

        private void PathText(string text, double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            GraphicsPath textPath = new GraphicsPath().AddText(x, y, text, Font, TextBaseline);

            for (int j = 0; j < textPath.Segments.Count; j++)
            {
                switch (textPath.Segments[j].Type)
                {
                    case VectSharp.SegmentType.Move:
                        this.MoveTo(textPath.Segments[j].Point.X, textPath.Segments[j].Point.Y);
                        break;
                    case VectSharp.SegmentType.Line:
                        this.LineTo(textPath.Segments[j].Point.X, textPath.Segments[j].Point.Y);
                        break;
                    case VectSharp.SegmentType.CubicBezier:
                        this.CubicBezierTo(textPath.Segments[j].Points[0].X, textPath.Segments[j].Points[0].Y, textPath.Segments[j].Points[1].X, textPath.Segments[j].Points[1].Y, textPath.Segments[j].Points[2].X, textPath.Segments[j].Points[2].Y);
                        break;
                    case VectSharp.SegmentType.Close:
                        this.Close();
                        break;
                }
            }
        }

        public void LineTo(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            if (figureInitialised)
            {
                Point newPoint = new Point(x, y);
                currentPath.AddLine(currentPoint.ToPointF(1), newPoint.ToPointF(1));
                currentPoint = newPoint;
            }
            else
            {
                currentPath.StartFigure();
                currentPoint = new Point(x, y);
                figureInitialised = true;
            }
        }

        public void MoveTo(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            currentPath.StartFigure();
            currentPoint = new Point(x, y);
            figureInitialised = true;
        }

        public void Rectangle(double x0, double y0, double width, double height)
        {
            MoveTo(x0, y0);
            LineTo(x0 + width, y0);
            LineTo(x0 + width, y0 + height);
            LineTo(x0, y0 + height);
            Close();
        }

        public void Restore()
        {
            _transform = states.Pop();

            Image newClip = clips.Pop();
            if (CurrentClip != newClip)
            {
                CurrentClip.Dispose();
            }

            CurrentClip = newClip;
        }

        public void Rotate(double angle)
        {
            Utils.CoerceNaNAndInfinityToZero(ref angle);

            _transform = MatrixUtils.Rotate(_transform, angle);

            currentPath = new PathBuilder();
            figureInitialised = false;
        }

        public void Save()
        {
            states.Push((double[,])_transform.Clone());
            clips.Push(CurrentClip);
        }

        public void Scale(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            _transform = MatrixUtils.Scale(_transform, x, y);

            currentPath = new PathBuilder();
            figureInitialised = false;
        }

        public void SetClippingPath()
        {
            IPath path = this.currentPath.Build();

            Image newClip;

            DrawingOptions opt = new DrawingOptions() { Transform = _transform.ToMatrix() };

            if (this.CurrentClip == null)
            {
                newClip = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(IntWidth, IntHeight);
                newClip.Mutate(x => x.Clear(Color.FromRgba(0, 0, 0, 0)));
                newClip.Mutate(x => x.Fill(opt, Color.FromRgb(0, 0, 0), path));
            }
            else
            {
                this.Buffer.Mutate(x => x.Clear(Color.FromRgba(0, 0, 0, 0)));
                this.Buffer.Mutate(x => x.Fill(opt, Color.FromRgb(0, 0, 0), path));

                newClip = this.CurrentClip.Clone(x => x.CheckedDrawImage(this.Buffer, new GraphicsOptions() { AlphaCompositionMode = SixLabors.ImageSharp.PixelFormats.PixelAlphaCompositionMode.SrcIn }));

                disposables.Add(this.CurrentClip);
            }

            this.CurrentClip = newClip;

            this.currentPath = new PathBuilder();
            this.figureInitialised = false;
        }

        public void SetFillStyle((int r, int g, int b, double a) style)
        {
            this.FillStyle = Colour.FromRgba(style);
        }

        public void SetFillStyle(Brush style)
        {
            this.FillStyle = style;
        }

        public void SetLineDash(LineDash dash)
        {
            this.Dash = dash;
        }

        public void SetStrokeStyle((int r, int g, int b, double a) style)
        {
            this.StrokeStyle = Colour.FromRgba(style);
        }

        public void SetStrokeStyle(Brush style)
        {
            this.StrokeStyle = style;
        }

        public void Stroke()
        {
            if (this.LineWidth > 0)
            {
                if (this.CurrentClip == null)
                {
                    this.Image.Mutate(x => x.Fill(new DrawingOptions() { Transform = _transform.ToMatrix() }, this.StrokeStyle.ToImageSharpBrush(_transform), this.currentPath.Build().GenerateOutline((float)(this.LineWidth), this.Dash.ToImageSharpDash(this.LineWidth), false, this.LineJoin.ToImageSharpJoint(), this.LineCap.ToImageSharpCap())));
                }
                else
                {
                    this.ClipBuffer.Mutate(x => x.Clear(Color.FromRgba(0, 0, 0, 0)));
                    this.ClipBuffer.Mutate(x => x.Fill(new DrawingOptions() { Transform = _transform.ToMatrix() }, this.StrokeStyle.ToImageSharpBrush(_transform), this.currentPath.Build().GenerateOutline((float)(this.LineWidth), this.Dash.ToImageSharpDash(this.LineWidth), false, this.LineJoin.ToImageSharpJoint(), this.LineCap.ToImageSharpCap())));

                    GraphicsOptions opt = new GraphicsOptions() { AlphaCompositionMode = SixLabors.ImageSharp.PixelFormats.PixelAlphaCompositionMode.SrcIn };

                    this.Buffer.Dispose();

                    this.Buffer = this.CurrentClip.Clone(x => x.CheckedDrawImage(this.ClipBuffer, opt));

                    this.Image.Mutate(x => x.CheckedDrawImage(this.Buffer, 1));
                }
            }

            this.currentPath = new PathBuilder();
            this.figureInitialised = false;
        }

        public void StrokeText(string text, double x, double y)
        {
            PathText(text, x, y);
            Stroke();
        }

        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            Utils.CoerceNaNAndInfinityToZero(ref a, ref b, ref c, ref d, ref e, ref f);

            double[,] transfMatrix = new double[3, 3] { { a, c, e }, { b, d, f }, { 0, 0, 1 } };
            _transform = MatrixUtils.Multiply(_transform, transfMatrix);

            currentPath = new PathBuilder();
            figureInitialised = false;
        }

        public void Translate(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            _transform = MatrixUtils.Translate(_transform, x, y);

            currentPath = new PathBuilder();
            figureInitialised = false;
        }

        public void DrawFilteredGraphics(Graphics graphics, IFilter filter)
        {
            double scale = this.scaleFactor * Math.Sqrt(MatrixUtils.Determinant(_transform));

            Rectangle bounds = graphics.GetBounds();

            bounds = new Rectangle(bounds.Location.X - filter.TopLeftMargin.X, bounds.Location.Y - filter.TopLeftMargin.Y, bounds.Size.Width + filter.TopLeftMargin.X + filter.BottomRightMargin.X, bounds.Size.Height + filter.TopLeftMargin.Y + filter.BottomRightMargin.Y);

            if (bounds.Size.Width > 0 && bounds.Size.Height > 0)
            {
                bool rasterisationNeeded = true;

                if (filter is GaussianBlurFilter gauss)
                {
                    rasterisationNeeded = false;

                    Page pag = new Page(1, 1);
                    pag.Graphics.DrawGraphics(0, 0, graphics);
                    pag.Crop(bounds.Location, bounds.Size);

                    Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img = ImageSharpContextInterpreter.SaveAsImage(pag, scale);
                    img.Mutate(x => x.GaussianBlur((float)(gauss.StandardDeviation * scale)));

                    DrawImage(0, 0, img.Width, img.Height, bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, true, img);

                    img.Dispose();
                }
                else if (filter is ColourMatrixFilter cmf)
                {
                    rasterisationNeeded = false;

                    Page pag = new Page(1, 1);
                    pag.Graphics.DrawGraphics(0, 0, graphics);
                    pag.Crop(bounds.Location, bounds.Size);

                    Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img = ImageSharpContextInterpreter.SaveAsImage(pag, scale);

                    img.Mutate(x => x.Filter(new ColorMatrix((float)cmf.ColourMatrix.R1, (float)cmf.ColourMatrix.G1, (float)cmf.ColourMatrix.B1, (float)cmf.ColourMatrix.A1, (float)cmf.ColourMatrix.R2, (float)cmf.ColourMatrix.G2, (float)cmf.ColourMatrix.B2, (float)cmf.ColourMatrix.A2, (float)cmf.ColourMatrix.R3, (float)cmf.ColourMatrix.G3, (float)cmf.ColourMatrix.B3, (float)cmf.ColourMatrix.A3, (float)cmf.ColourMatrix.R4, (float)cmf.ColourMatrix.G4, (float)cmf.ColourMatrix.B4, (float)cmf.ColourMatrix.A4, (float)cmf.ColourMatrix.R5, (float)cmf.ColourMatrix.G5, (float)cmf.ColourMatrix.B5, (float)cmf.ColourMatrix.A5)));

                    DrawImage(0, 0, img.Width, img.Height, bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, true, img);

                    img.Dispose();
                }
                else if (filter is BoxBlurFilter bbf)
                {
                    rasterisationNeeded = false;

                    Page pag = new Page(1, 1);
                    pag.Graphics.DrawGraphics(0, 0, graphics);
                    pag.Crop(bounds.Location, bounds.Size);

                    Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img = ImageSharpContextInterpreter.SaveAsImage(pag, scale);

                    img.Mutate(x => x.BoxBlur((int)Math.Round(bbf.BoxRadius * scale)));

                    DrawImage(0, 0, img.Width, img.Height, bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, true, img);

                    img.Dispose();
                }
                else if (filter is CompositeLocationInvariantFilter comp)
                {
                    bool allSupported = true;

                    foreach (IFilter filter2 in comp.Filters)
                    {
                        if (!(filter2 is GaussianBlurFilter) && !(filter2 is ColourMatrixFilter) && !(filter2 is BoxBlurFilter))
                        {
                            allSupported = false;
                            break;
                        }
                    }

                    if (allSupported)
                    {
                        rasterisationNeeded = false;

                        Page pag = new Page(1, 1);
                        pag.Graphics.DrawGraphics(0, 0, graphics);
                        pag.Crop(bounds.Location, bounds.Size);

                        Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img = ImageSharpContextInterpreter.SaveAsImage(pag, scale);

                        foreach (IFilter filter2 in comp.Filters)
                        {
                            if (filter2 is GaussianBlurFilter gauss2)
                            {
                                img.Mutate(x => x.GaussianBlur((float)(gauss2.StandardDeviation * scale)));
                            }
                            else if (filter2 is ColourMatrixFilter cmf2)
                            {
                                img.Mutate(x => x.Filter(new ColorMatrix((float)cmf2.ColourMatrix.R1, (float)cmf2.ColourMatrix.G1, (float)cmf2.ColourMatrix.B1, (float)cmf2.ColourMatrix.A1, (float)cmf2.ColourMatrix.R2, (float)cmf2.ColourMatrix.G2, (float)cmf2.ColourMatrix.B2, (float)cmf2.ColourMatrix.A2, (float)cmf2.ColourMatrix.R3, (float)cmf2.ColourMatrix.G3, (float)cmf2.ColourMatrix.B3, (float)cmf2.ColourMatrix.A3, (float)cmf2.ColourMatrix.R4, (float)cmf2.ColourMatrix.G4, (float)cmf2.ColourMatrix.B4, (float)cmf2.ColourMatrix.A4, (float)cmf2.ColourMatrix.R5, (float)cmf2.ColourMatrix.G5, (float)cmf2.ColourMatrix.B5, (float)cmf2.ColourMatrix.A5)));
                            }
                            else if (filter2 is BoxBlurFilter bbf2)
                            {
                                img.Mutate(x => x.BoxBlur((int)Math.Round(bbf2.BoxRadius * scale)));
                            }
                        }


                        DrawImage(0, 0, img.Width, img.Height, bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, true, img);

                        img.Dispose();
                    }
                }

                if (rasterisationNeeded)
                {
                    RasterImage rasterised = ImageSharpContextInterpreter.Rasterise(graphics, bounds, scale, true);
                    RasterImage filtered = null;

                    if (filter is IFilterWithRasterisableParameter filterWithRastParam)
                    {
                        filterWithRastParam.RasteriseParameter(ImageSharpContextInterpreter.Rasterise, scale);
                    }

                    if (filter is ILocationInvariantFilter locInvFilter)
                    {
                        filtered = locInvFilter.Filter(rasterised, scale);
                    }
                    else if (filter is IFilterWithLocation filterWithLoc)
                    {
                        filtered = filterWithLoc.Filter(rasterised, bounds, scale);
                    }

                    if (filtered != null)
                    {
                        rasterised.Dispose();

                        DrawRasterImage(0, 0, filtered.Width, filtered.Height, bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, filtered);
                    }
                }
            }
        }
    }

    internal static class Utils
    {
        public static IImageProcessingContext CheckedDrawImage(this IImageProcessingContext x, Image image, SixLabors.ImageSharp.Point point, float opacity)
        {
            int x0A = 0;
            int y0A = 0;
            SixLabors.ImageSharp.Size size = x.GetCurrentSize();
            int x1A = size.Width;
            int y1A = size.Height;

            int x0B = point.X;
            int y0B = point.Y;
            int x1B = point.X + image.Width;
            int y1B = point.Y + image.Height;

            if (!(x0A >= x1B || x1A <= x0B || y0A >= y1B || y1A <= y0B))
            {
                return x.DrawImage(image, point, opacity);
            }
            else
            {
                return x;
            }
        }

        public static IImageProcessingContext CheckedDrawImage(this IImageProcessingContext x, Image image, float opacity)
        {
            return x.DrawImage(image, opacity);
        }

        public static IImageProcessingContext CheckedDrawImage(this IImageProcessingContext x, Image image, GraphicsOptions options)
        {
            return x.DrawImage(image, options);
        }

        public static PointF ToPointF(this Point pt, double scaleFactor)
        {
            return new PointF((float)(pt.X * scaleFactor), (float)(pt.Y * scaleFactor));
        }

        public static void CoerceNaNAndInfinityToZero(ref double val)
        {
            if (double.IsNaN(val) || double.IsInfinity(val) || val == double.MinValue || val == double.MaxValue)
            {
                val = 0;
            }
        }

        public static void CoerceNaNAndInfinityToZero(ref double val1, ref double val2)
        {
            if (double.IsNaN(val1) || double.IsInfinity(val1) || val1 == double.MinValue || val1 == double.MaxValue)
            {
                val1 = 0;
            }

            if (double.IsNaN(val2) || double.IsInfinity(val2) || val2 == double.MinValue || val2 == double.MaxValue)
            {
                val2 = 0;
            }
        }

        public static void CoerceNaNAndInfinityToZero(ref double val1, ref double val2, ref double val3, ref double val4)
        {
            if (double.IsNaN(val1) || double.IsInfinity(val1) || val1 == double.MinValue || val1 == double.MaxValue)
            {
                val1 = 0;
            }

            if (double.IsNaN(val2) || double.IsInfinity(val2) || val2 == double.MinValue || val2 == double.MaxValue)
            {
                val2 = 0;
            }

            if (double.IsNaN(val3) || double.IsInfinity(val3) || val3 == double.MinValue || val3 == double.MaxValue)
            {
                val3 = 0;
            }

            if (double.IsNaN(val4) || double.IsInfinity(val4) || val4 == double.MinValue || val4 == double.MaxValue)
            {
                val4 = 0;
            }
        }

        public static void CoerceNaNAndInfinityToZero(ref double val1, ref double val2, ref double val3, ref double val4, ref double val5, ref double val6)
        {
            if (double.IsNaN(val1) || double.IsInfinity(val1) || val1 == double.MinValue || val1 == double.MaxValue)
            {
                val1 = 0;
            }

            if (double.IsNaN(val2) || double.IsInfinity(val2) || val2 == double.MinValue || val2 == double.MaxValue)
            {
                val2 = 0;
            }

            if (double.IsNaN(val3) || double.IsInfinity(val3) || val3 == double.MinValue || val3 == double.MaxValue)
            {
                val3 = 0;
            }

            if (double.IsNaN(val4) || double.IsInfinity(val4) || val4 == double.MinValue || val4 == double.MaxValue)
            {
                val4 = 0;
            }

            if (double.IsNaN(val5) || double.IsInfinity(val5) || val5 == double.MinValue || val5 == double.MaxValue)
            {
                val5 = 0;
            }

            if (double.IsNaN(val6) || double.IsInfinity(val6) || val6 == double.MinValue || val6 == double.MaxValue)
            {
                val6 = 0;
            }
        }

        public static Color ToImageSharpColor(this Colour colour)
        {
            return Color.FromRgba((byte)(colour.R * 255), (byte)(colour.G * 255), (byte)(colour.B * 255), (byte)(colour.A * 255));
        }

        public static SixLabors.ImageSharp.Drawing.Processing.Brush ToImageSharpBrush(this Brush brush, double[,] transform)
        {
            if (brush is SolidColourBrush solid)
            {
                return new SixLabors.ImageSharp.Drawing.Processing.SolidBrush(solid.Colour.ToImageSharpColor());
            }
            else if (brush is LinearGradientBrush linear)
            {
                ColorStop[] colorStops = new ColorStop[linear.GradientStops.Count];

                for (int i = 0; i < linear.GradientStops.Count; i++)
                {
                    colorStops[i] = new ColorStop((float)linear.GradientStops[i].Offset, linear.GradientStops[i].Colour.ToImageSharpColor());
                }

                return new SixLabors.ImageSharp.Drawing.Processing.LinearGradientBrush(transform.Multiply(linear.StartPoint).ToPointF(1), transform.Multiply(linear.EndPoint).ToPointF(1), GradientRepetitionMode.None, colorStops);
            }
            else if (brush is RadialGradientBrush radial)
            {
                ColorStop[] colorStops = new ColorStop[radial.GradientStops.Count];

                for (int i = 0; i < radial.GradientStops.Count; i++)
                {
                    colorStops[i] = new ColorStop((float)radial.GradientStops[i].Offset, radial.GradientStops[i].Colour.ToImageSharpColor());
                }

                return new RadialGradientBrushSVGStyle(radial.Centre, radial.FocalPoint, radial.Radius, transform, GradientRepetitionMode.None, colorStops);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static float[] ToImageSharpDash(this LineDash dash, double lineThickness)
        {
            if (dash.UnitsOn > 0 || dash.UnitsOff > 0)
            {
                return new float[]
                {
                   (float)(dash.UnitsOn / lineThickness),
                    (float)(dash.UnitsOff / lineThickness)
                };
            }
            else
            {
                return new float[] { };
            }
        }

        public static JointStyle ToImageSharpJoint(this LineJoins join)
        {
            switch (join)
            {
                case LineJoins.Round:
                    return JointStyle.Round;
                case LineJoins.Miter:
                    return JointStyle.Miter;
                case LineJoins.Bevel:
                    return JointStyle.Square;
                default:
                    return JointStyle.Miter;
            }
        }

        public static EndCapStyle ToImageSharpCap(this LineCaps cap)
        {
            switch (cap)
            {
                case LineCaps.Square:
                    return EndCapStyle.Square;
                case LineCaps.Round:
                    return EndCapStyle.Round;
                case LineCaps.Butt:
                    return EndCapStyle.Butt;
                default:
                    throw new NotImplementedException();
            }
        }

        internal class RadialGradientBrushSVGStyle : SixLabors.ImageSharp.Drawing.Processing.GradientBrush
        {
            private readonly Point Centre;
            private readonly Point FocalPoint;
            private readonly double Radius;
            private readonly double[,] InverseTransform;

            public RadialGradientBrushSVGStyle(Point centre, Point focalPoint, double radius, double[,] transform, GradientRepetitionMode repetitionMode, params ColorStop[] colorStops) : base(repetitionMode, colorStops)
            {
                Centre = centre;
                FocalPoint = focalPoint;
                Radius = radius;
                InverseTransform = MatrixUtils.Invert(transform);
            }

            public override BrushApplicator<TPixel> CreateApplicator<TPixel>(Configuration configuration, GraphicsOptions options, ImageFrame<TPixel> source, RectangleF region)
            {
                return new RadialGradientBrushPDFStyleApplicator<TPixel>(configuration, options, source, Centre, FocalPoint, Radius, InverseTransform, ColorStops, RepetitionMode);
            }

            private class RadialGradientBrushPDFStyleApplicator<TPixel> : GradientBrushApplicator<TPixel> where TPixel : unmanaged, SixLabors.ImageSharp.PixelFormats.IPixel<TPixel>
            {
                private readonly Point Centre;
                private readonly Point FocalPoint;
                private readonly double Radius;
                private readonly double[,] InverseTransform;

                private readonly double a;

                public RadialGradientBrushPDFStyleApplicator(
                    Configuration configuration,
                    GraphicsOptions options,
                    ImageFrame<TPixel> target,
                    Point centre, Point focalPoint, double radius, double[,] inverseTransform,
                    ColorStop[] colorStops,
                    GradientRepetitionMode repetitionMode)
                : base(configuration, options, target, colorStops, repetitionMode)
                {
                    this.Centre = centre;
                    this.FocalPoint = focalPoint;
                    this.Radius = radius;
                    this.InverseTransform = inverseTransform;

                    a = (centre.X - focalPoint.X) * (centre.X - focalPoint.X) + (centre.Y - focalPoint.Y) * (centre.Y - focalPoint.Y) - radius * radius;
                }

                Random rnd = new Random();

                protected override float PositionOnGradient(float x, float y)
                {
                    Point realPoint = InverseTransform.Multiply(new Point(x, y));

                    double c = (realPoint.X - FocalPoint.X) * (realPoint.X - FocalPoint.X) + (realPoint.Y - FocalPoint.Y) * (realPoint.Y - FocalPoint.Y);

                    double halfB = -((realPoint.X - FocalPoint.X) * (Centre.X - FocalPoint.X) + (realPoint.Y - FocalPoint.Y) * (Centre.Y - FocalPoint.Y));

                    double sqrt = Math.Sqrt(halfB * halfB - a * c);

                    double tbr1 = (-halfB + sqrt) / a;
                    double tbr2 = (-halfB - sqrt) / a;

                    if (tbr1 >= 0 && tbr2 < 0)
                    {
                        return (float)tbr1;
                    }
                    else if (tbr1 < 0 && tbr2 >= 0)
                    {
                        return (float)tbr2;
                    }
                    else if (tbr1 < 0 && tbr2 < 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return (float)Math.Min(tbr1, tbr2);
                    }
                }

                /// <inheritdoc/>
                public override void Apply(Span<float> scanline, int x, int y)
                {
                    base.Apply(scanline, x, y);
                }
            }
        }
    }

    /// <summary>
    /// Enumeration containing the supported output formats.
    /// </summary>
    public enum OutputFormats
    {
        /// <summary>
        /// Windows bitmap format
        /// </summary>
        BMP,

        /// <summary>
        /// Graphics interchange format
        /// </summary>
        GIF,

        /// <summary>
        /// Joint photographic experts group format
        /// </summary>
        JPEG,


        /// <summary>
        /// Portable bitmap format
        /// </summary>
        PBM,

        /// <summary>
        /// Portable network graphics format
        /// </summary>
        PNG,

        /// <summary>
        /// Truevision graphics adapter format
        /// </summary>
        TGA,

        /// <summary>
        /// Tag image file format
        /// </summary>
        TIFF,

        /// <summary>
        /// WebP format
        /// </summary>
        WebP
    }

    /// <summary>
    /// Contains methods to render a <see cref="Page"/> to an <see cref="Image"/>.
    /// </summary>
    public static class ImageSharpContextInterpreter
    {
        /// <summary>
        /// Render the page to an <see cref="Image"/> object.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="scale">The scale to be used when rasterising the page. This will determine the width and height of the <see cref="Image"/>.</param>
        /// <returns>An <see cref="Image"/> containing the rasterised page.</returns>
        public static Image<SixLabors.ImageSharp.PixelFormats.Rgba32> SaveAsImage(this Page page, double scale = 1)
        {
            ImageSharpContext ctx = new ImageSharpContext(page.Width, page.Height, scale, page.Background);
            page.Graphics.CopyToIGraphicsContext(ctx);
            ctx.DisposeAllExceptImage();
            return ctx.Image;
        }


        /// <summary>
        /// Render the page to an image stream.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="imageStream">The <see cref="Stream"/> on which the image data will be written.</param>
        /// <param name="outputFormat">The format of the image that will be created.</param>
        /// <param name="scale">The scale to be used when rasterising the page. This will determine the width and height of the image.</param>
        public static void SaveAsImage(this Page page, Stream imageStream, OutputFormats outputFormat, double scale = 1)
        {
            Image image = SaveAsImage(page, scale);

            switch (outputFormat)
            {
                case OutputFormats.BMP:
                    image.SaveAsBmp(imageStream);
                    break;
                case OutputFormats.GIF:
                    image.SaveAsGif(imageStream);
                    break;
                case OutputFormats.JPEG:
                    image.SaveAsJpeg(imageStream);
                    break;
                case OutputFormats.PBM:
                    image.SaveAsPbm(imageStream);
                    break;
                case OutputFormats.PNG:
                    image.SaveAsPng(imageStream);
                    break;
                case OutputFormats.TGA:
                    image.SaveAsTga(imageStream);
                    break;
                case OutputFormats.TIFF:
                    image.SaveAsTiff(imageStream);
                    break;
                case OutputFormats.WebP:
                    image.SaveAsWebp(imageStream);
                    break;
            }

            image.Dispose();
        }

        /// <summary>
        /// The exception that is raised when the output file format is not specified and the file name does not have an extension corresponding to a known file format.
        /// </summary>
        public class UnknownFormatException : Exception
        {
            /// <summary>
            /// The extension of the file that does not correspond to any known file format.
            /// </summary>
            public string Format { get; }

            internal UnknownFormatException(string format) : base("The extension " + format + " does not correspond to any known file format!")
            {
                this.Format = format;
            }
        }

        /// <summary>
        /// Render the page to an image file.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="fileName">The path of the file where the image will be saved.</param>
        /// <param name="outputFormat">The format of the image that will be created. If this is <see langword="null" /> (the default), the format is desumed from the extension of the file.</param>
        /// <param name="scale">The scale to be used when rasterising the page. This will determine the width and height of the image.</param>
        public static void SaveAsImage(this Page page, string fileName, OutputFormats? outputFormat = null, double scale = 1)
        {
            OutputFormats actualOutputFormat;

            if (outputFormat.HasValue)
            {
                actualOutputFormat = outputFormat.Value;
            }
            else
            {
                string extension = System.IO.Path.GetExtension(fileName).ToLower();

                switch (extension)
                {
                    case ".bmp":
                    case ".dib":
                        actualOutputFormat = OutputFormats.BMP;
                        break;

                    case ".gif":
                        actualOutputFormat = OutputFormats.GIF;
                        break;

                    case ".jpeg":
                    case ".jpg":
                        actualOutputFormat = OutputFormats.JPEG;
                        break;

                    case ".pbm":
                        actualOutputFormat = OutputFormats.PBM;
                        break;

                    case ".png":
                        actualOutputFormat = OutputFormats.PNG;
                        break;

                    case ".tga":
                    case ".targa":
                        actualOutputFormat = OutputFormats.TGA;
                        break;

                    case ".tif":
                    case ".tiff":
                        actualOutputFormat = OutputFormats.TIFF;
                        break;

                    case ".webp":
                        actualOutputFormat = OutputFormats.WebP;
                        break;

                    default:
                        throw new UnknownFormatException(extension);
                }
            }

            using (FileStream imageStream = new FileStream(fileName, FileMode.Create))
            {
                SaveAsImage(page, imageStream, actualOutputFormat, scale);
            }
        }

        /// <summary>
        /// Render the page to raw pixel data, in 32bpp RGBA format.
        /// </summary>
        /// <param name="pag">The <see cref="Page"/> to render.</param>
        /// <param name="scale">The scale to be used when rasterising the page. This will determine the width and height of the image.</param>
        /// <param name="width">The width of the rendered image.</param>
        /// <param name="height">The height of the rendered image.</param>
        /// <param name="totalSize">The size in bytes of the raw pixel data.</param>
        /// <returns>A <see cref="DisposableIntPtr"/> containing a pointer to the raw pixel data, stored in unmanaged memory. Dispose this object to release the unmanaged memory.</returns>
        public static DisposableIntPtr SaveAsRawBytes(this Page pag, out int width, out int height, out int totalSize, double scale = 1)
        {
            Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img = SaveAsImage(pag, scale);

            int stride = img.Width * 4;
            int size = stride * img.Height;

            IntPtr tbr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            GC.AddMemoryPressure(size);

            IntPtr pointer = tbr;

            unsafe
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Memory<SixLabors.ImageSharp.PixelFormats.Rgba32> row = img.DangerousGetPixelRowMemory(y);

                    Span<SixLabors.ImageSharp.PixelFormats.Rgba32> newRow = new Span<SixLabors.ImageSharp.PixelFormats.Rgba32>(pointer.ToPointer(), row.Length);
                    row.Span.CopyTo(newRow);

                    pointer = IntPtr.Add(pointer, stride);
                }
            }

            width = img.Width;
            height = img.Height;
            totalSize = size;

            img.Dispose();

            return new DisposableIntPtr(tbr);
        }


        /// <summary>
        /// Return the page to raw pixel data, in 32bpp RGBA format.
        /// </summary>
        /// <param name="pag">The <see cref="Page"/> to render.</param>
        /// <param name="scale">The scale to be used when rasterising the page. This will determine the width and height of the image.</param>
        /// <param name="width">The width of the rendered image.</param>
        /// <param name="height">The height of the rendered image.</param>
        /// <returns>A byte array containing the raw pixel data.</returns>
        public static byte[] SaveAsRawBytes(this Page pag, out int width, out int height, double scale = 1)
        {
            Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img = SaveAsImage(pag, scale);

            int stride = img.Width * 4;
            int size = stride * img.Height;

            byte[] tbr = new byte[size];

            unsafe
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Memory<SixLabors.ImageSharp.PixelFormats.Rgba32> row = img.DangerousGetPixelRowMemory(y);

                    Span<byte> bytes = System.Runtime.InteropServices.MemoryMarshal.Cast<SixLabors.ImageSharp.PixelFormats.Rgba32, byte>(row.Span);
                    Span<byte> newRow = new Span<byte>(tbr, y * stride, stride);
                    bytes.CopyTo(newRow);
                }
            }

            width = img.Width;
            height = img.Height;

            img.Dispose();

            return tbr;
        }

        /// <summary>
        /// Rasterise a region of a <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object that will be rasterised.</param>
        /// <param name="region">The region of the <paramref name="graphics"/> that will be rasterised.</param>
        /// <param name="scale">The scale at which the image will be rendered.</param>
        /// <param name="interpolate">Whether the resulting image should be interpolated or not when it is drawn on another <see cref="Graphics"/> surface.</param>
        /// <returns>A <see cref="RasterImage"/> containing the rasterised graphics.</returns>
        public static RasterImage Rasterise(this Graphics graphics, Rectangle region, double scale, bool interpolate)
        {
            Page pag = new Page(1, 1);
            pag.Graphics.DrawGraphics(0, 0, graphics);
            pag.Crop(region.Location, region.Size);

            Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img = SaveAsImage(pag, scale);

            int stride = img.Width * 4;
            int size = stride * img.Height;

            IntPtr tbr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            GC.AddMemoryPressure(size);

            IntPtr pointer = tbr;

            unsafe
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Memory<SixLabors.ImageSharp.PixelFormats.Rgba32> row = img.DangerousGetPixelRowMemory(y);

                    Span<SixLabors.ImageSharp.PixelFormats.Rgba32> newRow = new Span<SixLabors.ImageSharp.PixelFormats.Rgba32>(pointer.ToPointer(), row.Length);
                    row.Span.CopyTo(newRow);

                    pointer = IntPtr.Add(pointer, stride);
                }
            }

            int width = img.Width;
            int height = img.Height;

            img.Dispose();

            DisposableIntPtr disp = new DisposableIntPtr(tbr);

            return new RasterImage(ref disp, width, height, true, interpolate);
        }

        /// <summary>
        /// Saves the animation to an animated GIF.
        /// </summary>
        /// <param name="animation">The animation to export.</param>
        /// <param name="scale">The scale at which the animation will be rendered.</param>
        /// <param name="frameRate">The target frame rate of the animation, in frames-per-second (fps). This is capped by the animated GIF specification at 50 fps.</param>
        /// <param name="durationScaling">A scaling factor that will be applied to all durations in the animation. Values greater than 1 slow down the animation, values smaller than 1 accelerate it. Note that this does not affect the frame rate of the animation.</param>
        /// <param name="colorTableMode">Determines whether a single colour table should be used for the whole image, or if a different colour table should be used for each frame.</param>
        /// <returns>An <see cref="Image"/> containing the animated GIF.</returns>
        public static Image SaveAsAnimatedGIF(this Animation animation, double scale = 1, double frameRate = 50, double durationScaling = 1, GifColorTableMode colorTableMode = GifColorTableMode.Local)
        {
            frameRate = Math.Min(frameRate, 50);

            Image gifAnimation = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>((int)(animation.Width * scale), (int)(animation.Height * scale));

            int frames = (int)Math.Ceiling(animation.Duration * frameRate * durationScaling / 1000);

            double accumulatedDelay = 0;

            for (int i = 0; i < frames; i++)
            {
                double frameTime = i / frameRate / durationScaling * 1000;
                double frameDuration = Math.Min(animation.Duration - i / durationScaling / frameRate * 1000, 1000 / frameRate / durationScaling);

                int gifFrameDuration = (int)Math.Round(frameDuration * durationScaling * 0.1 + accumulatedDelay);

                if (gifFrameDuration < 2)
                {
                    accumulatedDelay += frameDuration * durationScaling * 0.1;
                }
                else
                {
                    accumulatedDelay += frameDuration * durationScaling * 0.1 - gifFrameDuration;

                    Page pag = animation.GetFrameAtAbsolute(frameTime);

                    Image frame = pag.SaveAsImage(scale);

                    GifFrameMetadata frameMetadata = frame.Frames[0].Metadata.GetFormatMetadata(GifFormat.Instance);
                    frameMetadata.FrameDelay = gifFrameDuration;
                    frameMetadata.DisposalMethod = GifDisposalMethod.RestoreToPrevious;
                    gifAnimation.Frames.AddFrame(frame.Frames[0]);
                }
            }

            if ((int)accumulatedDelay > 0)
            {
                gifAnimation.Frames[gifAnimation.Frames.Count - 1].Metadata.GetFormatMetadata(GifFormat.Instance).FrameDelay += (int)accumulatedDelay;
            }

            gifAnimation.Frames.RemoveFrame(0);

            GifMetadata metadata = gifAnimation.Metadata.GetFormatMetadata(GifFormat.Instance);
            metadata.ColorTableMode = colorTableMode;
            metadata.RepeatCount = (ushort)animation.RepeatCount;

            return gifAnimation;
        }

        /// <summary>
        /// Saves the animation to an animated GIF stream.
        /// </summary>
        /// <param name="animation">The animation to export.</param>
        /// <param name="imageStream">The stream on which the animated GIF will be written.</param>
        /// <param name="scale">The scale at which the animation will be rendered.</param>
        /// <param name="frameRate">The target frame rate of the animation, in frames-per-second (fps). This is capped by the animated GIF specification at 50 fps.</param>
        /// <param name="durationScaling">A scaling factor that will be applied to all durations in the animation. Values greater than 1 slow down the animation, values smaller than 1 accelerate it. Note that this does not affect the frame rate of the animation.</param>
        /// <param name="colorTableMode">Determines whether a single colour table should be used for the whole image, or if a different colour table should be used for each frame.</param>
        public static void SaveAsAnimatedGIF(this Animation animation, Stream imageStream, double scale = 1, double frameRate = 50, double durationScaling = 1, GifColorTableMode colorTableMode = GifColorTableMode.Local)
        {
            Image gifAnimation = SaveAsAnimatedGIF(animation, scale, frameRate, durationScaling, colorTableMode);

            gifAnimation.SaveAsGif(imageStream);
        }

        /// <summary>
        /// Saves the animation to an animated GIF file.
        /// </summary>
        /// <param name="animation">The animation to export.</param>
        /// <param name="fileName">The output file to create.</param>
        /// <param name="scale">The scale at which the animation will be rendered.</param>
        /// <param name="frameRate">The target frame rate of the animation, in frames-per-second (fps). This is capped by the animated GIF specification at 50 fps.</param>
        /// <param name="durationScaling">A scaling factor that will be applied to all durations in the animation. Values greater than 1 slow down the animation, values smaller than 1 accelerate it. Note that this does not affect the frame rate of the animation.</param>
        /// <param name="colorTableMode">Determines whether a single colour table should be used for the whole image, or if a different colour table should be used for each frame.</param>
        public static void SaveAsAnimatedGIF(this Animation animation, string fileName, double scale = 1, double frameRate = 50, double durationScaling = 1, GifColorTableMode colorTableMode = GifColorTableMode.Local)
        {
            using (FileStream fs = File.Create(fileName))
            {
                SaveAsAnimatedGIF(animation, fs, scale, frameRate, durationScaling, colorTableMode);
            }
        }

        /// <summary>
        /// Saves the animation to a stream in animated PNG format.
        /// </summary>
        /// <param name="animation">The animation to export.</param>
        /// <param name="imageStream">The stream on which the animated PNG will be written.</param>
        /// <param name="scale">The scale at which the animation will be rendered.</param>
        /// <param name="frameRate">The target frame rate of the animation, in frames-per-second (fps). This is capped by the animated PNG specification at 90 fps.</param>
        /// <param name="durationScaling">A scaling factor that will be applied to all durations in the animation. Values greater than 1 slow down the animation, values smaller than 1 accelerate it. Note that this does not affect the frame rate of the animation.</param>
        /// <param name="interframeCompression">The kind of compression that will be used to reduce file size. Note that if the animation has a transparent background, no compression can be performed, and the value of this parameter is ignored.</param>
        public static void SaveAsAnimatedPNG(this Animation animation, Stream imageStream, double scale = 1, double frameRate = 60, double durationScaling = 1, AnimatedPNG.InterframeCompression interframeCompression = AnimatedPNG.InterframeCompression.First)
        {
            frameRate = Math.Min(frameRate, 90);

            int frames = (int)Math.Ceiling(animation.Duration * frameRate * durationScaling / 1000);

            Stream fs = imageStream;

            AnimatedPNG.CompressedFrame[] compressedFrames = new AnimatedPNG.CompressedFrame[frames];

            int width = (int)(animation.Width * scale);
            int height = (int)(animation.Height * scale);

            if (animation.Background.A < 1 || interframeCompression == AnimatedPNG.InterframeCompression.None)
            {
                Parallel.For(0, frames, i =>
                {
                    double frameTime = i / frameRate / durationScaling * 1000;

                    double frameDuration = Math.Min((animation.Duration - frameTime) * durationScaling, 1000 / frameRate);

                    Page pag = animation.GetFrameAtAbsolute(frameTime);

                    using (DisposableIntPtr rawFrame = pag.SaveAsRawBytes(out _, out _, out _, scale))
                    {
                        compressedFrames[i] = new AnimatedPNG.CompressedFrame(rawFrame, width, height, true, frameDuration);
                    }
                });
            }
            else if (interframeCompression == AnimatedPNG.InterframeCompression.First)
            {
                DisposableIntPtr firstFrame = animation.GetFrameAtAbsolute(0).SaveAsRawBytes(out _, out _, out _, scale);
                compressedFrames[0] = new AnimatedPNG.CompressedFrame(firstFrame, width, height, true, Math.Min(animation.Duration * durationScaling, 1000 / frameRate));

                Parallel.For(1, frames, i =>
                {
                    double frameTime = i / frameRate / durationScaling * 1000;

                    double frameDuration = Math.Min((animation.Duration - frameTime) * durationScaling, 1000 / frameRate);

                    Page pag = animation.GetFrameAtAbsolute(frameTime);

                    using (DisposableIntPtr rawFrame = pag.SaveAsRawBytes(out _, out _, out _, scale))
                    {
                        compressedFrames[i] = new AnimatedPNG.CompressedFrame(rawFrame, firstFrame, true, width, height, true, frameDuration);
                    }
                });

                firstFrame.Dispose();
            }
            else if (interframeCompression == AnimatedPNG.InterframeCompression.Previous)
            {
                DisposableIntPtr[] rawFrames = new DisposableIntPtr[frames];

                Parallel.For(0, frames, i =>
                {
                    double frameTime = i / frameRate / durationScaling * 1000;
                    Page pag = animation.GetFrameAtAbsolute(frameTime);
                    rawFrames[i] = pag.SaveAsRawBytes(out _, out _, out _, scale);
                });

                compressedFrames[0] = new AnimatedPNG.CompressedFrame(rawFrames[0], width, height, true, Math.Min(animation.Duration * durationScaling, 1000 / frameRate));

                Parallel.For(1, frames, i =>
                {
                    double frameTime = i / frameRate / durationScaling * 1000;
                    double frameDuration = Math.Min((animation.Duration - frameTime) * durationScaling, 1000 / frameRate);
                    compressedFrames[i] = new AnimatedPNG.CompressedFrame(rawFrames[i], rawFrames[i - 1], false, width, height, true, frameDuration);
                });
            }

            AnimatedPNG.Create(fs, (int)(animation.Width * scale), (int)(animation.Height * scale), true, compressedFrames, animation.RepeatCount);

            for (int i = 0; i < compressedFrames.Length; i++)
            {
                compressedFrames[i].Dispose();
            }
        }

        /// <summary>
        /// Saves the animation to an animated PNG file.
        /// </summary>
        /// <param name="animation">The animation to export.</param>
        /// <param name="fileName">The output file to create.</param>
        /// <param name="scale">The scale at which the animation will be rendered.</param>
        /// <param name="frameRate">The target frame rate of the animation, in frames-per-second (fps). This is capped by the animated PNG specification at 90 fps.</param>
        /// <param name="durationScaling">A scaling factor that will be applied to all durations in the animation. Values greater than 1 slow down the animation, values smaller than 1 accelerate it. Note that this does not affect the frame rate of the animation.</param>
        /// <param name="interframeCompression">The kind of compression that will be used to reduce file size. Note that if the animation has a transparent background, no compression can be performed, and the value of this parameter is ignored.</param>
        public static void SaveAsAnimatedPNG(this Animation animation, string fileName, double scale = 1, double frameRate = 60, double durationScaling = 1, AnimatedPNG.InterframeCompression interframeCompression = AnimatedPNG.InterframeCompression.First)
        {
            using (FileStream fs = File.Create(fileName))
            {
                SaveAsAnimatedPNG(animation, fs, scale, frameRate, durationScaling, interframeCompression);
            }
        }
    }
}
