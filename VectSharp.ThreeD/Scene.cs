using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace VectSharp.ThreeD
{
    public interface IScene
    {
        IEnumerable<Element3D> SceneElements { get; }

        void AddElement(Element3D element);
        void AddRange(IEnumerable<Element3D> element);
        void Replace(Func<Element3D, Element3D> replacementFunction);
        void Replace(Func<Element3D, IEnumerable<Element3D>> replacementFunction);

        object SceneLock { get; }
    }


    public class Scene : IScene
    {
        public static double Tolerance { get; } = 1e-4;

        public object SceneLock { get; }

        private List<Element3D> sceneElements;
        public IEnumerable<Element3D> SceneElements => sceneElements;

        public Scene()
        {
            sceneElements = new List<Element3D>();
            this.SceneLock = new object();
        }

        public void AddElement(Element3D element)
        {
            this.sceneElements.Add(element);
        }

        public void AddRange(IEnumerable<Element3D> elements)
        {
            this.sceneElements.AddRange(elements);
        }

        public void Replace(Func<Element3D, Element3D> replacementFunction)
        {
            for (int i = 0; i < sceneElements.Count; i++)
            {
                sceneElements[i] = replacementFunction(sceneElements[i]);
            }
        }

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
