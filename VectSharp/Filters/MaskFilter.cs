using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VectSharp.Filters
{
    /// <summary>
    /// Represents a filter that uses the alpha channel of an image to mask another image.
    /// </summary>
    public class MaskFilter : FilterWithRasterisableParameter, IFilterWithLocation
    {
        /// <inheritdoc/>
        public Point TopLeftMargin => new Point(0, 0);
        /// <inheritdoc/>
        public Point BottomRightMargin => new Point(0, 0);

        /// <summary>
        /// The image that is used to mask the input image.
        /// </summary>
        public Graphics Mask => this.RasterisableParameter;

        /// <summary>
        /// Creates a new <see cref="MaskFilter"/> with the specified mask image.
        /// </summary>
        /// <param name="mask">The image that is used to mask the input image.</param>
        public MaskFilter(Graphics mask) : base(mask) { }

        /// <inheritdoc/>
        public RasterImage Filter(RasterImage image, Rectangle bounds, double scale)
        {
            RasterImage mask = this.GetCachedRasterisation(scale);

            IntPtr tbrData = System.Runtime.InteropServices.Marshal.AllocHGlobal(image.Width * image.Height * 4);
            GC.AddMemoryPressure(image.Width * image.Height * 4);

            int width = image.Width;
            int height = image.Height;

            int pixelSizeInput = image.HasAlpha ? 4 : 3;
            int strideInput = image.Width * pixelSizeInput;

            int pixelSizeOutput = 4;
            int strideOutput = image.Width * pixelSizeOutput;

            int pixelSizeMask = 4;
            int strideMask = mask.Width * pixelSizeMask;

            int maskWidth = mask.Width;
            int maskHeight = mask.Height;

            int threads = Math.Min(8, Environment.ProcessorCount);

            unsafe
            {
                byte* input = (byte*)image.ImageDataAddress;
                byte* maskBytes = (byte*)mask.ImageDataAddress;
                byte* output = (byte*)tbrData;

                Action<int> yLoop;

                if (image.HasAlpha)
                {
                    yLoop = (y) =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int maskX = (int)Math.Round((bounds.Location.X + (x + 0.5) * bounds.Size.Width / width - CachedBounds.Location.X) / CachedBounds.Size.Width * maskWidth);
                            int maskY = (int)Math.Round((bounds.Location.Y + (y + 0.5) * bounds.Size.Height / height - CachedBounds.Location.Y) / CachedBounds.Size.Height * maskHeight);

                            double weight = 0;

                            if (maskX >= 0 && maskX < maskWidth && maskY >= 0 && maskY < maskHeight)
                            {
                                weight = (maskBytes[maskY * strideMask + maskX * pixelSizeMask] * 0.2126 + maskBytes[maskY * strideMask + maskX * pixelSizeMask + 1] * 0.7152 + maskBytes[maskY * strideMask + maskX * pixelSizeMask + 2] * 0.0722) / 255.0;

                                if (mask.HasAlpha)
                                {
                                    weight *= maskBytes[maskY * strideMask + maskX * pixelSizeMask + 3] / 255.0;
                                }
                            }

                            if (weight > 0)
                            {
                                output[y * strideOutput + x * 4] = input[y * strideInput + x * 4];
                                output[y * strideOutput + x * 4 + 1] = input[y * strideInput + x * 4 + 1];
                                output[y * strideOutput + x * 4 + 2] = input[y * strideInput + x * 4 + 2];
                                output[y * strideOutput + x * 4 + 3] = (byte)Math.Round(input[y * strideInput + x * 4 + 3] * weight);
                            }
                            else
                            {
                                output[y * strideOutput + x * 4] = 0;
                                output[y * strideOutput + x * 4 + 1] = 0;
                                output[y * strideOutput + x * 4 + 2] = 0;
                                output[y * strideOutput + x * 4 + 3] = 0;
                            }
                        }
                    };
                }
                else
                {
                    yLoop = (y) =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int maskX = (int)Math.Round((bounds.Location.X + (x + 0.5) * bounds.Size.Width / width - CachedBounds.Location.X) / CachedBounds.Size.Width * maskWidth);
                            int maskY = (int)Math.Round((bounds.Location.Y + (y + 0.5) * bounds.Size.Height / height - CachedBounds.Location.Y) / CachedBounds.Size.Height * maskHeight);

                            double weight = 0;

                            if (maskX >= 0 && maskX < maskWidth && maskY >= 0 && maskY < maskHeight)
                            {
                                weight = (maskBytes[maskY * strideMask + maskX * pixelSizeMask] * 0.2126 + maskBytes[maskY * strideMask + maskX * pixelSizeMask + 1] * 0.7152 + maskBytes[maskY * strideMask + maskX * pixelSizeMask + 2] * 0.0722);

                                if (mask.HasAlpha)
                                {
                                    weight *= maskBytes[maskY * strideMask + maskX * pixelSizeMask + 3] / 255.0;
                                }
                            }

                            if (weight > 0)
                            {
                                output[y * strideOutput + x * 4] = input[y * strideInput + x * 3];
                                output[y * strideOutput + x * 4 + 1] = input[y * strideInput + x * 3 + 1];
                                output[y * strideOutput + x * 4 + 2] = input[y * strideInput + x * 3 + 2];
                                output[y * strideOutput + x * 4 + 3] = (byte)Math.Round(weight);
                            }
                            else
                            {
                                output[y * strideOutput + x * 4] = 0;
                                output[y * strideOutput + x * 4 + 1] = 0;
                                output[y * strideOutput + x * 4 + 2] = 0;
                                output[y * strideOutput + x * 4 + 3] = 0;
                            }
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

            DisposableIntPtr disp = new DisposableIntPtr(tbrData);

            return new RasterImage(ref disp, width, height, image.HasAlpha, image.Interpolate);

        }
    }
}
