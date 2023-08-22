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

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace VectSharp.Canvas
{
    internal interface ISKRenderCanvas
    {
        void InvalidateDirty();
        void InvalidateZIndex();
    }

    /// <summary>
    /// Represents a multi-threaded, triple-buffered canvas on which the image is drawn using SkiaSharp.
    /// </summary>
    public class SKMultiLayerRenderCanvas : Avalonia.Controls.UserControl, IDisposable, ISKRenderCanvas
    {
        /// <summary>
        /// The width of the page that is rendered on this canvas.
        /// </summary>
        public double PageWidth { get; set; }

        /// <summary>
        /// The height of the page that is rendered on this canvas.
        /// </summary>
        public double PageHeight { get; set; }
        private SKColor BackgroundColour;

        /// <summary>
        /// Defines an overdraw margin for the clipping area. Set this to a larger value if quickly moving the
        /// canvas around (e.g. in a ZoomBorder) causes the edge of the image to disappear for a few milliseconds
        /// and that annoys you. If <see cref="RelativePoint.Unit"/> is set to <see cref="RelativeUnit.Absolute"/>,
        /// the value corresponds to units on the <see cref="VectSharp.Page"/>; if it is set to <see cref="RelativeUnit.Relative"/>,
        /// it corresponds to a proportion of the visible area.
        /// </summary>
        public RelativePoint ClipMargin { get; set; } = new RelativePoint(0, 0, RelativeUnit.Absolute);

        /// <summary>
        /// The list of render actions. Each element in this list is itself a list, containing the actions that correspond to a layer in the image.
        /// </summary>
        public List<List<SKRenderAction>> RenderActions;

        /// <summary>
        /// The list of transforms associated with each layer.
        /// </summary>
        public List<SKRenderAction> LayerTransforms;

        private Dictionary<string, (SKBitmap, bool)> Images;
        private List<List<SKRenderAction>> TaggedRenderActions;

        /// <summary>
        /// Create a new SKMultiLayerRenderCanvas from a <see cref="Document"/>, where each page represents a layer.
        /// </summary>
        /// <param name="document">The document containing the layers as <see cref="Page"/>s.</param>
        /// <param name="layerTransforms">A list of transforms associated with each layer. This list should contain the same number of elements as the number of pages in <paramref name="document"/>. This is useful to manipulate the position of each layer individually. If this is null, an identity transform is applied to each layer.</param>
        /// <param name="backgroundColour">The background colour of the canvas.</param>
        /// <param name="width">The width of the canvas and the pages it contains.</param>
        /// <param name="height">The height of the canvas and the pages it contains.</param>
        public SKMultiLayerRenderCanvas(Document document, Colour backgroundColour, double width, double height, List<SKRenderAction> layerTransforms = null) : this(document.Pages, backgroundColour, width, height, layerTransforms) { }

        /// <summary>
        /// Create a new SKMultiLayerRenderCanvas from a collection of <see cref="Page"/>s, each representing a layer.
        /// </summary>
        /// <param name="layers">The contents of the canvas. Each element in this list represents a layer.</param>
        /// <param name="layerTransforms">A list of transforms associated with each layer. This list should contain the same number of elements as <paramref name="layers"/>. This is useful to manipulate the position of each layer individually. If this is null, an identity transform is applied to each layer.</param>
        /// <param name="backgroundColour">The background colour of the canvas.</param>
        /// <param name="width">The width of the canvas and the pages it contains.</param>
        /// <param name="height">The height of the canvas and the pages it contains.</param>
        public SKMultiLayerRenderCanvas(IEnumerable<Page> layers, Colour backgroundColour, double width, double height, List<SKRenderAction> layerTransforms = null)
        {
            List<SKRenderContext> contents = (from el in layers select el.CopyToSKRenderContext()).ToList();
            List<SKRenderAction> contentTransforms;

            if (layerTransforms == null)
            {
                contentTransforms = (from el in Enumerable.Range(0, contents.Count) select SKRenderAction.TransformAction(SKMatrix.Identity)).ToList();
            }
            else
            {
                contentTransforms = layerTransforms;
            }

            UpdateWith(contents, contentTransforms, backgroundColour, width, height);

            this.Width = width;
            this.Height = height;

            this.PointerPressed += this.PointerPressedAction;
            this.PointerReleased += this.PointerReleasedAction;
            this.PointerMoved += this.PointerMoveAction;
            this.PointerExited += this.PointerLeaveAction;
        }

        /// <summary>
        /// Create a new SKMultiLayerRenderCanvas from a list of SKRenderContexts, each representing a layer.
        /// </summary>
        /// <param name="contents">The contents of the canvas. Each element in this list represents a layer. A Page can be converded to a SKRenderContext through the CopyToSKRenderContext method.</param>
        /// <param name="contentTransforms">A list of transforms associated with each layer. This list should contain the same number of elements as <paramref name="contents"/>. This is useful to manipulate the position of each layer individually.</param>
        /// <param name="backgroundColour">The background colour of the canvas.</param>
        /// <param name="width">The width of the canvas and the page it contains.</param>
        /// <param name="height">The height of the canvas and the page it contains.</param>
        public SKMultiLayerRenderCanvas(List<SKRenderContext> contents, List<SKRenderAction> contentTransforms, Colour backgroundColour, double width, double height)
        {
            UpdateWith(contents, contentTransforms, backgroundColour, width, height);

            this.Width = width;
            this.Height = height;

            this.PointerPressed += this.PointerPressedAction;
            this.PointerReleased += this.PointerReleasedAction;
            this.PointerMoved += this.PointerMoveAction;
            this.PointerExited += this.PointerLeaveAction;
        }

        /// <summary>
        /// Replace the contents of the SKMultiLayerRenderCanvas with the specified layers.
        /// </summary>
        /// <param name="contents">The contents of the canvas. Each element in this list represents a layer. A Page can be converded to a SKRenderContext through the CopyToSKRenderContext method.</param>
        /// <param name="contentTransforms">A list of transforms associated with each layer. This list should contain the same number of elements as <paramref name="contents"/>. This is useful to manipulate the position of each layer individually.</param>
        /// <param name="backgroundColour">The background colour of the canvas.</param>
        /// <param name="width">The width of the canvas and the page it contains.</param>
        /// <param name="height">The height of the canvas and the page it contains.</param>
        public void UpdateWith(List<SKRenderContext> contents, List<SKRenderAction> contentTransforms, Colour backgroundColour, double width, double height)
        {
            lock (RenderLock)
            {
                if (this.RenderActions == null)
                {
                    this.RenderActions = new List<List<SKRenderAction>>();
                }
                else
                {
                    this.RenderActions.Clear();
                }

                if (this.LayerTransforms == null)
                {
                    this.LayerTransforms = new List<SKRenderAction>();
                }
                else
                {
                    this.LayerTransforms.Clear();
                }

                this.PageWidth = width;
                this.PageHeight = height;
                this.BackgroundColour = backgroundColour.ToSKColor();

                if (this.Images == null)
                {
                    Images = new Dictionary<string, (SKBitmap, bool)>();
                }
                else
                {
                    Images.Clear();
                }

                for (int i = 0; i < contents.Count; i++)
                {
                    this.LayerTransforms.Add(contentTransforms[i]);
                    this.RenderActions.Add(contents[i].SKRenderActions);

                    foreach (KeyValuePair<string, (SKBitmap, bool)> kvp in contents[i].Images)
                    {
                        this.Images[kvp.Key] = kvp.Value;
                    }
                }

                if (this.TaggedRenderActions == null)
                {
                    this.TaggedRenderActions = new List<List<SKRenderAction>>();
                }
                else
                {
                    this.TaggedRenderActions.Clear();
                }


                for (int i = this.RenderActions.Count - 1; i >= 0; i--)
                {
                    TaggedRenderActions.Add(new List<SKRenderAction>());

                    for (int j = this.RenderActions[i].Count - 1; j >= 0; j--)
                    {
                        RenderActions[i][j].InternalParent = this;
                        if (!string.IsNullOrEmpty(this.RenderActions[i][j].Tag))
                        {
                            TaggedRenderActions[this.RenderActions.Count - 1 - i].Add(this.RenderActions[i][j]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replace a single layer with the specified content.
        /// </summary>
        /// <param name="layer">The index of the layer to replace.</param>
        /// <param name="newContent">The new contents of the layer. A Page can be converded to a SKRenderContext through the CopyToSKRenderContext method.</param>
        /// <param name="newTransform">The new transform for the layer.</param>
        public void UpdateLayer(int layer, SKRenderContext newContent, SKRenderAction newTransform)
        {
            lock (RenderLock)
            {
                this.LayerTransforms[layer] = newTransform;
                this.RenderActions[layer] = newContent.SKRenderActions;

                foreach (KeyValuePair<string, (SKBitmap, bool)> kvp in newContent.Images)
                {
                    this.Images[kvp.Key] = kvp.Value;
                }

                TaggedRenderActions[this.RenderActions.Count - 1 - layer].Clear();

                for (int j = this.RenderActions[layer].Count - 1; j >= 0; j--)
                {
                    RenderActions[layer][j].InternalParent = this;
                    if (!string.IsNullOrEmpty(this.RenderActions[layer][j].Tag))
                    {
                        TaggedRenderActions[this.RenderActions.Count - 1 - layer].Add(this.RenderActions[layer][j]);
                    }
                }
            }

            this.InvalidateDirty();
        }

        /// <summary>
        /// Add a new layer to the image.
        /// </summary>
        /// <param name="newContent">The contents of the new layer. A Page can be converded to a SKRenderContext through the CopyToSKRenderContext method.</param>
        /// <param name="newTransform">The transform for the new layer.</param>
        public void AddLayer(SKRenderContext newContent, SKRenderAction newTransform)
        {
            lock (RenderLock)
            {
                this.LayerTransforms.Add(newTransform);
                this.RenderActions.Add(newContent.SKRenderActions);

                foreach (KeyValuePair<string, (SKBitmap, bool)> kvp in newContent.Images)
                {
                    this.Images[kvp.Key] = kvp.Value;
                }

                TaggedRenderActions.Insert(0, new List<SKRenderAction>());

                for (int j = this.RenderActions[this.RenderActions.Count - 1].Count - 1; j >= 0; j--)
                {
                    RenderActions[this.RenderActions.Count - 1][j].InternalParent = this;
                    if (!string.IsNullOrEmpty(this.RenderActions[this.RenderActions.Count - 1][j].Tag))
                    {
                        TaggedRenderActions[0].Add(this.RenderActions[this.RenderActions.Count - 1][j]);
                    }
                }
            }

            this.InvalidateDirty();
        }

        /// <summary>
        /// Insert a new layer at the specified index.
        /// </summary>
        /// <param name="index">The position at which the new layer will be inserted.</param>
        /// <param name="newContent">The contents of the new layer.</param>
        /// <param name="newTransform">The transform for the new layer.</param>
        public void InsertLayer(int index, SKRenderContext newContent, SKRenderAction newTransform)
        {
            lock (RenderLock)
            {
                this.LayerTransforms.Insert(index, newTransform);
                this.RenderActions.Insert(index, newContent.SKRenderActions);

                foreach (KeyValuePair<string, (SKBitmap, bool)> kvp in newContent.Images)
                {
                    this.Images[kvp.Key] = kvp.Value;
                }

                TaggedRenderActions.Insert(this.RenderActions.Count - 1 - index, new List<SKRenderAction>());

                for (int j = this.RenderActions[index].Count - 1; j >= 0; j--)
                {
                    RenderActions[index][j].InternalParent = this;
                    if (!string.IsNullOrEmpty(this.RenderActions[index][j].Tag))
                    {
                        TaggedRenderActions[this.RenderActions.Count - 1 - index].Add(this.RenderActions[index][j]);
                    }
                }
            }

            this.InvalidateDirty();
        }

        /// <summary>
        /// Remove the specified layer from the image.
        /// </summary>
        /// <param name="layer">The index of the layer to remove.</param>
        public void RemoveLayer(int layer)
        {
            lock (RenderLock)
            {
                this.LayerTransforms[layer].Dispose();
                this.LayerTransforms.RemoveAt(layer);

                for (int i = 0; i < this.RenderActions[layer].Count; i++)
                {
                    this.RenderActions[layer][i].Dispose();
                }
                this.RenderActions.RemoveAt(layer);

                TaggedRenderActions.RemoveAt(this.TaggedRenderActions.Count - 1 - layer);
            }

            this.InvalidateDirty();
        }

        /// <summary>
        /// Switch the position of the two specified layers.
        /// </summary>
        /// <param name="layer1">The index of the first layer to switch.</param>
        /// <param name="layer2">The index of the second layer to switch.</param>
        public void SwitchLayers(int layer1, int layer2)
        {
            lock (RenderLock)
            {
                var temp = this.LayerTransforms[layer1];
                this.LayerTransforms[layer1] = this.LayerTransforms[layer2];
                this.LayerTransforms[layer2] = temp;

                var temp2 = this.RenderActions[layer1];
                this.RenderActions[layer1] = this.RenderActions[layer2];
                this.RenderActions[layer2] = temp2;

                var temp3 = this.TaggedRenderActions[this.RenderActions.Count - 1 - layer1];
                this.TaggedRenderActions[this.RenderActions.Count - 1 - layer1] = this.TaggedRenderActions[this.RenderActions.Count - 1 - layer2];
                this.TaggedRenderActions[this.RenderActions.Count - 1 - layer2] = temp3;
            }

            this.InvalidateDirty();
        }

        /// <summary>
        /// Move the specified layer to the specified position, shifting all other layers as necessary.
        /// </summary>
        /// <param name="oldIndex">The current index of the layer to move.</param>
        /// <param name="newIndex">The final index of the layer. Layers after this will be shifted by 1 in order to accommodate the moved layer.</param>
        public void MoveLayer(int oldIndex, int newIndex)
        {
            lock (RenderLock)
            {
                var temp = this.LayerTransforms[oldIndex];
                this.LayerTransforms.RemoveAt(oldIndex);
                this.LayerTransforms.Insert(newIndex, temp);

                var temp2 = this.RenderActions[oldIndex];
                this.RenderActions.RemoveAt(oldIndex);
                this.RenderActions.Insert(newIndex, temp2);

                var temp3 = this.TaggedRenderActions[this.RenderActions.Count - 1 - oldIndex];
                this.TaggedRenderActions.RemoveAt(this.RenderActions.Count - 1 - oldIndex);
                this.TaggedRenderActions.Insert(this.RenderActions.Count - 1 - newIndex, temp3);
            }

            this.InvalidateDirty();
        }


        private SKRenderAction CurrentPressedAction = null;
        private void PointerPressedAction(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            SKPoint position = e.GetPosition(this).ToSKPoint();

            for (int i = 0; i < TaggedRenderActions.Count; i++)
            {
                bool found = false;

                for (int j = 0; j < TaggedRenderActions[i].Count; j++)
                {
                    if (TaggedRenderActions[i][j].LastRenderedGlobalHitTestPath?.Contains(position.X, position.Y) == true)
                    {

                        if (TaggedRenderActions[i][j].ActionType == SKRenderAction.ActionTypes.Path)
                        {
                            CurrentPressedAction = TaggedRenderActions[i][j];
                            TaggedRenderActions[i][j].FirePointerPressed(e);
                            found = true;
                            break;
                        }
                        else if (TaggedRenderActions[i][j].ActionType == SKRenderAction.ActionTypes.Text)
                        {
                            CurrentPressedAction = TaggedRenderActions[i][j];
                            TaggedRenderActions[i][j].FirePointerPressed(e);
                            found = true;
                            break;
                        }
                        else if (TaggedRenderActions[i][j].ActionType == SKRenderAction.ActionTypes.RasterImage)
                        {
                            CurrentPressedAction = TaggedRenderActions[i][j];
                            TaggedRenderActions[i][j].FirePointerPressed(e);
                            found = true;
                            break;
                        }
                    }
                }

                if (found)
                {
                    break;
                }
            }
        }

        private void PointerReleasedAction(object sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (CurrentPressedAction != null)
            {
                if (!CurrentPressedAction.Disposed)
                {
                    CurrentPressedAction.FirePointerReleased(e);
                }
                CurrentPressedAction = null;
            }
            else
            {
                SKPoint position = e.GetPosition(this).ToSKPoint();

                for (int i = 0; i < TaggedRenderActions.Count; i++)
                {
                    bool found = false;

                    for (int j = 0; j < TaggedRenderActions[i].Count; j++)
                    {
                        if (TaggedRenderActions[i][j].LastRenderedGlobalHitTestPath?.Contains(position.X, position.Y) == true)
                        {
                            if (TaggedRenderActions[i][j].ActionType == SKRenderAction.ActionTypes.Path)
                            {
                                TaggedRenderActions[i][j].FirePointerReleased(e);
                                found = true;
                                break;
                            }
                            else if (TaggedRenderActions[i][j].ActionType == SKRenderAction.ActionTypes.Text)
                            {
                                TaggedRenderActions[i][j].FirePointerReleased(e);
                                found = true;
                                break;
                            }
                            else if (TaggedRenderActions[i][j].ActionType == SKRenderAction.ActionTypes.RasterImage)
                            {
                                TaggedRenderActions[i][j].FirePointerReleased(e);
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found)
                    {
                        break;
                    }
                }
            }
        }

        private SKRenderAction CurrentOverAction = null;
        private void PointerMoveAction(object sender, Avalonia.Input.PointerEventArgs e)
        {
            SKPoint position = e.GetPosition(this).ToSKPoint();

            bool found = false;

            for (int i = 0; i < TaggedRenderActions.Count; i++)
            {
                for (int j = 0; j < TaggedRenderActions[i].Count; j++)
                {
                    if (TaggedRenderActions[i][j].LastRenderedGlobalHitTestPath?.Contains(position.X, position.Y) == true)
                    {
                        if (TaggedRenderActions[i][j].ActionType == SKRenderAction.ActionTypes.Path)
                        {
                            found = true;

                            if (CurrentOverAction != TaggedRenderActions[i][j])
                            {
                                if (CurrentOverAction != null && !CurrentOverAction.Disposed)
                                {
                                    CurrentOverAction.FirePointerExited(e);
                                }
                                CurrentOverAction = TaggedRenderActions[i][j];
                                CurrentOverAction.FirePointerEntered(e);
                            }

                            break;
                        }
                        else if (TaggedRenderActions[i][j].ActionType == SKRenderAction.ActionTypes.Text)
                        {
                            found = true;

                            if (CurrentOverAction != TaggedRenderActions[i][j])
                            {
                                if (CurrentOverAction != null && !CurrentOverAction.Disposed)
                                {
                                    CurrentOverAction.FirePointerExited(e);
                                }
                                CurrentOverAction = TaggedRenderActions[i][j];
                                CurrentOverAction.FirePointerEntered(e);
                            }

                            break;
                        }
                        else if (TaggedRenderActions[i][j].ActionType == SKRenderAction.ActionTypes.RasterImage)
                        {
                            found = true;

                            if (CurrentOverAction != TaggedRenderActions[i][j])
                            {
                                if (CurrentOverAction != null && !CurrentOverAction.Disposed)
                                {
                                    CurrentOverAction.FirePointerExited(e);
                                }
                                CurrentOverAction = TaggedRenderActions[i][j];
                                CurrentOverAction.FirePointerEntered(e);
                            }

                            break;
                        }
                    }
                }

                if (found)
                {
                    break;
                }
            }

            if (!found)
            {
                if (CurrentOverAction != null && !CurrentOverAction.Disposed)
                {
                    CurrentOverAction.FirePointerExited(e);
                }
                CurrentOverAction = null;
            }
        }

        private void PointerLeaveAction(object sender, Avalonia.Input.PointerEventArgs e)
        {
            if (CurrentOverAction != null && !CurrentOverAction.Disposed)
            {
                CurrentOverAction.FirePointerExited(e);
            }
            CurrentOverAction = null;
        }

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            StartRenderingThread();
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            StopRenderingThread();
        }

        private void StopRenderingThread()
        {
            DisposedHandle.Set();
        }

        private EventWaitHandle DisposedHandle;
        private EventWaitHandle RenderRequestedHandle;

        private RenderingParameters RenderingRequest = null;
        private readonly object RenderingRequestLock = new object();

        private void StartRenderingThread()
        {
            DisposedHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            RenderRequestedHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            Thread renderingThread = new Thread(async () =>
            {
                bool finished = false;
                WaitHandle[] handles = new WaitHandle[] { DisposedHandle, RenderRequestedHandle };

                while (!finished)
                {
                    int handle = EventWaitHandle.WaitAny(handles);

                    if (handle == 0)
                    {
                        finished = true;
                    }
                    else if (handle == 1)
                    {
                        RenderingParameters requestParams;

                        lock (RenderingRequestLock)
                        {
                            requestParams = RenderingRequest.Clone();
                            RenderRequestedHandle.Reset();
                        }

                        SKCanvas canvas;

                        if (BackBuffer == null || BackBufferRenderingParams == null || requestParams.RenderWidth > BackBufferRenderingParams.RenderWidth || requestParams.RenderHeight > BackBufferRenderingParams.RenderHeight)
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                SkiaBitmap tempBufferReference = BackBuffer;

                                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    tempBufferReference?.Dispose();
                                }, Avalonia.Threading.DispatcherPriority.Send);

                                BackBuffer = new SkiaBitmap(requestParams.RenderWidth, requestParams.RenderHeight);
                                BackBufferSkiaCanvas = BackBuffer.SKCanvas;
                            }, Avalonia.Threading.DispatcherPriority.MaxValue);
                        }

                        canvas = BackBufferSkiaCanvas;

                        canvas.Save();

                        canvas.Scale(requestParams.Scale);
                        canvas.Translate(-requestParams.Left, -requestParams.Top);

                        canvas.Clear(BackgroundColour);

                        canvas.ClipRect(new SKRect(requestParams.Left, requestParams.Top, requestParams.Left + requestParams.Width, requestParams.Top + requestParams.Height));

                        RenderImage(canvas);

                        canvas.RestoreToCount(-1);

                        BackBufferRenderingParams = requestParams;

                        lock (FrontBufferLock)
                        {
                            SkiaBitmap tempFrontReference = FrontBuffer;
                            RenderingParameters tempFrontRenderingParameters = FrontBufferRenderingParams;
                            SKCanvas tempFrontCanvas = FrontBufferSkiaCanvas;

                            FrontBuffer = BackBuffer;
                            FrontBufferRenderingParams = BackBufferRenderingParams;
                            FrontBufferSkiaCanvas = BackBufferSkiaCanvas;

                            BackBuffer = BackBuffer2;
                            BackBufferRenderingParams = BackBufferRenderingParams2;
                            BackBufferSkiaCanvas = BackBufferSkiaCanvas2;

                            BackBuffer2 = tempFrontReference;
                            BackBufferRenderingParams2 = tempFrontRenderingParameters;
                            BackBufferSkiaCanvas2 = tempFrontCanvas;
                        }

                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            base.InvalidateVisual();
                        }, Avalonia.Threading.DispatcherPriority.MaxValue);
                    }
                }
            });

            renderingThread.Start();
        }

        /// <summary>
        /// A lock for the rendering loop. The public methods of this class already lock on this, but you may need it if you want to directly manipulate the contents of the canvas.
        /// </summary>
        public object RenderLock = new object();

        /// <summary>
        /// Render the image to a bitmap at the specified resolution.
        /// </summary>
        /// <param name="width">The width of the rendered image. Note that the actual width of the returned image might be lower than this, depending on the aspect ratio of the image.</param>
        /// <param name="height">The height of the rendered image. Note that the actual height of the returned image might be lower than this, depending on the aspect ratio of the image.</param>
        /// <param name="background">The background colour for the image. If this is <see langword="null" />, the current background colour is used.</param>
        /// <returns>A <see cref="RenderTargetBitmap"/> containing the image rendered at the specified resolution.</returns>
        public RenderTargetBitmap RenderAtResolution(int width, int height, SKColor? background = null)
        {
            SKColor realBackground = background ?? this.BackgroundColour;

            double scale = Math.Min(width / this.PageWidth, height / this.PageHeight);

            width = (int)Math.Round(this.PageWidth * scale);
            height = (int)Math.Round(this.PageHeight * scale);

            SkiaBitmap skBmp = new SkiaBitmap(width, height);
            SKCanvas canvas = skBmp.SKCanvas;

            canvas.Clear(realBackground);

            canvas.Save();

            canvas.Scale((float)scale);

            RenderImage(canvas);

            canvas.RestoreToCount(-1);

            skBmp.Indispose();

            return skBmp.AvaloniaBitmap;
        }

        private void RenderImage(SKCanvas canvas)
        {
            lock (RenderLock)
            {
                for (int i = 0; i < this.RenderActions.Count; i++)
                {
                    canvas.Save();

                    if (this.LayerTransforms[i].ActionType == SKRenderAction.ActionTypes.Transform)
                    {
                        SKMatrix mat = this.LayerTransforms[i].Transform.Value;
                        canvas.Concat(ref mat);
                    }

                    RenderLayer(canvas, i);
                    canvas.Restore();
                }
            }
        }

        private void RenderLayer(SKCanvas canvas, int layer)
        {
            bool updateHitTests = this.TaggedRenderActions[this.TaggedRenderActions.Count - 1 - layer].Count > 0;

            HashSet<uint> ZIndices = new HashSet<uint>();

            canvas.Save();

            SKMatrix invertedInitialTransform;

            if (this.LayerTransforms[layer].ActionType == SKRenderAction.ActionTypes.Transform)
            {
                invertedInitialTransform = (canvas.TotalMatrix.PreConcat(this.LayerTransforms[layer].Transform.Value.Invert())).Invert();
            }
            else
            {
                invertedInitialTransform = canvas.TotalMatrix.Invert();
            }

            SKPath clipPath = null;
            Stack<SKPath> clipPaths = new Stack<SKPath>();
            clipPaths.Push(null);

            for (int i = 0; i < this.RenderActions[layer].Count; i++)
            {
                ZIndices.Add(this.RenderActions[layer][i].ZIndex);

                if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Clip)
                {
                    canvas.ClipPath(this.RenderActions[layer][i].Path, antialias: true);

                    if (updateHitTests)
                    {
                        if (clipPath == null)
                        {
                            clipPath = this.RenderActions[layer][i].Path.Clone();
                            clipPath.Transform(canvas.TotalMatrix.PostConcat(invertedInitialTransform));
                        }
                        else
                        {
                            using (SKPath tempPath = this.RenderActions[layer][i].Path.Clone())
                            {
                                tempPath.Transform(canvas.TotalMatrix.PostConcat(invertedInitialTransform));
                                clipPath.Op(tempPath, SKPathOp.Intersect);
                            }
                        }
                    }
                }
                else if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Restore)
                {
                    canvas.Restore();

                    if (updateHitTests)
                    {
                        clipPath = clipPaths.Pop();
                    }
                }
                else if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Save)
                {
                    canvas.Save();

                    if (updateHitTests)
                    {
                        clipPaths.Push(clipPath?.Clone());
                    }
                }
                else if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Transform)
                {
                    SKMatrix mat = this.RenderActions[layer][i].Transform.Value;
                    canvas.Concat(ref mat);
                }
                else
                {
                    if (this.RenderActions[layer][i].ZIndex == 0)
                    {

                        if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Path && this.RenderActions[layer][i].Path != null)
                        {
                            canvas.DrawPath(this.RenderActions[layer][i].Path, this.RenderActions[layer][i].Paint);
                        }
                        else if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Text)
                        {
                            canvas.DrawText(this.RenderActions[layer][i].Text, this.RenderActions[layer][i].TextX, this.RenderActions[layer][i].TextY, this.RenderActions[layer][i].Font, this.RenderActions[layer][i].Paint);
                        }
                        else if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.RasterImage)
                        {
                            (SKBitmap image, bool interpolate) = this.Images[this.RenderActions[layer][i].ImageId];

                            SKPaint paint;

                            if (!interpolate)
                            {
                                paint = null;
                            }
                            else
                            {
                                paint = new SKPaint() { FilterQuality = SKFilterQuality.Medium };
                            }

                            canvas.DrawBitmap(image, this.RenderActions[layer][i].ImageSource.Value, this.RenderActions[layer][i].ImageDestination.Value, paint);

                            paint?.Dispose();
                        }
                    }

                    if (updateHitTests && this.RenderActions[layer][i].Tag != null && this.RenderActions[layer][i].HitTestPath != null)
                    {
                        SKPath hitTestPath = this.RenderActions[layer][i].HitTestPath.Clone();
                        hitTestPath.Transform(canvas.TotalMatrix.PostConcat(invertedInitialTransform));

                        if (clipPath != null)
                        {
                            hitTestPath.Op(clipPath, SKPathOp.Intersect);
                        }

                        this.RenderActions[layer][i].LastRenderedGlobalHitTestPath = hitTestPath;
                    }
                }
            }

            canvas.Restore();

            uint[] sortedIndices = ZIndices.OrderBy(x => x).ToArray();

            if (sortedIndices.Length > 1 || sortedIndices[0] != 0)
            {
                for (int j = 0; j < sortedIndices.Length; j++)
                {
                    canvas.Save();

                    for (int i = 0; i < this.RenderActions[layer].Count; i++)
                    {
                        if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Clip)
                        {
                            canvas.ClipPath(this.RenderActions[layer][i].Path, antialias: true);
                        }
                        else if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Restore)
                        {
                            canvas.Restore();
                        }
                        else if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Save)
                        {
                            canvas.Save();
                        }
                        else if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Transform)
                        {
                            SKMatrix mat = this.RenderActions[layer][i].Transform.Value;
                            canvas.Concat(ref mat);
                        }
                        else if (sortedIndices[j] != 0 && this.RenderActions[layer][i].ZIndex == sortedIndices[j])
                        {
                            if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Path && this.RenderActions[layer][i].Path != null)
                            {
                                canvas.DrawPath(this.RenderActions[layer][i].Path, this.RenderActions[layer][i].Paint);
                            }
                            else if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.Text)
                            {
                                canvas.DrawText(this.RenderActions[layer][i].Text, this.RenderActions[layer][i].TextX, this.RenderActions[layer][i].TextY, this.RenderActions[layer][i].Font, this.RenderActions[layer][i].Paint);
                            }
                            else if (this.RenderActions[layer][i].ActionType == SKRenderAction.ActionTypes.RasterImage)
                            {
                                (SKBitmap image, bool interpolate) = this.Images[this.RenderActions[layer][i].ImageId];

                                SKPaint paint;

                                if (!interpolate)
                                {
                                    paint = null;
                                }
                                else
                                {
                                    paint = new SKPaint() { FilterQuality = SKFilterQuality.Medium };
                                }

                                canvas.DrawBitmap(image, this.RenderActions[layer][i].ImageSource.Value, this.RenderActions[layer][i].ImageDestination.Value, paint);

                                paint?.Dispose();
                            }
                        }
                    }

                    canvas.Restore();
                }
            }
        }

        private bool IsDirty = false;

        /// <summary>
        /// Invalidate the contents of the canvas, forcing it to redraw itself.
        /// </summary>
        public void InvalidateDirty()
        {
            this.IsDirty = true;
            base.InvalidateVisual();
        }

        /// <summary>
        /// Invalidate the contents of the canvas, specifying that the order of the layers has changed.
        /// </summary>
        public void InvalidateZIndex()
        {
            for (int i = 0; i < this.TaggedRenderActions.Count; i++)
            {
                this.TaggedRenderActions[i] = this.TaggedRenderActions[i].OrderByDescending(a => a.ZIndex).ToList();
            }
            this.InvalidateDirty();
        }


        private SkiaBitmap FrontBuffer = null;
        private RenderingParameters FrontBufferRenderingParams = null;
        private readonly object EmptyFrontBufferLock = new object();
        private object FrontBufferLock => FrontBuffer?.Lock ?? EmptyFrontBufferLock;
        SKCanvas FrontBufferSkiaCanvas = null;

        private SkiaBitmap BackBuffer = null;
        private RenderingParameters BackBufferRenderingParams = null;
        SKCanvas BackBufferSkiaCanvas = null;

        private SkiaBitmap BackBuffer2 = null;
        private RenderingParameters BackBufferRenderingParams2 = null;
        SKCanvas BackBufferSkiaCanvas2 = null;

        /// <inheritdoc/>
        public override void Render(DrawingContext context)
        {
            lock (FrontBufferLock)
            {
                double scale;

                PixelPoint layoutTopLeft = this.PointToScreen(new Avalonia.Point(0, 0));
                PixelPoint layoutBottomRight = this.PointToScreen(new Avalonia.Point(this.Bounds.Width, this.Bounds.Height));

                scale = 0.5 * (this.PageWidth / (layoutBottomRight.X - layoutTopLeft.X) + this.PageHeight / (layoutBottomRight.Y - layoutTopLeft.Y));

                Avalonia.Controls.Control parent = this.Parent as Avalonia.Controls.Control;

                double left;
                double top;
                double width;
                double height;

                if (this.ClipToBounds && parent != null)
                {
                    Avalonia.Point clipTopLeft = this.PointToClient(parent.PointToScreen(new Avalonia.Point(0, 0)));
                    Avalonia.Point clipBottomRight = this.PointToClient(parent.PointToScreen(new Avalonia.Point(parent.Bounds.Width, parent.Bounds.Height)));

                    Rect clipBounds;

                    if (ClipMargin.Unit == RelativeUnit.Absolute)
                    {
                        clipBounds = new Rect(clipTopLeft.X - ClipMargin.Point.X, clipTopLeft.Y - ClipMargin.Point.Y, clipBottomRight.X - clipTopLeft.X + ClipMargin.Point.X * 2, clipBottomRight.Y - clipTopLeft.Y + ClipMargin.Point.Y * 2);
                    }
                    else
                    {
                        double clipW = clipBottomRight.X - clipTopLeft.X;
                        double clipH = clipBottomRight.Y - clipTopLeft.Y;

                        clipBounds = new Rect(clipTopLeft.X - clipW * ClipMargin.Point.X, clipTopLeft.Y - clipH * ClipMargin.Point.Y, clipW * (1 + 2 * ClipMargin.Point.X), clipH * (1 + 2 * ClipMargin.Point.Y));
                    }

                    left = Math.Max(0, clipBounds.Left);
                    top = Math.Max(0, clipBounds.Top);

                    width = Math.Min(clipBounds.Width, this.PageWidth);
                    height = Math.Min(clipBounds.Height, this.PageHeight);
                }
                else
                {
                    left = 0;
                    top = 0;

                    width = this.PageWidth;
                    height = this.PageHeight;
                }

                int pixelWidth = (int)Math.Round(width / scale);
                int pixelHeight = (int)Math.Round(height / scale);

                if (pixelWidth > 0 && pixelHeight > 0)
                {
                    if (!this.ClipToBounds && (pixelWidth * pixelHeight >= int.MaxValue / 8 || pixelWidth * pixelHeight < 0))
                    {
                        double ratio = width / height;

                        pixelHeight = (int)Math.Round(Math.Sqrt(int.MaxValue / (8 * ratio)));
                        pixelWidth = (int)Math.Round(pixelHeight * ratio);
                    }

                    PixelPoint topLeftScreen = this.PointToScreen(new Avalonia.Point(left, top));
                    Avalonia.Point topLeft = this.PointToClient(topLeftScreen);
                    Avalonia.Point bottomRight = this.PointToClient(new PixelPoint(topLeftScreen.X + pixelWidth, topLeftScreen.Y + pixelHeight));

                    Rect targetRect = new Rect(topLeft, bottomRight);
                    
                    pixelWidth = this.PointToScreen(targetRect.BottomRight).X - this.PointToScreen(targetRect.TopLeft).X;
                    pixelHeight = this.PointToScreen(targetRect.BottomRight).Y - this.PointToScreen(targetRect.TopLeft).Y;

                    RenderingParameters currentParameters = new RenderingParameters((float)left, (float)top, (float)width, (float)height, (float)(1 / scale), pixelWidth, pixelHeight);

                    if (FrontBuffer != null && FrontBufferRenderingParams == currentParameters && !IsDirty)
                    {
                        if (RenderRequestedHandle.WaitOne(0))
                        {
                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                this.InvalidateVisual();
                            });
                        }

                        context.DrawImage(FrontBuffer, new Rect(0, 0, pixelWidth, pixelHeight), targetRect);
                    }
                    else if (FrontBuffer != null && FrontBufferRenderingParams.GoodEnough(currentParameters) && !IsDirty)
                    {
                        if (RenderRequestedHandle.WaitOne(0))
                        {
                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                this.InvalidateVisual();
                            });
                        }
                        context.DrawImage(FrontBuffer, new Rect(0, 0, FrontBufferRenderingParams.RenderWidth, FrontBufferRenderingParams.RenderHeight), new Rect(FrontBufferRenderingParams.Left, FrontBufferRenderingParams.Top, FrontBufferRenderingParams.Width, FrontBufferRenderingParams.Height));
                    }
                    else
                    {
                        lock (RenderingRequestLock)
                        {
                            IsDirty = false;
                            RenderingRequest = currentParameters;
                            RenderRequestedHandle.Set();
                        }

                        if (FrontBuffer != null)
                        {
                            context.DrawImage(FrontBuffer, new Rect(0, 0, FrontBufferRenderingParams.RenderWidth, FrontBufferRenderingParams.RenderHeight), new Rect(FrontBufferRenderingParams.Left, FrontBufferRenderingParams.Top, FrontBufferRenderingParams.Width, FrontBufferRenderingParams.Height));
                        }

                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            this.InvalidateVisual();
                        });
                    }
                }
            }
        }

        private bool disposedValue;

        /// <inheritdoc cref="Dispose()"/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.BackBuffer?.Dispose();
                    this.BackBuffer2?.Dispose();
                    this.DisposedHandle?.Set();
                    this.FrontBuffer?.Dispose();

                    foreach (KeyValuePair<string, (SKBitmap, bool)> image in this.Images)
                    {
                        image.Value.Item1?.Dispose();
                    }

                    for (int i = 0; i < this.RenderActions?.Count; i++)
                    {
                        foreach (SKRenderAction act in this.RenderActions[i])
                        {
                            act?.Dispose();
                        }

                        this.LayerTransforms?[i]?.Dispose();
                    }
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
}
