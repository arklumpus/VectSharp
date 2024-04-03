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
using System.Linq;
using System.Text;
using VectSharp.PDF.Figures;
using VectSharp.PDF.PDFObjects;

namespace VectSharp.PDF
{
    internal static class PDFProcessFigure
    {
        public static Rectangle MeasureFigure(IFigure figure, ref double[,] transformationMatrix, Stack<double[,]> savedStates)
        {
            if (figure is PathFigure)
            {
                PathFigure fig = figure as PathFigure;

                double minX = double.MaxValue;
                double maxX = double.MinValue;
                double minY = double.MaxValue;
                double maxY = double.MinValue;

                for (int i = 0; i < fig.Segments.Length; i++)
                {
                    switch (fig.Segments[i].Type)
                    {
                        case Figures.SegmentType.Move:
                            {
                                Point pt = transformationMatrix.Multiply(fig.Segments[i].Point);
                                minX = Math.Min(minX, pt.X);
                                minY = Math.Min(minY, pt.Y);
                                maxX = Math.Max(maxX, pt.X);
                                maxY = Math.Max(maxY, pt.Y);
                            }
                            break;
                        case Figures.SegmentType.Line:
                            {
                                Point pt = transformationMatrix.Multiply(fig.Segments[i].Point);
                                minX = Math.Min(minX, pt.X);
                                minY = Math.Min(minY, pt.Y);
                                maxX = Math.Max(maxX, pt.X);
                                maxY = Math.Max(maxY, pt.Y);
                            }
                            break;
                        case Figures.SegmentType.CubicBezier:
                            for (int j = 0; j < fig.Segments[i].Points.Length; j++)
                            {
                                Point pt = transformationMatrix.Multiply(fig.Segments[i].Points[j]);
                                minX = Math.Min(minX, pt.X);
                                minY = Math.Min(minY, pt.Y);
                                maxX = Math.Max(maxX, pt.X);
                                maxY = Math.Max(maxY, pt.Y);
                            }
                            break;
                        case Figures.SegmentType.Close:
                            break;
                    }
                }

                return new Rectangle(minX, minY, maxX - minX, maxY - minY);
            }
            else if (figure is TextFigure)
            {
                TextFigure fig = figure as TextFigure;

                double yMax = 0;
                double yMin = 0;

                if (fig.Font.FontFamily.TrueTypeFile != null)
                {
                    for (int i = 0; i < fig.Text.Length; i++)
                    {
                        TrueTypeFile.VerticalMetrics vMet = fig.Font.FontFamily.TrueTypeFile.Get1000EmGlyphVerticalMetrics(fig.Text[i]);
                        yMin = Math.Min(yMin, vMet.YMin * fig.Font.FontSize / 1000);
                        yMax = Math.Max(yMax, vMet.YMax * fig.Font.FontSize / 1000);
                    }
                }

                double realY = fig.Position.Y;

                if (fig.TextBaseline == TextBaselines.Bottom)
                {
                    realY -= yMax;
                }
                else if (fig.TextBaseline == TextBaselines.Top)
                {
                    realY -= yMin;
                }
                else if (fig.TextBaseline == TextBaselines.Middle)
                {
                    realY -= (yMax + yMin) * 0.5;
                }
                else if (fig.TextBaseline == TextBaselines.Baseline)
                {
                    realY -= yMax + yMin;
                }

                Font.DetailedFontMetrics metrics = fig.Font.MeasureTextAdvanced(fig.Text);

                Point corner1 = new Point(fig.Position.X - metrics.LeftSideBearing, realY + metrics.Top);
                Point corner2 = new Point(fig.Position.X + metrics.Width, realY + metrics.Top);
                Point corner3 = new Point(fig.Position.X + metrics.Width, realY + metrics.Bottom);
                Point corner4 = new Point(fig.Position.X - metrics.LeftSideBearing, realY + metrics.Bottom);

                corner1 = transformationMatrix.Multiply(corner1);
                corner2 = transformationMatrix.Multiply(corner2);
                corner3 = transformationMatrix.Multiply(corner3);
                corner4 = transformationMatrix.Multiply(corner4);

                double x = Math.Min(corner1.X, Math.Min(corner2.X, Math.Min(corner3.X, corner4.X)));
                double y = Math.Min(corner1.Y, Math.Min(corner2.Y, Math.Min(corner3.Y, corner4.Y)));
                double w = Math.Max(corner1.X, Math.Max(corner2.X, Math.Max(corner3.X, corner4.X))) - x;
                double h = Math.Max(corner1.Y, Math.Max(corner2.Y, Math.Max(corner3.Y, corner4.Y))) - y;

                return new Rectangle(x, y, w, h);
            }
            else if (figure is TransformFigure transf)
            {
                if (transf.TransformType == TransformFigure.TransformTypes.Transform)
                {
                    transformationMatrix = transformationMatrix.Multiply(transf.TransformationMatrix);
                }
                else if (transf.TransformType == TransformFigure.TransformTypes.Save)
                {
                    savedStates.Push(transformationMatrix);
                }
                else if (transf.TransformType == TransformFigure.TransformTypes.Restore)
                {
                    transformationMatrix = savedStates.Pop();
                }

                return new Rectangle(0, 0, 0, 0);
            }
            else if (figure is RasterImageFigure)
            {
                Point corner1 = new Point(0, 0);
                Point corner2 = new Point(0, 1);
                Point corner3 = new Point(1, 1);
                Point corner4 = new Point(1, 0);

                corner1 = transformationMatrix.Multiply(corner1);
                corner2 = transformationMatrix.Multiply(corner2);
                corner3 = transformationMatrix.Multiply(corner3);
                corner4 = transformationMatrix.Multiply(corner4);

                double x = Math.Min(corner1.X, Math.Min(corner2.X, Math.Min(corner3.X, corner4.X)));
                double y = Math.Min(corner1.Y, Math.Min(corner2.Y, Math.Min(corner3.Y, corner4.Y)));
                double w = Math.Max(corner1.X, Math.Max(corner2.X, Math.Max(corner3.X, corner4.X))) - x;
                double h = Math.Max(corner1.Y, Math.Max(corner2.Y, Math.Max(corner3.Y, corner4.Y))) - y;

                return new Rectangle(x, y, w, h);
            }
            else
            {
                return new Rectangle(0, 0, 0, 0);
            }
        }

        private static double[,] Multiply(this double[,] matrix, double[,] matrix2)
        {
            double[,] tbr = new double[3, 3];

            tbr[0, 0] = matrix[0, 0] * matrix2[0, 0] - matrix[0, 1] * matrix2[1, 0] + matrix[0, 2] * matrix2[2, 0];
            tbr[0, 1] = -matrix[0, 0] * matrix2[0, 1] + matrix[0, 1] * matrix2[1, 1] + matrix[0, 2] * matrix2[2, 1];
            tbr[0, 2] = matrix[0, 0] * matrix2[0, 2] + matrix[0, 1] * matrix2[1, 2] + matrix[0, 2] * matrix2[2, 2];

            tbr[1, 0] = matrix[1, 0] * matrix2[0, 0] - matrix[1, 1] * matrix2[1, 0] + matrix[1, 2] * matrix2[2, 0];
            tbr[1, 1] = -matrix[1, 0] * matrix2[0, 1] + matrix[1, 1] * matrix2[1, 1] + matrix[1, 2] * matrix2[2, 1];
            tbr[1, 2] = matrix[1, 0] * matrix2[0, 2] + matrix[1, 1] * matrix2[1, 2] + matrix[1, 2] * matrix2[2, 2];

            tbr[2, 0] = matrix[2, 0] * matrix2[0, 0] - matrix[2, 1] * matrix2[1, 0] + matrix[2, 2] * matrix2[2, 0];
            tbr[2, 1] = -matrix[2, 0] * matrix2[0, 1] + matrix[2, 1] * matrix2[1, 1] + matrix[2, 2] * matrix2[2, 1];
            tbr[2, 2] = matrix[2, 0] * matrix2[0, 2] + matrix[2, 1] * matrix2[1, 2] + matrix[2, 2] * matrix2[2, 2];

            return tbr;
        }

        private static Point Multiply(this double[,] matrix, Point point)
        {
            double[] transPt = new double[3];

            transPt[0] = matrix[0, 0] * point.X + matrix[0, 1] * point.Y + matrix[0, 2];
            transPt[1] = matrix[1, 0] * point.X + matrix[1, 1] * point.Y + matrix[1, 2];
            transPt[2] = matrix[2, 0] * point.X + matrix[2, 1] * point.Y + matrix[2, 2];

            return new Point(transPt[0] / transPt[2], transPt[1] / transPt[2]);
        }

        private static string GetKernedString(string text, Font font)
        {
            List<(string, Point)> tSpans = new List<(string, Point)>();

            StringBuilder currentRun = new StringBuilder();
            Point currentKerning = new Point();

            Point currentGlyphPlacementDelta = new Point();
            Point currentGlyphAdvanceDelta = new Point();
            Point nextGlyphPlacementDelta = new Point();
            Point nextGlyphAdvanceDelta = new Point();

            for (int i = 0; i < text.Length; i++)
            {
                if (i < text.Length - 1)
                {
                    currentGlyphPlacementDelta = nextGlyphPlacementDelta;
                    currentGlyphAdvanceDelta = nextGlyphAdvanceDelta;
                    nextGlyphAdvanceDelta = new Point();
                    nextGlyphPlacementDelta = new Point();

                    TrueTypeFile.PairKerning kerning = font.FontFamily.TrueTypeFile.Get1000EmKerning(text[i], text[i + 1]);

                    if (kerning != null)
                    {
                        currentGlyphPlacementDelta = new Point(currentGlyphPlacementDelta.X + kerning.Glyph1Placement.X, currentGlyphPlacementDelta.Y + kerning.Glyph1Placement.Y);
                        currentGlyphAdvanceDelta = new Point(currentGlyphAdvanceDelta.X + kerning.Glyph1Advance.X, currentGlyphAdvanceDelta.Y + kerning.Glyph1Advance.Y);

                        nextGlyphPlacementDelta = new Point(nextGlyphPlacementDelta.X + kerning.Glyph2Placement.X, nextGlyphPlacementDelta.Y + kerning.Glyph2Placement.Y);
                        nextGlyphAdvanceDelta = new Point(nextGlyphAdvanceDelta.X + kerning.Glyph2Advance.X, nextGlyphAdvanceDelta.Y + kerning.Glyph2Advance.Y);
                    }
                }

                if (currentGlyphPlacementDelta.X != 0 || currentGlyphPlacementDelta.Y != 0 || currentGlyphAdvanceDelta.X != 0 || currentGlyphAdvanceDelta.Y != 0)
                {
                    if (currentRun.Length > 0)
                    {
                        tSpans.Add((currentRun.ToString(), currentKerning));

                        tSpans.Add((text[i].ToString(), new Point(currentGlyphPlacementDelta.X, currentGlyphPlacementDelta.Y)));

                        currentRun.Clear();
                        currentKerning = new Point(currentGlyphAdvanceDelta.X - currentGlyphPlacementDelta.X, currentGlyphAdvanceDelta.Y - currentGlyphPlacementDelta.Y);
                    }
                    else
                    {
                        tSpans.Add((text[i].ToString(), new Point(currentGlyphPlacementDelta.X + currentKerning.X, currentGlyphPlacementDelta.Y + currentKerning.Y)));

                        currentRun.Clear();
                        currentKerning = new Point(currentGlyphAdvanceDelta.X - currentGlyphPlacementDelta.X, currentGlyphAdvanceDelta.Y - currentGlyphPlacementDelta.Y);
                    }
                }
                else
                {
                    currentRun.Append(text[i]);
                }
            }

            if (currentRun.Length > 0)
            {
                tSpans.Add((currentRun.ToString(), currentKerning));
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            for (int i = 0; i < tSpans.Count; i++)
            {
                if (tSpans[i].Item2.X != 0)
                {
                    sb.Append((-tSpans[i].Item2.X).ToString("0.################", System.Globalization.CultureInfo.InvariantCulture));
                }

                sb.Append("(");
                sb.Append(PDFString.EscapeStringForPDF(tSpans[i].Item1));
                sb.Append(")");
            }

            sb.Append("]");

            return sb.ToString();
        }

        private static string EscapeSymbolStringForPDF(string str, Dictionary<char, int> glyphIndices)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                sb.Append((glyphIndices[str[i]]).ToString("X4"));
            }
            return sb.ToString();
        }

        public static void WriteFigure(IFigure figure, Dictionary<(string fontFamilyName, bool isSymbolic), PDFFont> fonts, double[] alphas, Dictionary<string, PDFImage> imageObjects, double[,] transformationMatrix, List<(GradientBrush, double[,], IFigure)> gradients, Dictionary<string, PDFOptionalContentGroupMembership> optionalContentGroups, StreamWriter sw)
        {
            bool restoreAtEnd = false;

            if (figure.Fill != null)
            {
                if (figure.Fill is SolidColourBrush solid)
                {
                    sw.Write(solid.R.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + solid.G.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + solid.B.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " rg\n");
                    sw.Write("/a" + Array.IndexOf(alphas, solid.A).ToString() + " gs\n");
                }
                else if (figure.Fill is GradientBrush gradient)
                {
                    int brushIndex = gradients.Count;
                    double[,] clonedMatrix = new double[3, 3] { { transformationMatrix[0, 0], transformationMatrix[0, 1], transformationMatrix[0, 2] }, { transformationMatrix[1, 0], transformationMatrix[1, 1], transformationMatrix[1, 2] }, { transformationMatrix[2, 0], transformationMatrix[2, 1], transformationMatrix[2, 2] } };

                    gradients.Add((gradient, clonedMatrix, figure));

                    bool gradientCompatible = PDFContext.IsCompatible(gradient);

                    if (!gradientCompatible)
                    {
                        sw.Write("q\n");
                        sw.Write("/ma" + brushIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + " gs ");
                        restoreAtEnd = true;
                    }

                    sw.Write("/Pattern cs /p" + brushIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + " scn /a" + Array.IndexOf(alphas, 1.0).ToString() + " gs\n");
                }
            }

            if (figure.Stroke != null)
            {
                if (figure.Stroke is SolidColourBrush solid)
                {
                    sw.Write(solid.R.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + solid.G.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + solid.B.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " RG\n");
                    sw.Write("/a" + Array.IndexOf(alphas, solid.A).ToString() + " gs\n");
                }
                else if (figure.Stroke is GradientBrush gradient)
                {
                    int brushIndex = gradients.Count;
                    double[,] clonedMatrix = new double[3, 3] { { transformationMatrix[0, 0], transformationMatrix[0, 1], transformationMatrix[0, 2] }, { transformationMatrix[1, 0], transformationMatrix[1, 1], transformationMatrix[1, 2] }, { transformationMatrix[2, 0], transformationMatrix[2, 1], transformationMatrix[2, 2] } };

                    gradients.Add((gradient, clonedMatrix, figure));

                    bool gradientCompatible = PDFContext.IsCompatible(gradient);

                    if (!gradientCompatible)
                    {
                        sw.Write("q\n");
                        sw.Write("/ma" + brushIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + " gs ");
                        restoreAtEnd = true;
                    }

                    sw.Write("/Pattern CS /p" + brushIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + " SCN /a" + Array.IndexOf(alphas, 1.0).ToString() + " gs\n");
                }

                sw.Write(figure.LineWidth.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " w\n");
                sw.Write(((int)figure.LineCap).ToString() + " J\n");
                sw.Write(((int)figure.LineJoin).ToString() + " j\n");
                if (figure.LineDash.UnitsOff != 0 || figure.LineDash.UnitsOn != 0)
                {
                    sw.Write("[ " + figure.LineDash.UnitsOn.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + figure.LineDash.UnitsOff.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ] " + figure.LineDash.Phase.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " d\n");
                }
                else
                {
                    sw.Write("[] 0 d\n");
                }
            }

            if (figure is PathFigure)
            {
                PathFigure fig = figure as PathFigure;

                for (int i = 0; i < fig.Segments.Length; i++)
                {
                    switch (fig.Segments[i].Type)
                    {
                        case Figures.SegmentType.Move:
                            {
                                sw.Write(fig.Segments[i].Point.X.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + fig.Segments[i].Point.Y.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " m ");
                            }
                            break;
                        case Figures.SegmentType.Line:
                            {
                                sw.Write(fig.Segments[i].Point.X.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + fig.Segments[i].Point.Y.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " l ");
                            }
                            break;
                        case Figures.SegmentType.CubicBezier:
                            for (int j = 0; j < fig.Segments[i].Points.Length; j++)
                            {
                                sw.Write(fig.Segments[i].Points[j].X.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + fig.Segments[i].Points[j].Y.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                            }
                            sw.Write("c ");
                            break;
                        case Figures.SegmentType.Close:
                            sw.Write("h ");
                            break;
                    }
                }

                if (fig.IsClipping)
                {
                    sw.Write("W n\n");
                }
                else
                {
                    if (fig.Fill != null)
                    {
                        if (fig.FillRule == FillRule.EvenOdd)
                        {
                            sw.Write("f*\n");
                        }
                        else
                        {
                            sw.Write("f\n");
                        }
                    }

                    if (fig.Stroke != null)
                    {
                        sw.Write("S\n");
                    }
                }
            }
            else if (figure is TextFigure)
            {
                TextFigure fig = figure as TextFigure;

                List<(string txt, bool isSymbolic)> segments = new List<(string txt, bool isSymbolic)>();

                StringBuilder currSeg = new StringBuilder();
                bool currSymbolic = false;

                for (int i = 0; i < fig.Text.Length; i++)
                {
                    if (PDFContextInterpreter.CP1252Chars.Contains(fig.Text[i]))
                    {
                        if (!currSymbolic)
                        {
                            currSeg.Append(fig.Text[i]);
                        }
                        else
                        {
                            if (currSeg.Length > 0)
                            {
                                segments.Add((currSeg.ToString(), currSymbolic));
                            }

                            currSeg = new StringBuilder();
                            currSymbolic = false;
                            currSeg.Append(fig.Text[i]);
                        }
                    }
                    else
                    {
                        if (currSymbolic)
                        {
                            currSeg.Append(fig.Text[i]);
                        }
                        else
                        {
                            if (currSeg.Length > 0)
                            {
                                segments.Add((currSeg.ToString(), currSymbolic));
                            }

                            currSeg = new StringBuilder();
                            currSymbolic = true;
                            currSeg.Append(fig.Text[i]);
                        }
                    }
                }

                if (currSeg.Length > 0)
                {
                    segments.Add((currSeg.ToString(), currSymbolic));
                }



                double realX = fig.Position.X;

                if (fig.Font.FontFamily.TrueTypeFile != null)
                {
                    realX = fig.Position.X - fig.Font.FontFamily.TrueTypeFile.Get1000EmGlyphBearings(fig.Text[0]).LeftSideBearing * fig.Font.FontSize / 1000;
                }

                double yMax = 0;
                double yMin = 0;

                if (fig.Font.FontFamily.TrueTypeFile != null)
                {
                    for (int i = 0; i < fig.Text.Length; i++)
                    {
                        TrueTypeFile.VerticalMetrics vMet = fig.Font.FontFamily.TrueTypeFile.Get1000EmGlyphVerticalMetrics(fig.Text[i]);
                        yMin = Math.Min(yMin, vMet.YMin * fig.Font.FontSize / 1000);
                        yMax = Math.Max(yMax, vMet.YMax * fig.Font.FontSize / 1000);
                    }
                }

                double realY = fig.Position.Y;

                if (fig.TextBaseline == TextBaselines.Bottom)
                {
                    realY -= yMax;
                }
                else if (fig.TextBaseline == TextBaselines.Top)
                {
                    realY -= yMin;
                }
                else if (fig.TextBaseline == TextBaselines.Middle)
                {
                    realY -= (yMax + yMin) * 0.5;
                }
                else if (fig.TextBaseline == TextBaselines.Baseline)
                {
                    realY -= yMax + yMin;
                }

                double middleY = realY + (yMax + yMin) * 0.5;



                sw.Write("q\n1 0 0 1 0 " + (middleY).ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " cm\n");
                sw.Write("1 0 0 -1 0 0 cm\n");
                sw.Write("1 0 0 1 0 " + (-middleY).ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " cm\n");

                sw.Write("BT\n");

                if (figure.Stroke != null && figure.Fill != null)
                {
                    sw.Write("2 Tr\n");
                }
                else if (figure.Stroke != null)
                {
                    sw.Write("1 Tr\n");
                }
                else if (figure.Fill != null)
                {
                    sw.Write("0 Tr\n");
                }

                sw.Write(realX.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " " + realY.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " Td\n");

                for (int i = 0; i < segments.Count; i++)
                {
                    if (!segments[i].isSymbolic)
                    {
                        sw.Write("/" + fonts[(fig.Font.FontFamily.FileName, false)].FontReferenceName + " " + fig.Font.FontSize.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " Tf\n");
                        sw.Write(GetKernedString(segments[i].txt, fig.Font) + " TJ\n");
                    }
                    else
                    {
                        sw.Write("/" + fonts[(fig.Font.FontFamily.FileName, true)].FontReferenceName + " " + fig.Font.FontSize.ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " Tf\n");
                        sw.Write("<" + EscapeSymbolStringForPDF(segments[i].txt, ((PDFType0Font)fonts[(fig.Font.FontFamily.FileName, true)]).DescendantFonts.Values[0].GlyphIndices) + "> Tj\n");
                    }
                }

                sw.Write("ET\nQ\n");
            }
            else if (figure is TransformFigure transf)
            {
                if (transf.TransformType == TransformFigure.TransformTypes.Transform)
                {
                    sw.Write(transf.TransformationMatrix[0, 0].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                    sw.Write(transf.TransformationMatrix[0, 1].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                    sw.Write(transf.TransformationMatrix[1, 0].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                    sw.Write(transf.TransformationMatrix[1, 1].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                    sw.Write(transf.TransformationMatrix[0, 2].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " ");
                    sw.Write(transf.TransformationMatrix[1, 2].ToString("0.################", System.Globalization.CultureInfo.InvariantCulture) + " cm\n");
                }
                else if (transf.TransformType == TransformFigure.TransformTypes.Save)
                {
                    sw.Write("q\n");
                }
                else if (transf.TransformType == TransformFigure.TransformTypes.Restore)
                {
                    sw.Write("Q\n");
                }
            }
            else if (figure is RasterImageFigure fig)
            {
                sw.Write("/a" + Array.IndexOf(alphas, 1).ToString() + " gs\n");
                sw.Write("/" + imageObjects[fig.Image.Id].ReferenceName + " Do\n");
            }
            else if (figure is OptionalContentFigure opt)
            {
                if (opt.FigureType == OptionalContentFigure.OptionalContentType.Start)
                {
                    sw.Write("/OC /" + optionalContentGroups[opt.VisibilityExpression.ToString()].ReferenceName + " BDC\n");
                }
                else if (opt.FigureType == OptionalContentFigure.OptionalContentType.End)
                {
                    sw.Write("EMC\n");
                }
            }

            if (restoreAtEnd)
            {
                sw.Write("Q\n");
            }
        }
    }
}
