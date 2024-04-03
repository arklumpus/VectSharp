/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2024 Giorgio Bianchini

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

using System.Collections.Generic;
using VectSharp.PDF.OptionalContentGroups;

namespace VectSharp.PDF.Figures
{
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

    internal interface IFigure
    {
        Brush Fill { get; }
        Brush Stroke { get; }
        double LineWidth { get; }
        LineCaps LineCap { get; }
        LineJoins LineJoin { get; }
        LineDash LineDash { get; }
        bool IsClipping { get; }
        string Tag { get; }
        Rectangle GetBounds();
    }

    internal class TransformFigure : IFigure
    {
        public enum TransformTypes
        {
            Transform, Save, Restore
        }

        public TransformTypes TransformType { get; }

        public Brush Fill { get; }
        public Brush Stroke { get; }

        public bool IsClipping { get; }
        public double LineWidth { get; }

        public double[,] TransformationMatrix { get; }

        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }
        public string Tag { get; }

        public TransformFigure(TransformTypes type, double[,] transformationMatrix, string tag)
        {
            this.TransformType = type;
            this.TransformationMatrix = transformationMatrix;
            this.Tag = tag;
        }

        public Rectangle GetBounds()
        {
            return Rectangle.NaN;
        }
    }

    internal class RasterImageFigure : IFigure
    {

        public Brush Fill { get; }
        public Brush Stroke { get; }

        public bool IsClipping { get; }
        public double LineWidth { get; }

        public double[,] TransformationMatrix { get; }

        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }

        public RasterImage Image { get; }

        public string Tag { get; }

        public RasterImageFigure(RasterImage image, string tag)
        {
            this.Image = image;
            this.Tag = tag;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(0, 0, 1, 1);
        }
    }

    internal class PathFigure : IFigure
    {
        public Brush Fill { get; }
        public FillRule FillRule { get; }
        public Brush Stroke { get; }
        public bool IsClipping { get; }
        public double LineWidth { get; }

        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }
        public Segment[] Segments { get; }

        public string Tag { get; }

        private Rectangle Bounds { get; }

        public PathFigure(IEnumerable<Segment> segments, Rectangle bounds, Brush fill, Brush stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, bool isClipping, FillRule fillRule, string tag)
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
            IsClipping = isClipping;
            this.Tag = tag;
            this.FillRule = fillRule;

            if (stroke == null)
            {
                this.Bounds = bounds;
            }
            else
            {
                this.Bounds = new Rectangle(bounds.Location.X - lineWidth * 0.5, bounds.Location.Y - lineWidth * 0.5, bounds.Size.Width + lineWidth, bounds.Size.Height + lineWidth);
            }
        }

        public Rectangle GetBounds()
        {
            return Bounds;
        }
    }

    internal class TextFigure : IFigure
    {
        public Brush Fill { get; }
        public Brush Stroke { get; }
        public double LineWidth { get; }
        public bool IsClipping { get; }
        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }

        public string Text { get; }

        public Font Font { get; }

        public Point Position { get; }

        public TextBaselines TextBaseline { get; }

        public string Tag { get; }

        public TextFigure(string text, Font font, Point position, TextBaselines textBaseline, Brush fill, Brush stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, string tag)
        {
            Text = text;
            Font = font;
            Position = position;
            TextBaseline = textBaseline;

            Fill = fill;
            Stroke = stroke;
            LineWidth = lineWidth;
            LineCap = lineCap;
            LineJoin = lineJoin;
            LineDash = lineDash;
            this.Tag = tag;
        }

        public Rectangle GetBounds()
        {
            Font.DetailedFontMetrics metrics = this.Font.MeasureTextAdvanced(this.Text);

            switch (this.TextBaseline)
            {
                case TextBaselines.Top:
                    return new Rectangle(this.Position, new Size(metrics.Width, metrics.Height));
                case TextBaselines.Bottom:
                    return new Rectangle(this.Position.X, this.Position.Y - metrics.Height, metrics.Width, metrics.Height);
                case TextBaselines.Middle:
                    return new Rectangle(this.Position.X, this.Position.Y - metrics.Height * 0.5, metrics.Width, metrics.Height);
                case TextBaselines.Baseline:
                    return new Rectangle(this.Position.X, this.Position.Y - metrics.Top, metrics.Width, metrics.Height);
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(TextBaseline), this.TextBaseline, "Invalid text baseline!");
            }
        }
    }

    internal class OptionalContentFigure : IFigure
    {
        public enum OptionalContentType
        {
            Start, End
        }

        public OptionalContentType FigureType { get; }

        public Brush Fill { get; }
        public Brush Stroke { get; }

        public bool IsClipping { get; }
        public double LineWidth { get; }

        public OptionalContentGroupExpression VisibilityExpression { get; }

        public LineCaps LineCap { get; }

        public LineJoins LineJoin { get; }

        public LineDash LineDash { get; }
        public string Tag { get; }

        public OptionalContentFigure(OptionalContentType type, OptionalContentGroupExpression visibilityExpression)
        {
            this.FigureType = type;
            this.VisibilityExpression = visibilityExpression;
        }

        public Rectangle GetBounds()
        {
            return Rectangle.NaN;
        }
    }

}
