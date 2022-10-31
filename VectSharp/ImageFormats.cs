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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading;

namespace VectSharp
{
    /// <summary>
    /// Contains methods to create animated PNG image files.
    /// </summary>
    public static class AnimatedPNG
    {
        /// <summary>
        /// Types of inter-frame compression.
        /// </summary>
        public enum InterframeCompression
        {
            /// <summary>
            /// No inter-frame compression is performed (fastest, but produces the largest files).
            /// </summary>
            None,

            /// <summary>
            /// Inter-frame compression is performed by encoding the difference between each frame and the first frame in the animation.
            /// </summary>
            First,

            /// <summary>
            /// Inter-frame compression is performed by encoding the difference between each frame and the previous frame (produces the smallest files, but it is the slowest and requires large amounts of memory).
            /// </summary>
            Previous
        }

        /// <summary>
        /// Represents an individual frame of a PNG animation.
        /// </summary>
        public class CompressedFrame : IDisposable
        {
            internal MemoryStream compressedStream;
            private bool disposedValue;

            /// <summary>
            /// The duration of the frame in milliseconds.
            /// </summary>
            public double Duration { get; }

            internal InterframeCompression InterframeCompression { get; } = InterframeCompression.None;

            /// <summary>
            /// Creates a new <see cref="CompressedFrame"/> from raw pixel data in RGB or RGBA format.
            /// </summary>
            /// <param name="rawFrameData">A pointer to the raw pixel data for the frame.</param>
            /// <param name="width">The width of the image in pixels.</param>
            /// <param name="height">The height of the image in pixels.</param>
            /// <param name="hasAlpha">If the image data is in RGBA format, set this to <see langword="true"/>; if it is in RGB format, set it to <see langword="false"/>.</param>
            /// <param name="duration">The duration of the frame in milliseconds.</param>
            public unsafe CompressedFrame(DisposableIntPtr rawFrameData, int width, int height, bool hasAlpha, double duration)
            {
                this.compressedStream = PNGEncoder.CompressAPNGFrame((byte*)rawFrameData.InternalPointer, width, height, hasAlpha, PNGEncoder.FilterModes.Adaptive, 1);
                this.Duration = duration;
                this.InterframeCompression = InterframeCompression.None;
            }

            /// <summary>
            /// Creates a new <see cref="CompressedFrame"/> from raw pixel data in RGB or RGBA format, applying inter-frame compression based on another frame.
            /// </summary>
            /// <param name="rawFrameData">A pointer to the raw pixel data for the new frame.</param>
            /// <param name="otherFrameData">A pointer to the raw pixel data for the frame to use as a reference. This can either be the first frame in the animation, or the frame immediately preceeding the current frame.</param>
            /// <param name="isFirstFrame">If <paramref name="otherFrameData"/> is the raw pixel data for the first frame in the animation, set this to <see langword="true"/>; if it is the immediately preceeding frame, set this to <see langword="false"/>.</param>
            /// <param name="width">The width of the image in pixels.</param>
            /// <param name="height">The height of the image in pixels.</param>
            /// <param name="hasAlpha">If the image data is in RGBA format, set this to <see langword="true"/>; if it is in RGB format, set it to <see langword="false"/>.</param>
            /// <param name="duration">The duration of the frame in milliseconds.</param>
            public unsafe CompressedFrame(DisposableIntPtr rawFrameData, DisposableIntPtr otherFrameData, bool isFirstFrame, int width, int height, bool hasAlpha, double duration)
            {
                int pixelSize = hasAlpha ? 4 : 3;

                IntPtr frameDiff = Marshal.AllocHGlobal(width * height * 4);

                byte* frameDiffData = (byte*)frameDiff;
                byte* previousFrame = (byte*)otherFrameData.InternalPointer;
                byte* currentFrame = (byte*)rawFrameData.InternalPointer;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int pixelIndex = (y * width + x) * pixelSize;

                        if (previousFrame[pixelIndex] != currentFrame[pixelIndex] || previousFrame[pixelIndex + 1] != currentFrame[pixelIndex + 1] ||
                            previousFrame[pixelIndex + 2] != currentFrame[pixelIndex + 2] || (hasAlpha && previousFrame[pixelIndex + 3] != currentFrame[pixelIndex + 3]))
                        {
                            frameDiffData[pixelIndex] = currentFrame[pixelIndex];
                            frameDiffData[pixelIndex + 1] = currentFrame[pixelIndex + 1];
                            frameDiffData[pixelIndex + 2] = currentFrame[pixelIndex + 2];

                            if (hasAlpha)
                            {
                                frameDiffData[pixelIndex + 3] = currentFrame[pixelIndex + 3];
                            }
                            else
                            {
                                frameDiffData[pixelIndex + 3] = 255;
                            }
                        }
                        else
                        {
                            frameDiffData[pixelIndex] = 0;
                            frameDiffData[pixelIndex + 1] = 0;
                            frameDiffData[pixelIndex + 2] = 0;
                            frameDiffData[pixelIndex + 3] = 0;
                        }
                    }
                }

                this.compressedStream = PNGEncoder.CompressAPNGFrame((byte*)frameDiff, width, height, true, PNGEncoder.FilterModes.Adaptive, 1);
                this.Duration = duration;

                if (isFirstFrame)
                {
                    this.InterframeCompression = InterframeCompression.First;
                }
                else
                {
                    this.InterframeCompression = InterframeCompression.Previous;
                }

                Marshal.FreeHGlobal(frameDiff);
            }

            /// <inheritdoc/>
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        this.compressedStream.Dispose();
                    }

                    disposedValue = true;
                }
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Create a new animated PNG image, outputting it to the specified stream.
        /// </summary>
        /// <param name="outputStream">The stream to which the animated PNG image will be written.</param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="hasAlpha">If the frames of the image have an alpha channel, set this to <see langword="true"/>; otherwise, set it to <see langword="false"/>.</param>
        /// <param name="compressedFrames">The frames that will be used to create the animated PNG image.</param>
        /// <param name="repeatCount">The number of times that the animation should loop. Set this to 0 for an infinitely repeating animation.</param>
        public static unsafe void Create(Stream outputStream, int width, int height, bool hasAlpha, IReadOnlyList<CompressedFrame> compressedFrames, int repeatCount)
        {
            PNGEncoder.StartAPNG(width, height, hasAlpha, outputStream, compressedFrames.Count, repeatCount);

            for (int i = 0; i < compressedFrames.Count; i++)
            {
                ushort frameDurationNumerator = (ushort)Math.Round(compressedFrames[i].Duration * 60);
                ushort frameDurationDenominator = 60000;
                PNGEncoder.AddPreCompressedAPNGFrame(compressedFrames[i], width, height, hasAlpha, outputStream, i, frameDurationNumerator, frameDurationDenominator);
            }

            PNGEncoder.FinishAPNG(outputStream);
        }
    }



    internal static class PNGEncoder
    {
        internal unsafe static void SavePNG(byte* image, int width, int height, bool hasAlpha, Stream fs, FilterModes filter, int threadCount = 0)
        {
            if (threadCount == 0)
            {
                threadCount = filter == FilterModes.Adaptive ? Math.Max(1, Math.Min(width / 600, Environment.ProcessorCount - 2)) : 1;
            }

            //Header
            fs.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);

            //IHDR chunk
            fs.WriteInt(13);
            using (MemoryStream ihdr = new MemoryStream(13))
            {
                ihdr.WriteASCIIString("IHDR");
                ihdr.WriteInt(width);
                ihdr.WriteInt(height);
                ihdr.WriteByte(8); //Bit depth

                if (hasAlpha)
                {
                    ihdr.WriteByte(6); //Colour type
                }
                else
                {
                    ihdr.WriteByte(2); //Colour type
                }

                ihdr.WriteByte(0); //Compression method
                ihdr.WriteByte(0); //Filter method
                ihdr.WriteByte(0); //Interlace

                ihdr.Seek(0, SeekOrigin.Begin);
                ihdr.CopyTo(fs);

                fs.WriteUInt(CRC32.ComputeCRC(ihdr));
            }

            //IDAT chunk
            IntPtr filteredImage;

            if (threadCount > 1)
            {
                filteredImage = FilterImageData(image, width, height, hasAlpha ? 4 : 3, filter, threadCount);
            }
            else
            {
                filteredImage = FilterImageData(image, width, height, hasAlpha ? 4 : 3, filter);
            }

            using (MemoryStream compressedImage = StreamUtils.ZLibCompress(filteredImage, height * (width * (hasAlpha ? 4 : 3) + 1)))
            {
                compressedImage.Seek(0, SeekOrigin.Begin);
                fs.WriteUInt((uint)compressedImage.Length);
                fs.WriteASCIIString("IDAT");
                compressedImage.Seek(0, SeekOrigin.Begin);
                compressedImage.CopyTo(fs);

                fs.WriteUInt(CRC32.ComputeCRC(compressedImage.GetBuffer(), (int)compressedImage.Length, new byte[] { 73, 68, 65, 84 }));
            }

            Marshal.FreeHGlobal(filteredImage);

            //IEND chunk
            fs.WriteInt(0);
            fs.WriteASCIIString("IEND");
            fs.Write(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, 0, 4);

        }

        public unsafe static void StartAPNG(int width, int height, bool hasAlpha, Stream fs, int frameCount, int repeatCount)
        {
            //Header
            fs.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);

            //IHDR chunk
            fs.WriteInt(13);
            using (MemoryStream ihdr = new MemoryStream(17))
            {
                ihdr.WriteASCIIString("IHDR");
                ihdr.WriteInt(width);
                ihdr.WriteInt(height);
                ihdr.WriteByte(8); //Bit depth

                if (hasAlpha)
                {
                    ihdr.WriteByte(6); //Colour type
                }
                else
                {
                    ihdr.WriteByte(2); //Colour type
                }

                ihdr.WriteByte(0); //Compression method
                ihdr.WriteByte(0); //Filter method
                ihdr.WriteByte(0); //Interlace

                ihdr.Seek(0, SeekOrigin.Begin);
                ihdr.CopyTo(fs);

                fs.WriteUInt(CRC32.ComputeCRC(ihdr));
            }

            //acTL chunk
            fs.WriteInt(8);
            using (MemoryStream actl = new MemoryStream(12))
            {
                actl.WriteASCIIString("acTL");
                actl.WriteInt(frameCount);
                actl.WriteInt(repeatCount);

                actl.Seek(0, SeekOrigin.Begin);
                actl.CopyTo(fs);

                fs.WriteUInt(CRC32.ComputeCRC(actl));
            }
        }

        internal unsafe static void AddAPNGFrame(byte* image, int width, int height, bool hasAlpha, Stream fs, FilterModes filter, int frameNumber, ushort frameDelayNumerator, ushort frameDelayDenominator, int threadCount = 0)
        {
            if (threadCount == 0)
            {
                threadCount = filter == FilterModes.Adaptive ? Math.Max(1, Math.Min(width / 600, Environment.ProcessorCount - 2)) : 1;
            }

            int fcTLIndex = frameNumber == 0 ? 0 : ((frameNumber - 1) * 2 + 1);
            int fDATIndex = frameNumber == 0 ? 0 : (frameNumber * 2);

            //fcTL chunk
            fs.WriteInt(26);
            using (MemoryStream fctl = new MemoryStream(30))
            {
                fctl.WriteASCIIString("fcTL");
                fctl.WriteInt(fcTLIndex);
                fctl.WriteInt(width);
                fctl.WriteInt(height);
                fctl.WriteInt(0);
                fctl.WriteInt(0);
                fctl.WriteUShort(frameDelayNumerator);
                fctl.WriteUShort(frameDelayDenominator);
                fctl.WriteByte(1);
                fctl.WriteByte(0);

                fctl.Seek(0, SeekOrigin.Begin);
                fctl.CopyTo(fs);

                fs.WriteUInt(CRC32.ComputeCRC(fctl));
            }

            //fDAT chunk
            IntPtr filteredImage;

            if (threadCount > 1)
            {
                filteredImage = FilterImageData(image, width, height, hasAlpha ? 4 : 3, filter, threadCount);
            }
            else
            {
                filteredImage = FilterImageData(image, width, height, hasAlpha ? 4 : 3, filter);
            }

            if (frameNumber > 0)
            {
                using (MemoryStream compressedImage = StreamUtils.ZLibCompress(filteredImage, height * (width * (hasAlpha ? 4 : 3) + 1)))
                {
                    fs.WriteUInt((uint)compressedImage.Length + 4);

                    fs.WriteASCIIString("fdAT");
                    fs.WriteInt(fDATIndex);

                    compressedImage.Seek(0, SeekOrigin.Begin);
                    compressedImage.CopyTo(fs);

                    fs.WriteUInt(CRC32.ComputeCRC(compressedImage.GetBuffer(), (int)compressedImage.Length, new byte[] { 102, 100, 65, 84, (byte)(fDATIndex >> 24), (byte)((fDATIndex >> 16) & 255), (byte)((fDATIndex >> 8) & 255), (byte)(fDATIndex & 255) }));
                    //fs.WriteUInt(CRC32.ComputeCRC(chunkStream.GetBuffer(), (int)chunkStream.Length, new byte[] { 73, 68, 65, 84 }));
                    //fs.WriteUInt(CRC32.ComputeCRC(chunkStream));   
                }
            }
            else
            {
                using (MemoryStream compressedImage = StreamUtils.ZLibCompress(filteredImage, height * (width * (hasAlpha ? 4 : 3) + 1)))
                {
                    compressedImage.Seek(0, SeekOrigin.Begin);
                    fs.WriteUInt((uint)compressedImage.Length);
                    fs.WriteASCIIString("IDAT");
                    compressedImage.Seek(0, SeekOrigin.Begin);
                    compressedImage.CopyTo(fs);

                    fs.WriteUInt(CRC32.ComputeCRC(compressedImage.GetBuffer(), (int)compressedImage.Length, new byte[] { 73, 68, 65, 84 }));
                }
            }

            Marshal.FreeHGlobal(filteredImage);
        }

        internal static unsafe MemoryStream CompressAPNGFrame(byte* image, int width, int height, bool hasAlpha, FilterModes filter, int threadCount)
        {
            //fDAT chunk
            IntPtr filteredImage;

            if (threadCount > 1)
            {
                filteredImage = FilterImageData(image, width, height, hasAlpha ? 4 : 3, filter, threadCount);
            }
            else
            {
                filteredImage = FilterImageData(image, width, height, hasAlpha ? 4 : 3, filter);
            }

            MemoryStream compressedImage = StreamUtils.ZLibCompress(filteredImage, height * (width * (hasAlpha ? 4 : 3) + 1));

            Marshal.FreeHGlobal(filteredImage);

            return compressedImage;
        }

        internal static void AddPreCompressedAPNGFrame(AnimatedPNG.CompressedFrame compressedFrame, int width, int height, bool hasAlpha, Stream fs, int frameNumber, ushort frameDelayNumerator, ushort frameDelayDenominator)
        {
            int fcTLIndex = frameNumber == 0 ? 0 : ((frameNumber - 1) * 2 + 1);
            int fDATIndex = frameNumber == 0 ? 0 : (frameNumber * 2);

            //fcTL chunk
            fs.WriteInt(26);
            using (MemoryStream fctl = new MemoryStream(30))
            {
                fctl.WriteASCIIString("fcTL");
                fctl.WriteInt(fcTLIndex);
                fctl.WriteInt(width);
                fctl.WriteInt(height);
                fctl.WriteInt(0);
                fctl.WriteInt(0);
                fctl.WriteUShort(frameDelayNumerator);
                fctl.WriteUShort(frameDelayDenominator);

                if (compressedFrame.InterframeCompression == AnimatedPNG.InterframeCompression.None || frameNumber == 0)
                {
                    fctl.WriteByte(0);
                    fctl.WriteByte(0);
                }
                else if (compressedFrame.InterframeCompression == AnimatedPNG.InterframeCompression.First)
                {
                    fctl.WriteByte(2);
                    fctl.WriteByte(1);
                }
                else if (compressedFrame.InterframeCompression == AnimatedPNG.InterframeCompression.Previous)
                {
                    fctl.WriteByte(0);
                    fctl.WriteByte(1);
                }

                fctl.Seek(0, SeekOrigin.Begin);
                fctl.CopyTo(fs);

                fs.WriteUInt(CRC32.ComputeCRC(fctl));
            }

            MemoryStream compressedImage = compressedFrame.compressedStream;

            if (frameNumber > 0)
            {
                fs.WriteUInt((uint)compressedImage.Length + 4);

                fs.WriteASCIIString("fdAT");
                fs.WriteInt(fDATIndex);

                compressedImage.Seek(0, SeekOrigin.Begin);
                compressedImage.CopyTo(fs);

                fs.WriteUInt(CRC32.ComputeCRC(compressedImage.GetBuffer(), (int)compressedImage.Length, new byte[] { 102, 100, 65, 84, (byte)(fDATIndex >> 24), (byte)((fDATIndex >> 16) & 255), (byte)((fDATIndex >> 8) & 255), (byte)(fDATIndex & 255) }));
            }
            else
            {
                compressedImage.Seek(0, SeekOrigin.Begin);
                fs.WriteUInt((uint)compressedImage.Length);
                fs.WriteASCIIString("IDAT");
                compressedImage.Seek(0, SeekOrigin.Begin);
                compressedImage.CopyTo(fs);

                fs.WriteUInt(CRC32.ComputeCRC(compressedImage.GetBuffer(), (int)compressedImage.Length, new byte[] { 73, 68, 65, 84 }));
            }
        }

        internal unsafe static void FinishAPNG(Stream fs)
        {
            //IEND chunk
            fs.WriteInt(0);
            fs.WriteASCIIString("IEND");
            fs.Write(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, 0, 4);
        }

        internal enum FilterModes
        {
            None = 0,
            Sub = 1,
            Up = 2,
            Average = 3,
            Paeth = 4,
            Adaptive = -1
        }

        internal unsafe static void FilterRow(byte* image, int width, int pixelSize, int stride, int y, FilterModes filter, byte* destinationImage, byte* tempBuffer)
        {
            int startIndex = y * stride;
            int destinationIndex = y * (stride + 1);

            if (filter == FilterModes.None)
            {
                destinationImage[destinationIndex++] = 0;
                for (int x = 0; x < width; x++)
                {
                    destinationImage[destinationIndex++] = image[startIndex++];
                    destinationImage[destinationIndex++] = image[startIndex++];
                    destinationImage[destinationIndex++] = image[startIndex++];

                    if (pixelSize == 4)
                    {
                        destinationImage[destinationIndex++] = image[startIndex++];
                    }

                }
            }
            else if (filter == FilterModes.Sub)
            {
                destinationImage[destinationIndex++] = 1;
                for (int x = 0; x < width; x++)
                {
                    if (x > 0)
                    {
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize]);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize]);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize]);
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize]);
                        }
                    }
                    else
                    {
                        destinationImage[destinationIndex++] = image[startIndex++];
                        destinationImage[destinationIndex++] = image[startIndex++];
                        destinationImage[destinationIndex++] = image[startIndex++];
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = image[startIndex++];
                        }
                    }
                }
            }
            else if (filter == FilterModes.Up)
            {
                destinationImage[destinationIndex++] = 2;
                for (int x = 0; x < width; x++)
                {
                    if (y > 0)
                    {
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride]);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride]);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride]);
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride]);
                        }
                    }
                    else
                    {
                        destinationImage[destinationIndex++] = image[startIndex++];
                        destinationImage[destinationIndex++] = image[startIndex++];
                        destinationImage[destinationIndex++] = image[startIndex++];
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = image[startIndex++];
                        }
                    }
                }
            }
            else if (filter == FilterModes.Average)
            {
                destinationImage[destinationIndex++] = 3;
                for (int x = 0; x < width; x++)
                {
                    if (x > 0 && y > 0)
                    {
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - (image[startIndex - pixelSize] + image[startIndex++ - stride]) / 2);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - (image[startIndex - pixelSize] + image[startIndex++ - stride]) / 2);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - (image[startIndex - pixelSize] + image[startIndex++ - stride]) / 2);
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = (byte)(image[startIndex] - (image[startIndex - pixelSize] + image[startIndex++ - stride]) / 2);
                        }
                    }
                    else if (x > 0 && y == 0)
                    {
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize] / 2);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize] / 2);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize] / 2);
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize] / 2);
                        }
                    }
                    else if (x == 0 && y > 0)
                    {
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride] / 2);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride] / 2);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride] / 2);
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride] / 2);
                        }
                    }
                    else
                    {
                        destinationImage[destinationIndex++] = image[startIndex++];
                        destinationImage[destinationIndex++] = image[startIndex++];
                        destinationImage[destinationIndex++] = image[startIndex++];
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = image[startIndex++];
                        }
                    }
                }
            }
            else if (filter == FilterModes.Paeth)
            {
                destinationImage[destinationIndex++] = 4;
                for (int x = 0; x < width; x++)
                {
                    if (x > 0 && y > 0)
                    {
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - PaethPredictor(image[startIndex - pixelSize], image[startIndex - stride], image[startIndex++ - pixelSize - stride]));
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - PaethPredictor(image[startIndex - pixelSize], image[startIndex - stride], image[startIndex++ - pixelSize - stride]));
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - PaethPredictor(image[startIndex - pixelSize], image[startIndex - stride], image[startIndex++ - pixelSize - stride]));
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = (byte)(image[startIndex] - PaethPredictor(image[startIndex - pixelSize], image[startIndex - stride], image[startIndex++ - pixelSize - stride]));
                        }
                    }
                    else if (x > 0 && y == 0)
                    {
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize]);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize]);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize]);
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - pixelSize]);
                        }
                    }
                    else if (x == 0 && y > 0)
                    {
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride]);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride]);
                        destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride]);
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = (byte)(image[startIndex] - image[startIndex++ - stride]);
                        }
                    }
                    else
                    {
                        destinationImage[destinationIndex++] = image[startIndex++];
                        destinationImage[destinationIndex++] = image[startIndex++];
                        destinationImage[destinationIndex++] = image[startIndex++];
                        if (pixelSize == 4)
                        {
                            destinationImage[destinationIndex++] = image[startIndex++];
                        }
                    }
                }
            }
            else if (filter == FilterModes.Adaptive)
            {
                int tempIndex = 0;

                int none = 0;
                int sub = 0;
                int up = 0;
                int average = 0;
                int paeth = 0;

                for (int x = 0; x < width; x++)
                {
                    for (int i = 0; i < pixelSize; i++)
                    {
                        //none
                        tempBuffer[tempIndex] = image[startIndex];

                        //sub
                        if (x > 0)
                        {
                            tempBuffer[tempIndex + stride] = (byte)(image[startIndex] - image[startIndex - pixelSize]);
                        }
                        else
                        {
                            tempBuffer[tempIndex + stride] = image[startIndex];
                        }

                        //up
                        if (y > 0)
                        {
                            tempBuffer[tempIndex + stride * 2] = (byte)(image[startIndex] - image[startIndex - stride]);
                        }
                        else
                        {
                            tempBuffer[tempIndex + stride * 2] = image[startIndex];
                        }

                        //average
                        if (x > 0 && y > 0)
                        {
                            tempBuffer[tempIndex + stride * 3] = (byte)(image[startIndex] - (image[startIndex - pixelSize] + image[startIndex - stride]) / 2);
                        }
                        else if (x > 0 && y == 0)
                        {
                            tempBuffer[tempIndex + stride * 3] = (byte)(image[startIndex] - image[startIndex - pixelSize] / 2);
                        }
                        else if (x == 0 && y > 0)
                        {
                            tempBuffer[tempIndex + stride * 3] = (byte)(image[startIndex] - image[startIndex - stride] / 2);
                        }
                        else
                        {
                            tempBuffer[tempIndex + stride * 3] = image[startIndex];
                        }

                        //paeth
                        if (x > 0 && y > 0)
                        {
                            tempBuffer[tempIndex + stride * 4] = (byte)(image[startIndex] - PaethPredictor(image[startIndex - pixelSize], image[startIndex - stride], image[startIndex - pixelSize - stride]));
                        }
                        else if (x > 0 && y == 0)
                        {
                            tempBuffer[tempIndex + stride * 4] = (byte)(image[startIndex] - image[startIndex - pixelSize]);
                        }
                        else if (x == 0 && y > 0)
                        {
                            tempBuffer[tempIndex + stride * 4] = (byte)(image[startIndex] - image[startIndex - stride]);
                        }
                        else
                        {
                            tempBuffer[tempIndex + stride * 4] = image[startIndex];
                        }

                        none += tempBuffer[tempIndex];
                        sub += tempBuffer[tempIndex + stride];
                        up += tempBuffer[tempIndex + stride * 2];
                        average += tempBuffer[tempIndex + stride * 3];
                        paeth += tempBuffer[tempIndex + stride * 4];
                        tempIndex++;
                        startIndex++;
                    }
                }

                if (paeth <= average && paeth <= up && paeth <= sub && paeth <= none)
                {
                    destinationImage[destinationIndex++] = 4;
                    tempIndex = stride * 4;
                    for (int i = 0; i < stride; i++)
                    {
                        destinationImage[destinationIndex++] = tempBuffer[tempIndex++];
                    }
                }
                else if (average <= up && average <= sub && average <= none)
                {
                    destinationImage[destinationIndex++] = 3;
                    tempIndex = stride * 3;
                    for (int i = 0; i < stride; i++)
                    {
                        destinationImage[destinationIndex++] = tempBuffer[tempIndex++];
                    }
                }
                else if (up <= sub && up <= none)
                {
                    destinationImage[destinationIndex++] = 2;
                    tempIndex = stride * 2;
                    for (int i = 0; i < stride; i++)
                    {
                        destinationImage[destinationIndex++] = tempBuffer[tempIndex++];
                    }
                }
                else if (sub <= none)
                {
                    destinationImage[destinationIndex++] = 1;
                    tempIndex = stride;
                    for (int i = 0; i < stride; i++)
                    {
                        destinationImage[destinationIndex++] = tempBuffer[tempIndex++];
                    }
                }
                else
                {
                    destinationImage[destinationIndex++] = 0;
                    tempIndex = 0;
                    for (int i = 0; i < stride; i++)
                    {
                        destinationImage[destinationIndex++] = tempBuffer[tempIndex++];
                    }
                }
            }
        }

        internal unsafe static IntPtr FilterImageData(byte* image, int width, int height, int pixelSize, FilterModes filter)
        {
            IntPtr filteredMemory = Marshal.AllocHGlobal(height * (1 + width * pixelSize));

            IntPtr tempBuffer;

            if (filter == FilterModes.Adaptive)
            {
                tempBuffer = Marshal.AllocHGlobal(5 * width * pixelSize);
            }
            else
            {
                tempBuffer = IntPtr.Zero;
            }

            int stride = width * pixelSize;

            byte* filteredPointer = (byte*)filteredMemory;

            byte* tempPointer = (byte*)tempBuffer;

            for (int y = 0; y < height; y++)
            {
                FilterRow(image, width, pixelSize, stride, y, filter, filteredPointer, tempPointer);
            }

            Marshal.FreeHGlobal(tempBuffer);

            return filteredMemory;
        }

        internal unsafe static IntPtr FilterImageData(byte* image, int width, int height, int pixelSize, FilterModes filter, int threadCount)
        {
            threadCount = Math.Min(threadCount, height);

            IntPtr filteredMemory = Marshal.AllocHGlobal(height * (1 + width * pixelSize));

            int stride = width * pixelSize;

            byte* filteredPointer = (byte*)filteredMemory;

            IntPtr[] tempBuffers = new IntPtr[threadCount];

            Thread[] threads = new Thread[threadCount];
            int[] rowsByThread = new int[threadCount];
            EventWaitHandle[] signalsFromThreads = new EventWaitHandle[threadCount];
            EventWaitHandle[] signalsToThreads = new EventWaitHandle[threadCount];

            int firstNeededRow = threadCount;

            for (int i = 0; i < threadCount; i++)
            {
                rowsByThread[i] = i;
                signalsFromThreads[i] = new EventWaitHandle(false, EventResetMode.ManualReset);
                signalsToThreads[i] = new EventWaitHandle(false, EventResetMode.ManualReset);

                if (filter == FilterModes.Adaptive)
                {
                    tempBuffers[i] = Marshal.AllocHGlobal(5 * width * pixelSize);
                }
                else
                {
                    tempBuffers[i] = IntPtr.Zero;
                }

                byte* tempPointer = (byte*)tempBuffers[i];

                int threadIndex = i;

                threads[i] = new Thread(() =>
                {
                    while (rowsByThread[threadIndex] >= 0)
                    {
                        FilterRow(image, width, pixelSize, stride, rowsByThread[threadIndex], filter, filteredPointer, tempPointer);
                        EventWaitHandle.SignalAndWait(signalsFromThreads[threadIndex], signalsToThreads[threadIndex]);
                        signalsToThreads[threadIndex].Reset();
                    }
                });
            }

            int finished = 0;

            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Start();
            }

            while (finished < threadCount)
            {
                int threadInd = EventWaitHandle.WaitAny(signalsFromThreads);

                signalsFromThreads[threadInd].Reset();

                if (firstNeededRow < height)
                {
                    rowsByThread[threadInd] = firstNeededRow;
                    firstNeededRow++;
                    signalsToThreads[threadInd].Set();
                }
                else
                {
                    rowsByThread[threadInd] = -1;
                    signalsToThreads[threadInd].Set();
                    finished++;
                }
            }

            for (int i = 0; i < threadCount; i++)
            {
                if (filter == FilterModes.Adaptive)
                {
                    Marshal.FreeHGlobal(tempBuffers[i]);
                }
            }

            return filteredMemory;
        }

        //Based on http://www.libpng.org/pub/png/spec/1.2/PNG-Filters.html
        internal static byte PaethPredictor(byte a, byte b, byte c)
        {
            int p = (int)a + (int)b - (int)c;
            int pa = Math.Abs(p - (int)a);
            int pb = Math.Abs(p - (int)b);
            int pc = Math.Abs(p - (int)c);

            if (pa <= pb && pa <= pc)
            {
                return a;
            }
            else if (pb <= pc)
            {
                return b;
            }
            else
            {
                return c;
            }
        }
    }

    //Derived from http://www.libpng.org/pub/png/spec/1.2/PNG-CRCAppendix.html
    internal static class CRC32
    {
        /* Table of CRCs of all 8-bit messages. */
        private static readonly uint[] crc_table = new uint[256];

        /* Flag: has the table been computed? Initially false. */
        private static bool crc_table_computed = false;

        /* Make the table for a fast CRC. */
        private static void MakeCRCtable()
        {
            uint c;
            int n, k;

            for (n = 0; n < 256; n++)
            {
                c = (uint)n;
                for (k = 0; k < 8; k++)
                {
                    if ((c & 1) != 0)
                    {
                        c = 0xedb88320 ^ (c >> 1);
                    }
                    else
                    {
                        c >>= 1;
                    }
                }
                crc_table[n] = c;
            }
            crc_table_computed = true;
        }

        /* Update a running CRC with the bytes buf[0..len-1]--the CRC
           should be initialized to all 1's, and the transmitted value
           is the 1's complement of the final running CRC (see the
           crc() routine below)). */

        private static uint UpdateCRC(uint crc, Stream buf)
        {
            uint c = crc;
            int n;

            if (!crc_table_computed)
            {
                MakeCRCtable();
            }

            buf.Seek(0, SeekOrigin.Begin);

            for (n = 0; n < buf.Length; n++)
            {
                c = crc_table[(c ^ (byte)buf.ReadByte()) & 0xff] ^ (c >> 8);
            }
            return c;
        }

        private static uint UpdateCRC(uint crc, byte[] buf, int length)
        {
            uint c = crc;
            int n;

            if (!crc_table_computed)
            {
                MakeCRCtable();
            }

            for (n = 0; n < length; n++)
            {
                c = crc_table[(c ^ buf[n]) & 0xff] ^ (c >> 8);
            }
            return c;
        }

        /* Return the CRC of the bytes buf[0..len-1]. */
        public static uint ComputeCRC(Stream buf)
        {
            return UpdateCRC(0xffffffff, buf) ^ 0xffffffff;
        }

        public static uint ComputeCRC(byte[] buf, int length, byte[] prefix)
        {
            uint temp = UpdateCRC(0xffffffff, prefix, prefix.Length);
            return UpdateCRC(temp, buf, length) ^ 0xffffffff;
        }
    }

    internal static class StreamUtils
    {
        public static void WriteASCIIString(this Stream sr, string val)
        {
            foreach (char c in val)
            {
                sr.WriteByte((byte)c);
            }
        }

        internal static MemoryStream ZLibCompress(IntPtr memoryAddress, int memoryLength)
        {
            MemoryStream compressedStream = new MemoryStream();
            compressedStream.Write(new byte[] { 0x78, 0x01 }, 0, 2);

            byte[] buffer = new byte[1024];

            AdlerHolder adlerHolder = new AdlerHolder();

            using (DeflateStream deflate = new DeflateStream(compressedStream, CompressionLevel.Optimal, true))
            {
                while (memoryLength > 0)
                {
                    int currentBytes = Math.Min(memoryLength, 1024);
                    Marshal.Copy(memoryAddress, buffer, 0, currentBytes);
                    deflate.Write(buffer, 0, currentBytes);
                    memoryLength -= currentBytes;
                    memoryAddress = IntPtr.Add(memoryAddress, currentBytes);
                    Adler32(buffer, currentBytes, adlerHolder);
                }
            }


            uint checksum = (adlerHolder.s2 << 16) + adlerHolder.s1;

            compressedStream.Write(new byte[] { (byte)((checksum >> 24) & 255), (byte)((checksum >> 16) & 255), (byte)((checksum >> 8) & 255), (byte)(checksum & 255) }, 0, 4);

            compressedStream.Seek(0, SeekOrigin.Begin);

            return compressedStream;
        }

        internal static uint Adler32(Stream contentStream)
        {
            uint s1 = 1;
            uint s2 = 0;

            int readByte;

            while ((readByte = contentStream.ReadByte()) >= 0)
            {
                s1 = (s1 + (byte)readByte) % 65521U;
                s2 = (s2 + s1) % 65521U;
            }

            return (s2 << 16) + s1;
        }


        internal class AdlerHolder
        {
            public uint s1;
            public uint s2;

            public AdlerHolder()
            {
                s1 = 1;
                s2 = 0;
            }
        }

        internal static void Adler32(byte[] buffer, int length, AdlerHolder holder)
        {
            for (int i = 0; i < length; i++)
            {
                holder.s1 = (holder.s1 + buffer[i]) % 65521U;
                holder.s2 = (holder.s2 + holder.s1) % 65521U;
            }
        }
    }
}
