/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2024 Giorgio Bianchini

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
using System.Linq;
using VectSharp.Filters;
using VectSharp.PDF.Figures;
using VectSharp.PDF.OptionalContentGroups;

namespace VectSharp.PDF
{
    internal class PDFContext : IGraphicsContext
    {
        public string Tag { get; set; }
        public double Width { get; }
        public double Height { get; }

        public Dictionary<string, (FontFamily, HashSet<char>)> FontFamilies;

        public Dictionary<string, RasterImage> Images;

        public HashSet<double> Alphas;

        private List<Figures.Segment> _currentFigure;

        internal List<IFigure> _figures;

        private Brush _strokeStyle;
        private Brush _fillStyle;
        private LineDash _lineDash;

        private readonly bool _textToPaths;

        private readonly PDFContextInterpreter.FilterOption _filterOption;

        private OptionalContentGroupExpression _currentVisibilityExpression = null;
        private readonly Dictionary<string, OptionalContentGroupExpression> _ocgVisibilityExpressions;
        public readonly Dictionary<string, OptionalContentGroupExpression> VisibilityExpressions;

        public PDFContext(double width, double height, Colour background, Dictionary<string, (FontFamily, HashSet<char>)> fontFamilies, Dictionary<string, RasterImage> images, HashSet<double> alphas, bool textToPaths, PDFContextInterpreter.FilterOption filterOption, Dictionary<string, OptionalContentGroupExpression> ocgVisibilityExpressions, Dictionary<string, OptionalContentGroupExpression> visibilityExpressions)
        {
            this.Width = width;
            this.Height = height;

            this._ocgVisibilityExpressions = ocgVisibilityExpressions;
            this.VisibilityExpressions = visibilityExpressions;

            _currentFigure = new List<Figures.Segment>();
            _figures = new List<IFigure>();

            _strokeStyle = Colour.FromRgb(0, 0, 0);
            _fillStyle = Colour.FromRgb(0, 0, 0);
            LineWidth = 1;

            LineCap = LineCaps.Butt;
            LineJoin = LineJoins.Miter;
            _lineDash = new LineDash(0, 0, 0);

            _textToPaths = textToPaths;

            Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);

            TextBaseline = TextBaselines.Top;

            this.Translate(0, height);
            this.Scale(1, -1);

            this.FontFamilies = fontFamilies;
            this.Images = images;
            this.Alphas = alphas;

            this.Rectangle(0, 0, width, height);
            this.SetFillStyle(background);
            this.Fill(FillRule.NonZeroWinding);

            this.SetFillStyle(Colour.FromRgb(0, 0, 0));

            this._filterOption = filterOption;
        }

        public void Finish()
        {
            if (_currentVisibilityExpression != null)
            {
                _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
            }
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

        public void SetStrokeStyle(Brush style)
        {
            _strokeStyle = style;
        }

        public void SetFillStyle((int r, int g, int b, double a) style)
        {
            _fillStyle = Colour.FromRgba(style.r, style.g, style.b, style.a);
        }

        public void SetFillStyle(Brush style)
        {
            _fillStyle = style;
        }


        public Brush FillStyle { get { return _fillStyle; } }
        public Brush StrokeStyle { get { return _strokeStyle; } }

        public double LineWidth { get; set; }

        public LineCaps LineCap { get; set; }
        public LineJoins LineJoin { get; set; }

        internal static bool IsCompatible(Brush brush)
        {
            if (brush is SolidColourBrush)
            {
                return true;
            }
            else if (brush is GradientBrush gradient)
            {
                foreach (GradientStop stop in gradient.GradientStops)
                {
                    if (stop.Colour.A != 1)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static Brush GetAlphaBrush(Brush brush)
        {
            if (brush is SolidColourBrush)
            {
                return brush;
            }
            else if (brush is LinearGradientBrush linear)
            {
                return new LinearGradientBrush(linear.StartPoint, linear.EndPoint, from el in linear.GradientStops select new GradientStop(Colour.FromRgb(el.Colour.A, el.Colour.A, el.Colour.A), el.Offset));
            }
            else if (brush is RadialGradientBrush radial)
            {
                return new RadialGradientBrush(radial.FocalPoint, radial.Centre, radial.Radius, from el in radial.GradientStops select new GradientStop(Colour.FromRgb(el.Colour.A, el.Colour.A, el.Colour.A), el.Offset));
            }
            else
            {
                return null;
            }
        }

        private static GraphicsPath GetGraphicsPath(IEnumerable<Figures.Segment> segments)
        {
            GraphicsPath tbr = new GraphicsPath();

            foreach (Figures.Segment seg in segments)
            {
                switch (seg.Type)
                {
                    case Figures.SegmentType.Close:
                        tbr.Close();
                        break;
                    case Figures.SegmentType.Move:
                        tbr.MoveTo(seg.Point);
                        break;
                    case Figures.SegmentType.Line:
                        tbr.LineTo(seg.Point);
                        break;
                    case Figures.SegmentType.CubicBezier:
                        tbr.CubicBezierTo(seg.Points[0], seg.Points[1], seg.Points[2]);
                        break;
                }
            }

            return tbr;
        }

        public void Fill(FillRule fillRule)
        {
            OptionalContentGroupExpression ocge = null;

            if (_ocgVisibilityExpressions != null && this.Tag != null)
            {
                _ocgVisibilityExpressions.TryGetValue(this.Tag, out ocge);
            }

            if (ocge != _currentVisibilityExpression)
            {
                if (_currentVisibilityExpression != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
                }

                if (ocge != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.Start, ocge));
                    this.VisibilityExpressions[ocge.ToString()] = ocge;
                }
                
                _currentVisibilityExpression = ocge;
            }

            if (IsCompatible(_fillStyle))
            {
                _figures.Add(new PathFigure(_currentFigure, VectSharp.Rectangle.NaN, _fillStyle, null, 0, LineCaps.Butt, LineJoins.Bevel, new LineDash(0, 0, 0), false, fillRule, this.Tag));
            }
            else
            {
                _figures.Add(new PathFigure(_currentFigure, GetGraphicsPath(_currentFigure).GetBounds(), _fillStyle, null, 0, LineCaps.Butt, LineJoins.Bevel, new LineDash(0, 0, 0), false, fillRule, this.Tag));
            }

            if (this.FillStyle is SolidColourBrush solidColourBrush)
            {
                this.Alphas.Add(solidColourBrush.A);
            }
            else if (this.FillStyle is GradientBrush gradientBrush)
            {
                foreach (GradientStop stop in gradientBrush.GradientStops)
                {
                    this.Alphas.Add(stop.Colour.A);
                }
            }

            _currentFigure = new List<Figures.Segment>();
        }

        public void Stroke()
        {
            OptionalContentGroupExpression ocge = null;

            if (_ocgVisibilityExpressions != null && this.Tag != null)
            {
                _ocgVisibilityExpressions.TryGetValue(this.Tag, out ocge);
            }

            if (ocge != _currentVisibilityExpression)
            {
                if (_currentVisibilityExpression != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
                }

                if (ocge != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.Start, ocge));
                    this.VisibilityExpressions[ocge.ToString()] = ocge;
                }

                _currentVisibilityExpression = ocge;
            }

            if (IsCompatible(_strokeStyle))
            {
                _figures.Add(new PathFigure(_currentFigure, VectSharp.Rectangle.NaN, null, _strokeStyle, LineWidth, LineCap, LineJoin, _lineDash, false, FillRule.NonZeroWinding, this.Tag));
            }
            else
            {
                _figures.Add(new PathFigure(_currentFigure, GetGraphicsPath(_currentFigure).GetBounds(), null, _strokeStyle, LineWidth, LineCap, LineJoin, _lineDash, false, FillRule.NonZeroWinding, this.Tag));
            }

            if (this.StrokeStyle is SolidColourBrush solidColourBrush)
            {
                this.Alphas.Add(solidColourBrush.A);
            }
            else if (this.StrokeStyle is GradientBrush gradientBrush)
            {
                foreach (GradientStop stop in gradientBrush.GradientStops)
                {
                    this.Alphas.Add(stop.Colour.A);
                }
            }

            _currentFigure = new List<Figures.Segment>();
        }

        public void SetClippingPath()
        {
            _figures.Add(new PathFigure(_currentFigure, VectSharp.Rectangle.NaN, null, null, 0, LineCaps.Butt, LineJoins.Bevel, new LineDash(0, 0, 0), true, FillRule.NonZeroWinding, this.Tag));
            _currentFigure = new List<Figures.Segment>();
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
            if (!_textToPaths)
            {
                OptionalContentGroupExpression ocge = null;

                if (_ocgVisibilityExpressions != null && this.Tag != null)
                {
                    _ocgVisibilityExpressions.TryGetValue(this.Tag, out ocge);
                }

                if (ocge != _currentVisibilityExpression)
                {
                    if (_currentVisibilityExpression != null)
                    {
                        _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
                    }

                    if (ocge != null)
                    {
                        _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.Start, ocge));
                        this.VisibilityExpressions[ocge.ToString()] = ocge;
                    }

                    _currentVisibilityExpression = ocge;
                }

                if (!FontFamilies.TryGetValue(this.Font.FontFamily.FileName, out (FontFamily, HashSet<char>) usedChars))
                {
                    usedChars = (this.Font.FontFamily, new HashSet<char>());
                    FontFamilies[this.Font.FontFamily.FileName] = usedChars;
                }

                foreach (char c in text)
                {
                    usedChars.Item2.Add(c);
                }

                _figures.Add(new TextFigure(text, Font, new Point(x, y), TextBaseline, _fillStyle, null, 0, LineCaps.Butt, LineJoins.Miter, new LineDash(0, 0, 0), this.Tag));
            }
            else
            {
                PathText(text, x, y);
                Fill(FillRule.NonZeroWinding);
            }

            if (this.FillStyle is SolidColourBrush solidColourBrush)
            {
                this.Alphas.Add(solidColourBrush.A);
            }
            else if (this.FillStyle is GradientBrush gradientBrush)
            {
                foreach (GradientStop stop in gradientBrush.GradientStops)
                {
                    this.Alphas.Add(stop.Colour.A);
                }
            }
        }

        public TextBaselines TextBaseline { get; set; }

        public void Restore()
        {
            OptionalContentGroupExpression ocge = null;

            if (_ocgVisibilityExpressions != null && this.Tag != null)
            {
                _ocgVisibilityExpressions.TryGetValue(this.Tag, out ocge);
            }

            if (ocge != _currentVisibilityExpression)
            {
                if (_currentVisibilityExpression != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
                }

                if (ocge != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.Start, ocge));
                    this.VisibilityExpressions[ocge.ToString()] = ocge;
                }

                _currentVisibilityExpression = ocge;
            }

            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Restore, null, this.Tag));
        }

        public void Rotate(double angle)
        {
            OptionalContentGroupExpression ocge = null;

            if (_ocgVisibilityExpressions != null && this.Tag != null)
            {
                _ocgVisibilityExpressions.TryGetValue(this.Tag, out ocge);
            }

            if (ocge != _currentVisibilityExpression)
            {
                if (_currentVisibilityExpression != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
                }

                if (ocge != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.Start, ocge));
                    this.VisibilityExpressions[ocge.ToString()] = ocge;
                }

                _currentVisibilityExpression = ocge;
            }

            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { Math.Cos(angle), Math.Sin(angle), 0 }, { -Math.Sin(angle), Math.Cos(angle), 0 }, { 0, 0, 1 } }, this.Tag));
        }

        public void Save()
        {
            OptionalContentGroupExpression ocge = null;

            if (_ocgVisibilityExpressions != null && this.Tag != null)
            {
                _ocgVisibilityExpressions.TryGetValue(this.Tag, out ocge);
            }

            if (ocge != _currentVisibilityExpression)
            {
                if (_currentVisibilityExpression != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
                }

                if (ocge != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.Start, ocge));
                    this.VisibilityExpressions[ocge.ToString()] = ocge;
                }

                _currentVisibilityExpression = ocge;
            }

            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Save, null, this.Tag));
        }


        public void StrokeText(string text, double x, double y)
        {
            if (!_textToPaths)
            {
                OptionalContentGroupExpression ocge = null;

                if (_ocgVisibilityExpressions != null && this.Tag != null)
                {
                    _ocgVisibilityExpressions.TryGetValue(this.Tag, out ocge);
                }

                if (ocge != _currentVisibilityExpression)
                {
                    if (_currentVisibilityExpression != null)
                    {
                        _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
                    }

                    if (ocge != null)
                    {
                        _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.Start, ocge));
                        this.VisibilityExpressions[ocge.ToString()] = ocge;
                    }

                    _currentVisibilityExpression = ocge;
                }

                if (!FontFamilies.TryGetValue(this.Font.FontFamily.FileName, out (FontFamily, HashSet<char>) usedChars))
                {
                    usedChars = (this.Font.FontFamily, new HashSet<char>());
                    FontFamilies[this.Font.FontFamily.FileName] = usedChars;
                }

                foreach (char c in text)
                {
                    usedChars.Item2.Add(c);
                }

                _figures.Add(new TextFigure(text, Font, new Point(x, y), TextBaseline, null, _strokeStyle, LineWidth, LineCap, LineJoin, _lineDash, this.Tag));
            }
            else
            {
                PathText(text, x, y);
                Stroke();
            }

            if (this.StrokeStyle is SolidColourBrush solidColourBrush)
            {
                this.Alphas.Add(solidColourBrush.A);
            }
            else if (this.StrokeStyle is GradientBrush gradientBrush)
            {
                foreach (GradientStop stop in gradientBrush.GradientStops)
                {
                    this.Alphas.Add(stop.Colour.A);
                }
            }
        }

        public void Translate(double x, double y)
        {
            OptionalContentGroupExpression ocge = null;

            if (_ocgVisibilityExpressions != null && this.Tag != null)
            {
                _ocgVisibilityExpressions.TryGetValue(this.Tag, out ocge);
            }

            if (ocge != _currentVisibilityExpression)
            {
                if (_currentVisibilityExpression != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
                }

                if (ocge != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.Start, ocge));
                    this.VisibilityExpressions[ocge.ToString()] = ocge;
                }

                _currentVisibilityExpression = ocge;
            }
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { 1, 0, x }, { 0, 1, y }, { 0, 0, 1 } }, this.Tag));
        }

        public void Scale(double scaleX, double scaleY)
        {
            OptionalContentGroupExpression ocge = null;

            if (_ocgVisibilityExpressions != null && this.Tag != null)
            {
                _ocgVisibilityExpressions.TryGetValue(this.Tag, out ocge);
            }

            if (ocge != _currentVisibilityExpression)
            {
                if (_currentVisibilityExpression != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
                }

                if (ocge != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.Start, ocge));
                    this.VisibilityExpressions[ocge.ToString()] = ocge;
                }

                _currentVisibilityExpression = ocge;
            }
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { scaleX, 0, 0 }, { 0, scaleY, 0 }, { 0, 0, 1 } }, this.Tag));
        }

        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            OptionalContentGroupExpression ocge = null;

            if (_ocgVisibilityExpressions != null && this.Tag != null)
            {
                _ocgVisibilityExpressions.TryGetValue(this.Tag, out ocge);
            }

            if (ocge != _currentVisibilityExpression)
            {
                if (_currentVisibilityExpression != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
                }

                if (ocge != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.Start, ocge));
                    this.VisibilityExpressions[ocge.ToString()] = ocge;
                }

                _currentVisibilityExpression = ocge;
            }
            _figures.Add(new TransformFigure(TransformFigure.TransformTypes.Transform, new double[,] { { a, b, e }, { c, d, f }, { 0, 0, 1 } }, this.Tag));
        }

        public void DrawRasterImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image)
        {
            OptionalContentGroupExpression ocge = null;

            if (_ocgVisibilityExpressions != null && this.Tag != null)
            {
                _ocgVisibilityExpressions.TryGetValue(this.Tag, out ocge);
            }

            if (ocge != _currentVisibilityExpression)
            {
                if (_currentVisibilityExpression != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.End, _currentVisibilityExpression));
                }

                if (ocge != null)
                {
                    _figures.Add(new OptionalContentFigure(OptionalContentFigure.OptionalContentType.Start, ocge));
                    this.VisibilityExpressions[ocge.ToString()] = ocge;
                }

                _currentVisibilityExpression = ocge;
            }

            Save();

            MoveTo(destinationX, destinationY);
            LineTo(destinationX + destinationWidth, destinationY);
            LineTo(destinationX + destinationWidth, destinationY + destinationHeight);
            LineTo(destinationX, destinationY + destinationHeight);
            Close();
            SetClippingPath();

            double sourceRectX = (double)sourceX / image.Width;
            double sourceRectY = 1 - (double)sourceY / image.Height;
            double sourceRectWidth = (double)sourceWidth / image.Width;
            double sourceRectHeight = -(double)sourceHeight / image.Height;

            double scaleX = destinationWidth / sourceRectWidth;
            double scaleY = destinationHeight / sourceRectHeight;

            double translationX = destinationX / scaleX - sourceRectX;
            double translationY = destinationY / scaleY - sourceRectY;


            Scale(scaleX, scaleY);
            Translate(translationX, translationY);

            _figures.Add(new RasterImageFigure(image, this.Tag));

            this.Images[image.Id] = image;

            Restore();
        }

        public void DrawFilteredGraphics(Graphics graphics, IFilter filter)
        {
            if (this._filterOption.Operation == PDFContextInterpreter.FilterOption.FilterOperations.RasteriseAll)
            {
                double scale = this._filterOption.RasterisationResolution;

                Rectangle bounds = graphics.GetBounds();

                bounds = new Rectangle(bounds.Location.X - filter.TopLeftMargin.X, bounds.Location.Y - filter.TopLeftMargin.Y, bounds.Size.Width + filter.TopLeftMargin.X + filter.BottomRightMargin.X, bounds.Size.Height + filter.TopLeftMargin.Y + filter.BottomRightMargin.Y);

                if (bounds.Size.Width > 0 && bounds.Size.Height > 0)
                {
                    if (!this._filterOption.RasterisationResolutionRelative)
                    {
                        scale /= Math.Min(bounds.Size.Width, bounds.Size.Height);
                    }

                    if (graphics.TryRasterise(bounds, scale, true, out RasterImage rasterised))
                    {
                        RasterImage filtered = null;

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
                    else
                    {
                        throw new NotImplementedException(@"The filter could not be rasterised! You can avoid this error by doing one of the following:
 • Add a reference to VectSharp.Raster or VectSharp.Raster.ImageSharp (you may also need to add a using directive somewhere to force the assembly to be loaded).
 • Provide your own implementation of Graphics.RasterisationMethod.
 • Set the FilterOption.Operation to ""IgnoreAll"" or ""SkipAll"".");
                    }
                }
            }
            else if (this._filterOption.Operation == PDFContextInterpreter.FilterOption.FilterOperations.IgnoreAll)
            {
                graphics.CopyToIGraphicsContext(this);
            }
            else
            {

            }
        }
    }
}
