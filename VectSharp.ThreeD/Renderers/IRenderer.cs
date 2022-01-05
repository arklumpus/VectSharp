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

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Represents a renderer that can be used to convert a 3D scene into a 2D image.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Renders the 3D scene to a 2D image using the specified <paramref name="lights" /> and <paramref name="camera"/>.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        /// <param name="lights">The lights to use for the rendering.</param>
        /// <param name="camera">The camera to use for the rendering.</param>
        /// <returns></returns>
        Page Render(IScene scene, IEnumerable<ILightSource> lights, Camera camera);
    }
}
