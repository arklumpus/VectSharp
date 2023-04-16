---
layout: default
nav_order: 7
grand_parent: Plots
parent: Plot elements
---

# 2-D function plots

2-D function plots are created by using the `Function2D` class. This class graphically represents a `Function2DGrid` object in a variety of ways, depending on the value of the `Type` property.

The following example shows how to use this class:

<div class="code-example">
    <iframe src="assets/images/plots/function2d2.svg" style="width: 100%; height: 25em; border: 0px solid black"></iframe>
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

// Define the function to be plotted.
static double myFunction(double[] p)
{
    // Square root of Himmelblau's function.
    return Math.Sqrt(Math.Pow(p[0] * p[0] + p[1] - 11, 2) + Math.Pow(p[0] + p[1] * p[1] - 7, 2));
}

// Create a Function2DGrid object that samples the function.
// The function will be sampled for values of X between -5 and
// 5, and for values of Y between -5 and 5. The function will be
// Sampled in 2500 points (50 * 50) at random coordinates within the
// range (because of the gridType).
Function2DGrid grid = new Function2DGrid(myFunction, -5, -5, 5, 5, 50, 50, Function2DGrid.GridType.Irregular);

// Create a linear coordinate system.
LinearCoordinateSystem2D coordinateSytem = new LinearCoordinateSystem2D(-5, 5, -5, 5, 250, 250);

// Create a Function2D object that plots the grid.
Function2D function = new Function2D(grid, coordinateSytem)
{
    Type = Function2D.PlotType.Tessellation,
    Colouring = Gradients.ViridisColouring
};

// Create the plot.
Plot plot = new Plot();

// Add the function to the plot.
plot.AddPlotElement(function);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG("function2d.svg");
{% endhighlight %}
</details>

## The `Function2D` class

This class produces a 2-D function plot from an underlying `Function2DGrid` object (stored in the `Function` property), whose appearance depends on the value of the `Type` property:

* If `Type` is `PlotType.SampledPoints`, the points that have been sampled in the `Function2DGrid` are shown by drawing them using the `SampledPointElement` symbol.
* If `Type` is `PlotType.Tessellation`, a Voronoi tessellation of the plot space is performed, based on the points that have been sampled in the `Function2DGrid`. Each cell is then coloured based on the corresponding sampled point. If the `Function2DGrid` has been sampled using a regular sampling strategy (rectangular or hexagonal), this is much faster because the shape of the cells is already known.
* If `Type` is `PlotType.Raster`, a raster image at a fixed resolution (determined by the `RasterResolutionX` and `RasterResolutionY` properties) is created by interpolating the values of the `Function2DGrid` using a bilinear interpolation strategy. This is then scaled to cover the required area in the plot space. If the `Function2DGrid` has been sampled using an irregular strategy, a Voronoi tessellation is performed and then rasterised, instead.

The constructor for this class requires the `Function2DGrid` object being plotted as an argument, as well as a continuous invertible coordinate system.

In all cases, the `Colouring` property is used to determine the colour of each point or Voronoi cell. This should be set to a function accepting a single argument of type `double`, ranging from `0` to `1` (scaling on the function values is performed automatically by the `Function2DGrid` class), and returning a `Colour` corresponding to that value. The static class `VectSharp.Gradients` has a number of functions that work like this, and there is also an implicit conversion operator defined for `GradientStops` objects that allow them to be used in this fashion.

The `Function2DGrid` class has already been described when talking about the [`Plot.Create.Function2D` method]({{ site.baseurl }}{% link plots.function2d.md %}).


## Bonus: Tupper's self-referential formula

Tupper's self-referential formula is a formula that plots itself [(Jeff Tupper, 2001)](http://www.dgp.toronto.edu/~mooncake/papers/SIGGRAPH2001_Tupper.pdf).

Here is an example of how it can be plotted using VectSharp.Plots.

<div class="code-example">
    <iframe src="assets/images/plots/tupper.svg" style="width: 100%; height: 10em; border: 0px solid black"></iframe>
</div>
<details markdown="block">
<summary>
    Expand source code
  </summary>
  {: .text-delta }
{% highlight CSharp %}
using System.Numerics;
using VectSharp;
using VectSharp.Plots;
using VectSharp.SVG;

// We need a BigInteger because the value is too large for a regular integer.
BigInteger n = BigInteger.Parse("4858450636189713423582095962494202044581400587983244549483093085061934704708809928450644769865524364849997247024915119110411605739177407856919754326571855442057210445735883681829823754139634338225199452191651284348332905131193199953502413758765239264874613394906870130562295813219481113685339535565290850023875092856892694555974281546386510730049106723058933586052544096664351265349363643957125565695936815184334857605266940161251266951421550539554519153785457525756590740540157929001765967965480064427829131488548259914721248506352686630476300");

// Define the function to be plotted.
double myFunction(double[] p)
{
    if (p[0] < 0 || p[1] < 0 || p[0] > 106 || p[1] > 16.5)
    {
        // White
        return 0;
    }
    else
    {
        int y = (int)Math.Floor(p[1]);

        BigInteger power = BigInteger.Pow(2, 17 * (int)Math.Floor(p[0]) + (int)((n + y) % 17));

        int mod = (int)BigInteger.ModPow((n + y) / 17 / power, 1, 2);

        if (mod > 0.5)
        {
            // Black
            return -1;
        }
        else
        {
            // White
            return 0;
        }
    }
}

// Create a Function2DGrid object that samples the function.
// The function will be sampled for values of X between -0.5 and
// 106.5, and for values of Y between -0.5 and 17.5. The shift by
// 0.5 is in order to avoid clipping issues (otherwise the "pixels"
// on the edges would be clipped).
Function2DGrid grid = new Function2DGrid(myFunction, -0.5, -0.5, 106.5, 17.5, 107, 18, Function2DGrid.GridType.Rectangular);

// Create a linear coordinate system.
LinearCoordinateSystem2D coordinateSytem = new LinearCoordinateSystem2D(-0.5, 106.5, -0.5, 17.5, 1070, 180);

// Create a Function2D object that plots the grid.
Function2D function = new Function2D(grid, coordinateSytem)
{
    Type = Function2D.PlotType.Tessellation,
};

// Create the plot.
Plot plot = new Plot();

// Add the function to the plot.
plot.AddPlotElement(function);

// Render the plot to a Page and save it as an SVG document.
Page pag = plot.Render();
pag.SaveAsSVG(@"tupper.svg");
{% endhighlight %}
</details>