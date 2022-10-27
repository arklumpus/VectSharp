/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020  Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using MuPDFCore;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VectSharp.PDF;

namespace VectSharp.Raster
{
    /// <summary>
    /// Contains methods to render a page to a PNG image.
    /// </summary>
    public static class Raster
    {

        /// <summary>
        /// Render the page to a PNG file.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="fileName">The full path to the file to save. If it exists, it will be overwritten.</param>
        /// <param name="scale">The scale to be used when rasterising the page. This will determine the width and height of the image file.</param>
        public static void SaveAsPNG(this Page page, string fileName, double scale = 1)
        {
            Document doc = new Document();
            doc.Pages.Add(page);

            MemoryStream ms = new MemoryStream();
            doc.SaveAsPDF(ms, filterOption: new PDFContextInterpreter.FilterOption(PDFContextInterpreter.FilterOption.FilterOperations.RasteriseAll, scale, true));

            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument muDoc = new MuPDFDocument(context, ref ms, InputFileTypes.PDF))
                {
                    muDoc.SaveImage(0, scale, MuPDFCore.PixelFormats.RGBA, fileName, RasterOutputFileTypes.PNG);
                }
            }

            ms.Dispose();
        }

        /// <summary>
        /// Render the page to a PNG stream.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="stream">The stream to which the PNG data will be written.</param>
        /// <param name="scale">The scale to be used when rasterising the page. This will determine the width and height of the image file.</param>
        public static void SaveAsPNG(this Page page, Stream stream, double scale = 1)
        {
            Document doc = new Document();
            doc.Pages.Add(page);

            MemoryStream ms = new MemoryStream();
            doc.SaveAsPDF(ms, filterOption: new PDFContextInterpreter.FilterOption(PDFContextInterpreter.FilterOption.FilterOperations.RasteriseAll, scale, true));

            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument muDoc = new MuPDFDocument(context, ref ms, InputFileTypes.PDF))
                {
                    muDoc.WriteImage(0, scale, MuPDFCore.PixelFormats.RGBA, stream, RasterOutputFileTypes.PNG);
                }
            }

            ms.Dispose();
        }

        /// <summary>
        /// Rasterise a region of a <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object that will be rasterised.</param>
        /// <param name="region">The region of the <paramref name="graphics"/> that will be rasterised.</param>
        /// <param name="scale">The scale at which the image will be rendered.</param>
        /// <param name="interpolate">Whether the resulting image should be interpolated or not when it is drawn on another <see cref="Graphics"/> surface.</param>
        /// <returns>A <see cref="RasterImage"/> containing the rasterised graphics.</returns>
        public static RasterImage Rasterise(this Graphics graphics, Rectangle region, double scale, bool interpolate)
        {
            Page pag = new Page(1, 1);
            pag.Graphics.DrawGraphics(0, 0, graphics);
            pag.Crop(region.Location, region.Size);

            Document doc = new Document();
            doc.Pages.Add(pag);

            MemoryStream ms = new MemoryStream();
            doc.SaveAsPDF(ms, filterOption: new PDFContextInterpreter.FilterOption(PDFContextInterpreter.FilterOption.FilterOperations.RasteriseAll, scale, true));

            IntPtr imageDataAddress;

            int width;
            int height;

            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument muDoc = new MuPDFDocument(context, ref ms, InputFileTypes.PDF))
                {
                    int imageSize = muDoc.GetRenderedSize(0, scale, MuPDFCore.PixelFormats.RGBA);

                    RoundedRectangle roundedBounds = muDoc.Pages[0].Bounds.Round(scale);

                    width = roundedBounds.Width;
                    height = roundedBounds.Height;


                    imageDataAddress = Marshal.AllocHGlobal(imageSize);
                    GC.AddMemoryPressure(imageSize);

                    muDoc.Render(0, scale, MuPDFCore.PixelFormats.RGBA, imageDataAddress);
                }
            }

            ms.Dispose();

            DisposableIntPtr disposableAddress = new DisposableIntPtr(imageDataAddress);

            return new RasterImage(ref disposableAddress, width, height, true, interpolate);
        }
    }
}