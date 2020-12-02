using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.ThreeD
{
    public abstract class Element3D : IReadOnlyList<Point3D>
    {
        public virtual string Tag { get; set; }
        public virtual int ZIndex { get; set; } = 0;

        public abstract int Count { get; }

        public abstract Point3D this[int index] { get; }

        public abstract void SetProjection(Camera camera);
        public abstract Point[] GetProjection();

        public abstract IEnumerator<Point3D> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal Element3D() { }
    }
}
