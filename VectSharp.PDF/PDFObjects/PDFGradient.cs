/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2024 Giorgio Bianchini

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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// Base class for PDF interpolation functions.
    /// </summary>
    public abstract class PDFInterpolationFunction : PDFDictionary
    {
        /// <summary>
        /// Type of interpolation function.
        /// </summary>
        public abstract PDFInt FunctionType { get; }

        /// <summary>
        /// Interpolation function domain.
        /// </summary>
        public virtual PDFArray<PDFDouble> Domain { get; } = new PDFArray<PDFDouble>(new PDFDouble(0), new PDFDouble(1));
    }

    /// <summary>
    /// PDF exponential interpolation function, interpolating between two values.
    /// </summary>
    public class PDFExponentialInterpolationFunction : PDFInterpolationFunction
    {
        /// <inheritdoc/>
        public override PDFInt FunctionType { get; } = new PDFInt(2);
        
        /// <summary>
        /// First value.
        /// </summary>
        public PDFArray<PDFDouble> C0 { get; }
        
        /// <summary>
        /// Second value.
        /// </summary>
        public PDFArray<PDFDouble> C1 { get; }
        
        /// <summary>
        /// Exponent (set to 1 for linear interpolation).
        /// </summary>
        public PDFDouble N { get; } = new PDFDouble(1);

        /// <summary>
        /// Create a new <see cref="PDFExponentialInterpolationFunction"/> interpolating between two colours.
        /// </summary>
        /// <param name="c0">The first colour.</param>
        /// <param name="c1">The second colour.</param>
        public PDFExponentialInterpolationFunction(Colour c0, Colour c1)
        {
            this.C0 = new PDFArray<PDFDouble>(new PDFDouble(c0.R), new PDFDouble(c0.G), new PDFDouble(c0.B));
            this.C1 = new PDFArray<PDFDouble>(new PDFDouble(c1.R), new PDFDouble(c1.G), new PDFDouble(c1.B));
        }
    }

    /// <summary>
    /// PDF stitching interpolation function.
    /// </summary>
    public class PDFStitchingFunction : PDFInterpolationFunction
    {
        /// <inheritdoc/>
        public override PDFInt FunctionType { get; } = new PDFInt(3);
        
        /// <summary>
        /// Stitched interpolation functions.
        /// </summary>
        public PDFArray<PDFInterpolationFunction> Functions { get; }

        /// <summary>
        /// Bounds of the stitched interpolation functions within the domain of the stitching function.
        /// </summary>
        public PDFArray<PDFDouble> Bounds { get; }

        /// <summary>
        /// Bounds of each stitched interpolation function.
        /// </summary>
        public PDFArray<PDFDouble> Encode { get; }

        /// <summary>
        /// Create a new <see cref="PDFStitchingFunction"/> that stitches the specified <paramref name="functions"/>.
        /// </summary>
        /// <param name="functions">Interpolation functions to be stitched.</param>
        /// <param name="bounds">Bounds of the stitched interpolation functions within the domain of the stitching function.</param>
        public PDFStitchingFunction(IEnumerable<PDFInterpolationFunction> functions, IEnumerable<double> bounds)
        {
            this.Functions = new PDFArray<PDFInterpolationFunction>(functions);
            this.Bounds = new PDFArray<PDFDouble>(bounds.Select(x => new PDFDouble(x)));
            this.Encode = new PDFArray<PDFDouble>(this.Functions.Values.SelectMany(x => new PDFDouble[] { new PDFDouble(0), new PDFDouble(1) }));
        }
    }

    /// <summary>
    /// Base clas for PDF gradient patterns.
    /// </summary>
    public abstract class PDFGradient : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("Pattern", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Pattern type.
        /// </summary>
        public PDFInt PatternType { get; } = new PDFInt(2);
        
        /// <summary>
        /// Gradient transformation matrix.
        /// </summary>
        public PDFArray<PDFDouble> Matrix { get; protected set; }
        
        /// <summary>
        /// Gradient shading.
        /// </summary>
        public PDFShading Shading { get; protected set; }
    }

    /// <summary>
    /// PDF shading dictionary. This associates brush coordinates to the gradient colours defined by a <see cref="PDFInterpolationFunction"/>.
    /// </summary>
    public class PDFShading : PDFDictionary
    {
        /// <summary>
        /// Types of shading.
        /// </summary>
        public enum PDFShadingType
        {
            /// <summary>
            /// Linear gradient.
            /// </summary>
            Linear = 2,

            /// <summary>
            /// Radial gradient.
            /// </summary>
            Radial = 3
        }

        /// <summary>
        /// Type of shading.
        /// </summary>
        public PDFInt ShadingType { get; }

        /// <summary>
        /// Colour space.
        /// </summary>
        public PDFString ColorSpace { get; } = new PDFString("DeviceRGB", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Shading coordinates.
        /// </summary>
        public PDFArray<PDFDouble> Coords { get; }
        
        /// <summary>
        /// Shading domain.
        /// </summary>
        public PDFArray<PDFDouble> Domain { get; } = new PDFArray<PDFDouble>(new PDFDouble(0), new PDFDouble(1));
        
        /// <summary>
        /// Determines whether the shading should be extended before and after the end of the <see cref="Domain"/>.
        /// </summary>
        public PDFArray<PDFBool> Extend { get; } = new PDFArray<PDFBool>(new PDFBool(true), new PDFBool(true));

        /// <summary>
        /// Gradient interpolation function.
        /// </summary>
        public PDFInterpolationFunction Function { get; protected set; }

        /// <summary>
        /// Create a new <see cref="PDFShading"/>.
        /// </summary>
        /// <param name="shadingType">The type of shading.</param>
        /// <param name="coords">The gradient coordinates.</param>
        /// <param name="gradientFunction">The gradient interpolation function.</param>
        public PDFShading(PDFShadingType shadingType, IEnumerable<double> coords, PDFInterpolationFunction gradientFunction)
        {
            this.ShadingType = new PDFInt((int)shadingType);
            this.Coords = new PDFArray<PDFDouble>(coords.Select(x => new PDFDouble(x)));
            this.Function = gradientFunction;
        }
    }

    /// <summary>
    /// Create a new linear gradient.
    /// </summary>
    public class PDFLinearGradient : PDFGradient
    {
        /// <summary>
        /// Create a new linear gradient.
        /// </summary>
        /// <param name="matrix">The gradient transformation matrix (or <see langword="null"/> to omit it from the gradient).</param>
        /// <param name="gradientFunction">The gradient interpolation function.</param>
        /// <param name="linear">The <see cref="LinearGradientBrush"/> from which this object is being created.</param>
        public PDFLinearGradient(double[,] matrix, PDFInterpolationFunction gradientFunction, LinearGradientBrush linear)
        {
            if (matrix != null)
            {
                this.Matrix = new PDFArray<PDFDouble>(new PDFDouble(matrix[0, 0]), new PDFDouble(matrix[1, 0]), new PDFDouble(matrix[0, 1]), new PDFDouble(matrix[1, 1]), new PDFDouble(matrix[0, 2]), new PDFDouble(matrix[1, 2]));
            }

            this.Shading = new PDFShading(PDFShading.PDFShadingType.Linear, new double[] { linear.StartPoint.X, linear.StartPoint.Y, linear.EndPoint.X, linear.EndPoint.Y }, gradientFunction);
        }
    }

    /// <summary>
    /// Create a new radial gradient.
    /// </summary>
    public class PDFRadialGradient : PDFGradient
    {
        /// <summary>
        /// Create a new radial gradient.
        /// </summary>
        /// <param name="matrix">The gradient transformation matrix (or <see langword="null"/> to omit it from the gradient).</param>
        /// <param name="gradientFunction">The gradient interpolation function.</param>
        /// <param name="radial">The <see cref="RadialGradientBrush"/> from which this object is being created.</param>
        public PDFRadialGradient(double[,] matrix, PDFInterpolationFunction gradientFunction, RadialGradientBrush radial)
        {
            if (matrix != null)
            {
                this.Matrix = new PDFArray<PDFDouble>(new PDFDouble(matrix[0, 0]), new PDFDouble(matrix[1, 0]), new PDFDouble(matrix[0, 1]), new PDFDouble(matrix[1, 1]), new PDFDouble(matrix[0, 2]), new PDFDouble(matrix[1, 2]));
            }

            this.Shading = new PDFShading(PDFShading.PDFShadingType.Radial, new double[] { radial.FocalPoint.X, radial.FocalPoint.Y, 0, radial.Centre.X, radial.Centre.Y, radial.Radius }, gradientFunction);
        }
    }

    /// <summary>
    /// PDF transparency group.
    /// </summary>
    public class PDFTransparencyGroup : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("Group", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Group type.
        /// </summary>
        public PDFString S { get; } = new PDFString("Transparency", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Specifies whether the group should be isolated.
        /// </summary>
        public PDFBool I { get; } = new PDFBool(true);

        /// <summary>
        /// Colour space.
        /// </summary>
        public PDFString CS { get; } = new PDFString("DeviceRGB", PDFString.StringDelimiter.StartingForwardSlash);
    }

    /// <summary>
    /// PDF gradient alpha mask XObject.
    /// </summary>
    public class PDFGradientAlphaMaskXObject : PDFStream
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("XObject", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// XObject subtype.
        /// </summary>
        public PDFString Subtype { get; } = new PDFString("Form", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// PDF transparency group.
        /// </summary>
        public PDFTransparencyGroup Group { get; } = new PDFTransparencyGroup();
        
        /// <summary>
        /// Mask bounding box.
        /// </summary>
        public PDFArray<PDFDouble> BBox { get; }
        
        /// <summary>
        /// Resource container containing a name for the alpha pattern.
        /// </summary>
        public PDFResources Resources { get; }

        private static MemoryStream GetContentStream(Rectangle bbox, string alphaPatternReferenceName)
        {
            MemoryStream contentStream = new MemoryStream();

            using (StreamWriter ctW = new StreamWriter(contentStream, Encoding.ASCII, 1024, true))
            {
                ctW.Write("q\n");
                ctW.Write(bbox.Location.X.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + bbox.Location.Y.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + bbox.Size.Width.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + bbox.Size.Height.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " re\n");
                ctW.Write("/Pattern cs\n");
                ctW.Write("/" + alphaPatternReferenceName + " scn\n");
                ctW.Write("f\n");
                ctW.Write("Q\n");
            }

            contentStream.Seek(0, SeekOrigin.Begin);
            return contentStream;
        }

        /// <summary>
        /// Create a new <see cref="PDFGradientAlphaMaskXObject"/>.
        /// </summary>
        /// <param name="bbox">The mask bounding box.</param>
        /// <param name="alphaPatternReferenceName">A name used within this object to refer to the alpha pattern.</param>
        /// <param name="alphaGradient">The gradient alpha.</param>
        /// <param name="compressStream">Determines whether the stream should be compressed.</param>
        public PDFGradientAlphaMaskXObject(Rectangle bbox, string alphaPatternReferenceName, PDFGradient alphaGradient, bool compressStream) : base(GetContentStream(bbox, alphaPatternReferenceName), compressStream)
        {
            this.Length1 = null;
            this.BBox = new PDFArray<PDFDouble>(new PDFDouble(bbox.Location.X), new PDFDouble(bbox.Location.Y), new PDFDouble(bbox.Location.X + bbox.Size.Width), new PDFDouble(bbox.Location.Y + bbox.Size.Height));
            this.Resources = new PDFResources(alphaPatternReferenceName, alphaGradient);
        }
    }

    /// <summary>
    /// A PDF luminosity mask.
    /// </summary>
    public class PDFLuminosityMask : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("Mask", PDFString.StringDelimiter.StartingForwardSlash);

        /// <summary>
        /// Mask subtype.
        /// </summary>
        public PDFString S { get; } = new PDFString("Luminosity", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Mask XObject.
        /// </summary>
        public PDFGradientAlphaMaskXObject G { get; }

        /// <summary>
        /// Create a new <see cref="PDFLuminosityMask"/> using the specified <paramref name="mask"/>.
        /// </summary>
        /// <param name="mask">The mask contents.</param>
        public PDFLuminosityMask(PDFGradientAlphaMaskXObject mask)
        {
            this.G = mask;
        }
    }

    /// <summary>
    /// PDF gradient alpha mask.
    /// </summary>
    public class PDFGradientAlphaMask : PDFDictionary
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public PDFString Type { get; } = new PDFString("ExtGState", PDFString.StringDelimiter.StartingForwardSlash);
        
        /// <summary>
        /// Soft mask.
        /// </summary>
        public PDFLuminosityMask SMask { get; }

        /// <summary>
        /// Create a new <see cref="PDFGradientAlphaMask"/> from the specified <paramref name="mask"/>.
        /// </summary>
        /// <param name="mask">The gradient alpha mask.</param>
        public PDFGradientAlphaMask(PDFGradientAlphaMaskXObject mask)
        {
            this.SMask = new PDFLuminosityMask(mask);
        }
    }
}
