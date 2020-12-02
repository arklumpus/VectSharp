using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectSharp.ThreeD
{
    public struct LightIntensity
    {
        public double Intensity;
        public NormalizedVector3D Direction;

        public LightIntensity(double intensity, NormalizedVector3D direction)
        {
            this.Intensity = intensity;
            this.Direction = direction;
        }

        public void Deconstruct(out double intensity, out NormalizedVector3D direction)
        {
            intensity = this.Intensity;
            direction = this.Direction;
        }
    }

    public interface ILightSource
    {
        LightIntensity GetLightAt(Point3D point);
        bool IsObstructed(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles);
        bool CastsShadow { get; }
    }

    public class AmbientLightSource : ILightSource
    {
        public double Intensity { get; set; }

        public bool CastsShadow => false;

        public AmbientLightSource(double intensity)
        {
            this.Intensity = intensity;
        }

        public LightIntensity GetLightAt(Point3D point)
        {
            return new LightIntensity(Intensity, new NormalizedVector3D(double.NaN, double.NaN, double.NaN));
        }

        public bool IsObstructed(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles)
        {
            return false;
        }
    }

    public class ParallelLightSource : ILightSource
    {
        public double Intensity { get; set; }
        public NormalizedVector3D Direction { get; }
        public NormalizedVector3D ReverseDirection { get; }

        public bool CastsShadow { get; set; } = true;

        public ParallelLightSource(double intensity, NormalizedVector3D direction)
        {
            this.Intensity = intensity;
            this.Direction = direction;
            this.ReverseDirection = direction.Reverse();
        }

        public LightIntensity GetLightAt(Point3D point)
        {
            return new LightIntensity(Intensity, Direction);
        }

        public bool IsObstructed(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles)
        {
            foreach (Triangle3DElement triangle in shadowingTriangles)
            {
                Point3D? projected = triangle.ProjectOnThisPlane(point, ReverseDirection, true, double.PositiveInfinity);

                if (projected != null && Intersections3D.PointInTriangle(projected.Value, triangle.Point1, triangle.Point2, triangle.Point3))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class PointLightSource : ILightSource
    {
        public bool CastsShadow { get; set; } = true;

        public Point3D Position { get; set; }

        public double Intensity { get; set; }

        public double DistanceAttenuationExponent { get; set; } = 2;

        public PointLightSource(double intensity, Point3D position)
        {
            this.Position = position;
            this.Intensity = intensity;
        }

        public LightIntensity GetLightAt(Point3D point)
        {
            if (DistanceAttenuationExponent == 2)
            {
                return new LightIntensity(Intensity / ((point.X - Position.X) * (point.X - Position.X) + (point.Y - Position.Y) * (point.Y - Position.Y) + (point.Z - Position.Z) * (point.Z - Position.Z)), (point - Position).Normalize());
            }
            else if (DistanceAttenuationExponent == 0)
            {
                return new LightIntensity(Intensity, (point - Position).Normalize());
            }
            else
            {
                return new LightIntensity(Intensity / Math.Pow((point.X - Position.X) * (point.X - Position.X) + (point.Y - Position.Y) * (point.Y - Position.Y) + (point.Z - Position.Z) * (point.Z - Position.Z), DistanceAttenuationExponent * 0.5), (point - Position).Normalize());
            }
        }

        public bool IsObstructed(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles)
        {
            Vector3D reverseDir = this.Position - point;
            double maxD = reverseDir.Modulus;
            NormalizedVector3D reverseDirNorm = new NormalizedVector3D(reverseDir.X / maxD, reverseDir.Y / maxD, reverseDir.Z / maxD, false);

            foreach (Triangle3DElement triangle in shadowingTriangles)
            {
                Point3D? projected = triangle.ProjectOnThisPlane(point, reverseDirNorm, true, maxD);

                if (projected != null && Intersections3D.PointInTriangle(projected.Value, triangle.Point1, triangle.Point2, triangle.Point3))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class SpotlightLightSource : ILightSource
    {
        public bool CastsShadow { get; set; } = true;
        public Point3D Position { get; set; }
        public NormalizedVector3D Direction { get; set; }
        public double Intensity { get; set; }
        public double BeamWidthAngle { get; set; }
        public double CutoffAngle { get; set; }
        public double DistanceAttenuationExponent { get; set; } = 2;
        public double AngleAttenuationExponent { get; set; } = 1;

        public SpotlightLightSource(double intensity, Point3D position, NormalizedVector3D direction, double beamWidthAngle, double cutoffAngle)
        {
            this.Position = position;
            this.Direction = direction;
            this.Intensity = intensity;
            this.BeamWidthAngle = beamWidthAngle;
            this.CutoffAngle = cutoffAngle;
        }

        public LightIntensity GetLightAt(Point3D point)
        {
            double angle = Math.Atan2(((point - Position) ^ Direction).Modulus, (point - Position) * Direction);
            if (angle > Math.PI)
            {
                angle -= 2 * Math.PI;
            }
            angle = Math.Abs(angle);
            double intensity;

            if (DistanceAttenuationExponent == 0)
            {
                intensity = Intensity;
            }
            else if (DistanceAttenuationExponent == 2)
            {
                intensity = Intensity / ((point.X - Position.X) * (point.X - Position.X) + (point.Y - Position.Y) * (point.Y - Position.Y) + (point.Z - Position.Z) * (point.Z - Position.Z));
            }
            else
            {
                intensity = Intensity / Math.Pow((point.X - Position.X) * (point.X - Position.X) + (point.Y - Position.Y) * (point.Y - Position.Y) + (point.Z - Position.Z) * (point.Z - Position.Z), 0.5 * DistanceAttenuationExponent);
            }

            if (angle <= BeamWidthAngle)
            {
                // * 1
            }
            else if (angle >= CutoffAngle)
            {
                intensity = 0;
            }
            else if (AngleAttenuationExponent == 0)
            {
                // * 1
            }
            else if (AngleAttenuationExponent == 1)
            {
                intensity *= (CutoffAngle - angle) / (CutoffAngle - BeamWidthAngle);
            }
            else
            {
                intensity *= Math.Pow((CutoffAngle - angle) / (CutoffAngle - BeamWidthAngle), AngleAttenuationExponent);
            }

            return new LightIntensity(intensity, (point - Position).Normalize());
        }

        public bool IsObstructed(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles)
        {
            Vector3D reverseDir = this.Position - point;
            double maxD = reverseDir.Modulus;
            NormalizedVector3D reverseDirNorm = new NormalizedVector3D(reverseDir.X / maxD, reverseDir.Y / maxD, reverseDir.Z / maxD, false);

            foreach (Triangle3DElement triangle in shadowingTriangles)
            {
                Point3D? projected = triangle.ProjectOnThisPlane(point, reverseDirNorm, true, maxD);

                if (projected != null && Intersections3D.PointInTriangle(projected.Value, triangle.Point1, triangle.Point2, triangle.Point3))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class MaskedLightSource : ILightSource
    {
        public bool CastsShadow { get; set; } = true;
        public Point3D Position { get; }
        public Point3D Origin { get; }
        public NormalizedVector3D Direction { get; }
        public double Distance { get; }
        public List<Point3D[]> TriangulatedMask { get; }
        public double Intensity { get; set; }
        public double DistanceAttenuationExponent { get; set; } = 2;

        public double AngleAttenuationExponent { get; set; } = 1;

        private double Numerator { get; }

        public MaskedLightSource(double intensity, Point3D position, NormalizedVector3D direction, double distance, GraphicsPath mask, Vector3D maskYAxis, double triangulationResolution) : this(intensity, position, direction, distance, mask.Triangulate(triangulationResolution, true), maskYAxis)
        {

        }

        public MaskedLightSource(double intensity, Point3D position, NormalizedVector3D direction, double distance, IEnumerable<GraphicsPath> triangulatedMask, Vector3D maskYAxis)
        {
            this.Intensity = intensity;
            this.Position = position;
            this.Direction = direction;
            this.Distance = distance;
            this.Origin = this.Position + distance * this.Direction;

            double[,] rotation = Matrix3D.RotationToAlignWithZ(direction).Inverse();

            NormalizedVector3D yAxis = (maskYAxis - direction * (maskYAxis * direction)).Normalize();

            NormalizedVector3D rotatedYAxis = ((Vector3D)(rotation * new Point3D(0, 1, 0))).Normalize();

            double[,] rotation2 = Matrix3D.RotationToAlignAWithB(rotatedYAxis, yAxis);

            this.TriangulatedMask = new List<Point3D[]>();

            foreach (GraphicsPath trianglePath in triangulatedMask)
            {
                Point3D[] triangle = new Point3D[3];

                List<Point> points = trianglePath.GetPoints().First();

                for (int i = 0; i < 3; i++)
                {
                    triangle[i] = (Point3D)((Vector3D)(rotation2 * (rotation * new Point3D(points[i].X, points[i].Y, 0))) + Origin);
                }

                this.TriangulatedMask.Add(triangle);
            }
        }

        public LightIntensity GetLightAt(Point3D point)
        {
            double d = Distance / ((point - this.Position) * this.Direction);
            Point3D pt = this.Position + (point - this.Position) * d;

            bool contained = false;

            foreach (Point3D[] triangle in TriangulatedMask)
            {
                if (Intersections3D.PointInTriangle(pt, triangle[0], triangle[1], triangle[2]))
                {
                    contained = true;
                    break;
                }
            }

            if (contained)
            {
                double intensity;

                if (DistanceAttenuationExponent == 0)
                {
                    intensity = Intensity;
                }
                else if (DistanceAttenuationExponent == 2)
                {
                    intensity = Intensity / ((point.X - Position.X) * (point.X - Position.X) + (point.Y - Position.Y) * (point.Y - Position.Y) + (point.Z - Position.Z) * (point.Z - Position.Z));
                }
                else
                {
                    intensity = Intensity / Math.Pow((point.X - Position.X) * (point.X - Position.X) + (point.Y - Position.Y) * (point.Y - Position.Y) + (point.Z - Position.Z) * (point.Z - Position.Z), 0.5 * DistanceAttenuationExponent);
                }

                if (AngleAttenuationExponent == 0)
                {
                    // * 1
                }
                else
                {
                    double angle = Math.Atan2(((point - Position) ^ Direction).Modulus, (point - Position) * Direction);
                    if (angle > Math.PI)
                    {
                        angle -= 2 * Math.PI;
                    }
                    angle = Math.Abs(angle) / Math.PI * 2;

                    if (AngleAttenuationExponent == 1)
                    {
                        intensity *= 1 - angle;
                    }
                    else
                    {
                        intensity *= Math.Pow(1 - angle, AngleAttenuationExponent);
                    }
                }

                return new LightIntensity(intensity, (point - Position).Normalize());
            }
            else
            {
                return new LightIntensity(0, (point - Position).Normalize());
            }
        }


        public bool IsObstructed(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles)
        {
            Vector3D reverseDir = this.Position - point;
            double maxD = reverseDir.Modulus;
            NormalizedVector3D reverseDirNorm = new NormalizedVector3D(reverseDir.X / maxD, reverseDir.Y / maxD, reverseDir.Z / maxD, false);

            foreach (Triangle3DElement triangle in shadowingTriangles)
            {
                Point3D? projected = triangle.ProjectOnThisPlane(point, reverseDirNorm, true, maxD);

                if (projected != null && Intersections3D.PointInTriangle(projected.Value, triangle.Point1, triangle.Point2, triangle.Point3))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
