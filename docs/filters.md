---
layout: default
nav_order: 13
---

# Filters
{: .no_toc }

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
1. TOC
{:toc}
</details>

Filters make it possible to apply a number of effects to the image. Depending on the output format and the kind of filter, the filter can be applied directly to the vector image, or it can require the image to be rasterised first. For example, a Gaussian blur filter can be applied directly to an SVG image, but it will require the image to be rasterised if it is being exported as a PDF.

All filters implement the `IFilter` interface, but, more importantly, they also implement either the `ILocationInvariantFilter` interface, or the `IFilterWithLocation` interface.

Filters implementing `IlocationInvariantFilter` describe an operation whose effect does not depend on the part of the image where it is applied: for example, a Gaussian blur will have the same blurring effect on any part of the image. Filters implementing `IFilterWithLocation`, instead, describe operations that affect different parts of the image differently, such as masking.

Both of these interfaces define two properties, `TopLeftMargin` and `BottomLeftMargin`, that describe how much the filter extends beyond the boundaries of the image it is applied to (e.g., a Gaussian blur filter extends by about three standard deviations outside the image). They also define a `Filter` method, which is used to apply the filter to a raster image. However, unless you are developing new filters, you should not need to worry about these.

VectSharp currently supports a number of filters; you can extend the capabilities of the library by creating new filters that implement one of these two interfaces, and then use them normally. When you apply them, the section of the image to which the filter is applied will be rasterised, and the filter's `Filter` method will be used to transform it.

To apply a filter, you need to invoke the overload of the `DrawGraphics` method that takes an `IFilter` as a parameter. The filter will be applied to the `Graphics` object that is drawn by this call. Filters are defined in the `VectSharp.Filters` namespace (included in the VectSharp assembly); thus it is also necessary to add an `using` directive.

{% highlight CSharp %}
// Using directive to access filter classes.
using VectSharp.Filters;

// Graphics object on which to draw.
Graphics graphics = ...

// Graphics object containing the elements to which the filter will be applied.
Graphics filterSubject = ...

// Filter object (see below).
IFilter filter = ...

// Draw the filterSubject on the graphics, using the filter.
graphics.DrawGraphics(0, 0, filterSubject, filter);
{% endhighlight %}

This section describes the filters that are currently implemented in VectSharp.

## Gaussian blur filter

The [Gaussian blur filter](https://en.wikipedia.org/wiki/Gaussian_blur) is implemented by the `GaussianBlurFilter` class. The constructor for this class takes a single argument, representing the standard deviation of the Gaussian blur in graphics units. The effect of the filter is to place a 2D Gaussian curve with the specified standard deviation at each pixel, and then compute the filtered colour of the pixel by weighing the original colours of neighbouring pixels with the Gaussian function.

The following example shows the effect of a `GaussianBlurFilter` in action.

<div class="code-example">
    <iframe src="Blazor?gaussianBlur" style="width: 100%; height: 15em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;
using VectSharp.Filters;

Page page = new Page(100, 100);
Graphics graphics = page.Graphics;

// Graphics object containing the elements to which the filter will be applied.
Graphics filterSubject = new Graphics();

// Draw some text that will be blurred.
Font font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 15);
filterSubject.FillText(15, 40, "VectSharp", font, Colours.Black);

// Standard deviation for the Gaussian blur.
double standardDeviation = 1;

// Create the GaussianBlurFilter object.
IFilter filter = new GaussianBlurFilter(standardDeviation);

// Draw the filterSubject on the graphics, using the filter.
graphics.DrawGraphics(0, 0, filterSubject, filter);

page.SaveAsSVG("GaussianBlur.svg");
{% endhighlight %}

Gaussian blur filters are supported natively by the SVG format; thus, when a `Page` is saved to an SVG file, they are not rasterised. They are instead rasterised if the document is exported in PDF format.

[Back to top](#){: .btn }

## Box blur filter

A [Box blur filter](https://en.wikipedia.org/wiki/Box_blur) blurs the image by replacing each pixel with the average of its neighbours. This filter is implemented by the `BoxBlurFilter` class, whose constructor also takes a single argument, which determines the size of the box used for the blurring (i.e., the number of pixels that are averaged).

A box blur filter is always rasterised when the image is exported. Therefore, in order to use it, you will need to install the VectSharp.Raster package or the VectSharp.Raster.ImageSharp package. Adding a `using` directive to the namespace in this package can be useful, because it ensures that the assembly is correctly loaded, even though you are not using it directly.

The following example shows the effect of a `BoxBlurFilter`.

<div class="code-example">
    <iframe src="Blazor?boxBlur" style="width: 100%; height: 15em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;
using VectSharp.Filters;
using VectSharp.Raster.ImageSharp;
// or using VectSharp.Raster;

Page page = new Page(100, 100);
Graphics graphics = page.Graphics;

// Graphics object containing the elements to which the filter will be applied.
Graphics filterSubject = new Graphics();

// Draw some text that will be blurred.
Font font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 15);
filterSubject.FillText(15, 40, "VectSharp", font, Colours.Black);

// Radius for the box blur filter.
double boxRadius = 1;

// Create the BoxBlurFilter object.
IFilter filter = new BoxBlurFilter(boxRadius);

// Draw the filterSubject on the graphics, using the filter.
graphics.DrawGraphics(0, 0, filterSubject, filter);

page.SaveAsSVG("BoxBlur.svg");
{% endhighlight %}

[Back to top](#){: .btn }

## Convolution filter

A [Convolution filter](https://en.wikipedia.org/wiki/Kernel_(image_processing)) replaces each pixel with a weighted average of its neighbours, using weights defined by a "kernel". Such a filter is implemented by the `ConvolutionFilter` class. The constructor for this class has two main parameters:

* `kernel`, a `double[,]` matrix, which specifies the kernel of the convolution. Each dimension of the matrix must be an odd number, otherwise an exception will be raised.
* `scale`, a `double`: this parameter determines the relationship between the dimensions of the `kernel` and the graphics units. When the filter is rasterised, this parameter is used to create the actual convolution kernel, based on the rasterisation resolution. This ensures that the effect of the filter is independent of the scale at which the image is viewed.

There are also three optional parameters: the `bool preserveAlpha` determines whether the alpha channel of the image is subject to the same convolution as the RGB channels, while the `double`s `normalisation` and `bias` are used to modify the result of the convolution.

The following example shows the effect of a `ConvolutionFilter`. In order to produce a meaningful effect, the `filterSubject` graphics is obtained by importing an SVG image; you can read more about this in the [next section](importing_svg.html). Note that applying a `ConvolutionFilter` can be rather slow (especially when the scale is high and on Blazor, as in the example below).

<div class="code-example">
    <iframe src="Blazor?convolution" style="width: 100%; height: 15em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;
using VectSharp.Filters;
using VectSharp.Raster.ImageSharp;
// or using VectSharp.Raster;

Page page = new Page(158, 72);
Graphics graphics = page.Graphics;

// Graphics object containing the elements to which the filter will be applied.
// This is loaded from an SVG file.
Graphics filterSubject = Parser.FromFile("/path/to/SurgeonFish.svg").Graphics;

// Kernel for the convolution filter.
// This is an example of an edge-detect matrix.
double[,] kernel = new double[3, 3]
{
    { -1, -1, -1 },
    { -1,  8, -1 },
    { -1, -1, -1 }
};

// Scale for the effect.
double scale = 1;

// Create the ConvolutionFilter object.
IFilter filter = new ConvolutionFilter(kernel, scale);

// Draw the filterSubject on the graphics, using the filter.
graphics.DrawGraphics(0, 0, filterSubject, filter);

page.SaveAsSVG("Convolution.svg");
{% endhighlight %}

The kernels used in the interactive example above are as follows:

$$\mathrm{Identity} = \begin{bmatrix} 0 & 0 & 0 \\ 0 & 1 & 0 \\ 0 & 0 & 0 \end{bmatrix}$$

$$\mathrm{Edge-detect} = \begin{bmatrix} -1 & -1 & -1 \\ -1 & 8 & -1 \\ -1 & -1 & -1 \end{bmatrix}$$

$$\mathrm{Sharpen} = \begin{bmatrix} 0 & -1 & 0 \\ -1 & 5 & -1 \\ 0 & -1 & 0 \end{bmatrix}$$

[Back to top](#){: .btn }

## Colour matrix filter

A [colour matrix filter](https://developer.mozilla.org/en-US/docs/Web/SVG/Element/feColorMatrix) transforms the colour of individual pixels using a colour transformation matrix. This filter is implemented by the `ColourMatrixFilter` class. The constructor for this class has a single `ColourMatrix` parameter, which is a 5x5 matrix describing the transformation. See the [description of this filter on the MDN](https://developer.mozilla.org/en-US/docs/Web/SVG/Element/feColorMatrix) for more details on how the matrix influences the result of the filter.

A `ColourMatrix` can be constructed from a `double[,]` matrix containing the matrix entries, or using one of the static methods and properties of the `ColourMatrix` class:

* The `Identity` static property returns a `ColourMatrix` that leaves the colours unchanged.
* The `GreyScale` static property returns a `ColourMatrix` that turns the image into greyscale.
* The `Pastel` static property returns a `ColourMatrix` that applies a "pastel" filter to the image, reducing the saturation.
* The `Inversion` static property returns a `ColourMatrix` that inverts each colour in the image.
* The `AlphaInversion` static property returns a `ColourMatrix` that inverts only the alpha channel of the image.
* The `InvertedAlphaShift` static property returns a `ColourMatrix` that adds to each component the inverted alpha value. This is useful, for example, to turn fully transparent black (RGBA `0, 0, 0, 0`) into fully transparent white (RGBA `255, 255, 255, 0`).

* The `ToColour` static method returns a `ColourMatrix` that transforms every colour from the original image into the specified colour (i.e., to that exact colour, not to shades of it). If the optional parameter `useAlpha` is set to `true`, the alpha value of the specified colour is also used; otherwise, the original alpha values are preserved.
* The `LuminanceToColour` static method returns a `ColourMatrix` that transforms the colours of the original image into shades of the specified colour (like a "coloured greyscale"). If the optional parameter `useAlpha` is set to `true`, the alpha value of the specified colour is also used; otherwise, the original alpha values are preserved.
* The `LuminanceToAlpha` static method returns a `ColourMatrix` that sets the alpha component of each pixel to a value corresponding to the luminance (greyscale value) of that pixel. If the optional parameter `preserveColour` is `true`, the colour of each pixel is preserved; otherwise, each pixel is set to black.

Furthermore, the `WithAlpha` (instance) method of the `ColourMatrix` class returns a new `ColourMatrix` that produces the same colours as the current instance, but whose alpha values are multiplied by the specified value.

Multiple effects can be combined by multiplying the `ColourMatri`ces. For example, the following code produces a `ColourMatrix` that, in this order:
* Adds the inverted alpha value to each component of the image (turning transparent black into transparent white).
* Inverts the colours of the image (which turns again transparent white into transparent black).
* Sets the alpha channel to the luminance of the inverted image.

{% highlight CSharp %}
ColourMatrix invertedLuminanceToAlpha = ColourMatrix.LuminanceToAlpha() * 
                                        ColourMatrix.Inversion * 
                                        ColourMatrix.InvertedAlphaShift;
{% endhighlight %}

Note that the matrices are specified in reverse order.

The following example shows the effect of a `ColourMatrixFilter`. Again, to obtain a meaningful effect, the `filterSubject` is obtained by loading an SVG image.

<div class="code-example">
    <iframe src="Blazor?colourMatrix" style="width: 100%; height: 15em; border: 0px solid black"></iframe>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;
using VectSharp.Filters;

Page page = new Page(158, 72);
Graphics graphics = page.Graphics;

// Graphics object containing the elements to which the filter will be applied.
// This is loaded from an SVG file.
Graphics filterSubject = Parser.FromFile("/path/to/SurgeonFish.svg").Graphics;

// The ColourMatrix for the filter.
ColourMatrix colourMatrix = ColourMatrix.Pastel;

// Create the ConvolutionFilter object.
IFilter filter = new ColourMatrixFilter(colourMatrix);

// Draw the filterSubject on the graphics, using the filter.
graphics.DrawGraphics(0, 0, filterSubject, filter);

page.SaveAsSVG("ColourMatrix.svg");
{% endhighlight %}

Colour matrix filters are supported natively by the SVG format; thus, when a `Page` is saved to an SVG file, they are not rasterised. They are instead rasterised if the document is exported in PDF format.

[Back to top](#){: .btn }

## Composite filter

All the filters described above implement the `ILocationInvariantFilter` interface. If you wish to combine the effect of multiple filters, you can do so by using the `CompositeLocationInvariantFilter` class (which itself implements the `ILocationInvariantFilter` interface). The constructor for this class accepts a collection of filters which are applied to the image in order.

This can be useful, for example, to create a drop shadow: a `ColourMatrixFilter` can be used to turn the image to a uniform grey, and then a `GaussianBlurFilter` can be used to blur the shadow.

The following example shows how to create a drop shadow by combining a `ColourMatrixFilter` and a `GaussianBlurFilter`.

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/DropShadow.svg" style="height: 10em" />
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;
using VectSharp.Filters;

Page page = new Page(100, 30);
Graphics graphics = page.Graphics;

// Graphics object containing the elements to which the filter will be applied.
Graphics filterSubject = new Graphics();

// Draw some text that will be blurred.
Font font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 15);
filterSubject.FillText(15, 10, "VectSharp", font, Colours.Green);

// ColourMatrix for the filter
ColourMatrix colourMatrix = ColourMatrix.ToColour(Colour.FromRgb(180, 180, 180));

// Create the ColourMatrixFilterObject
ColourMatrixFilter colourMatrixFilter = new ColourMatrixFilter(colourMatrix);

// Create the GaussianBlurFilter object.
GaussianBlurFilter gaussianBlurFilter = new GaussianBlurFilter(1);

// Create the composite filter.
IFilter compositeFilter = new CompositeLocationInvariantFilter(colourMatrixFilter, gaussianBlurFilter);

// Draw the filterSubject on the graphics, using the composite (this creates the shadow).
graphics.DrawGraphics(1, 1, filterSubject, compositeFilter);

// Draw the filterSubject without the filter.
graphics.DrawGraphics(0, 0, filterSubject);

page.SaveAsSVG("DropShadow.svg");
{% endhighlight %}

As long as all the filters used in the composite filter are supported by the SVG format, the composite filter itself is also supported and will not be rasterised. It will instead be rasterised if the document is exported as a PDF file.

[Back to top](#){: .btn }

## Mask filter

A mask filter uses the luminance information of an image to mask another image. Essentially, the mask image is overlapped on the subject image; then, where the mask image is white, the subject image is preserved, while where the mask image is black the subject image becomes transparent. Intermediate colours apply intermediate alpha values.

A mask is conceptually similar to a [clipping path](clipping.html), in that both hide part of the image they are applied to. However, the differences between the two are:

* The clipping path is applied using the `SetClippingPath` method _before_ the drawing calls, while the mask is applied as a filter.
* The clipping path operates a "binary" operation (a point is either visible or hidden by the clipping path), while masks allow intermediate values where a point's alpha is affected.

Mask filters are implemented by the `MaskFilter` class. The constructor for this class accepts a single `Graphics` parameter, which is used as the mask. Unlike the other filters presented here, the `MaskFilter` implements the `IFilterWithLocation` interface, because the effect of the mask depends on where the subject image is being drawn.

The following example shows how to use a `MaskFilter`. In this case, a linear gradient from white (to the left) to black (to the right) is used to mask the surgeon fish image from the previous examples.

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/Mask.svg" style="height: 10em" />
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;
using VectSharp.Filters;

Page page = new Page(158, 72);
Graphics graphics = page.Graphics;

// Graphics object containing the mask.
Graphics mask = new Graphics();

// Add a linear gradient to the mask.
LinearGradientBrush gradient = new LinearGradientBrush(new Point(0, 0), new Point(158, 72),
                                                       new GradientStop(Colours.White, 0),
                                                       new GradientStop(Colours.Black, 1));
mask.FillRectangle(0, 0, 158, 72, gradient);

// Graphics object containing the elements to which the filter will be applied.
// This is loaded from an SVG file.
Graphics filterSubject = Parser.FromFile("/path/to/SurgeonFish.svg").Graphics;

// Create the MaskFilter object.
IFilter filter = new MaskFilter(mask);

// Draw the filterSubject on the graphics, using the filter.
graphics.DrawGraphics(0, 0, filterSubject, filter);

page.SaveAsSVG("Mask.svg");
{% endhighlight %}

Mask filters are supported natively by the SVG format; thus, when a `Page` is saved to an SVG file, they are not rasterised. They are instead rasterised if the document is exported in PDF format.

[Back to top](#){: .btn }

## Exporting documents containing filters

When you use the `SaveAsSVG` or `SaveAsPDF` methods to create PDF or SVG documents, you can use the optional `filterOption` parameter to determine how filters that have been used in the image should be exported.

### VectSharp.SVG

For the `SaveAsSVG` method, `filterOption` should be an instance of `SVGContextInterpreter.FilterOption`. The constructor for this class accepts three parameters.

The first parameter is a `SVGContextInterpreter.FilterOption.FilterOperations` enumeration, which determines what happens to the filters. This `enum` has 6 possible values:

* `RasteriseAll` means that all filters, even those that would be supported natively by the SVG format, are rasterised.
* `RasteriseIfNecessary` means that only those filters that are not supported by the SVG format are rasterised. This is the default.
* `NeverRasteriseAndIgnore` means that filters will never be rasterised. If a filter that is not supported has been used, the filter is ignored and the subject image is drawn unchanged (as if it had not been filtered).
* `NeverRasteriseAndSkip` also means that the filters will never be rasterised. However, images that have been drawn with a filter that is not supported will not be drawn at all.
* `IgnoreAll` means that no filter will be applied, and those images that have been drawn using a filter will be drawn as if the filter had not been used.
* `SkipAll` means that no filter will be applied, and those image that have been drawn using a filter will not be drawn at all.

The second parameter, `rasterisationResolution`, is a `double` that determines the scale at which filters that need to be rasterised are rasterised; by default, this is `1`. The third parameter, `rasterisationResolutionRelative`, is a `bool` determining whether the `rasterisationResolution` is relative or absolute. If this is `true` (the default), the `rasterisationResolution` is multiplied by the size of the image in graphics units to obtain the size in pixel of the image to rasterise. Otherwise, the `rasterisationResolution` itself is used as the size in pixels for the rasterised filter.

As a result of the default values, filters are only rasterised when necessary, using a scale corresponding to the size of the image in pixel units. If you wish to produce a higher-quality image, you may wish to specify a `filterOption` when exporting the image, increasing the `rasterisationResolution` (though this will come at an increased computational cost).

If a filter needs to be rasterised, you need to make sure that there is a way to do this. This can be achieved by installing either the [VectSharp.Raster](https://www.nuget.org/packages/VectSharp.Raster) package, or the [VectSharp.Raster.ImageSharp](https://www.nuget.org/packages/VectSharp.Raster.ImageSharp) package. Alternatively, if you have some other way to rasterise the image, you can set the `Graphics.RasterisationMethod` static property to point to a method that does this. The library will choose automatically between the available options.

### VectSharp.PDF

As mentioned above, all filters must be rasterised when exporting the image as a PDF document. For the `SaveAsPDF` method, `filterOption` is an instance of `PDFContextInterpreter.FilterOption`; the constructor for this class has three parameters, that correspond to the same parameters of the `SVGContextInterpreter.FilterOption` class.

In this case, the `PDFContextInterpreter.FilterOption.FilterOperations` `enum` only has three options (`RasteriseAll`, `IgnoreAll`, and `SkipAll`), which correspond to the same options as for VectSharp.SVG.

### VectSharp.Canvas

The `PaintToCanvas` and `PaintToSKCanvas` methods also accept a `filterOption` parameter, which is an instance of the `VectSharp.Canvas.FilterOption` class. In this case, the `FilterOption.FilterOperations` `enum` has four possible values: `IgnoreAll` and `SkipAll`, with the same meaning as in VectSharp.SVG, as well as `RasteriseAllWithSkia` and `RasteriseAllWithVectSharp`.

`RasteriseAllWithSkia` (which is the default) does not require any other package to be installed, and uses the SkiaSharp library (which is installed as a dependency of Avalonia anyways) to create the raster images. Using this option is recommended. The `RasteriseAllWithVectSharp` option, instead, uses the same approach as the other output layers, requiring VectSharp.Raster or VectSharp.Raster.ImageSharp to be installed.

### VectSharp.Raster and VectSharp.Raster.ImageSharp

The `SaveAsPNG` and `SaveAsImage` methods provided by these packages do not have a `filterOption` parameter, because everything needs to be rasterised anyways.

[Back to top](#){: .btn }