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
using System.Threading.Tasks;

namespace VectSharp.Filters
{
    /// <summary>
    /// Represents a filter that applies a matrix convolution to the image.
    /// </summary>
    public class ConvolutionFilter : ILocationInvariantFilter
    {
        /// <inheritdoc/>
        public Point TopLeftMargin { get; protected set; }
        /// <inheritdoc/>
        public Point BottomRightMargin { get; protected set; }

        /// <summary>
        /// The kernel of the <see cref="ConvolutionFilter"/>. The dimensions of this matrix should all be odd numbers. The larger the kernel, the worse the performance.
        /// </summary>
        public virtual double[,] Kernel { get; protected set; }

        /// <summary>
        /// The normalisation value that is applies to the kernel.
        /// </summary>
        public virtual double Normalisation { get; protected set; } = 1;

        /// <summary>
        /// The bias value that is added to every colour component when the filter is applied.
        /// </summary>
        public virtual double Bias { get; protected set; } = 0;

        /// <summary>
        /// The scale relating the size of the kernel to graphics units.
        /// </summary>
        public virtual double Scale { get; protected set; }

        /// <summary>
        /// If this is <see langword="true"/>, the alpha value of the input pixels is preserved. Otherwise, the alpha channel is subject to the same convolution process as the other colour components.
        /// </summary>
        public virtual bool PreserveAlpha { get; protected set; } = true;

        /// <summary>
        /// Creates a new <see cref="ConvolutionFilter"/> with the specified parameters.
        /// </summary>
        /// <param name="kernel">The kernel of the <see cref="ConvolutionFilter"/>. The dimensions of this matrix should all be odd numbers. The larger the kernel, the worse the performance.</param>
        /// <param name="scale">The scale relating the size of the kernel to graphics units.</param>
        /// <param name="preserveAlpha">If this is <see langword="true"/>, the alpha value of the input pixels is preserved. Otherwise, the alpha channel is subject to the same convolution process as the other colour components.</param>
        /// <param name="normalisation">The normalisation value that is applies to the kernel.</param>
        /// <param name="bias">The bias value that is added to every colour component when the filter is applied.</param>
        /// <exception cref="ArgumentException">This exception is thrown when the kernel dimensions are not odd numbers.</exception>
        public ConvolutionFilter(double[,] kernel, double scale, bool preserveAlpha = true, double normalisation = 1, double bias = 0)
        {
            if (kernel.GetLength(0) % 2 != 1 || kernel.GetLength(1) % 2 != 1)
            {
                throw new ArgumentException("The kernel must have an odd number of rows and columns!", nameof(kernel));
            }

            this.Kernel = kernel;

            int kernelWidth = (this.Kernel.GetLength(0) - 1) / 2;
            int kernelHeight = (this.Kernel.GetLength(1) - 1) / 2;

            this.TopLeftMargin = new Point(kernelWidth * scale, kernelHeight * scale);
            this.BottomRightMargin = new Point(kernelWidth * scale, kernelHeight * scale);
            this.Scale = scale;
            this.PreserveAlpha = preserveAlpha;
            this.Normalisation = normalisation;
            this.Bias = bias;
        }

        /// <inheritdoc/>
        public virtual RasterImage Filter(RasterImage image, double scale)
        {
            IntPtr tbrData = System.Runtime.InteropServices.Marshal.AllocHGlobal(image.Width * image.Height * (image.HasAlpha ? 4 : 3));

            int width = image.Width;
            int height = image.Height;

            int kernelWidth = (this.Kernel.GetLength(0) - 1) / 2;
            int kernelHeight = (this.Kernel.GetLength(1) - 1) / 2;

            int actualKernelWidth = (int)Math.Round(kernelWidth * scale * this.Scale);
            int actualKernelHeight = (int)Math.Round(kernelHeight * scale * this.Scale);

            actualKernelWidth = Math.Max(actualKernelWidth, 1);
            actualKernelHeight = Math.Max(actualKernelHeight, 1);

            int[] kernelX = new int[actualKernelWidth * 2 + 1];

            for (int x = 0; x < actualKernelWidth * 2 + 1; x++)
            {
                kernelX[x] = (int)Math.Round((double)(x - actualKernelWidth) / actualKernelWidth * kernelWidth + kernelWidth);
            }

            int[] kernelY = new int[actualKernelHeight * 2 + 1];

            for (int y = 0; y < actualKernelHeight * 2 + 1; y++)
            {
                kernelY[y] = (int)Math.Round((double)(y - actualKernelHeight) / actualKernelHeight * kernelHeight + kernelHeight);
            }

            int[] countsX = new int[2 * kernelWidth + 1];

            for (int i = 0; i < 2 * actualKernelWidth + 1; i++)
            {
                countsX[kernelX[i]]++;
            }

            int[] countsY = new int[2 * kernelHeight + 1];

            for (int i = 0; i < 2 * actualKernelHeight + 1; i++)
            {
                countsY[kernelY[i]]++;
            }


            double[] weightsX = new double[2 * actualKernelWidth + 1];

            for (int i = 0; i < 2 * actualKernelWidth + 1; i++)
            {
                weightsX[i] = 1.0 / countsX[kernelX[i]];
            }

            double[] weightsY = new double[2 * actualKernelHeight + 1];

            for (int i = 0; i < 2 * actualKernelHeight + 1; i++)
            {
                weightsY[i] = 1.0 / countsY[kernelY[i]];
            }

            kernelWidth = actualKernelWidth;
            kernelHeight = actualKernelHeight;

            double normalisation = this.Normalisation;

            if (double.IsNaN(normalisation) || normalisation == 0)
            {
                normalisation = 1;
            }

            double totalWeight = 0;
            for (int i = 0; i < kernelWidth * 2 + 1; i++)
            {
                for (int j = 0; j < kernelHeight * 2 + 1; j++)
                {
                    totalWeight += Kernel[kernelX[i], kernelY[j]] * weightsX[i] * weightsY[j];
                }
            }

            if (Math.Abs(totalWeight) < 1e-5)
            {
                totalWeight = 1;
            }


            double bias = this.Bias * 255;
            int pixelSize = image.HasAlpha ? 4 : 3;
            int stride = image.Width * pixelSize;

            int threads = Math.Min(8, Environment.ProcessorCount);

            unsafe
            {
                byte* input = (byte*)image.ImageDataAddress;
                byte* output = (byte*)tbrData;

                Action<int> yLoop;

                if (image.HasAlpha && !PreserveAlpha)
                {
                    yLoop = (y) =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double R = 0;
                            double G = 0;
                            double B = 0;

                            double weight = 0;

                            for (int targetX = 0; targetX <= kernelWidth * 2; targetX++)
                            {
                                for (int targetY = 0; targetY <= kernelHeight * 2; targetY++)
                                {
                                    int tX = Math.Min(Math.Max(0, x + targetX - kernelWidth), width - 1);
                                    int tY = Math.Min(Math.Max(0, y + targetY - kernelHeight), height - 1);

                                    double a = input[tY * stride + tX * 4 + 3] / 255.0 * weightsX[targetX] * weightsY[targetY];

                                    int projectedX = kernelX[targetX];
                                    int projectedY = kernelY[targetY];

                                    weight += Kernel[projectedX, projectedY] * a;

                                    R += Kernel[projectedX, projectedY] * input[tY * stride + tX * 4] * a;
                                    G += Kernel[projectedX, projectedY] * input[tY * stride + tX * 4 + 1] * a;
                                    B += Kernel[projectedX, projectedY] * input[tY * stride + tX * 4 + 2] * a;
                                }
                            }

                            if (weight != 0)
                            {
                                output[y * stride + x * 4] = (byte)Math.Min(255, Math.Max(0, R / (normalisation * weight) + bias));
                                output[y * stride + x * 4 + 1] = (byte)Math.Min(255, Math.Max(0, G / (normalisation * weight) + bias));
                                output[y * stride + x * 4 + 2] = (byte)Math.Min(255, Math.Max(0, B / (normalisation * weight) + bias));
                                output[y * stride + x * 4 + 3] = (byte)Math.Min(255, Math.Max(0, (weight / (normalisation * totalWeight) * 255 + bias)));
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
                else if (image.HasAlpha && PreserveAlpha)
                {
                    yLoop = (y) =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double R = 0;
                            double G = 0;
                            double B = 0;

                            for (int targetX = 0; targetX <= kernelWidth * 2; targetX++)
                            {
                                for (int targetY = 0; targetY <= kernelHeight * 2; targetY++)
                                {
                                    int tX = Math.Min(Math.Max(0, x + targetX - kernelWidth), width - 1);
                                    int tY = Math.Min(Math.Max(0, y + targetY - kernelHeight), height - 1);

                                    int projectedX = kernelX[targetX];
                                    int projectedY = kernelY[targetY];

                                    R += Kernel[projectedX, projectedY] * input[tY * stride + tX * 4] * weightsX[targetX] * weightsY[targetY];
                                    G += Kernel[projectedX, projectedY] * input[tY * stride + tX * 4 + 1] * weightsX[targetX] * weightsY[targetY];
                                    B += Kernel[projectedX, projectedY] * input[tY * stride + tX * 4 + 2] * weightsX[targetX] * weightsY[targetY];
                                }
                            }

                            output[y * stride + x * 4] = (byte)Math.Min(255, Math.Max(0, R / (normalisation * totalWeight) + bias));
                            output[y * stride + x * 4 + 1] = (byte)Math.Min(255, Math.Max(0, G / (normalisation * totalWeight) + bias));
                            output[y * stride + x * 4 + 2] = (byte)Math.Min(255, Math.Max(0, B / (normalisation * totalWeight) + bias));
                            output[y * stride + x * 4 + 3] = input[y * stride + x * 4 + 3];
                        }
                    };
                }
                else
                {
                    yLoop = (y) =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double R = 0;
                            double G = 0;
                            double B = 0;

                            for (int targetX = 0; targetX <= kernelWidth * 2; targetX++)
                            {
                                for (int targetY = 0; targetY <= kernelHeight * 2; targetY++)
                                {
                                    int tX = Math.Min(Math.Max(0, x + targetX - kernelWidth), width - 1);
                                    int tY = Math.Min(Math.Max(0, y + targetY - kernelHeight), height - 1);

                                    int projectedX = kernelX[targetX];
                                    int projectedY = kernelY[targetY];

                                    R += Kernel[projectedX, projectedY] * input[tY * stride + tX * 3] * weightsX[targetX] * weightsY[targetY];
                                    G += Kernel[projectedX, projectedY] * input[tY * stride + tX * 3 + 1] * weightsX[targetX] * weightsY[targetY];
                                    B += Kernel[projectedX, projectedY] * input[tY * stride + tX * 3 + 2] * weightsX[targetX] * weightsY[targetY];
                                }
                            }

                            output[y * stride + x * 3] = (byte)Math.Min(255, Math.Max(0, R / (normalisation * totalWeight) + bias));
                            output[y * stride + x * 3 + 1] = (byte)Math.Min(255, Math.Max(0, G / (normalisation * totalWeight) + bias));
                            output[y * stride + x * 3 + 2] = (byte)Math.Min(255, Math.Max(0, B / (normalisation * totalWeight) + bias));
                        }
                    };
                }

                if (threads == 1)
                {
                    for (int y = 0; y < height; y++)
                    {
                        yLoop(y);
                    }
                }
                else
                {
                    ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = threads };

                    Parallel.For(0, height, options, yLoop);
                }
            }

            return new RasterImage(tbrData, image.Width, image.Height, image.HasAlpha, image.Interpolate);
        }
    }
}
