using System;
using System.Collections.Generic;
using System.Linq;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Represents the intensity of a light source at a particular point.
    /// </summary>
    public struct LightIntensity
    {
        /// <summary>
        /// The intensity of the light.
        /// </summary>
        public double Intensity;

        /// <summary>
        /// The direction towards from which the light comes.
        /// </summary>
        public NormalizedVector3D Direction;

        /// <summary>
        /// Creates a new <see cref="LightIntensity"/>.
        /// </summary>
        /// <param name="intensity">The intensity of the light.</param>
        /// <param name="direction">The direction from which the light comes.</param>
        public LightIntensity(double intensity, NormalizedVector3D direction)
        {
            this.Intensity = intensity;
            this.Direction = direction;
        }

        /// <summary>
        /// Deconstructs the struct.
        /// </summary>
        /// <param name="intensity">This parameter will hold the <see cref="Intensity"/> of the light.</param>
        /// <param name="direction">This parameter will hold the <see cref="Direction"/> of the light.</param>
        public void Deconstruct(out double intensity, out NormalizedVector3D direction)
        {
            intensity = this.Intensity;
            direction = this.Direction;
        }
    }

    /// <summary>
    /// Represents a light source.
    /// </summary>
    public interface ILightSource
    {
        /// <summary>
        /// Computes the light intensity at the specified point, without taking into account any obstructions.
        /// </summary>
        /// <param name="point">The <see cref="Point3DElement"/> at which the light intensity should be computed.</param>
        /// <returns></returns>
        LightIntensity GetLightAt(Point3D point);

        /// <summary>
        /// Determines whether the light casts a shadow or not.
        /// </summary>
        bool CastsShadow { get; }

        /// <summary>
        /// Determines the amount of obstruction of the light that results at <paramref name="point"/> due to the specified <paramref name="shadowingTriangles"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point3D"/> at which the obstruction should be computed.</param>
        /// <param name="shadowingTriangles">A collection of <see cref="Triangle3DElement"/> casting shadows.</param>
        /// <returns>1 if the light is completely obstructed, 0 if the light is completely visible, a value between these if the light is partially obstructed.</returns>
        double GetObstruction(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles);
    }

    /// <summary>
    /// Represents a uniform ambien light source.
    /// </summary>
    public class AmbientLightSource : ILightSource
    {
        /// <summary>
        /// The intensity of the light.
        /// </summary>
        public double Intensity { get; set; }

        /// <inheritdoc/>
        public bool CastsShadow => false;

        /// <summary>
        /// Creates a new <see cref="AmbientLightSource"/> instance.
        /// </summary>
        /// <param name="intensity">The intensity of the light.</param>
        public AmbientLightSource(double intensity)
        {
            this.Intensity = intensity;
        }

        /// <inheritdoc/>
        public LightIntensity GetLightAt(Point3D point)
        {
            return new LightIntensity(Intensity, new NormalizedVector3D(double.NaN, double.NaN, double.NaN));
        }

        /// <inheritdoc/>
        public double GetObstruction(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles)
        {
            return 0;
        }
    }

    /// <summary>
    /// Represents a parallel light source.
    /// </summary>
    public class ParallelLightSource : ILightSource
    {
        /// <summary>
        /// The intensity of the light.
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// The direction along which the light travels.
        /// </summary>
        public NormalizedVector3D Direction { get; }

        /// <summary>
        /// The reverse of <see cref="Direction"/>.
        /// </summary>
        public NormalizedVector3D ReverseDirection { get; }

        /// <inheritdoc/>
        public bool CastsShadow { get; set; } = true;

        /// <summary>
        /// Creates a new <see cref="ParallelLightSource"/> instance.
        /// </summary>
        /// <param name="intensity">The intensity of the light.</param>
        /// <param name="direction">The direction along which the light travels.</param>
        public ParallelLightSource(double intensity, NormalizedVector3D direction)
        {
            this.Intensity = intensity;
            this.Direction = direction;
            this.ReverseDirection = direction.Reverse();
        }

        /// <inheritdoc/>
        public LightIntensity GetLightAt(Point3D point)
        {
            return new LightIntensity(Intensity, Direction);
        }

        /// <inheritdoc/>
        public double GetObstruction(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles)
        {
            foreach (Triangle3DElement triangle in shadowingTriangles)
            {
                Point3D? projected = triangle.ProjectOnThisPlane(point, ReverseDirection, true, double.PositiveInfinity);

                if (projected != null && Intersections3D.PointInTriangle(projected.Value, triangle.Point1, triangle.Point2, triangle.Point3))
                {
                    return 1;
                }
            }

            return 0;
        }
    }

    /// <summary>
    /// Represents a point light source.
    /// </summary>
    public class PointLightSource : ILightSource
    {
        /// <inheritdoc/>
        public bool CastsShadow { get; set; } = true;

        /// <summary>
        /// The position of the light source.
        /// </summary>
        public Point3D Position { get; set; }

        /// <summary>
        /// The base intensity of the light.
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// An exponent determining how fast the light attenuates with increasing distance. Set to 0 to disable distance attenuation.
        /// </summary>
        public double DistanceAttenuationExponent { get; set; } = 2;

        /// <summary>
        /// Creates a new <see cref="PointLightSource"/> instance.
        /// </summary>
        /// <param name="intensity">The intensity of the light.</param>
        /// <param name="position">The position of the light source.</param>
        public PointLightSource(double intensity, Point3D position)
        {
            this.Position = position;
            this.Intensity = intensity;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public double GetObstruction(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles)
        {
            Vector3D reverseDir = this.Position - point;
            double maxD = reverseDir.Modulus;
            NormalizedVector3D reverseDirNorm = new NormalizedVector3D(reverseDir.X / maxD, reverseDir.Y / maxD, reverseDir.Z / maxD, false);

            foreach (Triangle3DElement triangle in shadowingTriangles)
            {
                Point3D? projected = triangle.ProjectOnThisPlane(point, reverseDirNorm, true, maxD);

                if (projected != null && Intersections3D.PointInTriangle(projected.Value, triangle.Point1, triangle.Point2, triangle.Point3))
                {
                    return 1;
                }
            }

            return 0;
        }
    }

    /// <summary>
    /// Represents a conic spotlight.
    /// </summary>
    public class SpotlightLightSource : ILightSource
    {
        /// <inheritdoc/>
        public bool CastsShadow { get; set; } = true;

        /// <summary>
        /// The position of the light source.
        /// </summary>
        public Point3D Position { get; set; }

        /// <summary>
        /// The direction of the cone axis.
        /// </summary>
        public NormalizedVector3D Direction { get; set; }

        /// <summary>
        /// The base intensity of the light.
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// The angular size of the light cone, in radians.
        /// </summary>
        public double BeamWidthAngle { get; set; }

        /// <summary>
        /// The angular size of the cutoff cone, in radians.
        /// </summary>
        public double CutoffAngle { get; set; }

        /// <summary>
        /// An exponent determining how fast the light attenuates with increasing distance. Set to 0 to disable distance attenuation.
        /// </summary>
        public double DistanceAttenuationExponent { get; set; } = 2;

        /// <summary>
        /// An exponent determining how fast the light attenuates between the main light cone and the cutoff cone.
        /// </summary>
        public double AngleAttenuationExponent { get; set; } = 1;

        /// <summary>
        /// Creates a new <see cref="SpotlightLightSource"/> instance.
        /// </summary>
        /// <param name="intensity">The intensity of the light.</param>
        /// <param name="position">The position of the light source.</param>
        /// <param name="direction">The direction of the cone's axis.</param>
        /// <param name="beamWidthAngle">The angular size of the light cone, in radians.</param>
        /// <param name="cutoffAngle">The angular size of the cutoff cone, in radians.</param>
        public SpotlightLightSource(double intensity, Point3D position, NormalizedVector3D direction, double beamWidthAngle, double cutoffAngle)
        {
            this.Position = position;
            this.Direction = direction;
            this.Intensity = intensity;
            this.BeamWidthAngle = beamWidthAngle;
            this.CutoffAngle = cutoffAngle;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public double GetObstruction(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles)
        {
            Vector3D reverseDir = this.Position - point;
            double maxD = reverseDir.Modulus;
            NormalizedVector3D reverseDirNorm = new NormalizedVector3D(reverseDir.X / maxD, reverseDir.Y / maxD, reverseDir.Z / maxD, false);

            foreach (Triangle3DElement triangle in shadowingTriangles)
            {
                Point3D? projected = triangle.ProjectOnThisPlane(point, reverseDirNorm, true, maxD);

                if (projected != null && Intersections3D.PointInTriangle(projected.Value, triangle.Point1, triangle.Point2, triangle.Point3))
                {
                    return 1;
                }
            }

            return 0;
        }
    }

    /// <summary>
    /// Represents a point light source with a stencil in front of it.
    /// </summary>
    public class MaskedLightSource : ILightSource
    {
        /// <inheritdoc/>
        public bool CastsShadow { get; set; } = true;

        /// <summary>
        /// The position of the light source.
        /// </summary>
        public Point3D Position { get; }

        /// <summary>
        /// The projection of the <see cref="Position"/> on the mask plane along the light's <see cref="Direction"/>.
        /// </summary>
        public Point3D Origin { get; }

        /// <summary>
        /// The direction of the light.
        /// </summary>
        public NormalizedVector3D Direction { get; }

        /// <summary>
        /// The distance between the light source and the mask plane.
        /// </summary>
        public double Distance { get; }

        private List<Point3D[]> TriangulatedMask { get; }

        /// <summary>
        /// The base intensity of the light.
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// An exponent determining how fast the light attenuates with increasing distance. Set to 0 to disable distance attenuation.
        /// </summary>
        public double DistanceAttenuationExponent { get; set; } = 2;

        /// <summary>
        /// An exponent determining how fast the light attenuates away from the light's axis. Set to 0 to disable angular attenuation.
        /// </summary>
        public double AngleAttenuationExponent { get; set; } = 1;

        /// <summary>
        /// Creates a new <see cref="MaskedLightSource"/> by triangulating the specified <see cref="GraphicsPath"/>.
        /// </summary>
        /// <param name="intensity">The base intensity of the light.</param>
        /// <param name="position">The position of the light source.</param>
        /// <param name="direction">The direction of the light.</param>
        /// <param name="distance">The distance between the light source and the mask plane.</param>
        /// <param name="mask">A <see cref="GraphicsPath"/> representing the transparent part of the mask.</param>
        /// <param name="maskOrientation">An angle in radians determining the orientation of the 2D mask in the mask plane.</param>
        /// <param name="triangulationResolution">The resolution to use to triangulate the <paramref name="mask"/>.</param>
        public MaskedLightSource(double intensity, Point3D position, NormalizedVector3D direction, double distance, GraphicsPath mask, double maskOrientation, double triangulationResolution) : this(intensity, position, direction, distance, mask.Triangulate(triangulationResolution, true), maskOrientation)
        {

        }

        /// <summary>
        /// Creates a new <see cref="MaskedLightSource"/> using the specified <paramref name="triangulatedMask"/>.
        /// </summary>
        /// <param name="intensity">The base intensity of the light.</param>
        /// <param name="position">The position of the light source.</param>
        /// <param name="direction">The direction of the light.</param>
        /// <param name="distance">The distance between the light source and the mask plane.</param>
        /// <param name="triangulatedMask">A collection of <see cref="GraphicsPath"/>s representing the transparent part of the mask. Each <see cref="GraphicsPath"/> should represent a single triangle.</param>
        /// <param name="maskOrientation">An angle in radians determining the orientation of the 2D mask in the mask plane.</param>
        public MaskedLightSource(double intensity, Point3D position, NormalizedVector3D direction, double distance, IEnumerable<GraphicsPath> triangulatedMask, double maskOrientation)
        {
            this.Intensity = intensity;
            this.Position = position;
            this.Direction = direction;
            this.Distance = distance;
            this.Origin = this.Position + distance * this.Direction;

            double[,] rotation = Matrix3D.RotationToAlignWithZ(direction).Inverse();

            double[,] rotation2 = Matrix3D.RotationAroundAxis(direction, maskOrientation);

            List<Point3D[]> maskList = new List<Point3D[]>();

            foreach (GraphicsPath trianglePath in triangulatedMask)
            {
                Point3D[] triangle = new Point3D[3];

                List<Point> points = trianglePath.GetPoints().First();

                for (int i = 0; i < 3; i++)
                {
                    triangle[i] = (Point3D)((Vector3D)(rotation2 * (rotation * new Point3D(points[i].X, points[i].Y, 0))) + Origin);
                }

                maskList.Add(triangle);
            }

            this.TriangulatedMask = maskList;
        }

        /// <inheritdoc/>
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

                    if (Math.Abs(angle) < Math.PI / 2)
                    {
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
                    else
                    {
                        intensity = 0;
                    }
                }

                return new LightIntensity(intensity, (point - Position).Normalize());
            }
            else
            {
                return new LightIntensity(0, (point - Position).Normalize());
            }
        }

        /// <inheritdoc/>
        public double GetObstruction(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles)
        {
            Vector3D reverseDir = this.Position - point;
            double maxD = reverseDir.Modulus;
            NormalizedVector3D reverseDirNorm = new NormalizedVector3D(reverseDir.X / maxD, reverseDir.Y / maxD, reverseDir.Z / maxD, false);

            foreach (Triangle3DElement triangle in shadowingTriangles)
            {
                Point3D? projected = triangle.ProjectOnThisPlane(point, reverseDirNorm, true, maxD);

                if (projected != null && Intersections3D.PointInTriangle(projected.Value, triangle.Point1, triangle.Point2, triangle.Point3))
                {
                    return 1;
                }
            }

            return 0;
        }
    }

    /// <summary>
    /// Represents a light source emitting light from a circular area.
    /// </summary>
    public class AreaLightSource : ILightSource
    {
        /// <inheritdoc/>
        public bool CastsShadow { get; set; } = true;

        /// <summary>
        /// The centre of the light-emitting area.
        /// </summary>
        public Point3D Center { get; }

        private Point3D VirtualSource { get; }

        /// <summary>
        /// The direction of the light's main axis, i.e. the normal to the plane containing the light-emitting area.
        /// </summary>
        public NormalizedVector3D Direction { get; }

        /// <summary>
        /// The radius of the light emitting area.
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// The radius of the penumbra area.
        /// </summary>
        public double PenumbraRadius { get; }

        /// <summary>
        /// The base intensity of the light.
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// The distance between the focal point of the light and the light's <see cref="Center"/>.
        /// </summary>
        public double SourceDistance { get; }

        /// <summary>
        /// An exponent determining how fast the light attenuates with increasing distance. Set to 0 to disable distance attenuation.
        /// </summary>
        public double DistanceAttenuationExponent { get; set; } = 2;

        /// <summary>
        /// An exponent determining how fast the light attenuates between the light-emitting area radius and the penumbra radius.
        /// </summary>
        public double PenumbraAttenuationExponent { get; set; } = 1;

        /// <summary>
        /// The number of points to use when determining the amount of light that is obstructed at a certain point.
        /// </summary>
        public int ShadowSamplingPointCount { get; }

        private Point3D[] ShadowSamplingPoints { get; }

        /// <summary>
        /// Creates a new <see cref="AreaLightSource"/> instance.
        /// </summary>
        /// <param name="intensity">The base intensity of the light.</param>
        /// <param name="center">The centre of the light-emitting area.</param>
        /// <param name="radius">The radius of the light-emitting area.</param>
        /// <param name="penumbraRadius">The radius of the penumbra area.</param>
        /// <param name="direction">The direction of the light.</param>
        /// <param name="sourceDistance">The distance between the focal point of the light and the light's center.</param>
        /// <param name="shadowSamplingPointCount">The number of points to use when determining the amount of light that is obstructed at a certain point.</param>
        public AreaLightSource(double intensity, Point3D center, double radius, double penumbraRadius, NormalizedVector3D direction, double sourceDistance, int shadowSamplingPointCount)
        {
            if (shadowSamplingPointCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(shadowSamplingPointCount), shadowSamplingPointCount, "The number of shadow sampling points must be greater than or equal to 1!");
            }

            this.Center = center;
            this.Direction = direction;
            this.Intensity = intensity;
            this.Radius = radius;
            this.PenumbraRadius = penumbraRadius;
            this.SourceDistance = sourceDistance;

            this.VirtualSource = this.Center - this.Direction * this.SourceDistance;

            this.ShadowSamplingPointCount = shadowSamplingPointCount;

            NormalizedVector3D yAxis;

            if (Math.Abs(new Vector3D(0, 1, 0) * this.Direction) < 1)
            {
                yAxis = (new Vector3D(0, 1, 0) - (new Vector3D(0, 1, 0) * this.Direction) * this.Direction).Normalize();
            }
            else
            {
                yAxis = (new Vector3D(0, 0, 1) - (new Vector3D(0, 0, 1) * this.Direction) * this.Direction).Normalize();
            }

            NormalizedVector3D xAxis = (this.Direction ^ yAxis).Normalize();

            this.ShadowSamplingPoints = new Point3D[shadowSamplingPointCount - 1];

            for (int i = 0; i < shadowSamplingPointCount - 1; i++)
            {
                double r = ((double)i / (shadowSamplingPointCount - 2)) * this.Radius;
                double theta = (double)i / (shadowSamplingPointCount - 2) * 2 * Math.PI * ((shadowSamplingPointCount - 1) / 3.7);

                double x = r * Math.Cos(theta);
                double y = r * Math.Sin(theta);

                Point3D pt = this.Center + x * xAxis + y * yAxis;
                this.ShadowSamplingPoints[i] = pt;
            }
        }

        /// <inheritdoc/>
        public LightIntensity GetLightAt(Point3D point)
        {
            double denom = ((point - this.VirtualSource) * this.Direction);

            if (denom <= 0)
            {
                return new LightIntensity(0, (point - this.VirtualSource).Normalize());
            }
            else
            {
                double d = this.SourceDistance / denom;
                Point3D pt = this.VirtualSource + d * (point - this.VirtualSource);
                double distSq = (pt.X - this.Center.X) * (pt.X - this.Center.X) + (pt.Y - this.Center.Y) * (pt.Y - this.Center.Y) + (pt.Z - this.Center.Z) * (pt.Z - this.Center.Z);

                if (distSq < this.Radius * this.Radius)
                {
                    double intensity;
                    if (DistanceAttenuationExponent == 0)
                    {
                        intensity = Intensity;
                    }
                    else if (DistanceAttenuationExponent == 2)
                    {
                        intensity = Intensity / ((point.X - this.VirtualSource.X) * (point.X - this.VirtualSource.X) + (point.Y - this.VirtualSource.Y) * (point.Y - this.VirtualSource.Y) + (point.Z - this.VirtualSource.Z) * (point.Z - this.VirtualSource.Z));
                    }
                    else
                    {
                        intensity = Intensity / Math.Pow((point.X - this.VirtualSource.X) * (point.X - this.VirtualSource.X) + (point.Y - this.VirtualSource.Y) * (point.Y - this.VirtualSource.Y) + (point.Z - this.VirtualSource.Z) * (point.Z - this.VirtualSource.Z), 0.5 * DistanceAttenuationExponent);
                    }

                    return new LightIntensity(intensity, (point - this.VirtualSource).Normalize());
                }
                else if (distSq < this.PenumbraRadius * this.PenumbraRadius)
                {
                    double intensity;
                    if (DistanceAttenuationExponent == 0)
                    {
                        intensity = Intensity;
                    }
                    else if (DistanceAttenuationExponent == 2)
                    {
                        intensity = Intensity / ((point.X - this.VirtualSource.X) * (point.X - this.VirtualSource.X) + (point.Y - this.VirtualSource.Y) * (point.Y - this.VirtualSource.Y) + (point.Z - VirtualSource.Z) * (point.Z - this.VirtualSource.Z));
                    }
                    else
                    {
                        intensity = Intensity / Math.Pow((point.X - this.VirtualSource.X) * (point.X - this.VirtualSource.X) + (point.Y - this.VirtualSource.Y) * (point.Y - this.VirtualSource.Y) + (point.Z - this.VirtualSource.Z) * (point.Z - this.VirtualSource.Z), 0.5 * DistanceAttenuationExponent);
                    }

                    if (PenumbraAttenuationExponent == 0)
                    {
                        //intensity *= 1;
                    }
                    else if (PenumbraAttenuationExponent == 1)
                    {
                        double factor = (Math.Sqrt(distSq) - this.Radius) / (this.PenumbraRadius - this.Radius);
                        intensity *= 1 - factor;
                    }
                    else
                    {
                        double factor = (Math.Sqrt(distSq) - this.Radius) / (this.PenumbraRadius - this.Radius);
                        intensity *= Math.Pow(1 - factor, PenumbraAttenuationExponent);
                    }

                    return new LightIntensity(intensity, (point - this.VirtualSource).Normalize());
                }
                else
                {
                    return new LightIntensity(0, (point - this.VirtualSource).Normalize());
                }
            }
        }

        /// <inheritdoc/>
        public double GetObstruction(Point3D point, IEnumerable<Triangle3DElement> shadowingTriangles)
        {
            double totalObstruction = 0;
            int sampleCount = 0;

            {
                Vector3D reverseDir = this.VirtualSource - point;

                double denom = (reverseDir * this.Direction);

                if (denom == 0)
                {
                    //totalObstruction += 0;
                    sampleCount++;
                }
                else
                {
                    double d = this.SourceDistance / denom;
                    Point3D pt = this.VirtualSource + d * reverseDir;

                    double maxD = (point - pt).Modulus;
                    NormalizedVector3D reverseDirNorm = new NormalizedVector3D(reverseDir.X / maxD, reverseDir.Y / maxD, reverseDir.Z / maxD, false);

                    bool found = false;

                    foreach (Triangle3DElement triangle in shadowingTriangles)
                    {
                        Point3D? projected = triangle.ProjectOnThisPlane(point, reverseDirNorm, true, maxD);

                        if (projected != null && Intersections3D.PointInTriangle(projected.Value, triangle.Point1, triangle.Point2, triangle.Point3))
                        {
                            totalObstruction += 1;
                            sampleCount++;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        //totalObstruction += 0;
                        sampleCount++;
                    }
                }
            }


            for (int i = 0; i < ShadowSamplingPoints.Length; i++)
            {
                Vector3D reverseDir = ShadowSamplingPoints[i] - point;

                double maxD = reverseDir.Modulus;
                NormalizedVector3D reverseDirNorm = new NormalizedVector3D(reverseDir.X / maxD, reverseDir.Y / maxD, reverseDir.Z / maxD, false);

                bool found = false;

                foreach (Triangle3DElement triangle in shadowingTriangles)
                {
                    Point3D? projected = triangle.ProjectOnThisPlane(point, reverseDirNorm, true, maxD);

                    if (projected != null && Intersections3D.PointInTriangle(projected.Value, triangle.Point1, triangle.Point2, triangle.Point3))
                    {
                        totalObstruction += 1;
                        sampleCount++;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    //totalObstruction += 0;
                    sampleCount++;
                }
            }

            return totalObstruction / sampleCount;
        }
    }
}
