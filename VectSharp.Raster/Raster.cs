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
using System.IO;
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
            doc.SaveAsPDF(ms);

            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument muDoc = new MuPDFDocument(context, ref ms, InputFileTypes.PDF))
                {
                    muDoc.SaveImage(0, scale, MuPDFCore.PixelFormats.RGBA, fileName, RasterOutputFileTypes.PNG);
                }
            }
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
            doc.SaveAsPDF(ms);

            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument muDoc = new MuPDFDocument(context, ref ms, InputFileTypes.PDF))
                {
                    muDoc.WriteImage(0, scale, MuPDFCore.PixelFormats.RGBA, stream, RasterOutputFileTypes.PNG);
                }
            }
        }
    }
}