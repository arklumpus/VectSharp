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

using System;

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
}
