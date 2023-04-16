/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2023 Giorgio Bianchini, University of Bristol

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
using System.Collections.Generic;

namespace VectSharp.Plots
{
    /// <summary>
    /// Represents a symbol that can be added to the plot at a specified position.
    /// </summary>
    public interface IDataPointElement
    {
        /// <summary>
        /// Draw the symbol on the plot.
        /// </summary>
        /// <param name="target">The <see cref="Graphics"/> object on which to draw. It is assumed that it has been transformed so that the symbol can be drawn centred at (0, 0)</param>
        /// <param name="presentationAttributes">Presentation attributes determining the appearance of the symbol.</param>
        /// <param name="tag">A tag to identify the symbol in the plot.</param>
        void Plot(Graphics target, PlotElementPresentationAttributes presentationAttributes, string tag);
    }

    /// <summary>
    /// A symbol defined by a <see cref="GraphicsPath"/>.
    /// </summary>
    public class PathDataPointElement : IDataPointElement
    {
        /// <summary>
        /// The <see cref="GraphicsPath"/> that constitutes the symbol (by default, a circle).
        /// </summary>
        public GraphicsPath Path { get; set; } = new GraphicsPath().Arc(0, 0, 1, 0, 2 * Math.PI).Close();

        /// <inheritdoc/>
        public void Plot(Graphics target, PlotElementPresentationAttributes presentationAttributes, string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                if (presentationAttributes.Fill != null)
                {
                    target.FillPath(Path, presentationAttributes.Fill);
                }

                if (presentationAttributes.Stroke != null)
                {
                    target.StrokePath(Path, presentationAttributes.Stroke, presentationAttributes.LineWidth, presentationAttributes.LineCap, presentationAttributes.LineJoin, presentationAttributes.LineDash);
                }
            }
            else
            {
                if (presentationAttributes.Fill != null)
                {
                    target.FillPath(Path, presentationAttributes.Fill, tag);
                }

                if (presentationAttributes.Stroke != null)
                {
                    target.StrokePath(Path, presentationAttributes.Stroke, presentationAttributes.LineWidth, presentationAttributes.LineCap, presentationAttributes.LineJoin, presentationAttributes.LineDash, tag + (target.UseUniqueTags ? "@stroke" : ""));
                }
            }
        }

        /// <summary>
        /// Create a new <see cref="PathDataPointElement"/> instance representing a circle.
        /// </summary>
        public PathDataPointElement()
        {

        }

        /// <summary>
        /// Create a new <see cref="PathDataPointElement"/> instance with the specified <paramref name="path"/>.
        /// </summary>
        public PathDataPointElement(GraphicsPath path)
        {
            this.Path = path;
        }
    }

    /// <summary>
    /// A symbol defined by a <see cref="VectSharp.Graphics"/> object.
    /// </summary>
    public class GraphicsDataPointElement : IDataPointElement
    {
        /// <summary>
        /// The <see cref="VectSharp.Graphics"/> object that will be copied on the plot.
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <inheritdoc/>
        public void Plot(Graphics target, PlotElementPresentationAttributes presentationAttributes, string tag)
        {
            target.DrawGraphics(0, 0, Graphics, tag);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDataPointElement"/> instance.
        /// </summary>
        /// <param name="graphics"></param>
        public GraphicsDataPointElement(Graphics graphics)
        {
            Graphics = graphics;
        }
    }

    /// <summary>
    /// A symbol drawn by a custom <see cref="Action"/>.
    /// </summary>
    public class ActionDataPointElement : IDataPointElement
    {
        /// <summary>
        /// The <see cref="Action"/> used to draw the symbol. This should take as arguments the <see cref="Graphics"/>
        /// object on which to draw the symbol, the <see cref="PlotElementPresentationAttributes"/> describing the
        /// appearance of the symbol, and a <see langword="string"/> representing a tag for the symbol.
        /// </summary>
        public Action<Graphics, PlotElementPresentationAttributes, string> PlotAction { get; set; }

        /// <inheritdoc/>
        public void Plot(Graphics target, PlotElementPresentationAttributes presentationAttributes, string tag) => PlotAction(target, presentationAttributes, tag);

        /// <summary>
        /// Creates a new <see cref="ActionDataPointElement"/> using the specified action to draw the symbol.
        /// </summary>
        /// <param name="plotAction">The <see cref="Action"/> used to draw the symbol. This should take as arguments the <see cref="Graphics"/>
        /// object on which to draw the symbol, the <see cref="PlotElementPresentationAttributes"/> describing the
        /// appearance of the symbol, and a <see langword="string"/> representing a tag for the symbol.</param>
        public ActionDataPointElement(Action<Graphics, PlotElementPresentationAttributes, string> plotAction)
        {
            PlotAction = plotAction;
        }
    }

    /// <summary>
    /// A plot element that draws a symbol at the location of multiple data points.
    /// </summary>
    /// <typeparam name="T">The kind of data describing the data points (generally, <c>IReadOnlyList&lt;double&gt;</c>).</typeparam>
    public class ScatterPoints<T> : IPlotElement
    {
        /// <summary>
        /// The data points at which the symbols will be drawn.
        /// </summary>
        public IEnumerable<T> Data { get; set; }

        /// <summary>
        /// The size of the symbols to draw, in plot coordinates.
        /// </summary>
        public double Size { get; set; } = 2;

        /// <summary>
        /// The symbol that will be drawn (by default, a circle).
        /// </summary>
        public IDataPointElement DataPointElement { get; set; } = new PathDataPointElement();

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<T> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes determining the appearance (stroke and fill colour, etc.) of the symbols.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes();
        
        /// <summary>
        /// A tag to identify the symbols in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Creates a new <see cref="ScatterPoints{T}"/> instance, using the specified <paramref name="data"/> and <paramref name="coordinateSystem"/>.
        /// </summary>
        /// <param name="data">The data points at which the symbols will be drawn.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public ScatterPoints(IEnumerable<T> data, ICoordinateSystem<T> coordinateSystem)
        {
            this.Data = data;
            this.CoordinateSystem = coordinateSystem;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            int index = 0;

            foreach (T data in Data)
            {
                Point pt = CoordinateSystem.ToPlotCoordinates(data);

                target.Save();
                target.Translate(pt);
                target.Scale(Size, Size);

                if (!target.UseUniqueTags || string.IsNullOrEmpty(Tag))
                {
                    DataPointElement.Plot(target, PresentationAttributes, Tag);
                }
                else
                {
                    DataPointElement.Plot(target, PresentationAttributes, Tag + "@" + index.ToString());
                    index++;
                }

                target.Restore();
            }
        }
    }

    /// <summary>
    /// A plot element that draws a single text label.
    /// </summary>
    /// <typeparam name="T">The kind of data describing the data points (generally, <c>IReadOnlyList&lt;double&gt;</c>).</typeparam>
    public class TextLabel<T> : IPlotElement
    {
        /// <summary>
        /// The position of the label, in data space coordinates.
        /// </summary>
        public T Position { get; set; }

        /// <summary>
        /// The text of the label. This can include formatting specifiers (see the documentation for the <see cref="FormattedText.Format(string, Font, Font, Font, Font, Brush)"/> method).
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The angle at which the text is drawn, with respect to the horizontal.
        /// </summary>
        public double Rotation { get; set; } = 0;

        /// <summary>
        /// The baseline for the text.
        /// </summary>
        public TextBaselines Baseline { get; set; } = TextBaselines.Middle;

        /// <summary>
        /// The alignment for the text.
        /// </summary>
        public TextAnchors Alignment { get; set; } = TextAnchors.Center;

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<T> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes determining the appearance (stroke and fill colour, etc.) of the text label.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { Stroke = null };
        
        /// <summary>
        /// A tag to identify the label in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Create a new <see cref="TextLabel{T}"/> instance.
        /// </summary>
        /// <param name="label">The text of the label. This can include formatting specifiers (see the documentation for the <see cref="FormattedText.Format(string, Font, Font, Font, Font, Brush)"/> method).</param>
        /// <param name="position">The position of the label, in data space coordinates.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public TextLabel(string label, T position, ICoordinateSystem<T> coordinateSystem)
        {
            this.Label = label;
            this.Position = position;
            this.CoordinateSystem = coordinateSystem;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            if (!string.IsNullOrEmpty(Label))
            {
                IEnumerable<FormattedText> label;

                if (PresentationAttributes.Font.FontFamily.IsStandardFamily)
                {
                    int i = Array.IndexOf(FontFamily.StandardFamilies, PresentationAttributes.Font.FontFamily.FamilyName.Replace(" ", "-"));

                    label = FormattedText.Format(this.Label, (FontFamily.StandardFontFamilies)i, PresentationAttributes.Font.FontSize);
                }
                else
                {
                    label = FormattedText.Format(this.Label, PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font);
                }

                Point point = CoordinateSystem.ToPlotCoordinates(Position);

                target.Save();
                target.Translate(point);
                target.Rotate(Rotation);

                double x = Alignment == TextAnchors.Left ? 0 : Alignment == TextAnchors.Right ? -label.Measure().Width : -label.Measure().Width * 0.5;

                string fillTag = Tag;
                string strokeTag = Tag;

                if (!string.IsNullOrEmpty(Tag) && target.UseUniqueTags)
                {
                    strokeTag = strokeTag + "@stroke";
                }

                if (PresentationAttributes.Fill != null)
                {
                    target.FillText(x, 0, label, PresentationAttributes.Fill, Baseline, fillTag);
                }

                if (PresentationAttributes.Stroke != null)
                {
                    target.StrokeText(x, 0, label, PresentationAttributes.Stroke, Baseline, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, strokeTag);
                }

                target.Restore();
            }
        }

    }

    /// <summary>
    /// A plot element that draws a text label at each data point.
    /// </summary>
    /// <typeparam name="T">The kind of data describing the data points (generally, <c>IReadOnlyList&lt;double&gt;</c>).</typeparam>
    public class DataLabels<T> : IPlotElement
    {
        /// <summary>
        /// The data points at which the labels will be drawn.
        /// </summary>
        public IEnumerable<T> Data { get; set; }

        private Func<int, T, IEnumerable<FormattedText>> labelFunction;
        private Func<int, T, object> label;
        
        /// <summary>
        /// A function used to determine the text of the labels to draw. The arguments for this function should be a <see langword="int"/>
        /// representing the index of the data point and a <typeparamref name="T"/> representing the coordinates of the data point. This
        /// function should return either an <see cref="IEnumerable{T}"/> of <see cref="FormattedText"/> objects, which will be used as-is,
        /// or any other kind of object, which will be converted to a <see langword="string"/> to be used in the label.
        /// </summary>
        public Func<int, T, object> Label
        {
            get
            {
                return label;
            }

            set
            {
                label = value;

                if (typeof(IEnumerable<FormattedText>).IsAssignableFrom(value.Method.ReturnType))
                {
                    labelFunction = (i, a) => (IEnumerable<FormattedText>)value(i, a);
                }
                else
                {
                    labelFunction = (i, a) =>
                    {
                        object lbl = value(i, a);

                        if (lbl is IEnumerable<FormattedText> frmt)
                        {
                            return frmt;
                        }
                        else if (lbl == null)
                        {
                            return null;
                        }
                        else
                        {
                            if (PresentationAttributes.Font.FontFamily.IsStandardFamily)
                            {
                                int j = Array.IndexOf(FontFamily.StandardFamilies, PresentationAttributes.Font.FontFamily.FamilyName.Replace(" ", "-"));

                                return FormattedText.Format(lbl.ToString(), (FontFamily.StandardFontFamilies)j, PresentationAttributes.Font.FontSize);
                            }
                            else
                            {
                                return FormattedText.Format(lbl.ToString(), PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font);
                            }
                        }
                    };
                }
            }
        }

        /// <summary>
        /// A function used to determine the orientation of the labels with respect to the horizontal. The arguments for this function should be a <see langword="int"/>
        /// representing the index of the data point and a <typeparamref name="T"/> representing the coordinates of the data point. This
        /// function should return a <see langword="double"/> representing the angle with respect to the horizontal at which the label
        /// should be drawn.
        /// </summary>
        public Func<int, T, double> Rotation { get; set; } = (i, d) => 0;

        /// <summary>
        /// A function used to determine the position of the labels with respect to the data points. The arguments for this function should be a <see langword="int"/>
        /// representing the index of the data point and a <typeparamref name="T"/> representing the coordinates of the data point. This
        /// function should return a <see cref="Point"/> defining the amount of space between the data point and the label, in plot coordinates.
        /// </summary>
        public Func<int, T, Point> Margin { get; set; } = (i, d) => new Point();
        
        /// <summary>
        /// The baseline for the labels.
        /// </summary>
        public TextBaselines Baseline { get; set; } = TextBaselines.Middle;

        /// <summary>
        /// The alignment for the labels.
        /// </summary>
        public TextAnchors Alignment { get; set; } = TextAnchors.Center;

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<T> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;
        
        /// <summary>
        /// Presentation attributes determining the appearance of the labels.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes() { Stroke = null };
        
        /// <summary>
        /// A tag to identify the labels in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Creates a new <see cref="DataLabels{T}"/> instance.
        /// </summary>
        /// <param name="data">The data points at which the labels will be drawn.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public DataLabels(IEnumerable<T> data, ICoordinateSystem<T> coordinateSystem)
        {
            this.Data = data;
            this.CoordinateSystem = coordinateSystem;
            this.Label = (i, d) =>
            {
                if (PresentationAttributes.Font.FontFamily.IsStandardFamily)
                {
                    int j = Array.IndexOf(FontFamily.StandardFamilies, PresentationAttributes.Font.FontFamily.FamilyName.Replace(" ", "-"));

                    return FormattedText.Format(i.ToString(), (FontFamily.StandardFontFamilies)j, PresentationAttributes.Font.FontSize);
                }
                else
                {
                    return FormattedText.Format(i.ToString(), PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font, PresentationAttributes.Font);
                }
            };
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            int index = 0;

            foreach (T data in Data)
            {
                IEnumerable<FormattedText> label = labelFunction(index, data);
                if (label != null)
                {
                    double rotation = Rotation(index, data);
                    Point pt = CoordinateSystem.ToPlotCoordinates(data);
                    Point margin = Margin(index, data);

                    pt = new Point(pt.X + margin.X, pt.Y + margin.Y);

                    target.Save();
                    target.Translate(pt);
                    target.Rotate(rotation);

                    string fillTag = Tag;
                    string strokeTag = Tag;


                    if (target.UseUniqueTags && !string.IsNullOrEmpty(Tag))
                    {
                        fillTag = fillTag + "@" + index.ToString();
                        strokeTag = strokeTag + "@stroke" + index.ToString();
                    }

                    if (PresentationAttributes.Fill != null)
                    {
                        target.FillText(Alignment == TextAnchors.Left ? 0 : Alignment == TextAnchors.Right ? -label.Measure().Width : -label.Measure().Width * 0.5, 0, label, PresentationAttributes.Fill, Baseline, fillTag);
                    }

                    if (PresentationAttributes.Stroke != null)
                    {
                        target.StrokeText(Alignment == TextAnchors.Left ? 0 : Alignment == TextAnchors.Right ? -label.Measure().Width : -label.Measure().Width * 0.5, 0, label, PresentationAttributes.Stroke, Baseline, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, fillTag);
                    }

                    target.Restore();
                }
                index++;
            }
        }
    }

    /// <summary>
    /// A plot element that draws a line passing through a set of points.
    /// </summary>
    /// <typeparam name="T">The kind of data describing the data points (generally, <c>IReadOnlyList&lt;double&gt;</c>).</typeparam>
    public class DataLine<T> : IPlotElement
    {
        /// <summary>
        /// The data points through which the line will pass.
        /// </summary>
        public IEnumerable<T> Data { get; set; }
        
        /// <summary>
        /// If this is <see langword="false"/>, straight line segments are used to join the data points. If this is <see langword="true"/>,
        /// a smooth spline passing through all of them is used instead.
        /// </summary>
        public bool Smooth { get; set; }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<T> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes determining the appearance of the line.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes();

        /// <summary>
        /// A tag to identify the labels in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Create a new instance of the <see cref="DataLine{T}"/> class.
        /// </summary>
        /// <param name="data">The data points through which the line will pass.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public DataLine(IEnumerable<T> data, ICoordinateSystem<T> coordinateSystem)
        {
            this.Data = data;
            this.CoordinateSystem = coordinateSystem;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            GraphicsPath pth = new GraphicsPath();

            if (!Smooth)
            {
                bool started = false;

                foreach (T data in Data)
                {
                    Point pt = CoordinateSystem.ToPlotCoordinates(data);

                    if (!started)
                    {
                        pth.MoveTo(pt);
                        started = true;
                    }
                    else
                    {
                        pth.LineTo(pt);
                    }
                }
            }
            else
            {
                List<Point> points = new List<Point>();

                foreach (T data in Data)
                {
                    Point pt = CoordinateSystem.ToPlotCoordinates(data);
                    points.Add(pt);
                }
                pth.AddSmoothSpline(points.ToArray());
            }

            target.StrokePath(pth, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, Tag);
        }
    }

    /// <summary>
    /// A plot element that fills an area between a line passing through some data points and a base line.
    /// </summary>
    /// <typeparam name="T">The kind of data describing the data points (generally, <c>IReadOnlyList&lt;double&gt;</c>).</typeparam>
    public class Area<T> : IPlotElement
    {
        /// <summary>
        /// The data points through which the upper part of the area will pass.
        /// </summary>
        public IEnumerable<T> Data { get; set; }

        /// <summary>
        /// A function returning the baseline for each data point.
        /// </summary>
        public Func<T, T> GetBaseline { get; set; }

        /// <summary>
        /// If this is <see langword="false"/>, straight line segments are used to join the data points. If this is <see langword="true"/>,
        /// a smooth spline passing through all of them is used instead.
        /// </summary>
        public bool Smooth { get; set; }


        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<T> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes determining the appearance of the area.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes();
        
        /// <summary>
        /// A tag to identify the area in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Create a new <see cref="Area{T}"/> instance.
        /// </summary>
        /// <param name="data">The data points through which the upper part of the area will pass.</param>
        /// <param name="getBaseline">A function returning the baseline for each data point.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Area(IEnumerable<T> data, Func<T, T> getBaseline, ICoordinateSystem<T> coordinateSystem)
        {
            this.Data = data;
            this.CoordinateSystem = coordinateSystem;
            this.GetBaseline = getBaseline;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            GraphicsPath pth = new GraphicsPath();
            GraphicsPath pthStroke = new GraphicsPath();

            if (!Smooth)
            {
                bool started = false;

                List<Point> oppositePoints = new List<Point>();

                foreach (T data in Data)
                {
                    Point pt = CoordinateSystem.ToPlotCoordinates(data);

                    oppositePoints.Add(CoordinateSystem.ToPlotCoordinates(GetBaseline(data)));

                    if (!started)
                    {
                        pth.MoveTo(pt);
                        pthStroke.MoveTo(pt);
                        started = true;
                    }
                    else
                    {
                        pth.LineTo(pt);
                        pthStroke.LineTo(pt);
                    }
                }

                for (int i = oppositePoints.Count - 1; i >= 0; i--)
                {
                    pth.LineTo(oppositePoints[i]);
                }

                pth.Close();
            }
            else
            {
                List<Point> points = new List<Point>();
                List<Point> oppositePoints = new List<Point>();

                foreach (T data in Data)
                {
                    Point pt = CoordinateSystem.ToPlotCoordinates(data);
                    points.Add(pt);
                    oppositePoints.Add(CoordinateSystem.ToPlotCoordinates(GetBaseline(data)));
                }
                pth.AddSmoothSpline(points.ToArray());
                pthStroke.AddSmoothSpline(points.ToArray());
                oppositePoints.Reverse();
                pth.AddSmoothSpline(oppositePoints.ToArray());
                pth.Close();
            }

            string tag = Tag;
            string strokeTag = tag;

            if (!string.IsNullOrEmpty(tag) && target.UseUniqueTags)
            {
                strokeTag += "@stroke";
            }

            if (PresentationAttributes.Fill != null)
            {
                target.FillPath(pth, PresentationAttributes.Fill, tag);
            }
            
            if (PresentationAttributes.Stroke != null)
            {
                target.StrokePath(pthStroke, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, strokeTag);
            }
        }
    }
}
