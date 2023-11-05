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

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace VectSharp.ImageSharpUtils
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
        /// <param name="alpha">A boolean value indicating whether transparency (alpha) data from the image should be preserved or not.</param>
        /// <param name="interpolate">A boolean value indicating whether the image should be interpolated when it is resized or not.</param>
        public RasterImageFile(string fileName, bool alpha = true, bool interpolate = true)
        {
            Image image;

            if (alpha)
            {
                image = Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(fileName);
            }
            else
            {
                image = Image.Load<SixLabors.ImageSharp.PixelFormats.Rgb24>(fileName);
            }

            image.Mutate(x => x.AutoOrient());

            int stride = image.Width * (alpha ? 4 : 3);
            int size = stride * image.Height;

            IntPtr tbr = Marshal.AllocHGlobal(size);
            GC.AddMemoryPressure(size);

            IntPtr pointer = tbr;

            unsafe
            {
                if (alpha)
                {
                    Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img = (Image<SixLabors.ImageSharp.PixelFormats.Rgba32>)image;

                    for (int y = 0; y < image.Height; y++)
                    {
                        Memory<SixLabors.ImageSharp.PixelFormats.Rgba32> row = img.DangerousGetPixelRowMemory(y);

                        Span<SixLabors.ImageSharp.PixelFormats.Rgba32> newRow = new Span<SixLabors.ImageSharp.PixelFormats.Rgba32>(pointer.ToPointer(), row.Length);
                        row.Span.CopyTo(newRow);

                        pointer = IntPtr.Add(pointer, stride);
                    }
                }
                else
                {
                    Image<SixLabors.ImageSharp.PixelFormats.Rgb24> img = (Image<SixLabors.ImageSharp.PixelFormats.Rgb24>)image;

                    for (int y = 0; y < image.Height; y++)
                    {
                        Memory<SixLabors.ImageSharp.PixelFormats.Rgb24> row = img.DangerousGetPixelRowMemory(y);

                        Span<SixLabors.ImageSharp.PixelFormats.Rgb24> newRow = new Span<SixLabors.ImageSharp.PixelFormats.Rgb24>(pointer.ToPointer(), row.Length);
                        row.Span.CopyTo(newRow);

                        pointer = IntPtr.Add(pointer, stride);
                    }
                }
            }

            this.Width = image.Width;
            this.Height = image.Height;
            this.HasAlpha = alpha;
            this.Interpolate = interpolate;
            this.Id = Guid.NewGuid().ToString();
            this.ImageDataAddress = tbr;
            this.DataHolder = new DisposableIntPtr(tbr);
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
        /// <param name="alpha">A boolean value indicating whether transparency (alpha) data from the image should be preserved or not.</param>
        /// <param name="interpolate">A boolean value indicating whether the image should be interpolated when it is resized or not.</param>
        public RasterImageStream(Stream imageStream, bool alpha = true, bool interpolate = true)
        {
            Image image;

            if (alpha)
            {
                image = Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(imageStream);
            }
            else
            {
                image = Image.Load<SixLabors.ImageSharp.PixelFormats.Rgb24>(imageStream);
            }

            image.Mutate(x => x.AutoOrient());

            int stride = image.Width * (alpha ? 4 : 3);
            int size = stride * image.Height;

            IntPtr tbr = Marshal.AllocHGlobal(size);
            GC.AddMemoryPressure(size);

            IntPtr pointer = tbr;

            unsafe
            {
                if (alpha)
                {
                    Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img = (Image<SixLabors.ImageSharp.PixelFormats.Rgba32>)image;

                    for (int y = 0; y < image.Height; y++)
                    {
                        Memory<SixLabors.ImageSharp.PixelFormats.Rgba32> row = img.DangerousGetPixelRowMemory(y);

                        Span<SixLabors.ImageSharp.PixelFormats.Rgba32> newRow = new Span<SixLabors.ImageSharp.PixelFormats.Rgba32>(pointer.ToPointer(), row.Length);
                        row.Span.CopyTo(newRow);

                        pointer = IntPtr.Add(pointer, stride);
                    }
                }
                else
                {
                    Image<SixLabors.ImageSharp.PixelFormats.Rgb24> img = (Image<SixLabors.ImageSharp.PixelFormats.Rgb24>)image;

                    for (int y = 0; y < image.Height; y++)
                    {
                        Memory<SixLabors.ImageSharp.PixelFormats.Rgb24> row = img.DangerousGetPixelRowMemory(y);

                        Span<SixLabors.ImageSharp.PixelFormats.Rgb24> newRow = new Span<SixLabors.ImageSharp.PixelFormats.Rgb24>(pointer.ToPointer(), row.Length);
                        row.Span.CopyTo(newRow);

                        pointer = IntPtr.Add(pointer, stride);
                    }
                }
            }

            this.Width = image.Width;
            this.Height = image.Height;
            this.HasAlpha = alpha;
            this.Interpolate = interpolate;
            this.Id = Guid.NewGuid().ToString();
            this.ImageDataAddress = tbr;
            this.DataHolder = new DisposableIntPtr(tbr);
        }

        /// <summary>
        /// Creates a new <see cref="RasterImage"/> from the specified stream.
        /// </summary>
        /// <param name="imageAddress">A pointer to the address where the image data is contained.</param>
        /// <param name="imageLength">The length in bytes of the image data.</param>
        /// <param name="alpha">A boolean value indicating whether transparency (alpha) data from the image should be preserved or not.</param>
        /// <param name="interpolate">A boolean value indicating whether the image should be interpolated when it is resized or not.</param>
        public RasterImageStream(IntPtr imageAddress, int imageLength, bool alpha = true, bool interpolate = true)
        {
            unsafe
            {
                ReadOnlySpan<byte> imageSpan = new ReadOnlySpan<byte>((void*)imageAddress, imageLength);

                Image image;

                if (alpha)
                {
                    image = Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(imageSpan);
                }
                else
                {
                    image = Image.Load<SixLabors.ImageSharp.PixelFormats.Rgb24>(imageSpan);
                }

                image.Mutate(x => x.AutoOrient());

                int stride = image.Width * (alpha ? 4 : 3);
                int size = stride * image.Height;

                IntPtr tbr = Marshal.AllocHGlobal(size);
                GC.AddMemoryPressure(size);

                IntPtr pointer = tbr;

                if (alpha)
                {
                    Image<SixLabors.ImageSharp.PixelFormats.Rgba32> img = (Image<SixLabors.ImageSharp.PixelFormats.Rgba32>)image;

                    for (int y = 0; y < image.Height; y++)
                    {
                        Memory<SixLabors.ImageSharp.PixelFormats.Rgba32> row = img.DangerousGetPixelRowMemory(y);

                        Span<SixLabors.ImageSharp.PixelFormats.Rgba32> newRow = new Span<SixLabors.ImageSharp.PixelFormats.Rgba32>(pointer.ToPointer(), row.Length);
                        row.Span.CopyTo(newRow);

                        pointer = IntPtr.Add(pointer, stride);
                    }
                }
                else
                {
                    Image<SixLabors.ImageSharp.PixelFormats.Rgb24> img = (Image<SixLabors.ImageSharp.PixelFormats.Rgb24>)image;

                    for (int y = 0; y < image.Height; y++)
                    {
                        Memory<SixLabors.ImageSharp.PixelFormats.Rgb24> row = img.DangerousGetPixelRowMemory(y);

                        Span<SixLabors.ImageSharp.PixelFormats.Rgb24> newRow = new Span<SixLabors.ImageSharp.PixelFormats.Rgb24>(pointer.ToPointer(), row.Length);
                        row.Span.CopyTo(newRow);

                        pointer = IntPtr.Add(pointer, stride);
                    }
                }

                this.Width = image.Width;
                this.Height = image.Height;
                this.HasAlpha = alpha;
                this.Interpolate = interpolate;
                this.Id = Guid.NewGuid().ToString();
                this.ImageDataAddress = tbr;
                this.DataHolder = new DisposableIntPtr(tbr);
            }
        }
    }
}
