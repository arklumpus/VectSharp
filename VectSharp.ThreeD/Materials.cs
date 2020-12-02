using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VectSharp.ThreeD
{
    public interface IMaterial
    {
        Colour GetColour(Point3D point, NormalizedVector3D surfaceNormal, Camera camera, IEnumerable<ILightSource> lights);
    }

    public class ColourMaterial : IMaterial
    {
        public Colour Colour { get; }

        public ColourMaterial(Colour colour)
        {
            this.Colour = colour;
        }

        public Colour GetColour(Point3D point, NormalizedVector3D surfaceNormal, Camera camera, IEnumerable<ILightSource> lights)
        {
            return Colour;
        }
    }

    public class PhongMaterial : IMaterial
    {
        public Colour Colour { get; }

        private (double L, double a, double b) LabColour;
        private (double H, double S, double L) LabBlackHSL;
        private (double H, double S, double L) LabWhiteHSL;
        private double IntensityExponent;
        private double TotalLength;

        public double AmbientReflectionCoefficient { get; set; } = 1;
        public double DiffuseReflectionCoefficient { get; set; } = 1;
        public double SpecularReflectionCoefficient { get; set; } = 1;
        public double SpecularShininess { get; set; } = 1;
        public PhongMaterial(Colour colour)
        {
            this.Colour = colour;
            this.LabColour = colour.ToLab();
            this.LabBlackHSL = Colour.FromLab(0, LabColour.a, LabColour.b).ToHSL();
            this.LabWhiteHSL = Colour.FromLab(1, LabColour.a, LabColour.b).ToHSL();
            this.TotalLength = 1 + LabBlackHSL.L + (1 - LabWhiteHSL.L);
            this.IntensityExponent = Math.Log((LabBlackHSL.L + LabColour.L) / TotalLength) / Math.Log(0.5);
        }

        private Colour GetScaledColour(double intensity)
        {
            intensity = Math.Max(Math.Min(intensity, 1), 0);

            double pos = Math.Pow(intensity, IntensityExponent) * TotalLength;

            if (pos <= LabBlackHSL.L)
            {
                return Colour.FromHSL(LabBlackHSL.H, LabBlackHSL.S, pos).WithAlpha(Colour.A);
            }
            else if (pos >= 1 + LabBlackHSL.L)
            {
                return Colour.FromHSL(LabWhiteHSL.H, LabWhiteHSL.S, LabWhiteHSL.L + pos - 1 - LabBlackHSL.L).WithAlpha(Colour.A);
            }
            else
            {
                return Colour.FromLab(pos - LabBlackHSL.L, LabColour.a, LabColour.b).WithAlpha(Colour.A);
            }
        }

        public Colour GetColour(Point3D point, NormalizedVector3D surfaceNormal, Camera camera, IEnumerable<ILightSource> lights)
        {
            double intensity = 0;

            foreach (ILightSource light in lights)
            {
                (double lightIntensity, NormalizedVector3D lightDirection) = light.GetLightAt(point);

                if (double.IsNaN(lightDirection.X))
                {
                    intensity += lightIntensity * AmbientReflectionCoefficient;
                }
                else
                {
                    double dotProd = lightDirection * surfaceNormal;

                    intensity += lightIntensity * Math.Max(0, dotProd) * DiffuseReflectionCoefficient;

                    if (dotProd > 0)
                    {
                        NormalizedVector3D mirroredDirection = (lightDirection - 2 * dotProd * surfaceNormal).Normalize();

                        NormalizedVector3D cameraDirection = (camera.ViewPoint - point).Normalize();

                        double dotProd2 = mirroredDirection * cameraDirection;

                        if (dotProd2 >= 0)
                        {
                            intensity += lightIntensity * Math.Pow(dotProd2, SpecularShininess) * SpecularReflectionCoefficient;
                        }
                    }
                }
            }

            return GetScaledColour(intensity);
        }
    }
}
