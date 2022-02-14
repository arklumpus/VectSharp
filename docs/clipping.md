---
layout: default
nav_order: 11
---

# Clipping

Another operation that you can perform is to set a clipping path on the `Graphics` object. When you do this, all subsequent drawing calls will only affect the part of the image that is inside of the clipping path.

You can apply a clipping path using the `SetClippingPath` method of the `Graphics` class. This method has two overloads that take parameters describing a rectangle (either as a `Point` and a `Size`, or as four `double`s), and another overload whose only parameter is a `GraphicsPath` object that is used as the clipping path. This latter option is particularly powerful, because it makes it possible to create arbitrary clipping path, which could even e.g. include "holes" or text.

The following code shows the effect of a clipping path.

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/ClippingPath.svg" style="height: 10em" />
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

Page page = new Page(100, 100);
Graphics graphics = page.Graphics;

// Create the clipping path.
GraphicsPath clippingPath = new GraphicsPath().Arc(50, 50, 20, 0, 2 * Math.PI);

// Stroke the path.
graphics.StrokePath(clippingPath, Colour.FromRgb(0, 158, 115));

// Apply the clipping path.
graphics.SetClippingPath(clippingPath);

// Draw a rectangle. Only the part of the rectangle that falls within the
// clipping path will actually be drawn.
graphics.FillRectangle(10, 10, 50, 50, Colours.Grey);

page.SaveAsSVG("ClippingPath.svg");
{% endhighlight %}

You can use the `SetClippingPath` method repeatedly, with different clipping paths. If you do this, the final clipping path will be the intersection of all the clipping paths that you provided (or, in other terms, only the parts of the image that fall within _all_ the clipping paths will be drawn).

The clipping path is preserved and restored by the `Save` and `Restore` methods; therefore, if you wish to remove a clipping path after you have applied it, you can follow the same approach as described when talking about [coordinate system transformations](transformations.html#saving-and-restoring-the-coordinate-system).

A clipping path is a binary operation: either a point is within the path and it is drawn normally, or it is outside, and it is not drawn. If you need to use intermediate values, where some points are only partially hidden (i.e., made transparent), you should use a [mask filter](filters.html#mask-filter) instead.