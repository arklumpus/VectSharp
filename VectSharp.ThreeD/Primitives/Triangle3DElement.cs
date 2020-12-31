using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Represents a triangle.
    /// </summary>
    public class Triangle3DElement : Element3D
    {
        /// <summary>
        /// The first vertex of the triangle.
        /// </summary>
        public virtual Point3D Point1 { get; }

        /// <summary>
        /// The second vertex of the triangle.
        /// </summary>
        public virtual Point3D Point2 { get; }

        /// <summary>
        /// The third vertex of the triangle.
        /// </summary>
        public virtual Point3D Point3 { get; }

        /// <summary>
        /// The fill material(s) that should be used to draw the triangle.
        /// </summary>
        public virtual List<IMaterial> Fill { get; protected set; }

        /// <summary>
        /// The centroid of the triangle.
        /// </summary>
        public virtual Point3D Centroid { get; }

        /// <summary>
        /// The normal to the surface of the triangle at the barycenter that should be used when performing lighting computations.
        /// </summary>
        public virtual NormalizedVector3D Normal { get; }

        /// <summary>
        /// The normal to the plane containing the vertices of the triangle that should be used when performing geometric computations.
        /// </summary>
        public virtual NormalizedVector3D ActualNormal { get; }

        /// <summary>
        /// The normal to the surface of the triangle at the first vertex that should be used when performing lighting computations.
        /// </summary>
        public virtual NormalizedVector3D Point1Normal { get; }

        /// <summary>
        /// The normal to the surface of the triangle at the second vertex that should be used when performing lighting computations.
        /// </summary>
        public virtual NormalizedVector3D Point2Normal { get; }

        /// <summary>
        /// The normal to the surface of the triangle at the third vertex that should be used when performing lighting computations.
        /// </summary>
        public virtual NormalizedVector3D Point3Normal { get; }

        /// <summary>
        /// Indicates whether the triangle can cast a shadow on other triangles.
        /// </summary>
        public virtual bool CastsShadow { get; set; } = false;

        /// <summary>
        /// Indicates wether the triangle can receive the shadow cast by other triangles.
        /// </summary>
        public virtual bool ReceivesShadow { get; set; } = false;

        /// <summary>
        /// Stores fixed values used to accelerate the computation of the barycentric coordinates of a point.
        /// </summary>
        protected (Vector3D v0, Vector3D v1, double d00, double d01, double d11, double invDenom) BarycentricHelper { get; }

        internal bool IsFlat { get; }

        /// <summary>
        /// Creates a new <see cref="Triangle3DElement"/> instance, representing a flat triangle.
        /// </summary>
        /// <param name="point1">The first vertex of the triangle.</param>
        /// <param name="point2">The second vertex of the triangle.</param>
        /// <param name="point3">The third vertex of the triangle.</param>
        public Triangle3DElement(Point3D point1, Point3D point2, Point3D point3)
        {
            this.Point1 = point1;
            this.Point2 = point2;
            this.Point3 = point3;
            this.Fill = new List<IMaterial>();

            this.Normal = ((this.Point2 - this.Point1) ^ (this.Point3 - this.Point1)).Normalize();
            this.ActualNormal = this.Normal;
            this.Centroid = (Point3D)(((Vector3D)point1 + (Vector3D)point2 + (Vector3D)point3) * (1.0 / 3));

            this.Point1Normal = this.Normal;
            this.Point2Normal = this.Normal;
            this.Point3Normal = this.Normal;

            this.IsFlat = true;

            Vector3D v0 = this.Point2 - this.Point1;
            Vector3D v1 = this.Point3 - this.Point1;
            double d00 = v0 * v0;
            double d01 = v0 * v1;
            double d11 = v1 * v1;

            double invDenomin = 1.0 / (d00 * d11 - d01 * d01);

            this.BarycentricHelper = (v0, v1, d00, d01, d11, invDenomin);
        }

        /// <summary>
        /// Creates a new <see cref="Triangle3DElement"/> instance, representing a triangle with the specified vertex normals.
        /// </summary>
        /// <param name="point1">The first vertex of the triangle.</param>
        /// <param name="point2">The second vertex of the triangle.</param>
        /// <param name="point3">The third vertex of the triangle.</param>
        /// <param name="point1Normal">The normal at the first vertex of the triangle.</param>
        /// <param name="point2Normal">The normal at the second vertex of the triangle.</param>
        /// <param name="point3Normal">The normal at the third vertex of the triangle.</param>
        public Triangle3DElement(Point3D point1, Point3D point2, Point3D point3, NormalizedVector3D point1Normal, NormalizedVector3D point2Normal, NormalizedVector3D point3Normal)
        {
            this.Point1 = point1;
            this.Point2 = point2;
            this.Point3 = point3;
            this.Fill = new List<IMaterial>();

            this.ActualNormal = ((this.Point2 - this.Point1) ^ (this.Point3 - this.Point1)).Normalize();
            this.Normal = ((Vector3D)point1Normal + point2Normal + point3Normal).Normalize();
            this.Centroid = (Point3D)(((Vector3D)point1 + (Vector3D)point2 + (Vector3D)point3) * (1.0 / 3));

            this.Point1Normal = point1Normal;
            this.Point2Normal = point2Normal;
            this.Point3Normal = point3Normal;

            this.IsFlat = this.Point1Normal.Equals(this.Point2Normal) && this.Point1Normal.Equals(this.Point3Normal);

            Vector3D v0 = this.Point2 - this.Point1;
            Vector3D v1 = this.Point3 - this.Point1;
            double d00 = v0 * v0;
            double d01 = v0 * v1;
            double d11 = v1 * v1;

            double invDenomin = 1.0 / (d00 * d11 - d01 * d01);

            this.BarycentricHelper = (v0, v1, d00, d01, d11, invDenomin);
        }

        /// <summary>
        /// Computes the normal at a specified point on the triangle.
        /// </summary>
        /// <param name="point">The point where the normal should be computed.</param>
        /// <returns>The normal at the specified <paramref name="point"/>.</returns>
        public virtual NormalizedVector3D GetNormalAt(Point3D point)
        {
            if (this.IsFlat)
            {
                return this.Normal;
            }

            Vector3D v2 = point - this.Point1;

            double d20 = v2 * BarycentricHelper.v0;
            double d21 = v2 * BarycentricHelper.v1;

            double v = (BarycentricHelper.d11 * d20 - BarycentricHelper.d01 * d21) * BarycentricHelper.invDenom;
            double w = (BarycentricHelper.d00 * d21 - BarycentricHelper.d01 * d20) * BarycentricHelper.invDenom;

            return ((1 - v - w) * Point1Normal + v * Point2Normal + w * Point3Normal).Normalize();
        }

        /// <summary>
        /// Computes the barycentric coordinates of a point with respect to the current triangle.
        /// </summary>
        /// <param name="point">The <see cref="Point3D"/> whose barycentric coordinates should be computed.</param>
        /// <returns>The barycentric coordinates of the specified <paramref name="point"/>.</returns>
        public virtual BarycentricPoint ComputeBarycentric(Point3D point)
        {
            Vector3D v2 = point - this.Point1;

            double d20 = v2 * BarycentricHelper.v0;
            double d21 = v2 * BarycentricHelper.v1;

            double v = (BarycentricHelper.d11 * d20 - BarycentricHelper.d01 * d21) * BarycentricHelper.invDenom;
            double w = (BarycentricHelper.d00 * d21 - BarycentricHelper.d01 * d20) * BarycentricHelper.invDenom;

            return new BarycentricPoint(1 - v - w, v, w);
        }

        /// <summary>
        /// Projects a <see cref="Point3D"/> on the plane of the triangle, along a specified direction.
        /// </summary>
        /// <param name="point">The point that should be projected on the triangle.</param>
        /// <param name="direction">The direction along which the point should be projected.</param>
        /// <param name="positiveOnly">If this is <see langword="true" />, only points that are in front of the <paramref name="point"/> (with respect to the <paramref name="direction"/>) are returned.</param>
        /// <param name="maxD">The maximum distance between the point and the plane.</param>
        /// <returns>The projection of the <paramref name="point"/> on the triangle plane along <paramref name="direction"/>, or <see langword="null"/> if: the <paramref name="direction"/> is parallel to the plane, or <paramref name="positiveOnly"/> is true and the plane is behind the <paramref name="point"/>, or the distance between the <paramref name="point"/> and the plane is greater than <paramref name="maxD"/>.</returns>
        public virtual Point3D? ProjectOnThisPlane(Point3D point, NormalizedVector3D direction, bool positiveOnly, double maxD)
        {
            double d = ((this.Centroid - point) * this.ActualNormal) / (direction * this.ActualNormal);

            if ((d >= 0 || !positiveOnly) && d <= maxD)
            {
                return point + d * direction;
            }
            else
            {
                return null;
            }
        }

        private Point[] Projection;

        /// <inheritdoc/>
        public override void SetProjection(Camera camera)
        {
            this.Projection = new Point[]
            {
                camera.Project(this.Point1),
                camera.Project(this.Point2),
                camera.Project(this.Point3)
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
                    case 2:
                        return Point3;
                    default:
                        throw new IndexOutOfRangeException("The index must be between 0 and 2!");
                }
            }
        }

        /// <inheritdoc/>
        public override int Count => 3;

        /// <inheritdoc/>
        public override IEnumerator<Point3D> GetEnumerator()
        {
            return new TriangleEnumerator(this);
        }

        internal class TriangleEnumerator : IEnumerator<Point3D>
        {
            public TriangleEnumerator(Triangle3DElement triangle)
            {
                this.Triangle = triangle;
            }

            private readonly Triangle3DElement Triangle;
            private int Position = -1;

            public Point3D Current => Triangle[Position];

            object IEnumerator.Current => Triangle[Position];

            public bool MoveNext()
            {
                this.Position++;
                return Position < 3;
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
        /// Converts a base <see cref="Triangle3DElement"/> into a derived element, keeping the value of the properties of the base element.
        /// </summary>
        /// <typeparam name="T">A type derived from <see cref="Triangle3DElement"/>.</typeparam>
        /// <returns>A derived <see cref="Triangle3DElement"/> of type <typeparamref name="T"/> with the same value for the properties of the base element.</returns>
        public T ToDerivedTriangle<T>() where T : Triangle3DElement
        {
            T t = (T)Activator.CreateInstance(typeof(T), this.Point1, this.Point2, this.Point3);

            t.CastsShadow = this.CastsShadow;
            t.Fill = this.Fill;
            t.ReceivesShadow = this.ReceivesShadow;
            t.Tag = this.Tag;
            t.ZIndex = this.ZIndex;

            return t;
        }
    }
}
