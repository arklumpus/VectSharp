---
layout: default
nav_order: 8
parent: Plots
---

# Box plots

A box plot can be used to compare multiple distributions of numerical variables. You can create a box plot using the `Plot.Create.BoxPlot` method:

<div class="code-example">
    <iframe src="assets/images/plots/boxplot.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
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

// Create the box plot using the random data and the default settings.
Plot plot = Plot.Create.BoxPlot(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG("boxplot.svg");
{% endhighlight %}

This method has three overloads, differing in the type of the first parameter they accept. The first overload (used in the example above) takes an `IReadOnlyList<(T, IReadOnlyList<double>)>`; each element in this array consists of a `T` whose `ToString` method is used to determine the box labels, and an `IReadOnlyList<double>` containing the elements whose distribution is displayed by the box plot. This method has a number of optional parameters that can be used to determine the appearance of some elements of the plot. Many of these are in common with other kinds of plots and are described in the page about [scatter plots]({{ site.baseurl }}{% link plots.scatter.md %}); here are the ones specific for box plots:

* `Plot.WhiskerType whiskerType`: this parameter determines the kind whiskers that are drawn with the box plot. If this is `Plot.WhiskerType.FullRange`, the whiskers go from the maximum to the minimum observed value. If this is `Plot.WhiskerType.IQR_1_5`, whiskers go from $Q_1 - 1.5 \cdot IQR$ to $Q_3 + 1.5 \cdot IQR$ (where $Q_1$ and $Q_3$ are the first and third quartiles, and $IQR$ is the interquartile range). If this is `Plot.WhiskerType.StandardDeviation`, the wiskers go from $\mu - 2\sigma$ to $\mu + 2\sigma$, where $\mu$ is the mean of the data and $\sigma$ is the standard deviation of the data.
* `bool useNotches`: this parameter determines whether notches are used when drawing the box plot. If this is `true` (the default), notches are drawn at a distance from the median corresponding to $1.58 \frac{Q_3 - Q_1}{\sqrt{n}}$, where $Q_1$ and $Q_3$ are the first and third quartiles, and $n$ is the number of samples.
* `bool proportionalWidth`: if this is `false` (the default), all boxes have the same width. If this is `true`, the width of each box in the plot is proportional to the number of samples.
* `bool showOutliers`: this parameter determines whether outlier points (i.e., data points that fall beyond the range of the whiskers) should be drawn or not.
* `double boxWidth`: this parameter determines the width of the boxes in the data space. You probably do not want to change this.
* `double spacing`: this determines the amount of spacing between consecutive boxes.
* `double? dataRangeMin`: this parameter determines the minimum value shown on the data value axis. If this is `null` (the default), this is determined from the data. Providing a value for this parameter is useful if you wish to create multiple plots with the same range of values.
* `double? dataRangeMax`: this parameter determines the maximum value shown on the data value axis. If this is `null` (the default), this is determined from the data. Providing a value for this parameter is useful if you wish to create multiple plots with the same range of values.
* `bool vertical`: if this is `true` (the default), the boxes are oriented vertically and parallel to the `Y` axis. If this is `false`, the boxes are horizontal.
* `IReadOnlyList<PlotElementPresentationAttributes> boxPresentationAttributes`: this determines the appearance (stroke, fill, etc) of the boxes and whiskers.
* `IDataPointElement outlierPointElement`: this determines the symbol drawn for the outlier points.
* `IReadOnlyList<PlotElementPresentationAttributes> outlierPresentationAttributes`: this determines the appearance (stroke, fill, etc) of the outliers.

The second overload for this method takes a first parameter of type `IReadOnlyList<IReadOnlyList<double>>`, and draws a box plot without labels on the `X` axis. The optional parameters for this overload are the same as the previous overload. Finally, the third overload for this method takes a first parameter of type `IReadOnlyList<double>` and draws a single box in the plot. This overload does not have the `proportionalWidth` and `spacing` parameters.

The following example shows the effect of some of these optional parameters on the plot:

<div class="code-example">
    <iframe src="Blazor?boxplot" style="width: 100%; height: 45em; border: 0px solid black"></iframe>
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

// Create the box plot using the random data.
Plot plot = Plot.Create.BoxPlot(data, useNotches: false, proportionalWidth: true,
    xAxisTitle: "Distributions", title: "Box plot");

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG("boxplot.svg");
{% endhighlight %}
