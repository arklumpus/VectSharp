using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.ThreeD
{
    public struct BarycentricPoint
    {
        public double U;
        public double V;
        public double W;

        public BarycentricPoint(double u, double v, double w)
        {
            this.U = u;
            this.V = v;
            this.W = w;
        }
    }

    public class Triangle3DElement : Element3D
    {
        public virtual Point3D Point1 { get; }
        public virtual Point3D Point2 { get; }
        public virtual Point3D Point3 { get; }

        public virtual List<IMaterial> Fill { get; protected set; }

        public virtual Point3D Centroid { get; }

        public virtual NormalizedVector3D Normal { get; }
        public virtual NormalizedVector3D ActualNormal { get; }

        public virtual NormalizedVector3D Point1Normal { get; }
        public virtual NormalizedVector3D Point2Normal { get; }
        public virtual NormalizedVector3D Point3Normal { get; }

        //public virtual Triangle3DElement Parent { get; set; } = null;

        public virtual bool CastsShadow { get; set; } = false;
        public virtual bool ReceivesShadow { get; set; } = false;

        //public virtual double OverFill { get; set; } = 0;

        protected (Vector3D v0, Vector3D v1, double d00, double d01, double d11, double invDenom) BarycentricHelper { get; }

        internal bool IsFlat { get; }

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

        public virtual BarycentricPoint ComputeBarycentric(Point3D point)
        {
            Vector3D v2 = point - this.Point1;

            double d20 = v2 * BarycentricHelper.v0;
            double d21 = v2 * BarycentricHelper.v1;

            double v = (BarycentricHelper.d11 * d20 - BarycentricHelper.d01 * d21) * BarycentricHelper.invDenom;
            double w = (BarycentricHelper.d00 * d21 - BarycentricHelper.d01 * d20) * BarycentricHelper.invDenom;

            return new BarycentricPoint(1 - v - w, v, w);
        }

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

        public override void SetProjection(Camera camera)
        {
            this.Projection = new Point[]
            {
                camera.Project(this.Point1),
                camera.Project(this.Point2),
                camera.Project(this.Point3)
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
                    case 2:
                        return Point3;
                    default:
                        throw new IndexOutOfRangeException("The index must be between 0 and 2!");
                }
            }
        }

        public override int Count => 3;

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

        /*  public bool IsAdjacentTo(Triangle3DElement triangle2)
          {
              int equalCount = 0;

              for (int i = 0; i < 3; i++)
              {
                  for (int j = 0; j < 3; j++)
                  {
                      if (this[i].Equals(triangle2[j], Scene.Tolerance))
                      {
                          equalCount++;

                          if (equalCount >= 2)
                          {
                              return true;
                          }

                          break;
                      }
                  }

                  if (i == 1 && equalCount == 0)
                  {
                      return false;
                  }
              }

              return false;
          }

          public int[] GetCommonSide(Triangle3DElement triangle2)
          {
              int[] tbr = new int[2];
              int equalCount = 0;

              for (int i = 0; i < 3; i++)
              {
                  for (int j = 0; j < 3; j++)
                  {
                      if (this[i].Equals(triangle2[j], Scene.Tolerance))
                      {
                          tbr[equalCount] = i;
                          equalCount++;

                          if (equalCount == 2)
                          {
                              return tbr;
                          }

                          break;
                      }
                  }

                  if (i == 1 && equalCount == 0)
                  {
                      return null;
                  }
              }

              return null;
          }*/

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
