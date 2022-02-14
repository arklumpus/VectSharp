---
layout: default
nav_order: 14
---

# Importing SVG files

In addition to making it possible to export VectSharp images as SVG files, VectSharp.SVG also makes it possible to import existing SVG files in order to use them when drawing on the graphics surface.

To do this, you can use the static methods in the `VectSharp.SVG.Parser` class. There are three main such methods:

* `FromString` parses an SVG document contained in a `string`. This method takes a single `string` parameter, corresponding to the SVG source.
* `FromFile` reads the SVG source from the specified file, and then parses it. This method also requires a single `string` parameter, corresponding to the path to the SVG file.
* `FromStream` instead reads the SVG source from a `Stream`, which is supplied as a parameter.

Each of these methods returns a `Page`, whose `Width` and `Height` correspond to the size of the viewbox defined in the SVG file, while the page's `Graphics` contains the image.

For example, the following code loads an image from an SVG file, draws it on a new `Page`, overlays it with some text, and then saves it again as an SVG file. You can download the SVG file for this example from <a href="/assets/tutorials/SurgeonFish.svg" download>here</a>.

<div class="code-example">
    <p style="text-align: center">
        <img src="assets/tutorials/ImportSVG.svg" style="height: 10em" />
    </p>
</div>
{% highlight CSharp %}
using VectSharp;
using VectSharp.SVG;

// Load the image.
Page surgeonFish = Parser.FromFile("/path/to/surgeonFish.svg");

// Create the new page, with the same size as the original image.
Page page = new Page(surgeonFish.Width, surgeonFish.Height);
Graphics graphics = page.Graphics;

// Draw the surgeon fish on the new page.
graphics.DrawGraphics(0, 0, surgeonFish.Graphics);

// Draw some text.
Font font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 32);
graphics.FillText(0, 20, "VectSharp", font, Colours.Green);

// Save the new page.
page.SaveAsSVG("ImportSVG.svg");
{% endhighlight %}

It is possible for an SVG file to embed other images using the [data URI scheme](https://en.wikipedia.org/wiki/Data_URI_scheme). There is no problem if another SVG image is embedded in this way; however, raster images (such as PNGs or JPEGs) cannot be interpreted directly by VectSharp.

If you need to parse an SVG file that contains raster images embedded using a data URI, you will need to install either the VectSharp.MuPDFUtils package or VectSharp.ImageSharpUtils package. You then need to enable data URI parsing using one of the following lines of code:

{% highlight CSharp %}
VectSharp.SVG.Parser.ParseImageURI = VectSharp.ImageSharpUtils.ImageURIParser
                                     .Parser(VectSharp.SVG.Parser.ParseSVGURI);
// or
VectSharp.SVG.Parser.ParseImageURI = VectSharp.MuPDFUtils.ImageURIParser
                                     .Parser(VectSharp.SVG.Parser.ParseSVGURI);
{% endhighlight %}

These instruct VectSharp to use the specified data URI parser, and only need to be used once in a program, at any time prior to using the methods from the `VectSharp.SVG.Parser` class.

