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
using System.Text;

namespace VectSharp
{
    /// <summary>
    /// Represents a collection of pages.
    /// </summary>
    public class Document
    {
        /// <summary>
        /// The pages in the document.
        /// </summary>
        public List<Page> Pages = new List<Page>();


        /// <summary>
        /// Create a new document.
        /// </summary>
        public Document()
        {

        }
    }

    /// <summary>
    /// Represents a <see cref="Graphics"/> object with a width and height.
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Width of the page.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Height of the page.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Graphics surface of the page.
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <summary>
        /// Background colour of the page.
        /// </summary>
        public Colour Background { get; set; } = Colour.FromRgba(255, 255, 255, 0);

        /// <summary>
        /// Create a new page.
        /// </summary>
        /// <param name="width">The width of the page.</param>
        /// <param name="height">The height of the page.</param>
        public Page(double width, double height)
        {
            this.Width = width;
            this.Height = height;

            this.Graphics = new Graphics();
            this.Graphics.Translate(0, 0);
        }

        /// <summary>
        /// Translate and resize the <see cref="Page"/> so that it displays the specified <paramref name="region"/>.
        /// </summary>
        /// <param name="region">The area to include in the page.</param>
        /// <param name="removeClippedGraphics">If this is <see langword="true"/>, graphics actions that fall outside of the specified region are completely removed from the plot, otherwise they are just hidden.</param>
        /// <param name="tag">A tag to identify the transform.</param>
        public void Crop(Rectangle region, bool removeClippedGraphics = false, string tag = null)
        {
            this.Crop(region.Location, region.Size, removeClippedGraphics, tag);
        }

        /// <summary>
        /// Translate and resize the <see cref="Page"/> so that it displays the rectangle defined by <paramref name="topLeft"/> and <paramref name="size"/>.
        /// </summary>
        /// <param name="topLeft">The top left corner of the area to include in the page.</param>
        /// <param name="size">The size of the area to include in the page.</param>
        /// <param name="removeClippedGraphics">If this is <see langword="true"/>, graphics actions that fall outside of the specified region are completely removed from the plot, otherwise they are just hidden.</param>
        /// <param name="tag">A tag to identify the transform.</param>
        public void Crop(Point topLeft, Size size, bool removeClippedGraphics = false, string tag = null)
        {
            if (removeClippedGraphics)
            {
                this.Graphics.Crop(new Rectangle(topLeft, size));
            }

            if (this.Graphics.Actions[0] is TransformAction transf)
            {
                double[,] currMatrix = transf.GetMatrix();

                double[,] newMatrix = Graphics.Multiply(Graphics.TranslationMatrix(-topLeft.X, -topLeft.Y), currMatrix);

                this.Graphics.Actions[0] = new TransformAction(newMatrix, tag);
            }
            else
            {
                this.Graphics.Actions.Insert(0, new TransformAction(new Point(-topLeft.X, -topLeft.Y), tag));
            }

            this.Width = size.Width;
            this.Height = size.Height;
        }

        /// <summary>
        /// Translate and resize the <see cref="Page"/> so that it displays the rectangle corresponding to the bounding box of its contents.
        /// </summary>
        /// <param name="tag">A tag to identify the transform.</param>
        public void Crop(string tag = null)
        {
            Rectangle bounds = this.Graphics.GetBounds();

            if (this.Graphics.Actions[0] is TransformAction transf)
            {
                double[,] currMatrix = transf.GetMatrix();

                double[,] newMatrix = Graphics.Multiply(Graphics.TranslationMatrix(-bounds.Location.X, -bounds.Location.Y), currMatrix);

                this.Graphics.Actions[0] = new TransformAction(newMatrix, tag);
            }
            else
            {
                this.Graphics.Actions.Insert(0, new TransformAction(new Point(-bounds.Location.X, -bounds.Location.Y), tag));
            }

            this.Width = bounds.Size.Width;
            this.Height = bounds.Size.Height;
        }
    }
}
