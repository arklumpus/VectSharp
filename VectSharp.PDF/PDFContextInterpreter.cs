/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2024 Giorgio Bianchini

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
using System.IO;
using System.Text;
using VectSharp.PDF.PDFObjects;
using System.Linq;
using VectSharp.PDF.Figures;
using VectSharp.PDF.OptionalContentGroups;

namespace VectSharp.PDF
{
    internal class AssetTagManager
    {
        private readonly Random Random;
        private readonly HashSet<string> TagCache = new HashSet<string>();
        private int FontIndex = 1;
        private int PatternIndex = 1;
        private int ImageIndex = 1;
        private int OptionalContentGroupMembershipIndex = 1;

        public AssetTagManager(Random random = null)
        {
            this.Random = random ?? new Random();
        }

        public string GetFontNameTag()
        {
            string randString = new string(Enumerable.Range(0, 6).Select(x => (char)Random.Next(65, 91)).ToArray());
            while (TagCache.Contains(randString))
            {
                randString = new string(Enumerable.Range(0, 6).Select(x => (char)Random.Next(65, 91)).ToArray());
            }

            return randString;
        }

        public string GetFontReferenceName()
        {
            string fontId = "F" + FontIndex.ToString();
            FontIndex++;
            return fontId;
        }

        public string GetGradientAlphaReferenceName()
        {
            string gradientAlphaId = "pa" + PatternIndex.ToString();
            PatternIndex++;
            return gradientAlphaId;
        }

        public string GetImageReferenceName()
        {
            string imageId = "Img" + ImageIndex.ToString();
            ImageIndex++;
            return imageId;
        }

        public string GetOptionalGroupMembershipReferenceName()
        {
            string ocId = "oc" + OptionalContentGroupMembershipIndex.ToString();
            OptionalContentGroupMembershipIndex++;
            return ocId;
        }
    }

    /// <summary>
    /// Contains methods to render a <see cref="Document"/> as a PDF document.
    /// </summary>
    public static class PDFContextInterpreter
    {
        /// <summary>
        /// Defines whether the used fonts should be included in the file.
        /// </summary>
        public enum TextOptions
        {
            /// <summary>
            /// Embeds subsetted font files containing only the glyphs for the characters that have been used.
            /// </summary>
            SubsetFonts,

            /// <summary>
            /// Does not embed any font file and converts all text items into paths.
            /// </summary>
            ConvertIntoPaths
        }

        /// <summary>
        /// Determines how and whether image filters are rasterised.
        /// </summary>
        public class FilterOption
        {
            /// <summary>
            /// Defines whether image filters should be rasterised or not.
            /// </summary>
            public enum FilterOperations
            {
                /// <summary>
                /// Image filters will always be rasterised.
                /// </summary>
                RasteriseAll,

                /// <summary>
                /// All image filters will be ignored.
                /// </summary>
                IgnoreAll,

                /// <summary>
                /// All the images that should be drawn with a filter will be ignored.
                /// </summary>
                SkipAll
            }

            /// <summary>
            /// Defines whether image filters should be rasterised or not.
            /// </summary>
            public FilterOperations Operation { get; } = FilterOperations.RasteriseAll;

            /// <summary>
            /// The resolution that will be used to rasterise image filters. Depending on the value of <see cref="RasterisationResolutionRelative"/>, this can either be an absolute resolution (i.e. a size in pixel), or a scale factor that is applied to the image size in graphics units.
            /// </summary>
            public double RasterisationResolution { get; } = 1;

            /// <summary>
            /// Determines whether the value of <see cref="RasterisationResolution"/> is absolute (i.e. a size in pixel), or relative (i.e. a scale factor that is applied to the image size in graphics units).
            /// </summary>
            public bool RasterisationResolutionRelative { get; } = true;

            /// <summary>
            /// The default options for image filter rasterisation.
            /// </summary>
            public static FilterOption Default = new FilterOption(FilterOperations.RasteriseAll, 1, true);

            /// <summary>
            /// Create a new <see cref="FilterOption"/> object.
            /// </summary>
            /// <param name="operation">Defines whether image filters should be rasterised or not.</param>
            /// <param name="rasterisationResolution">The resolution that will be used to rasterise image filters. Depending on the value of <see cref="RasterisationResolutionRelative"/>, this can either be an absolute resolution (i.e. a size in pixel), or a scale factor that is applied to the image size in graphics units.</param>
            /// <param name="rasterisationResolutionRelative">Determines whether the value of <see cref="RasterisationResolution"/> is absolute (i.e. a size in pixel), or relative (i.e. a scale factor that is applied to the image size in graphics units).</param>
            public FilterOption(FilterOperations operation, double rasterisationResolution, bool rasterisationResolutionRelative)
            {
                this.Operation = operation;
                this.RasterisationResolution = rasterisationResolution;
                this.RasterisationResolutionRelative = rasterisationResolutionRelative;
            }
        }

        internal static readonly char[] CP1252Chars = new char[] { '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', '\u0008', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F', '\u0020', '\u0021', '\u0022', '\u0023', '\u0024', '\u0025', '\u0026', '\u0027', '\u0028', '\u0029', '\u002A', '\u002B', '\u002C', '\u002D', '\u002E', '\u002F', '\u0030', '\u0031', '\u0032', '\u0033', '\u0034', '\u0035', '\u0036', '\u0037', '\u0038', '\u0039', '\u003A', '\u003B', '\u003C', '\u003D', '\u003E', '\u003F', '\u0040', '\u0041', '\u0042', '\u0043', '\u0044', '\u0045', '\u0046', '\u0047', '\u0048', '\u0049', '\u004A', '\u004B', '\u004C', '\u004D', '\u004E', '\u004F', '\u0050', '\u0051', '\u0052', '\u0053', '\u0054', '\u0055', '\u0056', '\u0057', '\u0058', '\u0059', '\u005A', '\u005B', '\u005C', '\u005D', '\u005E', '\u005F', '\u0060', '\u0061', '\u0062', '\u0063', '\u0064', '\u0065', '\u0066', '\u0067', '\u0068', '\u0069', '\u006A', '\u006B', '\u006C', '\u006D', '\u006E', '\u006F', '\u0070', '\u0071', '\u0072', '\u0073', '\u0074', '\u0075', '\u0076', '\u0077', '\u0078', '\u0079', '\u007A', '\u007B', '\u007C', '\u007D', '\u007E', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u00A0', '\u00A1', '\u00A2', '\u00A3', '\u0000', '\u00A5', '\u0000', '\u00A7', '\u0000', '\u0000', '\u0000', '\u00AB', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u00B5', '\u00B6', '\u0000', '\u0000', '\u0000', '\u0000', '\u00BB', '\u0000', '\u0000', '\u00BE', '\u00BF', '\u00C0', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u0000', '\u00C9', '\u0000', '\u0000', '\u00CC', '\u0000', '\u0000', '\u0000', '\u0000', '\u00D1', '\u00D2', '\u00D3', '\u00D4', '\u00D5', '\u00D6', '\u00D7', '\u00D8', '\u00D9', '\u00DA', '\u00DB', '\u00DC', '\u00DD', '\u00DE', '\u00DF', '\u00E0', '\u0000', '\u00E2', '\u0000', '\u00E4', '\u00E5', '\u00E6', '\u00E7', '\u0000', '\u0000', '\u0000', '\u0000', '\u00EC', '\u00ED', '\u00EE', '\u00EF', '\u00F0', '\u0000', '\u00F2', '\u00F3', '\u00F4', '\u0000', '\u00F6', '\u00F7', '\u0000', '\u0000', '\u0000', '\u0000', '\u00FC', '\u00FD', '\u00FE', '\u00FF' };

        private static Dictionary<(string fontFamilyName, bool isSymbolic), PDFFont> GenerateFontObjects(Dictionary<string, (FontFamily, HashSet<char>)> fontFamilies, AssetTagManager tagManager, List<PDFReferenceableObject> pdfObjects, bool compressStreams, out PDFRawDictionary fontList)
        {
            Dictionary<(string fontFamilyName, bool isSymbolic), PDFFont> fontObjects = new Dictionary<(string fontFamilyName, bool isSymbolic), PDFFont>();

            if (fontFamilies.Count > 0)
            {
                fontList = new PDFRawDictionary();
            }
            else
            {
                fontList = null;
            }

            foreach (KeyValuePair<string, (FontFamily, HashSet<char>)> fontFamily in fontFamilies)
            {
                List<char> nonSymbol = new List<char>();
                List<char> symbol = new List<char>();

                foreach (char c in fontFamily.Value.Item2)
                {
                    if (CP1252Chars.Contains(c))
                    {
                        nonSymbol.Add(c);
                    }
                    else
                    {
                        symbol.Add(c);
                    }
                }

                if ((fontFamily.Value.Item1.IsStandardFamily && fontFamily.Value.Item1.FileName != "Symbol" && fontFamily.Value.Item1.FileName != "ZapfDingbats" && symbol.Count == 0) || fontFamily.Value.Item1.TrueTypeFile == null)
                {
                    PDFType1Font font = new PDFType1Font(fontFamily.Value.Item1.IsStandardFamily ? fontFamily.Value.Item1.FileName.Replace(" ", "-") : fontFamily.Value.Item1.FamilyName, tagManager.GetFontReferenceName());
                    pdfObjects.Add(font);
                    fontObjects[(fontFamily.Value.Item1.FileName, true)] = font;
                    fontObjects[(fontFamily.Value.Item1.FileName, false)] = font;
                    fontList.Keys[font.FontReferenceName] = font;
                }
                else
                {
                    TrueTypeFile subsettedFont = fontFamily.Value.Item1.TrueTypeFile.SubsetFont(new string(fontFamily.Value.Item2.ToArray()));

                    PDFTTFStream ttfStream = new PDFTTFStream(subsettedFont, compressStreams);
                    pdfObjects.Add(ttfStream);

                    if (nonSymbol.Count > 0)
                    {
                        if (fontFamily.Value.Item1.IsStandardFamily && fontFamily.Value.Item1.FileName != "Symbol" && fontFamily.Value.Item1.FileName != "ZapfDingbats")
                        {
                            PDFType1Font font = new PDFType1Font(fontFamily.Value.Item1.IsStandardFamily ? fontFamily.Value.Item1.FileName.Replace(" ", "-") : fontFamily.Value.Item1.FamilyName, tagManager.GetFontReferenceName());
                            pdfObjects.Add(font);
                            fontObjects[(fontFamily.Value.Item1.FileName, true)] = font;
                            fontObjects[(fontFamily.Value.Item1.FileName, false)] = font;
                            fontList.Keys[font.FontReferenceName] = font;
                        }
                        else
                        {
                            PDFFontDescriptor fontDescriptor = new PDFFontDescriptor(subsettedFont, false, ttfStream, tagManager.GetFontNameTag());
                            pdfObjects.Add(fontDescriptor);

                            PDFTrueTypeFont font = new PDFTrueTypeFont(nonSymbol, fontDescriptor, subsettedFont, tagManager.GetFontReferenceName());
                            pdfObjects.Add(font);
                            fontObjects[(fontFamily.Value.Item1.FileName, false)] = font;
                            fontList.Keys[font.FontReferenceName] = font;
                        }
                    }


                    if (symbol.Count > 0)
                    {
                        PDFFontDescriptor fontDescriptor = new PDFFontDescriptor(subsettedFont, true, ttfStream, tagManager.GetFontNameTag());
                        pdfObjects.Add(fontDescriptor);

                        PDFCIDFontType2Font cidFont = new PDFCIDFontType2Font(symbol, fontDescriptor, subsettedFont, null);
                        pdfObjects.Add(cidFont);

                        PDFToUnicodeStream toUnicodeStream = new PDFToUnicodeStream(symbol, cidFont.GlyphIndices, compressStreams);
                        pdfObjects.Add(toUnicodeStream);

                        PDFType0Font font = new PDFType0Font(fontDescriptor, cidFont, toUnicodeStream, tagManager.GetFontReferenceName());
                        pdfObjects.Add(font);
                        fontObjects[(fontFamily.Value.Item1.FileName, true)] = font;
                        fontList.Keys[font.FontReferenceName] = font;
                    }

                    subsettedFont.Destroy();
                }
            }

            if (fontList != null)
            {
                pdfObjects.Add(fontList);
            }

            return fontObjects;
        }

        private static Dictionary<string, PDFImage> GenerateImageObjects(Dictionary<string, RasterImage> allImages, AssetTagManager tagManager, List<PDFReferenceableObject> pdfObjects, bool compressStreams)
        {
            Dictionary<string, PDFImage> imageObjects = new Dictionary<string, PDFImage>();

            foreach (KeyValuePair<string, RasterImage> kvp in allImages)
            {
                PDFImageAlpha imageAlpha = null;

                if (kvp.Value.HasAlpha)
                {
                    imageAlpha = new PDFImageAlpha(kvp.Value, compressStreams);
                    pdfObjects.Add(imageAlpha);
                }

                PDFImage image = new PDFImage(kvp.Value, imageAlpha, compressStreams, tagManager.GetImageReferenceName());
                pdfObjects.Add(image);

                imageObjects[kvp.Key] = image;
            }

            return imageObjects;
        }

        private static PDFStream[] GeneratePageContentStreams(PDFContext[] pageContexts, Dictionary<(string fontFamilyName, bool isSymbolic), PDFFont> fontObjects, double[] alphas, Dictionary<string, PDFImage> imageObjects, bool compressStreams, List<PDFReferenceableObject> pdfObjects, out Dictionary<string, List<Rectangle>>[] taggedObjectRectsByPage, out List<(GradientBrush, double[,], IFigure)> gradients, Dictionary<string, PDFOptionalContentGroupMembership> contentGroups)
        {
            PDFStream[] pageContentStreams = new PDFStream[pageContexts.Length];

            taggedObjectRectsByPage = new Dictionary<string, List<Rectangle>>[pageContexts.Length];
            gradients = new List<(GradientBrush, double[,], IFigure)>();

            for (int pageInd = 0; pageInd < pageContexts.Length; pageInd++)
            {
                taggedObjectRectsByPage[pageInd] = new Dictionary<string, List<Rectangle>>();

                double[,] transformationMatrix = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
                Stack<double[,]> savedStates = new Stack<double[,]>();

                MemoryStream contentStream = new MemoryStream();

                using (StreamWriter ctW = new StreamWriter(contentStream, Encoding.ASCII, 1024, true))
                {
                    for (int i = 0; i < pageContexts[pageInd]._figures.Count; i++)
                    {
                        bool isTransform = pageContexts[pageInd]._figures[i] is TransformFigure;

                        bool isClip = pageContexts[pageInd]._figures[i] is PathFigure pathFig && pathFig.IsClipping;

                        if (!string.IsNullOrEmpty(pageContexts[pageInd]._figures[i].Tag) && !isTransform && !isClip)
                        {
                            Rectangle boundingRect = PDFProcessFigure.MeasureFigure(pageContexts[pageInd]._figures[i], ref transformationMatrix, savedStates);

                            if (!taggedObjectRectsByPage[pageInd].TryGetValue(pageContexts[pageInd]._figures[i].Tag, out List<Rectangle> rects))
                            {
                                rects = new List<Rectangle>();
                                taggedObjectRectsByPage[pageInd].Add(pageContexts[pageInd]._figures[i].Tag, rects);
                            }
                            rects.Add(boundingRect);
                        }
                        else if (isTransform)
                        {
                            PDFProcessFigure.MeasureFigure(pageContexts[pageInd]._figures[i], ref transformationMatrix, savedStates);
                        }

                        PDFProcessFigure.WriteFigure(pageContexts[pageInd]._figures[i], fontObjects, alphas, imageObjects, transformationMatrix, gradients, contentGroups, ctW);
                    }
                }

                contentStream.Seek(0, SeekOrigin.Begin);

                PDFStream pageStream = new PDFStream(contentStream, compressStreams);
                pdfObjects.Add(pageStream);

                pageContentStreams[pageInd] = pageStream;
            }

            return pageContentStreams;
        }

        private static PDFGradient CreateGradient(bool includeMatrix, GradientBrush gradient, double[,] matrix, List<PDFReferenceableObject> pdfObjects)
        {
            PDFInterpolationFunction functionObject;

            if (gradient.GradientStops.Count == 2)
            {
                functionObject = new PDFExponentialInterpolationFunction(gradient.GradientStops[0].Colour, gradient.GradientStops[1].Colour);
                pdfObjects.Add(functionObject);
            }
            else
            {
                List<double> bounds = new List<double>();
                List<PDFExponentialInterpolationFunction> functions = new List<PDFExponentialInterpolationFunction>();

                for (int j = 0; j < gradient.GradientStops.Count - 1; j++)
                {
                    PDFExponentialInterpolationFunction function2 = new PDFExponentialInterpolationFunction(gradient.GradientStops[j].Colour, gradient.GradientStops[j + 1].Colour);
                    pdfObjects.Add(function2);
                    functions.Add(function2);

                    if (j < gradient.GradientStops.Count - 2)
                    {
                        bounds.Add(gradient.GradientStops[j + 1].Offset);
                    }
                }

                functionObject = new PDFStitchingFunction(functions, bounds);
                pdfObjects.Add(functionObject);
            }

            if (gradient is LinearGradientBrush linear)
            {
                PDFLinearGradient pdfLin = new PDFLinearGradient(includeMatrix ? matrix : null, functionObject, linear);
                pdfObjects.Add(pdfLin);
                return pdfLin;
            }
            else if (gradient is RadialGradientBrush radial)
            {
                PDFRadialGradient pdfRad = new PDFRadialGradient(includeMatrix ? matrix : null, functionObject, radial);
                pdfObjects.Add(pdfRad);
                return pdfRad;
            }
            else
            {
                throw new NotImplementedException("Unknown type of gradient!");
            }
        }

        private static List<(PDFGradient gradient, PDFGradientAlphaMask alphaMask)> GenerateGradients(List<(GradientBrush, double[,], IFigure)> gradients, AssetTagManager tagManager, List<PDFReferenceableObject> pdfObjects, bool compressStreams)
        {
            List<(PDFGradient gradient, PDFGradientAlphaMask alphaMask)> pdfGradients = new List<(PDFGradient gradient, PDFGradientAlphaMask alphaMask)>();

            for (int i = 0; i < gradients.Count; i++)
            {
                (GradientBrush gradient, double[,] matrix, IFigure figure) = gradients[i];

                PDFGradient pdfGradient = CreateGradient(true, gradient, matrix, pdfObjects);

                bool hasAlpha = gradient.GradientStops.Any(x => x.Colour.A != 1);

                if (hasAlpha)
                {
                    GradientBrush alphaGradient = (GradientBrush)PDFContext.GetAlphaBrush(gradient);

                    PDFGradient alphaPDFGradient = CreateGradient(false, alphaGradient, matrix, pdfObjects);

                    string alphaPatternReferenceName = tagManager.GetGradientAlphaReferenceName();

                    Rectangle bbox = figure.GetBounds();

                    PDFGradientAlphaMaskXObject gradientMaskForm = new PDFGradientAlphaMaskXObject(bbox, alphaPatternReferenceName, alphaPDFGradient, compressStreams);
                    pdfObjects.Add(gradientMaskForm);

                    PDFGradientAlphaMask gradientMask = new PDFGradientAlphaMask(gradientMaskForm);
                    pdfObjects.Add(gradientMask);

                    pdfGradients.Add((pdfGradient, gradientMask));
                }
                else
                {
                    pdfGradients.Add((pdfGradient, null));
                }
            }

            return pdfGradients;
        }


        private static (int page, Rectangle rect) GetTagRectangle(string tag, Dictionary<string, List<Rectangle>>[] taggedObjectRectsByPage)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                for (int k = 0; k < taggedObjectRectsByPage.Length; k++)
                {
                    if (taggedObjectRectsByPage[k].TryGetValue(tag, out List<Rectangle> target))
                    {
                        return (k, target[0]);
                    }
                }
            }

            return (-1, new Rectangle());
        }


        private static void GenerateLinkAnnotations(PDFPage[] pages, Dictionary<string, List<Rectangle>>[] taggedObjectRectsByPage, Dictionary<string, string> linkDestinations, Dictionary<string, PDFOptionalContentGroup> optionalContentGroups, Dictionary<string, OptionalContentGroupExpression> optionalContenGroupExpressions, Dictionary<string, PDFOptionalContentGroupMembership> optionalContentGroupMemberships, List<PDFReferenceableObject> pdfObjects, AnnotationStyleCollection annotationStyles)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                foreach (KeyValuePair<string, List<Rectangle>> kvp in taggedObjectRectsByPage[i])
                {
                    if (linkDestinations.TryGetValue(kvp.Key, out string destination))
                    {
                        if (destination.StartsWith("#"))
                        {
                            if (destination.StartsWith("#@OCG:"))
                            {
                                destination = destination.Substring(6);

                                string[] splitDestination = destination.Split(';');

                                List<PDFOptionalContentGroup> on = new List<PDFOptionalContentGroup>();
                                List<PDFOptionalContentGroup> off = new List<PDFOptionalContentGroup>();
                                List<PDFOptionalContentGroup> toggle = new List<PDFOptionalContentGroup>();

                                for (int j = 0; j < splitDestination.Length; j++)
                                {
                                    string[] splitSplitDestination = splitDestination[j].Split(',');

                                    switch (splitSplitDestination[1].ToLowerInvariant())
                                    {
                                        case "on":
                                            on.Add(optionalContentGroups[splitSplitDestination[0]]);
                                            break;
                                        case "off":
                                            off.Add(optionalContentGroups[splitSplitDestination[0]]);
                                            break;
                                        case "toggle":
                                            toggle.Add(optionalContentGroups[splitSplitDestination[0]]);
                                            break;
                                    }
                                }

                                if (!annotationStyles.TryGetValue(kvp.Key, out AnnotationStyle style))
                                {
                                    style = annotationStyles.DefaultStyle;
                                }

                                for (int l = 0; l < kvp.Value.Count; l++)
                                {
                                    PDFSetOCGStateActionAnnotation annot = new PDFSetOCGStateActionAnnotation(kvp.Value[l], new PDFSetOCGStateAction(on, off, toggle), style.BorderWidth, style.BorderDash, style.BorderColour);

                                    if (optionalContenGroupExpressions.TryGetValue(kvp.Key, out OptionalContentGroupExpression ocge))
                                    {
                                        annot.OC = optionalContentGroupMemberships[ocge.ToString()];
                                    }

                                    pdfObjects.Add(annot);
                                    pages[i].AddAnnotation(annot);
                                }
                            }
                            else
                            {
                                (int pageNum, Rectangle target) = GetTagRectangle(destination.Substring(1), taggedObjectRectsByPage);

                                if (pageNum >= 0)
                                {
                                    if (!annotationStyles.TryGetValue(kvp.Key, out AnnotationStyle style))
                                    {
                                        style = annotationStyles.DefaultStyle;
                                    }

                                    for (int l = 0; l < kvp.Value.Count; l++)
                                    {
                                        PDFInternalLinkAnnotation annot = new PDFInternalLinkAnnotation(kvp.Value[l], pages[pageNum], target.Location.X, target.Location.Y + target.Size.Height, style.BorderWidth, style.BorderDash, style.BorderColour);
                                        
                                        if (optionalContentGroupMemberships.TryGetValue(kvp.Key, out PDFOptionalContentGroupMembership ocgm))
                                        {
                                            annot.OC = ocgm;
                                        }

                                        pdfObjects.Add(annot);
                                        pages[i].AddAnnotation(annot);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!annotationStyles.TryGetValue(kvp.Key, out AnnotationStyle style))
                            {
                                style = annotationStyles.DefaultStyle;
                            }

                            for (int l = 0; l < kvp.Value.Count; l++)
                            {
                                PDFExternalLinkAnnotation annot = new PDFExternalLinkAnnotation(kvp.Value[l], destination, style.BorderWidth, style.BorderDash, style.BorderColour);

                                if (optionalContentGroupMemberships.TryGetValue(kvp.Key, out PDFOptionalContentGroupMembership ocgm))
                                {
                                    annot.OC = ocgm;
                                }

                                pdfObjects.Add(annot);
                                pages[i].AddAnnotation(annot);
                            }
                        }
                    }
                }
            }
        }

        private static PDFOutlineItem CreateOutlineItem(OutlineTreeNode outlineNode, PDFPage[] pages, Dictionary<string, List<Rectangle>>[] taggedObjectRectsByPage, List<PDFReferenceableObject> pdfObjects)
        {
            (int pageNum, Rectangle target) = GetTagRectangle(outlineNode.DestinationTag, taggedObjectRectsByPage);

            PDFOutlineItem item;

            if (pageNum >= 0)
            {
                item = new PDFOutlineItem(outlineNode.Title, pages[pageNum], target.Location.X, target.Location.Y + target.Size.Height, colour: outlineNode.Colour, bold: outlineNode.Bold, italic: outlineNode.Italic);
            }
            else
            {
                item = new PDFOutlineItem(outlineNode.Title, colour: outlineNode.Colour, bold: outlineNode.Bold, italic: outlineNode.Italic);
            }

            if (outlineNode.Children.Count > 0)
            {
                PDFOutlineItem[] children = new PDFOutlineItem[outlineNode.Children.Count];

                for (int i = 0; i < outlineNode.Children.Count; i++)
                {
                    children[i] = CreateOutlineItem(outlineNode.Children[i], pages, taggedObjectRectsByPage, pdfObjects);
                }

                for (int i = 0; i < children.Length; i++)
                {
                    if (i > 0)
                    {
                        children[i].Prev = children[i - 1];
                    }

                    if (i < children.Length - 1)
                    {
                        children[i].Next = children[i + 1];
                    }

                    children[i].Parent = item;
                }

                item.First = children[0];
                item.Last = children[children.Length - 1];
            }

            pdfObjects.Add(item);
            return item;
        }

        private static PDFOutline GenerateOutline(OutlineTree outline, PDFPage[] pages, Dictionary<string, List<Rectangle>>[] taggedObjectRectsByPage, List<PDFReferenceableObject> pdfObjects)
        {
            PDFOutlineItem[] topLevelItems = new PDFOutlineItem[outline.TopLevelItems.Count];

            for (int i = 0; i < outline.TopLevelItems.Count; i++)
            {
                topLevelItems[i] = CreateOutlineItem(outline.TopLevelItems[i], pages, taggedObjectRectsByPage, pdfObjects);
            }

            PDFOutline pdfOutline = new PDFOutline(topLevelItems[0], topLevelItems[topLevelItems.Length - 1]);
            pdfObjects.Add(pdfOutline);

            for (int i = 0; i < topLevelItems.Length; i++)
            {
                if (i > 0)
                {
                    topLevelItems[i].Prev = topLevelItems[i - 1];
                }

                if (i < topLevelItems.Length - 1)
                {
                    topLevelItems[i].Next = topLevelItems[i + 1];
                }

                topLevelItems[i].Parent = pdfOutline;
            }

            return pdfOutline;
        }

        private static void CreateOptionalContentGroups(OptionalContentGroupExpression expression, Dictionary<string, PDFOptionalContentGroup> pdfOcgs, List<PDFReferenceableObject> pdfObjects, Dictionary<string, OptionalContentGroup> ocgs)
        {
            if (expression is OptionalContentGroup group)
            {
                if (!pdfOcgs.ContainsKey(group.ToString()))
                {
                    ocgs[group.ToString()] = group;
                    PDFOptionalContentGroup ocg = new PDFOptionalContentGroup(group.Name, group.Intent, group.DefaultViewState, group.DefaultPrintState, group.DefaultExportState);
                    pdfOcgs[group.ToString()] = ocg;
                    pdfObjects.Add(ocg);
                }
            }
            else
            {
                foreach (OptionalContentGroupExpression expr in expression.Arguments)
                {
                    CreateOptionalContentGroups(expr, pdfOcgs, pdfObjects, ocgs);
                }
            }
        }

        private static IEnumerable<IPDFObject> CreateOCGOrder(OptionalContentGroupTreeNode node, Dictionary<string, PDFOptionalContentGroup> pdfOcgs)
        {
            if (node.Children.Count == 0)
            {
                if (node.Label.LabelType == OptionalContentGroupTreeLabel.Type.OptionalContentGroup)
                {
                    yield return pdfOcgs[node.Label.LabelOptionalContentGroup.ToString()];
                }
                else
                {
                    throw new ArgumentException("A leaf node must have an optional content group label and not a string label!");
                }
            }
            else
            {
                if (node.Label.LabelType == OptionalContentGroupTreeLabel.Type.String)
                {
                    yield return new PDFArray<IPDFObject>(new IPDFObject[] { new PDFString(node.Label.LabelString, PDFString.StringDelimiter.Brackets) }.Concat(node.Children.SelectMany(x => CreateOCGOrder(x, pdfOcgs))));
                }
                else if (node.Label.LabelType == OptionalContentGroupTreeLabel.Type.OptionalContentGroup)
                {
                    yield return pdfOcgs[node.Label.LabelOptionalContentGroup.ToString()];
                    yield return new PDFArray<IPDFObject>(node.Children.SelectMany(x => CreateOCGOrder(x, pdfOcgs)));
                }
            }
        }

        private static Dictionary<string, PDFOptionalContentGroupMembership> GenerateOptionalContentGroups(Dictionary<string, OptionalContentGroupExpression> expressions, OptionalContentGroupSettings groupSettings, IEnumerable<IEnumerable<OptionalContentGroup>> radioButtonGroups, AssetTagManager tagManager, List<PDFReferenceableObject> pdfObjects, out Dictionary<string, PDFOptionalContentGroup> contentGroups, out PDFOptionalContentProperties optionalContentProperties)
        {
            Dictionary<string, PDFOptionalContentGroupMembership> tbr = new Dictionary<string, PDFOptionalContentGroupMembership>();
            Dictionary<string, PDFOptionalContentGroup> pdfOcgs = new Dictionary<string, PDFOptionalContentGroup>();
            Dictionary<string, OptionalContentGroup> ocgs = new Dictionary<string, OptionalContentGroup>();

            foreach (KeyValuePair<string, OptionalContentGroupExpression> kvp in expressions)
            {
                CreateOptionalContentGroups(kvp.Value, pdfOcgs, pdfObjects, ocgs);

                if (!tbr.ContainsKey(kvp.Key))
                {
                    PDFOptionalContentGroupMembership ocgm = new PDFOptionalContentGroupMembership(kvp.Value, pdfOcgs, tagManager.GetOptionalGroupMembershipReferenceName());
                    tbr[kvp.Key] = ocgm;
                    pdfObjects.Add(ocgm);
                }
            }

            List<PDFUsageApplication> uas = new List<PDFUsageApplication>();

            PDFUsageApplication uaView = new PDFUsageApplication(PDFUsageApplication.UsageApplicationEvent.View, pdfOcgs.Values);
            if (uaView.OCGs.Values.Count > 0)
            {
                pdfObjects.Add(uaView);
                uas.Add(uaView);
            }

            PDFUsageApplication uaPrint = new PDFUsageApplication(PDFUsageApplication.UsageApplicationEvent.Print, pdfOcgs.Values);
            if (uaPrint.OCGs.Values.Count > 0)
            {
                pdfObjects.Add(uaPrint);
                uas.Add(uaPrint);
            }

            PDFUsageApplication uaExport = new PDFUsageApplication(PDFUsageApplication.UsageApplicationEvent.Export, pdfOcgs.Values);
            if (uaExport.OCGs.Values.Count > 0)
            {
                pdfObjects.Add(uaExport);
                uas.Add(uaExport);
            }

            IEnumerable<IPDFObject> order = pdfOcgs.Values;

            if (groupSettings?.OptionalContentGroupTree != null)
            {
                order = groupSettings.OptionalContentGroupTree.SelectMany(x => CreateOCGOrder(x, pdfOcgs));
            }

            PDFOptionalContentConfiguration defaultConfiguration = new PDFOptionalContentConfiguration(ocgs.Values.Where(x => !x.DefaultState).Select(x => pdfOcgs[x.ToString()]), uas, order, radioButtonGroups?.Select(x => x.Select(y => pdfOcgs[y.ToString()])), "Default");
            pdfObjects.Add(defaultConfiguration);

            optionalContentProperties = new PDFOptionalContentProperties(pdfOcgs.Values, defaultConfiguration);
            contentGroups = pdfOcgs;

            return tbr;

        }

        /// <summary>
        /// Convert the document to a <see cref="PDFDocument"/> representation.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to convert.</param>
        /// <param name="textOption">Defines whether the used fonts should be included in the PDF document.</param>
        /// <param name="compressStreams">Indicates whether the streams in the PDF document should be compressed.</param>
        /// <param name="linkDestinations">A dictionary associating element tags to link targets. If this is provided, objects that have been drawn with a tag contained in the dictionary will become hyperlink to the destination specified in the dictionary. If the destination starts with a hash (#), it is interpreted as the tag of another object in the current document; otherwise, it is interpreted as an external URI.</param>
        /// <param name="outline">Document outline (table of contents).</param>
        /// <param name="metadata">Document metadata. Use <see cref="PDFMetadata.NoMetadata()"/> if you do not wish to include metadata in the document.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <param name="optionalContentGroupSettings">Settings for optional content groups (layers).</param>
        /// <param name="annotationStyles">Annotation appearance styles.</param>
        public static PDFDocument CreatePDFDocument(this Document document, TextOptions textOption = TextOptions.SubsetFonts, bool compressStreams = true, Dictionary<string, string> linkDestinations = null, OutlineTree outline = null, PDFMetadata metadata = default, FilterOption filterOption = default, OptionalContentGroupSettings optionalContentGroupSettings = default, AnnotationStyleCollection annotationStyles = default)
        {
            if (linkDestinations == null)
            {
                linkDestinations = new Dictionary<string, string>();
            }

            if (filterOption == null)
            {
                filterOption = FilterOption.Default;
            }

            if (metadata == null)
            {
                metadata = new PDFMetadata();
            }

            if (optionalContentGroupSettings == null)
            {
                optionalContentGroupSettings = new OptionalContentGroupSettings();
            }

            if (annotationStyles == null)
            {
                annotationStyles = new AnnotationStyleCollection();
            }

            PDFContext[] pageContexts = new PDFContext[document.Pages.Count];

            Dictionary<string, (FontFamily, HashSet<char>)> fontFamilies = new Dictionary<string, (FontFamily, HashSet<char>)>();
            Dictionary<string, RasterImage> allImages = new Dictionary<string, RasterImage>();
            HashSet<double> allAlphas = new HashSet<double>() { 0, 1 };
            Dictionary<string, OptionalContentGroupExpression> allVisibilityExpressions = new Dictionary<string, OptionalContentGroupExpression>();

            for (int i = 0; i < document.Pages.Count; i++)
            {
                pageContexts[i] = new PDFContext(document.Pages[i].Width, document.Pages[i].Height, document.Pages[i].Background, fontFamilies, allImages, allAlphas, textOption == TextOptions.ConvertIntoPaths, filterOption, optionalContentGroupSettings.Groups, allVisibilityExpressions);
                document.Pages[i].Graphics.CopyToIGraphicsContext(pageContexts[i]);
                pageContexts[i].Finish();
            }

            double[] alphas = allAlphas.ToArray();

            AssetTagManager tagManager = new AssetTagManager();
            List<PDFReferenceableObject> pdfObjects = new List<PDFReferenceableObject>();

            Dictionary<(string fontFamilyName, bool isSymbolic), PDFFont> fontObjects = GenerateFontObjects(fontFamilies, tagManager, pdfObjects, compressStreams, out PDFRawDictionary fontList);
            Dictionary<string, PDFImage> imageObjects = GenerateImageObjects(allImages, tagManager, pdfObjects, compressStreams);

            PDFOptionalContentProperties optionalContentProperties = null;
            Dictionary<string, PDFOptionalContentGroupMembership> contentGroupMemberships = new Dictionary<string, PDFOptionalContentGroupMembership>();
            Dictionary<string, PDFOptionalContentGroup> contentGroups = new Dictionary<string, PDFOptionalContentGroup>();

            if (allVisibilityExpressions.Count > 0)
            {
                contentGroupMemberships = GenerateOptionalContentGroups(allVisibilityExpressions, optionalContentGroupSettings, optionalContentGroupSettings?.RadioButtonGroups, tagManager, pdfObjects, out contentGroups, out optionalContentProperties);
            }

            PDFStream[] pageContentStreams = GeneratePageContentStreams(pageContexts, fontObjects, alphas, imageObjects, compressStreams, pdfObjects, out Dictionary<string, List<Rectangle>>[] taggedObjectRectsByPage, out List<(GradientBrush, double[,], IFigure)> gradients, contentGroupMemberships);

            List<(PDFGradient, PDFGradientAlphaMask)> pdfGradients = GenerateGradients(gradients, tagManager, pdfObjects, compressStreams);

            PDFResources resources = new PDFResources(fontList, alphas, pdfGradients, imageObjects.Values, contentGroupMemberships);
            pdfObjects.Add(resources);

            PDFPage[] pages = new PDFPage[document.Pages.Count];

            for (int i = 0; i < document.Pages.Count; i++)
            {
                pages[i] = new PDFPage(document.Pages[i].Width, document.Pages[i].Height, resources, pageContentStreams[i]);
                pdfObjects.Add(pages[i]);
            }

            GenerateLinkAnnotations(pages, taggedObjectRectsByPage, linkDestinations, contentGroups, optionalContentGroupSettings.Groups, contentGroupMemberships, pdfObjects, annotationStyles);

            PDFPages pdfPages = new PDFPages(pages);
            pdfObjects.Add(pdfPages);

            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].Parent = pdfPages;
            }

            PDFCatalog catalog = new PDFCatalog(pdfPages);
            pdfObjects.Add(catalog);

            if (outline != null)
            {
                PDFOutline pdfOutline = GenerateOutline(outline, pages, taggedObjectRectsByPage, pdfObjects);
                catalog.Outlines = pdfOutline;
                catalog.PageMode = new PDFString("UseOutlines", PDFString.StringDelimiter.StartingForwardSlash);
            }

            PDFDocument doc = new PDFDocument(pdfObjects);

            if (optionalContentProperties != null)
            {
                catalog.OCProperties = optionalContentProperties;
                doc.PDFVersion = "1.6";
            }

            PDFDocumentInfo info = metadata.ToPDFDocumentInfo();

            if (info != null)
            {
                doc.Info = info;

                if (info.Title != null)
                {
                    catalog.ViewerPreferences = new PDFRawDictionary();
                    catalog.ViewerPreferences.Keys["DisplayDocTitle"] = new PDFBool(true);
                }
            }

            return doc;
        }

        /// <summary>
        /// Save the document to a PDF file.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to save.</param>
        /// <param name="fileName">The full path to the file to save. If it exists, it will be overwritten.</param>
        /// <param name="textOption">Defines whether the used fonts should be included in the file.</param>
        /// <param name="compressStreams">Indicates whether the streams in the PDF file should be compressed.</param>
        /// <param name="linkDestinations">A dictionary associating element tags to link targets. If this is provided, objects that have been drawn with a tag contained in the dictionary will become hyperlink to the destination specified in the dictionary. If the destination starts with a hash (#), it is interpreted as the tag of another object in the current document; otherwise, it is interpreted as an external URI.</param>
        /// <param name="outline">Document outline (table of contents).</param>
        /// <param name="metadata">Document metadata. Use <see cref="PDFMetadata.NoMetadata()"/> if you do not wish to include metadata in the document.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <param name="optionalContentGroupSettings">Settings for optional content groups (layers).</param>
        /// <param name="annotationStyles">Annotation appearance styles.</param>
        public static void SaveAsPDF(this Document document, string fileName, TextOptions textOption = TextOptions.SubsetFonts, bool compressStreams = true, Dictionary<string, string> linkDestinations = null, OutlineTree outline = null, PDFMetadata metadata = default, FilterOption filterOption = default, OptionalContentGroupSettings optionalContentGroupSettings = default, AnnotationStyleCollection annotationStyles = default)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                document.SaveAsPDF(stream, textOption, compressStreams, linkDestinations, outline, metadata, filterOption, optionalContentGroupSettings, annotationStyles);
            }
        }

        /// <summary>
        /// Save the document to a PDF stream.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to save.</param>
        /// <param name="stream">The stream to which the PDF data will be written.</param>
        /// <param name="textOption">Defines whether the used fonts should be included in the file.</param>
        /// <param name="compressStreams">Indicates whether the streams in the PDF file should be compressed.</param>
        /// <param name="linkDestinations">A dictionary associating element tags to link targets. If this is provided, objects that have been drawn with a tag contained in the dictionary will become hyperlink to the destination specified in the dictionary. If the destination starts with a hash (#), it is interpreted as the tag of another object in the current document; otherwise, it is interpreted as an external URI.</param>
        /// <param name="outline">Document outline (table of contents).</param>
        /// <param name="metadata">Document metadata. Use <see cref="PDFMetadata.NoMetadata()"/> if you do not wish to include metadata in the document.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <param name="optionalContentGroupSettings">Settings for optional content groups (layers).</param>
        /// <param name="annotationStyles">Annotation appearance styles.</param>
        public static void SaveAsPDF(this Document document, Stream stream, TextOptions textOption = TextOptions.SubsetFonts, bool compressStreams = true, Dictionary<string, string> linkDestinations = null, OutlineTree outline = null, PDFMetadata metadata = default, FilterOption filterOption = default, OptionalContentGroupSettings optionalContentGroupSettings = default, AnnotationStyleCollection annotationStyles = default)
        {
            PDFDocument pdfDoc = document.CreatePDFDocument(textOption, compressStreams, linkDestinations, outline, metadata, filterOption, optionalContentGroupSettings, annotationStyles);
            pdfDoc.Write(stream);
        }
    }
}
