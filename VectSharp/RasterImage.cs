/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2022 Giorgio Bianchini

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
using System.IO;
using System.Runtime.InteropServices;

namespace VectSharp
{
    /// <summary>
    /// Represents the pixel format of a raster image.
    /// </summary>
    public enum PixelFormats
    {
        /// <summary>
        /// RGB 24bpp format.
        /// </summary>
        RGB,

        /// <summary>
        /// RGBA 32bpp format.
        /// </summary>
        RGBA,

        /// <summary>
        /// BGR 24bpp format.
        /// </summary>
        BGR,

        /// <summary>
        /// BGR 32bpp format.
        /// </summary>
        BGRA
    }

    /// <summary>
    /// An <see cref="IDisposable"/> wrapper around an <see cref="IntPtr"/> that frees the allocated memory when it is disposed.
    /// </summary>
    public class DisposableIntPtr : IDisposable
    {
        /// <summary>
        /// The pointer to the unmanaged memory.
        /// </summary>
        public readonly IntPtr InternalPointer;

        /// <summary>
        /// Create a new DisposableIntPtr.
        /// </summary>
        /// <param name="pointer">The pointer that should be freed upon disposing of this object.</param>
        public DisposableIntPtr(IntPtr pointer)
        {
            this.InternalPointer = pointer;
        }

        private bool disposedValue;

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Marshal.FreeHGlobal(InternalPointer);
                disposedValue = true;
            }
        }

        ///<inheritdoc/>
        ~DisposableIntPtr()
        {
            Dispose(disposing: false);
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents a raster image, created from raw pixel data. Consider using the derived classes included in the NuGet package "VectSharp.MuPDFUtils" if you need to load a raster image from a file or a <see cref="Stream"/>.
    /// </summary>
    public class RasterImage : IDisposable
    {
        /// <summary>
        /// The memory address of the image pixel data.
        /// </summary>
        public IntPtr ImageDataAddress { get; protected set; }

        /// <summary>
        /// An <see cref="IDisposable"/> that will be disposed when the image is disposed.
        /// </summary>
        public IDisposable DataHolder { get; protected set; }

        /// <summary>
        /// A univocal identifier for this image.
        /// </summary>
        public string Id { get; protected set; }

        /// <summary>
        /// Determines whether the image has an alpha channel.
        /// </summary>
        public bool HasAlpha { get; protected set; }

        /// <summary>
        /// The width in pixels of the image.
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// The height in pixels of the image.
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// Determines whether the image should be interpolated when it is resized.
        /// </summary>
        public bool Interpolate { get; protected set; }

        private MemoryStream _PNGStream = null;

        /// <summary>
        /// Contains a representation of the image in PNG format. Generated at the first access and cached until the image is disposed.
        /// </summary>
        public MemoryStream PNGStream
        {
            get
            {
                if (_PNGStream == null)
                {
                    _PNGStream = new MemoryStream();
                    this.EncodeAsPNG(_PNGStream);
                }
                _PNGStream.Seek(0, SeekOrigin.Begin);
                return _PNGStream;
            }
        }

        /// <summary>
        /// Default constructor, necessary for inheritance.
        /// </summary>
        protected RasterImage()
        {

        }

        /// <summary>
        /// Creates a new <see cref="RasterImage"/> instance from the specified pixel data in RGB or RGBA format.
        /// </summary>
        /// <param name="pixelData">The address of the image pixel data in RGB or RGBA format.</param>
        /// <param name="width">The width in pixels of the image.</param>
        /// <param name="height">The height in pixels of the image.</param>
        /// <param name="hasAlpha">true if the image is in RGBA format, false if it is in RGB format.</param>
        /// <param name="interpolate">Whether the image should be interpolated when it is resized.</param>
        public RasterImage(IntPtr pixelData, int width, int height, bool hasAlpha, bool interpolate)
        {
            this.Id = Guid.NewGuid().ToString();
            this.ImageDataAddress = pixelData;
            this.Width = width;
            this.Height = height;
            this.HasAlpha = hasAlpha;
            this.Interpolate = interpolate;
        }

        /// <summary>
        /// Creates a new <see cref="RasterImage"/> instance from the specified pixel data in RGB or RGBA format.
        /// </summary>
        /// <param name="pixelData">The address of the image pixel data in RGB or RGBA format wrapped in a <see cref="DisposableIntPtr"/>. The <see cref="RasterImage"/> will take ownership of this memory.</param>
        /// <param name="width">The width in pixels of the image.</param>
        /// <param name="height">The height in pixels of the image.</param>
        /// <param name="hasAlpha">true if the image is in RGBA format, false if it is in RGB format.</param>
        /// <param name="interpolate">Whether the image should be interpolated when it is resized.</param>
        public RasterImage(ref DisposableIntPtr pixelData, int width, int height, bool hasAlpha, bool interpolate)
        {
            this.Id = Guid.NewGuid().ToString();
            this.ImageDataAddress = pixelData.InternalPointer;
            this.DataHolder = pixelData;
            this.Width = width;
            this.Height = height;
            this.HasAlpha = hasAlpha;
            this.Interpolate = interpolate;
        }

        /// <summary>
        /// Creates a new <see cref="RasterImage"/> instance copying the specified pixel data.
        /// </summary>
        /// <param name="data">The image pixel data that will be copied.</param>
        /// <param name="width">The width in pixels of the image.</param>
        /// <param name="height">The height in pixels of the image.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        /// <param name="interpolate">Whether the image should be interpolated when it is resized.</param>
        public RasterImage(byte[] data, int width, int height, PixelFormats pixelFormat, bool interpolate)
        {
            this.ImageDataAddress = Marshal.AllocHGlobal(data.Length);
            GC.AddMemoryPressure(data.Length);
            this.DataHolder = new DisposableIntPtr(this.ImageDataAddress);
            this.Id = Guid.NewGuid().ToString();
            this.Width = width;
            this.Height = height;
            this.Interpolate = interpolate;

            switch (pixelFormat)
            {
                case PixelFormats.RGB:
                case PixelFormats.RGBA:
                    Marshal.Copy(data, 0, this.ImageDataAddress, data.Length);
                    this.HasAlpha = pixelFormat == PixelFormats.RGBA;
                    break;

                case PixelFormats.BGRA:
                case PixelFormats.BGR:
                    this.HasAlpha = pixelFormat == PixelFormats.BGRA;

                    int pixelSize = pixelFormat == PixelFormats.BGRA ? 4 : 3;
                    int pixelCount = width * height;

                    unsafe
                    {
                        byte* dataPointer = (byte*)this.ImageDataAddress;
                        for (int i = 0; i < pixelCount; i++)
                        {
                            dataPointer[i * pixelSize] = data[i * pixelSize + 2];
                            dataPointer[i * pixelSize + 1] = data[i * pixelSize + 1];
                            dataPointer[i * pixelSize + 2] = data[i * pixelSize];
                            if (pixelSize == 4)
                            {
                                dataPointer[i * pixelSize + 3] = data[i * pixelSize + 3];
                            }
                        }
                    }
                    break;
            }
        }

        private void EncodeAsPNG(Stream outputStream)
        {
            unsafe
            {
                PNGEncoder.SavePNG((byte*)this.ImageDataAddress, this.Width, this.Height, this.HasAlpha, outputStream, PNGEncoder.FilterModes.Adaptive);
            }
        }

        /// <summary>
        /// Disposes the <see cref="PNGStream"/>. Also useful if is is necessary to regenerate it, e.g. because the underlying image pixel data has changed.
        /// </summary>
        public void ClearPNGCache()
        {
            _PNGStream?.Dispose();
            _PNGStream = null;
        }

        private bool disposedValue;

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _PNGStream?.Dispose();

                    if (DataHolder != null)
                    {
                        DataHolder.Dispose();
                        GC.RemoveMemoryPressure(Height * Width * (HasAlpha ? 4 : 3));
                    }
                }
                disposedValue = true;
            }
        }

        ///<inheritdoc/>
        ~RasterImage()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
