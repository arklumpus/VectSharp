/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2022-2023 Giorgio Bianchini, University of Bristol

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

using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace VectSharp.Canvas
{
    /// <summary>
    /// An <see cref="Avalonia.Controls.Canvas"/> containing an animation.
    /// </summary>
    public class AnimatedCanvas : Avalonia.Controls.UserControl
    {
        /// <summary>
        /// Defines the <see cref="CurrentFrame"/> property.
        /// </summary>
        public static readonly StyledProperty<int> CurrentFrameProperty = AvaloniaProperty.Register<AnimatedCanvas, int>(nameof(CurrentFrame), coerce: (ownerObject, val) =>
        {
            AnimatedCanvas owner = (AnimatedCanvas)ownerObject;

            if (owner.maxFrameIndex > 0)
            {
                if (val < owner.maxFrameIndex)
                {
                    return val;
                }
                else
                {
                    owner.IsPlaying = false;
                    return owner.maxFrameIndex - 1;
                }
            }
            else
            {
                return val % owner.RenderActions.Length;
            }
        });

        /// <summary>
        /// The current frame in the animation.
        /// </summary>
        public int CurrentFrame
        {
            get { return GetValue(CurrentFrameProperty); }
            set { SetValue(CurrentFrameProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="FrameCount"/> property.
        /// </summary>
        public static readonly DirectProperty<AnimatedCanvas, int> FrameCountProperty = AvaloniaProperty.RegisterDirect<AnimatedCanvas, int>(nameof(FrameCount), o => o.maxFrameIndex);

        private int frameCount;

        /// <summary>
        /// The number of frames in the animation.
        /// </summary>
        public int FrameCount
        {
            get
            {
                return frameCount;
            }
            private set
            {
                SetAndRaise(FrameCountProperty, ref frameCount, value);
            }
        }

        private int maxFrameIndex { get; }

        /// <summary>
        /// Defines the <see cref="FrameRate"/> property.
        /// </summary>
        public static readonly DirectProperty<AnimatedCanvas, double> FrameRateProperty = AvaloniaProperty.RegisterDirect<AnimatedCanvas, double>(nameof(FrameRate), o => o.FrameRate);

        private double frameRate;

        /// <summary>
        /// The target frame rate of the animation.
        /// </summary>
        public double FrameRate
        {
            get
            {
                return frameRate;
            }
            private set
            {
                SetAndRaise(FrameRateProperty, ref frameRate, value);
            }
        }



        /// <summary>
        /// Defines the <see cref="IsPlaying"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsPlayingProperty = AvaloniaProperty.Register<AnimatedCanvas, bool>(nameof(IsPlaying), false, coerce: (ownerObject, val) =>
        {
            AnimatedCanvas owner = (AnimatedCanvas)ownerObject;

            if (owner.IsPlaying && !val)
            {
                owner.RefreshTimer.Dispose();
            }
            else if (!owner.IsPlaying && val)
            {
                owner.RefreshTimer = new System.Threading.Timer(_ =>
                {
                    _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        owner.CurrentFrame++;
                    });
                }, null, 0, (long)(1000.0 / owner.FrameRate));
            }

            return val;
        });

        /// <summary>
        /// The current frame in the animation.
        /// </summary>
        public bool IsPlaying
        {
            get { return GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }


        private List<RenderAction>[] RenderActions;

        static Avalonia.Point Origin = new Avalonia.Point(0, 0);

        private SolidColorBrush BackgroundBrush;

        private Dictionary<string, (IImage, bool)> Images;

        private System.Threading.Timer RefreshTimer { get; set; }

        static AnimatedCanvas()
        {
            AffectsRender<AnimatedCanvas>(CurrentFrameProperty);
        }

        internal AnimatedCanvas(VectSharp.Animation animation, double durationScaling, double frameRate, AvaloniaContextInterpreter.TextOptions textOption, FilterOption filterOption)
        {
            Colour backgroundColour = animation.Background;

            this.BackgroundBrush = new SolidColorBrush(Color.FromArgb((byte)(backgroundColour.A * 255), (byte)(backgroundColour.R * 255), (byte)(backgroundColour.G * 255), (byte)(backgroundColour.B * 255)));

            this.Width = animation.Width;
            this.Height = animation.Height;
            this.Images = new Dictionary<string, (IImage, bool)>();

            int frames = (int)Math.Ceiling(animation.Duration * frameRate * durationScaling / 1000);

            this.RenderActions = new List<RenderAction>[frames];

            for (int i = 0; i < frames; i++)
            {
                double frameTime = i / frameRate / durationScaling * 1000;

                Page pag = animation.GetFrameAtAbsolute(frameTime);

                AvaloniaDrawingContext ctx = new AvaloniaDrawingContext(this.Width, this.Height, false, textOption, this.Images, filterOption);

                pag.Graphics.CopyToIGraphicsContext(ctx);
                this.RenderActions[i] = ctx.RenderActions;
            }

            if (animation.RepeatCount > 0)
            {
                this.maxFrameIndex = animation.RepeatCount * frames;
            }
            else
            {
                this.maxFrameIndex = 0;
            }

            this.FrameCount = frames;

            this.FrameRate = frameRate;
        }

        /// <inheritdoc/>
        public override void Render(DrawingContext context)
        {
            context.FillRectangle(this.BackgroundBrush, new Avalonia.Rect(0, 0, Width, Height));

            foreach (RenderAction act in this.RenderActions[this.CurrentFrame % this.RenderActions.Length])
            {
                if (act.ActionType == RenderAction.ActionTypes.Path)
                {
                    DrawingContext.PushedState? state = null;

                    if (act.ClippingPath != null)
                    {
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                        state = context.PushGeometryClip(act.ClippingPath);
                    }

                    using (context.PushTransform(act.Transform))
                    {
                        context.DrawGeometry(act.Fill, act.Stroke, act.Geometry);
                    }

                    if (state != null)
                    {
                        state?.Dispose();
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                    }
                }
                else if (act.ActionType == RenderAction.ActionTypes.Text)
                {
                    DrawingContext.PushedState? state = null;

                    if (act.ClippingPath != null)
                    {
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                        state = context.PushGeometryClip(act.ClippingPath);
                    }

                    using (context.PushTransform(act.Transform))
                    {
                        context.DrawText(act.Text, Origin);
                    }

                    if (state != null)
                    {
                        state?.Dispose();
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                    }
                }
                else if (act.ActionType == RenderAction.ActionTypes.RasterImage)
                {
                    DrawingContext.PushedState? state = null;

                    if (act.ClippingPath != null)
                    {
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                        state = context.PushGeometryClip(act.ClippingPath);
                    }

                    (IImage, bool) image = Images[act.ImageId];

                    using (context.PushTransform(act.Transform))
                    {
                        RenderOptions.SetBitmapInterpolationMode(this, image.Item2 ? Avalonia.Media.Imaging.BitmapInterpolationMode.HighQuality : Avalonia.Media.Imaging.BitmapInterpolationMode.None);
                        context.DrawImage(image.Item1, act.ImageSource.Value, act.ImageDestination.Value);
                    }

                    if (state != null)
                    {
                        state?.Dispose();
                        //Random draw operation needed due to https://github.com/AvaloniaUI/Avalonia/issues/4408
                        context.DrawGeometry(null, new Pen(Brushes.Transparent), act.ClippingPath);
                    }
                }
            }

        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            IsPlaying = false;
        }

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            IsPlaying = true;
        }
    }

}
