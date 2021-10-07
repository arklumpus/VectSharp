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

using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace VectSharp.Canvas
{
    /// <summary>
    /// Represents a FontFamily created from a resource stream.
    /// </summary>
    public class ResourceFontFamily : FontFamily
    {
        internal string ResourceName;

        /// <summary>
        /// Create a new <see cref="ResourceFontFamily"/> from the specified <paramref name="resourceStream"/> containing a TTF file, passing the specified <paramref name="resourceName"/> to the <see cref="Avalonia.Media.FontFamily.Parse(string, Uri)"/> method.
        /// </summary>
        /// <param name="resourceStream">A resource stream containing a TTF file.</param>
        /// <param name="resourceName">The name of the embedded resource, which will be parsed using <see cref="Avalonia.Media.FontFamily.Parse(string, Uri)"/>.</param>
        public ResourceFontFamily(System.IO.Stream resourceStream, string resourceName) : base(resourceStream)
        {
            this.ResourceName = resourceName;
        }
    }

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
    }

    internal static class Utils
    {
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
    }

    internal class AvaloniaContext : IGraphicsContext
    {
        public Dictionary<string, Delegate> TaggedActions { get; set; } = new Dictionary<string, Delegate>();

        private bool removeTaggedActions = true;

        public string Tag { get; set; }

        private AvaloniaContextInterpreter.TextOptions _textOption;

        private Avalonia.Controls.Canvas currControlElement;

        private Stack<Avalonia.Controls.Canvas> controlElements;

        public AvaloniaContext(double width, double height, bool removeTaggedActionsAfterExecution, AvaloniaContextInterpreter.TextOptions textOption)
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

            _textOption = textOption;

            currControlElement = ControlItem;
            controlElements = new Stack<Avalonia.Controls.Canvas>();
            controlElements.Push(ControlItem);
        }

        public Avalonia.Controls.Canvas ControlItem { get; }

        public double Width { get { return ControlItem.Width; } }
        public double Height { get { return ControlItem.Height; } }

        public void Translate(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            _transform = MatrixUtils.Translate(_transform, x, y);

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }

        public TextBaselines TextBaseline { get; set; }

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

        public void StrokeText(string text, double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            PathText(text, x, y);
            Stroke();
        }

        public void FillText(string text, double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            if (_textOption == AvaloniaContextInterpreter.TextOptions.NeverConvert || (_textOption == AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary && Font.FontFamily.IsStandardFamily))
            {
                TextBlock blk = new TextBlock() { ClipToBounds = false, Text = text, FontFamily = Avalonia.Media.FontFamily.Parse(FontFamily), FontSize = Font.FontSize, FontStyle = (Font.FontFamily.IsOblique ? FontStyle.Oblique : Font.FontFamily.IsItalic ? FontStyle.Italic : FontStyle.Normal), FontWeight = (Font.FontFamily.IsBold ? FontWeight.Bold : FontWeight.Regular) };

                double top = y;
                double left = x;

                double[,] currTransform = null;
                double[,] deltaTransform = MatrixUtils.Identity;

                if (Font.FontFamily.TrueTypeFile != null)
                {
                    currTransform = MatrixUtils.Translate(_transform, x, y);
                }

                Font.DetailedFontMetrics metrics = Font.MeasureTextAdvanced(text);

                if (TextBaseline == TextBaselines.Top)
                {
                    blk.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top - Font.YMax);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top + metrics.Top - Font.YMax);
                        }
                        else
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top - Font.Ascent);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top + metrics.Top - Font.Ascent);
                        }
                    }
                }
                else if (TextBaseline == TextBaselines.Middle)
                {
                    blk.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top / 2 + metrics.Bottom / 2 - Font.YMax);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top + metrics.Top / 2 + metrics.Bottom / 2 - Font.YMax);
                        }
                        else
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top / 2 + metrics.Bottom / 2 - Font.Ascent);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top + metrics.Top / 2 + metrics.Bottom / 2 - Font.Ascent);
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
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - lsb, top - Font.YMax);
                        }
                        else
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - lsb, top - Font.Ascent);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - lsb, top - Font.YMax);
                        }
                    }
                }
                else if (TextBaseline == TextBaselines.Bottom)
                {
                    blk.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;

                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top - Font.YMax + metrics.Bottom);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top - Font.YMax + metrics.Bottom);
                        }
                        else
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top - Font.Ascent + metrics.Bottom);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top - Font.Ascent + metrics.Bottom);
                        }
                    }
                }

                blk.RenderTransform = new MatrixTransform(currTransform.ToAvaloniaMatrix());
                blk.RenderTransformOrigin = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Absolute);

                Avalonia.Media.Brush foreground = null;

                if (this.FillStyle is SolidColourBrush solid)
                {
                    foreground = new SolidColorBrush(Color.FromArgb(FillAlpha, (byte)(solid.R * 255), (byte)(solid.G * 255), (byte)(solid.B * 255)));
                }
                else if (this.FillStyle is LinearGradientBrush linearGradient)
                {
                    foreground = linearGradient.ToLinearGradientBrush(deltaTransform);
                }
                else if (this.FillStyle is RadialGradientBrush radialGradient)
                {
                    foreground = radialGradient.ToRadialGradientBrush(metrics.Width + metrics.LeftSideBearing + metrics.RightSideBearing, deltaTransform);
                }

                blk.Foreground = foreground;

                currControlElement.Children.Add(blk);

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
            else
            {
                PathText(text, x, y);
                Fill();
            }
        }

        public Brush StrokeStyle { get; private set; } = Colour.FromRgb(0, 0, 0);
        private byte StrokeAlpha = 255;

        public Brush FillStyle { get; private set; } = Colour.FromRgb(0, 0, 0);
        private byte FillAlpha = 255;

        public void SetFillStyle((int r, int g, int b, double a) style)
        {
            FillStyle = Colour.FromRgba(style.r, style.g, style.b, (int)(style.a * 255));
            FillAlpha = (byte)(style.a * 255);
        }

        public void SetFillStyle(Brush style)
        {
            FillStyle = style;

            if (style is SolidColourBrush solid)
            {
                FillAlpha = (byte)(solid.A * 255);
            }
            else
            {
                FillAlpha = 255;
            }
        }

        public void SetStrokeStyle((int r, int g, int b, double a) style)
        {
            StrokeStyle = Colour.FromRgba(style.r, style.g, style.b, (int)(style.a * 255));
            StrokeAlpha = (byte)(style.a * 255);
        }

        public void SetStrokeStyle(Brush style)
        {
            StrokeStyle = style;

            if (style is SolidColourBrush solid)
            {
                StrokeAlpha = (byte)(solid.A * 255);
            }
            else
            {
                StrokeAlpha = 255;
            }
        }

        private double[] LineDash;

        public void SetLineDash(LineDash dash)
        {
            LineDash = new double[] { dash.UnitsOn, dash.UnitsOff, dash.Phase };
        }

        public void Rotate(double angle)
        {
            Utils.CoerceNaNAndInfinityToZero(ref angle);

            _transform = MatrixUtils.Rotate(_transform, angle);

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }

        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            Utils.CoerceNaNAndInfinityToZero(ref a, ref b, ref c, ref d, ref e, ref f);

            double[,] transfMatrix = new double[3, 3] { { a, c, e }, { b, d, f }, { 0, 0, 1 } };
            _transform = MatrixUtils.Multiply(_transform, transfMatrix);

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }

        public void Scale(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

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
            controlElements.Push(currControlElement);
        }

        public void Restore()
        {
            _transform = states.Pop();
            currControlElement = controlElements.Pop();
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
                    if (Font.FontFamily is ResourceFontFamily fam)
                    {
                        FontFamily = fam.ResourceName;
                    }
                    else
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
                }
                else
                {
                    FontFamily = "resm:VectSharp.StandardFonts.?assembly=VectSharp#" + Font.FontFamily.TrueTypeFile.GetFontFamilyName();
                }
            }
        }

        public (double Width, double Height) MeasureText(string text)
        {
            Avalonia.Media.FormattedText txt = new Avalonia.Media.FormattedText() { Text = text, Typeface = new Typeface(FontFamily), FontSize = Font.FontSize };
            return (txt.Bounds.Width, txt.Bounds.Height);
        }

        private PathGeometry currentPath;
        private PathFigure currentFigure;

        private bool figureInitialised = false;

        public void MoveTo(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            if (figureInitialised)
            {
                currentPath.Figures.Add(currentFigure);
            }

            currentFigure = new PathFigure() { StartPoint = new Avalonia.Point(x, y), IsClosed = false };
            figureInitialised = true;
        }

        public void LineTo(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

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
            Utils.CoerceNaNAndInfinityToZero(ref x0, ref y0, ref width, ref height);

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
            Utils.CoerceNaNAndInfinityToZero(ref p1X, ref p1Y, ref p2X, ref p2Y, ref p3X, ref p3Y);

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

            Avalonia.Media.Brush stroke = null;

            if (this.StrokeStyle is SolidColourBrush solid)
            {
                stroke = new SolidColorBrush(Color.FromArgb(StrokeAlpha, (byte)(solid.R * 255), (byte)(solid.G * 255), (byte)(solid.B * 255)));
            }
            else if (this.StrokeStyle is LinearGradientBrush linearGradient)
            {
                stroke = linearGradient.ToLinearGradientBrush();
            }
            else if (this.StrokeStyle is RadialGradientBrush radialGradient)
            {
                stroke = radialGradient.ToRadialGradientBrush(currentPath.Bounds.Width);
            }


            Path pth = new Path() { Fill = null, Stroke = stroke, StrokeThickness = LineWidth, StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { (LineDash[0] + (LineCap == LineCaps.Butt ? 0 : LineWidth)) / LineWidth, (LineDash[1] - (LineCap == LineCaps.Butt ? 0 : LineWidth)) / LineWidth }, StrokeDashOffset = LineDash[2] / LineWidth };

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

            currControlElement.Children.Add(pth);

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

            Avalonia.Media.Brush fill = null;

            if (this.FillStyle is SolidColourBrush solid)
            {
                fill = new SolidColorBrush(Color.FromArgb(FillAlpha, (byte)(solid.R * 255), (byte)(solid.G * 255), (byte)(solid.B * 255)));
            }
            else if (this.FillStyle is LinearGradientBrush linearGradient)
            {
                fill = linearGradient.ToLinearGradientBrush();
            }
            else if (this.FillStyle is RadialGradientBrush radialGradient)
            {
                fill = radialGradient.ToRadialGradientBrush(currentPath.Bounds.Width);
            }

            Path pth = new Path() { Fill = fill, Stroke = null };

            pth.Data = currentPath;

            pth.RenderTransform = new MatrixTransform(_transform.ToAvaloniaMatrix());
            pth.RenderTransformOrigin = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Absolute);

            currControlElement.Children.Add(pth);

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

        public void SetClippingPath()
        {
            if (figureInitialised)
            {
                currentPath.Figures.Add(currentFigure);
            }

            Avalonia.Controls.Canvas newControlElement = new Avalonia.Controls.Canvas();

            newControlElement.Clip = currentPath;

            newControlElement.RenderTransformOrigin = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Absolute);
            newControlElement.RenderTransform = new MatrixTransform(_transform.ToAvaloniaMatrix());

            _transform = new double[3, 3];

            _transform[0, 0] = 1;
            _transform[1, 1] = 1;
            _transform[2, 2] = 1;

            currControlElement.Children.Add(newControlElement);
            currControlElement = newControlElement;

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }


        public void DrawRasterImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image)
        {
            Utils.CoerceNaNAndInfinityToZero(ref destinationX, ref destinationY, ref destinationWidth, ref destinationHeight);

            Image img = new Image() { Source = new CroppedBitmap(new Bitmap(image.PNGStream), new Avalonia.PixelRect(sourceX, sourceY, sourceWidth, sourceHeight)), Width = destinationWidth, Height = destinationHeight };

            if (image.Interpolate)
            {
                RenderOptions.SetBitmapInterpolationMode(img, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);
            }
            else
            {
                RenderOptions.SetBitmapInterpolationMode(img, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.Default);
            }

            double[,] transf = MatrixUtils.Translate(_transform, destinationX, destinationY);
            img.RenderTransform = new MatrixTransform(transf.ToAvaloniaMatrix());
            img.RenderTransformOrigin = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Absolute);

            currControlElement.Children.Add(img);

            if (!string.IsNullOrEmpty(Tag))
            {
                if (TaggedActions.ContainsKey(Tag))
                {
                    TaggedActions[Tag].DynamicInvoke(img);

                    if (removeTaggedActions)
                    {
                        TaggedActions.Remove(Tag);
                    }
                }
            }
        }
    }

    internal class RenderCanvas : Avalonia.Controls.Canvas
    {
        private List<RenderAction> RenderActions;
        private List<RenderAction> TaggedRenderActions;

        private SolidColorBrush BackgroundBrush;

        static Avalonia.Point Origin = new Avalonia.Point(0, 0);

        public Dictionary<string, (IImage, bool)> Images;

        public void BringToFront(RenderAction action)
        {
            this.RenderActions.Remove(action);
            this.RenderActions.Add(action);

            if (!string.IsNullOrEmpty(action.Tag))
            {
                int index = this.TaggedRenderActions.IndexOf(action);
                if (index >= 0)
                {
                    this.TaggedRenderActions.RemoveAt(index);
                    this.TaggedRenderActions.Insert(0, action);
                }
            }
        }

        public void SendToBack(RenderAction action)
        {
            this.RenderActions.Remove(action);
            this.RenderActions.Insert(0, action);

            if (!string.IsNullOrEmpty(action.Tag))
            {
                int index = this.TaggedRenderActions.IndexOf(action);
                if (index >= 0)
                {
                    this.TaggedRenderActions.RemoveAt(index);
                    this.TaggedRenderActions.Add(action);
                }
            }
        }

        public RenderCanvas(Graphics content, Colour backgroundColour, double width, double height, Dictionary<string, Delegate> taggedActions, bool removeTaggedActionsAfterExecution, AvaloniaContextInterpreter.TextOptions textOption)
        {
            this.BackgroundBrush = new SolidColorBrush(Color.FromArgb((byte)(backgroundColour.A * 255), (byte)(backgroundColour.R * 255), (byte)(backgroundColour.G * 255), (byte)(backgroundColour.B * 255)));

            this.Width = width;
            this.Height = height;
            this.Images = new Dictionary<string, (IImage, bool)>();
            AvaloniaDrawingContext ctx = new AvaloniaDrawingContext(this.Width, this.Height, removeTaggedActionsAfterExecution, textOption, this.Images);
            foreach (KeyValuePair<string, Delegate> action in taggedActions)
            {
                ctx.TaggedActions.Add(action.Key, (Func<RenderAction, IEnumerable<RenderAction>>)action.Value);
            }

            content.CopyToIGraphicsContext(ctx);
            this.RenderActions = ctx.RenderActions;

            this.TaggedRenderActions = new List<RenderAction>();

            for (int i = this.RenderActions.Count - 1; i >= 0; i--)
            {
                RenderActions[i].InternalParent = this;
                if (!string.IsNullOrEmpty(this.RenderActions[i].Tag))
                {
                    TaggedRenderActions.Add(this.RenderActions[i]);
                }
            }

            this.PointerPressed += PointerPressedAction;
            this.PointerReleased += PointerReleasedAction;
            this.PointerMoved += PointerMoveAction;
            this.PointerLeave += PointerLeaveAction;
        }


        private int CurrentPressedAction = -1;
        private void PointerPressedAction(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Avalonia.Point position = e.GetPosition(this);

            for (int i = 0; i < TaggedRenderActions.Count; i++)
            {
                if (TaggedRenderActions[i].ClippingPath == null || TaggedRenderActions[i].ClippingPath.FillContains(position))
                {
                    Avalonia.Point localPosition = position.Transform(TaggedRenderActions[i].InverseTransform);

                    if (TaggedRenderActions[i].ActionType == RenderAction.ActionTypes.Path)
                    {
                        if ((TaggedRenderActions[i].Fill != null && TaggedRenderActions[i].Geometry.FillContains(localPosition)) || TaggedRenderActions[i].Geometry.StrokeContains(TaggedRenderActions[i].Stroke, localPosition))
                        {
                            TaggedRenderActions[i].FirePointerPressed(e);
                            CurrentPressedAction = i;
                            break;
                        }
                    }
                    else if (TaggedRenderActions[i].ActionType == RenderAction.ActionTypes.Text)
                    {
                        if (TaggedRenderActions[i].Fill != null && TaggedRenderActions[i].Text.HitTestPoint(localPosition).IsInside)
                        {
                            TaggedRenderActions[i].FirePointerPressed(e);
                            CurrentPressedAction = i;
                            break;
                        }
                    }
                    else if (TaggedRenderActions[i].ActionType == RenderAction.ActionTypes.RasterImage)
                    {
                        if (TaggedRenderActions[i].ImageDestination.Value.Contains(localPosition))
                        {
                            TaggedRenderActions[i].FirePointerPressed(e);
                            CurrentPressedAction = i;
                            break;
                        }
                    }
                }
            }
        }

        private void PointerReleasedAction(object sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (CurrentPressedAction >= 0)
            {
                TaggedRenderActions[CurrentPressedAction].FirePointerReleased(e);
                CurrentPressedAction = -1;
            }
            else
            {
                Avalonia.Point position = e.GetPosition(this);

                for (int i = 0; i < TaggedRenderActions.Count; i++)
                {
                    if (TaggedRenderActions[i].ClippingPath == null || TaggedRenderActions[i].ClippingPath.FillContains(position))
                    {
                        Avalonia.Point localPosition = position.Transform(TaggedRenderActions[i].InverseTransform);

                        if (TaggedRenderActions[i].ActionType == RenderAction.ActionTypes.Path)
                        {
                            if ((TaggedRenderActions[i].Fill != null && TaggedRenderActions[i].Geometry.FillContains(localPosition)) || TaggedRenderActions[i].Geometry.StrokeContains(TaggedRenderActions[i].Stroke, localPosition))
                            {
                                TaggedRenderActions[i].FirePointerReleased(e);
                                break;
                            }
                        }
                        else if (TaggedRenderActions[i].ActionType == RenderAction.ActionTypes.Text)
                        {
                            if (TaggedRenderActions[i].Fill != null && TaggedRenderActions[i].Text.HitTestPoint(localPosition).IsInside)
                            {
                                TaggedRenderActions[i].FirePointerReleased(e);
                                break;
                            }
                        }
                        else if (TaggedRenderActions[i].ActionType == RenderAction.ActionTypes.RasterImage)
                        {
                            if (TaggedRenderActions[i].ImageDestination.Value.Contains(localPosition))
                            {
                                TaggedRenderActions[i].FirePointerReleased(e);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private int CurrentOverAction = -1;
        private void PointerMoveAction(object sender, Avalonia.Input.PointerEventArgs e)
        {
            Avalonia.Point position = e.GetPosition(this);

            bool found = false;

            for (int i = 0; i < TaggedRenderActions.Count; i++)
            {
                if (TaggedRenderActions[i].ClippingPath == null || TaggedRenderActions[i].ClippingPath.FillContains(position))
                {
                    Avalonia.Point localPosition = position.Transform(TaggedRenderActions[i].InverseTransform);

                    if (TaggedRenderActions[i].ActionType == RenderAction.ActionTypes.Path)
                    {
                        if ((TaggedRenderActions[i].Fill != null && TaggedRenderActions[i].Geometry.FillContains(localPosition)) || TaggedRenderActions[i].Geometry.StrokeContains(TaggedRenderActions[i].Stroke, localPosition))
                        {
                            found = true;

                            if (CurrentOverAction != i)
                            {
                                if (CurrentOverAction >= 0)
                                {
                                    TaggedRenderActions[CurrentOverAction].FirePointerLeave(e);
                                }
                                CurrentOverAction = i;
                                TaggedRenderActions[CurrentOverAction].FirePointerEnter(e);
                            }

                            break;
                        }
                    }
                    else if (TaggedRenderActions[i].ActionType == RenderAction.ActionTypes.Text)
                    {
                        if (TaggedRenderActions[i].Fill != null && TaggedRenderActions[i].Text.HitTestPoint(localPosition).IsInside)
                        {
                            found = true;

                            if (CurrentOverAction != i)
                            {
                                if (CurrentOverAction >= 0)
                                {
                                    TaggedRenderActions[CurrentOverAction].FirePointerLeave(e);
                                }
                                CurrentOverAction = i;
                                TaggedRenderActions[CurrentOverAction].FirePointerEnter(e);
                            }

                            break;
                        }
                    }
                    else if (TaggedRenderActions[i].ActionType == RenderAction.ActionTypes.RasterImage)
                    {
                        if (TaggedRenderActions[i].ImageDestination.Value.Contains(localPosition))
                        {
                            found = true;

                            if (CurrentOverAction != i)
                            {
                                if (CurrentOverAction >= 0)
                                {
                                    TaggedRenderActions[CurrentOverAction].FirePointerLeave(e);
                                }
                                CurrentOverAction = i;
                                TaggedRenderActions[CurrentOverAction].FirePointerEnter(e);
                            }

                            break;
                        }
                    }
                }
            }

            if (!found)
            {
                if (CurrentOverAction >= 0)
                {
                    TaggedRenderActions[CurrentOverAction].FirePointerLeave(e);
                }
                CurrentOverAction = -1;
            }
        }

        private void PointerLeaveAction(object sender, Avalonia.Input.PointerEventArgs e)
        {
            if (CurrentOverAction >= 0)
            {
                TaggedRenderActions[CurrentOverAction].FirePointerLeave(e);
            }
            CurrentOverAction = -1;
        }


        public override void Render(DrawingContext context)
        {
            context.FillRectangle(this.BackgroundBrush, new Avalonia.Rect(0, 0, Width, Height));

            foreach (RenderAction act in this.RenderActions)
            {
                if (act.ActionType == RenderAction.ActionTypes.Path)
                {
                    DrawingContext.PushedState? state = null;

                    if (act.ClippingPath != null)
                    {
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                        state = context.PushGeometryClip(act.ClippingPath);
                    }

                    using (context.PushPreTransform(act.Transform))
                    {
                        context.DrawGeometry(act.Fill, act.Stroke, act.Geometry);
                    }

                    if (state != null)
                    {
                        state?.Dispose();
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                    }
                }
                else if (act.ActionType == RenderAction.ActionTypes.Text)
                {
                    DrawingContext.PushedState? state = null;

                    if (act.ClippingPath != null)
                    {
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                        state = context.PushGeometryClip(act.ClippingPath);
                    }

                    using (context.PushPreTransform(act.Transform))
                    {
                        context.DrawText(act.Fill, Origin, act.Text);
                    }

                    if (state != null)
                    {
                        state?.Dispose();
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                    }
                }
                else if (act.ActionType == RenderAction.ActionTypes.RasterImage)
                {
                    DrawingContext.PushedState? state = null;

                    if (act.ClippingPath != null)
                    {
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                        state = context.PushGeometryClip(act.ClippingPath);
                    }

                    (IImage, bool) image = Images[act.ImageId];

                    using (context.PushPreTransform(act.Transform))
                    {
                        context.DrawImage(image.Item1, act.ImageSource.Value, act.ImageDestination.Value, image.Item2 ? Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality : Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.Default);
                    }

                    if (state != null)
                    {
                        state?.Dispose();
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                    }
                }
            }

        }
    }


    /// <summary>
    /// Represents a light-weight rendering action.
    /// </summary>
    public class RenderAction
    {
        /// <summary>
        /// Types of rendering actions.
        /// </summary>
        public enum ActionTypes
        {
            /// <summary>
            /// The render action represents a path object.
            /// </summary>
            Path,

            /// <summary>
            /// The render action represents a text object.
            /// </summary>
            Text,

            /// <summary>
            /// The render action represents a raster image.
            /// </summary>
            RasterImage
        }

        /// <summary>
        /// Type of the rendering action.
        /// </summary>
        public ActionTypes ActionType { get; private set; }

        /// <summary>
        /// Geometry that needs to be rendered (null if the action type is <see cref="ActionTypes.Text"/>). If you change this, you need to invalidate the <see cref="Parent"/>'s visual.
        /// </summary>
        public Geometry Geometry { get; set; }

        /// <summary>
        /// Text that needs to be rendered (null if the action type is <see cref="ActionTypes.Path"/>). If you change this, you need to invalidate the <see cref="Parent"/>'s visual.
        /// </summary>
        public Avalonia.Media.FormattedText Text { get; set; }

        /// <summary>
        /// Rendering stroke (null if the action type is <see cref="ActionTypes.Text"/> or if the rendered action only has a <see cref="Fill"/>). If you change this, you need to invalidate the <see cref="Parent"/>'s visual.
        /// </summary>
        public Pen Stroke { get; set; }

        /// <summary>
        /// Rendering fill (null if the rendered action only has a <see cref="Stroke"/>). If you change this, you need to invalidate the <see cref="Parent"/>'s visual.
        /// </summary>
        public IBrush Fill { get; set; }

        /// <summary>
        /// Univocal identifier of the image that needs to be drawn.
        /// </summary>
        public string ImageId { get; set; }

        /// <summary>
        /// The source rectangle of the image.
        /// </summary>
        public Avalonia.Rect? ImageSource { get; set; }

        /// <summary>
        /// The destination rectangle of the image.
        /// </summary>
        public Avalonia.Rect? ImageDestination { get; set; }

        /// <summary>
        /// The current clipping path.
        /// </summary>
        public Geometry ClippingPath { get; set; }

        private Avalonia.Matrix _transform = Avalonia.Matrix.Identity;

        /// <summary>
        /// Inverse transformation matrix.
        /// </summary>
        public Avalonia.Matrix InverseTransform { get; private set; } = Avalonia.Matrix.Identity;

        /// <summary>
        /// Rendering transformation matrix. If you change this, you need to invalidate the <see cref="Parent"/>'s visual.
        /// </summary>
        public Avalonia.Matrix Transform
        {
            get { return _transform; }
            set
            {
                _transform = value;
                InverseTransform = _transform.Invert();
            }
        }

        /// <summary>
        /// A tag to access the <see cref="RenderAction"/>.
        /// </summary>
        public string Tag { get; set; }

        internal RenderCanvas InternalParent { get; set; }

        /// <summary>
        /// The container of this <see cref="RenderAction"/>.
        /// </summary>
        public Avalonia.Controls.Canvas Parent
        {
            get
            {
                return InternalParent;
            }
        }

        /// <summary>
        /// Raised when the pointer enters the area covered by the <see cref="RenderAction"/>.
        /// </summary>
        public event EventHandler<Avalonia.Input.PointerEventArgs> PointerEnter;

        /// <summary>
        /// Raised when the pointer leaves the area covered by the <see cref="RenderAction"/>.
        /// </summary>
        public event EventHandler<Avalonia.Input.PointerEventArgs> PointerLeave;

        /// <summary>
        /// Raised when the pointer is pressed while over the area covered by the <see cref="RenderAction"/>.
        /// </summary>
        public event EventHandler<Avalonia.Input.PointerPressedEventArgs> PointerPressed;

        /// <summary>
        /// Raised when the pointer is released after a <see cref="PointerPressed"/> event.
        /// </summary>
        public event EventHandler<Avalonia.Input.PointerReleasedEventArgs> PointerReleased;


        internal void FirePointerEnter(Avalonia.Input.PointerEventArgs e)
        {
            this.PointerEnter?.Invoke(this, e);
        }

        internal void FirePointerLeave(Avalonia.Input.PointerEventArgs e)
        {
            this.PointerLeave?.Invoke(this, e);
        }

        internal void FirePointerPressed(Avalonia.Input.PointerPressedEventArgs e)
        {
            this.PointerPressed?.Invoke(this, e);
        }

        internal void FirePointerReleased(Avalonia.Input.PointerReleasedEventArgs e)
        {
            this.PointerReleased?.Invoke(this, e);
        }

        private RenderAction()
        {

        }

        /// <summary>
        /// Creates a new <see cref="RenderAction"/> representing a path.
        /// </summary>
        /// <param name="geometry">The geometry to be rendered.</param>
        /// <param name="stroke">The stroke of the path (can be null).</param>
        /// <param name="fill">The fill of the path (can be null).</param>
        /// <param name="transform">The transform that will be applied to the path.</param>
        /// <param name="clippingPath">The clipping path.</param>
        /// <param name="tag">A tag to access the <see cref="RenderAction"/>. If this is null this <see cref="RenderAction"/> is not visible in the hit test.</param>
        /// <returns>A new <see cref="RenderAction"/> representing a path.</returns>
        public static RenderAction PathAction(Geometry geometry, Pen stroke, IBrush fill, Avalonia.Matrix transform, Geometry clippingPath, string tag = null)
        {
            return new RenderAction()
            {
                ActionType = ActionTypes.Path,
                Geometry = geometry,
                Stroke = stroke,
                Fill = fill,
                Transform = transform,
                ClippingPath = clippingPath,
                Tag = tag
            };
        }

        /// <summary>
        /// Creates a new <see cref="RenderAction"/> representing text.
        /// </summary>
        /// <param name="text">The text to be rendered.</param>
        /// <param name="fill">The fill of the text (can be null).</param>
        /// <param name="transform">The transform that will be applied to the text.</param>
        /// <param name="clippingPath">The clipping path.</param>
        /// <param name="tag">A tag to access the <see cref="RenderAction"/>. If this is null this <see cref="RenderAction"/> is not visible in the hit test.</param>
        /// <returns>A new <see cref="RenderAction"/> representing text.</returns>
        public static RenderAction TextAction(Avalonia.Media.FormattedText text, IBrush fill, Avalonia.Matrix transform, Geometry clippingPath, string tag = null)
        {
            return new RenderAction()
            {
                ActionType = ActionTypes.Text,
                Text = text,
                Stroke = null,
                Fill = fill,
                Transform = transform,
                ClippingPath = clippingPath,
                Tag = tag
            };
        }

        /// <summary>
        /// Creates a new <see cref="RenderAction"/> representing an image.
        /// </summary>
        /// <param name="imageId">The univocal identifier of the image to draw.</param>
        /// <param name="sourceRect">The source rectangle of the image.</param>
        /// <param name="destinationRect">The destination rectangle of the image.</param>
        /// <param name="transform">The transform that will be applied to the image.</param>
        /// <param name="clippingPath">The clipping path.</param>
        /// <param name="tag">A tag to access the <see cref="RenderAction"/>. If this is null this <see cref="RenderAction"/> is not visible in the hit test.</param>
        /// <returns>A new <see cref="RenderAction"/> representing an image.</returns>
        public static RenderAction ImageAction(string imageId, Avalonia.Rect sourceRect, Avalonia.Rect destinationRect, Avalonia.Matrix transform, Geometry clippingPath, string tag = null)
        {
            return new RenderAction()
            {
                ActionType = ActionTypes.RasterImage,
                ImageId = imageId,
                ImageSource = sourceRect,
                ImageDestination = destinationRect,
                Transform = transform,
                ClippingPath = clippingPath,
                Tag = tag
            };
        }

        /// <summary>
        /// Brings the render action to the front of the rendering queue. This method can only be invoked after the output has been fully initialised.
        /// </summary>
        public void BringToFront()
        {
            this.InternalParent.BringToFront(this);
        }

        /// <summary>
        /// Brings the render action to the back of the rendering queue. This method can only be invoked after the output has been fully initialised.
        /// </summary>
        public void SendToBack()
        {
            this.InternalParent.SendToBack(this);
        }
    }


    internal class AvaloniaDrawingContext : IGraphicsContext
    {
        public Dictionary<string, Func<RenderAction, IEnumerable<RenderAction>>> TaggedActions { get; set; } = new Dictionary<string, Func<RenderAction, IEnumerable<RenderAction>>>();

        private bool removeTaggedActions = true;

        public string Tag { get; set; }

        AvaloniaContextInterpreter.TextOptions _textOption;

        private Dictionary<string, (IImage, bool)> Images;

        private Geometry _clippingPath;
        private Stack<Geometry> clippingPaths;

        public AvaloniaDrawingContext(double width, double height, bool removeTaggedActionsAfterExecution, AvaloniaContextInterpreter.TextOptions textOption, Dictionary<string, (IImage, bool)> images)
        {
            this.Images = images;

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;

            RenderActions = new List<RenderAction>();
            removeTaggedActions = removeTaggedActionsAfterExecution;

            Width = width;
            Height = height;

            _transform = new double[3, 3];

            _transform[0, 0] = 1;
            _transform[1, 1] = 1;
            _transform[2, 2] = 1;

            states = new Stack<double[,]>();

            _textOption = textOption;

            _clippingPath = null;
            clippingPaths = new Stack<Geometry>();
            clippingPaths.Push(_clippingPath);
        }

        public List<RenderAction> RenderActions { get; set; }

        public double Width { get; private set; }
        public double Height { get; private set; }

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

        public void StrokeText(string text, double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            PathText(text, x, y);
            Stroke();
        }

        public void FillText(string text, double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            if (_textOption == AvaloniaContextInterpreter.TextOptions.NeverConvert || (_textOption == AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary && Font.FontFamily.IsStandardFamily))
            {
                Avalonia.Media.FormattedText txt = new Avalonia.Media.FormattedText()
                {
                    Text = text,
                    Typeface = new Typeface(Avalonia.Media.FontFamily.Parse(FontFamily), (Font.FontFamily.IsOblique ? FontStyle.Oblique : Font.FontFamily.IsItalic ? FontStyle.Italic : FontStyle.Normal), (Font.FontFamily.IsBold ? FontWeight.Bold : FontWeight.Regular)),
                    FontSize = Font.FontSize
                };



                double top = y;
                double left = x;

                double[,] currTransform = null;
                double[,] deltaTransform = MatrixUtils.Identity;

                if (Font.FontFamily.TrueTypeFile != null)
                {
                    currTransform = MatrixUtils.Translate(_transform, x, y);
                }

                Font.DetailedFontMetrics metrics = Font.MeasureTextAdvanced(text);

                if (TextBaseline == TextBaselines.Top)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top - Font.YMax);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top + metrics.Top - Font.YMax);
                        }
                        else
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top - Font.Ascent);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top + metrics.Top - Font.Ascent);
                        }

                    }
                }
                else if (TextBaseline == TextBaselines.Middle)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top / 2 + metrics.Bottom / 2 - Font.YMax);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top + metrics.Top / 2 + metrics.Bottom / 2 - Font.YMax);
                        }
                        else
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top + metrics.Top / 2 + metrics.Bottom / 2 - Font.Ascent);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top + metrics.Top / 2 + metrics.Bottom / 2 - Font.Ascent);
                        }
                    }
                }
                else if (TextBaseline == TextBaselines.Baseline)
                {
                    double lsb = Font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(text[0]).LeftSideBearing * Font.FontSize / 1000;

                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - lsb, top - Font.YMax);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - lsb, top - Font.YMax);
                        }
                        else
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - lsb, top - Font.Ascent);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - lsb, top - Font.YMax);
                        }
                    }
                }
                else if (TextBaseline == TextBaselines.Bottom)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top - Font.YMax + metrics.Bottom);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top - Font.YMax + metrics.Bottom);
                        }
                        else
                        {
                            currTransform = MatrixUtils.Translate(_transform, left - metrics.LeftSideBearing, top - Font.Ascent + metrics.Bottom);
                            deltaTransform = MatrixUtils.Translate(deltaTransform, left - metrics.LeftSideBearing, top - Font.Ascent + metrics.Bottom);
                        }
                    }
                }

                Avalonia.Media.Brush fill = null;

                if (this.FillStyle is SolidColourBrush solid)
                {
                    fill = new SolidColorBrush(Color.FromArgb(FillAlpha, (byte)(solid.R * 255), (byte)(solid.G * 255), (byte)(solid.B * 255)));
                }
                else if (this.FillStyle is LinearGradientBrush linearGradient)
                {
                    fill = linearGradient.ToLinearGradientBrush(deltaTransform);
                }
                else if (this.FillStyle is RadialGradientBrush radialGradient)
                {
                    fill = radialGradient.ToRadialGradientBrush(metrics.Width + metrics.LeftSideBearing + metrics.RightSideBearing, deltaTransform);
                }

                RenderAction act = RenderAction.TextAction(txt, fill, currTransform.ToAvaloniaMatrix(), _clippingPath?.Clone(), Tag);

                if (!string.IsNullOrEmpty(Tag))
                {
                    if (TaggedActions.ContainsKey(Tag))
                    {
                        IEnumerable<RenderAction> actions = TaggedActions[Tag](act);

                        foreach (RenderAction action in actions)
                        {
                            RenderActions.Add(action);
                        }

                        if (removeTaggedActions)
                        {
                            TaggedActions.Remove(Tag);
                        }
                    }
                    else
                    {
                        RenderActions.Add(act);
                    }
                }
                else if (TaggedActions.ContainsKey(""))
                {
                    IEnumerable<RenderAction> actions = TaggedActions[""](act);

                    foreach (RenderAction action in actions)
                    {
                        RenderActions.Add(action);
                    }
                }
                else
                {
                    RenderActions.Add(act);
                }
            }
            else
            {
                PathText(text, x, y);
                Fill();
            }
        }

        public Brush StrokeStyle { get; private set; } = Colour.FromRgb(0, 0, 0);
        private byte StrokeAlpha = 255;

        public Brush FillStyle { get; private set; } = Colour.FromRgb(0, 0, 0);
        private byte FillAlpha = 255;

        public void SetFillStyle((int r, int g, int b, double a) style)
        {
            FillStyle = Colour.FromRgba(style.r, style.g, style.b, (int)(style.a * 255));
            FillAlpha = (byte)(style.a * 255);
        }

        public void SetFillStyle(Brush style)
        {
            FillStyle = style;

            if (style is SolidColourBrush solid)
            {
                FillAlpha = (byte)(solid.A * 255);
            }
            else
            {
                FillAlpha = 255;
            }
        }

        public void SetStrokeStyle((int r, int g, int b, double a) style)
        {
            StrokeStyle = Colour.FromRgba(style.r, style.g, style.b, (int)(style.a * 255));
            StrokeAlpha = (byte)(style.a * 255);
        }

        public void SetStrokeStyle(Brush style)
        {
            StrokeStyle = style;

            if (style is SolidColourBrush solid)
            {
                StrokeAlpha = (byte)(solid.A * 255);
            }
            else
            {
                StrokeAlpha = 255;
            }
        }

        private double[] LineDash;

        public void SetLineDash(LineDash dash)
        {
            LineDash = new double[] { dash.UnitsOn, dash.UnitsOff, dash.Phase };
        }

        public void Rotate(double angle)
        {
            Utils.CoerceNaNAndInfinityToZero(ref angle);

            _transform = MatrixUtils.Rotate(_transform, angle);

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }

        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            Utils.CoerceNaNAndInfinityToZero(ref a, ref b, ref c, ref d, ref e, ref f);

            double[,] transfMatrix = new double[3, 3] { { a, c, e }, { b, d, f }, { 0, 0, 1 } };
            _transform = MatrixUtils.Multiply(_transform, transfMatrix);

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }

        public void Scale(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

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
            clippingPaths.Push(_clippingPath?.Clone());
        }

        public void Restore()
        {
            _transform = states.Pop();
            _clippingPath = clippingPaths.Pop();
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
                    if (Font.FontFamily is ResourceFontFamily fam)
                    {
                        FontFamily = fam.ResourceName;
                    }
                    else
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
                }
                else
                {
                    FontFamily = "resm:VectSharp.StandardFonts.?assembly=VectSharp#" + Font.FontFamily.TrueTypeFile.GetFontFamilyName();
                }
            }
        }

        public (double Width, double Height) MeasureText(string text)
        {
            Avalonia.Media.FormattedText txt = new Avalonia.Media.FormattedText() { Text = text, Typeface = new Typeface(FontFamily), FontSize = Font.FontSize };
            return (txt.Bounds.Width, txt.Bounds.Height);
        }

        private PathGeometry currentPath;
        private PathFigure currentFigure;

        private bool figureInitialised = false;

        public void MoveTo(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            if (figureInitialised)
            {
                currentPath.Figures.Add(currentFigure);
            }

            currentFigure = new PathFigure() { StartPoint = new Avalonia.Point(x, y), IsClosed = false };
            figureInitialised = true;
        }

        public void LineTo(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

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
            Utils.CoerceNaNAndInfinityToZero(ref x0, ref y0, ref width, ref height);

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
            Utils.CoerceNaNAndInfinityToZero(ref p1X, ref p1Y, ref p2X, ref p2Y, ref p3X, ref p3Y);

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

            Avalonia.Media.Brush stroke = null;

            if (this.StrokeStyle is SolidColourBrush solid)
            {
                stroke = new SolidColorBrush(Color.FromArgb(StrokeAlpha, (byte)(solid.R * 255), (byte)(solid.G * 255), (byte)(solid.B * 255)));
            }
            else if (this.StrokeStyle is LinearGradientBrush linearGradient)
            {
                stroke = linearGradient.ToLinearGradientBrush();
            }
            else if (this.StrokeStyle is RadialGradientBrush radialGradient)
            {
                stroke = radialGradient.ToRadialGradientBrush(currentPath.Bounds.Width);
            }

            Pen pen = new Pen(stroke,
                    LineWidth,
                    new DashStyle(new double[] { (LineDash[0] + (LineCap == LineCaps.Butt ? 0 : LineWidth)) / LineWidth, (LineDash[1] - (LineCap == LineCaps.Butt ? 0 : LineWidth)) / LineWidth }, LineDash[2] / LineWidth));

            switch (LineCap)
            {
                case LineCaps.Butt:
                    pen.LineCap = PenLineCap.Flat;
                    break;
                case LineCaps.Round:
                    pen.LineCap = PenLineCap.Round;
                    break;
                case LineCaps.Square:
                    pen.LineCap = PenLineCap.Square;
                    break;
            }

            switch (LineJoin)
            {
                case LineJoins.Bevel:
                    pen.LineJoin = PenLineJoin.Bevel;
                    break;
                case LineJoins.Round:
                    pen.LineJoin = PenLineJoin.Round;
                    break;
                case LineJoins.Miter:
                    pen.LineJoin = PenLineJoin.Miter;
                    break;
            }

            RenderAction act = RenderAction.PathAction(currentPath, pen, null, _transform.ToAvaloniaMatrix(), _clippingPath?.Clone(), Tag);

            if (!string.IsNullOrEmpty(Tag))
            {
                if (TaggedActions.ContainsKey(Tag))
                {
                    IEnumerable<RenderAction> actions = TaggedActions[Tag](act);

                    foreach (RenderAction action in actions)
                    {
                        RenderActions.Add(action);
                    }

                    if (removeTaggedActions)
                    {
                        TaggedActions.Remove(Tag);
                    }
                }
                else
                {
                    RenderActions.Add(act);
                }
            }
            else if (TaggedActions.ContainsKey(""))
            {
                IEnumerable<RenderAction> actions = TaggedActions[""](act);

                foreach (RenderAction action in actions)
                {
                    RenderActions.Add(action);
                }
            }
            else
            {
                RenderActions.Add(act);
            }

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }

        public void Fill()
        {
            if (figureInitialised)
            {
                currentPath.Figures.Add(currentFigure);
            }

            Avalonia.Media.Brush fill = null;

            if (this.FillStyle is SolidColourBrush solid)
            {
                fill = new SolidColorBrush(Color.FromArgb(FillAlpha, (byte)(solid.R * 255), (byte)(solid.G * 255), (byte)(solid.B * 255)));
            }
            else if (this.FillStyle is LinearGradientBrush linearGradient)
            {
                fill = linearGradient.ToLinearGradientBrush();
            }
            else if (this.FillStyle is RadialGradientBrush radialGradient)
            {
                fill = radialGradient.ToRadialGradientBrush(currentPath.Bounds.Width);
            }

            RenderAction act = RenderAction.PathAction(currentPath, null, fill, _transform.ToAvaloniaMatrix(), _clippingPath?.Clone(), Tag);

            if (!string.IsNullOrEmpty(Tag))
            {
                if (TaggedActions.ContainsKey(Tag))
                {
                    IEnumerable<RenderAction> actions = TaggedActions[Tag](act);

                    foreach (RenderAction action in actions)
                    {
                        RenderActions.Add(action);
                    }

                    if (removeTaggedActions)
                    {
                        TaggedActions.Remove(Tag);
                    }
                }
                else
                {
                    RenderActions.Add(act);
                }
            }
            else if (TaggedActions.ContainsKey(""))
            {
                IEnumerable<RenderAction> actions = TaggedActions[""](act);

                foreach (RenderAction action in actions)
                {
                    RenderActions.Add(action);
                }
            }
            else
            {
                RenderActions.Add(act);
            }

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }

        private static void TransformGeometry(PathGeometry geo, double[,] transf)
        {
            for (int i = 0; i < geo.Figures.Count; i++)
            {
                double[] tP = MatrixUtils.Multiply(transf, new double[] { geo.Figures[i].StartPoint.X, geo.Figures[i].StartPoint.Y });
                geo.Figures[i].StartPoint = new Avalonia.Point(tP[0], tP[1]);

                for (int j = 0; j < geo.Figures[i].Segments.Count; j++)
                {
                    if (geo.Figures[i].Segments[j] is LineSegment lS)
                    {
                        tP = MatrixUtils.Multiply(transf, new double[] { lS.Point.X, lS.Point.Y });
                        lS.Point = new Avalonia.Point(tP[0], tP[1]);
                    }
                    else if (geo.Figures[i].Segments[j] is BezierSegment bS)
                    {
                        tP = MatrixUtils.Multiply(transf, new double[] { bS.Point1.X, bS.Point1.Y });
                        double[] tP2 = MatrixUtils.Multiply(transf, new double[] { bS.Point2.X, bS.Point2.Y });
                        double[] tP3 = MatrixUtils.Multiply(transf, new double[] { bS.Point3.X, bS.Point3.Y });

                        bS.Point1 = new Avalonia.Point(tP[0], tP[1]);
                        bS.Point2 = new Avalonia.Point(tP2[0], tP2[1]);
                        bS.Point3 = new Avalonia.Point(tP3[0], tP3[1]);
                    }
                }
            }
        }

        public void SetClippingPath()
        {
            if (figureInitialised)
            {
                currentPath.Figures.Add(currentFigure);
            }

            TransformGeometry(currentPath, _transform);

            if (_clippingPath == null)
            {
                _clippingPath = currentPath;
            }
            else
            {
                //Can't find a better way of transforming an IStreamGeometryImpl into a Geometry...
                _clippingPath = (StreamGeometry)Activator.CreateInstance(typeof(StreamGeometry), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new object[] { _clippingPath.PlatformImpl.Intersect(currentPath.PlatformImpl) }, null);
            }

            currentPath = new PathGeometry();
            currentFigure = new PathFigure() { IsClosed = false };
            figureInitialised = false;
        }

        public void DrawRasterImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image)
        {
            Utils.CoerceNaNAndInfinityToZero(ref destinationX, ref destinationY, ref destinationWidth, ref destinationHeight);

            if (!this.Images.ContainsKey(image.Id))
            {
                Bitmap bmp = new Bitmap(image.PNGStream);
                this.Images.Add(image.Id, (bmp, image.Interpolate));
            }

            RenderAction act = RenderAction.ImageAction(image.Id, new Avalonia.Rect(sourceX, sourceY, sourceWidth, sourceHeight), new Avalonia.Rect(destinationX, destinationY, destinationWidth, destinationHeight), _transform.ToAvaloniaMatrix(), _clippingPath?.Clone(), Tag);

            if (!string.IsNullOrEmpty(Tag))
            {
                if (TaggedActions.ContainsKey(Tag))
                {
                    IEnumerable<RenderAction> actions = TaggedActions[Tag](act);

                    foreach (RenderAction action in actions)
                    {
                        RenderActions.Add(action);
                    }

                    if (removeTaggedActions)
                    {
                        TaggedActions.Remove(Tag);
                    }
                }
                else
                {
                    RenderActions.Add(act);
                }
            }
            else if (TaggedActions.ContainsKey(""))
            {
                IEnumerable<RenderAction> actions = TaggedActions[""](act);

                foreach (RenderAction action in actions)
                {
                    RenderActions.Add(action);
                }
            }
            else
            {
                RenderActions.Add(act);
            }
        }
    }


    /// <summary>
    /// Contains methods to render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/>.
    /// </summary>
    public static class AvaloniaContextInterpreter
    {
        /// <summary>
        /// Defines whether text items should be converted into paths when drawing.
        /// </summary>
        public enum TextOptions
        {
            /// <summary>
            /// Converts all text items into paths.
            /// </summary>
            AlwaysConvert,

            /// <summary>
            /// Converts all text items into paths, with the exception of those that use a standard font.
            /// </summary>
            ConvertIfNecessary,

            /// <summary>
            /// Does not convert any text items into paths.
            /// </summary>
            NeverConvert
        }

        /// <summary>
        /// Render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/>.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static Avalonia.Controls.Canvas PaintToCanvas(this Page page, TextOptions textOption = TextOptions.ConvertIfNecessary)
        {
            AvaloniaContext ctx = new AvaloniaContext(page.Width, page.Height, true, textOption);
            page.Graphics.CopyToIGraphicsContext(ctx);
            ctx.ControlItem.Background = new SolidColorBrush(Color.FromArgb((byte)(page.Background.A * 255), (byte)(page.Background.R * 255), (byte)(page.Background.G * 255), (byte)(page.Background.B * 255)));
            return ctx.ControlItem;
        }

        /// <summary>
        /// Render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/>.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="graphicsAsControls">If this is true, each graphics object (e.g. paths, text...) is rendered as a separate <see cref="Avalonia.Controls.Control"/>. Otherwise, they are directly rendered onto the drawing context (which is faster, but does not allow interactivity).</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static Avalonia.Controls.Canvas PaintToCanvas(this Page page, bool graphicsAsControls, TextOptions textOption = TextOptions.ConvertIfNecessary)
        {
            if (graphicsAsControls)
            {
                Avalonia.Controls.Canvas tbr = page.PaintToCanvas(textOption);
                tbr.Background = new SolidColorBrush(Color.FromArgb((byte)(page.Background.A * 255), (byte)(page.Background.R * 255), (byte)(page.Background.G * 255), (byte)(page.Background.B * 255)));
                return tbr;
            }
            else
            {
                return new RenderCanvas(page.Graphics, page.Background, page.Width, page.Height, new Dictionary<string, Delegate>(), true, textOption) { Background = new SolidColorBrush(Color.FromArgb((byte)(page.Background.A * 255), (byte)(page.Background.R * 255), (byte)(page.Background.G * 255), (byte)(page.Background.B * 255))) };
            }
        }

        /// <summary>
        /// Render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/>.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="graphicsAsControls">If this is true, each graphics object (e.g. paths, text...) is rendered as a separate <see cref="Avalonia.Controls.Control"/>. Otherwise, they are directly rendered onto the drawing context (which is faster, but does not allow interactivity).</param>
        /// <param name="taggedActions">A <see cref="Dictionary{String, Delegate}"/> containing the <see cref="Action"/>s that will be performed on items with the corresponding tag.
        /// If <paramref name="graphicsAsControls"/> is true, the delegates should be voids that accept one parameter of type <see cref="TextBlock"/> or <see cref="Path"/> (depending on the tagged item), otherwise, they should accept one parameter of type <see cref="RenderAction"/> and return an <see cref="IEnumerable{RenderAction}"/> of the actions that will actually be performed.</param>
        /// <param name="removeTaggedActionsAfterExecution">Whether the <see cref="Action"/>s should be removed from <paramref name="taggedActions"/> after their execution. Set to false if the same <see cref="Action"/> should be performed on multiple items with the same tag.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static Avalonia.Controls.Canvas PaintToCanvas(this Page page, bool graphicsAsControls, Dictionary<string, Delegate> taggedActions, bool removeTaggedActionsAfterExecution = true, TextOptions textOption = TextOptions.ConvertIfNecessary)
        {
            if (graphicsAsControls)
            {
                Avalonia.Controls.Canvas tbr = page.PaintToCanvas(taggedActions, removeTaggedActionsAfterExecution, textOption);
                tbr.Background = new SolidColorBrush(Color.FromArgb((byte)(page.Background.A * 255), (byte)(page.Background.R * 255), (byte)(page.Background.G * 255), (byte)(page.Background.B * 255)));
                return tbr;
            }
            else
            {
                return new RenderCanvas(page.Graphics, page.Background, page.Width, page.Height, taggedActions, removeTaggedActionsAfterExecution, textOption) { Background = new SolidColorBrush(Color.FromArgb((byte)(page.Background.A * 255), (byte)(page.Background.R * 255), (byte)(page.Background.G * 255), (byte)(page.Background.B * 255))) }; ;
            }
        }

        /// <summary>
        /// Render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/>.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="taggedActions">A <see cref="Dictionary{String, Delegate}"/> containing the <see cref="Action"/>s that will be performed on items with the corresponding tag.
        /// The delegates should accept one parameter of type <see cref="TextBlock"/> or <see cref="Path"/> (depending on the tagged item).</param>
        /// <param name="removeTaggedActionsAfterExecution">Whether the <see cref="Action"/>s should be removed from <paramref name="taggedActions"/> after their execution. Set to false if the same <see cref="Action"/> should be performed on multiple items with the same tag.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static Avalonia.Controls.Canvas PaintToCanvas(this Page page, Dictionary<string, Delegate> taggedActions, bool removeTaggedActionsAfterExecution = true, TextOptions textOption = TextOptions.ConvertIfNecessary)
        {
            AvaloniaContext ctx = new AvaloniaContext(page.Width, page.Height, removeTaggedActionsAfterExecution, textOption)
            {
                TaggedActions = taggedActions
            };
            page.Graphics.CopyToIGraphicsContext(ctx);
            ctx.ControlItem.Background = new SolidColorBrush(Color.FromArgb((byte)(page.Background.A * 255), (byte)(page.Background.R * 255), (byte)(page.Background.G * 255), (byte)(page.Background.B * 255)));
            return ctx.ControlItem;
        }

        internal static Avalonia.Media.LinearGradientBrush ToLinearGradientBrush(this LinearGradientBrush brush, double[,] transformMatrix = null)
        {
            Point start = brush.StartPoint;
            Point end = brush.EndPoint;

            if (transformMatrix != null)
            {
                double[,] inverse = MatrixUtils.Invert(transformMatrix);

                double[] startVec = MatrixUtils.Multiply(inverse, new double[] { start.X, start.Y });
                double[] endVec = MatrixUtils.Multiply(inverse, new double[] { end.X, end.Y });

                start = new Point(startVec[0], startVec[1]);
                end = new Point(endVec[0], endVec[1]);
            }

            Avalonia.Media.LinearGradientBrush tbr = new Avalonia.Media.LinearGradientBrush()
            {
                SpreadMethod = GradientSpreadMethod.Pad,
                StartPoint = new Avalonia.RelativePoint(start.X, start.Y, Avalonia.RelativeUnit.Absolute),
                EndPoint = new Avalonia.RelativePoint(end.X, end.Y, Avalonia.RelativeUnit.Absolute)
            };

            Avalonia.Media.GradientStops stops = new Avalonia.Media.GradientStops();
            stops.AddRange(from el in brush.GradientStops select new Avalonia.Media.GradientStop(Color.FromArgb((byte)(el.Colour.A * 255), (byte)(el.Colour.R * 255), (byte)(el.Colour.G * 255), (byte)(el.Colour.B * 255)), el.Offset));

            tbr.GradientStops = stops;

            return tbr;
        }

        internal static Avalonia.Media.RadialGradientBrush ToRadialGradientBrush(this RadialGradientBrush brush, double objectWidth, double[,] transformMatrix = null)
        {
            Point focus = brush.FocalPoint;
            Point centre = brush.Centre;

            if (transformMatrix != null)
            {
                double[,] inverse = MatrixUtils.Invert(transformMatrix);

                double[] focusVec = MatrixUtils.Multiply(inverse, new double[] { focus.X, focus.Y });
                double[] centreVec = MatrixUtils.Multiply(inverse, new double[] { centre.X, centre.Y });

                focus = new Point(focusVec[0], focusVec[1]);
                centre = new Point(centreVec[0], centreVec[1]);
            }

            Avalonia.Media.RadialGradientBrush tbr = new Avalonia.Media.RadialGradientBrush()
            {
                SpreadMethod = GradientSpreadMethod.Pad,
                Center = new Avalonia.RelativePoint(centre.X, centre.Y, Avalonia.RelativeUnit.Absolute),
                GradientOrigin = new Avalonia.RelativePoint(focus.X, focus.Y, Avalonia.RelativeUnit.Absolute),
                Radius = brush.Radius / objectWidth
            };

            Avalonia.Media.GradientStops stops = new Avalonia.Media.GradientStops();
            stops.AddRange(from el in brush.GradientStops select new Avalonia.Media.GradientStop(Color.FromArgb((byte)(el.Colour.A * 255), (byte)(el.Colour.R * 255), (byte)(el.Colour.G * 255), (byte)(el.Colour.B * 255)), el.Offset));
            tbr.GradientStops = stops;

            return tbr;
        }
    }
}
