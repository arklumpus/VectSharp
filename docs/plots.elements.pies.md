---
layout: default
nav_order: 4
grand_parent: Plots
parent: Plot elements
---

# Pies and doughnuts

Pies and doughnuts are used to represent relative proportions of data. These can be drawn using the `Pie` class; this class can be used to draw both pies and doughnuts.

The following example shows how to use it:

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

<iframe src="assets/images/plots/pie2.svg" style="width: 100%; height: 25em; border: 0px solid black; display: block" id="plotPie"></iframe>
<iframe src="assets/images/plots/doughnut2.svg" style="width: 100%; height: 25em; border: 0px solid black; display: none" id="plotDoughnut"></iframe>

<p style="text-align: center; position: relative">
    <span style="display: inline-block; width: 16em; background: rgb(240, 240, 240); padding: 0.5em; border-radius: 0.5em; position: absolute; top: 0; left: calc(50% - 8em); font-size: 1.25em; z-index: -1; transition: left 200ms ease-in-out">&nbsp;</span>
    <span style="display: inline-block; width: 8em; background: rgb(220, 220, 220); padding: 0.5em; border-radius: 0.5em; position: absolute; top: 0; left: calc(50% - 8em); font-size: 1.25em; z-index: -1; transition: left 200ms ease-in-out" id="itemSelector">&nbsp;</span>
    <input type="radio" name="displayItem" id="pieButton" checked class="radio"><label for="pieButton" class="radioLabel">Pie</label><input type="radio" name="displayItem" id="doughnutButton" class="radio"><label for="doughnutButton" class="radioLabel">Doughnut</label>
</p>

<script>
    function showHide()
    {
        if (document.getElementById("pieButton").checked)
        {
            document.getElementById("plotPie").style.display = "block";
            document.getElementById("plotDoughnut").style.display = "none";
            document.getElementById("itemSelector").style.left = "calc(50% - 8em)";
        }
        else if (document.getElementById("doughnutButton").checked)
        {
            document.getElementById("plotPie").style.display = "none";
            document.getElementById("plotDoughnut").style.display = "block";
            document.getElementById("itemSelector").style.left = "calc(50%)";
        }
    }

    document.getElementById("pieButton").onclick = showHide;
    document.getElementById("doughnutButton").onclick = showHide;
</script>

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

// Pie chart
{
    // Create some random data.
    Random rnd = new Random();
    double[] data = new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) };

    // Create a linear coordinate system.
    LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(-1, 1, -1, 1, 250, 250);

    // Create the pie chart plot element.
    Pie pie = new Pie(data, new double[] { 0, 0 }, new double[] { 1, 1 }, coordinateSystem)
    {
        PresentationAttributes = new PlotElementPresentationAttributes[]
        {
            new PlotElementPresentationAttributes(){ Stroke= null, Fill = Colour.FromRgb(0, 114, 178) },
            new PlotElementPresentationAttributes(){ Stroke= null, Fill = Colour.FromRgb(213, 94, 0) },
            new PlotElementPresentationAttributes(){ Stroke= null, Fill = Colour.FromRgb(204, 121, 167) },
            new PlotElementPresentationAttributes(){ Stroke= null, Fill = Colour.FromRgb(230, 159, 0) },
        }
    };

    // Create the plot.
    Plot plot = new Plot();

    // Add the pie to the plot.
    plot.AddPlotElement(pie);

    // Render the plot to a Page and save it as an SVG document.
    Page pag = plot.Render();
    pag.SaveAsSVG("pie.svg");
}

// Doughnut chart
{
    // Create some random data.
    Random rnd = new Random();
    double[] data = new double[] { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) };

    // Create a linear coordinate system.
    LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(-1, 1, -1, 1, 250, 250);

    // Create the doughnut chart plot element.
    Pie doughnut = new Pie(data, new double[] { 0, 0 }, new double[] { 0.5, 0.5 }, new double[] { 1, 1 }, coordinateSystem)
    {
        PresentationAttributes = new PlotElementPresentationAttributes[]
        {
            new PlotElementPresentationAttributes(){ Stroke= null, Fill = Colour.FromRgb(0, 114, 178) },
            new PlotElementPresentationAttributes(){ Stroke= null, Fill = Colour.FromRgb(213, 94, 0) },
            new PlotElementPresentationAttributes(){ Stroke= null, Fill = Colour.FromRgb(204, 121, 167) },
            new PlotElementPresentationAttributes(){ Stroke= null, Fill = Colour.FromRgb(230, 159, 0) },
        }
    };

    // Create the plot.
    Plot plot = new Plot();

    // Add the doughnut to the plot.
    plot.AddPlotElement(doughnut);

    // Render the plot to a Page and save it as an SVG document.
    Page pag = plot.Render();
    pag.SaveAsSVG("doughnut.svg");
}
{% endhighlight %}
</details>

## The `Pie` class

This plot element draws both pie charts and doughnut charts. Whether a pie or a doughnut is drawn depends on the value of the `InnerRadius` property: if this is `[0, 0]`, a pie is drawn; otherwise, this determines the size of the "hole" and a doughnut is drawn. The `Centre` of the pie/doughnut is specified in data coordinates (which are then converted by the `CoordinateSystem`), but also the `InnerRadius` and `OuterRadius` are specified as vectors in the data coordinate space. The first and second elements of these arrays specify, respectively, the width and the height of the pie chart. Depending on the values for these and on the coordinate system, the chart may be a perfect circle, or it may be more or less "squished".
