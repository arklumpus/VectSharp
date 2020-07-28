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

namespace VectSharp.MuPDFUtils
{
    /// <summary>
    /// A <see cref="RasterImage"/> created from a file.
    /// </summary>
    public class RasterImageFile : RasterImage
    {
        /// <summary>
        /// Creates a new <see cref="RasterImage"/> from the specified file.
        /// </summary>
        /// <param name="fileName">The path to the file containing the image.</param>
        /// <param name="pageNumber">The number of the page in the file from which the image should be created, starting at 0. Only useful for multi-page formats, such as PDF.</param>
        /// <param name="scale">The scale factor at which to render the image.</param>
        /// <param name="alpha">A boolean value indicating whether transparency (alpha) data from the image should be preserved or not.</param>
        /// <param name="interpolate">A boolean value indicating whether the image should be interpolated when it is resized or not.</param>
        public RasterImageFile(string fileName, int pageNumber = 0, double scale = 1, bool alpha = true, bool interpolate = true)
        {
            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument document = new MuPDFDocument(context, fileName))
                {
                    RoundedRectangle roundedBounds = document.Pages[pageNumber].Bounds.Round(scale);

                    this.Width = roundedBounds.Width;
                    this.Height = roundedBounds.Height;

                    this.HasAlpha = alpha;
                    this.Interpolate = interpolate;

                    this.Id = Guid.NewGuid().ToString();

                    int imageSize = document.GetRenderedSize(pageNumber, scale, alpha ? MuPDFCore.PixelFormats.RGBA : MuPDFCore.PixelFormats.RGB);

                    this.ImageDataAddress = Marshal.AllocHGlobal(imageSize);
                    this.DataHolder = new DisposableIntPtr(this.ImageDataAddress);
                    GC.AddMemoryPressure(imageSize);

                    document.Render(pageNumber, scale, alpha ? MuPDFCore.PixelFormats.RGBA : MuPDFCore.PixelFormats.RGB, this.ImageDataAddress);
                }
            }
        }
    }

    /// <summary>
    /// A <see cref="RasterImage"/> created from a stream.
    /// </summary>
    public class RasterImageStream : RasterImage
    {
        /// <summary>
        /// Creates a new <see cref="RasterImage"/> from the specified stream.
        /// </summary>
        /// <param name="imageStream">The stream containing the image data.</param>
        /// <param name="fileType">The type of the image contained in the stream.</param>
        /// <param name="pageNumber">The number of the page in the file from which the image should be created, starting at 0. Only useful for multi-page formats, such as PDF.</param>
        /// <param name="scale">The scale factor at which to render the image.</param>
        /// <param name="alpha">A boolean value indicating whether transparency (alpha) data from the image should be preserved or not.</param>
        /// <param name="interpolate">A boolean value indicating whether the image should be interpolated when it is resized or not.</param>
        public RasterImageStream(Stream imageStream, InputFileTypes fileType, int pageNumber = 0, double scale = 1, bool alpha = true, bool interpolate = true)
        {
            using (MuPDFContext context = new MuPDFContext())
            {
                IntPtr originalImageAddress;
                long originalImageLength;

                IDisposable toBeDisposed = null;
                GCHandle handleToFree;

                if (imageStream is MemoryStream ms)
                {
                    int origin = (int)ms.Seek(0, SeekOrigin.Begin);
                    originalImageLength = ms.Length;

                    handleToFree = GCHandle.Alloc(ms.GetBuffer(), GCHandleType.Pinned);
                    originalImageAddress = handleToFree.AddrOfPinnedObject();
                }
                else
                {
                    MemoryStream mem = new MemoryStream((int)imageStream.Length);
                    imageStream.CopyTo(mem);

                    toBeDisposed = mem;

                    int origin = (int)mem.Seek(0, SeekOrigin.Begin);
                    originalImageLength = mem.Length;

                    handleToFree = GCHandle.Alloc(mem.GetBuffer(), GCHandleType.Pinned);
                    originalImageAddress = handleToFree.AddrOfPinnedObject();
                }

                using (MuPDFDocument document = new MuPDFDocument(context, originalImageAddress, originalImageLength, fileType))
                {
                    RoundedRectangle roundedBounds = document.Pages[pageNumber].Bounds.Round(scale);

                    this.Width = roundedBounds.Width;
                    this.Height = roundedBounds.Height;

                    this.HasAlpha = alpha;
                    this.Interpolate = interpolate;

                    this.Id = Guid.NewGuid().ToString();

                    int imageSize = document.GetRenderedSize(pageNumber, scale, alpha ? MuPDFCore.PixelFormats.RGBA : MuPDFCore.PixelFormats.RGB);

                    this.ImageDataAddress = Marshal.AllocHGlobal(imageSize);
                    this.DataHolder = new DisposableIntPtr(this.ImageDataAddress);
                    GC.AddMemoryPressure(imageSize);

                    document.Render(pageNumber, scale, alpha ? MuPDFCore.PixelFormats.RGBA : MuPDFCore.PixelFormats.RGB, this.ImageDataAddress);
                }

                handleToFree.Free();
                toBeDisposed?.Dispose();
            }
        }

        /// <summary>
        /// Creates a new <see cref="RasterImage"/> from the specified stream.
        /// </summary>
        /// <param name="imageAddress">A pointer to the address where the image data is contained.</param>
        /// <param name="imageLength">The length in bytes of the image data.</param>
        /// <param name="fileType">The type of the image contained in the stream.</param>
        /// <param name="pageNumber">The number of the page in the file from which the image should be created, starting at 0. Only useful for multi-page formats, such as PDF.</param>
        /// <param name="scale">The scale factor at which to render the image.</param>
        /// <param name="alpha">A boolean value indicating whether transparency (alpha) data from the image should be preserved or not.</param>
        /// <param name="interpolate">A boolean value indicating whether the image should be interpolated when it is resized or not.</param>
        public RasterImageStream(IntPtr imageAddress, long imageLength, InputFileTypes fileType, int pageNumber = 0, double scale = 1, bool alpha = true, bool interpolate = true)
        {
            using (MuPDFContext context = new MuPDFContext())
            {
                using (MuPDFDocument document = new MuPDFDocument(context, imageAddress, imageLength, fileType))
                {
                    RoundedRectangle roundedBounds = document.Pages[pageNumber].Bounds.Round(scale);

                    this.Width = roundedBounds.Width;
                    this.Height = roundedBounds.Height;

                    this.HasAlpha = alpha;
                    this.Interpolate = interpolate;

                    this.Id = Guid.NewGuid().ToString();

                    int imageSize = document.GetRenderedSize(pageNumber, scale, alpha ? MuPDFCore.PixelFormats.RGBA : MuPDFCore.PixelFormats.RGB);

                    this.ImageDataAddress = Marshal.AllocHGlobal(imageSize);
                    this.DataHolder = new DisposableIntPtr(this.ImageDataAddress);
                    GC.AddMemoryPressure(imageSize);

                    document.Render(pageNumber, scale, alpha ? MuPDFCore.PixelFormats.RGBA : MuPDFCore.PixelFormats.RGB, this.ImageDataAddress);
                }
            }
        }
    }
}
