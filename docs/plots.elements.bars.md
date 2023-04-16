---
layout: default
nav_order: 3
grand_parent: Plots
parent: Plot elements
---

# Bars

Bars are used to create plots such as histograms and bar charts. The main plot elements used to plot bars are the classes `Bars<T>` (plotting individual bars), `StackedBars` (plotting stacked bars), and `ClusteredBars` (plotting clustered bars). The classes `CategoricalBars<T>` and `Bars` (without the generic type argument) are shorthands for particular types of `Bars<T>`.

For all of these classes, the underlying `Data` consists in data points representing the tips of the bars; they all have a `GetBaseline` property, which can be used to set a function that determines the starting point (e.g., bottom) of each bar. In this way, it is possible to specify bars that expand in any direction (horizontal, vertical, etc.).

The following example shows how to use these elements:

<div class="code-example">

<style>
    .radio
    {
        display: none;
    }

    .radioLabel
    {
        display: inline-block;
        padding: 0.5em;
        border-radius: 0.5em;
        width: 8em;
        text-align: center;
        font-size: 1.25em;
        cursor: pointer;
    }
</style>

<iframe src="assets/images/plots/bars2.svg" style="width: 100%; height: 25em; border: 0px solid black; display: block" id="plotBars"></iframe>
<iframe src="assets/images/plots/stacked_bars2.svg" style="width: 100%; height: 25em; border: 0px solid black; display: none" id="stackedBars"></iframe>
<iframe src="assets/images/plots/clustered_bars2.svg" style="width: 100%; height: 25em; border: 0px solid black; display: none" id="clusteredBars"></iframe>

<p style="text-align: center; position: relative">
    <span style="display: inline-block; width: 24em; background: rgb(240, 240, 240); padding: 0.5em; border-radius: 0.5em; position: absolute; top: 0; left: calc(50% - 12em); font-size: 1.25em; z-index: -1; transition: left 200ms ease-in-out">&nbsp;</span>
    <span style="display: inline-block; width: 8em; background: rgb(220, 220, 220); padding: 0.5em; border-radius: 0.5em; position: absolute; top: 0; left: calc(50% - 12em); font-size: 1.25em; z-index: -1; transition: left 200ms ease-in-out" id="itemSelector">&nbsp;</span>
    <input type="radio" name="displayItem" id="barButton" checked class="radio"><label for="barButton" class="radioLabel">Bars</label><input type="radio" name="displayItem" id="stackedBarsButton" class="radio"><label for="stackedBarsButton" class="radioLabel">Stacked bars</label><input type="radio" name="displayItem" id="clusteredBarsButton" class="radio"><label for="clusteredBarsButton" class="radioLabel">Clustered bars</label>
</p>

<script>
    function showHide()
    {
        if (document.getElementById("barButton").checked)
        {
            document.getElementById("plotBars").style.display = "block";
            document.getElementById("stackedBars").style.display = "none";
            document.getElementById("clusteredBars").style.display = "none";
            document.getElementById("itemSelector").style.left = "calc(50% - 12em)";
        }
        else if (document.getElementById("stackedBarsButton").checked)
        {
            document.getElementById("plotBars").style.display = "none";
            document.getElementById("stackedBars").style.display = "block";
            document.getElementById("clusteredBars").style.display = "none";
            document.getElementById("itemSelector").style.left = "calc(50% - 4em)";
        }
        else if (document.getElementById("clusteredBarsButton").checked)
        {
            document.getElementById("plotBars").style.display = "none";
            document.getElementById("stackedBars").style.display = "none";
            document.getElementById("clusteredBars").style.display = "block";
            document.getElementById("itemSelector").style.left = "calc(50% + 4em)";
        }
    }

    document.getElementById("barButton").onclick = showHide;
    document.getElementById("stackedBarsButton").onclick = showHide;
    document.getElementById("clusteredBarsButton").onclick = showHide;
</script>

</div>
<details markdown="block">
<summary>
    Expand source code
  </summary>
  {: .text-delta }
{% highlight CSharp %}
// Individual bars.
{
    // Generate some random data.
    Random rnd = new Random();
    double[][] data = (from el in Enumerable.Range(0, 20) select new double[] { el, rnd.NextDouble() * 100 }).ToArray();

    // Create a linear coordinate system.
    LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(0, 20, 0, 100, 350, 250);

    // Create the bars.
    Bars bars = new Bars(data, coordinateSystem)
    {
        PresentationAttributes = new PlotElementPresentationAttributes() { Fill = Colour.FromRgb(0, 114, 178), Stroke = null },
        Margin = 0.1
    };

    // Create the plot.
    Plot plot = new Plot();

    // Add the bars to the plot.
    plot.AddPlotElements(bars);

    // Render the plot to a Page and save it as an SVG document.
    Page pag = plot.Render();
    pag.SaveAsSVG(@"C:\Users\Giorgio\Downloads\bars.svg");
}

// Stacked bars.
{
    // Generate some random data.
    Random rnd = new Random();
    double[][] data = (from el in Enumerable.Range(0, 20) select new double[] { el, rnd.NextDouble() * 100, rnd.NextDouble() * 100, rnd.NextDouble() * 100 }).ToArray();

    // Create a linear coordinate system.
    LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(0, 20, 0, 300, 350, 250);

    // Create the stacked bars.
    StackedBars bars = new StackedBars(data, coordinateSystem)
    {
        PresentationAttributes = new PlotElementPresentationAttributes[] {
            new PlotElementPresentationAttributes() { Fill = Colour.FromRgb(0, 114, 178), Stroke = null },
            new PlotElementPresentationAttributes() { Fill = Colour.FromRgb(213, 94, 0), Stroke = null },
            new PlotElementPresentationAttributes() { Fill = Colour.FromRgb(0, 158, 115), Stroke = null } },
        Margin = 0.1
    };

    // Create the plot.
    Plot plot = new Plot();

    // Add the bars to the plot.
    plot.AddPlotElements(bars);

    // Render the plot to a Page and save it as an SVG document.
    Page pag = plot.Render();
    pag.SaveAsSVG(@"C:\Users\Giorgio\Downloads\stacked_bars.svg");
}

// Clustered bars.
{
    // Generate some random data.
    Random rnd = new Random();
    double[][] data = (from el in Enumerable.Range(0, 10) select new double[] { el, rnd.NextDouble() * 100, rnd.NextDouble() * 100, rnd.NextDouble() * 100 }).ToArray();

    // Create a linear coordinate system.
    LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(0, 10, 0, 100, 350, 250);

    // Create the clustered bars.
    ClusteredBars bars = new ClusteredBars(data, coordinateSystem)
    {
        PresentationAttributes = new PlotElementPresentationAttributes[] {
            new PlotElementPresentationAttributes() { Fill = Colour.FromRgb(0, 114, 178), Stroke = null },
            new PlotElementPresentationAttributes() { Fill = Colour.FromRgb(213, 94, 0), Stroke = null },
            new PlotElementPresentationAttributes() { Fill = Colour.FromRgb(0, 158, 115), Stroke = null } },
        InterClusterMargin = 0.25,
        IntraClusterMargin = 0.05
    };

    // Create the plot.
    Plot plot = new Plot();

    // Add the bars to the plot.
    plot.AddPlotElements(bars);

    // Render the plot to a Page and save it as an SVG document.
    Page pag = plot.Render();
    pag.SaveAsSVG(@"C:\Users\Giorgio\Downloads\clustered_bars.svg");
}
{% endhighlight %}
</details>



## The `Bars<T>` class

This class represents the most generic kind of bars. The type parameter `T` represents the underlying data time (which is also used by the coordinate system). If your data is an `IReadOnlyList<double>` or a tuple like `(T, double)`, you should use the `Bars` (non-generic) or the `CategoricalBars<T>` classes instead.

To draw the bars, this class uses the `GetBaseline` function to determine the starting point of each bar in data coordinates, then uses the coordinate system to convert these to plot coordinates. This provides an axis along which the bar is centred; the thickness of the bar is determined by looking at the preceding and following bar, such that the bars fill all the available space. Thus, the bars will all have the same width if they are equally spaced; otherwise, they will have different widths.

By manipulating the coordinate system and/or the values returned by the `GetBaseline` function, you can determine the alignment of the bars. For example, these could be aligned horizontally or vertically, but they do not necessarily have to flow along a straight line.

This class has multiple constructors, depending on the kind of data collection that you use. The constructor that accepts an `IReadOnlyList<T> data` parameter assumes that the data are already sorted, and uses the intrinsic ordering in the list. The other constructors require a sorting function to sort the data. The data must be sorted, so that it is possible to define the "previous" and "following" bar for each bar.

## The `Bars` class

The non-generic `Bars` class is roughly equivalent to a `Bars<IReadOnlyList<double>>`. The main difference is that it has an additional constructor, accepting an optional `bool vertical` parameter; this constructor automatically sets up the `GetBaseline` function in order to create horizontal or vertical bars, depending on the value of this parameter. Most of the time, this is the class that you will be using.

## The `CategoricalBars<T>` class

The `CategoricalBars<T>` is roughly equivalent to a `Bars<(T, double)>`. The main difference is that it has two constructors that do not accept a `getBaseline` parameter, and instead set up the `GetBaseline` function so that for each `(T x, double y)` it returns `(x, 0)`.

## The `StackedBars` class

This plot element draws stacked bars, i.e., multiple sets of bars, one on top of the other. The data for this plot element are provided as an `IEnumerable` of `IReadOnlyList<double>` data elements. Each data element contains an entry determining the position of the bar stack, and one or more entries determining the heights for each bar in the stack. There are three constructors: two of these are equivalent, and require you to specify a method to sort the data (either as a `Comparison` or as an `IComparer`) and to determine the baseline.

The other constructor, other than the data and coordinate system, only has one additional optional parameter, `bool vertical`. If this is `true` (the default), the first entry in each data element is assumed to represent the `X` coordinate of the bar stack, and all the other entries represent the lengths of the stacked bars, which are drawn vertically. If this is `false`, the second entry in each data element is assumed to represent the `Y` coordinate of the bar stack, while the first entry represents the length of the first stacked bar, and the other entries represent the lengths of the other bars (drawn horizontally).

## The `ClusteredBars` class

This plot element draws clustered bars, i.e., multiple bars next to each other for each data point. Similarly to the `StackedBars` class, the data for this plot element are provided as an `IEnumerable` of `IReadOnlyList<double>` data elements. Each data element contains an entry determining the position of the bar cluster, and one or more entries determining the heights for each bar in the cluster. There are again three constructors for this class, equivalent to those described for the `StackedBars` class.