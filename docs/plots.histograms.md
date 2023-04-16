---
layout: default
nav_order: 6
parent: Plots
---

# Histograms

A histogram is similar to a bar chart, but both axes represent numerical values. Histograms are generally used to display the distribution of values for a numeric variable. You can create a histogram using the `Plot.Create.Histogram` method:

<div class="code-example">
    <iframe src="assets/images/plots/histogram.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;
using MathNet.Numerics.Distributions;

// Generate some samples from a standard normal distribution.
double[] data = Normal.Samples(0, 1).Take(500).ToArray();

// Create the histogram using the random data and the default settings.
Plot plot = Plot.Create.Histogram(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"histogram.svg");
{% endhighlight %}

This method has two overloads: the first overload (used in the example above) accepts an `IReadOnlyList<double>` as a first parameter and creates a histogram showing the distribution of the values in the input array. This has a number of optional parameters that can be used to determine the appearance of some elements of the plot. Many of these are in common with other kinds of plots and are described in the page about [scatter plots]({{ site.baseurl }}{% link plots.scatter.md %}); here are the ones specific to this method:

* `bool vertical`: if this is `true` (the default), the base of the bars lies on the `X` axis and the bars go from bottom to top. If this is `false`, the base of the bars lies on the `Y` axis and the bars go from left to right.
* `double margin`: this determines the amount of space between consecutive bars.
* `int binCount`: this parameter determines the number of bins (bars) in the histogram. If this is &le; 1 (the default is `-1`), the number of bins to use is determined automatically using the [Freedman-Diaconis rule](https://en.wikipedia.org/wiki/Freedman%E2%80%93Diaconis_rule).
* `double underflow`: values in the input array that are smaller than this value are excluded from the histogram and included in an underflow bar. The default is $-\infty$, i.e., no underflow bar is drawn.
* `double overflow`: values in the input array that are larger than this value are excluded from the histogram and included in an overflow bar. The default is $+\infty$, i.e., no overflow bar is drawn.
* `PlotElementPresentationAttributes dataPresentationAttributes`: this determines the appearance (stroke, fill, etc) of the bars.

The second overload accepts a first parameter of type `IReadOnlyList<IReadOnlyList<double>>` which, instead, should contain data that have already been pre-binned. Each element of this array should be an array containing two elements, the first being the X coordinate of the centre of a bar, and the second being the height of the bar. This makes it possible to create histograms with unequal bin widths, but for this you will have to bin the data yourself. As a consequence, this method does not have `binCount`, `underflow` or `overflow` parameters, while the other parameters are the same as for the other overload of the `Histogram` method.

