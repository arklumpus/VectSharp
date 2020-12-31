using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Represents a camera the projects the scene using an orthographic projection.
    /// </summary>
    public sealed class OrthographicCamera : CameraWithControls
    {
        /// <inheritdoc/>
        public override Point TopLeft { get; protected set; }

        /// <inheritdoc/>
        public override Size Size { get; protected set; }

        /// <inheritdoc/>
        public override double ScaleFactor { get; }

        /// <summary>
        /// The position of the centre of the camera plane.
        /// </summary>
        public Point3D Position { get; private set; }

        /// <inheritdoc/>
        public override Point3D ViewPoint => this.Position;

        /// <summary>
        /// The direction towards which the camera is pointing.
        /// </summary>
        public NormalizedVector3D Direction { get; private set; }

        private double[,] RotationMatrix { get; set; }

        /// <summary>
        /// The fixed point about which the camera rotates while orbiting.
        /// </summary>
        public Point3D OrbitOrigin { get; private set; }

        private NormalizedVector3D RotationReference { get; set; }
        private double[,] CameraRotationMatrix { get; set; }

        /// <summary>
        /// Creates a new <see cref="OrthographicCamera"/> instance.
        /// </summary>
        /// <param name="position">The position of the centre of the camera plane.</param>
        /// <param name="direction">The direction towards which the camera is pointing.</param>
        /// <param name="viewSize">The size of the image produced by the camera.</param>
        /// <param name="scaleFactor">The scale factor used to convert camera plane units into 2D units.</param>
        public OrthographicCamera(Point3D position, NormalizedVector3D direction, Size viewSize, double scaleFactor)
        {
            this.Position = position;
            this.Direction = direction;

            this.RotationMatrix = Matrix3D.RotationToAlignWithZ(direction);

            this.ScaleFactor = scaleFactor;

            this.TopLeft = new Point(-viewSize.Width * 0.5 * ScaleFactor, -viewSize.Height * 0.5 * ScaleFactor);
            this.Size = new Size(viewSize.Width * ScaleFactor, viewSize.Height * ScaleFactor);

            this.OrbitOrigin = position + direction * ((Vector3D)position).Modulus;

            if (new Vector3D(0, 1, 0) * this.Direction < 1)
            {
                this.RotationReference = (new Vector3D(0, 1, 0) - (new Vector3D(0, 1, 0) * this.Direction) * this.Direction).Normalize();
            }
            else
            {
                this.RotationReference = (new Vector3D(0, 0, 1) - (new Vector3D(0, 0, 1) * this.Direction) * this.Direction).Normalize();
            }

            Point3D rotatedY = this.RotationMatrix * (Point3D)(Vector3D)this.RotationReference;
            double rotationAngle = Math.PI / 2 - Math.Atan2(rotatedY.Y, rotatedY.X);
            this.CameraRotationMatrix = Matrix3D.RotationAroundAxis(new NormalizedVector3D(0, 0, 1), rotationAngle);
        }

        /// <summary>
        /// Computes the z-depth of a point, corresponding to the distance of the point from the camera plane.
        /// </summary>
        /// <param name="point">The <see cref="Point3D"/> whose z-depth is to be computed.</param>
        /// <returns>The z-depth of the <paramref name="point"/>.</returns>
        public override double ZDepth(Point3D point)
        {
            return (point - this.Position) * this.Direction;
        }

        /// <inheritdoc/>
        public override bool IsCulled(Element3D element)
        {
            if (element is Point3DElement)
            {
                return (element[0] - this.Position) * this.Direction < 0;
            }
            else if (element is Line3DElement)
            {
                foreach (Point3D pt in element)
                {
                    if ((pt - this.Position) * this.Direction > 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (element is Triangle3DElement triangle)
            {
                bool found = false;

                foreach (Point3D pt in element)
                {
                    if ((pt - this.Position) * this.Direction > 0)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    return true;
                }
                else
                {
                    return triangle.ActualNormal * this.Direction < 0;
                }
            }
            else
            {
                return true;
            }
        }

        /// <inheritdoc/>
        public override Point Project(Point3D point)
        {
            Point3D projectedPoint = (Point3D)(point + Direction * ((this.Position - point) * this.Direction) - this.Position);

            Point3D rotatedPoint = this.CameraRotationMatrix * (this.RotationMatrix * projectedPoint);

            return new Point(rotatedPoint.X * ScaleFactor, rotatedPoint.Y * ScaleFactor);
        }

        /// <inheritdoc/>
        public override Point3D Deproject(Point point, Line3DElement line)
        {
            Point3D rotatedPoint = new Point3D(point.X / ScaleFactor, point.Y / ScaleFactor, 0);

            Point3D projectedPoint = RotationMatrix.Inverse() * (CameraRotationMatrix.Inverse() * rotatedPoint);
            Point3D cameraPlanePoint = projectedPoint + (Vector3D)this.Position;

            NormalizedVector3D v = this.Direction;
            NormalizedVector3D l = (line[1] - line[0]).Normalize();

            double t;

            if (v.X * l.Y - v.Y * l.X != 0)
            {
                t = (l.X * (cameraPlanePoint.Y - line[0].Y) - l.Y * (cameraPlanePoint.X - line[0].X)) / (v.X * l.Y - v.Y * l.X);
            }
            else if (v.Z * l.Y - v.Y * l.Z != 0)
            {
                t = (l.Z * (cameraPlanePoint.Y - line[0].Y) - l.Y * (cameraPlanePoint.Z - line[0].Z)) / (v.Z * l.Y - v.Y * l.Z);
            }
            else if (v.Z * l.X - v.X * l.Z != 0)
            {
                t = (l.Z * (cameraPlanePoint.X - line[0].X) - l.X * (cameraPlanePoint.Z - line[0].Z)) / (v.Z * l.X - v.X * l.Z);
            }
            else
            {
                throw new Exception("The lines do not intersect!");
            }

            Point3D pt = cameraPlanePoint + v * t;
            return pt;
        }

        /// <inheritdoc/>
        public override Point3D Deproject(Point point, Triangle3DElement triangle)
        {
            Point3D rotatedPoint = new Point3D(point.X / ScaleFactor, point.Y / ScaleFactor, 0);

            Point3D projectedPoint = RotationMatrix.Inverse() * (CameraRotationMatrix.Inverse() * rotatedPoint);
            Point3D cameraPlanePoint = projectedPoint + (Vector3D)this.Position;

            Point3D centroid = (Point3D)(((Vector3D)triangle[0] + (Vector3D)triangle[1] + (Vector3D)triangle[2]) * (1.0 / 3.0));

            Vector3D l = Direction;

            double d = ((centroid - cameraPlanePoint) * triangle.ActualNormal) / (l * triangle.ActualNormal);

            Point3D pt = cameraPlanePoint + l * d;
            return pt;
        }

        /// <inheritdoc/>
        public override void Orbit(double theta, double phi)
        {
            Vector3D vect = this.Position - this.OrbitOrigin;

            NormalizedVector3D yAxis = new NormalizedVector3D(0, 1, 0);
            NormalizedVector3D xAxis = (this.Direction ^ (Vector3D)yAxis).Normalize();

            phi *= Math.Sign(Math.Atan2(xAxis.Z, xAxis.X));
            theta *= Math.Sign(yAxis * this.RotationReference);

            double[,] thetaRotation = Matrix3D.RotationAroundAxis(yAxis, theta);

            double[,] phiRotation = Matrix3D.RotationAroundAxis(xAxis, phi);

            Vector3D rotatedVect = (Vector3D)(phiRotation * (thetaRotation * (Point3D)vect));

            this.Position = this.OrbitOrigin + rotatedVect;
            this.Direction = (this.OrbitOrigin - this.Position).Normalize();
            this.RotationReference = ((Vector3D)(phiRotation * (thetaRotation * (Point3D)(Vector3D)this.RotationReference))).Normalize();
            this.RotationMatrix = Matrix3D.RotationToAlignWithZ(this.Direction);

            Point3D rotatedY = this.RotationMatrix * (Point3D)(Vector3D)this.RotationReference;
            double rotationAngle = Math.PI / 2 - Math.Atan2(rotatedY.Y, rotatedY.X);
            this.CameraRotationMatrix = Matrix3D.RotationAroundAxis(new NormalizedVector3D(0, 0, 1), rotationAngle);
        }

        /// <summary>
        /// Increases or decreases the field of view of the camera.
        /// </summary>
        /// <param name="amount">How much the field of view should be increased or decreased. Negative values increase the field of view, positive values decrease it.</param>
        public override void Zoom(double amount)
        {
            this.Size = new Size(this.Size.Width * Math.Pow(2, -amount), this.Size.Height * Math.Pow(2, -amount));
            this.TopLeft = new Point(-this.Size.Width * 0.5, -this.Size.Height * 0.5);
        }

        /// <inheritdoc/>
        public override void Pan(double x, double y)
        {
            Vector3D delta = this.RotationMatrix.Inverse() * new Point3D(-x / ScaleFactor, -y / ScaleFactor, 0);

            this.Position = this.Position + delta;

            this.OrbitOrigin = this.OrbitOrigin + delta;
        }
    }
}
