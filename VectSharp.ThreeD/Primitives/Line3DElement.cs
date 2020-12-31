using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Represents a line segment.
    /// </summary>
    public class Line3DElement : Element3D
    {
        /// <summary>
        /// The start point of the line.
        /// </summary>
        public virtual Point3D Point1 { get; }

        /// <summary>
        /// The end point of the line.
        /// </summary>
        public virtual Point3D Point2 { get; }

        /// <summary>
        /// The colour with which the line should be drawn.
        /// </summary>
        public virtual Colour Colour { get; set; } = Colour.FromRgb(0, 0, 0);

        /// <summary>
        /// The cap of the line.
        /// </summary>
        public virtual LineCaps LineCap { get; set; }

        /// <summary>
        /// The dash of the line.
        /// </summary>
        public virtual LineDash LineDash { get; set; }

        /// <summary>
        /// The thickness of the line in 2D units.
        /// </summary>
        public virtual double Thickness { get; set; } = 1;

        /// <summary>
        /// Creates a new <see cref="Line3DElement"/> instance.
        /// </summary>
        /// <param name="point1">The start point of the line.</param>
        /// <param name="point2">The end point of the line.</param>
        public Line3DElement(Point3D point1, Point3D point2)
        {
            this.Point1 = point1;
            this.Point2 = point2;
        }

        private Point[] Projection;

        /// <inheritdoc/>
        public override void SetProjection(Camera camera)
        {
            this.Projection = new Point[]
            {
                camera.Project(this.Point1),
                camera.Project(this.Point2)
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
                        return Point1;
                    case 1:
                        return Point2;
                    default:
                        throw new IndexOutOfRangeException("The index must be between 0 and 1!");
                }
            }
        }

        /// <inheritdoc/>
        public override int Count => 2;

        /// <inheritdoc/>
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

        /// <summary>
        /// Converts a base <see cref="Line3DElement"/> into a derived element, keeping the value of the properties of the base element.
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="Line3DElement"/>.</typeparam>
        /// <returns>A derived <see cref="Line3DElement"/> of type <typeparamref name="T"/> with the same value for the properties of the base element.</returns>
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
