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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Represents a generic 3D element.
    /// </summary>
    public abstract class Element3D : IReadOnlyList<Point3D>
    {
        /// <summary>
        /// A tag that can be used to identify the element.
        /// </summary>
        public virtual string Tag { get; set; }

        /// <summary>
        /// The z-index of the object. Objects with a higher value appear in front of objects with a lower value, regardless of the camera positioning.
        /// </summary>
        public virtual int ZIndex { get; set; } = 0;

        /// <summary>
        /// The number of <see cref="Point3D"/>s in the element (1, 2 or 3).
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Returns a point from the element.
        /// </summary>
        /// <param name="index">The index of the element to obtain.</param>
        /// <returns>A point from the element.</returns>
        public abstract Point3D this[int index] { get; }

        /// <summary>
        /// Stores the projected 2D coordinates of this object when viewed using the specified <paramref name="camera"/>.
        /// </summary>
        /// <param name="camera">The camera that should be used to project the coordinates.</param>
        public abstract void SetProjection(Camera camera);

        /// <summary>
        /// Returns the stored 2D projection of this object.
        /// </summary>
        /// <returns>An array of <see cref="Point"/>s containing the last stored 2D projection of this object, or <see langword="null" /> if <see cref="SetProjection(Camera)"/> has never been called.</returns>
        public abstract Point[] GetProjection();

        /// <inheritdoc/>
        public abstract IEnumerator<Point3D> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal Element3D() { }
    }
}
