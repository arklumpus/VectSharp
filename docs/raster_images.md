---
layout: default
nav_order: 9
---

# Drawing raster images
{: .no_toc }

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
1. TOC
{:toc}
</details>


In addition to drawing vector graphics, you may also want to include raster images in your plot. This can be useful e.g. to include icons, screenshots, or for effects that are too complex to be created in a vector form. To do this, you can use the `DrawRasterImage` method of the `Graphics` class.

## The `RasterImage` class

First of all, you need to load the raster image into a `RasterImage` object. The image data can come in two forms:

* Raw pixel data in RGB, RGBA, BGR or BGRA format (3 or 4 bytes per pixel).
* Encoded pixel data in some image format (e.g., PNG or JPEG).

Loading raw pixel data is the easiest, as you can just use one of the `RasterImage` constructors:

{% highlight CSharp %}
// Raw pixel data obtained from some other source.
byte[] pixelData = ...

// Image width.
int width = 100;

// Image height.
int height = 100;

// Image pixel format.
PixelFormats pixelFormat = PixelFormats.RGBA;

// Whether the image should be interpolated or not - see below.
bool interpolate = true;

RasterImage image = new RasterImage(pixelData, width, height, pixelFormat, interpolate);
{% endhighlight %}

Additional overloads can be used if the pixel data is accessible via an `IntPtr` rather than a `byte[]` array. This is useful, because external libraries may work with pointers, and by using these constructors, you avoid having to copy the data back and forth between managed and unmanaged memory. Expand the section below for more details about these.

<details markdown="block">
  <summary>
    Working with pixel data from an <code>IntPtr</code>
  </summary>
  {: .text-delta }

While raw pixel data coming from a `byte[]` array can be in RGB/RGBA or BGR/BGRA format, pixel data from an `IntPtr` must be either in RGB or RGBA format. When you use one of the constructors described here, the pixel data is not copied: accordingly, they are much faster and less resource-intensive than the constructor that takes a `byte[]` array.

The first constructor accepts the address of the image data as an `IntPtr`. As before, you need to specify the width and height of the image, as well as whether it contains an alpha channel (and thus is in RGBA format) or not (and thus is in RGB format).

{% highlight CSharp %}
// Raw pixel data obtained from some other source.
IntPtr pixelData = ...

// Image width.
int width = 100;

// Image height.
int height = 100;

// Whether the image contains an alpha channel or not.
bool alpha = true;

// Whether the image should be interpolated or not - see below.
bool interpolate = true;

RasterImage image = new RasterImage(pixelData, width, height, alpha, interpolate);
{% endhighlight %}

If you use this approach, you will have to keep track of the lifetime of the `RasterImage`, so that you can free the memory associated to the `IntPtr` at the right time.

If you create the image using the following constructor, instead, `VectSharp` will take care of automatically freeing the memory when the `RasterImage` is disposed (the library will also call `GC.RemoveMemoryPressure` to notify the garbage collector that the memory has been freed). This constructor, instead of a raw `IntPtr`, takes a `DisposableIntPtr` as a `ref` parameter. The `ref` keyword is used to indicate that the `RasterImage` takes ownership of the `DisposableIntPtr` and will take care of disposing it when necessary.

A `DisposableIntPtr` is a simple wrapper over an `IntPtr` that implements `IDisposable` and, when disposed, calls `Marshal.FreeHGlobal` to free the memory associated to the `IntPtr`.

{% highlight CSharp %}
// Raw pixel data obtained from some other source.
IntPtr pixelData = ...

// DisposableIntPtr wrapper over the pixelData pointer.
DisposableIntPtr disposableWrapper = new DisposableIntPtr(pixelData);

// Image width.
int width = 100;

// Image height.
int height = 100;

// Whether the image contains an alpha channel or not.
bool alpha = true;

// Whether the image should be interpolated or not - see below.
bool interpolate = true;

RasterImage image = new RasterImage(ref disposableWrapper, width, height, alpha, interpolate);
{% endhighlight %}

Unless you have reason to do otherwise, it is recommended that you use this second constructor, as it ensures that the lifetime of the unmanaged memory associated to the image corresponds with the lifetime of the `RasterImage` itself (otherwise, you may accidentally free the unmanaged memory before having finished using the image).
</details>

Note that the `RasterImage` class implements the `IDisposable` interface; therefore, you may want to call its `Dispose` method when you are finished with it. However, make sure to only `Dispose` the image _after exporting the image file_, and not immediately after drawing it on the `Graphics` surface! Otherwise, when you try to save the image it will already have been disposed.

If you wish, you can also avoid calling `Dispose` on the `RasterImage` at all; this way, the image will be disposed automatically when it is garbage-collected. This may lead to some memory being wasted, but it is safer (because it is only disposed at some point after all references to it are unreachable).

[Back to top](#){: .btn }

## Loading an image from a file

Most of the time, the image will not be in raw pixel format; instead, you may want to include in the plot a file in a format such as PNG or JPEG (or more). To do this, you need to install either the [VectSharp.MuPDFUtils](https://nuget.org/packages/VectSharp.MuPDFUtils) package, or the [VectSharp.ImageSharpUtils](https://nuget.org/packages/VectSharp.ImageSharpUtils) package.

These packages offer equivalent functionalities, but have the same differences as VectSharp.Raster and VectSharp.Raster.ImageSharp (see [Choosing the output layer]({% link output_layer.md %})). In these examples, we will use VectSharp.ImageSharpUtils, but the only change you need to make to use VectSharp.MuPDFUtils is the `using` directive at the top of the code (in addition to installing the appropriate NuGet package, of course).

Both of these packages provide two classes that inherit from `RasterImage`: `RasterImageFile` and `RasterImageStream`. As the name implies, `RasterImageFile` is used to create a `RasterImage` from an image file, while `RasterImageStream` is used to read the image data from a `Stream`.

The main parameter for the `RasterImageFile` constructor is the path to the image file (provided as a `string`):

{% highlight CSharp %}
RasterImage image = new RasterImageFile("/path/to/image.png");
{% endhighlight %}

Additionally, the constructor has two optional boolean parameters:

* `alpha` determines whether the resulting `RasterImage` has an alpha channel or not. If the source image does not include transparency information (e.g., a JPEG image), changing the value of this parameter does not have a discernible effect (beyond the fact that the image will take about 30% more space in memory). Instead, if the image is in a format that includes transparency information (e.g., PNG), the transparency is only preserved if `alpha` is set to `true` (which is the default), and it is discarded if `alpha` is set to `false`. Essentially, you should only set this parameter to `false` if you are 100% sure that the image does not have any transparency information associated to it.

* The other parameter, `interpolate`, determines what happens to the image when it is drawn at a scale other than 1:1. If you set this to `false`, the image will have a "pixelated" appearance, while if you set it to `true`, the image will be interpolated, which will result in a fuzzier image, though it will look better at low zoom levels. The following example shows the difference between these two settings:

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/RasterImages.svg" style="height: 10em" />
    </p>
</div>

The `RasterImageFile` constructor from VectSharp.MuPDFUtils also takes two more optional parameters: `pageNumber` and `scale`. These are used because VectSharp.MuPDFUtils supports creating a `RasterImage` from a PDF file, and thus these parameters specify which page should be rendered (starting at `0`) and the scale at which the page should be rendered.

[Back to top](#){: .btn }

## Loading an image from a `Stream`

The `RasterImageStream` class provided by VectSharp.MuPDFUtils and VectSharp.ImageSharpUtils can be used to create a `RasterImage` from a `Stream`. The syntax of the constructor for this class is similar to the constructor for `RasterImageFile`, except that this constructor accepts a `Stream` containing the image data, instead of the path to a file.

{% highlight CSharp %}
Stream imageStream = ...

RasterImage image = new RasterImageStream(imageStream);
{% endhighlight %}

The `alpha` and `interpolate` parameters are also used in this case, with the same meaning as before. The `RasterImageStream` class from VectSharp.MuPDFUtils also requires an additional parameter specifying the file type (this is inferred automatically in VectSharp.ImageSharpUtils, instead).

Furthermore, this class also has an additional constructor that takes an `IntPtr` pointing to the image data instead of a stream. Again, this is useful if you need to combine VectSharp with some other library that saves the rasterised image data in unmanaged memory. For this constructor, you also need to specify the length of the image data in memory.

[Back to top](#){: .btn }

## Drawing the image

Once you have obtained a `RasterImage` with your image data, it is finally time to draw it on the graphics surface. The `DrawRasterImage` method can be used in three main ways. The easiest way is to just specify the location of the image on the page (either as a `Point`, or as two `double`s):

{% highlight CSharp %}
Graphics graphics = ...
RasterImage image = ...

// Draw the image with the top-left corner at 10, 10
graphics.DrawRasterImage(10, 10, image);
{% endhighlight %}

In this case, 1 pixel of the image will be equivalent to 1 graphics unit.

Alternatively, you can also specify the width and height of the rendered image in graphics units (either as a `Size`, or as two `double`s):

{% highlight CSharp %}
Graphics graphics = ...
RasterImage image = ...

// Draw the image with the top-left corner at 10, 10
// and the bottom-right corner at 90, 90
graphics.DrawRasterImage(10, 10, 80, 80, image);
{% endhighlight %}

The image will be scaled to the specified width and height. Note that this does not preserve the aspect ratio of the image.

Finally, you can also specify the coordinates (top-left corner and size) of a rectangle on the source image. These are specified as four `int`s (in pixel units):

{% highlight CSharp %}
Graphics graphics = ...
RasterImage image = ...

// Draw the image, causing the pixel at 20, 20 in the original
// image to be placed at 10, 10 in the graphics surface, and the
// pixel at 70, 70 in the original image to be placed at 90, 90.
graphics.DrawRasterImage(20, 20, 50, 50, 10, 10, 80, 80, image);
{% endhighlight %}

This lets you draw a subregion of the image on the graphics surface, rather than the whole image.

Here is a full example of how to draw a raster image on a page (naturally, replace `/path/to/file.png` with the path to an actual image file):

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/RasterImage.svg" style="height: 5em" />
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;
using VectSharp.ImageSharpUtils;
// or, using VectSharp.MuPDFUtils;

Page page = new Page(100, 100);
Graphics graphics = page.Graphics;

// Open the image file.
RasterImage rasterImage = new RasterImageFile("/path/to/file.png");

// Draw the image at 10, 10, with a width of 80, and preserving aspect ratio.
graphics.DrawRasterImage(10, 27, 80, 80 * rasterImage.Height / rasterImage.Width, rasterImage);

page.SaveAsSVG("RasterImage.svg");
{% endhighlight %}

[Back to top](#){: .btn }
