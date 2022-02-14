---
layout: default
nav_order: 3
---

# Choosing the output layer

VectSharp currently has five output layers, which can be used to produce files in different formats and in different conditions.

* [**VectSharp.PDF**](https://www.nuget.org/packages/VectSharp.PDF/) can be used to produce PDF documents, which can include multiple pages.
    
    Referencing the VectSharp.PDF assembly provides a `SaveAsPDF` extension method on the `Document` object, that is used to save the PDF to a file or to a `Stream`.

    PDF is a widespread vector format that is supported by many viewers and editors; however, editing a PDF document can be more cumbersome than working with an SVG file.

* [**VectSharp.SVG**](https://www.nuget.org/packages/VectSharp.SVG/) produces SVG documents. Referencing this assembly provides a `SaveAsSVG` extension method on `Page` objects, which can be used to export the contents of the page to a file or to a stream.

    The SVG format is often used to embed vector graphics in web pages. SVG files are also easier to edit than PDF documents. Font files can be embedded within an SVG image, and they should be interpreted correctly by most web browsers. However, some editors (e.g., Inkscape) do not recognise them.

* [**VectSharp.Raster**](https://www.nuget.org/packages/VectSharp.Raster/) produces raster images in PNG format. Referencing this assembly provides a `SaveAsPNG` extension method on `Page` objects, which can be used to create the PNG file or to write the contents of the image to a `Stream`.

    PNG is a raster format, which means that the image may become "pixelated" if the resolution at which it is exported is too much different than the resolution at which the image is viewed. On the other hand, a raster image means that the file will look exactly the same on any platform.

    To create the PNG image, this package first uses VectSharp.PDF to create a PDF document, and then [MuPDFCore](https://github.com/arklumpus/MuPDFCore) to render the PDF using [MuPDF](https://mupdf.com/). This offers good performance; however, it comes with a few drawbacks:
    * MuPDFCore carries a native dependency. While this library is available for many different architectures (Windows x86, x64 and ARM; macOS x64 and ARM; Linux x64 and ARM), this can be an issue in some instances (e.g. if you want to create a WebAssembly project).
    * As a result of embedding the MuPDF library, MuPDFCore (and, thus, VectSharp.Raster) is released under an AGPL license, unlike the other VectSharp packages, which are released under a more permissive LGPL license.

* [**VectSharp.Raster.ImageSharp**](https://www.nuget.org/packages/VectSharp.Raster.ImageSharp/) produces raster images in a number of formats (e.g. PNG, JPEG, TIFF...). This assembly provides a `SaveAsImage` extension method on `Page` objects, which can be used to create the image file or to write the image to a `Stream`.

    All the formats supported by this package are raster formats, so the previous remarks also apply here.

    Unlike VectSharp.Raster, this package uses the [ImageSharp](https://github.com/SixLabors/ImageSharp) library to create the raster images. This has worse performance than the previous approach, but avoids the native dependency and licensing issues that come with MuPDFCore. A downside is that the ImageSharp API is somewhat unstable (in the sense that the way the library works often changes unexpectedly, not in the sense that a program using it will not work properly); however, this will hopefully not be too much of a problem.

    To install this package you will need to add to Visual Studio the [ImageSharp MyGet repository](https://www.myget.org/feed/sixlabors/package/nuget/SixLabors.ImageSharp), where the latest builds of ImageSharp are found.

* [**VectSharp.Canvas**](https://www.nuget.org/packages/VectSharp.Canvas/) can be used in combination with the [Avalonia UI library](https://github.com/AvaloniaUI/Avalonia) to display the contents of a `Page` in a cross-platform application.

    Referencing this assembly provides two extension methods on `Page` objects: `PaintToCanvas` and `PaintToSKCanvas`. Both of these methods create an `Avalonia.Controls.Canvas` object with the image and support user interaction (i.e. you can subscribe to events when a user clicks on an object in the plot). `PaintToSKCanvas` directly uses the Skia backend and has the best performance; however, using the `Canvas` produced by this method is slightly harder than using the result of `PaintToCanvas`.
