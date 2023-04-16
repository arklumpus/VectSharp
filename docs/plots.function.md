---
layout: default
nav_order: 10
parent: Plots
---

# 1-D Function plots

VectSharp.Plots can be used to create function plots, i.e. plots showing the value of a function over a range of its arguments. To create a plot for a function of one variable, you can use the `Plot.Create.Function` method:

<div class="code-example">
    <iframe src="assets/images/plots/function.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create a function plot.
Plot plot = Plot.Create.Function(x => x * Math.Sin(x), -20, 20);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"function.svg");
{% endhighlight %}

This method has five overloads, two of which are used to plot functions of one variable. One overload's first argument is a `Func<double, double>` delegate (i.e., a function taking a single `double` argument and returning a `double`, which is the one used in the example above), while the other has as a first argument an `IReadOnly<Func<double, double>>` (i.e., a collection of functions). The first overload plots a single function, while the second overload plots multiple functions one over the other. Both overloads have two additional required parameters, `double min` and `double max`, which define the range of arguments over which the function is plotted.

Other optional parameters can be used to determine the appearance of some elements of the plot. Many of these are in common with other kinds of plots and are described in the page about [scatter plots]({{ site.baseurl }}{% link plots.scatter.md %}); here are the ones specific for function plots:

* `double resolution`: this determines in how many points between the `min` and the `max` the function is sampled. If this is `double.NaN` (the default), the resolution is determined automatically based on the coordinate system. Set this to a small value to make the plot smoother.
* `bool vertical`: if this is `true` (the default), the argument of the function is on the `X` axis, and the value of the function is on the `Y` axis. If this is `false`, the argument of the function is on the `Y` axis and the value of the function is on the `X` axis.
* `bool smooth`: if this is `true`, the sampled values for the function are connected by a spline that passes through all of them. If this is `false` (the default), straight line segments are used instead.
* `PlotElementPresentationAttributes linePresentationAttributes`: this determines the appearance of the line connecting the sampled values for the function.
* `PlotElementPresentationAttributes pointPresentationAttributes`: this determines the appearance of the symbols drawn at the sampled values for the function.
* `double pointSize`: this determines the size of the symbols drawn at the sampled values for the function. The default for this is `0`, which means that no symbols are drawn at these points.
* `IDataPointElement dataPointElement`: this parameter determines the appearance of the symbols drawn at the sampled values for the function.

For the overload taking a collection of functions, the parameters `linePresentationAttributes`, `pointPresentationAttributes`, `pointSize` and `dataPointElements` are defined as `IReadOnlyList<>`s instead of individual elements.

The following example shows the effect of some of these parameters when plotting multiple functions:

<div class="code-example">
    <iframe src="assets/images/plots/function2.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

Func<double, double>[] functions = new Func<double, double>[]
{
    x => x * Math.Sin(x),
    x => 0.05 * x * x,
    x => 10 * (Math.Exp(x) - 1) / (1 + Math.Exp(x))
};

// Create a function plot.
Plot plot = Plot.Create.Function(functions, -20, 20,
    xAxisTitle: "x", yAxisTitle: "f(x)", title: "Function plot",
    pointSize: new double[] { 1.5 }, resolution: 0.5, smooth: true);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"function.svg");
{% endhighlight %}
