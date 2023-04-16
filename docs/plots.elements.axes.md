---
layout: default
nav_order: 1
grand_parent: Plots
parent: Plot elements
---

# Axes

Elements involved in creating plot axes include the classes `ContinuousAxis` (which draws the axis line and arrow), `ContinuousAxisTicks` (which draws the tick marks), `ContinuousAxisLabels` (which draws the labels), `ContinuousAxisTitle` (which draws the title), and `Grid` (which draws a grid).

The following example shows how to use these elements:

<div class="code-example">
    <iframe src="assets/images/plots/axes.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
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

// Create an empty plot.
Plot plot = new Plot();

// Create a coordinate system.
LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(0, 10, 0, 10, 350, 250);

// Create a horizontal grid.
Grid gridHorizontal = new Grid(new double[] { 0, 0 }, new double[] { 0, 10 },    // First side
                               new double[] { 10, 0 }, new double[] { 10, 10 },  // Second side
                               coordinateSystem)
{
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = Colour.FromRgba(0, 114, 178, 0.25) }  // Semi-transparent blue
};

// Create a vertical grid.
Grid gridVertical = new Grid(new double[] { 0, 0 }, new double[] { 10, 0 },    // First side
                             new double[] { 0, 10 }, new double[] { 10, 10 },  // Second side
                             coordinateSystem)
{
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = Colour.FromRgba(213, 94, 0, 0.25) }  // Semi-transparent orange
};

// Create the horizontal axis. This goes up to 10.5 so that the arrow tip does not interfere with the ticks.
ContinuousAxis xAxis = new ContinuousAxis(new double[] { 0, 0 }, new double[] { 10.5, 0 }, coordinateSystem)
{
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = Colour.FromRgb(0, 114, 178), Fill = Colour.FromRgb(0, 114, 178) }  // Blue
};

// Create the vertical axis. This goes up to 10.5 so that the arrow tip does not interfere with the ticks.
ContinuousAxis yAxis = new ContinuousAxis(new double[] { 0, 0 }, new double[] { 0, 10.5 }, coordinateSystem)
{
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = Colour.FromRgb(213, 94, 0), Fill = Colour.FromRgb(213, 94, 0) }  // Orange
};

// Create ticks for the horizontal axis
ContinuousAxisTicks xAxisTicks = new ContinuousAxisTicks(new double[] { 0, 0 }, new double[] { 10, 0 }, coordinateSystem)
{
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = Colour.FromRgb(0, 114, 178) }  // Blue
};

// Create ticks for the vertical axis
ContinuousAxisTicks yAxisTicks = new ContinuousAxisTicks(new double[] { 0, 0 }, new double[] { 0, 10 }, coordinateSystem)
{
    PresentationAttributes = new PlotElementPresentationAttributes() { Stroke = Colour.FromRgb(213, 94, 0) }  // Orange
};

// Create labels for the horizontal axis
ContinuousAxisLabels xAxisLabels = new ContinuousAxisLabels(new double[] { 0, 0 }, new double[] { 10, 0 }, coordinateSystem)
{
    Rotation = 0, // Horizontal
    Alignment = TextAnchors.Center,
    Baseline = TextBaselines.Top,
    PresentationAttributes = new PlotElementPresentationAttributes() { Fill = Colour.FromRgb(0, 114, 178), Stroke = null }  // Blue
};

// Create labels for the vertical axis
ContinuousAxisLabels yAxisLabels = new ContinuousAxisLabels(new double[] { 0, 0 }, new double[] { 0, 10 }, coordinateSystem)
{
    Position = _ => -10,
    Alignment = TextAnchors.Right,
    Baseline = TextBaselines.Middle,
    PresentationAttributes = new PlotElementPresentationAttributes() { Fill = Colour.FromRgb(213, 94, 0), Stroke = null }  // Orange
};

// Create a title for the horizontal axis
ContinuousAxisTitle xAxisTitle = new ContinuousAxisTitle("X axis", new double[] { 0, 0 }, new double[] { 10, 0 }, coordinateSystem)
{
    Alignment = TextAnchors.Center,
    Baseline = TextBaselines.Top
};
xAxisTitle.PresentationAttributes.Fill = Colour.FromRgb(0, 114, 178); // Blue

// Create a title for the vertical axis
ContinuousAxisTitle yAxisTitle = new ContinuousAxisTitle("Y axis", new double[] { 0, 0 }, new double[] { 0, 10 }, coordinateSystem)
{
    Alignment = TextAnchors.Center,
    Baseline = TextBaselines.Bottom,
    Position = -40
};
yAxisTitle.PresentationAttributes.Fill = Colour.FromRgb(213, 94, 0); // Orange

// Add the plot elements to the plot.
plot.AddPlotElements(gridHorizontal, gridVertical, xAxis, yAxis, xAxisTicks, yAxisTicks, xAxisLabels, yAxisLabels, xAxisTitle, yAxisTitle);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"axes.svg");
{% endhighlight %}
</details>

## The `ContinuousAxis` class

A `ContinuousAxis` draws the axis line and its arrow. An instance of this class can be created using the constructor, which requires a `startPoint`, `endPoint`, and `coordinateSystem`. The `startPoint` and `endPoint` should be expressed in data space coordinates, and the `coordinateSystem` is used to convert these to plot space.

## The `ContinuousAxisTicks` class

This plot elements draws the ticks on an axis. The constructor for this class is the same as for the `ContinuousAxis` class; by default, 11 ticks (determining 10 intervals) are drawn on the axis. For example, if the axis goes from `(0, 0)` to `(10, 0)` in data coordinates, ticks will be drawn at `(0, 0)`, `(1, 0)`, ..., `(10, 0)`. You can change this by changing the value of the `IntervalCount` property, and you can change the size of the ticks with the `SizeAbove` and `SizeBelow` properties. These should be set to a function delegate taking as a parameter the tick index and returning the size of the tick in plot coordinates.

Note that the ticks are necessarily drawn at equally-spaced intervals. If you wish to draw ticks at specific positions, you should instead use something like a `ScatterPoints` plot element, where you can specify the position at which to draw the symbol and the symbol you want to draw.

## The `ContinuousAxisLabels` class

This plot elements draws the labels on an axis. The constructor for this class is the same as for the `ContinuousAxis` class; by default, 11 labels (determining 10 intervals) are drawn on the axis. For example, if the axis goes from `(0, 0)` to `(10, 0)` in data coordinates, labels will be drawn at `(0, 0)`, `(1, 0)`, ..., `(10, 0)`. You can change this by changing the value of the `IntervalCount` property. Other properties of this class can be used to determine the position, orientation and text of the labels.

Note that the labels are necessarily drawn at equally-spaced intervals. If you wish to draw ticks at specific positions, you should instead use something like a `DataLabels` plot element, where you can specify the position at which to draw the labels.

## The `ContinuousAxisTitle` class

This plot element draws a title for an axis. The constructor for this class requires the title to draw (either as a `string` or as an `IEnumerable<FormattedText>`), the starting and ending points for the axis, the coordinate system and (if the title was provided as a `string`) the presentation attributes.

Note that if you provide the title as an `IEnumerable<FormattedText>`, the font properties of the presentation attributes will not be used when drawing the title (the information within the `FormattedText` objects will be used instead). Furthermore, even when providing the title as a `string`, the font from the presentation attributes is only used in the constructor when creating the object. This means that if you change the element's `PresentationAttributes` later on, only things like the colour of the title will change, but not the font or the font size; if you wish to change the font or font size, you will need to change the value of the `IEnumerable<FormattedText> title` property.

## The `Grid` class

This plot element draws a grid. This is specified by providing four points (and a coordinate system) to the constructor for this class; the four points identify two "sides", and grid lines are drawn from equally spaced points on the first side to the corresponding equally spaced points on the second side. Note that the two sides do not necessarily need to be parallel or have the same length.

