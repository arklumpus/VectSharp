---
layout: default
nav_order: 2
grand_parent: Plots
parent: Plot elements
---

# Data points

Plot elements that can be used to highlight data points include the classes `ScatterPoints` (which draws a symbol at each point location), `TextLabel` (which draws a single text label at the specified point), `DataLabels` (which draws a label at each point location), `DataLine` (which draws a line connecting multiple points), and `Area` (which shades an area between two lines).

All of these classes have a generic type parameter `T` that defines the kind of data point that they use to determine the coordinates of the points or the labels. In most instances, you will want `T` to be `IReadOnlyList<double>`, but there may be instances when you want to use a different data kind (e.g., for categorical data).

The following example shows how to use these elements:

<div class="code-example">
    <iframe src="assets/images/plots/datapoints.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
</div>
<details markdown="block">
<summary>
    Expand source code
  </summary>
  {: .text-delta }
{% highlight CSharp %}
// Create some data points
double[][] data = new double[][] { new double[] { 0, 0.5 }, new double[] { 1, 1.5 }, new double[] { 2, 2 }, new double[] { 3, 2.5 }, new double[] { 4, 4.5 }, new double[] { 5, 5 }, new double[] { 6, 6.5 }, new double[] { 7, 7 }, new double[] { 8, 7.5 }, new double[] { 9, 9.5 }, new double[] { 10, 10.5 } };

// Create an empty plot.
Plot plot = new Plot();

// Create a coordinate system.
LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(0, 10, 0, 10, 350, 250);

// Create the scatter points.
ScatterPoints<IReadOnlyList<double>> scatterPoints = new ScatterPoints<IReadOnlyList<double>>(data, coordinateSystem)
{
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Fill = Colour.FromRgb(0, 114, 178) } // Blue
};

// Create the data labels, plotting the labels down and to the right of the points.
// By default, the labels will contain the index of the point.
DataLabels<IReadOnlyList<double>> dataLabels = new DataLabels<IReadOnlyList<double>>(data, coordinateSystem)
{
    Margin = (_, _) => new Point(10, 10)
};

// Create a data line.
DataLine<IReadOnlyList<double>> dataLine = new DataLine<IReadOnlyList<double>>(data, coordinateSystem)
{
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = Colour.FromRgb(86, 180, 233) } // Light blue
};

// Create the area. This will highlight an area from the data line shifted upwards by 1 to the data line shifted downwards by 1.
Area<IReadOnlyList<double>> area = new Area<IReadOnlyList<double>>(
    (from el in data select new double[] { el[0], el[1] + 1 }),
    p => new double[] { p[0], p[1] - 2 },
    coordinateSystem)
{
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = null, Fill = Colour.FromRgba(0, 114, 178, 0.25) } // Semi-transparent blue.
};

// Create the text label
TextLabel<IReadOnlyList<double>> textLabel = new TextLabel<IReadOnlyList<double>>("Text label", new double[] { 5.5, 4 }, coordinateSystem)
{
    Alignment = TextAnchors.Left,
    Baseline = TextBaselines.Top
};

// Add the plot elements to the plot.
plot.AddPlotElements(area, dataLine, scatterPoints, dataLabels, textLabel);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"datapoints.svg");
{% endhighlight %}
</details>

## The `ScatterPoints` class

This plot element draws a symbol at each data point. The constructor for this class takes an `IEnumerable<T>` parameter representing the data points, as well as a coordinate system for converting them to plot coordinates. Most of the time, the type parameter `T` will be `IReadOnlyList<double>`, which will allow the use of a regular coordinate system (e.g., linear or logarithmic coordinates).

The symbol that is drawn is determined by the value of the `DataPointElement` property, which should be an instance of a class implementing the `IDataPointElement` class.

### The `IDataPointElement` interface

This interface is used to define symbols to draw at particular points on the plot. The assumption is that the `Graphics` object on which the plot is being created will be transformed in such a way that the symbol can be drawn centred at `(0, 0)`. This means that implementations of this interface can be agnostic towards the position of the symbol and its size (because these will be taken care, e.g., by the `ScatterPoints` class).

There are currently three implementations of this interface:

* The `PathDataPointElement` class, which represents a symbol defined by a `GraphicsPath` object (by default, a circle). The graphics path is filled and/or stroked using the attributes from the `PresentationAttributes` object that is supplied to the `IDataPointElement.Plot` method.

* The `GraphicsDataPointElement` class, which represents a symbol defined by a `Graphics` object that is copied on the plot. In this case, the presentation attributes provided to the `Plot` method will have no effect.

* The `ActionDataPointElement` class, which represents a symbol that is drawn by a custom `Action`, supplied to the constructor for this class. This `Action` is responsible for drawing the symbol, and could do anything, in principle.

## The `TextLabel` class

This plot element draws a single text label on the plot, at the specified position, which is provided in the constructor, together with the text to draw and the coordinate system. Properties of this class can be used to determine the orientation and the alignment of the label.

## The `DataLabels` class

This plot element is similar to the `ScatterPoints` class, but instead of drawing a symbol at each data point, it draws a text label. The label to draw is determined by the `Label` property, which should be set to a function accepting the label index and coordinates, and returning the text of the label (either as a `string`, an `IEnumerable<FormattedText>`, or another kind of object that will be converted into a string in order to be displayed).

## The `DataLine` class

This plot element draws a line passing through all the data points. The line is drawn from the first to the last point in the collection, thus the points should be reasonably ordered.

## The `Area` class

This plot element is very similar to the `DataLine` class, but rather than just drawing a single line, it fills an area comprised between a line and another line (the "baseline"). The baseline is defined by the `GetBaseline` property, which should be set to a function accepting a single data point parameter and returning the corresponding baseline point. For example, if the data are represente as `IReadOnlyList<double>` and the baseline should be the `X` axis, you could use something like `p => new double[] { p[0], 0 }`.