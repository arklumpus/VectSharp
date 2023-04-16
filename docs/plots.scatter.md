---
layout: default
nav_order: 1
parent: Plots
---

# Scatter plots

A scatter plot is conceptually one of the simplest kinds of plots that can be produced with VectSharp.Plots. In a scatter plot, the data point are transformed into plot coordinates, and a "symbol" of some kind (e.g., a circle) is drawn at each point. A scatter plot can be produced using the `Plot.Create.ScatterPlot` method; creating a scatter plot can be as simple as collecting some data and invoking this method:

<div class="code-example">
    <iframe src="assets/images/plots/scatter.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create some random data.
Random rnd = new Random();
(double, double)[] data = (from el in Enumerable.Range(0, 100) select (el + rnd.NextDouble(), 2 * el + 100 * rnd.NextDouble())).ToArray();

// Create the scatter plot using the random data and the default settings.
Plot plot = Plot.Create.ScatterPlot(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"scatter.svg");
{% endhighlight %}

This method has a few overloads, which make it possible to use different kinds of data (e.g., data stored as a `double[][]`, `Point[]`, `(double, double)[]`). Each overload has a required parameter (the data to plot), and a number of optional parameters can be used to determine the appearance of some elements of the plot. Many of these are in common with other kinds of plots:

* `double width`: the width of the plot. Ignored if a custom `coordinateSystem` is supplied.
* `double height`: the height of the plot. Ignored if a custom `coordinateSystem` is supplied.
* `PlotElementPresentationAttributes axisPresentationAttributes`: the presentation attributes for the axes.
* `double axisArrowSize`: the size for the arrows at the end of the axes.
* `PlotElementPresentationAttributes axisLabelPresentationAttributes`: the presentation attributes for the labels on the axes.
* `PlotElementPresentationAttributes axisTitlePresentationAttributes`: the presentation attributes for the axis titles.
* `string xAxisTitle`: the title for the horizontal axis.
* `string yAxisTitle`: the title for the vertical axis.
* `string title`: the title for the plot.
* `PlotElementPresentationAttributes titlePresentationAttributes`: the presentation attributes for the title of the plot.
* `PlotElementPresentationAttributes gridPresentationAttributes`: the presentation attributes for the grid behind the data points.
* `PlotElementPresentationAttributes dataPresentationAttributes`: the presentation attributes for the data points.
* `double pointSize`: the size of the data points.
* `IDataPointElement dataPointElement`: symbol used for each point (see the page describing the `ScatterPoints` plot element for more details).
* `IContinuousInvertibleCoordinateSystem coordinateSystem`: the coordinate system used to get the point coordinates (if this is not supplied, a linear coordinate system is used).

## Plotting multiple sets of points

The overloads of this method that take data as a `IReadOnlyList<IReadOnlyList<IReadOnlyList<double>>>` (i.e., `double[][][]`) or as a `IReadOnlyList<IReadOnlyList<(double, double)>>` (i.e., `(double, double)[][]`) can be used to plot multiple sets of data in the same plot with different appearances (e.g., colours, sizes, symbols, etc.). Some parameters are different for this methods:

* `IReadOnlyList<PlotElementPresentationAttributes> dataPresentationAttributes`
* `IReadOnlyList<double> pointSizes`
* `IReadOnlyList<IDataPointElement> dataPointElements`

These are specified as lists (or arrays) instead of individual values, which makes it possible to specify a different value for each data set. If these arrays are shorter than the number of data sets, the values are wrapped (e.g., if only two `pointSizes` are specified and 3 data sets are being plotted, the third data set will use the same point size as the first data set).

The following example shows the effects of some of these settings:

<div class="code-example">
    <iframe src="Blazor?scatter" style="width: 100%; height: 58em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create some random data.
Random rnd = new Random();
(double, double)[] data1 = (from el in Enumerable.Range(1, 100) select (el + rnd.NextDouble(), 2 * el + 100 * rnd.NextDouble())).ToArray();
(double, double)[] data2 = (from el in Enumerable.Range(1, 70) select (el + rnd.NextDouble(), Math.Exp(el * 0.1) * 0.25 * (1 + rnd.NextDouble()))).ToArray();

// Create a log-lin coordinate system.
IContinuousInvertibleCoordinateSystem logLinCoordinates = new LogLinCoordinateSystem2D(1, 100, 0.5, 650, 350, 250);

// Create the scatter plot using the random data.
Plot plot = Plot.Create.ScatterPlot(new[] { data1, data2 },
	xAxisTitle: "Horizontal axis",
	yAxisTitle: "Vertical axis",
	title: "Scatter plot",
	dataPresentationAttributes: new PlotElementPresentationAttributes[]
	{
		// First dataset: blue filled points.
		new PlotElementPresentationAttributes() { Stroke = null, Fill = Colour.FromRgb(0, 114, 178) },
		// Second dataset: orange empty points.
		new PlotElementPresentationAttributes() { Fill = null, Stroke = Colour.FromRgb(213, 94, 0), LineWidth = 0.5 }
    },
	dataPointElements: new IDataPointElement[]
	{
		// First dataset: circle (default)
		new PathDataPointElement(),
		// Second dataset: diamond
		new PathDataPointElement(new GraphicsPath().MoveTo(-1, 0).LineTo(0, 1).LineTo(1, 0).LineTo(0, -1).Close())
	},
    coordinateSystem: logLinCoordinates);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"scatter.svg");
{% endhighlight %}

## Adding trendlines

Additional elements can always be added to a `Plot`; one element that you might wish to add to a scatter plot is a trendline. This can be done by creating a trendline object and using the `Plot` object's `AddPlotElement` method to add the trendline to the plot. The following types of trendlines are currently implemented in VectSharp.Plots:

* `LinearTrendline`: a linear trendline ($y=a\cdot x+b$).
* `ExponentialTrendLine`: an exponential trendline ($y=b\cdot\mathrm{e}^{a\cdot x}$).
* `LogarithmicTrendLine`: a logarithmic trendline ($y=a\ln(x) + b$).
* `PolynomialTrendLine`: a polynomial trendline of the specified degree $n$ ($y=\sum_{i = 0}^na_ix^i$).
* `PowerLawTrendline`: a power law ($y = b\cdot x^a$).
* `MovingAverageTrendLine`: a moving average with the specified period and weight function.

A trendline can be created either by specifying the parameters, or by providing the constructor with some data from which the parameters can be estimated (see the page describing them for more details). The following example shows how to add an exponential trendline and a linear trendline to the previous plot. Note that in a lin-log coordinate system, the linear trendline is curved and the exponential trendline is a straight line.

<div class="code-example">
    <iframe src="assets/images/plots/scatter2.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
// ...

// Create the linear trendline, using the coordinate system from the plot.
LinearTrendLine linearTrendLine = new LinearTrendLine(data1, plot.GetFirst<IContinuousCoordinateSystem>());
plot.AddPlotElement(linearTrendLine);

// Create the exponential trendline, using the coordinate system from the plot.
ExponentialTrendLine exponentialTrendLine = new ExponentialTrendLine(data2, plot.GetFirst<IContinuousCoordinateSystem>());
plot.AddPlotElement(exponentialTrendLine);

// ...
{% endhighlight %}

For more information about trendlines, see the [page describing them]({{ site.baseurl }}{% link plots.elements.trendlines.md %}).