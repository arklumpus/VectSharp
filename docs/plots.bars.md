---
layout: default
nav_order: 5
parent: Plots
---

# Bar charts

A bar chart can be used to plot numerical values for a categorical variable. You can create a bar chart using the `Plot.Create.BarChart` method:

<div class="code-example">
    <iframe src="assets/images/plots/bars.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create some random data.
Random rnd = new Random();
(string, double)[] data = new (string, double)[] {
    ( "Category 1", rnd.Next(1, 10) ),
    ( "Category 2", rnd.Next(1, 10) ),
    ( "Category 3", rnd.Next(1, 10) ),
    ( "Category 4", rnd.Next(1, 10) ) };

// Create the bar chart using the random data and the default settings.
Plot plot = Plot.Create.BarChart(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"bars.svg");
{% endhighlight %}

This method has two overloads:

* The generic overload `BarChart<T>` takes an argument of type `IReadOnlyList<(T, double)>` (as in the example) and calls the `ToString` method on the `T` item to display the category labels.
* The overload that takes an argument of type `IReadOnlyList<double>` assumes that the supplied data represent the height of the bars, and does not show any category labels.

Optional parameters can be used to determine the appearance of some elements of the plot. Many of these are in common with other kinds of plots and are described in the page about [scatter plots]({{ site.baseurl }}{% link plots.scatter.md %}); here are the ones specific to bar charts:

* `bool vertical`: if this is `true` (the default), the base of the bars lies on the `X` axis and the bars go from bottom to top. If this is `false`, the base of the bars lies on the `Y` axis and the bars go from left to right.
* `double margin`: this determines the amount of space between consecutive bars.
* `PlotElementPresentationAttributes dataPresentationAttributes`: this determines the appearance (stroke, fill, etc) of the bars.

## Stacked bar charts

The two overloads of the `Plot.Create.StackedBarChart` method can be used to plot a stacked bar chart, i.e., a plot where multiple bars of different colours are stacked one on top of the other. The overload that takes a first parameter of type `IReadOnlyList<(T, IReadOnlyList<double>)>` uses the first element of each tuple (of type `T`) to determine the category labels, as in the `BarChart<T>` method, while the second element (which is an array of `double`s) represents the heights for each stacked bar in the category. The other overload takes a first parameter of type `IReadOnlyList<IReadOnlyList<double>>` and is similar to the overload creating a simple bar chart from an `IReadOnlyList<double>`: the first parameter is an array of arrays, and each array refers to all the stacked bars in a single category (again, category labels are not shown here).

Most of the parameters for these methods are the same as for the `BarChart` method, with the exception of `IReadOnlyList<PlotElementPresentationAttributes> dataPresentationAttributes`, which is an array instead of being a single element. The elements of this array are used to determine the apperance of each bar in the stack; if the array contains fewer elements than bars, they are wrapped.

<div class="code-example">
    <iframe src="assets/images/plots/stacked_bars.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create some random data.
Random rnd = new Random();
// Note that because of C# tuples are invariant, we have to declare the tuple as
// (string, IReadOnlyList<double>) instead of (string, double[]) - we can still use
// a double[] for the second item of the tuple, though.
(string, IReadOnlyList<double>)[] data = new (string, IReadOnlyList<double>)[] {
    ( "Category 1", new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) } ),
    ( "Category 2", new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) } ),
    ( "Category 3", new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) } ),
    ( "Category 4", new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) } ) };

// Create the bar chart using the random data and the default settings.
Plot plot = Plot.Create.StackedBarChart(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"stacked_bars.svg");
{% endhighlight %}

## Clustered bar charts

A clustered bar chart is similar to a collection of multiple "mini bar charts" one next to the other. These can be created using the `Plot.Create.ClusteredBarChart` method, which as usual has two overloads: the overload that takes a first parameter of type `IReadOnlyList<(T, IReadOnlyList<double>)>` uses the first element of each tuple (of type `T`) to determine the category labels, while the second element (which is an array of `double`s) represents the heights for each clustered bar in the category. The other overload takes a first parameter of type `IReadOnlyList<IReadOnlyList<double>>` and is similar to the overload creating a simple bar chart from an `IReadOnlyList<double>`: the first parameter is an array of arrays, and each array refers to all the clustered bars in a single category (again, category labels are not shown here).

The parameters for this method are similar to the `StackedBarChart` methods, but here, instead of a single `double margin` parameter, there are two `double interClusterMargin` and `double intraClusterMargin`, which determine the margin _between_ clusters and _within_ clusters, respectively.

<div class="code-example">
    <iframe src="assets/images/plots/clustered_bars.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create some random data.
Random rnd = new Random();
// Note that because of C# tuples are invariant, we have to declare the tuple as
// (string, IReadOnlyList<double>) instead of (string, double[]) - we can still use
// a double[] for the second item of the tuple, though.
(string, IReadOnlyList<double>)[] data = new (string, IReadOnlyList<double>)[] {
    ( "Category 1", new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) } ),
    ( "Category 2", new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) } ),
    ( "Category 3", new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) } ),
    ( "Category 4", new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) } ) };

// Create the bar chart using the random data and the default settings.
Plot plot = Plot.Create.ClusteredBarChart(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"clustered_bars.svg");
{% endhighlight %}