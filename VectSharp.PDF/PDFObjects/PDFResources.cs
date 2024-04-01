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
using System.Linq;

namespace VectSharp.PDF.PDFObjects
{
    /// <summary>
    /// PDF alpha value.
    /// </summary>
    public class PDFAlphaState : PDFDictionary
    {
        /// <summary>
        /// Stroke alpha.
        /// </summary>
        public PDFDouble CA { get; }

        /// <summary>
        /// Fill alpha.
        /// </summary>
        public PDFDouble ca { get; }

        /// <summary>
        /// Create a new <see cref="PDFAlphaState"/>.
        /// </summary>
        /// <param name="alpha">Alpha value (range: 0 - 1).</param>
        public PDFAlphaState(double alpha)
        {
            this.CA = new PDFDouble(alpha);
            this.ca = new PDFDouble(alpha);
        }
    }

    /// <summary>
    /// PDF resource container.
    /// </summary>
    public class PDFResources : PDFDictionary
    {
        /// <summary>
        /// Font list dictionary.
        /// </summary>
        public PDFRawDictionary Font { get; }

        /// <summary>
        /// External graphics states dictionary.
        /// </summary>
        public PDFRawDictionary ExtGState { get; }

        /// <summary>
        /// XObject list dictionary.
        /// </summary>
        public PDFRawDictionary XObject { get; }

        /// <summary>
        /// Pattern list dictionary.
        /// </summary>
        public PDFRawDictionary Pattern { get; }

        /// <summary>
        /// Create a new <see cref="PDFResources"/> container.
        /// </summary>
        /// <param name="font">Font list dictionary.</param>
        /// <param name="alphas">Alpha values used in the document.</param>
        /// <param name="gradients">Gradients used in the document.</param>
        /// <param name="images">Images used in the document.</param>
        public PDFResources(PDFRawDictionary font, double[] alphas, List<(PDFGradient gradient, PDFGradientAlphaMask alphaMask)> gradients, IEnumerable<PDFImage> images)
        {
            this.Font = font;

            if (alphas.Length > 0 || gradients.Any(x => x.alphaMask != null))
            {
                this.ExtGState = new PDFRawDictionary();

                for (int i = 0; i < alphas.Length; i++)
                {
                    this.ExtGState.Keys["a" + i.ToString()] = new PDFAlphaState(alphas[i]);
                }

                for (int i = 0; i < gradients.Count; i++)
                {
                    if (gradients[i].alphaMask != null)
                    {
                        this.ExtGState.Keys["ma" + i.ToString()] = gradients[i].alphaMask;
                    }
                }
            }
            else
            {
                this.ExtGState = null;
            }

            bool anyImages = false;

            foreach (PDFImage image in images)
            {
                if (!anyImages)
                {
                    this.XObject = new PDFRawDictionary();
                    anyImages = true;
                }

                this.XObject.Keys[image.ReferenceName] = image;
            }

            if (!anyImages)
            {
                this.XObject = null;
            }

            if (gradients.Count > 0)
            {
                this.Pattern = new PDFRawDictionary();
                for (int i = 0; i < gradients.Count; i++)
                {
                    this.Pattern.Keys["p" + i.ToString()] = gradients[i].gradient;
                }
            }
            else
            {
                this.Pattern = null;
            }
        }

        /// <summary>
        /// Create a new <see cref="PDFResources"/> container containing an alpha gradient.
        /// </summary>
        /// <param name="alphaPatternReferenceName">The used to refer to the gradient within content streams.</param>
        /// <param name="alphaGradient">The alpha gradient.</param>
        public PDFResources(string alphaPatternReferenceName, PDFGradient alphaGradient)
        {
            this.Font = null;
            this.ExtGState = null;
            this.XObject = null;

            this.Pattern = new PDFRawDictionary();
            this.Pattern.Keys[alphaPatternReferenceName] = alphaGradient;
        }
    }
}
