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
        public Graphics Graphics { get; set; } = new Graphics();
        
        /// <summary>
        /// Create a new page.
        /// </summary>
        /// <param name="width">The width of the page.</param>
        /// <param name="height">The height of the page.</param>
        public Page(double width, double height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}
