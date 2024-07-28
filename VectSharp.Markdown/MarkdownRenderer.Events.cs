/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2024 Giorgio Bianchini

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

using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;

namespace VectSharp.Markdown
{
    public partial class MarkdownRenderer
    {
        /// <summary>
        /// Invoked when a new page is started.
        /// </summary>
        public event EventHandler<PageStartedEventArgs> PageStarted;

        /// <summary>
        /// Invoked when a page is finished, before starting a new page (or at the end of the document).
        /// </summary>
        public event EventHandler<PageFinishedEventArgs> PageFinished;

        /// <summary>
        /// Invoked just before rendering a <see cref="Block"/>.
        /// </summary>
        public event EventHandler<BlockRenderingEventArgs> BlockRendering;

        /// <summary>
        /// Invoked just after rendering a <see cref="Block"/>.
        /// </summary>
        public event EventHandler<BlockRenderedEventArgs> BlockRendered;

        /// <summary>
        /// Invoked just before rendering an <see cref="Inline"/>.
        /// </summary>
        public event EventHandler<InlineRenderingEventArgs> InlineRendering;

        /// <summary>
        /// Invoked just after rendering an <see cref="Inline"/>.
        /// </summary>
        public event EventHandler<InlineRenderedEventArgs> InlineRendered;

        /// <summary>
        /// Invoked after content in a <see cref="Line"/> has been measured, but before deciding whether to render the line on the current page or on a new page.
        /// </summary>
        public event EventHandler<LineMeasuredEventArgs> LineMeasured;

        /// <summary>
        /// Invoked just before rendering a <see cref="Line"/>, after a new page has been created if necessary.
        /// </summary>
        public event EventHandler<LineEventArgs> LineRendering;

        /// <summary>
        /// Invoked just after rendering a <see cref="Line"/>.
        /// </summary>
        public event EventHandler<LineEventArgs> LineRendered;

        /// <summary>
        /// Raises the <see cref="PageStarted"/> event.
        /// </summary>
        /// <param name="context">The current <see cref="MarkdownContext"/>.</param>
        /// <param name="pageGraphics">The current <see cref="Graphics"/> surface.</param>
        /// <param name="pag">The new empty <see cref="Page"/>.</param>
        protected virtual void OnPageStarted(ref MarkdownContext context, ref Graphics pageGraphics, Page pag)
        {
            PageStartedEventArgs eventArgs = new PageStartedEventArgs(context, pag, pageGraphics);
            PageStarted?.Invoke(this, eventArgs);
            context = eventArgs.MarkdownContext;
            pageGraphics = eventArgs.Graphics;
        }

        /// <summary>
        /// Raises the <see cref="PageFinished"/> event.
        /// </summary>
        /// <param name="context">The current <see cref="MarkdownContext"/>.</param>
        /// <param name="pageGraphics">The current <see cref="Graphics"/> surface.</param>
        /// <param name="pag">The new empty <see cref="Page"/>.</param>
        protected virtual void OnPageFinished(ref MarkdownContext context, ref Graphics pageGraphics, Page pag)
        {
            PageFinishedEventArgs eventArgs = new PageFinishedEventArgs(context, pag, pageGraphics);
            PageFinished?.Invoke(this, eventArgs);
            context = eventArgs.MarkdownContext;
            pageGraphics = eventArgs.Graphics;
        }

        /// <summary>
        /// Raises the <see cref="BlockRendering"/> event.
        /// </summary>
        /// <param name="context">The current <see cref="MarkdownContext"/>.</param>
        /// <param name="graphics">The current <see cref="Graphics"/> surface.</param>
        /// <param name="block">The <see cref="Block"/> that will be rendered.</param>
        protected virtual void OnBlockRendering(ref MarkdownContext context, ref Graphics graphics, ref Block block)
        {
            BlockRenderingEventArgs eventArgs = new BlockRenderingEventArgs(context, graphics, block);
            BlockRendering?.Invoke(this, eventArgs);
            context = eventArgs.MarkdownContext;
            graphics = eventArgs.Graphics;
            block = eventArgs.Block;
        }

        /// <summary>
        /// Raises the <see cref="BlockRendered"/> event.
        /// </summary>
        /// <param name="context">The current <see cref="MarkdownContext"/>.</param>
        /// <param name="graphics">The current <see cref="Graphics"/> surface.</param>
        /// <param name="block">The <see cref="Block"/> that has been rendered.</param>
        protected virtual void OnBlockRendered(ref MarkdownContext context, ref Graphics graphics, Block block)
        {
            BlockRenderedEventArgs eventArgs = new BlockRenderedEventArgs(context, graphics, block);
            BlockRendered?.Invoke(this, eventArgs);
            context = eventArgs.MarkdownContext;
            graphics = eventArgs.Graphics;
        }

        /// <summary>
        /// Raises the <see cref="InlineRendering"/> event.
        /// </summary>
        /// <param name="context">The current <see cref="MarkdownContext"/>.</param>
        /// <param name="graphics">The current <see cref="Graphics"/> surface.</param>
        /// <param name="inline">The <see cref="Inline"/> that will be rendered.</param>
        protected virtual void OnInlineRendering(ref MarkdownContext context, ref Graphics graphics, ref Inline inline)
        {
            InlineRenderingEventArgs eventArgs = new InlineRenderingEventArgs(context, graphics, inline);
            InlineRendering?.Invoke(this, eventArgs);
            context = eventArgs.MarkdownContext;
            graphics = eventArgs.Graphics;
            inline = eventArgs.Inline;
        }

        /// <summary>
        /// Raises the <see cref="InlineRendered"/> event.
        /// </summary>
        /// <param name="context">The current <see cref="MarkdownContext"/>.</param>
        /// <param name="graphics">The current <see cref="Graphics"/> surface.</param>
        /// <param name="inline">The <see cref="Inline"/> that has been rendered.</param>
        protected virtual void OnInlineRendered(ref MarkdownContext context, ref Graphics graphics, Inline inline)
        {
            InlineRenderedEventArgs eventArgs = new InlineRenderedEventArgs(context, graphics, inline);
            InlineRendered?.Invoke(this, eventArgs);
            context = eventArgs.MarkdownContext;
            graphics = eventArgs.Graphics;
        }

        /// <summary>
        /// Raises the <see cref="LineMeasured"/> event.
        /// </summary>
        /// <param name="context">The current <see cref="VectSharp.Markdown.MarkdownContext"/>.</param>
        /// <param name="graphics">The current <see cref="VectSharp.Graphics"/> surface.</param>
        /// <param name="line">The line that will be rendered.</param>
        /// <param name="pageMaxX">The maximum space available for the line on the x axis.</param>
        /// <param name="pageMaxY">The maximum space available for the line on the y axis.</param>
        /// <param name="contentMaxX">The maximum x coordinate of the content in the line.</param>
        /// <param name="contentMinY">The minimum y coordinate of the content in the line.</param>
        /// <param name="contentMaxY">The maximum y coordinate of the content in the line.</param>
        /// <param name="baseline">The vertical coordinate of the baseline.</param>
        protected virtual void OnLineMeasured(ref MarkdownContext context, ref Graphics graphics, ref Line line, double pageMaxX, double pageMaxY, double contentMaxX, double contentMinY, double contentMaxY, double baseline)
        {
            LineMeasuredEventArgs eventArgs = new LineMeasuredEventArgs(context, graphics, line, pageMaxX, pageMaxY, contentMaxX, contentMinY, contentMaxY, baseline);
            LineMeasured?.Invoke(this, eventArgs);
            context = eventArgs.MarkdownContext;
            graphics = eventArgs.Graphics;
            line = eventArgs.Line;
        }

        /// <summary>
        /// Raises the <see cref="LineRendering"/> event.
        /// </summary>
        /// <param name="context">The current <see cref="VectSharp.Markdown.MarkdownContext"/>.</param>
        /// <param name="graphics">The current <see cref="VectSharp.Graphics"/> surface.</param>
        /// <param name="line">The line that will be rendered.</param>
        /// <param name="pageMaxX">The maximum space available for the line on the x axis.</param>
        /// <param name="pageMaxY">The maximum space available for the line on the y axis.</param>
        /// <param name="contentMaxX">The maximum x coordinate of the content in the line.</param>
        /// <param name="contentMinY">The minimum y coordinate of the content in the line.</param>
        /// <param name="contentMaxY">The maximum y coordinate of the content in the line.</param>
        /// <param name="baseline">The vertical coordinate of the baseline.</param>
        protected virtual void OnLineRendering(ref MarkdownContext context, ref Graphics graphics, Line line, double pageMaxX, double pageMaxY, double contentMaxX, double contentMinY, double contentMaxY, double baseline)
        {
            LineEventArgs eventArgs = new LineEventArgs(context, graphics, line, pageMaxX, pageMaxY, contentMaxX, contentMinY, contentMaxY, baseline);
            LineRendering?.Invoke(this, eventArgs);
            context = eventArgs.MarkdownContext;
            graphics = eventArgs.Graphics;
        }

        /// <summary>
        /// Raises the <see cref="LineRendered"/> event.
        /// </summary>
        /// <param name="context">The current <see cref="VectSharp.Markdown.MarkdownContext"/>.</param>
        /// <param name="graphics">The current <see cref="VectSharp.Graphics"/> surface.</param>
        /// <param name="line">The line that will be rendered.</param>
        /// <param name="pageMaxX">The maximum space available for the line on the x axis.</param>
        /// <param name="pageMaxY">The maximum space available for the line on the y axis.</param>
        /// <param name="contentMaxX">The maximum x coordinate of the content in the line.</param>
        /// <param name="contentMinY">The minimum y coordinate of the content in the line.</param>
        /// <param name="contentMaxY">The maximum y coordinate of the content in the line.</param>
        /// <param name="baseline">The vertical coordinate of the baseline.</param>
        protected virtual void OnLineRendered(ref MarkdownContext context, ref Graphics graphics, Line line, double pageMaxX, double pageMaxY, double contentMaxX, double contentMinY, double contentMaxY, double baseline)
        {
            LineEventArgs eventArgs = new LineEventArgs(context, graphics, line, pageMaxX, pageMaxY, contentMaxX, contentMinY, contentMaxY, baseline);
            LineRendered?.Invoke(this, eventArgs);
            context = eventArgs.MarkdownContext;
            graphics = eventArgs.Graphics;
        }

        internal void RaiseLineMeasured(ref MarkdownContext context, ref Graphics graphics, ref Line line, double pageMaxX, double pageMaxY, double contentMaxX, double contentMinY, double contentMaxY, double baseline)
        {
            OnLineMeasured(ref context, ref graphics, ref line, pageMaxX, pageMaxY, contentMaxX, contentMinY, contentMaxY, baseline);
        }

        internal void RaiseLineRendering(ref MarkdownContext context, ref Graphics graphics, Line line, double pageMaxX, double pageMaxY, double contentMaxX, double contentMinY, double contentMaxY, double baseline)
        {
            OnLineRendering(ref context, ref graphics, line, pageMaxX, pageMaxY, contentMaxX, contentMinY, contentMaxY, baseline);
        }

        internal void RaiseLineRendered(ref MarkdownContext context, ref Graphics graphics, Line line, double pageMaxX, double pageMaxY, double contentMaxX, double contentMinY, double contentMaxY, double baseline)
        {
            OnLineRendered(ref context, ref graphics, line, pageMaxX, pageMaxY, contentMaxX, contentMinY, contentMaxY, baseline);
        }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for the <see cref="MarkdownRenderer.PageStarted"/> event.
    /// </summary>
    public class PageStartedEventArgs : EventArgs
    {
        /// <summary>
        /// The current <see cref="VectSharp.Markdown.MarkdownContext"/>.
        /// </summary>
        public MarkdownContext MarkdownContext { get; set; }

        /// <summary>
        /// The new empty <see cref="VectSharp.Page"/>.
        /// </summary>
        public Page Page { get; }

        /// <summary>
        /// The <see cref="VectSharp.Graphics"/> surface on which the page contents will be drawn.
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <summary>
        /// Create a new <see cref="PageStartedEventArgs"/> instance.
        /// </summary>
        /// <param name="markdownContext">The current <see cref="VectSharp.Markdown.MarkdownContext"/>.</param>
        /// <param name="page">The new empty <see cref="VectSharp.Page"/>.</param>
        /// <param name="graphics">The <see cref="VectSharp.Graphics"/> surface on which the page contents will be drawn.</param>
        public PageStartedEventArgs(MarkdownContext markdownContext, Page page, Graphics graphics)
        {
            this.MarkdownContext = markdownContext;
            this.Page = page;
            this.Graphics = graphics;
        }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for the <see cref="MarkdownRenderer.PageFinished"/> event.
    /// </summary>
    public class PageFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// The current <see cref="VectSharp.Markdown.MarkdownContext"/>.
        /// </summary>
        public MarkdownContext MarkdownContext { get; set; }

        /// <summary>
        /// The finished <see cref="VectSharp.Page"/>.
        /// </summary>
        public Page Page { get; }

        /// <summary>
        /// The <see cref="VectSharp.Graphics"/> surface on which the page contents have been drawn.
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <summary>
        /// Create a new <see cref="PageFinishedEventArgs"/> instance.
        /// </summary>
        /// <param name="markdownContext">The current <see cref="VectSharp.Markdown.MarkdownContext"/>.</param>
        /// <param name="page">The finished <see cref="VectSharp.Page"/>.</param>
        /// <param name="graphics">The <see cref="VectSharp.Graphics"/> surface on which the page contents have been drawn.</param>
        public PageFinishedEventArgs(MarkdownContext markdownContext, Page page, Graphics graphics)
        {
            this.MarkdownContext = markdownContext;
            this.Page = page;
            this.Graphics = graphics;
        }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for the <see cref="MarkdownRenderer.BlockRendering"/> event.
    /// </summary>
    public class BlockRenderingEventArgs : EventArgs
    {
        /// <summary>
        /// The current <see cref="VectSharp.Markdown.MarkdownContext"/>.
        /// </summary>
        public MarkdownContext MarkdownContext { get; set; }

        /// <summary>
        /// The <see cref="VectSharp.Graphics"/> surface on which the <see cref="Block"/> will be drawn.
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <summary>
        /// The <see cref="Markdig.Syntax.Block"/> that will be rendered.
        /// </summary>
        public Block Block { get; set; }

        /// <summary>
        /// Create a new <see cref="BlockRenderingEventArgs"/> instance.
        /// </summary>
        /// <param name="markdownContext">The current <see cref="VectSharp.Markdown.MarkdownContext"/>.</param>
        /// <param name="graphics">The <see cref="VectSharp.Graphics"/> surface on which the <see cref="Block"/> will be drawn.</param>
        /// <param name="block">The <see cref="Markdig.Syntax.Block"/> that will be rendered.</param>
        public BlockRenderingEventArgs(MarkdownContext markdownContext, Graphics graphics, Block block)
        {
            MarkdownContext = markdownContext;
            Graphics = graphics;
            Block = block;
        }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for the <see cref="MarkdownRenderer.BlockRendered"/> event.
    /// </summary>
    public class BlockRenderedEventArgs : EventArgs
    {
        /// <summary>
        /// The current <see cref="VectSharp.Markdown.MarkdownContext"/>.
        /// </summary>
        public MarkdownContext MarkdownContext { get; set; }

        /// <summary>
        /// The current <see cref="VectSharp.Graphics"/> surface.
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <summary>
        /// The <see cref="Markdig.Syntax.Block"/> that has been rendered.
        /// </summary>
        public Block Block { get; }

        /// <summary>
        /// Create a new <see cref="BlockRenderingEventArgs"/> instance.
        /// </summary>
        /// <param name="markdownContext">The current <see cref="VectSharp.Markdown.MarkdownContext"/>.</param>
        /// <param name="graphics">The current <see cref="VectSharp.Graphics"/> surface.</param>
        /// <param name="block">The <see cref="Markdig.Syntax.Block"/> that has been rendered.</param>
        public BlockRenderedEventArgs(MarkdownContext markdownContext, Graphics graphics, Block block)
        {
            MarkdownContext = markdownContext;
            Graphics = graphics;
            Block = block;
        }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for the <see cref="MarkdownRenderer.InlineRendering"/> event.
    /// </summary>
    public class InlineRenderingEventArgs : EventArgs
    {
        /// <summary>
        /// The current <see cref="VectSharp.Markdown.MarkdownContext"/>.
        /// </summary>
        public MarkdownContext MarkdownContext { get; set; }

        /// <summary>
        /// The <see cref="VectSharp.Graphics"/> surface on which the <see cref="Inline"/> will be drawn.
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <summary>
        /// The <see cref="Markdig.Syntax.Inlines.Inline"/> that will be rendered.
        /// </summary>
        public Inline Inline { get; set; }

        /// <summary>
        /// Create a new <see cref="InlineRenderingEventArgs"/> instance.
        /// </summary>
        /// <param name="markdownContext">The current <see cref="VectSharp.Markdown.MarkdownContext"/>.</param>
        /// <param name="graphics">The <see cref="VectSharp.Graphics"/> surface on which the <see cref="Inline"/> will be drawn.</param>
        /// <param name="inline">The <see cref="Markdig.Syntax.Inlines.Inline"/> that will be rendered.</param>
        public InlineRenderingEventArgs(MarkdownContext markdownContext, Graphics graphics, Inline inline)
        {
            MarkdownContext = markdownContext;
            Graphics = graphics;
            Inline = inline;
        }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for the <see cref="MarkdownRenderer.InlineRendered"/> event.
    /// </summary>
    public class InlineRenderedEventArgs : EventArgs
    {
        /// <summary>
        /// The current <see cref="VectSharp.Markdown.MarkdownContext"/>.
        /// </summary>
        public MarkdownContext MarkdownContext { get; set; }

        /// <summary>
        /// The current <see cref="VectSharp.Graphics"/> surface.
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <summary>
        /// The <see cref="Markdig.Syntax.Inlines.Inline"/> that has been rendered.
        /// </summary>
        public Inline Inline { get; }

        /// <summary>
        /// Create a new <see cref="InlineRenderingEventArgs"/> instance.
        /// </summary>
        /// <param name="markdownContext">The current <see cref="VectSharp.Markdown.MarkdownContext"/>.</param>
        /// <param name="graphics">The current <see cref="VectSharp.Graphics"/> surface.</param>
        /// <param name="inline">The <see cref="Markdig.Syntax.Inlines.Inline"/> that has been rendered.</param>
        public InlineRenderedEventArgs(MarkdownContext markdownContext, Graphics graphics, Inline inline)
        {
            MarkdownContext = markdownContext;
            Graphics = graphics;
            Inline = inline;
        }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for the <see cref="MarkdownRenderer.LineMeasured"/> event.
    /// </summary>
    public class LineMeasuredEventArgs : EventArgs
    {
        /// <summary>
        /// The current <see cref="VectSharp.Markdown.MarkdownContext"/>.
        /// </summary>
        public MarkdownContext MarkdownContext { get; set; }

        /// <summary>
        /// The current <see cref="VectSharp.Graphics"/> surface.
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <summary>
        /// The line that will be rendered.
        /// </summary>
        public Line Line { get; set; }

        /// <summary>
        /// The maximum space available for the line on the x axis.
        /// </summary>
        public double PageMaxX { get; }

        /// <summary>
        /// The maximum space available for the line on the y axis.
        /// </summary>
        public double PageMaxY { get; }

        /// <summary>
        /// The maximum x coordinate of the content in the line.
        /// </summary>
        public double ContentMaxX { get; }

        /// <summary>
        /// The minimum y coordinate of the content in the line.
        /// </summary>
        public double ContentMinY { get; }

        /// <summary>
        /// The maximum y coordinate of the content in the line.
        /// </summary>
        public double ContentMaxY { get; }

        /// <summary>
        /// The vertical coordinate of the baseline.
        /// </summary>
        public double Baseline { get; }

        /// <summary>
        /// Create a new <see cref="LineMeasuredEventArgs"/>
        /// </summary>
        /// <param name="markdownContext">The current <see cref="VectSharp.Markdown.MarkdownContext"/>.</param>
        /// <param name="graphics">The current <see cref="VectSharp.Graphics"/> surface.</param>
        /// <param name="line">The line that will be rendered.</param>
        /// <param name="pageMaxX">The maximum space available for the line on the x axis.</param>
        /// <param name="pageMaxY">The maximum space available for the line on the y axis.</param>
        /// <param name="contentMaxX">The maximum x coordinate of the content in the line.</param>
        /// <param name="contentMinY">The minimum y coordinate of the content in the line.</param>
        /// <param name="contentMaxY">The maximum y coordinate of the content in the line.</param>
        /// <param name="baseline">The vertical coordinate of the baseline.</param>
        public LineMeasuredEventArgs(MarkdownContext markdownContext, Graphics graphics, Line line, double pageMaxX, double pageMaxY, double contentMaxX, double contentMinY, double contentMaxY, double baseline)
        {
            MarkdownContext = markdownContext;
            Graphics = graphics;
            Line = line;
            PageMaxX = pageMaxX;
            PageMaxY = pageMaxY;
            ContentMaxX = contentMaxX;
            ContentMinY = contentMinY;
            ContentMaxY = contentMaxY;
            Baseline = baseline;
        }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for the <see cref="MarkdownRenderer.LineRendering"/> and <see cref="MarkdownRenderer.LineRendered"/> events.
    /// </summary>
    public class LineEventArgs : EventArgs
    {
        /// <summary>
        /// The current <see cref="VectSharp.Markdown.MarkdownContext"/>.
        /// </summary>
        public MarkdownContext MarkdownContext { get; set; }

        /// <summary>
        /// The current <see cref="VectSharp.Graphics"/> surface.
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <summary>
        /// The line that will be rendered.
        /// </summary>
        public Line Line { get; }

        /// <summary>
        /// The maximum space available for the line on the x axis.
        /// </summary>
        public double PageMaxX { get; }

        /// <summary>
        /// The maximum space available for the line on the y axis.
        /// </summary>
        public double PageMaxY { get; }

        /// <summary>
        /// The maximum x coordinate of the content in the line.
        /// </summary>
        public double ContentMaxX { get; }

        /// <summary>
        /// The minimum y coordinate of the content in the line.
        /// </summary>
        public double ContentMinY { get; }

        /// <summary>
        /// The maximum y coordinate of the content in the line.
        /// </summary>
        public double ContentMaxY { get; }

        /// <summary>
        /// The vertical coordinate of the baseline.
        /// </summary>
        public double Baseline { get; }

        /// <summary>
        /// Create a new <see cref="LineEventArgs"/>
        /// </summary>
        /// <param name="markdownContext">The current <see cref="VectSharp.Markdown.MarkdownContext"/>.</param>
        /// <param name="graphics">The current <see cref="VectSharp.Graphics"/> surface.</param>
        /// <param name="line">The line that will be rendered.</param>
        /// <param name="pageMaxX">The maximum space available for the line on the x axis.</param>
        /// <param name="pageMaxY">The maximum space available for the line on the y axis.</param>
        /// <param name="contentMaxX">The maximum x coordinate of the content in the line.</param>
        /// <param name="contentMinY">The minimum y coordinate of the content in the line.</param>
        /// <param name="contentMaxY">The maximum y coordinate of the content in the line.</param>
        /// <param name="baseline">The vertical coordinate of the baseline.</param>
        public LineEventArgs(MarkdownContext markdownContext, Graphics graphics, Line line, double pageMaxX, double pageMaxY, double contentMaxX, double contentMinY, double contentMaxY, double baseline)
        {
            MarkdownContext = markdownContext;
            Graphics = graphics;
            Line = line;
            PageMaxX = pageMaxX;
            PageMaxY = pageMaxY;
            ContentMaxX = contentMaxX;
            ContentMinY = contentMinY;
            ContentMaxY = contentMaxY;
            Baseline = baseline;
        }
    }
}
