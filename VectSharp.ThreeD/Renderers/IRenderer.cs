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
