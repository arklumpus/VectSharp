---
layout: default
nav_order: 11
parent: Plots
---

# 2-D Function plots

VectSharp.Plots can plot functions of two variables. These can be created using some overloads of the `Plot.Create.Function` method:

<div class="code-example">
    <iframe src="assets/images/plots/function2d.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create a function plot.
Plot plot = Plot.Create.Function((x, y) => Math.Sin(x) * Math.Cos(y), -10, -10, 10, 10);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"function2d.svg");
{% endhighlight %}

There are three overloads of the `Function` method that can be used to plot functions of two variables:

1. An overload taking a first argument of type `Func<double, double, double>` (i.e., a function accepting two `double` parameters and returning a `double`).
2. An overload taking a first argument of type `Func<double[], double>` (i.e., a function accepting a `double[]` parameter and returning a `double`). The `double[]` argument to the function will contain the values of the two variables that should be used to compute the function's value.
3. An overload taking a first argument of type `Function2DGrid`. This consists of a function that has already been sampled over a range of values (see below).

The first two overloads are very similar; they have the same parameters, which include four required parameters (`double minX`, `double minY`, `double maxX`, and `double maxY`) used to define the range of values over which the function will be sampled, as well as some optional parameters that can be used to determine the appearance of some elements of the plot. Many of these are in common with other kinds of plots and are described in the page about [scatter plots]({{ site.baseurl }}{% link plots.scatter.md %}); here are the ones specific for function plots:

* `double resolutionX` and `double resolutionY` determine how often the function is sampled along both axes. If these are `double.NaN` (the default), the resolution is determined automatically based on the coordinate system.
* `Function2DGrid.GridType? gridType`: this parameter determines the kind of grid along which the values are sampled. If this is `Function2DGrid.GridType.Rectangular`, values are sampled along a simple rectangular grid. If this is `Function2DGrid.GridType.HexagonHorizontal` or `Function2DGrid.GridType.HexagonVertical`, values are sampled along a hexagonal grid. If this is `Function2DGrid.GridType.Irregular`, values are sampled at randomly chosen values.
* `Function2D.PlotType plotType`: this parameter determines what kind of plot is produced. If this is `Function2D.PlotType.SampledPoints`, a symbol is drawn at the values of the function that have been sampled. If this is `Function2D.PlotType.Tessellation` (the default), the plot area is coloured using shapes appropriates to the kind of grid along which the values have been sampled. If this is `Function2D.PlotType.Raster`, the sampled values are used to produce a raster image, which is then stretched to cover the plot area.
* `int rasterResolutionX` and `int rasterResolutionY`: if the `plotType` is `Function2D.PlotType.Raster`, these parameters determine the width and height of the raster image used to show the function values.
* `IDataPointElement pointElement`: if the `plotType` is `Function2D.PlotType.SampledPoints`, this determines the symbol drawn at each sampled point.
* `Func<double, Colour> colouring`: this parameter determines the function used to transform the function values into colours for the plot. By default, a [viridis](https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html) gradient is used. You should set this to a function accepting a single `double` argument ranging between `0` and `1` (inclusive) and returning a `Colour`. You can also provide a `GradientStops` object. For example, the `VectSharp.Gradients` static class has a number of gradients and functions that are ready to use.

The following example shows the effect of some of these optional parameters (the example runs at a relatively low resolution because Blazor is slow):

<div class="code-example">
    <iframe src="Blazor?function2d" style="width: 100%; height: 44em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create a function plot.
Plot plot = Plot.Create.Function((x, y) => Math.Sin(x) * Math.Cos(y), -10, -10, 10, 10,
    colouring: Gradients.CividisColouring,
    plotType: Function2D.PlotType.Tessellation,
    gridType: Function2DGrid.GridType.Irregular);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"function2d.svg");
{% endhighlight %}

## The `Function2DGrid` class

The `Function2DGrid` parameter used by the third overload of this method consists of a collection of values that have been sampled for the function. This overload does not have the `minX`, `minY`, `maxX`, `maxY`, `resolution`, and `gridType` arguments, because these are determined by the `Function2DGrid`; the other arguments are the same as the other overloads.

A `Function2DGrid` can be created using the `Function2DGrid` constructor; this has two overloads. The first overload takes an `IReadOnlyList<IReadOnlyList<double>>` argument containing values that have already been sampled for the function; each element of this array is itself an array of `double`s, where the first two elements are the $x$ and $y$ arguments of the function and the third element is the value of $f(x, y)$. The second overload takes the function delegate, as well as the range parameters `minX`, `minY`, `maxX`, and `maxY`. The `int stepsX` and `int stepsY` parameters determine how many times the function is sampled, while the `GridType type` parameter determines the kind of grid (rectangular, hexagonal, or irregular).

You can manually instantiate a `Function2DGrid` object rather than plotting the function directly if you wish to have more control over the sampling of the function (especially if you use the constructor taking the collection of sampled values).