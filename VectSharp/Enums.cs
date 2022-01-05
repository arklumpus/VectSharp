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

namespace VectSharp
{
    /// <summary>
    /// Represent text baselines.
    /// </summary>
    public enum TextBaselines
    {
        /// <summary>
        /// The current vertical coordinate determines where the top of the text string will be placed.
        /// </summary>
        Top,

        /// <summary>
        /// The current vertical coordinate determines where the bottom of the text string will be placed.
        /// </summary>
        Bottom,

        /// <summary>
        /// The current vertical coordinate determines where the middle of the text string will be placed.
        /// </summary>
        Middle,

        /// <summary>
        /// The current vertical coordinate determines where the baseline of the text string will be placed.
        /// </summary>
        Baseline
    }

    /// <summary>
    /// Represents text anchors.
    /// </summary>
    public enum TextAnchors
    {
        /// <summary>
        /// The current coordinate will determine the position of the left side of the text string.
        /// </summary>
        Left,

        /// <summary>
        /// The current coordinate will determine the position of the center of the text string.
        /// </summary>
        Center,

        /// <summary>
        /// The current coordinate will determine the position of the right side of the text string.
        /// </summary>
        Right
    }

    /// <summary>
    /// Represents line caps.
    /// </summary>
    public enum LineCaps
    {
        /// <summary>
        /// The ends of the line are squared off at the endpoints.
        /// </summary>
        Butt = 0,

        /// <summary>
        /// The ends of the lines are rounded.
        /// </summary>
        Round = 1,

        /// <summary>
        /// The ends of the lines are squared off by adding an half square box at each end.
        /// </summary>
        Square = 2
    }

    /// <summary>
    /// Represents line joining options.
    /// </summary>
    public enum LineJoins
    {
        /// <summary>
        /// Consecutive segments are joined by straight corners.
        /// </summary>
        Bevel = 2,

        /// <summary>
        /// Consecutive segments are joined by extending their outside edges until they meet.
        /// </summary>
        Miter = 0,

        /// <summary>
        /// Consecutive segments are joined by arc segments.
        /// </summary>
        Round = 1
    }

    /// <summary>
    /// Represents instructions on how to paint a dashed line.
    /// </summary>
    public struct LineDash
    {
        /// <summary>
        /// A solid (not dashed) line
        /// </summary>
        public static LineDash SolidLine = new LineDash(0, 0, 0);

        /// <summary>
        /// Length of the "on" (painted) segment.
        /// </summary>
        public double UnitsOn;

        /// <summary>
        /// Length of the "off" (not painted) segment.
        /// </summary>
        public double UnitsOff;

        /// <summary>
        /// Position in the dash pattern at which the line starts.
        /// </summary>
        public double Phase;

        /// <summary>
        /// Define a new line dash pattern.
        /// </summary>
        /// <param name="unitsOn">The length of the "on" (painted) segment.</param>
        /// <param name="unitsOff">The length of the "off" (not painted) segment.</param>
        /// <param name="phase">The position in the dash pattern at which the line starts.</param>
        public LineDash(double unitsOn, double unitsOff, double phase)
        {
            UnitsOn = unitsOn;
            UnitsOff = unitsOff;
            Phase = phase;
        }
    }

    /// <summary>
    /// Types of <see cref="Segment"/>.
    /// </summary>
    public enum SegmentType
    {
        /// <summary>
        /// The segment represents a move from the current point to a new point.
        /// </summary>
        Move,

        /// <summary>
        /// The segment represents a straight line from the current point to a new point.
        /// </summary>
        Line,

        /// <summary>
        /// The segment represents a cubic bezier curve from the current point to a new point.
        /// </summary>
        CubicBezier,

        /// <summary>
        /// The segment represents a circular arc from the current point to a new point.
        /// </summary>
        Arc,

        /// <summary>
        /// The segment represents the closing segment of a figure.
        /// </summary>
        Close
    }

    /// <summary>
    /// Represents ways to deal with unbalanced graphics state stacks.
    /// </summary>
    public enum UnbalancedStackActions
    {
        /// <summary>
        /// If the graphics state stack is unbalanced, an exception will be thrown.
        /// </summary>
        Throw,

        /// <summary>
        /// The graphics state stack will be automatically balanced by adding or removing calls to <see cref="Graphics.Restore"/> as necessary.
        /// </summary>
        SilentlyFix,

        /// <summary>
        /// No attempt will be made at correcting an unbalanced graphics state stack. This may cause issues with some consumers.
        /// </summary>
        Ignore
    }
}
