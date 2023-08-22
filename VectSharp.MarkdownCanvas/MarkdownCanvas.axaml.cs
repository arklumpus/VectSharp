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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Markdig;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using VectSharp.Canvas;
using VectSharp.Markdown;

namespace VectSharp.MarkdownCanvas
{
    /// <summary>
    /// A control to display a Markdown document in an Avalonia application.
    /// </summary>
    public partial class MarkdownCanvasControl : UserControl
    {
        /// <summary>
        /// Defines the <see cref="MaxRenderWidth"/> property.
        /// </summary>
        public static readonly StyledProperty<double> MaxRenderWidthProperty = AvaloniaProperty.Register<MarkdownCanvasControl, double>(nameof(MaxRenderWidth), double.PositiveInfinity);

        /// <summary>
        /// The maximum width for the rendered document. This will be used even if the control's client area is larger than this (the alignment of the document within the controll will depend on the control's <see cref="ContentControl.HorizontalContentAlignment"/>).
        /// </summary>
        public double MaxRenderWidth
        {
            get { return GetValue(MaxRenderWidthProperty); }
            set { SetValue(MaxRenderWidthProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="MinRenderWidth"/> property.
        /// </summary>
        public static readonly StyledProperty<double> MinRenderWidthProperty = AvaloniaProperty.Register<MarkdownCanvasControl, double>(nameof(MinRenderWidth), 200);

        /// <summary>
        /// The minimum width for the rendered document. If the control's client area is smaller than this, the horizontal scroll bar will be activated.
        /// </summary>
        public double MinRenderWidth
        {
            get { return GetValue(MinRenderWidthProperty); }
            set { SetValue(MinRenderWidthProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="MinVariation"/> property.
        /// </summary>
        public static readonly StyledProperty<double> MinVariationProperty = AvaloniaProperty.Register<MarkdownCanvasControl, double>(nameof(MinVariation), 10);
        
        /// <summary>
        /// The minimum width variation that triggers a document reflow. If the control is resized, but the width changes by less than this amount, the document is not re-drawn.
        /// </summary>
        public double MinVariation
        {
            get { return GetValue(MinVariationProperty); }
            set { SetValue(MinVariationProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="DocumentSource"/> property.
        /// </summary>
        public static readonly StyledProperty<string> DocumentSourceProperty = AvaloniaProperty.Register<MarkdownCanvasControl, string>(nameof(DocumentSource));
        
        /// <summary>
        /// Sets the currently displayed document from Markdown source.
        /// </summary>
        public string DocumentSource
        {
            set { SetValue(DocumentSourceProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="Document"/> property.
        /// </summary>
        public static readonly StyledProperty<MarkdownDocument> DocumentProperty = AvaloniaProperty.Register<MarkdownCanvasControl, MarkdownDocument>(nameof(Document));
        
        /// <summary>
        /// Gets or sets the currently displayed <see cref="MarkdownDocument"/>.
        /// </summary>
        public MarkdownDocument Document
        {
            get { return GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="TextConversionOption"/> property.
        /// </summary>
        public static readonly StyledProperty<AvaloniaContextInterpreter.TextOptions> TextConversionOptionsProperty = AvaloniaProperty.Register<MarkdownCanvasControl, AvaloniaContextInterpreter.TextOptions>(nameof(TextConversionOption), AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary);

        /// <summary>
        /// Gets or sets the value that determines whether text items should be converted into paths when drawing.
        /// Setting this to <see cref="AvaloniaContextInterpreter.TextOptions.NeverConvert"/> will improve performance if you are using custom fonts, but may cause unexpected results unless the font families being used are of type <see cref="ResourceFontFamily"/>.
        /// </summary>
        public AvaloniaContextInterpreter.TextOptions TextConversionOption
        {
            get { return GetValue(TextConversionOptionsProperty); }
            set { SetValue(TextConversionOptionsProperty, value); }
        }

        /// <summary>
        /// The <see cref="MarkdownRenderer"/> used to render the <see cref="Document"/>. You can use the properties of this object to customise the rendering. Note that setting the <see cref="Avalonia.Controls.Primitives.TemplatedControl.FontSize"/> of the <see cref="MarkdownCanvasControl"/> will propagate to the <see cref="Renderer"/>'s <see cref="MarkdownRenderer.BaseFontSize"/>.
        /// </summary>
        public MarkdownRenderer Renderer { get; }

        private double lastRenderedWidth = double.NaN;

        private bool initialized = false;

        /// <summary>
        /// Initialises a new <see cref="MarkdownCanvasControl"/>.
        /// </summary>
        public MarkdownCanvasControl()
        {
            InitializeComponent();
            this.Renderer = new MarkdownRenderer() { BaseFontSize = this.FontSize, Margins = new Margins(10, 10, 10, 10), ImageUriResolver = ImageCache.ImageUriResolver };
            this.initialized = true;
            ImageCache.SetExitEventHandler();
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (this.initialized)
            {
                if (change.Property == MarkdownCanvasControl.BoundsProperty || change.Property == MarkdownCanvasControl.MinRenderWidthProperty || change.Property == MarkdownCanvasControl.MaxRenderWidthProperty || change.Property == MarkdownCanvasControl.MinVariationProperty)
                {
                    Render();
                }
                else if (change.Property == MarkdownCanvasControl.DocumentProperty)
                {
                    forcedRerender = true;
                    Render();
                }
                else if (change.Property == MarkdownCanvasControl.FontSizeProperty)
                {
                    this.Renderer.BaseFontSize = this.FontSize;
                    forcedRerender = true;
                    Render();
                }
                else if (change.Property == MarkdownCanvasControl.DocumentSourceProperty)
                {
                    MarkdownDocument document = Markdig.Markdown.Parse(change.GetNewValue<string>(), new MarkdownPipelineBuilder().UseGridTables().UsePipeTables().UseEmphasisExtras().UseGenericAttributes().UseAutoIdentifiers().UseAutoLinks().UseTaskLists().UseListExtras().UseCitations().UseMathematics().UseSmartyPants().Build());
                    this.Document = document;
                }
                else if (change.Property == MarkdownCanvasControl.VerticalContentAlignmentProperty)
                {
                    this.FindControl<ScrollViewer>("ScrollViewer").VerticalContentAlignment = this.VerticalContentAlignment;
                }
                else if (change.Property == MarkdownCanvasControl.HorizontalContentAlignmentProperty)
                {
                    this.FindControl<ScrollViewer>("ScrollViewer").HorizontalContentAlignment = this.HorizontalContentAlignment;
                }
            }
        }

        private bool forcedRerender = false;

        private void Render()
        {
            if (Document != null)
            {
                double width = Math.Min(MaxRenderWidth, Math.Max(MinRenderWidth, this.Bounds.Width - MinVariation - 13));

                if (forcedRerender || double.IsNaN(lastRenderedWidth) || width != lastRenderedWidth && width < lastRenderedWidth - MinVariation || width > lastRenderedWidth + MinVariation)
                {
                    Page pag;
                    Dictionary<string, string> linkDestinations;

                    try
                    {
                        pag = Renderer.RenderSinglePage(this.Document, width, out linkDestinations);
                    }
                    catch
                    {
                        pag = new Page(width, 0);
                        linkDestinations = new Dictionary<string, string>();
                    }

                    Dictionary<string, Delegate> taggedActions = new Dictionary<string, Delegate>();
                    Dictionary<string, Avalonia.Point> linkDestinationPoints = new Dictionary<string, Avalonia.Point>();

                    foreach (KeyValuePair<string, string> linkDestination in linkDestinations)
                    {
                        string url = linkDestination.Value;

                        taggedActions.Add(linkDestination.Key, (Func<RenderAction, IEnumerable<RenderAction>>)(act =>
                        {
                            act.PointerEntered += (s, e) =>
                            {
                                act.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                            };

                            act.PointerExited += (s, e) =>
                            {
                                act.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
                            };

                            act.PointerPressed += (s, e) =>
                            {
                                if (url.StartsWith("#"))
                                {
                                    if (linkDestinationPoints.TryGetValue(url.Substring(1), out Avalonia.Point target))
                                    {
                                        ScrollViewer scrollViewer = this.FindControl<ScrollViewer>("ScrollViewer");

                                        scrollViewer.Offset = new Vector(Math.Max(Math.Min(scrollViewer.Offset.X, target.X), target.X - scrollViewer.Viewport.Width), target.Y);
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = url, UseShellExecute = true });
                                }
                            };

                            if (act.ActionType == RenderAction.ActionTypes.Path)
                            {
                                linkDestinationPoints[linkDestination.Key] = act.Geometry.Bounds.TopLeft.Transform(act.Transform);
                            }
                            else if (act.ActionType == RenderAction.ActionTypes.Text)
                            {
                                linkDestinationPoints[linkDestination.Key] = new Avalonia.Point(0, 0).Transform(act.Transform);
                            }
                            else if (act.ActionType == RenderAction.ActionTypes.RasterImage)
                            {
                                linkDestinationPoints[linkDestination.Key] = act.ImageDestination.Value.TopLeft.Transform(act.Transform);
                            }

                            return new RenderAction[] { act };
                        }));

                        if (url.StartsWith("#"))
                        {
                            if (!taggedActions.ContainsKey(url.Substring(1)))
                            {
                                taggedActions.Add(url.Substring(1), (Func<RenderAction, IEnumerable<RenderAction>>)(act =>
                                {
                                    if (act.ActionType == RenderAction.ActionTypes.Path)
                                    {
                                        linkDestinationPoints[url.Substring(1)] = act.Geometry.Bounds.TopLeft.Transform(act.Transform);
                                    }
                                    else if (act.ActionType == RenderAction.ActionTypes.Text)
                                    {
                                        linkDestinationPoints[url.Substring(1)] = new Avalonia.Point(0, 0).Transform(act.Transform);
                                    }
                                    else if (act.ActionType == RenderAction.ActionTypes.RasterImage)
                                    {
                                        linkDestinationPoints[url.Substring(1)] =  act.ImageDestination.Value.TopLeft.Transform(act.Transform);
                                    }

                                    return new RenderAction[] { act };
                                }));
                            }
                        }
                    }

                    Control can = pag.PaintToCanvas(false, taggedActions, false, this.TextConversionOption);

                    this.FindControl<ScrollViewer>("ScrollViewer").Content = can;
                    this.FindControl<ScrollViewer>("ScrollViewer").Padding = new Thickness(0, 0, 0, 0);
                    lastRenderedWidth = width;
                    forcedRerender = false;
                }
                else
                {
                    this.FindControl<ScrollViewer>("ScrollViewer").Padding = new Thickness(0, 0, width - lastRenderedWidth, 0);
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
