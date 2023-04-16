---
layout: default
nav_order: 9
parent: Plots
---

# Violin plots

A violin plot is another way to compare multiple distributions of numerical variables. It is similar to a box plot, but a distribution is shown rather than (or, in addition to) the boxes. You can create a violin plot using the `Plot.Create.ViolinPlot` method:

<div class="code-example">
    <iframe src="assets/images/plots/violin.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;
using MathNet.Numerics.Distributions;

// Generate some samples from a few distributions.
(string, IReadOnlyList<double>)[] data = new (string, IReadOnlyList<double>)[]{
    ( "N(0, 1)", Normal.Samples(0, 1).Take(500).ToArray() ),
    ( "N(2, 1)", Normal.Samples(2, 1).Take(1000).ToArray() ),
    ( "Γ(3, 3)", Gamma.Samples(3, 3).Take(100).ToArray() ),
    ( "E(1)", Exponential.Samples(1).Take(200).ToArray() ),
    ( "LogNorm(1, 0.2)", LogNormal.Samples(1, 0.2).Take(300).ToArray()) };

// Create the violin plot using the random data and the default settings.
Plot plot = Plot.Create.ViolinPlot(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"violin.svg");
{% endhighlight %}

Like the `Plot.Create.BoxPlot` method, this method has three overloads, differing in the type of the first parameter they accept. The first overload (used in the example above) takes an `IReadOnlyList<(T, IReadOnlyList<double>)>`; each element in this array consists of a `T` whose `ToString` method is used to determine the labels, and an `IReadOnlyList<double>` containing the elements whose distribution is displayed by the violin plot. This method has a number of optional parameters that can be used to determine the appearance of some elements of the plot. Many of these are in common with other kinds of plots and are described in the page about [scatter plots]({{ site.baseurl }}{% link plots.scatter.md %}); here are the ones specific for violin plots:

* `bool proportionalWidth`: if this is `false` (the default), all violins have the same width. If this is `true`, the width of each violin in the plot is proportional to the number of samples.
* `bool smooth`: if this `true` (the default), the distributions are smoothed by drawing a spline curve that passes through each point. If this is `false`, straight line segments are drawn.
* `Violin.ViolinSide sides`: this parameter determines whether the violins should be drawn on the left, on the right, or both (the default).
* `bool showBoxPlots`: if this is `true` (the default), a small box plot is drawn together with each violin. Otherwise, only the violins are drawn.
* `double violinWidth`: this parameter determines the width of the violins in the data space. You probably do not want to change this.
* `double boxWidth`: this parameter determines the width of the boxes (if `showBoxPlots` is `true`), relative to the width of the violins.
* `double spacing`: this determines the amount of spacing between consecutive violins.
* `double? dataRangeMin`: this parameter determines the minimum value shown on the data value axis. If this is `null` (the default), this is determined from the data. Providing a value for this parameter is useful if you wish to create multiple plots with the same range of values.
* `double? dataRangeMax`: this parameter determines the maximum value shown on the data value axis. If this is `null` (the default), this is determined from the data. Providing a value for this parameter is useful if you wish to create multiple plots with the same range of values.
* `bool vertical`: if this is `true` (the default), the violins are oriented vertically and parallel to the `Y` axis. If this is `false`, the violins are horizontal.
* `IReadOnlyList<PlotElementPresentationAttributes> violinPresentationAttributes`: this determines the appearance (stroke, fill, etc) of the violins.
* `IReadOnlyList<PlotElementPresentationAttributes> boxPresentationAttributes`: this determines the appearance (stroke, fill, etc) of the boxes and whiskers.
* `IReadOnlyList<PlotElementPresentationAttributes> medianPresentationAttributes`: this determines the appearance (stroke, fill, etc) of the symbols used to highlight the median in the box plots.
* `IDataPointElement medianSymbol`: this determines the symbol used to highlight the median in the box plots.

The second overload for this method takes a first parameter of type `IReadOnlyList<IReadOnlyList<double>>`, and draws a violin plot without labels on the `X` axis. The optional parameters for this overload are the same as the previous overload. Finally, the third overload for this method takes a first parameter of type `IReadOnlyList<double>` and draws a single violin in the plot. This overload does not have the `proportionalWidth` and `spacing` parameters.

Note that in all cases, the distributions drawn here are "histograms in disguise" (see [the page about distribution plots]({{ site.baseurl }}{% link plots.distribution.md %})) and not kernel density estimations.

The following example shows the effect of some of these optional parameters on the plot:

<div class="code-example">
    <iframe src="Blazor?violin" style="width: 100%; height: 50em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;
using MathNet.Numerics.Distributions;

// Generate some samples from a few distributions.
(string, IReadOnlyList<double>)[] data = new (string, IReadOnlyList<double>)[]{
    ( "N(0, 1)", Normal.Samples(0, 1).Take(500).ToArray() ),
    ( "N(2, 1)", Normal.Samples(2, 1).Take(1000).ToArray() ),
    ( "Γ(3, 3)", Gamma.Samples(3, 3).Take(100).ToArray() ),
    ( "E(1)", Exponential.Samples(1).Take(200).ToArray() ),
    ( "LogNorm(1, 0.2)", LogNormal.Samples(1, 0.2).Take(300).ToArray()) };

// Create the violin plot using the random data.
Plot plot = Plot.Create.ViolinPlot(data, proportionalWidth: true, showBoxPlots: false,
    xAxisTitle: "Distributions", title: "Violin plot");

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"violin.svg");
{% endhighlight %}
