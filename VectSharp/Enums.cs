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
using System.Diagnostics;
using System.Linq;

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
    /// Contains information about text spacing.
    /// </summary>
    public struct TextSpacing : IEquatable<TextSpacing>
    {
        /// <summary>
        /// Scaling factor for whitespace characters.
        /// </summary>
        public double WhitespaceScale { get; }

        /// <summary>
        /// Increment for whitespace characters.
        /// </summary>
        public double WhitespaceAdd { get; }


        /// <summary>
        /// Scaling factor for non-whitespace characters.
        /// </summary>
        public double NonWhitespaceScale { get; }

        /// <summary>
        /// Increment for non-whitespace characters.
        /// </summary>
        public double NonWhitespaceAdd { get; }

        /// <summary>
        /// Create a new <see cref="TextSpacing"/> with the specified spacing parameters.
        /// </summary>
        /// <param name="nonWhitespaceScale">Scaling factor for non-whitespace characters.</param>
        /// <param name="nonWhitespaceAdd">Increment for non-whitespace characters.</param>
        /// <param name="whitespaceScale">Scaling factor for whitespace characters.</param>
        /// <param name="whitespaceAdd">Increment for whitespace characters.</param>
        public TextSpacing(double nonWhitespaceScale, double nonWhitespaceAdd, double whitespaceScale, double whitespaceAdd)
        {
            this.WhitespaceScale = whitespaceScale;
            this.WhitespaceAdd = whitespaceAdd;
            this.NonWhitespaceScale = nonWhitespaceScale;
            this.NonWhitespaceAdd = nonWhitespaceAdd;
        }

        /// <summary>
        /// Create a new <see cref="TextSpacing"/> with the specified spacing parameters.
        /// </summary>
        /// <param name="scale">Scaling factor.</param>
        /// <param name="add">Increment.</param>
        public TextSpacing(double scale, double add)
        {
            this.WhitespaceScale = scale;
            this.WhitespaceAdd = add;
            this.NonWhitespaceScale = scale;
            this.NonWhitespaceAdd = add;
        }

        /// <summary>
        /// Default text spacing.
        /// </summary>
        public static TextSpacing Default { get; } = new TextSpacing(1, 0);

        /// <inheritdoc/>
        public bool Equals(TextSpacing other)
        {
            return this.WhitespaceScale == other.WhitespaceScale && this.WhitespaceAdd == other.WhitespaceAdd && this.NonWhitespaceScale == other.NonWhitespaceScale && this.NonWhitespaceAdd == other.NonWhitespaceAdd;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is TextSpacing other)
            {
                return Equals(other);
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + WhitespaceScale.GetHashCode();
                hash = hash * 31 + WhitespaceAdd.GetHashCode();
                hash = hash * 31 + NonWhitespaceAdd.GetHashCode();
                hash = hash * 31 + NonWhitespaceAdd.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Compares two <see cref="TextSpacing"/> objects.
        /// </summary>
        /// <param name="left">The first <see cref="TextSpacing"/> object.</param>
        /// <param name="right">The second <see cref="TextSpacing"/> object.</param>
        /// <returns><see langword="true"/> if the two <see cref="TextSpacing"/> objects specify the same spacing parameters, <see langword="false"/> otherwise.</returns>
        public static bool operator ==(TextSpacing left, TextSpacing right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Compares two <see cref="TextSpacing"/> objects.
        /// </summary>
        /// <param name="left">The first <see cref="TextSpacing"/> object.</param>
        /// <param name="right">The second <see cref="TextSpacing"/> object.</param>
        /// <returns><see langword="false"/> if the two <see cref="TextSpacing"/> objects specify the same spacing parameters, <see langword="true"/> otherwise.</returns>
        public static bool operator !=(TextSpacing left, TextSpacing right)
        {
            return !Equals(left, right);
        }
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
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct LineDash
    {
        /// <summary>
        /// A solid (not dashed) line
        /// </summary>
        public static LineDash SolidLine = new LineDash(0, 0, 0);

        /// <summary>
        /// Length of the "on" (painted) segment.
        /// </summary>
        [Obsolete("Please use the LineDash.DashArray instead.")]
        public double UnitsOn;

        /// <summary>
        /// Length of the "off" (not painted) segment.
        /// </summary>
        [Obsolete("Please use the LineDash.DashArray instead.")]
        public double UnitsOff;

        /// <summary>
        /// An array specifying lenghts of alternating dashes and gaps.
        /// </summary>
        public double[] DashArray;

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
            this.DashArray = new double[] { unitsOn, unitsOff };
            Phase = phase;

            /* Deprecated */
#pragma warning disable 618
            this.UnitsOn = unitsOn;
            this.UnitsOff = unitsOff;
#pragma warning restore 618
        }

        /// <summary>
        /// Define a new line dash pattern.
        /// </summary>
        /// <param name="units">The length of the dash segments and gaps.</param>
        /// <param name="phase">The position in the dash pattern at which the line starts.</param>
        public LineDash(double units, double phase = 0)
        {
            this.DashArray = new double[] { units };
            Phase = phase;

            /* Deprecated */
#pragma warning disable 618
            this.UnitsOn = units;
            this.UnitsOff = units;
#pragma warning restore 618
        }

        /// <summary>
        /// Define a new line dash pattern.
        /// </summary>
        /// <param name="dashArray">An array specifying lenghts of alternating dashes and gaps.</param>
        /// <param name="phase">The position in the dash pattern at which the line starts.</param>
        public LineDash(double[] dashArray, double phase = 0)
        {
            this.DashArray = dashArray;
            this.Phase = phase;

            /* Deprecated */
#pragma warning disable 618
            this.UnitsOn = dashArray != null && dashArray.Length > 0 ? dashArray[0] : 0;
            this.UnitsOff = dashArray != null && dashArray.Length > 0 ? dashArray[System.Math.Min(dashArray.Length - 1, 1)] : 0;
#pragma warning restore 618
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                if (this.Phase == 0 && this.DashArray.All(x => x == 0))
                {
                    return "{ Solid line }";
                }
                else
                {
                    return string.Format("{{ Phase: {0}, DashArray: {1} }}", this.Phase, "[ " + this.DashArray.Take(Math.Min(4, this.DashArray.Length)).Select(x => x.ToString()).Aggregate((a, b) => a + ", " + b) + (this.DashArray.Length > 4 ? " ..." : "") + " ]");
                }
            }
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
