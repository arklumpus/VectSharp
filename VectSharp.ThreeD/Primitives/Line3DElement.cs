using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.ThreeD
{
    public class Line3DElement : Element3D
    {
        public virtual Point3D Point1 { get; set; }
        public virtual Point3D Point2 { get; set; }
        public virtual Colour Colour { get; set; } = Colour.FromRgb(0, 0, 0);
        public virtual LineCaps LineCap { get; set; }
        public virtual LineDash LineDash { get; set; }
        public virtual double Thickness { get; set; } = 1;

        public Line3DElement(Point3D point1, Point3D point2)
        {
            this.Point1 = point1;
            this.Point2 = point2;
        }

        private Point[] Projection;

        public override void SetProjection(Camera camera)
        {
            this.Projection = new Point[]
            {
                camera.Project(this.Point1),
                camera.Project(this.Point2)
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
                        return Point1;
                    case 1:
                        return Point2;
                    default:
                        throw new IndexOutOfRangeException("The index must be between 0 and 1!");
                }
            }
        }

        public override int Count => 2;

        public override IEnumerator<Point3D> GetEnumerator()
        {
            return new LineEnumerator(this);
        }

        internal class LineEnumerator : IEnumerator<Point3D>
        {
            public LineEnumerator(Line3DElement line)
            {
                this.Line = line;
            }

            private readonly Line3DElement Line;
            private int Position = -1;

            public Point3D Current => Line[Position];

            object IEnumerator.Current => Line[Position];

            public bool MoveNext()
            {
                this.Position++;
                return Position < 2;
            }

            public void Reset()
            {
                Position = -1;
            }

            public void Dispose()
            {

            }
        }

        public T ToDerivedLine<T>() where T : Line3DElement
        {
            T t = (T)Activator.CreateInstance(typeof(T), this.Point1, this.Point2);

            t.Colour = this.Colour;
            t.LineCap = this.LineCap;
            t.LineDash = this.LineDash;
            t.Thickness = this.Thickness;
            t.Tag = this.Tag;
            t.ZIndex = this.ZIndex;

            return t;
        }
    }


}
