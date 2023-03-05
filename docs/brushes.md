---
layout: default
nav_order: 8
---

# Brushes
{: .no_toc }

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

In the previous examples, the elements that were drawn on the graphics surface (rectangles, paths and text) were all drawn using a solid `Colour`. However, you may have noticed that the all the `Fill`/`Stroke` methods accept a `Brush` parameter, rather than a `Colour`. VectSharp currently implements three kinds of `Brush`es:

* A `SolidColourBrush` represents a brush that fills/strokes with a solid colour. The `SolidColourBrush` constructor accepts a single parameter, corresponding to the `Colour` of the brush. There is also an implicit conversion operator that is defined between a `Colour` and a `SolidColourBrush` (and a `Brush`), which is why we were able to directly provide a `Colour` to methods that actually require a `Brush` as an parameter.

* A `LinearGradientBrush` represents a brush that paints with a [linear gradient](https://en.wikipedia.org/wiki/Color_gradient#Axial_gradients). The constructor for this class takes two `Point` parameters, which determine the start and end point of the gradient, and a collection of `GradientStop`s, which determine the colours in the gradient (see below).

* A `RadialGradientBrush` represents a brush that paints with a [radial gradient](https://en.wikipedia.org/wiki/Color_gradient#Radial_gradients). The constructor for this class requires two `Point` parameters, determining the centre of the circle defining the gradient and the starting point of the gradient, and a `double` which represents the radius of the circle (this is explained in more detail below). It also requires a collection of `GradientStop`s.

The colours used by a gradient brush are defined by a series of `GradientStop`s. A `GradientStop` associates a `Colour` with a `double` indicating its position (`offset`) in the gradient: an `offset` of `0` corresponds to the start of the gradient, an offset of `1` corresponds to the end of the gradient, and intermediate values correspond to intermediate points. Accordingly, the constructor for the `GradientStop` class accepts two parameters, corresponding to the `Colour` and the `offset`.

A collection of `GradientStop` objects can be represented as a `GradientStops` object (note the plural), which implements `IReadOnlyList<GradientStop>`. This provides a method `GetColourAt(double position)`, which returns the colour at a specific offset (for offsets &le; 0 or &ge; 1, the first and last colour are returned, respectively). There is also an implicit conversion operator defined allowing the conversion of a `GradientStops` object into a `Func<double, Colour>`.

## Linear gradients

A linear gradient is defined by two `Point`s, determining the start and end of the gradient, and a number of `GradientStops` indicating the colours used for the gradient. The following example shows how to use a linear gradient to fill a rectangle:

<div class="code-example">
    <iframe src="Blazor?linearGradientBrush" style="width: 100%; height: 18em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 100);
Graphics graphics = page.Graphics;

// Gradient stops defining the colours for the gradient.
GradientStop start = new GradientStop(Colour.FromRgb(86, 180, 233), 0);
GradientStop middle = new GradientStop(Colour.FromRgb(0, 158, 115), 0.5);
GradientStop end = new GradientStop(Colour.FromRgb(0, 114, 178), 1);

// Start point of the gradient.
Point startPoint = new Point(10, 20);

// End point of the gradient.
Point endPoint = new Point(90, 80);

// Create the linear gradient brush.
LinearGradientBrush brush = new LinearGradientBrush(startPoint, endPoint, start, middle, end);

// Fill the rectangle.
graphics.FillRectangle(10, 10, 80, 80, brush);

page.SaveAsSVG("LinearGradientBrush.svg");
{% endhighlight %}

Expand the section below for more information on how the colour of each point in a linear gradient is determined.

<details markdown="block">
<summary>
    Determining the colour of a point in a linear gradient
  </summary>
  {: .text-delta }

In general, a gradient (linear or otherwise) defines a mapping $C$ from $\left [ 0, 1 \right ]$ to the space of RGBA colours. This is defined by the `GradientStop`s. Given a real value $0 \leq t \leq 1$, to determine the colour associated to this value:

1. Find the `GradientStop` with the largest `Offset` value $s$ such that $s \leq t$. Let the colour of this `GradientStop` be $\mathbf{S}$.

2. Find the `GradientStop` with the smallest `Offset` value $l$ such that $l \geq t$. Let the colour of this `GradientStop` be $\mathbf{L}$.

3. If $s = l$, $\mathbf{C}\left ( t \right ) := \mathbf{L}$. Otherwise, the colour is determined by the following equation:

$$\mathbf{C}\left ( t \right ) := \left (1 - \frac{t - s}{l - s} \right) \mathbf{S} + \frac{t - s}{l - s} \mathbf{L}$$

Now the task is, given an arbitrary point $\mathbf{P} \in \mathbb{R}^2$ on the plane, to find the corresponding value of $t$. To do this, we need to project the point on the line defined by the start and end point of the gradient.

Let $\mathbf{P_0}$ and $\mathbf{P_1}$ be, respectively, the start and end point. Then, $t$ can be found using the following equation:

$$t(\mathbf{P}) = \max \left (0, \min \left ( 1, \frac{\left (\mathbf{P} - \mathbf{P_0} \right ) \cdot \left (\mathbf{P_1} - \mathbf{P_0} \right )}{\left \| \mathbf{P_1} - \mathbf{P_0} \right \|^2} \right ) \right )$$

Here, $\cdot$ is the [dot product](https://en.wikipedia.org/wiki/Dot_product), and $\lVert \mathbf{V} \rVert$ is the [length](https://en.wikipedia.org/wiki/Norm_(mathematics)) of the vector $\mathbf{V}$.

Using these equations, given a point $\mathbf{P}$ you can compute $t$, and given $t$ you can determine the colour $\mathbf{C}$.
</details>

[Back to top](#){: .btn }

## Radial gradients

In VectSharp, radial gradients are defined by a "focal point" and an outer circle. These identify a bundle of circumferences starting from the focal point at radius 0, with increasing radii and centers moving from the focal point to the centre of the outer circle.

The following example shows how to use a radial gradient to fill a rectangle:

<div class="code-example">
    <iframe src="Blazor?radialGradientBrush" style="width: 100%; height: 21em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 100);
Graphics graphics = page.Graphics;

// Gradient stops defining the colours for the gradient.
GradientStop start = new GradientStop(Colour.FromRgb(86, 180, 233), 0);
GradientStop middle = new GradientStop(Colour.FromRgb(0, 158, 115), 0.5);
GradientStop end = new GradientStop(Colour.FromRgb(0, 114, 178), 1);

// Focal point of the gradient.
Point focalPoint = new Point(30, 30);

// Centre of the outer circle.
Point centre = new Point(50, 50);

// Radius of the outer circle.
double radius = 40;

// Create the radial gradient brush.
RadialGradientBrush brush = new RadialGradientBrush(focalPoint, centre, radius, start, middle, end);

// Fill the rectangle.
graphics.FillRectangle(10, 10, 80, 80, brush);

page.SaveAsSVG("RadialGradientBrush.svg");
{% endhighlight %}

Expand the section below for more information on how the colour of each point in a radial gradient is determined.

<details markdown="block">
<summary>
    Determining the colour of a point in a radial gradient
  </summary>
  {: .text-delta }

As before, the task is, given an arbitrary point $\mathbf{P} \in \mathbb{R}^2$ on the plane, to find the corresponding value of $t$ to determine the colour of the gradient.

Let $\mathbf{F}$ be the focal point of the gradient, $\mathbf{O}$ be the centre, and $r$ be the radius.

For every $t \in \mathbb{R}$, consider the circumferences centred at $\mathbf{Q}\left ( t \right ) = (1 - t) \cdot \mathbf{F} + t \cdot \mathbf{O}$ with radius $s\left ( t \right) = t \cdot r$.

We seek to find the circumference(s) of this form, to which $\mathbf{P}$ belongs. If $\mathbf{F} \equiv \mathbf{O}$, the circumferences all have the same centre, and the radius of the circumference containing $\mathbf{P}$ is:

$$s = \left \| \mathbf{P} - \mathbf{F} \right \| = t \cdot r \Rightarrow t = \frac{s}{r} = \frac{\left \| \mathbf{P} - \mathbf{F} \right \|}{r}$$

Otherwise, we need to solve the system of equations:

$$ \left \{ \begin{array}{l} \left \| \mathbf{P} - \mathbf{Q} \right \| = t \cdot r \\ \mathbf{Q} = (1 - t) \cdot \mathbf{F} + t \cdot \mathbf{O} \end{array} \right .$$

Substituting $\mathbf{Q}$ in the first equation and rearranging, we get:

$$ \left \| \left (\mathbf{P} - \mathbf{F} \right ) - t \left (\mathbf{O} - \mathbf{F} \right ) \right \| = t \cdot r $$

Squaring both sides and expanding the norm:

$$ \left \| \mathbf{P} - \mathbf{F} \right \|^2 + t^2 \left \| \mathbf{O} - \mathbf{F} \right \|^2 - 2 t \cdot \left (\mathbf{P} - \mathbf{F} \right) \cdot \left ( \mathbf{O} - \mathbf{F} \right) = t^2 \cdot r^2 $$

Rearranging again:

$$ \left ( \left \| \mathbf{O} - \mathbf{F} \right \|^2 - r^2 \right) t^2 - 2 \left ( \mathbf{P} - \mathbf{F} \right ) \cdot \left ( \mathbf{O} - \mathbf{F} \right ) \cdot t + \left \| \mathbf{P} - \mathbf{F} \right \|^2 = 0$$

This is a simple second degree equation. If $\lVert \mathbf{O} - \mathbf{F} \rVert^2 - r^2 = 0$ (i.e., the focal point lies on the outer circumference), then if $\left ( \mathbf{P} - \mathbf{F} \right ) \cdot \left ( \mathbf{O} - \mathbf{F} \right ) = 0$, the equation has no solutions and the point is not affected by the gradient. If this term is not $0$, then:

$$ t = \frac{ \left \| \mathbf{P} - \mathbf{F} \right \| ^2}{2 \left ( \mathbf{P} - \mathbf{F} \right ) \cdot \left ( \mathbf{O} - \mathbf{F} \right )} $$

Otherwise, let:

$$ d = \left (\left ( \mathbf{P} - \mathbf{F} \right ) \cdot \left ( \mathbf{O} - \mathbf{F} \right ) \right )^2 - \left \| \mathbf{P} - \mathbf{F} \right \|^2 \left ( \left \| \mathbf{O} - \mathbf{F} \right \|^2 - r^2 \right )$$

If $d < 0$, then the equation has no real solutions and the point is not affected by the gradient. Otherwise, the solutions are:

$$ t_1 = \frac{ \left ( \mathbf{P} - \mathbf{F} \right ) \cdot \left ( \mathbf{O} - \mathbf{F} \right ) + \sqrt{d} }{\left \| \mathbf{O} - \mathbf{F} \right \|^2 - r^2}$$

$$ t_2 = \frac{ \left ( \mathbf{P} - \mathbf{F} \right ) \cdot \left ( \mathbf{O} - \mathbf{F} \right ) - \sqrt{d} }{\left \| \mathbf{O} - \mathbf{F} \right \|^2 - r^2}$$

Now, if both solutions lie in the interval $\left [ 0, 1 \right ]$, choose the smallest of the two; if instead only one lies in the interval $\left [ 0, 1 \right ]$, then choose that solution. Otherwise, if at least one solution is $> 1$, then $t = 1$. If both are $< 0$, then $t = 0$.

With this value of $t$, you can determine the colour to associate to the point based on the `GradientStops`, using the algorithm described above when talking about linear gradients. 
</details>

## Standard gradients

VectSharp includes a number of "standard" gradients, defined in the static `Gradients` class. These are implemented as static `GradientStop` objects, which you can use when initialising a `LinearGradientBrush` or a `RadialGradientBrush`. For example:

{% highlight CSharp %}
// Create a linear gradient brush.
LinearGradientBrush brush = new LinearGradientBrush(startPoint, endPoint, Gradients.Viridis);

// Create a radial gradient brush.
RadialGradientBrush brush = new RadialGradientBrush(focalPoint, centre, radius, Gradients.Cividis);
{% endhighlight %}

For some of these gradients (nominally, those included in the "viridis" R package), the `Gradients` class also defines a static method (e.g., `Colour ViridisColouring(double x)` for the `Viridis` gradient, `Colour CividisColouring(double x)` for the `Cividis` gradient, and so on), which can be used directly to get the colour in the gradient at the specified position. This is both faster and more accurate than using the `GetColourAt` on the `GradientStops` objects themselves (because the gradients are defined using only 20 colour stops, while these methods use 256 samples).

Here is a list of all the standard gradients included in VectSharp:

* `TransparentToBlack` - Gradient from transparent black (0) to opaque black (1).
  <p style="text-align: center">
      <img src="assets/images/gradients/TransparentToBlack.svg" style="height: 3em" />
  </p>

* `WhiteToBlack` - Gradient from white (0) to black (1).
  <p style="text-align: center">
      <img src="assets/images/gradients/WhiteToBlack.svg" style="height: 3em" />
  </p>

* `RedToGreen` - Gradient from red (0) to green (1).
  <p style="text-align: center">
      <img src="assets/images/gradients/RedToGreen.svg" style="height: 3em" />
  </p>

* `Rainbow` - Rainbow gradient (red, orange, yellow, green, blue, indigo, violet).
  <p style="text-align: center">
      <img src="assets/images/gradients/Rainbow.svg" style="height: 3em" />
  </p>

* `RedYellowGreen` - Gradient from red (0) to yellow (0.5) to green (1).
  <p style="text-align: center">
      <img src="assets/images/gradients/RedYellowGreen.svg" style="height: 3em" />
  </p>

* `OkabeItoRainbow` - Rainbow gradient with Okabe-Ito colour-blind safe colours ([https://jfly.uni-koeln.de/color/](https://jfly.uni-koeln.de/color/)).
  <p style="text-align: center">
      <img src="assets/images/gradients/OkabeItoRainbow.svg" style="height: 3em" />
  </p>

* `OkabeItoRainbowDiscrete` - Rainbow gradient with Okabe-Ito colour-blind safe colours ([https://jfly.uni-koeln.de/color/](https://jfly.uni-koeln.de/color/)) in discrete steps.
  <p style="text-align: center">
      <img src="assets/images/gradients/OkabeItoRainbowDiscrete.svg" style="height: 3em" />
  </p>

* `MutedRainbow` - Rainbow gradient with Paul Tol's Muted palette ([https://personal.sron.nl/~pault/](https://personal.sron.nl/~pault/)).
  <p style="text-align: center">
      <img src="assets/images/gradients/MutedRainbow.svg" style="height: 3em" />
  </p>

* `MutedRainbowDiscrete` - Rainbow gradient with Paul Tol's Muted palette ([https://personal.sron.nl/~pault/](https://personal.sron.nl/~pault/)) in discrete steps.
  <p style="text-align: center">
      <img src="assets/images/gradients/MutedRainbowDiscrete.svg" style="height: 3em" />
  </p>

* `Magma` - Magma gradient, based on the homonymous colour scale included in the "viridis" R package ([https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html](https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html)).
  <p style="text-align: center">
      <img src="assets/images/gradients/Magma.svg" style="height: 3em" />
  </p>

* `Inferno` - Inferno gradient, based on the homonymous colour scale included in the "viridis" R package ([https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html](https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html)).
  <p style="text-align: center">
      <img src="assets/images/gradients/Inferno.svg" style="height: 3em" />
  </p>

* `Plasma` - Plasma gradient, based on the homonymous colour scale included in the "viridis" R package ([https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html](https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html)).
  <p style="text-align: center">
      <img src="assets/images/gradients/Plasma.svg" style="height: 3em" />
  </p>

* `Viridis` - Viridis gradient, based on the homonymous colour scale included in the "viridis" R package ([https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html](https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html)).
  <p style="text-align: center">
      <img src="assets/images/gradients/Viridis.svg" style="height: 3em" />
  </p>

* `Cividis` - Cividis gradient, based on the homonymous colour scale included in the "viridis" R package ([https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html](https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html)).
  <p style="text-align: center">
      <img src="assets/images/gradients/Cividis.svg" style="height: 3em" />
  </p>

* `Rocket` - Rocket gradient, based on the homonymous colour scale included in the "viridis" R package ([https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html](https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html)).
  <p style="text-align: center">
      <img src="assets/images/gradients/Rocket.svg" style="height: 3em" />
  </p>

* `Mako` - Mako gradient, based on the homonymous colour scale included in the "viridis" R package ([https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html](https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html)).
  <p style="text-align: center">
      <img src="assets/images/gradients/Mako.svg" style="height: 3em" />
  </p>

* `Turbo` - Turbo gradient, based on the homonymous colour scale included in the "viridis" R package ([https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html](https://cran.r-project.org/web/packages/viridis/vignettes/intro-to-viridis.html)).
  <p style="text-align: center">
      <img src="assets/images/gradients/Turbo.svg" style="height: 3em" />
  </p>

[Back to top](#){: .btn }
