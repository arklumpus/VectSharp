

# VectSharp: a light library for C# vector graphics
## Introduction
**VectSharp** is a library to create vector graphics (including text) in C#, without too many dependencies.

It includes an abstract layer on top of which output layers can be written. Currently, there are three available output layers: **VectSharp.PDF** produces PDF documents, **VectSharp.Canvas** produces an `Avalonia.Controls.Canvas` object ([https://avaloniaui.net/api/Avalonia.Controls/Canvas/](https://avaloniaui.net/api/Avalonia.Controls/Canvas/)) containing the rendered graphics objects, and **VectSharp.Raster** produces raster images in PNG format.

VectSharp is written using .NET Core, and is available for Mac, Windows and Linux. It is released under a GPLv3 license. It includes 14 standard fonts, also released under a GPL license.

## Installing VectSharp
To include VectSharp in your project, you will need one of the output layer NuGet packages: [VectSharp.PDF](https://www.nuget.org/packages/VectSharp.PDF/), [VectSharp.Canvas](https://www.nuget.org/packages/VectSharp.Canvas/) or [VectSharp.Raster](https://www.nuget.org/packages/VectSharp.Raster/).

## Usage
In general, working with VectSharp involves: creating a `Document`, adding `Page`s, drawing to the `Page`s' `Graphics` objects and, finally, exporting them to a PDF document, `Canvas` or PNG image.

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
doc.SaveAsPDF(@"Test.pdf");
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
The public classes and methods are fully documented, and you can find a (much) more detailed code example in [MainWindow.xaml.cs](https://github.com/arklumpus/VectSharp/blob/master/VectSharp.Demo/MainWindow.xaml.cs).

## Creating new output layers

VectSharp can be easily extended to provide additional output layers. To do so:
1. Create a new class implementing the `IGraphicsContext` interface.
2. Provide an extension method to either the `Page` or `Document` types.
3. Somewhere in the extension method, call the `CopyToIGraphicsContext` method on the `Graphics` object of the `Page`s.
4. Opportunely save or return the rendered result.

## Compiling VectSharp from source

The VectSharp source code includes an example project (*VectSharp.Demo*) presenting how VectSharp can be used to produce graphics.

To be able to compile VectSharp from source, you will need to install the [.NET Core 3.0 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.0) for your operating system.

You can use [Microsoft Visual Studio](https://visualstudio.microsoft.com/it/vs/) to compile the program. The following instructions will cover compiling VectSharp from the command line, instead.

First of all, you will need to download the VectSharp source code: [VectSharp.tar.gz](https://github.com/arklumpus/VectSharp/archive/v1.2.0.tar.gz) and extract it somewhere.

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
