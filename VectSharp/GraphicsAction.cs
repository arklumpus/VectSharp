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

namespace VectSharp
{
    internal interface IGraphicsAction
    {

    }

    internal interface IPrintableAction
    {
        Brush Fill { get; }
        Brush Stroke { get; }
        double LineWidth { get; }
        LineCaps LineCap { get; }
        LineJoins LineJoin { get; }
        LineDash LineDash { get; }
        string Tag { get; }
    }

    internal class TransformAction : IGraphicsAction
    {
        public Point? Delta { get; } = null;

        public double? Angle { get; } = null;

        public Size? Scale { get; } = null;

        public double[,] Matrix { get; } = null;

        public TransformAction(Point delta)
        {
            this.Delta = delta;
        }

        public TransformAction(double angle)
        {
            this.Angle = angle;
        }

        public TransformAction(Size scale)
        {
            this.Scale = scale;
        }

        public TransformAction(double[,] matrix)
        {
            this.Matrix = matrix;
        }
    }

    internal class StateAction : IGraphicsAction
    {
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
        public Brush Fill { get; }
        public Brush Stroke { get; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public string Tag { get; }
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
    }

    internal class RectangleAction : IGraphicsAction, IPrintableAction
    {
        public Brush Fill { get; }
        public Brush Stroke { get; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public string Tag { get; }
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
    }

    internal class PathAction : IGraphicsAction, IPrintableAction
    {
        public GraphicsPath Path { get; }
        public Brush Fill { get; }
        public Brush Stroke { get; }
        public string Tag { get; }
        public double LineWidth { get; }
        public LineCaps LineCap { get; }
        public LineJoins LineJoin { get; }
        public LineDash LineDash { get; }
        public bool IsClipping { get; }
        public PathAction(GraphicsPath path, Brush fill, Brush stroke, double lineWidth, LineCaps lineCap, LineJoins lineJoin, LineDash lineDash, string tag, bool isClipping)
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
        }
    }

    internal class RasterImageAction : IGraphicsAction, IPrintableAction
    {
        public Brush Fill { get; }
        public Brush Stroke { get; }
        public string Tag { get; }
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
    }
}
