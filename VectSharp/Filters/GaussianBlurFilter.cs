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
    /// Represents a filter that applies a Gaussian blur effect.
    /// </summary>
    public class GaussianBlurFilter : ILocationInvariantFilter
    {
        /// <summary>
        /// The standard deviation of the Gaussian blur.
        /// </summary>
        public double StandardDeviation { get; }

        /// <inheritdoc/>
        public Point TopLeftMargin { get; }

        /// <inheritdoc/>
        public Point BottomRightMargin { get; }

        /// <summary>
        /// Creates a new <see cref="GaussianBlurFilter"/> with the specified standard deviation.
        /// </summary>
        /// <param name="standardDeviation">The standard deviation of the Gaussian blur.</param>
        public GaussianBlurFilter(double standardDeviation)
        {
            this.StandardDeviation = standardDeviation;
            this.TopLeftMargin = new Point(standardDeviation * 3, standardDeviation * 3);
            this.BottomRightMargin = new Point(standardDeviation * 3, standardDeviation * 3);
        }

        /// <inheritdoc/>
        [Pure]
        public RasterImage Filter(RasterImage image, double scale)
        {
            return FilterSRGB(image, scale);
        }

        private RasterImage FilterSRGB(RasterImage image, double scale)
        {
            if (this.StandardDeviation * scale > 2)
            {
                int[] boxes = BoxesForGauss(this.StandardDeviation * scale, 3);

                for (int i = 0; i < boxes.Length; i++)
                {
                    BoxBlurFilter box = new BoxBlurFilter((boxes[i] - 1) / 2);

                    image = box.Filter(image, 1);
                }
            }
            else
            {
                image = FilterSRGBExact(image, this.StandardDeviation * scale);
            }

            return image;
        }

        // Adapted from http://blog.ivank.net/fastest-gaussian-blur.html
        private static int[] BoxesForGauss(double standardDeviation, int count)
        {
            double idealWeight = Math.Sqrt((12 * standardDeviation * standardDeviation / count) + 1);

            int lowerWeight = (int)Math.Floor(idealWeight);

            if (lowerWeight % 2 == 0)
            {
                lowerWeight--;
            }

            int upperWeight = lowerWeight + 2;

            double idealSize = (12 * standardDeviation * standardDeviation - count * lowerWeight * lowerWeight - 4 * count * lowerWeight - 3 * count) / (-4 * lowerWeight - 4);

            int m = (int)Math.Round(idealSize);

            int[] sizes = new int[count];

            for (int i = 0; i < count; i++)
            {
                sizes[i] = i < m ? lowerWeight : upperWeight;
            }

            return sizes;
        }

        private RasterImage FilterSRGBExact(RasterImage image, double standardDeviation)
        {
            IntPtr intermediateData = System.Runtime.InteropServices.Marshal.AllocHGlobal(image.Width * image.Height * (image.HasAlpha ? 4 : 3));
            IntPtr tbrData = System.Runtime.InteropServices.Marshal.AllocHGlobal(image.Width * image.Height * (image.HasAlpha ? 4 : 3));
            GC.AddMemoryPressure(2 * image.Width * image.Height * (image.HasAlpha ? 4 : 3));

            int width = image.Width;
            int height = image.Height;

            int kernelSize = (int)Math.Ceiling(standardDeviation * 3);

            double[] kernel = new double[kernelSize * 2 + 1];

            double total = 0;

            for (int i = 0; i <= kernelSize; i++)
            {
                kernel[i] = Math.Exp(-(kernelSize - i) * (kernelSize - i) / (2 * standardDeviation * standardDeviation));

                if (i < kernelSize)
                {
                    kernel[2 * kernelSize - i] = kernel[i];

                    total += kernel[i] * 2;
                }
                else
                {
                    total += kernel[i];
                }
            }

            for (int i = 0; i < kernel.Length; i++)
            {
                kernel[i] /= total;
            }

            int pixelSize = image.HasAlpha ? 4 : 3;
            int stride = image.Width * pixelSize;

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
                        for (int x = 0; x < width; x++)
                        {
                            double R = 0;
                            double G = 0;
                            double B = 0;
                            double weight = 0;

                            for (int targetX = 0; targetX <= kernelSize * 2; targetX++)
                            {
                                int tX = Math.Min(Math.Max(0, x + targetX - kernelSize), width - 1);

                                double a = input[y * stride + tX * 4 + 3] / 255.0;

                                weight += kernel[targetX] * a;

                                R += kernel[targetX] * input[y * stride + tX * 4] * a;
                                G += kernel[targetX] * input[y * stride + tX * 4 + 1] * a;
                                B += kernel[targetX] * input[y * stride + tX * 4 + 2] * a;
                            }

                            if (weight != 0)
                            {
                                intermediate[y * stride + x * 4] = (byte)Math.Min(255, Math.Max(0, R / weight));
                                intermediate[y * stride + x * 4 + 1] = (byte)Math.Min(255, Math.Max(0, G / weight));
                                intermediate[y * stride + x * 4 + 2] = (byte)Math.Min(255, Math.Max(0, B / weight));
                                intermediate[y * stride + x * 4 + 3] = (byte)Math.Min(255, Math.Max(0, weight * 255));
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
                        for (int x = 0; x < width; x++)
                        {
                            double R = 0;
                            double G = 0;
                            double B = 0;

                            for (int targetX = 0; targetX <= kernelSize * 2; targetX++)
                            {
                                int tX = Math.Min(Math.Max(0, x + targetX - kernelSize), width - 1);

                                R += kernel[targetX] * input[y * stride + tX * 3];
                                G += kernel[targetX] * input[y * stride + tX * 3 + 1];
                                B += kernel[targetX] * input[y * stride + tX * 3 + 2];
                            }

                            intermediate[y * stride + x * 3] = (byte)Math.Min(255, Math.Max(0, R));
                            intermediate[y * stride + x * 3 + 1] = (byte)Math.Min(255, Math.Max(0, G));
                            intermediate[y * stride + x * 3 + 2] = (byte)Math.Min(255, Math.Max(0, B));
                        }
                    };
                }

                Action<int> xLoop;

                if (image.HasAlpha)
                {
                    xLoop = y =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double R = 0;
                            double G = 0;
                            double B = 0;


                            double weight = 0;

                            for (int targetY = 0; targetY <= kernelSize * 2; targetY++)
                            {
                                int tY = Math.Min(Math.Max(0, y + targetY - kernelSize), height - 1);

                                double a = intermediate[tY * stride + x * 4 + 3] / 255.0;

                                weight += kernel[targetY] * a;

                                R += kernel[targetY] * intermediate[tY * stride + x * 4] * a;
                                G += kernel[targetY] * intermediate[tY * stride + x * 4 + 1] * a;
                                B += kernel[targetY] * intermediate[tY * stride + x * 4 + 2] * a;
                            }

                            if (weight != 0)
                            {
                                output[y * stride + x * 4] = (byte)Math.Min(255, Math.Max(0, R / weight));
                                output[y * stride + x * 4 + 1] = (byte)Math.Min(255, Math.Max(0, G / weight));
                                output[y * stride + x * 4 + 2] = (byte)Math.Min(255, Math.Max(0, B / weight));
                                output[y * stride + x * 4 + 3] = (byte)Math.Min(255, Math.Max(0, weight * 255));
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
                    xLoop = y =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double R = 0;
                            double G = 0;
                            double B = 0;

                            for (int targetY = 0; targetY <= kernelSize * 2; targetY++)
                            {
                                int tY = Math.Min(Math.Max(0, y + targetY - kernelSize), height - 1);

                                R += kernel[targetY] * intermediate[tY * stride + x * 3];
                                G += kernel[targetY] * intermediate[tY * stride + x * 3 + 1];
                                B += kernel[targetY] * intermediate[tY * stride + x * 3 + 2];
                            }

                            output[y * stride + x * 3] = (byte)Math.Min(255, Math.Max(0, R));
                            output[y * stride + x * 3 + 1] = (byte)Math.Min(255, Math.Max(0, G));
                            output[y * stride + x * 3 + 2] = (byte)Math.Min(255, Math.Max(0, B));
                        }
                    };
                }



                if (threads == 1)
                {
                    for (int y = 0; y < height; y++)
                    {
                        yLoop(y);
                    }

                    for (int y = 0; y < height; y++)
                    {
                        xLoop(y);
                    }
                }
                else
                {
                    ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = threads };

                    Parallel.For(0, height, options, yLoop);
                    Parallel.For(0, height, options, xLoop);
                }
            }

            System.Runtime.InteropServices.Marshal.FreeHGlobal(intermediateData);
            GC.RemoveMemoryPressure(image.Width * image.Height * (image.HasAlpha ? 4 : 3));

            DisposableIntPtr disp = new DisposableIntPtr(tbrData);

            return new RasterImage(ref disp, image.Width, image.Height, image.HasAlpha, image.Interpolate);
        }

    }
}

