using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VectSharp.ThreeD
{
    /// <summary>
    /// Represents a material used to the determine the appearance of <see cref="Triangle3DElement"/>.
    /// </summary>
    public interface IMaterial
    {
        /// <summary>
        /// Obtains the <see cref="Colour"/> at the specified point.
        /// </summary>
        /// <param name="point">The point whose colour should be determined.</param>
        /// <param name="surfaceNormal">The normal to the surface at the specified <paramref name="point"/>.</param>
        /// <param name="camera">The camera being used to render the scene.</param>
        /// <param name="lights">A list of light sources that are present in the scene.</param>
        /// <param name="obstructions">A list of values indicating how obstructed each light source is.</param>
        /// <returns>The <see cref="Colour"/> of the specified point.</returns>
        Colour GetColour(Point3D point, NormalizedVector3D surfaceNormal, Camera camera, IList<ILightSource> lights, IList<double> obstructions);
    }

    /// <summary>
    /// Represents a material that always has the same colour, regardless of light.
    /// </summary>
    public class ColourMaterial : IMaterial
    {
        /// <summary>
        /// The colour of the material.
        /// </summary>
        public Colour Colour { get; }

        /// <summary>
        /// Creates a new <see cref="ColourMaterial"/> instance.
        /// </summary>
        /// <param name="colour">The colour of the material.</param>
        public ColourMaterial(Colour colour)
        {
            this.Colour = colour;
        }

        /// <inheritdoc/>
        public Colour GetColour(Point3D point, NormalizedVector3D surfaceNormal, Camera camera, IList<ILightSource> lights, IList<double> obstructions)
        {
            return Colour;
        }
    }

    /// <summary>
    /// Represents a material that uses a Phong reflection model to determine the colour of the material based on the light sources that hit it.
    /// </summary>
    public class PhongMaterial : IMaterial
    {
        /// <summary>
        /// The base colour of the material.
        /// </summary>
        public Colour Colour { get; }

        private (double L, double a, double b) LabColour;
        private (double H, double S, double L) LabBlackHSL;
        private (double H, double S, double L) LabWhiteHSL;
        private double IntensityExponent;
        private double TotalLength;

        /// <summary>
        /// A coefficient determining how much ambient light is reflected by the material.
        /// </summary>
        public double AmbientReflectionCoefficient { get; set; } = 1;

        /// <summary>
        /// A coefficient determining how much directional light is reflected by the material.
        /// </summary>
        public double DiffuseReflectionCoefficient { get; set; } = 1;

        /// <summary>
        /// A coefficient determining the intensity of specular highlights.
        /// </summary>
        public double SpecularReflectionCoefficient { get; set; } = 1;

        /// <summary>
        /// A coefficient determining the extent of specular highlights.
        /// </summary>
        public double SpecularShininess { get; set; } = 1;

        /// <summary>
        /// Creates a new <see cref="PhongMaterial"/> instance.
        /// </summary>
        /// <param name="colour">The base colour of the material.</param>
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

        /// <inheritdoc/>
        public Colour GetColour(Point3D point, NormalizedVector3D surfaceNormal, Camera camera, IList<ILightSource> lights, IList<double> obstructions)
        {
            double intensity = 0;

            for (int i = 0; i < lights.Count; i++)
            {
                (double lightIntensity, NormalizedVector3D lightDirection) = lights[i].GetLightAt(point);
                lightIntensity *= 1 - obstructions[i];

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
