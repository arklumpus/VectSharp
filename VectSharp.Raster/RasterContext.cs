/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020  Giorgio Bianchini
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;

namespace VectSharp.Raster
{
    internal static class MatrixUtils
    {
        public static double[] Multiply(double[,] matrix, double[] vector)
        {
            double[] tbr = new double[2];

            tbr[0] = matrix[0, 0] * vector[0] + matrix[0, 1] * vector[1] + matrix[0, 2];
            tbr[1] = matrix[1, 0] * vector[0] + matrix[1, 1] * vector[1] + matrix[1, 2];

            return tbr;
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
    }


    internal enum SegmentType
    {
        Move, Line, CubicBezier, Close
    }

    internal abstract class Segment
    {
        public abstract SegmentType Type { get; }
        public Point[] Points { get; protected set; }

        public virtual Point Point
        {
            get
            {
                return Points[Points.Length - 1];
            }
        }

        public abstract Segment Clone();
    }

    internal class MoveSegment : Segment
    {
        public override SegmentType Type => SegmentType.Move;

        public MoveSegment(Point p)
        {
            this.Points = new Point[] { p };
        }

        public MoveSegment(double x, double y)
        {
            this.Points = new Point[] { new Point(x, y) };
        }

        public override Segment Clone()
        {
            return new MoveSegment(this.Point);
        }
    }

    internal class LineSegment : Segment
    {
        public override SegmentType Type => SegmentType.Line;

        public LineSegment(Point p)
        {
            this.Points = new Point[] { p };
        }

        public LineSegment(double x, double y)
        {
            this.Points = new Point[] { new Point(x, y) };
        }

        public override Segment Clone()
        {
            return new LineSegment(this.Point);
        }
    }

    internal class CloseSegment : Segment
    {
        public override SegmentType Type => SegmentType.Close;

        public CloseSegment() { }

        public override Segment Clone()
        {
            return new CloseSegment();
        }
    }

    internal class CubicBezierSegment : Segment
    {
        public override SegmentType Type => SegmentType.CubicBezier;
        public CubicBezierSegment(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            Points = new Point[] { new Point(x1, y1), new Point(x2, y2), new Point(x3, y3) };
        }

        public CubicBezierSegment(Point p1, Point p2, Point p3)
        {
            Points = new Point[] { p1, p2, p3 };
        }

        public override Segment Clone()
        {
            return new CubicBezierSegment(Points[0], Points[1], Points[2]);
        }
    }

    internal class PathFigure
    {
        public Colour? Fill { get; }
        public Colour? Stroke { get; }
        public double LineWidth { get; }

        public bool IsClosed { get; set; } = false;
        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }
        public Segment[] Segments { get; }

        public double[,] TransformMatrix { get; set; }

        public PathFigure(IEnumerable<Segment> segments, Colour? fill, Colour? stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, double[,] transform)
        {
            List<Segment> segs = new List<Segment>();

            foreach (Segment s in segments)
            {
                segs.Add(s.Clone());
            }

            this.Segments = segs.ToArray();

            Fill = fill;
            Stroke = stroke;
            LineWidth = lineWidth;
            LineCap = lineCap;
            LineJoin = lineJoin;
            LineDash = lineDash;

            TransformMatrix = (double[,])transform.Clone();
        }
    }


    internal class RasterContext : IGraphicsContext
    {
        public string Tag { get; set; }
        public double Width { get; }
        public double Height { get; }


        private List<Segment> _currentFigure;

        internal List<PathFigure> _figures;

        private Colour _strokeStyle;
        private Colour _fillStyle;
        private LineDash _lineDash;

        private double[,] _transform;

        private readonly Stack<double[,]> states;

        public RasterContext(double width, double height)
        {
            this.Width = width;
            this.Height = height;

            _currentFigure = new List<Segment>();
            _figures = new List<PathFigure>();

            _strokeStyle = Colour.FromRgb(0, 0, 0);
            _fillStyle = Colour.FromRgb(0, 0, 0);
            LineWidth = 1;

            LineCap = LineCaps.Butt;
            LineJoin = LineJoins.Miter;
            _lineDash = new LineDash(0, 0, 0);

            Font = new Font(new FontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);

            TextBaseline = TextBaselines.Top;

            _transform = new double[3, 3];

            _transform[0, 0] = 1;
            _transform[1, 1] = 1;
            _transform[2, 2] = 1;

            states = new Stack<double[,]>();
        }


        public void MoveTo(double x, double y)
        {
            _currentFigure.Add(new MoveSegment(x, y));
        }

        public void LineTo(double x, double y)
        {
            _currentFigure.Add(new LineSegment(x, y));
        }

        public void Close()
        {
            _currentFigure.Add(new CloseSegment());
        }

        public void Rectangle(double x0, double y0, double width, double height)
        {
            MoveTo(x0, y0);
            LineTo(x0 + width, y0);
            LineTo(x0 + width, y0 + height);
            LineTo(x0, y0 + height);
            Close();
        }
        public void SetStrokeStyle((int r, int g, int b, double a) style)
        {
            _strokeStyle = Colour.FromRgba(style.r, style.g, style.b, style.a);
        }

        public void SetStrokeStyle(Colour style)
        {
            _strokeStyle = style;
        }

        public void SetFillStyle((int r, int g, int b, double a) style)
        {
            _fillStyle = Colour.FromRgba(style.r, style.g, style.b, style.a);
        }

        public void SetFillStyle(Colour style)
        {
            _fillStyle = style;
        }


        public Colour FillStyle { get { return _fillStyle; } }
        public Colour StrokeStyle { get { return _strokeStyle; } }

        public double LineWidth { get; set; }

        public LineCaps LineCap { get; set; }
        public LineJoins LineJoin { get; set; }

        public void Fill()
        {
            if (_currentFigure.Count > 0)
            {
                _figures.Add(new PathFigure(_currentFigure, _fillStyle, null, 0, LineCaps.Butt, LineJoins.Bevel, new LineDash(0, 0, 0), _transform));
            }
            _currentFigure = new List<Segment>();
        }

        public void Stroke()
        {
            if (_currentFigure.Count > 0)
            {
                _figures.Add(new PathFigure(_currentFigure, null, _strokeStyle, LineWidth, LineCap, LineJoin, _lineDash, _transform) { IsClosed = _currentFigure.Last().Type == SegmentType.Close });
            }
            _currentFigure = new List<Segment>();
        }

        public void CubicBezierTo(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            _currentFigure.Add(new CubicBezierSegment(x1, y1, x2, y2, x3, y3));
        }

        public void SetLineDash(LineDash dash)
        {
            _lineDash = dash;
        }

        public Font Font { get; set; }

        private void PathText(string text, double x, double y)
        {
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

        public void FillText(string text, double x, double y)
        {
            PathText(text, x, y);
            Fill();
        }

        public TextBaselines TextBaseline { get; set; }

        public void Restore()
        {
            _transform = states.Pop();
        }

        public void Rotate(double angle)
        {
            _transform = MatrixUtils.Rotate(_transform, angle);
        }

        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            double[,] transfMatrix = new double[3, 3] { { a, c, e }, { b, d, f }, { 0, 0, 1 } };
            _transform = MatrixUtils.Multiply(_transform, transfMatrix);
        }

        public void Save()
        {
            states.Push((double[,])_transform.Clone());
        }


        public void StrokeText(string text, double x, double y)
        {
            PathText(text, x, y);
            Stroke();
        }

        public void Translate(double x, double y)
        {
            _transform = MatrixUtils.Translate(_transform, x, y);
        }

        public void Scale(double scaleX, double scaleY)
        {
            _transform = MatrixUtils.Scale(_transform, scaleX, scaleY);
        }
    }

    internal static class ImageFormats
    {
        public static byte[,,] AllocateImage(int width, int height, Colour backgroundColour)
        {
            byte r = (byte)(backgroundColour.R * 255);
            byte g = (byte)(backgroundColour.G * 255);
            byte b = (byte)(backgroundColour.B * 255);
            byte a = (byte)(backgroundColour.A * 255);

            byte[,,] tbr = new byte[width, height, 4];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tbr[x, y, 0] = r;
                    tbr[x, y, 1] = g;
                    tbr[x, y, 2] = b;
                    tbr[x, y, 3] = a;
                }
            }

            return tbr;
        }

        public static void SavePNG(byte[,,] image, Stream fs)
        {
            //Header
            fs.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);

            int width = image.GetLength(0);
            int height = image.GetLength(1);

            //IHDR chunk
            fs.WriteInt(13);
            using (MemoryStream ihdr = new MemoryStream(13))
            {
                ihdr.WriteASCIIString("IHDR");
                ihdr.WriteInt(width);
                ihdr.WriteInt(height);
                ihdr.WriteByte(8); //Bit depth
                ihdr.WriteByte(6); //Colour type
                ihdr.WriteByte(0); //Compression method
                ihdr.WriteByte(0); //Filter method
                ihdr.WriteByte(0); //Interlace

                ihdr.Seek(0, SeekOrigin.Begin);
                ihdr.CopyTo(fs);

                fs.WriteUInt(CRC32.ComputeCRC(ihdr));
            }

            //IDAT chunk
            using (MemoryStream filteredStream = FilterImageData(image))
            {
                using (MemoryStream compressedImage = filteredStream.ZLibCompress())
                {
                    using (MemoryStream idat = new MemoryStream((int)compressedImage.Length + 4))
                    {
                        compressedImage.Seek(0, SeekOrigin.Begin);
                        idat.WriteASCIIString("IDAT");
                        compressedImage.CopyTo(idat);

                        fs.WriteUInt((uint)compressedImage.Length);

                        idat.Seek(0, SeekOrigin.Begin);
                        idat.CopyTo(fs);

                        fs.WriteUInt(CRC32.ComputeCRC(idat));
                    }
                }
            }

            //IEND chunk
            fs.WriteInt(0);
            fs.WriteASCIIString("IEND");
            fs.Write(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, 0, 4);

        }

        internal enum FilterModes
        {
            None = 0,
            Sub = 1,
            Up = 2,
            Average = 3,
            Paeth = 4,
            Adaptive = -1
        }

        internal static MemoryStream FilterImageData(byte[,,] image, FilterModes filter = FilterModes.Adaptive)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);

            MemoryStream tbr = new MemoryStream(height * (1 + width * 4));

            if (filter == FilterModes.None)
            {
                for (int y = 0; y < height; y++)
                {
                    tbr.WriteByte(0);
                    for (int x = 0; x < width; x++)
                    {
                        tbr.WriteByte(image[x, y, 0]);
                        tbr.WriteByte(image[x, y, 1]);
                        tbr.WriteByte(image[x, y, 2]);
                        tbr.WriteByte(image[x, y, 3]);
                    }
                }
            }
            else if (filter == FilterModes.Sub)
            {
                for (int y = 0; y < height; y++)
                {
                    tbr.WriteByte(1);
                    for (int x = 0; x < width; x++)
                    {
                        if (x > 0)
                        {
                            tbr.WriteByte((byte)(image[x, y, 0] - image[x - 1, y, 0]));
                            tbr.WriteByte((byte)(image[x, y, 1] - image[x - 1, y, 1]));
                            tbr.WriteByte((byte)(image[x, y, 2] - image[x - 1, y, 2]));
                            tbr.WriteByte((byte)(image[x, y, 3] - image[x - 1, y, 3]));
                        }
                        else
                        {
                            tbr.WriteByte(image[x, y, 0]);
                            tbr.WriteByte(image[x, y, 1]);
                            tbr.WriteByte(image[x, y, 2]);
                            tbr.WriteByte(image[x, y, 3]);
                        }
                    }
                }
            }
            else if (filter == FilterModes.Up)
            {
                for (int y = 0; y < height; y++)
                {
                    tbr.WriteByte(2);
                    for (int x = 0; x < width; x++)
                    {
                        if (y > 0)
                        {
                            tbr.WriteByte((byte)(image[x, y, 0] - image[x, y - 1, 0]));
                            tbr.WriteByte((byte)(image[x, y, 1] - image[x, y - 1, 1]));
                            tbr.WriteByte((byte)(image[x, y, 2] - image[x, y - 1, 2]));
                            tbr.WriteByte((byte)(image[x, y, 3] - image[x, y - 1, 3]));
                        }
                        else
                        {
                            tbr.WriteByte(image[x, y, 0]);
                            tbr.WriteByte(image[x, y, 1]);
                            tbr.WriteByte(image[x, y, 2]);
                            tbr.WriteByte(image[x, y, 3]);
                        }
                    }
                }
            }
            else if (filter == FilterModes.Average)
            {
                for (int y = 0; y < height; y++)
                {
                    tbr.WriteByte(3);
                    for (int x = 0; x < width; x++)
                    {
                        if (x > 0 && y > 0)
                        {
                            tbr.WriteByte((byte)(image[x, y, 0] - (image[x - 1, y, 0] + image[x, y - 1, 0]) / 2));
                            tbr.WriteByte((byte)(image[x, y, 1] - (image[x - 1, y, 1] + image[x, y - 1, 1]) / 2));
                            tbr.WriteByte((byte)(image[x, y, 2] - (image[x - 1, y, 2] + image[x, y - 1, 2]) / 2));
                            tbr.WriteByte((byte)(image[x, y, 3] - (image[x - 1, y, 3] + image[x, y - 1, 3]) / 2));
                        }
                        else if (x > 0 && y == 0)
                        {
                            tbr.WriteByte((byte)(image[x, y, 0] - image[x - 1, y, 0] / 2));
                            tbr.WriteByte((byte)(image[x, y, 1] - image[x - 1, y, 1] / 2));
                            tbr.WriteByte((byte)(image[x, y, 2] - image[x - 1, y, 2] / 2));
                            tbr.WriteByte((byte)(image[x, y, 3] - image[x - 1, y, 3] / 2));
                        }
                        else if (x == 0 && y > 0)
                        {
                            tbr.WriteByte((byte)(image[x, y, 0] - image[x, y - 1, 0] / 2));
                            tbr.WriteByte((byte)(image[x, y, 1] - image[x, y - 1, 1] / 2));
                            tbr.WriteByte((byte)(image[x, y, 2] - image[x, y - 1, 2] / 2));
                            tbr.WriteByte((byte)(image[x, y, 3] - image[x, y - 1, 3] / 2));
                        }
                        else
                        {
                            tbr.WriteByte(image[x, y, 0]);
                            tbr.WriteByte(image[x, y, 1]);
                            tbr.WriteByte(image[x, y, 2]);
                            tbr.WriteByte(image[x, y, 3]);
                        }
                    }
                }
            }
            else if (filter == FilterModes.Paeth)
            {
                for (int y = 0; y < height; y++)
                {
                    tbr.WriteByte(4);
                    for (int x = 0; x < width; x++)
                    {
                        if (x > 0 && y > 0)
                        {
                            tbr.WriteByte((byte)(image[x, y, 0] - PaethPredictor(image[x - 1, y, 0], image[x, y - 1, 0], image[x - 1, y - 1, 0])));
                            tbr.WriteByte((byte)(image[x, y, 1] - PaethPredictor(image[x - 1, y, 1], image[x, y - 1, 1], image[x - 1, y - 1, 1])));
                            tbr.WriteByte((byte)(image[x, y, 2] - PaethPredictor(image[x - 1, y, 2], image[x, y - 1, 2], image[x - 1, y - 1, 2])));
                            tbr.WriteByte((byte)(image[x, y, 3] - PaethPredictor(image[x - 1, y, 3], image[x, y - 1, 3], image[x - 1, y - 1, 3])));
                        }
                        else if (x > 0 && y == 0)
                        {
                            tbr.WriteByte((byte)(image[x, y, 0] - image[x - 1, y, 0]));
                            tbr.WriteByte((byte)(image[x, y, 1] - image[x - 1, y, 1]));
                            tbr.WriteByte((byte)(image[x, y, 2] - image[x - 1, y, 2]));
                            tbr.WriteByte((byte)(image[x, y, 3] - image[x - 1, y, 3]));
                        }
                        else if (x == 0 && y > 0)
                        {
                            tbr.WriteByte((byte)(image[x, y, 0] - image[x, y - 1, 0]));
                            tbr.WriteByte((byte)(image[x, y, 1] - image[x, y - 1, 1]));
                            tbr.WriteByte((byte)(image[x, y, 2] - image[x, y - 1, 2]));
                            tbr.WriteByte((byte)(image[x, y, 3] - image[x, y - 1, 3]));
                        }
                        else
                        {
                            tbr.WriteByte(image[x, y, 0]);
                            tbr.WriteByte(image[x, y, 1]);
                            tbr.WriteByte(image[x, y, 2]);
                            tbr.WriteByte(image[x, y, 3]);
                        }
                    }
                }
            }
            else if (filter == FilterModes.Adaptive)
            {
                for (int y = 0; y < height; y++)
                {
                    byte[] none = new byte[width * 4];
                    double noneSum = 0;

                    byte[] sub = new byte[width * 4];
                    double subSum = 0;

                    byte[] up = new byte[width * 4];
                    double upSum = 0;

                    byte[] average = new byte[width * 4];
                    double averageSum = 0;

                    byte[] paeth = new byte[width * 4];
                    double paethSum = 0;

                    for (int x = 0; x < width; x++)
                    {
                        //None
                        none[4 * x] = image[x, y, 0];
                        none[4 * x + 1] = image[x, y, 1];
                        none[4 * x + 2] = image[x, y, 2];
                        none[4 * x + 3] = image[x, y, 3];

                        noneSum += image[x, y, 0] + image[x, y, 1] + image[x, y, 2] + image[x, y, 3];

                        //Sub
                        if (x > 0)
                        {
                            sub[4 * x] = (byte)(image[x, y, 0] - image[x - 1, y, 0]);
                            sub[4 * x + 1] = (byte)(image[x, y, 1] - image[x - 1, y, 1]);
                            sub[4 * x + 2] = (byte)(image[x, y, 2] - image[x - 1, y, 2]);
                            sub[4 * x + 3] = (byte)(image[x, y, 3] - image[x - 1, y, 3]);

                            subSum += Math.Abs(image[x, y, 0] - image[x - 1, y, 0]) + Math.Abs(image[x, y, 1] - image[x - 1, y, 1]) + Math.Abs(image[x, y, 2] - image[x - 1, y, 2]) + Math.Abs(image[x, y, 3] - image[x - 1, y, 3]);
                        }
                        else
                        {
                            sub[4 * x] = image[x, y, 0];
                            sub[4 * x + 1] = image[x, y, 1];
                            sub[4 * x + 2] = image[x, y, 2];
                            sub[4 * x + 3] = image[x, y, 3];

                            subSum += image[x, y, 0] + image[x, y, 1] + image[x, y, 2] + image[x, y, 3];
                        }

                        //Up
                        if (y > 0)
                        {
                            up[4 * x] = (byte)(image[x, y, 0] - image[x, y - 1, 0]);
                            up[4 * x + 1] = (byte)(image[x, y, 1] - image[x, y - 1, 1]);
                            up[4 * x + 2] = (byte)(image[x, y, 2] - image[x, y - 1, 2]);
                            up[4 * x + 3] = (byte)(image[x, y, 3] - image[x, y - 1, 3]);

                            upSum += Math.Abs(image[x, y, 0] - image[x, y - 1, 0]) + Math.Abs(image[x, y, 1] - image[x, y - 1, 1]) + Math.Abs(image[x, y, 2] - image[x, y - 1, 2]) + Math.Abs(image[x, y, 3] - image[x, y - 1, 3]);
                        }
                        else
                        {
                            up[4 * x] = image[x, y, 0];
                            up[4 * x + 1] = image[x, y, 1];
                            up[4 * x + 2] = image[x, y, 2];
                            up[4 * x + 3] = image[x, y, 3];

                            upSum += image[x, y, 0] + image[x, y, 1] + image[x, y, 2] + image[x, y, 3];
                        }


                        //Average
                        if (x > 0 && y > 0)
                        {
                            average[4 * x] = (byte)(image[x, y, 0] - (image[x - 1, y, 0] + image[x, y - 1, 0]) / 2);
                            average[4 * x + 1] = (byte)(image[x, y, 1] - (image[x - 1, y, 1] + image[x, y - 1, 1]) / 2);
                            average[4 * x + 2] = (byte)(image[x, y, 2] - (image[x - 1, y, 2] + image[x, y - 1, 2]) / 2);
                            average[4 * x + 3] = (byte)(image[x, y, 3] - (image[x - 1, y, 3] + image[x, y - 1, 3]) / 2);

                            averageSum += Math.Abs(image[x, y, 0] - (image[x - 1, y, 0] + image[x, y - 1, 0]) / 2) + Math.Abs(image[x, y, 1] - (image[x - 1, y, 1] + image[x, y - 1, 1]) / 2) + Math.Abs(image[x, y, 2] - (image[x - 1, y, 2] + image[x, y - 1, 2]) / 2) + Math.Abs(image[x, y, 3] - (image[x - 1, y, 3] + image[x, y - 1, 3]) / 2);
                        }
                        else if (x > 0 && y == 0)
                        {
                            average[4 * x] = (byte)(image[x, y, 0] - image[x - 1, y, 0] / 2);
                            average[4 * x + 1] = (byte)(image[x, y, 1] - image[x - 1, y, 1] / 2);
                            average[4 * x + 2] = (byte)(image[x, y, 2] - image[x - 1, y, 2] / 2);
                            average[4 * x + 3] = (byte)(image[x, y, 3] - image[x - 1, y, 3] / 2);

                            averageSum += Math.Abs(image[x, y, 0] - image[x - 1, y, 0] / 2) + Math.Abs(image[x, y, 1] - image[x - 1, y, 1] / 2) + Math.Abs(image[x, y, 2] - image[x - 1, y, 2] / 2) + Math.Abs(image[x, y, 3] - image[x - 1, y, 3] / 2);
                        }
                        else if (x == 0 && y > 0)
                        {
                            average[4 * x] = (byte)(image[x, y, 0] - image[x, y - 1, 0] / 2);
                            average[4 * x + 1] = (byte)(image[x, y, 1] - image[x, y - 1, 1] / 2);
                            average[4 * x + 2] = (byte)(image[x, y, 2] - image[x, y - 1, 2] / 2);
                            average[4 * x + 3] = (byte)(image[x, y, 3] - image[x, y - 1, 3] / 2);

                            averageSum += Math.Abs(image[x, y, 0] - image[x, y - 1, 0] / 2) + Math.Abs(image[x, y, 1] - image[x, y - 1, 1] / 2) + Math.Abs(image[x, y, 2] - image[x, y - 1, 2] / 2) + Math.Abs(image[x, y, 3] - image[x, y - 1, 3] / 2);
                        }
                        else
                        {
                            average[4 * x] = image[x, y, 0];
                            average[4 * x + 1] = image[x, y, 1];
                            average[4 * x + 2] = image[x, y, 2];
                            average[4 * x + 3] = image[x, y, 3];

                            averageSum += image[x, y, 0] + image[x, y, 1] + image[x, y, 2] + image[x, y, 3];
                        }


                        //Paeth
                        if (x > 0 && y > 0)
                        {
                            paeth[4 * x] = (byte)(image[x, y, 0] - PaethPredictor(image[x - 1, y, 0], image[x, y - 1, 0], image[x - 1, y - 1, 0]));
                            paeth[4 * x + 1] = (byte)(image[x, y, 1] - PaethPredictor(image[x - 1, y, 1], image[x, y - 1, 1], image[x - 1, y - 1, 1]));
                            paeth[4 * x + 2] = (byte)(image[x, y, 2] - PaethPredictor(image[x - 1, y, 2], image[x, y - 1, 2], image[x - 1, y - 1, 2]));
                            paeth[4 * x + 3] = (byte)(image[x, y, 3] - PaethPredictor(image[x - 1, y, 3], image[x, y - 1, 3], image[x - 1, y - 1, 3]));

                            paethSum += Math.Abs(image[x, y, 0] - PaethPredictor(image[x - 1, y, 0], image[x, y - 1, 0], image[x - 1, y - 1, 0])) + Math.Abs(image[x, y, 1] - PaethPredictor(image[x - 1, y, 1], image[x, y - 1, 1], image[x - 1, y - 1, 1])) + Math.Abs(image[x, y, 2] - PaethPredictor(image[x - 1, y, 2], image[x, y - 1, 2], image[x - 1, y - 1, 2])) + Math.Abs(image[x, y, 3] - PaethPredictor(image[x - 1, y, 3], image[x, y - 1, 3], image[x - 1, y - 1, 3]));
                        }
                        else if (x > 0 && y == 0)
                        {
                            paeth[4 * x] = (byte)(image[x, y, 0] - image[x - 1, y, 0]);
                            paeth[4 * x + 1] = (byte)(image[x, y, 1] - image[x - 1, y, 1]);
                            paeth[4 * x + 2] = (byte)(image[x, y, 2] - image[x - 1, y, 2]);
                            paeth[4 * x + 3] = (byte)(image[x, y, 3] - image[x - 1, y, 3]);

                            paethSum += Math.Abs(image[x, y, 0] - image[x - 1, y, 0]) + Math.Abs(image[x, y, 1] - image[x - 1, y, 1]) + Math.Abs(image[x, y, 2] - image[x - 1, y, 2]) + Math.Abs(image[x, y, 3] - image[x - 1, y, 3]);
                        }
                        else if (x == 0 && y > 0)
                        {
                            paeth[4 * x] = (byte)(image[x, y, 0] - image[x, y - 1, 0]);
                            paeth[4 * x + 1] = (byte)(image[x, y, 1] - image[x, y - 1, 1]);
                            paeth[4 * x + 2] = (byte)(image[x, y, 2] - image[x, y - 1, 2]);
                            paeth[4 * x + 3] = (byte)(image[x, y, 3] - image[x, y - 1, 3]);

                            paethSum += Math.Abs(image[x, y, 0] - image[x, y - 1, 0]) + Math.Abs(image[x, y, 1] - image[x, y - 1, 1]) + Math.Abs(image[x, y, 2] - image[x, y - 1, 2]) + Math.Abs(image[x, y, 3] - image[x, y - 1, 3]);
                        }
                        else
                        {
                            paeth[4 * x] = image[x, y, 0];
                            paeth[4 * x + 1] = image[x, y, 1];
                            paeth[4 * x + 2] = image[x, y, 2];
                            paeth[4 * x + 3] = image[x, y, 3];

                            paethSum += image[x, y, 0] + image[x, y, 1] + image[x, y, 2] + image[x, y, 3];
                        }
                    }

                    double min = Math.Min(Math.Min(Math.Min(Math.Min(noneSum, subSum), upSum), averageSum), paethSum);

                    if (min == noneSum)
                    {
                        tbr.WriteByte(0);
                        tbr.Write(none, 0, none.Length);
                    }
                    else if (min == subSum)
                    {
                        tbr.WriteByte(1);
                        tbr.Write(sub, 0, sub.Length);
                    }
                    else if (min == upSum)
                    {
                        tbr.WriteByte(2);
                        tbr.Write(up, 0, up.Length);
                    }
                    else if (min == averageSum)
                    {
                        tbr.WriteByte(3);
                        tbr.Write(average, 0, average.Length);
                    }
                    else if (min == paethSum)
                    {
                        tbr.WriteByte(4);
                        tbr.Write(paeth, 0, paeth.Length);
                    }
                }
            }


            return tbr;
        }

        //Based on http://www.libpng.org/pub/png/spec/1.2/PNG-Filters.html
        internal static byte PaethPredictor(byte a, byte b, byte c)
        {
            int p = (int)a + (int)b - (int)c;
            int pa = Math.Abs(p - (int)a);
            int pb = Math.Abs(p - (int)b);
            int pc = Math.Abs(p - (int)c);

            if (pa <= pb && pa <= pc)
            {
                return a;
            }
            else if (pb <= pc)
            {
                return b;
            }
            else
            {
                return c;
            }
        }
    }

    //Derived from http://www.libpng.org/pub/png/spec/1.2/PNG-CRCAppendix.html
    internal static class CRC32
    {
        /* Table of CRCs of all 8-bit messages. */
        private static readonly uint[] crc_table = new uint[256];

        /* Flag: has the table been computed? Initially false. */
        private static bool crc_table_computed = false;

        /* Make the table for a fast CRC. */
        private static void MakeCRCtable()
        {
            uint c;
            int n, k;

            for (n = 0; n < 256; n++)
            {
                c = (uint)n;
                for (k = 0; k < 8; k++)
                {
                    if ((c & 1) != 0)
                    {
                        c = 0xedb88320 ^ (c >> 1);
                    }
                    else
                    {
                        c >>= 1;
                    }
                }
                crc_table[n] = c;
            }
            crc_table_computed = true;
        }

        /* Update a running CRC with the bytes buf[0..len-1]--the CRC
           should be initialized to all 1's, and the transmitted value
           is the 1's complement of the final running CRC (see the
           crc() routine below)). */

        private static uint UpdateCRC(uint crc, Stream buf)
        {
            uint c = crc;
            int n;

            if (!crc_table_computed)
            {
                MakeCRCtable();
            }

            buf.Seek(0, SeekOrigin.Begin);

            for (n = 0; n < buf.Length; n++)
            {
                c = crc_table[(c ^ (byte)buf.ReadByte()) & 0xff] ^ (c >> 8);
            }
            return c;
        }

        /* Return the CRC of the bytes buf[0..len-1]. */
        public static uint ComputeCRC(Stream buf)
        {
            return UpdateCRC(0xffffffff, buf) ^ 0xffffffff;
        }
    }

    internal static class StreamUtils
    {
        public static void WriteInt(this Stream sr, int val)
        {
            sr.WriteUInt((uint)val);
        }

        public static void WriteUInt(this Stream sr, uint val)
        {
            sr.Write(new byte[] { (byte)(val >> 24), (byte)((val >> 16) & 255), (byte)((val >> 8) & 255), (byte)(val & 255) }, 0, 4);
        }

        public static void WriteASCIIString(this Stream sr, string val)
        {
            foreach (char c in val)
            {
                sr.WriteByte((byte)c);
            }
        }

        internal static MemoryStream ZLibCompress(this Stream contentStream)
        {
            MemoryStream compressedStream = new MemoryStream();
            compressedStream.Write(new byte[] { 0x78, 0x01 }, 0, 2);
            contentStream.Seek(0, SeekOrigin.Begin);

            using (DeflateStream deflate = new DeflateStream(compressedStream, CompressionLevel.Optimal, true))
            {
                contentStream.CopyTo(deflate);
            }
            contentStream.Seek(0, SeekOrigin.Begin);

            uint checksum = Adler32(contentStream);

            compressedStream.Write(new byte[] { (byte)((checksum >> 24) & 255), (byte)((checksum >> 16) & 255), (byte)((checksum >> 8) & 255), (byte)(checksum & 255) }, 0, 4);

            compressedStream.Seek(0, SeekOrigin.Begin);

            return compressedStream;
        }

        internal static uint Adler32(Stream contentStream)
        {
            uint s1 = 1;
            uint s2 = 0;

            int readByte;

            while ((readByte = contentStream.ReadByte()) >= 0)
            {
                s1 = (s1 + (byte)readByte) % 65521U;
                s2 = (s2 + s1) % 65521U;
            }

            return (s2 << 16) + s1;
        }
    }

    /// <summary>
    /// Contains methods to render a <see cref="Page"/> as a raster image.
    /// </summary>
    public static class RasterContextInterpreter
    {

        /// <summary>
        /// Render the page to a PNG file.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="fileName">The full path to the file to save. If it exists, it will be overwritten.</param>
        /// <param name="scale">The scale to be used when rasterising the page. This will determine the width and height of the image file.</param>
        public static void SaveAsPNG(this Page page, string fileName, double scale = 1)
        {
            using (FileStream sr = new FileStream(fileName, FileMode.Create))
            {
                page.SaveAsPNG(sr, scale);
            }
        }

        /// <summary>
        /// Render the page to a PNG stream.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="stream">The stream to which the PNG data will be written.</param>
        /// <param name="scale">The scale to be used when rasterising the page. This will determine the width and height of the image file.</param>
        public static void SaveAsPNG(this Page page, Stream stream, double scale = 1)
        {
            RasterContext ctx = new RasterContext(page.Width, page.Height);
            page.Graphics.CopyToIGraphicsContext(ctx);

            byte[,,] image = ImageFormats.AllocateImage((int)(ctx.Width * scale), (int)(ctx.Height * scale), page.Background);

            int width = image.GetLength(0);
            int height = image.GetLength(1);

            double[,] scaleMatrix = new double[3, 3] { { scale, 0, 0 }, { 0, scale, 0 }, { 0, 0, 1 } };

            foreach (PathFigure fig in ctx._figures)
            {
                fig.TransformMatrix = MatrixUtils.Multiply(scaleMatrix, fig.TransformMatrix);

                if (fig.Fill != null)
                {
                    FillFigure(fig, image, width, height);
                }

                if (fig.Stroke != null)
                {
                    StrokeFigure(fig, image, width, height);
                }
            }

            ImageFormats.SavePNG(image, stream);
        }

        private static bool ClampToSegmentLeft(ref double pointX, ref double pointY, double startX, double startY, double endX, double endY)
        {
            double t;

            if (Math.Abs(endX - startX) > Math.Abs(endY - startY))
            {
                t = (pointX - startX) / (endX - startX);
            }
            else
            {
                t = (pointY - startY) / (endY - startY);
            }

            if (t < 0)
            {
                t = Math.Max(0, Math.Min(t, 1));
                pointX = startX + t * (endX - startX);
                pointY = startY + t * (endY - startY);
                return true;
            }
            return false;
        }

        private static bool ClampToSegmentRight(ref double pointX, ref double pointY, double startX, double startY, double endX, double endY)
        {
            double t;

            if (Math.Abs(endX - startX) > Math.Abs(endY - startY))
            {
                t = (pointX - startX) / (endX - startX);
            }
            else
            {
                t = (pointY - startY) / (endY - startY);
            }

            if (t > 1)
            {
                t = Math.Max(0, Math.Min(t, 1));
                pointX = startX + t * (endX - startX);
                pointY = startY + t * (endY - startY);
                return true;
            }
            return false;
        }

        private static void StrokeThickSegment(double x0, double y0, double x1, double y1, byte[,] mask, int maskWidth, int maskHeight, double halfLineWidth, bool isFirst, bool isLast, LineCaps lineCap, LineJoins lineJoin, double nextX, double nextY, double[,] transformMatrix, int maskLeft, int maskTop, double scaleFactor)
        {
            if (Math.Abs(x0 - x1) < 1e-4)
            {
                x1 = x0;
            }

            if (Math.Abs(y0 - y1) < 1e-4)
            {
                y1 = y0;
            }

            if (Math.Abs(x0 - nextX) < 1e-4)
            {
                nextX = x0;
            }

            if (Math.Abs(y0 - nextY) < 1e-4)
            {
                nextY = y0;
            }

            if (isFirst && lineCap == LineCaps.Square)
            {
                if (y1 != y0)
                {
                    double m = (y1 - y0) / (x1 - x0);
                    double term = halfLineWidth / Math.Sqrt(1 + m * m);

                    if (x1 > x0)
                    {
                        x0 -= term;
                        y0 -= m * term;
                    }
                    else
                    {
                        x0 += term;
                        y0 += m * term;
                    }
                }
                else
                {
                    if (x1 > x0)
                    {
                        x0 -= halfLineWidth;
                    }
                    else
                    {
                        x0 += halfLineWidth;
                    }
                }
            }


            if (isLast && lineCap == LineCaps.Square)
            {
                if (y1 != y0)
                {
                    double m = (y1 - y0) / (x1 - x0);
                    double term = halfLineWidth / Math.Sqrt(1 + m * m);

                    if (x1 > x0)
                    {
                        x1 += term;
                        y1 += m * term;
                    }
                    else
                    {
                        x1 -= term;
                        y1 -= m * term;
                    }
                }
                else
                {
                    if (x1 > x0)
                    {
                        x1 += halfLineWidth;
                    }
                    else
                    {
                        x1 -= halfLineWidth;
                    }
                }
            }


            List<double[]> realPoints = new List<double[]>(5);

            double x2, x3, x4, x5, y2, y3, y4, y5;

            if (y1 != y0)
            {
                double mp = -(x1 - x0) / (y1 - y0);
                double term = halfLineWidth / Math.Sqrt(1 + mp * mp);

                x2 = x0 + term;
                y2 = y0 + mp * (x2 - x0);

                x5 = x0 - term;
                y5 = y0 + mp * (x5 - x0);

                x3 = x1 + term;
                y3 = y1 + mp * (x3 - x1);

                x4 = x1 - term;
                y4 = y1 + mp * (x4 - x1);
            }
            else
            {
                x2 = x0;
                y2 = y0 + halfLineWidth;

                x5 = x0;
                y5 = y0 - halfLineWidth;

                x3 = x1;
                y3 = y1 + halfLineWidth;

                x4 = x1;
                y4 = y1 - halfLineWidth;
            }

            double minX = Math.Min(Math.Min(x2, x3), Math.Min(x4, x5));
            double maxX = Math.Max(Math.Max(x2, x3), Math.Max(x4, x5));

            double minY = Math.Min(Math.Min(y2, y3), Math.Min(y4, y5));
            double maxY = Math.Max(Math.Max(y2, y3), Math.Max(y4, y5));

            realPoints.Add(new double[] { x2, y2 });
            realPoints.Add(new double[] { x3, y3 });

            List<double[]> roundCap = new List<double[]>();

            if ((isLast && lineCap == LineCaps.Round) || (!isLast && lineJoin == LineJoins.Round))
            {
                int count = (int)Math.Ceiling(2 * Math.PI * halfLineWidth / scaleFactor);
                double multiplier = 2 * Math.PI / count;

                for (int ang = 0; ang <= count; ang++)
                {
                    double[] pt = new double[] { x1 + halfLineWidth * Math.Cos(multiplier * ang), y1 + halfLineWidth * Math.Sin(multiplier * ang) };
                    roundCap.Add(pt);
                    minX = Math.Min(minX, pt[0]);
                    maxX = Math.Max(maxX, pt[0]);
                    minY = Math.Min(minY, pt[1]);
                    maxY = Math.Max(maxY, pt[1]);
                }
            }

            realPoints.Add(new double[] { x4, y4 });
            realPoints.Add(new double[] { x5, y5 });

            if ((isFirst && lineCap == LineCaps.Round) || (!isFirst && lineJoin == LineJoins.Round))
            {
                int count = (int)Math.Ceiling(2 * Math.PI * halfLineWidth / scaleFactor);
                double multiplier = 2 * Math.PI / count;

                for (int ang = 0; ang <= count; ang++)
                {
                    double[] pt = new double[] { x0 + halfLineWidth * Math.Cos(multiplier * ang), y0 + halfLineWidth * Math.Sin(multiplier * ang) };
                    roundCap.Add(pt);
                    minX = Math.Min(minX, pt[0]);
                    maxX = Math.Max(maxX, pt[0]);
                    minY = Math.Min(minY, pt[1]);
                    maxY = Math.Max(maxY, pt[1]);
                }
            }

            realPoints.Add(new double[] { x2, y2 });

            List<double[]> joiner = new List<double[]>(9);

            if (!isLast && lineJoin == LineJoins.Bevel && (nextY != y1 || nextX != x1))
            {
                double nextX2, nextY2, nextX5, nextY5;

                if (nextY != y1)
                {
                    double mp = -(nextX - x1) / (nextY - y1);
                    double term = halfLineWidth / Math.Sqrt(1 + mp * mp);

                    nextX2 = x1 + term;
                    nextY2 = y1 + mp * (nextX2 - x1);

                    nextX5 = x1 - term;
                    nextY5 = y1 + mp * (nextX5 - x1);
                }
                else
                {
                    nextX2 = x1;
                    nextY2 = y1 + halfLineWidth;

                    nextX5 = x1;
                    nextY5 = y1 - halfLineWidth;
                }

                joiner.Add(new double[] { nextX2, nextY2 });
                joiner.Add(new double[] { x3, y3 });
                joiner.Add(new double[] { nextX5, nextY5 });
                joiner.Add(new double[] { x4, y4 });
                joiner.Add(new double[] { nextX2, nextY2 });

                minX = Math.Min(minX, Math.Min(nextX2, nextX5));
                maxX = Math.Max(maxX, Math.Max(nextX2, nextX5));
                minY = Math.Min(minY, Math.Min(nextY2, nextY5));
                maxY = Math.Max(maxY, Math.Max(nextY2, nextY5));
            }

            if (!isLast && lineJoin == LineJoins.Miter && (nextY != y1 || nextX != x1) && (y0 != y1 || x0 != x1))
            {
                double nextX2, nextY2, nextX3, nextY3, nextX4, nextY4, nextX5, nextY5;
                double x6, y6, x7, y7, x8, y8, x9, y9;

                if (nextY != y1)
                {
                    double mp = -(nextX - x1) / (nextY - y1);
                    double term = halfLineWidth / Math.Sqrt(1 + mp * mp);

                    nextX2 = x1 + term;
                    nextY2 = y1 + mp * (nextX2 - x1);

                    nextX5 = x1 - term;
                    nextY5 = y1 + mp * (nextX5 - x1);

                    nextX3 = nextX + term;
                    nextY3 = nextY + mp * (nextX3 - nextX);

                    nextX4 = nextX - term;
                    nextY4 = nextY + mp * (nextX4 - nextX);
                }
                else
                {
                    nextX2 = x1;
                    nextY2 = y1 + halfLineWidth;

                    nextX5 = x1;
                    nextY5 = y1 - halfLineWidth;

                    nextX3 = nextX;
                    nextY3 = nextY + halfLineWidth;

                    nextX4 = nextX;
                    nextY4 = nextY - halfLineWidth;
                }

                if (nextX != x1 && x1 != x0)
                {
                    double m0 = (y1 - y0) / (x1 - x0);
                    double m1 = (nextY - y1) / (nextX - x1);

                    if (m0 != m1)
                    {
                        x6 = (nextY5 - y4 - m1 * nextX5 + m0 * x4) / (m0 - m1);
                        y6 = y4 + m0 * (x6 - x4);

                        x7 = (nextY2 - y3 - m1 * nextX2 + m0 * x3) / (m0 - m1);
                        y7 = y3 + m0 * (x7 - x3);

                        x8 = (nextY5 - y3 - m1 * nextX5 + m0 * x3) / (m0 - m1);
                        y8 = y3 + m0 * (x8 - x3);


                        x9 = (nextY2 - y4 - m1 * nextX2 + m0 * x4) / (m0 - m1);
                        y9 = y4 + m0 * (x9 - x4);

                        if (!ClampToSegmentLeft(ref x6, ref y6, x5, y5, x4, y4))
                        {
                            ClampToSegmentRight(ref x6, ref y6, nextX5, nextY5, nextX4, nextY4);
                        }

                        if (!ClampToSegmentLeft(ref x7, ref y7, x2, y2, x3, y3))
                        {
                            ClampToSegmentRight(ref x7, ref y7, nextX2, nextY2, nextX3, nextY3);
                        }

                        if (!ClampToSegmentLeft(ref x8, ref y8, x2, y2, x3, y3))
                        {
                            ClampToSegmentRight(ref x8, ref y8, nextX5, nextY5, nextX4, nextY4);
                        }

                        if (!ClampToSegmentLeft(ref x9, ref y9, x5, y5, x4, y4))
                        {
                            ClampToSegmentRight(ref x9, ref y9, nextX2, nextY2, nextX3, nextY3);
                        }
                    }
                    else
                    {
                        x6 = double.NegativeInfinity;
                        y6 = double.NegativeInfinity;

                        x7 = double.NegativeInfinity;
                        y7 = double.NegativeInfinity;

                        x8 = double.NegativeInfinity;
                        y8 = double.NegativeInfinity;

                        x9 = double.NegativeInfinity;
                        y9 = double.NegativeInfinity;
                    }
                }
                else if (nextX != x1 && x1 == x0)
                {
                    double m1 = (nextY - y1) / (nextX - x1);

                    x6 = x1 - halfLineWidth;
                    y6 = nextY5 + m1 * (x6 - nextX5);

                    x7 = x1 + halfLineWidth;
                    y7 = nextY2 + m1 * (x7 - nextX2);

                    x8 = x1 + halfLineWidth;
                    y8 = nextY5 + m1 * (x8 - nextX5);

                    x9 = x1 - halfLineWidth;
                    y9 = nextY2 + m1 * (x9 - nextX2);

                    if (!ClampToSegmentLeft(ref x6, ref y6, x5, y5, x4, y4))
                    {
                        ClampToSegmentRight(ref x6, ref y6, nextX5, nextY5, nextX4, nextY4);
                    }

                    if (!ClampToSegmentLeft(ref x7, ref y7, x2, y2, x3, y3))
                    {
                        ClampToSegmentRight(ref x7, ref y7, nextX2, nextY2, nextX3, nextY3);
                    }

                    if (!ClampToSegmentLeft(ref x8, ref y8, x2, y2, x3, y3))
                    {
                        ClampToSegmentRight(ref x8, ref y8, nextX5, nextY5, nextX4, nextY4);
                    }

                    if (!ClampToSegmentLeft(ref x9, ref y9, x5, y5, x4, y4))
                    {
                        ClampToSegmentRight(ref x9, ref y9, nextX2, nextY2, nextX3, nextY3);
                    }
                }
                else if (nextX == x1 && x1 != x0)
                {
                    double m0 = (y1 - y0) / (x1 - x0);

                    x6 = x1 - halfLineWidth;
                    y6 = y4 + m0 * (x6 - x4);

                    x7 = x1 + halfLineWidth;
                    y7 = y3 + m0 * (x7 - x3);

                    x8 = x1 - halfLineWidth;
                    y8 = y3 + m0 * (x8 - x3);

                    x9 = x1 + halfLineWidth;
                    y9 = y4 + m0 * (x9 - x4);

                    if (!ClampToSegmentLeft(ref x6, ref y6, x5, y5, x4, y4))
                    {
                        ClampToSegmentRight(ref x6, ref y6, nextX5, nextY5, nextX4, nextY4);
                    }

                    if (!ClampToSegmentLeft(ref x7, ref y7, x2, y2, x3, y3))
                    {
                        ClampToSegmentRight(ref x7, ref y7, nextX2, nextY2, nextX3, nextY3);
                    }

                    if (!ClampToSegmentLeft(ref x8, ref y8, x2, y2, x3, y3))
                    {
                        ClampToSegmentRight(ref x8, ref y8, nextX5, nextY5, nextX4, nextY4);
                    }

                    if (!ClampToSegmentLeft(ref x9, ref y9, x5, y5, x4, y4))
                    {
                        ClampToSegmentRight(ref x9, ref y9, nextX2, nextY2, nextX3, nextY3);
                    }
                }
                else
                {
                    x6 = double.NegativeInfinity;
                    y6 = double.NegativeInfinity;

                    x7 = double.NegativeInfinity;
                    y7 = double.NegativeInfinity;

                    x8 = double.NegativeInfinity;
                    y8 = double.NegativeInfinity;

                    x9 = double.NegativeInfinity;
                    y9 = double.NegativeInfinity;
                }

                if (x6 != double.NegativeInfinity && x7 != double.NegativeInfinity)
                {
                    joiner.Add(new double[] { nextX2, nextY2 });
                    joiner.Add(new double[] { x7, y7 });
                    joiner.Add(new double[] { x3, y3 });
                    joiner.Add(new double[] { x8, y8 });
                    joiner.Add(new double[] { nextX5, nextY5 });
                    joiner.Add(new double[] { x6, y6 });
                    joiner.Add(new double[] { x4, y4 });
                    joiner.Add(new double[] { x9, y9 });
                    joiner.Add(new double[] { nextX2, nextY2 });

                    minX = Math.Min(minX, Math.Min(nextX2, nextX5));
                    maxX = Math.Max(maxX, Math.Max(nextX2, nextX5));
                    minX = Math.Min(minX, Math.Min(x6, x7));
                    maxX = Math.Max(maxX, Math.Max(x6, x7));
                    minX = Math.Min(minX, Math.Min(x8, x9));
                    maxX = Math.Max(maxX, Math.Max(x8, x9));

                    minY = Math.Min(minY, Math.Min(nextY2, nextY5));
                    maxY = Math.Max(maxY, Math.Max(nextY2, nextY5));
                    minY = Math.Min(minY, Math.Min(y6, y7));
                    maxY = Math.Max(maxY, Math.Max(y6, y7));
                    minY = Math.Min(minY, Math.Min(y8, y9));
                    maxY = Math.Max(maxY, Math.Max(y8, y9));
                }
                else
                {
                    joiner.Add(new double[] { nextX2, nextY2 });
                    joiner.Add(new double[] { x3, y3 });
                    joiner.Add(new double[] { nextX5, nextY5 });
                    joiner.Add(new double[] { x4, y4 });
                    joiner.Add(new double[] { nextX2, nextY2 });

                    minX = Math.Min(minX, Math.Min(nextX2, nextX5));
                    maxX = Math.Max(maxX, Math.Max(nextX2, nextX5));
                    minY = Math.Min(minY, Math.Min(nextY2, nextY5));
                    maxY = Math.Max(maxY, Math.Max(nextY2, nextY5));
                }
            }

            //Apply transforms and remove duplicate points
            {
                for (int j = 0; j < realPoints.Count; j++)
                {
                    realPoints[j] = MatrixUtils.Multiply(transformMatrix, realPoints[j]);
                }

                List<double[]> currPerim = new List<double[]>();

                for (int j = 0; j < realPoints.Count; j++)
                {
                    if (currPerim.Count == 0 || (j == realPoints.Count - 1 && (Math.Abs(realPoints[j][0] - currPerim[currPerim.Count - 1][0]) >= 1e-4 || Math.Abs(realPoints[j][1] - currPerim[currPerim.Count - 1][1]) >= 1e-4)) || Math.Abs(realPoints[j][0] - currPerim[currPerim.Count - 1][0]) >= 1 || Math.Abs(realPoints[j][1] - currPerim[currPerim.Count - 1][1]) >= 1)
                    {
                        if (Math.Abs(Math.Round(realPoints[j][0]) - realPoints[j][0]) < 1e-2)
                        {
                            realPoints[j][0] = Math.Round(realPoints[j][0]);
                        }

                        if (Math.Abs(Math.Round(realPoints[j][1]) - realPoints[j][1]) < 1e-2)
                        {
                            realPoints[j][1] = Math.Round(realPoints[j][1]);
                        }

                        currPerim.Add(realPoints[j]);
                    }
                }

                realPoints = currPerim;
            }

            //Apply transforms and remove duplicate points
            {
                for (int j = 0; j < joiner.Count; j++)
                {
                    joiner[j] = MatrixUtils.Multiply(transformMatrix, joiner[j]);
                }

                List<double[]> currPerim = new List<double[]>();

                for (int j = 0; j < joiner.Count; j++)
                {
                    if (currPerim.Count == 0 || (j == joiner.Count - 1 && (Math.Abs(joiner[j][0] - currPerim[currPerim.Count - 1][0]) >= 1e-4 || Math.Abs(joiner[j][1] - currPerim[currPerim.Count - 1][1]) >= 1e-4)) || Math.Abs(joiner[j][0] - currPerim[currPerim.Count - 1][0]) >= 1 || Math.Abs(joiner[j][1] - currPerim[currPerim.Count - 1][1]) >= 1)
                    {
                        if (Math.Abs(Math.Round(joiner[j][0]) - joiner[j][0]) < 1e-2)
                        {
                            joiner[j][0] = Math.Round(joiner[j][0]);
                        }

                        if (Math.Abs(Math.Round(joiner[j][1]) - joiner[j][1]) < 1e-2)
                        {
                            joiner[j][1] = Math.Round(joiner[j][1]);
                        }

                        currPerim.Add(joiner[j]);
                    }
                }

                joiner = currPerim;
            }

            //Apply transforms and remove duplicate points
            {
                for (int j = 0; j < roundCap.Count; j++)
                {
                    roundCap[j] = MatrixUtils.Multiply(transformMatrix, roundCap[j]);
                }

                List<double[]> currPerim = new List<double[]>();

                for (int j = 0; j < roundCap.Count; j++)
                {
                    if (currPerim.Count == 0 || (j == roundCap.Count - 1 && (Math.Abs(roundCap[j][0] - currPerim[currPerim.Count - 1][0]) >= 1e-4 || Math.Abs(roundCap[j][1] - currPerim[currPerim.Count - 1][1]) >= 1e-4)) || Math.Abs(roundCap[j][0] - currPerim[currPerim.Count - 1][0]) >= 1 || Math.Abs(roundCap[j][1] - currPerim[currPerim.Count - 1][1]) >= 1)
                    {
                        if (Math.Abs(Math.Round(roundCap[j][0]) - roundCap[j][0]) < 1e-2)
                        {
                            roundCap[j][0] = Math.Round(roundCap[j][0]);
                        }

                        if (Math.Abs(Math.Round(roundCap[j][1]) - roundCap[j][1]) < 1e-2)
                        {
                            roundCap[j][1] = Math.Round(roundCap[j][1]);
                        }

                        currPerim.Add(roundCap[j]);
                    }
                }

                roundCap = currPerim;
            }

            double[] bound1 = new double[] { minX, minY };
            double[] bound2 = new double[] { minX, maxY };
            double[] bound3 = new double[] { maxX, maxY };
            double[] bound4 = new double[] { maxX, minY };

            bound1 = MatrixUtils.Multiply(transformMatrix, bound1);
            bound2 = MatrixUtils.Multiply(transformMatrix, bound2);
            bound3 = MatrixUtils.Multiply(transformMatrix, bound3);
            bound4 = MatrixUtils.Multiply(transformMatrix, bound4);

            minX = Math.Min(Math.Min(bound1[0], bound2[0]), Math.Min(bound3[0], bound4[0]));
            minY = Math.Min(Math.Min(bound1[1], bound2[1]), Math.Min(bound3[1], bound4[1]));
            maxX = Math.Max(Math.Max(bound1[0], bound2[0]), Math.Max(bound3[0], bound4[0]));
            maxY = Math.Max(Math.Max(bound1[1], bound2[1]), Math.Max(bound3[1], bound4[1]));

            for (int i = 1; i < realPoints.Count; i++)
            {
                XiaolinWuLineMask(realPoints[i - 1][0] - maskLeft, realPoints[i - 1][1] - maskTop, realPoints[i][0] - maskLeft, realPoints[i][1] - maskTop, mask);
            }

            for (int i = 1; i < joiner.Count; i++)
            {
                XiaolinWuLineMask(joiner[i - 1][0] - maskLeft, joiner[i - 1][1] - maskTop, joiner[i][0] - maskLeft, joiner[i][1] - maskTop, mask);
            }

            for (int i = 1; i < roundCap.Count; i++)
            {
                XiaolinWuLineMask(roundCap[i - 1][0] - maskLeft, roundCap[i - 1][1] - maskTop, roundCap[i][0] - maskLeft, roundCap[i][1] - maskTop, mask);
            }

            FillInsidePerimeters(new List<double[][]>() { realPoints.ToArray() }, minX, maxX, minY, maxY, maskWidth, maskHeight, mask, maskLeft, maskTop);
            FillInsidePerimeters(new List<double[][]>() { joiner.ToArray() }, minX, maxX, minY, maxY, maskWidth, maskHeight, mask, maskLeft, maskTop);
            FillInsidePerimeters(new List<double[][]>() { roundCap.ToArray() }, minX, maxX, minY, maxY, maskWidth, maskHeight, mask, maskLeft, maskTop);
        }

        private static void StrokeFigure(PathFigure fig, byte[,,] image, int width, int height)
        {
            List<double[][]> perimeters = new List<double[][]>();
            List<double[]> currFig = new List<double[]>();
            List<bool> joiners = new List<bool>();
            List<bool[]> perJoiners = new List<bool[]>();

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            double currMinX = double.MaxValue;
            double currMinY = double.MaxValue;
            double currMaxX = double.MinValue;
            double currMaxY = double.MinValue;

            byte R = (byte)(fig.Stroke?.R * 255);
            byte G = (byte)(fig.Stroke?.G * 255);
            byte B = (byte)(fig.Stroke?.B * 255);
            byte A = (byte)(fig.Stroke?.A * 255);

            double[] unitVector = MatrixUtils.Multiply(fig.TransformMatrix, new double[] { 1, 1 });
            double[] origin = MatrixUtils.Multiply(fig.TransformMatrix, new double[] { 0, 0 });
            double scaleFactor = 1 / Math.Sqrt(((unitVector[0] - origin[0]) * (unitVector[0] - origin[0]) + (unitVector[1] - origin[1]) * (unitVector[1] - origin[1])) / 2);

            foreach (Segment seg in fig.Segments)
            {
                if (seg.Type == SegmentType.Move)
                {
                    if (currFig.Count > 0)
                    {
                        perimeters.Add(currFig.ToArray());
                        perJoiners.Add(joiners.ToArray());
                    }
                    double[] pt = new double[] { (seg.Point.X), (seg.Point.Y) };

                    currFig = new List<double[]>() { pt };
                    joiners = new List<bool>() { true };

                    minX = Math.Min(minX, pt[0]);
                    maxX = Math.Max(maxX, pt[0]);
                    minY = Math.Min(minY, pt[1]);
                    maxY = Math.Max(maxY, pt[1]);

                    currMinX = pt[0];
                    currMaxX = pt[0];
                    currMinY = pt[1];
                    currMaxY = pt[1];
                }
                else if (seg.Type == SegmentType.Close)
                {
                    if (currFig.Count > 0)
                    {
                        if (currFig.Last()[0] != currFig[0][0] || currFig.Last()[1] != currFig[0][1])
                        {
                            currFig.Add(new double[] { currFig[0][0], currFig[0][1] });
                            joiners.Add(true);
                        }
                        else
                        {
                            joiners[joiners.Count - 1] = true;
                        }

                        if (fig.LineJoin == LineJoins.Miter && currFig.Count > 2)
                        {
                            double theta = 0.5 * Math.Abs(Math.Atan2(currFig[currFig.Count - 3][1] - currFig[currFig.Count - 2][1], currFig[currFig.Count - 3][0] - currFig[currFig.Count - 2][0]) - Math.Atan2(currFig[currFig.Count - 1][1] - currFig[currFig.Count - 2][1], currFig[currFig.Count - 1][0] - currFig[currFig.Count - 2][0]));

                            if (Math.Sin(theta) != 0)
                            {
                                double h = Math.Abs(fig.LineWidth * 0.5 / Math.Sin(theta));

                                minX = Math.Min(minX, currFig[currFig.Count - 2][0] - h);
                                maxX = Math.Max(maxX, currFig[currFig.Count - 2][0] + h);
                                minY = Math.Min(minY, currFig[currFig.Count - 2][1] - h);
                                maxY = Math.Max(maxY, currFig[currFig.Count - 2][1] + h);

                                currMinX = Math.Min(currMinX, currFig[currFig.Count - 2][0] - h);
                                currMaxX = Math.Max(currMaxX, currFig[currFig.Count - 2][0] + h);
                                currMinY = Math.Min(currMinY, currFig[currFig.Count - 2][1] - h);
                                currMaxY = Math.Max(currMaxY, currFig[currFig.Count - 2][1] + h);
                            }
                        }

                        if (fig.LineJoin == LineJoins.Miter && currFig.Count > 2)
                        {
                            double theta = 0.5 * Math.Abs(Math.Atan2(currFig[currFig.Count - 2][1] - currFig[currFig.Count - 1][1], currFig[currFig.Count - 2][0] - currFig[currFig.Count - 1][0]) - Math.Atan2(currFig[1][1] - currFig[currFig.Count - 1][1], currFig[1][0] - currFig[currFig.Count - 1][0]));

                            if (Math.Sin(theta) != 0)
                            {
                                double h = Math.Abs(fig.LineWidth * 0.5 / Math.Sin(theta));

                                minX = Math.Min(minX, currFig[currFig.Count - 1][0] - h);
                                maxX = Math.Max(maxX, currFig[currFig.Count - 1][0] + h);
                                minY = Math.Min(minY, currFig[currFig.Count - 1][1] - h);
                                maxY = Math.Max(maxY, currFig[currFig.Count - 1][1] + h);

                                currMinX = Math.Min(currMinX, currFig[currFig.Count - 1][0] - h);
                                currMaxX = Math.Max(currMaxX, currFig[currFig.Count - 1][0] + h);
                                currMinY = Math.Min(currMinY, currFig[currFig.Count - 1][1] - h);
                                currMaxY = Math.Max(currMaxY, currFig[currFig.Count - 1][1] + h);
                            }
                        }


                        perimeters.Add(currFig.ToArray());
                        perJoiners.Add(joiners.ToArray());
                    }
                    currFig = new List<double[]>();
                    joiners = new List<bool>();
                }
                if (seg.Type == SegmentType.Line)
                {
                    double[] pt = new double[] { (seg.Point.X), (seg.Point.Y) };

                    currFig.Add(pt);
                    joiners.Add(true);

                    minX = Math.Min(minX, pt[0]);
                    maxX = Math.Max(maxX, pt[0]);
                    minY = Math.Min(minY, pt[1]);
                    maxY = Math.Max(maxY, pt[1]);

                    currMinX = Math.Min(currMinX, pt[0]);
                    currMaxX = Math.Max(currMaxX, pt[0]);
                    currMinY = Math.Min(currMinY, pt[1]);
                    currMaxY = Math.Max(currMaxY, pt[1]);

                    if (fig.LineJoin == LineJoins.Miter && currFig.Count > 2)
                    {
                        double theta = 0.5 * Math.Abs(Math.Atan2(currFig[currFig.Count - 3][1] - currFig[currFig.Count - 2][1], currFig[currFig.Count - 3][0] - currFig[currFig.Count - 2][0]) - Math.Atan2(currFig[currFig.Count - 1][1] - currFig[currFig.Count - 2][1], currFig[currFig.Count - 1][0] - currFig[currFig.Count - 2][0]));

                        if (Math.Sin(theta) != 0)
                        {
                            double h = Math.Abs(fig.LineWidth * 0.5 / Math.Sin(theta));

                            minX = Math.Min(minX, currFig[currFig.Count - 2][0] - h);
                            maxX = Math.Max(maxX, currFig[currFig.Count - 2][0] + h);
                            minY = Math.Min(minY, currFig[currFig.Count - 2][1] - h);
                            maxY = Math.Max(maxY, currFig[currFig.Count - 2][1] + h);

                            currMinX = Math.Min(currMinX, currFig[currFig.Count - 2][0] - h);
                            currMaxX = Math.Max(currMaxX, currFig[currFig.Count - 2][0] + h);
                            currMinY = Math.Min(currMinY, currFig[currFig.Count - 2][1] - h);
                            currMaxY = Math.Max(currMaxY, currFig[currFig.Count - 2][1] + h);
                        }
                    }
                }
                else if (seg.Type == SegmentType.CubicBezier)
                {
                    double[] startPoint = currFig.Last();

                    double[] currPoint = currFig.Last();

                    double t = 0;

                    while (t < 1)
                    {
                        double delta = Math.Min(0.1, 1 - t);

                        double tooSmall = 0;
                        double tooLarge = 1 - t;

                        bool recompute = true;

                        double newT = 0, newX = 0, newY = 0, dist = 0;

                        while (recompute)
                        {
                            recompute = false;
                            newT = t + delta;

                            newX = startPoint[0] * (1 - newT) * (1 - newT) * (1 - newT) + 3 * seg.Points[0].X * newT * (1 - newT) * (1 - newT) + 3 * seg.Points[1].X * newT * newT * (1 - newT) + seg.Points[2].X * newT * newT * newT;
                            newY = startPoint[1] * (1 - newT) * (1 - newT) * (1 - newT) + 3 * seg.Points[0].Y * newT * (1 - newT) * (1 - newT) + 3 * seg.Points[1].Y * newT * newT * (1 - newT) + seg.Points[2].Y * newT * newT * newT;

                            dist = (newX - currPoint[0]) * (newX - currPoint[0]) + (newY - currPoint[1]) * (newY - currPoint[1]);

                            if (dist < 0.5 * scaleFactor && delta < 1 - t)
                            {
                                tooSmall = delta;
                                delta = (tooSmall + tooLarge) * 0.5;
                                recompute = true;
                            }

                            if (dist > 4.5 * scaleFactor)
                            {
                                tooLarge = delta;
                                delta = (tooSmall + tooLarge) * 0.5;
                                recompute = true;
                            }

                        }

                        currPoint = new double[] { newX, newY };
                        currFig.Add(currPoint);
                        joiners.Add(false);

                        minX = Math.Min(minX, newX);
                        maxX = Math.Max(maxX, newX);
                        minY = Math.Min(minY, newY);
                        maxY = Math.Max(maxY, newY);

                        currMinX = Math.Min(currMinX, newX);
                        currMaxX = Math.Max(currMaxX, newX);
                        currMinY = Math.Min(currMinY, newY);
                        currMaxY = Math.Max(currMaxY, newY);

                        t = newT;
                    }

                    if (t != 1)
                    {
                        double[] pt = new double[] { (seg.Points[2].X), (seg.Points[2].Y) };

                        currFig.Add(pt);
                        joiners.Add(false);

                        minX = Math.Min(minX, pt[0]);
                        maxX = Math.Max(maxX, pt[0]);
                        minY = Math.Min(minY, pt[1]);
                        maxY = Math.Max(maxY, pt[1]);

                        currMinX = Math.Min(currMinX, pt[0]);
                        currMaxX = Math.Max(currMaxX, pt[0]);
                        currMinY = Math.Min(currMinY, pt[1]);
                        currMaxY = Math.Max(currMaxY, pt[1]);
                    }

                    joiners[joiners.Count - 1] = true;
                }
            }

            if (currFig.Count > 0)
            {
                perimeters.Add(currFig.ToArray());
                perJoiners.Add(joiners.ToArray());
            }

            minX -= fig.LineWidth;
            minY -= fig.LineWidth;
            maxX += fig.LineWidth;
            maxY += fig.LineWidth;

            if (fig.LineDash.UnitsOn != 0 && fig.LineDash.UnitsOff != 0)
            {
                List<double[][]> newPerimeters = new List<double[][]>();
                List<bool[]> newPerJoiners = new List<bool[]>();
                List<double[]> newPerBounds = new List<double[]>();

                for (int i = 0; i < perimeters.Count; i++)
                {
                    double position = fig.LineDash.Phase % (fig.LineDash.UnitsOn + fig.LineDash.UnitsOff);

                    currFig = new List<double[]>();
                    List<bool> currJoiners = new List<bool>();

                    double[] currPoint = perimeters[i][0];
                    currFig.Add(currPoint);
                    currJoiners.Add(perJoiners[i][0]);

                    currMinX = currPoint[0];
                    currMaxX = currPoint[0];
                    currMinY = currPoint[1];
                    currMaxY = currPoint[1];

                    for (int j = 1; j < perimeters[i].Length; j++)
                    {
                        double length = Math.Sqrt((perimeters[i][j][0] - currPoint[0]) * (perimeters[i][j][0] - currPoint[0]) + (perimeters[i][j][1] - currPoint[1]) * (perimeters[i][j][1] - currPoint[1]));
                        if (position < fig.LineDash.UnitsOn)
                        {
                            if (length < fig.LineDash.UnitsOn - position)
                            {
                                position += length;

                                currPoint = perimeters[i][j];
                                currFig.Add(currPoint);
                                currJoiners.Add(perJoiners[i][j]);

                                currMinX = Math.Min(currMinX, currPoint[0]);
                                currMaxX = Math.Max(currMaxX, currPoint[0]);
                                currMinY = Math.Min(currMinY, currPoint[1]);
                                currMaxY = Math.Max(currMaxY, currPoint[1]);
                            }
                            else
                            {
                                double prop = (fig.LineDash.UnitsOn - position) / length;

                                position += fig.LineDash.UnitsOn - position;

                                currPoint = new double[] { currPoint[0] * (1 - prop) + perimeters[i][j][0] * prop, currPoint[1] * (1 - prop) + perimeters[i][j][1] * prop };

                                currFig.Add(currPoint);
                                currJoiners.Add(perJoiners[i][j]);

                                currMinX = Math.Min(currMinX, currPoint[0]);
                                currMaxX = Math.Max(currMaxX, currPoint[0]);
                                currMinY = Math.Min(currMinY, currPoint[1]);
                                currMaxY = Math.Max(currMaxY, currPoint[1]);

                                newPerimeters.Add(currFig.ToArray());
                                newPerJoiners.Add(currJoiners.ToArray());
                                newPerBounds.Add(new double[] { currMinX - fig.LineWidth, currMaxX + fig.LineWidth, currMinY - fig.LineWidth, currMaxY + fig.LineWidth });

                                currFig = null;
                                currJoiners = null;

                                j--;
                            }
                        }
                        else
                        {
                            if (length < fig.LineDash.UnitsOff - (position - fig.LineDash.UnitsOn))
                            {
                                position += length;
                                currPoint = perimeters[i][j];
                            }
                            else
                            {
                                double prop = (fig.LineDash.UnitsOff - (position - fig.LineDash.UnitsOn)) / length;

                                position += (fig.LineDash.UnitsOff - (position - fig.LineDash.UnitsOn));

                                currPoint = new double[] { currPoint[0] * (1 - prop) + perimeters[i][j][0] * prop, currPoint[1] * (1 - prop) + perimeters[i][j][1] * prop };

                                currFig = new List<double[]>();
                                currJoiners = new List<bool>();

                                currFig.Add(currPoint);
                                currJoiners.Add(perJoiners[i][j]);

                                currMinX = currPoint[0];
                                currMaxX = currPoint[0];
                                currMinY = currPoint[1];
                                currMaxY = currPoint[1];

                                j--;
                            }

                        }

                        position %= (fig.LineDash.UnitsOff + fig.LineDash.UnitsOn);
                    }


                    if (currFig?.Count > 0)
                    {
                        newPerimeters.Add(currFig.ToArray());
                        newPerJoiners.Add(currJoiners.ToArray());
                        newPerBounds.Add(new double[] { currMinX - fig.LineWidth, currMaxX + fig.LineWidth, currMinY - fig.LineWidth, currMaxY + fig.LineWidth });
                    }
                }


                perimeters = newPerimeters;
                perJoiners = newPerJoiners;

                fig.IsClosed = false;

            }

            double lineWidthHalf = fig.LineWidth / 2;

            double[] bound1 = new double[] { minX, minY };
            double[] bound2 = new double[] { minX, maxY };
            double[] bound3 = new double[] { maxX, maxY };
            double[] bound4 = new double[] { maxX, minY };

            bound1 = MatrixUtils.Multiply(fig.TransformMatrix, bound1);
            bound2 = MatrixUtils.Multiply(fig.TransformMatrix, bound2);
            bound3 = MatrixUtils.Multiply(fig.TransformMatrix, bound3);
            bound4 = MatrixUtils.Multiply(fig.TransformMatrix, bound4);

            minX = Math.Min(Math.Min(bound1[0], bound2[0]), Math.Min(bound3[0], bound4[0]));
            minY = Math.Min(Math.Min(bound1[1], bound2[1]), Math.Min(bound3[1], bound4[1]));
            maxX = Math.Max(Math.Max(bound1[0], bound2[0]), Math.Max(bound3[0], bound4[0]));
            maxY = Math.Max(Math.Max(bound1[1], bound2[1]), Math.Max(bound3[1], bound4[1]));

            if (!((minX < 0 && maxX < 0) || (minX >= width && maxX >= width) || (minY < 0 && maxY < 0) || (minY >= height && maxY >= height)))
            {
                int fullWidth = (int)Math.Ceiling(maxX) - (int)Math.Floor(minX) + 2;
                int fullHeight = (int)Math.Ceiling(maxY) - (int)Math.Floor(minY) + 2;

                int fullLeft = (int)Math.Floor(minX) - 1;
                int fullTop = (int)Math.Floor(minY) - 1;

                byte[,] fullMask = new byte[fullWidth, fullHeight];

                for (int i = 0; i < perimeters.Count; i++)
                {
                    for (int k = 1; k < perimeters[i].Length; k++)
                    {
                        if (perimeters[i][k - 1][0] != perimeters[i][k][0] || perimeters[i][k - 1][1] != perimeters[i][k][1])
                        {
                            StrokeThickSegment(perimeters[i][k - 1][0], perimeters[i][k - 1][1], perimeters[i][k][0], perimeters[i][k][1], fullMask, fullWidth, fullHeight, lineWidthHalf, !fig.IsClosed && k == 1, !(fig.IsClosed && perimeters[i].Length > 2) ? (k == perimeters[i].Length - 1) : false, fig.LineCap, perJoiners[i][k] ? fig.LineJoin : LineJoins.Bevel, k < perimeters[i].Length - 1 ? perimeters[i][k + 1][0] : (fig.IsClosed && perimeters[i].Length > 2) ? perimeters[i][1][0] : 0, k < perimeters[i].Length - 1 ? perimeters[i][k + 1][1] : (fig.IsClosed && perimeters[i].Length > 2) ? perimeters[i][1][1] : 0, fig.TransformMatrix, fullLeft, fullTop, scaleFactor);
                        }
                    }
                }

                for (int x = 0; x < fullWidth; x++)
                {
                    for (int y = 0; y < fullHeight; y++)
                    {
                        int realX = x + fullLeft;
                        int realY = y + fullTop;

                        if (realX >= 0 && realX < width && realY >= 0 && realY < height)
                        {
                            byte newA = (byte)(A * fullMask[x, y] / 255);

                            if (newA == 255)
                            {
                                image[realX, realY, 0] = R;
                                image[realX, realY, 1] = G;
                                image[realX, realY, 2] = B;
                                image[realX, realY, 3] = 255;
                            }
                            else
                            {
                                byte outA = (byte)(newA + (image[realX, realY, 3] * (255 - newA)) / 255);

                                if (outA > 0)
                                {

                                    image[realX, realY, 0] = (byte)((R * newA + (image[realX, realY, 0] * image[realX, realY, 3] * (255 - newA)) / 255) / outA);
                                    image[realX, realY, 1] = (byte)((G * newA + (image[realX, realY, 1] * image[realX, realY, 3] * (255 - newA)) / 255) / outA);
                                    image[realX, realY, 2] = (byte)((B * newA + (image[realX, realY, 2] * image[realX, realY, 3] * (255 - newA)) / 255) / outA);
                                    image[realX, realY, 3] = outA;
                                }
                                else
                                {
                                    image[realX, realY, 0] = 0;
                                    image[realX, realY, 1] = 0;
                                    image[realX, realY, 2] = 0;
                                    image[realX, realY, 3] = 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void FillFigure(PathFigure fig, byte[,,] image, int width, int height)
        {
            List<double[][]> perimeters = new List<double[][]>();
            List<double[]> currFig = new List<double[]>();

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            byte R = (byte)(fig.Fill?.R * 255);
            byte G = (byte)(fig.Fill?.G * 255);
            byte B = (byte)(fig.Fill?.B * 255);
            byte A = (byte)(fig.Fill?.A * 255);

            double[] unitVector = MatrixUtils.Multiply(fig.TransformMatrix, new double[] { 1, 1 });
            double[] origin = MatrixUtils.Multiply(fig.TransformMatrix, new double[] { 0, 0 });
            double scaleFactor = 1 / Math.Sqrt(((unitVector[0] - origin[0]) * (unitVector[0] - origin[0]) + (unitVector[1] - origin[1]) * (unitVector[1] - origin[1])) / 2);

            foreach (Segment seg in fig.Segments)
            {
                if (seg.Type == SegmentType.Move)
                {
                    if (currFig.Count > 0)
                    {
                        perimeters.Add(currFig.ToArray());
                    }
                    double[] pt = new double[] { (seg.Point.X), (seg.Point.Y) };

                    currFig = new List<double[]>() { pt };

                    minX = Math.Min(minX, pt[0]);
                    maxX = Math.Max(maxX, pt[0]);
                    minY = Math.Min(minY, pt[1]);
                    maxY = Math.Max(maxY, pt[1]);
                }
                else if (seg.Type == SegmentType.Close)
                {
                    if (currFig.Count > 0)
                    {
                        currFig.Add(new double[] { currFig[0][0], currFig[0][1] });
                        perimeters.Add(currFig.ToArray());
                    }
                    currFig = new List<double[]>();
                }
                if (seg.Type == SegmentType.Line)
                {
                    double[] pt = new double[] { (seg.Point.X), (seg.Point.Y) };

                    currFig.Add(pt);

                    minX = Math.Min(minX, pt[0]);
                    maxX = Math.Max(maxX, pt[0]);
                    minY = Math.Min(minY, pt[1]);
                    maxY = Math.Max(maxY, pt[1]);
                }
                else if (seg.Type == SegmentType.CubicBezier)
                {
                    double[] startPoint = currFig.Last();

                    double[] currPoint = currFig.Last();

                    double t = 0;

                    while (t < 1)
                    {
                        double delta = Math.Min(0.1, 1 - t);

                        double tooSmall = 0;
                        double tooLarge = 1 - t;

                        bool recompute = true;

                        double newT = 0, newX = 0, newY = 0, dist = 0;

                        while (recompute)
                        {
                            recompute = false;
                            newT = t + delta;

                            newX = startPoint[0] * (1 - newT) * (1 - newT) * (1 - newT) + 3 * seg.Points[0].X * newT * (1 - newT) * (1 - newT) + 3 * seg.Points[1].X * newT * newT * (1 - newT) + seg.Points[2].X * newT * newT * newT;
                            newY = startPoint[1] * (1 - newT) * (1 - newT) * (1 - newT) + 3 * seg.Points[0].Y * newT * (1 - newT) * (1 - newT) + 3 * seg.Points[1].Y * newT * newT * (1 - newT) + seg.Points[2].Y * newT * newT * newT;

                            dist = (newX - currPoint[0]) * (newX - currPoint[0]) + (newY - currPoint[1]) * (newY - currPoint[1]);

                            if (dist < 0.5 * scaleFactor && delta < 1 - t)
                            {
                                tooSmall = delta;
                                delta = (tooSmall + tooLarge) * 0.5;
                                recompute = true;
                            }

                            if (dist > 4.5 * scaleFactor)
                            {
                                tooLarge = delta;
                                delta = (tooSmall + tooLarge) * 0.5;
                                recompute = true;
                            }

                        }

                        currPoint = new double[] { newX, newY };
                        currFig.Add(currPoint);

                        minX = Math.Min(minX, newX);
                        maxX = Math.Max(maxX, newX);
                        minY = Math.Min(minY, newY);
                        maxY = Math.Max(maxY, newY);

                        t = newT;
                    }

                    if (t != 1)
                    {
                        double[] pt = new double[] { (seg.Points[2].X), (seg.Points[2].Y) };

                        currFig.Add(pt);

                        minX = Math.Min(minX, pt[0]);
                        maxX = Math.Max(maxX, pt[0]);
                        minY = Math.Min(minY, pt[1]);
                        maxY = Math.Max(maxY, pt[1]);
                    }
                }
            }

            if (currFig.Count > 0)
            {
                perimeters.Add(currFig.ToArray());
            }

            //Apply transforms and remove duplicate points
            for (int i = 0; i < perimeters.Count; i++)
            {
                for (int j = 0; j < perimeters[i].Length; j++)
                {
                    perimeters[i][j] = MatrixUtils.Multiply(fig.TransformMatrix, perimeters[i][j]);
                }

                List<double[]> currPerim = new List<double[]>();

                for (int j = 0; j < perimeters[i].Length; j++)
                {
                    if (currPerim.Count == 0 || (j == perimeters[i].Length - 1 && (Math.Abs(perimeters[i][j][0] - currPerim[currPerim.Count - 1][0]) >= 1e-4 || Math.Abs(perimeters[i][j][1] - currPerim[currPerim.Count - 1][1]) >= 1e-4)) || Math.Abs(perimeters[i][j][0] - currPerim[currPerim.Count - 1][0]) >= 1 || Math.Abs(perimeters[i][j][1] - currPerim[currPerim.Count - 1][1]) >= 1)
                    {
                        if (Math.Abs(Math.Round(perimeters[i][j][0]) - perimeters[i][j][0]) < 1e-2)
                        {
                            perimeters[i][j][0] = Math.Round(perimeters[i][j][0]);
                        }

                        if (Math.Abs(Math.Round(perimeters[i][j][1]) - perimeters[i][j][1]) < 1e-2)
                        {
                            perimeters[i][j][1] = Math.Round(perimeters[i][j][1]);
                        }

                        currPerim.Add(perimeters[i][j]);
                    }
                }

                perimeters[i] = currPerim.ToArray();
            }



            double[] bound1 = new double[] { minX, minY };
            double[] bound2 = new double[] { minX, maxY };
            double[] bound3 = new double[] { maxX, maxY };
            double[] bound4 = new double[] { maxX, minY };

            bound1 = MatrixUtils.Multiply(fig.TransformMatrix, bound1);
            bound2 = MatrixUtils.Multiply(fig.TransformMatrix, bound2);
            bound3 = MatrixUtils.Multiply(fig.TransformMatrix, bound3);
            bound4 = MatrixUtils.Multiply(fig.TransformMatrix, bound4);

            minX = Math.Min(Math.Min(bound1[0], bound2[0]), Math.Min(bound3[0], bound4[0]));
            minY = Math.Min(Math.Min(bound1[1], bound2[1]), Math.Min(bound3[1], bound4[1]));
            maxX = Math.Max(Math.Max(bound1[0], bound2[0]), Math.Max(bound3[0], bound4[0]));
            maxY = Math.Max(Math.Max(bound1[1], bound2[1]), Math.Max(bound3[1], bound4[1]));

            if (!((minX < 0 && maxX < 0) || (minX >= width && maxX >= width) || (minY < 0 && maxY < 0) || (minY >= height && maxY >= height)))
            {
                minX = Math.Max(0, minX);
                maxX = Math.Min(width - 1, maxX);

                int maskLeft = (int)Math.Floor(minX) - 2;
                int maskTop = (int)Math.Floor(minY) - 2;
                int maskWidth = (int)Math.Ceiling(maxX) - maskLeft + 4;
                int maskHeight = (int)Math.Ceiling(maxY) - maskTop + 4;

                byte[,] mask = new byte[maskWidth, maskHeight];

                for (int i = 0; i < perimeters.Count; i++)
                {
                    for (int k = 1; k < perimeters[i].Length; k++)
                    {
                        XiaolinWuLineMask(perimeters[i][k - 1][0] - maskLeft, perimeters[i][k - 1][1] - maskTop, perimeters[i][k][0] - maskLeft, perimeters[i][k][1] - maskTop, mask);
                    }
                }

                FillInsidePerimeters(perimeters, minX, maxX, minY, maxY, width, height, mask, maskLeft, maskTop);

                for (int x = 0; x < maskWidth; x++)
                {
                    for (int y = 0; y < maskHeight; y++)
                    {
                        int realX = x + maskLeft;
                        int realY = y + maskTop;

                        if (realX >= 0 && realX < width && realY >= 0 && realY < height)
                        {
                            byte newA = (byte)(A * mask[x, y] / 255);

                            if (newA == 255)
                            {
                                image[realX, realY, 0] = R;
                                image[realX, realY, 1] = G;
                                image[realX, realY, 2] = B;
                                image[realX, realY, 3] = 255;
                            }
                            else
                            {
                                byte outA = (byte)(newA + (image[realX, realY, 3] * (255 - newA)) / 255);

                                if (outA > 0)
                                {

                                    image[realX, realY, 0] = (byte)((R * newA + (image[realX, realY, 0] * image[realX, realY, 3] * (255 - newA)) / 255) / outA);
                                    image[realX, realY, 1] = (byte)((G * newA + (image[realX, realY, 1] * image[realX, realY, 3] * (255 - newA)) / 255) / outA);
                                    image[realX, realY, 2] = (byte)((B * newA + (image[realX, realY, 2] * image[realX, realY, 3] * (255 - newA)) / 255) / outA);
                                    image[realX, realY, 3] = outA;
                                }
                                else
                                {
                                    image[realX, realY, 0] = 0;
                                    image[realX, realY, 1] = 0;
                                    image[realX, realY, 2] = 0;
                                    image[realX, realY, 3] = 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void FillInsidePerimeters(List<double[][]> perimeters, double minX, double maxX, double minY, double maxY, int width, int height, byte[,] mask, int maskLeft, int maskTop)
        {
            for (int x = (int)Math.Floor(minX); x <= Math.Ceiling(maxX); x++)
            {
                bool skip = false;
                List<double> intersections = new List<double>();

                for (int i = 0; i < perimeters.Count; i++)
                {
                    for (int j = 1; j < perimeters[i].Length; j++)
                    {
                        if ((perimeters[i][j][0] - x <= 1e-4 && perimeters[i][j - 1][0] - x >= -1e-4) || (perimeters[i][j - 1][0] - x <= 1e-4 && perimeters[i][j][0] - x >= -1e-4))
                        {
                            if (perimeters[i][j][0] != perimeters[i][j - 1][0])
                            {
                                double diff = (x - perimeters[i][j - 1][0]) / (perimeters[i][j][0] - perimeters[i][j - 1][0]);

                                if (diff >= -1e-4)
                                {
                                    if (diff < 1 - 1e-4 ||
                                        (j < perimeters[i].Length - 1 && Math.Sign(perimeters[i][j - 1][0] - perimeters[i][j][0]) * Math.Sign(perimeters[i][j + 1][0] - perimeters[i][j][0]) > 0) || //Deal with cusps
                                        (j == perimeters[i].Length - 1 && Math.Sign(perimeters[i][j - 1][0] - perimeters[i][j][0]) * Math.Sign(perimeters[i][1][0] - perimeters[i][j][0]) > 0))
                                    {
                                        intersections.Add(perimeters[i][j - 1][1] + diff * (perimeters[i][j][1] - perimeters[i][j - 1][1]));
                                    }
                                }
                            }
                            else
                            {
                                skip = true;
                                break;
                            }
                        }
                    }
                }


                if (intersections.Count > 1 && !skip)
                {
                    intersections.Sort();

                    double localMinY = intersections.Min();
                    double localMaxY = intersections.Max();

                    if (!((localMinY - maskTop < 0 && localMaxY - maskTop < 0) || (localMinY - maskTop >= height && localMaxY - maskTop >= height)))
                    {
                        for (int i = 0; i < intersections.Count - 1; i += 2)
                        {
                            if (!(intersections[i] - maskTop <= 0 && intersections[i + 1] - maskTop <= 0 || intersections[i] - maskTop >= height && intersections[i + 1] - maskTop >= height))
                            {
                                for (int y = (int)Math.Ceiling(intersections[i]); y <= (int)Math.Floor(intersections[i + 1]); y++)
                                {
                                    mask[x - maskLeft, y - maskTop] = 255;
                                }
                            }
                        }
                    }
                }
            }

            for (int y = (int)Math.Floor(minY); y <= Math.Ceiling(maxY); y++)
            {
                bool skip = false;
                List<double> intersections = new List<double>();

                for (int i = 0; i < perimeters.Count; i++)
                {
                    for (int j = 1; j < perimeters[i].Length; j++)
                    {
                        if ((perimeters[i][j][1] - y <= 1e-4 && perimeters[i][j - 1][1] - y >= -1e-4) || (perimeters[i][j - 1][1] - y <= 1e-4 && perimeters[i][j][1] - y >= -1e-4))
                        {
                            if (perimeters[i][j][1] != perimeters[i][j - 1][1])
                            {
                                double diff = (double)(y - perimeters[i][j - 1][1]) / (perimeters[i][j][1] - perimeters[i][j - 1][1]);

                                if (diff >= -1e-4)
                                {
                                    if (diff < 1 - 1e-4 ||
                                        (j < perimeters[i].Length - 1 && Math.Sign(perimeters[i][j - 1][1] - perimeters[i][j][1]) * Math.Sign(perimeters[i][j + 1][1] - perimeters[i][j][1]) > 0) || //Deal with cusps
                                        (j == perimeters[i].Length - 1 && Math.Sign(perimeters[i][j - 1][1] - perimeters[i][j][1]) * Math.Sign(perimeters[i][1][1] - perimeters[i][j][1]) > 0))
                                    {
                                        intersections.Add(perimeters[i][j - 1][0] + diff * (perimeters[i][j][0] - perimeters[i][j - 1][0]));
                                    }
                                }
                            }
                            else
                            {
                                skip = true;
                                break;
                            }
                        }
                    }
                }

                if (intersections.Count > 1 && !skip)
                {
                    intersections.Sort();

                    double localMinY = intersections.Min();
                    double localMaxY = intersections.Max();

                    if (!((localMinY - maskLeft < 0 && localMaxY - maskLeft < 0) || (localMinY - maskLeft >= width && localMaxY - maskLeft >= width)))
                    {
                        for (int i = 0; i < intersections.Count - 1; i += 2)
                        {
                            if (!(intersections[i] - maskLeft <= 0 && intersections[i + 1] - maskLeft <= 0 || intersections[i] - maskLeft >= width && intersections[i + 1] - maskLeft >= width))
                            {
                                for (int x = (int)Math.Ceiling(intersections[i]); x <= (int)Math.Floor(intersections[i + 1]); x++)
                                {
                                    mask[x - maskLeft, y - maskTop] = 255;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void XiaolinWuLineMask(double x0, double y0, double x1, double y1, byte[,] mask)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

            if (steep)
            {
                double swap = y0;
                y0 = x0;
                x0 = swap;

                swap = y1;
                y1 = x1;
                x1 = swap;
            }

            if (x0 > x1)
            {
                double swap = x1;
                x1 = x0;
                x0 = swap;

                swap = y1;
                y1 = y0;
                y0 = swap;
            }

            double dx = x1 - x0;
            double dy = y1 - y0;
            double gradient = dy / dx;

            if (dx == 0)
            {
                gradient = 1;
            }

            int xend = (int)Math.Round(x0);
            double yend = y0 + gradient * (xend - x0);
            double xgap = RFPart(x0 + 0.5);

            int xpxl1 = xend;
            int ypxl1 = (int)Math.Floor(yend);

            if (steep)
            {
                mask[ypxl1, xpxl1] = (byte)(mask[ypxl1, xpxl1] + RFPart(yend) * xgap * 255 - RFPart(yend) * xgap * mask[ypxl1, xpxl1]);
                mask[ypxl1 + 1, xpxl1] = (byte)(mask[ypxl1 + 1, xpxl1] + FPart(yend) * xgap * 255 - FPart(yend) * xgap * mask[ypxl1 + 1, xpxl1]);
            }
            else
            {
                mask[xpxl1, ypxl1] = (byte)(mask[xpxl1, ypxl1] + RFPart(yend) * xgap * 255 - RFPart(yend) * xgap * mask[xpxl1, ypxl1]);
                mask[xpxl1, ypxl1 + 1] = (byte)(mask[xpxl1, ypxl1 + 1] + FPart(yend) * xgap * 255 - FPart(yend) * xgap * mask[xpxl1, ypxl1 + 1]);
            }

            double intery = yend + gradient;

            xend = (int)Math.Round(x1);
            yend = y1 + gradient * (xend - x1);

            xgap = FPart(x1 + 0.5);

            int xpxl2 = xend;
            int ypxl2 = (int)Math.Floor(yend);

            if (steep)
            {
                mask[ypxl2, xpxl2] = (byte)(mask[ypxl2, xpxl2] + RFPart(yend) * xgap * 255 - RFPart(yend) * xgap * mask[ypxl2, xpxl2]);
                mask[ypxl2 + 1, xpxl2] = (byte)(mask[ypxl2 + 1, xpxl2] + FPart(yend) * xgap * 255 - FPart(yend) * xgap * mask[ypxl2 + 1, xpxl2]);
            }
            else
            {
                mask[xpxl2, ypxl2] = (byte)(mask[xpxl2, ypxl2] + RFPart(yend) * xgap * 255 - RFPart(yend) * xgap * mask[xpxl2, ypxl2]);
                mask[xpxl2, ypxl2 + 1] = (byte)(mask[xpxl2, ypxl2 + 1] + FPart(yend) * xgap * 255 - FPart(yend) * xgap * mask[xpxl2, ypxl2 + 1]);
            }


            if (steep)
            {
                for (int x = xpxl1 + 1; x < xpxl2; x++)
                {
                    int y = ((int)Math.Floor(intery));

                    mask[y, x] = (byte)(mask[y, x] + RFPart(intery) * 255 - RFPart(intery) * mask[y, x]);
                    mask[y + 1, x] = (byte)(mask[y + 1, x] + FPart(intery) * 255 - FPart(intery) * mask[y + 1, x]);

                    intery += gradient;
                }
            }
            else
            {
                for (int x = xpxl1 + 1; x < xpxl2; x++)
                {
                    int y = ((int)Math.Floor(intery));

                    mask[x, y] = (byte)(mask[x, y] + RFPart(intery) * 255 - RFPart(intery) * mask[x, y]);
                    mask[x, y + 1] = (byte)(mask[x, y + 1] + FPart(intery) * 255 - FPart(intery) * mask[x, y + 1]);

                    intery += gradient;
                }
            }
        }

        private static double FPart(double x) { return x - Math.Floor(x); }
        private static double RFPart(double x) { return 1 - FPart(x); }
    }

}
