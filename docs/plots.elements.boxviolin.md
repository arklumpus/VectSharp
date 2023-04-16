---
layout: default
nav_order: 6
grand_parent: Plots
parent: Plot elements
---

# Box and violin plots

Box plots and violin plots are used similarly to show the distribution of a numerical quantity. These can be created using the `BoxPlot` and the `ViolinPlot` classes, respectively. Each instance of these classes draws a single box/violin; multiple instances can be combined in the same plot in order to compare different distributions.

The following example shows how to use these classes:

<div class="code-example">
    <iframe src="assets/images/plots/boxviolin.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
<details markdown="block">
<summary>
    Expand source code
  </summary>
  {: .text-delta }
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

// Generate some samples from a few distributions.
double[] data1 = Normal.Samples(0, 1).Take(500).ToArray();
double[] data2 = Normal.Samples(2, 1).Take(1000).ToArray();
double[] data3 = Gamma.Samples(3, 3).Take(100).ToArray();
double[] data4 = Exponential.Samples(1).Take(200).ToArray();
double[] data5 = LogNormal.Samples(1, 0.2).Take(300).ToArray();

// Determine the overall range for the samples (we will need this for the coordinate system).
double overallMin = new[] { data1.Min(), data2.Min(), data3.Min(), data4.Min(), data5.Min() }.Min();
double overallMax = new[] { data1.Max(), data2.Max(), data3.Max(), data4.Max(), data5.Max() }.Max();

// Create a linear coordinate system.
LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(-25, 125, overallMin, overallMax, 350, 250);

// Compute statistics for the distributions that will be plotted using box plots.
double min1 = data1.Min();
double firstQuart1 = data1.LowerQuartile();
double median1 = data1.Median();
double thirdQuart1 = data1.UpperQuartile();
double max1 = data1.Max();

double min2 = data2.Min();
double firstQuart2 = data2.LowerQuartile();
double median2 = data2.Median();
double thirdQuart2 = data2.UpperQuartile();
double max2 = data2.Max();

double min3 = data3.Min();
double firstQuart3 = data3.LowerQuartile();
double median3 = data3.Median();
double thirdQuart3 = data3.UpperQuartile();
double max3 = data3.Max();

// Create three vertical box plots.
BoxPlot box1 = new BoxPlot(new double[] { 0, median1 }, // Centred on the median.
    new double[] { 0, 1 }, // Vertical direction
    min1 - median1, // Position of the first whisker, relative to the centre. This will be < 0.
    firstQuart1 - median1, // Position of the first side of the box, relative to the centre. This will be < 0.
    thirdQuart1 - median1, // Position of the second side of the box, relative to the centre. This will be > 0.
    max1 - median1, // Position of the second whisker, relative to the centre. This will be > 0.
    coordinateSystem);

// Same as above, but shifted on the X coordinate.
BoxPlot box2 = new BoxPlot(new double[] { 25, median2 }, new double[] { 0, 1 }, min2 - median2, firstQuart2 - median2, thirdQuart2 - median2, max2 - median2, coordinateSystem);

// Same as above, but shifted on the X coordinate.
BoxPlot box3 = new BoxPlot(new double[] { 50, median3 }, new double[] { 0, 1 }, min3 - median3, firstQuart3 - median3, thirdQuart3 - median3, max3 - median3, coordinateSystem)
{
    Width = 2, // Reduce the width.
    WhiskerWidth = 0 // Hide the end symbols for the whiskers.
};

// Create three vertical violin plots.
Violin violin3 = new Violin(new double[] { 50, 0 }, // Starting at the same X as box3, but at 0 instead of being centred on the median.
    new double[] { 0, 1 }, // Vertical direction
    data3, // Data samples.
    coordinateSystem);

// Same as above, but shifted on the X coordinate.
Violin violin4 = new Violin(new double[] { 75, 0 }, new double[] { 0, 1 }, data4, coordinateSystem);

// Same as above, but shifted on the X coordinate.
Violin violin5 = new Violin(new double[] { 100, 0 }, new double[] { 0, 1 }, data5, coordinateSystem);

// Create the plot.
Plot plot = new Plot();

// Add the plot elements to the plot.
plot.AddPlotElements(box1, box2, violin3, box3,  violin4, violin5);

// Renderthe plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG("boxviolin.svg");
{% endhighlight %}
</details>


## The `BoxPlot` class

The `BoxPlot` class draws a box plot. When creating an instance of this class using the constructor, you will have to supply the `position` (i.e., the data space coordinates for the centre of the box, such as the median or mean of a distribution), the `direction` (i.e., a vector defining the direction along which the box is drawn, still in data space coordinates), as well as the distance between the centre and the ends of the whiskers and the sides of the box. In most cases, for the `direction` you will want to provide something like `{0, 1}` (for vertical box plots) or `{1, 0}` (for horizontal box plots), and then use negative values for the first whisker and the first side of the box, and positive values for the second side of the box and the second whisker.

On the one hand, this means that you have to compute the positions of the various elements "manually" (e.g., by computing the interquartile range as in the example above); on the other end, this allows you to use the `BoxPlot` class for things that are not actually distributions of values; for example, you could use `BoxPlot` instances to draw error bars in a bar chart.

## The `Violin` class

The `Violin` class draws a violin plot representing the distribution of a numerical variable. When creating an instance of this class using the constructor, you will have to supply a `position` in data space coordinates, which will correspond to the value `0` for the numerical variable. You will also need a `direction` along which the violin will extend; values for the numerical variable will be multiplied by this to determine their corresponding coordinates in the plot. The violin will extend perpendicularly from the `direction`, on one or both sides (as determined by the `Sides` property). Unlike the box plot, a violin plot must necessarily represent a distribution of values.