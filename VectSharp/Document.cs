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
        /// Translate and resize the <see cref="Page"/> so that it displays the rectangle defined by <paramref name="topLeft"/> and <paramref name="size"/>.
        /// </summary>
        /// <param name="topLeft">The top left corner of the area to include in the page.</param>
        /// <param name="size">The size of the area to include in the page.</param>
        public void Crop(Point topLeft, Size size)
        {
            this.Graphics.Actions[0] = new TransformAction(new Point(-topLeft.X, -topLeft.Y));
            this.Width = size.Width;
            this.Height = size.Height;
        }
    }
}
