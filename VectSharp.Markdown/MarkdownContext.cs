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
    /// Represents the current status of the Markdown rendering process.
    /// </summary>
    public class MarkdownContext
    {
        /// <summary>
        /// The current font.
        /// </summary>
        public Font Font { get; set; }

        private Point cursor;
        
        /// <summary>
        /// The current position of the cursor.
        /// </summary>
        public Point Cursor
        {
            get
            {
                return cursor;
            }

            set
            {
                cursor = value;

                double maxX = Math.Max(BottomRight.X, value.X + Translation.X);
                double maxY = Math.Max(BottomRight.Y, value.Y + Translation.Y);

                this.BottomRight = new Point(maxX, maxY);
            }
        }

        /// <summary>
        /// The current text colour.
        /// </summary>
        public Colour Colour { get; set; }
        
        /// <summary>
        /// Whether text is currently underlined.
        /// </summary>
        public bool Underline { get; set; }

        /// <summary>
        /// Whether text is currently struck out. 
        /// </summary>
        public bool StrikeThrough { get; set; }

        private Point translation;
        
        /// <summary>
        /// Translation for the current text block.
        /// </summary>
        public Point Translation
        {
            get
            {
                return translation;
            }

            set
            {
                translation = value;

                double maxX = Math.Max(BottomRight.X, cursor.X + value.X);
                double maxY = Math.Max(BottomRight.Y, cursor.Y + value.Y);

                this.BottomRight = new Point(maxX, maxY);
            }
        }

        /// <summary>
        /// The coordinates of the bottom right corner of the last rendered text element.
        /// </summary>
        public Point BottomRight { get; set; }

        /// <summary>
        /// The current margin on the bottom right.
        /// </summary>
        public Point MarginBottomRight { get; set; }
        
        /// <summary>
        /// The current list depth (for bullet lists and numbered lists).
        /// </summary>
        public int ListDepth { get; set; }

        /// <summary>
        /// A list of areas on which text cannot be drawn (because they contain a right-aligned image).
        /// Each element of this list specifies a maximum x value (first element) and a y range (second and third element).
        /// Within the specified y range, text cannot extend beyond the maximum x value.
        /// </summary>
        public List<(double MaxX, double MinY, double MaxY)> ForbiddenAreasRight { get; set; }

        /// <summary>
        /// A list of areas on which text cannot be drawn (because they contain a left-aligned image).
        /// Each element of this list specifies a minimum x value (first element) and a y range (second and third element).
        /// Within the specified y range, text starts at the minimum x value.
        /// </summary>
        public List<(double MinX, double MinY, double MaxY)> ForbiddenAreasLeft { get; set; }
        
        /// <summary>
        /// The <see cref="Page"/> on which the document is being rendered.
        /// </summary>
        public Page CurrentPage { get; set; }

        /// <summary>
        /// The current tag.
        /// </summary>
        public string Tag { get; set; } = null;

        /// <summary>
        /// A dictionary used to associate graphic action tags to hyperlinks.
        /// </summary>
        public Dictionary<string, string> LinkDestinations { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// A dictionary used to associate internal html anchors (e.g., "#something") to graphics action tags.
        /// </summary>
        public Dictionary<string, string> InternalAnchors { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// The list of headings defined in the document.
        /// </summary>
        public List<(int level, string heading, string tag)> Headings { get; private set; } = new List<(int level, string heading, string tag)>();

        /// <summary>
        /// The current line being drawn.
        /// </summary>
        public Line CurrentLine { get; set; }

        /// <summary>
        /// Create a new <see cref="MarkdownContext"/>.
        /// </summary>
        internal MarkdownContext()
        {
            this.ForbiddenAreasRight = new List<(double MaxX, double MinY, double MaxY)>();
            this.ForbiddenAreasLeft = new List<(double MinX, double MinY, double MaxY)>();
        }

        /// <summary>
        /// Create an independent copy of the current <see cref="MarkdownContext"/>.
        /// </summary>
        /// <returns>The copied context.</returns>
        public MarkdownContext Clone()
        {
            return new MarkdownContext()
            {
                Font = this.Font,
                Cursor = this.Cursor,
                Colour = this.Colour,
                Underline = this.Underline,
                StrikeThrough = this.StrikeThrough,
                Translation = this.Translation,
                CurrentLine = this.CurrentLine,
                ListDepth = this.ListDepth,
                ForbiddenAreasRight = this.ForbiddenAreasRight,
                ForbiddenAreasLeft = this.ForbiddenAreasLeft,
                CurrentPage = this.CurrentPage,
                Tag = this.Tag,
                LinkDestinations = this.LinkDestinations,
                InternalAnchors = this.InternalAnchors,
                MarginBottomRight = this.MarginBottomRight,
                Headings = this.Headings,
            };
        }

        /// <summary>
        /// Get the maximum x value for text drawn at the specified <paramref name="y"/> position.
        /// </summary>
        /// <param name="y">The vertical position at which text should be drawn.</param>
        /// <param name="pageMaxX">The overall maximum x value for the page, after taking margins into account.</param>
        /// <returns>The maximum x value for text drawn at the specified <paramref name="y"/> position.</returns>
        public double GetMaxX(double y, double pageMaxX)
        {
            y += Translation.Y;

            double maxX = pageMaxX;

            foreach ((double MaxX, double MinY, double MaxY) in ForbiddenAreasRight)
            {
                if (MinY <= y && MaxY >= y)
                {
                    maxX = Math.Min(maxX, MaxX - this.Translation.X);
                }
            }

            return maxX;
        }

        /// <summary>
        /// Get the maximum x value for text that will extend from <paramref name="y0"/> to <paramref name="y1"/>.
        /// </summary>
        /// <param name="y0">The vertical position of the start of the text block.</param>
        /// <param name="y1">The vertical position of the end of the text block.</param>
        /// <param name="pageMaxX">The overall maximum x value for the page, after taking margins into account.</param>
        /// <returns>The maximum x value for text that will extend from <paramref name="y0"/> to <paramref name="y1"/>.</returns>
        public double GetMaxX(double y0, double y1, double pageMaxX)
        {
            y0 += Translation.Y;
            y1 += Translation.Y;

            double maxX = pageMaxX;

            foreach ((double MaxX, double MinY, double MaxY) in ForbiddenAreasRight)
            {
                if (!((MinY < y0 && MaxY < y0) || (MinY > y1 && MaxY > y1)))
                {
                    maxX = Math.Min(maxX, MaxX - this.Translation.X);
                }
            }

            return maxX;
        }

        /// <summary>
        /// Get the minimum x value at which text can start at the specified <paramref name="y"/> position.
        /// </summary>
        /// <param name="y">The vertical position at which text should be drawn.</param>
        /// <returns>The minimum x value at which text can start at the specified <paramref name="y"/> position.</returns>
        public double GetMinX(double y)
        {
            y += Translation.Y;

            double minX = 0;

            foreach ((double MinX, double MinY, double MaxY) in ForbiddenAreasLeft)
            {
                if (MinY <= y && MaxY >= y)
                {
                    minX = Math.Max(minX, MinX - Translation.X);
                }
            }

            return minX;
        }

        /// <summary>
        /// Get the minimum x value for text that will extend from <paramref name="y0"/> to <paramref name="y1"/>.
        /// </summary>
        /// <param name="y0">The vertical position of the start of the text block.</param>
        /// <param name="y1">The vertical position of the end of the text block.</param>
        /// <returns>The minimum x value for text that will extend from <paramref name="y0"/> to <paramref name="y1"/>.</returns>
        public double GetMinX(double y0, double y1)
        {
            y0 += Translation.Y;
            y1 += Translation.Y;

            double minX = 0;

            foreach ((double MinX, double MinY, double MaxY) in ForbiddenAreasLeft)
            {
                if (!((MinY < y0 && MaxY < y0) || (MinY > y1 && MaxY > y1)))
                {
                    minX = Math.Max(minX, MinX - Translation.X);
                }
            }

            return minX;
        }
    }

    /// <summary>
    /// Represents the margins of a page.
    /// </summary>
    public class Margins
    {
        /// <summary>
        /// The left margin.
        /// </summary>
        public double Left { get; }

        /// <summary>
        /// The right margin.
        /// </summary>
        public double Right { get; }

        /// <summary>
        /// The top margin.
        /// </summary>
        public double Top { get; }

        /// <summary>
        /// The bottom margin.
        /// </summary>
        public double Bottom { get; }

        /// <summary>
        /// Creates a new <see cref="Margins"/> instance.
        /// </summary>
        /// <param name="left">The left margin.</param>
        /// <param name="top">The top margin.</param>
        /// <param name="right">The right margin.</param>
        /// <param name="bottom">The bottom margin.</param>
        public Margins(double left, double top, double right, double bottom)
        {
            this.Left = left;
            this.Right = right;
            this.Top = top;
            this.Bottom = bottom;
        }
    }
}
