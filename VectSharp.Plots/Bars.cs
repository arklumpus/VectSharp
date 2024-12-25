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
using System.Linq;

namespace VectSharp.Plots
{
    /// <summary>
    /// A plot element that draws bars.
    /// </summary>
    /// <typeparam name="T">The type of data elements.</typeparam>
    public class Bars<T> : IPlotElement
    {
        private double margin = 0;

        /// <summary>
        /// The data points corresponding to the tips of the bars.
        /// </summary>
        public SortedSet<T> Data { get; set; }

        /// <summary>
        /// A function that returns the bottom for each bar. This function should accept
        /// a single parameter of type <typeparamref name="T"/> and return another <typeparamref name="T"/>
        /// object, representing the bottom of the bar in data space.
        /// </summary>
        public Func<T, T> GetBaseline { get; set; }

        /// <summary>
        /// The margin between consecutive bars. This should range between 0 and 1.
        /// </summary>
        public double Margin
        {
            get => margin;
            set
            {
                if (value >= 0 && value <= 1)
                {
                    margin = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The value for the margin must be within 0 and 1 (inclusive)!");
                }
            }
        }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<T> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes for the bars.
        /// </summary>
        public PlotElementPresentationAttributes PresentationAttributes { get; set; } = new PlotElementPresentationAttributes();

        /// <summary>
        /// A tag to identify the bars in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Create a new <see cref="Bars{T}"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars.</param>
        /// <param name="sorting">A comparer used to sort the bars.</param>
        /// <param name="getBaseline">A function that returns the bottom for each bar. This function should accept
        /// a single parameter of type <typeparamref name="T"/> and return another <typeparamref name="T"/>
        /// object, representing the bottom of the bar in data space.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Bars(IEnumerable<T> data, IComparer<T> sorting, Func<T, T> getBaseline, ICoordinateSystem<T> coordinateSystem)
        {
            this.Data = new SortedSet<T>(data, sorting);
            this.CoordinateSystem = coordinateSystem;
            this.GetBaseline = getBaseline;
        }

        /// <summary>
        /// Create a new <see cref="Bars{T}"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars.</param>
        /// <param name="sorting">A comparer used to sort the bars.</param>
        /// <param name="getBaseline">A function that returns the bottom for each bar. This function should accept
        /// a single parameter of type <typeparamref name="T"/> and return another <typeparamref name="T"/>
        /// object, representing the bottom of the bar in data space.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Bars(IEnumerable<T> data, Comparison<T> sorting, Func<T, T> getBaseline, ICoordinateSystem<T> coordinateSystem) : this(data, Comparer<T>.Create(sorting), getBaseline, coordinateSystem) { }

        /// <summary>
        /// Create a new <see cref="Bars{T}"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars. These should already be sorted.</param>
        /// <param name="getBaseline">A function that returns the bottom for each bar. This function should accept
        /// a single parameter of type <typeparamref name="T"/> and return another <typeparamref name="T"/>
        /// object, representing the bottom of the bar in data space.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Bars(IReadOnlyList<T> data, Func<T, T> getBaseline, ICoordinateSystem<T> coordinateSystem)
        {
            Dictionary<T, int> indices = new Dictionary<T, int>();

            for (int i = 0; i < data.Count; i++)
            {
                indices[data[i]] = i;
            }

            this.Data = new SortedSet<T>(data, Comparer<T>.Create((x, y) => Math.Sign(indices[x] - indices[y])));
            this.CoordinateSystem = coordinateSystem;
            this.GetBaseline = getBaseline;
        }

        private static bool AnyNan(params Point[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (double.IsNaN(points[i].X) || double.IsNaN(points[i].Y))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            List<(Point, Point)> bars = new List<(Point, Point)>();

            foreach (T data in Data)
            {
                Point pt = CoordinateSystem.ToPlotCoordinates(data);
                Point baseline = CoordinateSystem.ToPlotCoordinates(GetBaseline(data));
                bars.Add((baseline, pt));
            }

            for (int i = 0; i < bars.Count; i++)
            {
                GraphicsPath pth = new GraphicsPath();

                if (i > 0 && i < bars.Count - 1)
                {
                    Point prevBaselineMid = new Point(bars[i].Item1.X * (0.5 + Margin * 0.5) + bars[i - 1].Item1.X * (0.5 - Margin * 0.5), bars[i].Item1.Y * (0.5 + Margin * 0.5) + bars[i - 1].Item1.Y * (0.5 - Margin * 0.5));

                    Point perpDir = new Point(bars[i].Item2.Y - bars[i].Item1.Y, -(bars[i].Item2.X - bars[i].Item1.X));
                    perpDir = perpDir.Normalize();

                    double t = (bars[i].Item2.X - prevBaselineMid.X) * perpDir.X + (bars[i].Item2.Y - prevBaselineMid.Y) * perpDir.Y;
                    Point prevTop = new Point(bars[i].Item2.X - perpDir.X * t, bars[i].Item2.Y - perpDir.Y * t);

                    Point nextBaselineMid = new Point(bars[i].Item1.X * (0.5 + Margin * 0.5) + bars[i + 1].Item1.X * (0.5 - Margin * 0.5), bars[i].Item1.Y * (0.5 + Margin * 0.5) + bars[i + 1].Item1.Y * (0.5 - Margin * 0.5));

                    double t2 = (bars[i].Item2.X - nextBaselineMid.X) * perpDir.X + (bars[i].Item2.Y - nextBaselineMid.Y) * perpDir.Y;
                    Point nextTop = new Point(bars[i].Item2.X - perpDir.X * t2, bars[i].Item2.Y - perpDir.Y * t2);

                    if (!AnyNan(prevBaselineMid, prevTop, nextTop, nextBaselineMid))
                    {
                        pth.MoveTo(prevBaselineMid).LineTo(prevTop).LineTo(nextTop).LineTo(nextBaselineMid).Close();
                    }
                }
                else if (i == 0)
                {
                    Point perpDir = new Point(bars[i].Item2.Y - bars[i].Item1.Y, -(bars[i].Item2.X - bars[i].Item1.X));
                    perpDir = perpDir.Normalize();

                    Point nextBaselineMid = new Point(bars[i].Item1.X * (0.5 + Margin * 0.5) + bars[i + 1].Item1.X * (0.5 - Margin * 0.5), bars[i].Item1.Y * (0.5 + Margin * 0.5) + bars[i + 1].Item1.Y * (0.5 - Margin * 0.5));

                    double t2 = (bars[i].Item2.X - nextBaselineMid.X) * perpDir.X + (bars[i].Item2.Y - nextBaselineMid.Y) * perpDir.Y;
                    Point nextTop = new Point(bars[i].Item2.X - perpDir.X * t2, bars[i].Item2.Y - perpDir.Y * t2);


                    Point prevBaselineMid = new Point(2 * bars[i].Item1.X - nextBaselineMid.X, 2 * bars[i].Item1.Y - nextBaselineMid.Y);
                    double t = -t2;
                    Point prevTop = new Point(bars[i].Item2.X - perpDir.X * t, bars[i].Item2.Y - perpDir.Y * t);

                    if (!AnyNan(prevBaselineMid, prevTop, nextTop, nextBaselineMid))
                    {
                        pth.MoveTo(prevBaselineMid).LineTo(prevTop).LineTo(nextTop).LineTo(nextBaselineMid).Close();
                    }
                }
                else if (i == bars.Count - 1)
                {
                    Point prevBaselineMid = new Point(bars[i].Item1.X * (0.5 + Margin * 0.5) + bars[i - 1].Item1.X * (0.5 - Margin * 0.5), bars[i].Item1.Y * (0.5 + Margin * 0.5) + bars[i - 1].Item1.Y * (0.5 - Margin * 0.5));

                    Point perpDir = new Point(bars[i].Item2.Y - bars[i].Item1.Y, -(bars[i].Item2.X - bars[i].Item1.X));
                    perpDir = perpDir.Normalize();

                    double t = (bars[i].Item2.X - prevBaselineMid.X) * perpDir.X + (bars[i].Item2.Y - prevBaselineMid.Y) * perpDir.Y;
                    Point prevTop = new Point(bars[i].Item2.X - perpDir.X * t, bars[i].Item2.Y - perpDir.Y * t);

                    Point nextBaselineMid = new Point(2 * bars[i].Item1.X - prevBaselineMid.X, 2 * bars[i].Item1.Y - prevBaselineMid.Y);

                    double t2 = -t;
                    Point nextTop = new Point(bars[i].Item2.X - perpDir.X * t2, bars[i].Item2.Y - perpDir.Y * t2);

                    if (!AnyNan(prevBaselineMid, prevTop, nextTop, nextBaselineMid))
                    {
                        pth.MoveTo(prevBaselineMid).LineTo(prevTop).LineTo(nextTop).LineTo(nextBaselineMid).Close();
                    }
                }

                string tag = Tag;
                string strokeTag = Tag;

                if (!string.IsNullOrEmpty(Tag))
                {
                    tag = tag + "@" + i.ToString();
                    strokeTag = tag + "@" + i.ToString() + "_stroke";
                }

                if (PresentationAttributes.Fill != null)
                {
                    target.FillPath(pth, PresentationAttributes.Fill, tag);
                }

                if (PresentationAttributes.Stroke != null)
                {
                    target.StrokePath(pth, PresentationAttributes.Stroke, PresentationAttributes.LineWidth, PresentationAttributes.LineCap, PresentationAttributes.LineJoin, PresentationAttributes.LineDash, strokeTag);
                }
            }
        }
    }

    /// <summary>
    /// A plot element that draws bars for categorical data.
    /// </summary>
    /// <typeparam name="T">The type of the data categories.</typeparam>
    public class CategoricalBars<T> : Bars<(T, double)>
    {
        /// <summary>
        /// Create a new <see cref="CategoricalBars{T}"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars.</param>
        /// <param name="sorting">A comparer used to sort the bars.</param>
        /// <param name="getBaseline">A function that returns the bottom for each bar. This function should accept
        /// a single parameter of type <typeparamref name="T"/> and return another <typeparamref name="T"/>
        /// object, representing the bottom of the bar in data space.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public CategoricalBars(IEnumerable<(T, double)> data, IComparer<(T, double)> sorting, Func<(T, double), (T, double)> getBaseline, ICoordinateSystem<(T, double)> coordinateSystem) : base(data, sorting, getBaseline, coordinateSystem) { }

        /// <summary>
        /// Create a new <see cref="CategoricalBars{T}"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars.</param>
        /// <param name="sorting">A comparer used to sort the bars.</param>
        /// <param name="getBaseline">A function that returns the bottom for each bar. This function should accept
        /// a single parameter of type <typeparamref name="T"/> and return another <typeparamref name="T"/>
        /// object, representing the bottom of the bar in data space.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public CategoricalBars(IEnumerable<(T, double)> data, Comparison<(T, double)> sorting, Func<(T, double), (T, double)> getBaseline, ICoordinateSystem<(T, double)> coordinateSystem) : base(data, sorting, getBaseline, coordinateSystem) { }

        /// <summary>
        /// Create a new <see cref="Bars{T}"/> instance. The baseline for each <c>(<typeparamref name="T"/> x, <see langword="double"/> y)</c> is determined automatically as <c>(x, 0)</c>.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars.</param>
        /// <param name="sorting">A comparer used to sort the bars.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public CategoricalBars(IEnumerable<(T, double)> data, Comparison<(T, double)> sorting, ICoordinateSystem<(T, double)> coordinateSystem) : base(data, sorting, x => (x.Item1, 0), coordinateSystem) { }

        /// <summary>
        /// Create a new <see cref="Bars{T}"/> instance. The baseline for each <c>(<typeparamref name="T"/> x, <see langword="double"/> y)</c> is determined automatically as <c>(x, 0)</c>.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars. These should already be sorted.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public CategoricalBars(IReadOnlyList<(T, double)> data, ICoordinateSystem<(T, double)> coordinateSystem) : base(data, x => (x.Item1, 0), coordinateSystem) { }
    }

    /// <summary>
    /// A plot element that draws bars for numerical data.
    /// </summary>
    public class Bars : Bars<IReadOnlyList<double>>
    {
        /// <summary>
        /// Create a new <see cref="Bars"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars.</param>
        /// <param name="sorting">A comparer used to sort the bars.</param>
        /// <param name="getBaseline">A function that returns the bottom for each bar. This function should accept
        /// a single parameter (an <see cref="IReadOnlyList{T}"/> of <see langword="double"/>s), and return another
        /// object of the same type, representing the bottom of the bar in data space.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Bars(IEnumerable<IReadOnlyList<double>> data, IComparer<IReadOnlyList<double>> sorting, Func<IReadOnlyList<double>, IReadOnlyList<double>> getBaseline, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem) : base(data, sorting, getBaseline, coordinateSystem) { }

        /// <summary>
        /// Create a new <see cref="Bars"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars.</param>
        /// <param name="sorting">A comparer used to sort the bars.</param>
        /// <param name="getBaseline">A function that returns the bottom for each bar. This function should accept
        /// a single parameter (an <see cref="IReadOnlyList{T}"/> of <see langword="double"/>s), and return another
        /// object of the same type, representing the bottom of the bar in data space.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public Bars(IEnumerable<IReadOnlyList<double>> data, Comparison<IReadOnlyList<double>> sorting, Func<IReadOnlyList<double>, IReadOnlyList<double>> getBaseline, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem) : base(data, sorting, getBaseline, coordinateSystem) { }

        /// <summary>
        /// Create a new <see cref="Bars"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="vertical">If this is <see langword="true"/> (the default), the bars rise vertically above the X axis
        /// Otherwise, the bars grow horizontally from the Y axis.</param>
        public Bars(IEnumerable<IReadOnlyList<double>> data, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem, bool vertical = true) : base(data, vertical ? Comparer<IReadOnlyList<double>>.Create((x, y) => Math.Sign(x[0] - y[0])) : Comparer<IReadOnlyList<double>>.Create((x, y) => Math.Sign(x[1] - y[1])), vertical ? new Func<IReadOnlyList<double>, IReadOnlyList<double>>(x => { double[] tbr = x.ToArray(); tbr[1] = 0; return tbr; }) : new Func<IReadOnlyList<double>, IReadOnlyList<double>>(x => { double[] tbr = x.ToArray(); tbr[0] = 0; return tbr; }), coordinateSystem) { }
    }

    /// <summary>
    /// A plot element that draws stacked bars.
    /// </summary>
    public class StackedBars : IPlotElement
    {
        private double margin = 0;

        /// <summary>
        /// If this is <see langword="true"/>, the bars rise vertically above the X axis
        /// Otherwise, the bars grow horizontally from the Y axis.
        /// </summary>
        public bool Vertical { get; set; } = true;

        /// <summary>
        /// The data points corresponding to the tips of the bars. For each bar stack, the data
        /// point contains an element determining the position of the bar on the X axis (if <see cref="Vertical"/>
        /// is <see langword="true"/>, or on the Y axis otherwise), and a set of elements determining the
        /// length of each segment in the bar stack.
        /// </summary>
        public SortedSet<IReadOnlyList<double>> Data { get; set; }

        /// <summary>
        /// A function that returns the bottom for each bar. This function should accept
        /// a single parameter (an <see cref="IReadOnlyList{T}"/> of <see langword="double"/>s), and return another
        /// object of the same type, representing the bottom of the bar in data space.
        /// </summary>
        public Func<IReadOnlyList<double>, IReadOnlyList<double>> GetBaseline { get; set; }

        /// <summary>
        /// The margin between consecutive bars. This should range between 0 and 1.
        /// </summary>
        public double Margin
        {
            get => margin;
            set
            {
                if (value >= 0 && value <= 1)
                {
                    margin = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The value for the margin must be within 0 and 1 (inclusive)!");
                }
            }
        }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<IReadOnlyList<double>> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes for the bars. An element from this collection is used for each segment in
        /// the stack; if there are more segments than elements in this collection, the presentation attributes
        /// are wrapped.
        /// </summary>
        public IReadOnlyList<PlotElementPresentationAttributes> PresentationAttributes { get; set; } = new PlotElementPresentationAttributes[] { new PlotElementPresentationAttributes() };

        /// <summary>
        /// A tag to identify the stacked bars in the plot.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Create a new <see cref="StackedBars"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars. For each bar stack, the data
        /// point contains an element determining the position of the bar on the X axis (if <see cref="Vertical"/>
        /// is <see langword="true"/>, or on the Y axis otherwise), and a set of elements determining the
        /// length of each segment in the bar stack.</param>
        /// <param name="sorting">A comparer used to sort the bars.</param>
        /// <param name="getBaseline">A function that returns the bottom for each bar. This function should accept
        /// a single parameter (an <see cref="IReadOnlyList{T}"/> of <see langword="double"/>s), and return another
        /// object of the same type, representing the bottom of the bar in data space.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public StackedBars(IEnumerable<IReadOnlyList<double>> data, IComparer<IReadOnlyList<double>> sorting, Func<IReadOnlyList<double>, IReadOnlyList<double>> getBaseline, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem)
        {
            this.Data = new SortedSet<IReadOnlyList<double>>(data, sorting);
            this.CoordinateSystem = coordinateSystem;
            this.GetBaseline = getBaseline;
        }

        /// <summary>
        /// Create a new <see cref="StackedBars"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars. For each bar stack, the data
        /// point contains an element determining the position of the bar on the X axis (if <see cref="Vertical"/>
        /// is <see langword="true"/>, or on the Y axis otherwise), and a set of elements determining the
        /// length of each segment in the bar stack.</param>
        /// <param name="sorting">A comparer used to sort the bars.</param>
        /// <param name="getBaseline">A function that returns the bottom for each bar. This function should accept
        /// a single parameter (an <see cref="IReadOnlyList{T}"/> of <see langword="double"/>s), and return another
        /// object of the same type, representing the bottom of the bar in data space.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public StackedBars(IEnumerable<IReadOnlyList<double>> data, Comparison<IReadOnlyList<double>> sorting, Func<IReadOnlyList<double>, IReadOnlyList<double>> getBaseline, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem) : this(data, Comparer<IReadOnlyList<double>>.Create(sorting), getBaseline, coordinateSystem) { }

        /// <summary>
        /// Create a new <see cref="StackedBars"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars. For each bar stack, the data
        /// point contains an element determining the position of the bar on the X axis (if <paramref name="vertical"/>
        /// is <see langword="true"/>, or on the Y axis otherwise), and a set of elements determining the
        /// length of each segment in the bar stack.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="vertical">If this is <see langword="true"/> (the default), the bars rise vertically above the X axis
        /// Otherwise, the bars grow horizontally from the Y axis.</param>
        public StackedBars(IEnumerable<IReadOnlyList<double>> data, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem, bool vertical = true) : this(data, vertical ? Comparer<IReadOnlyList<double>>.Create((x, y) => Math.Sign(x[0] - y[0])) : Comparer<IReadOnlyList<double>>.Create((x, y) => Math.Sign(x[1] - y[1])), vertical ? new Func<IReadOnlyList<double>, IReadOnlyList<double>>(x => { double[] tbr = x.ToArray(); tbr[1] = 0; return tbr; }) : new Func<IReadOnlyList<double>, IReadOnlyList<double>>(x => { double[] tbr = x.ToArray(); tbr[0] = 0; return tbr; }), coordinateSystem) { Vertical = vertical; }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            List<(Point, Point)> bars = new List<(Point, Point)>();
            List<double[]> ratios = new List<double[]>();

            foreach (IReadOnlyList<double> data in Data)
            {
                Point baseline = CoordinateSystem.ToPlotCoordinates(GetBaseline(data));

                double total = 0;

                double[] currRatios = new double[data.Count - 1];

                for (int i = 0; i < data.Count - 1; i++)
                {
                    if (i == 0)
                    {
                        total += Vertical ? data[1] : data[0];
                    }
                    else
                    {
                        total += data[i + 1];
                    }

                    Point currPt = CoordinateSystem.ToPlotCoordinates(Vertical ? new double[] { data[0], total } : new double[] { total, data[1] });

                    currRatios[i] = new Point(currPt.X - baseline.X, currPt.Y - baseline.Y).Modulus();
                }

                for (int i = 0; i < currRatios.Length; i++)
                {
                    currRatios[i] /= currRatios[currRatios.Length - 1];
                }

                Point pt = CoordinateSystem.ToPlotCoordinates(Vertical ? new double[] { data[0], total } : new double[] { total, data[1] });

                bars.Add((baseline, pt));
                ratios.Add(currRatios);
            }

            for (int i = 0; i < bars.Count; i++)
            {
                List<GraphicsPath> paths = new List<GraphicsPath>();

                if (i > 0 && i < bars.Count - 1)
                {
                    Point prevBaselineMid = new Point(bars[i].Item1.X * (0.5 + Margin * 0.5) + bars[i - 1].Item1.X * (0.5 - Margin * 0.5), bars[i].Item1.Y * (0.5 + Margin * 0.5) + bars[i - 1].Item1.Y * (0.5 - Margin * 0.5));

                    Point perpDir = new Point(bars[i].Item2.Y - bars[i].Item1.Y, -(bars[i].Item2.X - bars[i].Item1.X));
                    perpDir = perpDir.Normalize();

                    double t = (bars[i].Item2.X - prevBaselineMid.X) * perpDir.X + (bars[i].Item2.Y - prevBaselineMid.Y) * perpDir.Y;
                    Point prevTop = new Point(bars[i].Item2.X - perpDir.X * t, bars[i].Item2.Y - perpDir.Y * t);

                    Point nextBaselineMid = new Point(bars[i].Item1.X * (0.5 + Margin * 0.5) + bars[i + 1].Item1.X * (0.5 - Margin * 0.5), bars[i].Item1.Y * (0.5 + Margin * 0.5) + bars[i + 1].Item1.Y * (0.5 - Margin * 0.5));

                    double t2 = (bars[i].Item2.X - nextBaselineMid.X) * perpDir.X + (bars[i].Item2.Y - nextBaselineMid.Y) * perpDir.Y;
                    Point nextTop = new Point(bars[i].Item2.X - perpDir.X * t2, bars[i].Item2.Y - perpDir.Y * t2);

                    Point lastBaseLeft = prevBaselineMid;
                    Point lastBaseRight = nextBaselineMid;

                    for (int j = 0; j < ratios[i].Length; j++)
                    {
                        Point pTop = new Point(prevBaselineMid.X + (prevTop.X - prevBaselineMid.X) * ratios[i][j], prevBaselineMid.Y + (prevTop.Y - prevBaselineMid.Y) * ratios[i][j]);
                        Point nTop = new Point(nextBaselineMid.X + (nextTop.X - nextBaselineMid.X) * ratios[i][j], nextBaselineMid.Y + (nextTop.Y - nextBaselineMid.Y) * ratios[i][j]);

                        paths.Add(new GraphicsPath().MoveTo(lastBaseLeft).LineTo(pTop).LineTo(nTop).LineTo(lastBaseRight).Close());

                        lastBaseLeft = pTop;
                        lastBaseRight = nTop;
                    }
                }
                else if (i == 0 && bars.Count == 1)
                {
                    Point perpDir = new Point(bars[i].Item2.Y - bars[i].Item1.Y, -(bars[i].Item2.X - bars[i].Item1.X));
                    perpDir = perpDir.Normalize();

                    Point nextPoint = CoordinateSystem.ToPlotCoordinates(Vertical ? new double[] { Data.First()[0] + 1, 0 } : new double[] { 0, Data.First()[1] + 1 });

                    Point nextBaselineMid = new Point(bars[i].Item1.X * (0.5 + Margin * 0.5) + nextPoint.X * (0.5 - Margin * 0.5), bars[i].Item1.Y * (0.5 + Margin * 0.5) + nextPoint.Y * (0.5 - Margin * 0.5));

                    double t2 = (bars[i].Item2.X - nextBaselineMid.X) * perpDir.X + (bars[i].Item2.Y - nextBaselineMid.Y) * perpDir.Y;
                    Point nextTop = new Point(bars[i].Item2.X - perpDir.X * t2, bars[i].Item2.Y - perpDir.Y * t2);

                    Point prevBaselineMid = new Point(2 * bars[i].Item1.X - nextBaselineMid.X, 2 * bars[i].Item1.Y - nextBaselineMid.Y);
                    double t = -t2;
                    Point prevTop = new Point(bars[i].Item2.X - perpDir.X * t, bars[i].Item2.Y - perpDir.Y * t);

                    Point lastBaseLeft = prevBaselineMid;
                    Point lastBaseRight = nextBaselineMid;

                    for (int j = 0; j < ratios[i].Length; j++)
                    {
                        Point pTop = new Point(prevBaselineMid.X + (prevTop.X - prevBaselineMid.X) * ratios[i][j], prevBaselineMid.Y + (prevTop.Y - prevBaselineMid.Y) * ratios[i][j]);
                        Point nTop = new Point(nextBaselineMid.X + (nextTop.X - nextBaselineMid.X) * ratios[i][j], nextBaselineMid.Y + (nextTop.Y - nextBaselineMid.Y) * ratios[i][j]);

                        paths.Add(new GraphicsPath().MoveTo(lastBaseLeft).LineTo(pTop).LineTo(nTop).LineTo(lastBaseRight).Close());

                        lastBaseLeft = pTop;
                        lastBaseRight = nTop;
                    }
                }
                else if (i == 0)
                {
                    Point perpDir = new Point(bars[i].Item2.Y - bars[i].Item1.Y, -(bars[i].Item2.X - bars[i].Item1.X));
                    perpDir = perpDir.Normalize();

                    Point nextBaselineMid = new Point(bars[i].Item1.X * (0.5 + Margin * 0.5) + bars[i + 1].Item1.X * (0.5 - Margin * 0.5), bars[i].Item1.Y * (0.5 + Margin * 0.5) + bars[i + 1].Item1.Y * (0.5 - Margin * 0.5));

                    double t2 = (bars[i].Item2.X - nextBaselineMid.X) * perpDir.X + (bars[i].Item2.Y - nextBaselineMid.Y) * perpDir.Y;
                    Point nextTop = new Point(bars[i].Item2.X - perpDir.X * t2, bars[i].Item2.Y - perpDir.Y * t2);


                    Point prevBaselineMid = new Point(2 * bars[i].Item1.X - nextBaselineMid.X, 2 * bars[i].Item1.Y - nextBaselineMid.Y);
                    double t = -t2;
                    Point prevTop = new Point(bars[i].Item2.X - perpDir.X * t, bars[i].Item2.Y - perpDir.Y * t);

                    Point lastBaseLeft = prevBaselineMid;
                    Point lastBaseRight = nextBaselineMid;

                    for (int j = 0; j < ratios[i].Length; j++)
                    {
                        Point pTop = new Point(prevBaselineMid.X + (prevTop.X - prevBaselineMid.X) * ratios[i][j], prevBaselineMid.Y + (prevTop.Y - prevBaselineMid.Y) * ratios[i][j]);
                        Point nTop = new Point(nextBaselineMid.X + (nextTop.X - nextBaselineMid.X) * ratios[i][j], nextBaselineMid.Y + (nextTop.Y - nextBaselineMid.Y) * ratios[i][j]);

                        paths.Add(new GraphicsPath().MoveTo(lastBaseLeft).LineTo(pTop).LineTo(nTop).LineTo(lastBaseRight).Close());

                        lastBaseLeft = pTop;
                        lastBaseRight = nTop;
                    }
                }
                else if (i == bars.Count - 1)
                {
                    Point prevBaselineMid = new Point(bars[i].Item1.X * (0.5 + Margin * 0.5) + bars[i - 1].Item1.X * (0.5 - Margin * 0.5), bars[i].Item1.Y * (0.5 + Margin * 0.5) + bars[i - 1].Item1.Y * (0.5 - Margin * 0.5));

                    Point perpDir = new Point(bars[i].Item2.Y - bars[i].Item1.Y, -(bars[i].Item2.X - bars[i].Item1.X));
                    perpDir = perpDir.Normalize();

                    double t = (bars[i].Item2.X - prevBaselineMid.X) * perpDir.X + (bars[i].Item2.Y - prevBaselineMid.Y) * perpDir.Y;
                    Point prevTop = new Point(bars[i].Item2.X - perpDir.X * t, bars[i].Item2.Y - perpDir.Y * t);

                    Point nextBaselineMid = new Point(2 * bars[i].Item1.X - prevBaselineMid.X, 2 * bars[i].Item1.Y - prevBaselineMid.Y);

                    double t2 = -t;
                    Point nextTop = new Point(bars[i].Item2.X - perpDir.X * t2, bars[i].Item2.Y - perpDir.Y * t2);

                    Point lastBaseLeft = prevBaselineMid;
                    Point lastBaseRight = nextBaselineMid;

                    for (int j = 0; j < ratios[i].Length; j++)
                    {
                        Point pTop = new Point(prevBaselineMid.X + (prevTop.X - prevBaselineMid.X) * ratios[i][j], prevBaselineMid.Y + (prevTop.Y - prevBaselineMid.Y) * ratios[i][j]);
                        Point nTop = new Point(nextBaselineMid.X + (nextTop.X - nextBaselineMid.X) * ratios[i][j], nextBaselineMid.Y + (nextTop.Y - nextBaselineMid.Y) * ratios[i][j]);

                        paths.Add(new GraphicsPath().MoveTo(lastBaseLeft).LineTo(pTop).LineTo(nTop).LineTo(lastBaseRight).Close());

                        lastBaseLeft = pTop;
                        lastBaseRight = nTop;
                    }
                }

                for (int j = 0; j < paths.Count; j++)
                {
                    string tag = Tag;
                    string strokeTag = Tag;

                    if (!string.IsNullOrEmpty(Tag))
                    {
                        tag = tag + "@" + i.ToString() + "/" + j.ToString();
                        strokeTag = tag + "@" + i.ToString() + "/" + j.ToString() + "_stroke";
                    }

                    int colourIndex = j % PresentationAttributes.Count;

                    if (PresentationAttributes[colourIndex].Fill != null)
                    {
                        target.FillPath(paths[j], PresentationAttributes[colourIndex].Fill, tag);
                    }

                    if (PresentationAttributes[colourIndex].Stroke != null)
                    {
                        target.StrokePath(paths[j], PresentationAttributes[colourIndex].Stroke, PresentationAttributes[colourIndex].LineWidth, PresentationAttributes[colourIndex].LineCap, PresentationAttributes[colourIndex].LineJoin, PresentationAttributes[colourIndex].LineDash, strokeTag);
                    }
                }
            }
        }
    }

    /// <summary>
    /// A plot element that draws clusters of bars.
    /// </summary>
    public class ClusteredBars : IPlotElement
    {
        private double interClusterMargin = 0;
        private double intraClusterMargin = 0;

        /// <summary>
        /// If this is <see langword="true"/>, the bars rise vertically above the X axis
        /// Otherwise, the bars grow horizontally from the Y axis.
        /// </summary>
        public bool Vertical { get; set; } = true;


        /// <summary>
        /// The data points corresponding to the tips of the bars. For each bar cluster, the data
        /// point contains an element determining the position of the cluster on the X axis (if <see cref="Vertical"/>
        /// is <see langword="true"/>, or on the Y axis otherwise), and a set of elements determining the
        /// length of each bar in the cluster.
        /// </summary>
        public SortedSet<IReadOnlyList<double>> Data { get; set; }

        /// <summary>
        /// A function that returns the bottom for each bar cluster. This function should accept
        /// a single parameter (an <see cref="IReadOnlyList{T}"/> of <see langword="double"/>s), and return another
        /// object of the same type, representing the bottom of the cluster in data space.
        /// </summary>
        public Func<IReadOnlyList<double>, IReadOnlyList<double>> GetBaseline { get; set; }

        /// <summary>
        /// The margin between consecutive bar clusters.
        /// </summary>
        public double InterClusterMargin
        {
            get => interClusterMargin;
            set
            {
                if (value >= 0 && value <= 1)
                {
                    interClusterMargin = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The value for the margin must be within 0 and 1 (inclusive)!");
                }
            }
        }

        /// <summary>
        /// The margin between consecutive bars within a single cluster.
        /// </summary>
        public double IntraClusterMargin
        {
            get => intraClusterMargin;
            set
            {
                if (value >= 0 && value <= 1)
                {
                    intraClusterMargin = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The value for the margin must be within 0 and 1 (inclusive)!");
                }
            }
        }

        /// <summary>
        /// The coordinate system used to transform the points from data space to plot space.
        /// </summary>
        public ICoordinateSystem<IReadOnlyList<double>> CoordinateSystem { get; set; }
        ICoordinateSystem IPlotElement.CoordinateSystem => CoordinateSystem;

        /// <summary>
        /// Presentation attributes for the bars. An element from this collection is used for each bar in
        /// the cluster; if there are more bars than elements in this collection, the presentation attributes
        /// are wrapped.
        /// </summary>
        public IReadOnlyList<PlotElementPresentationAttributes> PresentationAttributes { get; set; } = new PlotElementPresentationAttributes[] { new PlotElementPresentationAttributes() };

        /// <summary>
        /// A tag to identify the clustered bars in the plot.
        /// </summary>
        public string Tag { get; set; }


        /// <summary>
        /// Create a new <see cref="ClusteredBars"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars. For each bar cluster, the data
        /// point contains an element determining the position of the cluster on the X axis (if <see cref="Vertical"/>
        /// is <see langword="true"/>, or on the Y axis otherwise), and a set of elements determining the
        /// length of each bar in the cluster.</param>
        /// <param name="sorting">A comparer used to sort the bar clusters.</param>
        /// <param name="getBaseline">A function that returns the bottom for each bar cluster. This function should accept
        /// a single parameter (an <see cref="IReadOnlyList{T}"/> of <see langword="double"/>s), and return another
        /// object of the same type, representing the bottom of the cluster in data space.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public ClusteredBars(IEnumerable<IReadOnlyList<double>> data, IComparer<IReadOnlyList<double>> sorting, Func<IReadOnlyList<double>, IReadOnlyList<double>> getBaseline, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem)
        {
            this.Data = new SortedSet<IReadOnlyList<double>>(data, sorting);
            this.CoordinateSystem = coordinateSystem;
            this.GetBaseline = getBaseline;
        }

        /// <summary>
        /// Create a new <see cref="ClusteredBars"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars. For each bar cluster, the data
        /// point contains an element determining the position of the cluster on the X axis (if <see cref="Vertical"/>
        /// is <see langword="true"/>, or on the Y axis otherwise), and a set of elements determining the
        /// length of each bar in the cluster.</param>
        /// <param name="sorting">A comparer used to sort the bar clusters.</param>
        /// <param name="getBaseline">A function that returns the bottom for each bar cluster. This function should accept
        /// a single parameter (an <see cref="IReadOnlyList{T}"/> of <see langword="double"/>s), and return another
        /// object of the same type, representing the bottom of the cluster in data space.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        public ClusteredBars(IEnumerable<IReadOnlyList<double>> data, Comparison<IReadOnlyList<double>> sorting, Func<IReadOnlyList<double>, IReadOnlyList<double>> getBaseline, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem) : this(data, Comparer<IReadOnlyList<double>>.Create(sorting), getBaseline, coordinateSystem) { }

        /// <summary>
        /// Create a new <see cref="ClusteredBars"/> instance.
        /// </summary>
        /// <param name="data">The data points corresponding to the tips of the bars. For each bar cluster, the data
        /// point contains an element determining the position of the cluster on the X axis (if <see cref="Vertical"/>
        /// is <see langword="true"/>, or on the Y axis otherwise), and a set of elements determining the
        /// length of each bar in the cluster.</param>
        /// <param name="coordinateSystem">The coordinate system used to transform the points from data space to plot space.</param>
        /// <param name="vertical">If this is <see langword="true"/> (the default), the bars rise vertically above the X axis
        /// Otherwise, the bars grow horizontally from the Y axis.</param>
        public ClusteredBars(IEnumerable<IReadOnlyList<double>> data, ICoordinateSystem<IReadOnlyList<double>> coordinateSystem, bool vertical = true) : this(data, vertical ? Comparer<IReadOnlyList<double>>.Create((x, y) => Math.Sign(x[0] - y[0])) : Comparer<IReadOnlyList<double>>.Create((x, y) => Math.Sign(x[1] - y[1])), vertical ? new Func<IReadOnlyList<double>, IReadOnlyList<double>>(x => { double[] tbr = x.ToArray(); tbr[1] = 0; return tbr; }) : new Func<IReadOnlyList<double>, IReadOnlyList<double>>(x => { double[] tbr = x.ToArray(); tbr[0] = 0; return tbr; }), coordinateSystem) { Vertical = vertical; }

        /// <inheritdoc/>
        public void Plot(Graphics target)
        {
            List<(Point, Point[])> bars = new List<(Point, Point[])>();

            foreach (IReadOnlyList<double> data in Data)
            {
                Point baseline = CoordinateSystem.ToPlotCoordinates(GetBaseline(data));

                Point[] points = new Point[data.Count - 1];

                for (int i = 0; i < data.Count - 1; i++)
                {
                    if (i == 0)
                    {
                        points[i] = CoordinateSystem.ToPlotCoordinates(data);
                    }
                    else
                    {
                        points[i] = CoordinateSystem.ToPlotCoordinates(Vertical ? new double[] { data[0], data[i + 1] } : new double[] { data[i + 1], data[1] });
                    }
                }

                bars.Add((baseline, points));
            }

            for (int i = 0; i < bars.Count; i++)
            {
                List<GraphicsPath> paths = new List<GraphicsPath>();

                if (i > 0 && i < bars.Count - 1)
                {
                    Point prevBaselineMid = new Point(bars[i].Item1.X * (0.5 + InterClusterMargin * 0.5) + bars[i - 1].Item1.X * (0.5 - InterClusterMargin * 0.5), bars[i].Item1.Y * (0.5 + InterClusterMargin * 0.5) + bars[i - 1].Item1.Y * (0.5 - InterClusterMargin * 0.5));
                    Point nextBaselineMid = new Point(bars[i].Item1.X * (0.5 + InterClusterMargin * 0.5) + bars[i + 1].Item1.X * (0.5 - InterClusterMargin * 0.5), bars[i].Item1.Y * (0.5 + InterClusterMargin * 0.5) + bars[i + 1].Item1.Y * (0.5 - InterClusterMargin * 0.5));

                    for (int j = 0; j < bars[i].Item2.Length; j++)
                    {
                        Point perpDir = new Point(bars[i].Item2[j].Y - bars[i].Item1.Y, -(bars[i].Item2[j].X - bars[i].Item1.X));
                        perpDir = perpDir.Normalize();

                        double t = (bars[i].Item2[j].X - prevBaselineMid.X) * perpDir.X + (bars[i].Item2[j].Y - prevBaselineMid.Y) * perpDir.Y;
                        Point prevTop = new Point(bars[i].Item2[j].X - perpDir.X * t, bars[i].Item2[j].Y - perpDir.Y * t);

                        double t2 = (bars[i].Item2[j].X - nextBaselineMid.X) * perpDir.X + (bars[i].Item2[j].Y - nextBaselineMid.Y) * perpDir.Y;
                        Point nextTop = new Point(bars[i].Item2[j].X - perpDir.X * t2, bars[i].Item2[j].Y - perpDir.Y * t2);

                        double start = (double)(j + IntraClusterMargin * 0.5) / bars[i].Item2.Length;
                        double end = (double)(j + 1 - IntraClusterMargin * 0.5) / bars[i].Item2.Length;

                        paths.Add(new GraphicsPath().MoveTo(new Point(prevBaselineMid.X + (nextBaselineMid.X - prevBaselineMid.X) * start, prevBaselineMid.Y + (nextBaselineMid.Y - prevBaselineMid.Y) * start))
                            .LineTo(new Point(prevTop.X + (nextTop.X - prevTop.X) * start, prevTop.Y + (nextTop.Y - prevTop.Y) * start))
                            .LineTo(new Point(prevTop.X + (nextTop.X - prevTop.X) * end, prevTop.Y + (nextTop.Y - prevTop.Y) * end))
                            .LineTo(new Point(prevBaselineMid.X + (nextBaselineMid.X - prevBaselineMid.X) * end, prevBaselineMid.Y + (nextBaselineMid.Y - prevBaselineMid.Y) * end)).Close());
                    }
                }
                else if (i == 0 && i == bars.Count - 1)
                {
                    Point nextPoint = CoordinateSystem.ToPlotCoordinates(Vertical ? new double[] { Data.First()[0] + 1, 0 } : new double[] { 0, Data.First()[1] + 1 });

                    Point nextBaselineMid = new Point(bars[i].Item1.X * (0.5 + InterClusterMargin * 0.5) + nextPoint.X * (0.5 - InterClusterMargin * 0.5), bars[i].Item1.Y * (0.5 + InterClusterMargin * 0.5) + nextPoint.Y * (0.5 - InterClusterMargin * 0.5));

                    for (int j = 0; j < bars[i].Item2.Length; j++)
                    {
                        Point perpDir = new Point(bars[i].Item2[j].Y - bars[i].Item1.Y, -(bars[i].Item2[j].X - bars[i].Item1.X));
                        perpDir = perpDir.Normalize();

                        double t2 = (bars[i].Item2[j].X - nextBaselineMid.X) * perpDir.X + (bars[i].Item2[j].Y - nextBaselineMid.Y) * perpDir.Y;
                        Point nextTop = new Point(bars[i].Item2[j].X - perpDir.X * t2, bars[i].Item2[j].Y - perpDir.Y * t2);

                        Point prevBaselineMid = new Point(2 * bars[i].Item1.X - nextBaselineMid.X, 2 * bars[i].Item1.Y - nextBaselineMid.Y);
                        double t = -t2;
                        Point prevTop = new Point(bars[i].Item2[j].X - perpDir.X * t, bars[i].Item2[j].Y - perpDir.Y * t);

                        double start = (double)(j + IntraClusterMargin * 0.5) / bars[i].Item2.Length;
                        double end = (double)(j + 1 - IntraClusterMargin * 0.5) / bars[i].Item2.Length;

                        paths.Add(new GraphicsPath().MoveTo(new Point(prevBaselineMid.X + (nextBaselineMid.X - prevBaselineMid.X) * start, prevBaselineMid.Y + (nextBaselineMid.Y - prevBaselineMid.Y) * start))
                            .LineTo(new Point(prevTop.X + (nextTop.X - prevTop.X) * start, prevTop.Y + (nextTop.Y - prevTop.Y) * start))
                            .LineTo(new Point(prevTop.X + (nextTop.X - prevTop.X) * end, prevTop.Y + (nextTop.Y - prevTop.Y) * end))
                            .LineTo(new Point(prevBaselineMid.X + (nextBaselineMid.X - prevBaselineMid.X) * end, prevBaselineMid.Y + (nextBaselineMid.Y - prevBaselineMid.Y) * end)).Close());
                    }
                }
                else if (i == 0)
                {
                    Point nextBaselineMid = new Point(bars[i].Item1.X * (0.5 + InterClusterMargin * 0.5) + bars[i + 1].Item1.X * (0.5 - InterClusterMargin * 0.5), bars[i].Item1.Y * (0.5 + InterClusterMargin * 0.5) + bars[i + 1].Item1.Y * (0.5 - InterClusterMargin * 0.5));

                    for (int j = 0; j < bars[i].Item2.Length; j++)
                    {
                        Point perpDir = new Point(bars[i].Item2[j].Y - bars[i].Item1.Y, -(bars[i].Item2[j].X - bars[i].Item1.X));
                        perpDir = perpDir.Normalize();

                        double t2 = (bars[i].Item2[j].X - nextBaselineMid.X) * perpDir.X + (bars[i].Item2[j].Y - nextBaselineMid.Y) * perpDir.Y;
                        Point nextTop = new Point(bars[i].Item2[j].X - perpDir.X * t2, bars[i].Item2[j].Y - perpDir.Y * t2);

                        Point prevBaselineMid = new Point(2 * bars[i].Item1.X - nextBaselineMid.X, 2 * bars[i].Item1.Y - nextBaselineMid.Y);
                        double t = -t2;
                        Point prevTop = new Point(bars[i].Item2[j].X - perpDir.X * t, bars[i].Item2[j].Y - perpDir.Y * t);

                        double start = (double)(j + IntraClusterMargin * 0.5) / bars[i].Item2.Length;
                        double end = (double)(j + 1 - IntraClusterMargin * 0.5) / bars[i].Item2.Length;

                        paths.Add(new GraphicsPath().MoveTo(new Point(prevBaselineMid.X + (nextBaselineMid.X - prevBaselineMid.X) * start, prevBaselineMid.Y + (nextBaselineMid.Y - prevBaselineMid.Y) * start))
                            .LineTo(new Point(prevTop.X + (nextTop.X - prevTop.X) * start, prevTop.Y + (nextTop.Y - prevTop.Y) * start))
                            .LineTo(new Point(prevTop.X + (nextTop.X - prevTop.X) * end, prevTop.Y + (nextTop.Y - prevTop.Y) * end))
                            .LineTo(new Point(prevBaselineMid.X + (nextBaselineMid.X - prevBaselineMid.X) * end, prevBaselineMid.Y + (nextBaselineMid.Y - prevBaselineMid.Y) * end)).Close());
                    }
                }
                else if (i == bars.Count - 1)
                {
                    Point prevBaselineMid = new Point(bars[i].Item1.X * (0.5 + InterClusterMargin * 0.5) + bars[i - 1].Item1.X * (0.5 - InterClusterMargin * 0.5), bars[i].Item1.Y * (0.5 + InterClusterMargin * 0.5) + bars[i - 1].Item1.Y * (0.5 - InterClusterMargin * 0.5));

                    for (int j = 0; j < bars[i].Item2.Length; j++)
                    {
                        Point perpDir = new Point(bars[i].Item2[j].Y - bars[i].Item1.Y, -(bars[i].Item2[j].X - bars[i].Item1.X));
                        perpDir = perpDir.Normalize();

                        double t = (bars[i].Item2[j].X - prevBaselineMid.X) * perpDir.X + (bars[i].Item2[j].Y - prevBaselineMid.Y) * perpDir.Y;
                        Point prevTop = new Point(bars[i].Item2[j].X - perpDir.X * t, bars[i].Item2[j].Y - perpDir.Y * t);

                        Point nextBaselineMid = new Point(2 * bars[i].Item1.X - prevBaselineMid.X, 2 * bars[i].Item1.Y - prevBaselineMid.Y);

                        double t2 = -t;
                        Point nextTop = new Point(bars[i].Item2[j].X - perpDir.X * t2, bars[i].Item2[j].Y - perpDir.Y * t2);

                        double start = (double)(j + IntraClusterMargin * 0.5) / bars[i].Item2.Length;
                        double end = (double)(j + 1 - IntraClusterMargin * 0.5) / bars[i].Item2.Length;

                        paths.Add(new GraphicsPath().MoveTo(new Point(prevBaselineMid.X + (nextBaselineMid.X - prevBaselineMid.X) * start, prevBaselineMid.Y + (nextBaselineMid.Y - prevBaselineMid.Y) * start))
                            .LineTo(new Point(prevTop.X + (nextTop.X - prevTop.X) * start, prevTop.Y + (nextTop.Y - prevTop.Y) * start))
                            .LineTo(new Point(prevTop.X + (nextTop.X - prevTop.X) * end, prevTop.Y + (nextTop.Y - prevTop.Y) * end))
                            .LineTo(new Point(prevBaselineMid.X + (nextBaselineMid.X - prevBaselineMid.X) * end, prevBaselineMid.Y + (nextBaselineMid.Y - prevBaselineMid.Y) * end)).Close());
                    }
                }

                for (int j = 0; j < paths.Count; j++)
                {
                    string tag = Tag;
                    string strokeTag = Tag;

                    if (!string.IsNullOrEmpty(Tag))
                    {
                        tag = tag + "@" + i.ToString() + "/" + j.ToString();
                        strokeTag = tag + "@" + i.ToString() + "/" + j.ToString() + "_stroke";
                    }

                    int colourIndex = j % PresentationAttributes.Count;

                    if (PresentationAttributes[colourIndex].Fill != null)
                    {
                        target.FillPath(paths[j], PresentationAttributes[colourIndex].Fill, tag);
                    }

                    if (PresentationAttributes[colourIndex].Stroke != null)
                    {
                        target.StrokePath(paths[j], PresentationAttributes[colourIndex].Stroke, PresentationAttributes[colourIndex].LineWidth, PresentationAttributes[colourIndex].LineCap, PresentationAttributes[colourIndex].LineJoin, PresentationAttributes[colourIndex].LineDash, strokeTag);
                    }
                }
            }
        }
    }
}
