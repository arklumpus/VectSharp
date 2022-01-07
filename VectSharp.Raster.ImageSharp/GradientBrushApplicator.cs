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
using System.Buffers;
using System.Numerics;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp;
using System.Reflection;

namespace VectSharp.Raster.ImageSharp
{
    // Adapted from https://github.com/SixLabors/ImageSharp.Drawing/blob/master/src/ImageSharp.Drawing/Processing/GradientBrush.cs - Why isn't this public? D:
    internal abstract class GradientBrushApplicator<TPixel> : BrushApplicator<TPixel>
             where TPixel : unmanaged, IPixel<TPixel>
    {
        private static readonly TPixel Transparent = Color.Transparent.ToPixel<TPixel>();

        private readonly ColorStop[] colorStops;

        private readonly GradientRepetitionMode repetitionMode;

        private readonly MemoryAllocator allocator;

        private readonly int scalineWidth;

        private readonly object blenderBuffers;

        private bool isDisposed;

        private PixelBlender<TPixel> _blender;

        IMemoryOwner<float> amountBuffer;
        IMemoryOwner<TPixel> overlayBuffer;
        static Type constructedLocalBlenderBuffersType;

        static GradientBrushApplicator()
        {
            Assembly utilities = Assembly.Load("SixLabors.ImageSharp.Drawing");
            Type genericThreadLocalBlenderBuffersType = utilities.GetType("SixLabors.ImageSharp.Drawing.Utilities.ThreadLocalBlenderBuffers`1", true);

            Type[] typeArgs = new Type[] { typeof(TPixel) };
            constructedLocalBlenderBuffersType = genericThreadLocalBlenderBuffersType.MakeGenericType(typeArgs);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="GradientBrushApplicator{TPixel}"/> class.
        /// </summary>
        /// <param name="configuration">The configuration instance to use when performing operations.</param>
        /// <param name="options">The graphics options.</param>
        /// <param name="target">The target image.</param>
        /// <param name="colorStops">An array of color stops sorted by their position.</param>
        /// <param name="repetitionMode">Defines if and how the gradient should be repeated.</param>
        protected GradientBrushApplicator(
            Configuration configuration,
            GraphicsOptions options,
            ImageFrame<TPixel> target,
            ColorStop[] colorStops,
            GradientRepetitionMode repetitionMode)
            : base(configuration, options, target)
        {
            // TODO: requires colorStops to be sorted by position.
            // Use Array.Sort with a custom comparer.
            this.colorStops = colorStops;
            this.repetitionMode = repetitionMode;
            this.scalineWidth = target.Width;
            this.allocator = configuration.MemoryAllocator;

            this.blenderBuffers = Activator.CreateInstance(constructedLocalBlenderBuffersType, new object[] { this.allocator, this.scalineWidth, false });

            this._blender = (PixelBlender<TPixel>)this.GetType().GetProperty("Blender", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty).GetValue(this);

            object data = this.blenderBuffers.GetType().GetField("data", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(this.blenderBuffers);
            object bufferOwner = data.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty).GetValue(data);
            amountBuffer = (IMemoryOwner<float>)bufferOwner.GetType().GetField("amountBuffer", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(bufferOwner);
            overlayBuffer = (IMemoryOwner<TPixel>)bufferOwner.GetType().GetField("overlayBuffer", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(bufferOwner);
        }

        internal TPixel this[int x, int y]
        {
            get
            {
                float positionOnCompleteGradient = this.PositionOnGradient(x + 0.5f, y + 0.5f);

                switch (this.repetitionMode)
                {
                    case GradientRepetitionMode.None:
                        // do nothing. The following could be done, but is not necessary:
                        // onLocalGradient = Math.Min(0, Math.Max(1, onLocalGradient));
                        break;
                    case GradientRepetitionMode.Repeat:
                        positionOnCompleteGradient %= 1;
                        break;
                    case GradientRepetitionMode.Reflect:
                        positionOnCompleteGradient %= 2;
                        if (positionOnCompleteGradient > 1)
                        {
                            positionOnCompleteGradient = 2 - positionOnCompleteGradient;
                        }

                        break;
                    case GradientRepetitionMode.DontFill:
                        if (positionOnCompleteGradient > 1 || positionOnCompleteGradient < 0)
                        {
                            return Transparent;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                (ColorStop from, ColorStop to) = this.GetGradientSegment(positionOnCompleteGradient);

                if (from.Color.Equals(to.Color))
                {
                    return from.Color.ToPixel<TPixel>();
                }

                float onLocalGradient = (positionOnCompleteGradient - from.Ratio) / (to.Ratio - from.Ratio);

                return new Color(Vector4.Lerp((Vector4)from.Color, (Vector4)to.Color, onLocalGradient)).ToPixel<TPixel>();
            }
        }

        /// <inheritdoc />
        public override void Apply(Span<float> scanline, int x, int y)
        {
            Span<float> amounts = amountBuffer.Memory.Span.Slice(0, scanline.Length);
            Span<TPixel> overlays = overlayBuffer.Memory.Span.Slice(0, scanline.Length);
            
            float blendPercentage = this.Options.BlendPercentage;

            // TODO: Remove bounds checks.
            if (blendPercentage < 1)
            {
                for (int i = 0; i < scanline.Length; i++)
                {
                    amounts[i] = scanline[i] * blendPercentage;
                    overlays[i] = this[x + i, y];
                }
            }
            else
            {
                for (int i = 0; i < scanline.Length; i++)
                {
                    amounts[i] = scanline[i];
                    overlays[i] = this[x + i, y];
                }
            }

            Span<TPixel> destinationRow = this.Target.PixelBuffer.DangerousGetRowSpan(y).Slice(x, scanline.Length);

            this._blender.Blend(this.Configuration, destinationRow, destinationRow, overlays, amounts);
        }

        /// <summary>
        /// Calculates the position on the gradient for a given point.
        /// This method is abstract as it's content depends on the shape of the gradient.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <returns>
        /// The position the given point has on the gradient.
        /// The position is not bound to the [0..1] interval.
        /// Values outside of that interval may be treated differently,
        /// e.g. for the <see cref="GradientRepetitionMode" /> enum.
        /// </returns>
        protected abstract float PositionOnGradient(float x, float y);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            base.Dispose(disposing);

            if (disposing)
            {
                ((IDisposable)this.blenderBuffers).Dispose();
            }

            this.isDisposed = true;
        }

        private (ColorStop From, ColorStop To) GetGradientSegment(float positionOnCompleteGradient)
        {
            ColorStop localGradientFrom = this.colorStops[0];
            ColorStop localGradientTo = default;

            // TODO: ensure colorStops has at least 2 items (technically 1 would be okay, but that's no gradient)
            foreach (ColorStop colorStop in this.colorStops)
            {
                localGradientTo = colorStop;

                if (colorStop.Ratio > positionOnCompleteGradient)
                {
                    // we're done here, so break it!
                    break;
                }

                localGradientFrom = localGradientTo;
            }

            return (localGradientFrom, localGradientTo);
        }
    }
}
