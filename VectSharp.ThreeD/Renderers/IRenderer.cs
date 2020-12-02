using System;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.ThreeD
{
    public interface IRenderer
    {
        Page Render(IScene scene, IEnumerable<ILightSource> lights, Camera camera);
    }
}
