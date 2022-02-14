---
layout: default
nav_order: 1
parent: Advanced topics
---

# Measuring paths and groups of objects
{: .no_toc }

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

Sometimes, it might be nececssary to measure a `GraphicsPath` object, either to determine a bounding box for the path, or to compute the path's total length. Similarly, it can also be useful to determine a bounding box for all the objects contained on a `Graphics` surface.

## Determining the bounding box of a `GraphicsPath`

The bounding box of a path is the smallest rectangle that contains all the points in the path. Note that this includes all the points through which the path passes, and not the control points of Bézier curves.

To compute the bounding box of a `GraphicsPath` object, you can use the `GetBounds` method:

{% highlight CSharp %}
GraphicsPath path = ...

// Compute the bounding box of the path.
Rectangle bounds = path.GetBounds();
{% endhighlight %}

This method returns a `Rectangle` object representing the bounding box of the path. Note that this does not include the path's stroke (because the `GraphicsPath` does not contain any information about it - the stroke is applied when it is drawn on a `Graphics` object).

Since computing the bounding box can be relatively expensive, the result of this method is cached, so that if it is called multiple times, the actual computation only takes place once. The cached bounding box is invalidated when any element is added to the `GraphicsPath`.

[Back to top](#){: .btn }&nbsp;&nbsp;&nbsp;&nbsp;[Back to Advanced topics](advanced.html){: .btn }

## Determinining the bounding box of a `Graphics`

The `Graphics` class also implements a `GetBounds` method; this method determines the smallest rectangle that contains all the graphics elements that have been drawn on the surface. This bounding box also includes the thickness of the stroke of any path.

{% highlight CSharp %}
Graphics graphics = ...

// Draw something on graphics.

// Compute the bounding box of the graphics surface.
Rectangle bounds = graphics.GetBounds();
{% endhighlight %}

Note that, unlike the `GraphicsPath.GetBounds` method, the results of this method are not cached. However, since it internally invokes the `GetBounds` method of any `GraphicsPath` object that has been drawn on the `Graphics`, successive calls to this method should be relatively cheap anyways (because the bounding boxes of all elements have been cached).

[Back to top](#){: .btn }&nbsp;&nbsp;&nbsp;&nbsp;[Back to Advanced topics](advanced.html){: .btn }

## Cropping a `Page`

Once you have determined the bounding box of all the elements contained in a `Graphics` surface, you may also want to crop the corresponding page to that bounding box. This can be achieved using the `Crop` method of the `Page` object. This method will add an initial transformation to the page's `Graphics` object so that the top-left corner of the bounding box of the `Graphics` lies at $\left (0, 0 \right)$, and change the `Page`'s size to the size of the bounding box.

For example, this code:

{% highlight CSharp %}
// The initial size does not matter.
Page page = new Page(1, 1);

// Draw a rectangle on the page's Graphics.
page.Graphics.FillRectangle(10, 10, 80, 80, Colours.Green);

// Crop the page.
page.Crop();
{% endhighlight %}

Is equivalent to:

{% highlight CSharp %}
// The size of the page is the same as the rectangle.
Page page = new Page(80, 80);

// Translation to place the top-left corner of the rectangle at (0, 0)
page.Graphics.Translate(-10, -10);

// Draw a rectangle on the page's Graphics.
page.Graphics.FillRectangle(10, 10, 80, 80, Colours.Green);
{% endhighlight %}

The `Crop` method has two overloads: one overload does not have any parameters, and crops the page to the bounding box of its `Graphics`; the other overload takes a `Point` and a `Size` defining the rectangular area to crop.

[Back to top](#){: .btn }&nbsp;&nbsp;&nbsp;&nbsp;[Back to Advanced topics](advanced.html){: .btn }

## Measuring the length of a `GraphicsPath`

It can also be interesting to measure the _length_ of a `GraphicsPath` object in graphics units. This means, e.g., that the length of a segment going from $\left (x_1, y_1 \right)$ to $\left (x_2, y_2 \right)$ is $\sqrt{\left (x_1 -x_2 \right)^2 + \left (y_1 -y_2 \right)^2}$, or that the length of an arc segment with a radius of $r$ going from $\theta_1$ to $\theta_2$ is $\lvert \theta_2 - \theta_1 \rvert \cdot r$.

This can be achieved by using the `MeasureLength` method of the `GraphicsPath` class, which returns a `double` representing the length of the path:

{% highlight CSharp %}
GraphicsPath path = ...

// Compute the length of the path.
double length = path.MeasureLength();
{% endhighlight %}

Since there is no analytical formula to compute the length of a cubic Bézier curve, the length of any Bézier segments is computed by splitting the curve in many small straight segments, and summing the length of each segment. To ensure a decent precision in the estimate, the number of segments used is increased until the relative improvement in the estimate becomes smaller than $10^{-4}$.

As a result, this is also a rather expensive operation (especially on Blazor, this can be very slow). However, the results of the computation are cached at the segment level, so that repeated calls do not result in unnecessary computations.

[Back to top](#){: .btn }&nbsp;&nbsp;&nbsp;&nbsp;[Back to Advanced topics](advanced.html){: .btn }
