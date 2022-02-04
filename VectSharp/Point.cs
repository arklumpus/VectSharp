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
using System.Linq;

namespace VectSharp
{
    /// <summary>
    /// Represents a point relative to an origin in the top-left corner.
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// Horizontal (x) coordinate, measured to the right of the origin.
        /// </summary>
        public double X;

        /// <summary>
        /// Vertical (y) coordinate, measured to the bottom of the origin.
        /// </summary>
        public double Y;

        /// <summary>
        /// Create a new <see cref="Point"/>.
        /// </summary>
        /// <param name="x">The horizontal (x) coordinate.</param>
        /// <param name="y">The vertical (y) coordinate.</param>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Computes the modulus of the vector represented by the <see cref="Point"/>.
        /// </summary>
        /// <returns>The modulus of the vector represented by the <see cref="Point"/>.</returns>
        public double Modulus()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        /// <summary>
        /// Normalises a <see cref="Point"/>.
        /// </summary>
        /// <returns>The normalised <see cref="Point"/>.</returns>
        public Point Normalize()
        {
            double mod = Modulus();
            return new Point(X / mod, Y / mod);
        }

        /// <summary>
        /// Checks whether this <see cref="Point"/> is equal to another <see cref="Point"/>, up to a specified tolerance.
        /// </summary>
        /// <param name="p2">The <see cref="Point"/> to compare.</param>
        /// <param name="tolerance">The tolerance threshold.</param>
        /// <returns><see langword="true"/> if both coordinates of the <see cref="Point"/>s are closer than <paramref name="tolerance"/> or if their relative difference (i.e. <c>(a - b) / (a + b) * 2</c>) is smaller than <paramref name="tolerance"/>. <see langword="false"/> otherwise.</returns>
        public bool IsEqual(Point p2, double tolerance)
        {
            return (Math.Abs(p2.X - this.X) <= tolerance || Math.Abs((p2.X - this.X) / (p2.X + this.X)) <= tolerance * 0.5) && (Math.Abs(p2.Y - this.Y) <= tolerance || Math.Abs((p2.Y - this.Y) / (p2.Y + this.Y)) <= tolerance * 0.5);
        }

        /// <summary>
        /// Computes the top-left corner of the <see cref="Rectangle"/> identified by two <see cref="Point"/>s.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <returns>A <see cref="Point"/> whose <see cref="X"/> coordinate is the smallest between the one of <paramref name="p1"/> and <paramref name="p2"/>, and likewise for the <see cref="Y"/> coordinate.</returns>
        public static Point Min(Point p1, Point p2)
        {
            return new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
        }

        /// <summary>
        /// Computes the bottom-right corner of the <see cref="Rectangle"/> identified by two <see cref="Point"/>s.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <returns>A <see cref="Point"/> whose <see cref="X"/> coordinate is the largest between the one of <paramref name="p1"/> and <paramref name="p2"/>, and likewise for the <see cref="Y"/> coordinate.</returns>
        public static Point Max(Point p1, Point p2)
        {
            return new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
        }

        /// <summary>
        /// Computes the smallest <see cref="Rectangle"/> that contains all the specified points.
        /// </summary>
        /// <param name="points">The points whose bounds are being computed.</param>
        /// <returns>The smallest <see cref="Rectangle"/> that contains all the specified points.</returns>
        public static Rectangle Bounds(IEnumerable<Point> points)
        {
            bool initialised = false;
            Point min = new Point(double.NaN, double.NaN);
            Point max = new Point(double.NaN, double.NaN);

            foreach (Point pt in points)
            {
                if (!initialised)
                {
                    min = pt;
                    max = pt;
                    initialised = true;
                }
                else
                {
                    min = Point.Min(min, pt);
                    max = Point.Max(max, pt);
                }
            }

            return new Rectangle(min, max);
        }

        /// <summary>
        /// Computes the smallest <see cref="Rectangle"/> that contains all the specified points.
        /// </summary>
        /// <param name="points">The points whose bounds are being computed.</param>
        /// <returns>The smallest <see cref="Rectangle"/> that contains all the specified points.</returns>
        public static Rectangle Bounds(params Point[] points)
        {
            return Bounds((IEnumerable<Point>)points);
        }
    }

    /// <summary>
    /// Represents the size of an object.
    /// </summary>
    public struct Size
    {
        /// <summary>
        /// Width of the object.
        /// </summary>
        public double Width;

        /// <summary>
        /// Height of the object.
        /// </summary>
        public double Height;

        /// <summary>
        /// Create a new <see cref="Size"/>.
        /// </summary>
        /// <param name="width">The width of the object.</param>
        /// <param name="height">The height of the object.</param>
        public Size(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    /// Represents a rectangle.
    /// </summary>
    public struct Rectangle
    {
        /// <summary>
        /// A rectangle whose dimensions are all <see cref="double.NaN"/>.
        /// </summary>
        public static readonly Rectangle NaN = new Rectangle(double.NaN, double.NaN, double.NaN, double.NaN);

        /// <summary>
        /// The top-left corner of the rectangle.
        /// </summary>
        public Point Location;

        /// <summary>
        /// The size of the rectangle.
        /// </summary>
        public Size Size;

        /// <summary>
        /// Create a new <see cref="Rectangle"/> given its top-left corner and its size.
        /// </summary>
        /// <param name="location">The top-left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        public Rectangle(Point location, Size size)
        {
            this.Location = location;
            this.Size = size;
        }

        /// <summary>
        /// Create a new <see cref="Rectangle"/> given its top-left corner and its size.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the top-left corner of the rectangle.</param>
        /// <param name="y">The vertical coordinate of the top-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public Rectangle(double x, double y, double width, double height)
        {
            Location = new Point(x, y);
            Size = new Size(width, height);
        }

        /// <summary>
        /// Create a new <see cref="Rectangle"/> given its top-left corner and its bottom-right corner.
        /// </summary>
        /// <param name="topLeft">The top-left corner of the rectangle.</param>
        /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
        public Rectangle(Point topLeft, Point bottomRight)
        {
            this.Location = topLeft;
            this.Size = new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }

        /// <summary>
        /// Computes the rectangular bounds of the union of two <see cref="Rectangle"/>s.
        /// </summary>
        /// <param name="rectangle1">The first <see cref="Rectangle"/>.</param>
        /// <param name="rectangle2">The second <see cref="Rectangle"/>.</param>
        /// <returns>The smallest <see cref="Rectangle"/> containing both <paramref name="rectangle1"/> and <paramref name="rectangle2"/>.</returns>
        public static Rectangle Union(Rectangle rectangle1, Rectangle rectangle2)
        {
            double minX = rectangle1.Location.X;
            double minY = rectangle1.Location.Y;
            double maxX = rectangle1.Location.X;
            double maxY = rectangle1.Location.Y;

            minX = Math.Min(minX, rectangle1.Location.X + rectangle1.Size.Width);
            minX = Math.Min(minX, rectangle2.Location.X);
            minX = Math.Min(minX, rectangle2.Location.X + rectangle2.Size.Width);

            minY = Math.Min(minY, rectangle1.Location.Y + rectangle1.Size.Height);
            minY = Math.Min(minY, rectangle2.Location.Y);
            minY = Math.Min(minY, rectangle2.Location.Y + rectangle2.Size.Height);


            maxX = Math.Max(maxX, rectangle1.Location.X + rectangle1.Size.Width);
            maxX = Math.Max(maxX, rectangle2.Location.X);
            maxX = Math.Max(maxX, rectangle2.Location.X + rectangle2.Size.Width);

            maxY = Math.Max(maxY, rectangle1.Location.Y + rectangle1.Size.Height);
            maxY = Math.Max(maxY, rectangle2.Location.Y);
            maxY = Math.Max(maxY, rectangle2.Location.Y + rectangle2.Size.Height);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Computes the rectangular bounds of the union of multiple <see cref="Rectangle"/>s.
        /// </summary>
        /// <param name="rectangles">The <see cref="Rectangle"/>s whose union will be computed.</param>
        /// <returns>The smallest <see cref="Rectangle"/> containing all the <paramref name="rectangles"/>.</returns>
        public static Rectangle Union(IEnumerable<Rectangle> rectangles)
        {
            if (rectangles.Any())
            {
                bool initialised = false;

                Rectangle tbr = new Rectangle();

                foreach (Rectangle rect in rectangles)
                {
                    if (!initialised)
                    {
                        tbr = rect;
                    }
                    else
                    {
                        tbr = Union(rect, tbr);
                    }
                }

                return tbr;
            }
            else
            {
                return Rectangle.NaN;
            }
        }

        /// <summary>
        /// Computes the rectangular bounds of the union of multiple <see cref="Rectangle"/>s.
        /// </summary>
        /// <param name="rectangles">The <see cref="Rectangle"/>s whose union will be computed.</param>
        /// <returns>The smallest <see cref="Rectangle"/> containing all the <paramref name="rectangles"/>.</returns>
        public static Rectangle Union(params Rectangle[] rectangles)
        {
            return Union((IEnumerable<Rectangle>)rectangles);
        }
    }
}
