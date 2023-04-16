---
layout: default
nav_order: 7
parent: Plots
---

# Distributions

Another way to plot the distribution of values for a numerical variable is to use a distribution plot. This is essentially a histogram "in disguise": instead of drawing the bars corresponding to each bin, this kind of plot draws a curve that passes through the tips of each bar. You can create a distribution plot using the `Plot.Create.Distribution` method:

<div class="code-example">
    <iframe src="assets/images/plots/distribution.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;
using MathNet.Numerics.Distributions;

// Generate some samples from a standard normal distribution.
double[] data = Normal.Samples(0, 1).Take(500).ToArray();

// Create the distribution plot using the random data and the default settings.
Plot plot = Plot.Create.Distribution(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"distribution.svg");
{% endhighlight %}

This method has a single overload, accepting a first parameter of type `IReadOnlyList<double>`, which represents the samples whose distribution should be plotted. It also has a number of optional parameters that can be used to determine the appearance of some elements of the plot. Many of these are in common with other kinds of plots and are described in the page about [scatter plots]({{ site.baseurl }}{% link plots.scatter.md %}); here are the ones specific to this method:

* `bool vertical`: if this is `true` (the default), the base of the distribution lies on the `X` axis. If this is `false`, the base of the distribution lies on the `Y` axis.
* `int binCount`: this parameter determines the number of bins used to plot the distribution (corresponding to the number of bars in a histogram). If this is &le; 1 (the default is `-1`), the number of bins to use is determined automatically using the [Freedman-Diaconis rule](https://en.wikipedia.org/wiki/Freedman%E2%80%93Diaconis_rule).
* `bool smooth`: if this is `false` (the default), the distribution is plotted using straight line segments that pass through each point. If this is `true`, a smooth spline that passes through each point is used instead.
* `PlotElementPresentationAttributes dataPresentationAttributes`: this determines the appearance (stroke, fill, etc) of the distribution.

Note that the plot produced by this method is **NOT** a kernel density estimation.

## Plotting multiple distributions

If you wish to compare multiple distributions, it may be useful to plot all of them at the same time. This can be achieved with the `Plot.Create.StackedDistribution` method:

<div class="code-example">
    <iframe src="assets/images/plots/stacked_distribution.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;
using MathNet.Numerics.Distributions;

// Generate some samples from two normal distributions.
double[][] data = new double[][]{
    Normal.Samples(0, 1).Take(500).ToArray(),
    Normal.Samples(2, 1).Take(1000).ToArray() };

// Create the stacked distribution plot using the random data
// and the default settings.
Plot plot = Plot.Create.StackedDistribution(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"stacked_distribution.svg");
{% endhighlight %}

The first parameter for this method is an `IReadOnlyList<IReadOnlyList<double>>` object; each element of this array should be an array containing the values for one of the distributions. This method has a few parameters in common with the `Distribution` method, i.e. `bool vertical`, `int binCount` (if this is &le; 1, the number of bins is determined independently for each distribution), `bool smooth`. Here, `dataPresentationAttributes` is an `IReadOnlyList<PlotElementPresentationAttributes>`, where each element determines the appearance of one distribution (if there are more distributions than element in this array, the values are wrapped).

Furthermore, this method has a parameter called `normalisationMode`, which determines what kind of scaling is applied to the distributions:
* If this is `Plot.NormalisationMode.None`, the raw bin counts are used. This will cause distributions built with more samples to be taller.
* If this is `Plot.NormalisationMode.Maximum`, the curves are normalised so that the maxima of all curves have the same height.
* If this is `Plot.NormalisationMode.Area` (the default), the curves are normalised so that they all have the same area.
