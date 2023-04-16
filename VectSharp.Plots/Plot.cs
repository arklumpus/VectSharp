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
using System.Collections.Immutable;

namespace VectSharp.Plots
{
    /// <summary>
    /// Determines the appearance of plot elements.
    /// </summary>
    public class PlotElementPresentationAttributes
    {
        /// <summary>
        /// The stroke of the plot element.
        /// </summary>
        public Brush Stroke { get; set; } = Colours.Black;

        /// <summary>
        /// The fill of the plot element.
        /// </summary>
        public Brush Fill { get; set; } = Colours.Black;

        /// <summary>
        /// The thickness of lines in the plot element.
        /// </summary>
        public double LineWidth { get; set; } = 1;

        /// <summary>
        /// The line dash style for the plot element.
        /// </summary>
        public LineDash? LineDash { get; set; } = null;

        /// <summary>
        /// The line cap for the plot element.
        /// </summary>
        public LineCaps LineCap { get; set; } = LineCaps.Square;

        /// <summary>
        /// The line join for the plot element.
        /// </summary>
        public LineJoins LineJoin { get; set; } = LineJoins.Miter;

        /// <summary>
        /// The font used to draw text in the plot element.
        /// </summary>
        public Font Font { get; set; } = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);

        /// <summary>
        /// Create a new <see cref="PlotElementPresentationAttributes"/> with the default values.
        /// </summary>
        public PlotElementPresentationAttributes()
        {

        }

        /// <summary>
        /// Creates a new <see cref="PlotElementPresentationAttributes"/> copying all settings from another instance.
        /// </summary>
        /// <param name="other">The other instance from which the settings will be copied.</param>
        public PlotElementPresentationAttributes(PlotElementPresentationAttributes other)
        {
            this.LineJoin = other.LineJoin;
            this.Stroke = other.Stroke;
            this.Fill = other.Fill;
            this.LineCap = other.LineCap;
            this.LineDash = other.LineDash;
            this.Font = other.Font;
            this.LineWidth = other.LineWidth;
        }
    }

    /// <summary>
    /// Represents a plot element.
    /// </summary>
    public interface IPlotElement
    {
        /// <summary>
        /// Draw the plot element on the specified <paramref name="target"/>&#160;<see cref="Graphics"/>.
        /// </summary>
        /// <param name="target">The <see cref="Graphics"/> on which to draw.</param>
        void Plot(Graphics target);

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        ICoordinateSystem CoordinateSystem { get; }
    }

    /// <summary>
    /// A plot element that uses an <see cref="Action"/> to draw its contents.
    /// </summary>
    /// <typeparam name="T">The type of data to plot (e.g. <c>double[]</c>).</typeparam>
    public class PlotElement<T> : IPlotElement
    {
        /// <summary>
        /// The action that is invoked when the plot element needs to be drawn.
        /// </summary>
        public Action<Graphics, ICoordinateSystem<T>> PlotAction { get; set; }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<T> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => this.CoordinateSystem;

        /// <summary>
        /// Create a new <see cref="PlotElement{T}"/> using the specified coordinate system and plot action.
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="plotAction">The action that is invoked when the plot element needs to be drawn.</param>
        public PlotElement(ICoordinateSystem<T> coordinateSystem, Action<Graphics, ICoordinateSystem<T>> plotAction)
        {
            PlotAction = plotAction;
            CoordinateSystem = coordinateSystem;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            PlotAction?.Invoke(target, CoordinateSystem);
        }
    }

    /// <summary>
    /// Represents a collection of plot elements.
    /// </summary>
    public partial class Plot
    {
        /// <summary>
        /// The elements contained in the plot.
        /// </summary>
        public ImmutableList<IPlotElement> PlotElements { get; private set; } = ImmutableList.Create<IPlotElement>();

        /// <summary>
        /// Add the specified plot element to the plot.
        /// </summary>
        /// <param name="plotElement">The plot element to add.</param>
        public void AddPlotElement(IPlotElement plotElement)
        {
            this.PlotElements = this.PlotElements.Add(plotElement);
        }

        /// <summary>
        /// Add the specified plot elements to the plot.
        /// </summary>
        /// <param name="plotElements">The plot elements to add.</param>
        public void AddPlotElements(IEnumerable<IPlotElement> plotElements)
        {
            this.PlotElements = this.PlotElements.AddRange(plotElements);
        }

        /// <summary>
        /// Add the specified plot elements to the plot.
        /// </summary>
        /// <param name="plotElements">The plot elements to add.</param>
        public void AddPlotElements(params IPlotElement[] plotElements)
        {
            this.AddPlotElements((IEnumerable<IPlotElement>)plotElements);
        }

        /// <summary>
        /// Remove the specified plot element from the plot.
        /// </summary>
        /// <param name="plotElement">The plot element to remove.</param>
        public void RemovePlotElement(IPlotElement plotElement)
        {
            this.PlotElements = this.PlotElements.Remove(plotElement);
        }

        /// <summary>
        /// Render the plot on the specified <paramref name="target"/> <see cref="Graphics"/>.
        /// </summary>
        /// <param name="target">The <see cref="Graphics"/> on which the plot should be drawn.</param>
        public void Render(Graphics target)
        {
            foreach (IPlotElement element in PlotElements)
            {
                element.Plot(target);
            }
        }

        /// <summary>
        /// Get the first <see cref="IPlotElement"/> of the specified type, or the first <see cref="ICoordinateSystem"/> of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of plot element or coordinate system to get.</typeparam>
        /// <returns>The first <see cref="IPlotElement"/> of the specified type, or the first <see cref="ICoordinateSystem"/> of the specified type, or <see langword="null"/> if none could be found.</returns>
        public T GetFirst<T>() where T : class
        {
            foreach (IPlotElement e in this.PlotElements)
            {
                if (e is T t)
                {
                    return t;
                }
                else if (e.CoordinateSystem is T t2)
                {
                    return t2;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all <see cref="IPlotElement"/>s of the specified type, or all <see cref="ICoordinateSystem"/>s of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of plot element or coordinate system to get.</typeparam>
        /// <returns>All <see cref="IPlotElement"/>s of the specified type, or all <see cref="ICoordinateSystem"/>s of the specified type.</returns>
        public IEnumerable<T> GetAll<T>() where T : class, IPlotElement
        {
            foreach (IPlotElement e in this.PlotElements)
            {
                if (e is T t)
                {
                    yield return t;
                }
                else if (e.CoordinateSystem is T t2)
                {
                    yield return t2;
                }
            }
        }

        /// <summary>
        /// Render the plot to a suitably cropped <see cref="Page"/> object.
        /// </summary>
        /// <returns>A <see cref="Page"/> object containing the rendered plot.</returns>
        public Page Render()
        {
            Page pag = new Page(1, 1);

            this.Render(pag.Graphics);

            Rectangle bounds = pag.Graphics.GetBounds();

            pag.Crop(new Point(bounds.Location.X - bounds.Size.Width * 0.01, bounds.Location.Y - bounds.Size.Height * 0.01), new Size(bounds.Size.Width * 1.02, bounds.Size.Height * 1.02));

            return pag;
        }

        /// <summary>
        /// Create a new empty <see cref="Plot"/>.
        /// </summary>
        public Plot()
        {

        }

        private static (double minX, double minY, double maxX, double maxY, double rangeX, double rangeY) GetDataRange(IReadOnlyList<IReadOnlyList<double>> data)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;

            double maxX = double.MinValue;
            double maxY = double.MinValue;

            for (int i = 0; i < data.Count; i++)
            {
                minX = Math.Min(minX, data[i][0]);
                minY = Math.Min(minY, data[i][1]);

                maxX = Math.Max(maxX, data[i][0]);
                maxY = Math.Max(maxY, data[i][1]);
            }

            double rangeX = maxX - minX;
            double rangeY = maxY - minY;

            return (minX, minY, maxX, maxY, rangeX, rangeY);
        }

        /// <summary>
        /// Contains methods to create plots.
        /// </summary>
        public static partial class Create
        {
            /// <summary>
            /// Creates a new empty plot.
            /// </summary>
            /// <returns>An empty plot.</returns>
            public static Plot Empty()
            {
                return new Plot();
            }
        }
    }
}
