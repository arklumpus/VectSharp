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

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Represents a 3D scene.
    /// </summary>
    public interface IScene
    {
        /// <summary>
        /// The <see cref="Element3D"/>s constituting the scene.
        /// </summary>
        IEnumerable<Element3D> SceneElements { get; }

        /// <summary>
        /// Adds the specified <paramref name="element"/> to the scene.
        /// </summary>
        /// <param name="element">The <see cref="Element3D"/> to add.</param>
        void AddElement(Element3D element);

        /// <summary>
        /// Adds the specified <paramref name="elements"/> to the scene.
        /// </summary>
        /// <param name="elements">A collection of <see cref="Element3D"/>s to add.</param>
        void AddRange(IEnumerable<Element3D> elements);

        /// <summary>
        /// Replaces each element in the scene with the element returned by the <paramref name="replacementFunction"/>.
        /// </summary>
        /// <param name="replacementFunction">A function replacing each <see cref="Element3D"/> in the scene with another <see cref="Element3D"/>.</param>
        void Replace(Func<Element3D, Element3D> replacementFunction);

        /// <summary>
        /// Replaces each element in the scene with the element(s) returned by the <paramref name="replacementFunction"/>.
        /// </summary>
        /// <param name="replacementFunction">A function replacing each <see cref="Element3D"/> in the scene with 0 or more <see cref="Element3D"/>s.</param>
        void Replace(Func<Element3D, IEnumerable<Element3D>> replacementFunction);

        /// <summary>
        /// An object used to synchronise multithreaded rendering of the same scene.
        /// </summary>
        object SceneLock { get; }
    }

    /// <summary>
    /// Represents a 3D scene.
    /// </summary>
    public class Scene : IScene
    {
        /// <inheritdoc/>
        public object SceneLock { get; }

        private List<Element3D> sceneElements;

        /// <inheritdoc/>
        public IEnumerable<Element3D> SceneElements => sceneElements;

        /// <summary>
        /// Creates a new <see cref="Scene"/>.
        /// </summary>
        public Scene()
        {
            sceneElements = new List<Element3D>();
            this.SceneLock = new object();
        }

        /// <inheritdoc/>
        public void AddElement(Element3D element)
        {
            this.sceneElements.Add(element);
        }

        /// <inheritdoc/>
        public void AddRange(IEnumerable<Element3D> elements)
        {
            this.sceneElements.AddRange(elements);
        }

        /// <inheritdoc/>
        public void Replace(Func<Element3D, Element3D> replacementFunction)
        {
            for (int i = 0; i < sceneElements.Count; i++)
            {
                sceneElements[i] = replacementFunction(sceneElements[i]);
            }
        }

        /// <inheritdoc/>
        public void Replace(Func<Element3D, IEnumerable<Element3D>> replacementFunction)
        {
            List<Element3D> newElements = new List<Element3D>(sceneElements.Count);

            for (int i = 0; i < this.sceneElements.Count; i++)
            {
                newElements.AddRange(replacementFunction(sceneElements[i]));
            }

            newElements.TrimExcess();

            this.sceneElements = newElements;
        }
    }
}
