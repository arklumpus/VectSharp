---
layout: default
nav_order: 12
parent: Plots
---

# Custom plots

The methods in the `Plot.Create` class are useful to create "standard" plots, but you can achieve a greater degree of customisation by manually creating a plot. This involves creating all the individual plot elements (axes, grid, data elements, etc) and adding them onto an empty `Plot` object.

The following example shows how to create a custom plot. Here, we sample some points from a Gaussian distribution, use them to compute a linear transformation, and plot at the same time the distribution of sampled points (as a histogram and box plot), the sampled points themselves, a trendline, and the equation plotted by the trendline. See the pages describing the various [plot elements]({{ site.baseurl }}{% link plots.elements.md %}) for more information about each plot element.

<div class="code-example">
    <iframe src="assets/images/plots/custom.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

// Sample some points from a Gaussian distribution
double[] samples = Normal.Samples(10, 2).Take(250).ToArray();

// Used to add some randomness to the data.
Random rnd = new Random();

// Use the sampled points to compute a linear function.
double[][] data = (from x in samples select new double[] { x, 2 * x + 3 + rnd.NextDouble() * 10 }).ToArray();

// Create the coordinate system for the points.
LinearCoordinateSystem2D mainCoordinateSystem = new LinearCoordinateSystem2D(data);

// Create the plot element that will plot the points.
ScatterPoints<IReadOnlyList<double>> scatterPoints = new ScatterPoints<IReadOnlyList<double>>(data, mainCoordinateSystem)
{
    PresentationAttributes = new PlotElementPresentationAttributes()
    {
        Stroke = null,
        Fill = Colour.FromRgb(0, 114, 178) // Draw points in blue.
    }
};

// Create the plot element that will draw the trendline.
LinearTrendLine trendline = new LinearTrendLine(data, mainCoordinateSystem);

// Bin the samples to create the histogram.
int binCount = 20;
double minSample = samples.Min();
double maxSample = samples.Max();
double binWidth = (maxSample - minSample) / binCount;
int[] bins = new int[binCount];

foreach (double sample in samples)
{
    bins[Math.Min((int)((sample - minSample) / binWidth), binCount - 1)]++;
}

double[][] binnedData = bins.Select((x, i) => new double[] { minSample + binWidth * (i + 0.5), bins[i] }).ToArray();

// Create a new coordinate system for the histogram.
LinearCoordinateSystem2D histogramCoordinateSystem = new LinearCoordinateSystem2D(mainCoordinateSystem.MinX, mainCoordinateSystem.MaxX, 0, bins.Max(), 350, 250);

// Create the plot element that draws the histogram.
Bars bars = new Bars(binnedData, histogramCoordinateSystem)
{
    Margin = 0.1,
    PresentationAttributes = new PlotElementPresentationAttributes()
    {
        Stroke = null,
        Fill = Colour.FromRgba(0, 0, 0, 32)
    }
};

// Compute some statistics for the box plot.
double quart1 = samples.LowerQuartile();
double median = samples.Median();
double quart3 = samples.UpperQuartile();
double mean = samples.Mean();
double stdDev = samples.StandardDeviation();
double whisker1 = Math.Max(mean - 2 * stdDev, minSample);
double whisker2 = Math.Min(mean + 2 * stdDev, maxSample);

// Create the plot element that will plot the box plot.
BoxPlot box = new BoxPlot(new double[] { median, 0 }, new double[] { 1, 0 }, whisker1 - median, quart1 - median, quart3 - median, whisker2 - median, histogramCoordinateSystem)
{
    Width = histogramCoordinateSystem.MaxY * 0.05,
    WhiskersPresentationAttributes = new PlotElementPresentationAttributes() { LineWidth = 2, Stroke = Colour.FromRgb(0, 114, 178) },
    BoxPresentationAttributes = new PlotElementPresentationAttributes() { Stroke = Colour.FromRgb(0, 114, 178), Fill = Colour.FromRgb(213, 240, 255) }
};

// Create the plot element that will display the equation
TextLabel<IReadOnlyList<double>> textLabel = new TextLabel<IReadOnlyList<double>>("<i>y</i> = " + trendline.Slope.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + " <i>x</i> + " + trendline.Intercept.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture), new double[] { samples.Min(), bins.Max() * 0.95 }, histogramCoordinateSystem)
{
    Alignment = TextAnchors.Left,
    Baseline = TextBaselines.Top,
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.TimesRoman), 12) }

};

// Create the X axis
ContinuousAxis xAxis = new ContinuousAxis(new double[] { minSample - (maxSample - minSample) * 0.05, 0 }, new double[] { maxSample + (maxSample - minSample) * 0.05, 0 }, histogramCoordinateSystem);

// Ticks for the X axis
ContinuousAxisTicks xTicks = new ContinuousAxisTicks(new double[] { minSample, 0 }, new double[] { maxSample, 0 }, histogramCoordinateSystem)
{
    SizeAbove = _ => 0,
    SizeBelow = _ => 20,
    PresentationAttributes = new PlotElementPresentationAttributes() { LineDash = new LineDash(3, 3, 0) },
    IntervalCount = 7
};

// Labels for the X axis
ContinuousAxisLabels xLabels = new ContinuousAxisLabels(new double[] { minSample, 0 }, new double[] { maxSample, 0 }, histogramCoordinateSystem)
{
    Rotation = 0,
    Alignment = TextAnchors.Center,
    Baseline = TextBaselines.Top,
    Position = _ => 25,
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null },
    IntervalCount = 7
};

// Create the Y axis
ContinuousAxis yAxis = new ContinuousAxis(new double[] { minSample - (maxSample - minSample) * 0.05, 0 }, new double[] { minSample - (maxSample - minSample) * 0.05, bins.Max() }, histogramCoordinateSystem);

// Ticks for the Y axis
ContinuousAxisTicks yTicks = new ContinuousAxisTicks(new double[] { minSample - (maxSample - minSample) * 0.05, data.Select(x => x[1]).Min() }, new double[] { minSample - (maxSample - minSample) * 0.05, data.Select(x => x[1]).Max() }, mainCoordinateSystem);

// Labels for the Y axis
ContinuousAxisLabels yLabels = new ContinuousAxisLabels(new double[] { minSample - (maxSample - minSample) * 0.05, data.Select(x => x[1]).Min() }, new double[] { minSample - (maxSample - minSample) * 0.05, data.Select(x => x[1]).Max() }, mainCoordinateSystem)
{
    Alignment = TextAnchors.Right,
    Baseline = TextBaselines.Middle,
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null },
    Position = _ => -10
};

// Create an empty plot.
Plot plot = new Plot();

// Add the plot elements to the plot.
plot.AddPlotElements(xTicks, xLabels, xAxis, yTicks, yLabels, yAxis, bars, box, scatterPoints, trendline, textLabel);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG("customPlot.svg");
{% endhighlight %}