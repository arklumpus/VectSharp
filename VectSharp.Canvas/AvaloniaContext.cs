using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VectSharp.Canvas
{
    internal static class MatrixUtils
    {
        public static Avalonia.Matrix ToAvaloniaMatrix(this double[,] matrix)
        {
            return new Avalonia.Matrix(matrix[0, 0], matrix[1, 0], matrix[0, 1], matrix[1, 1], matrix[0, 2], matrix[1, 2]);
        }

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

        public static double[,] Identity = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
    }

    internal class AvaloniaContext : IGraphicsContext
    {
        public Dictionary<string, Delegate> TaggedActions { get; set; } = new Dictionary<string, Delegate>();

        private bool removeTaggedActions = true;

        public string Tag { get; set; }

        public AvaloniaContext(double width, double height, bool removeTaggedActionsAfterExecution)
        {
            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
            ControlItem = new Avalonia.Controls.Canvas() { Width = width, Height = height, ClipToBounds = true };
            removeTaggedActions = removeTaggedActionsAfterExecution;

            _transform = new double[3, 3];

            _transform[0, 0] = 1;
            _transform[1, 1] = 1;
            _transform[2, 2] = 1;

            states = new Stack<double[,]>();
        }

        public Avalonia.Controls.Canvas ControlItem { get; }

        public double Width { get { return ControlItem.Width; } }
        public double Height { get { return ControlItem.Height; } }

        public void Translate(double x, double y)
        {
            _transform = MatrixUtils.Translate(_transform, x, y);

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }

        public TextBaselines TextBaseline { get; set; }

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

        public void StrokeText(string text, double x, double y)
        {
            PathText(text, x, y);
            Stroke();
        }

        public void FillText(string text, double x, double y)
        {
            TextBlock blk = new TextBlock() { ClipToBounds = false, Text = text, Foreground = new SolidColorBrush(Color.FromArgb(FillAlpha, (byte)(FillStyle.R * 255), (byte)(FillStyle.G * 255), (byte)(FillStyle.B * 255))), FontFamily = Avalonia.Media.FontFamily.Parse(FontFamily), FontSize = Font.FontSize, FontStyle = (Font.FontFamily.IsOblique ? FontStyle.Oblique : Font.FontFamily.IsItalic ? FontStyle.Italic : FontStyle.Normal), FontWeight = (Font.FontFamily.IsBold ? FontWeight.Bold : FontWeight.Regular) };

            double top = y;
            double left = x;

            double[,] currTransform = null;

            if (Font.FontFamily.TrueTypeFile != null)
            {
                currTransform = MatrixUtils.Translate(_transform, x, y);
            }

            if (TextBaseline == TextBaselines.Top)
            {
                Font.DetailedFontMetrics metrics = Font.MeasureTextAdvanced(text);
                blk.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

                if (Font.FontFamily.TrueTypeFile != null)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {                        
                        currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top - Font.YMax);
                    }
                    else
                    {
                        currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top - Font.Ascent);
                    }
                }
            }
            else if (TextBaseline == TextBaselines.Middle)
            {
                Font.DetailedFontMetrics metrics = Font.MeasureTextAdvanced(text);

                blk.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

                if (Font.FontFamily.TrueTypeFile != null)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top / 2 + metrics.Bottom / 2 - Font.YMax);
                    }
                    else
                    {
                        currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top / 2 + metrics.Bottom / 2 - Font.Ascent);
                    }
                }
            }
            else if (TextBaseline == TextBaselines.Baseline)
            {
                double lsb = Font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(text[0]).LeftSideBearing * Font.FontSize / 1000;

                blk.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

                if (Font.FontFamily.TrueTypeFile != null)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        currTransform = MatrixUtils.Translate(_transform, left - lsb, top - Font.YMax);
                    }
                    else
                    {
                        currTransform = MatrixUtils.Translate(_transform, left - lsb, top - Font.Ascent);
                    }
                }
            }
            else if (TextBaseline == TextBaselines.Bottom)
            {
                Font.DetailedFontMetrics metrics = Font.MeasureTextAdvanced(text);

                blk.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;

                if (Font.FontFamily.TrueTypeFile != null)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top - Font.YMax + metrics.Bottom);
                    }
                    else
                    {
                        currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top - Font.Ascent + metrics.Bottom);
                    }
                }
            }

            blk.RenderTransform = new MatrixTransform(currTransform.ToAvaloniaMatrix());
            blk.RenderTransformOrigin = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Absolute);

            ControlItem.Children.Add(blk);

            if (!string.IsNullOrEmpty(Tag))
            {
                if (TaggedActions.ContainsKey(Tag))
                {
                    TaggedActions[Tag].DynamicInvoke(blk);

                    if (removeTaggedActions)
                    {
                        TaggedActions.Remove(Tag);
                    }
                }
            }
        }

        public Colour StrokeStyle { get; private set; } = Colour.FromRgb(0, 0, 0);
        private byte StrokeAlpha = 255;

        public Colour FillStyle { get; private set; } = Colour.FromRgb(0, 0, 0);
        private byte FillAlpha = 255;

        public void SetFillStyle((int r, int g, int b, double a) style)
        {
            FillStyle = Colour.FromRgba(style.r, style.g, style.b, (int)(style.a * 255));
            FillAlpha = (byte)(style.a * 255);
        }

        public void SetFillStyle(Colour style)
        {
            FillStyle = style;
            FillAlpha = (byte)(style.A * 255);
        }

        public void SetStrokeStyle((int r, int g, int b, double a) style)
        {
            StrokeStyle = Colour.FromRgba(style.r, style.g, style.b, (int)(style.a * 255));
            StrokeAlpha = (byte)(style.a * 255);
        }

        public void SetStrokeStyle(Colour style)
        {
            StrokeStyle = style;
            StrokeAlpha = (byte)(style.A * 255);
        }

        private double[] LineDash;

        public void SetLineDash(LineDash dash)
        {
            LineDash = new double[] { dash.UnitsOn, dash.UnitsOff, dash.Phase };
        }

        public void Rotate(double angle)
        {
            _transform = MatrixUtils.Rotate(_transform, angle);

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }

        public void Scale(double x, double y)
        {
            _transform = MatrixUtils.Scale(_transform, x, y);

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }

        private double[,] _transform;

        private readonly Stack<double[,]> states;

        public void Save()
        {
            states.Push((double[,])_transform.Clone());
        }

        public void Restore()
        {
            _transform = states.Pop();
        }

        public double LineWidth { get; set; }
        public LineCaps LineCap { get; set; }
        public LineJoins LineJoin { get; set; }

        private string FontFamily;
        private Font _Font;

        public Font Font
        {
            get
            {
                return _Font;
            }

            set
            {
                _Font = value;

                if (!Font.FontFamily.IsStandardFamily)
                {

                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        FontFamily = Font.FontFamily.TrueTypeFile.GetFontFamilyName();
                    }
                    else
                    {
                        FontFamily = Font.FontFamily.FileName;
                    }
                }
                else
                {
                    FontFamily = "resm:VectSharp.StandardFonts.?assembly=VectSharp#" + Font.FontFamily.TrueTypeFile.GetFontFamilyName();
                }
            }
        }

        public (double Width, double Height) MeasureText(string text)
        {
            FormattedText txt = new FormattedText() { Text = text, Typeface = new Typeface(FontFamily, Font.FontSize) };
            return (txt.Bounds.Width, txt.Bounds.Height);
        }

        private PathGeometry currentPath;
        private PathFigure currentFigure;

        private bool figureInitialised = false;

        public void MoveTo(double x, double y)
        {
            if (figureInitialised)
            {
                currentPath.Figures.Add(currentFigure);
            }

            currentFigure = new PathFigure() { StartPoint = new Avalonia.Point(x, y), IsClosed = false };
            figureInitialised = true;
        }

        public void LineTo(double x, double y)
        {
            if (figureInitialised)
            {
                currentFigure.Segments.Add(new Avalonia.Media.LineSegment() { Point = new Avalonia.Point(x, y) });
            }
            else
            {
                currentFigure = new PathFigure() { StartPoint = new Avalonia.Point(x, y), IsClosed = false };
                figureInitialised = true;
            }
        }

        public void Rectangle(double x0, double y0, double width, double height)
        {
            if (currentFigure != null && figureInitialised)
            {
                currentPath.Figures.Add(currentFigure);
            }

            currentFigure = new PathFigure() { StartPoint = new Avalonia.Point(x0, y0), IsClosed = false };
            currentFigure.Segments.Add(new Avalonia.Media.LineSegment() { Point = new Avalonia.Point(x0 + width, y0) });
            currentFigure.Segments.Add(new Avalonia.Media.LineSegment() { Point = new Avalonia.Point(x0 + width, y0 + height) });
            currentFigure.Segments.Add(new Avalonia.Media.LineSegment() { Point = new Avalonia.Point(x0, y0 + height) });
            currentFigure.IsClosed = true;

            currentPath.Figures.Add(currentFigure);
            figureInitialised = false;
        }

        public void CubicBezierTo(double p1X, double p1Y, double p2X, double p2Y, double p3X, double p3Y)
        {
            if (figureInitialised)
            {
                currentFigure.Segments.Add(new Avalonia.Media.BezierSegment() { Point1 = new Avalonia.Point(p1X, p1Y), Point2 = new Avalonia.Point(p2X, p2Y), Point3 = new Avalonia.Point(p3X, p3Y) });
            }
            else
            {
                currentFigure = new PathFigure() { StartPoint = new Avalonia.Point(p1X, p1Y), IsClosed = false };
                figureInitialised = true;
            }
        }

        public void Close()
        {
            currentFigure.IsClosed = true;
            currentPath.Figures.Add(currentFigure);
            figureInitialised = false;
        }

        public void Stroke()
        {
            if (figureInitialised)
            {
                currentFigure.IsClosed = false;
                currentPath.Figures.Add(currentFigure);
            }

            Path pth = new Path() { Fill = null, Stroke = new SolidColorBrush(Color.FromArgb(StrokeAlpha, (byte)(StrokeStyle.R * 255), (byte)(StrokeStyle.G * 255), (byte)(StrokeStyle.B * 255))), StrokeThickness = LineWidth, StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { (LineDash[0] + (LineCap == LineCaps.Butt ? 0 : LineWidth)) / LineWidth, (LineDash[1] - (LineCap == LineCaps.Butt ? 0 : LineWidth)) / LineWidth }, StrokeDashOffset = LineDash[2] / LineWidth };

            switch (LineCap)
            {
                case LineCaps.Butt:
                    pth.StrokeLineCap = PenLineCap.Flat;
                    break;
                case LineCaps.Round:
                    pth.StrokeLineCap = PenLineCap.Round;
                    break;
                case LineCaps.Square:
                    pth.StrokeLineCap = PenLineCap.Square;
                    break;
            }

            switch (LineJoin)
            {
                case LineJoins.Bevel:
                    pth.StrokeJoin = PenLineJoin.Bevel;
                    break;
                case LineJoins.Round:
                    pth.StrokeJoin = PenLineJoin.Round;
                    break;
                case LineJoins.Miter:
                    pth.StrokeJoin = PenLineJoin.Miter;
                    break;
            }

            pth.Data = currentPath;

            pth.RenderTransform = new MatrixTransform(_transform.ToAvaloniaMatrix());
            pth.RenderTransformOrigin = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Absolute);

            ControlItem.Children.Add(pth);

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;

            if (!string.IsNullOrEmpty(Tag))
            {
                if (TaggedActions.ContainsKey(Tag))
                {
                    TaggedActions[Tag].DynamicInvoke(pth);

                    if (removeTaggedActions)
                    {
                        TaggedActions.Remove(Tag);
                    }
                }
            }
        }

        public void Fill()
        {
            if (figureInitialised)
            {
                currentPath.Figures.Add(currentFigure);
            }

            Path pth = new Path() { Fill = new SolidColorBrush(Color.FromArgb(FillAlpha, (byte)(FillStyle.R * 255), (byte)(FillStyle.G * 255), (byte)(FillStyle.B * 255))), Stroke = null };

            pth.Data = currentPath;

            pth.RenderTransform = new MatrixTransform(_transform.ToAvaloniaMatrix());
            pth.RenderTransformOrigin = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Absolute);

            ControlItem.Children.Add(pth);

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;

            if (!string.IsNullOrEmpty(Tag))
            {
                if (TaggedActions.ContainsKey(Tag))
                {
                    TaggedActions[Tag].DynamicInvoke(pth);

                    if (removeTaggedActions)
                    {
                        TaggedActions.Remove(Tag);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Contains methods to render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/>.
    /// </summary>
    public static class AvaloniaContextInterpreter
    {
        /// <summary>
        /// Render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/>.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static Avalonia.Controls.Canvas PaintToCanvas(this Page page)
        {
            AvaloniaContext ctx = new AvaloniaContext(page.Width, page.Height, true);
            page.Graphics.CopyToIGraphicsContext(ctx);
            return ctx.ControlItem;
        }

        /// <summary>
        /// Render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/>.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="taggedActions">A <see cref="Dictionary{string, Delegate}"/> containing the <see cref="Action"/>s that will be performed on items with the corresponding tag.</param>
        /// <param name="removeTaggedActionsAfterExecution">Whether the <see cref="Action"/>s should be removed from <paramref name="taggedActions"/> after their execution. Set to false if the same <see cref="Action"/> should be performed on multiple items with the same tag.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static Avalonia.Controls.Canvas PaintToCanvas(this Page page, Dictionary<string, Delegate> taggedActions, bool removeTaggedActionsAfterExecution = true)
        {
            AvaloniaContext ctx = new AvaloniaContext(page.Width, page.Height, removeTaggedActionsAfterExecution)
            {
                TaggedActions = taggedActions
            };
            page.Graphics.CopyToIGraphicsContext(ctx);
            return ctx.ControlItem;
        }
    }
}
