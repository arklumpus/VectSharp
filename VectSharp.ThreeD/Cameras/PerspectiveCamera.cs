/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2022 Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Represents a camera that projects the scene using perspective projection.
    /// </summary>
    public sealed class PerspectiveCamera : CameraWithControls, IBlurrableCamera
    {
        /// <inheritdoc/>
        public override double ScaleFactor { get; }

        /// <summary>
        /// The position of the eye of the camera.
        /// </summary>
        public Point3D Position { get; private set; }

        /// <inheritdoc/>
        public override Point3D ViewPoint => Position;

        /// <summary>
        /// Represents the direction towards which the camera is facing.
        /// </summary>
        public NormalizedVector3D Direction { get; private set; }

        /// <summary>
        /// The distance between the camera eye and the camera (focus) plane.
        /// </summary>
        public double Distance { get; }

        private double[,] RotationMatrix { get; set; }

        private double[,] CameraRotationMatrix { get; set; }

        /// <summary>
        /// The fixed point about which the camera rotates while orbiting.
        /// </summary>
        public Point3D OrbitOrigin { get; private set; }

        private Point3D Origin { get; set; }

        private Point3D Origin2DReference { get; set; }

        private NormalizedVector3D RotationReference { get; set; }

        /// <inheritdoc/>
        public override Point TopLeft { get; protected set; }

        /// <inheritdoc/>
        public override Size Size { get; protected set; }

        private double _lensWidth = 0;

        /// <summary>
        /// The size of the camera lens. Larger values result in images that are more blurred. The minimum is 0 (corresponding to a perfectly crisp image).
        /// </summary>
        public double LensWidth
        {
            get
            {
                return _lensWidth;
            }
            set
            {
                if (value >= 0)
                {
                    _lensWidth = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(LensWidth), value, "The lens width must be greater than or equal to 0!");
                }
            }
        }

        private int _samplingPoints = 1;

        /// <summary>
        /// The number of samples per point to use when blurring the image. Higher values result in more faithful images, but increase rendering time. The minimum is 1 (no blurring is performed).
        /// </summary>
        public int SamplingPoints
        {
            get
            {
                return _samplingPoints;
            }
            set
            {
                if (value >= 1)
                {
                    _samplingPoints = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(SamplingPoints), value, "The number of sampling points must be greater than or equal to 1!");
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="PerspectiveCamera"/> instance.
        /// </summary>
        /// <param name="position">The position of the eye of the camera.</param>
        /// <param name="direction">The direction towards which the camera is pointing.</param>
        /// <param name="distance">The distance between the camera eye and the camera plane.</param>
        /// <param name="viewSize">The size of the image viewed by the camera.</param>
        /// <param name="scaleFactor">The scale factor used to convert camera plane units to 2D units.</param>
        public PerspectiveCamera(Point3D position, NormalizedVector3D direction, double distance, Size viewSize, double scaleFactor)
        {
            this.Position = position;
            this.Direction = direction;
            this.Distance = distance;

            this.Origin = position + direction * distance;
            this.Origin2DReference = this.Origin;
            this.RotationMatrix = Matrix3D.RotationToAlignWithZ(direction);

            this.ScaleFactor = scaleFactor;

            this.TopLeft = new Point(-viewSize.Width * 0.5 * ScaleFactor, -viewSize.Height * 0.5 * ScaleFactor);
            this.Size = new Size(viewSize.Width * ScaleFactor, viewSize.Height * ScaleFactor);

            this.OrbitOrigin = position + direction * ((Vector3D)position).Modulus;

            if (Math.Abs(new Vector3D(0, 1, 0) * this.Direction) < 1)
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

        private PerspectiveCamera(Point3D position, NormalizedVector3D direction, double distance, Size viewSize, double scaleFactor, NormalizedVector3D rotationReference, Point3D origin2DReference)
        {
            this.Position = position;
            this.Direction = direction;
            this.Distance = distance;

            this.Origin2DReference = origin2DReference;
            this.Origin = position + direction * distance;
            this.RotationMatrix = Matrix3D.RotationToAlignWithZ(direction);

            this.ScaleFactor = scaleFactor;

            this.TopLeft = new Point(-viewSize.Width * 0.5 * ScaleFactor, -viewSize.Height * 0.5 * ScaleFactor);
            this.Size = new Size(viewSize.Width * ScaleFactor, viewSize.Height * ScaleFactor);

            this.OrbitOrigin = position + direction * ((Vector3D)position).Modulus;

            this.RotationReference = rotationReference;

            Point3D rotatedY = this.RotationMatrix * (Point3D)(Vector3D)this.RotationReference;
            double rotationAngle = Math.PI / 2 - Math.Atan2(rotatedY.Y, rotatedY.X);
            this.CameraRotationMatrix = Matrix3D.RotationAroundAxis(new NormalizedVector3D(0, 0, 1), rotationAngle);
        }

        /// <inheritdoc/>
        public Camera[] GetCameras()
        {
            if (this.LensWidth == 0 || this.SamplingPoints == 1)
            {
                return new Camera[] { this };
            }
            else
            {
                NormalizedVector3D xAxis = (this.Direction ^ this.RotationReference).Normalize();

                Camera[] tbr = new Camera[this.SamplingPoints];

                for (int i = 0; i < this.SamplingPoints; i++)
                {
                    double r = ((double)i / (this.SamplingPoints - 1)) * this.LensWidth;
                    double theta = (double)i / (this.SamplingPoints - 1) * 2 * Math.PI * (this.SamplingPoints / 3.7);

                    double x = r * Math.Cos(theta);
                    double y = r * Math.Sin(theta);

                    Point3D pt = this.Position + x * xAxis + y * this.RotationReference;
                    tbr[i] = new PerspectiveCamera(pt, this.Direction, this.Distance, this.Size, this.ScaleFactor, this.RotationReference, this.Origin);
                }

                return tbr;
            }
        }

        /// <inheritdoc/>
        public override Point Project(Point3D point)
        {
            Vector3D cameraDirection = (this.Position - point).Normalize();

            Point3D projectedPoint = (Point3D)(point + cameraDirection * (((this.Origin - point) * this.Direction) / (cameraDirection * this.Direction)) - this.Origin2DReference);

            Point3D rotatedPoint = this.CameraRotationMatrix * (this.RotationMatrix * projectedPoint);

            return new Point(rotatedPoint.X * ScaleFactor, rotatedPoint.Y * ScaleFactor);
        }

        /// <inheritdoc/>
        public override Point3D Deproject(Point point, Line3DElement line)
        {
            Point3D rotatedPoint = new Point3D(point.X / ScaleFactor, point.Y / ScaleFactor, 0);

            Point3D projectedPoint = RotationMatrix.Inverse() * (CameraRotationMatrix.Inverse() * rotatedPoint);
            Point3D cameraPlanePoint = projectedPoint + (Vector3D)this.Origin2DReference;

            NormalizedVector3D v = (cameraPlanePoint - this.Position).Normalize();
            NormalizedVector3D l = (line[1] - line[0]).Normalize();

            double t;

            if (v.X * l.Y - v.Y * l.X != 0)
            {
                t = (l.X * (this.Position.Y - line[0].Y) - l.Y * (this.Position.X - line[0].X)) / (v.X * l.Y - v.Y * l.X);
            }
            else if (v.Z * l.Y - v.Y * l.Z != 0)
            {
                t = (l.Z * (this.Position.Y - line[0].Y) - l.Y * (this.Position.Z - line[0].Z)) / (v.Z * l.Y - v.Y * l.Z);
            }
            else if (v.Z * l.X - v.X * l.Z != 0)
            {
                t = (l.Z * (this.Position.X - line[0].X) - l.X * (this.Position.Z - line[0].Z)) / (v.Z * l.X - v.X * l.Z);
            }
            else
            {
                throw new Exception("The lines do not intersect!");
            }

            Point3D pt = this.Position + v * t;
            return pt;
        }

        /// <inheritdoc/>
        public override Point3D Deproject(Point point, Triangle3DElement triangle)
        {
            Point3D rotatedPoint = new Point3D(point.X / ScaleFactor, point.Y / ScaleFactor, 0);

            Point3D projectedPoint = RotationMatrix.Inverse() * (CameraRotationMatrix.Inverse() * rotatedPoint);
            Point3D cameraPlanePoint = projectedPoint + (Vector3D)this.Origin2DReference;

            Point3D centroid = (Point3D)(((Vector3D)triangle[0] + (Vector3D)triangle[1] + (Vector3D)triangle[2]) * (1.0 / 3.0));

            Vector3D l = (cameraPlanePoint - this.Position).Normalize();

            double d = ((centroid - this.Position) * triangle.ActualNormal) / (l * triangle.ActualNormal);

            Point3D pt = this.Position + l * d;
            return pt;
        }

        /// <summary>
        /// Computes the z-depth of a point. This corresponds to the square of the distance between the point and the camera eye.
        /// </summary>
        /// <param name="point">The <see cref="Point3D"/> whose z-depth should be computed.</param>
        /// <returns>The z-depth of the <paramref name="point"/>.</returns>
        public override double ZDepth(Point3D point)
        {
            return (point.X - this.Position.X) * (point.X - this.Position.X) + (point.Y - this.Position.Y) * (point.Y - this.Position.Y) + (point.Z - this.Position.Z) * (point.Z - this.Position.Z);
        }

        /// <inheritdoc/>
        public override bool IsCulled(Element3D element)
        {
            if (element is Point3DElement)
            {
                return (element[0] - this.Position) * this.Direction <= 0;
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
                    return triangle.ActualNormal * (triangle.Centroid - this.Position) < 0;
                }
            }
            else
            {
                return true;
            }
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

            this.Origin = this.Position + this.Direction * this.Distance;
            this.Origin2DReference = this.Origin;
            this.RotationMatrix = Matrix3D.RotationToAlignWithZ(this.Direction);

            Point3D rotatedY = this.RotationMatrix * (Point3D)(Vector3D)this.RotationReference;
            double rotationAngle = Math.PI / 2 - Math.Atan2(rotatedY.Y, rotatedY.X);
            this.CameraRotationMatrix = Matrix3D.RotationAroundAxis(new NormalizedVector3D(0, 0, 1), rotationAngle);
        }

        /// <summary>
        /// Increases or decreases the field of view of the camera.
        /// </summary>
        /// <param name="amount">How much the field of view should be increased or decreased. Positive values move the camera closer to the scene, negative values move it farther away.</param>
        public override void Zoom(double amount)
        {
            this.Position = this.Position + this.Direction * amount;
            this.Origin = this.Position + this.Direction * this.Distance;
            this.Origin2DReference = this.Origin;
        }

        /// <inheritdoc/>
        public override void Pan(double x, double y)
        {
            Vector3D delta = this.RotationMatrix.Inverse() * new Point3D(-x / ScaleFactor, -y / ScaleFactor, 0);

            this.Position = this.Position + delta;
            this.Origin = this.Position + this.Direction * this.Distance;

            this.OrbitOrigin = this.OrbitOrigin + delta;
            this.Origin2DReference = this.Origin;
        }
    }
}
