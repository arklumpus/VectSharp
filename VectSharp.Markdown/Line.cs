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

namespace VectSharp.Markdown
{
    /// <summary>
    /// Represents one of the fragments that make up a <see cref="Line"/>.
    /// </summary>
    public abstract class LineFragment
    {
        /// <summary>
        /// Applies the specified translation to the line fragment.
        /// </summary>
        /// <param name="deltaX">The translation in the horizontal direction.</param>
        /// <param name="deltaY">The translation in the vertical direction.</param>
        public abstract void Translate(double deltaX, double deltaY);
        
        /// <summary>
        /// Graphics action tag for the fragment.
        /// </summary>
        public string Tag { get; protected set; }

        /// <summary>
        /// Get the ascent of the <see cref="LineFragment"/>.
        /// </summary>
        /// <param name="lineAscent">The ascent of the <see cref="Line"/> on which the fragment should be drawn.</param>
        /// <returns>The ascent of the <see cref="LineFragment"/>.</returns>
        public abstract double GetAscent(double lineAscent);

        /// <summary>
        /// Get the vertical coordinates of the bottom of the <see cref="LineFragment"/>.
        /// </summary>
        /// <returns></returns>
        public abstract double GetMaxY();

        /// <summary>
        /// Render the <see cref="LineFragment"/> on the specified <paramref name="graphics"/> surface.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> surface on which the <see cref="LineFragment"/> will be drawn.</param>
        /// <param name="deltaY">Additional translation on the vertical axis.</param>
        /// <returns>If a text element has been drawn, this method should return the bottom-right corner of the drawn text element, otherwise <see langword="null"/>. This is used to update the <see cref="MarkdownContext.BottomRight"/> property.</returns>
        public abstract Point? Render(Graphics graphics, double deltaY);
    }

    /// <summary>
    /// A fragment containing text.
    /// </summary>
    public class TextFragment : LineFragment
    {
        /// <summary>
        /// The text contained in the fragment.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The position of the text, relative to the cursor at the start of the line.
        /// </summary>
        public Point Position { get; private set; }
        
        /// <summary>
        /// The font used to draw the text.
        /// </summary>
        public Font Font { get; }

        /// <summary>
        /// The colour used to draw the text.
        /// </summary>
        public Colour Colour { get; }

        /// <summary>
        /// Create a new <see cref="TextFragment"/>.
        /// </summary>
        /// <param name="position">The position of the text.</param>
        /// <param name="text">The text contained in the fragment.</param>
        /// <param name="font">The font used to draw the text.</param>
        /// <param name="colour">The colour used to draw the text.</param>
        /// <param name="tag">Graphics action tag for the text.</param>
        public TextFragment(Point position, string text, Font font, Colour colour, string tag)
        {
            this.Position = position;
            this.Text = text;
            this.Font = font;
            this.Colour = colour;
            this.Tag = tag;
        }

        /// <inheritdoc/>
        public override void Translate(double deltaX, double deltaY)
        {
            this.Position = new Point(this.Position.X + deltaX, this.Position.Y + deltaY);
        }

        /// <inheritdoc/>
        public override double GetAscent(double lineAscent)
        {
            return this.Font.Ascent;
        }

        /// <inheritdoc/>
        public override double GetMaxY()
        {
            return this.Position.Y - this.Font.Descent;
        }

        /// <inheritdoc/>
        public override Point? Render(Graphics graphics, double deltaY)
        {
            Size size = this.Font.MeasureText(this.Text);
            graphics.FillText(this.Position.X, this.Position.Y + deltaY, this.Text, this.Font, this.Colour, TextBaselines.Baseline, tag: this.Tag);
            return new Point(size.Width + this.Position.X, this.Position.Y + size.Height + deltaY);
        }
    }

    /// <summary>
    /// Represents a text underline.
    /// </summary>
    public class UnderlineFragment : LineFragment
    {
        /// <summary>
        /// The start position of the underline, relative to the cursor at the start of the line.
        /// </summary>
        public Point Start { get; private set; }

        /// <summary>
        /// The end position of the underline, relative to the cursor at the start of the line.
        /// </summary>
        public Point End { get; private set; }

        /// <summary>
        /// The colour of the underline.
        /// </summary>
        public Colour Colour { get; }
        
        /// <summary>
        /// The thickness of the underline.
        /// </summary>
        public double Thickness { get; }

        /// <summary>
        /// Create a new <see cref="UnderlineFragment"/>.
        /// </summary>
        /// <param name="start">The start position of the underline.</param>
        /// <param name="end">The end position of the underline.</param>
        /// <param name="colour">The colour of the underline.</param>
        /// <param name="thickness">The thickness of the underline.</param>
        /// <param name="tag">Graphics action tag for the underline.</param>
        public UnderlineFragment(Point start, Point end, Colour colour, double thickness, string tag)
        {
            this.Start = start;
            this.End = end;
            this.Colour = colour;
            this.Thickness = thickness;
            this.Tag = tag;
        }

        /// <inheritdoc/>
        public override void Translate(double deltaX, double deltaY)
        {
            this.Start = new Point(this.Start.X + deltaX, this.Start.Y + deltaY);
            this.End = new Point(this.End.X + deltaX, this.End.Y + deltaY);
        }

        /// <inheritdoc/>
        public override double GetAscent(double lineAscent)
        {
            return lineAscent;
        }

        /// <inheritdoc/>
        public override double GetMaxY()
        {
            return Math.Max(this.Start.Y, this.End.Y);
        }

        /// <inheritdoc/>
        public override Point? Render(Graphics graphics, double deltaY)
        {
            graphics.StrokePath(new GraphicsPath().MoveTo(this.Start.X, this.Start.Y + deltaY).LineTo(this.End.X, this.End.Y + deltaY), this.Colour, this.Thickness, tag: this.Tag);
            return null;
        }
    }

    /// <summary>
    /// Represents a rectangle.
    /// </summary>
    public class RectangleFragment : LineFragment
    {
        /// <summary>
        /// The top-left corner of the rectangle, relative to the cursor at the start of the line.
        /// </summary>
        public Point TopLeft { get; private set; }

        /// <summary>
        /// The size of the rectangle.
        /// </summary>
        public Size Size { get; }

        /// <summary>
        /// The colour used to draw the rectangle.
        /// </summary>
        public Colour Colour { get; }

        /// <summary>
        /// Create a new <see cref="RectangleFragment"/>.
        /// </summary>
        /// <param name="topLeft">The top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="colour">The colour used to draw the rectangle.</param>
        /// <param name="tag">Graphics action tag for the rectangle.</param>
        public RectangleFragment(Point topLeft, Size size, Colour colour, string tag)
        {
            this.TopLeft = topLeft;
            this.Size = size;
            this.Colour = colour;
            this.Tag = tag;
        }

        /// <inheritdoc/>
        public override void Translate(double deltaX, double deltaY)
        {
            this.TopLeft = new Point(this.TopLeft.X + deltaX, this.TopLeft.Y + deltaY);
        }

        /// <inheritdoc/>
        public override double GetAscent(double lineAscent)
        {
            return lineAscent;
        }

        /// <inheritdoc/>
        public override double GetMaxY()
        {
            return this.TopLeft.Y + this.Size.Height;
        }

        /// <inheritdoc/>
        public override Point? Render(Graphics graphics, double deltaY)
        {
            graphics.FillRectangle(this.TopLeft.X, this.TopLeft.Y + deltaY, this.Size.Width, this.Size.Height, this.Colour, tag: this.Tag);
            return null;
        }
    }

    /// <summary>
    /// Represents a <see cref="LineFragment"/> that draws something.
    /// </summary>
    internal class GraphicsFragment : LineFragment
    {
        /// <summary>
        /// The origin of the graphics fragment being drawn, relative to the cursor at the start of the line.
        /// </summary>
        public Point Origin { get; private set; }

        /// <summary>
        /// The <see cref="Graphics"/> to draw.
        /// </summary>
        public Graphics Graphics { get; }

        /// <summary>
        /// The ascent for the graphics.
        /// </summary>
        public double Ascent { get; }

        /// <summary>
        /// Create a new <see cref="GraphicsFragment"/>.
        /// </summary>
        /// <param name="origin">The origin of the graphics fragment being drawn.</param>
        /// <param name="graphics">The <see cref="Graphics"/> to draw.</param>
        /// <param name="ascent">The ascent for the graphics.</param>
        public GraphicsFragment(Point origin, Graphics graphics, double ascent)
        {
            this.Origin = origin;
            this.Graphics = graphics;
            this.Ascent = ascent;
        }

        /// <inheritdoc/>
        public override void Translate(double deltaX, double deltaY)
        {
            this.Origin = new Point(this.Origin.X + deltaX, this.Origin.Y + deltaY);
        }

        /// <inheritdoc/>
        public override double GetAscent(double lineAscent)
        {
            return this.Ascent;
        }

        /// <inheritdoc/>
        public override double GetMaxY()
        {
            return this.Origin.Y;
        }

        /// <inheritdoc/>
        public override Point? Render(Graphics graphics, double deltaY)
        {
            graphics.DrawGraphics(this.Origin.X, this.Origin.Y + deltaY, this.Graphics);
            return null;
        }
    }

    /// <summary>
    /// Represents a text line.
    /// </summary>
    public class Line
    {
        /// <summary>
        /// The fragments contained in the line.
        /// </summary>
        public List<LineFragment> Fragments { get; }
        
        /// <summary>
        /// The initial ascent for text on the <see cref="Line"/>.
        /// </summary>
        public double InitialAscent { get; }

        /// <summary>
        /// Create a new <see cref="Line"/>.
        /// </summary>
        /// <param name="initialAscent">The initial ascent for text on the <see cref="Line"/>.</param>
        public Line(double initialAscent)
        {
            this.InitialAscent = initialAscent;
            this.Fragments = new List<LineFragment>();
        }

        internal void Render(ref Graphics graphics, ref MarkdownContext context, MarkdownRenderer.NewPageAction newPageAction, double pageMaxY)
        {
            double deltaY = 0;
            double maxY = 0;

            foreach (LineFragment fragment in this.Fragments)
            {
                deltaY = Math.Max(deltaY, fragment.GetAscent(this.InitialAscent) - InitialAscent);
                maxY = Math.Max(maxY, fragment.GetMaxY());
            }

            maxY += deltaY;
            
            if (maxY > pageMaxY)
            {
                double currCursY = context.Cursor.Y;

                newPageAction(ref context, ref graphics);

                double currDelta = deltaY;

                context.Cursor = new Point(context.Cursor.X, context.Cursor.Y + this.InitialAscent + currDelta);

                deltaY -= currCursY - context.Cursor.Y + currDelta;

                context.Cursor = new Point(context.Cursor.X, context.Cursor.Y);
            }
            else
            {
                context.Cursor = new Point(context.Cursor.X, context.Cursor.Y + deltaY);
            }
            

            for (int i = 0; i < this.Fragments.Count; i++)
            {
                Point? pt = this.Fragments[i].Render(graphics, deltaY);

                if (pt != null)
                {
                    context.BottomRight = new Point(Math.Max(context.BottomRight.X, pt.Value.X + context.Translation.X), Math.Max(context.BottomRight.Y, pt.Value.Y + context.Translation.Y));
                }
            }
        }
    }
}
