using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Represents a point.
    /// </summary>
    public class Point3DElement : Element3D
    {
        /// <summary>
        /// The coordinates of the point.
        /// </summary>
        public virtual Point3D Point { get; }

        /// <summary>
        /// The colour with which the point should be drawn.
        /// </summary>
        public virtual Colour Colour { get; set; } = Colours.Black;

        /// <summary>
        /// The diameter of the point in 2D units.
        /// </summary>
        public virtual double Diameter { get; set; } = 1;

        /// <summary>
        /// Creates a new <see cref="Point3DElement"/> instance.
        /// </summary>
        /// <param name="point"></param>
        public Point3DElement(Point3D point)
        {
            this.Point = point;
        }

        private Point[] Projection;

        /// <inheritdoc/>
        public override void SetProjection(Camera camera)
        {
            this.Projection = new Point[]
            {
                camera.Project(this.Point),
            };
        }

        /// <inheritdoc/>
        public override Point[] GetProjection()
        {
            return this.Projection;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override int Count => 1;

        /// <inheritdoc/>
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

        /// <summary>
        /// Converts a base <see cref="Point3DElement"/> into a derived element, keeping the value of the properties of the base element.
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="Point3DElement"/>.</typeparam>
        /// <returns>A derived <see cref="Point3DElement"/> of type <typeparamref name="T"/> with the same value for the properties of the base element.</returns>
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
