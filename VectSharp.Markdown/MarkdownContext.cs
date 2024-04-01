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

namespace VectSharp.Markdown
{
    internal class MarkdownContext
    {
        public Font Font { get; set; }

        private Point cursor;
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

        public Colour Colour { get; set; }
        public bool Underline { get; set; }
        public bool StrikeThrough { get; set; }

        private Point translation;
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

        public Point MarginBottomRight { get; set; }

        public Line CurrentLine { get; set; }
        public int ListDepth { get; set; }
        public List<(double MaxX, double MinY, double MaxY)> ForbiddenAreasRight { get; set; }
        public List<(double MinX, double MinY, double MaxY)> ForbiddenAreasLeft { get; set; }
        public Page CurrentPage { get; set; }
        public Point BottomRight { get; set; }
        public string Tag { get; set; } = null;
        public Dictionary<string, string> LinkDestinations { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> InternalAnchors { get; set; } = new Dictionary<string, string>();

        public List<(int level, string heading, string tag)> Headings = new List<(int level, string heading, string tag)>();
        public MarkdownContext()
        {
            this.ForbiddenAreasRight = new List<(double MaxX, double MinY, double MaxY)>();
            this.ForbiddenAreasLeft = new List<(double MinX, double MinY, double MaxY)>();
        }

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

        public double GetMaxX(double y, double pageMaxX)
        {
            y = y + Translation.Y;

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

        public double GetMaxX(double y0, double y1, double pageMaxX)
        {
            y0 = y0 + Translation.Y;
            y1 = y1 + Translation.Y;

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

        public double GetMinX(double y)
        {
            y = y + Translation.Y;

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

        public double GetMinX(double y0, double y1)
        {

            y0 = y0 + Translation.Y;
            y1 = y1 + Translation.Y;

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
