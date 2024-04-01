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

namespace VectSharp.PDF
{
    internal class AssetTagManager
    {
        private readonly Random Random;
        private readonly HashSet<string> TagCache = new HashSet<string>();
        private int FontIndex = 1;
        private int PatternIndex = 1;
        private int ImageIndex = 1;

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

        private static PDFStream[] GeneratePageContentStreams(PDFContext[] pageContexts, Dictionary<(string fontFamilyName, bool isSymbolic), PDFFont> fontObjects, double[] alphas, Dictionary<string, PDFImage> imageObjects, bool compressStreams, List<PDFReferenceableObject> pdfObjects, out Dictionary<string, (int index, List<Rectangle> taggedRects)>[] taggedObjectRectsByPage, out List<(GradientBrush, double[,], IFigure)> gradients)
        {
            PDFStream[] pageContentStreams = new PDFStream[pageContexts.Length];

            taggedObjectRectsByPage = new Dictionary<string, (int index, List<Rectangle> taggedRects)>[pageContexts.Length];
            gradients = new List<(GradientBrush, double[,], IFigure)>();

            for (int pageInd = 0; pageInd < pageContexts.Length; pageInd++)
            {
                taggedObjectRectsByPage[pageInd] = new Dictionary<string, (int, List<Rectangle>)>();

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

                            if (!taggedObjectRectsByPage[pageInd].TryGetValue(pageContexts[pageInd]._figures[i].Tag, out (int index, List<Rectangle> taggedRects) rects))
                            {
                                rects = (taggedObjectRectsByPage[pageInd].Count, new List<Rectangle>());
                                taggedObjectRectsByPage[pageInd].Add(pageContexts[pageInd]._figures[i].Tag, rects);
                            }
                            rects.taggedRects.Add(boundingRect);
                        }
                        else if (isTransform)
                        {
                            PDFProcessFigure.MeasureFigure(pageContexts[pageInd]._figures[i], ref transformationMatrix, savedStates);
                        }

                        PDFProcessFigure.WriteFigure(pageContexts[pageInd]._figures[i], fontObjects, alphas, imageObjects, transformationMatrix, gradients, ctW);
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

        private static void GenerateLinkAnnotations(PDFPage[] pages, Dictionary<string, (int, List<Rectangle>)>[] taggedObjectRectsByPage, Dictionary<string, string> linkDestinations, List<PDFReferenceableObject> pdfObjects)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                foreach (KeyValuePair<string, (int, List<Rectangle>)> kvp in taggedObjectRectsByPage[i])
                {
                    if (linkDestinations.TryGetValue(kvp.Key, out string destination))
                    {
                        if (destination.StartsWith("#"))
                        {
                            for (int k = 0; k < taggedObjectRectsByPage.Length; k++)
                            {
                                if (taggedObjectRectsByPage[k].TryGetValue(destination.Substring(1), out (int index, List<Rectangle> taggedRects) target))
                                {
                                    for (int l = 0; l < kvp.Value.Item2.Count; l++)
                                    {
                                        PDFInternalLinkAnnotation annot = new PDFInternalLinkAnnotation(kvp.Value.Item2[l], pages[k], target.taggedRects[0].Location.X, target.taggedRects[0].Location.Y);
                                        pdfObjects.Add(annot);
                                        pages[i].AddAnnotation(annot);
                                    }

                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int l = 0; l < kvp.Value.Item2.Count; l++)
                            {
                                PDFExternalLinkAnnotation annot = new PDFExternalLinkAnnotation(kvp.Value.Item2[l], destination);
                                pdfObjects.Add(annot);
                                pages[i].AddAnnotation(annot);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Convert the document to a <see cref="PDFDocument"/> representation.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to convert.</param>
        /// <param name="textOption">Defines whether the used fonts should be included in the PDF document.</param>
        /// <param name="compressStreams">Indicates whether the streams in the PDF document should be compressed.</param>
        /// <param name="linkDestinations">A dictionary associating element tags to link targets. If this is provided, objects that have been drawn with a tag contained in the dictionary will become hyperlink to the destination specified in the dictionary. If the destination starts with a hash (#), it is interpreted as the tag of another object in the current document; otherwise, it is interpreted as an external URI.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        public static PDFDocument CreatePDFDocument(this Document document, TextOptions textOption = TextOptions.SubsetFonts, bool compressStreams = true, Dictionary<string, string> linkDestinations = null, FilterOption filterOption = default)
        {
            if (linkDestinations == null)
            {
                linkDestinations = new Dictionary<string, string>();
            }

            if (filterOption == null)
            {
                filterOption = FilterOption.Default;
            }

            PDFContext[] pageContexts = new PDFContext[document.Pages.Count];

            Dictionary<string, (FontFamily, HashSet<char>)> fontFamilies = new Dictionary<string, (FontFamily, HashSet<char>)>();
            Dictionary<string, RasterImage> allImages = new Dictionary<string, RasterImage>();
            HashSet<double> allAlphas = new HashSet<double>();

            for (int i = 0; i < document.Pages.Count; i++)
            {
                pageContexts[i] = new PDFContext(document.Pages[i].Width, document.Pages[i].Height, document.Pages[i].Background, fontFamilies, allImages, allAlphas, textOption == TextOptions.ConvertIntoPaths, filterOption);
                document.Pages[i].Graphics.CopyToIGraphicsContext(pageContexts[i]);
            }

            double[] alphas = allAlphas.ToArray();

            AssetTagManager tagManager = new AssetTagManager();
            List<PDFReferenceableObject> pdfObjects = new List<PDFReferenceableObject>();

            Dictionary<(string fontFamilyName, bool isSymbolic), PDFFont> fontObjects = GenerateFontObjects(fontFamilies, tagManager, pdfObjects, compressStreams, out PDFRawDictionary fontList);
            Dictionary<string, PDFImage> imageObjects = GenerateImageObjects(allImages, tagManager, pdfObjects, compressStreams);

            PDFStream[] pageContentStreams = GeneratePageContentStreams(pageContexts, fontObjects, alphas, imageObjects, compressStreams, pdfObjects, out Dictionary<string, (int index, List<Rectangle> taggedRects)>[] taggedObjectRectsByPage, out List<(GradientBrush, double[,], IFigure)> gradients);

            List<(PDFGradient, PDFGradientAlphaMask)> pdfGradients = GenerateGradients(gradients, tagManager, pdfObjects, compressStreams);

            PDFResources resources = new PDFResources(fontList, alphas, pdfGradients, imageObjects.Values);
            pdfObjects.Add(resources);

            PDFPage[] pages = new PDFPage[document.Pages.Count];

            for (int i = 0; i < document.Pages.Count; i++)
            {
                pages[i] = new PDFPage(document.Pages[i].Width, document.Pages[i].Height, resources, pageContentStreams[i]);
                pdfObjects.Add(pages[i]);
            }

            GenerateLinkAnnotations(pages, taggedObjectRectsByPage, linkDestinations, pdfObjects);

            PDFPages pdfPages = new PDFPages(pages);
            pdfObjects.Add(pdfPages);

            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].Parent = pdfPages;
            }

            PDFCatalog catalog = new PDFCatalog(pdfPages);
            pdfObjects.Add(catalog);

            return new PDFDocument(pdfObjects);
        }

        /// <summary>
        /// Save the document to a PDF file.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to save.</param>
        /// <param name="fileName">The full path to the file to save. If it exists, it will be overwritten.</param>
        /// <param name="textOption">Defines whether the used fonts should be included in the file.</param>
        /// <param name="compressStreams">Indicates whether the streams in the PDF file should be compressed.</param>
        /// <param name="linkDestinations">A dictionary associating element tags to link targets. If this is provided, objects that have been drawn with a tag contained in the dictionary will become hyperlink to the destination specified in the dictionary. If the destination starts with a hash (#), it is interpreted as the tag of another object in the current document; otherwise, it is interpreted as an external URI.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        public static void SaveAsPDF(this Document document, string fileName, TextOptions textOption = TextOptions.SubsetFonts, bool compressStreams = true, Dictionary<string, string> linkDestinations = null, FilterOption filterOption = default)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                document.SaveAsPDF(stream, textOption, compressStreams, linkDestinations, filterOption);
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
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        public static void SaveAsPDF(this Document document, Stream stream, TextOptions textOption = TextOptions.SubsetFonts, bool compressStreams = true, Dictionary<string, string> linkDestinations = null, FilterOption filterOption = default)
        {
            PDFDocument pdfDoc = document.CreatePDFDocument(textOption, compressStreams, linkDestinations, filterOption);
            pdfDoc.Write(stream);
        }
    }
}
