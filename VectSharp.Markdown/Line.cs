using System;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.Markdown
{
    internal abstract class LineFragment
    {
        public abstract void Translate(double deltaX, double deltaY);
        public string Tag { get; protected set; }
    }

    internal class TextFragment : LineFragment
    {
        public string Text { get; }
        public Point Position { get; private set; }
        public Font Font { get; }
        public Colour Colour { get; }

        public TextFragment(Point position, string text, Font font, Colour colour, string tag)
        {
            this.Position = position;
            this.Text = text;
            this.Font = font;
            this.Colour = colour;
            this.Tag = tag;
        }

        public override void Translate(double deltaX, double deltaY)
        {
            this.Position = new Point(this.Position.X + deltaX, this.Position.Y + deltaY);
        }
    }

    internal class UnderlineFragment : LineFragment
    {
        public Point Start { get; private set; }
        public Point End { get; private set; }
        public Colour Colour { get; }
        public double Thickness { get; }

        public UnderlineFragment(Point start, Point end, Colour colour, double thickness, string tag)
        {
            this.Start = start;
            this.End = end;
            this.Colour = colour;
            this.Thickness = thickness;
            this.Tag = tag;
        }

        public override void Translate(double deltaX, double deltaY)
        {
            this.Start = new Point(this.Start.X + deltaX, this.Start.Y + deltaY);
            this.End = new Point(this.End.X + deltaX, this.End.Y + deltaY);
        }
    }

    internal class RectangleFragment : LineFragment
    {
        public Point TopLeft { get; private set; }
        public Size Size { get; }
        public Colour Colour { get; }

        public RectangleFragment(Point topLeft, Size size, Colour colour, string tag)
        {
            this.TopLeft = topLeft;
            this.Size = size;
            this.Colour = colour;
            this.Tag = tag;
        }

        public override void Translate(double deltaX, double deltaY)
        {
            this.TopLeft = new Point(this.TopLeft.X + deltaX, this.TopLeft.Y + deltaY);
        }
    }

    internal class GraphicsFragment : LineFragment
    {
        public Point Origin { get; private set; }
        public Graphics Graphics { get; }
        public double Ascent { get; }

        public GraphicsFragment(Point origin, Graphics graphics, double ascent)
        {
            this.Origin = origin;
            this.Graphics = graphics;
            this.Ascent = ascent;
        }

        public override void Translate(double deltaX, double deltaY)
        {
            this.Origin = new Point(this.Origin.X + deltaX, this.Origin.Y + deltaY);
        }
    }

    internal class Line
    {
        public List<LineFragment> Fragments { get; }
        public double InitialAscent { get; }

        public Line(double initialAscent)
        {
            this.InitialAscent = initialAscent;
            this.Fragments = new List<LineFragment>();
        }

        public void Render(ref Graphics graphics, ref MarkdownContext context, MarkdownRenderer.NewPageAction newPageAction, double pageMaxY)
        {
            double deltaY = 0;
            double maxY = 0;

            foreach (LineFragment fragment in this.Fragments)
            {
                if (fragment is TextFragment text)
                {
                    deltaY = Math.Max(deltaY, text.Font.Ascent - this.InitialAscent);
                    maxY = Math.Max(maxY, text.Position.Y - text.Font.Descent);
                }
                else if (fragment is UnderlineFragment underline)
                {
                    maxY = Math.Max(maxY, Math.Max(underline.Start.Y, underline.End.Y));
                }
                else if (fragment is RectangleFragment rectangle)
                {
                    maxY = Math.Max(maxY, rectangle.TopLeft.Y + rectangle.Size.Height);
                }
                else if (fragment is GraphicsFragment graphicsFragment)
                {
                    deltaY = Math.Max(deltaY, graphicsFragment.Ascent - this.InitialAscent);
                    maxY = Math.Max(maxY, graphicsFragment.Origin.Y);
                }
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
                LineFragment fragment = this.Fragments[i];
                if (fragment is TextFragment text)
                {
                    Size size = text.Font.MeasureText(text.Text);
                    context.BottomRight = new Point(Math.Max(context.BottomRight.X, size.Width + text.Position.X + context.Translation.X), Math.Max(context.BottomRight.Y, text.Position.Y + size.Height + deltaY + context.Translation.Y));
                    graphics.FillText(text.Position.X, text.Position.Y + deltaY, text.Text, text.Font, text.Colour, TextBaselines.Baseline, tag: fragment.Tag);
                }
                else if (fragment is UnderlineFragment underline)
                {
                    graphics.StrokePath(new GraphicsPath().MoveTo(underline.Start.X, underline.Start.Y + deltaY).LineTo(underline.End.X, underline.End.Y + deltaY), underline.Colour, underline.Thickness, tag: fragment.Tag);
                }
                else if (fragment is RectangleFragment rectangle)
                {
                    graphics.FillRectangle(rectangle.TopLeft.X, rectangle.TopLeft.Y + deltaY, rectangle.Size.Width, rectangle.Size.Height, rectangle.Colour, tag: fragment.Tag);
                }
                else if (fragment is GraphicsFragment graphicsFragment)
                {
                    graphics.DrawGraphics(graphicsFragment.Origin.X, graphicsFragment.Origin.Y + deltaY, graphicsFragment.Graphics);
                }
            }
        }
    }
}
