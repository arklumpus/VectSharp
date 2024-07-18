/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2023 Giorgio Bianchini, University of Bristol

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

using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using VectSharp.Filters;

namespace VectSharp.Canvas
{

    /// <summary>
    /// Represents a light-weight rendering action.
    /// </summary>
    public class SKRenderAction : IDisposable
    {
        /// <summary>
        /// Returns a boolean value indicating whether the current instance has been disposed.
        /// </summary>
        public bool Disposed => disposedValue;

        private bool disposedValue;

        /// <summary>
        /// Types of rendering actions.
        /// </summary>
        public enum ActionTypes
        {
            /// <summary>
            /// The render action represents a path object.
            /// </summary>
            Path,

            /// <summary>
            /// The render action represents a text object.
            /// </summary>
            Text,

            /// <summary>
            /// The render action represents a raster image.
            /// </summary>
            RasterImage,

            /// <summary>
            /// The render action represents a transformation of the coordinate space.
            /// </summary>
            Transform,

            /// <summary>
            /// The render action represents saving the current graphics state.
            /// </summary>
            Save,

            /// <summary>
            /// The render action represents restoring the last saved graphics state.
            /// </summary>
            Restore,

            /// <summary>
            /// The render action represents an update of the current clip path.
            /// </summary>
            Clip,

            /// <summary>
            /// The render action represents rendering a graphics object with a filter.
            /// </summary>
            DrawFiltered
        }

        /// <summary>
        /// Type of the rendering action.
        /// </summary>
        public ActionTypes ActionType { get; private set; }

        /// <summary>
        /// Path that needs to be rendered (null if the action type is not <see cref="ActionTypes.Path"/>). If you change this, you probably want to call this object's <see cref="InvalidateHitTestPath"/> method.
        /// </summary>
        public SKPath Path { get; set; }

        internal SKPath HitTestPath { get; set; }
        internal SKPath LastRenderedGlobalHitTestPath { get; set; }

        /// <summary>
        /// Text that needs to be rendered (null if the action type is not <see cref="ActionTypes.Text"/>). If you change this, you probably want to call this object's <see cref="InvalidateHitTestPath"/> method.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The font that will be used to render the text (null if the action type is not <see cref="ActionTypes.Text"/>). If you change this, you probably want to call this object's <see cref="InvalidateHitTestPath"/> method.
        /// </summary>
        public SKFont Font { get; set; }

        /// <summary>
        /// The X coordainate at which the text will be drawn (null if the action type is not <see cref="ActionTypes.Text"/>). If you change this, you probably want to call this object's <see cref="InvalidateHitTestPath"/> method.
        /// </summary>
        public float TextX { get; set; }

        /// <summary>
        /// The Y coordainate at which the text will be drawn (null if the action type is not <see cref="ActionTypes.Text"/>). If you change this, you probably want to call this object's <see cref="InvalidateHitTestPath"/> method.
        /// </summary>
        public float TextY { get; set; }

        /// <summary>
        /// Paint used to render the text or path (<see langword="null"/> if the action type is neither <see cref="ActionTypes.Text"/> nor <see cref="ActionTypes.Path"/>). If you change this, you probably want to call this object's <see cref="InvalidateHitTestPath"/> method.
        /// </summary>
        public SKPaint Paint { get; set; }

        /// <summary>
        /// Univocal identifier of the image that needs to be drawn.
        /// </summary>
        public string ImageId { get; set; }

        /// <summary>
        /// The source rectangle of the image (<see langword="null"/> if the action type is not <see cref="ActionTypes.RasterImage"/>). If you change this, you probably want to call this object's <see cref="InvalidateVisual"/> method.
        /// </summary>
        public SKRect? ImageSource { get; set; }

        /// <summary>
        /// The destination rectangle of the image (<see langword="null"/> if the action type is not <see cref="ActionTypes.RasterImage"/>). If you change this, you probably want to call this object's <see cref="InvalidateHitTestPath"/> method.
        /// </summary>
        public SKRect? ImageDestination { get; set; }

        /// <summary>
        /// The transformation matrix that will be applied to the current coordinate system (<see langword="null"/> if the action type is not <see cref="ActionTypes.Transform"/>). If you change this, you probably want to call this object's <see cref="InvalidateVisual"/> method.
        /// </summary>
        public SKMatrix? Transform { get; set; } = null;

        /// <summary>
        /// A tag to access the <see cref="SKRenderAction"/>.
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// The Z-index of the rendering action (an action with a higher Z-index will always appear above an action with a lower Z-index).
        /// The more different values there are for the Z-index, the slower the rendering, so keep use of this property to a minimum.
        /// If you change this, you probably want to call this object's <see cref="InvalidateZIndex"/> method.
        /// </summary>
        public uint ZIndex { get; set; } = 0;

        /// <summary>
        /// The graphics that will be drawn with the specified <see cref="Filter"/>. If you change this, you probably want to call this object's <see cref="InvalidateVisual"/> method.
        /// </summary>
        public SKRenderContext Graphics { get; set; } = null;

        /// <summary>
        /// The filter with which the <see cref="Graphics"/> is drawn. If you change this, you probably want to call this object's <see cref="InvalidateVisual"/> method.
        /// </summary>
        public IFilter Filter { get; set; }

        /// <summary>
        /// An arbitrary object associated with the RenderAction.
        /// </summary>
        public object Payload { get; set; }

        internal ISKRenderCanvas InternalParent { get; set; }

        /// <summary>
        /// The container of this <see cref="SKRenderAction"/>.
        /// </summary>
        public Avalonia.Controls.Canvas Parent
        {
            get
            {
                return (Avalonia.Controls.Canvas)InternalParent;
            }
        }

        /// <summary>
        /// Raised when the pointer enters the area covered by the <see cref="SKRenderAction"/>.
        /// </summary>
        public event EventHandler<Avalonia.Input.PointerEventArgs> PointerEntered;

        /// <summary>
        /// Raised when the pointer leaves the area covered by the <see cref="SKRenderAction"/>.
        /// </summary>
        public event EventHandler<Avalonia.Input.PointerEventArgs> PointerExited;

        /// <summary>
        /// Raised when the pointer is pressed while over the area covered by the <see cref="SKRenderAction"/>.
        /// </summary>
        public event EventHandler<Avalonia.Input.PointerPressedEventArgs> PointerPressed;

        /// <summary>
        /// Raised when the pointer is released after a <see cref="PointerPressed"/> event.
        /// </summary>
        public event EventHandler<Avalonia.Input.PointerReleasedEventArgs> PointerReleased;


        internal void FirePointerEntered(Avalonia.Input.PointerEventArgs e)
        {
            this.PointerEntered?.Invoke(this, e);
        }

        internal void FirePointerExited(Avalonia.Input.PointerEventArgs e)
        {
            this.PointerExited?.Invoke(this, e);
        }

        internal void FirePointerPressed(Avalonia.Input.PointerPressedEventArgs e)
        {
            this.PointerPressed?.Invoke(this, e);
        }

        internal void FirePointerReleased(Avalonia.Input.PointerReleasedEventArgs e)
        {
            this.PointerReleased?.Invoke(this, e);
        }

        /// <summary>
        /// <para>Signals to this object that its shape has changed and a new path needs to be computed for the purpose of hit-testing.
        /// Also signals to the <see cref="Parent"/> that the visual properties of this object have changed and triggers a redraw.</para>
        /// <para>This method should be called whenever the "shape" of the object represented by the <see cref="SKRenderAction"/> changes.
        /// If only the visual properties of this object have changed (e.g. the colour), call the <see cref="InvalidateVisual"/> method instead.</para>
        /// <para>If you make changes to more than one <see cref="SKRenderAction"/> contained in the same <see cref="SKMultiLayerRenderCanvas"/>, you only need to invalidate the last one.</para>
        /// <para>This method should only be called after the output has been fully initialized.</para>
        /// </summary>
        public void InvalidateHitTestPath()
        {
            CreateHitTestPath();

            this.InvalidateVisual();
        }

        internal void CreateHitTestPath()
        {
            if (this.ActionType == ActionTypes.Path)
            {
                if (this.Path != null && this.Paint != null)
                {
                    this.HitTestPath?.Dispose();
                    this.HitTestPath = this.Paint.GetFillPath(this.Path);
                }
            }
            else if (this.ActionType == ActionTypes.RasterImage)
            {
                if (this.ImageDestination != null)
                {
                    this.HitTestPath?.Dispose();
                    this.HitTestPath = new SKPath();
                    this.HitTestPath.AddRect(this.ImageDestination.Value);
                }
            }
            else if (this.ActionType == ActionTypes.Text)
            {
                if (this.Paint != null && this.Font != null)
                {
                    this.HitTestPath?.Dispose();
                    this.Paint.Typeface = this.Font.Typeface;
                    this.Paint.TextSize = this.Font.Size;

                    using (SKPath pth = this.Paint.GetTextPath(this.Text, this.TextX, this.TextY))
                    {
                        this.HitTestPath = this.Paint.GetFillPath(pth);
                    }
                }
            }
        }

        /// <summary>
        /// <para>This methods signals to the <see cref="Parent"/> that the visual properties (e.g. the colour) of this object have changed and triggers a redraw.</para>
        /// <para>If the "shape" of the object has changed as well, call the <see cref="InvalidateHitTestPath"/> method instead. If the Z-index of the
        /// object has changed, call the <see cref="InvalidateZIndex"/> method instead. If both the "shape" and the Z-index of the object have changed,
        /// call the <see cref="InvalidateAll"/> method.</para>
        /// <para>If you make changes to more than one <see cref="SKRenderAction"/> contained in the same <see cref="Canvas"/>, you only need to invalidate the last one.</para>
        /// <para>This method should only be called after the output has been fully initialized.</para>
        /// </summary>
        public void InvalidateVisual()
        {
            this.InternalParent?.InvalidateDirty();
        }

        /// <summary>
        /// <para>This methods signals to the <see cref="Parent"/> that the Z-index and visual properties (e.g. the colour) of this object have changed and triggers a redraw.</para>
        /// <para>If the "shape" of the object has changed as well, call the <see cref="InvalidateAll"/> method instead.</para>
        /// <para>If you make changes to more than one <see cref="SKRenderAction"/> contained in the same <see cref="Canvas"/>, you only need to invalidate the last one.</para>
        /// <para>This method should only be called after the output has been fully initialized.</para>
        /// </summary>
        public void InvalidateZIndex()
        {
            this.InternalParent?.InvalidateZIndex();
        }

        /// <summary>
        /// <para>This methods signals to the <see cref="Parent"/> that the Z-index, shape and visual properties (e.g. the colour) of this object have changed and triggers a redraw.</para>
        /// <para>If you make changes to more than one <see cref="SKRenderAction"/> contained in the same <see cref="Canvas"/>, you only need to invalidate the last one.</para>
        /// <para>This method should only be called after the output has been fully initialized.</para>
        /// </summary>
        public void InvalidateAll()
        {
            this.InvalidateHitTestPath();
            this.InvalidateZIndex();
        }

        private SKRenderAction()
        {

        }

        /// <summary>
        /// Creates a new <see cref="SKRenderAction"/> representing a path.
        /// </summary>
        /// <param name="path">The geometry to be rendered.</param>
        /// <param name="paint">The paint used to fill or stroke the path.</param>
        /// <param name="tag">A tag to access the <see cref="SKRenderAction"/>. If this is null this <see cref="SKRenderAction"/> is not visible in the hit test.</param>
        /// <returns>A new <see cref="SKRenderAction"/> representing a path.</returns>
        public static SKRenderAction PathAction(SKPath path, SKPaint paint, string tag = null)
        {
            SKRenderAction act = new SKRenderAction()
            {
                ActionType = ActionTypes.Path,
                Path = path,
                Paint = paint,
                Tag = tag
            };

            if (!string.IsNullOrEmpty(tag))
            {
                act.CreateHitTestPath();
            }

            return act;
        }

        /// <summary>
        /// Creates a new <see cref="SKRenderAction"/> representing a clipping action.
        /// </summary>
        /// <param name="clippingPath">The path to be used for clipping.</param>
        /// <param name="tag">A tag to access the <see cref="SKRenderAction"/>.</param>
        /// <returns>A new <see cref="SKRenderAction"/> representing a clipping action.</returns>
        public static SKRenderAction ClipAction(SKPath clippingPath, string tag = null)
        {
            return new SKRenderAction()
            {
                ActionType = ActionTypes.Clip,
                Path = clippingPath,
                Tag = tag
            };
        }

        /// <summary>
        /// Creates a new <see cref="SKRenderAction"/> representing text.
        /// </summary>
        /// <param name="text">The text to be rendered.</param>
        /// <param name="x">The X coordinate at which the text will be drawn.</param>
        /// <param name="y">The Y coordinate at which the text will be drawn.</param>
        /// <param name="font">The font to be used to render the text.</param>
        /// <param name="paint">The paint to be used to fill or stroke the text.</param>
        /// <param name="tag">A tag to access the <see cref="SKRenderAction"/>. If this is null this <see cref="SKRenderAction"/> is not visible in the hit test.</param>
        /// <returns>A new <see cref="SKRenderAction"/> representing text.</returns>
        public static SKRenderAction TextAction(string text, float x, float y, SKFont font, SKPaint paint, string tag = null)
        {
            SKRenderAction act = new SKRenderAction()
            {
                ActionType = ActionTypes.Text,
                Text = text,
                TextX = x,
                TextY = y,
                Font = font,
                Paint = paint,
                Tag = tag
            };

            act.Paint.Typeface = font.Typeface;
            act.Paint.TextSize = font.Size;

            if (!string.IsNullOrEmpty(tag))
            {
                act.CreateHitTestPath();
            }

            return act;
        }

        /// <summary>
        /// Creates a new <see cref="SKRenderAction"/> representing an image.
        /// </summary>
        /// <param name="imageId">The univocal identifier of the image to draw.</param>
        /// <param name="sourceRect">The source rectangle of the image.</param>
        /// <param name="destinationRect">The destination rectangle of the image.</param>
        /// <param name="tag">A tag to access the <see cref="SKRenderAction"/>. If this is null this <see cref="SKRenderAction"/> is not visible in the hit test.</param>
        /// <returns>A new <see cref="SKRenderAction"/> representing an image.</returns>
        public static SKRenderAction ImageAction(string imageId, SKRect sourceRect, SKRect destinationRect, string tag = null)
        {
            SKRenderAction act = new SKRenderAction()
            {
                ActionType = ActionTypes.RasterImage,
                ImageId = imageId,
                ImageSource = sourceRect,
                ImageDestination = destinationRect,
                Tag = tag
            };

            if (!string.IsNullOrEmpty(tag))
            {
                act.CreateHitTestPath();
            }

            return act;
        }

        /// <summary>
        /// Creates a new <see cref="SKRenderAction"/> representing a transform.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        /// <param name="tag">A tag to access the <see cref="SKRenderAction"/>.</param>
        /// <returns>A new <see cref="SKRenderAction"/> representing a transform.</returns>
        public static SKRenderAction TransformAction(SKMatrix transform, string tag = null)
        {
            return new SKRenderAction()
            {
                ActionType = ActionTypes.Transform,
                Transform = transform,
                Tag = tag
            };
        }

        /// <summary>
        /// Creates a new <see cref="SKRenderAction"/> that saves the current graphics state.
        /// </summary>
        /// <param name="tag">A tag to access the <see cref="SKRenderAction"/>.</param>
        /// <returns>A new <see cref="SKRenderAction"/> that saves the current graphics state.</returns>
        public static SKRenderAction SaveAction(string tag = null)
        {
            return new SKRenderAction()
            {
                ActionType = ActionTypes.Save,
                Tag = tag
            };
        }

        /// <summary>
        /// Creates a new <see cref="SKRenderAction"/> that saves the current graphics state.
        /// </summary>
        /// <param name="tag">A tag to access the <see cref="SKRenderAction"/>.</param>
        /// <returns>A new <see cref="SKRenderAction"/> that restores the last saved graphics state.</returns>
        public static SKRenderAction RestoreAction(string tag = null)
        {
            return new SKRenderAction()
            {
                ActionType = ActionTypes.Restore,
                Tag = tag
            };
        }

        /// <summary>
        /// Create a new <see cref="SKRenderAction"/> that draws some graphics with a filter.
        /// </summary>
        /// <returns>A new <see cref="SKRenderAction"/> that draws some graphics with a filter.</returns>
        public static SKRenderAction DrawFilteredGraphicsAction(SKRenderContext graphics, IFilter filter, string tag = null)
        {
            return new SKRenderAction()
            {
                ActionType = ActionTypes.DrawFiltered,
                Graphics = graphics,
                Filter = filter,
                Tag = tag
            };
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Font?.Dispose();
                    this.HitTestPath?.Dispose();
                    this.LastRenderedGlobalHitTestPath?.Dispose();
                    this.Paint?.Dispose();
                    this.Path?.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


    internal static class SKTypefaceCache
    {
        private static readonly object LockObject = new object();
        private static readonly Dictionary<string, SKTypeface> Typefaces = new Dictionary<string, SKTypeface>();
        public static SKTypeface GetSKTypeface(FontFamily family)
        {

            lock (LockObject)
            {
                if (Typefaces.TryGetValue(family.FileName, out SKTypeface tbr))
                {
                    return tbr;
                }
                else
                {
                    try
                    {
                        System.IO.MemoryStream fontStream = new System.IO.MemoryStream((int)family.TrueTypeFile.FontStream.Length);
                        family.TrueTypeFile.FontStream.Seek(0, System.IO.SeekOrigin.Begin);
                        family.TrueTypeFile.FontStream.CopyTo(fontStream);
                        fontStream.Seek(0, System.IO.SeekOrigin.Begin);

                        SKTypeface typeface = SKTypeface.FromData(SKData.Create(fontStream));

                        Typefaces[family.FileName] = typeface;

                        return typeface;
                    }
                    catch
                    {
                        SKTypeface typeface = SKTypeface.Default;

                        Typefaces[family.FileName] = typeface;

                        return typeface;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a page that has been prepared for fast rendering using the SkiaSharp renderer.
    /// </summary>
    public class SKRenderContext
    {
        internal virtual Dictionary<string, (SKBitmap, bool)> Images { get; set; }
        internal virtual List<SKRenderAction> SKRenderActions { get; set; }
    }


    internal class SKRenderContextImpl : SKRenderContext, IGraphicsContext, IDisposable
    {
        public Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>> TaggedActions { get; set; } = new Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>>();

        private readonly bool removeTaggedActions = true;

        public string Tag { get; set; }

        private readonly AvaloniaContextInterpreter.TextOptions _textOption;

        internal override Dictionary<string, (SKBitmap, bool)> Images { get; set; }

        private readonly FilterOption _filterOption;

        public SKRenderContextImpl(double width, double height, bool removeTaggedActionsAfterExecution, AvaloniaContextInterpreter.TextOptions textOption, Dictionary<string, (SKBitmap, bool)> images, FilterOption filterOption)
        {
            this.Images = images;

            currentPath = null;
            figureInitialised = false;

            SKRenderActions = new List<SKRenderAction>();
            removeTaggedActions = removeTaggedActionsAfterExecution;

            Width = width;
            Height = height;

            _textOption = textOption;
            _filterOption = filterOption;
        }

        internal override List<SKRenderAction> SKRenderActions { get; set; }

        public double Width { get; private set; }
        public double Height { get; private set; }

        private void AddAction(SKRenderAction act)
        {
            if (!string.IsNullOrEmpty(Tag))
            {
                if (TaggedActions.ContainsKey(Tag))
                {
                    IEnumerable<SKRenderAction> actions = TaggedActions[Tag](act);

                    foreach (SKRenderAction action in actions)
                    {
                        SKRenderActions.Add(action);
                    }

                    if (removeTaggedActions)
                    {
                        TaggedActions.Remove(Tag);
                    }
                }
                else
                {
                    SKRenderActions.Add(act);
                }
            }
            else if (TaggedActions.ContainsKey(""))
            {
                IEnumerable<SKRenderAction> actions = TaggedActions[""](act);

                foreach (SKRenderAction action in actions)
                {
                    SKRenderActions.Add(action);
                }
            }
            else
            {
                SKRenderActions.Add(act);
            }
        }

        public void Translate(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            SKRenderAction act = SKRenderAction.TransformAction(SKMatrix.CreateTranslation((float)x, (float)y), Tag);
            AddAction(act);

            currentPath = null;
            figureInitialised = false;
        }

        public TextBaselines TextBaseline { get; set; }

        private void PathText(string text, double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            GraphicsPath textPath = new GraphicsPath().AddText(x, y, text, Font, TextBaseline);

            for (int j = 0; j < textPath.Segments.Count; j++)
            {
                switch (textPath.Segments[j].Type)
                {
                    case VectSharp.SegmentType.Move:
                        this.MoveTo(textPath.Segments[j].Point.X, textPath.Segments[j].Point.Y);
                        break;
                    case VectSharp.SegmentType.Line:
                        this.LineTo(textPath.Segments[j].Point.X, textPath.Segments[j].Point.Y);
                        break;
                    case VectSharp.SegmentType.CubicBezier:
                        this.CubicBezierTo(textPath.Segments[j].Points[0].X, textPath.Segments[j].Points[0].Y, textPath.Segments[j].Points[1].X, textPath.Segments[j].Points[1].Y, textPath.Segments[j].Points[2].X, textPath.Segments[j].Points[2].Y);
                        break;
                    case VectSharp.SegmentType.Close:
                        this.Close();
                        break;
                }
            }
        }

        public void StrokeSimpleText(string text, double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            if ((_textOption == AvaloniaContextInterpreter.TextOptions.NeverConvert || (_textOption == AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary && (Font.FontFamily.IsStandardFamily || Font.FontFamily.TrueTypeFile?.FontStream != null))) && !System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                SKTypeface typeface = SKTypefaceCache.GetSKTypeface(Font.FontFamily);

                double top = y;
                double left = x;

                Font.DetailedFontMetrics metrics = Font.MeasureTextAdvanced(text);

                if (TextBaseline == TextBaselines.Top)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        left -= metrics.LeftSideBearing;
                        top += metrics.Top;
                    }
                }
                else if (TextBaseline == TextBaselines.Middle)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        left -= metrics.LeftSideBearing;
                        top += (metrics.Top + metrics.Bottom) * 0.5;
                    }
                }
                else if (TextBaseline == TextBaselines.Baseline)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        left -= metrics.LeftSideBearing;
                    }
                }
                else if (TextBaseline == TextBaselines.Bottom)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        left -= metrics.LeftSideBearing;
                        top += metrics.Bottom;
                    }
                }

                SKPaint stroke = new SKPaint() { IsStroke = true, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)LineWidth, SubpixelText = true };

                if (this.StrokeStyle is SolidColourBrush solid)
                {
                    stroke.Color = new SKColor((byte)(solid.R * 255), (byte)(solid.G * 255), (byte)(solid.B * 255), StrokeAlpha);
                }
                else if (this.StrokeStyle is LinearGradientBrush linearGradient)
                {
                    stroke.Shader = linearGradient.ToSKShader();
                }
                else if (this.StrokeStyle is RadialGradientBrush radialGradient)
                {
                    stroke.Shader = radialGradient.ToSKShader();
                }

                stroke.PathEffect = SKPathEffect.CreateDash(LineDash.DashArray.Select(x => (float)x).ToArray(), (float)LineDash.Phase);

                switch (LineCap)
                {
                    case LineCaps.Butt:
                        stroke.StrokeCap = SKStrokeCap.Butt;
                        break;
                    case LineCaps.Round:
                        stroke.StrokeCap = SKStrokeCap.Round;
                        break;
                    case LineCaps.Square:
                        stroke.StrokeCap = SKStrokeCap.Square;
                        break;
                }

                switch (LineJoin)
                {
                    case LineJoins.Bevel:
                        stroke.StrokeJoin = SKStrokeJoin.Bevel;
                        break;
                    case LineJoins.Round:
                        stroke.StrokeJoin = SKStrokeJoin.Round;
                        break;
                    case LineJoins.Miter:
                        stroke.StrokeJoin = SKStrokeJoin.Miter;
                        break;
                }

                SKRenderAction act = SKRenderAction.TextAction(text, (float)left, (float)top, new SKFont(typeface, (float)Font.FontSize), stroke, Tag);

                AddAction(act);
            }
            else
            {
                PathText(text, x, y);
                Stroke();
            }
        }

        public void FillText(string text, double x, double y)
        {
            if (!Font.EnableKerning)
            {
                FillSimpleText(text, x, y);
            }
            else
            {
                List<(string, Point)> tSpans = new List<(string, Point)>();

                System.Text.StringBuilder currentRun = new System.Text.StringBuilder();
                Point currentKerning = new Point();

                Point currentGlyphPlacementDelta = new Point();
                Point currentGlyphAdvanceDelta = new Point();
                Point nextGlyphPlacementDelta = new Point();
                Point nextGlyphAdvanceDelta = new Point();

                for (int i = 0; i < text.Length; i++)
                {
                    if (i < text.Length - 1)
                    {
                        currentGlyphPlacementDelta = nextGlyphPlacementDelta;
                        currentGlyphAdvanceDelta = nextGlyphAdvanceDelta;
                        nextGlyphAdvanceDelta = new Point();
                        nextGlyphPlacementDelta = new Point();

                        TrueTypeFile.PairKerning kerning = Font.FontFamily.TrueTypeFile.Get1000EmKerning(text[i], text[i + 1]);

                        if (kerning != null)
                        {
                            currentGlyphPlacementDelta = new Point(currentGlyphPlacementDelta.X + kerning.Glyph1Placement.X, currentGlyphPlacementDelta.Y + kerning.Glyph1Placement.Y);
                            currentGlyphAdvanceDelta = new Point(currentGlyphAdvanceDelta.X + kerning.Glyph1Advance.X, currentGlyphAdvanceDelta.Y + kerning.Glyph1Advance.Y);

                            nextGlyphPlacementDelta = new Point(nextGlyphPlacementDelta.X + kerning.Glyph2Placement.X, nextGlyphPlacementDelta.Y + kerning.Glyph2Placement.Y);
                            nextGlyphAdvanceDelta = new Point(nextGlyphAdvanceDelta.X + kerning.Glyph2Advance.X, nextGlyphAdvanceDelta.Y + kerning.Glyph2Advance.Y);
                        }
                    }

                    if (currentGlyphPlacementDelta.X != 0 || currentGlyphPlacementDelta.Y != 0 || currentGlyphAdvanceDelta.X != 0 || currentGlyphAdvanceDelta.Y != 0)
                    {
                        if (currentRun.Length > 0)
                        {
                            tSpans.Add((currentRun.ToString(), currentKerning));

                            tSpans.Add((text[i].ToString(), new Point(currentGlyphPlacementDelta.X * Font.FontSize / 1000, currentGlyphPlacementDelta.Y * Font.FontSize / 1000)));

                            currentRun.Clear();
                            currentKerning = new Point((currentGlyphAdvanceDelta.X - currentGlyphPlacementDelta.X) * Font.FontSize / 1000, (currentGlyphAdvanceDelta.Y - currentGlyphPlacementDelta.Y) * Font.FontSize / 1000);
                        }
                        else
                        {
                            tSpans.Add((text[i].ToString(), new Point(currentGlyphPlacementDelta.X * Font.FontSize / 1000 + currentKerning.X, currentGlyphPlacementDelta.Y * Font.FontSize / 1000 + currentKerning.Y)));

                            currentRun.Clear();
                            currentKerning = new Point((currentGlyphAdvanceDelta.X - currentGlyphPlacementDelta.X) * Font.FontSize / 1000, (currentGlyphAdvanceDelta.Y - currentGlyphPlacementDelta.Y) * Font.FontSize / 1000);
                        }
                    }
                    else
                    {
                        currentRun.Append(text[i]);
                    }
                }

                if (currentRun.Length > 0)
                {
                    tSpans.Add((currentRun.ToString(), currentKerning));
                }

                double currX = x;
                double currY = y;

                Font.DetailedFontMetrics fullMetrics = Font.MeasureTextAdvanced(text);

                if (TextBaseline == TextBaselines.Top)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        currY += fullMetrics.Top;
                    }
                }
                else if (TextBaseline == TextBaselines.Middle)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        currY += (fullMetrics.Top + fullMetrics.Bottom) * 0.5;
                    }
                }
                else if (TextBaseline == TextBaselines.Bottom)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        currY += fullMetrics.Bottom;
                    }
                }

                TextBaseline = TextBaselines.Baseline;

                for (int i = 0; i < tSpans.Count; i++)
                {
                    Font.DetailedFontMetrics metrics = Font.MeasureTextAdvanced(tSpans[i].Item1);

                    if (i == 0)
                    {
                        FillSimpleText(tSpans[i].Item1, currX + tSpans[i].Item2.X, currY + tSpans[i].Item2.Y);
                    }
                    else
                    {
                        FillSimpleText(tSpans[i].Item1, currX + metrics.LeftSideBearing - fullMetrics.LeftSideBearing + tSpans[i].Item2.X, currY + tSpans[i].Item2.Y);
                    }


                    currX += metrics.AdvanceWidth + tSpans[i].Item2.X;
                    currY += tSpans[i].Item2.Y;
                }
            }
        }

        public void StrokeText(string text, double x, double y)
        {
            if (!Font.EnableKerning)
            {
                StrokeSimpleText(text, x, y);
            }
            else
            {
                List<(string, Point)> tSpans = new List<(string, Point)>();

                System.Text.StringBuilder currentRun = new System.Text.StringBuilder();
                Point currentKerning = new Point();

                Point currentGlyphPlacementDelta = new Point();
                Point currentGlyphAdvanceDelta = new Point();
                Point nextGlyphPlacementDelta = new Point();
                Point nextGlyphAdvanceDelta = new Point();

                for (int i = 0; i < text.Length; i++)
                {
                    if (i < text.Length - 1)
                    {
                        currentGlyphPlacementDelta = nextGlyphPlacementDelta;
                        currentGlyphAdvanceDelta = nextGlyphAdvanceDelta;
                        nextGlyphAdvanceDelta = new Point();
                        nextGlyphPlacementDelta = new Point();

                        TrueTypeFile.PairKerning kerning = Font.FontFamily.TrueTypeFile.Get1000EmKerning(text[i], text[i + 1]);

                        if (kerning != null)
                        {
                            currentGlyphPlacementDelta = new Point(currentGlyphPlacementDelta.X + kerning.Glyph1Placement.X, currentGlyphPlacementDelta.Y + kerning.Glyph1Placement.Y);
                            currentGlyphAdvanceDelta = new Point(currentGlyphAdvanceDelta.X + kerning.Glyph1Advance.X, currentGlyphAdvanceDelta.Y + kerning.Glyph1Advance.Y);

                            nextGlyphPlacementDelta = new Point(nextGlyphPlacementDelta.X + kerning.Glyph2Placement.X, nextGlyphPlacementDelta.Y + kerning.Glyph2Placement.Y);
                            nextGlyphAdvanceDelta = new Point(nextGlyphAdvanceDelta.X + kerning.Glyph2Advance.X, nextGlyphAdvanceDelta.Y + kerning.Glyph2Advance.Y);
                        }
                    }

                    if (currentGlyphPlacementDelta.X != 0 || currentGlyphPlacementDelta.Y != 0 || currentGlyphAdvanceDelta.X != 0 || currentGlyphAdvanceDelta.Y != 0)
                    {
                        if (currentRun.Length > 0)
                        {
                            tSpans.Add((currentRun.ToString(), currentKerning));

                            tSpans.Add((text[i].ToString(), new Point(currentGlyphPlacementDelta.X * Font.FontSize / 1000, currentGlyphPlacementDelta.Y * Font.FontSize / 1000)));

                            currentRun.Clear();
                            currentKerning = new Point((currentGlyphAdvanceDelta.X - currentGlyphPlacementDelta.X) * Font.FontSize / 1000, (currentGlyphAdvanceDelta.Y - currentGlyphPlacementDelta.Y) * Font.FontSize / 1000);
                        }
                        else
                        {
                            tSpans.Add((text[i].ToString(), new Point(currentGlyphPlacementDelta.X * Font.FontSize / 1000 + currentKerning.X, currentGlyphPlacementDelta.Y * Font.FontSize / 1000 + currentKerning.Y)));

                            currentRun.Clear();
                            currentKerning = new Point((currentGlyphAdvanceDelta.X - currentGlyphPlacementDelta.X) * Font.FontSize / 1000, (currentGlyphAdvanceDelta.Y - currentGlyphPlacementDelta.Y) * Font.FontSize / 1000);
                        }
                    }
                    else
                    {
                        currentRun.Append(text[i]);
                    }
                }

                if (currentRun.Length > 0)
                {
                    tSpans.Add((currentRun.ToString(), currentKerning));
                }

                double currX = x;
                double currY = y;

                Font.DetailedFontMetrics fullMetrics = Font.MeasureTextAdvanced(text);

                for (int i = 0; i < tSpans.Count; i++)
                {
                    Font.DetailedFontMetrics metrics = Font.MeasureTextAdvanced(tSpans[i].Item1);

                    if (i == 0)
                    {
                        StrokeSimpleText(tSpans[i].Item1, currX + tSpans[i].Item2.X, currY + tSpans[i].Item2.Y);
                    }
                    else
                    {
                        StrokeSimpleText(tSpans[i].Item1, currX + metrics.LeftSideBearing - fullMetrics.LeftSideBearing + tSpans[i].Item2.X, currY + tSpans[i].Item2.Y);
                    }


                    currX += metrics.AdvanceWidth + tSpans[i].Item2.X;
                    currY += tSpans[i].Item2.Y;
                }
            }
        }


        public void FillSimpleText(string text, double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            if ((_textOption == AvaloniaContextInterpreter.TextOptions.NeverConvert || (_textOption == AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary && (Font.FontFamily.IsStandardFamily || Font.FontFamily.TrueTypeFile?.FontStream != null))) && !System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                SKTypeface typeface = SKTypefaceCache.GetSKTypeface(Font.FontFamily);

                double top = y;
                double left = x;

                Font.DetailedFontMetrics metrics = Font.MeasureTextAdvanced(text);

                if (TextBaseline == TextBaselines.Top)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        left -= metrics.LeftSideBearing;
                        top += metrics.Top;
                    }
                }
                else if (TextBaseline == TextBaselines.Middle)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        left -= metrics.LeftSideBearing;
                        top += (metrics.Top + metrics.Bottom) * 0.5;
                    }
                }
                else if (TextBaseline == TextBaselines.Baseline)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        left -= metrics.LeftSideBearing;
                    }
                }
                else if (TextBaseline == TextBaselines.Bottom)
                {
                    if (Font.FontFamily.TrueTypeFile != null)
                    {
                        left -= metrics.LeftSideBearing;
                        top += metrics.Bottom;
                    }
                }

                SKPaint fill = new SKPaint() { IsStroke = false, IsAntialias = true, Style = SKPaintStyle.Fill, SubpixelText = true };

                if (this.FillStyle is SolidColourBrush solid)
                {
                    fill.Color = new SKColor((byte)(solid.R * 255), (byte)(solid.G * 255), (byte)(solid.B * 255), FillAlpha);
                }
                else if (this.FillStyle is LinearGradientBrush linearGradient)
                {
                    fill.Shader = linearGradient.ToSKShader();
                }
                else if (this.FillStyle is RadialGradientBrush radialGradient)
                {
                    fill.Shader = radialGradient.ToSKShader();
                }

                SKRenderAction act = SKRenderAction.TextAction(text, (float)left, (float)top, new SKFont(typeface, (float)Font.FontSize), fill, Tag);

                AddAction(act);
            }
            else
            {
                PathText(text, x, y);
                Fill(FillRule.NonZeroWinding);
            }
        }

        public Brush StrokeStyle { get; private set; } = Colour.FromRgb(0, 0, 0);
        private byte StrokeAlpha = 255;

        public Brush FillStyle { get; private set; } = Colour.FromRgb(0, 0, 0);
        private byte FillAlpha = 255;

        public void SetFillStyle((int r, int g, int b, double a) style)
        {
            FillStyle = Colour.FromRgba(style.r, style.g, style.b, (int)(style.a * 255));
            FillAlpha = (byte)(style.a * 255);
        }

        public void SetFillStyle(Brush style)
        {
            FillStyle = style;

            if (style is SolidColourBrush solid)
            {
                FillAlpha = (byte)(solid.A * 255);
            }
            else
            {
                FillAlpha = 255;
            }
        }

        public void SetStrokeStyle((int r, int g, int b, double a) style)
        {
            StrokeStyle = Colour.FromRgba(style.r, style.g, style.b, (int)(style.a * 255));
            StrokeAlpha = (byte)(style.a * 255);
        }

        public void SetStrokeStyle(Brush style)
        {
            StrokeStyle = style;

            if (style is SolidColourBrush solid)
            {
                StrokeAlpha = (byte)(solid.A * 255);
            }
            else
            {
                StrokeAlpha = 255;
            }
        }

        private LineDash LineDash;

        public void SetLineDash(LineDash dash)
        {
            LineDash = new LineDash(dash.DashArray, dash.Phase);
        }

        public void Rotate(double angle)
        {
            Utils.CoerceNaNAndInfinityToZero(ref angle);

            SKRenderAction act = SKRenderAction.TransformAction(SKMatrix.CreateRotation((float)angle), Tag);
            AddAction(act);

            currentPath = null;
            figureInitialised = false;
        }

        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            Utils.CoerceNaNAndInfinityToZero(ref a, ref b, ref c, ref d, ref e, ref f);

            SKRenderAction act = SKRenderAction.TransformAction(new SKMatrix((float)a, (float)c, (float)e, (float)b, (float)d, (float)f, 0, 0, 1), Tag);
            AddAction(act);

            currentPath = null;
            figureInitialised = false;
        }

        public void Scale(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            SKRenderAction act = SKRenderAction.TransformAction(SKMatrix.CreateScale((float)x, (float)y), Tag);
            AddAction(act);

            currentPath = null;
            figureInitialised = false;
        }

        public void Save()
        {
            SKRenderAction act = SKRenderAction.SaveAction(Tag);
            AddAction(act);

            currentPath = null;
            figureInitialised = false;
        }

        public void Restore()
        {
            SKRenderAction act = SKRenderAction.RestoreAction(Tag);
            AddAction(act);

            currentPath = null;
            figureInitialised = false;
        }

        public double LineWidth { get; set; }
        public LineCaps LineCap { get; set; }
        public LineJoins LineJoin { get; set; }

        public Font Font { get; set; }

        private SKPath currentPath;

        private bool figureInitialised = false;
        private bool disposedValue;

        public void MoveTo(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            if (currentPath == null)
            {
                currentPath = new SKPath() { FillType = SKPathFillType.EvenOdd };
            }

            currentPath.MoveTo((float)x, (float)y);
            figureInitialised = true;
        }

        public void LineTo(double x, double y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x, ref y);

            if (currentPath == null)
            {
                currentPath = new SKPath() { FillType = SKPathFillType.EvenOdd };
            }

            if (!figureInitialised)
            {
                figureInitialised = true;
                currentPath.MoveTo((float)x, (float)y);
            }
            else
            {
                currentPath.LineTo((float)x, (float)y);
            }
        }

        public void Rectangle(double x0, double y0, double width, double height)
        {
            Utils.CoerceNaNAndInfinityToZero(ref x0, ref y0, ref width, ref height);

            if (currentPath == null)
            {
                currentPath = new SKPath() { FillType = SKPathFillType.EvenOdd };
            }

            currentPath.MoveTo((float)x0, (float)y0);
            currentPath.LineTo((float)(x0 + width), (float)y0);
            currentPath.LineTo((float)(x0 + width), (float)(y0 + height));
            currentPath.LineTo((float)x0, (float)(y0 + height));

            currentPath.Close();
            figureInitialised = false;
        }

        public void CubicBezierTo(double p1X, double p1Y, double p2X, double p2Y, double p3X, double p3Y)
        {
            Utils.CoerceNaNAndInfinityToZero(ref p1X, ref p1Y, ref p2X, ref p2Y, ref p3X, ref p3Y);

            if (currentPath == null)
            {
                currentPath = new SKPath() { FillType = SKPathFillType.EvenOdd };
            }

            if (figureInitialised)
            {
                currentPath.CubicTo((float)p1X, (float)p1Y, (float)p2X, (float)p2Y, (float)p3X, (float)p3Y);
            }
            else
            {
                currentPath.MoveTo((float)p1X, (float)p1Y);
                figureInitialised = true;
            }
        }

        public void Close()
        {
            currentPath.Close();

            figureInitialised = false;
        }

        public void Stroke()
        {
            SKPaint stroke = new SKPaint() { IsStroke = true, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = (float)LineWidth };

            if (this.StrokeStyle is SolidColourBrush solid)
            {
                stroke.Color = new SKColor((byte)(solid.R * 255), (byte)(solid.G * 255), (byte)(solid.B * 255), StrokeAlpha);
            }
            else if (this.StrokeStyle is LinearGradientBrush linearGradient)
            {
                stroke.Shader = linearGradient.ToSKShader();
            }
            else if (this.StrokeStyle is RadialGradientBrush radialGradient)
            {
                stroke.Shader = radialGradient.ToSKShader();
            }

            stroke.PathEffect = SKPathEffect.CreateDash(LineDash.DashArray.Select(x => (float)x).ToArray(), (float)LineDash.Phase);

            switch (LineCap)
            {
                case LineCaps.Butt:
                    stroke.StrokeCap = SKStrokeCap.Butt;
                    break;
                case LineCaps.Round:
                    stroke.StrokeCap = SKStrokeCap.Round;
                    break;
                case LineCaps.Square:
                    stroke.StrokeCap = SKStrokeCap.Square;
                    break;
            }

            switch (LineJoin)
            {
                case LineJoins.Bevel:
                    stroke.StrokeJoin = SKStrokeJoin.Bevel;
                    break;
                case LineJoins.Round:
                    stroke.StrokeJoin = SKStrokeJoin.Round;
                    break;
                case LineJoins.Miter:
                    stroke.StrokeJoin = SKStrokeJoin.Miter;
                    break;
            }

            SKRenderAction act = SKRenderAction.PathAction(currentPath, stroke, Tag);

            AddAction(act);

            currentPath = null;
            figureInitialised = false;
        }

        public void Fill(FillRule fillRule)
        {
            SKPaint fill = new SKPaint() { IsStroke = false, IsAntialias = true, Style = SKPaintStyle.Fill };

            if (this.FillStyle is SolidColourBrush solid)
            {
                fill.Color = new SKColor((byte)(solid.R * 255), (byte)(solid.G * 255), (byte)(solid.B * 255), FillAlpha);
            }
            else if (this.FillStyle is LinearGradientBrush linearGradient)
            {
                fill.Shader = linearGradient.ToSKShader();
            }
            else if (this.FillStyle is RadialGradientBrush radialGradient)
            {
                fill.Shader = radialGradient.ToSKShader();
            }

            switch (fillRule)
            {
                case FillRule.NonZeroWinding:
                    currentPath.FillType = SKPathFillType.Winding;
                    break;

                case FillRule.EvenOdd:
                    currentPath.FillType = SKPathFillType.EvenOdd;
                    break;
            }

            SKRenderAction act = SKRenderAction.PathAction(currentPath, fill, Tag);

            AddAction(act);

            currentPath = null;
            figureInitialised = false;
        }

        public void SetClippingPath()
        {
            SKRenderAction act = SKRenderAction.ClipAction(currentPath, Tag);

            AddAction(act);

            currentPath = null;
            figureInitialised = false;
        }

        public void DrawRasterImage(int sourceX, int sourceY, int sourceWidth, int sourceHeight, double destinationX, double destinationY, double destinationWidth, double destinationHeight, RasterImage image)
        {
            Utils.CoerceNaNAndInfinityToZero(ref destinationX, ref destinationY, ref destinationWidth, ref destinationHeight);

            if (!this.Images.ContainsKey(image.Id))
            {
                SKBitmap bmp = SKBitmap.Decode(image.PNGStream);
                this.Images.Add(image.Id, (bmp, image.Interpolate));
            }

            SKRenderAction act = SKRenderAction.ImageAction(image.Id, new SKRect(sourceX, sourceY, sourceX + sourceWidth, sourceY + sourceHeight), new SKRect((float)destinationX, (float)destinationY, (float)(destinationX + destinationWidth), (float)(destinationY + destinationHeight)), Tag);

            AddAction(act);
        }

        public void DrawFilteredGraphics(Graphics graphics, IFilter filter)
        {
            if (this._filterOption.Operation == FilterOption.FilterOperations.RasteriseAllWithSkia)
            {
                double scale = this._filterOption.RasterisationResolution;

                Rectangle bounds = graphics.GetBounds();

                bounds = new Rectangle(bounds.Location.X - filter.TopLeftMargin.X, bounds.Location.Y - filter.TopLeftMargin.Y, bounds.Size.Width + filter.TopLeftMargin.X + filter.BottomRightMargin.X, bounds.Size.Height + filter.TopLeftMargin.Y + filter.BottomRightMargin.Y);

                if (bounds.Size.Width > 0 && bounds.Size.Height > 0)
                {
                    if (!this._filterOption.RasterisationResolutionRelative)
                    {
                        scale = scale / Math.Min(bounds.Size.Width, bounds.Size.Height);
                    }

                    RasterImage rasterised = SKRenderContextInterpreter.Rasterise(graphics, bounds, scale, true);
                    RasterImage filtered = null;

                    if (filter is IFilterWithRasterisableParameter filterWithRastParam)
                    {
                        filterWithRastParam.RasteriseParameter(SKRenderContextInterpreter.Rasterise, scale);
                    }
                    
                    if (filter is ILocationInvariantFilter locInvFilter)
                    {
                        filtered = locInvFilter.Filter(rasterised, scale);
                    }
                    else if (filter is IFilterWithLocation filterWithLoc)
                    {
                        filtered = filterWithLoc.Filter(rasterised, bounds, scale);
                    }

                    if (filtered != null)
                    {
                        rasterised.Dispose();

                        DrawRasterImage(0, 0, filtered.Width, filtered.Height, bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, filtered);
                    }
                }
            }
            else if (this._filterOption.Operation == FilterOption.FilterOperations.RasteriseAllWithVectSharp)
            {
                double scale = this._filterOption.RasterisationResolution;

                Rectangle bounds = graphics.GetBounds();

                bounds = new Rectangle(bounds.Location.X - filter.TopLeftMargin.X, bounds.Location.Y - filter.TopLeftMargin.Y, bounds.Size.Width + filter.TopLeftMargin.X + filter.BottomRightMargin.X, bounds.Size.Height + filter.TopLeftMargin.Y + filter.BottomRightMargin.Y);

                if (bounds.Size.Width > 0 && bounds.Size.Height > 0)
                {
                    if (!this._filterOption.RasterisationResolutionRelative)
                    {
                        scale = scale / Math.Min(bounds.Size.Width, bounds.Size.Height);
                    }

                    if (graphics.TryRasterise(bounds, scale, true, out RasterImage rasterised))
                    {
                        RasterImage filtered = null;

                        if (filter is IFilterWithRasterisableParameter filterWithRastParam)
                        {
                            filterWithRastParam.RasteriseParameter(SKRenderContextInterpreter.Rasterise, scale);
                        }

                        if (filter is ILocationInvariantFilter locInvFilter)
                        {
                            filtered = locInvFilter.Filter(rasterised, scale);
                        }
                        else if (filter is IFilterWithLocation filterWithLoc)
                        {
                            filtered = filterWithLoc.Filter(rasterised, bounds, scale);
                        }

                        if (filtered != null)
                        {
                            rasterised.Dispose();

                            DrawRasterImage(0, 0, filtered.Width, filtered.Height, bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, filtered);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException(@"The filter could not be rasterised! You can avoid this error by doing one of the following:
 • Add a reference to VectSharp.Raster or VectSharp.Raster.ImageSharp (you may also need to add a using directive somewhere to force the assembly to be loaded).
 • Provide your own implementation of Graphics.RasterisationMethod.
 • Set the FilterOption.Operation to ""RasteriseAllWithSkia"", ""IgnoreAll"" or ""SkipAll"".");
                    }
                }
            }
            else if (this._filterOption.Operation == FilterOption.FilterOperations.IgnoreAll)
            {
                graphics.CopyToIGraphicsContext(this);
            }
            else
            {

            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.currentPath?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Determines how and whether image filters are rasterised.
    /// </summary>
    public class FilterOption
    {
        /// <summary>
        /// Defines whether image filters should be rasterised or not.
        /// </summary>
        public enum FilterOperations
        {
            /// <summary>
            /// Image filters will always be rasterised using the SkiaSharp backend.
            /// </summary>
            RasteriseAllWithSkia,

            /// <summary>
            /// Image filters will always be rasterised using the VectSharp.Raster or VectSharp.Raster.ImageSharp. This option requires a reference to VectSharp.Raster or to VectSharp.Raster.ImageSharp to be added.
            /// </summary>
            RasteriseAllWithVectSharp,

            /// <summary>
            /// All image filters will be ignored.
            /// </summary>
            IgnoreAll,

            /// <summary>
            /// All the images that should be drawn with a filter will be ignored.
            /// </summary>
            SkipAll
        }

        /// <summary>
        /// Defines whether image filters should be rasterised or not.
        /// </summary>
        public FilterOperations Operation { get; } = FilterOperations.RasteriseAllWithSkia;

        /// <summary>
        /// The resolution that will be used to rasterise image filters. Depending on the value of <see cref="RasterisationResolutionRelative"/>, this can either be an absolute resolution (i.e. a size in pixel), or a scale factor that is applied to the image size in graphics units.
        /// </summary>
        public double RasterisationResolution { get; } = 1;

        /// <summary>
        /// Determines whether the value of <see cref="RasterisationResolution"/> is absolute (i.e. a size in pixel), or relative (i.e. a scale factor that is applied to the image size in graphics units).
        /// </summary>
        public bool RasterisationResolutionRelative { get; } = true;

        /// <summary>
        /// The default options for image filter rasterisation.
        /// </summary>
        public static FilterOption Default = new FilterOption(FilterOperations.RasteriseAllWithSkia, 1, true);

        /// <summary>
        /// Create a new <see cref="FilterOption"/> object.
        /// </summary>
        /// <param name="operation">Defines whether image filters should be rasterised or not.</param>
        /// <param name="rasterisationResolution">The resolution that will be used to rasterise image filters. Depending on the value of <see cref="RasterisationResolutionRelative"/>, this can either be an absolute resolution (i.e. a size in pixel), or a scale factor that is applied to the image size in graphics units.</param>
        /// <param name="rasterisationResolutionRelative">Determines whether the value of <see cref="RasterisationResolution"/> is absolute (i.e. a size in pixel), or relative (i.e. a scale factor that is applied to the image size in graphics units).</param>
        public FilterOption(FilterOperations operation, double rasterisationResolution, bool rasterisationResolutionRelative)
        {
            this.Operation = operation;
            this.RasterisationResolution = rasterisationResolution;
            this.RasterisationResolutionRelative = rasterisationResolutionRelative;
        }
    }

    /// <summary>
    /// Contains methods to render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/> using the SkiaSharp renderer.
    /// </summary>
    public static class SKRenderContextInterpreter
    {
        internal static SKColor ToSKColor(this Colour colour)
        {
            return new SKColor((byte)(255 * colour.R), (byte)(255 * colour.G), (byte)(255 * colour.B), (byte)(255 * colour.A));
        }

        internal static SKShader ToSKShader(this LinearGradientBrush brush)
        {
            return SKShader.CreateLinearGradient(new SKPoint((float)brush.StartPoint.X, (float)brush.StartPoint.Y), new SKPoint((float)brush.EndPoint.X, (float)brush.EndPoint.Y), (from el in brush.GradientStops select el.Colour.ToSKColor()).ToArray(), (from el in brush.GradientStops select (float)el.Offset).ToArray(), SKShaderTileMode.Clamp);
        }

        internal static SKShader ToSKShader(this RadialGradientBrush brush)
        {
            return SKShader.CreateTwoPointConicalGradient(new SKPoint((float)brush.FocalPoint.X, (float)brush.FocalPoint.Y), 0, new SKPoint((float)brush.Centre.X, (float)brush.Centre.Y), (float)brush.Radius, (from el in brush.GradientStops select el.Colour.ToSKColor()).ToArray(), (from el in brush.GradientStops select (float)el.Offset).ToArray(), SKShaderTileMode.Clamp);
        }

        /// <summary>
        /// Render a <see cref="Document"/> to an <see cref="Avalonia.Controls.Canvas"/> using the SkiaSharp renderer. Each page corresponds to a layer in the image.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to render.</param>
        /// <param name="width">The width of the document. If this is <see langword="null" />, the width of the largest page is used.</param>
        /// <param name="height">The height of the document. If this is <see langword="null" />, the height of the largest page is used.</param>
        /// <param name="background">The background colour of the document. If this is <see langword="null" />, a transparent background is used.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static SKMultiLayerRenderCanvas PaintToSKCanvas(this Document document, double? width = null, double? height = null, Colour? background = null, AvaloniaContextInterpreter.TextOptions textOption = AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary, FilterOption filterOption = default)
        {
            filterOption = filterOption ?? FilterOption.Default;

            return new SKMultiLayerRenderCanvas((from el in document.Pages select el.CopyToSKRenderContext(textOption, filterOption)).ToList(), (from el in document.Pages select SKRenderAction.TransformAction(SKMatrix.Identity)).ToList(), background ?? Colour.FromRgba(0, 0, 0, 0), width ?? (from el in document.Pages select el.Width).Max(), height ?? (from el in document.Pages select el.Height).Max());
        }

        /// <summary>
        /// Render a <see cref="Document"/> to an <see cref="Avalonia.Controls.Canvas"/> using the SkiaSharp renderer. Each page corresponds to a layer in the image.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to render.</param>
        /// <param name="taggedActions">A Dictionary containing the actions that will be performed on items with the corresponding tag.
        /// These should be functions that accept one parameter of type <see cref="SKRenderAction"/> and return an <see cref="IEnumerable{SKRenderAction}"/> of the render actions that will actually be added to the plot.</param>
        /// <param name="removeTaggedActionsAfterExecution">Whether the actions should be removed from <paramref name="taggedActions"/> after their execution. Set to false if the same action should be performed on multiple items with the same tag.</param>
        /// <param name="width">The width of the document. If this is <see langword="null" />, the width of the largest page is used.</param>
        /// <param name="height">The height of the document. If this is <see langword="null" />, the height of the largest page is used.</param>
        /// <param name="background">The background colour of the document. If this is <see langword="null" />, a transparent background is used.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static SKMultiLayerRenderCanvas PaintToSKCanvas(this Document document, Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>> taggedActions, bool removeTaggedActionsAfterExecution = true, double? width = null, double? height = null, Colour? background = null, AvaloniaContextInterpreter.TextOptions textOption = AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary, FilterOption filterOption = default)
        {
            filterOption = filterOption ?? FilterOption.Default;

            return new SKMultiLayerRenderCanvas((from el in document.Pages select el.CopyToSKRenderContext(taggedActions, removeTaggedActionsAfterExecution, textOption, filterOption)).ToList(), (from el in document.Pages select SKRenderAction.TransformAction(SKMatrix.Identity)).ToList(), background ?? Colour.FromRgba(0, 0, 0, 0), width ?? (from el in document.Pages select el.Width).Max(), height ?? (from el in document.Pages select el.Height).Max());
        }

        /// <summary>
        /// Render a <see cref="Document"/> to an <see cref="Avalonia.Controls.Canvas"/> using the SkiaSharp renderer. Each page corresponds to a layer in the image.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to render.</param>
        /// <param name="taggedActions">A Dictionary containing the actions that will be performed on items with the corresponding tag.
        /// These should be functions that accept one parameter of type <see cref="SKRenderAction"/> and return an <see cref="IEnumerable{SKRenderAction}"/> of the render actions that will actually be added to the plot.</param>
        /// <param name="images">A dictionary that associates to each raster image path (or data URL) the image rendered as a <see cref="SKBitmap"/> and a boolean value indicating whether it should be drawn as "pixelated" or not. This will be populated automatically as the page is rendered.
        /// If you are rendering multiple <see cref="Page"/>s (or you are rendering the same page multiple times), it will be beneficial to keep a reference to this dictionary and pass it again on further rendering requests; otherwise, you can just pass an empty dictionary.</param>
        /// <param name="removeTaggedActionsAfterExecution">Whether the actions should be removed from <paramref name="taggedActions"/> after their execution. Set to false if the same action should be performed on multiple items with the same tag.</param>
        /// <param name="width">The width of the document. If this is <see langword="null" />, the width of the largest page is used.</param>
        /// <param name="height">The height of the document. If this is <see langword="null" />, the height of the largest page is used.</param>
        /// <param name="background">The background colour of the document. If this is <see langword="null" />, a transparent background is used.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static SKMultiLayerRenderCanvas PaintToSKCanvas(this Document document, Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>> taggedActions, Dictionary<string, (SKBitmap, bool)> images, bool removeTaggedActionsAfterExecution = true, double? width = null, double? height = null, Colour? background = null, AvaloniaContextInterpreter.TextOptions textOption = AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary, FilterOption filterOption = default)
        {
            filterOption = filterOption ?? FilterOption.Default;

            return new SKMultiLayerRenderCanvas((from el in document.Pages select el.CopyToSKRenderContext(taggedActions, images, removeTaggedActionsAfterExecution, textOption, filterOption)).ToList(), (from el in document.Pages select SKRenderAction.TransformAction(SKMatrix.Identity)).ToList(), background ?? Colour.FromRgba(0, 0, 0, 0), width ?? (from el in document.Pages select el.Width).Max(), height ?? (from el in document.Pages select el.Height).Max());
        }

        /// <summary>
        /// Render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/> using the SkiaSharp renderer.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static SKMultiLayerRenderCanvas PaintToSKCanvas(this Page page, AvaloniaContextInterpreter.TextOptions textOption = AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary, FilterOption filterOption = default)
        {
            filterOption = filterOption ?? FilterOption.Default;

            return new SKMultiLayerRenderCanvas(new List<SKRenderContext>() { page.CopyToSKRenderContext(textOption, filterOption) }, new List<SKRenderAction>() { SKRenderAction.TransformAction(SKMatrix.Identity) }, page.Background, page.Width, page.Height);
        }

        /// <summary>
        /// Render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/> using the SkiaSharpRenderer.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="taggedActions">A Dictionary containing the actions that will be performed on items with the corresponding tag.
        /// These should be functions that accept one parameter of type <see cref="SKRenderAction"/> and return an <see cref="IEnumerable{SKRenderAction}"/> of the render actions that will actually be added to the plot.</param>
        /// <param name="removeTaggedActionsAfterExecution">Whether the actions should be removed from <paramref name="taggedActions"/> after their execution. Set to false if the same action should be performed on multiple items with the same tag.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static SKMultiLayerRenderCanvas PaintToSKCanvas(this Page page, Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>> taggedActions, bool removeTaggedActionsAfterExecution = true, AvaloniaContextInterpreter.TextOptions textOption = AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary, FilterOption filterOption = default)
        {
            filterOption = filterOption ?? FilterOption.Default;

            return new SKMultiLayerRenderCanvas(new List<SKRenderContext>() { page.CopyToSKRenderContext(taggedActions, removeTaggedActionsAfterExecution, textOption, filterOption) }, new List<SKRenderAction>() { SKRenderAction.TransformAction(SKMatrix.Identity) }, page.Background, page.Width, page.Height);
        }

        /// <summary>
        /// Render a <see cref="Page"/> to an <see cref="Avalonia.Controls.Canvas"/> using the SkiaSharpRenderer.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="taggedActions">A Dictionary containing the actions that will be performed on items with the corresponding tag.
        /// These should be functions that accept one parameter of type <see cref="SKRenderAction"/> and return an <see cref="IEnumerable{SKRenderAction}"/> of the render actions that will actually be added to the plot.</param>
        /// <param name="images">A dictionary that associates to each raster image path (or data URL) the image rendered as a <see cref="SKBitmap"/> and a boolean value indicating whether it should be drawn as "pixelated" or not. This will be populated automatically as the page is rendered.
        /// If you are rendering multiple <see cref="Page"/>s (or you are rendering the same page multiple times), it will be beneficial to keep a reference to this dictionary and pass it again on further rendering requests; otherwise, you can just pass an empty dictionary.</param>
        /// <param name="removeTaggedActionsAfterExecution">Whether the actions should be removed from <paramref name="taggedActions"/> after their execution. Set to false if the same action should be performed on multiple items with the same tag.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <returns>An <see cref="Avalonia.Controls.Canvas"/> containing the rendered graphics objects.</returns>
        public static SKMultiLayerRenderCanvas PaintToSKCanvas(this Page page, Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>> taggedActions, Dictionary<string, (SKBitmap, bool)> images, bool removeTaggedActionsAfterExecution = true, AvaloniaContextInterpreter.TextOptions textOption = AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary, FilterOption filterOption = default)
        {
            filterOption = filterOption ?? FilterOption.Default;

            return new SKMultiLayerRenderCanvas(new List<SKRenderContext>() { page.CopyToSKRenderContext(taggedActions, images, removeTaggedActionsAfterExecution, textOption, filterOption) }, new List<SKRenderAction>() { SKRenderAction.TransformAction(SKMatrix.Identity) }, page.Background, page.Width, page.Height);
        }

        /// <summary>
        /// Render a <see cref="Page"/> to a <see cref="SKRenderContext"/>. This can be drawn using the SkiaSharpRenderer by adding it to a <see cref="SKMultiLayerRenderCanvas"/>.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <returns>A <see cref="SKRenderContext"/> containing the rendered graphics objects.</returns>
        public static SKRenderContext CopyToSKRenderContext(this Page page, AvaloniaContextInterpreter.TextOptions textOption = AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary, FilterOption filterOption = default)
        {
            filterOption = filterOption ?? FilterOption.Default;

            return CopyToSKRenderContext(page, new Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>>(), new Dictionary<string, (SKBitmap, bool)>(), textOption: textOption, filterOption: filterOption);
        }

        /// <summary>
        /// Render a <see cref="Page"/> to a <see cref="SKRenderContext"/>. This can be drawn using the SkiaSharpRenderer by adding it to a <see cref="SKMultiLayerRenderCanvas"/>.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="taggedActions">A Dictionary containing the actions that will be performed on items with the corresponding tag.
        /// These should be functions that accept one parameter of type <see cref="SKRenderAction"/> and return an <see cref="IEnumerable{SKRenderAction}"/> of the render actions that will actually be added to the plot.</param>
        /// <param name="removeTaggedActionsAfterExecution">Whether the actions should be removed from <paramref name="taggedActions"/> after their execution. Set to false if the same action should be performed on multiple items with the same tag.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <returns>A <see cref="SKRenderContext"/> containing the rendered graphics objects.</returns>
        public static SKRenderContext CopyToSKRenderContext(this Page page, Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>> taggedActions, bool removeTaggedActionsAfterExecution = true, AvaloniaContextInterpreter.TextOptions textOption = AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary, FilterOption filterOption = default)
        {
            filterOption = filterOption ?? FilterOption.Default;

            return CopyToSKRenderContext(page, taggedActions, new Dictionary<string, (SKBitmap, bool)>(), removeTaggedActionsAfterExecution, textOption, filterOption);
        }

        /// <summary>
        /// Render a <see cref="Page"/> to a <see cref="SKRenderContext"/>. This can be drawn using the SkiaSharpRenderer by adding it to a <see cref="SKMultiLayerRenderCanvas"/>.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to render.</param>
        /// <param name="taggedActions">A Dictionary containing the actions that will be performed on items with the corresponding tag.
        /// These should be functions that accept one parameter of type <see cref="SKRenderAction"/> and return an <see cref="IEnumerable{SKRenderAction}"/> of the render actions that will actually be added to the plot.</param>
        /// <param name="images">A dictionary that associates to each raster image path (or data URL) the image rendered as a <see cref="SKBitmap"/> and a boolean value indicating whether it should be drawn as "pixelated" or not. This will be populated automatically as the page is rendered.
        /// If you are rendering multiple <see cref="Page"/>s (or you are rendering the same page multiple times), it will be beneficial to keep a reference to this dictionary and pass it again on further rendering requests; otherwise, you can just pass an empty dictionary.</param>
        /// <param name="removeTaggedActionsAfterExecution">Whether the actions should be removed from <paramref name="taggedActions"/> after their execution. Set to false if the same action should be performed on multiple items with the same tag.</param>
        /// <param name="textOption">Defines whether text items should be converted into paths when drawing.</param>
        /// <param name="filterOption">Defines how and whether image filters should be rasterised when rendering the image.</param>
        /// <returns>A <see cref="SKRenderContext"/> containing the rendered graphics objects.</returns>
        public static SKRenderContext CopyToSKRenderContext(this Page page, Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>> taggedActions, Dictionary<string, (SKBitmap, bool)> images, bool removeTaggedActionsAfterExecution = true, AvaloniaContextInterpreter.TextOptions textOption = AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary, FilterOption filterOption = default)
        {
            filterOption = filterOption ?? FilterOption.Default;

            SKRenderContextImpl tbr = new SKRenderContextImpl(page.Width, page.Height, removeTaggedActionsAfterExecution, textOption, images, filterOption)
            {
                TaggedActions = taggedActions
            };
            page.Graphics.CopyToIGraphicsContext(tbr);

            return tbr;
        }


        /// <summary>
        /// Rasterise a region of a <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object that will be rasterised.</param>
        /// <param name="region">The region of the <paramref name="graphics"/> that will be rasterised.</param>
        /// <param name="scale">The scale at which the image will be rendered.</param>
        /// <param name="interpolate">Whether the resulting image should be interpolated or not when it is drawn on another <see cref="Graphics"/> surface.</param>
        /// <returns>A <see cref="RasterImage"/> containing the rasterised graphics.</returns>
        public static RasterImage Rasterise(this Graphics graphics, Rectangle region, double scale, bool interpolate)
        {
            Page pag = new Page(1, 1);
            pag.Graphics.DrawGraphics(0, 0, graphics);
            pag.Crop(region.Location, region.Size);

            //pag.PaintToSKCanvas().RenderAtResolution((int)Math.Round(region.Size.Width * scale), (int)Math.Round(region.Size.Height * scale), null);

            int width = (int)Math.Round(region.Size.Width * scale);
            int height = (int)Math.Round(region.Size.Height * scale);

            SKRenderContext ctx = pag.CopyToSKRenderContext();

            SKBitmap bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);

            SKCanvas canvas = new SKCanvas(bitmap);

            canvas.Save();

            for (int i = 0; i < ctx.SKRenderActions.Count; i++)
            {
                if (ctx.SKRenderActions[i].ActionType == SKRenderAction.ActionTypes.Clip)
                {
                    canvas.ClipPath(ctx.SKRenderActions[i].Path, antialias: true);
                }
                else if (ctx.SKRenderActions[i].ActionType == SKRenderAction.ActionTypes.Restore)
                {
                    canvas.Restore();
                }
                else if (ctx.SKRenderActions[i].ActionType == SKRenderAction.ActionTypes.Save)
                {
                    canvas.Save();
                }
                else if (ctx.SKRenderActions[i].ActionType == SKRenderAction.ActionTypes.Transform)
                {
                    SKMatrix mat = ctx.SKRenderActions[i].Transform.Value;
                    canvas.Concat(ref mat);
                }
                else
                {
                    if (ctx.SKRenderActions[i].ActionType == SKRenderAction.ActionTypes.Path && ctx.SKRenderActions[i].Path != null)
                    {
                        canvas.DrawPath(ctx.SKRenderActions[i].Path, ctx.SKRenderActions[i].Paint);
                    }
                    else if (ctx.SKRenderActions[i].ActionType == SKRenderAction.ActionTypes.Text)
                    {
                        canvas.DrawText(ctx.SKRenderActions[i].Text, ctx.SKRenderActions[i].TextX, ctx.SKRenderActions[i].TextY, ctx.SKRenderActions[i].Font, ctx.SKRenderActions[i].Paint);
                    }
                    else if (ctx.SKRenderActions[i].ActionType == SKRenderAction.ActionTypes.RasterImage)
                    {
                        (SKBitmap image, bool interpolateIt) = ctx.Images[ctx.SKRenderActions[i].ImageId];

                        SKPaint paint;

                        if (!interpolateIt)
                        {
                            paint = null;
                        }
                        else
                        {
                            paint = new SKPaint() { FilterQuality = SKFilterQuality.Medium };
                        }

                        canvas.DrawBitmap(image, ctx.SKRenderActions[i].ImageSource.Value, ctx.SKRenderActions[i].ImageDestination.Value, paint);

                        paint?.Dispose();
                    }
                }
            }

            canvas.Restore();

            IntPtr pixels = bitmap.GetPixels(out IntPtr length);

            IntPtr tbrData = System.Runtime.InteropServices.Marshal.AllocHGlobal(length);
            GC.AddMemoryPressure((long)length);

            unsafe
            {
                Buffer.MemoryCopy((void*)pixels, (void*)tbrData, (long)length, (long)length);
            }

            canvas.Dispose();
            bitmap.Dispose();

            DisposableIntPtr disp = new DisposableIntPtr(tbrData);
            return new RasterImage(ref disp, width, height, true, interpolate);
        }
    }
}
