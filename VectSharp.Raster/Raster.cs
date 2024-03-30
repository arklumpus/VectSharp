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
            doc.SaveAsPDF(ms, textOption: PDFContextInterpreter.TextOptions.ConvertIntoPaths, filterOption: new PDFContextInterpreter.FilterOption(PDFContextInterpreter.FilterOption.FilterOperations.RasteriseAll, scale, true));

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
            doc.SaveAsPDF(ms, textOption: PDFContextInterpreter.TextOptions.ConvertIntoPaths, filterOption: new PDFContextInterpreter.FilterOption(PDFContextInterpreter.FilterOption.FilterOperations.RasteriseAll, scale, true));

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
            doc.SaveAsPDF(ms, textOption: PDFContextInterpreter.TextOptions.ConvertIntoPaths, filterOption: new PDFContextInterpreter.FilterOption(PDFContextInterpreter.FilterOption.FilterOperations.RasteriseAll, scale, true));

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

        /// <summary>
        /// Render the page to raw pixel data, in 32bpp RGBA format.
        /// </summary>
        /// <param name="pag">The <see cref="Page"/> to render.</param>
        /// <param name="scale">The scale to be used when rasterising the page. This will determine the width and height of the image.</param>
        /// <param name="width">The width of the rendered image.</param>
        /// <param name="height">The height of the rendered image.</param>
        /// <param name="totalSize">The size in bytes of the raw pixel data.</param>
        /// <returns>A <see cref="DisposableIntPtr"/> containing a pointer to the raw pixel data, stored in unmanaged memory. Dispose this object to release the unmanaged memory.</returns>
        public static DisposableIntPtr SaveAsRawBytes(this Page pag, out int width, out int height, out int totalSize, double scale = 1)
        {
            Document doc = new Document();
            doc.Pages.Add(pag);

            MemoryStream ms = new MemoryStream();
            doc.SaveAsPDF(ms, textOption: PDFContextInterpreter.TextOptions.ConvertIntoPaths, filterOption: new PDFContextInterpreter.FilterOption(PDFContextInterpreter.FilterOption.FilterOperations.RasteriseAll, scale, true));

            IntPtr renderedPage;

            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument muDoc = new MuPDFDocument(context, ref ms, InputFileTypes.PDF))
                {
                    totalSize = muDoc.GetRenderedSize(0, scale, MuPDFCore.PixelFormats.RGBA);

                    renderedPage = Marshal.AllocHGlobal(totalSize);

                    RoundedRectangle bounds = muDoc.Pages[0].Bounds.Round(scale);

                    width = bounds.Width;
                    height = bounds.Height;

                    muDoc.Render(0, scale, MuPDFCore.PixelFormats.RGBA, renderedPage);
                }
            }

            ms.Dispose();

            return new DisposableIntPtr(renderedPage);
        }

        /// <summary>
        /// Saves the animation to a stream in animated PNG format.
        /// </summary>
        /// <param name="animation">The animation to export.</param>
        /// <param name="imageStream">The stream on which the animated PNG will be written.</param>
        /// <param name="scale">The scale at which the animation will be rendered.</param>
        /// <param name="frameRate">The target frame rate of the animation, in frames-per-second (fps). This is capped by the animated PNG specification at 90 fps.</param>
        /// <param name="durationScaling">A scaling factor that will be applied to all durations in the animation. Values greater than 1 slow down the animation, values smaller than 1 accelerate it. Note that this does not affect the frame rate of the animation.</param>
        /// <param name="interframeCompression">The kind of compression that will be used to reduce file size. Note that if the animation has a transparent background, no compression can be performed, and the value of this parameter is ignored.</param>
        public static void SaveAsAnimatedPNG(this Animation animation, Stream imageStream, double scale = 1, double frameRate = 60, double durationScaling = 1, AnimatedPNG.InterframeCompression interframeCompression = AnimatedPNG.InterframeCompression.First)
        {
            frameRate = Math.Min(frameRate, 90);

            int frames = (int)Math.Ceiling(animation.Duration * frameRate * durationScaling / 1000);

            Stream fs = imageStream;

            AnimatedPNG.CompressedFrame[] compressedFrames = new AnimatedPNG.CompressedFrame[frames];

            int width = (int)(animation.Width * scale);
            int height = (int)(animation.Height * scale);

            if (animation.Background.A < 1 || interframeCompression == AnimatedPNG.InterframeCompression.None)
            {
                Parallel.For(0, frames, i =>
                {
                    double frameTime = i / frameRate / durationScaling * 1000;

                    double frameDuration = Math.Min((animation.Duration - frameTime) * durationScaling, 1000 / frameRate);

                    Page pag = animation.GetFrameAtAbsolute(frameTime);

                    using (DisposableIntPtr rawFrame = pag.SaveAsRawBytes(out _, out _, out _, scale))
                    {
                        compressedFrames[i] = new AnimatedPNG.CompressedFrame(rawFrame, width, height, true, frameDuration);
                    }
                });
            }
            else if (interframeCompression == AnimatedPNG.InterframeCompression.First)
            {
                DisposableIntPtr firstFrame = animation.GetFrameAtAbsolute(0).SaveAsRawBytes(out _, out _, out _, scale);
                compressedFrames[0] = new AnimatedPNG.CompressedFrame(firstFrame, width, height, true, Math.Min(animation.Duration * durationScaling, 1000 / frameRate));

                Parallel.For(1, frames, i =>
                {
                    double frameTime = i / frameRate / durationScaling * 1000;

                    double frameDuration = Math.Min((animation.Duration - frameTime) * durationScaling, 1000 / frameRate);

                    Page pag = animation.GetFrameAtAbsolute(frameTime);

                    using (DisposableIntPtr rawFrame = pag.SaveAsRawBytes(out _, out _, out _, scale))
                    {
                        compressedFrames[i] = new AnimatedPNG.CompressedFrame(rawFrame, firstFrame, true, width, height, true, frameDuration);
                    }
                });

                firstFrame.Dispose();
            }
            else if (interframeCompression == AnimatedPNG.InterframeCompression.Previous)
            {
                DisposableIntPtr[] rawFrames = new DisposableIntPtr[frames];

                Parallel.For(0, frames, i =>
                {
                    double frameTime = i / frameRate / durationScaling * 1000;
                    Page pag = animation.GetFrameAtAbsolute(frameTime);
                    rawFrames[i] = pag.SaveAsRawBytes(out _, out _, out _, scale);
                });

                compressedFrames[0] = new AnimatedPNG.CompressedFrame(rawFrames[0], width, height, true, Math.Min(animation.Duration * durationScaling, 1000 / frameRate));

                Parallel.For(1, frames, i =>
                {
                    double frameTime = i / frameRate / durationScaling * 1000;
                    double frameDuration = Math.Min((animation.Duration - frameTime) * durationScaling, 1000 / frameRate);
                    compressedFrames[i] = new AnimatedPNG.CompressedFrame(rawFrames[i], rawFrames[i - 1], false, width, height, true, frameDuration);
                });
            }

            AnimatedPNG.Create(fs, (int)(animation.Width * scale), (int)(animation.Height * scale), true, compressedFrames, animation.RepeatCount);
        }

        /// <summary>
        /// Saves the animation to an animated PNG file.
        /// </summary>
        /// <param name="animation">The animation to export.</param>
        /// <param name="fileName">The output file to create.</param>
        /// <param name="scale">The scale at which the animation will be rendered.</param>
        /// <param name="frameRate">The target frame rate of the animation, in frames-per-second (fps). This is capped by the animated PNG specification at 90 fps.</param>
        /// <param name="durationScaling">A scaling factor that will be applied to all durations in the animation. Values greater than 1 slow down the animation, values smaller than 1 accelerate it. Note that this does not affect the frame rate of the animation.</param>
        /// <param name="interframeCompression">The kind of compression that will be used to reduce file size. Note that if the animation has a transparent background, no compression can be performed, and the value of this parameter is ignored.</param>
        public static void SaveAsAnimatedPNG(this Animation animation, string fileName, double scale = 1, double frameRate = 60, double durationScaling = 1, AnimatedPNG.InterframeCompression interframeCompression = AnimatedPNG.InterframeCompression.First)
        {
            using (FileStream fs = File.Create(fileName))
            {
                SaveAsAnimatedPNG(animation, fs, scale, frameRate, durationScaling, interframeCompression);
            }
        }


        /// <summary>
        /// Render the page to a JPEG file.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="fileName">The full path to the file to save. If it exists, it will be overwritten.</param>
        /// <param name="scale">The scale to be used when rasterising the page. This will determine the width and height of the image file.</param>
        /// <param name="quality">The quality of the JPEG output (ranging from 0 to 100).</param>
        public static void SaveAsJPEG(this Page page, string fileName, double scale = 1, int quality = 90)
        {
            Document doc = new Document();
            doc.Pages.Add(page);

            MemoryStream ms = new MemoryStream();
            doc.SaveAsPDF(ms, textOption: PDFContextInterpreter.TextOptions.ConvertIntoPaths, filterOption: new PDFContextInterpreter.FilterOption(PDFContextInterpreter.FilterOption.FilterOperations.RasteriseAll, scale, true));

            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument muDoc = new MuPDFDocument(context, ref ms, InputFileTypes.PDF))
                {
                    muDoc.SaveImageAsJPEG(0, scale, fileName, quality);
                }
            }

            ms.Dispose();
        }
    }
}