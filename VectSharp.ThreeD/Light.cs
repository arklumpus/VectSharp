using System;
using System.Collections.Generic;
using System.Text;

namespace VectSharp.ThreeD
{
    public interface ILightSource
    {
        (double intensity, NormalizedVector3D direction) GetLightAt(Point3D point);
    }

    public class AmbientLightSource : ILightSource
    {
        public double Intensity { get; set; }

        public AmbientLightSource(double intensity)
        {
            this.Intensity = intensity;
        }

        public (double intensity, NormalizedVector3D direction) GetLightAt(Point3D point)
        {
            return (Intensity, new NormalizedVector3D(double.NaN, double.NaN, double.NaN));
        }
    }

    public class ParallelLightSource : ILightSource
    {
        public double Intensity { get; set; }
        public NormalizedVector3D Direction { get; set; }

        public ParallelLightSource(double intensity, NormalizedVector3D direction)
        {
            this.Intensity = intensity;
            this.Direction = direction;
        }

        public (double intensity, NormalizedVector3D direction) GetLightAt(Point3D point)
        {
            return (Intensity, Direction);
        }
    }
}
