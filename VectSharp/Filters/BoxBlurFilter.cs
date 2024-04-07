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
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace VectSharp.Filters
{
    /// <summary>
    /// Represents a filter applying a box blur.
    /// </summary>
    public class BoxBlurFilter : ILocationInvariantFilter
    {
        /// <summary>
        /// The radius of the box blur (the actual size of the box is 2 * <see cref="BoxRadius"/> + 1).
        /// </summary>
        public double BoxRadius { get; }

        /// <inheritdoc/>
        public Point TopLeftMargin { get; }
        /// <inheritdoc/>
        public Point BottomRightMargin { get; }

        /// <summary>
        /// Creates a new <see cref="BoxBlurFilter"/> with the specified radius.
        /// </summary>
        /// <param name="boxRadius">The radius of the box blur (the actual size of the box is 2 * <paramref name="boxRadius"/> + 1).</param>
        public BoxBlurFilter(double boxRadius)
        {
            this.BoxRadius = boxRadius;
            this.TopLeftMargin = new Point(boxRadius, boxRadius);
            this.BottomRightMargin = new Point(boxRadius, boxRadius);
        }

        /// <inheritdoc/>
        [Pure]
        public RasterImage Filter(RasterImage image, double scale)
        {
            return BoxBlurSRGB(image, (int)Math.Round(this.BoxRadius * scale));
        }

        private RasterImage BoxBlurSRGB(RasterImage image, int boxRadius)
        {
            IntPtr intermediateData = System.Runtime.InteropServices.Marshal.AllocHGlobal(image.Width * image.Height * (image.HasAlpha ? 4 : 3));
            IntPtr tbrData = System.Runtime.InteropServices.Marshal.AllocHGlobal(image.Width * image.Height * (image.HasAlpha ? 4 : 3));
            GC.AddMemoryPressure(2 * image.Width * image.Height * (image.HasAlpha ? 4 : 3));

            int width = image.Width;
            int height = image.Height;

            int pixelSize = image.HasAlpha ? 4 : 3;
            int stride = image.Width * pixelSize;

            double normFactor = 1.0 / (2 * boxRadius + 1);

            int threads;

            double size = Math.Sqrt((double)image.Width * image.Height);

            if (size <= 128)
            {
                threads = 1;
            }
            else if (size <= 512)
            {
                threads = Math.Min(4, Environment.ProcessorCount);
            }
            else
            {
                threads = Math.Min(8, Environment.ProcessorCount);
            }

            unsafe
            {
                byte* input = (byte*)image.ImageDataAddress;
                byte* intermediate = (byte*)intermediateData;
                byte* output = (byte*)tbrData;

                Action<int> yLoop;

                if (image.HasAlpha)
                {
                    yLoop = y =>
                    {
                        double rAcc = 0;
                        double gAcc = 0;
                        double bAcc = 0;
                        double aAcc = 0;

                        double prevR = 0;
                        double prevG = 0;
                        double prevB = 0;
                        double prevA = 0;

                        double r = 0;
                        double g = 0;
                        double b = 0;

                        double a = input[y * stride + 3] / 255.0;

                        prevR = input[y * stride] * a;
                        prevG = input[y * stride + 1] * a;
                        prevB = input[y * stride + 2] * a;
                        prevA = a;

                        rAcc = prevR * (boxRadius + 1);
                        gAcc = prevG * (boxRadius + 1);
                        bAcc = prevB * (boxRadius + 1);
                        aAcc = a * (boxRadius + 1);

                        int rX = 0;

                        for (int x = 0; x < boxRadius; x++)
                        {
                            rX = Math.Min(x, width - 1);

                            a = input[y * stride + rX * 4 + 3] / 255.0;

                            rAcc += input[y * stride + rX * 4] * a;
                            gAcc += input[y * stride + rX * 4 + 1] * a;
                            bAcc += input[y * stride + rX * 4 + 2] * a;
                            aAcc += a;
                        }

                        rX = Math.Min(boxRadius, width - 1);
                        int lX = 0;

                        for (int x = 0; x < width; x++)
                        {
                            rX = Math.Min(x + boxRadius, width - 1);
                            lX = Math.Max(x - boxRadius - 1, 0);

                            a = input[y * stride + rX * 4 + 3] / 255.0;
                            r = input[y * stride + rX * 4] * a;
                            g = input[y * stride + rX * 4 + 1] * a;
                            b = input[y * stride + rX * 4 + 2] * a;

                            prevA = input[y * stride + lX * 4 + 3] / 255.0;
                            prevR = input[y * stride + lX * 4] * prevA;
                            prevG = input[y * stride + lX * 4 + 1] * prevA;
                            prevB = input[y * stride + lX * 4 + 2] * prevA;

                            rAcc += r - prevR;
                            gAcc += g - prevG;
                            bAcc += b - prevB;
                            aAcc += a - prevA;

                            if (aAcc != 0)
                            {
                                intermediate[y * stride + x * 4] = (byte)Math.Min(255, Math.Max(0, rAcc / aAcc));
                                intermediate[y * stride + x * 4 + 1] = (byte)Math.Min(255, Math.Max(0, gAcc / aAcc));
                                intermediate[y * stride + x * 4 + 2] = (byte)Math.Min(255, Math.Max(0, bAcc / aAcc));
                                intermediate[y * stride + x * 4 + 3] = (byte)Math.Min(255, Math.Max(0, aAcc * 255 * normFactor));
                            }
                            else
                            {
                                intermediate[y * stride + x * 4] = 0;
                                intermediate[y * stride + x * 4 + 1] = 0;
                                intermediate[y * stride + x * 4 + 2] = 0;
                                intermediate[y * stride + x * 4 + 3] = 0;
                            }
                        }
                    };
                }
                else
                {
                    yLoop = y =>
                    {
                        double rAcc = 0;
                        double gAcc = 0;
                        double bAcc = 0;

                        double prevR = 0;
                        double prevG = 0;
                        double prevB = 0;

                        double r = 0;
                        double g = 0;
                        double b = 0;

                        prevR = input[y * stride];
                        prevG = input[y * stride + 1];
                        prevB = input[y * stride + 2];

                        rAcc = prevR * (boxRadius + 1);
                        gAcc = prevG * (boxRadius + 1);
                        bAcc = prevB * (boxRadius + 1);

                        int rX = 0;

                        for (int x = 0; x < boxRadius; x++)
                        {
                            rX = Math.Min(x, width - 1);

                            rAcc += input[y * stride + rX * 3];
                            gAcc += input[y * stride + rX * 3 + 1];
                            bAcc += input[y * stride + rX * 3 + 2];
                        }

                        rX = Math.Min(boxRadius, width - 1);
                        int lX = 0;

                        for (int x = 0; x < width; x++)
                        {
                            rX = Math.Min(x + boxRadius, width - 1);
                            lX = Math.Max(x - boxRadius - 1, 0);

                            r = input[y * stride + rX * 3];
                            g = input[y * stride + rX * 3 + 1];
                            b = input[y * stride + rX * 3 + 2];

                            prevR = input[y * stride + lX * 3];
                            prevG = input[y * stride + lX * 3 + 1];
                            prevB = input[y * stride + lX * 3 + 2];

                            rAcc += r - prevR;
                            gAcc += g - prevG;
                            bAcc += b - prevB;

                            intermediate[y * stride + x * 3] = (byte)Math.Min(255, Math.Max(0, rAcc * normFactor));
                            intermediate[y * stride + x * 3 + 1] = (byte)Math.Min(255, Math.Max(0, gAcc * normFactor));
                            intermediate[y * stride + x * 3 + 2] = (byte)Math.Min(255, Math.Max(0, bAcc * normFactor));
                        }
                    };
                }

                Action<int> xLoop;

                if (image.HasAlpha)
                {
                    xLoop = x =>
                    {
                        double rAcc = 0;
                        double gAcc = 0;
                        double bAcc = 0;
                        double aAcc = 0;

                        double prevR = 0;
                        double prevG = 0;
                        double prevB = 0;
                        double prevA = 0;

                        double r = 0;
                        double g = 0;
                        double b = 0;

                        double a = intermediate[x * 4 + 3] / 255.0;

                        prevR = intermediate[x * 4] * a;
                        prevG = intermediate[x * 4 + 1] * a;
                        prevB = intermediate[x * 4 + 2] * a;
                        prevA = a;

                        rAcc = prevR * (boxRadius + 1);
                        gAcc = prevG * (boxRadius + 1);
                        bAcc = prevB * (boxRadius + 1);
                        aAcc = a * (boxRadius + 1);

                        int rY = 0;

                        for (int y = 0; y < boxRadius; y++)
                        {
                            rY = Math.Min(y, height - 1);

                            a = intermediate[rY * stride + x * 4 + 3] / 255.0;

                            rAcc += intermediate[rY * stride + x * 4] * a;
                            gAcc += intermediate[rY * stride + x * 4 + 1] * a;
                            bAcc += intermediate[rY * stride + x * 4 + 2] * a;
                            aAcc += a;
                        }

                        rY = Math.Min(boxRadius, height - 1);
                        int lY = 0;

                        for (int y = 0; y < height; y++)
                        {
                            rY = Math.Min(y + boxRadius, height - 1);
                            lY = Math.Max(y - boxRadius - 1, 0);

                            a = intermediate[rY * stride + x * 4 + 3] / 255.0;
                            r = intermediate[rY * stride + x * 4] * a;
                            g = intermediate[rY * stride + x * 4 + 1] * a;
                            b = intermediate[rY * stride + x * 4 + 2] * a;

                            prevA = intermediate[lY * stride + x * 4 + 3] / 255.0;
                            prevR = intermediate[lY * stride + x * 4] * prevA;
                            prevG = intermediate[lY * stride + x * 4 + 1] * prevA;
                            prevB = intermediate[lY * stride + x * 4 + 2] * prevA;

                            rAcc += r - prevR;
                            gAcc += g - prevG;
                            bAcc += b - prevB;
                            aAcc += a - prevA;

                            if (aAcc != 0)
                            {
                                output[y * stride + x * 4] = (byte)Math.Min(255, Math.Max(0, rAcc / aAcc));
                                output[y * stride + x * 4 + 1] = (byte)Math.Min(255, Math.Max(0, gAcc / aAcc));
                                output[y * stride + x * 4 + 2] = (byte)Math.Min(255, Math.Max(0, bAcc / aAcc));
                                output[y * stride + x * 4 + 3] = (byte)Math.Min(255, Math.Max(0, aAcc * 255 * normFactor));
                            }
                            else
                            {
                                output[y * stride + x * 4] = 0;
                                output[y * stride + x * 4 + 1] = 0;
                                output[y * stride + x * 4 + 2] = 0;
                                output[y * stride + x * 4 + 3] = 0;
                            }
                        }
                    };
                }
                else
                {
                    xLoop = x =>
                    {
                        double rAcc = 0;
                        double gAcc = 0;
                        double bAcc = 0;

                        double prevR = 0;
                        double prevG = 0;
                        double prevB = 0;

                        double r = 0;
                        double g = 0;
                        double b = 0;

                        prevR = intermediate[x * 3];
                        prevG = intermediate[x * 3 + 1];
                        prevB = intermediate[x * 3 + 2];

                        rAcc = prevR * (boxRadius + 1);
                        gAcc = prevG * (boxRadius + 1);
                        bAcc = prevB * (boxRadius + 1);

                        int rY = 0;

                        for (int y = 0; y < boxRadius; y++)
                        {
                            rY = Math.Min(y, height - 1);

                            rAcc += intermediate[rY * stride + x * 3];
                            gAcc += intermediate[rY * stride + x * 3 + 1];
                            bAcc += intermediate[rY * stride + x * 3 + 2];
                        }

                        rY = Math.Min(boxRadius, height - 1);
                        int lY = 0;

                        for (int y = 0; y < height; y++)
                        {
                            rY = Math.Min(y + boxRadius, height - 1);
                            lY = Math.Max(y - boxRadius - 1, 0);

                            r = intermediate[rY * stride + x * 3];
                            g = intermediate[rY * stride + x * 3 + 1];
                            b = intermediate[rY * stride + x * 3 + 2];

                            prevR = intermediate[lY * stride + x * 3];
                            prevG = intermediate[lY * stride + x * 3 + 1];
                            prevB = intermediate[lY * stride + x * 3 + 2];

                            rAcc += r - prevR;
                            gAcc += g - prevG;
                            bAcc += b - prevB;

                            output[y * stride + x * 3] = (byte)Math.Min(255, Math.Max(0, rAcc * normFactor));
                            output[y * stride + x * 3 + 1] = (byte)Math.Min(255, Math.Max(0, gAcc * normFactor));
                            output[y * stride + x * 3 + 2] = (byte)Math.Min(255, Math.Max(0, bAcc * normFactor));
                        }
                    };
                }

                if (threads == 1)
                {
                    for (int y = 0; y < height; y++)
                    {
                        yLoop(y);
                    }

                    for (int x = 0; x < width; x++)
                    {
                        xLoop(x);
                    }
                }
                else
                {
                    ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = threads };

                    Parallel.For(0, height, options, yLoop);
                    Parallel.For(0, width, options, xLoop);
                }
            }

            System.Runtime.InteropServices.Marshal.FreeHGlobal(intermediateData);
            GC.RemoveMemoryPressure(image.Width * image.Height * (image.HasAlpha ? 4 : 3));

            DisposableIntPtr disp = new DisposableIntPtr(tbrData);

            return new RasterImage(ref disp, image.Width, image.Height, image.HasAlpha, image.Interpolate);
        }
    }
}
