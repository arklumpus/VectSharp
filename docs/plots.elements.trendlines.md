---
layout: default
nav_order: 5
grand_parent: Plots
parent: Plot elements
---

# Trendlines

Trendlines represent a model of the relationship between two variables. Multiple kinds of trendlines can be added to plots: 

* A `LinearTrendLine` represents a linear relationship $y = a \cdot x + b$.
* An `ExponentialTrendLine` represents a relationship of the form $y = b \cdot \mathrm{e}^{a \cdot x}$.
* A `LogarithmicTrendLine` represents a relationship of the form $y = a \cdot \mathrm{ln} \left ( x \right ) + b$.
* A `PowerLawTrendLine` represents a relationship of the form $y = b \cdot x^a$.
* A `PolynomialTrendLine` represents a relationship of the form $y = \sum_{i = 0}^n a_i \cdot x^i$.
* A `MovingAverageTrendLine` represents a moving average (see below).

Trendlines can be created either by manually specifying the parameters of the functions, or by providing the constructor with the data and letting it perform a regression to automatically determine their values. With the exception of the `MovingAverageTrendLine` and the `PolynomialTrendLine`, all trendline classes have a `Slope` and `Intercept` property, which represent, respectively, $a$ and $b$ in the equations above. They also have `MinX`, `MaxX`, `MinY` and `MaxY` properties, which define the range in which the trendline is drawn.

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
        padding-top: 0.5em;
        padding-bottom: 0.5em;
        border-radius: 0.5em;
        width: 8em;
        text-align: center;
        font-size: 1.25em;
        cursor: pointer;
    }
</style>

<iframe src="assets/images/plots/linearTrendline.svg" style="width: 100%; height: 25em; border: 0px solid black; display: block" id="linearTrendline"></iframe>
<iframe src="assets/images/plots/exponentialTrendline.svg" style="width: 100%; height: 25em; border: 0px solid black; display: none" id="exponentialTrendline"></iframe>
<iframe src="assets/images/plots/logarithmicTrendline.svg" style="width: 100%; height: 25em; border: 0px solid black; display: none" id="logarithmicTrendline"></iframe>
<iframe src="assets/images/plots/powerLawTrendline.svg" style="width: 100%; height: 25em; border: 0px solid black; display: none" id="powerLawTrendline"></iframe>
<iframe src="assets/images/plots/polynomialTrendline.svg" style="width: 100%; height: 25em; border: 0px solid black; display: none" id="polynomialTrendline"></iframe>
<iframe src="assets/images/plots/movingAverageTrendline.svg" style="width: 100%; height: 25em; border: 0px solid black; display: none" id="movingAverageTrendline"></iframe>

<p style="text-align: center; position: relative">
    <span style="display: inline-block; width: 24em; background: rgb(240, 240, 240); padding-top: 1em; padding-bottom: 1em; border-radius: 0.5em; position: absolute; top: 0; left: calc(50% - 12em); font-size: 1.25em; z-index: -1">&nbsp;<br />&nbsp;</span>
    <span style="display: inline-block; width: 8em; background: rgb(220, 220, 220); padding: 0.5em; border-radius: 0.5em; position: absolute; top: 0; left: calc(50% - 12em); font-size: 1.25em; z-index: -1; transition: left 200ms ease-in-out, top 200ms ease-in-out" id="itemSelector">&nbsp;</span>
    <input type="radio" name="displayItem" id="linearTrendlineButton" checked class="radio"><label for="linearTrendlineButton" class="radioLabel">Linear</label><input type="radio" name="displayItem" id="exponentialTrendlineButton" class="radio"><label for="exponentialTrendlineButton" class="radioLabel">Exponential</label><input type="radio" name="displayItem" id="logarithmicTrendlineButton" class="radio"><label for="logarithmicTrendlineButton" class="radioLabel">Logarithmic</label><br />
    <input type="radio" name="displayItem" id="powerLawTrendlineButton" checked class="radio"><label for="powerLawTrendlineButton" class="radioLabel">Power law</label><input type="radio" name="displayItem" id="polynomialTrendlineButton" class="radio"><label for="polynomialTrendlineButton" class="radioLabel">Polynomial</label><input type="radio" name="displayItem" id="movingAverageTrendlineButton" class="radio"><label for="movingAverageTrendlineButton" class="radioLabel">Moving average</label>
</p>

<script>
    function showHide()
    {
        if (document.getElementById("linearTrendlineButton").checked)
        {
            document.getElementById("linearTrendline").style.display = "block";
            document.getElementById("exponentialTrendline").style.display = "none";
            document.getElementById("logarithmicTrendline").style.display = "none";
            document.getElementById("powerLawTrendline").style.display = "none";
            document.getElementById("polynomialTrendline").style.display = "none";
            document.getElementById("movingAverageTrendline").style.display = "none";
            document.getElementById("itemSelector").style.left = "calc(50% - 12em)";
            document.getElementById("itemSelector").style.top = "0";
        }
        else if (document.getElementById("exponentialTrendlineButton").checked)
        {
            document.getElementById("linearTrendline").style.display = "none";
            document.getElementById("exponentialTrendline").style.display = "block";
            document.getElementById("logarithmicTrendline").style.display = "none";
            document.getElementById("powerLawTrendline").style.display = "none";
            document.getElementById("polynomialTrendline").style.display = "none";
            document.getElementById("movingAverageTrendline").style.display = "none";
            document.getElementById("itemSelector").style.left = "calc(50% - 4em)";
            document.getElementById("itemSelector").style.top = "0";
        }
        else if (document.getElementById("logarithmicTrendlineButton").checked)
        {
            document.getElementById("linearTrendline").style.display = "none";
            document.getElementById("exponentialTrendline").style.display = "none";
            document.getElementById("logarithmicTrendline").style.display = "block";
            document.getElementById("powerLawTrendline").style.display = "none";
            document.getElementById("polynomialTrendline").style.display = "none";
            document.getElementById("movingAverageTrendline").style.display = "none";
            document.getElementById("itemSelector").style.left = "calc(50% + 4em)";
            document.getElementById("itemSelector").style.top = "0";
        }
        else if (document.getElementById("powerLawTrendlineButton").checked)
        {
            document.getElementById("linearTrendline").style.display = "none";
            document.getElementById("exponentialTrendline").style.display = "none";
            document.getElementById("logarithmicTrendline").style.display = "none";
            document.getElementById("powerLawTrendline").style.display = "block";
            document.getElementById("polynomialTrendline").style.display = "none";
            document.getElementById("movingAverageTrendline").style.display = "none";
            document.getElementById("itemSelector").style.left = "calc(50% - 12em)";
            document.getElementById("itemSelector").style.top = "50%";
        }
        else if (document.getElementById("polynomialTrendlineButton").checked)
        {
            document.getElementById("linearTrendline").style.display = "none";
            document.getElementById("exponentialTrendline").style.display = "none";
            document.getElementById("logarithmicTrendline").style.display = "none";
            document.getElementById("powerLawTrendline").style.display = "none";
            document.getElementById("polynomialTrendline").style.display = "block";
            document.getElementById("movingAverageTrendline").style.display = "none";
            document.getElementById("itemSelector").style.left = "calc(50% - 4em)";
            document.getElementById("itemSelector").style.top = "50%";
        }
        else if (document.getElementById("movingAverageTrendlineButton").checked)
        {
            document.getElementById("linearTrendline").style.display = "none";
            document.getElementById("exponentialTrendline").style.display = "none";
            document.getElementById("logarithmicTrendline").style.display = "none";
            document.getElementById("powerLawTrendline").style.display = "none";
            document.getElementById("polynomialTrendline").style.display = "none";
            document.getElementById("movingAverageTrendline").style.display = "block";
            document.getElementById("itemSelector").style.left = "calc(50% + 4em)";
            document.getElementById("itemSelector").style.top = "50%";
        }
    }

    document.getElementById("linearTrendlineButton").onclick = showHide;
    document.getElementById("exponentialTrendlineButton").onclick = showHide;
    document.getElementById("logarithmicTrendlineButton").onclick = showHide;
    document.getElementById("powerLawTrendlineButton").onclick = showHide;
    document.getElementById("polynomialTrendlineButton").onclick = showHide;
    document.getElementById("movingAverageTrendlineButton").onclick = showHide;
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

// Linear trendline
{
    // Create some random data.
    Random rnd = new Random();
    double[][] data = (from el in Enumerable.Range(0, 20) select new double[] { el, 3 + el * 2 + rnd.NextDouble() * 10 }).ToArray();

    // Create a linear coordinate system.
    LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(data, 350, 250);

    // Create a plot element drawing the data points.
    ScatterPoints<IReadOnlyList<double>> points = new ScatterPoints<IReadOnlyList<double>>(data, coordinateSystem);

    // Create the trendline.
    LinearTrendLine trendLine = new LinearTrendLine(data, coordinateSystem);

    // Create the plot.
    Plot plot = new Plot();

    // Add the points and the trendline to the plot.
    plot.AddPlotElements(points, trendLine);

    // Render the plot to a Page and save it as an SVG document.
    Page pag = plot.Render();
    pag.SaveAsSVG("linearTrendline.svg");
}

// Exponential trendline
{
    // Create some random data.
    Random rnd = new Random();
    double[][] data = (from el in Enumerable.Range(0, 20) select new double[] { el, 3 * Math.Exp(el * 0.15) + rnd.NextDouble() * 10 }).ToArray();

    // Create a linear coordinate system.
    LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(data, 350, 250);

    // Create a plot element drawing the data points.
    ScatterPoints<IReadOnlyList<double>> points = new ScatterPoints<IReadOnlyList<double>>(data, coordinateSystem);

    // Create the trendline.
    ExponentialTrendLine trendLine = new ExponentialTrendLine(data, coordinateSystem);

    // Create the plot.
    Plot plot = new Plot();

    // Add the points and the trendline to the plot.
    plot.AddPlotElements(points, trendLine);

    // Render the plot to a Page and save it as an SVG document.
    Page pag = plot.Render();
    pag.SaveAsSVG("exponentialTrendline.svg");
}

// Logarithmic trendline
{
    // Create some random data.
    Random rnd = new Random();
    double[][] data = (from el in Enumerable.Range(1, 20) select new double[] { el, 3 + 2 * Math.Log(el) + rnd.NextDouble() * 2 }).ToArray();

    // Create a linear coordinate system.
    LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(data, 350, 250);

    // Create a plot element drawing the data points.
    ScatterPoints<IReadOnlyList<double>> points = new ScatterPoints<IReadOnlyList<double>>(data, coordinateSystem);

    // Create the trendline.
    LogarithmicTrendLine trendLine = new LogarithmicTrendLine(data, coordinateSystem);

    // Create the plot.
    Plot plot = new Plot();

    // Add the points and the trendline to the plot.
    plot.AddPlotElements(points, trendLine);

    // Render the plot to a Page and save it as an SVG document.
    Page pag = plot.Render();
    pag.SaveAsSVG("logarithmicTrendline.svg");
}

// Power law trendline
{
    // Create some random data.
    Random rnd = new Random();
    double[][] data = (from el in Enumerable.Range(1, 20) select new double[] { el, 2 * Math.Pow(el, 0.5) + rnd.NextDouble() * 2 }).ToArray();

    // Create a linear coordinate system.
    LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(data, 350, 250);

    // Create a plot element drawing the data points.
    ScatterPoints<IReadOnlyList<double>> points = new ScatterPoints<IReadOnlyList<double>>(data, coordinateSystem);

    // Create the trendline.
    PowerLawTrendLine trendLine = new PowerLawTrendLine(data, coordinateSystem);

    // Create the plot.
    Plot plot = new Plot();

    // Add the points and the trendline to the plot.
    plot.AddPlotElements(points, trendLine);

    // Render the plot to a Page and save it as an SVG document.
    Page pag = plot.Render();
    pag.SaveAsSVG("powerLawTrendline.svg");
}

// Polynomal trendline
{
    // Create some random data.
    Random rnd = new Random();
    double[][] data = (from el in Enumerable.Range(1, 20) select new double[] { el, 1 + 10 * el - 1.2 * Math.Pow(el, 2) + 0.04 * Math.Pow(el, 3) + rnd.NextDouble() * 10 }).ToArray();

    // Create a linear coordinate system.
    LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(data, 350, 250);

    // Create a plot element drawing the data points.
    ScatterPoints<IReadOnlyList<double>> points = new ScatterPoints<IReadOnlyList<double>>(data, coordinateSystem);

    // Create the trendline.
    PolynomialTrendLine trendLine = new PolynomialTrendLine(data, 3, coordinateSystem);

    // Create the plot.
    Plot plot = new Plot();

    // Add the points and the trendline to the plot.
    plot.AddPlotElements(points, trendLine);

    // Render the plot to a Page and save it as an SVG document.
    Page pag = plot.Render();
    pag.SaveAsSVG("polynomialTrendline.svg");
}

// Moving average trendline
{
    // Create some random data.
    Random rnd = new Random();
    double[][] data = (from el in Enumerable.Range(1, 20) select new double[] { el, 1 + 10 * el - 1.2 * Math.Pow(el, 2) + 0.04 * Math.Pow(el, 3) + rnd.NextDouble() * 5 }).ToArray();

    // Create a linear coordinate system.
    LinearCoordinateSystem2D coordinateSystem = new LinearCoordinateSystem2D(data, 350, 250);

    // Create a plot element drawing the data points.
    ScatterPoints<IReadOnlyList<double>> points = new ScatterPoints<IReadOnlyList<double>>(data, coordinateSystem);

    // Create the trendline.
    MovingAverageTrendLine trendLine = new MovingAverageTrendLine(data, 2, coordinateSystem);

    // Create the plot.
    Plot plot = new Plot();

    // Add the points and the trendline to the plot.
    plot.AddPlotElements(points, trendLine);

    // Render the plot to a Page and save it as an SVG document.
    Page pag = plot.Render();
    pag.SaveAsSVG("movingAverageTrendline.svg");
}
{% endhighlight %}
</details>

## The `LinearTrendLine` class

The `LinearTrendLine` class represents a simple linear trendline, with the following equation:

$$y = a \cdot x + b$$

This class has three constructors: one allows you to manually specify the `slope` ($a$), `intercept` ($b$), and range of values across which to plot the trendline, while the other two determine these automatically from data provided as an `IReadOnlyList<IReadOnlyList<double>>` (i.e., an array of `double[]`) or as an `IReadOnlyList<(double, double)>` (i.e., an array of `(double, double)` tuples). These also allow you to fix the intercept value.

## The `ExponentialTrendLine` class

The `ExponentialTrendLine` class represents an exponential trendline, with the following equation:

$$y = b \cdot \mathrm{e}^{a \cdot x}$$

Like the `LinearTrendLine` class, this class has three constructors: one allowing you to manually specify the `slope` ($a$), `intercept` ($b$), and range of values across which to plot the trendline, and the other two that determine these automatically from data provided as an `IReadOnlyList<IReadOnlyList<double>>` (i.e., an array of `double[]`) or as an `IReadOnlyList<(double, double)>` (i.e., an array of `(double, double)` tuples). These also allow you to fix the intercept value.

## The `LogarithmicTrendLine` class

The `LogarithmicTrendLine` class represents a logarithmic trendline, with the following equation:

$$y = a \cdot \mathrm{ln} \left ( x \right ) + b$$

Like the `LinearTrendLine` class, this class has three constructors: one allowing you to manually specify the `slope` ($a$), `intercept` ($b$), and range of values across which to plot the trendline, and the other two that determine these automatically from data provided as an `IReadOnlyList<IReadOnlyList<double>>` (i.e., an array of `double[]`) or as an `IReadOnlyList<(double, double)>` (i.e., an array of `(double, double)` tuples). These also allow you to fix the intercept value.

## The `PowerLawTrendLine` class

The `PowerLawTrendLine` class represents a power law trendline, with the following equation:

$$y = b \cdot x^a$$

Like the `LinearTrendLine` class, this class has three constructors: one allowing you to manually specify the `slope` ($a$), `intercept` ($b$), and range of values across which to plot the trendline, and the other two that determine these automatically from data provided as an `IReadOnlyList<IReadOnlyList<double>>` (i.e., an array of `double[]`) or as an `IReadOnlyList<(double, double)>` (i.e., an array of `(double, double)` tuples). Note that you cannot fix the intercept in this case.

## The `PolynomialTrendLine` class

The `PolynomialTrendLine` class represents a polynomial trendline, with the following equation:

$$y = \sum_{i = 0}^n a_i \cdot x^i$$

Like the `LinearTrendLine` class, this class has three constructors: one allowing you to manually specify the coefficients ($a_i$) and range of values across which to plot the trendline, and the other two that determine these automatically from data provided as an `IReadOnlyList<IReadOnlyList<double>>` (i.e., an array of `double[]`) or as an `IReadOnlyList<(double, double)>` (i.e., an array of `(double, double)` tuples). For these, you also have to specify the order of the polynomial (i.e., $n$ in the equation above), and you can fix the intercept value (i.e., $a_0$).

## The `MovingAverageTrendLine` class

The `MovingAverageTrendLine` represents a moving average. Given:

* A set of data points $\left \\{ \left (x_i, y_i \right) \right \\}_i$;
* A "weight function" $w(\Delta i, \Delta x )$;
* A "period" $p \in \mathbb{N^+}$;

For each $i$, we define:
$$y'_i = \frac {\sum_{k=-p}^{p} y_{i + k} \cdot w \left (k, x_{i+k} - x_i \right )}{\sum_{k=-p}^{p} w \left (k, x_{i+k} - x_i \right )}$$

Then, the moving average trendline is a polygonal line that passes through the points $\left \\{ \left (x_i, y'_i \right) \right \\}_i$.

This class has two constructors, one accepting the data as an `IReadOnlyList<IReadOnlyList<double>>` (i.e., an array of `double[]`) and the other as an `IReadOnlyList<(double, double)>` (i.e., an array of `(double, double)` tuples); both require you to also specify the `period` $p$. Note that from the definition above it follows that the ordering of the data is important, therefore you should ensure that the data array is properly sorted (e.g., based on `X` values).

The weight function $w$ is determined by the `Weight` property, which should be set to a function accepting two parameters: a `int` representing the difference in indices between the point being weighted and the "focus point" for the average, and a `double` representing the difference in `X` values. The default weight function returns `1` regardless of the values of its arguments.

