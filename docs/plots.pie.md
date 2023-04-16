---
layout: default
nav_order: 4
parent: Plots
---

# Pie charts

A pie chart is used to display the relative proportions of data. You can create a pie chart using the `Plot.Create.PieChart` method:

<div class="code-example">
    <iframe src="assets/images/plots/pie.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create some random data.
Random rnd = new Random();
double[] data = new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) };

// Create the pie chart using the random data and the default settings.
Plot plot = Plot.Create.PieChart(data);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"pie.svg");
{% endhighlight %}

This method's optional parameters can be used to determine the appearance of some elements of the plot. Many of these are in common with other kinds of plots and are described in the page about [scatter plots]({{ site.baseurl }}{% link plots.scatter.md %}); here are the ones specific to pie charts:

* `bool clockwise`: determines whether the slices are drawn in clockwise or anticlockwise (default) fashion.
* `double startAngle`: the initial angle for the first slice.
* `IReadOnlyList<PlotElementPresentationAttributes> dataPresentationAttributes`: the presentation attributes for the each slice. If there are more slices than elements in this array, the elements are wrapped.

Furthermore, the `Plot.Create.DoughnutChart` method has almost exactly the same parameters, and can be used to create a doughnut chart (i.e., a pie chart where a circle has been "cut out"). The only additional parameter is `double innerRadius`, which determines how much of the centre of the plot is cut out.

## Plotting multiple pie charts

If you have multiple distributions of data to plot, you can use the `Plot.Create.PieCharts` or `Plot.Create.DoughnutCharts` methods to plot all of them at once. The only difference between these methods and the ones that create a single pie/doughnut chart is that they take as a first argument an `IReadOnlyList<IReadOnlyList<double>>` array rather than an `IReadOnlyList<double>` array.

<div class="code-example">
    <iframe src="assets/images/plots/doughnut.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create some random data.
Random rnd = new Random();
double[][] data = new double[][] {
        new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) },
        new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) } };


// Create the doughnut charts using the random data and the default settings.
Plot plot = Plot.Create.DoughnutCharts(data, title: "Doughnut charts");

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"doughnut.svg");
{% endhighlight %}

## Plotting stacked doughnut charts

Another way to display multiple distributions, especially if you would like to highlight the differences between them, is to create a stacked doughnut chart. Here, multiple doughnut charts are drawn in each other's "hole". This can be achieved using the `Plot.Create.StackedDoughnutChart` method. Compared to the previous methods, this has an additional parameter, `double margin`, which determines the spacing between the concentric doughnuts.

<div class="code-example">
    <iframe src="assets/images/plots/stacked_doughnut.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// Create some random data.
Random rnd = new Random();
double[][] data = new double[][] {
        new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) },
        new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) },
        new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) }};


// Create the doughnut charts using the random data and the default settings.
Plot plot = Plot.Create.StackedDoughnutChart(data, title: "Stacked doughnut chart");

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"stacked_doughnut.svg");
{% endhighlight %}
