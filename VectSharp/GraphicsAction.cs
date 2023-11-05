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

using VectSharp.Filters;

namespace VectSharp
{
    internal interface IGraphicsAction
    {
        IGraphicsAction ShallowClone();
        string Tag { get; set; }
    }

    internal interface IPrintableAction
    {
        Brush Fill { get; }
        Brush Stroke { get; }
        double LineWidth { get; }
        LineCaps LineCap { get; }
        LineJoins LineJoin { get; }
        LineDash LineDash { get; }
        string Tag { get; set; }
        Rectangle GetBounds();
    }

    internal class TransformAction : IGraphicsAction
    {
        public Point? Delta { get; } = null;

        public double? Angle { get; } = null;

        public Size? Scale { get; } = null;

        public double[,] Matrix { get; } = null;
        public string Tag { get; set; }

        public IGraphicsAction ShallowClone() => (IGraphicsAction)MemberwiseClone();

        public TransformAction(Point delta, string tag)
        {
            this.Delta = delta;
            this.Tag = tag;
        }

        public TransformAction(double angle, string tag)
        {
            this.Angle = angle;
            this.Tag = tag;
        }

        public TransformAction(Size scale, string tag)
        {
            this.Scale = scale;
            this.Tag = tag;
        }

        public TransformAction(double[,] matrix, string tag)
        {
            this.Matrix = matrix;
            this.Tag = tag;
        }

        public double[,] GetMatrix()
        {
            if (this.Matrix != null)
            {
                return this.Matrix;
            }
            else if (this.Delta != null)
            {
                return Graphics.TranslationMatrix(this.Delta.Value.X, this.Delta.Value.Y);
            }
            else if (this.Angle != null)
            {
                return Graphics.RotationMatrix(this.Angle.Value);
            }
            else if (this.Scale != null)
            {
                return Graphics.ScaleMatrix(this.Scale.Value.Width, this.Scale.Value.Height);
            }
            else
            {
                return null;
            }
        }
    }

    internal class StateAction : IGraphicsAction
    {
        public string Tag { get; set; } = null;

        public IGraphicsAction ShallowClone() => (IGraphicsAction)MemberwiseClone();

        public enum StateActionTypes
        {
            Save, Restore
        }

        public StateActionTypes StateActionType { get; }

        public StateAction(StateActionTypes type)
        {
            this.StateActionType = type;
        }
    }

    internal class TextAction : IGraphicsAction, IPrintableAction
    {
        public IGraphicsAction ShallowClone() => (IGraphicsAction)MemberwiseClone();

        public Brush Fill { get; }
        public Brush Stroke { get; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public string Tag { get; set; }
        public string Text { get; }
        public Point Origin { get; }
        public TextBaselines TextBaseline { get; }
        public Font Font { get; }

        public TextAction(Point origin, string text, Font font, TextBaselines textBaseLine, Brush fill, Brush stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, string tag)
        {
            this.Origin = origin;
            this.Text = text;
            this.Font = font;
            this.TextBaseline = textBaseLine;
            this.Fill = fill;
            this.Stroke = stroke;
            this.LineCap = lineCap;
            this.LineJoin = lineJoin;
            this.LineWidth = lineWidth;
            this.Tag = tag;
            this.LineDash = lineDash;
        }

        public Rectangle GetBounds()
        {
            Font.DetailedFontMetrics metrics = this.Font.MeasureTextAdvanced(this.Text);

            switch (this.TextBaseline)
            {
                case TextBaselines.Top:
                    return new Rectangle(this.Origin, new Size(metrics.Width, metrics.Height));
                case TextBaselines.Bottom:
                    return new Rectangle(this.Origin.X, this.Origin.Y - metrics.Height, metrics.Width, metrics.Height);
                case TextBaselines.Middle:
                    return new Rectangle(this.Origin.X, this.Origin.Y - metrics.Height * 0.5, metrics.Width, metrics.Height);
                case TextBaselines.Baseline:
                    return new Rectangle(this.Origin.X, this.Origin.Y - metrics.Top, metrics.Width, metrics.Height);
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(TextBaseline), this.TextBaseline, "Invalid text baseline!");
            }
        }
    }

    internal class RectangleAction : IGraphicsAction, IPrintableAction
    {
        public IGraphicsAction ShallowClone() => (IGraphicsAction)MemberwiseClone();
        public Brush Fill { get; }
        public Brush Stroke { get; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public string Tag { get; set; }
        public Point TopLeft { get; }
        public Size Size { get; }

        public RectangleAction(Point topLeft, Size size, Brush fill, Brush stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, string tag)
        {
            this.TopLeft = topLeft;
            this.Size = size;
            this.Fill = fill;
            this.Stroke = stroke;
            this.LineCap = lineCap;
            this.LineJoin = lineJoin;
            this.LineWidth = lineWidth;
            this.LineDash = lineDash;
            this.Tag = tag;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(this.TopLeft, this.Size);
        }
    }

    internal class PathAction : IGraphicsAction, IPrintableAction
    {
        public IGraphicsAction ShallowClone() => (IGraphicsAction)MemberwiseClone();
        public GraphicsPath Path { get; }
        public Brush Fill { get; }
        public Brush Stroke { get; }
        public string Tag { get; set; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public bool IsClipping { get; }
        public FillRule FillRule { get; }
        public Rectangle GetBounds()
        {
            return this.Path.GetBounds();
        }
        public PathAction(GraphicsPath path, Brush fill, Brush stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, string tag, FillRule fillRule, bool isClipping)
        {
            this.Path = path;
            this.Fill = fill;
            this.Stroke = stroke;
            this.LineCap = lineCap;
            this.LineJoin = lineJoin;
            this.LineWidth = lineWidth;
            this.LineDash = lineDash;
            this.Tag = tag;
            this.IsClipping = isClipping;
            this.FillRule = fillRule;
        }
    }

    internal class RasterImageAction : IGraphicsAction, IPrintableAction
    {
        public IGraphicsAction ShallowClone() => (IGraphicsAction)MemberwiseClone();
        public Brush Fill { get; }
        public Brush Stroke { get; }
        public string Tag { get; set; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public int SourceX { get; }
        public int SourceY { get; }
        public int SourceWidth { get; }
        public int SourceHeight { get; }
        public double DestinationX { get; }
        public double DestinationY { get; }
        public double DestinationWidth { get; }
        public double DestinationHeight { get; }
        public RasterImage Image { get; }

        public RasterImageAction(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image, string tag)
        {
            this.SourceX = sourceX;
            this.SourceY = sourceY;
            this.SourceWidth = sourceWidth;
            this.SourceHeight = sourceHeight;

            this.DestinationX = destinationX;
            this.DestinationY = destinationY;
            this.DestinationWidth = destinationWidth;
            this.DestinationHeight = destinationHeight;

            this.Image = image;
            this.Tag = tag;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(this.DestinationX, this.DestinationY, this.DestinationWidth, this.DestinationHeight);
        }
    }

    internal class FilteredGraphicsAction : IGraphicsAction, IPrintableAction
    {
        public IGraphicsAction ShallowClone() => (IGraphicsAction)MemberwiseClone();
        public Brush Fill { get; }
        public Brush Stroke { get; }
        public string Tag { get; set; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public int SourceX { get; }
        public int SourceY { get; }
        public int SourceWidth { get; }
        public int SourceHeight { get; }
        public double DestinationX { get; }
        public double DestinationY { get; }
        public double DestinationWidth { get; }
        public double DestinationHeight { get; }
        public RasterImage Image { get; }
        public Graphics Content { get; }
        public IFilter Filter { get; }

        public FilteredGraphicsAction(Graphics graphics, IFilter filter)
        {
            this.Content = graphics;
            this.Filter = filter;
        }

        public Rectangle GetBounds()
        {
            Rectangle bounds = this.Content.GetBounds();

            return new Rectangle(bounds.Location.X - this.Filter.TopLeftMargin.X, bounds.Location.Y - this.Filter.TopLeftMargin.Y, bounds.Size.Width + this.Filter.TopLeftMargin.X + this.Filter.BottomRightMargin.X, bounds.Size.Height + this.Filter.TopLeftMargin.Y + this.Filter.BottomRightMargin.Y);
        }
    }
}
