---
layout: default
nav_order: 3
parent: Plots
---

# Area charts

Area charts are very similar to [line charts]({{ site.baseurl }}{% link plots.line.md %}), except that the area between the line and a "baseline" (e.g., the `X` axis) is shaded. You can create them using the `Plot.Create.AreaChart` method:

<div class="code-example">
    <iframe src="assets/images/plots/area.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create some random data.
Random rnd = new Random();
double[][] data = (from el in Enumerable.Range(0, 100) select new double[] { el + rnd.NextDouble(), 2 * el + 100 * rnd.NextDouble() }).ToArray();

// Create the line chart using the random data and the default settings.
Plot plot = Plot.Create.AreaChart(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"area.svg");
{% endhighlight %}

Optional parameters can be used to determine the appearance of some elements of the plot. Many of these are in common with other kinds of plots and are described in the page about [scatter plots]({{ site.baseurl }}{% link plots.scatter.md %}); here are the ones specific to area charts:

* `bool vertical`: if this is `true` (the default), the area between the data points and the `X` axis is shaded. If this is set to `false`, the area between the data points and the `Y` axis is shaded instead.
* `bool smooth`: by default, the points are connected by straight lines. If you set this to `true`, a smooth spline will be computed, which passes through all the data points.
* `PlotElementPresentationAttributes dataPresentationAttributes`: this determines the presentation attributes for the line and the filled area.

Note that, in order for the area chart to be displayed correctly, the values for the `x` axis should be monotonically increasing (if `vertical` is `true`; this should apply to the `y` axis otherwise), that is, the data values should be provided in order of increasing `x`.

## Stacked area charts

The `Plot.Create.StackedAreaChart` method can be used to create a stacked area chart. This consists of multiple areas stacked one over the other.

<div class="code-example">
    <iframe src="assets/images/plots/area2.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create some random data.
Random rnd = new Random();
double[][] data = (from el in Enumerable.Range(1, 100)
                   let x = el + rnd.NextDouble()
                   let y1 = 2 * el + 100 * rnd.NextDouble()
                   let y2 = y1 * 2
                   select new double[] { x, y1, y2 }).ToArray();

// Create the stacked area chart using the random data.
Plot plot = Plot.Create.StackedAreaChart(data,
    xAxisTitle: "Horizontal axis",
    yAxisTitle: "Vertical axis",
    title: "Area chart");

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"area.svg");
{% endhighlight %}

This method has the same parameters as the `AreaChart` method. Here are a few notes to create a sucessful stacked area chart:

* The data should be provided in the following format:
    * If `vertical` is `true` (the default), `data[i][0]` shall be the `x` value for all samples. Then, `data[i][1]`, `data[i][2]`, etc., are the `y` axis values for each of the stacked lines.
    * If `vertical` is `false`, `data[i][1]` shall be the `y` value for all samples. Then, `data[i][0]`, `data[i][2]`, etc., are the `x` axis values for each of the stacked lines.
* All data elements must be of the same length (`n + 1`, where `n` is the number of stacked areas).
* The values for the `x` axis (or `y` axis, if `vertical` is false) should be monotonically increasing.
* The values for the stacked lines should be in increasing order. E.g., if `vertical` is `true`, `data[i][2]` must be greater than `data[i][1]` etc.


    