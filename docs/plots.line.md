---
layout: default
nav_order: 2
parent: Plots
---

# Line charts

A line chart consists of multiple data points that are connected by line segments. You can create a line chart using the `Plot.Create.LineChart` method:

<div class="code-example">
    <iframe src="assets/images/plots/line.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create some random data.
Random rnd = new Random();
(double, double)[] data = (from el in Enumerable.Range(0, 100) select (el + rnd.NextDouble(), 2 * el + 100 * rnd.NextDouble())).ToArray();

// Create the line chart using the random data and the default settings.
Plot plot = Plot.Create.LineChart(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"line.svg");
{% endhighlight %}

This method has a few overloads, differing mainly in the type of the first argument: the overload taking an `IReadOnlyList<(double, double)>` (i.e., the one used in the example above) represents each individual data point as a tuple of coordinates (the first element is the `X` coordinate and the second element is the `Y` coordinate). The overload taking an `IReadOnlyList<IReadOnlyList<double>>` represents each data point as an array of coordinates (again, the first element is the `X` coordinate and the second element is the `Y` coordinate). Finally, the overload taking an `IReadOnlyList<double>` assumes that the values provided are only the `Y` coordinates of the points, and that the points are equally spaced on the `X` axis (this is useful e.g. if you have data that has been sampled at regular intervals from something and you do not care about the `X` axis).

Optional parameters can be used to determine the appearance of some elements of the plot. Many of these are in common with other kinds of plots and are described in the page about [scatter plots]({{ site.baseurl }}{% link plots.scatter.md %}); here are the ones specific to line charts:

* `bool smooth`: by default, the points are connected by straight lines. If you set this to `true`, a smooth spline will be computed, which passes through all the data points.
* `PlotElementPresentationAttributes linePresentationAttributes`: the presentation attributes for the line.
* `PlotElementPresentationAttributes pointPresentationAttributes`: the presentation attributes for the data points.
* `double pointSize`: the size of the data points (by default, this is `0`, so that the data points are not drawn).
* `IDataPointElement dataPointElement`: symbol used for each point (see the page describing the `ScatterPoints` plot element for more details).

## Plotting multiple line charts

There are a few overloads of the `LineCharts` method (note the plural `s`), which can be used to draw multiple line charts on the same plot. Again, the different overloads can be use to plot data in different forms; there are also overloads taking exactly two collections of data points, which can be used if you need to plot just two lines. These overloads take very similar parameters to the `LineChart` methods, but have the following parameters specified as arrays instead of individual objects:

* `IReadOnlyList<PlotElementPresentationAttributes> linePresentationAttributess`
* `IReadOnlyList<PlotElementPresentationAttributes> pointPresentationAttributess`
* `IReadOnlyList<double> pointSizes`
* `IReadOnlyList<IDataPointElement> dataPointElements`

This makes it possible to specify a different value for each data set. If these arrays are shorter than the number of data sets, the values are wrapped (e.g., if only two `pointSizes` are specified and 3 data sets are being plotted, the third data set will use the same point size as the first data set).

The following example shows the effects of some of these settings:

<div class="code-example">
    <iframe src="Blazor?lines" style="width: 100%; height: 62em; border: 0px solid black"></iframe>
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

// Create the line charts using the random data.
Plot plot = Plot.Create.LineCharts(new[] { data1, data2 },
    xAxisTitle: "Horizontal axis",
    yAxisTitle: "Vertical axis",
    title: "Line chart",
    pointPresentationAttributes: new PlotElementPresentationAttributes[]
    {
		// First dataset: blue filled points.
		new PlotElementPresentationAttributes() { Stroke = null, Fill = Colour.FromRgb(0, 114, 178) },
		// Second dataset: orange empty points.
		new PlotElementPresentationAttributes() { Fill = Colours.White, Stroke = Colour.FromRgb(213, 94, 0), LineWidth = 0.5 }
    },
    dataPointElements: new IDataPointElement[]
    {
		// First dataset: circle (default)
		new PathDataPointElement(),
		// Second dataset: diamond
		new PathDataPointElement(new GraphicsPath().MoveTo(-1, 0).LineTo(0, 1).LineTo(1, 0).LineTo(0, -1).Close())
    },
    pointSizes: new double[] { 2, 2 },
    coordinateSystem: logLinCoordinates);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"line.svg");
{% endhighlight %}

As in a [scatter plot]({{ site.baseurl }}{% link plots.scatter.md %}), you can add additional elements to a line chart, e.g. a trendline.