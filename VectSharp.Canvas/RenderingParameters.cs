/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2021  Giorgio Bianchini
 
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

namespace VectSharp.Canvas
{
    internal class RenderingParameters : IEquatable<RenderingParameters>
    {
        public static float Tolerance = 1e-5f;
        public float Left { get; }
        public float Top { get; }
        public float Width { get; }
        public float Height { get; }
        public float Scale { get; }
        public int RenderWidth { get; }
        public int RenderHeight { get; }

        public RenderingParameters(float left, float top, float width, float height, float scale, int renderWidth, int renderHeight)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
            this.Scale = scale;
            this.RenderWidth = renderWidth;
            this.RenderHeight = renderHeight;
        }

        public RenderingParameters Clone()
        {
            return new RenderingParameters(this.Left, this.Top, this.Width, this.Height, this.Scale, this.RenderWidth, this.RenderHeight);
        }

        public bool Equals(RenderingParameters other)
        {
            if (!object.ReferenceEquals(other, null))
            {
                return this.Scale == other.Scale && this.Left == other.Left && this.Top == other.Top && this.Width == other.Width && this.Height == other.Height && this.RenderWidth == other.RenderWidth && this.RenderHeight == other.RenderHeight;
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is RenderingParameters other)
            {
                return this.Equals(other);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hash = 13;

            unchecked
            {
                hash = (hash * 7) + this.Left.GetHashCode();
                hash = (hash * 7) + this.Top.GetHashCode();
                hash = (hash * 7) + this.Width.GetHashCode();
                hash = (hash * 7) + this.Height.GetHashCode();
                hash = (hash * 7) + this.Scale.GetHashCode();
                hash = (hash * 7) + this.RenderWidth.GetHashCode();
                hash = (hash * 7) + this.RenderHeight.GetHashCode();
            }

            return hash;
        }

        public static bool operator ==(RenderingParameters param1, RenderingParameters param2)
        {
            if (object.ReferenceEquals(param1, null))
            {
                return object.ReferenceEquals(param2, null);
            }
            else
            {
                return param1.Equals(param2);
            }
        }

        public static bool operator !=(RenderingParameters param1, RenderingParameters param2)
        {
            if (!object.ReferenceEquals(param1, null) && !object.ReferenceEquals(param2, null))
            {
                return param1.Scale != param2.Scale || param1.Left != param2.Left || param1.Top != param2.Top || param1.Width != param2.Width || param1.Height != param2.Height || param1.RenderWidth != param2.RenderWidth || param1.RenderHeight != param2.RenderHeight;
            }
            else
            {
                return !(object.ReferenceEquals(param1, null) && object.ReferenceEquals(param2, null));
            }
        }

        public bool GoodEnough(RenderingParameters other)
        {
            if (this.Scale == other.Scale)
            {
                return (this.Left <= other.Left && (this.Left + this.Width - other.Left - other.Width) / (this.Left + this.Width + other.Left + other.Width) >= -Tolerance && this.Top <= other.Top && (this.Top + this.Height - other.Top - other.Height) / (this.Top + this.Height + other.Top + other.Height) >= -Tolerance);
            }
            else
            {
                return false;
            }
        }
    }
}
