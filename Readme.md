

# VectSharp: a light library for C# vector graphics

<img src="icon.svg" width="256" align="right">

## Introduction
**VectSharp** is a library to create vector graphics (including text) in C#, without too many dependencies.

It includes an abstract layer on top of which output layers can be written. Currently, there are four available output layers: **VectSharp.PDF** produces PDF documents, **VectSharp.Canvas** produces an `Avalonia.Controls.Canvas` object ([https://avaloniaui.net/docs/controls/canvas](https://avaloniaui.net/docs/controls/canvas)) containing the rendered graphics objects, **VectSharp.Raster** produces raster images in PNG format, and **VectSharp.SVG** produces vector graphics in SVG format.

[**VectSharp.ThreeD**](https://github.com/arklumpus/VectSharp/tree/master/VectSharp.ThreeD) adds support for 3D vector and raster graphics.

[**VectSharp.Markdown**](https://github.com/arklumpus/VectSharp/tree/master/VectSharp.Markdown) can be used to transform Markdown documents into VectSharp objects, that can then be exported e.g. as PDF or SVG files, or displayed in an Avalonia `Canvas`. **VectSharp.MarkdownCanvas** uses VectSharp.Markdown to render Markdown documents in Avalonia applications (an example of this is in the [MarkdownViewerDemo project](https://github.com/arklumpus/VectSharp/tree/master/MarkdownViewerDemo)).

VectSharp is written using .NET Core, and is available for Mac, Windows and Linux. It is released under a GPLv3 license. It includes 14 standard fonts, also released under a GPL license.

Since version 2.0.0, VectSharp.Raster is released under an AGPLv3 license.

**VectSharp.MuPDFUtils**, also released under an AGPLv3 license, contains some utility functions that use [MuPDFCore](https://github.com/arklumpus/MuPDFCore) to make it possible to include in VectSharp graphics images in various formats.

## Installing VectSharp
To include VectSharp in your project, you will need one of the output layer NuGet packages: [VectSharp.PDF](https://www.nuget.org/packages/VectSharp.PDF/), [VectSharp.Canvas](https://www.nuget.org/packages/VectSharp.Canvas/), [VectSharp.Raster](https://www.nuget.org/packages/VectSharp.Raster/), or [VectSharp.SVG](https://www.nuget.org/packages/VectSharp.SVG/). You will need [VectSharp.ThreeD](https://www.nuget.org/packages/VectSharp.ThreeD/) to work with 3D graphics. You may want the [VectSharp.MuPDFUtils](https://www.nuget.org/packages/VectSharp.MuPDFUtils/) package if you wish to manipulate raster images.

## Usage
You can find the full documentation for the VectSharp library at the [documentation website](https://arklumpus.github.io/VectSharp). A [PDF reference manual](https://arklumpus.github.io/VectSharp/VectSharp.pdf) is also available.

In general, working with VectSharp involves: creating a `Document`, adding `Page`s, drawing to the `Page`s' `Graphics` objects and, finally, exporting them to a PDF document, `Canvas`, PNG image or SVG document.

* Create a `Document`:
```Csharp
using VectSharp;
// ...
Document doc = new Document();
```
* Add a `Page`:
```Csharp
doc.Pages.Add(new Page(1000, 1000));
``` 
* Draw to the `Page`'s `Graphics` object:
```Csharp
Graphics gpr = doc.Pages.Last().Graphics;
gpr.FillRectangle(100, 100, 800, 800, Colour.FromRgb(128, 128, 128));
``` 
* Save as PDF document:
```Csharp
using VectSharp.PDF;
//...
doc.SaveAsPDF(@"Sample.pdf");
``` 
* Export the graphics to a `Canvas`:
```Csharp
using VectSharp.Canvas;
//...
Avalonia.Controls.Canvas can = doc.Pages.Last().PaintToCanvas();
``` 
* Save as a PNG image:
```Csharp
using VectSharp.Raster;
//...
doc.Pages.Last().SaveAsPNG(@"Sample.png");
``` 
* Save as an SVG document:
```Csharp
using VectSharp.SVG;
//...
doc.Pages.Last().SaveAsSVG(@"Sample.svg");
``` 
* PDF and SVG documents support both internal and external links:
```CSharp
using VectSharp;
using VectSharp.PDF;
using VectSharp.SVG;
//...
Document document = new Document();
Page page = new Page(1000, 1000);
document.Pages.Add(page);

page.Graphics.FillRectangle(100, 100, 800, 50, Colour.FromRgb(128, 128, 128), tag: "linkToGitHub");
page.Graphics.FillRectangle(100, 300, 800, 50, Colour.FromRgb(255, 0, 0), tag: "linkToBlueRectangle");
page.Graphics.FillRectangle(100, 850, 800, 50, Colour.FromRgb(0, 0, 255), tag: "blueRectangle");

Dictionary<string, string> links = new Dictionary<string, string>() { { "linkToGitHub", "https://github.com/" }, { "linkToBlueRectangle", "#blueRectangle" } };

page.SaveAsSVG(@"Links.svg", linkDestinations: links);
document.SaveAsPDF(@"Links.pdf", linkDestinations: links);
``` 
This code produces a document with three rectangles: the grey one at the top links to the GitHub home page, while the red one in the middle is a hyperlink to the blue one at the bottom. Links in PDF documents can refer to objects that are in a different page than the one containing the link.

The public classes and methods are [fully documented](https://arklumpus.github.io/VectSharp), and you can find a (much) more detailed code example in [MainWindow.xaml.cs](https://github.com/arklumpus/VectSharp/blob/master/VectSharp.Demo/MainWindow.xaml.cs). A detailed guide about 3D graphics in VectSharp.ThreeD is available in the [`VectSharp.ThreeD`](https://github.com/arklumpus/VectSharp/tree/master/VectSharp.ThreeD) folder.

## Creating new output layers

VectSharp can be easily extended to provide additional output layers. To do so:
1. Create a new class implementing the `IGraphicsContext` interface.
2. Provide an extension method to either the `Page` or `Document` types.
3. Somewhere in the extension method, call the `CopyToIGraphicsContext` method on the `Graphics` object of the `Page`s.
4. Opportunely save or return the rendered result.

## Compiling VectSharp from source

The VectSharp source code includes an example project (*VectSharp.Demo*) presenting how VectSharp can be used to produce graphics.

To be able to compile VectSharp from source, you will need to install the latest [.NET SDK](https://dotnet.microsoft.com/download/dotnet/current) for your operating system.

You can use [Microsoft Visual Studio](https://visualstudio.microsoft.com/it/vs/) to compile the program. The following instructions will cover compiling VectSharp from the command line, instead.

First of all, you will need to download the VectSharp source code: [VectSharp.tar.gz](https://github.com/arklumpus/VectSharp/archive/v1.7.0.tar.gz) and extract it somewhere.

### Windows
Open a command-line window in the folder where you have extracted the source code, and type:

	BuildDemo <Target>

Where `<Target>` can be one of `Win-x64`, `Linux-x64` or `Mac-x64` depending on which platform you wish to generate executables for.

In the Release folder and in the appropriate subfolder for the target platform you selected, you will find the compiled program.

### macOS and Linux
Open a terminal in the folder where you have extracted the source code, and type:

	./BuildDemo.sh <Target>

Where `<Target>` can be one of `Win-x64`, `Linux-x64` or `Mac-x64` depending on which platform you wish to generate executables for.

In the Release folder and in the appropriate subfolder for the target platform you selected, you will find the compiled program.

If you receive an error about permissions being denied, try typing `chmod +x BuildDemo.sh` first.

## Note about VectSharp.MuPDFUtils and .NET Framework

If you wish to use VectSharp.MuPDFUtils in a .NET Framework project, you will need to manually copy the native MuPDFWrapper library for the platform you are using to the executable directory (this is done automatically if you target .NET core).

One way to obtain the appropriate library files is:

1. Manually download the NuGet package for [MuPFDCore](https://www.nuget.org/packages/MuPDFCore/) (click on the "Download package" link on the right).
2. Rename the `.nupkg` file so that it has a `.zip` extension.
3. Extract the zip file.
4. Within the extracted folder, the library files are in the `runtimes/xxx-x64/native/` folder, where `xxx` is either `linux`, `osx` or `win`, depending on the platform you are using.

Make sure you copy the appropriate file to the same folder as the executable!