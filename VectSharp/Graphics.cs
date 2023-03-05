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
using VectSharp.Filters;

namespace VectSharp
{
    internal static class Utils
    {
        public static Point RotatePoint(Point point, double angle)
        {
            return new Point(point.X * Math.Cos(angle) - point.Y * Math.Sin(angle), point.X * Math.Sin(angle) + point.Y * Math.Cos(angle));
        }
    }

    /// <summary>
    /// This interface should be implemented by classes intended to provide graphics output capability to a <see cref="Graphics"/> object.
    /// </summary>
    public interface IGraphicsContext
    {
        /// <summary>
        /// Width of the graphic surface.
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Height of the graphic surface.
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Save the current transform state (rotation, translation, scale). This should be implemented as a LIFO stack.
        /// </summary>
        void Save();

        /// <summary>
        /// Restore the previous transform state (rotation, translation, scale). This should be implemented as a LIFO stack.
        /// </summary>
        void Restore();

        /// <summary>
        /// Translate the coordinate system origin.
        /// </summary>
        /// <param name="x">The horizontal translation.</param>
        /// <param name="y">The vertical translation.</param>
        void Translate(double x, double y);

        /// <summary>
        /// Rotate the coordinate system around the origin.
        /// </summary>
        /// <param name="angle">The angle (in radians) by which to rotate the coordinate system.</param>
        void Rotate(double angle);

        /// <summary>
        /// Scale the coordinate system with respect to the origin.
        /// </summary>
        /// <param name="scaleX">The horizontal scale.</param>
        /// <param name="scaleY">The vertical scale.</param>
        void Scale(double scaleX, double scaleY);

        /// <summary>
        /// Transform the coordinate system with the specified transformation matrix [ [a, c, e], [b, d, f], [0, 0, 1] ].
        /// </summary>
        /// <param name="a">The first element of the first column.</param>
        /// <param name="b">The second element of the first column.</param>
        /// <param name="c">The first element of the second column.</param>
        /// <param name="d">The second element of the second column.</param>
        /// <param name="e">The first element of the third column.</param>
        /// <param name="f">The second element of the third column.</param>
        void Transform(double a, double b, double c, double d, double e, double f);

        /// <summary>
        /// The current font.
        /// </summary>
        Font Font { get; set; }

        /// <summary>
        /// The current text baseline.
        /// </summary>
        TextBaselines TextBaseline { get; set; }

        /// <summary>
        /// Fill a text string using the current <see cref="Font"/> and <see cref="TextBaseline"/>.
        /// </summary>
        /// <param name="text">The string to draw.</param>
        /// <param name="x">The horizontal coordinate of the text origin.</param>
        /// <param name="y">The vertical coordinate of the text origin.</param>
        void FillText(string text, double x, double y);

        /// <summary>
        /// Stroke the outline of a text string using the current <see cref="Font"/> and <see cref="TextBaseline"/>.
        /// </summary>
        /// <param name="text">The string to draw.</param>
        /// <param name="x">The horizontal coordinate of the text origin.</param>
        /// <param name="y">The vertical coordinate of the text origin.</param>
        void StrokeText(string text, double x, double y);

        /// <summary>
        /// Change the current point without drawing a line from the previous point. If necessary, start a new figure.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the point.</param>
        /// <param name="y">The vertical coordinate of the point.</param>
        void MoveTo(double x, double y);

        /// <summary>
        /// Draw a line from the previous point to the specified point.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the point.</param>
        /// <param name="y">The vertical coordinate of the point.</param>
        void LineTo(double x, double y);

        /// <summary>
        /// Close the current figure.
        /// </summary>
        void Close();

        /// <summary>
        /// Stroke the current path using the current <see cref="StrokeStyle"/>, <see cref="LineWidth"/>, <see cref="LineCap"/>, <see cref="LineJoin"/> and <see cref="LineDash"/>.
        /// </summary>
        void Stroke();

        /// <summary>
        /// Set the current clipping path as the intersection of the previous clipping path and the current path.
        /// </summary>
        void SetClippingPath();

        /// <summary>
        /// Current brush used to fill paths.
        /// </summary>
        Brush FillStyle { get; }

        /// <summary>
        /// Set the current <see cref="FillStyle"/>.
        /// </summary>
        /// <param name="style">A <see cref="ValueTuple{Int32, Int32, Int32, Double}"/> containing component information for the colour. For r, g, and b, range: [0, 255]; for a, range: [0, 1].</param>
        void SetFillStyle((int r, int g, int b, double a) style);

        /// <summary>
        /// Set the current <see cref="FillStyle"/>.
        /// </summary>
        /// <param name="style">The new fill style.</param>
        void SetFillStyle(Brush style);

        /// <summary>
        /// Current brush used to stroke paths.
        /// </summary>
        Brush StrokeStyle { get; }

        /// <summary>
        /// Set the current <see cref="StrokeStyle"/>.
        /// </summary>
        /// <param name="style">A <see cref="ValueTuple{Int32, Int32, Int32, Double}"/> containing component information for the colour. For r, g, and b, range: [0, 255]; for a, range: [0, 1].</param>
        void SetStrokeStyle((int r, int g, int b, double a) style);

        /// <summary>
        /// Set the current <see cref="StrokeStyle"/>.
        /// </summary>
        /// <param name="style">The new stroke style.</param>
        void SetStrokeStyle(Brush style);

        /// <summary>
        /// Add to the current figure a cubic Bezier from the current point to a destination point, with two control points.
        /// </summary>
        /// <param name="p1X">The horizontal coordinate of the first control point.</param>
        /// <param name="p1Y">The vertical coordinate of the first control point.</param>
        /// <param name="p2X">The horizontal coordinate of the second control point.</param>
        /// <param name="p2Y">The vertical coordinate of the second control point.</param>
        /// <param name="p3X">The horizontal coordinate of the destination point.</param>
        /// <param name="p3Y">The vertical coordinate of the destination point.</param>
        void CubicBezierTo(double p1X, double p1Y, double p2X, double p2Y, double p3X, double p3Y);

        /// <summary>
        /// Add a rectangle figure to the current path.
        /// </summary>
        /// <param name="x0">The horizontal coordinate of the top-left corner of the rectangle.</param>
        /// <param name="y0">The vertical coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of corner of the rectangle.</param>
        /// <param name="height">The height of corner of the rectangle.</param>
        void Rectangle(double x0, double y0, double width, double height);

        /// <summary>
        /// Fill the current path using the current <see cref="FillStyle"/>.
        /// </summary>
        void Fill();

        /// <summary>
        /// Current line width used to stroke paths.
        /// </summary>
        double LineWidth { get; set; }

        /// <summary>
        /// Current line cap used to stroke paths.
        /// </summary>
        LineCaps LineCap { set; }

        /// <summary>
        /// Current line join used to stroke paths.
        /// </summary>
        LineJoins LineJoin { set; }

        /// <summary>
        /// Set the current line dash pattern.
        /// </summary>
        /// <param name="dash">The line dash pattern.</param>
        void SetLineDash(LineDash dash);

        /// <summary>
        /// The current tag. How this can be used depends on each implementation.
        /// </summary>
        string Tag { get; set; }

        /// <summary>
        /// Draw a raster image.
        /// </summary>
        /// <param name="sourceX">The horizontal coordinate of the top-left corner of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceY">The vertical coordinate of the top-left corner of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceWidth">The width of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceHeight">The height of the rectangle delimiting the source area of the image.</param>
        /// <param name="destinationX">The horizontal coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationY">The vertical coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationWidth">The width of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationHeight">The height of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        void DrawRasterImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image);

        /// <summary>
        /// Draws a <see cref="Graphics"/> object, applying the specified <paramref name="filter"/>.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to draw on the current <see cref="Graphics"/> object.</param>
        /// <param name="filter">An <see cref="IFilter"/> object, representing the filter to apply to the <paramref name="graphics"/> object.</param>
        void DrawFilteredGraphics(Graphics graphics, IFilter filter);
    }

    /// <summary>
    /// The exception that is thrown when an unbalanced graphics state stack occurs.
    /// </summary>
    public class UnbalancedStackException : Exception
    {
        internal UnbalancedStackException(int excessSave, int excessRestore) : base("The graphics state stack is unbalanced!\nThere are " + excessSave + " calls to Graphics.Save() and " + excessRestore + " calls to Graphics.Restore() in excess!") { }
    }

    /// <summary>
    /// Represents an abstract drawing surface.
    /// </summary>
    public partial class Graphics
    {
        /// <summary>
        /// Determines how an unbalanced graphics state stack (which occurs if the number of calls to <see cref="Save"/> and <see cref="Restore"/> is not equal) will be treated. The default is <see cref="UnbalancedStackActions.Throw"/>.
        /// </summary>
        public static UnbalancedStackActions UnbalancedStackAction { get; set; } = UnbalancedStackActions.Throw;

        internal List<IGraphicsAction> Actions = new List<IGraphicsAction>();

        /// <summary>
        /// Fill a <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> to fill.</param>
        /// <param name="fillColour">The <see cref="Brush"/> with which to fill the <see cref="GraphicsPath"/>.</param>
        /// <param name="tag">A tag to identify the filled path.</param>
        public void FillPath(GraphicsPath path, Brush fillColour, string tag = null)
        {
            Actions.Add(new PathAction(path, fillColour, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag, false));
        }

        /// <summary>
        /// Determines whether unique tags should be used for graphics actions that create multiple objects (e.g. drawing text).
        /// </summary>
        public bool UseUniqueTags { get; set; } = true;

        /// <summary>
        /// Stroke a <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> to stroke.</param>
        /// <param name="strokeColour">The <see cref="Brush"/> with which to stroke the <see cref="GraphicsPath"/>.</param>
        /// <param name="lineWidth">The width of the line with which the path is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the path.</param>
        /// <param name="lineJoin">The line join to use to stroke the path.</param>
        /// <param name="lineDash">The line dash to use to stroke the path.</param>
        /// <param name="tag">A tag to identify the stroked path.</param>
        public void StrokePath(GraphicsPath path, Brush strokeColour, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new PathAction(path, null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag, false));
        }

        /// <summary>
        /// Intersect the current clipping path with the specified <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> to intersect with the current clipping path.</param>
        /// <param name="tag">A tag to identify the clipping path.</param>
        public void SetClippingPath(GraphicsPath path, string tag = null)
        {
            Actions.Add(new PathAction(path, null, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag, true));
        }

        /// <summary>
        /// Intersect the current clipping path with the specified rectangle.
        /// </summary>
        /// <param name="leftX">The horizontal coordinate of the top-left corner of the rectangle.</param>
        /// <param name="topY">The vertical coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="tag">A tag to identify the clipping path.</param>
        public void SetClippingPath(double leftX, double topY, double width, double height, string tag = null)
        {
            SetClippingPath(new Point(leftX, topY), new Size(width, height), tag);
        }

        /// <summary>
        /// Intersect the current clipping path with the specified rectangle.
        /// </summary>
        /// <param name="topLeft">The top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="tag">A tag to identify the clipping path.</param>
        public void SetClippingPath(Point topLeft, Size size, string tag = null)
        {
            Actions.Add(new PathAction(new GraphicsPath().MoveTo(topLeft).LineTo(topLeft.X + size.Width, topLeft.Y).LineTo(topLeft.X + size.Width, topLeft.Y + size.Height).LineTo(topLeft.X, topLeft.Y + size.Height).Close(), null, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag, true));
        }

        /// <summary>
        /// Rotate the coordinate system around the origin.
        /// </summary>
        /// <param name="angle">The angle (in radians) by which to rotate the coordinate system.</param>
        /// <param name="tag">A tag to identify the transform.</param>
        public void Rotate(double angle, string tag = null)
        {
            Actions.Add(new TransformAction(angle, tag));
        }

        /// <summary>
        /// Rotate the coordinate system around a pivot point.
        /// </summary>
        /// <param name="angle">The angle (in radians) by which to rotate the coordinate system.</param>
        /// <param name="pivot">The pivot around which the coordinate system is to be rotated.</param>
        /// <param name="tag">A tag to identify the transform.</param>
        public void RotateAt(double angle, Point pivot, string tag = null)
        {
            string tag1 = null;
            string tag2 = null;
            string tag3 = null;

            if (!string.IsNullOrEmpty(tag))
            {
                if (UseUniqueTags)
                {
                    tag1 = tag + "_@1";
                    tag2 = tag + "_@2";
                    tag3 = tag + "_@3";
                }
                else
                {
                    tag1 = tag;
                    tag2 = tag;
                    tag3 = tag;
                }
            }

            Actions.Add(new TransformAction(pivot, tag1));
            Actions.Add(new TransformAction(angle, tag2));
            Actions.Add(new TransformAction(new Point(-pivot.X, -pivot.Y), tag3));
        }


        /// <summary>
        /// Transform the coordinate system with the specified transformation matrix [ [a, c, e], [b, d, f], [0, 0, 1] ].
        /// </summary>
        /// <param name="a">The first element of the first column.</param>
        /// <param name="b">The second element of the first column.</param>
        /// <param name="c">The first element of the second column.</param>
        /// <param name="d">The second element of the second column.</param>
        /// <param name="e">The first element of the third column.</param>
        /// <param name="f">The second element of the third column.</param>
        /// <param name="tag">A tag to identify the transform.</param>
        public void Transform(double a, double b, double c, double d, double e, double f, string tag = null)
        {
            double[,] matrix = new double[,] { { a, c, e }, { b, d, f }, { 0, 0, 1 } };
            Actions.Add(new TransformAction(matrix, tag));
        }

        /// <summary>
        /// Translate the coordinate system origin.
        /// </summary>
        /// <param name="x">The horizontal translation.</param>
        /// <param name="y">The vertical translation.</param>
        /// <param name="tag">A tag to identify the transform.</param>
        public void Translate(double x, double y, string tag = null)
        {
            Actions.Add(new TransformAction(new Point(x, y), tag));
        }

        /// <summary>
        /// Translate the coordinate system origin.
        /// </summary>
        /// <param name="delta">The new origin point.</param>
        /// <param name="tag">A tag to identify the transform.</param>
        public void Translate(Point delta, string tag = null)
        {
            Actions.Add(new TransformAction(delta, tag));
        }

        /// <summary>
        /// Scale the coordinate system with respect to the origin.
        /// </summary>
        /// <param name="scaleX">The horizontal scale.</param>
        /// <param name="scaleY">The vertical scale.</param>
        /// <param name="tag">A tag to identify the transform.</param>
        public void Scale(double scaleX, double scaleY, string tag = null)
        {
            Actions.Add(new TransformAction(new Size(scaleX, scaleY), tag));
        }

        /// <summary>
        /// Fill a rectangle.
        /// </summary>
        /// <param name="topLeft">The top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="fillColour">The colour with which to fill the rectangle.</param>
        /// <param name="tag">A tag to identify the filled rectangle.</param>
        public void FillRectangle(Point topLeft, Size size, Brush fillColour, string tag = null)
        {
            Actions.Add(new RectangleAction(topLeft, size, fillColour, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Fill a rectangle.
        /// </summary>
        /// <param name="leftX">The horizontal coordinate of the top-left corner of the rectangle.</param>
        /// <param name="topY">The vertical coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="fillColour">The colour with which to fill the rectangle.</param>
        /// <param name="tag">A tag to identify the filled rectangle.</param>
        public void FillRectangle(double leftX, double topY, double width, double height, Brush fillColour, string tag = null)
        {
            Actions.Add(new RectangleAction(new Point(leftX, topY), new Size(width, height), fillColour, null, 0, LineCaps.Butt, LineJoins.Miter, LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Stroke a rectangle.
        /// </summary>
        /// <param name="topLeft">The top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="strokeColour">The colour with which to stroke the rectangle.</param>
        /// <param name="lineWidth">The width of the line with which the rectangle is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the rectangle.</param>
        /// <param name="lineJoin">The line join to use to stroke the rectangle.</param>
        /// <param name="lineDash">The line dash to use to stroke the rectangle.</param>
        /// <param name="tag">A tag to identify the filled rectangle.</param>
        public void StrokeRectangle(Point topLeft, Size size, Brush strokeColour, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new RectangleAction(topLeft, size, null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Stroke a rectangle.
        /// </summary>
        /// <param name="leftX">The horizontal coordinate of the top-left corner of the rectangle.</param>
        /// <param name="topY">The vertical coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="strokeColour">The colour with which to stroke the rectangle.</param>
        /// <param name="lineWidth">The width of the line with which the rectangle is stroked.</param>
        /// <param name="lineCap">The line cap to use to stroke the rectangle.</param>
        /// <param name="lineJoin">The line join to use to stroke the rectangle.</param>
        /// <param name="lineDash">The line dash to use to stroke the rectangle.</param>
        /// <param name="tag">A tag to identify the filled rectangle.</param>
        public void StrokeRectangle(double leftX, double topY, double width, double height, Brush strokeColour, double lineWidth = 1, LineCaps lineCap = LineCaps.Butt, LineJoins lineJoin = LineJoins.Miter, LineDash? lineDash = null, string tag = null)
        {
            Actions.Add(new RectangleAction(new Point(leftX, topY), new Size(width, height), null, strokeColour, lineWidth, lineCap, lineJoin, lineDash ?? LineDash.SolidLine, tag));
        }

        /// <summary>
        /// Draw a raster image.
        /// </summary>
        /// <param name="sourceX">The horizontal coordinate of the top-left corner of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceY">The vertical coordinate of the top-left corner of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceWidth">The width of the rectangle delimiting the source area of the image.</param>
        /// <param name="sourceHeight">The height of the rectangle delimiting the source area of the image.</param>
        /// <param name="destinationX">The horizontal coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationY">The vertical coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationWidth">The width of the rectangle delimiting the destination area of the image.</param>
        /// <param name="destinationHeight">The height of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        /// <param name="tag">A tag to identify the drawn image.</param>
        public void DrawRasterImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image, string tag = null)
        {
            Actions.Add(new RasterImageAction(sourceX, sourceY, sourceWidth, sourceHeight, destinationX, destinationY, destinationWidth, destinationHeight, image, tag));
        }

        /// <summary>
        /// Draw a raster image.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="y">The vertical coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        /// <param name="tag">A tag to identify the drawn image.</param>
        public void DrawRasterImage(double x, double y, RasterImage image, string tag = null)
        {
            DrawRasterImage(0, 0, image.Width, image.Height, x, y, image.Width, image.Height, image, tag);
        }

        /// <summary>
        /// Draw a raster image.
        /// </summary>
        /// <param name="position">The the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        /// <param name="tag">A tag to identify the drawn image.</param>
        public void DrawRasterImage(Point position, RasterImage image, string tag = null)
        {
            DrawRasterImage(0, 0, image.Width, image.Height, position.X, position.Y, image.Width, image.Height, image, tag);
        }

        /// <summary>
        /// Draw a raster image.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="y">The vertical coordinate of the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="width">The width of the rectangle delimiting the destination area of the image.</param>
        /// <param name="height">The height of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        /// <param name="tag">A tag to identify the drawn image.</param>
        public void DrawRasterImage(double x, double y, double width, double height, RasterImage image, string tag = null)
        {
            DrawRasterImage(0, 0, image.Width, image.Height, x, y, width, height, image, tag);
        }

        /// <summary>
        /// Draw a raster image.
        /// </summary>
        /// <param name="position">The the top-left corner of the rectangle delimiting the destination area of the image.</param>
        /// <param name="size">The size of the rectangle delimiting the destination area of the image.</param>
        /// <param name="image">The image to draw.</param>
        /// <param name="tag">A tag to identify the drawn image.</param>
        public void DrawRasterImage(Point position, Size size, RasterImage image, string tag = null)
        {
            DrawRasterImage(0, 0, image.Width, image.Height, position.X, position.Y, size.Width, size.Height, image, tag);
        }

        /// <summary>
        /// Save the current transform state (rotation, translation, scale).
        /// </summary>
        public void Save()
        {
            Actions.Add(new StateAction(StateAction.StateActionTypes.Save));
        }

        /// <summary>
        /// Restore the previous transform state (rotation, translation scale).
        /// </summary>
        public void Restore()
        {
            Actions.Add(new StateAction(StateAction.StateActionTypes.Restore));
        }

        private void FixGraphicsStateStack()
        {
            if (UnbalancedStackAction == UnbalancedStackActions.Ignore)
            {
                return;
            }

            int count = 0;
            List<int> toBeRemoved = new List<int>();

            for (int i = 0; i < this.Actions.Count; i++)
            {
                if (this.Actions[i] is StateAction st)
                {
                    if (st.StateActionType == StateAction.StateActionTypes.Save)
                    {
                        count++;
                    }
                    else if (st.StateActionType == StateAction.StateActionTypes.Restore)
                    {
                        if (count == 0)
                        {
                            toBeRemoved.Add(i);
                        }
                        else
                        {
                            count--;
                        }
                    }
                }
            }

            if (UnbalancedStackAction == UnbalancedStackActions.Throw)
            {
                if (count > 0 || toBeRemoved.Count > 0)
                {
                    throw new UnbalancedStackException(count, toBeRemoved.Count);
                }
            }
            else if (UnbalancedStackAction == UnbalancedStackActions.SilentlyFix)
            {
                if (count > 0 || toBeRemoved.Count > 0)
                {
                    for (int i = toBeRemoved.Count - 1; i >= 0; i--)
                    {
                        this.Actions.RemoveAt(toBeRemoved[i]);
                    }

                    for (int i = 0; i < count; i++)
                    {
                        this.Restore();
                    }

                    this.FixGraphicsStateStack();
                }
            }
        }

        /// <summary>
        /// Copy the current graphics to an instance of a class implementing <see cref="IGraphicsContext"/>.
        /// </summary>
        /// <param name="destinationContext">The <see cref="IGraphicsContext"/> on which the graphics are to be copied.</param>
        public void CopyToIGraphicsContext(IGraphicsContext destinationContext)
        {
            this.FixGraphicsStateStack();

            for (int i = 0; i < this.Actions.Count; i++)
            {
                if (this.Actions[i] is RectangleAction)
                {
                    RectangleAction rec = this.Actions[i] as RectangleAction;

                    destinationContext.Tag = rec.Tag;
                    destinationContext.Rectangle(rec.TopLeft.X, rec.TopLeft.Y, rec.Size.Width, rec.Size.Height);

                    if (rec.Fill != null)
                    {
                        if (destinationContext.FillStyle != rec.Fill)
                        {
                            destinationContext.SetFillStyle(rec.Fill);
                        }
                        destinationContext.Fill();
                    }
                    else if (rec.Stroke != null)
                    {
                        if (destinationContext.StrokeStyle != rec.Stroke)
                        {
                            destinationContext.SetStrokeStyle(rec.Stroke);
                        }
                        if (destinationContext.LineWidth != rec.LineWidth)
                        {
                            destinationContext.LineWidth = rec.LineWidth;
                        }
                        destinationContext.SetLineDash(rec.LineDash);
                        destinationContext.LineCap = rec.LineCap;
                        destinationContext.LineJoin = rec.LineJoin;

                        destinationContext.Stroke();
                    }
                }
                else if (this.Actions[i] is PathAction)
                {
                    PathAction pth = this.Actions[i] as PathAction;

                    destinationContext.Tag = pth.Tag;

                    for (int j = 0; j < pth.Path.Segments.Count; j++)
                    {
                        switch (pth.Path.Segments[j].Type)
                        {
                            case SegmentType.Move:
                                destinationContext.MoveTo(pth.Path.Segments[j].Point.X, pth.Path.Segments[j].Point.Y);
                                break;
                            case SegmentType.Line:
                                destinationContext.LineTo(pth.Path.Segments[j].Point.X, pth.Path.Segments[j].Point.Y);
                                break;
                            case SegmentType.CubicBezier:
                                destinationContext.CubicBezierTo(pth.Path.Segments[j].Points[0].X, pth.Path.Segments[j].Points[0].Y, pth.Path.Segments[j].Points[1].X, pth.Path.Segments[j].Points[1].Y, pth.Path.Segments[j].Points[2].X, pth.Path.Segments[j].Points[2].Y);
                                break;
                            case SegmentType.Arc:
                                {
                                    ArcSegment seg = pth.Path.Segments[j] as ArcSegment;
                                    Segment[] segs = seg.ToBezierSegments();
                                    for (int k = 0; k < segs.Length; k++)
                                    {
                                        switch (segs[k].Type)
                                        {
                                            case SegmentType.Move:
                                                destinationContext.MoveTo(segs[k].Point.X, segs[k].Point.Y);
                                                break;
                                            case SegmentType.Line:
                                                destinationContext.LineTo(segs[k].Point.X, segs[k].Point.Y);
                                                break;
                                            case SegmentType.CubicBezier:
                                                destinationContext.CubicBezierTo(segs[k].Points[0].X, segs[k].Points[0].Y, segs[k].Points[1].X, segs[k].Points[1].Y, segs[k].Points[2].X, segs[k].Points[2].Y);
                                                break;
                                        }
                                    }
                                }
                                break;
                            case SegmentType.Close:
                                destinationContext.Close();
                                break;
                        }
                    }

                    if (pth.IsClipping)
                    {
                        destinationContext.SetClippingPath();
                    }
                    else
                    {
                        if (pth.Fill != null)
                        {
                            if (destinationContext.FillStyle != pth.Fill)
                            {
                                destinationContext.SetFillStyle(pth.Fill);
                            }
                            destinationContext.Fill();
                        }
                        else if (pth.Stroke != null)
                        {
                            if (destinationContext.StrokeStyle != pth.Stroke)
                            {
                                destinationContext.SetStrokeStyle(pth.Stroke);
                            }
                            if (destinationContext.LineWidth != pth.LineWidth)
                            {
                                destinationContext.LineWidth = pth.LineWidth;
                            }
                            destinationContext.SetLineDash(pth.LineDash);
                            destinationContext.LineCap = pth.LineCap;
                            destinationContext.LineJoin = pth.LineJoin;

                            destinationContext.Stroke();
                        }
                    }
                }
                else if (this.Actions[i] is TextAction)
                {
                    TextAction txt = this.Actions[i] as TextAction;

                    destinationContext.Tag = txt.Tag;
                    if (destinationContext.TextBaseline != txt.TextBaseline)
                    {
                        destinationContext.TextBaseline = txt.TextBaseline;
                    }
                    destinationContext.Font = txt.Font;

                    if (txt.Fill != null)
                    {
                        if (destinationContext.FillStyle != txt.Fill)
                        {
                            destinationContext.SetFillStyle(txt.Fill);
                        }
                        destinationContext.FillText(txt.Text, txt.Origin.X, txt.Origin.Y);
                    }
                    else if (txt.Stroke != null)
                    {
                        if (destinationContext.StrokeStyle != txt.Stroke)
                        {
                            destinationContext.SetStrokeStyle(txt.Stroke);
                        }
                        if (destinationContext.LineWidth != txt.LineWidth)
                        {
                            destinationContext.LineWidth = txt.LineWidth;
                        }
                        destinationContext.SetLineDash(txt.LineDash);
                        destinationContext.LineCap = txt.LineCap;
                        destinationContext.LineJoin = txt.LineJoin;

                        destinationContext.StrokeText(txt.Text, txt.Origin.X, txt.Origin.Y);
                    }
                }
                else if (this.Actions[i] is TransformAction)
                {
                    TransformAction trf = this.Actions[i] as TransformAction;

                    destinationContext.Tag = trf.Tag;

                    if (trf.Delta != null)
                    {
                        destinationContext.Translate(((Point)trf.Delta).X, ((Point)trf.Delta).Y);
                    }
                    else if (trf.Angle != null)
                    {
                        destinationContext.Rotate((double)trf.Angle);
                    }
                    else if (trf.Scale != null)
                    {
                        destinationContext.Scale(((Size)trf.Scale).Width, ((Size)trf.Scale).Height);
                    }
                    else if (trf.Matrix != null)
                    {
                        destinationContext.Transform(trf.Matrix[0, 0], trf.Matrix[1, 0], trf.Matrix[0, 1], trf.Matrix[1, 1], trf.Matrix[0, 2], trf.Matrix[1, 2]);
                    }
                }
                else if (this.Actions[i] is StateAction)
                {
                    destinationContext.Tag = ((StateAction)this.Actions[i]).Tag;

                    if (((StateAction)this.Actions[i]).StateActionType == StateAction.StateActionTypes.Save)
                    {
                        destinationContext.Save();
                    }
                    else
                    {
                        destinationContext.Restore();
                    }
                }
                else if (this.Actions[i] is RasterImageAction)
                {
                    RasterImageAction img = this.Actions[i] as RasterImageAction;
                    destinationContext.Tag = img.Tag;
                    destinationContext.DrawRasterImage(img.SourceX, img.SourceY, img.SourceWidth, img.SourceHeight, img.DestinationX, img.DestinationY, img.DestinationWidth, img.DestinationHeight, img.Image);
                }
                else if (this.Actions[i] is FilteredGraphicsAction)
                {
                    FilteredGraphicsAction fil = this.Actions[i] as FilteredGraphicsAction;
                    destinationContext.Tag = fil.Tag;
                    destinationContext.DrawFilteredGraphics(fil.Content, fil.Filter);
                }
            }
        }

        /// <summary>
        /// Draws a <see cref="Graphics"/> object on the current <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="origin">The point at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="graphics">The <see cref="Graphics"/> object to draw on the current <see cref="Graphics"/> object.</param>
        public void DrawGraphics(Point origin, Graphics graphics)
        {
            this.Save();
            this.Translate(origin);

            graphics.FixGraphicsStateStack();

            this.Actions.AddRange(graphics.Actions);

            this.Restore();
        }

        /// <summary>
        /// Draws a <see cref="Graphics"/> object on the current <see cref="Graphics"/> object, prepending the supplied <paramref name="tag"/> to the tags contained in the <see cref="Graphics"/> object being drawn.
        /// </summary>
        /// <param name="originX">The horizontal coordinate at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="originY">The vertical coordinate at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="graphics">The <see cref="Graphics"/> object to draw on the current <see cref="Graphics"/> object.</param>
        /// <param name="tag">The tag to prepend to the tags contained in the <paramref name="graphics"/> object.</param>
        public void DrawGraphics(double originX, double originY, Graphics graphics, string tag)
        {
            this.DrawGraphics(new Point(originX, originY), graphics, tag);
        }

        /// <summary>
        /// Draws a <see cref="Graphics"/> object on the current <see cref="Graphics"/> object, prepending the supplied <paramref name="tag"/> to the tags contained in the <see cref="Graphics"/> object being drawn.
        /// </summary>
        /// <param name="origin">The point at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="graphics">The <see cref="Graphics"/> object to draw on the current <see cref="Graphics"/> object.</param>
        /// <param name="tag">The tag to prepend to the tags contained in the <paramref name="graphics"/> object.</param>
        public void DrawGraphics(Point origin, Graphics graphics, string tag)
        {
            this.Save();

            if (!string.IsNullOrEmpty(tag) && UseUniqueTags)
            {
                this.Translate(origin, tag + "@t");
            }
            else
            {
                this.Translate(origin, tag);
            }

            graphics.FixGraphicsStateStack();

            foreach (IGraphicsAction action in graphics.Actions)
            {
                IGraphicsAction clone = action.ShallowClone();

                if (clone is IPrintableAction print)
                {
                    if (UseUniqueTags)
                    {
                        if (string.IsNullOrEmpty(print.Tag))
                        {
                            print.Tag = tag;
                        }
                        else
                        {
                            print.Tag = tag + "/" + print.Tag;
                        }
                    }
                }

                this.Actions.Add(clone);
            }

            this.Restore();
        }

        /// <summary>
        /// Draws a <see cref="Graphics"/> object on the current <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="originX">The horizontal coordinate at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="originY">The vertical coordinate at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="graphics">The <see cref="Graphics"/> object to draw on the current <see cref="Graphics"/> object.</param>
        public void DrawGraphics(double originX, double originY, Graphics graphics)
        {
            this.DrawGraphics(new Point(originX, originY), graphics);
        }

        /// <summary>
        /// Draws a <see cref="Graphics"/> object on the current <see cref="Graphics"/> object, applying the specified <paramref name="filter"/>.
        /// </summary>
        /// <param name="origin">The point at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="graphics">The <see cref="Graphics"/> object to draw on the current <see cref="Graphics"/> object.</param>
        /// <param name="filter">An <see cref="IFilter"/> object, representing the filter to apply to the <paramref name="graphics"/> object.</param>
        /// <param name="tag">A tag to identify the filter.</param>
        public void DrawGraphics(Point origin, Graphics graphics, IFilter filter, string tag = null)
        {
            this.Save();

            if (!string.IsNullOrEmpty(tag) && UseUniqueTags)
            {
                this.Translate(origin, tag + "@t");
            }
            else
            {
                this.Translate(origin, tag);
            }

            Graphics clone = new Graphics();
            clone.Actions.AddRange(graphics.Actions);
            clone.FixGraphicsStateStack();

            this.Actions.Add(new FilteredGraphicsAction(clone, filter) { Tag = tag });

            this.Restore();
        }

        /// <summary>
        /// Draws a <see cref="Graphics"/> object on the current <see cref="Graphics"/> object, applying the specified <paramref name="filter"/>.
        /// </summary>
        /// <param name="originX">The horizontal coordinate at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="originY">The vertical coordinate at which to place the origin of <paramref name="graphics"/>.</param>
        /// <param name="graphics">The <see cref="Graphics"/> object to draw on the current <see cref="Graphics"/> object.</param>
        /// <param name="filter">An <see cref="IFilter"/> object, representing the filter to apply to the <paramref name="graphics"/> object.</param>
        /// <param name="tag">A tag to identify the filter.</param>
        public void DrawGraphics(double originX, double originY, Graphics graphics, IFilter filter, string tag = null)
        {
            this.DrawGraphics(new Point(originX, originY), graphics, filter, tag);
        }


        internal static Point Multiply(double[,] matrix, Point pt)
        {
            double x = matrix[0, 0] * pt.X + matrix[0, 1] * pt.Y + matrix[0, 2];
            double y = matrix[1, 0] * pt.X + matrix[1, 1] * pt.Y + matrix[1, 2];
            double z = matrix[2, 0] * pt.X + matrix[2, 1] * pt.Y + matrix[2, 2];

            return new Point(x / z, y / z);
        }

        internal static double[,] Multiply(double[,] m1, double[,] m2)
        {
            return new double[3, 3]
            {
                { m1[0,0] * m2[0,0] + m1[0,1] * m2[1,0] + m1[0,2] *m2[2,0], m1[0,0] * m2[0,1] + m1[0,1] * m2[1,1] + m1[0,2] * m2[2,1], m1[0,0] * m2[0,2] + m1[0,1] * m2[1,2] + m1[0,2] * m2[2,2] },
                { m1[1,0] * m2[0,0] + m1[1,1] * m2[1,0] + m1[1,2] *m2[2,0], m1[1,0] * m2[0,1] + m1[1,1] * m2[1,1] + m1[1,2] * m2[2,1], m1[1,0] * m2[0,2] + m1[1,1] * m2[1,2] + m1[1,2] * m2[2,2] },
                { m1[2,0] * m2[0,0] + m1[2,1] * m2[1,0] + m1[2,2] *m2[2,0], m1[2,0] * m2[0,1] + m1[2,1] * m2[1,1] + m1[2,2] * m2[2,1], m1[2,0] * m2[0,2] + m1[2,1] * m2[1,2] + m1[2,2] * m2[2,2] }
            };
        }

        internal static double[,] TranslationMatrix(double dx, double dy)
        {
            return new double[3, 3]
            {
                {1, 0, dx },
                {0, 1, dy },
                {0, 0, 1 }
            };
        }

        internal static double[,] ScaleMatrix(double sx, double sy)
        {
            return new double[3, 3]
            {
                {sx, 0, 0 },
                {0, sy, 0 },
                {0, 0, 1 }
            };
        }

        internal static double[,] RotationMatrix(double theta)
        {
            return new double[3, 3]
            {
                {Math.Cos(theta), -Math.Sin(theta), 0 },
                {Math.Sin(theta), Math.Cos(theta), 0 },
                {0, 0, 1 }
            };
        }

        internal static double[,] Invert(double[,] m)
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

        /// <summary>
        /// Creates a new <see cref="Graphics"/> object in which all the graphics actions have been transformed using an arbitrary transformation function. Raster images are replaced by grey rectangles.
        /// </summary>
        /// <param name="transformationFunction">An arbitrary transformation function.</param>
        /// <param name="linearisationResolution">The resolution that will be used to linearise curve segments.</param>
        /// <returns>A new <see cref="Graphics"/> object in which all graphics actions have been linearised and transformed using the <paramref name="transformationFunction"/>.</returns>
        public Graphics Transform(Func<Point, Point> transformationFunction, double linearisationResolution)
        {
            Graphics destinationGraphics = new Graphics();

            Stack<double[,]> transformMatrix = new Stack<double[,]>();
            double[,] currMatrix = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

            for (int i = 0; i < this.Actions.Count; i++)
            {
                if (this.Actions[i] is RectangleAction)
                {
                    RectangleAction rec = this.Actions[i] as RectangleAction;

                    GraphicsPath rectanglePath = new GraphicsPath();

                    Point pt1 = transformationFunction(Multiply(currMatrix, rec.TopLeft));
                    Point pt2 = transformationFunction(Multiply(currMatrix, new Point(rec.TopLeft.X + rec.Size.Width, rec.TopLeft.Y)));
                    Point pt3 = transformationFunction(Multiply(currMatrix, new Point(rec.TopLeft.X + rec.Size.Width, rec.TopLeft.Y + rec.Size.Height)));
                    Point pt4 = transformationFunction(Multiply(currMatrix, new Point(rec.TopLeft.X, rec.TopLeft.Y + rec.Size.Height)));

                    rectanglePath.MoveTo(pt1).LineTo(pt2).LineTo(pt3).LineTo(pt4).Close();

                    if (rec.Fill != null)
                    {
                        destinationGraphics.FillPath(rectanglePath, rec.Fill, rec.Tag);
                    }
                    else if (rec.Stroke != null)
                    {
                        destinationGraphics.StrokePath(rectanglePath, rec.Stroke, rec.LineWidth, rec.LineCap, rec.LineJoin, rec.LineDash, rec.Tag);
                    }
                }
                else if (this.Actions[i] is PathAction)
                {
                    PathAction pth = this.Actions[i] as PathAction;

                    GraphicsPath newPath = pth.Path.Linearise(linearisationResolution).Transform(pt => transformationFunction(Multiply(currMatrix, pt)));

                    if (pth.IsClipping)
                    {
                        destinationGraphics.SetClippingPath(newPath);
                    }
                    else
                    {
                        if (pth.Fill != null)
                        {
                            destinationGraphics.FillPath(newPath, pth.Fill, pth.Tag);
                        }
                        else if (pth.Stroke != null)
                        {
                            destinationGraphics.StrokePath(newPath, pth.Stroke, pth.LineWidth, pth.LineCap, pth.LineJoin, pth.LineDash, pth.Tag);
                        }
                    }
                }
                else if (this.Actions[i] is TextAction)
                {
                    TextAction txt = this.Actions[i] as TextAction;

                    GraphicsPath textPath = new GraphicsPath().AddText(txt.Origin, txt.Text, txt.Font, txt.TextBaseline).Linearise(linearisationResolution).Transform(pt => transformationFunction(Multiply(currMatrix, pt)));

                    if (txt.Fill != null)
                    {
                        destinationGraphics.FillPath(textPath, txt.Fill, txt.Tag);
                    }
                    else if (txt.Stroke != null)
                    {
                        destinationGraphics.StrokePath(textPath, txt.Stroke, txt.LineWidth, txt.LineCap, txt.LineJoin, txt.LineDash, txt.Tag);
                    }
                }
                else if (this.Actions[i] is TransformAction)
                {
                    TransformAction trf = this.Actions[i] as TransformAction;

                    if (trf.Delta != null)
                    {
                        currMatrix = Multiply(currMatrix, TranslationMatrix(trf.Delta.Value.X, trf.Delta.Value.Y));
                    }
                    else if (trf.Angle != null)
                    {
                        currMatrix = Multiply(currMatrix, RotationMatrix(trf.Angle.Value));
                    }
                    else if (trf.Scale != null)
                    {
                        currMatrix = Multiply(currMatrix, ScaleMatrix(trf.Scale.Value.Width, trf.Scale.Value.Height));
                    }
                    else if (trf.Matrix != null)
                    {
                        currMatrix = Multiply(currMatrix, trf.Matrix);
                    }
                }
                else if (this.Actions[i] is StateAction)
                {
                    if (((StateAction)this.Actions[i]).StateActionType == StateAction.StateActionTypes.Save)
                    {
                        transformMatrix.Push(currMatrix);
                    }
                    else
                    {
                        currMatrix = transformMatrix.Pop();
                    }
                }
                else if (this.Actions[i] is RasterImageAction)
                {
                    RasterImageAction img = this.Actions[i] as RasterImageAction;

                    GraphicsPath rectanglePath = new GraphicsPath();

                    Point pt1 = transformationFunction(Multiply(currMatrix, new Point(img.DestinationX, img.DestinationY)));
                    Point pt2 = transformationFunction(Multiply(currMatrix, new Point(img.DestinationX + img.DestinationWidth, img.DestinationY)));
                    Point pt3 = transformationFunction(Multiply(currMatrix, new Point(img.DestinationX + img.DestinationWidth, img.DestinationY + img.DestinationHeight)));
                    Point pt4 = transformationFunction(Multiply(currMatrix, new Point(img.DestinationX, img.DestinationY + img.DestinationHeight)));

                    rectanglePath.MoveTo(pt1).LineTo(pt2).LineTo(pt3).LineTo(pt4).Close();

                    destinationGraphics.FillPath(rectanglePath, Colour.FromRgb(220, 220, 220), img.Tag);
                }
            }

            return destinationGraphics;
        }

        internal static GraphicsPath ReduceMaximumLength(GraphicsPath path, double maxLength)
        {
            GraphicsPath shortLinearisedPath = new GraphicsPath();

            // Square of the max length - so that we avoid the square roots.
            double maxLengthSq = maxLength * maxLength;

            Point currPoint = new Point();
            Point startFigurePoint = new Point();

            foreach (Segment seg in path.Segments)
            {
                if (seg is MoveSegment mov)
                {
                    shortLinearisedPath.MoveTo(mov.Point);
                    startFigurePoint = mov.Point;
                    currPoint = mov.Point;
                }
                else if (seg is LineSegment line)
                {
                    double lengthSq = (line.Point.X - currPoint.X) * (line.Point.X - currPoint.X) +
                                      (line.Point.Y - currPoint.Y) * (line.Point.Y - currPoint.Y);

                    if (lengthSq < maxLengthSq)
                    {
                        shortLinearisedPath.LineTo(line.Point);
                    }
                    else
                    {
                        int segmentCount = (int)Math.Ceiling(Math.Sqrt(lengthSq / maxLengthSq));

                        for (int j = 0; j < segmentCount - 1; j++)
                        {
                            Point endPoint = new Point(currPoint.X + (line.Point.X - currPoint.X) * (j + 1) / segmentCount,
                                                       currPoint.Y + (line.Point.Y - currPoint.Y) * (j + 1) / segmentCount);
                            shortLinearisedPath.LineTo(endPoint);
                        }

                        shortLinearisedPath.LineTo(line.Point);
                    }
                    currPoint = line.Point;
                }
                else if (seg is CloseSegment close)
                {
                    double lengthSq = (startFigurePoint.X - currPoint.X) * (startFigurePoint.X - currPoint.X) +
                                      (startFigurePoint.Y - currPoint.Y) * (startFigurePoint.Y - currPoint.Y);

                    if (lengthSq < maxLengthSq)
                    {
                        shortLinearisedPath.Close();
                    }
                    else
                    {
                        int segmentCount = (int)Math.Ceiling(Math.Sqrt(lengthSq / maxLengthSq));

                        for (int j = 0; j < segmentCount - 1; j++)
                        {
                            Point endPoint = new Point(currPoint.X + (startFigurePoint.X - currPoint.X) * (j + 1) / segmentCount,
                                                       currPoint.Y + (startFigurePoint.Y - currPoint.Y) * (j + 1) / segmentCount);
                            shortLinearisedPath.LineTo(endPoint);
                        }

                        shortLinearisedPath.Close();
                    }
                    currPoint = startFigurePoint;
                }
            }

            return shortLinearisedPath;
        }

        /// <summary>
        /// Creates a new <see cref="Graphics"/> object in which all the graphics actions have been transformed using an arbitrary transformation function. Raster images are replaced by grey rectangles.
        /// </summary>
        /// <param name="transformationFunction">An arbitrary transformation function.</param>
        /// <param name="linearisationResolution">The resolution that will be used to linearise curve segments.</param>
        /// <param name="maxSegmentLength">The maximum length of line segments.</param>
        /// <returns>A new <see cref="Graphics"/> object in which all graphics actions have been linearised and transformed using the <paramref name="transformationFunction"/>.</returns>
        public Graphics Transform(Func<Point, Point> transformationFunction, double linearisationResolution, double maxSegmentLength)
        {
            Graphics destinationGraphics = new Graphics();

            Stack<double[,]> transformMatrix = new Stack<double[,]>();
            double[,] currMatrix = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

            for (int i = 0; i < this.Actions.Count; i++)
            {
                if (this.Actions[i] is RectangleAction)
                {
                    RectangleAction rec = this.Actions[i] as RectangleAction;

                    GraphicsPath rectanglePath = new GraphicsPath();

                    Point pt1 = rec.TopLeft;
                    Point pt2 = new Point(rec.TopLeft.X + rec.Size.Width, rec.TopLeft.Y);
                    Point pt3 = new Point(rec.TopLeft.X + rec.Size.Width, rec.TopLeft.Y + rec.Size.Height);
                    Point pt4 = new Point(rec.TopLeft.X, rec.TopLeft.Y + rec.Size.Height);

                    rectanglePath.MoveTo(pt1).LineTo(pt2).LineTo(pt3).LineTo(pt4).Close();

                    rectanglePath = ReduceMaximumLength(rectanglePath, maxSegmentLength).Transform(pt => transformationFunction(Multiply(currMatrix, pt)));

                    if (rec.Fill != null)
                    {
                        destinationGraphics.FillPath(rectanglePath, rec.Fill, rec.Tag);
                    }
                    else if (rec.Stroke != null)
                    {
                        destinationGraphics.StrokePath(rectanglePath, rec.Stroke, rec.LineWidth, rec.LineCap, rec.LineJoin, rec.LineDash, rec.Tag);
                    }
                }
                else if (this.Actions[i] is PathAction)
                {
                    PathAction pth = this.Actions[i] as PathAction;

                    GraphicsPath newPath = ReduceMaximumLength(pth.Path.Linearise(linearisationResolution), maxSegmentLength).Transform(pt => transformationFunction(Multiply(currMatrix, pt)));

                    if (pth.IsClipping)
                    {
                        destinationGraphics.SetClippingPath(newPath);
                    }
                    else
                    {
                        if (pth.Fill != null)
                        {
                            destinationGraphics.FillPath(newPath, pth.Fill, pth.Tag);
                        }
                        else if (pth.Stroke != null)
                        {
                            destinationGraphics.StrokePath(newPath, pth.Stroke, pth.LineWidth, pth.LineCap, pth.LineJoin, pth.LineDash, pth.Tag);
                        }
                    }
                }
                else if (this.Actions[i] is TextAction)
                {
                    TextAction txt = this.Actions[i] as TextAction;

                    GraphicsPath textPath = ReduceMaximumLength(new GraphicsPath().AddText(txt.Origin, txt.Text, txt.Font, txt.TextBaseline).Linearise(linearisationResolution), maxSegmentLength).Transform(pt => transformationFunction(Multiply(currMatrix, pt)));

                    if (txt.Fill != null)
                    {
                        destinationGraphics.FillPath(textPath, txt.Fill, txt.Tag);
                    }
                    else if (txt.Stroke != null)
                    {
                        destinationGraphics.StrokePath(textPath, txt.Stroke, txt.LineWidth, txt.LineCap, txt.LineJoin, txt.LineDash, txt.Tag);
                    }
                }
                else if (this.Actions[i] is TransformAction)
                {
                    TransformAction trf = this.Actions[i] as TransformAction;

                    if (trf.Delta != null)
                    {
                        currMatrix = Multiply(currMatrix, TranslationMatrix(trf.Delta.Value.X, trf.Delta.Value.Y));
                    }
                    else if (trf.Angle != null)
                    {
                        currMatrix = Multiply(currMatrix, RotationMatrix(trf.Angle.Value));
                    }
                    else if (trf.Scale != null)
                    {
                        currMatrix = Multiply(currMatrix, ScaleMatrix(trf.Scale.Value.Width, trf.Scale.Value.Height));
                    }
                    else if (trf.Matrix != null)
                    {
                        currMatrix = Multiply(currMatrix, trf.Matrix);
                    }
                }
                else if (this.Actions[i] is StateAction)
                {
                    if (((StateAction)this.Actions[i]).StateActionType == StateAction.StateActionTypes.Save)
                    {
                        transformMatrix.Push(currMatrix);
                    }
                    else
                    {
                        currMatrix = transformMatrix.Pop();
                    }
                }
                else if (this.Actions[i] is RasterImageAction)
                {
                    RasterImageAction img = this.Actions[i] as RasterImageAction;

                    GraphicsPath rectanglePath = new GraphicsPath();

                    Point pt1 = new Point(img.DestinationX, img.DestinationY);
                    Point pt2 = new Point(img.DestinationX + img.DestinationWidth, img.DestinationY);
                    Point pt3 = new Point(img.DestinationX + img.DestinationWidth, img.DestinationY + img.DestinationHeight);
                    Point pt4 = new Point(img.DestinationX, img.DestinationY + img.DestinationHeight);

                    rectanglePath.MoveTo(pt1).LineTo(pt2).LineTo(pt3).LineTo(pt4).Close();

                    rectanglePath = ReduceMaximumLength(rectanglePath, maxSegmentLength).Transform(pt => transformationFunction(Multiply(currMatrix, pt)));

                    destinationGraphics.FillPath(rectanglePath, Colour.FromRgb(220, 220, 220), img.Tag);
                }
            }

            return destinationGraphics;
        }

        /// <summary>
        /// Creates a new <see cref="Graphics"/> object by linearising all of the elements of the current instance, i.e. replacing curve segments with series of line segments that approximate them. Raster images are left unchanged.
        /// </summary>
        /// <param name="resolution">The resolution that will be used to linearise curve segments.</param>
        /// <returns>A new <see cref="Graphics"/> object containing the linearised elements.</returns>
        public Graphics Linearise(double resolution)
        {
            Graphics destinationGraphics = new Graphics();

            for (int i = 0; i < this.Actions.Count; i++)
            {
                if (this.Actions[i] is RectangleAction || this.Actions[i] is TransformAction || this.Actions[i] is StateAction || this.Actions[i] is RasterImageAction)
                {
                    destinationGraphics.Actions.Add(this.Actions[i]);
                }
                else if (this.Actions[i] is PathAction)
                {
                    PathAction pth = this.Actions[i] as PathAction;

                    GraphicsPath newPath = pth.Path.Linearise(resolution);

                    if (pth.IsClipping)
                    {
                        destinationGraphics.SetClippingPath(newPath);
                    }
                    else
                    {
                        if (pth.Fill != null)
                        {
                            destinationGraphics.FillPath(newPath, pth.Fill, pth.Tag);
                        }
                        else if (pth.Stroke != null)
                        {
                            destinationGraphics.StrokePath(newPath, pth.Stroke, pth.LineWidth, pth.LineCap, pth.LineJoin, pth.LineDash, pth.Tag);
                        }
                    }
                }
                else if (this.Actions[i] is TextAction)
                {
                    TextAction txt = this.Actions[i] as TextAction;

                    GraphicsPath textPath = new GraphicsPath().AddText(txt.Origin, txt.Text, txt.Font, txt.TextBaseline).Linearise(resolution);

                    if (txt.Fill != null)
                    {
                        destinationGraphics.FillPath(textPath, txt.Fill, txt.Tag);
                    }
                    else if (txt.Stroke != null)
                    {
                        destinationGraphics.StrokePath(textPath, txt.Stroke, txt.LineWidth, txt.LineCap, txt.LineJoin, txt.LineDash, txt.Tag);
                    }
                }
            }

            return destinationGraphics;
        }

        /// <summary>
        /// Computes the rectangular bounds of the region affected by the drawing operations performed on the <see cref="Graphics"/> object.
        /// </summary>
        /// <returns>The smallest rectangle that contains all the elements drawn on the <see cref="Graphics"/>.</returns>
        public Rectangle GetBounds()
        {
            Rectangle tbr = Rectangle.NaN;
            bool initialised = false;

            Stack<Rectangle> clippingPaths = new Stack<Rectangle>();
            Rectangle currClippingPath = Rectangle.NaN;

            Stack<double[,]> transformMatrix = new Stack<double[,]>();
            double[,] currMatrix = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

            for (int i = 0; i < this.Actions.Count; i++)
            {
                if (this.Actions[i] is IPrintableAction act)
                {
                    Rectangle bounds = act.GetBounds();

                    double lineThickness = double.IsNaN(act.LineWidth) ? 0 : act.LineWidth;

                    Point pt1 = Multiply(currMatrix, new Point(bounds.Location.X - lineThickness * 0.5, bounds.Location.Y - lineThickness * 0.5));
                    Point pt2 = Multiply(currMatrix, new Point(bounds.Location.X + bounds.Size.Width + lineThickness * 0.5, bounds.Location.Y - lineThickness * 0.5));
                    Point pt3 = Multiply(currMatrix, new Point(bounds.Location.X + bounds.Size.Width + lineThickness * 0.5, bounds.Location.Y + bounds.Size.Height + lineThickness * 0.5));
                    Point pt4 = Multiply(currMatrix, new Point(bounds.Location.X - lineThickness * 0.5, bounds.Location.Y + bounds.Size.Height + lineThickness * 0.5));

                    bounds = Point.Bounds(pt1, pt2, pt3, pt4);

                    if (act is PathAction pth && pth.IsClipping)
                    {
                        currClippingPath = bounds;
                    }
                    else
                    {
                        if (!currClippingPath.IsNaN())
                        {
                            bounds = Rectangle.Intersection(bounds, currClippingPath);
                        }

                        if (!double.IsNaN(bounds.Location.X) && !double.IsNaN(bounds.Location.Y) && !double.IsNaN(bounds.Size.Width) && !double.IsNaN(bounds.Size.Height))
                        {
                            if (!initialised)
                            {
                                tbr = bounds;
                                initialised = true;
                            }
                            else
                            {
                                tbr = Rectangle.Union(tbr, bounds);
                            }
                        }
                    }
                }
                else if (this.Actions[i] is TransformAction)
                {
                    TransformAction trf = this.Actions[i] as TransformAction;

                    if (trf.Delta != null)
                    {
                        currMatrix = Multiply(currMatrix, TranslationMatrix(trf.Delta.Value.X, trf.Delta.Value.Y));
                    }
                    else if (trf.Angle != null)
                    {
                        currMatrix = Multiply(currMatrix, RotationMatrix(trf.Angle.Value));
                    }
                    else if (trf.Scale != null)
                    {
                        currMatrix = Multiply(currMatrix, ScaleMatrix(trf.Scale.Value.Width, trf.Scale.Value.Height));
                    }
                    else if (trf.Matrix != null)
                    {
                        currMatrix = Multiply(currMatrix, trf.Matrix);
                    }
                }
                else if (this.Actions[i] is StateAction)
                {
                    if (((StateAction)this.Actions[i]).StateActionType == StateAction.StateActionTypes.Save)
                    {
                        transformMatrix.Push(currMatrix);
                        clippingPaths.Push(currClippingPath);
                    }
                    else
                    {
                        currMatrix = transformMatrix.Pop();
                        currClippingPath = clippingPaths.Pop();
                    }
                }
            }

            return tbr;
        }

        /// <summary>
        /// A method that is used to rasterise a region of a <see cref="Graphics"/> object. Set this to <see langword="null"/> if you wish to use the default
        /// rasterisation methods (implemented by either VectSharp.Raster, or VectSharp.Raster.ImageSharp). You will have to provide your own implementation
        /// of this method if neither VectSharp.Raster nor VectSharp.Raster.ImageSharp are referenced by your project. The first argument of this method is
        /// the <see cref="Graphics"/> to be rasterised, the second is a <see cref="Rectangle"/> representing the region to rasterise, the third is a
        /// <see cref="double"/> representing the scale, and the third is a boolean value indicating whether the resulting <see cref="RasterImage"/> should
        /// be interpolated.
        /// </summary>
        public static Func<Graphics, Rectangle, double, bool, RasterImage> RasterisationMethod = null;

        /// <summary>
        /// Tries to rasterise specified region of this <see cref="Graphics"/> object using the default rasterisation method.
        /// </summary>
        /// <param name="region">The region of the <see cref="Graphics"/> to rasterise.</param>
        /// <param name="scale">The scale at which the image is rasterised.</param>
        /// <param name="interpolate">Determines whether the resulting <see cref="RasterImage"/> should be interpolated or not.</param>
        /// <param name="output">When this method returns, this will contain the rasterised image (or <see langword="null"/> if the image could not be rasterised.</param>
        /// <returns><see langword="true"/> if the image could be rasterised; <see langword="false"/> if it could not be rasterised.</returns>
        public bool TryRasterise(Rectangle region, double scale, bool interpolate, out RasterImage output)
        {
            if (RasterisationMethod != null)
            {
                output = RasterisationMethod(this, region, scale, interpolate);
                return true;
            }
            else
            {
                System.Reflection.Assembly raster;

                try
                {
                    raster = System.Reflection.Assembly.Load("VectSharp.Raster");
                    if (raster != null)
                    {
                        System.Reflection.MethodInfo rasteriser = raster.GetType("VectSharp.Raster.Raster").GetMethod("Rasterise", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        output = (RasterImage)rasteriser.Invoke(null, new object[] { this, region, scale, interpolate });
                        return true;
                    }
                }
                catch { }

                try
                {
                    raster = System.Reflection.Assembly.Load("VectSharp.Raster.ImageSharp");
                    if (raster != null)
                    {
                        System.Reflection.MethodInfo rasteriser = raster.GetType("VectSharp.Raster.ImageSharp.ImageSharpContextInterpreter").GetMethod("Rasterise", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        output = (RasterImage)rasteriser.Invoke(null, new object[] { this, region, scale, interpolate });
                        return true;
                    }
                }
                catch { }


                output = null;
                return false;
            }
        }

        /// <summary>
        /// Removes graphics actions that fall completely outside of the specified <paramref name="region"/>.
        /// </summary>
        /// <param name="region">The area to preserve.</param>
        public void Crop(Rectangle region)
        {
            List<int> itemsToRemove = new List<int>();

            Stack<Rectangle> clippingPaths = new Stack<Rectangle>();
            Rectangle currClippingPath = Rectangle.NaN;

            Stack<double[,]> transformMatrix = new Stack<double[,]>();
            double[,] currMatrix = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

            for (int i = 0; i < this.Actions.Count; i++)
            {
                if (this.Actions[i] is IPrintableAction act)
                {
                    Rectangle bounds = act.GetBounds();

                    double lineThickness = double.IsNaN(act.LineWidth) ? 0 : act.LineWidth;

                    Point pt1 = Multiply(currMatrix, new Point(bounds.Location.X - lineThickness * 0.5, bounds.Location.Y - lineThickness * 0.5));
                    Point pt2 = Multiply(currMatrix, new Point(bounds.Location.X + bounds.Size.Width + lineThickness * 0.5, bounds.Location.Y - lineThickness * 0.5));
                    Point pt3 = Multiply(currMatrix, new Point(bounds.Location.X + bounds.Size.Width + lineThickness * 0.5, bounds.Location.Y + bounds.Size.Height + lineThickness * 0.5));
                    Point pt4 = Multiply(currMatrix, new Point(bounds.Location.X - lineThickness * 0.5, bounds.Location.Y + bounds.Size.Height + lineThickness * 0.5));

                    bounds = Point.Bounds(pt1, pt2, pt3, pt4);

                    if (act is PathAction pth && pth.IsClipping)
                    {
                        currClippingPath = bounds;
                    }
                    else
                    {
                        if (!currClippingPath.IsNaN())
                        {
                            bounds = Rectangle.Intersection(bounds, currClippingPath);
                        }

                        if (double.IsNaN(bounds.Location.X) || double.IsNaN(bounds.Location.Y) || double.IsNaN(bounds.Size.Width) || double.IsNaN(bounds.Size.Height) || Rectangle.Intersection(bounds, region).IsNaN())
                        {
                            itemsToRemove.Add(i);
                        }
                    }
                }
                else if (this.Actions[i] is TransformAction)
                {
                    TransformAction trf = this.Actions[i] as TransformAction;

                    if (trf.Delta != null)
                    {
                        currMatrix = Multiply(currMatrix, TranslationMatrix(trf.Delta.Value.X, trf.Delta.Value.Y));
                    }
                    else if (trf.Angle != null)
                    {
                        currMatrix = Multiply(currMatrix, RotationMatrix(trf.Angle.Value));
                    }
                    else if (trf.Scale != null)
                    {
                        currMatrix = Multiply(currMatrix, ScaleMatrix(trf.Scale.Value.Width, trf.Scale.Value.Height));
                    }
                    else if (trf.Matrix != null)
                    {
                        currMatrix = Multiply(currMatrix, trf.Matrix);
                    }
                }
                else if (this.Actions[i] is StateAction)
                {
                    if (((StateAction)this.Actions[i]).StateActionType == StateAction.StateActionTypes.Save)
                    {
                        transformMatrix.Push(currMatrix);
                        clippingPaths.Push(currClippingPath);
                    }
                    else
                    {
                        currMatrix = transformMatrix.Pop();
                        currClippingPath = clippingPaths.Pop();
                    }
                }
            }

            for (int i = itemsToRemove.Count - 1; i >= 0; i--)
            {
                this.Actions.RemoveAt(itemsToRemove[i]);
            }
        }

        /// <summary>
        /// Removes graphics actions that fall completely outside of the specified region.
        /// </summary>
        /// <param name="topLeft">The top-left corner of the area to preserve.</param>
        /// <param name="size">The size of the area to preserve.</param>
        public void Crop(Point topLeft, Size size)
        {
            Rectangle region = new Rectangle(topLeft, size);
            this.Crop(region);
        }

        /// <summary>
        /// Gets all the tags that have been defined in the <see cref="Graphics"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/>s that, when enumerated, returns all the tags that have been defined in the <see cref="Graphics"/>.</returns>
        public IEnumerable<string> GetTags()
        {
            foreach (IGraphicsAction action in this.Actions)
            {
                if (!string.IsNullOrEmpty(action.Tag))
                {
                    yield return action.Tag;
                }
            }
        }
    }
}
