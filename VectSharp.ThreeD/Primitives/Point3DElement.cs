using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.ThreeD
{
    public class Point3DElement : Element3D
    {
        public virtual Point3D Point { get; }
        public virtual Colour Colour { get; set; } = Colours.Black;
        public virtual double Diameter { get; set; } = 1;

        public Point3DElement(Point3D point)
        {
            this.Point = point;
        }

        private Point[] Projection;

        public override void SetProjection(Camera camera)
        {
            this.Projection = new Point[]
            {
                camera.Project(this.Point),
            };
        }

        public override Point[] GetProjection()
        {
            return this.Projection;
        }

        public override Point3D this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Point;
                    default:
                        throw new IndexOutOfRangeException("The index must be equal to 0!");
                }
            }
        }

        public override int Count => 1;

        public override IEnumerator<Point3D> GetEnumerator()
        {
            return new PointEnumerator(this);
        }

        internal class PointEnumerator : IEnumerator<Point3D>
        {
            public PointEnumerator(Point3DElement point)
            {
                this.Point = point;
            }

            private readonly Point3DElement Point;
            private int Position = -1;

            public Point3D Current => Point[Position];

            object IEnumerator.Current => Point[Position];

            public bool MoveNext()
            {
                this.Position++;
                return Position < 1;
            }

            public void Reset()
            {
                Position = -1;
            }

            public void Dispose()
            {

            }
        }

        public T ToDerivedPoint<T>() where T : Point3DElement
        {
            T t = (T)Activator.CreateInstance(typeof(T), this.Point);

            t.Colour = this.Colour;
            t.Diameter = this.Diameter;
            t.Tag = this.Tag;
            t.ZIndex = this.ZIndex;

            return t;
        }
    }

}
